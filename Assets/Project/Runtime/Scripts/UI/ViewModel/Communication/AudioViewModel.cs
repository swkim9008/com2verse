/*===============================================================
* Product:		Com2Verse
* File Name:	LocalAudioViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-08-29 21:34
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Communication;
using Com2Verse.Communication.Unity;
using Cysharp.Text;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class VoiceViewModel : AudioViewModel<Audio>
	{
		public static VoiceViewModel Empty { get; } = new();

		public VoiceViewModel(Audio audio) : base(audio) { }

		public VoiceViewModel() : this(ModuleManager.Instance.Voice) { }
	}

	public abstract class AudioViewModel<T> : ViewModelBase, IDisposable where T : Audio
	{
		public T Value { get; }

		[UsedImplicitly] public CommandHandler<bool> SetInputState   { get; }
		[UsedImplicitly] public CommandHandler<bool> SetAudibleState { get; }

		[UsedImplicitly] public CommandHandler ToggleInputState   { get; }
		[UsedImplicitly] public CommandHandler ToggleAudibleState { get; }

		protected AudioViewModel(T audio)
		{
			Value = audio;

			SetInputState   = new CommandHandler<bool>(value => IsRunning = value);
			SetAudibleState = new CommandHandler<bool>(value => IsAudible = value);

			ToggleInputState   = new CommandHandler(() => IsRunning ^= true);
			ToggleAudibleState = new CommandHandler(() => IsAudible ^= true);

			RegisterEvents();
		}

		public void Dispose()
		{
			Value.Input.StateChanged   -= OnInputRunningStateChanged;
			Value.Input.AudibleChanged -= OnInputAudibleChanged;
			Value.Input.LevelChanged   -= OnInputLevelChanged;

			if (Value.Input is MicrophoneAudioProvider)
			{
				var moduleManager = ModuleManager.InstanceOrNull;
				if (moduleManager != null)
					moduleManager.LoopbackAudioPipeline.StateChanged -= OnLoopbackStateChanged;
			}

			Value.VolumeDetector.VuLevelChanged     -= OnVuLevelChanged;
			Value.SpeechDetector.SpeakerTypeChanged -= OnSpeakerTypeChanged;
		}

		private void RegisterEvents()
		{
			Value.Input.StateChanged   += OnInputRunningStateChanged;
			Value.Input.AudibleChanged += OnInputAudibleChanged;
			Value.Input.LevelChanged   += OnInputLevelChanged;

			if (Value.Input is MicrophoneAudioProvider)
				ModuleManager.Instance.LoopbackAudioPipeline.StateChanged += OnLoopbackStateChanged;

			Value.VolumeDetector.VuLevelChanged     += OnVuLevelChanged;
			Value.SpeechDetector.SpeakerTypeChanged += OnSpeakerTypeChanged;
		}

		private void OnInputRunningStateChanged(bool _)
		{
			InvokePropertyValueChanged(nameof(IsRunning), IsRunning);
		}

		private void OnInputAudibleChanged(bool _)
		{
			InvokePropertyValueChanged(nameof(IsAudible), IsAudible);
		}

		private void OnInputLevelChanged(float _)
		{
			InvokePropertyValueChanged(nameof(Level),            Level);
			InvokePropertyValueChanged(nameof(LevelPercentText), LevelPercentText);
		}

		private void OnLoopbackStateChanged(bool obj)
		{
			InvokePropertyValueChanged(nameof(IsAudible), IsAudible);
		}

		private void OnVuLevelChanged(float _)
		{
			InvokePropertyValueChanged(nameof(VuLevel),                     VuLevel);
			InvokePropertyValueChanged(nameof(InversedVuLevel),             InversedVuLevel);
			InvokePropertyValueChanged(nameof(InterpolatedVuLevel),         InterpolatedVuLevel);
			InvokePropertyValueChanged(nameof(InterpolatedInversedVuLevel), InterpolatedInversedVuLevel);
		}

		private void OnSpeakerTypeChanged(eSpeakerType _)
		{
			InvokePropertyValueChanged(nameof(IsSpeaking), IsSpeaking);
			InvokePropertyValueChanged(nameof(IsSpeaker),  IsSpeaker);
		}

#region ViewModelProperties
		/// <summary>
		/// Local: 시스템 마이크 연결 여부
		/// Remote: 트랙 Subscribe 여부
		/// </summary>
		public bool IsRunning
		{
			get => Value.Input.IsRunning;
			set => Value.Input.IsRunning = value;
		}

		/// <summary>
		/// Local: 루프백 오디오 활성화 여부
		/// Remote: 오디오 믹서 출력 여부
		/// </summary>
		public bool IsAudible
		{
			get
			{
				if (Value.Input is MicrophoneAudioProvider)
					return ModuleManager.Instance.LoopbackAudioPipeline.IsRunning;

				return Value.Input.IsAudible;
			}
			set
			{
				if (Value.Input is MicrophoneAudioProvider)
				{
					ModuleManager.Instance.LoopbackAudioPipeline.IsRunning = value;
					return;
				}

				Value.Input.IsAudible = value;
			}
		}

		/// <summary>
		/// 개별 AudioSource 음량 설정
		/// </summary>
		public float Level
		{
			get => Value.Input.Level;
			set => Value.Input.Level = value;
		}

		/// <summary>
		/// 개별 AudioSource 음량 (UI 표시용, 16%)
		/// </summary>
		public string LevelPercentText => ZString.Format("{0:0}%", Level * 100f);

#region VuLevel
		/// <summary>
		/// 실제 녹음되는 소리의 음량
		/// </summary>
		public float VuLevel => Value.VolumeDetector.VuLevel;

		/// <summary>
		/// 1 - VuLevel (UI 표시용)
		/// </summary>
		public float InversedVuLevel => 1f - VuLevel;

		/// <summary>
		/// VuLevel을 증폭한 값 (UI 표시용)
		/// </summary>
		public float InterpolatedVuLevel => VuLevel * Utils.Define.AudioInputLevelUiMultiplier;

		/// <summary>
		/// 1 - InterpolatedVuLevel (UI 표시용)
		/// </summary>
		public float InterpolatedInversedVuLevel => 1f - InterpolatedVuLevel;
#endregion // VuLevel

#region SpeechType
		/// <summary>
		/// 음성 인식 여부
		/// </summary>
		public bool IsSpeaking => Value.SpeechDetector.IsSpeaking;

		/// <summary>
		/// 음성 지속 유지 여부
		/// </summary>
		public bool IsSpeaker => Value.SpeechDetector.IsSpeaker;
#endregion // SpeechType
#endregion // ViewModelProperties
	}
}
