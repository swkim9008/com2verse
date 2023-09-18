/*===============================================================
* Product:	  Com2Verse
* File Name:  PlayerController.cs
* Developer:  eugene9721
* Date:       2022-05-02 18:55
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.AvatarAnimation;
using Com2Verse.Chat;
using Com2Verse.Director;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.Project.Animation;
using Com2Verse.Project.InputSystem;
using Com2Verse.UI;
using Com2Verse.Utils;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;
using Protocols;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace Com2Verse.PlayerControl
{
	public sealed class PlayerController : MonoSingleton<PlayerController>, IDisposable
	{
		public const float MovingThresholdVelocityOnNavigation = 0.001f;
		
		private const int DefaultTableIndex = 1;

		public void Dispose()
		{
			foreach (var states in _movementStateMap.Values)
				states.Clear();
		}

		private readonly Dictionary<ePlayerMovementType, PlayerMovementBase> _movementStateMap = new();

		// 서버 기준 (11.9)
		// walk 5
		// run 10
		// back 3
		// rotation 1.3

#region Fields
		private static readonly string MinimapPopupPrefab = "UI_Popup_Map";

		[SerializeField] private bool  _analogMovement;
		[SerializeField] private float _clickMoveInterval = 0.7f;

		[Header("Player Jump")]
		[SerializeField] private float _jumpTimeout = 1.2f;

		[SerializeField] private float _delayedJumpTime = 0.1f;

		private ActiveObject? _characterView;
		private Vector2       _prevSentMoveValue;
		private Vector2       _inputMoveValue;

		private float _clickMoveTimer; // 길찾기 요청, 파티클을 위함
		private float _jumpTimeoutTimer;
		private float _preventInputTimer;

		private bool _reservedJump;
		private bool _isNeedSprintUpdate;

		private UIClickFx?     _clickFx;
		private CharacterState _characterState;

		private bool _enableControl = true;
#if !UNITY_EDITOR
		private bool _prevFocusOn = true;
#endif
		[field: SerializeField, ReadOnly] private bool _canMoveState     = true;
		private                                   bool _canMoveAnimation = true;

		private PlayerMovementBase? _currentMovementState;

		private Action<IReadOnlyList<Vector3>>?         _onChangeWaypoints;
		private Action<float>?                          _onProgressWaypoints;
		private Action<CharacterState, CharacterState>? _onCharacterStateChanged;
		private Action<IMovementData>?                  _onMoveDataChanged;

		private TableAvatarControl?          _avatarControlTable;
		private AvatarControl?               _avatarControlData;
		private TableAnimationParameterSync? _animationParameterSync;
#endregion Fields

#region Properties
		public bool EnableControl
		{
			get => _enableControl;
			set
			{
				_enableControl = value;
				if (value) _currentMovementState?.Initialize();
				else _currentMovementState?.Clear();
			}
		}

		public bool CanInput => _preventInputTimer <= float.Epsilon;
		public bool CanMove => _canMoveState &&
		                       _canMoveAnimation &&
		                       !_characterView.IsReferenceNull() &&
		                       _characterView.CanMove() &&
		                       !UserDirector.Instance.IsPlayingWarpEffect &&
		                       !_characterView!.IsForceStopUpdate &&
		                       CanInput;

		public bool CanEmotion => _canMoveState                              &&
		                          _canMoveAnimation                          &&
		                          !_characterView.IsReferenceNull()          &&
		                          _characterView.CanEmotion()                &&
		                          !UserDirector.Instance.IsPlayingWarpEffect &&
		                          !_characterView!.IsForceStopUpdate         &&
		                          CanInput;

		private bool _isCommandInput  = false;
		private bool _enableClickMove = true;

		public bool IsMoving => !_inputMoveValue.Equals(Vector2.zero);

		public bool IsControlledByServer { get; set; } = false;

		public GestureHelper GestureHelper { get; } = new();

		public ActiveObject? CharacterView => _characterView;

		public event Action<IReadOnlyList<Vector3>>? OnChangeWaypoints
		{
			add
			{
				_onChangeWaypoints -= value;
				_onChangeWaypoints += value;
			}
			remove => _onChangeWaypoints -= value;
		}

		public event Action<float>? OnProgressWaypoints
		{
			add
			{
				_onProgressWaypoints -= value;
				_onProgressWaypoints += value;
			}
			remove => _onProgressWaypoints -= value;
		}

		public event Action<CharacterState, CharacterState>? OnCharacterStateChanged
		{
			add
			{
				_onCharacterStateChanged -= value;
				_onCharacterStateChanged += value;
			}
			remove => _onCharacterStateChanged -= value;
		}

		public event Action<IMovementData>? OnMoveDataChanged
		{
			add
			{
				_onMoveDataChanged -= value;
				_onMoveDataChanged += value;
			}
			remove => _onMoveDataChanged -= value;
		}

		private Action?                _onMinimapUIOpened;
		private Action?                _onMinimapUIClosed;
		private GUIView?               _minimapGUIView;
		private MinimapPopupViewModel? _minimapViewModel;
		public event Action OnMinimapUIOpenedEvent
		{
			add
			{
				_onMinimapUIOpened -= value;
				_onMinimapUIOpened += value;
			}
			remove => _onMinimapUIOpened -= value;
		}
		public event Action OnMinimapUIClosedEvent
		{
			add
			{
				_onMinimapUIClosed -= value;
				_onMinimapUIClosed += value;
			}
			remove => _onMinimapUIClosed -= value;
		}

		public JumpData                   JumpData                   { get; private set; } = new();
		public MovementData               MovementData               { get; private set; } = new();
		public GroundCheckData            GroundCheckData            { get; private set; } = new();
		public ForwardCheckData           ForwardCheckData           { get; private set; } = new();
		public AnimationParameterSyncData AnimationParameterSyncData { get; private set; } = new();
#endregion Properties

#region MonoBehaviour
		protected override void AwakeInvoked()
		{
			InputSystemManager.Instance.AnalogMovement = _analogMovement;

			_clickFx = Util.GetOrAddComponent<UIClickFx>(gameObject);
			InitializeStateMap();
			GestureHelper.Initialize();

			LoadTable();
		}
#if !UNITY_EDITOR
		private void OnApplicationFocus(bool hasFocus)
		{
			if (_prevFocusOn != hasFocus)
			{
				if (!_inputMoveValue.Equals(Vector2.zero))
				{
					_inputMoveValue = Vector2.zero;
					CommandMoveCheck();
					// TODO: 최종 입력 값이 남아있어 다시 호출되는 이슈 있음
				}
			}
			_prevFocusOn = hasFocus;
		}
#endif //!UNITY_EDITOR

		protected override void OnDestroyInvoked()
		{
			RemoveEvents();
		}

		public void OnUpdate()
		{
			_clickMoveTimer    += Time.deltaTime;
			_jumpTimeoutTimer  += Time.deltaTime;
			_preventInputTimer =  Math.Max(0, _preventInputTimer - Time.deltaTime);

			CheckReservedJump();
			SprintCheck();
			CommandMoveCheck();

			if (EnableControl)
				_currentMovementState?.OnUpdate();

#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.F10))
				FindMyCharacter();
#endif // UNITY_EDITOR
		}
#endregion MonoBehaviour

#region StateCheck
		private void SprintCheck()
		{
			if (!_isNeedSprintUpdate) return;
			if (InputFieldExtensions.IsExistFocused) return;
			if (_characterView.IsUnityNull()) return;

			_isNeedSprintUpdate = false;
			SendMessageSprint();
		}

		private void CommandMoveCheck()
		{
			if (_characterView.IsUnityNull()) return;
			if (InputFieldExtensions.IsExistFocused) return;
			if (!CanMove) return;

			if (!_isCommandInput)
			{
				if (_inputMoveValue.Equals(Vector2.zero)) return;
				_isCommandInput             = true;
			}

			SendMessageMoveCommand();
			if (_isCommandInput && _inputMoveValue.Equals(Vector2.zero))
				_isCommandInput = false;
		}
#endregion StateCheck

#region Initialize
		private void InitializeStateMap()
		{
			_movementStateMap[ePlayerMovementType.ONLY_CLIENT] = new PlayerMovementOnlyClient();
			_movementStateMap[ePlayerMovementType.BY_SERVER]   = new PlayerMovementByServer();
			_movementStateMap[ePlayerMovementType.WITH_SERVER] = new PlayerMovementWithServer();
			SetMovementState(ePlayerMovementType.ONLY_CLIENT);
		}

		public void SetMovementState(ePlayerMovementType type)
		{
			if (!_movementStateMap.ContainsKey(type))
			{
				C2VDebug.LogErrorCategory(nameof(PlayerController), $"Not found movement state type : {type}");
				return;
			}

			_currentMovementState?.Clear();
			var currentTarget = _currentMovementState?.GetCharacter();
			_currentMovementState = _movementStateMap[type];
			if (!currentTarget.IsReferenceNull())
				_currentMovementState?.SetCharacter(currentTarget!);
			ApplyAvatarControlTableData();
		}

		public void PreventMovementForSeconds(float seconds)
		{
			_preventInputTimer = seconds;
		}

		public void SetComponents(ActiveObject mapObject)
		{
			if (mapObject.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(nameof(PlayerController), "MapObject is null");
				return;
			}

			_currentMovementState!.SetCharacter(mapObject);
			_characterView = mapObject;
			AddEvents();
		}

		public void SetStopAndCannotMove(bool isStop)
		{
			_canMoveState = !isStop;
			if (!isStop) return;

			_inputMoveValue = Vector2.zero;
			SendMessageMoveCommand();
		}

		public void SetCharacterState(CharacterState state)
		{
			if (_characterState != state)
				_onCharacterStateChanged?.Invoke(_characterState, state);

			switch (state)
			{
				case CharacterState.None:
				case CharacterState.IdleWalkRun:
				case CharacterState.InAir:
				case CharacterState.JumpStart:
				case CharacterState.JumpLand:
				case CharacterState.Sit:
					_canMoveState = true;
					break;
				case CharacterState.NavigationFinish:
					_canMoveState = true;
					_currentMovementState!.OnFinishNavigation();
					break;
			}

			_characterState = state;
		}

		public void SetCanMoveAnimation(bool value)
		{
			_canMoveAnimation = value;
		}

		public void Teleport(Vector3 position, Quaternion rotation)
		{
			_currentMovementState!.Teleport(position, rotation);
		}

		public void ForceSetCharacterState(CharacterState characterState)
		{
			_currentMovementState!.ForceSetCharacterState(characterState);
		}

		private void AddEvents()
		{
			var actionMapCharacterControl = InputSystemManager.Instance.GetActionMap<ActionMapCharacterControl>();
			if (actionMapCharacterControl == null)
			{
				C2VDebug.LogError($"[CameraSystem] ActionMap Not Found");
			}
			else
			{
				actionMapCharacterControl.ClickWorldPositionAction += OnClickWorldPosition;
				actionMapCharacterControl.JumpAction               += OnJump;
				actionMapCharacterControl.SprintAction             += OnSprint;
				actionMapCharacterControl.MoveAction               += OnMove;
				actionMapCharacterControl.ChatAction               += OnChat;
				actionMapCharacterControl.MinimapAction            += OnMinimap;
			}

			if (!InputSystemManager.InstanceExists) return;
			var actionMapUI = InputSystemManager.Instance.GetActionMap<ActionMapUIControl>();
			if (actionMapUI == null)
				C2VDebug.LogError($"[UI] ActionMap Not Found");
			else
				actionMapUI.MinimapAction += OnMinimap;
		}

		public void RemoveEvents()
		{
			_characterView = null;
			var actionMapCharacterControl = InputSystemManager.InstanceOrNull?.GetActionMap<ActionMapCharacterControl>();
			if (actionMapCharacterControl != null)
			{
				actionMapCharacterControl.ClickWorldPositionAction -= OnClickWorldPosition;
				actionMapCharacterControl.JumpAction               -= OnJump;
				actionMapCharacterControl.SprintAction             -= OnSprint;
				actionMapCharacterControl.MoveAction               -= OnMove;
				actionMapCharacterControl.ChatAction               -= OnChat;
				actionMapCharacterControl.MinimapAction            -= OnMinimap;
			}

			if (!InputSystemManager.InstanceExists) return;
			var actionMapUI = InputSystemManager.Instance.GetActionMap<ActionMapUIControl>();
			if (actionMapUI != null)
				actionMapUI.MinimapAction -= OnMinimap;
		}

		public void SetData(IMovementData data)
		{
			foreach (var movementState in _movementStateMap.Values)
			{
				movementState.SetData(data);
			}

			_onMoveDataChanged?.Invoke(data);
		}

		private void LoadTable()
		{
			_avatarControlTable     = TableDataManager.Instance.Get<TableAvatarControl>();
			_animationParameterSync = TableDataManager.Instance.Get<TableAnimationParameterSync>();

			ApplyAvatarControlTableData();
			ApplyAnimationParameterSyncTableData();
		}

		private void ApplyAvatarControlTableData()
		{
			if (_avatarControlTable == null) return;
			var data = _avatarControlTable.Datas?[DefaultTableIndex];
			if (data == null) return;

			MovementData = new MovementData();
			MovementData.SetData(data);
			SetData(MovementData);

			JumpData = new JumpData();
			JumpData.SetData(data);
			SetData(JumpData);

			GroundCheckData = new GroundCheckData();
			GroundCheckData.SetData(data);
			SetData(GroundCheckData);

			ForwardCheckData = new ForwardCheckData();
			ForwardCheckData.SetData(data);
			SetData(ForwardCheckData);
		}

		private void ApplyAnimationParameterSyncTableData()
		{
			if (_animationParameterSync == null) return;
			var data = _animationParameterSync.Datas?[DefaultTableIndex];
			if (data == null) return;

			AnimationParameterSyncData = new AnimationParameterSyncData();
			AnimationParameterSyncData.SetData(data);
			SetData(AnimationParameterSyncData);
		}
#endregion Initialize

#region InputEvent
		private void OnClickWorldPosition(Vector3 value)
		{
			if (!User.Instance.Standby) return;
			if (!CanMove) return;
			if (_characterView.IsUnityNull()) return;
			if (_characterView!.IsGesturing)
				_characterView.CancelEmotion();

			if (!_enableClickMove) return;

			if (_clickMoveTimer >= _clickMoveInterval)
			{
				if (!_clickFx.IsReferenceNull())
					_clickFx!.PlayEffect(value);
				_clickMoveTimer = 0f;

				if (_inputMoveValue != Vector2.zero) return;
				_currentMovementState!.MoveTo(value);
			}
		}

		private void OnMove(Vector2 value)
		{
			if (!CanInput) return;
			if (!User.Instance.Standby) return;
			if (_characterView.IsUnityNull()) return;
			if (InputFieldExtensions.IsExistFocused)
			{
				_inputMoveValue = Vector2.zero;
				SendMessageMoveCommand();
				return;
			}
			if (_characterView!.IsGesturing)
				_characterView.CancelEmotion();

			_inputMoveValue = value;
		}

		private void OnSprint(bool value)
		{
			if (!User.Instance.Standby) return;
			if (!value) return;

			_isNeedSprintUpdate = true;
		}

		private void OnJump()
		{
			if (InputFieldExtensions.IsExistFocused) return;
			if (!User.Instance.Standby) return;
			if (!CanMove) return;
			if (_characterView.IsUnityNull()) return;
			CharacterState currentState = CharacterState.None;
			Animator       animator     = _characterView!.ObjAnimator;
			if (!animator.IsReferenceNull())
				currentState = (CharacterState)animator!.GetInteger(AnimationDefine.HashState);
			if (currentState is CharacterState.JumpStart or CharacterState.InAir) return;

			if (_jumpTimeoutTimer < _jumpTimeout)
			{
				if (_jumpTimeoutTimer >= _jumpTimeout / 2) // 연달아 눌렀을때 예약을 해버리면 의도치 않은 동작처럼 느껴짐
					_reservedJump = true;
				return;
			}

			if (_characterView.IsGesturing)
				_characterView.CancelEmotion();
			_reservedJump     = false;
			_jumpTimeoutTimer = -_delayedJumpTime;
			_currentMovementState!.MoveAction(false, true);
		}

		private void CheckReservedJump()
		{
			if (!_reservedJump) return;

			OnJump();
		}

		private void OnChat()
		{
			if (_minimapViewModel != null) return;
			if (!InputFieldExtensions.IsExistFocused && !_inputMoveValue.Equals(Vector2.zero))
			{
				_inputMoveValue = Vector2.zero;
				CommandMoveCheck();
			}
			ChatManager.Instance.OpenChatUI();
		}

		private void OnMinimap()
		{
			if (!User.Instance.Standby) return;
			if (_characterView.IsUnityNull()) return;
			if (InputFieldExtensions.IsExistFocused) return;
			if (CurrentScene.MapType is not eSpaceOptionMap.USE) return;
			if (MyPadManager.Instance.IsOpen) return;
			if (UIStackManager.Instance.Count >= 1 && !UIStackManager.Instance.TopMostViewName!.Equals(MinimapPopupPrefab)) return;

			if (_minimapViewModel == null)
				OnMiniMapOpen();
			else
				OnMinimapClose();
		}

		public void OnMiniMapOpen()
		{
			UIManager.Instance.CreatePopup(MinimapPopupPrefab, (guiView) =>
			{
				_minimapGUIView = guiView;
				guiView.Show();
				_minimapViewModel     =  guiView.ViewModelContainer.GetViewModel<MinimapPopupViewModel>();
				guiView.OnOpenedEvent += _ => _onMinimapUIOpened?.Invoke();
				guiView.OnClosedEvent += _ =>
				{
					_onMinimapUIClosed?.Invoke();
					if (!_characterView.IsUnityNull())
						_characterView!.Updated -= OnMyObjectUpdate;
				};
				WaitForMapDataLoading().Forget();
			}).Forget();
		}

		private void OnMyObjectUpdate(BaseMapObject baseObject)
		{
			if (_minimapViewModel == null) return;
			_minimapViewModel.MyPosition   = _characterView!.transform.position;
			_minimapViewModel.MyEulerAngle = _characterView.transform.eulerAngles;
		}

		private static readonly int DelayTimeout = 3000;
		private async UniTaskVoid WaitForMapDataLoading()
		{
			if (_minimapViewModel == null) return;
			var expireTime = MetaverseWatch.Realtime + DelayTimeout;
			while (_minimapViewModel.MapData == null || _characterView.IsUnityNull())
			{
				if (MetaverseWatch.Realtime > expireTime) break;
				await UniTask.Yield();
			}
			_minimapViewModel.SetWarpIcons();
			if (!_characterView.IsUnityNull())
				_characterView!.Updated += OnMyObjectUpdate;
		}

		public void OnMinimapClose()
		{
			if (!_minimapGUIView.IsUnityNull())
				_minimapGUIView!.Hide();
			_minimapGUIView   = null;
			_minimapViewModel = null;
		}

		public void ForceForwardMove(float distance = 0.5f)
		{
			if (_characterView.IsUnityNull()) return;
			var direction = _characterView!.transform.forward;
			_characterView.transform.position += direction * distance;
		}

		private void SendMessageMoveCommand()
		{
			_prevSentMoveValue = _inputMoveValue;
			_currentMovementState!.MoveCommand(_inputMoveValue);
		}

		private void SendMessageSprint()
		{
			if (InputFieldExtensions.IsExistFocused) return;
			if (!CanMove) return;
			if (_characterView.IsUnityNull()) return;

			if (_characterView!.IsGesturing)
				_characterView.CancelEmotion();

			_currentMovementState!.MoveAction(true, false);
		}
#endregion InputEvent

		public void SetEnableClickMove(bool isEnable)
		{
			_enableClickMove = isEnable;
		}

		public void SetWaypoints(IReadOnlyList<Vector3> waypoints)
		{
			_onChangeWaypoints?.Invoke(waypoints);
		}

		public void SetProgressWaypoints(float progress)
		{
			_onProgressWaypoints?.Invoke(progress);
		}

		public void SetNavigationMode(bool value)
		{
			if (!_characterView.IsUnityNull())
				_characterView!.IsNavigating = value;
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			_currentMovementState?.OnDrawGizmo();
		}

		private void FindMyCharacter()
		{
			if (_characterView.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "My CharacterView is null");
				return;
			}

			Selection.activeGameObject = CharacterView!.gameObject;
			SceneView.FrameLastActiveSceneView();
		}
#endif // UNITY_EDITOR
	}
}
