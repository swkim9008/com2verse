/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarStateMachineJumpLand.cs
* Developer:	haminjeong
* Date:			2022-12-19 14:37
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
	public sealed class AvatarStateMachineJumpLand : StateMachineBehaviour
	{
		private AvatarAnimatorController? _activeObject;

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (_activeObject.IsUnityNull() && !animator.TryGetComponent(out _activeObject)) return;
			if (_activeObject!.JumpPrevState == Protocols.CharacterState.JumpStart) return;

			var animatorState = (Protocols.CharacterState)animator.GetInteger(AnimationDefine.HashState);
			switch (animatorState)
			{
				case Protocols.CharacterState.JumpStart:
					animator.SetInteger(AnimationDefine.HashState, (int)_activeObject.JumpPrevState);
					break;
			}
		}
	}
}
