/*===============================================================
* Product:		Com2Verse
* File Name:	WebSocketClient.cs
* Developer:	jhkim
* Date:			2023-05-01 15:35
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Com2Verse.Logger;
using WebSocketSharp;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;

namespace Com2Verse.Network
{
	public sealed class WebSocketClient : SocketBase, IDisposable
	{
#region Variables
		public enum eWebSocketState
		{
			NEW,
			CONNECTING,
			OPEN,
			CLOSING,
			CLOSED,
		}

		private                 string _url;
		private static readonly string WebSocketCloseStatusInfoUrl = "https://learn.microsoft.com/ko-kr/dotnet/api/system.net.websockets.websocketclosestatus";
		private static readonly Dictionary<ushort, string> ErrorCodeMap = new Dictionary<ushort, string>
		{
			{1005, "No Error"},
			{1001, "End point Unavailable"},
			{1011, "Internal Server Error"},
			{1003, "Invalid Message Type"},
			{1007, "Invalid PayloadData"},
			{1010, "Mandatory Extension"},
			{1009, "Message TooBig"},
			{1000, "Normal Closure"},
			{1008, "Policy Violation"},
			{1002, "Protocol Error"},
		};
#endregion // Variables

#region Properties
		public eWebSocketState State
		{
			get
			{
				switch (InternalWebSocket?.ReadyState)
				{
					case WebSocketState.New:
						return eWebSocketState.NEW;
					case WebSocketState.Connecting:
						return eWebSocketState.CONNECTING;
					case WebSocketState.Open:
						return eWebSocketState.OPEN;
					case WebSocketState.Closing:
						return eWebSocketState.CLOSING;
					case WebSocketState.Closed:
						return eWebSocketState.CLOSED;
					case null:
					default:
						break;
				}
				return eWebSocketState.CLOSED;
			}
		}

		public delegate void StateChanged(eWebSocketState state);

		public delegate void EventReceived(EventArgs e);

		public event StateChanged  OnStateChanged  = _ => { };
		public event EventReceived OnEventReceived = _ => { };

		public override bool IsConnected => State == eWebSocketState.OPEN;
#endregion // Properties

#region Public Functions
		public void SetUrl(string   url)   => _url = url;
		
		public override void Connect(string url, string token = null)
		{
			if (InternalWebSocket != null)
			{
				switch (InternalWebSocket.ReadyState)
				{
					case WebSocketState.Connecting:
					case WebSocketState.Open:
					case WebSocketState.Closing:
						C2VDebug.LogWarning($"websocket client already connected = {InternalWebSocket.ReadyState}");
						return;
					case WebSocketState.New: // ?
					case WebSocketState.Closed:
						DisposeClient();
						break;
				}
			}

			_url = url;
			base.Connect(_url, token);
			
			InternalWebSocket!.OnOpen -= OnOpen;
			InternalWebSocket.OnOpen += OnOpen;
			InternalWebSocket.OnClose -= OnClose;
			InternalWebSocket.OnClose += OnClose;
			InternalWebSocket.OnError -= OnError;
			InternalWebSocket.OnError += OnError;
			InternalWebSocket.OnMessage -= OnMessage;
			InternalWebSocket.OnMessage += OnMessage;
		}

		private void OnOpen(object sender, EventArgs e)
		{
			OnStateChanged?.Invoke(State);
			OnEventReceived?.Invoke(e);
		}

		private void OnClose(object sender, CloseEventArgs e)
		{
			PrintCloseEventError(e);
			OnStateChanged?.Invoke(State);
			OnEventReceived?.Invoke(e);
		}

		private void OnError(object sender, ErrorEventArgs e)
		{
			OnStateChanged?.Invoke(State);
			OnEventReceived?.Invoke(e);
		}

		private void OnMessage(object sender, MessageEventArgs e)
		{
			OnEventReceived?.Invoke(e);
		}

		public override void Stop()
		{
			InternalWebSocket?.Close();
			InternalWebSocket = null;
		}

		public void Send(string   message) => InternalWebSocket?.Send(message);

		protected override bool Send(LogicPacket.IBuffer data)
		{
			var result = base.Send(data);
			if (!result) return false;
			InternalWebSocket?.Send(data!.Array);
			return true;
		}
		public void Send(Stream   stream, int length) => InternalWebSocket?.Send(stream, length);
		public void Send(FileInfo fileInfo) => InternalWebSocket?.Send(fileInfo);

		public void SendAsync(string   message,  Action<bool> onComplete)                      => InternalWebSocket?.SendAsync(message, onComplete);
		public void SendAsync(byte[]   data,     Action<bool> onComplete)                      => InternalWebSocket?.SendAsync(data,    onComplete);
		public void SendAsync(Stream   stream,   int          length, Action<bool> onComplete) => InternalWebSocket?.SendAsync(stream,  length, onComplete);
		public void SendAsync(FileInfo fileInfo, Action<bool> onComplete)                      => InternalWebSocket?.SendAsync(fileInfo, onComplete);
#endregion // Public Functions

		public void Dispose()
		{
			DisposeClient();
		}

		private void DisposeClient()
		{
			(InternalWebSocket as IDisposable)?.Dispose();
		}

		[Conditional("UNITY_EDITOR")]
		private void PrintCloseEventError(CloseEventArgs e)
		{
			if (ErrorCodeMap.TryGetValue(e.Code, out var message))
				C2VDebug.Log($"WebSocket Closed ({e.Code}) {message}\n{WebSocketCloseStatusInfoUrl}");
			else
				C2VDebug.Log($"WebSocket Closed ({e.Code})\n{WebSocketCloseStatusInfoUrl}");
		}
	}
}
