/*===============================================================
 * Product:		Com2Verse
 * File Name:	RemoteMediaTrack.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-14 22:38
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Utils;
using Com2Verse.Solution.UnityRTCSdk;

namespace Com2Verse.Communication.MediaSdk
{
	internal class RemoteMediaTrack : IRemoteMediaTrack, IDisposable
	{
		public event Action<IMediaTrack, eConnectionState>? ConnectionChanged;

		public MediaTrackConnector Connector { get; }
		public ICommunicationUser  Owner     { get; }
		public eTrackType          Type      { get; }

		public ObservableHashSet<IRemoteTrackObserver> Observers { get; }

		public RemoteMediaTrack(
			eTrackType                              trackType
		  , RtcTrackController                      trackController
		  , ISubscribableRemoteUser                 owner
		  , RemoteTrack                             payload
		  , ObservableHashSet<IRemoteTrackObserver> observers
		)
		{
			Connector = new RemoteMediaTrackConnector(trackType, trackController, owner, payload, observers);
			Owner     = owner;
			Type      = trackType;

			Observers = observers;

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
