/*===============================================================
* Product:		Com2Verse
* File Name:	ChatManager_Connecting.cs
* Developer:	ksw
* Date:			2023-06-20 17:41
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Communication;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Com2Verse.Chat
{
	public sealed partial class ChatManager
	{
		[Serializable]
		public class TrackPublishResponseData
		{
			[JsonProperty("TrackType")]
			public eTrackType TrackType;
			[JsonProperty("Result")]
			public bool Result;
		}

		[Serializable]
		public class AuthorityChangedNotifyData
		{
			[JsonProperty("ChangedUser")]
			public long ChangedUser;

			[JsonProperty("OldAuthority")]
			public int OldAuthority;

			[JsonProperty("NewAuthority")]
			public int NewAuthority;
		}

		[Serializable]
		public class ForcedOutNotifyData
		{
			[JsonProperty("TargetUser")]
			public long TargetUser;
		}

		public event Action<long, long> OnForcedOutNotify;

		public event Action<long> OnHandsUpNotify;
		public event Action<long> OnHandsDownNotify;
		public event Action       OnHandsStateRequest;

		public event Action<long, eTrackType>       OnTrackPublishRequest;
		public event Action<long, eTrackType, bool> OnTrackPublishResponse;
		public event Action<long, eTrackType>       OnTrackUnpublishNotify;

		public event Action<long, int, int> OnAuthorityChangedNotify;

		public event Action OnRecordStartNotify;
		public event Action OnRecordEndNotify;
		public event Action OnTimeExtensionNotify;

		private void ProcessCustomDataConnecting(CustomDataType type, long sender, long receiver, [CanBeNull] object data)
		{
			switch (type)
			{
				case CustomDataType.FORCED_OUT_NOTIFY:
				{
					var jsonBody = data?.ToString();
					if (jsonBody == null)
						return;

					var target = JsonConvert.DeserializeObject<ForcedOutNotifyData>(jsonBody);
					if (target == null)
						return;
					OnForcedOutNotify?.Invoke(sender, target.TargetUser);
					break;
				}
				case CustomDataType.HANDS_UP:
					OnHandsUpNotify?.Invoke(sender);
					break;
				case CustomDataType.HANDS_DOWN:
					OnHandsDownNotify?.Invoke(sender);
					break;
				case CustomDataType.TRACK_PUBLISH_REQUEST:
				{
					var jsonBody = data?.ToString();
					if (jsonBody == null)
						return;

					var trackType = JsonConvert.DeserializeObject<eTrackType>(jsonBody);
					OnTrackPublishRequest?.Invoke(sender, trackType);
					break;
				}
				case CustomDataType.TRACK_PUBLISH_RESPONSE:
				{
					var jsonBody = data?.ToString();
					if (jsonBody == null)
						return;

					var trackPublishResponseData = JsonConvert.DeserializeObject<TrackPublishResponseData>(jsonBody);
					if (trackPublishResponseData == null)
						return;
					OnTrackPublishResponse?.Invoke(sender, trackPublishResponseData.TrackType, trackPublishResponseData.Result);
					break;
				}
				case CustomDataType.TRACK_UNPUBLISH_NOTIFY:
				{
					var jsonBody = data?.ToString();
					if (jsonBody == null)
						return;

					var trackType = JsonConvert.DeserializeObject<eTrackType>(jsonBody);
					OnTrackUnpublishNotify?.Invoke(sender, trackType);
					break;
				}
				case CustomDataType.AUTHORITY_CHANGE_NOTIFY:
				{
					var jsonBody   = data?.ToString();
					if (jsonBody == null)
						return;

					var authorityChangedNotifyData = JsonConvert.DeserializeObject<AuthorityChangedNotifyData>(jsonBody);
					if (authorityChangedNotifyData == null)
						return;
					OnAuthorityChangedNotify?.Invoke(authorityChangedNotifyData.ChangedUser, authorityChangedNotifyData.OldAuthority, authorityChangedNotifyData.NewAuthority);
					break;
				}
				case CustomDataType.HANDS_STATE_REQUEST:
				{
					OnHandsStateRequest?.Invoke();
					break;
				}
				case CustomDataType.RECORD_START_NOTIFY:
				{
					OnRecordStartNotify?.Invoke();
					break;
				}
				case CustomDataType.RECORD_END_NOTIFY:
				{
					OnRecordEndNotify?.Invoke();
					break;
				}
				case CustomDataType.TIME_EXTENSION_NOTIFY:
				{
					OnTimeExtensionNotify?.Invoke();
					break;
				}
			}
		}
	}
}
