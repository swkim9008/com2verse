/*===============================================================
* Product:		Com2Verse
* File Name:	PlayerMovementBase.cs
* Developer:	eugene9721
* Date:			2023-02-03 10:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Avatar;
using Com2Verse.AvatarAnimation;
using Com2Verse.Extension;
using Com2Verse.Logger;
using UnityEngine;
using Com2Verse.Network;
using Pathfinding;
using Protocols;
using Util = Com2Verse.Utils.Util;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Com2Verse.PlayerControl
{
	public enum ePlayerMovementType
	{
		/// <summary>
		/// 서버와 통신 없이 클라이언트 측에서만 이동(샘플, 테스트 용도)
		/// </summary>
		ONLY_CLIENT,

		/// <summary>
		/// 현재 서비스에 적용될 방식,<br/>
		/// WASD - 클라이언트에서 좌표 계산<br/>
		/// 클릭이동 - 서버에서 좌표 계산
		/// </summary>
		WITH_SERVER,

		/// <summary>
		/// 서버에 이동 정보만 보내고, CSU를 통한 클라이언트 오브젝트 이동
		/// </summary>
		BY_SERVER
	}

	public abstract class PlayerMovementBase
	{
		protected static readonly float ServerRadius = 0.2f;
		protected static readonly float ServerHeight = 1.8f;
		protected static readonly float CharacterCenter = 0.9f;

#region Internal Classes
		protected sealed class Character
		{
			// TODO: ActiveObject로 변환 확인
			public MapObject?                MapObject;
			public Transform?                Transform;
			public Rigidbody?                TriggerRigidbody;
			public CapsuleCollider?          TriggerCollider;
			public CharacterController?      CharacterController;
			public AvatarAnimatorController? AvatarAnimatorController;

			public void SetComponents(MapObject mapObject)
			{
				Clear();
				MapObject           = mapObject;
				Transform           = mapObject.transform;
				TriggerRigidbody    = Util.GetOrAddComponent<Rigidbody>(mapObject.gameObject);
				TriggerCollider     = Util.GetOrAddComponent<CapsuleCollider>(mapObject.gameObject);
				CharacterController = Util.GetOrAddComponent<CharacterController>(mapObject.gameObject);

				if (!mapObject.TryGetComponent(out CharacterController))
					C2VDebug.LogErrorCategory(nameof(PlayerMovementOnlyClient), "Can't find CharacterController");

				if (!mapObject.TryGetComponent(out TriggerRigidbody))
					C2VDebug.LogErrorCategory(nameof(PlayerMovementOnlyClient), "Can't find TriggerRigidbody");

				if (!mapObject.TryGetComponent(out TriggerCollider))
					C2VDebug.LogErrorCategory(nameof(PlayerMovementOnlyClient), "Can't find TriggerCollider");

				var baseBody = Transform.Find(MetaverseAvatarDefine.BaseBodyObjectName);
				if (baseBody.IsUnityNull() || !baseBody!.TryGetComponent(out AvatarAnimatorController))
					C2VDebug.LogErrorCategory(nameof(PlayerMovementOnlyClient), "Can't find AvatarAnimatorController");
			}

			public void Clear()
			{
				MapObject                = null;
				Transform                = null;
				TriggerRigidbody         = null;
				TriggerCollider          = null;
				CharacterController      = null;
				AvatarAnimatorController = null;
			}
		}

		protected sealed class CharacterMoveState
		{
			public bool    IsSprint;
			public bool    IsGround;
			public float   InAirHeight;
			public bool    IsForwardBlocked;
			public Vector3 ContactNormal;

			public void Clear()
			{
				ClearJumpState();
				IsForwardBlocked = default;
				ContactNormal    = Vector3.up;
			}

			public void ClearJumpState()
			{
				IsGround    = default;
				InAirHeight = default;
			}
		}
#endregion Internal Classes

		public virtual void SetData(IMovementData data) { }
		public virtual void Initialize()                { }
		public virtual void OnUpdate()                  { }
		public virtual void Clear()                     { }

		public virtual void OnFinishNavigation()
		{
			var playerController = PlayerController.InstanceOrNull;
			if (playerController.IsUnityNull()) return;
			playerController!.SetNavigationMode(false);
		}

		public abstract void SetCharacter(MapObject character);
		public abstract void SetCharacterControllerDefault();
		public abstract void MoveTo(Vector3 position);
		public abstract void MoveCommand(Vector2 position);
		public abstract void MoveAction(bool sprint, bool jump);
		public abstract void Teleport(Vector3 position, Quaternion rotation);
		public abstract void ForceSetCharacterState(CharacterState characterState);

		public abstract MapObject? GetCharacter();


#if UNITY_EDITOR
		public virtual void OnDrawGizmo() { }
#endif // UNITY_EDITOR
	}
}
