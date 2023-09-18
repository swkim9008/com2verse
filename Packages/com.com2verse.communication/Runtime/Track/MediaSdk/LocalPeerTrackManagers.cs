/*===============================================================
 * Product:		Com2Verse
 * File Name:	LocalPeerTrackManagers.cs
 * Developer:	urun4m0r1
 * Date:		2023-03-27 15:25
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;

namespace Com2Verse.Communication.MediaSdk
{
	internal sealed class LocalPeerTrackManagers : IPeerTrackManagers, IDisposable
	{
		public event Action<IPublishableRemoteUser, ILocalTrackManager>? PeerAdded;
		public event Action<IPublishableRemoteUser, ILocalTrackManager>? PeerRemoved;

		public IReadOnlyDictionary<IPublishableRemoteUser, ILocalTrackManager> PeerMap => _peerMap;

		private readonly Dictionary<IPublishableRemoteUser, ILocalTrackManager> _peerMap = new();

		private readonly RtcTrackController _trackController;
		private readonly IPeerLocalUser     _publisher;

		public LocalPeerTrackManagers(RtcTrackController trackController, IPeerLocalUser publisher)
		{
			_trackController = trackController;
			_publisher       = publisher;
		}

		public bool ContainsPeer(IPublishableRemoteUser target)
		{
			return _peerMap.ContainsKey(target);
		}

		public bool TryAddPeer(IPublishableRemoteUser target)
		{
			var manager = new LocalPeerTrackManager(_trackController, _publisher, target);
			var result  = _peerMap.TryAdd(target, manager);
			if (!result)
				manager.Dispose();

			PeerAdded?.Invoke(target, manager);
			return result;
		}

		public bool RemovePeer(IPublishableRemoteUser target)
		{
			if (!_peerMap.TryGetValue(target, out var manager))
				return false;

			var result = _peerMap.Remove(target);
			if (!result)
				return false;

			PeerRemoved?.Invoke(target, manager);

			(manager as IDisposable)?.Dispose();
			return result;
		}

		public void Dispose()
		{
			foreach (var (user, trackManager) in _peerMap)
			{
				PeerRemoved?.Invoke(user, trackManager);
				(trackManager as IDisposable)?.Dispose();
			}
		}
	}
}
