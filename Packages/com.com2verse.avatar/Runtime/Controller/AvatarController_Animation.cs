/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarController_Animation.cs
* Developer:	tlghks1009
* Date:			2022-06-07 17:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.AvatarAnimation;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.Avatar
{
	[RequireComponent(typeof(Animator))]
	public partial class AvatarController
	{
		public AvatarAnimatorController AvatarAnimatorController { get; private set; }
		
		private Animator _animator;

		private void FindAnimator()
		{
			_animator                = GetComponent<Animator>();
			AvatarAnimatorController = gameObject.GetOrAddComponent<AvatarAnimatorController>();
			AvatarAnimatorController.Initialize();
		}


		public void SetRuntimeAnimatorController(RuntimeAnimatorController runtimeAnimatorController)
		{
			_animator.runtimeAnimatorController = runtimeAnimatorController;
		}
	}
}
