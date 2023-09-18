/*===============================================================
* Product:		Com2Verse
* File Name:	MiceSeat.cs
* Developer:	wlemon
* Date:			2023-05-23 15:00
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections;
using Com2Verse.AvatarAnimation;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.PlayerControl;
using Com2Verse.Rendering.Utility;
using Protocols;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Com2Verse.Mice
{
	public sealed class MiceSeat : MonoBehaviour
	{
		[SerializeField]
		private bool isMySeat;

		private ActiveObject      _activeObject;
		private WaitForEndOfFrame _waitForEndOfFrame = new();

		public MiceSeatController.Group Group { get; private set; } = default;

		public bool IsMySeat => this.isMySeat;
		

		public bool IsUsing { get; private set; } = false;

		public void SetGroup(MiceSeatController.Group group)
		{
			Group = group;
		}

		public void Use(ActiveObject activeObject)
		{
			IsUsing       = true;
			_activeObject = activeObject;
			_activeObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
			_activeObject.ForceSetCurrentPositionToState();

			_activeObject.gameObject.FlagRenderingLayerMask(false, RenderStateUtility.eRenderingLayerMask.UNUSED_8);
			_activeObject.gameObject.FlagRenderingLayerMask(true,  RenderStateUtility.eRenderingLayerMask.UNUSED_9);

			var avatarController = _activeObject.AvatarController;
			if (_activeObject == User.Instance.CharacterObject)
			{
				PlayerController.Instance.Teleport(transform.position, transform.rotation);
				PlayerController.Instance.ForceSetCharacterState(CharacterState.Sit);
			}

			avatarController.AvatarAnimatorController.SetAnimatorState((int)CharacterState.Sit, -1);
			_activeObject.ForceSetCurrentCharacterStateToState();
		}

		public void Clear(ActiveObject activeObject)
		{
			IsUsing       = false;
			_activeObject = null;
			if (activeObject == User.Instance.CharacterObject)
			{
				PlayerController.Instance.ForceSetCharacterState(CharacterState.IdleWalkRun);
			}
		}
	}
}
