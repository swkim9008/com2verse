/*===============================================================
* Product:		Com2Verse
* File Name:	CameraManager.cs
* Developer:	eugene9721
* Date:			2022-05-17 12:20
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Cinemachine;
using Com2Verse.AssetSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace Com2Verse.CameraSystem
{
	public enum eMainCameraType
	{
		NONE,
		BACKGROUND,
		UI,
	}

	public sealed partial class CameraManager : Singleton<CameraManager>, IDisposable
	{
#region CameraPrefabAddressable
		private const string BackGroundCameraAddressable           = "Camera_background.prefab";
		private const string UICameraAddressable                   = "Camera_UI.prefab";
		private const string AvatarCameraAddressable               = "Camera_Avatar.prefab";
		private const string CinemachineBlenderSettingsAddressable = "CinemachineBlenderSettings.asset";

		public GameObject? BackgroundCameraPrefab { get; private set; }
		public GameObject? UICameraPrefab         { get; private set; }
		public GameObject? AvatarCameraPrefab     { get; private set; }

		public CinemachineBlenderSettings? CinemachineBlenderSettings { get; private set; }
#endregion CameraPrefabAddressable

#region MainCamera
		private eMainCameraType _mainCameraType = eMainCameraType.NONE;

		public eMainCameraType MainCameraType
		{
			get => _mainCameraType;
			set
			{
				_mainCameraType = value;
				UpdateMainCamera();
			}
		}

		public Camera?          MainCamera      { get; private set; }
		public MetaverseCamera? MetaverseCamera { get; private set; }

		public int CurrentRendererIndex { get; private set; }

		private readonly Dictionary<eCullingGroupType, float[]> _cullingGroupBoundingDistanceMap = new();

		private int _lastCullingMask;
#endregion MainCamera

#region CameraFeatures
		private static readonly int UpdateCameraViewDelayTime = 500;

		public CinemachineVirtualCamera MainVirtualCamera { get; }

		public Dictionary<eCameraState, CameraBase> StateMap { get; } = new();

		public eCameraState CurrentState { get; private set; }

		public List<IUpdatableCamera> UpdatableCameras { get; } = new();

		private CancellationTokenSource? _updateCts;

		private int _currentViewIndex;

		/// <summary>
		/// 카메라 충돌이 필요한 경우, 충돌을 감지할 오브젝트의 레이어 설정
		/// </summary>
		public LayerMask CameraCollisionFilter { get; set; }

		public IAvatarTarget? UserAvatarTarget { get; set; }

		private CameraBase? _currentMainCamera;

		public Transform? MainCameraTarget
		{
			get => _currentMainCamera?.TargetObject;
			set => _currentMainCamera?.OnChangeTarget(value);
		}
#endregion CameraFeatures

#region Initialization
		public bool IsInitializing { get; private set; }
		public bool IsInitialized  { get; private set; }

		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private CameraManager()
		{
			MainVirtualCamera = CreateVirtualCamera();
		}

		private CinemachineVirtualCamera CreateVirtualCamera()
		{
			var gameObject = new GameObject { name = "MainVirtualCamera" };

			if (Application.isPlaying)
				UnityEngine.Object.DontDestroyOnLoad(gameObject);

			return gameObject.AddComponent<CinemachineVirtualCamera>()!;
		}

		public async UniTask TryInitializeAsync()
		{
			if (IsInitialized || IsInitializing)
				return;

			IsInitializing = true;
			{
				LoadTable();
				await LoadAssetAsync();
			}
			IsInitializing = false;
			IsInitialized  = true;
		}

		private async UniTask LoadAssetAsync()
		{
			BackgroundCameraPrefab     = await TryGetCameraAddressable<GameObject>(BackGroundCameraAddressable);
			UICameraPrefab             = await TryGetCameraAddressable<GameObject>(UICameraAddressable);
			AvatarCameraPrefab         = await TryGetCameraAddressable<GameObject>(AvatarCameraAddressable);
			CinemachineBlenderSettings = await TryGetCameraAddressable<CinemachineBlenderSettings>(CinemachineBlenderSettingsAddressable);
		}

		private async UniTask<T?> TryGetCameraAddressable<T>(string addressablePath) where T : UnityEngine.Object
		{
			var handle = C2VAddressables.LoadAssetAsync<T>(addressablePath);
			if (handle == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"Failed to load camera asset : {addressablePath}");
				return null;
			}

			var result = await handle.ToUniTask();
			if (result.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"Failed to load camera asset : {addressablePath}");
				return null;
			}

			return result;
		}
#endregion Initialization

#region Dispose
		public void Dispose()
		{
			ClearEvents();

			MainVirtualCamera.DestroyGameObject();
			StateMap.Clear();

			var screenSize = ScreenSize.InstanceOrNull;
			if (screenSize != null)
				screenSize.ScreenResized -= OnScreenResized;
		}
#endregion Dispose

#region MainCameraSettings
		private void UpdateMainCamera()
		{
			var prevCamera = MainCamera;
			var mainCamera = Camera.main;

			if (mainCamera.IsUnityNull())
			{
				MainCamera = CreateMainCamera();
			}
			else
			{
				if (prevCamera == mainCamera)
					return;

				MainCamera = mainCamera;
			}

			UpdateMainCameraProperties();
		}

		private Camera? CreateMainCamera()
		{
			var cameraPrefab = MainCameraType switch
			{
				eMainCameraType.BACKGROUND => BackgroundCameraPrefab,
				eMainCameraType.UI         => UICameraPrefab,
				_                          => null,
			};

			if (cameraPrefab.IsReferenceNull())
				return null;

			var cameraObject = UnityEngine.Object.Instantiate(cameraPrefab)!;
			var mainCamera   = cameraObject.GetComponent<Camera>()!;
			mainCamera.name = "MainCamera";
			mainCamera.tag  = "MainCamera";
			return mainCamera;
		}

		private void UpdateMainCameraProperties()
		{
			if (MainCamera.IsUnityNull())
			{
				_lastCullingMask = 0;
				MetaverseCamera  = null;
			}
			else
			{
				_lastCullingMask = MainCamera!.cullingMask;
				MetaverseCamera  = Util.GetOrAddComponent<MetaverseCamera>(MainCamera!.gameObject);
			}
			MetaverseCamera.Brain.m_CustomBlends = CinemachineBlenderSettings;

			foreach (var cameras in StateMap.Values)
				cameras.SetCamera(MainCamera);
		}

		public void SetCameraBlendTime(float value)
		{
			if (!MetaverseCamera.IsUnityNull() && !MetaverseCamera!.Brain.IsUnityNull())
				MetaverseCamera!.Brain!.m_DefaultBlend.m_Time = value;
		}

		public void SetDefaultBlendTime()
		{
			if (!MetaverseCamera.IsUnityNull())
				MetaverseCamera!.SetDefaultBlendTime();
		}
#endregion MainCameraSettings

#region CameraFeatures
		public void OnUpdate()
		{
			_currentMainCamera?.OnUpdate();

			foreach (var updatableCamera in UpdatableCameras)
				updatableCamera?.OnUpdate();

			InvokeLensEvents();
		}

		public void OnLateUpdate()
		{
			if (!MetaverseCamera.IsReferenceNull())
				MetaverseCamera!.OnLateUpdate();

			_currentMainCamera?.OnLateUpdate();

			foreach (var updatableCamera in UpdatableCameras)
			{
				updatableCamera?.OnLateUpdate();
			}
		}

		public void SetCullingMask(int cullingMask)
		{
			var mainCamera = MainCamera;
			if (mainCamera.IsUnityNull())
				return;

			mainCamera!.cullingMask = cullingMask;
		}

		public void SetPrevCullingMask()
		{
			SetCullingMask(_lastCullingMask);
		}

		public void SetRenderer(int renderer)
		{
			var mainCamera = MainCamera;
			if (mainCamera.IsUnityNull())
				return;

			mainCamera!.GetUniversalAdditionalCameraData()?.SetRenderer(renderer);
			CurrentRendererIndex = renderer;
			InvokeCameraRendererChange(renderer);
		}

		public void ChangeState(eCameraState nextState, string? jigKey = null, Action? onComplete = null)
		{
			if (!StateMap.ContainsKey(nextState))
			{
				C2VDebug.LogErrorCategory(nameof(CameraManager), "This CameraState is null");
				if (!MetaverseCamera.IsReferenceNull())
					MetaverseCamera!.OnBlending(onComplete);
				return;
			}

			var prevCamera    = _currentMainCamera;
			var currentTarget = _currentMainCamera?.TargetObject;

			if (nextState is eCameraState.FOLLOW_CAMERA)
			{
				var userAvatarTarget = GetUserAvatarTarget();
				if (!userAvatarTarget.IsReferenceNull())
					currentTarget = userAvatarTarget;
			}

			_currentMainCamera?.OnStateExit();
			CurrentState       = nextState;
			_currentMainCamera = StateMap[CurrentState];

			_currentMainCamera!.OnStateEnter();
			_currentMainCamera.OnChangeTarget(currentTarget);

			if (!MetaverseCamera.IsReferenceNull())
				MetaverseCamera!.OnBlending(onComplete);

			if (nextState == eCameraState.FIXED_CAMERA)
				FixedCameraManager.Instance.SwitchNearestCamera(jigKey);

			InvokeCameraStateChange(prevCamera, _currentMainCamera);
		}

		public void InitializeValueCurrentCamera()
		{
			_currentMainCamera?.InitializeValue();
		}

		private Transform? GetUserAvatarTarget()
		{
			if (_currentMainCamera == null) return null;
			if (UserAvatarTarget   == null) return null;
			if (UserAvatarTarget.AvatarTarget.IsUnityNull()) return null;
			return UserAvatarTarget.AvatarTarget;
		}

		public void ChangeTarget(Transform cameraTarget, Action? onComplete = null)
		{
			_currentMainCamera?.OnChangeTarget(cameraTarget);
			if (!MetaverseCamera.IsReferenceNull())
				MetaverseCamera!.OnBlending(onComplete);
		}

		public async UniTaskVoid UpdateCameraView()
		{
			if (_updateCts is { IsCancellationRequested: false })
			{
				_updateCts.Cancel();
				_updateCts = null;
			}

			_updateCts = new CancellationTokenSource();

			while (!_updateCts.IsCancellationRequested)
			{
				SendUpdateCameraView();
				await UniTask.Delay(UpdateCameraViewDelayTime, cancellationToken: _updateCts.Token);
			}

			_updateCts = null;
		}

		public void CleanupComponents()
		{
			StopUpdateCameraView();
		}

		private void StopUpdateCameraView()
		{
			_updateCts?.Cancel();
		}

		private void SendUpdateCameraView()
		{
			if (MainVirtualCamera.IsReferenceNull())
				return;

			var followTarget = MainVirtualCamera.Follow;
			if (followTarget.IsUnityNull())
				return;

			var viewIndex = MathUtil.GetIndexOfAngle(followTarget!.rotation);
			if (_currentViewIndex == viewIndex) return;

			_currentViewIndex = viewIndex;
			InvokeUpdateCameraView(_currentViewIndex);
		}

		public Ray GetRayFromCamera()
		{
			if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
				return default;

			Vector2 clickPosition = Mouse.current.position.ReadValue();
			float   screenWidth   = Screen.width;
			float   screenHeight  = Screen.height;

			if (clickPosition.x >= screenWidth  ||
			    clickPosition.x < 0             ||
			    clickPosition.y >= screenHeight ||
			    clickPosition.y < 0)
				return default;

			var mainCamera = MainCamera;
			if (mainCamera.IsUnityNull())
				return default;

			return mainCamera!.ScreenPointToRay(clickPosition);
		}

		public bool IsFrontOfCamera(Vector3 worldPosition)
		{
			var mainCamera = MainCamera;
			if (mainCamera.IsUnityNull())
				return false;

			var mainCameraTransform = mainCamera!.transform;
			return Vector3.Dot(mainCameraTransform.forward,
			                   (worldPosition - mainCameraTransform.position).normalized) > 0;
		}

		public void SetPreventZoomCamera(bool isPrevent)
		{
			if (CurrentState != eCameraState.FOLLOW_CAMERA) return;
			if (_currentMainCamera is not FollowCamera camera) return;
			camera.SetPreventZoom(isPrevent);
		}
#endregion CameraFeatures

#region CullingMask
		public void OnCullingMaskLayer(int layerIndex)
		{
			OnCullingMaskLayer(MainCamera, layerIndex);
		}

		public void OffCullingMaskLayer(int layerIndex)
		{
			OffCullingMaskLayer(MainCamera, layerIndex);
		}

		public static void OnCullingMaskLayer(Camera? camera, int layerIndex)
		{
			if (camera.IsUnityNull()) return;

			camera!.cullingMask |= 1 << layerIndex;
		}

		public static void OffCullingMaskLayer(Camera? camera, int layerIndex)
		{
			if (camera.IsUnityNull()) return;

			camera!.cullingMask &= ~(1 << layerIndex);
		}

		public static void FlipCullingMaskLayer(Camera? camera, int layerIndex)
		{
			if (camera.IsUnityNull()) return;

			camera!.cullingMask ^= (1 << layerIndex);
		}

		public static bool GetCullingMaskLayer(Camera? camera, int layerIndex)
		{
			if (camera.IsUnityNull()) return false;

			return (camera!.cullingMask & (1 << layerIndex)) >= 1;
		}
#endregion CullingMask

#region CullingGroupProxy
		public CullingGroupProxy SetCullingGroupProxy(GameObject gameObject, eCullingGroupType cullingGroupType)
		{
			var cullingGroups = gameObject.GetComponents<CullingGroupProxy>();

			CullingGroupProxy result;
			if (cullingGroups == null || cullingGroups.Length == 0)
			{
				result = gameObject.AddComponent<CullingGroupProxy>();
				result.SetCullingGroupType(cullingGroupType);
				return result;
			}

			foreach (var cullingGroup in cullingGroups)
				if (cullingGroup.CullingGroupType == cullingGroupType)
					return cullingGroup;

			result = gameObject.AddComponent<CullingGroupProxy>();
			result.SetCullingGroupType(cullingGroupType);

			return result;
		}

		public float[]? GetCullingGroupBoundingDistance(eCullingGroupType cullingGroupType)
		{
			if (_cullingGroupBoundingDistanceMap.ContainsKey(cullingGroupType))
				return _cullingGroupBoundingDistanceMap[cullingGroupType];

			return null;
		}

		public void SetCullingGroupBoundingDistance(eCullingGroupType cullingGroupType, float[] boundingDistance)
		{
			if (_cullingGroupBoundingDistanceMap.ContainsKey(cullingGroupType))
				_cullingGroupBoundingDistanceMap[cullingGroupType] = boundingDistance;
			else
				_cullingGroupBoundingDistanceMap.Add(cullingGroupType, boundingDistance);
		}
#endregion CullingGroupProxy
	}
}
