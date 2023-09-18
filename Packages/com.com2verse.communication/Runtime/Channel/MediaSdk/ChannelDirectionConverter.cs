/*===============================================================
 * Product:		Com2Verse
 * File Name:	ChannelDirectionConverter.cs
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
	internal static class ChannelDirectionConverter
	{
		internal static eChannelDirection GetChannelDirection(this DIRECTION direction)
		{
			return direction switch
			{
				DIRECTION.Bilateral => eChannelDirection.BILATERAL,
				DIRECTION.Incomming => eChannelDirection.INCOMING,
				DIRECTION.Outgoing  => eChannelDirection.OUTGOING,
				_                   => eChannelDirection.UNDEFINED,
			};
		}

		internal static DIRECTION? GetDirection(this eChannelDirection channelDirection)
		{
			return channelDirection switch
			{
				eChannelDirection.BILATERAL => DIRECTION.Bilateral,
				eChannelDirection.INCOMING  => DIRECTION.Incomming,
				eChannelDirection.OUTGOING  => DIRECTION.Outgoing,
				_                           => null,
			};
		}
	}
}
