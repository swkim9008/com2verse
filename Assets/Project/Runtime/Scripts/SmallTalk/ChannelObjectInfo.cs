/*===============================================================
 * Product:		Com2Verse
 * File Name:	ChannelObjectInfo.cs
 * Developer:	urun4m0r1
 * Date:		2023-05-10 00:10
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Communication;
using Com2Verse.Network;

namespace Com2Verse.SmallTalk
{
	public readonly struct ChannelObjectInfo
	{
		public long   OwnerId   => ActiveObject.OwnerID;
		public string ChannelId => Channel.Info.ChannelId;

		public ActiveObject ActiveObject { get; init; }
		public IChannel     Channel      { get; init; }
	}
}
