/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarJigController.cs
* Developer:	eugene9721
* Date:			2023-05-12 15:53
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.AvatarAnimation;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.LruObjectPool;
using Com2Verse.UI;
using UnityEngine;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Com2Verse.Avatar.UI
{
	public sealed class AvatarJigController : MonoBehaviour
	{
		public const int ZoomLevelMax = 10;

		private static readonly Lazy<Shader> AlphaFilterShader = new(() => Shader.Find("Hidden/Com2Verse/Editor/AlphaFilterOnCapture(EDT)"));
		private static readonly int PropTexNonAlpha = Shader.PropertyToID("u_render_texture_non_alpha");
		private static readonly int PropTexAlpha = Shader.PropertyToID("u_render_texture_alpha");

#region SerializeField
		[SerializeField] [ReadOnly]
		private int _zoomLevel = ZoomLevelMax;

		[Header("Components")]
		[SerializeField] private Transform _fullBodyTransform = null!;

		[SerializeField] private Transform _faceTransform = null!;

		[field: SerializeField] public Transform CameraJigTransform = null!;
		[field: SerializeField] public Transform AvatarJigTransform = null!;
		[field: SerializeField] public Transform FxJig              = null!;
		[field: SerializeField] public Transform AiJig              = null!;

		[SerializeField] private Camera _aiCamera = null!;

		[Header("Zoom Option")]
		[SerializeField] private float _zoomDuration = 0.65f;

		[SerializeField] private float _zoomRefreshDuration = 1.2f;

		[SerializeField] private Ease _zoomEaseType = Ease.InSine;

		[SerializeField] private Ease _zoomRefreshEaseType = Ease.InQuart;

		[Header("Rotate Option")]
		[SerializeField] private float _rotatePower = 1.25f;

		[SerializeField] private float _resetRotateDuration = 0.85f;

		[Header("Camera Frame")]
		[SerializeField] private Vector2Int _aiTextureSize = new(0, 0);
		[SerializeField] private float      _renderScale   = 0.5f;

		[Header("Etc")]
		[SerializeField] private List<AssetReference> _manGestureFxList = null!;
		[SerializeField] private List<AssetReference> _womanGestureFxList = null!;

		[Header("Camera Jig")]
		[SerializeField] private float FullBodyCameraMargin = 0.2f;
		[SerializeField] private float HeadCameraMargin = 0.04f;

		[SerializeField] private float _headYOffset     = -0.02f;
		[SerializeField] private float _fullBodyYOffset = -0.08f;

		[SerializeField] private Vector3 _manAiCameraPosition   = new(0, 1.68f, 1.2f);
		[SerializeField] private Vector3 _womanAiCameraPosition = new(0, 1.55f, 1.2f);

		[Space]
		[SerializeField] private bool _isCalculateFullBodyCameraPosition;

		/// <summary>
		/// <see cref="_isCalculateFullBodyCameraPosition"/>가 false인 경우 적용되는 남자의 전신 카메라 포지션입니다.
		/// </summary>
		[SerializeField] private Vector3 _manFullBodyCameraPosition = new(0, 1.14665639f, 8.09097099f);

		/// <summary>
		/// <see cref="_isCalculateFullBodyCameraPosition"/>가 false인 경우 적용되는 여자의 전신 카메라 포지션입니다.
		/// </summary>
		[SerializeField] private Vector3 _womanFullBodyCameraPosition = new(0, 1.01255214f, 7.33042812f);


		[Header("LookAt")]
		[SerializeField] private float _lookAtDelayWhenRotate = 0.4f;
#endregion SerializeField

#region Fields
		private AvatarController? _avatarController;
		private LookAtController? _lookAtController;
		private CameraFrame?      _cameraFrame;
		private RawImage?         _avatarRawImage;
		private Animator?         _avatarAnimator;

		private bool _isRotating;

		private bool _isOnLookAtState;
		private bool _isOnLookAt;

		private float _reservedRotate;
		private float _lookAtDelayTimer;

		private bool _needCameraPositionRefresh;
		private bool _needZoomRefresh;

		private int _currentGestureIndex;

		private DG.Tweening.Tweener? _zoomTween;

		private int _clickMaintainFrameCount;

		private AvatarSelectionManagerViewModel? _viewModel;

		public bool IsOnGesture { get; set; }
#endregion Fields

		public void Enable()
		{
			gameObject.SetActive(true);
		}

		public void Disable()
		{
			if (!_avatarAnimator.IsUnityNull())
				_avatarAnimator!.Play(AnimationDefine.HashIdleCustomize);

			if (!_avatarController.IsUnityNull() && !_avatarController!.AvatarAnimatorController.IsUnityNull())
				_avatarController.AvatarAnimatorController!.OnRelease();

			gameObject.SetActive(false);
		}

#region UnityEventFunction
		private void Update()
		{
			if (_isOnLookAtState)
				OnLookAt(CanLookAt());

			UpdateRotate();

			_lookAtDelayTimer -= Time.deltaTime;

			if (_needCameraPositionRefresh)
			{
				CameraPositionRefresh();
				_needCameraPositionRefresh = false;
			}
		}

		private void OnAvatarBodyChanged()
		{
			_needCameraPositionRefresh = true;
		}

		public void SetNeedZoomRefresh()
		{
			_needCameraPositionRefresh = true;
			_needZoomRefresh           = true;
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			CameraPositionRefresh();
		}
#endif //UNITY_EDITOR
#endregion UnityEventFunction

#region Camera Jig Position
		private const float Fov = 20;

		private void CameraPositionRefresh()
		{
			if (_avatarController.IsUnityNull())
				return;

			var height     = _avatarController!.GetCombinedSkinnedMeshHeight();
			var headHeight = _avatarController.GetHeadMeshHeight();
			SetAvatarCameraHeight(height, headHeight);

			if (_needZoomRefresh)
			{
				ZoomRefresh();
				_needZoomRefresh = false;
			}
		}

		private void SetAvatarCameraHeight(float height, float headSize)
		{
			var bodySize = height - headSize;
			if (!_avatarController.IsUnityNull() && !_avatarController!.HeadBone.IsUnityNull())
				bodySize = _avatarController.HeadBone!.position.y - _avatarController.transform.position.y;
			bodySize = Mathf.Abs(bodySize);
			height   = Mathf.Max(height, bodySize);

			var denominator          = Mathf.Tan(Fov * 0.5f * Mathf.Deg2Rad) * 2;
			var fullBodyTargetHeight = height   + FullBodyCameraMargin * 2;
			var headTargetHeight     = headSize + HeadCameraMargin     * 2;

			var fullBodyDistance = fullBodyTargetHeight / denominator;
			var headDistance     = headTargetHeight     / denominator;

			var fullBodyHeight = height * 0.5f;
			var headHeight     = bodySize + headSize * 0.33f;

			if (_isCalculateFullBodyCameraPosition || _avatarController!.Info == null)
			{
				_fullBodyTransform.localPosition = new Vector3(0, fullBodyHeight + _fullBodyYOffset, fullBodyDistance);
			}
			else
			{
				var avatarType = _avatarController!.Info.AvatarType;
				_fullBodyTransform.localPosition = avatarType == eAvatarType.PC01_M ? _manFullBodyCameraPosition : _womanFullBodyCameraPosition;
			}
			_faceTransform.localPosition     = new Vector3(0, headHeight     + _headYOffset, headDistance);
		}
#endregion Camera Jig Position

#region Animation
		public void SetAvatar(AvatarController avatarController)
		{
			if (!_avatarController.IsUnityNull())
				_avatarController!.OnAvatarBodyChanged -= OnAvatarBodyChanged;

			_avatarController = avatarController;
			_lookAtController = avatarController.gameObject.GetOrAddComponent<LookAtController>()!;

			var parent = _avatarController.transform.parent;

			_avatarController.transform.SetParent(AvatarJigTransform, false);
			_avatarAnimator = _avatarController.GetComponent<Animator>();

			// 불필요한 Base 오브젝트 제거
			if (!parent.IsUnityNull())
				Destroy(parent!.gameObject);

			_lookAtController.Initialize(avatarController.HeadBone!, CameraJigTransform);
			_lookAtController.DisableLookAt();

			avatarController.OnAvatarBodyChanged += OnAvatarBodyChanged;
		}

		public void SetAnimatorSelectBody()
		{
			if (_avatarAnimator.IsUnityNull()) return;

			_avatarAnimator!.SetTrigger(AnimationDefine.HashSelectBody);
		}

		public void SetAnimatorSelectHead()
		{
			if (_avatarAnimator.IsUnityNull()) return;

			_avatarAnimator!.SetTrigger(AnimationDefine.HashSelectHead);
		}

		private const int GestureFxCount = 3;

		/// <summary>
		/// 블링크 대응 임시 제스처 재생 코드입니다
		/// </summary>
		public void PlayRandomGestureByAnimator()
		{
			if (_avatarController.IsUnityNull() || _avatarController!.AvatarAnimatorController.IsUnityNull()) return;
			var animatorController = _avatarController!.AvatarAnimatorController;

			_currentGestureIndex %= GestureFxCount;
			animatorController!.DisposeGestureToken();
			animatorController.TrySetTrigger(Animator.StringToHash($"Gesture {_currentGestureIndex}"));
			_currentGestureIndex++;
		}

		public async UniTask PlayRandomGestureAsync(eAvatarType avatarType)
		{
			AnimationClip? randomClip = null;

			switch (avatarType)
			{
				case eAvatarType.PC01_W:
					_currentGestureIndex %= _womanGestureFxList.Count;
					var womanReference  = _womanGestureFxList[_currentGestureIndex];
					if (womanReference == null)
						return;

					// var womanClipHandle = C2VAddressables.LoadAssetAsync<AnimationClip>(womanReference);
					// if (womanClipHandle == null)
					// 	return;
					//
					// randomClip = await womanClipHandle.ToUniTask();
					randomClip = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<AnimationClip>(womanReference);
					_currentGestureIndex++;
					break;
				case eAvatarType.PC01_M:
					_currentGestureIndex %= _manGestureFxList.Count;
					var manReference  = _manGestureFxList[_currentGestureIndex];
					if (manReference == null)
						return;

					// var manClipHandle = C2VAddressables.LoadAssetAsync<AnimationClip>(manReference);
					// if (manClipHandle == null)
					// 	return;
					//
					// randomClip = await manClipHandle.ToUniTask();
					randomClip = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<AnimationClip>(manReference);
					_currentGestureIndex++;
					break;
			}

			if (randomClip.IsUnityNull()) return;
			if (_avatarController.IsUnityNull() || _avatarController!.AvatarAnimatorController.IsUnityNull()) return;
			var animatorController = _avatarController!.AvatarAnimatorController;

			IsOnGesture = true;
			_avatarController.SetOverrideEyeBlink(false);
			await animatorController!.PlayGestureAsync(randomClip!);
			_avatarController.SetOverrideEyeBlink(true);
			IsOnGesture = false;
		}
#endregion Animation

#region Ai
		public void ClearAiJig()
		{
			foreach (Transform child in AiJig)
				Destroy(child.gameObject);
		}

		public void SetAvatarToAiJig(AvatarController avatarController)
		{
			avatarController.transform.SetParent(AiJig, false);
			avatarController.gameObject.SetActive(true);
		}

		public Texture2D RenderAvatarImage(eAvatarType avatarType = eAvatarType.PC01_W)
		{
			var width = (int)(_aiTextureSize.x * _renderScale);
			var height = (int)(_aiTextureSize.y * _renderScale);

			_aiCamera.transform.position = AiJig.position + (avatarType == eAvatarType.PC01_W ? _womanAiCameraPosition : _manAiCameraPosition);
			var urpCamera = _aiCamera.GetComponent<UniversalAdditionalCameraData>();

			var colorTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.BGRA32, RenderTextureReadWrite.sRGB);
			var alphaTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.BGRA32, RenderTextureReadWrite.sRGB);
			var filteredTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.BGRA32, RenderTextureReadWrite.sRGB);
			var image = new Texture2D(width, height, TextureFormat.BGRA32, false, false);

			var currentRT = RenderTexture.active;
			var alphaFilter = new Material(AlphaFilterShader.Value);

			var prevClearFlags = _aiCamera.clearFlags;
			var prevBg = _aiCamera.backgroundColor;

			_aiCamera.clearFlags = CameraClearFlags.SolidColor;
			_aiCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

			RenderTexture.active = alphaTexture;
			{
				_aiCamera.targetTexture = alphaTexture;
				_aiCamera.allowMSAA = false;
				urpCamera.renderPostProcessing = false;
				urpCamera.antialiasing = AntialiasingMode.None;
				_aiCamera.Render();
			}

			RenderTexture.active = colorTexture;
			{
				_aiCamera.targetTexture = colorTexture;
				_aiCamera.allowMSAA = true;
				urpCamera.renderPostProcessing = true;
				urpCamera.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
				_aiCamera.Render();
			}

			alphaFilter.SetTexture(PropTexAlpha, alphaTexture);
			alphaFilter.SetTexture(PropTexNonAlpha, colorTexture);
			Graphics.Blit(null, filteredTexture, alphaFilter);

			var req = AsyncGPUReadback.Request(filteredTexture);
			req.WaitForCompletion();
			image.SetPixelData(req.GetData<byte>(), 0);
			image.Apply();

			colorTexture.Release();
			alphaTexture.Release();
			filteredTexture.Release();
			Destroy(alphaFilter);

			RenderTexture.active = currentRT;
			_aiCamera.clearFlags = prevClearFlags;
			_aiCamera.backgroundColor = prevBg;

			return image;
		}
