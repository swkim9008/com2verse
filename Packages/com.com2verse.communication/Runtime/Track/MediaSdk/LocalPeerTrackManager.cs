/*===============================================================
 * Product:		Com2Verse
 * File Name:	LocalPeerTrackManager.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-14 22:38
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;

namespace Com2Verse.Communication.MediaSdk
{
	internal sealed class LocalPeerTrackManager : LocalTrackManager
	{
		private readonly List<eTrackType> _publishRequestedTracks = new();

		public LocalPeerTrackManager(RtcTrackController trackController, IPeerLocalUser publisher, IPublishableRemoteUser target) : base(trackController, publisher, target)
		{
			trackController.PublishRequested   += OnPublishRequested;
			trackController.UnpublishRequested += OnUnpublishRequested;
		}

		private void OnPublishRequested(IPublishableRemoteUser target, eTrackType trackType)
		{
			if (target != Target)
				return;

			if (_publishRequestedTracks.Contains(trackType))
				return;

			_publishRequestedTracks.Add(trackType);
			UpdateTrackConnection(trackType, IsModuleContentAvailable(trackType), IsConnectionTarget(trackType));
		}

		private void OnUnpublishRequested(IPublishableRemoteUser target, eTrackType trackType)
		{
			if (target != Target)
				return;

			if (!_publishRequestedTracks.Contains(trackType))
				return;

			_publishRequestedTracks.Remove(trackType);
			UpdateTrackConnection(trackType, IsModuleContentAvailable(trackType), IsConnectionTarget(trackType));
		}

		protected override void UpdateTrackConnection(eTrackType trackType, bool isModuleContentAvailable, bool isConnectionTarget)
		{
			if (isModuleContentAvailable && isConnectionTarget && _publishRequestedTracks.Contains(trackType))
				GetOrCreateLocalTrack(trackType).Connector.TryForceConnect();
			else
				DestroyLocalTrack(trackType);
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				TrackController.PublishRequested   -= OnPublishRequested;
				TrackController.UnpublishRequested -= OnUnpublishRequested;

				_publishRequestedTracks.Clear();
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
