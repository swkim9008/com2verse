/*===============================================================
* Product:		Com2Verse
* File Name:	PeerLocalUser.cs
* Developer:	urun4m0r1
* Date:			2023-03-27 14:09
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;

namespace Com2Verse.Communication.MediaSdk
{
	/// <inheritdoc cref="IPeerLocalUser"/>
	/// <summary>
	/// <see cref="PublishableLocalUser"/>의 <see cref="IPeerLocalUser"/> 구현체입니다.
	/// <br/>
	/// <br/>추가적으로 <see cref="IPublishableRemoteUser"/>에서 사용하기 위한 <see cref="IPeerTrackManagers"/>를 할당할 수 있습니다.
	/// </summary>
	internal sealed class PeerLocalUser : PublishableLocalUser, IPeerLocalUser, IPeerTrackManagersContainer
	{
		public event Action<IPublishableRemoteUser, ILocalTrackManager>? PeerAdded;
		public event Action<IPublishableRemoteUser, ILocalTrackManager>? PeerRemoved;

		public IReadOnlyDictionary<IPublishableRemoteUser, ILocalTrackManager>? PeerMap => PeerTrackManagers?.PeerMap;

		public IPeerTrackManagers? PeerTrackManagers { get; private set; }

		public PeerLocalUser(ChannelInfo channelInfo) : base(channelInfo) { }

		/// <summary>
		/// <see cref="IPeerTrackManagers"/>를 할당합니다.
		/// <br/><see cref="CommunicationUser.Dispose"/>시 할당된 <paramref name="peerTrackManagers"/>도 함께 해제됩니다.
		/// </summary>
		public void AssignPeerTrackManagers(IPeerTrackManagers peerTrackManagers)
		{
			if (PeerTrackManagers != null)
				throw new InvalidOperationException("Track manager is already assigned.");

			PeerTrackManagers = peerTrackManagers;

			PeerTrackManagers.PeerAdded   += OnPeerAdded;
			PeerTrackManagers.PeerRemoved += OnPeerRemoved;
		}

		private void OnPeerAdded(IPublishableRemoteUser   peer, ILocalTrackManager trackManager) => PeerAdded?.Invoke(peer, trackManager);
		private void OnPeerRemoved(IPublishableRemoteUser peer, ILocalTrackManager trackManager) => PeerRemoved?.Invoke(peer, trackManager);

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (PeerTrackManagers != null)
				{
					PeerTrackManagers.PeerAdded   -= OnPeerAdded;
					PeerTrackManagers.PeerRemoved -= OnPeerRemoved;
				}

				(PeerTrackManagers as IDisposable)?.Dispose();
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
