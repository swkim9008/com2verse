/*===============================================================
 * Product:		Com2Verse
 * File Name:	RtcChannelEventDispatcher.cs
 * Developer:	urun4m0r1
 * Date:		2023-03-22 17:02
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Threading;
using Com2Verse.Solution.UnityRTCSdk;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using MediaSdkUser = Com2Verse.Solution.UnityRTCSdk.User;

namespace Com2Verse.Communication.MediaSdk
{
	/// <summary>
	/// <see cref="UnityRTCChannel"/>의 이벤트를 메인 스레드에서 처리하기 위한 클래스.
	/// </summary>
	public sealed class RtcChannelEventDispatcher : IDisposable
	{
		public event OnAddUser?    UserJoined;
		public event OnRemoveUser? UserLeft;
		public event OnUpdateUser? UserUpdated;

		public event OnInitializeError? InitializeError;
		public event OnDisconnect?      DisconnectRequest;
		public event OnJoinFailed?      JoinFailed;
		public event OnLeaveFailed?     LeaveFailed;

		public event OnJoinSuccess?  JoinSuccess;
		public event OnLeaveSuccess? LeaveSuccess;

		public event OnSetVideoQuality? VideoQualityChanged;

		public event OnAddAudioTrack?     AudioTrackAdded;
		public event OnAddVideoTrack?     VideoTrackAdded;
		public event OnRemoveAudioTrack?  AudioTrackRemoved;
		public event OnRemoveVideoTrack?  VideoTrackRemoved;
		public event OnSubscribeAudio?    AudioTrackSubscribed;
		public event OnSubscribeVideo?    VideoTrackSubscribed;
		public event OnUpdateRemoteVideo? VideoTrackUpdated;

		public event OnRequestPublishAudio? PublishAudioRequested;
		public event OnRequestPublishVideo? PublishVideoRequested;

		public UnityRTCChannel RtcChannel { get; }

		private CancellationTokenSource? _eventToken = new();

		public RtcChannelEventDispatcher(UnityRTCChannel rtcChannel)
		{
			RtcChannel = rtcChannel;

			RegisterChannelCallbacks();
		}

		private void RegisterChannelCallbacks()
		{
			UnRegisterChannelCallbacks();

#if DISABLE_WEBRTC
			return;
#endif // !DISABLE_WEBRTC

			RtcChannel.OnUserAdded   = OnUserJoined;
			RtcChannel.OnUserRemoved = OnUserLeft;
			RtcChannel.OnUserUpdated = OnUserUpdated;

			RtcChannel.OnInitializeError = OnInitializeError;
			RtcChannel.OnDisconnected    = OnDisconnected;
			RtcChannel.OnJoinFailed      = OnJoinFailed;
			RtcChannel.OnLeaveFailed     = OnLeaveFailed;

			RtcChannel.OnJoinSuccess  = OnJoinSuccess;
			RtcChannel.OnLeaveSuccess = OnLeaveSuccess;

			RtcChannel.OnSetVideoQuality = OnVideoQualityChanged;

			RtcChannel.OnAudioTrackAdded   = OnAudioTrackAdded;
			RtcChannel.OnVideoTrackAdded   = OnVideoTrackAdded;
			RtcChannel.OnAudioTrackRemoved = OnAudioTrackRemoved;
			RtcChannel.OnVideoTrackRemoved = OnVideoTrackRemoved;
			RtcChannel.OnSubscribeAudio    = OnAudioTrackSubscribed;
			RtcChannel.OnSubscribeVideo    = OnVideoTrackSubscribed;
			RtcChannel.OnUpdateRemoteVideo = OnVideoTrackUpdated;

			RtcChannel.OnRequestPublishAudio = OnRequestPublishAudio;
			RtcChannel.OnRequestPublishVideo = OnRequestPublishVideo;
		}

		private void UnRegisterChannelCallbacks()
		{
#if DISABLE_WEBRTC
			return;
#endif // !DISABLE_WEBRTC

			RtcChannel.OnUserAdded   = null;
			RtcChannel.OnUserRemoved = null;
			RtcChannel.OnUserUpdated = null;

			RtcChannel.OnInitializeError = null;
			RtcChannel.OnDisconnected    = null;
			RtcChannel.OnJoinFailed      = null;
			RtcChannel.OnLeaveFailed     = null;

			RtcChannel.OnJoinSuccess  = null;
			RtcChannel.OnLeaveSuccess = null;

			RtcChannel.OnSetVideoQuality = null;

			RtcChannel.OnAudioTrackAdded   = null;
			RtcChannel.OnVideoTrackAdded   = null;
			RtcChannel.OnAudioTrackRemoved = null;
			RtcChannel.OnVideoTrackRemoved = null;
			RtcChannel.OnSubscribeAudio    = null;
			RtcChannel.OnSubscribeVideo    = null;
			RtcChannel.OnUpdateRemoteVideo = null;

			RtcChannel.OnRequestPublishAudio = null;
			RtcChannel.OnRequestPublishVideo = null;
		}

		private void OnUserJoined(MediaSdkUser  mediaSdkUser) => RaiseEvent(() => UserJoined?.Invoke(mediaSdkUser));
		private void OnUserLeft(MediaSdkUser    mediaSdkUser) => RaiseEvent(() => UserLeft?.Invoke(mediaSdkUser));
		private void OnUserUpdated(MediaSdkUser mediaSdkUser) => RaiseEvent(() => UserUpdated?.Invoke(mediaSdkUser));

		private void OnInitializeError(string channelId, string reason) => RaiseEvent(() => InitializeError?.Invoke(channelId, reason));
		private void OnDisconnected(string    channelId, string reason) => RaiseEvent(() => DisconnectRequest?.Invoke(channelId, reason));
		private void OnJoinFailed(string      channelId, string reason) => RaiseEvent(() => JoinFailed?.Invoke(channelId, reason));
		private void OnLeaveFailed(string     channelId, string reason) => RaiseEvent(() => LeaveFailed?.Invoke(channelId, reason));

		private void OnJoinSuccess(string  channelId) => RaiseEvent(() => JoinSuccess?.Invoke(channelId));
		private void OnLeaveSuccess(string channelId) => RaiseEvent(() => LeaveSuccess?.Invoke(channelId));

		private void OnVideoQualityChanged(VideoQuality quality) => RaiseEvent(() => VideoQualityChanged?.Invoke(quality));

		private void OnAudioTrackAdded(RemoteAudioTrack      audioTrack) => RaiseEvent(() => AudioTrackAdded?.Invoke(audioTrack));
		private void OnVideoTrackAdded(RemoteVideoTrack      videoTrack) => RaiseEvent(() => VideoTrackAdded?.Invoke(videoTrack));
		private void OnAudioTrackRemoved(RemoteAudioTrack    audioTrack) => RaiseEvent(() => AudioTrackRemoved?.Invoke(audioTrack));
		private void OnVideoTrackRemoved(RemoteVideoTrack    videoTrack) => RaiseEvent(() => VideoTrackRemoved?.Invoke(videoTrack));
		private void OnAudioTrackSubscribed(RemoteAudioTrack audioTrack) => RaiseEvent(() => AudioTrackSubscribed?.Invoke(audioTrack));
		private void OnVideoTrackSubscribed(RemoteVideoTrack videoTrack) => RaiseEvent(() => VideoTrackSubscribed?.Invoke(videoTrack));
		private void OnVideoTrackUpdated(RemoteVideoTrack    videoTrack) => RaiseEvent(() => VideoTrackUpdated?.Invoke(videoTrack));

		private void OnRequestPublishAudio(MediaSdkUser mediaSdkUser, AUDIO_TYPE audioType, bool isPublish) => RaiseEvent(() => PublishAudioRequested?.Invoke(mediaSdkUser, audioType, isPublish));
		private void OnRequestPublishVideo(MediaSdkUser mediaSdkUser, VIDEO_TYPE videoType, bool isPublish) => RaiseEvent(() => PublishVideoRequested?.Invoke(mediaSdkUser, videoType, isPublish));

		private void RaiseEvent(Action action)
		{
			UniTaskHelper.InvokeOnMainThread(action, _eventToken).Forget();
		}

		public void Dispose()
		{
			UnRegisterChannelCallbacks();
			_eventToken?.Cancel();
			_eventToken?.Dispose();
			_eventToken = null;
		}
	}
}
