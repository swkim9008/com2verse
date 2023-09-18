/*===============================================================
 * Product:		Com2Verse
 * File Name:	RtcChannelAdapter.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-16 12:56
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Solution.UnityRTCSdk;
using UnityEngine;
using MediaSdkUser = Com2Verse.Solution.UnityRTCSdk.User;

namespace Com2Verse.Communication.MediaSdk
{
	/// <summary>
	/// <see cref="UnityRTCChannel"/>의 메서드를 추상화된 인터페이스로 제공하기 위한 클래스.
	/// </summary>
	public sealed class RtcChannelAdapter : IDisposable
	{
		public UnityRTCChannel RtcChannel { get; }

		public RtcChannelAdapter(UnityRTCChannel rtcChannel)
		{
			RtcChannel = rtcChannel;
		}

		public void Dispose()
		{
			Leave();
			DeInitialize();
		}

		public void Initialize()   => RtcChannel.Initialize();
		public void Join()         => RtcChannel.Join();
		public void Leave()        => RtcChannel.Leave();
		public void DeInitialize() => RtcChannel.Deinitialize();

		public bool RequestPublishTrack(IPublishRequestableRemoteUser target, eTrackType trackType, bool isPublish)
		{
#if DISABLE_WEBRTC
			return false;
#endif // DISABLE_WEBRTC

			var audioType = trackType.GetAudioType();
			if (audioType != null)
			{
				RtcChannel.RequestPublishAudio(target.MediaSdkUser, audioType.Value, isPublish);
				return true;
			}

			var videoType = trackType.GetVideoType();
			if (videoType != null)
			{
				RtcChannel.RequestPublishVideo(target.MediaSdkUser, videoType.Value, isPublish);
				return true;
			}

			return false;
		}

		public bool SubscribeTrack(RemoteTrack track)
		{
#if DISABLE_WEBRTC
			return false;
#endif // DISABLE_WEBRTC

			RtcChannel.SubscribeTrack(track);
			return true;
		}

		public bool UnsubscribeTrack(RemoteTrack track)
		{
#if DISABLE_WEBRTC
			return false;
#endif // DISABLE_WEBRTC

			RtcChannel.UnsubscribeTrack(track);
			return true;
		}

		public LocalTrack? PublishTrack(eTrackType trackType, IPublishableLocalUser publisher, IPublishableRemoteUser? target = null)
		{
			if (!publisher.Modules.IsConnectionTarget(trackType))
				return null;

			var audioPublishSettings = publisher.GetAudioPublishSettings(trackType);
			if (audioPublishSettings != null)
			{
				var audio       = publisher.Modules.GetAudio(trackType);
				var audioSource = audio?.RawAudioSource;
				if (audioSource.IsUnityNull() || !audioSource!.isActiveAndEnabled || !audioSource.isPlaying)
					return null;

				return trackType switch
				{
					eTrackType.VOICE => PublishVoice(audioPublishSettings, audioSource, target),
					eTrackType.AUDIO => PublishAudio(audioPublishSettings, audioSource, target),
					_                => null,
				};
			}

			var videoPublishSettings = publisher.GetVideoPublishSettings(trackType);
			if (videoPublishSettings != null)
			{
				var video        = publisher.Modules.GetVideo(trackType);
				var videoTexture = video?.Texture;
				if (videoTexture.IsUnityNull())
					return null;

				return trackType switch
				{
					eTrackType.CAMERA => PublishCamera(videoPublishSettings, videoTexture!, target),
					eTrackType.SCREEN => PublishScreen(videoPublishSettings, videoTexture!, target),
					eTrackType.VIDEO  => PublishVideo(videoPublishSettings, videoTexture!, target),
					_                 => null,
				};
			}

			return null;
		}

		public bool UnpublishTrack(LocalTrack track)
		{
#if DISABLE_WEBRTC
			return false;
#endif // DISABLE_WEBRTC

			try
			{
				RtcChannel.UnpublishTrack(track);
				return true;
			}
			catch (ObjectDisposedException e)
			{
				C2VDebug.LogWarningMethod(nameof(RtcChannelAdapter), e.Message);
				return false;
			}
		}

		private LocalTrack? PublishVoice(AudioPublishSettings settings, AudioSource audioSource, IPublishableRemoteUser? target = null)
		{
#if DISABLE_WEBRTC
			return null;
#endif // DISABLE_WEBRTC

			return target == null
				? RtcChannel.PublishMic(audioSource, settings.Convert(), true, true)
				: RtcChannel.PublishMic(target.MediaSdkUser, audioSource, settings.Convert(), true, true)
			   ?? throw new SdkException($"{nameof(PublishVoice)}Failed: {target}");
		}

		private LocalTrack? PublishAudio(AudioPublishSettings settings, AudioSource audioSource, IPublishableRemoteUser? target = null)
		{
#if DISABLE_WEBRTC
			return null;
#endif // DISABLE_WEBRTC

			// TODO: SDK쪽 구현 필요
			C2VDebug.LogWarningMethod(nameof(RtcChannelAdapter), $"{nameof(PublishAudio)} is not implemented yet.");
			return null;
		}

		private LocalTrack? PublishCamera(VideoPublishSettings settings, Texture videoTexture, IPublishableRemoteUser? target = null)
		{
#if DISABLE_WEBRTC
			return null;
#endif // DISABLE_WEBRTC

			return target == null
				? RtcChannel.PublishCamera(videoTexture!, settings.Convert())
				: RtcChannel.PublishCamera(target.MediaSdkUser, videoTexture!, settings.Convert())
			   ?? throw new SdkException($"{nameof(PublishCamera)}Failed: {target}");
		}

		private LocalTrack? PublishScreen(VideoPublishSettings settings, Texture videoTexture, IPublishableRemoteUser? target = null)
		{
#if DISABLE_WEBRTC
			return null;
#endif // DISABLE_WEBRTC

			return target == null
				? RtcChannel.PublishScreen(videoTexture!, settings.Convert())
				: RtcChannel.PublishScreen(target.MediaSdkUser, videoTexture!, settings.Convert())
			   ?? throw new SdkException($"{nameof(PublishCamera)}Failed: {target}");
		}

		private LocalTrack? PublishVideo(VideoPublishSettings settings, Texture videoTexture, IPublishableRemoteUser? target = null)
		{
#if DISABLE_WEBRTC
			return null;
#endif // DISABLE_WEBRTC

			// TODO: SDK쪽 구현 필요
			C2VDebug.LogWarningMethod(nameof(RtcChannelAdapter), $"{nameof(PublishVideo)} is not implemented");
			return null;
		}

		public bool GetVideoQuality(out VideoQuality quality)
		{
			return RtcChannel.GetCurrentVideoQuality(out quality);
		}

		public void ChangeAudioQuality(LocalAudioTrack track, ulong bitrate)
		{
			// TODO: SDK쪽 구현 필요
			C2VDebug.LogWarningMethod(nameof(RtcChannelAdapter), $"{nameof(ChangeAudioQuality)} is not implemented");
		}

		public void ChangeVideoQuality(LocalVideoTrack track, VideoQuality quality)
		{
			RtcChannel.ChangePublisVideohSetting(track, quality);
		}
	}
}
