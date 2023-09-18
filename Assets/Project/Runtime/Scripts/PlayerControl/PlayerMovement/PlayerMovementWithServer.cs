/*===============================================================
* Product:		Com2Verse
* File Name:	PlayerMovementWithServer.cs
* Developer:	eugene9721
* Date:			2023-03-10 12:34
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.AvatarAnimation;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Protocols;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Com2Verse.PlayerControl
{
	public class PlayerMovementWithServer : PlayerMovementWithClientBase
	{
		private const float DistOfAnimationOffWayPoint = 0.5f;
		private const float DistOfTouchedWayPoint      = 0.1f;

#region Fields
		private CommandIntervalData _commandIntervalData = new();

		private float _moveToTimer;
#endregion Fields

#region Virtual Method Override
		public override void SetData(IMovementData data)
		{
			switch (data)
			{
				case CommandIntervalData commandIntervalData:
					_commandIntervalData = commandIntervalData;
					break;
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
			_moveToTimer += Time.deltaTime;
			JumpTimer    += Time.deltaTime;
			_moveToTimer =  Mathf.Min(_moveToTimer, _commandIntervalData.MoveToInterval);
			JumpTimer    =  Mathf.Min(JumpTimer, JumpData.JumpInterval);
		}

		protected override bool CheckDoMove() => base.CheckDoMove() && !CurrentCharacter.MapObject!.IsNavigating;

		protected override void UpdateStateBeforeMove()
		{
			if (CurrentCharacter.MapObject!.IsNavigating)
				MoveYValue = 0.0f;
			CheckOnGround();
			CheckCharacterState();
			CheckForward();
		}


		public override void OnFinishNavigation()
		{
			if (CurrentCharacter.MapObject.IsUnityNull()) return;
			if (CurrentCharacter.MapObject!.TargetPositionDiff.sqrMagnitude < DistOfAnimationOffWayPoint)
				ClearSpeedParameter();
			if (CurrentCharacter.MapObject!.TargetPositionDiff.sqrMagnitude < DistOfTouchedWayPoint)
				base.OnFinishNavigation();
		}
#endregion Virtual Method Override

#region Abstract Method Override
		public override void MoveTo(Vector3 position)
		{
			var mapObject = CurrentCharacter.MapObject;
			if (!mapObject.IsUnityNull() && !mapObject!.IsNavigating && 
			    (!CurrentState.IsGround || CurrentCurrentState is CharacterState.JumpStart or CharacterState.InAir or CharacterState.JumpLand))
				return;

			if (CurrentCurrentState == CharacterState.Sit)
			{
				CurrentCurrentState = CharacterState.IdleWalkRun;
			}

			if (_moveToTimer >= _commandIntervalData.MoveToInterval)
			{
				_moveToTimer = 0f;
				PlayerController.Instance.SetNavigationMode(true);
				Commander.Instance.MoveTo(position);
			}
		}

		public override void MoveCommand(Vector2 inputValue)
		{
			var mapObject = CurrentCharacter.MapObject;
			if (!mapObject.IsUnityNull() && mapObject!.IsNavigating &&
			    (!CurrentState.IsGround || CurrentCurrentState is CharacterState.JumpStart or CharacterState.InAir or CharacterState.JumpLand))
				return;

			base.MoveCommand(inputValue);
			PlayerController.Instance.SetNavigationMode(false);
		}

		public override void MoveAction(bool changeSprint, bool jump)
		{
			if (!CurrentState.IsGround) return;

			if (changeSprint)
			{
				CurrentState.IsSprint = !CurrentState.IsSprint;
				Commander.Instance.MoveAction(CurrentState.IsSprint, jump);
			}

			if (jump && JumpTimer >= JumpData.JumpInterval)
			{
				if (!CurrentCharacter.MapObject.IsUnityNull() && CurrentCharacter.MapObject!.IsNavigating)
				{
					ClearSpeedParameter();
					PlayerController.Instance.SetNavigationMode(false);
				}

				JumpTimer = 0;
				if (JumpData.NeedJumpReady &&
				    !CurrentCharacter.AvatarAnimatorController.IsReferenceNull() && CurrentCharacter.AvatarAnimatorController!.CurrentAnimationState == eAnimationState.STAND) OnJumpReady();
				else OnJump();
			}
		}

		public override void Teleport(Vector3 position, Quaternion rotation)
		{
			PlayerController.Instance.SetNavigationMode(false);
			base.Teleport(position, rotation);
		}

		protected override void OnChangeCharacterState()
		{
			CheckObjectStateDirty();

			if (CurrentCurrentState == CharacterState.JumpLand)
				Commander.Instance.MoveAction(CurrentState.IsSprint, false);
		}

		private void CheckObjectStateDirty()
		{
			var mapController = MapController.InstanceOrNull;
			if (mapController.IsReferenceNull()) return;

			var publisher = mapController!.StatePublisher as ActiveObjectStatePublisher;
			publisher?.CheckStateDirty(true);
		}
#endregion Abstract Method Override

#region Helper
		protected override void ClearCharacter()
		{
			base.ClearCharacter();
			_moveToTimer = 0f;
		}
#endregion Helper

#region Movement
		protected override void JumpAction()
		{
			var mapObject = CurrentCharacter.MapObject;
			Commander.Instance.MoveAction(CurrentState.IsSprint, true);
			if (mapObject.IsUnityNull()) return;

			MoveYValue      += JumpData.JumpPower;
			JumpStartHeight =  CurrentCharacter.Transform!.position.y;
		}
#endregion Movement
	}
}
