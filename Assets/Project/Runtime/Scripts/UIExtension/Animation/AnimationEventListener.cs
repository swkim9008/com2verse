/*===============================================================
* Product:		Com2Verse
* File Name:	AnimationEventDispatcher.cs
* Developer:	tlghks1009
* Date:			2022-05-13 17:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.UI
{
	[Serializable]
	public class UnityAnimationEvent : UnityEvent<string> { };

	[AddComponentMenu("[CVUI]/[CVUI] AnimationEventListener")]
	[RequireComponent(typeof(UnityEngine.Animation))]
	public class AnimationEventListener : MonoBehaviour
	{
		[Serializable]
		public class UnityAnimationEventHandler
		{
			[SerializeField] private string _animationName;
			[SerializeField] private UnityAnimationEvent _onStartEvent;
			[SerializeField] private UnityAnimationEvent _onCompletedEvent;

			public string AnimationName => _animationName;

			public UnityAnimationEventHandler(string animationName)
			{
				_animationName = animationName;
			}

			public void OnCompleted(UnityAction<string> onFunc)
			{
				_onStartEvent.AddListener(onFunc);
			}

			public void InvokeStartEvent()
			{
				_onStartEvent?.Invoke(_animationName);
			}

			public void InvokeCompletedEvent()
			{
				_onCompletedEvent?.Invoke(_animationName);
			}
		}

		[SerializeField, NonReorderable] private List<UnityAnimationEventHandler> _animationEventHandlerList;

		private AnimationPlayer _animationPlayer;

		private void Awake()
		{
			_animationPlayer = GetComponent<AnimationPlayer>();

			RegisterAnimationEvent();
		}


		private void RegisterAnimationEvent()
		{
			foreach (var animationEventHandler in _animationEventHandlerList)
			{
				var animationUnit = _animationPlayer.GetAnimationUnit(animationEventHandler.AnimationName);
				if (animationUnit == null)
				{
					continue;
				}

				animationUnit.AddHandler(OnAnimationCompleteHandler);
			}
		}


		private void OnAnimationCompleteHandler(string animationName)
		{
			GetUnityAnimationEventHandler(animationName)?.InvokeCompletedEvent();
		}


		private UnityAnimationEventHandler GetUnityAnimationEventHandler(string animationName)
		{
			foreach (var animationEventHandler in _animationEventHandlerList)
			{
				if (animationEventHandler.AnimationName == animationName)
					return animationEventHandler;
			}

			return null;
		}
	}
}
