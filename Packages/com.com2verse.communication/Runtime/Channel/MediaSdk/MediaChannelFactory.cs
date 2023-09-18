/*===============================================================
* Product:		Com2Verse
* File Name:	MediaChannelFactory.cs
* Developer:	urun4m0r1
* Date:			2022-05-11 19:45
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Protocols.Communication;
using MediaSdkUser = Com2Verse.Solution.UnityRTCSdk.User;
using MediaSdkChannelInfo = Com2Verse.Solution.UnityRTCSdk.RTCChannelInfo;

namespace Com2Verse.Communication.MediaSdk
{
	public static partial class MediaChannelFactory
	{
#if ENABLE_CHEATING
		public static Cheat.DummyChannelDecorator CreateDebugInstance(User user, string channelId, string? loginToken)
		{
			var response = GetJoinChannelDebugResponse(channelId);
			var channel  = CreateInstance(response.ChannelId!, loginToken, response.ChannelType, user, response.RtcChannelInfo, eUserRole.DEFAULT);
			return new Cheat.DummyChannelDecorator(channel);
		}

		private static RTCChannelInfo GetDebugChannelInfo() => new()
		{
			ServerUrl     = "43.133.64.22:5551",
			MediaName     = "c2v-devint-sfu-t01.com2us.kr",
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

		private static JoinChannelResponse GetJoinChannelDebugResponse(string channelId) => new()
		{
			ChannelId      = channelId,
			ChannelType    = ChannelType.Meeting,
			RtcChannelInfo = GetDebugChannelInfo(),
			FieldId        = 0,
			MapId          = 0,
		};
#endif // ENABLE_CHEATING
	}
}
