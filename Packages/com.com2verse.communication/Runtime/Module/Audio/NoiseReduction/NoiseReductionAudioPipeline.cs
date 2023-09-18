/*===============================================================
* Product:		Com2Verse
* File Name:	HumanMattingTexturePipeline.cs
* Developer:	urun4m0r1
* Date:			2022-07-11 17:04
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Threading;
using com.com2verse.voicedenoise;
using Com2Verse.Communication.Unity;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Sound;
using Com2Verse.Utils;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Com2Verse.Communication.Matting
{
	public sealed class NoiseReductionAudioPipeline : BaseVolume, IAudioSourcePipeline
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

			if (!_denoiseSource.IsUnityNull())
				_denoiseSource!.Volume = value;
		}

		protected override void ApplyAudible(bool value)
		{
			base.ApplyAudible(value);

			if (!_denoiseSource.IsUnityNull())
				_denoiseSource!.TargetMixerGroup = GetAudioMixerGroupIndex(value);
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

		public bool IsInitializing { get; private set; }
		public bool IsInitialized  { get; private set; }

		private readonly IDevice _audioRecorder;

		private DenoiseManager?          _predictor;
		private MetaverseAudioSource?    _denoiseSource;
		private CancellationTokenSource? _tokenSource;

		public NoiseReductionAudioPipeline(IDevice audioRecorder) : base(1f)
		{
			_audioRecorder = audioRecorder;
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
				AudioSource = audioSource;

				StopProcessLoop();
			}
		}

		private static async UniTask<DenoiseManager?> TryCreateModelAsync(AudioClip? inputClip, CancellationTokenSource? tokenSource)
		{
			var resourceHandler1 = Resources.LoadAsync("model_p1_split");
			var resourceHandler2 = Resources.LoadAsync("model_p2_split");
			if (resourceHandler1 == null || resourceHandler2 == null)
			{
				C2VDebug.LogErrorMethod(nameof(NoiseReductionAudioPipeline), "Failed to load model");
				return null;
			}

			var predictor    = new DenoiseManager();
			var model1       = await resourceHandler1.ToUniTask();
			var model2       = await resourceHandler2.ToUniTask();
			var modelHandler = predictor.CreateModelAsync(model1, model2);
			if (modelHandler == null)
			{
				C2VDebug.LogErrorMethod(nameof(NoiseReductionAudioPipeline), "Failed to create model");
				predictor.Dispose();
				return null;
			}

			if (!await modelHandler.AsUniTask())
			{
				C2VDebug.LogErrorMethod(nameof(NoiseReductionAudioPipeline), "Failed to create model");
				modelHandler.Dispose();
				predictor.Dispose();
				return null;
			}

			if (!predictor.CreatePredictor())
			{
				C2VDebug.LogErrorMethod(nameof(NoiseReductionAudioPipeline), "Failed to create predictor");
				modelHandler.Dispose();
				predictor.Dispose();
				return null;
			}

			if (!await UniTaskHelper.WaitUntil(() => predictor.GetState() == DenoiseManager.State.Ready, tokenSource))
			{
				C2VDebug.LogErrorMethod(nameof(NoiseReductionAudioPipeline), "Failed to load model");
				modelHandler.Dispose();
				predictor.Dispose();
				return null;
			}

			if (inputClip.IsUnityNull())
			{
				C2VDebug.LogErrorMethod(nameof(NoiseReductionAudioPipeline), "Failed to get clip");
				modelHandler.Dispose();
				predictor.Dispose();
				return null;
			}

			if (inputClip!.frequency != predictor.GetSamplingRate())
			{
				C2VDebug.LogErrorMethod(nameof(NoiseReductionAudioPipeline), "Invalid frequency");
				modelHandler.Dispose();
				predictor.Dispose();
				return null;
			}

			return predictor;
		}

		private static MetaverseAudioSource CreateDenoiseSource(string name, AudioClip inputClip)
		{
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
			if (IsInitialized || IsInitializing)
				return;

			IsInitializing = true;

			var inputClip = source.GetClip();
			if (_predictor == null)
			{
				var predictor = await TryCreateModelAsync(inputClip, _tokenSource);
				_predictor = predictor;
			}

			if (_predictor == null)
			{
				IsInitializing = false;
				StopProcessLoop();
				return;
			}

			var timer = 0;
			while (_predictor.GetState() != DenoiseManager.State.Ready)
			{
				if (timer > 3000)
				{
					IsInitializing = false;
					StopProcessLoop();
					return;
				}
				timer += (int)(Time.deltaTime * 1000);
				await UniTask.Yield();
			}

			var className     = GetType().Name;
			var name          = ZString.Format("[{0}]", className);
			var denoiseSource = CreateDenoiseSource(name, inputClip!);
			var outputClip    = denoiseSource.GetClip()!;
			_denoiseSource = denoiseSource;
			AudioSource    = denoiseSource;

			var denoisePosition  = 0;
			var microphoneBuffer = new float[inputClip!.samples];
			var denoiseBuffer    = new float[_predictor!.GetBufferSize()];

			var delayMs = MathUtil.ToMilliseconds((float)denoiseBuffer.Length / inputClip.frequency);

			IsInitializing = false;
			IsInitialized  = true;

			do
			{
				Process(_predictor, inputClip, outputClip, microphoneBuffer, denoiseBuffer, ref denoisePosition);
			}
			while (await UniTaskHelper.Delay(delayMs, _tokenSource));
		}

		private void Process(DenoiseManager denoiseManager, AudioClip inputClip, AudioClip outputClip, float[] inputBuffer, float[] outputBuffer, ref int denoisePosition)
		{
			var microphonePosition = MicrophoneConnector.GetPosition(_audioRecorder.Current);
			if (microphonePosition < 0 || microphonePosition == denoisePosition)
				return;

			inputClip.GetData(inputBuffer, offsetSamples: 0);

			while (ArrayUtils.GetCycleArrayLength(inputBuffer.Length, denoisePosition, microphonePosition) > outputBuffer.Length)
			{
				ArrayUtils.CycleCopyArray(inputBuffer, denoisePosition, outputBuffer, destinationIndex: 0, outputBuffer.Length);

				var denoisedData = denoiseManager.ProcessBuffer(outputBuffer);
				if (denoisedData != null)
				{
					Array.Copy(denoisedData, sourceIndex: 0, outputBuffer, destinationIndex: 0, outputBuffer.Length);
					outputClip.SetData(outputBuffer, denoisePosition);
				}

				denoisePosition += outputBuffer.Length;
				if (denoisePosition >= inputBuffer.Length)
					denoisePosition -= inputBuffer.Length;
			}
		}

		private void StopProcessLoop()
		{
			_tokenSource?.Cancel();
			_tokenSource?.Dispose();
			_tokenSource = null;

			_denoiseSource.DestroyGameObject();
			_denoiseSource = null;

			IsInitializing = false;
			IsInitialized  = false;
		}

#region IDisposable
		private bool _disposed;

		~NoiseReductionAudioPipeline()
		{
			Dispose(false);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				StopProcessLoop();
			}

			_predictor?.Dispose();
			_predictor = null;

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
