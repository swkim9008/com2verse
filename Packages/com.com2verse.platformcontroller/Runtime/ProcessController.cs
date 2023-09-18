/*===============================================================
* Product:		Com2Verse
* File Name:	ProcessController.cs
* Developer:	mikeyid77
* Date:			2023-03-06 11:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;

namespace Com2Verse.PlatformControl
{
	internal sealed class ProcessController
	{
		private string[] _arguments;

		public void Initialize()
		{
			SocketUtils.Initialize();
#if UNITY_EDITOR
			CheckDaemonProcess();
#else
			_arguments = Environment.GetCommandLineArgs();
			foreach (var args in _arguments)
				C2VDebug.LogCategory("PlatformController", $"Args : {args}");

			if (_arguments.Length > 1)
			{
				try
				{
					var message = JsonUtility.FromJson<PacketMessage>(_arguments[1]);
					CheckDaemonProcess(message);
				}
				catch
				{
					CheckDaemonProcess();
				}
			}
			else
			{
				CheckDaemonProcess();
			}
#endif
		}
		
		private void CheckDaemonProcess(PacketMessage? arguments = null)
		{
			C2VDebug.LogWarningCategory("PlatformController", $"Initialize SocketClientController");

			if (IsDaemonExists())
			{
				OpenConnect();
			}
			else
			{
				try
				{
					Process.Start(SocketUtils.DaemonPath);
					if (arguments.HasValue) _messageQueue.Enqueue(arguments.Value);
				}
				catch (Win32Exception)
				{
					C2VDebug.LogErrorCategory("PlatformController", "Daemon Process is not Installed");
					IsDaemonNotFound = true;
				}
			}
		}

		public void OnUpdate()
		{
			if (_messageQueue.Count > 0)
			{
				try
				{
					_messageQueue.TryDequeue(out var packetMessage);
					var action = Enum.Parse<eReceiveMessage>(packetMessage.Action);
					switch (action)
					{
						case eReceiveMessage.NORMAL:
						case eReceiveMessage.ALL:
							C2VDebug.LogCategory("PlatformController", $"Open Message");
							_receiveMessageAction?.Invoke(packetMessage.Sender, packetMessage.Message);
							break;
						case eReceiveMessage.QUICK_CONNECT:
							C2VDebug.LogCategory("PlatformController", $"Open QuickConnect Message");
							_receiveQuickConnectAction?.Invoke(packetMessage.Sender, packetMessage.Message);
							break;
						case eReceiveMessage.PING:
							// TODO : Ping 처리 필요시 작성
							break;
					}
				}
				catch (ArgumentException e)
				{
					C2VDebug.LogWarning(e);
					C2VDebug.LogWarningCategory("PlatformController", $"packetMessage Invalid");
				}
				catch (Exception e)
				{
					C2VDebug.LogWarning(e);
					C2VDebug.LogWarningCategory("PlatformController", $"OnUpdate Exception");
				}
			}
			else if (_connectType == eConnectType.DISCONNECTED)
			{
				CheckInitConnect().Forget();
			}
			else if (!_daemon?.Connected ?? false)
			{
				if (!IsDaemonExists())
					Process.Start(SocketUtils.DaemonPath);
				CheckInitConnect().Forget();
			}
		}
		
		public void Terminate()
		{
			C2VDebug.LogWarningCategory("PlatformController", $"Terminate SocketClientController");
			CloseConnect(true);
		}
		
#region IPC
		private          eConnectType                   _connectType = eConnectType.DISCONNECTED;
		private          Socket                         _daemon;
		private          IPEndPoint                     _endPoint;
		private readonly byte[]                         _receivePacket = new byte[4144];
		private readonly ConcurrentQueue<PacketMessage> _messageQueue  = new();

		private UniTask CheckInitConnect()
		{
			_startInitializeAction?.Invoke();

			if (_endPoint == null)
				_endPoint = new IPEndPoint(IPAddress.Parse(SocketUtils.Ip), SocketUtils.SocketPort);

			try
			{
				_connectType = eConnectType.CONNECTING_USER;
				_daemon = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				var result = _daemon.BeginConnect(_endPoint, null, null);
				var success = result.AsyncWaitHandle.WaitOne(1000, true);
				if (success)
				{
					_connectType = eConnectType.CONNECTED;
					_endInitializeAction?.Invoke();
					_daemon.EndConnect(result);
					_daemon.BeginReceive(_receivePacket, 0, _receivePacket.Length, SocketFlags.None, ReceiveMessage, null);
				}
				else
				{
					_connectType = eConnectType.DISCONNECTED;
					_daemon.Close();
					_daemon = null;
				}
			}
			catch
			{
				_connectType = eConnectType.DISCONNECTED;
				_daemon.Close();
				_daemon = null;
			}

			return UniTask.Delay(1);
		}
		
		private void OpenConnect()
		{
			C2VDebug.LogCategory("PlatformController", $"Start Connect");
			_startInitializeAction?.Invoke();

			if (_daemon != null)
			{
				_daemon?.Close();
				_daemon = null;
			}

			if (_endPoint == null)
				_endPoint = new IPEndPoint(IPAddress.Parse(SocketUtils.Ip), SocketUtils.SocketPort);

			_connectType = eConnectType.CONNECTING_USER;
			_daemon = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_daemon.BeginConnect(_endPoint, ConnectAsync, null);
		}
		
		private void ConnectAsync(IAsyncResult result)
		{
			if (_connectType == eConnectType.CONNECTING_USER)
			{
				_endInitializeAction?.Invoke();

				_connectType = eConnectType.CONNECTED;
				_daemon.EndConnect(result);
				_daemon.BeginReceive(_receivePacket, 0, _receivePacket.Length, SocketFlags.None, ReceiveMessage, null);
			}
			else
			{
				_connectType = eConnectType.DISCONNECTED;
				_daemon.Close();
				_daemon = null;
			}
		}
		
		private void CloseConnect(bool forceShutdown = false)
		{
			C2VDebug.LogCategory("PlatformController", $"Close Connect");

			_startCloseAction?.Invoke();

			if (_daemon != null)
			{
				_daemon.Close();
				_daemon = null;
			}

			_connectType = eConnectType.DISCONNECTED;
			_endCloseAction?.Invoke();
		}
		
		public void SendMessage(PacketMessage message)
		{
			try
			{
				C2VDebug.LogCategory("PlatformController", $"Send Packet to {message.Receiver}");

				byte[] messageByte = StructToByteArray(message);
				_daemon.Send(messageByte, 0, messageByte.Length, SocketFlags.None);
			}
			catch (ArgumentNullException e)
			{
				C2VDebug.LogWarning(e);
				C2VDebug.LogWarningCategory("PlatformController", "messageByte is NULL");
			}
			catch (ArgumentOutOfRangeException e)
			{
				C2VDebug.LogWarning(e);
				C2VDebug.LogWarningCategory("PlatformController", "messageByte size is 0");
			}
			catch (SocketException e)
			{
				C2VDebug.LogWarning(e);
				C2VDebug.LogWarningCategory("PlatformController", "Socket Exception");
			}
			catch (ObjectDisposedException e)
			{
				C2VDebug.LogWarning(e);
				C2VDebug.LogWarningCategory("PlatformController", "Socket is Closed");
			}
			catch (Exception e)
			{
				C2VDebug.LogWarning(e);
				C2VDebug.LogWarningCategory("PlatformController", "SendMessage Exception");
			}
		}
		
		private void ReceiveMessage(IAsyncResult result)
		{
			try
			{
				var size = _daemon.EndReceive(result);
				if (size > 0)
				{
					var message = ByteArrayToStruct<PacketMessage>(_receivePacket);
					_messageQueue.Enqueue(message);
					Array.Clear(_receivePacket, 0x0, size);
					_daemon.BeginReceive(_receivePacket, 0, _receivePacket.Length, SocketFlags.None, ReceiveMessage, null);
				}
			}
			catch (InvalidOperationException e)
			{
				C2VDebug.LogWarning(e);
				C2VDebug.LogWarningCategory("PlatformController", $"{SocketUtils.Name} - Wrong size buffer");
			}
			catch (ArgumentException e)
			{
				C2VDebug.LogWarning(e);
				C2VDebug.LogWarningCategory("PlatformController", $"{SocketUtils.Name} - messageType Invalid");
			}
			catch (Exception e)
			{
				C2VDebug.LogWarning(e);
				C2VDebug.LogWarningCategory("PlatformController", $"{SocketUtils.Name} - ReceiveAsync Exception");
			}
		}
#endregion // IPC

#region ACTION
		private Action                 _startInitializeAction     = null;
        private Action                 _endInitializeAction       = null;
        private Action                 _startCloseAction          = null;
        private Action                 _endCloseAction            = null;
        private Action                 _startDisconnectAction     = null;
        private Action                 _endDisconnectAction       = null;
        private Action                 _connectionErrorAction     = null;
        private Action<string, string> _receiveMessageAction      = null;
        private Action<string, string> _receiveQuickConnectAction = null;

        public event Action StartInitializeEvent
        {
            add
            {
                _startInitializeAction -= value;
                _startInitializeAction += value;
            }
            remove => _startInitializeAction -= value;
        }
        
        public event Action EndInitializeEvent
        {
            add
            {
                _endInitializeAction -= value;
                _endInitializeAction += value;
            }
            remove => _endInitializeAction -= value;
        }
        
        public event Action StartCloseEvent
        {
            add
            {
                _startCloseAction -= value;
                _startCloseAction += value;
            }
            remove => _startCloseAction -= value;
        }
        
        public event Action EndCloseEvent
        {
            add
            {
                _endCloseAction -= value;
                _endCloseAction += value;
            }
            remove => _endCloseAction -= value;
        }
        
        public event Action StartDisconnectEvent
        {
            add
            {
                _startDisconnectAction -= value;
                _startDisconnectAction += value;
            }
            remove => _startDisconnectAction -= value;
        }
        
        public event Action EndDisconnectEvent
        {
            add
            {
                _endDisconnectAction -= value;
                _endDisconnectAction += value;
            }
            remove => _endDisconnectAction -= value;
        }
        
        public event Action ConnectionErrorEvent
        {
            add
            {
                _connectionErrorAction -= value;
                _connectionErrorAction += value;
            }
            remove => _connectionErrorAction -= value;
        }

        public event Action<string, string> ReceiveMessageEvent
        {
	        add
	        {
		        _receiveMessageAction -= value;
		        _receiveMessageAction += value;
	        }
	        remove => _receiveMessageAction -= value;
        }

        public event Action<string, string> ReceiveQuickConnectEvent
        {
	        add
	        {
		        _receiveQuickConnectAction -= value;
		        _receiveQuickConnectAction += value;
	        }
	        remove => _receiveQuickConnectAction -= value;
        }
#endregion // ACTION

#region UTILS
		public  bool IsDaemonNotFound = false;
		private bool IsDaemonExists() => Process.GetProcessesByName("DaemonConsole").Length > 0;

		private byte[] StructToByteArray(object obj)
		{
			int size = Marshal.SizeOf(obj);
			byte[] arr = new byte[size];
			IntPtr ptr = Marshal.AllocHGlobal(size);

			Marshal.StructureToPtr(obj, ptr, true);
			Marshal.Copy(ptr, arr, 0, size);
			Marshal.FreeHGlobal(ptr);
			return arr;
		}

		private T ByteArrayToStruct<T>(byte[] buffer) where T : struct
		{
			int size = Marshal.SizeOf(typeof(T));
			if (size > buffer.Length)
			{
				throw new InvalidOperationException();
			}

			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.Copy(buffer, 0, ptr, size);
			T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
			Marshal.FreeHGlobal(ptr);
			return obj;
		}
#endregion
	}
}
