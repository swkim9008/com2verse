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
		public static IChannel CreateInstance(string channelId, string? loginToken, ChannelType protocolType, User loginUser, RTCChannelInfo? responseInfo, eUserRole role)
		{
			var mediaSdkUser      = MediaSdkChannelAdapter.GetMediaSdkUser(channelId, loginUser, role);
			var rtcChannelContext = MediaSdkChannelAdapter.GetRtcChannelContext(channelId, loginToken, protocolType, responseInfo);
			var rtcChannelInfo    = new MediaSdkChannelInfo(mediaSdkUser, rtcChannelContext);

			var context          = rtcChannelInfo.Context;
			var channelType      = context.Profile.GetChannelType();
			var channelDirection = context.Direction.GetChannelDirection();
			var userRole         = rtcChannelInfo.Localuser.Role.GetUserRole();

			var channelInfo = new ChannelInfo(channelId, channelType, channelDirection, loginUser, userRole);

			return new MediaChannel(channelInfo, rtcChannelInfo);
		}
	}
}
