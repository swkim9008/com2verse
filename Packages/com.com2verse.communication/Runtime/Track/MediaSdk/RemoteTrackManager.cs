/*===============================================================
 * Product:		Com2Verse
 * File Name:	RemoteTrackManager.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-14 22:38
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Utils;
using Com2Verse.Solution.UnityRTCSdk;

namespace Com2Verse.Communication.MediaSdk
{
	internal sealed class RemoteTrackManager : IRemoteTrackManager, IDisposable
	{
		public event Action<eTrackType, IRemoteMediaTrack>? TrackAdded;
		public event Action<eTrackType, IRemoteMediaTrack>? TrackRemoved;
		public event Action<eTrackType, IRemoteMediaTrack>? TrackUpdated;

		public IReadOnlyDictionary<eTrackType, IRemoteMediaTrack> Tracks => _tracks;

		private readonly Dictionary<eTrackType, IRemoteMediaTrack> _tracks = new();

		private readonly Dictionary<eTrackType, ObservableHashSet<IRemoteTrackObserver>> _observersMap = new();

		private readonly RtcTrackController      _trackController;
		private readonly ISubscribableRemoteUser _owner;

		public RemoteTrackManager(RtcTrackController trackController, ISubscribableRemoteUser owner)
		{
			_trackController = trackController;
			_owner           = owner;

			_trackController.TrackAdded   += OnTrackAdded;
			_trackController.TrackRemoved += OnTrackRemoved;
			_trackController.TrackUpdated += OnTrackUpdated;
		}

		private ObservableHashSet<IRemoteTrackObserver> GetOrCreateObservers(eTrackType trackType)
		{
			var observers = GetObservers(trackType);
			if (observers == null)
			{
				observers = new ObservableHashSet<IRemoteTrackObserver>();
				_observersMap.Add(trackType, observers);
			}

			return observers;
		}

		private ObservableHashSet<IRemoteTrackObserver>? GetObservers(eTrackType trackType)
		{
			return _observersMap.TryGetValue(trackType, out var observers) ? observers : null;
		}

		private IRemoteMediaTrack CreateTrackInstance(eTrackType trackType, RemoteTrack payload, ObservableHashSet<IRemoteTrackObserver> observers) => payload switch
		{
			RemoteAudioTrack audioPayload => new RemoteAudioSourceTrack(trackType, _trackController, _owner, audioPayload, observers),
			RemoteVideoTrack videoPayload => new RemoteVideoTextureTrack(trackType, _trackController, _owner, videoPayload, observers),
			_                             => new RemoteMediaTrack(trackType, _trackController, _owner, payload, observers),
		};

		private void OnTrackAdded(ISubscribableRemoteUser owner, eTrackType trackType, RemoteTrack payload)
		{
			if (owner != _owner)
				return;

			if (_tracks.TryGetValue(trackType, out var track))
				throw new InvalidOperationException($"Track {trackType} already exists.");

			track = CreateTrackInstance(trackType, payload, GetOrCreateObservers(trackType));

			_tracks.Add(trackType, track);
			TrackAdded?.Invoke(trackType, track);
		}

		private void OnTrackRemoved(ISubscribableRemoteUser owner, eTrackType trackType, RemoteTrack payload)
		{
			if (owner != _owner)
				return;

			if (!_tracks.TryGetValue(trackType, out var track))
				throw new InvalidOperationException($"Track {trackType} does not exist.");

			_tracks.Remove(trackType);
			TrackRemoved?.Invoke(trackType, track);
			(track as IDisposable)?.Dispose();
		}

		private void OnTrackUpdated(ISubscribableRemoteUser owner, eTrackType trackType, RemoteTrack payload)
		{
			if (owner != _owner)
				return;

			if (!_tracks.TryGetValue(trackType, out var track))
				throw new InvalidOperationException($"Track {trackType} does not exist.");

			TrackUpdated?.Invoke(trackType, track);
		}

		public bool ContainsObserver(eTrackType trackType, IRemoteTrackObserver observer)
		{
			return GetObservers(trackType)?.Contains(observer) ?? false;
		}

		public bool TryAddObserver(eTrackType trackType, IRemoteTrackObserver observer)
		{
			return GetOrCreateObservers(trackType).TryAdd(observer);
		}

		public bool RemoveObserver(eTrackType trackType, IRemoteTrackObserver observer)
		{
			return GetObservers(trackType)?.Remove(observer) ?? false;
		}

		public void Dispose()
		{
			_trackController.TrackAdded   -= OnTrackAdded;
			_trackController.TrackRemoved -= OnTrackRemoved;
			_trackController.TrackUpdated -= OnTrackUpdated;

			foreach (var observers in _observersMap.Values)
				observers.Clear();

			foreach (var track in _tracks)
			{
				TrackRemoved?.Invoke(track.Key, track.Value);
				(track.Value as IDisposable)?.Dispose();
			}

			_tracks.Clear();
		}
	}
}
