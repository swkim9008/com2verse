#if ENABLE_CHEATING

/*===============================================================
* Product:		Com2Verse
* File Name:	DebugUtils.cs
* Developer:	eugene9721
* Date:			2022-07-27 14:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.Avatar;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Network;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Com2Verse.Cheat
{
	/// <summary>
	/// 에디터와 Debug빌드에서 테스트를 도와주는 역할,
	/// Define Symbol 주의
	/// </summary>
	public static class DebugUtils
	{
		private static readonly Dictionary<long, long> _communacationCharacterDict = new ();
		public static float COMMUNICATION_ROOM_RADIUS = 8; // 임시 값
		
		public static void Clear()
		{
			_communacationCharacterDict.Clear();
		}
		
	#region Character
		public static async UniTask<long> DebugAddCharacter(long ownerId, eAvatarType avatarType = eAvatarType.PC01_W, Action<long> onCompleted = null)
		{
			var info      = AvatarInfo.GetTestInfo(avatarType);
			var avatar    = await AvatarCreator.CreateAvatarAsync(info, eAnimatorType.WORLD, Vector3.zero, (int)Define.eLayer.CHARACTER);
			var character = avatar.gameObject;
			
			SetupLayer(character, Define.eLayer.CHARACTER);

			var activeObject = character.AddComponent<ActiveObject>();
			long objectId =  MapController.Instance.CreateObjectDebug(activeObject, ownerId);
			
			onCompleted?.Invoke(objectId);
			return objectId;
		}
		
		public static async UniTask<long> DebugAddCharacterIsMine(long ownerId, eAvatarType avatarType = eAvatarType.PC01_W, Action<long> onCompleted = null)
		{
			long objectId = await DebugAddCharacter(ownerId, avatarType);
			
			User.Instance.OnSetCharacter(objectId, true);
			onCompleted?.Invoke(objectId);
			return objectId;
		}

		private static void SetupLayer(GameObject parent, Define.eLayer layer)
		{
			if (parent == null) return;

			var transforms = parent.GetComponentsInChildren<Transform>();
			if (transforms == null) return;

			foreach (var transform in transforms)
			{
				if (transform != null)
				{
					transform.gameObject.layer = (int)layer;
				}
			}
		}
	#endregion Character

		public static void AddCommunicationCharacter(long ownerId, bool isMine = false)
		{
			// TODO: Communication Type에 따라 로직이 달라질 수 있음
			void Func(long objectId)
			{
				var randomPoint = MathUtil.RandomPositionOnCircle(COMMUNICATION_ROOM_RADIUS);
				MapController.Instance[objectId].transform.position += randomPoint;
				_communacationCharacterDict.TryAdd(ownerId, objectId);
			}

			eAvatarType avatarType = Random.Range(0, 2) == 0 ? eAvatarType.PC01_W : eAvatarType.PC01_M;
			if (isMine) DebugAddCharacterIsMine(ownerId, onCompleted: Func).Forget();
			else DebugAddCharacter(ownerId, avatarType, Func).Forget();
		}

		public static void RemoveCommunicationCharacter(long ownerId)
		{
			if (_communacationCharacterDict.TryGetValue(ownerId, out long id) && MapController.InstanceExists)
				MapController.Instance.RemoveObjectDebug(id);
			_communacationCharacterDict.Remove(ownerId);
		}
	}
}
#endif // ENABLE_CHEATING
