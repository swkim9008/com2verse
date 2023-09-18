/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarAnimatorController.cs
* Developer:	eugene9721
* Date:			2022-12-27 18:46
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Linq;
using System.Threading;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.LruObjectPool;
using Com2Verse.Project.Animation;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using Protocols;
using UnityEngine;

namespace Com2Verse.AvatarAnimation
{
	[RequireComponent(typeof(Animator))]
	public sealed class AvatarAnimatorController : MonoBehaviour, IEventCommand
	{
#region static / constant
		public static int TargetMixerGroup;

		private static float _movingThresholdVelocity = 1.0f; // Moving 상태가 되는 속도 값
		private static float _runThresholdVelocity    = 5.2f; // Run상태가 되는 속도 값

		private readonly AnimationSoundController _animationSoundController = new();
#endregion static / constant

#region Action / Event
		private Action<CharacterState, bool>? _onAvatarStateChange;

		/// <summary>
		/// 아바타의 상태가 변경시 호출되는 이벤트
		/// 첫번쨰 인자: 변경되는 아바타의 상태
		/// 두번쨰 인자: 상태가 활성화된 경우 true, 비활성화되는 경우 false 
		/// </summary>
		public event Action<CharacterState, bool> OnAvatarStateChange
		{
			add
			{
				_onAvatarStateChange -= value;
				_onAvatarStateChange += value;
			}
			remove => _onAvatarStateChange -= value;
		}

		private Action<Emotion, int>? _onSetEmotion;

		public event Action<Emotion, int> OnSetEmotion
		{
			add
			{
				_onSetEmotion -= value;
				_onSetEmotion += value;
			}
			remove => _onSetEmotion -= value;
		}

		private Action? _onJumpReadyEnd;

		private Action<eAnimationState>? _onSetAnimationState;

		public event Action<eAnimationState> OnSetAnimationState
		{
			add
			{
				_onSetAnimationState -= value;
				_onSetAnimationState += value;
			}
			remove => _onSetAnimationState -= value;
		}

		private Action<Emotion, bool>? _onGestureEnd;

		/// <summary>
		/// 제스쳐가 끝난 경우 실행되는 액션
		/// 첫번째 인자: 실행되었던 제스쳐 종류
		/// 두번째 인자: 캔슬되었는지 여부
		/// </summary>
		public event Action<Emotion, bool> OnGestureEnd
		{
			add
			{
				_onGestureEnd -= value;
				_onGestureEnd += value;
			}
			remove => _onGestureEnd -= value;
		}
#endregion Action / Event

#region SerializeFileds
		[SerializeField] [ReadOnly]
		private float _currentAnimationVelocity;

		[SerializeField] [ReadOnly]
		private float _currentFallingDistance;

		[SerializeField] [ReadOnly]
		private CharacterState _currentRealState = CharacterState.None;

		[SerializeField] [ReadOnly]
		private eAnimationState _currentAnimationState;
		private eAnimationState _currentSpeedState;

		[field: SerializeField] [field: ReadOnly]
		public CharacterState JumpPrevState { get; private set; }

		public int GestureType => _currentGesture?.ID ?? -1;

		[SerializeField]
		private float _velocitySmoothFactorByClient = 10f;

		[SerializeField]
		private float _velocitySmoothFactorByServer = 2f;

		[SerializeField]
		private float _velocityFactorInTurning = 0.85f;

		[SerializeField]
		private float _gravitySmoothFactor = 12f;
#endregion SerializeFileds

#region Fields
		private Transform? _bip001HeadBone;
		private Transform? _fxEmitter;

		public Transform? HeadBone  => _bip001HeadBone;
		public Transform? FxEmitter => _fxEmitter;

		private Animator _animator = null!;
		private string   _prevPlayGestureFX = string.Empty;

		private AvatarAnimator? _currentAnimatorData;
		private Emotion?        _currentGesture;

		private CancellationTokenSource? _gestureCancellationTokenSource;

		private int _prevAnimatorID     = -1;
		private int _prevCharacterState = -1;
		private int _prevEmotionState   = -1;

		private float _targetVelocity;
		private float _targetFallingDistance;

		private bool _isInitialized;

		private bool _isPlayedJumpReady;

		private float _avatarWalkSpeed = 4.5f;
		private float _avatarRunSpeed  = 5.2f;

		[SerializeField]
		private ParameterSyncFilter _parameterSyncFilter = new();
		private AnimationMontage    _animationMontage    = new();

		[SerializeField] [ReadOnly]
		private bool _isOnHandsUpState;

