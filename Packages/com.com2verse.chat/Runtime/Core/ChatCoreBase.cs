/*===============================================================
* Product:		Com2Verse
* File Name:	ChatCoreBase.cs
* Developer:	ksw
* Date:			2022-12-29 13:58
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Logger;
using Com2Verse.Network;
using Newtonsoft.Json;

namespace Com2Verse.Chat
{
	public abstract class ChatCoreBase
	{
		public enum eMessageType
		{
			SYSTEM = 0,
			AREA,
			NEARBY,
			WHISPER,
			AREA_ENTER = 200,
			AREA_EXIT,
		}

		public struct MessageInfo
		{
			public eMessageType Type;

			public long   UserID;
			public string SenderName;
			public string TimeString;
			public string Message;
			public bool   IsMobile;
		}

		public virtual void Initialize()
		{
			ReleaseObjects();
		}

		public virtual void UpdateQueue() { }

		public abstract void SetChatTableData(int chatSaveCount);

		public abstract void ResetProperties();

		public abstract void SendSystemMessage(string message, string time, string senderName);

		public abstract void ReleaseObjects();

#region Network
#region Callback Event
		public abstract event Action<LinkedList<MessageInfo>> OnReceivedAreaMessage;
		public abstract event Action<LinkedList<MessageInfo>> OnReceivedWhisperMessage;
#endregion Callback Event

		protected void OnReceivedString(string receiveString)
		{
			if (string.IsNullOrEmpty(receiveString))
			{
				C2VDebug.LogErrorMethod(GetType().Name, "response string is null or empty!");
				return;
			}

			var responseBase = JsonConvert.DeserializeObject<ChatApi.ResponseBase>(receiveString);
			if (responseBase != null)
			{
				if (responseBase.Error != null)
				{
					OnHandleError(responseBase.Error.Code, responseBase.Error.Msg);
				}
				else
				{
					var type = ChatApi.GetMessageType(responseBase.MessageType);
					OnHandleMessageType(type, receiveString);
				}
			}
		}

		protected abstract void OnHandleMessageType(ChatApi.eMessageType type, string messageString);
		protected virtual  void OnHandleError(int code, string message) { }

		protected virtual void ChatServerEventReceived(EventArgs e) { }

		public virtual void OnWhisperChattingResponse(Protocols.CommonLogic.WhisperChattingResponse notify) { }

		public virtual void OnWhisperChattingNotify(Protocols.CommonLogic.WhisperChattingNotify notify) { }

		protected virtual void ChatServerStateChanged(WebSocketClient.eWebSocketState state) { }

		public abstract LinkedList<MessageInfo> GetAreaChatMessages();
		public abstract LinkedList<MessageInfo> GetWhisperMessages();
		public abstract LinkedList<MessageInfo> GetAllMessages();
#endregion Network
	}
}
