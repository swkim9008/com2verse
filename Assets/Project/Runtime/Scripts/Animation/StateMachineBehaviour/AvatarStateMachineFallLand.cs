/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarStateMachineFallLand.cs
* Developer:	eugene9721
* Date:			2022-07-05 15:07
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.AvatarAnimation;
using Com2Verse.Extension;
using Com2Verse.PlayerControl;
using UnityEngine;

namespace Com2Verse.Project.Animation
{
	public sealed class AvatarStateMachineFallLand : StateMachineBehaviour
	{
		private AvatarAnimatorController? _avatar;

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (_avatar.IsUnityNull() && !animator.TryGetComponent(out _avatar)) return;
			if (_avatar!.IsMine) PlayerController.Instance.SetCanMoveAnimation(false);
		}

		// 주의: 해당 Transition에 Interruption Source 사용시 OnStateExit이 실행되지 않을 수 있음 
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (_avatar.IsUnityNull() && !animator.TryGetComponent(out _avatar)) return;
			if (_avatar!.IsMine) PlayerController.Instance.SetCanMoveAnimation(true);
		}
	}
}