		private long _objectId;
#endregion Fields

#region Properties
		public CharacterState CurrentCharacterState => (CharacterState)_prevCharacterState;

		public bool IsGesturing => _gestureCancellationTokenSource != null;

		// TODO: 나중에 이동 중에도 가능한 제스쳐가 추가된다면 수정 필요
		public bool NeedGestureEnd =>
			(Mathf.Abs(_targetVelocity) > _movingThresholdVelocity || _targetFallingDistance > _movingThresholdVelocity) &&
			IsGesturing;

		public eAnimationState CurrentAnimationState => _currentAnimationState;

		public float TargetVelocity
		{
			get => _targetVelocity;
			set => _targetVelocity = value;
		}

		public float TargetFallingDistance
		{
			get => _targetFallingDistance;
			set => _targetFallingDistance = value;
		}

		public eAvatarType AvatarType { get; set; }

		public bool IsMine { get; set; }

		public bool IsOnSpecialIdleAnimation { get; private set; }

		public bool IsOnHandsUpState
		{
			get => _isOnHandsUpState;
			set
			{
				if (value)
				{
					_isOnHandsUpState = true;
					OnSetAnimationState += OnSetAnimationStateWhenHandsUp;
					OnGestureEnd        += OnGestureEndWhenHandsUp;
					if (_currentGesture == null) OnSetAnimationStateWhenHandsUp(_currentAnimationState);
				}
				else
				{
					OnSetAnimationState -= OnSetAnimationStateWhenHandsUp;
					OnGestureEnd        -= OnGestureEndWhenHandsUp;
					PlaySpecialIdleAnimation(eIdleState.HAND_UP_OFF);
					_isOnHandsUpState = false;
				}
			}
		}

		public float CurrentAnimationVelocity => _currentAnimationVelocity;

		public ParameterSyncFilter ParameterSyncFilter => _parameterSyncFilter;
#endregion Properties

#region Initialize
		// public void Awake()
		// {
		// 	Initialize();
		// }

		private void FindBones()
		{
			_bip001HeadBone = transform.FindRecursive(AnimationDefine.HeadBoneName);
			if (_bip001HeadBone.IsReferenceNull())
				C2VDebug.LogErrorCategory(GetType().Name, "Bip001 Head is null");

			_fxEmitter = transform.FindRecursive(AnimationDefine.FxEmitterBoneName);
			if (_fxEmitter.IsReferenceNull())
				C2VDebug.LogErrorCategory(GetType().Name, "Fx Emitter is null");
		}

		public void Initialize()
		{
			if (_isInitialized) return;
			_isInitialized = true;
			_animator      = GetComponent<Animator>()!;
			_animationSoundController.InitializeAudioComponent(TargetMixerGroup, this.gameObject);
			FindBones();

			var animationManager  = AnimationManager.Instance;
			var avatarControlData = animationManager.AvatarControlData;
			if (avatarControlData != null)
			{
				_avatarWalkSpeed = avatarControlData.Speed;
				_avatarRunSpeed  = avatarControlData.SprintSpeed;
			}
		}

		private void Update()
		{
			_animationSoundController.OnUpdate();
		}
#endregion Initialize

#region Enable
		public void OnObjectEnable(long objectId, eAvatarType avatarType = eAvatarType.NONE, bool isMine = false)
		{
			_objectId  = objectId;
			AvatarType = avatarType;
			IsMine     = isMine;

			_animationMontage.Initialize(name, avatarType, _animator);
			_animationSoundController.Initialize(this, avatarType);
		}

		public void OnRelease()
		{
			IsMine = false;
			_animationSoundController.Clear();
			DisposeGestureToken();
			_prevPlayGestureFX = string.Empty;
			_onAvatarStateChange = null;

			SetDefaultAnimationParameter();
			_animationMontage.OnRelease();

			_currentRealState      = CharacterState.None;
			_currentAnimationState = eAnimationState.STAND;

			IsOnHandsUpState = false;
		}

