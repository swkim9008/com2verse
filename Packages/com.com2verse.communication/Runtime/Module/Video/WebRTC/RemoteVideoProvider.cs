/*===============================================================
* Product:		Com2Verse
* File Name:	RemoteVideoProvider.cs
* Developer:	urun4m0r1
* Date:			2022-10-11 19:24
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using UnityEngine;

namespace Com2Verse.Communication
{
	public class RemoteVideoProvider : IVideoTextureProvider, IDisposable
	{
#region IModule
		/// <summary>
		/// <see cref="IsRunning"/>이 변경될 때 발생합니다.
		/// </summary>
		public event Action<bool>? StateChanged;

		private bool _isRunning;

		/// <summary>
		/// RemoteUser의 Video트랙을 Subscribe할지 여부.
		/// </summary>
		public bool IsRunning
		{
			get => _isRunning;
			set
			{
				var prevValue = _isRunning;
				if (prevValue == value)
					return;

				_isRunning = value;
				StateChanged?.Invoke(value);

				UpdateTrackConnection();
			}
		}
#endregion // IModule

#region IVideoTextureProvider
		public event Action<Texture?>? TextureChanged;

		private Texture? _texture;

		public Texture? Texture
		{
			get => _texture;
			private set
			{
				var prevValue = _texture;
				if (prevValue == value)
					return;

				_texture = value;
				TextureChanged?.Invoke(value);
			}
		}
#endregion // IVideoTextureProvider

		public IRemoteMediaTrack? Track { get; private set; }

		public IRemoteTrackManager TrackManager { get; }
		public eTrackType          TrackType    { get; }

		public RemoteVideoProvider(IRemoteTrackManager trackManager, eTrackType trackType)
		{
			TrackManager = trackManager;
			TrackType    = trackType;

			TrackManager.TrackAdded   += OnTrackAdded;
			TrackManager.TrackRemoved += OnTrackRemoved;
			TrackManager.TrackUpdated += OnTrackUpdated;
		}

		private void OnTrackAdded(eTrackType trackType, IRemoteMediaTrack track)
		{
			if (trackType != TrackType)
				return;

			Track = track;

			track.ConnectionChanged              += OnTrackConnectionChanged;
			track.Observers.ItemExistenceChanged += OnObserverExistenceChanged;

			UpdateTrackConnection();
		}

		private void OnTrackRemoved(eTrackType trackType, IRemoteMediaTrack track)
		{
			if (trackType != TrackType)
				return;

			Track = null;

			track.ConnectionChanged              -= OnTrackConnectionChanged;
			track.Observers.ItemExistenceChanged -= OnObserverExistenceChanged;

			UpdateTrackConnection();
		}

		private void OnTrackUpdated(eTrackType trackType, IRemoteMediaTrack track)
		{
			if (trackType != TrackType)
				return;

			if (track != Track)
				throw new InvalidOperationException("Track is not matched.");

			Texture = track.Connector.State is eConnectionState.CONNECTED
				? (track as IVideoTextureTrack)?.VideoTexture
				: null;
		}

		private void OnObserverExistenceChanged(bool isAnyItemExists)
		{
			UpdateTrackConnection();
		}

		private void UpdateTrackConnection()
		{
			if (Track == null)
			{
				Texture = null;
				return;
			}

			if (Track.Observers.IsAnyItemExists && IsRunning)
			{
				Track.Connector.TryConnect();
			}
			else
			{
				Texture = null;
				Track.Connector.TryDisconnect();
			}
		}

		private void OnTrackConnectionChanged(IMediaTrack track, eConnectionState state)
		{
			if (track != Track)
				throw new InvalidOperationException("Track is not matched.");

			Texture = state is eConnectionState.CONNECTED
				? (track as IVideoTextureTrack)?.VideoTexture
				: null;
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
				if (Track != null)
				{
					Track.ConnectionChanged              -= OnTrackConnectionChanged;
					Track.Observers.ItemExistenceChanged -= OnObserverExistenceChanged;

					Track.Connector.TryDisconnect();

					Track = null;
				}

				TrackManager.TrackAdded   -= OnTrackAdded;
				TrackManager.TrackRemoved -= OnTrackRemoved;
				TrackManager.TrackRemoved -= OnTrackRemoved;

				Texture = null;
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			// base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
