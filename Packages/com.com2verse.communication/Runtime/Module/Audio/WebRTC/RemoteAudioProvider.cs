/*===============================================================
* Product:		Com2Verse
* File Name:	RemoteAudioProvider.cs
* Developer:	urun4m0r1
* Date:			2022-11-04 16:57
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Extension;
using Com2Verse.Sound;

namespace Com2Verse.Communication
{
	public sealed class RemoteAudioProvider : BaseVolume, IAudioSourceProvider
	{
#region IModule
		/// <summary>
		/// <see cref="IsRunning"/>이 변경될 때 발생합니다.
		/// </summary>
		public event Action<bool>? StateChanged;

		private bool _isRunning;

		/// <summary>
		/// RemoteUser의 Audio트랙을 Subscribe할지 여부.
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

#region IVolume
		protected override void ApplyLevel(float value)
		{
			base.ApplyLevel(value);

			if (!AudioSource.IsUnityNull())
				AudioSource!.Volume = value;
		}

		protected override void ApplyAudible(bool value)
		{
			base.ApplyAudible(value);

			if (!AudioSource.IsUnityNull())
				AudioSource!.TargetMixerGroup = GetAudioMixerGroupIndex(value);
		}

		private static int GetAudioMixerGroupIndex(bool isAudible) => isAudible
			? AudioMixerGroupIndex.RemoteVoice
			: AudioMixerGroupIndex.Mute;
#endregion // IVolume

#region IAudioSourceProvider
		public event Action<MetaverseAudioSource?>? AudioSourceChanged;

		private MetaverseAudioSource? _audioSource;

		public MetaverseAudioSource? AudioSource
		{
			get => _audioSource;
			private set
			{
				var prevValue = _audioSource;
				if (prevValue == value)
					return;

				_audioSource = value;
				AudioSourceChanged?.Invoke(value);

				ApplyLevel(Level);
				ApplyAudible(IsAudible);
			}
		}
#endregion // IAudioSourceProvider

		public IRemoteMediaTrack? Track { get; private set; }

		public IRemoteTrackManager TrackManager { get; }
		public eTrackType          TrackType    { get; }

		public RemoteAudioProvider(IRemoteTrackManager trackManager, eTrackType trackType) : base(1f)
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

			AudioSource = track.Connector.State is eConnectionState.CONNECTED
				? (track as IAudioSourceTrack)?.AudioSource
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
				AudioSource = null;
				return;
			}

			if (Track.Observers.IsAnyItemExists && IsRunning)
			{
				Track.Connector.TryConnect();
			}
			else
			{
				AudioSource = null;
				Track.Connector.TryDisconnect();
			}
		}

		private void OnTrackConnectionChanged(IMediaTrack track, eConnectionState state)
		{
			if (track != Track)
				throw new InvalidOperationException("Track is not matched.");

			AudioSource = state is eConnectionState.CONNECTED
				? (track as IAudioSourceTrack)?.AudioSource
				: null;
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
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
				TrackManager.TrackUpdated -= OnTrackUpdated;

				AudioSource = null;
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