		private void SetDefaultAnimationParameter()
		{
			_currentAnimatorData = null;
			_prevCharacterState  = -1;
			_prevEmotionState    = -1;
			_prevAnimatorID      = -1;
		}
#endregion Enable

#region Animator
		public async UniTask<RuntimeAnimatorController?> LoadAnimatorAsync(int animatorId, Action<RuntimeAnimatorController>? onComplete = null, CancellationTokenSource? cancellationTokenSource = null)
		{
			// 중복으로 생성이 올수 있기에 같은걸 에러 메세지 출력만 하여 노티함
			if (animatorId == _prevAnimatorID)
				C2VDebug.LogWarningCategory((nameof(AvatarAnimatorController)), "이미 생성되어 있는데 다시 한번 생성이 옴");

			_prevAnimatorID = animatorId;

			var animationManager = AnimationManager.Instance;
			_currentAnimatorData = animationManager.GetAnimatorData(animatorId);

			var animatorAsset = await AnimationManager.Instance.LoadRuntimeAnimatorAsync(_currentAnimatorData.AnimatorAssetName, cancellationTokenSource);

			_animator.runtimeAnimatorController = animatorAsset;
			_animationMontage.Initialize($"PC_{name}_{_objectId}", AvatarType, _animator);

			onComplete?.Invoke(animatorAsset);
			return animatorAsset;
		}

		public bool TrySetFloat(int id, float value)
		{
			if (!gameObject.activeSelf) return false;
			_animator.SetFloat(id, value);
			return true;
		}

		public bool TrySetBool(int id, bool value)
		{
			if (!gameObject.activeSelf) return false;
			_animator.SetBool(id, value);
			return true;
		}

		public bool TrySetTrigger(int id)
		{
			if (!gameObject.activeSelf) return false;
			_animator.SetTrigger(id);
			return true;
		}

		public bool TrySetInteger(int id, int value)
		{
			if (!gameObject.activeSelf) return false;
			_animator.SetInteger(id, value);
			return true;
		}

		public void ApplyRootMotion(bool isApply)
		{
			_animator.applyRootMotion = isApply;
		}
#endregion Animator

#region SetAnimatorParameters
		public void SetAnimatorState(int characterState, int gestureState)
		{
			if (characterState == 6)
				characterState = _prevCharacterState;

			var prevState = _currentRealState;
			var currentRealState = _animator.GetInteger(AnimationDefine.HashState);
			_currentRealState = (CharacterState)currentRealState;

			if (_currentRealState == CharacterState.None)
				_currentRealState = CharacterState.IdleWalkRun;

			if (prevState != CharacterState.JumpStart && _currentRealState == CharacterState.JumpStart)
				JumpPrevState = prevState;

			SetAnimatorState(characterState);
			CheckPlayGesture();

			if (characterState != -1)
			{
				if (characterState != currentRealState)
				{
					_animator.SetInteger(AnimationDefine.HashState, characterState);
				}
				if (_prevCharacterState != characterState)
				{
					AvatarStateChange(_prevCharacterState, characterState);
					_prevCharacterState = characterState;
				}
			}
			else if (_prevCharacterState != -1)
			{
				if (_prevCharacterState != currentRealState)
				{
					_animator.SetInteger(AnimationDefine.HashState, _prevCharacterState);
				}
			}

			if (gestureState != -1 && _prevEmotionState != gestureState)
			{
				TrySetEmotion(gestureState);
				_prevEmotionState = gestureState;
			}
		}

		private void SetOtherAvatarAnimation(eAnimationState animationState)
		{
			switch (animationState)
			{
				case eAnimationState.SIT:
					_animator.ResetTrigger(AnimationDefine.HashSetWait);
					break;
				case eAnimationState.JUMP:
					_animator.CrossFadeInFixedTime(AnimationDefine.HashJumpUp, AnimationDefine.DefaultCrossFadeDuration, AnimationDefine.BaseLayerIndex);
					break;
			}
		}

