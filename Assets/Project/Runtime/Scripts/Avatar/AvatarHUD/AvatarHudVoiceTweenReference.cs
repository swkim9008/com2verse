/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarHudVoiceTweenController.cs
* Developer:	tlghks1009
* Date:			2023-09-08 15:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using UnityEngine;
using Com2Verse.Tweener;

namespace Com2Verse.UI
{
	public sealed class AvatarHudVoiceTweenReference : MonoBehaviour
	{
		[SerializeField] private List<AvatarHudVoiceTweenForwarder> _forwarder;


		public void SetTweenController(TweenController speakingTweenController, TweenController speakerTweenController)
		{
			ForwardTweenController(speakingTweenController, speakerTweenController);
		}

		private void ForwardTweenController(TweenController speakingTweenController, TweenController speakerTweenController)
		{
			foreach (var forwarder in _forwarder)
			{
				forwarder.AllocateController(speakingTweenController, speakerTweenController);
			}
		}
	}
}
