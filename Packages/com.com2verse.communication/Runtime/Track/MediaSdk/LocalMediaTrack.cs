/*===============================================================
 * Product:		Com2Verse
 * File Name:	LocalMediaTrack.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-14 22:38
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2Verse.Communication.MediaSdk
{
	internal class LocalMediaTrack : ILocalMediaTrack, IDisposable
	{
		public event Action<IMediaTrack, eConnectionState>? ConnectionChanged;

		public MediaTrackConnector Connector { get; }
		public ICommunicationUser  Owner     { get; }
		public eTrackType          Type      { get; }

		public LocalMediaTrack(
			eTrackType              trackType
		  , RtcTrackController      trackController
		  , IPublishableLocalUser   owner
		  , IPublishableRemoteUser? target = null
		)
		{
			Connector = new LocalMediaTrackConnector(trackType, trackController, owner, target);
			Owner     = owner;
			Type      = trackType;

			Connector.StateChanged += (_, state) => ConnectionChanged?.Invoke(this, state);
		}

#region IDisposable
		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				Connector.Dispose();
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			// base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
