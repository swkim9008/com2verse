/*===============================================================
* Product:		Com2Verse
* File Name:	ModuleManager.cs
* Developer:	urun4m0r1
* Date:			2022-08-19 13:33
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Communication.Matting;
using Com2Verse.Logger;
using Com2Verse.ScreenShare;
using JetBrains.Annotations;

namespace Com2Verse.Communication.Unity
{
	public sealed class ModuleManager : Singleton<ModuleManager>, IDisposable
	{
		public AudioSettings VoiceSettings  { get; } = new();
		public VideoSettings CameraSettings { get; } = new();
		public VideoSettings ScreenSettings { get; } = new();

		public AudioPublishSettings VoicePublishSettings  { get; } = new();
		public VideoPublishSettings CameraPublishSettings { get; } = new();
		public VideoPublishSettings ScreenPublishSettings { get; } = new();

		public Audio Voice  { get; }
		public Video Camera { get; }
		public Video Screen { get; }

		public MicrophoneAudioProvider MicrophoneAudioProvider { get; }
		public WebcamTextureProvider   WebcamTextureProvider   { get; }
		public ScreenTextureProvider   ScreenTextureProvider   { get; }

		public NoiseReductionAudioPipeline VoiceNoiseReductionPipeline { get; }
		public LoopbackAudioPipeline       LoopbackAudioPipeline       { get; }
		public HumanMattingTexturePipeline HumanMattingTexturePipeline { get; }

		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private ModuleManager()
		{
			var audioRecorder     = DeviceManager.Instance.AudioRecorder;
			var videoRecorder     = DeviceManager.Instance.VideoRecorder;
			var captureController = ScreenCaptureManager.Instance.Controller;

			MicrophoneAudioProvider = new MicrophoneAudioProvider(audioRecorder, VoiceSettings)
			{
				Level     = 1f,
				IsAudible = false,
			};

			WebcamTextureProvider = new WebcamTextureProvider(videoRecorder, CameraSettings);
			ScreenTextureProvider = new ScreenTextureProvider(captureController, ScreenSettings);

			VoiceNoiseReductionPipeline = new NoiseReductionAudioPipeline(audioRecorder)
			{
				Level     = 1f,
				IsAudible = false,
			};

			LoopbackAudioPipeline = new LoopbackAudioPipeline(audioRecorder, VoiceSettings)
			{
				Level     = 1f,
				IsAudible = true,
			};

			HumanMattingTexturePipeline = new HumanMattingTexturePipeline();

			Voice = new Audio(MicrophoneAudioProvider, new List<IAudioSourcePipeline>
			{
				VoiceNoiseReductionPipeline,
				LoopbackAudioPipeline,
			});

			Camera = new Video(WebcamTextureProvider, new List<IVideoTexturePipeline>
			{
				HumanMattingTexturePipeline,
			});

			Screen = new Video(ScreenTextureProvider);

			VoiceSettings.SettingsChanged  += OnRequestedVoiceSettingsChanged;
			CameraSettings.SettingsChanged += OnRequestedCameraSettingsChanged;
			ScreenSettings.SettingsChanged += OnRequestedScreenSettingsChanged;

			VoicePublishSettings.SettingsChanged  += OnRequestedVoicePublishSettingsChanged;
			CameraPublishSettings.SettingsChanged += OnRequestedCameraPublishSettingsChanged;
			ScreenPublishSettings.SettingsChanged += OnRequestedScreenPublishSettingsChanged;

			LoopbackAudioPipeline.StateChanged         += OnLoopbackAudioPipelineStateChanged;
			if (ChannelManager.InstanceExists)
				ChannelManager.Instance.ViewModelUserAdded += OnViewModelUserAdded;
		}

		/// <summary>
		/// 루프백 오디오 사용 도중에는 WebRTC Publish 타깃에서 제외
		/// </summary>
		private void OnLoopbackAudioPipelineStateChanged(bool isRunning)
		{
			foreach (var user in ChannelManager.Instance.GetViewModelUsers())
			{
				if (isRunning) user.Modules.TryAddConnectionBlocker(eTrackType.VOICE, LoopbackAudioPipeline);
				else user.Modules.RemoveConnectionBlocker(eTrackType.VOICE, LoopbackAudioPipeline);
			}
		}

		/// <summary>
		/// 루프백 오디오 사용 여부에 따라 WebRTC Publish 타깃에서 제외
		/// </summary>
		private void OnViewModelUserAdded(IChannel channel, IViewModelUser user)
		{
			if (LoopbackAudioPipeline.IsRunning) user.Modules.TryAddConnectionBlocker(eTrackType.VOICE, LoopbackAudioPipeline);
			else user.Modules.RemoveConnectionBlocker(eTrackType.VOICE, LoopbackAudioPipeline);
		}

		private void OnRequestedVoiceSettingsChanged(IReadOnlyAudioSettings  settings) => C2VDebug.LogMethod(GetType().Name, settings.GetInfoText());
		private void OnRequestedCameraSettingsChanged(IReadOnlyVideoSettings settings) => C2VDebug.LogMethod(GetType().Name, settings.GetInfoText());
		private void OnRequestedScreenSettingsChanged(IReadOnlyVideoSettings settings) => C2VDebug.LogMethod(GetType().Name, settings.GetInfoText());

		private void OnRequestedVoicePublishSettingsChanged(IReadOnlyAudioPublishSettings  settings) => C2VDebug.LogMethod(GetType().Name, settings.GetInfoText());
		private void OnRequestedCameraPublishSettingsChanged(IReadOnlyVideoPublishSettings settings) => C2VDebug.LogMethod(GetType().Name, settings.GetInfoText());
		private void OnRequestedScreenPublishSettingsChanged(IReadOnlyVideoPublishSettings settings) => C2VDebug.LogMethod(GetType().Name, settings.GetInfoText());

		public void Dispose()
		{
			var channelManager = ChannelManager.InstanceOrNull;
			if (channelManager != null)
				channelManager.ViewModelUserAdded -= OnViewModelUserAdded;

			MicrophoneAudioProvider.Dispose();
			WebcamTextureProvider.Dispose();
			ScreenTextureProvider.Dispose();

			VoiceNoiseReductionPipeline.Dispose();
			HumanMattingTexturePipeline.Dispose();

			Voice.Dispose();
			Camera.Dispose();
			Screen.Dispose();
		}
	}
}
