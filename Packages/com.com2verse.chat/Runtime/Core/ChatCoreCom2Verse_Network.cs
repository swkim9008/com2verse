/*===============================================================
* Product:		Com2Verse
* File Name:	ChatCoreCom2Verse_Network.cs
* Developer:	ksw
* Date:			2022-12-29 13:58
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Logger;
using Com2Verse.Network;
using Protocols.GameLogic;
using WebSocketSharp;

namespace Com2Verse.Chat
{
	public sealed partial class ChatCoreCom2Verse
	{
#region Callback Event
		private Action<LinkedList<MessageInfo>> _onReceivedAreaChat;

		public override event Action<LinkedList<MessageInfo>> OnReceivedAreaMessage
		{
			add
			{
				_onReceivedAreaChat -= value;
				_onReceivedAreaChat += value;
			}
			remove => _onReceivedAreaChat -= value;
		}

		private Action<LinkedList<MessageInfo>> _onReceivedWhisperMessage;

		public override event Action<LinkedList<MessageInfo>> OnReceivedWhisperMessage
		{
			add
			{
				_onReceivedWhisperMessage -= value;
				_onReceivedWhisperMessage += value;
			}
			remove => _onReceivedWhisperMessage -= value;
		}

		public event Action<string, long, string> OnEnterUserInGroup;
		public event Action<long, string> OnLeaveUserInGroup;
#endregion Callback Event

#region Add Message
		private void AddAreaMessageList(MessageInfo info)
		{
			if (_areaMessages.Count >= _chatSaveCount)
				_areaMessages.RemoveFirst();
			_areaMessages.AddLast(info);
		}

		private void AddWhisperMessageList(MessageInfo info)
		{
			if (_whisperMessages.Count >= _chatSaveCount)
				_whisperMessages.RemoveFirst();
			_whisperMessages.AddLast(info);
		}

		private void AddAllMessageList(MessageInfo info)
		{
			if (_allMessages.Count >= _chatSaveCount)
				_allMessages.RemoveFirst();
			_allMessages.AddLast(info);
		}
#endregion Add Message

		private void HandleAreaChatMessage(bool isMobile)
		{
			var currentAreaMessage = CurrentAreaMessages?.Last();

			if (currentAreaMessage == null)
			{
				_onReceivedAreaChat?.Invoke(_areaMessages);
				return;
			}

			MessageInfo info = new MessageInfo()
			{
				Message    = currentAreaMessage.Comments?[0]?.Text ?? string.Empty,
				Type       = eMessageType.AREA,
				SenderName = currentAreaMessage.SendUserName,
				TimeString = ConvertToDateFormatOnlyTime(currentAreaMessage.MessageTime),
				UserID     = long.Parse(currentAreaMessage.SendUserId ?? "0"),
				IsMobile   = isMobile,
			};
			info.Message = ConvertUrlFromMessage(info.Type, info.Message);

			AddAreaMessageList(info);
			AddAllMessageList(info);
			_onReceivedAreaChat?.Invoke(_areaMessages);
		}

		protected override void ChatServerEventReceived(EventArgs e)
		{
			_chatEventArgsQueue.Enqueue(e);
		}

		/// <summary>
		/// WebSocketClient.cs에 정의된 4개의 이벤트(Open,Close, Error, Message)를 처리한다.
		/// </summary>
		/// <param name="e">이벤트 내용</param>
		private void ProcessReceivedServerEvent(EventArgs e)
		{
			switch (e)
			{
				case CloseEventArgs closeEventArgs:
					C2VDebug.LogCategory("Chatting", "ChatServerEventReceived CloseEventArgs : " + closeEventArgs.Reason);
					break;
				case ErrorEventArgs errorEventArgs:
					C2VDebug.LogCategory("Chatting", "ChatServerEventReceived ErrorException : " + errorEventArgs.Exception + "   Message : " + errorEventArgs.Message);
					break;
				case MessageEventArgs messageEventArgs:
					// C2VDebug.LogCategory("Chatting", "ChatServerEventReceived MessageEventArgs : " + messageEventArgs.Data);
					break;
				default:
					C2VDebug.LogCategory("Chatting", "ChatServerEventReceived OpenEvent");
					break;
			}
		}

		protected override void ChatServerStateChanged(WebSocketClient.eWebSocketState state)
		{
			switch (state)
			{
				case WebSocketClient.eWebSocketState.NEW:
					break;
				case WebSocketClient.eWebSocketState.CONNECTING:
					break;
				case WebSocketClient.eWebSocketState.OPEN:
					C2VDebug.LogCategory("Chatting", "ChatServerStateChanged Open!!");
					break;
				case WebSocketClient.eWebSocketState.CLOSING:
					break;
				case WebSocketClient.eWebSocketState.CLOSED:
					Dispose();
					C2VDebug.LogCategory("Chatting", "ChatServerStateChanged Closed!!");
					break;
				default:
					C2VDebug.LogCategory("Chatting", "ChatServerStateChange OtherEvent : " + state);
					break;
			}
		}

		public override LinkedList<MessageInfo> GetAreaChatMessages() => _areaMessages;

		public override LinkedList<MessageInfo> GetWhisperMessages() => _whisperMessages;

		public override LinkedList<MessageInfo> GetAllMessages() => _allMessages;

		public void OnEnterChattingAreaNotify(EnterChattingAreaNotify notify)
		{
			C2VDebug.Log($"OnEnterChattingAreaNotify {notify.ChattingAreaName}, {notify.ChattingAreaGroupId}");
			EnterAreaChat(notify.ChattingAreaGroupId);
		}

		public void OnExitChattingAreaNotify(ExitChattingAreaNotify notify)
		{
			C2VDebug.Log($"OnExitChattingAreaNotify {notify.ChattingAreaName}, {notify.ChattingAreaGroupId}");
			ExitAreaChat(notify.ChattingAreaGroupId);
		}

#region Whisper
		public void OnWhisperChattingResponse(MessageInfo info)
		{
			info.Message = ConvertUrlFromMessage(info.Type, info.Message);
			AddWhisperMessageList(info);
			AddAllMessageList(info);
			_onReceivedWhisperMessage?.Invoke(_whisperMessages);
		}

		public override void OnWhisperChattingNotify(Protocols.CommonLogic.WhisperChattingNotify notify)
		{
			MessageInfo info = new MessageInfo()
			{
				Message    = notify.Contents,
				Type       = eMessageType.WHISPER,
				SenderName = notify.SendingAvatarName,
				TimeString = DateTime.Now.ToString("HH:mm"),
				// UserID     = long.Parse(currentAreaMessage.Last().SendUserId),
			};
			info.Message = ConvertUrlFromMessage(info.Type, info.Message);

			AddWhisperMessageList(info);
			AddAllMessageList(info);
			_onReceivedWhisperMessage?.Invoke(_whisperMessages);
		}
#endregion Whisper
	}
}
