/*===============================================================
* Product:		Com2Verse
* File Name:	DeviceOption.cs
* Developer:	tlghks1009
* Date:			2022-10-05 14:51
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Communication.Unity;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.UI;
using UnityEngine;

namespace Com2Verse.Option
{
	[Serializable] [MetaverseOption("DeviceOption")]
	public sealed class DeviceOption : BaseMetaverseOption, IDisposable
	{
		[field: SerializeField] public float VoiceLevel                  { get; set; } = -1f;
		[field: SerializeField] public bool  UseVoiceNoiseReduction      { get; set; } = true;
		[field: SerializeField] public int   HumanMattingBackgroundIndex { get; set; } = -1;
		[field: SerializeField] public int   UseVoiceRecordingQuality    { get; set; } = 2;

		[field: SerializeField] private bool voiceNoiseReductionCheck   = false;
		[field: SerializeField] private bool voiceRecordingQualityCheck = false;

		public DeviceOption()
		{
			ModuleManager.Instance.Voice.LevelChanged += OnVoiceLevelChanged;
		}

		public void Dispose()
		{
			var moduleManager = ModuleManager.InstanceOrNull;
			if (moduleManager != null)
				moduleManager.Voice.LevelChanged -= OnVoiceLevelChanged;
		}

		private void OnVoiceLevelChanged(float level)
		{
			VoiceLevel = level;
			SaveData();
		}

		public override void OnInitialize()
		{
			base.OnInitialize();

			ModuleManager.Instance.Voice.Level = VoiceLevel;
		}

		public override void SetTableOption()
		{
			if (VoiceLevel < 0)
			{
				VoiceLevel = Convert.ToInt32(TargetTableData[eSetting.MIC_VOLUME].Default) / 100f;
			}

			if (!voiceNoiseReductionCheck)
			{
				C2VDebug.LogCategory("OptionController", $"DeviceOption - new UseVoiceNoiseReduction");
				voiceNoiseReductionCheck = true;
				UseVoiceNoiseReduction = Convert.ToInt32(TargetTableData[eSetting.MIC_NOISECANCEL].Default) == 1;
			}

			if (!voiceRecordingQualityCheck)
			{
				C2VDebug.LogCategory("OptionController", $"DeviceOption - new UseVoiceRecordingQuality");
				voiceRecordingQualityCheck   = true;
				UseVoiceRecordingQuality = Convert.ToInt32(TargetTableData[eSetting.MIC_SOUND_QUALITY].Default);
			}

			if (HumanMattingBackgroundIndex < 0)
			{
				C2VDebug.LogCategory("OptionController", $"DeviceOption - new HumanMattingBackgroundIndex");
				HumanMattingBackgroundIndex = Convert.ToInt32(TargetTableData[eSetting.CAMERA_BACKGROUND].Default) - 1;
			}
		}

		public override void Apply()
		{
			base.Apply();

			var humanMattingViewModel = ViewModelManager.Instance.Get<HumanMattingViewModel>();
			if (humanMattingViewModel != null)
				humanMattingViewModel.SelectedBackgroundIndex = HumanMattingBackgroundIndex;

			var noiseReductionViewModel = ViewModelManager.Instance.Get<NoiseReductionViewModel>();
			if (noiseReductionViewModel != null)
				noiseReductionViewModel.UseVoiceNoiseReduction = UseVoiceNoiseReduction;

			var audioRecordingQuality = ViewModelManager.Instance.Get<AudioRecordingQualityViewModel>();
			if (audioRecordingQuality != null)
				audioRecordingQuality.UseVoiceRecordingQuality = UseVoiceRecordingQuality-1;

			var voice = ModuleManager.InstanceOrNull?.Voice;
			if (voice != null)
				voice.Level = VoiceLevel;
		}
	}
}
