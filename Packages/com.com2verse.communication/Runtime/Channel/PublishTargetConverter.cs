/*===============================================================
 * Product:		Com2Verse
 * File Name:	PublishTargetConverter.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-10 11:48
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

#define ENABLE_PEER_CONNECTION_ON_BILATERAL_P2P

namespace Com2Verse.Communication
{
	internal static class PublishTargetConverter
	{
		internal static ePublishTarget GetPublishTarget(eChannelType channelType, eChannelDirection channelDirection)
		{
			return channelType switch
			{
				eChannelType.P2P_DIRECT   => GetP2PPublishTarget(channelDirection),
				eChannelType.MEDIA_SERVER => GetMediaPublishTarget(channelDirection),
				_                         => ePublishTarget.UNDEFINED,
			};
		}

		private static ePublishTarget GetP2PPublishTarget(eChannelDirection channelDirection) => channelDirection switch
		{
#if ENABLE_PEER_CONNECTION_ON_BILATERAL_P2P
			eChannelDirection.BILATERAL => ePublishTarget.PEER,
#else
			eChannelDirection.BILATERAL => ePublishTarget.CHANNEL,
#endif // ENABLE_PEER_CONNECTION_ON_BILATERAL_P2P
			eChannelDirection.OUTGOING => ePublishTarget.PEER,
			eChannelDirection.INCOMING => ePublishTarget.NONE,
			_                          => ePublishTarget.UNDEFINED,
		};
		
		private static ePublishTarget GetMediaPublishTarget(eChannelDirection channelDirection) => channelDirection switch
		{
			eChannelDirection.BILATERAL => ePublishTarget.CHANNEL,
			eChannelDirection.OUTGOING => ePublishTarget.CHANNEL,
			eChannelDirection.INCOMING => ePublishTarget.NONE,
			_                          => ePublishTarget.UNDEFINED,
		};
	}
}
