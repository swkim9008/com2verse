/*===============================================================
* Product:		Com2Verse
* File Name:	Channel.cs
* Developer:	urun4m0r1
* Date:			2022-08-05 14:09
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Com2Verse.Logger;
using Com2Verse.Solution.UnityRTCSdk;
using Com2Verse.Utils;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MediaSdkUser = Com2Verse.Solution.UnityRTCSdk.User;

namespace Com2Verse.Communication.MediaSdk
{
	internal class Channel : BaseChannel
	{
		public override IConnectionController Connector => ChannelConnector;

		public ChannelUserManager UserManager      { get; }
		public ChannelConnector   ChannelConnector { get; }

		protected RtcChannelEventDispatcher EventDispatcher { get; }
		protected RtcChannelAdapter         ChannelAdapter  { get; }

		private CancellationTokenSource? _remoteUserCreationToken = new();

		public event Action? DisconnectRequest;

		protected Channel(ChannelInfo channelInfo, RTCChannelInfo rtcChannelInfo) : base(channelInfo)
		{
			var rtcChannel = new UnityRTCChannel(rtcChannelInfo);
			ChannelAdapter  = new RtcChannelAdapter(rtcChannel);
			EventDispatcher = new RtcChannelEventDispatcher(rtcChannel);

			UserManager      = new ChannelUserManager(Info, EventDispatcher);
			ChannelConnector = new ChannelConnector(Info, ChannelAdapter, EventDispatcher);

			UserManager.UserJoined        += OnUserJoined;
			UserManager.UserLeft          += OnUserLeft;
			UserManager.DisconnectRequest += OnDisconnectRequest;

			ChannelConnector.StateChanged += RaiseConnectionChangedEvent;
		}

		protected override ILocalUser CreateSelf() => new EmptyLocalUser(Info);

		private void OnUserJoined(User user, MediaSdkUser mediaSdkUser)
		{
			OnUserJoinedAsync(user, mediaSdkUser).Forget();
		}

		private async UniTaskVoid OnUserJoinedAsync(User user, MediaSdkUser mediaSdkUser)
		{
			if (!await UniTaskHelper.WaitUntil(() => Self != null, _remoteUserCreationToken))
			{
				LogWarning(Format("Failed to wait until self user creation.", mediaSdkUser));
				return;
			}

			var remoteUser = CreateRemoteUser(user, mediaSdkUser);
			if (remoteUser == null)
			{
				LogError(Format("Failed to create remote user.", mediaSdkUser));
				return;
			}

			UserEventInvoker.RaiseUserJoinedEvent(remoteUser);
		}

		private void OnUserLeft(User user, MediaSdkUser mediaSdkUser)
		{
			if (!TryFindRemoteUser(user, out var remoteUser))
			{
				LogWarning(Format("User not found, operation ignored.", mediaSdkUser));
				return;
			}

			UserEventInvoker.RaiseUserLeftEvent(remoteUser);
		}

		private void OnDisconnectRequest(string channelId, string reason)
		{
			DisconnectRequest?.Invoke();
		}

		protected virtual IRemoteUser? CreateRemoteUser(User user, MediaSdkUser mediaSdkUser)
		{
			return new EmptyRemoteUser(Info, user, mediaSdkUser.Role.GetUserRole());
		}

		protected bool TryFindRemoteUser(MediaSdkUser mediaSdkUser, [NotNullWhen(true)] out IRemoteUser? remote)
		{
			if (!mediaSdkUser.TryParseUser(out var user, Info.SkipRemoteUserIdValidation))
			{
				remote = null;
				return false;
			}

			return TryFindRemoteUser(user, out remote);
		}

		protected IRemoteUser? FindRemoteUser(MediaSdkUser mediaSdkUser)
		{
			return TryFindRemoteUser(mediaSdkUser, out var remote) ? remote : null;
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				ChannelAdapter.Dispose();
				EventDispatcher.Dispose();

				_remoteUserCreationToken?.Cancel();
				_remoteUserCreationToken?.Dispose();
				_remoteUserCreationToken = null;

				UserManager.Dispose();
				ChannelConnector.Dispose();
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable

#region Debug
		[DebuggerHidden, StackTraceIgnore]
		protected virtual string Format(string message, MediaSdkUser target)
		{
			return ZString.Format(
				"{0} / {1}"
			  , message, target.GetInfoText());
		}
#endregion // Debug
	}
}
