/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarStateMachineJumpReady.cs
* Developer:	eugene9721
* Date:			2023-03-09 10:44
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.AvatarAnimation;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.Project.Animation
{
	public sealed class AvatarStateMachineJumpReady : StateMachineBehaviour
	{
		private AvatarAnimatorController? _animatorController;

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (_animatorController.IsUnityNull() && !animator.TryGetComponent(out _animatorController)) return;
			_animatorController!.OnJumpReadyEnd();
		}
	}
}
