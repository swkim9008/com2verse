/*===============================================================
* Product:		Com2Verse
* File Name:	TweenStackRegisterer.cs
* Developer:	mikeyid77
* Date:			2023-07-14 15:33
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.Tweener;
using UnityEngine.Events;

namespace Com2Verse.UI
{
	[AddComponentMenu("[UI Stack]/[UI Stack] Tween Stack Registerer")]
	[RequireComponent(typeof(EventTrigger))]
	public sealed class TweenStackRegisterer : StackRegisterer
	{
		[SerializeField] private bool       _restoreIsClose     = true;
		[SerializeField] private UnityEvent _hideCompleteAction = null;
		private TweenBase _tweener = null;

		public UnityEvent HideCompleteEvent
		{
			get => _hideCompleteAction;
			set => _hideCompleteAction = value;
		}

		private void Awake()
		{
			_tweener = gameObject.GetComponent<TweenBase>();
			if (_tweener == null)
			{
				C2VDebug.LogErrorCategory("UIStackManager", $"${nameof(gameObject.transform.parent.name)} : Need Tweener");
			}
			else
			{
				var targetState = (NeedCharacterMove) ? eInputSystemState.CHARACTER_CONTROL : eInputSystemState.UI;
				if (_restoreIsClose)
				{
					_tweener.OnTweeningEvent  += () => AddToManager(gameObject.transform.parent.name, targetState);
					_tweener.OnRestoringEvent += RemoveFromManager;
					_tweener.OnTweenedEvent   += FinishViewEvent;
					_tweener.OnRestoredEvent  += FinishViewEvent;
				}
				else
				{
					_tweener.OnTweeningEvent  += RemoveFromManager;
					_tweener.OnRestoringEvent += () => AddToManager(gameObject.transform.parent.name, targetState);
					_tweener.OnTweenedEvent   += FinishViewEvent;
					_tweener.OnRestoredEvent  += FinishViewEvent;
				}
			}
		}

		private void OnDestroy()
		{
			RemoveFromManager();
			_hideCompleteAction = null;
			_tweener            = null;
		}

		public override void HideComplete()
		{
			_hideCompleteAction?.Invoke();
		}
	}
}
