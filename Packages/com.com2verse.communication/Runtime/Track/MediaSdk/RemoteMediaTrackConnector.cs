/*===============================================================
 * Product:		Com2Verse
 * File Name:	RemoteMediaTrackConnector.cs
 * Developer:	urun4m0r1
 * Date:		2023-03-10 16:37
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Diagnostics;
using System.Threading;
using Com2Verse.Logger;
using Com2Verse.Solution.UnityRTCSdk;
using Com2Verse.Utils;
using Cysharp.Text;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Communication.MediaSdk
{
	internal sealed class RemoteMediaTrackConnector : MediaTrackConnector
	{
		public ObservableHashSet<IRemoteTrackObserver> Observers { get; }

		private bool _isSubscribed;

		private readonly RtcTrackController _trackController;
		private readonly RemoteTrack        _payload;

		public RemoteMediaTrackConnector(
			eTrackType                              trackType
		  , RtcTrackController                      trackController
		  , ISubscribableRemoteUser                 owner
		  , RemoteTrack                             payload
		  , ObservableHashSet<IRemoteTrackObserver> observers
		) : base(owner, trackType)
		{
			_trackController = trackController;
			_payload         = payload;

			Observers = observers;

			_trackController.TrackSubscribed   += OnTrackSubscribed;
			_trackController.TrackUnsubscribed += OnTrackUnsubscribed;
		}

		private void OnTrackSubscribed(ISubscribableRemoteUser owner, eTrackType trackType, RemoteTrack payload)
		{
			if (owner != Owner || trackType != Type)
				return;

			if (payload != _payload)
				throw new InvalidOperationException("Payload is not matched.");

			if (_isSubscribed)
				throw new InvalidOperationException("Track is already subscribed.");

			_isSubscribed = true;
		}

		private void OnTrackUnsubscribed(ISubscribableRemoteUser owner, eTrackType trackType, RemoteTrack payload)
		{
			if (owner != Owner || trackType != Type)
				return;

			if (payload != _payload)
				throw new InvalidOperationException("Payload is not matched.");

			if (!_isSubscribed)
			{
				// Subscribe 호출 후 OnTrackSubscribed 이벤트 발성 전에 Unsubscribe 호출시 발생할 수 있음.
				LogWarning("Track is already unsubscribed.");
			}

			_isSubscribed = false;
		}

		protected override async UniTask<bool> ConnectAsyncImpl(CancellationTokenSource cancellationTokenSource)
		{
			var context = SynchronizationContext.Current;
			if (!await UniTaskHelper.TrySwitchToMainThread(context, cancellationTokenSource))
			{
				LogWarning("Failed to switch to main thread.");
				await ReturnContext(context);
				return false;
			}

			if (!_trackController.SubscribeTrack(_payload))
			{
				LogWarning("Failed to subscribe track.");
				await ReturnContext(context);
				return false;
			}

			if (!await UniTaskHelper.WaitUntil(() => _isSubscribed, cancellationTokenSource))
			{
				LogWarning("Failed to wait for track subscription changed.");
				await ReturnContext(context);
				return false;
			}

			await ReturnContext(context);
			return _isSubscribed;
		}

		protected override async UniTask<bool> DisconnectAsyncImpl(CancellationTokenSource cancellationTokenSource)
		{
			var context = SynchronizationContext.Current;
			if (!await UniTaskHelper.TrySwitchToMainThread(context, cancellationTokenSource))
			{
				LogWarning("Failed to switch to main thread.");
				await ReturnContext(context);
				return false;
			}

			if (!_trackController.UnsubscribeTrack(_payload))
			{
				LogWarning("Failed to unsubscribe track.");
				await ReturnContext(context);
				return false;
			}

			if (!await UniTaskHelper.WaitUntil(() => !_isSubscribed, cancellationTokenSource))
			{
				LogWarning("Failed to wait for track subscription changed.");
				await ReturnContext(context);
				return false;
			}

			await ReturnContext(context);
			return !_isSubscribed;
		}

		private static async UniTask ReturnContext(SynchronizationContext? context)
		{
			await UniTaskHelper.TrySwitchToSynchronizationContext(context);
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_trackController.TrackSubscribed   -= OnTrackSubscribed;
				_trackController.TrackUnsubscribed -= OnTrackUnsubscribed;
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable

#region Debug
		[DebuggerHidden, StackTraceIgnore]
		protected override string GetLogCategory()
		{
			var baseCategory = base.GetLogCategory();
			var channelInfo  = _trackController.ChannelInfo.GetInfoText();
			var ownerInfo    = Owner.GetInfoText();

			return ZString.Format(
				"{0}: {1} / {2}"
			  , baseCategory, channelInfo, ownerInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		protected override string FormatMessage(string message)
		{
			var baseMessage = base.FormatMessage(message);
			var channelInfo = _trackController.ChannelInfo.GetDebugInfo();

			return ZString.Format(
				"{0}\n----------\n{1}"
			  , baseMessage, channelInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		public override string GetDebugInfo()
		{
			var baseInfo    = base.GetDebugInfo();
			var targetInfo  = _trackController.ChannelInfo.LoginUser.GetInfoText();
			var payloadInfo = _payload.GetInfoText();

			return ZString.Format(
				"{0}\n: Target = {1}\n: Payload = \"{2}\"\n: IsSubscribed = {3}\n: ObserversCount = {4}"
			  , baseInfo, targetInfo, payloadInfo, _isSubscribed, Observers.Count);
		}
#endregion // Debug
	}
}
