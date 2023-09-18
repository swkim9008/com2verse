/*===============================================================
* Product:		Com2Verse
* File Name:	PlayerMovementOnlyClient.cs
* Developer:	eugene9721
* Date:			2023-02-03 10:32
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Com2Verse.AvatarAnimation;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Pathfinder;
using Com2Verse.Utils;
using Pathfinding;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Com2Verse.PlayerControl
{
	public class PlayerMovementOnlyClient : PlayerMovementWithClientBase
	{
#region Fields
		private readonly List<Vector3> _wayPoints = new();
		private          ABPath?       _currentPath;

		private Vector3 _lastWayPoint;
		private int _waypointIndex;

		/// <summary>
		/// 첫번째 웨이포인트에서 마지막 웨이포인트까지의 거리
		/// </summary>
		private float _distOfWaypoints;

		private float _minDist = 0.1f;

		/// <summary>
		/// 한 웨이포인트에 도달했음을 판단하는 거리
		/// </summary>
		private readonly float _distOfTouchedWayPoint = 0.03f;

		protected override void OnChangeCharacterState() { }
#endregion Fields

#region Virtual Method Override
		public override void SetData(IMovementData data)
		{
			switch (data)
			{
				case JumpData jumpData:
					JumpData = jumpData;
					break;
				case MovementData movementData:
					MovementData = movementData;
					break;
				case GroundCheckData groundCheckData:
					GroundCheckData = groundCheckData;
					break;
				case ForwardCheckData forwardCheckData:
					ForwardCheckData = forwardCheckData;
					break;
			}
		}

		protected override void UpdateTimers()
		{
			JumpTimer += Time.deltaTime;
			JumpTimer =  Mathf.Min(JumpTimer, JumpData.JumpInterval);
		}

		protected override void UpdateStateBeforeMove()
		{
			CheckOnGround();
			MoveToWaypoint();
			CheckCharacterState();
			CheckForward();
		}

		public override void Clear()
		{
			base.Clear();
			ClearWaypoint();
		}
#endregion Virtual Method Override

#region Abstract Method Override
		public override void SetCharacter(MapObject mapObject)
		{
			base.SetCharacter(mapObject);
			mapObject.IsNavigating = false;
		}

		public override void SetCharacterControllerDefault()
		{
			base.SetCharacterControllerDefault();
			if (!CurrentCharacter.CharacterController.IsUnityNull())
				CurrentCharacter.CharacterController!.enabled = true;
			if (!CurrentCharacter.TriggerCollider.IsUnityNull())
				CurrentCharacter.TriggerCollider!.enabled = false;
			if (!CurrentCharacter.TriggerRigidbody.IsUnityNull())
				CurrentCharacter.TriggerRigidbody!.detectCollisions = false;
		}

		public override void MoveTo(Vector3 position)
		{
			ClearWaypoint();
			_currentPath = ABPath.Construct(CurrentCharacter.Transform!.position, position);

			Vector2 originPointBeforePathComplete = new Vector2(CurrentCharacter.Transform.position.x, CurrentCharacter.Transform.position.z);
			ClientPathFinding.Instance.PlayerSeeker.StartPath(_currentPath, path =>
			{
				ClearWaypoint();
				if (path.CompleteState == PathCompleteState.Error)
				{
					DoMove(Vector3.zero);
					return;
				}

				foreach (var wayPoint in path.vectorPath)
				{
					Vector2 testPoint = new Vector2(wayPoint.x, wayPoint.z);
					if (Vector2.SqrMagnitude(testPoint - originPointBeforePathComplete) < _distOfTouchedWayPoint) continue;
					
					_wayPoints.Add(wayPoint);
				}

				if (_wayPoints.Count > 0)
				{
					_lastWayPoint = CurrentCharacter.Transform.position;
					
					var playerController = PlayerController.InstanceOrNull;
					if (!playerController.IsReferenceNull())
						playerController!.SetWaypoints(_wayPoints);

					_distOfWaypoints = MathUtil.GetDistanceFromWaypoint(_wayPoints[0], _wayPoints);
				}
			});
		}

		public override void MoveCommand(Vector2 inputValue)
		{
			if (inputValue == Vector2.zero)
				ClearWaypoint();
				
			base.MoveCommand(inputValue);
		}

		protected override void ClearStateBeforeMoveCommand()
		{
			ClearWaypoint();
		}

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

#region WayPoint
		private void MoveToWaypoint()
		{
			if (_wayPoints.Count == 0)
				return;

			var transform           = CurrentCharacter.Transform;
			var characterController = CurrentCharacter.CharacterController;
			if (transform.IsReferenceNull() || characterController.IsReferenceNull())
			{
				C2VDebug.LogErrorCategory(nameof(PlayerMovementOnlyClient), "Can't find required components");
				return;
			}

			Vector3 targetPos = _wayPoints[_waypointIndex];
			Vector3 currPos   = transform!.position;

			targetPos.y = currPos.y;

			Vector3 moveDist  = targetPos - currPos;
			Vector3 moveValue = moveDist.normalized;

			bool reached = moveDist.sqrMagnitude <= _distOfTouchedWayPoint;

			Vector3 destinationDelta = targetPos - _lastWayPoint;
			Vector3 positionDelta = currPos - _lastWayPoint;
			bool overReached = Vector3.Project(positionDelta, destinationDelta).sqrMagnitude > destinationDelta.sqrMagnitude;

			if (reached | overReached)
			{
				_lastWayPoint = _wayPoints[_waypointIndex];
				_waypointIndex++;
				
				if (_waypointIndex >= _wayPoints.Count)
				{
					ClearWaypoint();
					DoMove(Vector3.zero);
				}
				else
				{
					var totalDist = MathUtil.GetDistanceFromWaypoint(currPos, _wayPoints, _waypointIndex);

					if (totalDist < _minDist)
					{
						ClearWaypoint();
						DoMove(Vector3.zero);
						return;
					}

					var playerController = PlayerController.InstanceOrNull;
					if (!playerController.IsUnityNull())
						playerController!.SetProgressWaypoints(1 - totalDist / _distOfWaypoints);
					
					MoveToWaypoint();
				}
			}
			else
			{
				DoMove(moveValue);
			}
		}

		private void ClearWaypoint()
		{
			_wayPoints.Clear();
			_waypointIndex = 0;

			var playerController = PlayerController.InstanceOrNull;
			if (!playerController.IsUnityNull())
			{
				playerController!.SetProgressWaypoints(1f);
				playerController.SetWaypoints(_wayPoints);
			}
		}
#endregion WayPoint

#region Movement
		protected override void JumpAction()
		{
			MoveYValue += JumpData.JumpPower;

			if (CurrentCharacter.Transform.IsReferenceNull()) return;

			JumpStartHeight = CurrentCharacter.Transform!.position.y;
		}
#endregion Movement
	}
}
