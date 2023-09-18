/*===============================================================
 * Product:		Com2Verse
 * File Name:	SoundManagerEditor.cs
 * Developer:	yangsehoon
 * Date:		2023-01-06 오전 11:11
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.SoundSystem;
using UnityEngine;
using UnityEngine.Audio;

namespace Com2VerseEditor.SoundSystem
{
	public class SoundManagerEditor : Com2VerseEditor.Sound.SoundManagerEditor
	{
		public static eAudioMixerGroup GetMixerGroupIndex(AudioMixerGroup group)
		{
			if (group != null)
			{
				string groupName = group.name.Split("_")[0];
				if (Enum.TryParse<eAudioMixerGroup>(groupName, out eAudioMixerGroup enumValue))
				{
					return enumValue;
				}
			}

			return eAudioMixerGroup.MASTER;
		}

		public static AudioMixerGroup GetMixerGroup(eAudioMixerGroup selectedMixerGroup)
		{
			AudioMixer[] audioMixers = Resources.FindObjectsOfTypeAll<AudioMixer>();
			foreach (AudioMixer mixer in audioMixers)
			{
				AudioMixerGroup[] audioMixerGroups = mixer.FindMatchingGroups(string.Empty);
				foreach (AudioMixerGroup group in audioMixerGroups)
				{
					if (!group.name.StartsWith('*'))
					{
						if (group.name.Split("_")[0].Equals(selectedMixerGroup.ToString()))
						{
							return group;
						}
					}
				}
			}

			return null;
		}
	}
}
