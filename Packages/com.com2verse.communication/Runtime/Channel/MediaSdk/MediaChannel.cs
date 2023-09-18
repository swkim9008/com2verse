/*===============================================================
* Product:		Com2Verse
* File Name:	MediaChannel.cs
* Developer:	urun4m0r1
* Date:			2022-08-23 14:36
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Solution.UnityRTCSdk;
using MediaSdkUser = Com2Verse.Solution.UnityRTCSdk.User;

namespace Com2Verse.Communication.MediaSdk
{
	internal class MediaChannel : Channel
	{
		public RtcTrackController TrackController { get; }

		/// <inheritdoc />
		public MediaChannel(ChannelInfo channelInfo, RTCChannelInfo rtcChannelInfo) : base(channelInfo, rtcChannelInfo)
		{
			TrackController = new RtcTrackController(channelInfo, ChannelAdapter, EventDispatcher, FindRemoteUser);
		}

#region CommunicationUser
		protected override ILocalUser CreateSelf()
		{
			var publishTarget = Info.PublishTarget;

			return publishTarget switch
			{
				ePublishTarget.NONE    => new EmptyLocalUser(Info),
				ePublishTarget.PEER    => CreatePeerLocalUser(),
				ePublishTarget.CHANNEL => CreateChannelLocalUser(),
				_                      => throw new ArgumentOutOfRangeException(nameof(publishTarget), publishTarget, null!),
			};

			PeerLocalUser CreatePeerLocalUser()
			{
				var localUser         = new PeerLocalUser(Info);
				var peerTrackManagers = new LocalPeerTrackManagers(TrackController, localUser);
				localUser.AssignPeerTrackManagers(peerTrackManagers);
				return localUser;
			}

			ChannelLocalUser CreateChannelLocalUser()
			{
				var localUser           = new ChannelLocalUser(Info);
				var channelTrackManager = new LocalChannelTrackManager(TrackController, localUser);
				localUser.AssignChannelTrackManager(channelTrackManager);
				return localUser;
			}
		}

		protected override IRemoteUser? CreateRemoteUser(User user, MediaSdkUser mediaSdkUser)
		{
			if (Self == null)
			{
				LogWarning(Format("Failed to create remote user. Self user is null.", mediaSdkUser));
				return null;
			}

			var userRole = mediaSdkUser.Role.GetUserRole();
			return CreateRemoteUser(Self, user, userRole, mediaSdkUser);
		}

		private IRemoteUser CreateRemoteUser(ILocalUser publisher, User user, eUserRole userRole, MediaSdkUser mediaSdkUser)
		{
			var remoteBehaviour = Info.GetRemoteBehaviour(userRole);

			return remoteBehaviour switch
			{
				eRemoteBehaviour.NONE                => new EmptyRemoteUser(Info, user, userRole),
				eRemoteBehaviour.PUBLISH_ONLY        => CreatePublishOnlyRemoteUser(),
				eRemoteBehaviour.SUBSCRIBE_ONLY      => CreateSubscribeOnlyRemoteUser(),
				eRemoteBehaviour.SUBSCRIBE_ON_DEMAND => CreateSubscribeOnDemandRemoteUser(),
				eRemoteBehaviour.FULL                => CreateBilateralRemoteUser(),
				_                                    => throw new ArgumentOutOfRangeException(nameof(remoteBehaviour), remoteBehaviour, null!),
			};

			PublishOnlyRemoteUser CreatePublishOnlyRemoteUser()
			{
				var remote = new PublishOnlyRemoteUser(Info, user, userRole, mediaSdkUser);
				remote.AssignPeerTrackManagers(GetPeerTrackManagers());
				return remote;
			}

			SubscribeOnlyRemoteUser CreateSubscribeOnlyRemoteUser()
			{
				var remote = new SubscribeOnlyRemoteUser(Info, user, userRole, mediaSdkUser);
				remote.AssignTrackManager(CreateTrackManager(remote));
				return remote;
			}

			SubscribeOnDemandRemoteUser CreateSubscribeOnDemandRemoteUser()
			{
				var remote = new SubscribeOnDemandRemoteUser(Info, user, userRole, mediaSdkUser);
				remote.AssignTrackManager(CreateTrackManager(remote));
				remote.AssignRequestSenderFactory(CreateRequestHandler(remote));
				return remote;
			}

			BilateralRemoteUser CreateBilateralRemoteUser()
			{
				var remote = new BilateralRemoteUser(Info, user, userRole, mediaSdkUser);
				remote.AssignPeerTrackManagers(GetPeerTrackManagers());
				remote.AssignTrackManager(CreateTrackManager(remote));
				remote.AssignRequestSenderFactory(CreateRequestHandler(remote));
				return remote;
			}

			IPeerTrackManagers GetPeerTrackManagers()
			{
				var container = publisher as IPeerTrackManagersContainer;
				return container?.PeerTrackManagers ?? throw new InvalidOperationException("Failed to get peer track managers.");
			}

			IRemoteTrackManager CreateTrackManager(ISubscribableRemoteUser remote)
			{
				var manager = new RemoteTrackManager(TrackController, remote);
				return manager;
			}

			RemoteTrackPublishRequestSenderFactory CreateRequestHandler(IPublishRequestableRemoteUser remote)
			{
				var handler = new RemoteTrackPublishRequestSenderFactory(TrackController, remote);
				return handler;
			}
		}
#endregion // CommunicationUser

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				TrackController.Dispose();
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
