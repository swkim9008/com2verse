/*===============================================================
* Product:		Com2Verse
* File Name:	MetaverseOptionViewModel_Volume.cs
* Developer:	tlghks1009
* Date:			2022-10-05 14:51
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Com2Verse.Option;
using Com2Verse.SoundSystem;
using Cysharp.Text;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	public partial class MetaverseOptionViewModel
	{
		// Key 값이 바뀔때 Value 값이 바뀌는 의존성 맵
		private readonly Dictionary<string, string> _propertyDependenciesMap = new()
		{
			[nameof(MasterGroupLevel)]      = nameof(IsMasterGroupMute),
			[nameof(BgmGroupLevel)]         = nameof(IsBgmGroupMute),
			[nameof(SfxGroupLevel)]         = nameof(IsSfxGroupMute),
			[nameof(AmbienceGroupLevel)]    = nameof(IsAmbienceGroupMute),
			[nameof(SystemVoiceGroupLevel)] = nameof(IsSystemVoiceGroupMute),
			[nameof(VoiceGroupLevel)]       = nameof(IsVoiceGroupMute),

			[nameof(MasterLevel)]      = nameof(IsMasterMute),
			[nameof(BgmLevel)]         = nameof(IsBgmMute),
			[nameof(SfxLevel)]         = nameof(IsSfxMute),
			[nameof(AmbienceLevel)]    = nameof(IsAmbienceMute),
			[nameof(UiLevel)]          = nameof(IsUiMute),
			[nameof(AudioRecordLevel)] = nameof(IsAudioRecordMute),
			[nameof(VideoLevel)]       = nameof(IsVideoMute),
			[nameof(EtcLevel)]         = nameof(IsEtcMute),
			[nameof(LocalVoiceLevel)]  = nameof(IsLocalVoiceMute),
			[nameof(RemoteVoiceLevel)] = nameof(IsRemoteVoiceMute),
			[nameof(SystemVoiceLevel)] = nameof(IsSystemVoiceMute),
			[nameof(ImportantLevel)]   = nameof(IsImportantMute),
		};

		[UsedImplicitly] public CommandHandler ResetVolumeSettingsCommand { get; }

		private VolumeOption? _volumeOption;

        private bool _isVoiceVideoOptionOn;
        public bool IsVoiceVideoOptionOn
        {
            get => _isVoiceVideoOptionOn;
            set
            {
                _isVoiceVideoOptionOn = value;
                InvokePropertyValueChanged(nameof(IsVoiceVideoOptionOn), IsVoiceVideoOptionOn);
            }
        }


        private void InitializeVolumeOption()
		{
            _isVoiceVideoOptionOn = false;
            _volumeOption = OptionController.Instance.GetOption<VolumeOption>();

			if (_volumeOption != null)
				RefreshVolumeSettingsProperties();
		}

		public float GetLevel(eAudioMixerGroup group)
		{
			return _volumeOption?.GetLevel(group) ?? GetDefaultLevel(group);
		}

		public bool GetIsMute(eAudioMixerGroup group)
		{
			return _volumeOption?.GetIsMute(group) ?? GetDefaultMuteState(group);
		}

		private float GetDefaultLevel(eAudioMixerGroup audioMixerGroup)
		{
			return audioMixerGroup is eAudioMixerGroup.MASTER ? 1f : 0.5f;
		}

		private bool GetDefaultMuteState(eAudioMixerGroup audioMixerGroup)
		{
			return false;
		}

		[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		public void SetLevel(eAudioMixerGroup group, float value, [CallerMemberName] string? propertyName = null)
		{
			var prevValue = GetLevel(group);
			if (prevValue == value)
				return;

			_volumeOption?.SetLevel(group, value);
			InvokePropertyValueChanged(propertyName!, GetLevel(group));

			if (value > 0f)
				SetIsMute(group, false, _propertyDependenciesMap[propertyName!]);
		}

		public void SetIsMute(eAudioMixerGroup group, bool value, [CallerMemberName] string? propertyName = null)
		{
			_volumeOption?.SetIsMute(group, value);
			InvokePropertyValueChanged(propertyName!, GetIsMute(group));
		}

		private void ResetVolumeSettings()
		{
			_volumeOption?.Reset();
			RefreshVolumeSettingsProperties();
		}

		private void RefreshVolumeSettingsProperties()
		{
			InvokePropertyValueChanged(nameof(MasterLevel),      MasterLevel);
			InvokePropertyValueChanged(nameof(BgmLevel),         BgmLevel);
			InvokePropertyValueChanged(nameof(SfxLevel),         SfxLevel);
			InvokePropertyValueChanged(nameof(AmbienceLevel),    AmbienceLevel);
			InvokePropertyValueChanged(nameof(UiLevel),          UiLevel);
			InvokePropertyValueChanged(nameof(AudioRecordLevel), AudioRecordLevel);
			InvokePropertyValueChanged(nameof(VideoLevel),       VideoLevel);
			InvokePropertyValueChanged(nameof(EtcLevel),         EtcLevel);
			InvokePropertyValueChanged(nameof(LocalVoiceLevel),  LocalVoiceLevel);
			InvokePropertyValueChanged(nameof(RemoteVoiceLevel), RemoteVoiceLevel);
			InvokePropertyValueChanged(nameof(SystemVoiceLevel), SystemVoiceLevel);
			InvokePropertyValueChanged(nameof(ImportantLevel),   ImportantLevel);

			InvokePropertyValueChanged(nameof(IsMasterMute),      IsMasterMute);
			InvokePropertyValueChanged(nameof(IsBgmMute),         IsBgmMute);
			InvokePropertyValueChanged(nameof(IsSfxMute),         IsSfxMute);
			InvokePropertyValueChanged(nameof(IsAmbienceMute),    IsAmbienceMute);
			InvokePropertyValueChanged(nameof(IsUiMute),          IsUiMute);
			InvokePropertyValueChanged(nameof(IsAudioRecordMute), IsAudioRecordMute);
			InvokePropertyValueChanged(nameof(IsVideoMute),       IsVideoMute);
			InvokePropertyValueChanged(nameof(IsEtcMute),         IsEtcMute);
			InvokePropertyValueChanged(nameof(IsLocalVoiceMute),  IsLocalVoiceMute);
			InvokePropertyValueChanged(nameof(IsRemoteVoiceMute), IsRemoteVoiceMute);
			InvokePropertyValueChanged(nameof(IsSystemVoiceMute), IsSystemVoiceMute);
			InvokePropertyValueChanged(nameof(IsImportantMute),   IsImportantMute);

			InvokePropertyValueChanged(nameof(MasterGroupLevel),      MasterGroupLevel);
			InvokePropertyValueChanged(nameof(BgmGroupLevel),         BgmGroupLevel);
			InvokePropertyValueChanged(nameof(SfxGroupLevel),         SfxGroupLevel);
			InvokePropertyValueChanged(nameof(AmbienceGroupLevel),    AmbienceGroupLevel);
			InvokePropertyValueChanged(nameof(SystemVoiceGroupLevel), SystemVoiceGroupLevel);
			InvokePropertyValueChanged(nameof(VoiceGroupLevel),       VoiceGroupLevel);

			InvokePropertyValueChanged(nameof(IsMasterGroupMute),      IsMasterGroupMute);
			InvokePropertyValueChanged(nameof(IsBgmGroupMute),         IsBgmGroupMute);
			InvokePropertyValueChanged(nameof(IsSfxGroupMute),         IsSfxGroupMute);
			InvokePropertyValueChanged(nameof(IsAmbienceGroupMute),    IsAmbienceGroupMute);
			InvokePropertyValueChanged(nameof(IsSystemVoiceGroupMute), IsSystemVoiceGroupMute);
			InvokePropertyValueChanged(nameof(IsVoiceGroupMute),       IsVoiceGroupMute);

			InvokePropertyValueChanged(nameof(MasterGroupLevelPercentText),      MasterGroupLevelPercentText);
			InvokePropertyValueChanged(nameof(BgmGroupLevelPercentText),         BgmGroupLevelPercentText);
			InvokePropertyValueChanged(nameof(SfxGroupLevelPercentText),         SfxGroupLevelPercentText);
			InvokePropertyValueChanged(nameof(AmbienceGroupLevelPercentText),    AmbienceGroupLevelPercentText);
			InvokePropertyValueChanged(nameof(SystemVoiceGroupLevelPercentText), SystemVoiceGroupLevelPercentText);
			InvokePropertyValueChanged(nameof(VoiceGroupLevelPercentText),       VoiceGroupLevelPercentText);
		}

#region ViewModelProperties - LevelGroup
		[UsedImplicitly]
		public float MasterGroupLevel

		{
			get => MasterLevel;
			set
			{
				MasterLevel = value;
				InvokePropertyValueChanged(nameof(MasterGroupLevel),  value);
				InvokePropertyValueChanged(nameof(IsMasterGroupMute), IsMasterGroupMute);

				InvokePropertyValueChanged(nameof(MasterGroupLevelPercentText), MasterGroupLevelPercentText);
			}
		}

		[UsedImplicitly] public float BgmGroupLevel
		{
			get => BgmLevel;
			set
			{
				BgmLevel = value;
				InvokePropertyValueChanged(nameof(BgmGroupLevel),  value);
				InvokePropertyValueChanged(nameof(IsBgmGroupMute), IsBgmGroupMute);

				InvokePropertyValueChanged(nameof(BgmGroupLevelPercentText), BgmGroupLevelPercentText);
			}
		}

		[UsedImplicitly] public float SfxGroupLevel
		{
			get => SfxLevel;
			set
			{
				SfxLevel = value;
				InvokePropertyValueChanged(nameof(SfxGroupLevel),  value);
				InvokePropertyValueChanged(nameof(IsSfxGroupMute), IsSfxGroupMute);

				InvokePropertyValueChanged(nameof(SfxGroupLevelPercentText), SfxGroupLevelPercentText);
			}
		}

		[UsedImplicitly] public float AmbienceGroupLevel
		{
			get => AmbienceLevel;
			set
			{
				AmbienceLevel = value;
				InvokePropertyValueChanged(nameof(AmbienceGroupLevel),  value);
				InvokePropertyValueChanged(nameof(IsAmbienceGroupMute), IsAmbienceGroupMute);

				InvokePropertyValueChanged(nameof(AmbienceGroupLevelPercentText), AmbienceGroupLevelPercentText);
			}
		}

		[UsedImplicitly] public float SystemVoiceGroupLevel
		{
			get => SystemVoiceLevel;
			set
			{
				SystemVoiceLevel = value;
				InvokePropertyValueChanged(nameof(SystemVoiceGroupLevel),  value);
				InvokePropertyValueChanged(nameof(IsSystemVoiceGroupMute), IsSystemVoiceGroupMute);

				InvokePropertyValueChanged(nameof(SystemVoiceGroupLevelPercentText), SystemVoiceGroupLevelPercentText);
			}
		}

		[UsedImplicitly] public float VoiceGroupLevel
		{
			get => RemoteVoiceLevel;
			set
			{
				RemoteVoiceLevel = value;
				InvokePropertyValueChanged(nameof(VoiceGroupLevel),  value);
				InvokePropertyValueChanged(nameof(IsVoiceGroupMute), IsVoiceGroupMute);

				InvokePropertyValueChanged(nameof(VoiceGroupLevelPercentText), VoiceGroupLevelPercentText);
			}
		}

		[UsedImplicitly] public string MasterGroupLevelPercentText      => ZString.Format("{0:0}%", MasterGroupLevel      * 100f);
		[UsedImplicitly] public string BgmGroupLevelPercentText         => ZString.Format("{0:0}%", BgmGroupLevel         * 100f);
		[UsedImplicitly] public string SfxGroupLevelPercentText         => ZString.Format("{0:0}%", SfxGroupLevel         * 100f);
		[UsedImplicitly] public string AmbienceGroupLevelPercentText    => ZString.Format("{0:0}%", AmbienceGroupLevel    * 100f);
		[UsedImplicitly] public string SystemVoiceGroupLevelPercentText => ZString.Format("{0:0}%", SystemVoiceGroupLevel * 100f);
		[UsedImplicitly] public string VoiceGroupLevelPercentText       => ZString.Format("{0:0}%", VoiceGroupLevel       * 100f);
#endregion // ViewModelProperties - LevelGroup

#region ViewModelProperties - IsMuteGroup
		[UsedImplicitly] public bool IsMasterGroupMute
		{
			get => IsMasterMute;
			set
			{
				IsMasterMute = value;
				InvokePropertyValueChanged(nameof(IsMasterGroupMute), value);
			}
		}

		[UsedImplicitly] public bool IsBgmGroupMute
		{
			get => IsBgmMute;
			set
			{
				IsBgmMute = value;
				InvokePropertyValueChanged(nameof(IsBgmGroupMute), value);
			}
		}

		[UsedImplicitly] public bool IsSfxGroupMute
		{
			get => IsSfxMute;
			set
			{
				IsSfxMute = value;
				InvokePropertyValueChanged(nameof(IsSfxGroupMute), value);
			}
		}

		[UsedImplicitly] public bool IsAmbienceGroupMute
		{
			get => IsAmbienceMute;
			set
			{
				IsAmbienceMute = value;
				InvokePropertyValueChanged(nameof(IsAmbienceGroupMute), value);
			}
		}

		[UsedImplicitly] public bool IsSystemVoiceGroupMute
		{
			get => IsSystemVoiceMute;
			set
			{
				IsSystemVoiceMute = value;
				InvokePropertyValueChanged(nameof(IsSystemVoiceGroupMute), value);
			}
		}

		[UsedImplicitly] public bool IsVoiceGroupMute
		{
			get => IsRemoteVoiceMute;
			set
			{
				IsRemoteVoiceMute = value;
				InvokePropertyValueChanged(nameof(IsVoiceGroupMute), value);
			}
		}
#endregion // ViewModelProperties - IsMuteGroup

#region ViewModelProperties - Level
		[UsedImplicitly] public float MasterLevel
		{
			get => GetLevel(eAudioMixerGroup.MASTER);
			set => SetLevel(eAudioMixerGroup.MASTER, value);
		}

		[UsedImplicitly] public float BgmLevel
		{
			get => GetLevel(eAudioMixerGroup.BGM);
			set => SetLevel(eAudioMixerGroup.BGM, value);
		}

		[UsedImplicitly] public float SfxLevel
		{
			get => GetLevel(eAudioMixerGroup.SFX);
			set => SetLevel(eAudioMixerGroup.SFX, value);
		}

		[UsedImplicitly] public float AmbienceLevel
		{
			get => GetLevel(eAudioMixerGroup.AMBIENCE);
			set => SetLevel(eAudioMixerGroup.AMBIENCE, value);
		}

		[UsedImplicitly] public float UiLevel
		{
			get => GetLevel(eAudioMixerGroup.UI);
			set => SetLevel(eAudioMixerGroup.UI, value);
		}

		[UsedImplicitly] public float AudioRecordLevel
		{
			get => GetLevel(eAudioMixerGroup.AUDIORECORD);
			set => SetLevel(eAudioMixerGroup.AUDIORECORD, value);
		}

		[UsedImplicitly] public float VideoLevel
		{
			get => GetLevel(eAudioMixerGroup.VIDEO);
			set => SetLevel(eAudioMixerGroup.VIDEO, value);
		}

		[UsedImplicitly] public float EtcLevel
		{
			get => GetLevel(eAudioMixerGroup.ETC);
			set => SetLevel(eAudioMixerGroup.ETC, value);
		}

		[UsedImplicitly] public float LocalVoiceLevel
		{
			get => GetLevel(eAudioMixerGroup.LOCAL_VOICE);
			set => SetLevel(eAudioMixerGroup.LOCAL_VOICE, value);
		}

		[UsedImplicitly] public float RemoteVoiceLevel
		{
			get => GetLevel(eAudioMixerGroup.REMOTE_VOICE);
			set => SetLevel(eAudioMixerGroup.REMOTE_VOICE, value);
		}

		[UsedImplicitly] public float SystemVoiceLevel
		{
			get => GetLevel(eAudioMixerGroup.SYSTEM_VOICE);
			set => SetLevel(eAudioMixerGroup.SYSTEM_VOICE, value);
		}

		[UsedImplicitly] public float ImportantLevel
		{
			get => GetLevel(eAudioMixerGroup.IMPORTANT);
			set => SetLevel(eAudioMixerGroup.IMPORTANT, value);
		}
#endregion // ViewModelProperties - Level

#region ViewModelProperties - IsMute
		[UsedImplicitly] public bool IsMasterMute
		{
			get => GetIsMute(eAudioMixerGroup.MASTER);
			set => SetIsMute(eAudioMixerGroup.MASTER, value);
		}

		[UsedImplicitly] public bool IsBgmMute
		{
			get => GetIsMute(eAudioMixerGroup.BGM);
			set => SetIsMute(eAudioMixerGroup.BGM, value);
		}

		[UsedImplicitly] public bool IsSfxMute
		{
			get => GetIsMute(eAudioMixerGroup.SFX);
			set => SetIsMute(eAudioMixerGroup.SFX, value);
		}

		[UsedImplicitly] public bool IsAmbienceMute
		{
			get => GetIsMute(eAudioMixerGroup.AMBIENCE);
			set => SetIsMute(eAudioMixerGroup.AMBIENCE, value);
		}

		[UsedImplicitly] public bool IsUiMute
		{
			get => GetIsMute(eAudioMixerGroup.UI);
			set => SetIsMute(eAudioMixerGroup.UI, value);
		}

		[UsedImplicitly] public bool IsAudioRecordMute
		{
			get => GetIsMute(eAudioMixerGroup.AUDIORECORD);
			set => SetIsMute(eAudioMixerGroup.AUDIORECORD, value);
		}

		[UsedImplicitly] public bool IsVideoMute
		{
			get => GetIsMute(eAudioMixerGroup.VIDEO);
			set => SetIsMute(eAudioMixerGroup.VIDEO, value);
		}

		[UsedImplicitly] public bool IsEtcMute
		{
			get => GetIsMute(eAudioMixerGroup.ETC);
			set => SetIsMute(eAudioMixerGroup.ETC, value);
		}

		[UsedImplicitly] public bool IsLocalVoiceMute
		{
			get => GetIsMute(eAudioMixerGroup.LOCAL_VOICE);
			set => SetIsMute(eAudioMixerGroup.LOCAL_VOICE, value);
		}

		[UsedImplicitly] public bool IsRemoteVoiceMute
		{
			get => GetIsMute(eAudioMixerGroup.REMOTE_VOICE);
			set => SetIsMute(eAudioMixerGroup.REMOTE_VOICE, value);
		}

		[UsedImplicitly] public bool IsSystemVoiceMute
		{
			get => GetIsMute(eAudioMixerGroup.SYSTEM_VOICE);
			set => SetIsMute(eAudioMixerGroup.SYSTEM_VOICE, value);
		}

		[UsedImplicitly] public bool IsImportantMute
		{
			get => GetIsMute(eAudioMixerGroup.IMPORTANT);
			set => SetIsMute(eAudioMixerGroup.IMPORTANT, value);
		}
#endregion // ViewModelProperties - IsMute
	}
}
