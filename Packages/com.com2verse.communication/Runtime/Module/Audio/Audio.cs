/*===============================================================
* Product:		Com2Verse
* File Name:	Audio.cs
* Developer:	urun4m0r1
* Date:			2022-08-29 20:12
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Extension;
using Com2Verse.Sound;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Communication
{
	public class Audio : IAudioSourceProvider, IDisposable
	{
#region Decorator
		public bool IsRunning
		{
			get => Input.IsRunning;
			set => Input.IsRunning = value;
		}

		public event Action<bool>? StateChanged
		{
			add => Input.StateChanged += value;
			remove => Input.StateChanged -= value;
		}

		public float Level
		{
			get => Input.Level;
			set => Input.Level = value;
		}

		public bool IsAudible
		{
			get => Input.IsAudible;
			set => Input.IsAudible = value;
		}

		public event Action<float>? LevelChanged
		{
			add => Input.LevelChanged += value;
			remove => Input.LevelChanged -= value;
		}

		public event Action<bool>? AudibleChanged
		{
			add => Input.AudibleChanged += value;
			remove => Input.AudibleChanged -= value;
		}
#endregion // Decorator

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
				UniTaskHelper.InvokeOnMainThread(() => AudioSourceChanged?.Invoke(value)).Forget();
			}
		}

		public AudioSource? RawAudioSource
		{
			get
			{
				if (AudioSource.IsUnityNull()) return null;
				return AudioSource!.AudioSource;
			}
		}

		public IAudioSourceProvider Input { get; }

		public VolumeDetector VolumeDetector { get; }
		public SpeechDetector SpeechDetector { get; }

		private readonly IEnumerable<IAudioSourcePipeline>? _audioSourcePipelines;

		public Audio(IAudioSourceProvider input, IEnumerable<IAudioSourcePipeline>? audioSourcePipelines = null)
		{
			Input = input;

			_audioSourcePipelines = audioSourcePipelines;

			if (_audioSourcePipelines != null)
			{
				var target = Input;
				foreach (var pipeline in _audioSourcePipelines)
				{
					pipeline.Target = target;
					target          = pipeline;
				}

				target.AudioSourceChanged += OnInputAudioSourceChanged;
			}

			Input.AudioSourceChanged += OnInputAudioSourceChanged;

			VolumeDetector = new VolumeDetector(this);
			SpeechDetector = new SpeechDetector(VolumeDetector);
		}

		public void Dispose()
		{
			if (_audioSourcePipelines != null)
			{
				var target = Input;
				foreach (var pipeline in _audioSourcePipelines)
				{
					pipeline.Target = null;
					target          = pipeline;
				}

				target.AudioSourceChanged -= OnInputAudioSourceChanged;
			}

			Input.AudioSourceChanged -= OnInputAudioSourceChanged;

			AudioSource = null;

			VolumeDetector.Dispose();
			SpeechDetector.Dispose();
		}

		private void OnInputAudioSourceChanged(MetaverseAudioSource? audioSource)
		{
			if (_audioSourcePipelines != null)
			{
				foreach (var pipeline in _audioSourcePipelines.Reverse())
				{
					if (!pipeline.IsRunning || pipeline.AudioSource.IsUnityNull())
						continue;

					AudioSource = pipeline.AudioSource;
					return;
				}

				AudioSource = Input.AudioSource;
				return;
			}

			AudioSource = audioSource;
		}
	}
}
