/*===============================================================
 * Product:		Com2Verse
 * File Name:	SoundManager.cs
 * Developer:	yangsehoon
 * Date:		2023-01-06 오전 10:17
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Linq;
using Com2Verse.AssetSystem;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.Option;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

namespace Com2Verse.SoundSystem
{
	public static class SoundManager
	{
		private static readonly string MainMixerAddressableName = "Com2verseMainMixer.mixer";

		public static eSoundIndex CurrentBGM = eSoundIndex.NONE;
		public static event Sound.SoundManager.Events.VolumeChanged OnVolumeChanged = (_, _) => { };
		public static void Initialize()
		{
			var handle = C2VAddressables.LoadAssetAsync<AudioMixer>((MainMixerAddressableName));

			handle.OnCompleted += (opHandle) =>
			{
				var mainMixer       = opHandle.Result;
				var soundManager    = Sound.SoundManager.Instance;
				int audioGroupCount = Enum.GetValues(typeof(eAudioMixerGroup)).Cast<int>().Max();
				soundManager.Initialize(mainMixer, audioGroupCount, (int)eAudioMixerGroup.UI, (int)eAudioMixerGroup.BGM, (int)eAudioMixerGroup.VIDEO);
				soundManager.SetOnVolumeChanged(VolumeChanged);

				OptionController.Instance.OptionDataLoadOnSplashFinished += OnOptionDataLoadFinished;
				ApplyCurrentVolumeOption();
			};

			void VolumeChanged(int mixerGroup, float volume)
			{
				var events = OnVolumeChanged.GetInvocationList();
				foreach (var evt in events)
				{
					if (evt is Sound.SoundManager.Events.VolumeChanged vcEvt)
						vcEvt.Invoke(mixerGroup, volume);
				}
			}
		}

		public static void SnapshotTransition(eAudioSnapshot to, float duration)
		{
			if (AudioSnapshotInfo.SnapshotMap.TryGetValue((int)to, out string snapshot))
			{
				Sound.SoundManager.Instance.SnapshotTransition(snapshot, duration);
			}
			else
			{
				C2VDebug.LogError($"Cannot find snapshot {to}");
			}
		}

		private static void OnOptionDataLoadFinished(OptionController _)
		{
			ApplyCurrentVolumeOption();
		}

		private static void ApplyCurrentVolumeOption()
		{
			OptionController.Instance.GetOption<VolumeOption>()?.Apply();
		}

		public static void PlayUISound(eSoundIndex index, float volumeScale = 1.0f)
		{
			Sound.SoundManager.Instance.PlayUISound(SoundManagerSetting.GetSoundClip(index), volumeScale);
		}

		public static void PlayBGM(eSoundIndex index, float fadeDuration, float fadeInDelay, float targetVolume = Sound.SoundManager.BGMDefaultVolume)
		{
			CurrentBGM = index;

			AssetReference reference = SoundManagerSetting.GetSoundClip(index);
			Sound.SoundManager.Instance.PlayBGM(reference, fadeDuration, fadeInDelay, targetVolume);
		}

		public static async void Play(AudioSource source, eSoundIndex index, ulong delay = 0)
		{
			AssetReference reference = SoundManagerSetting.GetSoundClip(index);
			await Sound.SoundManager.Instance.Play(source, reference, delay);
		}

		public static void PlayBgm([NotNull] SceneProperty sceneProperty)
		{
			var bgm = GetBgm(sceneProperty);
			if (bgm is eSoundIndex.NONE)
			{
				PlayBGM(bgm, 2, 0, 0);
				return;
			}

			PlayBGM(bgm, 4, 3);
		}

		public static eSoundIndex GetBgm([NotNull] SceneProperty sceneProperty) => sceneProperty.ServiceType switch
		{
			eServiceType.WORLD  => eSoundIndex.BGM__WORLD__PLAZA,
			eServiceType.OFFICE => GetOfficeBgm(sceneProperty),
			eServiceType.MICE   => GetMiceBgm(sceneProperty), //eSoundIndex.BGM__WORLD,
			_                   => eSoundIndex.NONE,
		};

		public static eSoundIndex GetOfficeBgm([NotNull] SceneProperty sceneProperty) => sceneProperty.SpaceTemplate?.SpaceCode switch
		{
			eSpaceCode.MEETING => eSoundIndex.NONE,
			eSpaceCode.LOUNGE => eSoundIndex.BGM__LOUNGE,
			_                  => eSoundIndex.BGM__OFFICELOBBY,
		};

        public static eSoundIndex GetMiceBgm([NotNull] SceneProperty sceneProperty) => sceneProperty.SpaceTemplate?.SpaceCode switch
        {
            eSpaceCode.MICE_CONFERENCE_HALL => eSoundIndex.NONE,
            _ => eSoundIndex.BGM__WORLD,
        };
    }
}
