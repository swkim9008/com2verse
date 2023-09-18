/*===============================================================
* Product:		Com2Verse
* File Name:	MediaSdkChannelAdapter.cs
* Developer:	urun4m0r1
* Date:			2022-09-08 18:04
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Solution.UnityRTCSdk;
using Protocols.OfficeMeeting;
using RTCChannelInfo = Protocols.OfficeMeeting.RTCChannelInfo;

namespace Com2Verse.Communication.MediaSdk
{
	public static partial class MediaSdkChannelAdapter
	{
		public static RTCChannelContext GetRtcChannelContext(string channelId, string? loginToken, ChannelType channelType, RTCChannelInfo? responseInfo)
		{
			var channelContext = new RTCChannelContext
			{
				ChannelId  = channelId,
				Token      = loginToken,
				Password   = String.Empty,
				Profile    = Convert(channelType),
				ServerUrl  = responseInfo?.ServerUrl,
				ServerName = responseInfo?.MediaName,
				IceConfigs = new List<RTCIceServerConfig>(),
				Direction  = responseInfo?.Direction == null ? DIRECTION.Bilateral : Convert(responseInfo.Direction),
			};
			return channelContext;
		}

		private static DIRECTION Convert(Direction direction) =>
			direction switch
			{
				Direction.Incomming => DIRECTION.Incomming,
				Direction.Outgoing  => DIRECTION.Outgoing,
				Direction.Bilateral => DIRECTION.Bilateral,
				_                   => throw new NotImplementedException(),
			};

		private static CHANNEL_PROFILE Convert(ChannelType channelType) => channelType switch
		{
			ChannelType.Meeting      => CHANNEL_PROFILE.Conference,
			ChannelType.SmallTalk    => CHANNEL_PROFILE.P2P,
			ChannelType.P2PCall      => CHANNEL_PROFILE.P2P,
			ChannelType.TeamworkCall => CHANNEL_PROFILE.P2P,
			ChannelType.PartyTalk    => CHANNEL_PROFILE.P2P,
			_                        => throw new NotImplementedException(),
		};

		private static List<RTCIceServerConfig> Convert(IEnumerable<IceServerConfig>? iceServerConfigs)
		{
			var configs = new List<RTCIceServerConfig>();
			if (iceServerConfigs == null) return configs;

			foreach (var iceServerConfig in iceServerConfigs)
			{
				configs.Add(new RTCIceServerConfig
				{
					url        = iceServerConfig.IceServerUrl,
					userName   = iceServerConfig.AccountName,
					credential = iceServerConfig.Credential,
				});
			}

			return configs;
		}
	}
}
