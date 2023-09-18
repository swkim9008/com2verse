/*===============================================================
* Product:		Com2Verse
* File Name:	PlayerMovementWithClientBase.cs
* Developer:	eugene9721
* Date:			2023-03-13 14:24
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Runtime.CompilerServices;
using Com2Verse.AvatarAnimation;
using Com2Verse.CameraSystem;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.UI;
using Com2Verse.Utils;
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
	public abstract class PlayerMovementWithClientBase : PlayerMovementBase
	{
		private const float GroundCheckLimit = 70f;

#region Fields
		protected readonly Character          CurrentCharacter = new();
		protected readonly CharacterMoveState CurrentState     = new();

		private readonly RaycastHit[] _hitRaycasts = new RaycastHit[20];

		protected JumpData         JumpData         = new();
		protected MovementData     MovementData     = new();
		protected GroundCheckData  GroundCheckData  = new();
		protected ForwardCheckData ForwardCheckData = new();

		protected float JumpTimer;
		private   float _moveAnimValue;

		private Transform? _referenceTransform;
		private float      _accumulateGravity;

		private Vector3 _prevPosition;

		private Quaternion _targetRotation = Quaternion.identity;

		private   Vector3 _moveXZValue = Vector3.zero;
		protected float   MoveYValue;

		private bool _sitRequested;
		private bool _jumpRequested;

		protected float JumpStartHeight = float.NegativeInfinity;

		private float _currentSpeed;
		private float _moveMaintainTimer;

		private float _gravityWeightInFallingTimer;

		private bool _isForceSetPosition;

		private CharacterState _prevCurrentState = CharacterState.None;
		private CharacterState _currCurrentState = CharacterState.IdleWalkRun;

		protected CharacterState CurrentCurrentState
		{
			get => _currCurrentState;
			set
			{
				if (_currCurrentState == value) return;

				if (value == CharacterState.JumpStart)
				{
					JumpAction();
					JumpStartTimer().Forget();
				}

				_currCurrentState = value;
				if (!CurrentCharacter.AvatarAnimatorController.IsReferenceNull())
					CurrentCharacter.AvatarAnimatorController!.SetAnimatorState((int)_currCurrentState, -1);
				OnChangeCharacterState();
			}
		}

		protected abstract void OnChangeCharacterState();
#endregion Fields

#region Virtual Method Override
		protected virtual bool CheckDoMove()
		{
			var playerController = PlayerController.InstanceOrNull;
			if (playerController.IsUnityNull() || !playerController.CanInput)
				return false;
			
			if (_isForceSetPosition)
			{
				_isForceSetPosition = false;
				return false;
			}

			return CurrentCurrentState != CharacterState.Sit;
		}

		public override void OnUpdate()
		{
			UpdateTimers();

			var mapObject = CurrentCharacter.MapObject;
			if (mapObject.IsUnityNull()) return;

			UpdateStateBeforeMove();

			if (mapObject!.IsNavigating)
			{
				var deltaPosition = mapObject.transform.position - _prevPosition;
				if (deltaPosition.magnitude < PlayerController.MovingThresholdVelocityOnNavigation)
				{
					ClearSpeedParameter();
				}
				else
				{
					SetSpeedParameter();
					_moveMaintainTimer += Time.deltaTime * MovementData.MoveSpeedAccelerationTimeFactor;
				}
			}
			
			PlayMovementAnimation();
			if (CheckDoMove())
			{
				if (!CurrentCharacter.AvatarAnimatorController.IsReferenceNull())
				{
					CurrentCharacter.AvatarAnimatorController!.SetLerpAnimatorParameters(Time.deltaTime);
					CurrentCharacter.AvatarAnimatorController.SetAnimatorState((int)_currCurrentState, -1);
				}

				if (!CurrentCharacter.CharacterController.IsUnityNull())
				{
					if (CurrentCharacter.CharacterController!.isGrounded)
					{
						MoveYValue         = Mathf.Max(MoveYValue, 0);
						_accumulateGravity = 0f;
					}

					if (MoveYValue > 0 && JumpStartHeight + JumpData.MaxJumpHeight < mapObject!.transform.position.y)
					{
						MoveYValue      = 0;
						JumpStartHeight = float.NegativeInfinity;
					}

					ApplyGravity();

					Vector3 moveValue;
					var playerController = PlayerController.InstanceOrNull;
					if (playerController.IsUnityNull() || !playerController!.CanMove)
					{
						moveValue = new Vector3(0, MoveYValue, 0) * Time.deltaTime;
					}
					else if (CurrentState.IsForwardBlocked)
					{
						moveValue = ApplySlidingSide(new Vector3(_moveXZValue.x, MoveYValue, _moveXZValue.z)) * Time.deltaTime;
					}
					else
					{
						if (CurrentState.IsGround)
							_moveXZValue = Vector3.ProjectOnPlane(_moveXZValue, CurrentState.ContactNormal);

						moveValue = (_moveXZValue + Vector3.up * MoveYValue) * Time.deltaTime;
					}

					if (CurrentCharacter.CharacterController.enabled)
						CurrentCharacter.CharacterController.Move(moveValue);

					// 점프없이 떨어지는 경우도 체크하기 위해 포지션 차이를 누적
					var yPositionDiff = mapObject!.transform.position.y - _prevPosition.y;
					if (yPositionDiff < 0)
						_accumulateGravity += yPositionDiff * JumpData.GravityCumulativeWeight;
				}

				var transform = mapObject.transform;

				var rotationSmoothFactor = MovementData.MoveToRotationSmoothFactor;
				transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, rotationSmoothFactor * Time.deltaTime);
			}
			else
			{
				_targetRotation = mapObject.transform.rotation;
			}

			_prevPosition = mapObject.transform.position;
		}

		public override void Initialize()
		{
			PacketReceiver.Instance.OnUseChairResponseEvent -= OnResponseUseChairResponse;
			PacketReceiver.Instance.OnUseChairResponseEvent += OnResponseUseChairResponse;
		}

		public override void Clear()
		{
			PacketReceiver.Instance.OnUseChairResponseEvent -= OnResponseUseChairResponse;
			ClearCharacter();
		}

		public override void OnFinishNavigation()
		{
			base.OnFinishNavigation();
			ClearSpeedParameter();
		}
#endregion Virtual Method Override

#region Abstract Method Override
		protected abstract void UpdateTimers();

		protected abstract void UpdateStateBeforeMove();

		public override void SetCharacter(MapObject mapObject)
		{
			ClearCharacter();
			CurrentCharacter.SetComponents(mapObject);

			var mainCamera = CameraManager.Instance.MainCamera;
			if (!mainCamera.IsUnityNull())
				_referenceTransform = mainCamera!.transform;

			_prevPosition = mapObject.transform.position;
			SetCharacterControllerDefault();
			_targetRotation = mapObject.transform.rotation;

			if (!CurrentCharacter.AvatarAnimatorController.IsReferenceNull())
				CurrentCharacter.AvatarAnimatorController!.ApplyRootMotion(false);
			ClearSpeedParameter();
		}

		public override void SetCharacterControllerDefault()
		{
			if (!CurrentCharacter.CharacterController.IsUnityNull())
			{
				CurrentCharacter.CharacterController!.radius   = ServerRadius;
				CurrentCharacter.CharacterController.height    = ServerHeight;
				CurrentCharacter.CharacterController.skinWidth = 0.001f;
				CurrentCharacter.CharacterController.center    = new Vector3(0, CharacterCenter, 0);
				CurrentCharacter.CharacterController.enabled   = false;
			}

			if (!CurrentCharacter.TriggerCollider.IsUnityNull())
			{
				CurrentCharacter.TriggerCollider!.radius = ServerRadius;
				CurrentCharacter.TriggerCollider.height  = ServerHeight;
				CurrentCharacter.TriggerCollider.center  = new Vector3(0, CharacterCenter, 0);
				CurrentCharacter.TriggerCollider.enabled = false;
			}

			if (!CurrentCharacter.TriggerRigidbody.IsUnityNull())
			{
				CurrentCharacter.TriggerRigidbody!.useGravity      = false;
				CurrentCharacter.TriggerRigidbody.detectCollisions = false;
				CurrentCharacter.TriggerRigidbody.isKinematic      = true;
				CurrentCharacter.TriggerRigidbody.constraints      = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
			}

			if (!CurrentCharacter.AvatarAnimatorController.IsReferenceNull())
				CurrentCharacter.AvatarAnimatorController!.ApplyRootMotion(false);
		}

		public override MapObject? GetCharacter()
		{
			var character = CurrentCharacter.MapObject;
			return character.IsUnityNull() ? null : character;
		}

		public override void MoveCommand(Vector2 inputValue)
		{
			var transform           = CurrentCharacter.Transform;
			var characterController = CurrentCharacter.CharacterController;
			if (transform.IsReferenceNull() || characterController.IsReferenceNull())
			{
				C2VDebug.LogErrorCategory(nameof(PlayerMovementOnlyClient), "Can't find required components");
				return;
			}

			if (inputValue == Vector2.zero)
			{
				_moveXZValue = Vector3.zero;
				DoMove(Vector3.zero);
				return;
			}

			ClearStateBeforeMoveCommand();

			var referenceForward = Vector3.forward;
			var referenceRight   = Vector3.right;

			if (!_referenceTransform.IsReferenceNull())
			{
				referenceForward = Vector3.ProjectOnPlane(_referenceTransform!.transform.forward, Vector3.up);
				referenceRight   = Vector3.Cross(Vector3.up, referenceForward);
			}

			var direction = GetMoveDirection(inputValue, referenceForward, referenceRight);
			DoMove(direction);
		}

		protected virtual void ClearStateBeforeMoveCommand() { }

		public override void MoveAction(bool changeSprint, bool jump)
		{
			if (changeSprint) CurrentState.IsSprint = !CurrentState.IsSprint;
			if (jump && JumpTimer >= JumpData.JumpInterval)
			{
				JumpTimer = 0;
				if (JumpData.NeedJumpReady && !CurrentCharacter.AvatarAnimatorController.IsReferenceNull() && CurrentCharacter.AvatarAnimatorController!.CurrentAnimationState == eAnimationState.STAND) OnJumpReady();
				else OnJump();
			}
		}
#endregion Abstract Method Override

#region Helper
		private Vector3 GetMoveDirection(Vector2 inputValue, Vector3 cameraForward, Vector3 cameraRight)
		{
			var direction = (cameraForward * inputValue.y + cameraRight * inputValue.x).normalized;
			return direction;
		}

		private void PlayMovementAnimation()
		{
			_moveAnimValue = Mathf.Lerp(_moveAnimValue, _currentSpeed, MovementData.MoveAnimValueSmoothFactor * Time.deltaTime);

			CurrentCharacter.AvatarAnimatorController!.TargetVelocity       = _moveAnimValue;
			CurrentCharacter.AvatarAnimatorController.TargetFallingDistance = CurrentState.InAirHeight;
		}

		protected virtual void ClearCharacter()
		{
			CurrentCharacter.Clear();
			CurrentState.Clear();
			JumpTimer      = 0f;
			_moveAnimValue = 0f;
		}

		private void ApplyGravity()
		{
			if (JumpData.IsApplyGravityWeightInAir && _currCurrentState == CharacterState.InAir && MoveYValue < 0)
			{
				_gravityWeightInFallingTimer += Time.deltaTime * JumpData.GravityTimeWeightInFalling;
				var weight = Mathf.Clamp(_gravityWeightInFallingTimer * _gravityWeightInFallingTimer * JumpData.SqrtGravityWeightInFalling, 0, 1);
				MoveYValue += JumpData.Gravity * Time.deltaTime * weight;
			}
			else
			{
				MoveYValue                   += JumpData.Gravity * Time.deltaTime;
				_gravityWeightInFallingTimer =  0f;
			}
		}

		private Vector3 ApplySlidingSide(Vector3 moveValue)
		{
			// 갈 수 없는 벽에 부딫혔을때, 진행방향에 따라 튕겨나가는 방향으로 진행한다.
			var characterTransform = CurrentCharacter.Transform;
			if (characterTransform.IsUnityNull()) return moveValue;
			var forward = characterTransform!.forward;
			var right   = characterTransform.right;

			var capsuleBottomCenterPoint = CapsuleBottomCenterPoint();
			var capsuleTopCenterPoint    = CapsuleTopCenterPoint();
			var capsuleCenterPoint = (capsuleTopCenterPoint - capsuleBottomCenterPoint) * 0.5f + capsuleBottomCenterPoint;

			bool cast =
				Physics.CapsuleCast(capsuleBottomCenterPoint, capsuleTopCenterPoint, ForwardCheckData.CheckCapsuleRadius, forward,
				                    out var hit, ForwardCheckData.CheckDistance, ForwardCheckData.LayerMask, QueryTriggerInteraction.Ignore);

			if (cast)
			{
				var ray = new Ray(capsuleCenterPoint, hit.point);
				if (Physics.Raycast(ray, out var hitInfo, ForwardCheckData.CheckCapsuleRadius + 0.1f, ForwardCheckData.LayerMask, QueryTriggerInteraction.Ignore))
				{
					Vector3 result;
					var     wallAngle = Vector3.SignedAngle(hitInfo.normal, forward * -1, right);
					if (Mathf.Abs(wallAngle) < 1f) // 뒤쪽으로 튕겨나감
					{
						result = forward * -1f * moveValue.magnitude * 0.5f;
					}
					else if (!CurrentState.IsGround)
					{
						var dir = Vector3.Cross(hitInfo.normal, right);
						result = dir * Mathf.Abs(moveValue.y);
					}
					else
					{
						result = Vector3.ProjectOnPlane(moveValue, hitInfo.normal);
					}

					return new Vector3(result.x, moveValue.y, result.z);
				}
			}

			return moveValue;
		}

		private async UniTask JumpStartTimer()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(JumpData.MaximumJumpStartTime));
			if (_currCurrentState == CharacterState.JumpStart)
			{
				CurrentCurrentState = CharacterState.InAir;
				C2VDebug.LogWarningCategory(nameof(PlayerMovementOnlyClient), "Unable to switch to the In-Air state, the state was forcibly converted.");
			}
		}
#endregion Helper

#region Movement
		/// <summary>
		/// 캐릭터 컨트롤러를 이동시킵니다.
		/// </summary>
		/// <param name="moveDirection">이동할 방향벡터입니다.</param>
		protected void DoMove(Vector3 moveDirection)
		{
			// 점프시 방향 전환 금지
			if (moveDirection != Vector3.zero && _currCurrentState != CharacterState.IdleWalkRun && !CurrentCharacter.Transform.IsReferenceNull())
			{
				moveDirection = Vector3.ProjectOnPlane(moveDirection, CurrentCharacter.Transform!.right);
				moveDirection = moveDirection.sqrMagnitude > 0.1f ? moveDirection.normalized : Vector3.zero;
			}

			SetSpeedParameter();

			_moveXZValue  = moveDirection * _currentSpeed;

			if (moveDirection == Vector3.zero)
			{
				ClearSpeedParameter();
				return;
			}

			_moveMaintainTimer += Time.deltaTime * MovementData.MoveSpeedAccelerationTimeFactor;

			// 점프시 방향 회전 금지, 앉을때 회전하면 일어남
			if (_currCurrentState == CharacterState.IdleWalkRun || _currCurrentState == CharacterState.Sit)
				_targetRotation = Quaternion.LookRotation(moveDirection);
		}

		protected void ClearSpeedParameter()
		{
			_moveMaintainTimer = 0f;
			_currentSpeed      = 0f;
		}

		private void SetSpeedParameter()
		{
			var speed = CurrentState.IsSprint ? MovementData.SprintSpeed : MovementData.Speed;
			speed *= CurrentState.IsGround ? 1f : MovementData.SpeedRatioInAir;
			var speedAccel = _moveMaintainTimer >= 0.9f ? 1f : Mathf.Min(1f, Mathf.Pow(_moveMaintainTimer, MovementData.MoveSpeedAccelerationFactor));
			_currentSpeed = speed * speedAccel;
		}

		protected void OnJumpReady()
		{
			var animator = CurrentCharacter.AvatarAnimatorController;
			if (!CurrentCharacter.AvatarAnimatorController.IsReferenceNull())
				animator!.SetJumpReady(OnExitJumpReady);
		}

		private void OnExitJumpReady()
		{
			OnJump(false);
		}

		protected void OnJump(bool needJumpStartAnimation = true)
		{
			var animator = CurrentCharacter.AvatarAnimatorController;
			if (needJumpStartAnimation && !CurrentCharacter.AvatarAnimatorController.IsReferenceNull())
				animator!.TrySetInteger(AnimationDefine.HashState, (int)CharacterState.JumpStart);

			var playerController = PlayerController.InstanceOrNull;
			if (!playerController.IsUnityNull())
				playerController!.SetCharacterState(CharacterState.JumpStart);

			_jumpRequested = true;
		}

		protected abstract void JumpAction();

		private void OnResponseUseChairResponse(Protocols.CommonLogic.UseChairResponse useChairResponse)
		{
			if (CurrentCharacter.MapObject.IsUnityNull()) return;

			var mapController = MapController.InstanceOrNull;
			if (mapController.IsReferenceNull())
				return;

			var chairObject = mapController!.GetStaticObjectByID(useChairResponse.TargetChairId) as MapObject;
			if (chairObject.IsUnityNull())
				return;

			Vector3 targetPosition = chairObject!.transform.position;
			Vector3 flooredPosition = new Vector3(RoundDownAtSecondDecimal(targetPosition.x),
			                                      RoundDownAtSecondDecimal(targetPosition.y),
			                                      RoundDownAtSecondDecimal(targetPosition.z));
			if (!CurrentCharacter.Transform.IsUnityNull())
				CurrentCharacter.Transform!.SetPositionAndRotation(flooredPosition, chairObject.transform.rotation);

			var animator = CurrentCharacter.AvatarAnimatorController;
			if (!CurrentCharacter.AvatarAnimatorController.IsReferenceNull())
				animator!.TrySetInteger(AnimationDefine.HashState, (int)CharacterState.Sit);

			var playerController = PlayerController.InstanceOrNull;
			if (!playerController.IsUnityNull())
			{
				playerController!.SetCharacterState(CharacterState.Sit);
				playerController!.PreventMovementForSeconds(1);	
			}

			// TODO: 오브젝트 높이 정보 추가
			_sitRequested = true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float RoundDownAtSecondDecimal(float value)
		{
			return (float)(int)(value * 100) / 100;
		}

		public override void Teleport(Vector3 position, Quaternion rotation)
		{
			ClearSpeedParameter();
			if (!CurrentCharacter.Transform.IsUnityNull())
				CurrentCharacter.Transform!.SetPositionAndRotation(position, rotation);

			_targetRotation = rotation;
		}

		public override void ForceSetCharacterState(CharacterState characterState)
		{
			CurrentCurrentState = characterState;

			var playerController = PlayerController.InstanceOrNull;
			if (!playerController.IsUnityNull())
				playerController!.SetCharacterState(CharacterState.Sit);
		}
#endregion Movement

#region CurrentState
		protected void CheckCharacterState()
		{
			var characterController = CurrentCharacter.CharacterController;
			if (characterController.IsUnityNull()) return;

			bool playerMoved = PlayerController.Instance.IsUnityNull() || PlayerController.InstanceOrNull.CanMove && (_moveXZValue != Vector3.zero || _targetRotation != CurrentCharacter.Transform.rotation);

			if (_sitRequested)
			{
				CurrentCurrentState = CharacterState.Sit;
				_sitRequested       = false;
				return;
			}

			if (_jumpRequested)
			{
				CurrentCurrentState = CharacterState.JumpStart;
				_jumpRequested      = false;
				return;
			}

			_prevCurrentState = CurrentCurrentState;
			if (CurrentState.IsGround)
			{
				switch (_prevCurrentState)
				{
					case CharacterState.JumpStart:
						break;
					case CharacterState.InAir:
						CurrentCurrentState = CharacterState.JumpLand;
						break;
					case CharacterState.Sit:
						if (playerMoved) CurrentCurrentState = CharacterState.IdleWalkRun;
						break;
					default:
						CurrentCurrentState = CharacterState.IdleWalkRun;
						break;
				}
			}
			// InAir 상태 도달 조건
			else if ((CurrentCurrentState == CharacterState.JumpStart && CurrentState.InAirHeight >= JumpData.InAirThresholdWhenYUp) ||
			         (CurrentCurrentState != CharacterState.JumpLand  && _accumulateGravity <= JumpData.InAirThresholdWhenYDown))
			{
				CurrentCurrentState = CharacterState.InAir;
			}
		}

		private const float FallingDistance = 6f;
		private bool _isFalling;

		protected void CheckOnGround()
		{
			var transform = CurrentCharacter.Transform;
			if (transform.IsUnityNull())
			{
				CurrentState.ClearJumpState();
				return;
			}

			EvaluateContact(transform!);
			var pos = transform!.position + Vector3.up * GroundCheckData.CheckSphereOffset;
			var ray = new Ray(pos, Vector3.down);
			var hit = Physics.SphereCast(ray, GroundCheckData.CheckSphereRadius, out var hitInfo, GroundCheckData.MaxDistance, ~GroundCheckData.IgnoreLayerMask);
			if (hit)
			{
				var distance = hitInfo.distance;
				CurrentState.InAirHeight = hitInfo.distance;
				if (distance >= GroundCheckData.GroundCheckDistance)
				{
					if (CurrentState.InAirHeight > FallingDistance)
						_isFalling = true;

					CurrentState.IsGround = false;
					return;
				}

				var groundAngle = Vector3.Angle(CurrentState.ContactNormal, Vector3.up);
				if (groundAngle > GroundCheckLimit)
				{
					CurrentState.IsGround = false;
					return;
				}

				_isFalling = false;
				CurrentState.IsGround = true;
			}
			else
			{
				CurrentState.IsGround    = false;
				CurrentState.InAirHeight = GroundCheckData.MaxDistance;
				ForceMoveToRandomPositionOnGround(transform);
			}
		}

		private void ForceMoveToRandomPositionOnGround(Transform transform)
		{
			var characterPosition = transform.position;
			var basePosition      = new Vector3(characterPosition.x, Mathf.Max(0f, characterPosition.y), characterPosition.z);
			var attempts          = 500;
			var radius            = 3f;
			var increasingRadius  = 0.03f;
			var height            = 10f;
			var increasingHeight  = 0.1f;
			while (attempts-- > 0)
			{
				var randomPosition = MathUtil.RandomPositionOnCircle(radius);
				radius += increasingRadius;

				var ray = new Ray(basePosition + randomPosition + Vector3.up * height, Vector3.down);
				height += increasingHeight;

				var hit = Physics.SphereCast(ray, GroundCheckData.CheckSphereRadius, out var hitInfo, float.PositiveInfinity, ~GroundCheckData.IgnoreLayerMask);
				if (hit)
				{
					transform.position = hitInfo.point;
					CurrentState.ClearJumpState();
					_isForceSetPosition = true;
					return;
				}
			}
		}

		protected void CheckForward()
		{
			var transform = CurrentCharacter.Transform;

			if (transform.IsReferenceNull())
			{
				CurrentState.IsForwardBlocked = true;
				return;
			}

			bool cast =
				Physics.CapsuleCast(CapsuleBottomCenterPoint(), CapsuleTopCenterPoint(), ForwardCheckData.CheckCapsuleRadius, transform!.forward,
				                    out var hit, ForwardCheckData.CheckDistance, ForwardCheckData.LayerMask, QueryTriggerInteraction.Ignore);

			CurrentState.IsForwardBlocked = false;
			if (cast)
			{
				var characterController = CurrentCharacter.CharacterController;
				if (!characterController.IsUnityNull())
				{
					float forwardObstacleAngle = Vector3.Angle(hit.normal, Vector3.up);
					CurrentState.IsForwardBlocked = forwardObstacleAngle > characterController!.slopeLimit;
				}
			}
		}

		private Vector3 CapsuleTopCenterPoint()
		{
			var transform           = CurrentCharacter.Transform;
			var characterController = CurrentCharacter.CharacterController;

			if (transform.IsReferenceNull() || characterController.IsUnityNull())
				return Vector3.zero;

			var position = transform!.position;
			return new Vector3(position.x, position.y + characterController!.height - characterController.radius, position.z);
		}

		private Vector3 CapsuleBottomCenterPoint()
		{
			var transform           = CurrentCharacter.Transform;
			var characterController = CurrentCharacter.CharacterController;

			if (transform.IsReferenceNull() || characterController.IsUnityNull())
				return Vector3.zero;

			var position = transform!.position;
			return new Vector3(position.x, position.y + characterController!.radius + characterController.stepOffset, position.z); // 계단 높이 무시
		}

		private void EvaluateContact(Transform transform)
		{
			var characterController = CurrentCharacter.CharacterController;
			if (characterController.IsUnityNull())
				CurrentState.ContactNormal = Vector3.up;

			var maxDistance = 0.1f;
			var sphereRay   = new Ray(transform.position, Vector3.down);
			var numHits     = Physics.SphereCastNonAlloc(sphereRay, characterController!.radius, _hitRaycasts, maxDistance, ~GroundCheckData.IgnoreLayerMask, QueryTriggerInteraction.Ignore);
			CurrentState.ContactNormal = Vector3.zero;
			for (var i = 0; i < numHits; ++i)
			{
				var hitPoint = _hitRaycasts[i].point;
				var ray      = new Ray(transform.position, hitPoint == Vector3.zero ? Vector3.down : hitPoint - transform.position);

				if (Physics.Raycast(ray, out var hitInfo, characterController.radius + maxDistance, ~GroundCheckData.IgnoreLayerMask, QueryTriggerInteraction.Ignore))
					CurrentState.ContactNormal += hitInfo.normal;
			}

			if (CurrentState.ContactNormal == Vector3.zero)
				CurrentState.ContactNormal = Vector3.up;
			else
				CurrentState.ContactNormal.Normalize();
		}
#endregion CurrentState

#if UNITY_EDITOR
#region Editor
		public override void OnDrawGizmo()
		{
			DrawCheckGroundGizmo();
			DrawContactNormal();
		}

		private void DrawCheckGroundGizmo()
		{
			var transform = CurrentCharacter.Transform;
			if (transform.IsReferenceNull())
			{
				CurrentState.ClearJumpState();
				return;
			}

			var pos = transform!.position + Vector3.up * GroundCheckData.CheckSphereOffset;
			var ray = new Ray(pos, Vector3.down);
			var hit = Physics.SphereCast(ray, GroundCheckData.CheckSphereRadius, out var hitInfo, GroundCheckData.MaxDistance, ~GroundCheckData.IgnoreLayerMask);

			Gizmos.color = CurrentState.IsGround ? Color.green : Color.red;

			var groundCheckSpherePoint = hit ? hitInfo.point : ray.GetPoint(GroundCheckData.MaxDistance);
			Gizmos.DrawSphere(groundCheckSpherePoint, GroundCheckData.CheckSphereRadius);

			var groundCheckHandleStyle = new GUIStyle();
			Handles.Label(groundCheckSpherePoint, $"IsGround: {CurrentState.IsGround}", groundCheckHandleStyle);
		}

		private void DrawContactNormal()
		{
			var transform = CurrentCharacter.Transform;
			if (transform.IsReferenceNull())
			{
				CurrentState.ClearJumpState();
				return;
			}

			var pos = transform!.position;
			Gizmos.color = Color.red;
			Gizmos.DrawLine(pos, pos + CurrentState.ContactNormal * 2f);
		}
#endregion Editor
#endif // UNITY_EDITOR
	}
}
