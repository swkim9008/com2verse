/*===============================================================
 * Product:		Com2Verse
 * File Name:	ChannelTypeConverter.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-07 15:28
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Solution.UnityRTCSdk;

namespace Com2Verse.Communication.MediaSdk
{
	internal static class ChannelTypeConverter
	{
		internal static eChannelType GetChannelType(this CHANNEL_PROFILE channelProfile)
		{
			return channelProfile switch
			{
				CHANNEL_PROFILE.Conference => eChannelType.MEDIA_SERVER,
				CHANNEL_PROFILE.P2P        => eChannelType.P2P_DIRECT,
				_                          => eChannelType.UNDEFINED,
			};
		}

		internal static CHANNEL_PROFILE? GetChannelProfile(this eChannelType channelType)
		{
			return channelType switch
			{
				eChannelType.MEDIA_SERVER => CHANNEL_PROFILE.Conference,
				eChannelType.P2P_DIRECT   => CHANNEL_PROFILE.P2P,
				_                         => null,
			};
		}
	}
}
