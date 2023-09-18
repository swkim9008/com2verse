/*===============================================================
* Product:		Com2Verse
* File Name:	TweenBase.cs
* Developer:	hyj
* Date:			2022-04-28 14:15
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Logger;
using Com2Verse.Utils;
using DG.Tweening;
using UnityEngine;

namespace Com2Verse.Tweener
{
	[RequireComponent(typeof(RectTransform))]
	public abstract class TweenBase : MonoBehaviour
	{
		private enum eActiveStateTransition
		{
			IGNORE,
			ACTIVE,
			INACTIVE,
		}

		[SerializeField] protected bool _autoTarget = true;
		[SerializeField, DrawIf(nameof(_autoTarget), false)]
		private TweenController? _controllerOverride;

		[SerializeField] protected bool _overrideTweenStyle;
		[SerializeField, DrawIf(nameof(_overrideTweenStyle), true)]
		private TweenStyle _tweenStyle = TweenStyle.Default;

		private readonly List<TweenController?> _controllers = new();

		private DG.Tweening.Tweener? _tweenToggle;
		private DG.Tweening.Tweener? _tweenOnce;
		private DG.Tweening.Tweener? _tweenLoop;

		[Header("Active State")]
		[SerializeField] private eActiveStateTransition _restoredStateTransition = eActiveStateTransition.IGNORE;
		[SerializeField] private eActiveStateTransition _tweeningStateTransition = eActiveStateTransition.IGNORE;
		[SerializeField] private eActiveStateTransition _tweenedStateTransition  = eActiveStateTransition.IGNORE;

		private Action _onTweening  = null;
		private Action _onTweened   = null;
		private Action _onRestoring = null;
		private Action _onRestored  = null;

		public TweenController? TweenController
		{
			get => _controllerOverride;
			set => _controllerOverride = value;
		}

		public event Action OnTweeningEvent
		{
			add
			{
				_onTweening -= value;
				_onTweening += value;
			}
			remove => _onTweening -= value;
		}

		public event Action OnTweenedEvent
		{
			add
			{
				_onTweened -= value;
				_onTweened += value;
			}
			remove => _onTweened -= value;
		}

		public event Action OnRestoringEvent
		{
			add
			{
				_onRestoring -= value;
				_onRestoring += value;
			}
			remove => _onRestoring -= value;
		}

		public event Action OnRestoredEvent
		{
			add
			{
				_onRestored -= value;
				_onRestored += value;
			}
			remove => _onRestored -= value;
		}

		private void Awake()
		{
			Initialize();
			OnRestoredImmediately();

			_controllers.Add(_autoTarget ? GetComponent<TweenController>() : _controllerOverride);

			foreach (var controller in _controllers)
				if (controller != null)
					AddEvents(controller);
		}

		private void OnDestroy()
		{
			foreach (var controller in _controllers)
				if (controller != null)
					RemoveEvents(controller);

			KillTweens();
		}

		public void ChangeController(TweenController controller)
		{
			if (_autoTarget)
			{
				C2VDebug.LogWarningCategory(nameof(TweenBase), "Cannot change controller when auto target is enabled.");
				return;
			}

			if (_controllerOverride != null)
			{
				RemoveEvents(_controllerOverride);
				if (!_controllers.Remove(_controllerOverride))
					C2VDebug.LogWarningCategory(nameof(TweenBase), "Cannot remove Previous controller.");
			}

			OnRestoredImmediately();
			_controllers.Add(controller);
			AddEvents(controller);

			_controllerOverride = controller;
		}

		private void AddEvents(TweenController controller)
		{
			controller.TweenedImmediately  += OnTweenedImmediately;
			controller.RestoredImmediately += OnRestoredImmediately;
			controller.Tweened             += OnTweened;
			controller.Restored            += OnRestored;
			controller.LoopStarted         += OnLoopStarted;
			controller.LoopStopped         += OnLoopStopped;
			controller.TweenedOnce         += OnTweenedOnce;
		}

		private void RemoveEvents(TweenController controller)
		{
			controller.TweenedImmediately  -= OnTweenedImmediately;
			controller.RestoredImmediately -= OnRestoredImmediately;
			controller.Tweened             -= OnTweened;
			controller.Restored            -= OnRestored;
			controller.LoopStarted         -= OnLoopStarted;
			controller.LoopStopped         -= OnLoopStopped;
			controller.TweenedOnce         -= OnTweenedOnce;
		}

		private void KillTweens()
		{
			_tweenToggle?.Kill();
			_tweenOnce?.Kill();
			_tweenLoop?.Kill();
		}

		protected virtual void Initialize() { }

		protected abstract DG.Tweening.Tweener? Tween(float   duration);
		protected abstract DG.Tweening.Tweener? Restore(float duration);

		protected abstract void TweenImmediately();
		protected abstract void RestoreImmediately();

		private void OnTweenedImmediately()
		{
			KillTweens();
			TweenImmediately();
			ApplyTweenedState();
		}

		private void OnRestoredImmediately()
		{
			KillTweens();
			RestoreImmediately();
			ApplyRestoredState();
		}

		private void OnTweened(TweenStyle style)
		{
			Tween(ref _tweenToggle, true, style, ApplyTweenedState);
		}

		private void OnRestored(TweenStyle style)
		{
			Tween(ref _tweenToggle, false, style, ApplyRestoredState);
		}

		private void OnTweenedOnce(TweenStyle style)
		{
			Tween(ref _tweenOnce, true, style, Callback);

			void Callback()
			{
				Tween(ref _tweenOnce, false, style, ApplyRestoredState);
			}
		}

		private void OnLoopStarted(TweenStyle style)
		{
			Tween(ref _tweenLoop, true, style, Callback);

			void Callback()
			{
				OnLoopComplete(style);
			}
		}

		private void OnLoopComplete(TweenStyle style)
		{
			Tween(ref _tweenLoop, false, style, Callback);

			void Callback()
			{
				OnLoopStarted(style);
			}
		}

		private void OnLoopStopped(TweenStyle style)
		{
			Tween(ref _tweenLoop, false, style, ApplyRestoredState);
		}

		private void Tween(ref DG.Tweening.Tweener? tweener, bool isOn, TweenStyle style, TweenCallback callback)
		{
			tweener?.Kill();

			ApplyTweeningState();

			style = GetTweenStyle(style);

			tweener = isOn
				? Tween(style.TweeningDuration).SetEase(style.TweeningType)
				: Restore(style.RestoringDuration).SetEase(style.RestoringType);

			if (isOn)
				_onTweening?.Invoke();
			else
				_onRestoring?.Invoke();

			tweener.OnComplete(callback);
		}

		private TweenStyle GetTweenStyle(TweenStyle style)
		{
			return _overrideTweenStyle ? _tweenStyle : style;
		}

		private void ApplyRestoredState() => ApplyGameObjectState(_restoredStateTransition, false);
		private void ApplyTweeningState() => ApplyGameObjectState(_tweeningStateTransition, true);
		private void ApplyTweenedState()  => ApplyGameObjectState(_tweenedStateTransition,  true);

		private void ApplyGameObjectState(eActiveStateTransition stateTransition, bool isOn)
		{
			if (isOn)
				_onTweened?.Invoke();
			else
				_onRestored?.Invoke();

			switch (stateTransition)
			{
				case eActiveStateTransition.IGNORE:
					break;
				case eActiveStateTransition.ACTIVE:
					gameObject.SetActive(true);
					break;
				case eActiveStateTransition.INACTIVE:
					gameObject.SetActive(false);
					break;
			}
		}
	}
}