		public void SetLerpAnimatorParameters(float deltaTime, bool isTurning = false, bool applySyncFilter = false)
		{
			if (applySyncFilter)
				_targetVelocity = _parameterSyncFilter.GetSyncVelocity(_targetVelocity, deltaTime);

			if (_currentAnimationVelocity > _targetVelocity && Mathf.Approximately(_targetVelocity, 0f))
				_currentAnimationVelocity = Mathf.Lerp(_currentAnimationVelocity, _targetVelocity, deltaTime * (IsMine ? _velocitySmoothFactorByClient : _velocitySmoothFactorByServer));
			else
			{
				if (isTurning)
					_targetVelocity *= _velocityFactorInTurning;
				_currentAnimationVelocity = Mathf.Lerp(_currentAnimationVelocity, _targetVelocity, deltaTime * (IsMine ? _velocitySmoothFactorByClient : _velocitySmoothFactorByServer));
			}
			_currentFallingDistance   = Mathf.Lerp(_currentFallingDistance, _targetFallingDistance, deltaTime * _gravitySmoothFactor);

			if (!gameObject.activeSelf) return;

			_animator.SetFloat(AnimationDefine.HashSpeed, Mathf.Min(_currentAnimationVelocity, _avatarRunSpeed));
			_animator.SetFloat(AnimationDefine.HashFallingDistance, _currentFallingDistance);
		}
#endregion SetAnimatorParameters

#region AvatarAnimatorStateEvent
		private void SetAnimatorState(int characterState)
		{
			if (characterState == -1)
			{
				if (_prevCharacterState == (int)CharacterState.IdleWalkRun)
					characterState = _prevCharacterState;
				else return;
			}

			var prevAnimationState = _currentAnimationState;

			SetSpeedState();
			switch ((CharacterState)characterState)
			{
				case CharacterState.IdleWalkRun:
					if (_isPlayedJumpReady)
						_currentAnimationState = eAnimationState.JUMP;
					else
						_currentAnimationState = _currentSpeedState;
					break;
				case CharacterState.JumpStart:
				case CharacterState.InAir:
				case CharacterState.JumpLand:
					_currentAnimationState = eAnimationState.JUMP;
					break;
				case CharacterState.Sit:
					_currentAnimationState = eAnimationState.SIT;
					break;
			}

			if (prevAnimationState != _currentAnimationState)
				_onSetAnimationState?.Invoke(_currentAnimationState);

			if (!IsMine && prevAnimationState != _currentAnimationState)
				SetOtherAvatarAnimation(_currentAnimationState);
		}

		private void SetSpeedState()
		{
			var movementSpeed = _animator.GetFloat(AnimationDefine.HashSpeed);
			if (movementSpeed < _movingThresholdVelocity)
				_currentSpeedState = eAnimationState.STAND;
			else if (movementSpeed < _runThresholdVelocity)
				_currentSpeedState = eAnimationState.WALK;
			else
				_currentSpeedState = eAnimationState.RUN;
		}

		private void CheckPlayGesture()
		{
			if (_currentGesture == null) return;

			if (!CanPlayEmotion(_currentGesture))
				SetGestureEnd();
		}

		private void AvatarStateChange(int prevState, int newState)
		{
			AvatarStateChangeAtPrevState((CharacterState)prevState);
			AvatarStateChangeAtNewState((CharacterState)newState);
		}

		private void AvatarStateChangeAtPrevState(CharacterState prevState)
		{
			_onAvatarStateChange?.Invoke(prevState, false);
		}

		private void AvatarStateChangeAtNewState(CharacterState newState)
		{
			_onAvatarStateChange?.Invoke(newState, true);
			if (newState == CharacterState.JumpStart)
			{
				if (!_isPlayedJumpReady)
					_animator.CrossFadeInFixedTime(AnimationDefine.HashJumpUp, AnimationDefine.DefaultCrossFadeDuration, AnimationDefine.BaseLayerIndex);

				_isPlayedJumpReady = false;
			}
		}

		public void SetJumpReady(Action onJumpReadyEnd)
		{
			_onJumpReadyEnd = onJumpReadyEnd;
			_animator.CrossFadeInFixedTime(AnimationDefine.HashJumpReady, AnimationDefine.DefaultCrossFadeDuration, AnimationDefine.BaseLayerIndex);
			_isPlayedJumpReady = true;
		}

		public void OnJumpReadyEnd()
		{
			_onJumpReadyEnd?.Invoke();
			_onJumpReadyEnd = null;

			TrySetInteger(AnimationDefine.HashState, (int)CharacterState.JumpStart);
		}
#endregion AvatarAnimatorStateEvent

#region Emotion
		private void TrySetEmotion(int emotionState)
		{
			if (emotionState == -1 || emotionState == GestureType) return;

			if (!AnimationManager.Instance.TableEmotion!.Datas!.TryGetValue(emotionState, out var emotion)) return;

			if (CanPlayEmotion(emotion)) SetEmotion(emotion!, emotionState);
		}

		private bool CanPlayEmotion(Emotion? emotion)
		{
			if (emotion == null) return false;

			switch (_currentAnimationState)
			{
				case eAnimationState.STAND:
					if (emotion.Stand) return true;

					break;
				case eAnimationState.SIT:
					if (emotion.Sit) return true;

					break;
				case eAnimationState.WALK:
					if (emotion.Walk) return true;

					break;
				case eAnimationState.RUN:
					if (emotion.Run) return true;

					break;
				case eAnimationState.JUMP:
					if (emotion.Jump) return true;

					break;
				default:
					return false;
			}
			return false;
		}

