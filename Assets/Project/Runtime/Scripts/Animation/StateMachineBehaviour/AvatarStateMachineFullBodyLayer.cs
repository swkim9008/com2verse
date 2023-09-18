/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarStateMachineUpperBodyLayer.cs
* Developer:	eugene9721
* Date:			2023-05-24 17:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.AvatarAnimation;
using UnityEngine;

namespace Com2Verse.Project.Animation
{
	public sealed class AvatarStateMachineFullBodyLayer : StateMachineBehaviour
	{
		private bool _hasMapObject;

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			SetStateLowerBodyLayer(animator, stateInfo, layerIndex);
		}

		private void SetStateLowerBodyLayer(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (stateInfo.shortNameHash == AnimationDefine.HashDefaultState)
				return;

			SetUpperBodyLayer(animator, stateInfo, layerIndex);
		}

		private void SetUpperBodyLayer(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			float crossFadeDuration = animator.GetAnimatorTransitionInfo(layerIndex).duration;
			animator.CrossFade(stateInfo.shortNameHash, crossFadeDuration, AnimationDefine.UpperBodyLayerIndex);
		}
	}
}