#endregion Ai

#region LookAt
		private bool CanLookAt()
		{
			if (IsOnGesture) return false;
			if (_avatarController.IsUnityNull()) return false;
			if (CameraJigTransform.IsUnityNull()) return false;
			if (_lookAtController.IsUnityNull()) return false;

			var avatarForward     = _avatarController!.transform.forward;
			var avatarToCameraDir = (CameraJigTransform.position - _avatarController.transform.position).normalized;
			var angle             = Vector3.Angle(avatarForward, Vector3.ProjectOnPlane(avatarToCameraDir, Vector3.up));

			if (angle < _lookAtController!.LookAtLimits.x) return false;
			if (angle > _lookAtController.LookAtLimits.y) return false;

			var isOnLookAtDelay = _lookAtDelayTimer > 0;
			_lookAtController.IsOnLookAtDelay = isOnLookAtDelay;
			_avatarController.IsOnLookAtDelay = isOnLookAtDelay;

			return true;
		}

		public void OnLookAtState(bool value)
		{
			_isOnLookAtState = value;

			if (!value)
			{
				OnLookAt(false);
				return;
			}

			if (value && !CanLookAt()) return;

			OnLookAt(value);
		}

		private void OnLookAt(bool value)
		{
			if (value)
			{
				if (!CanLookAt()) return;
				if (_isOnLookAt) return;

				_isOnLookAt = true;
				if (!_lookAtController.IsUnityNull())
					_lookAtController!.EnableLookAt();

				if (!_avatarController.IsUnityNull())
					_avatarController!.EnableLookAtEye(CameraJigTransform);
			}
			else
			{
				if (!_isOnLookAt) return;

				_isOnLookAt = false;
				if (!_lookAtController.IsUnityNull())
					_lookAtController!.DisableLookAt();

				if (!_avatarController.IsUnityNull())
					_avatarController!.DisableLookAtEye();
			}
		}
