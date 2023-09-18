/*===============================================================
* Product:		Com2Verse
* File Name:	VolumeDetector.cs
* Developer:	urun4m0r1
* Date:			2022-08-19 17:05
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Com2Verse.Extension;
using Com2Verse.Sound;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Communication
{
	public sealed class VolumeDetector : IDisposable
	{
		public event Action? DetectionStarted;
		public event Action? DetectionStopped;

#region VuLevel
		public event Action<float>? VuLevelChanged;

		private float _vuLevel;

		public float VuLevel
		{
			get => _vuLevel;
			private set
			{
				_vuLevel = value;
				PpmLevel = GetPpmLevel(PpmLevel, value, Define.VoiceDetection.UpdateInterval / 1000f, VoiceDetectionManager.Instance.Settings?.VolumeSmoothingRate ?? 1f);
				VuLevelChanged?.Invoke(value);
			}
		}
#endregion // VuLevel

#region PpmLevel
		public event Action<float>? PpmLevelChanged;

		private float _ppmLevel;

		public float PpmLevel
		{
			get => _ppmLevel;
			private set
			{
				_ppmLevel = value;
				PpmLevelChanged?.Invoke(value);
			}
		}
#endregion // PpmLevel

		private CancellationTokenSource? _tokenSource;

		private float[]? _sampleData;

		private readonly IAudioSourceProvider _audioSourceProvider;

		public VolumeDetector(IAudioSourceProvider audioSourceProvider)
		{
			_audioSourceProvider = audioSourceProvider;

			_audioSourceProvider.AudioSourceChanged += OnAudioSourceChanged;
		}

		public void Dispose()
		{
			_audioSourceProvider.AudioSourceChanged -= OnAudioSourceChanged;

			StopInputLevelDetection();
		}

		private void OnAudioSourceChanged(MetaverseAudioSource? audioSource)
		{
			if (audioSource.IsUnityNull())
			{
				StopInputLevelDetection();
			}
			else
			{
				StartInputLevelDetection();
			}
		}

		private void StartInputLevelDetection()
		{
			if (_tokenSource != null)
				return;

			_tokenSource = new CancellationTokenSource();
			StartUpdateInputLevel().Forget();
			DetectionStarted?.Invoke();
		}

		private async UniTask StartUpdateInputLevel()
		{
			var context = SynchronizationContext.Current;
			if (!await UniTaskHelper.TrySwitchToMainThread(context, _tokenSource))
				return;

			_sampleData ??= new float[Define.VoiceDetection.SampleLength];

			do
			{
				UpdateInputLevel(_sampleData);
			}
			while (await UniTaskHelper.Delay(Define.VoiceDetection.UpdateInterval, _tokenSource));

			await UniTaskHelper.TrySwitchToSynchronizationContext(context);

			StopInputLevelDetection();
		}

		private void UpdateInputLevel(float[] sampleData)
		{
			var audioSource = _audioSourceProvider.AudioSource;
			if (audioSource.IsUnityNull() || !audioSource!.IsPlaying)
			{
				VuLevel = 0f;
			}
			else
			{
				audioSource.GetData(sampleData, applySourceVolume: true);
				VuLevel = GetVuLevel(sampleData, Define.VoiceDetection.SampleLength);
			}
		}

		private void StopInputLevelDetection()
		{
			_tokenSource?.Cancel();
			_tokenSource?.Dispose();
			_tokenSource = null;

			_vuLevel  = 0f;
			_ppmLevel = 0f;

			VuLevelChanged?.Invoke(_vuLevel);
			PpmLevelChanged?.Invoke(_ppmLevel);
			DetectionStopped?.Invoke();
		}

#region VoiceDetectionAlgorithm
		private static float GetPpmLevel(float prevLevel, float newLevel, float updateInterval, float decreaseRate)
		{
			var levelDecrease  = decreaseRate * updateInterval;
			var decreasedLevel = Mathf.Clamp01(prevLevel - levelDecrease);
			return Mathf.Max(newLevel, decreasedLevel);
		}

		private static float GetRmsLevel(IReadOnlyList<float> input, int length)
		{
			var powSum = 0f;
			for (var i = 0; i < length; ++i)
			{
				var sample = input[i];
				powSum += sample * sample;
			}

			return Mathf.Sqrt(powSum / length);
		}

		private static float GetVuLevel(IReadOnlyList<float> input, int length)
		{
			var sum = 0f;
			for (var i = 0; i < length; ++i)
				sum += Mathf.Abs(input[i]);

			return sum / length;
		}
#endregion // VoiceDetectionAlgorithm
	}
}
