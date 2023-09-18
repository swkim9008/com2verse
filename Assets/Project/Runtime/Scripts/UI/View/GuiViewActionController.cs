/*===============================================================
* Product:		Com2Verse
* File Name:	GuiViewActionController.cs
* Developer:	tlghks1009
* Date:			2022-10-25 19:38
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Extension;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.UI
{
	public static class GuiViewActionController
	{
		private static Dictionary<GUIView.eActiveTransitionType, BaseGuiViewAction> _guiViewActionDict = new();

		private static bool _isInitialized;


		public static void PlayAction(GUIView guiView, Action onActionFinished)
		{
			if (!_isInitialized)
				Initialize();

			if (_guiViewActionDict.TryGetValue(guiView.ActiveTransitionType, out var guiViewAction))
			{
				guiViewAction.Set(guiView);

				guiViewAction.PlayAction(onActionFinished);
			}
		}


		private static void Initialize()
		{
			RegisterTransitionAction();
		}


		private static void RegisterTransitionAction()
		{
			AddGuiViewAction(GUIView.eActiveTransitionType.NONE, new GuiViewNoneAction());
			AddGuiViewAction(GUIView.eActiveTransitionType.ANIMATION, new GuiViewAnimationAction());
			AddGuiViewAction(GUIView.eActiveTransitionType.FADE, new GuiViewFadeAction());

			_isInitialized = true;
		}

		private static void AddGuiViewAction(GUIView.eActiveTransitionType activeTransitionType, BaseGuiViewAction guiViewAction)
		{
			if (_guiViewActionDict.ContainsKey(activeTransitionType))
				return;

			_guiViewActionDict.Add(activeTransitionType, guiViewAction);
		}
	}


	public abstract class BaseGuiViewAction
	{
		protected GUIView _guiView;

		public void Set(GUIView guiView)
		{
			_guiView = guiView;
		}

		public abstract void PlayAction(Action onActionFinished);
	}


	public class GuiViewNoneAction : BaseGuiViewAction
	{
		public override void PlayAction(Action onActionFinished)
		{
			onActionFinished?.Invoke();
		}
	}


	public class GuiViewAnimationAction : BaseGuiViewAction
	{
		public override void PlayAction(Action onActionFinished)
		{
			int animationClipIndex = _guiView.VisibleState == GUIView.eVisibleState.OPENING ? 0 : 1;

			var clip = _guiView.AnimationPlayer.GetClipByIndex(animationClipIndex);
			clip?.SampleAnimation(_guiView.AnimationPlayer.gameObject, animationClipIndex);

			var animationUnit = _guiView.AnimationPlayer.Play(animationClipIndex);

			if (animationUnit == null)
			{
				onActionFinished?.Invoke();
			}
			else
			{
				animationUnit.OnFinished((animationName) =>
				{
					onActionFinished?.Invoke();
				});
			}
		}
	}


	public class GuiViewFadeAction : BaseGuiViewAction
	{
		public override void PlayAction(Action onActionFinished)
		{
			if (_guiView.VisibleState == GUIView.eVisibleState.OPENING)
			{
				OnFadeProc(UIFadeView.eFadeType.IN, onActionFinished).Forget();
			}
			else
			{
				if (_guiView.CanvasGroup.IsReferenceNull())
				{
					onActionFinished?.Invoke();

					return;
				}

				OnFadeProc(UIFadeView.eFadeType.OUT, onActionFinished).Forget();
			}
		}

		private async UniTask OnFadeProc(UIFadeView.eFadeType fadeType, Action onCompleted)
		{
			float progress = 0;
			float min = fadeType == UIFadeView.eFadeType.IN ? 0 : 1f;
			float max = fadeType == UIFadeView.eFadeType.IN ? 1f : 0;

			while (true)
			{
				await UniTask.Yield(cancellationToken: _guiView.GetCancellationTokenOnDestroy());

				if (progress > 1) break;
				progress += Time.deltaTime / _guiView.FadeSpeed;

				var alpha = Mathf.Lerp(min, max, progress);

				if (_guiView.CanvasGroup.IsReferenceNull())
				{
					onCompleted?.Invoke();

					return;
				}
				_guiView.CanvasGroup.alpha = alpha;
			}

			int alphaMax = fadeType == UIFadeView.eFadeType.IN ? 1 : 0;
			_guiView.CanvasGroup.alpha = alphaMax;
			onCompleted?.Invoke();
		}
	}
}