#endregion LookAt

#region Zoom
		[ContextMenu("ZoomIn")]
		public void ZoomIn()
		{
			_zoomTween?.Kill();

			_zoomLevel = Mathf.Clamp(_zoomLevel + 1, 0, ZoomLevelMax);
			OnZoom(_zoomDuration, _zoomEaseType);
		}

		[ContextMenu("ZoomOut")]
		public void ZoomOut()
		{
			_zoomTween?.Kill();

			_zoomLevel = Mathf.Clamp(_zoomLevel - 1, 0, ZoomLevelMax);
			OnZoom(_zoomDuration, _zoomEaseType);
		}

		public void ZoomRefresh()
		{
			_zoomTween?.Kill();

			_zoomLevel = Mathf.Clamp(_zoomLevel, 0, ZoomLevelMax);
			OnZoom(_zoomRefreshDuration, _zoomRefreshEaseType);
		}

		public void SetZoomLevel(int zoomLevel)
		{
			SetZoomLevel(zoomLevel, _zoomDuration);
		}

		public void SetZoomLevel(int zoomLevel, float zoomDuration)
		{
			_zoomTween?.Kill();

			_zoomLevel = Mathf.Clamp(zoomLevel, 0, ZoomLevelMax);
			OnZoom(zoomDuration, _zoomEaseType);
		}

		private void OnZoom(float duration, Ease easeType)
		{
			_zoomTween?.Kill();

			var fullBodyPosition = _fullBodyTransform.position;
			var facePosition     = _faceTransform.position;
			var targetPosition   = Vector3.Lerp(fullBodyPosition, facePosition, _zoomLevel / (float)ZoomLevelMax);

			_zoomTween = CameraJigTransform.transform.DOMove(targetPosition, duration).SetEase(easeType).OnComplete(OnZoomComplete);
		}

		private void OnZoomComplete()
		{
			_zoomTween = null;
		}
