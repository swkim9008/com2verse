/*===============================================================
* Product:		Com2Verse
* File Name:	UserDirector.cs
* Developer:	eugene9721
* Date:			2022-12-16 20:39
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Threading;
using Cinemachine;
using Com2Verse.AssetSystem;
using Com2Verse.Avatar;
using Com2Verse.AvatarAnimation;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.Deeplink;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Sound;
using Com2Verse.Tutorial;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Playables;

namespace Com2Verse.Director
{
	public sealed class UserDirector : MonoSingleton<UserDirector>, IDisposable
	{
		private const           string WarpOnFXPrefabPath   = "fx_cha_warp_01.prefab";
		private const           string WarpOffFXPrefabPath  = "fx_cha_warp_02.prefab";
		private const           string EnteringDirectorPath = "EnteringDirector.prefab";
		private static readonly string WarpStartSE          = "SE_PC01_M_WarpStart.wav";
		private static readonly string WarpEndSE            = "SE_PC01_M_WarpEnd.wav";

		private static readonly int DelayedForRemoveTimeMilliSecond = 1000;

		private static readonly float WarpEffectTime = 2.8f;

		private readonly CancellationTokenSource _cancellationTokenSource;

		private CancellationTokenSource? _warpFxCts;

		private bool _isInitialized;

		private GameObject?   _warpOnFXObject;
		private ParticleRoot? _warpOnParticleRoot;
		private GameObject?   _warpOffFXObject;
		private ParticleRoot? _warpOffParticleRoot;
		private GameObject?   _currentFXObject;

		private EnteringDirector? _enteringDirector;

		// TODO: 추후 쓰임이 있다면 복구
		// private bool _needEnteringDirection = true;

		public bool IsInitialized => _isInitialized;

		public bool IsPlayingWarpEffect { get; private set; }

		public bool IsActiveWarpEffect { get; set; } = true;

#region Initialize
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly]
		private UserDirector()
		{
			_cancellationTokenSource = new CancellationTokenSource();
		}

		public void Initialize()
		{
			InitializeAsync().Forget();
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();

			if (!_warpOnFXObject.IsUnityNull())
				Destroy(_warpOnFXObject!);
			if (!_warpOffFXObject.IsUnityNull())
				Destroy(_warpOffFXObject!);
		}

		private async UniTask InitializeAsync()
		{
			await LoadFxAssets();
			await LoadDirectorPrefabs();
			_isInitialized = true;
		}

		private async UniTask LoadDirectorPrefabs()
		{
			var prefabHandle = C2VAddressables.LoadAssetAsync<GameObject>(EnteringDirectorPath);
			if (prefabHandle == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"Failed to load EnteringDirector. Path: {EnteringDirectorPath}");
				return;
			}

			var prefab      = await prefabHandle.ToUniTask();
			var directorObj = Instantiate(prefab, transform);
			if (directorObj.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"Failed to load EnteringDirector. Path: {EnteringDirectorPath}");
				return;
			}
			directorObj!.SetActive(false);
			_enteringDirector = directorObj.GetComponent<EnteringDirector>();
		}

		private async UniTask LoadFxAssets()
		{
			await LoadWarpOnFxObject();
			await LoadWarpOffFxObject();
		}

		private async UniTask LoadWarpOnFxObject()
		{
			var objHandle = C2VAddressables.LoadAssetAsync<GameObject>(WarpOnFXPrefabPath);
			if (objHandle == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"Failed to load WarpOnFXObject. Path: {WarpOnFXPrefabPath}");
				return;
			}

			var obj = await objHandle.ToUniTask();
			_warpOnFXObject = Instantiate(obj, transform);
			if (_warpOnFXObject.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"Failed to load WarpOnFXObject. Path: {WarpOnFXPrefabPath}");
				return;
			}

			_warpOnFXObject!.SetActive(false);
			_warpOnFXObject.transform.localPosition = Vector3.zero;

			_warpOnParticleRoot = _warpOnFXObject.GetComponentInChildren<ParticleRoot>(true);
		}

		private async UniTask LoadWarpOffFxObject()
		{
			var objHandle = C2VAddressables.LoadAssetAsync<GameObject>(WarpOffFXPrefabPath);
			if (objHandle == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"Failed to load WarpOffFXObject. Path: {WarpOffFXPrefabPath}");
				return;
			}

			var obj = await objHandle.ToUniTask();
			_warpOffFXObject = Instantiate(obj, transform);
			if (_warpOffFXObject.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"Failed to load WarpOffFXObject. Path: {WarpOffFXPrefabPath}");
				return;
			}

			_warpOffFXObject!.SetActive(false);
			_warpOffFXObject.transform.localPosition = Vector3.zero;

			_warpOffParticleRoot = _warpOffFXObject.GetComponentInChildren<ParticleRoot>(true);
		}
#endregion Initialize

		public bool              NeedEnteringDirecting    { get; set; } = true;
		public TimelineDirector? EnteringDirector         => _enteringDirector;
		public TimelineDirector? OverrideEnteringDirector { get; set; } = null;

		public async UniTask WaitingForWarpEffect(bool isOn, Action? onCompleted)
		{
			var myObject = User.Instance.CharacterObject;
			if (myObject.IsUnityNull())
			{
				onCompleted?.Invoke();
				return;
			}

			if (!IsActiveWarpEffect)
			{
				onCompleted?.Invoke();
				return;
			}

			if (isOn && NeedEnteringDirecting)
			{
				NeedEnteringDirecting = false;
				await PlayEnteringDirector(myObject!);
				OverrideEnteringDirector = null;
			}
			else
			{
				await PlayWarpEffectAsync(myObject!, isOn);
			}

			onCompleted?.Invoke();
		}

		private async UniTask PlayWarpEffectAsync(ActiveObject myObject, bool isOn)
		{
			if (!myObject.AvatarController.IsUnityNull())
				myObject.AvatarController!.SetUseFadeIn(true);

			if (isOn)
			{
				var meshRenderers                              = myObject.transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
				foreach (var smr in meshRenderers) smr.enabled = false;
				await UniTask.Delay(DelayedForRemoveTimeMilliSecond, cancellationToken: _cancellationTokenSource.Token);
				meshRenderers = myObject.transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
				foreach (var smr in meshRenderers) smr.enabled = true;
				await PlayWarpEffect(myObject, true);
			}
			else
			{
				await PlayWarpEffect(myObject, false);
				var meshRenderers = myObject.transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
				foreach (var smr in meshRenderers) smr.enabled = false;
			}

			if (!myObject.AvatarController.IsUnityNull())
				myObject.AvatarController!.SetUseFadeIn(false);
		}

		private async UniTask PlayWarpEffect(ActiveObject avatarObject, bool isOn)
		{
			if (_warpOnFXObject.IsUnityNull() || _warpOffFXObject.IsUnityNull()) return;

			if (!_currentFXObject.IsReferenceNull())
				_currentFXObject!.SetActive(false);

			if (_warpFxCts != null)
			{
				_warpFxCts.Cancel();
				_warpFxCts.Dispose();
				_warpFxCts = null;
			}
			
			SoundManager.Instance.PlayUISound(isOn ? WarpStartSE : WarpEndSE);

			var animator = avatarObject.AnimatorController;
			if (!animator.IsUnityNull())
				animator!.TrySetTrigger(isOn ? AnimationDefine.HashFadeIn : AnimationDefine.HashFadeOut);

			_warpFxCts = new CancellationTokenSource();
			await PlayWarpParticle(isOn, avatarObject.transform.position, _warpFxCts);
		}

		private async UniTask PlayWarpParticle(bool isOn, Vector3 position, CancellationTokenSource cancellationTokenSource)
		{
			var currentParticleRoot = isOn ? _warpOnParticleRoot : _warpOffParticleRoot;
			_currentFXObject = isOn ? _warpOnFXObject : _warpOffFXObject;

			// fxObject가 활성화되자마자 바로 비활성화되지 않도록 token cancel이후 1프레임 대기
			await UniTask.NextFrame();

			_currentFXObject!.transform.position = position;
			_currentFXObject.SetActive(true);

			if (_currentFXObject.IsUnityNull()) return;

			var playTime = 0f;
			if (!currentParticleRoot.IsUnityNull())
			{
				playTime = currentParticleRoot!.PlayTime;
			}
			else
			{
				var particle = _currentFXObject.GetComponentInChildren<ParticleSystem>();
				if (_currentFXObject.IsReferenceNull()) return;
				playTime = particle!.GetPlayTime();
			}

			playTime = Mathf.Min(WarpEffectTime, playTime);

			IsPlayingWarpEffect = true;
			await UniTask.Delay(TimeSpan.FromSeconds(playTime), cancellationToken: cancellationTokenSource.Token).SuppressCancellationThrow();
			IsPlayingWarpEffect = false;
			if (_currentFXObject.IsUnityNull()) return;
			_currentFXObject!.SetActive(false);
		}

#region Director
		public async UniTask PlayEnteringDirector(ActiveObject myObject)
		{
			var enteringDirector = OverrideEnteringDirector.IsUnityNull() ? _enteringDirector : OverrideEnteringDirector;
			
			if (enteringDirector.IsUnityNull())
			{
				C2VDebug.LogError(GetType().Name, "EnteringDirector is null");
				return;
			}

			var mainCamera = CameraManager.Instance.MainCamera;
			if (mainCamera.IsUnityNull())
			{
				C2VDebug.LogError(GetType().Name, "MainCamera is null");
				return;
			}

			if (!mainCamera!.TryGetComponent(out CinemachineBrain brain))
			{
				C2VDebug.LogError(GetType().Name, "CinemachineBrain is null");
				return;
			}

			if (myObject.AvatarController.IsUnityNull())
			{
				C2VDebug.LogError(GetType().Name, "AvatarController is null");
				return;
			}

			if (myObject.AvatarController!.Info == null)
			{
				C2VDebug.LogError(GetType().Name, "AvatarInfo is null");
				return;
			}

			var message = new EnteringMessage(myObject, brain!, myObject.AvatarController.Info.AvatarType);
			enteringDirector!.gameObject.SetActive(true);
			enteringDirector.Play(message);
			await UniTask.Delay(TimeSpan.FromSeconds(enteringDirector.GetDuration()));
			await UniTask.WaitUntil(() => enteringDirector.PlayableDirector.IsUnityNull() || enteringDirector.PlayableDirector!.state == PlayState.Paused);
			if (CurrentScene.IsWorldScene && !DeeplinkManager.InfoStandby)
				TutorialManager.Instance.TutorialPlay(eTutorialGroup.MOVE).Forget();
		}
#endregion Director
	}
}
