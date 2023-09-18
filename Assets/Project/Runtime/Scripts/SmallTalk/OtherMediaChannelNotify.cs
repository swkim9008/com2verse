/*===============================================================
 * Product:		Com2Verse
 * File Name:	OtherMediaChannelNotify.cs
 * Developer:	urun4m0r1
 * Date:		2023-05-09 21:53
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Protocols.Communication;

namespace Com2Verse.SmallTalk
{
	public sealed class OtherMediaChannelNotify
	{
		public long            OwnerId        { get; init; }
		public string?         ChannelId      { get; init; }
		public RTCChannelInfo? RtcChannelInfo { get; init; }
	}
}
