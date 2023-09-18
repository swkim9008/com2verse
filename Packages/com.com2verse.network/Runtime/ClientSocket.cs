/*===============================================================
* Product:		Com2Verse
* File Name:	ClientSocket.cs
* Developer:	haminjeong
* Date:			2022-12-01 17:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using Com2Verse.Logger;
using Google.Protobuf;

namespace Com2Verse.Network
{
	public sealed class ClientSocket : SocketBase
	{
		private struct ReceiveContext
		{
			public Protocols.Channels Channel;
			public int Command;
			public ArraySegment<byte> Message;
			public Protocols.ErrorCode ErrorCode;
		}

		private static ReceiveContext _convertedContext;

		/// <summary>
		/// 파싱된 메시지를 넘겨주는 이벤트. NetworkManager 전용으로 호출되고 있다.
		/// </summary>
		public event Action<int, int, int, ArraySegment<byte>> OnPacketReceived;
		
		private static readonly int HeartbeatInterval = 1000;
		private static readonly int FirstPingPongCount = 3;
		private static readonly int PingThreadSleepTime = 50;
#if UNITY_EDITOR && !METAVERSE_RELEASE
		private static readonly int PingPongTimeout = 3600000;
		private static readonly int PacketTimeout = 3600000;
#else
		private static readonly int PingPongTimeout = 15000;
		private static readonly int PacketTimeout = 30000;
#endif
		
		private readonly ConcurrentQueue<ArraySegment<byte>> _pingpongQueue = new();
		
		private                 Thread                  _pingpongThread;
		private readonly        Protocols.Tick.Ping     _ping             = new();
		private readonly        Protocols.SourceAddress _protocolsAddress = new();
		private readonly        Stopwatch               _watch            = new();
		private                 long                    CurrentTime => _watch!.ElapsedMilliseconds;
		private                 byte[]                  _destinationAddress;
		private                 byte[]                  _returnAddress;
		private                 long                    _sendPingAt;
		private                 int                     _pongReceived;
		private                 bool                    _pingPongTrigger = false;
		private                 int                     _averageRTT;
		private                 long                    _lastPongReceived;
		private static readonly int                     AverageRTTCount = 5;
		private readonly        int[]                   _rttRecords     = new int[AverageRTTCount];
		/// <summary>
		/// 평균 RTT 값
		/// </summary>
		public int AverageRTT => _averageRTT;

		/// <summary>
		/// 지정된 버퍼 사이즈로 소켓을 생성합니다. 기본사이즈는 보내기 8192, 받기 65535입니다.
		/// </summary>
		/// <param name="sendsize">보내기 최대 버퍼 사이즈</param>
		/// <param name="receivesize">받기 최대 버퍼 사이즈</param>
		public ClientSocket(int sendsize = -1, int receivesize = -1) : base(sendsize, receivesize) { }

		public override bool IsConnected => InternalSocket?.Connected ?? false;

		/// <summary>
		/// 지정된 주소, 포트로 연결을 수립합니다. 기본 타임아웃은 10초로 설정되어 있습니다.
		/// </summary>
		/// <param name="address">네트워크 주소</param>
		/// <param name="port">포트</param>
		/// <param name="type">프로토콜 방식. 기본은 TCP</param>
		public override void Connect(string address, int port, ProtocolType type = ProtocolType.Tcp)
		{
			OnConnected += PostConnected;
			OnReceivedData += ReceiveProcess;
			
			base.Connect(address, port, type);
			InternalSocket!.ReceiveTimeout   = InternalSocket.SendTimeout = PacketTimeout; // default : 0(unlimited)
			InternalSocket.SendBufferSize    = SendBufferSize;                             // default : 8192
			InternalSocket.ReceiveBufferSize = ReceiveBufferSize;                          // default : 65535
			
			LogicPacket.BufferAllocatable    ??= new ClientNetworkBufferManager();
		}

		/// <summary>
		/// 내부 소켓의 타임아웃 시간을 정합니다.
		/// </summary>
		/// <param name="time">타임아웃 시간(ms)</param>
		public void SetTimeout(int time)
		{
			if (InternalSocket == null) return;
			InternalSocket.ReceiveTimeout = InternalSocket.SendTimeout = time;
		}
		
		private void PostConnected()
		{
			_pingpongThread = new Thread(PingPongWork);
			_pingpongThread.Start();
			_sendPingAt = long.MaxValue;
			_averageRTT = 0;
			_pongReceived = 0;
		}

		/// <summary>
		/// 연결을 중단합니다.
		/// </summary>
		public override void Stop()
		{
			StopPingPong();
			if (_pingpongThread is { IsAlive: true })
			{
				_pingpongThread.Abort();
				_pingpongThread = null;
			}

			OnConnected    -= PostConnected;
			OnReceivedData -= ReceiveProcess;
			Security.ClearTransforms();
			base.Stop();
		}

		private void ReceiveProcess(byte[] context)
		{
			LogicPacket.ParseVariableHeaderPacket(context, out var messageType, out var offset, out var length);
			if (length == 0) return;
			if (Security.IsActivatedEncrypt)
			{
				var contextSegment = new ArraySegment<byte>(context, offset, length - offset);
				var decryptedData  = messageType == LogicPacket.ClientMessageType.Normal ? Security.DecryptByAES(contextSegment) : Security.DecryptCSUByAES(contextSegment);
				context = decryptedData.Array;
				length  = decryptedData.BodyCount;
				offset  = decryptedData.BodyOffset;
			}
			else
				length -= offset;
			switch (messageType)
			{
				case LogicPacket.ClientMessageType.Normal:
					LogicPacket.ParseMessage(context, length, offset,
					                         out var logicalAddress, out var returnAddress,
					                         out var channel1, out var command, out var errorCode, out var payload1);
					_convertedContext.Channel   = (Protocols.Channels)channel1;
					_convertedContext.Command   = command;
					_convertedContext.ErrorCode = errorCode;
					_convertedContext.Message   = payload1;
					break;
				case LogicPacket.ClientMessageType.Simple:
					LogicPacket.ParseSimpleMessage(context, length, offset, out var channel2, out var payload2);
					_convertedContext.Channel   = (Protocols.Channels)channel2;
					_convertedContext.Command   = (int)Protocols.CellStateUpdate.MessageTypes.CellStateUpdate;
					_convertedContext.ErrorCode = Protocols.ErrorCode.Success;
					_convertedContext.Message   = payload2;
					break;
			}
			if (_convertedContext is { Channel: Protocols.Channels.Tick, Command: (int)Protocols.Tick.MessageTypes.Pong })
			{
				_pingpongQueue!.Enqueue(_convertedContext.Message);
				return;
			}
			if (_convertedContext is { Channel: Protocols.Channels.ClientConnection, Command: (int)Protocols.ClientConnection.MessageTypes.ExchangeKeyResponse })
			{
				var response = Protocols.ClientConnection.ExchangeKeyResponse.Parser.ParseFrom(_convertedContext.Message);
				Security.SetDecryptorCSU(response.ServerAesKey.ToByteArray(), response.ServerAesIv.ToByteArray());
				return;
			}
			if (_convertedContext is { Channel: Protocols.Channels.ClientConnection, Command: (int)Protocols.ClientConnection.MessageTypes.GetEncryptionStatusResponse })
			{
				var response = Protocols.ClientConnection.GetEncryptionStatusResponse.Parser.ParseFrom(_convertedContext.Message);
				Security.IsActivatedEncrypt   = response.EnableEncryption;
				Security.IsActivatedIfEncrypt = true;
				return;
			}
			OnPacketReceived?.Invoke((int)_convertedContext.Channel, _convertedContext.Command, (int)_convertedContext.ErrorCode, _convertedContext.Message);
		}

#region PingPong
		/// <summary>
		/// 핑퐁 스레드를 시작합니다. 연결이 수립된 후 실행되어야 합니다.
		/// </summary>
		/// <param name="userID">식별자로 사용할 유저 ID</param>
		public void StartPingPong(long userID)
		{
			_watch!.Start();
			_pingPongTrigger = true;
			_destinationAddress = Protocols.DestinationLogicalAddress.GetLogicalAddress(Protocols.Channels.Tick);
			_protocolsAddress!.ServerType = Protocols.ServerType.Client;
			_protocolsAddress.Identifiers = userID;
			_returnAddress = _protocolsAddress.ToByteArray();
			_lastPongReceived = CurrentTime;
			SendPing();
		}

		private void SendPing()
		{
			if (_pingPongTrigger)
			{
				_ping!.Time = CurrentTime;
				_sendPingAt = CurrentTime + HeartbeatInterval;
				SendMessage(_destinationAddress, (int)Protocols.Channels.Tick, (int)Protocols.Tick.MessageTypes.Ping, _ping.ToByteArray(), _returnAddress);
			}
		}

		private void OnPong(long sendTime)
		{
			_lastPongReceived = sendTime;
			int rtt = (int)(CurrentTime - sendTime);
			int index = ++_pongReceived % AverageRTTCount;
			_rttRecords![index] = rtt;
			int sum = 0;
			for (int i = 0; i < AverageRTTCount; ++i)
				sum += _rttRecords[i];
			_averageRTT = sum / AverageRTTCount;
			if (_pongReceived < FirstPingPongCount)
				SendPing();
		}

		private void PingPongWork()
		{
			while (!IsConnected)
				Thread.Yield();
			while (IsConnected)
			{
				try
				{
					while (_pingpongQueue!.TryDequeue(out var message))
					{
						var pong = Protocols.Tick.Pong.Parser.ParseFrom(message);
						OnPong(pong!.Time);
					}

					if (_sendPingAt < CurrentTime)
						SendPing();
					if (_pingPongTrigger && CurrentTime - _lastPongReceived > PingPongTimeout)
					{
						Stop();
						throw new SocketException((int)SocketError.TimedOut);
					}
					Thread.Sleep(PingThreadSleepTime);
				}
				catch (ThreadAbortException thread_exc)
				{
					C2VDebug.Log($"{thread_exc.Message}\n{thread_exc.StackTrace}");
				}
				catch (Exception exc)
				{
					C2VDebug.LogError($"{exc.Message}\n{exc.StackTrace}");
				}
			}
		}

		/// <summary>
		/// 핑퐁 스레드의 동작만을 비활성화 합니다.
		/// </summary>
		public void StopPingPong()
		{
			_pingPongTrigger = false;
		}
#endregion

		protected override bool Send(LogicPacket.IBuffer context)
		{
			var result = base.Send(context);
			if (!result) return false;
			SendMessages!.Enqueue(context);
			return true;
		}

		/// <summary>
		/// 메시지를 보냅니다.
		/// </summary>
		/// <param name="logicalAddressDestination">도착지 주소(Protobuf)</param>
		/// <param name="channel">채널</param>
		/// <param name="command">커맨드</param>
		/// <param name="buffer">페이로드</param>
		/// <param name="logicalAddressReturn">반환될 주소(Protobuf)</param>
		/// <returns></returns>
		public bool SendMessage(byte[] logicalAddressDestination, int channel, int command, byte[] buffer, byte[] logicalAddressReturn)
		{
			if (logicalAddressDestination is not { Length: > 0 }) return false;
			if (logicalAddressReturn is not { Length: > 0 }) return false;
			if (Security.IsActivatedEncrypt)
			{
				var sendBuffer = LogicPacket.Make(logicalAddressDestination, channel, command, 0, buffer, logicalAddressReturn);
				var encrypted  = Security.EncryptByAES(sendBuffer);
				return Send(encrypted);
			}
			return Send(LogicPacket.Make(logicalAddressDestination, channel, command, 0, buffer, logicalAddressReturn));
		}

		public void SendRSA(byte[] logicalDestination, int channel, int type, byte[] data, byte[] returnAddress)
		{
			var sendBuffer = LogicPacket.Make(logicalDestination, channel, type, 0, data, returnAddress);
			var encrypted  = Security.RSAEncrypt(sendBuffer);
			Send(encrypted);
		}
	}
}
