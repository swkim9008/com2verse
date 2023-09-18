/*===============================================================
* Product:		Com2Verse
* File Name:	WebSocketHelperUIModel.cs
* Developer:	jhkim
* Date:			2023-05-01 15:18
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Chat;
using Com2Verse.Network;
using Com2VerseEditor.UGC.UIToolkitExtension;
using UnityEngine;

namespace Com2Verse
{
	public class WebSocketHelperUIModel : EditorWindowExModel
	{
#region Variables
		[SerializeField] private List<ConnectionInfo> _connectionInfos = new();
		[SerializeField] private string _inputLabel;
		[SerializeField] private string _inputUrl;
		[SerializeField] private string _connectionState;
		[SerializeField] private string _sendText;

		// Chat
		[SerializeField] private ChatCoreCom2Verse _chatClient;
		[SerializeField] private string _chatServerAddr = "test-ocm.com2verse.com";
		[SerializeField] private string _chatUserId = "1";
		[SerializeField] private int _chatDeviceId = 1;
		[SerializeField] private int _chatAppId = 1;
		[SerializeField] private string _chatMessage;
		[SerializeField] private string _chatArea;
#endregion // Variables

#region Properties
		public IReadOnlyList<ConnectionInfo> ConnectionInfos => _connectionInfos;
		public string ConnectionState
		{
			set => _connectionState = value;
		}

		public string SendText => _sendText;

		// Chat
		public ChatCoreCom2Verse ChatClient
		{
			get => _chatClient;
			set => _chatClient = value;
		}

		public string ChatServerAddr => _chatServerAddr;
		public string ChatUserId => _chatUserId;
		public int ChatDeviceId => _chatDeviceId;
		public int ChatAppId => _chatAppId;
		public string ChatMessage => _chatMessage;
		public string ChatArea
		{
			get => _chatArea;
			set => _chatArea = value;
		}
#endregion // Properties

#region Connection Info
		public bool TryGetConnectionInfo(int idx, out ConnectionInfo result)
		{
			result = default;
			if (idx < 0 || idx >= _connectionInfos.Count) return false;

			result = _connectionInfos[idx];
			return true;
		}

		public void AddConnectionInfos(ConnectionInfo info)
		{
			info.SendMessages ??= new List<string>();
			info.ReceiveMessages ??= new List<string>();

			_connectionInfos.Add(info);
		}

		public void RemoveConnectionInfos(int idx)
		{
			if (idx >= _connectionInfos.Count) return;

			_connectionInfos.RemoveAt(idx);
		}
		public bool SaveConnectionInfo(int idx)
		{
			if (TryGetConnectionInfo(idx, out var info))
			{
				info.Label = _inputLabel;
				info.Url = _inputUrl;

				_connectionInfos[idx] = info;
				return true;
			}

			return false;
		}

		public int GetLastConnectionInfoIdx() => _connectionInfos.Count() - 1;
		public void SetClient(int idx, WebSocketClient client)
		{
			if (idx < _connectionInfos.Count)
			{
				var item = _connectionInfos[idx];
				item.Client = client;
				_connectionInfos[idx] = item;
			}
		}

		public void Refresh(ConnectionInfo info)
		{
			_inputLabel = info.Label;
			_inputUrl = info.Url;
			ConnectionState = info.Client == null ? "NONE" : info.Client.State.ToString();
		}
#endregion // Connection Info

#region Send Messages
		public void ClearSendText() => _sendText = string.Empty;
#endregion // Send Messages

		public void ClearTemporaryField()
		{
			_inputLabel = string.Empty;
			_inputUrl = string.Empty;
			_connectionState = string.Empty;
			_sendText = string.Empty;
		}
#region Data
		[Serializable]
		public struct ConnectionInfo
		{
			public string Label;
			public string Url;
			public WebSocketClient Client;
			public List<string> SendMessages;
			public List<string> ReceiveMessages;

			public void ClearSendMessages() => SendMessages?.Clear();
			public void AddSendMessage(string mesage) => SendMessages?.Add(mesage);
			public string GetSendMessage(int idx) => idx < SendMessages?.Count ? SendMessages[idx] : string.Empty;

			public void ClearReceiveMessages() => ReceiveMessages?.Clear();
			public void AddReceiveMessage(string message) => ReceiveMessages?.Add(message);
			public string GetReceiveMessage(int idx) => idx < ReceiveMessages?.Count ? ReceiveMessages[idx] : string.Empty;
		}
#endregion // Data

		public void DisposeAll()
		{
			foreach (var connectionInfo in _connectionInfos)
				connectionInfo.Client?.Stop();
		}
	}
}
