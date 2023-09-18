/*===============================================================
* Product:		Com2Verse
* File Name:	VolumnOption.cs
* Developer:	tlghks1009
* Date:			2022-10-05 14:51
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.SoundSystem;
using Com2Verse.Utils;
using UnityEngine;
using SoundManager = Com2Verse.Sound.SoundManager;

namespace Com2Verse.Option
{
	[Serializable]
	public sealed class OptionVolume
	{
		[field: SerializeField] public eAudioMixerGroup AudioMixerGroup { get; set; }
		[field: SerializeField] public float            Level           { get; set; }
		[field: SerializeField] public bool             IsMute          { get; set; }
	}

	[Serializable] [MetaverseOption("VolumeOption")]
	public sealed class VolumeOption : BaseMetaverseOption
	{
		[field: SerializeField] public List<OptionVolume>? OptionVolumes { get; set; }

		public float GetLevel(eAudioMixerGroup audioMixerGroup)
		{
			return GetValueOrDefault(audioMixerGroup)?.Level ?? GetDefaultLevel(audioMixerGroup);
		}

		public bool GetIsMute(eAudioMixerGroup audioMixerGroup)
		{
			return GetValueOrDefault(audioMixerGroup)?.IsMute ?? GetDefaultMuteState(audioMixerGroup);
		}

		public void SetLevel(eAudioMixerGroup audioMixerGroup, float value)
		{
			GetOrCreateVolume(audioMixerGroup).Level = value;
			SoundManager.Instance.SetVolume((int)audioMixerGroup, value);
		}

		public void SetIsMute(eAudioMixerGroup audioMixerGroup, bool value)
		{
			GetOrCreateVolume(audioMixerGroup).IsMute = value;

			var level = value ? 0f : GetLevel(audioMixerGroup);
			SoundManager.Instance.SetVolume((int)audioMixerGroup, level);
		}

		private OptionVolume GetOrCreateVolume(eAudioMixerGroup audioMixerGroup)
		{
			OptionVolumes ??= new List<OptionVolume>();
			var volume = GetValueOrDefault(audioMixerGroup);
			if (volume == null)
			{
				volume = new OptionVolume
				{
					AudioMixerGroup = audioMixerGroup,
					Level           = GetDefaultLevel(audioMixerGroup),
					IsMute          = GetDefaultMuteState(audioMixerGroup),
				};
				OptionVolumes.Add(volume);
			}

			return volume;
		}

		private OptionVolume? GetValueOrDefault(eAudioMixerGroup audioMixerGroup)
		{
			if (OptionVolumes == null)
				return null;

			foreach (var volume in OptionVolumes)
			{
				if (volume.AudioMixerGroup == audioMixerGroup)
					return volume;
			}

			return null;
		}

		public override void Apply()
		{
			base.Apply();

			if (OptionVolumes == null || OptionVolumes.Count == 0)
			{
				foreach (var mixerGroup in EnumUtility.Foreach<eAudioMixerGroup>())
				{
					var level = GetDefaultMuteState(mixerGroup) ? 0f : GetDefaultLevel(mixerGroup);
					SoundManager.Instance.SetVolume((int)mixerGroup, level);
				}
			}
			else
			{
				foreach (var volume in OptionVolumes)
				{
					var level = volume.IsMute ? 0f : volume.Level;
					SoundManager.Instance.SetVolume((int)volume.AudioMixerGroup, level);
				}
			}
		}

		public void Reset()
		{
			if (OptionVolumes == null || OptionVolumes.Count == 0)
			{
				foreach (var mixerGroup in EnumUtility.Foreach<eAudioMixerGroup>())
				{
					var level = GetDefaultMuteState(mixerGroup) ? 0f : GetDefaultLevel(mixerGroup);
					SoundManager.Instance.SetVolume((int)mixerGroup, level);
				}
			}
			else
			{
				foreach (var volume in OptionVolumes)
				{
					var level = GetDefaultMuteState(volume.AudioMixerGroup) ? 0f : GetDefaultLevel(volume.AudioMixerGroup);
					SoundManager.Instance.SetVolume((int)volume.AudioMixerGroup, level);
				}
			}

			OptionVolumes?.Clear();
			OptionVolumes = null;

			SaveData();
		}

		private float GetDefaultLevel(eAudioMixerGroup audioMixerGroup) => audioMixerGroup switch
		{
			eAudioMixerGroup.MASTER       => Convert.ToInt32(TargetTableData[eSetting.SOUND_MASTER].Default) / 100f,
			eAudioMixerGroup.BGM          => Convert.ToInt32(TargetTableData[eSetting.SOUND_MUSIC].Default) / 100f,
			eAudioMixerGroup.SFX          => Convert.ToInt32(TargetTableData[eSetting.SOUND_EFFECT].Default) / 100f,
			eAudioMixerGroup.AMBIENCE     => Convert.ToInt32(TargetTableData[eSetting.SOUND_AMBIENCE].Default) / 100f,
			eAudioMixerGroup.SYSTEM_VOICE => Convert.ToInt32(TargetTableData[eSetting.SOUND_SYSTEM].Default) / 100f,
			eAudioMixerGroup.REMOTE_VOICE => Convert.ToInt32(TargetTableData[eSetting.SOUND_VOICECHAT].Default) / 100f,
			eAudioMixerGroup.MUTE         => 0.0f,
			eAudioMixerGroup.IMPORTANT    => 1.0f,
			_                             => 1.0f,
		};

		public override void SetTableOption()
		{
			foreach (var mixerGroup in EnumUtility.Foreach<eAudioMixerGroup>())
			{
				var level = GetDefaultMuteState(mixerGroup) ? 0f : GetDefaultLevel(mixerGroup);
				SoundManager.Instance.SetVolume((int)mixerGroup, level);
			}
		}

		private bool GetDefaultMuteState(eAudioMixerGroup audioMixerGroup)
		{
			return false;
		}
	}
}
