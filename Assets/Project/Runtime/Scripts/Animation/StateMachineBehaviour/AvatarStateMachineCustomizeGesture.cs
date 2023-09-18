/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarStateMachineCustomizeGesture.cs
* Developer:	eugene9721
* Date:			2023-08-02 15:21
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Avatar;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.Project.Animation
{
	public sealed class AvatarStateMachineCustomizeGesture : StateMachineBehaviour
	{
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var jigController = AvatarMediator.Instance.AvatarCloset.AvatarJig;
			if (!jigController.IsUnityNull())
				jigController!.IsOnGesture = true;
		}

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var jigController = AvatarMediator.Instance.AvatarCloset.AvatarJig;
			if (!jigController.IsUnityNull())
				jigController!.IsOnGesture = false;
		}
	}
}
