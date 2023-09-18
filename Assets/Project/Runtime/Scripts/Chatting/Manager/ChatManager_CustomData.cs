/*===============================================================
* Product:		Com2Verse
* File Name:	ChatManager_CustomData.cs
* Developer:	ksw
* Date:			2023-06-20 17:41
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using System;
using Com2Verse.Network;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Com2Verse.Chat
{
	public sealed partial class ChatManager
	{
		public enum CustomDataType
		{
			NONE,
			FORCED_OUT_NOTIFY = 100,	// Connecting
			HANDS_UP,
			HANDS_DOWN,
			AUTHORITY_CHANGE_NOTIFY,
			TRACK_PUBLISH_REQUEST,
			TRACK_PUBLISH_RESPONSE,
			TRACK_UNPUBLISH_NOTIFY,
			HANDS_STATE_REQUEST,
			RECORD_START_NOTIFY,
			RECORD_END_NOTIFY,
			TIME_EXTENSION_NOTIFY,
			AUDIO_RECORD = 200,
		}

		[Serializable]
		private class CustomData
		{
			[JsonProperty("Type")]
			public CustomDataType Type;
			[JsonProperty("Sender")]
			public long Sender;
			[JsonProperty("Receiver")]
			public long Receiver;
			[JsonProperty("Data")]
			public object Data;
		}

		/// <summary>
		/// 채팅방에 있는 모든 유저에게 노티를 줄때 사용
		/// </summary>
		public void BroadcastCustomNotify(CustomDataType type)
		{
			SendSocketMessage(type, 0, null);
		}

		/// <summary>
		/// 채팅방에 있는 모든 유저에게 데이터를 줄때 사용
		/// </summary>
		public void BroadcastCustomData(CustomDataType type, [CanBeNull] object data)
		{
			SendSocketMessage(type, 0, data);
		}

		/// <summary>
		/// 특정 유저에게 노티만 줄때 사용
		/// </summary>
		public void SendCustomNotify(CustomDataType type, long receiver)
		{
			SendSocketMessage(type, receiver, null);
		}

		/// <summary>
		/// Json으로 복잡한 데이터를 보낼때 사용
		/// </summary>
		public void SendCustomData(CustomDataType type, long receiver, [CanBeNull] object data)
		{
			SendSocketMessage(type, receiver, data);
		}

		private void SendSocketMessage(CustomDataType type, long receiver, [CanBeNull] object data)
		{
			object customData = new CustomData
			{
				Type     = type,
				Sender   = User.Instance.CurrentUserData.ID,
				Receiver = receiver,
				Data     = data,
			};

			if (IsConnected)
				_chat?.SendSocketCommunicationMessage(customData);
			else
				Connect();
		}

		private void OnReceivedCustomData([CanBeNull] object data)
		{
			var jsonBody = data?.ToString();
			if (jsonBody == null)
				return;

			var customData = JsonConvert.DeserializeObject<CustomData>(jsonBody);
			if (customData == null)
				return;

			if (customData.Receiver > 0 && customData.Receiver != User.Instance.CurrentUserData.ID)
				return;

			ProcessCustomDataConnecting(customData.Type, customData.Sender, customData.Receiver, customData.Data);
			ProcessCustomDataAudioRecord(customData.Type, customData.Sender, customData.Receiver, customData.Data);
		}
	}
}
