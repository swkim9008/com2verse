/*===============================================================
* Product:		Com2Verse
* File Name:	SocketBase.cs
* Developer:	haminjeong
* Date:			2022-12-01 11:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Com2Verse.Logger;
using WebSocketSharp;

namespace Com2Verse.Network
{
	public abstract class SocketBase
	{
		/// <summary>
		/// 내부 소켓 연결 상태
		/// </summary>
		public abstract bool IsConnected { get; }

		/// <summary>
		/// 연결이 수립되면 호출되는 이벤트
		/// </summary>
		public event Action OnConnected;
		/// <summary>
		/// byte[] 메시지를 수신할때마다 호출되는 이벤트
		/// </summary>
		public event Action<byte[]> OnReceivedData;
		/// <summary>
		/// string 메시지를 수신할때마다 호출되는 이벤트
		/// </summary>
		public event Action<string> OnReceivedString;
		/// <summary>
		/// 연결이 끊어지면 호출되는 이벤트
		/// </summary>
		public event Action<CloseEventArgs> OnDisconnected;
		/// <summary>
		/// 소켓관련 Exception이 발생하면 호출되는 이벤트
		/// </summary>
		public event Action<SocketException> OnSocketException;
		/// <summary>
		/// 웹소켓관련 Error가 발생하면 호출되는 이벤트
		/// </summary>
		public event Action<ErrorEventArgs> OnSocketError;

		protected Socket    InternalSocket;
		protected WebSocket InternalWebSocket;

		private            Thread                               _listenerThread;
		private            Thread                               _senderThread;
		protected readonly ConcurrentQueue<LogicPacket.IBuffer> SendMessages = new();
		
		private static readonly int DefaultSendBufferSize    = 8192;  // 소켓 내부 버퍼사이즈(보내기)
		private static readonly int DefaultReceiveBufferSize = 65535; // 소켓 내부 버퍼사이즈(받기)
		private static readonly int ReadBufferSizeAtOnce     = 8192;  // 한번에 읽을 수 있는 버퍼사이즈

		private readonly PacketBufferController _bufferController = new();

		protected int SendBufferSize;
		protected int ReceiveBufferSize;
		protected int ListenerThreadSleepTime = 1;
		protected int SenderThreadSleepTime   = 10;

		private static IPAddress GetIP4Address(string address)
		{
			var addresses = Dns.GetHostAddresses(address);
			IPAddress selected = null;
			for (int index = 0; index < addresses.Length; ++index)
			{
				IPAddress ipAddress = addresses[index];
				if (ipAddress is not { AddressFamily: AddressFamily.InterNetwork }) continue;

				if (selected == null)
					selected = ipAddress;
				else if (ipAddress.ToString().Equals(address))
					selected = ipAddress;
			}
			return selected;
		}

		/// <summary>
		/// 지정된 버퍼 사이즈로 소켓을 생성합니다. 기본사이즈는 보내기 8192, 받기 65535입니다.
		/// </summary>
		/// <param name="sendBuffer">최대 보내기 버퍼 사이즈</param>
		/// <param name="receiveBuffer">최대 받기 버퍼 사이즈</param>
		public SocketBase(int sendBuffer = -1, int receiveBuffer = -1)
		{
			if (sendBuffer == -1)
				SendBufferSize = DefaultSendBufferSize;
			if (receiveBuffer == -1)
				ReceiveBufferSize = DefaultReceiveBufferSize;
		}

#region WebSocket
		/// <summary>
		/// 지정된 주소로 접속을 시도합니다.(웹소켓 전용)
		/// </summary>
		/// <param name="url">접속할 URL</param>
		public virtual void Connect(string url, string token)
		{
			if (string.IsNullOrEmpty(url) || !url.Contains("wss://"))
			{
				C2VDebug.LogError($"{url} is unsupported url!, please use Connect(string, int, ProtocolType) method for socket type.");
				return;
			}
			
			InternalWebSocket           =  new WebSocketSharp.WebSocket(url);

			if (!string.IsNullOrEmpty(token))
				InternalWebSocket.SetCredentials(token);
			
			InternalWebSocket.OnOpen    -= OnOpen;
			InternalWebSocket.OnOpen    += OnOpen;
			InternalWebSocket.OnClose   -= OnClose;
			InternalWebSocket.OnClose   += OnClose;
			InternalWebSocket.OnError   -= OnError;
			InternalWebSocket.OnError   += OnError;
			InternalWebSocket.OnMessage -= OnMessage;
			InternalWebSocket.OnMessage += OnMessage;
			InternalWebSocket?.ConnectAsync();
		}

		private void OnOpen(object sender, EventArgs e)
		{
			OnConnected?.Invoke();
		}

		private void OnClose(object sender, CloseEventArgs e)
		{
			OnDisconnected?.Invoke(e);
		}

		private void OnError(object sender, ErrorEventArgs e)
		{
			OnSocketError?.Invoke(e);
		}

		private void OnMessage(object sender, MessageEventArgs e)
		{
			OnReceivedString?.Invoke(e?.Data);
		}
#endregion WebSocket

#region General Socket
		/// <summary>
		/// 주어진 주소와 포트로 연결을 시도합니다.(일반 소켓용)
		/// </summary>
		/// <param name="address">네트워크 주소</param>
		/// <param name="port">포트</param>
		/// <param name="type">프로토콜 방식. 기본은 TCP</param>
		public virtual void Connect(string address, int port, ProtocolType type = ProtocolType.Tcp)
		{
			if (address.Contains("wss://"))
			{
				C2VDebug.LogError($"{address} is unsupported url!, please use Connect(string) method for websocket type.");
				return;
			}

			IPAddress  ipAddress = GetIP4Address(address);
			IPEndPoint endPoint  = new IPEndPoint(ipAddress, port);
			if (type != ProtocolType.Tcp && type != ProtocolType.Udp)
			{
				C2VDebug.LogError($"{type.ToString()} is unsupported Type! Cannot connected remote server.");
				return;
			}

			if (type == ProtocolType.Tcp)
				InternalSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, type);
			else
				InternalSocket = new Socket(ipAddress.AddressFamily, SocketType.Dgram, type);

			try
			{
				InternalSocket.BeginConnect(endPoint, OnConnect, null);
			}
			catch (Exception e)
			{
				C2VDebug.LogError($"Error while accepting connection: {e.Message}\n{e.StackTrace}");
			}
		}

		private void OnConnect(IAsyncResult ar)
		{
			if (InternalSocket.Connected == false || ar == null)
				return;

			InternalSocket.EndConnect(ar);

			_senderThread = new Thread(SenderWork);
			_senderThread.Start();
			_listenerThread = new Thread(ListenerWork);
			_listenerThread.Start();

			OnConnected?.Invoke();
		}
#endregion General Socket

		/// <summary>
		/// 연결을 중단합니다.
		/// </summary>
		public virtual void Stop()
		{
			SendMessages!.Clear();
			if (_senderThread is { IsAlive: true })
			{
				_senderThread.Abort();
				_senderThread = null;
			}
			if (_listenerThread is { IsAlive: true })
			{
				_listenerThread.Abort();
				_listenerThread = null;
			}

			try
			{
				if (InternalSocket?.Connected ?? false)
					InternalSocket?.Disconnect(false);
				InternalSocket?.Close();
				InternalSocket?.Dispose();
			}
			catch (ObjectDisposedException e)
			{
				C2VDebug.Log($"{e.Message}\n{e.StackTrace}");
			}
		}

		private void SenderWork()
		{
			while (!IsConnected)
				Thread.Yield();
			while (IsConnected)
			{
				try
				{
					if (InternalSocket == null) break;
					while (SendMessages.TryDequeue(out var context))
					{
						if (context == null) continue;
						InternalSocket.Send(context.Array!, SocketFlags.None);
					}
					Thread.Sleep(SenderThreadSleepTime);
				}
				catch (SocketException sock_exc)
				{
					C2VDebug.LogError($"{sock_exc.Message}\n{sock_exc.StackTrace}");
					OnSocketException?.Invoke(sock_exc);
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
		
		private void ListenerWork()
		{
			while (!IsConnected)
				Thread.Yield();
			while (IsConnected)
			{
				try
				{
					if (InternalSocket == null) break;
					var readData = ArrayPool<byte>.Shared.Rent(ReadBufferSizeAtOnce);
					var nRecv = InternalSocket.Receive(readData, 0, readData.Length, SocketFlags.None);
					if (nRecv == 0)
					{
						ArrayPool<byte>.Shared.Return(readData);
						Thread.Sleep(ListenerThreadSleepTime);
						continue;
					}
					
					int rest = 0;
					do
					{
						var packetData = _bufferController!.Filter(readData, nRecv, ref rest);
						// 완료된 패킷이 오지 않은 경우
						if (packetData == null) continue;

						OnReceivedData?.Invoke(packetData);
					}
					while (rest > 0);
					ArrayPool<byte>.Shared.Return(readData);
					Thread.Sleep(ListenerThreadSleepTime);
				}
				catch (SocketException sock_exc)
				{
					C2VDebug.LogError($"{sock_exc.Message}\n{sock_exc.StackTrace}");
					OnSocketException?.Invoke(sock_exc);
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
			OnDisconnected?.Invoke(null);
		}

		protected virtual bool Send(LogicPacket.IBuffer context)
		{
			if (context is not { Count: > 0 }) return false;
			if (IsConnected == false)
			{
				C2VDebug.LogError("Cannot send message. It was disconnected.");
				return false;
			}
			return true;
		}
	}
}