		private void SetEmotion(Emotion emotion, int emotionState)
		{
			if (emotion.EmotionType == eEmotionType.GESTURE)
				PlayGesture(emotion);

			_onSetEmotion?.Invoke(emotion, emotionState);
		}

		public void SetGestureEnd(bool isCanceled = false)
		{
			if (_currentAnimatorData == null)
			{
				C2VDebug.LogWarningCategory(nameof(AvatarAnimatorController), "Animator Data is null!");
				return;
			}

			if (!isCanceled) DisposeGestureToken();

			switch (_currentAnimatorData.AnimatorType)
			{
				case eAnimatorType.WORLD:
					if (_currentGesture == null) return;
					_currentGesture = null;
					break;
				case eAnimatorType.AVATAR_CUSTOMIZE:
					break;
				default:
					return;
			}
		}

		public void PlayGesture(int animationId)
		{
			var emotionId = GetEmotionIdFromSortOrder(animationId, AvatarType);
			if (emotionId != 0)
			{
				TrySetEmotion(emotionId);
				return;
			}

			_animator.SetTrigger(AnimationDefine.HashEmotionEnd);
			_animator.SetInteger(AnimationDefine.HashEmotion, animationId);
		}

		public async UniTask PlayGestureAsync(AnimationClip animationClip)
		{
			DisposeGestureToken();
			_gestureCancellationTokenSource ??= new CancellationTokenSource();
			var cancellationTokenSource = _gestureCancellationTokenSource;

			var isFullBody = _currentRealState == CharacterState.IdleWalkRun && _currentAnimationState == eAnimationState.STAND;
			var isCanceled = await _animationMontage.PlayClip(animationClip, cancellationTokenSource, isFullBody, !isFullBody, !isFullBody);
			SetGestureEnd(isCanceled);
			PlaySpecialIdleAnimation(eIdleState.NORMAL);
		}

		private void PlayGesture(Emotion emotion)
		{
			DisposeGestureToken();
			_gestureCancellationTokenSource ??= new CancellationTokenSource();
			PlayGesture(emotion, _gestureCancellationTokenSource).Forget();
			PlaySpecialIdleAnimation(eIdleState.NORMAL);
		}

		private async UniTask PlayGesture(Emotion emotion, CancellationTokenSource cancellationTokenSource)
		{
			await UniTask.NextFrame();
			if (!CanPlayEmotion(emotion) || string.IsNullOrWhiteSpace(emotion.ResName!))
			{
				SetGestureEnd();
				return;
			}

			// var clipHandle = C2VAddressables.LoadAssetAsync<AnimationClip>($"{emotion.ResName}_anim.anim");
			// if (clipHandle == null)
			// {
			// 	C2VDebug.LogErrorCategory(GetType().Name, "clipHandle is null!");
			// 	SetGestureEnd();
			// 	return;
			// }

			// var loadedAsset = await clipHandle.ToUniTask();
			var loadedAsset = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<AnimationClip>($"{emotion.ResName}_anim.anim");
			if (loadedAsset.IsReferenceNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "AnimationClip is null!");
				SetGestureEnd();
				return;
			}

			if (cancellationTokenSource.IsCancellationRequested)
			{
				SetGestureEnd(true);
				_onGestureEnd?.Invoke(emotion, true);
				return;
			}

			_currentGesture = emotion;

			if (!string.IsNullOrEmpty(emotion.SoundName!))
				_animationSoundController.PlayGesture($"{emotion.SoundName}.wav");
			if (!string.IsNullOrEmpty(emotion.FxName!))
				PlayFX(emotion.FxName);

			var isFullBody = _currentRealState == CharacterState.IdleWalkRun && _currentAnimationState == eAnimationState.STAND;
			var isCanceled = await _animationMontage.PlayClip(loadedAsset, cancellationTokenSource, isFullBody, !isFullBody, !isFullBody);
			SetGestureEnd(isCanceled);
			_onGestureEnd?.Invoke(emotion, false);
		}

		public void DisposeGestureToken()
		{
			if (!string.IsNullOrEmpty(_prevPlayGestureFX))
				_prevPlayGestureFX = string.Empty;

			_animationSoundController.StopGestureAudio();

			if (_gestureCancellationTokenSource == null)
				return;

			_gestureCancellationTokenSource?.Cancel();
			_gestureCancellationTokenSource?.Dispose();
			_gestureCancellationTokenSource = null;
		}