#endregion Zoom

#region Rotate
		/// <summary>
		/// 다음 업데이트 이벤트 타이밍에 실행될 회전을 예약합니다
		/// </summary>
		/// <param name="value">다음 프레임에 회전할 값</param>
		public void ReserveRotate(float value)
		{
			_lookAtDelayTimer = _lookAtDelayWhenRotate;
			_reservedRotate   = value;
		}

		private void UpdateRotate()
		{
			Rotate(_reservedRotate);
			_reservedRotate = 0f;
		}

		private void Rotate(float value)
		{
			if (_isRotating) return;

			var eventSystem = EventSystem.current;
			if (eventSystem.IsUnityNull() || eventSystem!.IsPointerOverGameObject()) return;

			AvatarJigTransform.Rotate(0, -value * _rotatePower, 0, Space.Self);
		}

		[ContextMenu("ResetRotation")]
		public void ResetRotation()
		{
			if (_isRotating) return;

			_isRotating = true;
			AvatarJigTransform.DOLocalRotate(Vector3.zero, _resetRotateDuration).SetEase(Ease.OutSine).OnComplete(OnResetRotationComplete);
		}

		public void ResetRotationImmediate()
		{
			AvatarJigTransform.localRotation = Quaternion.identity;
		}

		private void OnResetRotationComplete()
		{
			_isRotating = false;
		}
#endregion Rotate
	}
}
