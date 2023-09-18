/*===============================================================
 * Product:		Com2Verse
 * File Name:	LocalTrackManager.cs
 * Developer:	urun4m0r1
 * Date:		2023-06-15 10:49
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Utils;

namespace Com2Verse.Communication.MediaSdk
{
	/// <inheritdoc cref="ILocalTrackManager"/>
	internal abstract class LocalTrackManager : ILocalTrackManager, IDisposable
	{
		public event Action<eTrackType, ILocalMediaTrack>? TrackAdded;
		public event Action<eTrackType, ILocalMediaTrack>? TrackRemoved;
		public event Action<eTrackType, ILocalMediaTrack>? TrackUpdated;

		public IReadOnlyDictionary<eTrackType, ILocalMediaTrack> Tracks => _tracks;

		private readonly Dictionary<eTrackType, ILocalMediaTrack> _tracks = new();

		public RtcTrackController      TrackController { get; }
		public IPublishableLocalUser   Publisher       { get; }
		public MediaModules            Modules         { get; }
		public IPublishableRemoteUser? Target          { get; }

		protected LocalTrackManager(RtcTrackController trackController, IPublishableLocalUser publisher, IPublishableRemoteUser? target = null)
		{
			TrackController = trackController;
			Publisher       = publisher;
			Modules         = publisher.Modules;
			Target          = target;

			Modules.ModuleContentChanged    += OnModuleContentChanged;
			Modules.ConnectionTargetChanged += OnConnectionTargetChanged;

			foreach (var trackType in EnumUtility.Foreach<eTrackType>())
			{
				OnModuleContentChanged(trackType, IsModuleContentAvailable(trackType));
				OnConnectionTargetChanged(trackType, IsConnectionTarget(trackType));
			}
		}

		private void OnModuleContentChanged(eTrackType trackType, bool isModuleContentAvailable)
		{
			UpdateTrackConnection(trackType, isModuleContentAvailable, IsConnectionTarget(trackType));
		}

		private void OnConnectionTargetChanged(eTrackType trackType, bool isConnectionTarget)
		{
			UpdateTrackConnection(trackType, IsModuleContentAvailable(trackType), isConnectionTarget);
		}

		protected abstract void UpdateTrackConnection(eTrackType trackType, bool isModuleContentAvailable, bool isConnectionTarget);

		protected bool IsModuleContentAvailable(eTrackType trackType)
		{
			return Modules.IsModuleContentAvailable(trackType);
		}

		protected bool IsConnectionTarget(eTrackType trackType)
		{
			return Modules.IsConnectionTarget(trackType);
		}

		protected ILocalMediaTrack? GetLocalTrack(eTrackType trackType)
		{
			return _tracks.TryGetValue(trackType, out var track) ? track : null;
		}

		protected ILocalMediaTrack GetOrCreateLocalTrack(eTrackType trackType)
		{
			var track = GetLocalTrack(trackType);
			if (track != null)
				return track;

			track = new LocalMediaTrack(trackType, TrackController, Publisher, Target);

			_tracks.Add(trackType, track);
			TrackAdded?.Invoke(trackType, track);

			return track;
		}

		protected void DestroyLocalTrack(eTrackType trackType)
		{
			var track = GetLocalTrack(trackType);
			if (track == null)
				return;

			_tracks.Remove(trackType);
			TrackRemoved?.Invoke(trackType, track);

			(track as IDisposable)?.Dispose();
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
				Modules.ModuleContentChanged    -= OnModuleContentChanged;
				Modules.ConnectionTargetChanged -= OnConnectionTargetChanged;

				foreach (var track in _tracks)
				{
					TrackRemoved?.Invoke(track.Key, track.Value);
					(track.Value as IDisposable)?.Dispose();
				}

				_tracks.Clear();
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			// base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
