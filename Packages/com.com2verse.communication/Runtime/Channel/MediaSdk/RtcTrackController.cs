/*===============================================================
* Product:		Com2Verse
* File Name:	RtcTrackController.cs
* Developer:	urun4m0r1
* Date:			2023-03-29 11:50
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Com2Verse.Logger;
using Com2Verse.Solution.UnityRTCSdk;
using Com2Verse.Utils;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MediaSdkUser = Com2Verse.Solution.UnityRTCSdk.User;

namespace Com2Verse.Communication.MediaSdk
{
	internal delegate void TrackEvent(ISubscribableRemoteUser owner, eTrackType trackType, RemoteTrack track);

	internal delegate void TrackRequestEvent(IPublishableRemoteUser requester, eTrackType trackType);

	/// <summary>
	/// <see cref="RtcChannelAdapter"/>와 <see cref="RtcChannelEventDispatcher"/>에서 정의된 <see cref="StreamTrack"/>관련 API를 감싸는 Facade 클래스.
	/// </summary>
	internal sealed class RtcTrackController : IDisposable
	{
		public event Action<VideoQuality>? VideoQualityChanged;

		public event TrackEvent? TrackAdded;
		public event TrackEvent? TrackRemoved;
		public event TrackEvent? TrackSubscribed;
		public event TrackEvent? TrackUnsubscribed;
		public event TrackEvent? TrackUpdated;

		public event TrackRequestEvent? PublishRequested;
		public event TrackRequestEvent? UnpublishRequested;

		public ChannelInfo               ChannelInfo     { get; }
		public RtcChannelAdapter         ChannelAdapter  { get; }
		public RtcChannelEventDispatcher EventDispatcher { get; }

		private Func<MediaSdkUser, IRemoteUser?> FindRemoteUser { get; }

		private CancellationTokenSource? _remoteUserAwaitToken = new();

		public RtcTrackController(ChannelInfo                      channelInfo
		                        , RtcChannelAdapter                channelAdapter
		                        , RtcChannelEventDispatcher        eventDispatcher
		                        , Func<MediaSdkUser, IRemoteUser?> findRemoteUser)
		{
			ChannelInfo     = channelInfo;
			ChannelAdapter  = channelAdapter;
			EventDispatcher = eventDispatcher;
			FindRemoteUser  = findRemoteUser;

			RegisterChannelCallbacks();
		}

		private void RegisterChannelCallbacks()
		{
			UnRegisterChannelCallbacks();

			EventDispatcher.VideoQualityChanged += OnVideoQualityChanged;

			EventDispatcher.AudioTrackAdded      += OnAudioTrackAdded;
			EventDispatcher.VideoTrackAdded      += OnVideoTrackAdded;
			EventDispatcher.AudioTrackRemoved    += OnAudioTrackRemoved;
			EventDispatcher.VideoTrackRemoved    += OnVideoTrackRemoved;
			EventDispatcher.AudioTrackSubscribed += OnAudioTrackSubscribed;
			EventDispatcher.VideoTrackSubscribed += OnVideoTrackSubscribed;
			EventDispatcher.VideoTrackUpdated    += OnVideoTrackUpdated;

			EventDispatcher.PublishAudioRequested += OnRequestPublishAudio;
			EventDispatcher.PublishVideoRequested += OnRequestPublishVideo;
		}

		private void UnRegisterChannelCallbacks()
		{
			EventDispatcher.VideoQualityChanged -= OnVideoQualityChanged;

			EventDispatcher.AudioTrackAdded      -= OnAudioTrackAdded;
			EventDispatcher.VideoTrackAdded      -= OnVideoTrackAdded;
			EventDispatcher.AudioTrackRemoved    -= OnAudioTrackRemoved;
			EventDispatcher.VideoTrackRemoved    -= OnVideoTrackRemoved;
			EventDispatcher.AudioTrackSubscribed -= OnAudioTrackSubscribed;
			EventDispatcher.VideoTrackSubscribed -= OnVideoTrackSubscribed;
			EventDispatcher.VideoTrackUpdated    -= OnVideoTrackUpdated;

			EventDispatcher.PublishAudioRequested -= OnRequestPublishAudio;
			EventDispatcher.PublishVideoRequested -= OnRequestPublishVideo;
		}

#region Events
		private void OnVideoQualityChanged(VideoQuality quality) => VideoQualityChanged?.Invoke(quality);

		private void OnAudioTrackAdded(RemoteAudioTrack?        track) => InvokeRemoteTrackEvent(true,  track, (arg1, arg2, arg3) => TrackAdded?.Invoke(arg1, arg2, arg3)).Forget();
		private void OnVideoTrackAdded(RemoteVideoTrack?        track) => InvokeRemoteTrackEvent(true,  track, (arg1, arg2, arg3) => TrackAdded?.Invoke(arg1, arg2, arg3)).Forget();
		private void OnAudioTrackRemoved(RemoteAudioTrack?      track) => InvokeRemoteTrackEvent(false, track, (arg1, arg2, arg3) => TrackRemoved?.Invoke(arg1, arg2, arg3)).Forget();
		private void OnVideoTrackRemoved(RemoteVideoTrack?      track) => InvokeRemoteTrackEvent(false, track, (arg1, arg2, arg3) => TrackRemoved?.Invoke(arg1, arg2, arg3)).Forget();
		private void OnAudioTrackSubscribed(RemoteAudioTrack?   track) => InvokeRemoteTrackEvent(true,  track, (arg1, arg2, arg3) => TrackSubscribed?.Invoke(arg1, arg2, arg3)).Forget();
		private void OnVideoTrackSubscribed(RemoteVideoTrack?   track) => InvokeRemoteTrackEvent(true,  track, (arg1, arg2, arg3) => TrackSubscribed?.Invoke(arg1, arg2, arg3)).Forget();
		private void OnAudioTrackUnsubscribed(RemoteAudioTrack? track) => InvokeRemoteTrackEvent(false, track, (arg1, arg2, arg3) => TrackUnsubscribed?.Invoke(arg1, arg2, arg3)).Forget();
		private void OnVideoTrackUnsubscribed(RemoteVideoTrack? track) => InvokeRemoteTrackEvent(false, track, (arg1, arg2, arg3) => TrackUnsubscribed?.Invoke(arg1, arg2, arg3)).Forget();
		private void OnVideoTrackUpdated(RemoteVideoTrack?      track) => InvokeRemoteTrackEvent(false, track, (arg1, arg2, arg3) => TrackUpdated?.Invoke(arg1, arg2, arg3)).Forget();

		private void OnRequestPublishAudio(MediaSdkUser user, AUDIO_TYPE type, bool isPublish) => InvokePublishRequestEvent(user, type.GetTrackType(), isPublish).Forget();
		private void OnRequestPublishVideo(MediaSdkUser user, VIDEO_TYPE type, bool isPublish) => InvokePublishRequestEvent(user, type.GetTrackType(), isPublish).Forget();

		private async UniTaskVoid InvokeRemoteTrackEvent(bool waitForUser, RemoteTrack? track, TrackEvent? callback, [CallerMemberName] string? caller = null)
		{
			if (track == null)
			{
				LogError("Remote track is null.", caller);
				return;
			}

			var trackType    = track.GetTrackType();
			var mediaSdkUser = track.User;

			var remote = await TryFindRemoteUserAsync(mediaSdkUser, waitForUser);
			if (remote == null)
			{
				LogWarning(Format("User not found, operation ignored.", trackType, mediaSdkUser), caller);
				return;
			}

			if (remote is not ISubscribableRemoteUser owner)
			{
				LogWarning(Format("Remote signal from non-subscribable user, operation ignored.", trackType, remote), caller);
				return;
			}

			Log(Format(trackType, owner), caller);
			callback?.Invoke(owner, trackType, track);
		}

		private async UniTaskVoid InvokePublishRequestEvent(MediaSdkUser mediaSdkUser, eTrackType trackType, bool isPublish, [CallerMemberName] string? caller = null)
		{
			var remote = await TryFindRemoteUserAsync(mediaSdkUser, isPublish);
			if (remote == null)
			{
				LogWarning(Format("User not found, operation ignored.", trackType, mediaSdkUser), caller);
				return;
			}

			if (remote is not IPublishableRemoteUser requester)
			{
				LogWarning(Format("Remote request from non-publishable user, operation ignored.", trackType, remote), caller);
				return;
			}

			Log(Format(trackType, requester), caller);

			var callback = isPublish ? PublishRequested : UnpublishRequested;
			callback?.Invoke(requester, trackType);
		}

		private async UniTask<IRemoteUser?> TryFindRemoteUserAsync(MediaSdkUser mediaSdkUser, bool waitForUser)
		{
			var remote = FindRemoteUser(mediaSdkUser);
			if (remote != null)
				return remote;

			if (!waitForUser)
				return null;

			var (isTimeout, result) = await UniTaskHelper.WaitUntil(() => TryFindRemoteUser(mediaSdkUser, out remote), _remoteUserAwaitToken)
			                                             .TimeoutWithoutException(Define.RemoteUserCreationAwaitTimeout);

			if (isTimeout || !result)
				return null;

			return remote;
		}

		private bool TryFindRemoteUser(MediaSdkUser mediaSdkUser, out IRemoteUser? remote)
		{
			remote = FindRemoteUser(mediaSdkUser);
			return remote != null;
		}
#endregion // Events

#region Track
		public bool RequestPublishTrack(IPublishRequestableRemoteUser target, eTrackType trackType, bool isPublish)
		{
			return ChannelAdapter.RequestPublishTrack(target, trackType, isPublish);
		}

		public bool TryPublishTrack(eTrackType trackType, IPublishableLocalUser publisher, IPublishableRemoteUser? target, [NotNullWhen(true)] out LocalTrack? track)
		{
			track = ChannelAdapter.PublishTrack(trackType, publisher, target);
			return track != null;
		}

		public bool UnpublishTrack(LocalTrack? track)
		{
			if (track == null)
				return false;

			return ChannelAdapter.UnpublishTrack(track);
		}

		public bool ChangeAudioQuality(LocalAudioTrack? track, ulong bitrate)
		{
			if (track == null)
				return false;

			ChannelAdapter.ChangeAudioQuality(track, bitrate);
			return true;
		}

		public bool ChangeVideoQuality(LocalVideoTrack? track, VideoQuality quality)
		{
			if (track == null)
				return false;

			ChannelAdapter.ChangeVideoQuality(track, quality);
			return true;
		}

		public bool SubscribeTrack(RemoteTrack? track)
		{
			if (track == null)
				return false;

			return ChannelAdapter.SubscribeTrack(track);
		}

		public bool UnsubscribeTrack(RemoteTrack? track)
		{
			if (track == null)
				return false;

			var result = ChannelAdapter.UnsubscribeTrack(track);
			if (!result)
				return false;

			switch (track)
			{
				case RemoteAudioTrack audioTrack:
					OnAudioTrackUnsubscribed(audioTrack);
					break;
				case RemoteVideoTrack videoTrack:
					OnVideoTrackUnsubscribed(videoTrack);
					break;
			}

			return true;
		}
#endregion // Track

		public void Dispose()
		{
			_remoteUserAwaitToken?.Cancel();
			_remoteUserAwaitToken?.Dispose();
			_remoteUserAwaitToken = null;

			UnRegisterChannelCallbacks();
		}

#region Debug
		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		private void Log(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		private void LogWarning(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogWarningMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		private void LogError(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogErrorMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[DebuggerHidden, StackTraceIgnore]
		private string GetLogCategory()
		{
			var className   = GetType().Name;
			var channelInfo = ChannelInfo.GetInfoText();

			return ZString.Format(
				"{0}: {1}"
			  , className, channelInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		private string FormatMessage(string message)
		{
			var channelInfo = ChannelInfo.GetDebugInfo();

			return ZString.Format(
				"{0}\n----------\n{1}"
			  , message, channelInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		private string Format(eTrackType trackType, IRemoteUser target)
		{
			return ZString.Format(
				"({0}) / {1}"
			  , trackType, target.GetDebugInfo());
		}

		[DebuggerHidden, StackTraceIgnore]
		private string Format(string message, eTrackType trackType, IRemoteUser target)
		{
			return ZString.Format(
				"{0} / ({1}) / {2}"
			  , message, trackType, target.GetDebugInfo());
		}

		[DebuggerHidden, StackTraceIgnore]
		private string Format(string message, eTrackType trackType, MediaSdkUser target)
		{
			return ZString.Format(
				"{0} / ({1}) / {2}"
			  , message, trackType, target.GetInfoText());
		}
#endregion // Debug
	}
}
