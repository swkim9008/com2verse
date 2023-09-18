/*===============================================================
* Product:		Com2Verse
* File Name:	ActiveObjectCreator.cs
* Developer:	haminjeong
* Date:			2022-12-28 19:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Avatar;
using Com2Verse.AvatarAnimation;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.SoundSystem;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Network
{
	[UsedImplicitly]
	[Definition(1)]
	public sealed class ActiveObjectCreator : BaseObjectCreator
	{
		public override void Initialize(Func<long, long, bool> checkExist, Func<long, int, BaseMapObject> checkPool, Transform rootTrans)
		{
			AvatarAnimatorController.TargetMixerGroup = (int)eAudioMixerGroup.SFX;
			
			SetDelegates((serial, definition, data, initialPosition, onCompleted) =>
			{
#if UNITY_EDITOR && !METAVERSE_RELEASE
				if (MapController.Instance.GetCreateInfo(serial).ObjState.OwnerId != MapController.Instance.UserID && !MapController.Instance.IsEnableAvatarCreate)
					return;
#endif
				if (checkExist.Invoke(serial, definition))
					return;
				var avatar = data as Protocols.Avatar;
				if (avatar == null) return;
				var mapObject = checkPool.Invoke(definition, avatar.AvatarType);
				if (mapObject.IsReferenceNull())
				{
					CreateAvatarAsync(serial, avatar, initialPosition, onCompleted, EnableAvatar).Forget();
				}
				else
				{
					UpdatePartsItems(mapObject, serial, avatar, initialPosition, onCompleted, EnableAvatar).Forget();
				}
			});
		}

		private async UniTask CreateAvatarAsync(long serialId, Protocols.Avatar avatar, Vector3 initialPosition, Action<long, BaseMapObject>? onCompletedBaseBody, Action<GameObject>? onCompletedAllParts = null)
		{
			var avatarInfo = new AvatarInfo(serialId, avatar);

			// FIXME: 임시처리
			var faceOptions = avatarInfo.GetFaceOptionList();
			if (faceOptions.Count == 1)
			{
				var presetData = avatarInfo.GetFaceOption(eFaceOption.PRESET_LIST);
				if (presetData != null)
					avatarInfo = AvatarManager.Instance.GetFacePresetInfo(presetData.ItemId, avatarInfo, false);
			}

			AvatarController avatarController = await AvatarCreator.CreateAvatarAsync(avatarInfo, eAnimatorType.WORLD, initialPosition, (int)Define.eLayer.CHARACTER, false, onCompletedAllParts);
			onCompletedBaseBody?.Invoke(avatarController.Info.SerialId, Util.GetOrAddComponent<ActiveObject>(avatarController.transform.parent.gameObject));
		}

		private async UniTask UpdatePartsItems(BaseMapObject mapObject, long serialId, Protocols.Avatar avatar, Vector3 initialPosition, Action<long, BaseMapObject>? onCompletedBaseBody, Action<GameObject>? onCompletedAllParts = null)
		{
			var baseBody = mapObject!.transform.Find(MetaverseAvatarDefine.BaseBodyObjectName);
			if (baseBody.IsUnityNull()) return;

			mapObject.transform.position = initialPosition;

			AvatarController avatarController = baseBody.gameObject.GetOrAddComponent<AvatarController>();
			avatarController.Create(new AvatarInfo(serialId, avatar), eAnimatorType.WORLD, (int)Define.eLayer.CHARACTER);
			onCompletedBaseBody?.Invoke(avatarController.Info.SerialId, Util.GetOrAddComponent<ActiveObject>(avatarController.transform.parent.gameObject));

			await AvatarCreator.UpdateAvatarParts(avatarController, false, onCompletedAllParts);
		}

		public override void ReleaseObject(BaseMapObject mapObject)
		{
			AvatarController avatarController = mapObject.GetComponentInChildren<AvatarController>(true);
			if (!avatarController.IsUnityNull())
				avatarController!.OnRelease();
		}

		private void EnableAvatar(GameObject avatar)
		{
			if(avatar == null)
				return;

			avatar.SetActive(true);
		}
	}
}
