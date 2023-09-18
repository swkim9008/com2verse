/*===============================================================
* Product:		Com2Verse
* File Name:	AnimationManager.cs
* Developer:	eugene9721
* Date:			2023-01-09 16:37
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Threading;
using Com2Verse.AssetSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.LruObjectPool;
using Com2Verse.Sound;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Pool;

namespace Com2Verse.AvatarAnimation
{
	public sealed class AnimationManager : MonoSingleton<AnimationManager>, IDisposable
	{
		private const int DefaultTableIndex   = 1;

		private const int PoolMaxSize         = 10;
		private const int PoolDefaultCapacity = 5;

		private readonly Dictionary<string, GameObject> _fxPrefabDict = new();

		private readonly Dictionary<string, IObjectPool<GameObject>> _fxPools = new();

		private TableAvatarAnimator _tableAvatarAnimator;
		public  TableAvatarAnimator TableAvatarAnimator => _tableAvatarAnimator;

		private TableEmotion _tableEmotion;
		public  TableEmotion TableEmotion => _tableEmotion;

		private TableAvatarControl _tableAvatarControl;
		public  AvatarControl AvatarControlData => _tableAvatarControl?.Datas?[DefaultTableIndex];

		private AvatarAnimator _defaultAvatarAnimatorData;

		private AvatarMask _womanCinematicAvatarMask;
		private AvatarMask _manCinematicAvatarMask;
		private AvatarMask _upperBodyAvatarMask;

		public AvatarMask GetCinematicAvatarMask(eAvatarType avatarType) => avatarType == eAvatarType.PC01_M ? _manCinematicAvatarMask : _womanCinematicAvatarMask;
		public AvatarMask GetUpperBodyAvatarMask => _upperBodyAvatarMask;

		private bool _isEnableAnimationEvents;

		protected override void AwakeInvoked()
		{
			_defaultAvatarAnimatorData = new AvatarAnimator
			{
				ID                = 140000,
				AnimatorAssetName = "ManWorldAnimator.controller",
				AnimatorType      = eAnimatorType.WORLD,
				AvatarType        = eAvatarType.PC01_M,
			};
		}

		public void Initialize()
		{
			LoadTable();
			LoadAssetAsync().Forget();
		}

		public void Dispose()
		{
			foreach (var pools in _fxPools.Values)
				pools?.Clear();
			_fxPools.Clear();
			_fxPrefabDict.Clear();
		}

		public void EnableEvents()
		{
			_isEnableAnimationEvents = true;
		}

		public void DisableEvents()
		{
			_isEnableAnimationEvents = false;
		}

#region Load Assets
		private void LoadTable()
		{
			_tableAvatarAnimator = TableDataManager.Instance.Get<TableAvatarAnimator>();
			_tableEmotion        = TableDataManager.Instance.Get<TableEmotion>();
			_tableAvatarControl  = TableDataManager.Instance.Get<TableAvatarControl>();
		}

		private async UniTask LoadAssetAsync()
		{
			_manCinematicAvatarMask   = await C2VAddressables.LoadAssetAsync<AvatarMask>(AnimationDefine.ManCinematicAvatarMaskAssetName).ToUniTask();
			_womanCinematicAvatarMask = await C2VAddressables.LoadAssetAsync<AvatarMask>(AnimationDefine.WomanCinematicAvatarMaskAssetName).ToUniTask();
			_upperBodyAvatarMask      = await C2VAddressables.LoadAssetAsync<AvatarMask>(AnimationDefine.UpperBodyMaskAssetName).ToUniTask();
		}
#endregion Load Assets

#region Animator
		public void LoadRuntimeAnimator(int animatorId, Action<RuntimeAnimatorController> onCompleted)
		{
			if (animatorId == -1) return;
			LoadRuntimeAnimator(GetAnimatorData(animatorId).AnimatorAssetName, onCompleted);
		}

		public void LoadRuntimeAnimator(string animatorAssetName, Action<RuntimeAnimatorController> onCompleted)
		{
			if (string.IsNullOrWhiteSpace(animatorAssetName)) return;
			LoadRuntimeAnimatorAsync(animatorAssetName, onCompleted).Forget();
		}

		public async UniTask LoadRuntimeAnimatorAsync(int animatorId, Action<RuntimeAnimatorController> onCompleted)
		{
			await LoadRuntimeAnimatorAsync(GetAnimatorData(animatorId).AnimatorAssetName, onCompleted);
		}

		public async UniTask LoadRuntimeAnimatorAsync(string animatorAssetName, Action<RuntimeAnimatorController> onCompleted)
		{
			var runtimeAnimatorController = await C2VAddressables.LoadAssetAsync<RuntimeAnimatorController>(animatorAssetName).ToUniTask();
			if (runtimeAnimatorController.IsReferenceNull())
			{
				C2VDebug.LogWarningCategory(nameof(AnimationManager), "Animator is not loaded.");
				onCompleted?.Invoke(null);
				return;
			}

			onCompleted?.Invoke(runtimeAnimatorController);
		}

		public async UniTask<RuntimeAnimatorController> LoadRuntimeAnimatorAsync(int animatorId) => await LoadRuntimeAnimatorAsync(GetAnimatorData(animatorId).AnimatorAssetName);

		public async UniTask<RuntimeAnimatorController> LoadRuntimeAnimatorAsync(string animatorAssetName, [CanBeNull] CancellationTokenSource cancellationTokenSource = null)
		{
			var runtimeAnimatorController = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<RuntimeAnimatorController>(animatorAssetName, cancellationTokenSource);
			if (runtimeAnimatorController.IsReferenceNull())
			{
				C2VDebug.LogWarningCategory(nameof(AnimationManager), "Animator is not loaded.");
				return null;
			}

			return runtimeAnimatorController;
		}

		public AvatarAnimator GetAnimatorData(int animatorId)
		{
			if (!TableAvatarAnimator.Datas.TryGetValue(animatorId, out var avatarAnimator))
			{
				C2VDebug.LogWarningCategory(nameof(AnimationManager),
				                            $"Can't find Animator. animationID : {animatorId.ToString()}");
				avatarAnimator = _defaultAvatarAnimatorData;
			}

			return avatarAnimator;
		}

		/// <summary>
		/// avatarType의 animatorType에 해당하는 Identifier를 리턴
		/// Identifier = avatarType * 10000 + animatorType
		/// </summary>
		/// <param name="avatarType">아바타의 타입 정보</param>
		/// <param name="animatorType">애니메이터 타입 정보</param>
		/// <returns></returns>
		public int GetFullAnimatorIdentifier(eAvatarType avatarType, eAnimatorType animatorType)
		{
			var isFirst               = true;
			var temporaryData         = 130000;
			var tableCostumeItemParts = TableAvatarAnimator;
			foreach (var data in tableCostumeItemParts.Datas.Values)
			{
				if (isFirst)
				{
					temporaryData = data.ID;
					isFirst       = false;
				}

				if (data.AvatarType == avatarType && data.AnimatorType == animatorType) return data.ID;
			}

			C2VDebug.LogError($"[AvatarManager] Can't find Animator. AvatarID : {avatarType.ToString()}, AnimatorID : {animatorType.ToString()}");
			return temporaryData;
		}
#endregion Animator

#region ClipEvent
		// private readonly Dictionary<string, AudioClip> _soundDict = new();
		// public async UniTask PlayAnimationSound(AudioSource source, string path, float volumeScale = 1.0f)
		// {
		// 	if (!_soundDict.ContainsKey(path) || _soundDict[path].IsUnityNull())
		// 	{
		// 		var assetLoadHandle = C2VAddressables.LoadAssetAsync<AudioClip>(path);
		// 		if (assetLoadHandle == null)
		// 		{
		// 			C2VDebug.LogErrorCategory(GetType().Name, $"Can't find sound. path : {path}");
		// 			return;
		// 		}
		// 	
		// 		var sound = await assetLoadHandle.ToUniTask();
		// 		if (sound.IsUnityNull())
		// 		{
		// 			C2VDebug.LogErrorCategory(GetType().Name, $"Loaded sound is null. path : {path}");
		// 			return;
		// 		}
		// 		_soundDict[path] = sound;
		// 	}
		// 	
		// 	SoundManager.Instance.PlayOneShot(source, _soundDict[path], volumeScale);
		// }

		public void PlayAnimationFx(AvatarAnimatorController animatorController, string assetName, CancellationTokenSource tokenSource)
		{
			if (!_isEnableAnimationEvents) return;
			PlayAnimationFxAsync(animatorController.FxEmitter, assetName, tokenSource, true).Forget();
		}

		private async UniTask PlayAnimationFxAsync([NotNull] Transform fxRoot, string assetName, [NotNull] CancellationTokenSource tokenSource, bool needSetParent = false)
		{
			if (fxRoot.IsUnityNull()) return;
			if (string.IsNullOrEmpty(assetName)) return;

			if (!_fxPrefabDict.TryGetValue(assetName, out var fxPrefab) || !_fxPools.TryGetValue(assetName, out var objectPool))
			{
				// var fxPrefabHandle = C2VAddressables.LoadAssetAsync<GameObject>($"{assetName}.prefab");
				// if (fxPrefabHandle == null)
				// {
				// 	C2VDebug.LogErrorCategory(nameof(AnimationManager), "not found fx asset prefab reference");
				// 	return;
				// }
				//
				// fxPrefab = await fxPrefabHandle!.ToUniTask(cancellationToken: tokenSource.Token);
				fxPrefab = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<GameObject>($"{assetName}.prefab", tokenSource);

				_fxPrefabDict[assetName] = fxPrefab;

				if (fxPrefab.IsReferenceNull())
				{
					C2VDebug.LogErrorCategory(nameof(AnimationManager), "not found fx asset prefab");
					return;
				}

				objectPool          = GetFxObjectPool(fxPrefab);
				_fxPools[assetName] = objectPool;
			}

			var fxObject = objectPool.Get();
			fxObject.transform.SetPositionAndRotation(fxRoot!.position, fxRoot.rotation);
			if (needSetParent) fxObject.transform.SetParent(fxRoot);
			var particleRoot = fxObject.GetComponent<ParticleRoot>();

			if (particleRoot.IsReferenceNull())
			{
				var particle = fxObject.GetComponentInChildren<ParticleSystem>();
				if (particle.IsReferenceNull())
				{
					C2VDebug.LogErrorCategory(nameof(AnimationManager), "This prefab has not particle system component");
					objectPool.Release(fxObject);
					return;
				}
				C2VDebug.LogWarningCategory(nameof(AnimationManager), "This prefab has not particle root component");
				await UniTask.Delay(TimeSpan.FromSeconds(particle.GetPlayTime()), DelayType.Realtime, cancellationToken: tokenSource.Token).SuppressCancellationThrow();
				objectPool.Release(fxObject);
				return;
			}

			await UniTask.Delay(TimeSpan.FromSeconds(particleRoot.PlayTime), DelayType.Realtime, cancellationToken: tokenSource.Token).SuppressCancellationThrow();
			objectPool.Release(fxObject);
		}

		private ObjectPool<GameObject> GetFxObjectPool(GameObject fxPrefab) => new(
			() => Instantiate(fxPrefab, transform, true),
			obj => obj.SetActive(true),
			obj =>
			{
				if (obj.IsUnityNull()) return;

				obj!.SetActive(false);
				obj.transform.SetParent(transform);
			},
			collectionCheck: false,
			defaultCapacity: PoolDefaultCapacity,
			maxSize: PoolMaxSize);
#endregion ClipEvent
	}
}
