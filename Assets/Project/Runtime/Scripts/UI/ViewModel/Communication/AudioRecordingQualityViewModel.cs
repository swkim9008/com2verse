/*===============================================================
 * Product:		Com2Verse
 * File Name:	AudioRecordingQualityViewModel.cs
 * Developer:	haminjeong
 * Date:		2023-08-18 10:56
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Com2Verse.Communication;
using Com2Verse.Communication.Unity;
using Com2Verse.Data;
using Com2Verse.Option;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class AudioRecordingQualityViewModel : ViewModelBase
	{
		private          int          _useVoiceRecordingQuality;
		private readonly List<string> _qualityNameList = new();

		public AudioRecordingQualityViewModel()
		{
			var option = OptionController.Instance.GetOption<DeviceOption>();
			if (option != null)
			{
				UseVoiceRecordingQuality = option.UseVoiceRecordingQuality-1;
			}

			UpdateQualityNames();
			InitVariables();
		}

		public override void OnLanguageChanged()
		{
			UpdateQualityNames();
		}

		private void UpdateQualityNames()
		{
			_qualityNameList.Clear();
			_qualityNameList.AddRange(new[] { Localization.Instance.GetString("SoundQualityType_HighQuality"), Localization.Instance.GetString("SoundQualityType_LowQuality") });
			InvokePropertyValueChanged(nameof(QualityNames), _qualityNameList);
		}

		public void InitVariables()
		{
			InvokePropertyValueChanged(nameof(IsVoiceQualityVisible), IsVoiceQualityVisible);
		}

#region ViewModelProperties
		public bool IsVoiceQualityVisible => AuditoriumController.Instance.CurrentMicTrigger != null;
		
		public List<string> QualityNames => _qualityNameList;

		public int UseVoiceRecordingQuality
		{
			get => _useVoiceRecordingQuality;
			set
			{
				var realValue = value + 1; // 실제 enum 값
				var prevValue = _useVoiceRecordingQuality;
				if (prevValue == value)
					return;

				SetProperty(ref _useVoiceRecordingQuality, value); // dropdown에 쓰이는 index 값

				var option = OptionController.Instance.GetOption<DeviceOption>();
				if (option != null)
				{
					option.UseVoiceRecordingQuality = realValue;
					option.SaveData();
				}

				if (IsVoiceQualityVisible && realValue == (int)eSoundQualityType.HIGH_QUALITY)
				{
					AuditoriumController.Instance.IsHighQuality = true;
					ModuleManager.Instance.VoiceSettings.ChangeSettings(frequency: 44100);
					ModuleManager.Instance.VoicePublishSettings.ChangeSettings(384000);
				}
				else
				{
					AuditoriumController.Instance.IsHighQuality = false;
					ModuleManager.Instance.VoiceSettings.ChangeSettings(frequency: 16000);
					ModuleManager.Instance.VoicePublishSettings.ChangeSettings(20000);
				}
			}
		}
#endregion // ViewModelProperties
	}
}
