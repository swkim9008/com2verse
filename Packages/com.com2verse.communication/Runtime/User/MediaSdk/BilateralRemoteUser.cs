/*===============================================================
* Product:		Com2Verse
* File Name:	BilateralRemoteUser.cs
* Developer:	urun4m0r1
* Date:			2022-09-02 14:46
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using MediaSdkUser = Com2Verse.Solution.UnityRTCSdk.User;

namespace Com2Verse.Communication.MediaSdk
{
	/// <inheritdoc cref="IPublishableRemoteUser"/>
	/// <summary>
	/// <see cref="SubscribeOnDemandRemoteUser"/>의 <see cref="IPublishableRemoteUser"/> 구현체입니다.
	/// <br/>
	/// <br/>즉, 완전한 양방향 통신을 지원하는 <see cref="IRemoteUser"/>입니다.
	/// </summary>
	internal class BilateralRemoteUser : SubscribeOnDemandRemoteUser, IPublishableRemoteUser
	{
		private IPeerTrackManagers? _peerTrackManagers;

		public BilateralRemoteUser(ChannelInfo channelInfo, User user, eUserRole role, MediaSdkUser mediaSdkUser)
			: base(channelInfo, user, role, mediaSdkUser) { }

		/// <summary>
		/// <see cref="IPeerTrackManagers"/>를 할당하고 자기 자신을 <paramref name="peerTrackManagers"/>에 Peer로 등록합니다.
		/// <br/><see cref="CommunicationUser.Dispose"/>시 할당된 <paramref name="peerTrackManagers"/>에서 자기 자신의 Peer를 제거합니다.
		/// </summary>
		public void AssignPeerTrackManagers(IPeerTrackManagers peerTrackManagers)
		{
			if (_peerTrackManagers != null)
				throw new InvalidOperationException("Track managers already assigned.");

			_peerTrackManagers = peerTrackManagers;
			_peerTrackManagers.TryAddPeer(this);
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_peerTrackManagers?.RemovePeer(this);
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
