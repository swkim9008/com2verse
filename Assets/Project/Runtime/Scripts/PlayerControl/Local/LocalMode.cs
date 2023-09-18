/*===============================================================
* Product:		Com2Verse
* File Name:	LocalMode.cs
* Developer:	eugene9721
* Date:			2023-02-02 16:31
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Avatar;
using UnityEngine;
using Com2Verse.AvatarAnimation;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.Director;
using Com2Verse.Extension;
using Com2Verse.InputSystem;
using Com2Verse.Network;
using Com2Verse.Project.InputSystem;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using FollowCamera = Com2Verse.CameraSystem.FollowCamera;

#if !METAVERSE_RELEASE

namespace Com2Verse.PlayerControl.LocalMode
{
	/// <summary>
	/// 서버를 통하지 않고 로컬에서 아바타를 테스트 할 때 사용하는 클래스
	/// </summary>
	public sealed class LocalMode : MonoBehaviour
	{
		[SerializeField] private bool _applyInspectorDataOnInitialize = false;
		[SerializeField] private bool _isNeedLoadCurrentData = true;

#if UNITY_EDITOR
		[Header("Game Speed")]
		[SerializeField] private bool _isApplyChangeGameSpeed = false;
		[SerializeField] private float _gameSpeed = 1.0f;
#endif //UNITY_EDITOR

		[Header("Movement Setting")]
		[SerializeField] private MovementData _movementData = new();
		[SerializeField] private JumpData         _jumpData         = new();
		[SerializeField] private GroundCheckData  _groundCheckData  = new();
		[SerializeField] private ForwardCheckData _forwardCheckData = new();

		[SerializeField] private AnimationParameterSyncData _animationParameterSyncData = new();

		[Header("Camera Setting")]
		[SerializeField] private FollowCamera.CameraTargetProxySetting _cameraTargetProxySetting = new();

		[SerializeField] private FollowCamera.CameraObstacleData       _cameraObstacleData       = new();
		[SerializeField] private FollowCamera.ZoomSettingData          _zoomSettingData          = new();
		[SerializeField] private FollowCamera.RotationBaseSettingData  _rotationBaseSettingData  = new();
		[SerializeField] private FollowCamera.RotationLimitSettingData _rotationLimitSettingData = new();
		[SerializeField] private FollowCamera.CinemachineSettingData   _cinemachineSettingData   = new();

#region Constant
		private const int WomanAnimatorID = 150000;
		private const int ManAnimatorID   = 160000;
#endregion Constant

#region Fields on Inspector
		[SerializeField]
		private ActiveObject? _setCharacterTarget = null;

		[SerializeField]
		private bool _isWoman = true;
#endregion Fields on Inspector

#region Fields
		private AvatarAnimatorController? _animatorController;

		private bool _isInitialized = false;
#endregion Fields

#region Properties
		public MovementData     MovementData     => _movementData;
		public JumpData         JumpData         => _jumpData;
		public GroundCheckData  GroundCheckData  => _groundCheckData;
		public ForwardCheckData ForwardCheckData => _forwardCheckData;
#endregion Properties

#region Mono
		private void Start()
		{
			Initialize().Forget();
			if (_isNeedLoadCurrentData) LoadCurrentData();
			UIManager.Instance.Initialize().Forget();
		}

		private async UniTask Initialize()
		{
			AvatarTable.LoadTable();
			await UniTask.Delay(1000);
			AnimationManager.Instance.EnableEvents();
			if (_applyInspectorDataOnInitialize)
			{
				SetMovementData();
				SetFollowCameraData();
			}

			await SetCharacterAsync();
			_isInitialized = true;
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			Time.timeScale = _isApplyChangeGameSpeed ? _gameSpeed : 1.0f;

			if (!_isInitialized) return;

			SetMovementData();
			SetFollowCameraData();
		}
#endif //UNITY_EDITOR
#endregion Mono

#region SetCharacter
		private void SetCharacter(ActiveObject target)
		{
			_animatorController = target.gameObject.GetOrAddComponent<AvatarAnimatorController>()!;

			InputSystemManager.Instance.ChangeActionMap<ActionMapCharacterControl>();
			CameraManager.Instance.ChangeState(eCameraState.FOLLOW_CAMERA);
			CameraManager.Instance.ChangeTarget(target.transform);
			PlayerController.Instance.SetComponents(target);
			_animatorController.OnObjectEnable(0, _isWoman ? eAvatarType.PC01_W : eAvatarType.PC01_M, true);
			if (target.TryGetComponent(out AvatarAnimatorController animatorController))
				animatorController!.LoadAnimatorAsync(_isWoman ? WomanAnimatorID : ManAnimatorID).Forget();
		}

		[ContextMenu("Character Change")]
		public void AddCharacterAvatar()
		{
			SetCharacterAsync().Forget();
		}

		public void ChangeCharacterGender(bool isWoman)
		{
			_isWoman = isWoman;
		}

		private async UniTask SetCharacterAsync()
		{
			var userAvatar = User.Instance.CharacterObject;
			if (userAvatar.IsUnityNull())
			{
				var info   = AvatarInfo.GetTestInfo();
				var avatar = await AvatarCreator.CreateAvatarAsync(info, eAnimatorType.WORLD, Vector3.zero, (int)Define.eLayer.CHARACTER);
				if (avatar.IsUnityNull()) return;

				var avatarObject = avatar!.gameObject;

				var activeObject = avatarObject.GetOrAddComponent<ActiveObject>()!;
				activeObject.Init(0, -1, false);
				avatarObject.SetActive(true);

				avatarObject.GetOrAddComponent<CharacterController>();
				avatarObject.GetOrAddComponent<AvatarAnimatorController>();

				if (!_setCharacterTarget.IsUnityNull())
				{
					await UniTask.NextFrame();
					var currentTransform = _setCharacterTarget!.transform;
					avatar.transform.SetPositionAndRotation(currentTransform.position, currentTransform.rotation);
					await _setCharacterTarget.DestroyGameObjectAsync();
				}

				_setCharacterTarget = activeObject;
				SetCharacter(activeObject);
				return;
			}

			_setCharacterTarget = userAvatar;
		}

		public void SetMovementData()
		{
			var playerController = PlayerController.InstanceOrNull;
			if (playerController.IsUnityNull()) return;
			playerController!.SetData(_movementData);
			playerController.SetData(_jumpData);
			playerController.SetData(_groundCheckData);
			playerController.SetData(_forwardCheckData);
			playerController.SetData(_animationParameterSyncData);
		}

		public void SetFollowCameraData()
		{
			var cameraManager = CameraManager.InstanceOrNull;
			var followCamera  = cameraManager?.StateMap[eCameraState.FOLLOW_CAMERA] as FollowCamera;
			if (followCamera == null) return;

			followCamera.SetCameraTargetProxySetting(_cameraTargetProxySetting);
			followCamera.SetCameraObstacleSetting(_cameraObstacleData);
			followCamera.SetCameraZoomSetting(_zoomSettingData);
			followCamera.SetCameraRotationBaseSetting(_rotationBaseSettingData);
			followCamera.SetCameraRotationLimitSetting(_rotationLimitSettingData);
			followCamera.SetCameraCinemachineSetting(_cinemachineSettingData);
		}
#endregion SetCharacter

#region UI Events
		[ContextMenu("Play Entering Director")]
		public void OnPlayEnteringDirector()
		{
			if (_setCharacterTarget.IsUnityNull()) return;
			UserDirector.Instance.PlayEnteringDirector(_setCharacterTarget!).Forget();
		}

		[ContextMenu("Hands Up")]
		public void OnHandsUp()
		{
			if (_animatorController.IsUnityNull()) return;
			_animatorController!.IsOnHandsUpState = true;
		}

		[ContextMenu("Hands Down")]
		public void OnHandsDown()
		{
			if (_animatorController.IsUnityNull()) return;
			_animatorController!.IsOnHandsUpState = false;
		}

		public void PlayEmotion(int animationId)
		{
			if (_animatorController.IsUnityNull()) return;
			_animatorController!.PlayGesture(animationId);
		}
#endregion UI Events

		public void ForceLoadCurrentData()
		{
			_isNeedLoadCurrentData = false;
			LoadCurrentData();
		}

		private void LoadCurrentData()
		{
			var playerController = PlayerController.InstanceOrNull;
			if (!playerController.IsUnityNull())
			{
				_movementData     = playerController!.MovementData;
				_jumpData         = playerController.JumpData;
				_groundCheckData  = playerController.GroundCheckData;
				_forwardCheckData = playerController.ForwardCheckData;
			}

			var cameraManager = CameraManager.InstanceOrNull;
			if (cameraManager?.StateMap[eCameraState.FOLLOW_CAMERA] is FollowCamera followCamera)
			{
				_cameraTargetProxySetting = followCamera.CameraTargetProxySettingValue;
				_cameraObstacleData       = followCamera.CameraObstacleDataValue;
				_zoomSettingData          = followCamera.ZoomSettingDataValue;
				_rotationBaseSettingData  = followCamera.RotationBaseSettingDataValue;
				_rotationLimitSettingData = followCamera.RotationLimitSettingDataValue;
				_cinemachineSettingData   = followCamera.CinemachineSettingDataValue;
			}
		}
	}
}

#endif // !METAVERSE_RELEASE
