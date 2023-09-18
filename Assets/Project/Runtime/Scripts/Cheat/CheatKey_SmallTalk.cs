#if ENABLE_CHEATING

/*===============================================================
* Product:		Com2Verse
* File Name:	CheatKey.cs
* Developer:	urun4m0r1
* Date:			2022-12-26 15:16
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using Com2Verse.SmallTalk;
using Protocols.Communication;

namespace Com2Verse.Cheat
{
	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static partial class CheatKey
	{
#region DebugChannel
		private static readonly string[] DebugChannelsId = { "room1", "room2", "room3", "room4", "room5", "room6", "room7", "room8", "room9", "room10" };

		[MetaverseCheat("Cheat/SmallTalk/MockSelfMediaChannelNotify")] [HelpText("room1~room10")]
		private static void MockSelfMediaChannelNotify(string channelId = "room1")
		{
			// return if channel id is not valid
			if (Array.IndexOf(DebugChannelsId, channelId) == -1)
				return;

			var selfChannelNotify = GetSelfDebugChannelNotify(channelId);
			SmallTalkDistance.Instance.OnSelfMediaChannelNotify(selfChannelNotify);
		}

		[MetaverseCheat("Cheat/SmallTalk/MockOtherMediaChannelNotify")] [HelpText("1~n", "room1~room10")]
		private static void MockOtherMediaChannelNotify(string otherUserId, string channelId = "room1")
		{
			// return if channel id is not valid
			if (Array.IndexOf(DebugChannelsId, channelId) == -1)
				return;

			var otherUserIdLong = Convert.ToInt64(otherUserId);

			var otherChannelNotify = GetOtherDebugChannelNotify(otherUserIdLong, channelId);
			SmallTalkDistance.Instance.OnOtherMediaChannelNotify(otherChannelNotify);
		}

		private static RTCChannelInfo GetDebugChannelInfo(Direction channelDirection) => new()
		{
			ServerUrl     = "test-media-g.com2verse.com:9000",
			MediaName     = "media-p2p-01",
			Direction     = channelDirection,
			AuthorityCode = AuthorityCode.Host,
			AccessToken   = string.Empty,
			Password      = string.Empty,
			IceServerConfigs =
			{
				new IceServerConfig
				{
					IceServerUrl = "stun:stun.l.google.com:19302",
					AccountName  = string.Empty,
					Credential   = string.Empty,
				},
			},
		};

		private static SelfMediaChannelNotify GetSelfDebugChannelNotify(string channelId) => new()
		{
			ChannelId      = channelId,
			RtcChannelInfo = GetDebugChannelInfo(Direction.Outgoing),
		};

		private static OtherMediaChannelNotify GetOtherDebugChannelNotify(long ownerId, string channelId) => new()
		{
			OwnerId        = ownerId,
			ChannelId      = channelId,
			RtcChannelInfo = GetDebugChannelInfo(Direction.Incomming),
		};
#endregion // DebugChannel

#region ForceEnable
		[MetaverseCheat("Cheat/SmallTalk/" + nameof(EnableDistanceSmallTalk))]
		private static void EnableDistanceSmallTalk()
		{
			SmallTalkDistance.Instance.Enable(SmallTalk.Define.TableIndex.Default);
		}

		[MetaverseCheat("Cheat/SmallTalk/" + nameof(EnableAreaSmallTalk))]
		private static void EnableAreaSmallTalk()
		{
			SmallTalkDistance.Instance.Enable(SmallTalk.Define.TableIndex.AreaBased);
		}

		[MetaverseCheat("Cheat/SmallTalk/" + nameof(DisableSmallTalk))]
		private static void DisableSmallTalk()
		{
			SmallTalkDistance.InstanceOrNull?.Disable();
		}
#endregion // ForceEnable
	}
}
#endif
