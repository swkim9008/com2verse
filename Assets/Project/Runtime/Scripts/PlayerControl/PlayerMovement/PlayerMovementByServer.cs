/*===============================================================
* Product:		Com2Verse
* File Name:	PlayerMovementByServer.cs
* Developer:	eugene9721
* Date:			2023-02-03 10:33
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.AvatarAnimation;
using Com2Verse.Extension;
using UnityEngine;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;
using Protocols;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Com2Verse.PlayerControl
{
	/// <summary>
	/// 현재의 서버 구조를 위한 임시 스크립트
	/// 물리서버 이관 후 삭제될 가능성 높음
	/// </summary>
	public sealed class PlayerMovementByServer : PlayerMovementBase
	{
		private CommandIntervalData _commandIntervalData = new();
		private JumpData            _jumpData            = new();

		private readonly Character _character = new Character();

		private float _moveToTimer;
		private float _moveCommandTimer;
		private float _sprintTimer;
		private float _jumpTimer;

		private bool _currentSprint;
		private bool _isNeedSprintUpdate;
		private bool _reservedSprint;

#region Virtual Method Override
		public override void SetData(IMovementData data)
		{
			switch (data)
			{
				case CommandIntervalData commandIntervalData:
					_commandIntervalData = commandIntervalData;
					break;
				case JumpData jumpData:
					_jumpData = jumpData;
					break;
			}
		}

		public override void OnUpdate()
		{
			_moveToTimer      += Time.deltaTime;
			_moveCommandTimer += Time.deltaTime;
			_sprintTimer      += Time.deltaTime;
			_jumpTimer        += Time.deltaTime;

			_moveToTimer      = Mathf.Min(_moveToTimer, _commandIntervalData.MoveToInterval);
			_moveCommandTimer = Mathf.Min(_moveCommandTimer, _commandIntervalData.MoveCommandInterval);
			_sprintTimer      = Mathf.Min(_sprintTimer, _commandIntervalData.SprintInterval);
			_jumpTimer        = Mathf.Min(_jumpTimer, _jumpData.JumpInterval);

			CheckSprint();
		}

		public override void Clear()
		{
			_moveToTimer        = 0f;
			_moveCommandTimer   = 0f;
			_sprintTimer        = 0f;
			_jumpTimer          = 0f;
			_currentSprint      = false;
			_isNeedSprintUpdate = true;
			_reservedSprint     = false;
			_character.Clear();
		}
#endregion Virtual Method Override

#region Abstract Method Override
		public override void SetCharacter(MapObject mapObject)
		{
			Clear();
			_character.SetComponents(mapObject);
			SetCharacterControllerDefault();
		}

		public override void SetCharacterControllerDefault()
		{
			if (!_character.CharacterController.IsUnityNull())
			{
				_character.CharacterController!.radius   = ServerRadius;
				_character.CharacterController.height    = ServerHeight;
				_character.CharacterController.skinWidth = 0.001f;
				_character.CharacterController.center    = new Vector3(0, CharacterCenter, 0);
				_character.CharacterController.enabled   = false;
			}
			if (!_character.TriggerCollider.IsUnityNull())
			{
				_character.TriggerCollider!.radius = ServerRadius;
				_character.TriggerCollider.height  = ServerHeight;
				_character.TriggerCollider.center  = new Vector3(0, CharacterCenter, 0);
				_character.TriggerCollider.enabled = false;
			}
			if (!_character.TriggerRigidbody.IsUnityNull())
			{
				_character.TriggerRigidbody!.useGravity      = false;
				_character.TriggerRigidbody.detectCollisions = false;
				_character.TriggerRigidbody.isKinematic      = true;
				_character.TriggerRigidbody.constraints      = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
			}
		}

		public override MapObject? GetCharacter()
		{
			var character = _character.MapObject;
			return character.IsUnityNull() ? null : character;
		}

		public override void MoveTo(Vector3 position)
		{
			if (_moveToTimer >= _commandIntervalData.MoveToInterval)
			{
				_moveToTimer = 0f;
				Commander.Instance.MoveTo(position);
			}
		}

		public override void MoveCommand(Vector2 position)
		{
			if (_moveCommandTimer >= _commandIntervalData.MoveCommandInterval)
			{
				_moveCommandTimer = 0f;
				Commander.Instance.MoveCommand(position);
			}
		}

		public override void MoveAction(bool sprint, bool jump)
		{
			if (jump && _jumpTimer >= _jumpData.JumpInterval)
			{
				_jumpTimer = 0;
				DelayedSendJump(_currentSprint).Forget();
				return;
			}

			if (_isNeedSprintUpdate || sprint != _currentSprint)
			{
				_isNeedSprintUpdate = true;
				_reservedSprint     = sprint;
			}
		}

		public override void Teleport(Vector3 position, Quaternion rotation) {}

		public override void ForceSetCharacterState(CharacterState characterState) {}
#endregion Abstract Method Override

#region MoveAction
		private void CheckSprint()
		{
			if (!_isNeedSprintUpdate) return;

			_isNeedSprintUpdate = false;

			if (_sprintTimer >= _commandIntervalData.SprintInterval)
			{
				_currentSprint = _reservedSprint;
				_sprintTimer   = 0f;
				Commander.Instance.MoveAction(_reservedSprint, false);
			}
		}

		private async UniTaskVoid DelayedSendJump(bool currentSprint)
		{
			var animator = _character.AvatarAnimatorController;
			if (!_character.AvatarAnimatorController.IsReferenceNull())
			{
				animator!.TrySetInteger(AnimationDefine.HashState, (int)Protocols.CharacterState.JumpStart);
				var playerController = PlayerController.InstanceOrNull;
				if (!playerController.IsUnityNull())
					playerController!.SetCharacterState(Protocols.CharacterState.JumpStart);
			}

			if (MapController.InstanceExists && MapController.Instance.StatePublisher != null)
			{
				var publisher = MapController.Instance.StatePublisher as ActiveObjectStatePublisher;
				publisher?.CheckStateDirty(true);
			}

			await UniTask.Delay(TimeSpan.FromSeconds(_jumpData.JumpDelay), DelayType.Realtime);
			Commander.Instance.MoveAction(currentSprint, true);
		}
#endregion MoveAction
	}
}
