/*===============================================================
* Product:		Com2Verse
* File Name:	ChatManager_Network.cs
* Developer:	haminjeong
* Date:			2022-07-19 18:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using Protocols.GameLogic;
using WebSocketSharp;
using Localization = Com2Verse.UI.Localization;

namespace Com2Verse.Chat
{
	public sealed partial class ChatManager
	{
#region Connectivity
		private const ushort AuthorizationError = 4001;
		private const ushort UnexpectedCloseError = 1005;
		
		private int _retryCounter = 0;
		
		public bool IsConnected { get; private set; } = false;

		public void Connect()
		{
			if (_chat.State == WebSocketClient.eWebSocketState.CONNECTING || _chat.State == WebSocketClient.eWebSocketState.OPEN) return;
			
			_chat.OnConnectComplete -= OnConnectComplete;
			_chat.OnDisconnectComplete -= OnDisconnectComplete;
			
			_chat.OnConnectComplete += OnConnectComplete;
			_chat.OnDisconnectComplete += OnDisconnectComplete;

			var currentUserData = User.Instance.CurrentUserData;
			if (LoginManager.Instance.IsHiveLogin())
				_chat.ConnectChatServerV2(currentUserData.ID, currentUserData.PlayerId, currentUserData.AccessToken);
			else
				_chat.ConnectChatServerV1(currentUserData.ID, currentUserData.AccessToken);
		}

		public void Disconnect()
		{
			NetworkManager.Instance.OnDisconnected -= Disconnect;
			_chat.DisconnectChatServer();
		}

		private void RetryOnTokenChanged(string accessToken)
		{
			BaseUserData.OnAccessTokenChanged -= RetryOnTokenChanged;
			
			Connect();
		}

		private async UniTask RefreshTokenAndRetryConnect()
		{
			BaseUserData.OnAccessTokenChanged -= RetryOnTokenChanged;
			BaseUserData.OnAccessTokenChanged += RetryOnTokenChanged;

			await UniTask.Delay(_retryCounter);
			_retryCounter = _retryCounter * 2 + 1000;
			
			if (_chat.State == WebSocketClient.eWebSocketState.CONNECTING || _chat.State == WebSocketClient.eWebSocketState.OPEN) return;
			await LoginManager.Instance.TryRefreshToken(() => {});
		}
		
		private async UniTask RetryConnect()
		{
			await UniTask.Delay(_retryCounter);
			_retryCounter = _retryCounter * 2 + 1000;
			
			if (_chat.State == WebSocketClient.eWebSocketState.CONNECTING || _chat.State == WebSocketClient.eWebSocketState.OPEN) return;
			Connect();
		}

		private void OnConnectComplete()
		{
			_chat.OnConnectComplete -= OnConnectComplete;
			_retryCounter = 0;
			IsConnected             =  true;

			Network.GameLogic.PacketReceiver.Instance.EnterChattingAreaNotify += _chat.OnEnterChattingAreaNotify;
			Network.GameLogic.PacketReceiver.Instance.ExitChattingAreaNotify  += _chat.OnExitChattingAreaNotify;
			UI.PacketReceiver.Instance.WhisperChattingResponse                += OnWhisperChatResponse;
			UI.PacketReceiver.Instance.WhisperChattingNotify                  += _chat.OnWhisperChattingNotify;

			_chat.OnEnterUserInGroup += OnEnterUserInGroup;
		}

		private void OnDisconnectComplete(CloseEventArgs e)
		{
			_chat.OnDisconnectComplete -= OnDisconnectComplete;

			Network.GameLogic.PacketReceiver.Instance.EnterChattingAreaNotify -= _chat.OnEnterChattingAreaNotify;
			Network.GameLogic.PacketReceiver.Instance.ExitChattingAreaNotify  -= _chat.OnExitChattingAreaNotify;
			UI.PacketReceiver.Instance.WhisperChattingResponse                -= OnWhisperChatResponse;
			UI.PacketReceiver.Instance.WhisperChattingNotify                  -= _chat.OnWhisperChattingNotify;

			_chat.OnEnterUserInGroup -= OnEnterUserInGroup;
			IsConnected = false;
			
			if (e != null)
			{
				if (e.Code == AuthorizationError)
				{
					RefreshTokenAndRetryConnect().Forget();
					return;
				}
				else if (e.Code == UnexpectedCloseError)
				{
					RetryConnect().Forget();
					return;
				}
			}

			ReleaseObjects();
		}

		public void SendAreaMessage(string message)
		{
			var userName = User.Instance.CurrentUserData.UserName;

			if (string.IsNullOrEmpty(userName)) return;

			if (IsConnected)
				_chat.SendAreaMessage(message, userName);
			else
				Connect();
		}

		public void SendPrivateMessage(string message, string targetId)
		{
			_chat.SendPrivateMessage(message, targetId);
		}
#endregion Connectivity

#region Chat Callback Event
		private void OnReceivedAreaMessage(LinkedList<ChatCoreBase.MessageInfo> messageInfos)
		{
			if (messageInfos == null)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "messageInfos is null");
				return;
			}

			_chatViewer.SyncAreaChatMessages(messageInfos);

			var lastMessage = messageInfos.Last;
			if (lastMessage != null)
				CreateChatSpeechBubble(lastMessage.Value);
		}

		private void CreateChatSpeechBubble(ChatCoreBase.MessageInfo message)
		{
			var mapController = MapController.InstanceOrNull;
			if (!mapController.IsReferenceNull())
			{
				var mapObject = mapController!.GetObjectByUserID(message.UserID);
				CreateSpeechBubble(mapObject as MapObject, message.Message, message.Type);
			}
		}

		private void OnReceivedWhisperMessage(LinkedList<ChatCoreBase.MessageInfo> messageInfos)
		{
			_chatViewer.SyncWhisperMessages(messageInfos);
		}

		private void OnWhisperChatResponse(Protocols.CommonLogic.WhisperChattingResponse response)
		{
			switch (response.ResultType)
			{
				case Protocols.CommonLogic.WhisperResultType.Success:
					ChatCoreBase.MessageInfo info = new ChatCoreBase.MessageInfo()
					{
						Message    = response.Contents,
						Type       = ChatCoreBase.eMessageType.WHISPER,
						SenderName = response.TargetAvatarName,
						TimeString = DateTime.Now.ToString("HH:mm"),
						UserID     = User.Instance.CurrentUserData.ID,
					};
					_chat.OnWhisperChattingResponse(info);
					break;
				case Protocols.CommonLogic.WhisperResultType.NotFoundUser:
					UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Chat_Whisper_NoneTarget_Msg"));
					break;
			}
		}
#endregion Chat Callback Event

		private void OnEnterUserInGroup(string groupId, long userId, string userName)
		{
			if (!IsConnected)
				return;

			if (userId == User.Instance.CurrentUserData.ID)
				SetAreaMove(groupId);
			NoticeManager.Instance.OnEnterChattingUserInGroup(userId, userName);
		}
	}
}
