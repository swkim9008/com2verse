/*===============================================================
* Product:		Com2Verse
* File Name:	AnimationSoundController.cs
* Developer:	eugene9721
* Date:			2023-06-29 13:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;
using Com2Verse.AvatarAnimation;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Sound;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Project.Animation
{
	public sealed class AnimationSoundController
	{
		public enum eSoundType
		{
			WALK,
			RUN,
			JUMP,
			LAND_LOW,
			LAND_HIGH,
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			// TODO: 테이블 데이터 적용
			SoundOtherAvatarVolume = 0.7f;
			SoundOtherAvatarCount  = 10;

			WalkSoundVolume = 0.2f;
			RunSoundVolume  = 0.3f;
			JumpSoundVolume = 0.3f;
			LandSoundVolume = 0.3f;
		}

		public static float SoundOtherAvatarVolume = 0.7f;
		public static int   SoundOtherAvatarCount  = 10;

		public static float WalkSoundVolume = 0.2f;
		public static float RunSoundVolume  = 0.3f;
		public static float JumpSoundVolume = 0.3f;
		public static float LandSoundVolume = 0.3f;

		// TODO: 테이블 데이터로 관리?
		private const int WalkSoundLength = 6;
		private const int RunSoundLength  = 6;
		private const int JumpSoundLength = 2;
		private const int LandSoundLength = 2;

		private const float WalkSoundDelay = 0.1f;
		private const float SoundDelay     = 0.02f;

		private bool _isInitialized;

		private float _dampeningFromDistance = 1f;

		private float _walkSoundCooldown;
		private float _soundCooldown;

		private AvatarAnimatorController? _owner;
		private MetaverseAudioSource? _animationAudioSource;
		private MetaverseAudioSource? _gestureAudioSource;

		private eAvatarType _avatarType;

		public void Initialize(AvatarAnimatorController owner, eAvatarType avatarType)
		{
			_isInitialized = true;
			_owner         = owner;
			_avatarType    = avatarType;

			_dampeningFromDistance = 1f;
		}

		public void Clear()
		{
			_owner         = null;
			_isInitialized = false;

			if (!_animationAudioSource.IsUnityNull())
				_animationAudioSource!.Stop();
			if (!_gestureAudioSource.IsUnityNull())
				_gestureAudioSource!.Stop();
		}

		public void OnUpdate()
		{
			_walkSoundCooldown = Mathf.Max(_walkSoundCooldown - Time.deltaTime, 0f);
			_soundCooldown     = Mathf.Max(_soundCooldown     - Time.deltaTime, 0f);
		}

		public void SetDampeningFromDistance(float dampeningFromDistance, int ranking)
		{
			if (ranking <= SoundOtherAvatarCount)
				_dampeningFromDistance = dampeningFromDistance;
			else
				_dampeningFromDistance = 0;
		}

		public void PlaySound(eSoundType soundType)
		{
			if (!_isInitialized || _soundCooldown > 0f || _owner == null)
				return;

			var soundVolume = _owner.IsMine ? 1f : SoundOtherAvatarVolume * _dampeningFromDistance;

			int    randomKey;
			string assetKey;

			switch (soundType)
			{
				case eSoundType.WALK:
					if (_walkSoundCooldown > 0f)
						return;

					randomKey = Random.Range(1, WalkSoundLength + 1);
					assetKey  = $"SE_{_avatarType.ToString()}_WalkA_{randomKey:D2}.wav";
					PlaySound(assetKey, WalkSoundVolume * soundVolume);
					break;
				case eSoundType.RUN:
					if (_walkSoundCooldown > 0f)
						return;

					randomKey = Random.Range(1, RunSoundLength + 1);
					assetKey  = $"SE_{_avatarType.ToString()}_RunA_{randomKey:D2}.wav";
					PlaySound(assetKey, RunSoundVolume * soundVolume);
					break;
				case eSoundType.JUMP:
					randomKey = Random.Range(1, JumpSoundLength + 1);
					assetKey  = $"SE_{_avatarType.ToString()}_JumpA_{randomKey:D2}.wav";
					PlaySound(assetKey, JumpSoundVolume * soundVolume);
					break;
				case eSoundType.LAND_LOW:
					randomKey = Random.Range(1, LandSoundLength + 1);
					assetKey = $"SE_{_avatarType.ToString()}_LandLowA_{randomKey:D2}.wav";
					PlaySound(assetKey, LandSoundVolume * soundVolume);
					break;
				case eSoundType.LAND_HIGH:
					assetKey = $"SE_{_avatarType.ToString()}_LandHighA.wav";
					PlaySound(assetKey, LandSoundVolume * soundVolume);
					break;
			}

			_walkSoundCooldown = WalkSoundDelay;
			_soundCooldown     = SoundDelay;
		}

		public void PlaySound(string assetKey)
		{
			PlaySound(assetKey, SoundManager.UIDefaultVolume * _dampeningFromDistance);
		}

		public void PlaySound(string assetKey, float volume)
		{
			_gestureAudioSource.Stop();

			if (_animationAudioSource.IsUnityNull() || _animationAudioSource!.AudioSource.IsUnityNull())
			{
				C2VDebug.LogWarningCategory(GetType().Name, "AudioSource is null");
				return;
			}

			// 조작과 관련된 애니메이션 사운드 클립의 경우 AnimationManager에 클립을 캐싱하여 사용
			//AnimationManager.Instance.PlayAnimationSound(_animationAudioSource!.AudioSource!, assetKey, volume).Forget();
			SoundManager.Instance.PlayOneShot(_animationAudioSource!.AudioSource!, assetKey, volume);
		}

		public void StopGestureAudio()
		{
			if (!_gestureAudioSource.IsUnityNull())
				_gestureAudioSource!.Stop();
		}

		public async void PlayGesture(string assetKey)
		{
			_gestureAudioSource.Stop();
			var gestureClip = await SoundManager.Instance.GetClip(assetKey);
			_gestureAudioSource.SetClip(gestureClip);

			_gestureAudioSource.Volume = SoundManager.UIDefaultVolume * _dampeningFromDistance;
			_gestureAudioSource.Play();
		}

		public void InitializeAudioComponent(int targetMixerGroup, GameObject gameObject)
		{
			if (_animationAudioSource.IsUnityNull())
			{
				_animationAudioSource = MetaverseAudioSource.CreateNew(gameObject);
				_animationAudioSource!.TargetMixerGroup = targetMixerGroup;
			}

			if (_gestureAudioSource.IsUnityNull())
			{
				_gestureAudioSource = MetaverseAudioSource.CreateNew(gameObject);
				_gestureAudioSource!.TargetMixerGroup = targetMixerGroup;
				_gestureAudioSource.Loop = false;
			}
		}
	}
}
