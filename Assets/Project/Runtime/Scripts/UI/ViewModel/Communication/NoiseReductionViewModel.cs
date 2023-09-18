/*===============================================================
 * Product:		Com2Verse
 * File Name:	NoiseReductionViewModel.cs
 * Developer:	urun4m0r1
 * Date:		2023-06-28 10:56
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Communication;
using Com2Verse.Communication.Unity;
using Com2Verse.Data;
using Com2Verse.Option;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class NoiseReductionViewModel : ViewModelBase
	{
		[UsedImplicitly] public CommandHandler<bool> SetVoiceNoiseReduction    { get; }
		[UsedImplicitly] public CommandHandler       ToggleVoiceNoiseReduction { get; }

		private bool _useVoiceNoiseReduction;
		private bool _isInteractable = true;

		public NoiseReductionViewModel()
		{
			var option = OptionController.Instance.GetOption<DeviceOption>();
			if (option != null)
			{
				UseVoiceNoiseReduction = option.UseVoiceNoiseReduction;
			}

			SetVoiceNoiseReduction    = new CommandHandler<bool>(value => UseVoiceNoiseReduction =  value);
			ToggleVoiceNoiseReduction = new CommandHandler(() => UseVoiceNoiseReduction          ^= true);

			ModuleManager.Instance.VoiceSettings.SettingsChanged += OnVoiceSettingsChanged;
		}

		private void OnVoiceSettingsChanged(IReadOnlyAudioSettings _)
		{
			UpdateVoiceNoiseReductionState();
		}

		private void UpdateVoiceNoiseReductionState()
		{
			if (ModuleManager.InstanceExists)
				ModuleManager.Instance.VoiceNoiseReductionPipeline.IsRunning = UseVoiceNoiseReduction;
			var audioRecordingQuality = ViewModelManager.InstanceOrNull?.Get<AudioRecordingQualityViewModel>();
			if (audioRecordingQuality is not { IsVoiceQualityVisible: true })
				IsUIInteractable = true;
			else
				IsUIInteractable = (audioRecordingQuality.UseVoiceRecordingQuality+1) == (int)eSoundQualityType.LOW_QUALITY;
		}

#region ViewModelProperties
		public bool UseVoiceNoiseReduction
		{
			get => _useVoiceNoiseReduction;
			set
			{
				var prevValue = _useVoiceNoiseReduction;
				if (prevValue == value)
					return;

				SetProperty(ref _useVoiceNoiseReduction, value);
				UpdateVoiceNoiseReductionState();

				var option = OptionController.Instance.GetOption<DeviceOption>();
				if (option != null)
				{
					option.UseVoiceNoiseReduction = value;
					option.SaveData();
				}
			}
		}

		public bool IsUIInteractable
		{
			get => _isInteractable;
			set
			{
				_isInteractable = value;
				InvokePropertyValueChanged(nameof(IsUIInteractable), IsUIInteractable);
			}
		}
#endregion // ViewModelProperties
	}
}
