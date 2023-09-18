/*===============================================================
* Product:		Com2Verse
* File Name:	RemoteScreenProvider.cs
* Developer:	urun4m0r1
* Date:			2022-10-11 19:24
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.ScreenShare;

namespace Com2Verse.Communication
{
	public sealed class RemoteScreenProvider : RemoteVideoProvider
	{
		public RemoteScreenProvider(IRemoteTrackManager trackManager, eTrackType trackType)
			: base(trackManager, trackType)
		{
			TrackManager.TrackAdded   += OnTrackAdded;
			TrackManager.TrackRemoved += OnTrackRemoved;
		}

		private void OnTrackAdded(eTrackType trackType, IRemoteMediaTrack track)
		{
			if (trackType != TrackType)
				return;

			ScreenCaptureManager.Instance.AddRemoteUser(this);
		}

		private void OnTrackRemoved(eTrackType trackType, IRemoteMediaTrack track)
		{
			if (trackType != TrackType)
				return;

			ScreenCaptureManager.InstanceOrNull?.RemoveRemoteUser(this);
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				TrackManager.TrackAdded   -= OnTrackAdded;
				TrackManager.TrackRemoved -= OnTrackRemoved;

				ScreenCaptureManager.InstanceOrNull?.RemoveRemoteUser(this);
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
