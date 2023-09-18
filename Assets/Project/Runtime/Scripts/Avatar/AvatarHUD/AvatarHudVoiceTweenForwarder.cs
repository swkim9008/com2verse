/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarHudSpeakerFinder.cs
* Developer:	tlghks1009
* Date:			2023-09-08 15:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Com2Verse.Tweener;

namespace Com2Verse.UI
{
	public sealed class AvatarHudVoiceTweenForwarder : MonoBehaviour
	{
		enum VoiceControllerType
		{
			SPEAKING,
			SPEAKER
		}

		[SerializeField] private TweenBase           _tween;
		[SerializeField] private VoiceControllerType _voiceControllerType;

		public void AllocateController(TweenController speakingTweenController, TweenController speakerTweenController)
		{
			_tween.TweenController = _voiceControllerType switch
			{
				VoiceControllerType.SPEAKER  => speakerTweenController,
				VoiceControllerType.SPEAKING => speakingTweenController,
				_                            => _tween.TweenController
			};
		}
	}
}
