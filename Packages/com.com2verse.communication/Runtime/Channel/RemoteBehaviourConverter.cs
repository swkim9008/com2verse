/*===============================================================
 * Product:		Com2Verse
 * File Name:	RemoteBehaviourConverter.cs
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
	internal static class RemoteBehaviourConverter
	{
		internal static bool IsP2PChannelHost(eChannelType channelType, eChannelDirection channelDirection, eUserRole userRole)
		{
			return channelType      == eChannelType.P2P_DIRECT
			    && channelDirection != eChannelDirection.BILATERAL
			    && userRole         == eUserRole.HOST;
		}

		internal static bool IsP2PChannelGuest(eChannelType channelType, eChannelDirection channelDirection, eUserRole userRole)
		{
			return channelType      == eChannelType.P2P_DIRECT
			    && channelDirection != eChannelDirection.BILATERAL
			    && userRole         == eUserRole.GUEST;
		}

		internal static eRemoteBehaviour GetRemoteBehaviour(eChannelType channelType, eChannelDirection channelDirection, eUserRole userRole)
		{
			return channelType switch
			{
				eChannelType.P2P_DIRECT   => GetP2PRemoteBehaviour(channelDirection, userRole),
				eChannelType.MEDIA_SERVER => eRemoteBehaviour.SUBSCRIBE_ONLY,
				_                         => eRemoteBehaviour.UNDEFINED,
			};
		}

		private static eRemoteBehaviour GetP2PRemoteBehaviour(eChannelDirection channelDirection, eUserRole userRole) => channelDirection switch
		{
#if ENABLE_PEER_CONNECTION_ON_BILATERAL_P2P
			eChannelDirection.BILATERAL => eRemoteBehaviour.FULL,
#else
			eChannelDirection.BILATERAL => eRemoteBehaviour.SUBSCRIBE_ON_DEMAND,
#endif // ENABLE_PEER_CONNECTION_ON_BILATERAL_P2P
			eChannelDirection.OUTGOING => eRemoteBehaviour.PUBLISH_ONLY,
			eChannelDirection.INCOMING => GetP2PIncomingRemoteBehaviour(userRole),
			_                          => eRemoteBehaviour.UNDEFINED,
		};

		private static eRemoteBehaviour GetP2PIncomingRemoteBehaviour(eUserRole userRole) => userRole switch
		{
			eUserRole.HOST  => eRemoteBehaviour.SUBSCRIBE_ON_DEMAND,
			eUserRole.GUEST => eRemoteBehaviour.NONE,
			_               => eRemoteBehaviour.UNDEFINED,
		};
	}
}