		/// <summary>
		/// sortOrder와 avatarType을 이용하여 emotionId를 가져온다.
		/// 성능상 여유가 있는 상황에서만 사용할 것을 추천
		/// </summary>
		/// <param name="sortOrder">제스쳐 리스트에서 해당 UI가 표시된 순서</param>
		/// <param name="avatarType">현재 아바타 타입</param>
		/// <returns></returns>
		private int GetEmotionIdFromSortOrder(int sortOrder, eAvatarType avatarType)
		{
			var emotionTable = AnimationManager.Instance.TableEmotion;
			var emotionData  = emotionTable?.Datas!.Values.FirstOrDefault((data) => data.SortOrder == sortOrder && data.AvatarType == avatarType);
			if (emotionData == null) return 0;

			return emotionData.ID;
		}
#endregion Emotion

#region Idle
		private void OnSetAnimationStateWhenHandsUp(eAnimationState state)
		{
			switch (state)
			{
				case eAnimationState.STAND:
				case eAnimationState.SIT:
					PlaySpecialIdleAnimation(eIdleState.HAND_UP_ON);
					break;
				case eAnimationState.WALK:
				case eAnimationState.RUN:
				case eAnimationState.JUMP:
					PlaySpecialIdleAnimation(eIdleState.HAND_UP_OFF);
					break;
			}
		}

		private void PlaySpecialIdleAnimation(eIdleState idleState)
		{
			_isOnHandsUpState = true;
			IsOnSpecialIdleAnimation = true;

			if (_currentAnimatorData is not { AnimatorType: eAnimatorType.WORLD })
				return;

			switch (idleState)
			{
				case eIdleState.NORMAL:
					_animator.SetBool(AnimationDefine.HashHandsUpIdle, false);
					break;
				case eIdleState.HAND_UP_ON:
					_animator.SetBool(AnimationDefine.HashHandsUpIdle, true);
					break;
				case eIdleState.HAND_UP_OFF:
					_animator.SetBool(AnimationDefine.HashHandsUpIdle, false);
					break;
			}
		}

		private void OnGestureEndWhenHandsUp(Emotion emotion, bool isCanceled)
		{
			OnSetAnimationStateWhenHandsUp(_currentAnimationState);
		}
#endregion Idle

#region Animation Event Command
		public void PlayFX(string assetName)
		{
			if (_prevPlayGestureFX == assetName) return;
			_prevPlayGestureFX = assetName;

			_gestureCancellationTokenSource ??= new CancellationTokenSource();

			var animationManager = AnimationManager.InstanceOrNull;
			if (animationManager.IsReferenceNull()) return;
			animationManager!.PlayAnimationFx(this, assetName, _gestureCancellationTokenSource);
		}

		public void PlaySound(string soundType)
		{
			// blend Tree를 사용하는 경우 사운드가 중복재생 되지 않도록 주의
			switch (soundType)
			{
				case "walk":
					if (CurrentAnimationState != eAnimationState.WALK) return;

					_animationSoundController.PlaySound(AnimationSoundController.eSoundType.WALK);
					break;
				case "run":
					if (CurrentAnimationState != eAnimationState.RUN) return;

					_animationSoundController.PlaySound(AnimationSoundController.eSoundType.RUN);
					break;
				case "jumpEnd":
					if (_currentSpeedState != eAnimationState.STAND) return;

					_animationSoundController.PlaySound(AnimationSoundController.eSoundType.LAND_LOW);
					break;
				case "landWalk":
					if (_currentSpeedState != eAnimationState.WALK) return;

					_animationSoundController.PlaySound(AnimationSoundController.eSoundType.LAND_LOW);
					break;
				case "landRun":
					if (_currentSpeedState != eAnimationState.RUN) return;

					_animationSoundController.PlaySound(AnimationSoundController.eSoundType.LAND_LOW);
					break;
				case "jump":
					_animationSoundController.PlaySound(AnimationSoundController.eSoundType.JUMP);
					break;
				case "landFall":
					_animationSoundController.PlaySound(AnimationSoundController.eSoundType.LAND_HIGH);
					break;
				default:
					// TODO: 파라메터 추가해서 제스쳐/일반 사운드 구분
					_animationSoundController.PlayGesture(soundType);
					break;
			}
		}

		public void SetDistance(float distance, int ranking, float ignoreDistance)
		{
			_animationSoundController.SetDampeningFromDistance(Mathf.Lerp(1, 0, Mathf.Min(distance / ignoreDistance, 1f)), ranking);
		}
#endregion Animation Event Command
	}
}
