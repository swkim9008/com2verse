/*===============================================================
* Product:		Com2Verse
* File Name:	ActiveObject.cs
* Developer:	haminjeong
* Date:			2022-12-29 14:11
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Com2Verse.Avatar;
using Com2Verse.AvatarAnimation;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Pathfinder;
using Com2Verse.PlayerControl;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Network
{
	public partial class ActiveObject : MapObject
	{
		private static GroundCheckData _checkData;

#region BaseActiveObject
		protected event Action<long> OnObjectUpdateAfterFindPoints;

		private Animator _animator;
		public Animator ObjAnimator => _animator;
		public bool HasAnimator { get; private set; }

		private bool _hasAvatarUIAssetLoadRequest = false;
		private bool _hasAvatarUIAsset            = false;

		private AvatarCustomizeInfo _prevPartsInfo;

		private readonly List<BaseMapObject> _nearByCharacters = new();

		public int AnimatorID => ControlPoints[1].AnimatorID;
		public int EmotionState => ControlPoints[1].EmotionState;

		private float _avatarHeight = FallbackHeight;

		public override Transform GetUIRoot()
		{
			if (!_avatarController.IsUnityNull())
				UIHeight =  _avatarHeight;

			var uiRoot = base.GetUIRoot();
			if (_hasAvatarUIAsset || _hasAvatarUIAssetLoadRequest) return uiRoot;
			return uiRoot;
		}

		[SerializeField] private float _avatarBoundHeightMarginRatio = -0.02f;

		[Header("Movement")]
		[SerializeField] private float _snapToGroundThreshold = 0.15f;
		[SerializeField] private float _snapToGroundSmoothFactor = 3f;

		[SerializeField] private float _activeObjectPositionXZSmoothFactor = 1.5f;
		[SerializeField] private float _activeObjectPositionYSmoothFactor  = 4f;
		[SerializeField] private float _activeObjectRotationSmoothFactor   = 3.0f;

		protected override float PositionXZSmoothFactor => _activeObjectPositionXZSmoothFactor;
		protected override float PositionYSmoothFactor  => _activeObjectPositionYSmoothFactor;
		protected override float RotationSmoothFactor   => _activeObjectRotationSmoothFactor;

#region Math
		protected Vector3 GetTargetVelocity(long serverTime)
		{
			return MathUtil.LinearInterpolation(
				ControlPoints[0].Time,
				ControlPoints[0].Velocity,
				ControlPoints[1].Time,
				ControlPoints[1].Velocity,
				serverTime);
		}

		protected float GetTargetGravity(long serverTime)
		{
			return MathUtil.LinearInterpolation(
				ControlPoints[0].Time,
				ControlPoints[0].Gravity,
				ControlPoints[1].Time,
				ControlPoints[1].Gravity,
				serverTime);
		}
#endregion // Math

#endregion // BaseActiveObject

		private AvatarController _avatarController;
		public AvatarController AvatarController => _avatarController;

		private AvatarAnimatorController _animatorController;
		public AvatarAnimatorController AnimatorController => _animatorController;

		public Transform BaseBodyTransform { get; private set; }

		protected override void Awake()
		{
			base.Awake();
			_animator           = GetComponentInChildren<Animator>(true);
			HasAnimator         = !_animator.IsReferenceNull();
			_animatorController = GetComponentInChildren<AvatarAnimatorController>(true);

			BaseBodyTransform = transform.Find(MetaverseAvatarDefine.BaseBodyObjectName);
			if (BaseBodyTransform.IsUnityNull())
				C2VDebug.LogErrorCategory(GetType().Name, $"BaseBodyTransform is null. {name}");
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			OnObjectUpdateAfterFindPoints -= OnUpdateProcessEx;
		}

		protected override void OnObjectAwakeEx()
		{
			base.OnObjectAwakeEx();
			OnObjectUpdateAfterFindPoints += OnUpdateProcessEx;
		}

		private void OnUpdateProcessEx(long serverTime)
		{
			SetLatestAnimator();
			SetLatestAvatarParts();

			if (AnimatorController.NeedGestureEnd)
				CancelEmotion();

			if (!IsMine)
			{
				AnimatorController.SetAnimatorState(CharacterState, EmotionState);
				AnimatorController.SetLerpAnimatorParameters(ServerTime.DeltaTime(Time.unscaledDeltaTime), IsTurning, true);
			}
			else
			{
				AnimatorController.SetAnimatorState(-1, EmotionState);
				AnimatorController.SetLerpAnimatorParameters(ServerTime.DeltaTime(Time.unscaledDeltaTime), IsTurning, IsNavigating);
			}

			if (EmotionState > 0)
			{
				var update = UpdatedState;
				update.EmotionState = 0;
				UpdatedState        = update;
			}

			SetCharacterStateForMovement();

			SetAvatarHeight();
		}

		/// <inheritdoc />
		public override void Init(long serial, long ownerID, bool needUpdate)
		{
			base.Init(serial, ownerID, needUpdate);

			var clientPathFinding = ClientPathFinding.InstanceOrNull;
			if (IsMine && !clientPathFinding.IsReferenceNull())
				_isNavigating = !clientPathFinding!.enabled;

			if (!BaseBodyTransform.IsUnityNull())
			{
				_avatarController   = BaseBodyTransform!.gameObject.GetOrAddComponent<AvatarController>();
				_animatorController = _avatarController!.gameObject.GetOrAddComponent<AvatarAnimatorController>();

				if (IsMine)
					_animatorController!.OnGestureEnd += OnGestureEnd;

				_avatarController.SetActive(true);
			}

			FindRenderer();
			OnAvatarBodyChanged();
		}

		protected override void SetLayer()
		{
			gameObject.layer = (int)Define.eLayer.CHARACTER;
		}

		protected override void OnObjectEnabled(Transform parent)
		{
			base.OnObjectEnabled(parent);

			AvatarTarget = transform;

			if (!_avatarController.IsUnityNull() && !AnimatorController.IsUnityNull())
			{
				_avatarController!.IsMine = IsMine;

				AvatarTarget.SetParent(parent);
				AvatarTarget.gameObject.SetActive(true);

				// 모든 파츠 로딩되기전까지 비활성화한다
				if (_avatarController.IsCompletedLoadAvatarParts)
					_avatarController.SetActive(false);

				var avatarType = _avatarController.Info?.AvatarType ?? eAvatarType.NONE;
				AnimatorController!.OnObjectEnable(ObjectID, avatarType, IsMine);

				_avatarController.OnAvatarBodyChanged        += OnAvatarBodyChanged;
				_avatarController.OnCompletedLoadAvatarParts += OnCompletedLoadAvatarParts;

				if (IsMine)
				{
					_animatorController!.OnGestureEnd += OnGestureEnd;
					RegisterCheckNearByObject();
				}
			}

			SetAvatarHeight();
			SetDefaultAnimationParameter();
			AddAvatarStateChangeEvent();
			AddAvatarMovementDataChangeEvent();
		}

		private void OnAvatarBodyChanged()
		{
			if (_avatarController.IsUnityNull())
				return;

			var meshBoundHeight = _avatarController!.GetCombinedSkinnedMeshHeight();
			_avatarHeight = meshBoundHeight - meshBoundHeight * _avatarBoundHeightMarginRatio;
		}

		private void OnCompletedLoadAvatarParts()
		{
			// 아바타가 풀로 들어갔는지 체크
			if (_avatarController.IsReferenceNull())
				return;

			var metaverseCamera = CameraManager.Instance.MetaverseCamera;
			if (!metaverseCamera.IsUnityNull())
			{
				HudCullingGroup = metaverseCamera!.GetOrAddCullingGroupProxy(eCullingGroupType.HUD);
				if (!HudCullingGroup.IsContained(this))
					HudCullingGroup.Add(this);
			}
		}

		protected override bool CanProcessUpdate()
		{
			if (!base.CanProcessUpdate()) return false;
			if (!HasAnimator || ObjAnimator.runtimeAnimatorController.IsReferenceNull()) return false;
			if (AnimatorController.IsReferenceNull()) return false;

			return true;
		}

		protected override void OnObjectUpdated()
		{
			Vector3 prevPosition = transform.position;

			base.OnObjectUpdated();
			// SnapToGroundOnUpdate(prevPosition);

			if (!IsMine)
			{
				var gravity   = prevPosition.y - transform.position.y;
				var realState = (Protocols.CharacterState)ObjAnimator.GetInteger(AnimationDefine.HashState);
				if (CharacterState == -1 && realState is Protocols.CharacterState.InAir && Mathf.Approximately(gravity, 0.0f))
				{
					var update = UpdatedState;
					update.CharacterState = (int)Protocols.CharacterState.IdleWalkRun;
					UpdatedState          = update;
					C2VDebug.LogWarningCategory("ActiveObject", "forced change state to idle");
				}
			}

			long serverTime = Math.Min(ServerTime.Time, ControlPoints[1].Time + ServerTime.SyncInterval * 2);

			OnObjectUpdateAfterFindPoints?.Invoke(serverTime);

			transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

			if (!IsMine && !AnimatorController.IsUnityNull())
			{
				var prevPositionXZ    = new Vector3(prevPosition.x,       0, prevPosition.z);
				var currentPositionXZ = new Vector3(transform.position.x, 0, transform.position.z);

				AnimatorController!.TargetVelocity       = (prevPositionXZ - currentPositionXZ).magnitude / Time.deltaTime;
				AnimatorController.TargetFallingDistance = GetTargetGravity(serverTime);
			}
		}

		protected override void OnObjectReleased()
		{
			_avatarHeight = FallbackHeight;

			if (ObjAnimator.playableGraph.IsValid())
				ObjAnimator.playableGraph.Destroy();

			var meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
			foreach (var smr in meshRenderers)
				smr.enabled = true;

			if (!_avatarController.IsUnityNull())
			{
				_avatarController!.OnAvatarBodyChanged       -= OnAvatarBodyChanged;
				_avatarController.OnCompletedLoadAvatarParts -= OnCompletedLoadAvatarParts;

				_avatarController.IsMine = false;
			}

			if (!_animatorController.IsUnityNull())
			{
				if (IsMine)
				{
					_animatorController!.OnGestureEnd -= OnGestureEnd;
					UnregisterCheckNearByObject();
				}

				_animatorController!.DisposeGestureToken();
				_animatorController.OnRelease();
			}

			SetDefaultAnimationParameter();

			RemoveAvatarStateChangeEvent();
			RemoveAvatarMovementDataChangeEvent();

			base.OnObjectReleased();

			_avatarController = null;
		}

		private void AddAvatarStateChangeEvent()
		{
			if (AnimatorController.IsUnityNull()) return;

			AnimatorController!.OnAvatarStateChange += OnAvatarStateChange;
			AnimatorController.OnSetEmotion         += OnSetEmotion;
		}

		private void RemoveAvatarStateChangeEvent()
		{
			if (AnimatorController.IsUnityNull()) return;

			AnimatorController!.OnAvatarStateChange -= OnAvatarStateChange;
			AnimatorController.OnSetEmotion         -= OnSetEmotion;
		}

		private void AddAvatarMovementDataChangeEvent()
		{
			var playerController = PlayerController.InstanceOrNull;
			if (playerController.IsUnityNull()) return;

			playerController!.OnMoveDataChanged += OnMovementDataChange;

			SetAnimationParameterSyncData(playerController.AnimationParameterSyncData);
		}

		private void RemoveAvatarMovementDataChangeEvent()
		{
			var playerController = PlayerController.InstanceOrNull;
			if (playerController.IsUnityNull()) return;
			playerController!.OnMoveDataChanged -= OnMovementDataChange;
		}

		private CancellationTokenSource _checkNearByObjectCts;

		private float _checkNearByObjectInterval = 200f;

		private void RegisterCheckNearByObject()
		{
			UnregisterCheckNearByObject();
			_checkNearByObjectCts = new CancellationTokenSource();
			CheckNearByObject().Forget();
		}

		private void UnregisterCheckNearByObject()
		{
			_checkNearByObjectCts?.Cancel();
			_checkNearByObjectCts?.Dispose();
			_checkNearByObjectCts = null;
		}

		private async UniTask CheckNearByObject()
		{
			while (_checkNearByObjectCts != null && !_checkNearByObjectCts.IsCancellationRequested)
			{
				await UniTask.Delay(TimeSpan.FromMilliseconds(_checkNearByObjectInterval), cancellationToken: _checkNearByObjectCts.Token);
				var nearByObjects = GetNearByCharacters(true);
				if (nearByObjects == null || nearByObjects.Count == 0) continue;

				var cameraManager       = CameraManager.Instance;
				var hudCullingDistances = cameraManager.GetCullingGroupBoundingDistance(eCullingGroupType.HUD);
				if (hudCullingDistances == null || hudCullingDistances.Length == 0) 
					continue;

				var ignoreDistance = hudCullingDistances[^1];
				for (int i = 0; i < nearByObjects.Count; ++i)
				{
					if (nearByObjects[i] is not ActiveObject activeObject)
						continue;

					if (activeObject._animatorController.IsUnityNull())
						continue;

					activeObject._animatorController!.SetDistance(activeObject.DistanceFromMe, i, ignoreDistance);
				}
			}
		}

#region NearByObject
		public ReadOnlyCollection<BaseMapObject> GetNearByCharacters(bool isSorted = false)
		{
			_nearByCharacters!.Clear();

			var activeObjects = MapController.Instance.ActiveObjects;
			if (activeObjects == null)
				return _nearByCharacters.AsReadOnly();

			foreach (var activeObject in activeObjects.Values)
				if (!activeObject.IsReferenceNull()) _nearByCharacters.Add(activeObject);

			if (isSorted)
				_nearByCharacters.Sort(SortDistanceComparision);

			return _nearByCharacters.AsReadOnly();
		}

		private int SortDistanceComparision([NotNull] BaseMapObject a, [NotNull] BaseMapObject b)
		{
			var position  = transform.position;
			var distanceA = Vector3.SqrMagnitude(position - a.transform.position);
			var distanceB = Vector3.SqrMagnitude(position - b.transform.position);
			return distanceA.CompareTo(distanceB);
		}
#endregion NearByObject

		/// <summary>
		/// 자신의 캐릭터뿐만 아니라 씬 내의 모든 캐릭터의 데이터의 변경이 필요한 경우 해당 이벤트에서 데이터를 적용합니다.
		/// </summary>
		/// <param name="movementData"></param>
		private void OnMovementDataChange(IMovementData movementData)
		{
			if (movementData is AnimationParameterSyncData animationParameterSyncData)
				SetAnimationParameterSyncData(animationParameterSyncData);
		}

		private void SetAnimationParameterSyncData(AnimationParameterSyncData data)
		{
			AnimatorController.ParameterSyncFilter.SetData(data.IsEnable, data.ThrottleFrames, data.ChangedFrame, data.DistinctUntilChangedTarget, data.MovingThresholdVelocity,
			                                               data.RunThresholdVelocity, data.VelocitySmoothFactor, data.AvatarWalkSpeed, data.AvatarRunSpeed);
		}

		private void OnGestureEnd(Emotion emotion, bool isCanceled)
		{
			if (!isCanceled)
			{
				if (User.Instance.CharacterObject.IsUnityNull()) return;

				Commander.Instance.SetEmotion(0, User.Instance.CharacterObject.ObjectID);
			}
		}

		private void SnapToGroundOnUpdate(Vector3 prevPosition)
		{
			var needSnapToGround = IsMine && IsNavigating;

			var deltaPosition = transform.position - prevPosition;
			needSnapToGround |= !IsMine && deltaPosition.magnitude > PlayerController.MovingThresholdVelocityOnNavigation;

			if (!needSnapToGround)
				return;

			SnapToGround(true);
		}

		private void SnapToGround(bool isSmooth)
		{
			var distanceToGround = GetDistanceToGround();
			if (distanceToGround > 0.0f && distanceToGround < _snapToGroundThreshold)
			{
				var position = transform.position;

				if (isSmooth)
					position.y = Mathf.Lerp(position.y, position.y - distanceToGround, _snapToGroundSmoothFactor * Time.deltaTime);
				else
					position.y -= distanceToGround;

				transform.position = position;
			}
		}

		private float GetDistanceToGround()
		{
			if (transform.IsUnityNull()) return 0.0f;

			_checkData ??= PlayerController.Instance.GroundCheckData;
			var pos = transform.position;
			var ray = new Ray(pos, Vector3.down);
			var hit = Physics.Raycast(ray, out var hitInfo, _checkData.MaxDistance, ~_checkData.IgnoreLayerMask);
			return hit ? hitInfo.distance : 0.0f;
		}

		public void ForceSetCurrentCharacterStateToState()
		{
			ForceSetCharacterStateToState((int)AvatarController.AvatarAnimatorController.CurrentCharacterState);
		}

		public bool CanMove()
		{
			return _conferenceObjectType != eConferenceObjectType.LISTENER;
		}

		public bool CanEmotion()
		{
			return true;
		}
	}
}
