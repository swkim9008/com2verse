/*===============================================================
 * Product:		Com2Verse
 * File Name:	LocalChannelTrackManager.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-14 22:38
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Communication.MediaSdk
{
	/// <inheritdoc cref="LocalTrackManager"/>
	internal sealed class LocalChannelTrackManager : LocalTrackManager
	{
		public LocalChannelTrackManager(RtcTrackController trackController, IChannelLocalUser publisher) : base(trackController, publisher) { }

		protected override void UpdateTrackConnection(eTrackType trackType, bool isModuleContentAvailable, bool isConnectionTarget)
		{
			if (isModuleContentAvailable && isConnectionTarget)
				GetOrCreateLocalTrack(trackType).Connector.TryForceConnect();
			else
				DestroyLocalTrack(trackType);
		}
	}
}
