/*===============================================================
* Product:		Com2Verse
* File Name:	MicrophoneAudioProvider.cs
* Developer:	urun4m0r1
* Date:			2022-08-24 18:07
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Threading;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Sound;
using Com2Verse.Utils;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Com2Verse.Communication.Unity
{
	public sealed class LoopbackAudioPipeline : BaseVolume, IAudioSourcePipeline
	{
#region IAudioSourcePipeline
		private IAudioSourceProvider? _target;

		public IAudioSourceProvider? Target
		{
			get => _target;
			set
			{
				var prevValue = _target;
				if (prevValue == value)
					return;

				_target = value;

				if (prevValue != null)
				{
					prevValue.LevelChanged       -= OnTargetLevelChanged;
					prevValue.AudioSourceChanged -= OnTargetAudioSourceChanged;
				}

				if (_target != null)
				{
					_target.LevelChanged       += OnTargetLevelChanged;
					_target.AudioSourceChanged += OnTargetAudioSourceChanged;

					Level = _target.Level;
				}

				UpdateProcessState();
			}
		}

		private void OnTargetLevelChanged(float level)
		{
			Level = level;
		}

		private void OnTargetAudioSourceChanged(MetaverseAudioSource? audioSource)
		{
			UpdateProcessState();
		}
#endregion // IAudioSourcePipeline

#region IModule
		public event Action<bool>? StateChanged;

		private bool _isRunning;

		public bool IsRunning
		{
			get => _isRunning;
			set
			{
				var prevValue = _isRunning;
				if (prevValue == value)
					return;

				_isRunning = value;
				StateChanged?.Invoke(value);

				UpdateProcessState();
			}
		}
#endregion // IModule

#region IVolume
		protected override void ApplyLevel(float value)
		{
			base.ApplyLevel(value);

			if (!_loopbackSource.IsUnityNull())
				_loopbackSource!.Volume = value;
		}

		protected override void ApplyAudible(bool value)
		{
			base.ApplyAudible(value);

			if (!_loopbackSource.IsUnityNull())
				_loopbackSource!.TargetMixerGroup = GetAudioMixerGroupIndex(value);
		}
#endregion // IVolume

#region IAudioSourceProvider
		public event Action<MetaverseAudioSource?>? AudioSourceChanged;

		private MetaverseAudioSource? _audioSource;

		public MetaverseAudioSource? AudioSource
		{
			get => _audioSource;
			private set
			{
				var prevValue = _audioSource;
				if (prevValue == value)
					return;

				_audioSource = value;
				AudioSourceChanged?.Invoke(value);

				ApplyLevel(Level);
				ApplyAudible(IsAudible);
			}
		}

		private static int GetAudioMixerGroupIndex(bool isAudible) => isAudible
			? AudioMixerGroupIndex.LocalVoice
			: AudioMixerGroupIndex.Mute;
#endregion // IAudioSourceProvider

		public int PlaybackDelay { get; set; } = Define.Audio.DefaultLoopbackDelay;

		private CancellationTokenSource? _tokenSource;

		private MetaverseAudioSource? _loopbackSource;

		private readonly IDevice                _audioRecorder;
		private readonly IReadOnlyAudioSettings _settings;

		public LoopbackAudioPipeline(IDevice audioRecorder, IReadOnlyAudioSettings settings) : base(1f)
		{
			_audioRecorder = audioRecorder;
			_settings      = settings;

			_audioRecorder.DeviceChanged += OnDeviceChanged;
			_audioRecorder.DeviceFailed  += OnDeviceFailed;
			_settings.SettingsChanged    += OnRequestedSettingsChanged;
		}

		private void OnDeviceChanged(DeviceInfo prevDevice, int prevIndex, DeviceInfo deviceInfo, int deviceIndex)
		{
			IsRunning = false;
		}

		private void OnDeviceFailed(DeviceInfo deviceInfo, int deviceIndex)
		{
			IsRunning = false;
		}

		private void OnRequestedSettingsChanged(IReadOnlyAudioSettings _)
		{
			IsRunning = false;
		}

		private void UpdateProcessState()
		{
			var audioSource = Target?.AudioSource;
			if (IsRunning && !audioSource.IsUnityNull())
			{
				StopProcessLoop();

				_tokenSource = new CancellationTokenSource();
				StartProcessLoopAsync(audioSource!).Forget();
			}
			else
			{
				IsRunning   = false;
				AudioSource = audioSource;

				StopProcessLoop();
			}
		}

		private MetaverseAudioSource CreateLoopbackSource(AudioClip inputClip)
		{
			var className = GetType().Name;
			var name      = ZString.Format("[{0}]", className);

			var go = new GameObject(name) { hideFlags = HideFlags.DontSave };
			Object.DontDestroyOnLoad(go);

			var audioSource    = go.AddComponent<AudioSource>()!;
			var metaverseAudio = MetaverseAudioSource.CreateWithSource(go, audioSource)!;
			audioSource.name    = name;
			metaverseAudio.name = name;
			metaverseAudio.Loop = true;

			var outputClip = AudioClip.Create(name, inputClip.samples, inputClip.channels, inputClip.frequency, stream: false)!;
			metaverseAudio.SetClip(outputClip);
			metaverseAudio.Play();
			return metaverseAudio;
		}

		private async UniTask StartProcessLoopAsync(MetaverseAudioSource source)
		{
			var inputClip = source.GetClip();
			if (inputClip.IsUnityNull())
			{
				C2VDebug.LogErrorMethod(nameof(LoopbackAudioPipeline), "Failed to get clip");
				StopProcessLoop();
				return;
			}

			var loopbackPosition = inputClip!.samples - MathUtil.ToSecondsInt(PlaybackDelay) * inputClip.frequency;
			var microphoneBuffer = new float[inputClip.samples];
			var loopbackBuffer   = new float[128];

			var delayMs = MathUtil.ToMilliseconds((float)loopbackBuffer.Length / inputClip.frequency);

			_loopbackSource = CreateLoopbackSource(inputClip);
			var outputClip = _loopbackSource.GetClip()!;
			AudioSource = _loopbackSource;

			while (await UniTaskHelper.Delay(delayMs, _tokenSource))
			{
				var rawAudioSource = source.AudioSource;
				if (rawAudioSource.IsUnityNull())
				{
					C2VDebug.LogErrorMethod(nameof(LoopbackAudioPipeline), "Failed to get raw audio source");
					StopProcessLoop();
					return;
				}

				Process(rawAudioSource!.timeSamples, inputClip, outputClip, microphoneBuffer, loopbackBuffer, ref loopbackPosition);
			}
		}

		private void Process(int currentPosition, AudioClip inputClip, AudioClip outputClip, float[] inputBuffer, float[] outputBuffer, ref int loopbackPosition)
		{
			if (currentPosition < 0 || currentPosition == loopbackPosition)
				return;

			inputClip.GetData(inputBuffer, offsetSamples: 0);

			while (ArrayUtils.GetCycleArrayLength(inputBuffer.Length, loopbackPosition, currentPosition) > outputBuffer.Length)
			{
				ArrayUtils.CycleCopyArray(inputBuffer, loopbackPosition, outputBuffer, destinationIndex: 0, outputBuffer.Length);
				outputClip.SetData(outputBuffer, loopbackPosition);

				loopbackPosition += outputBuffer.Length;
				if (loopbackPosition >= inputBuffer.Length)
					loopbackPosition -= inputBuffer.Length;
			}
		}

		private void StopProcessLoop()
		{
			_tokenSource?.Cancel();
			_tokenSource?.Dispose();
			_tokenSource = null;

			_loopbackSource.DestroyGameObject();
			_loopbackSource = null;
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				StopProcessLoop();

				_audioRecorder.DeviceChanged -= OnDeviceChanged;
				_audioRecorder.DeviceFailed  -= OnDeviceFailed;
				_settings.SettingsChanged    -= OnRequestedSettingsChanged;
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
