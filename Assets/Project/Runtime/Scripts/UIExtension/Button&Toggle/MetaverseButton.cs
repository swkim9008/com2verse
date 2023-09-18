/*===============================================================
* Product:		Com2Verse
* File Name:	MetaverseButton.cs
* Developer:	tlghks1009
* Date:			2022-05-10 11:31
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Extension;
using Com2Verse.InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	[AddComponentMenu("[CVUI]/[CVUI] MetaverseButton")]
	[ExecuteInEditMode]
	public sealed class MetaverseButton : Button
	{
		private UnityEngine.Animation _animation;

		private Vector3 _originalScale;

		private bool _isButtonClickable;

		protected override void Awake()
		{
			base.Awake();

			_isButtonClickable = true;

			_animation = gameObject.GetComponentInChildren<UnityEngine.Animation>();
			if (_animation.IsReferenceNull()) return;

			_originalScale = _animation.transform.localScale;
		}

		protected override void DoStateTransition(SelectionState state, bool instant)
		{
			base.DoStateTransition(state, instant);

			switch (state)
			{
				case SelectionState.Disabled:
					_onDisabledEvent?.Invoke();
					break;

				case SelectionState.Highlighted:
					_onHighlightedEvent?.Invoke();
					break;

				case SelectionState.Normal:
					_onNormalEvent?.Invoke();
					break;

				case SelectionState.Pressed:
					_onPressedEvent?.Invoke();
					break;

				case SelectionState.Selected:
					_onSelectedEvent?.Invoke();
					break;
			}
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;
			if (!_isButtonClickable || !InputSystemManager.CanClick())
				return;

			base.OnPointerDown(eventData);

			PlayPointerDownAnimation();
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;
			if (!_isButtonClickable || !InputSystemManager.CanClick())
				return;

			base.OnPointerClick(eventData);

			StartTimerWhenButtonClicked();

			PlayPointerClickAnimation();

			if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
		}

		private void PlayPointerDownAnimation()
		{
			if (!_animation.IsReferenceNull() && base.interactable)
				_animation.Play();
		}

		private void PlayPointerClickAnimation()
		{
			if (!_animation.IsReferenceNull() && base.interactable)
				_animation.transform.localScale = _originalScale;
		}


		private void StartTimerWhenButtonClicked()
		{
			if (InputSystemManager.BlockInterval > 0)
			{
				_isButtonClickable = false;
				InputSystemManager.RequestMouseBlock();
				UIManager.Instance.StartTimer(InputSystemManager.BlockInterval,
				                              () =>
				                              {
					                              _isButtonClickable = true;
					                              InputSystemManager.RequestMouseUnblock();
				                              });
			}
		}

		public bool IsInteractableInversed
		{
			get => !interactable;
			set => interactable = !value;
		}

#region Transition Events
		private Action _onDisabledEvent;
		private Action _onHighlightedEvent;
		private Action _onNormalEvent;
		private Action _onPressedEvent;
		private Action _onSelectedEvent;
#endregion Transition Events

#region Button Event
		public event Action OnDisabledEvent
		{
			add
			{
				_onDisabledEvent -= value;
				_onDisabledEvent += value;
			}
			remove => _onDisabledEvent -= value;
		}

		public event Action OnHighlightedEvent
		{
			add
			{
				_onHighlightedEvent -= value;
				_onHighlightedEvent += value;
			}
			remove => _onHighlightedEvent -= value;
		}

		public event Action OnNormalEvent
		{
			add
			{
				_onNormalEvent -= value;
				_onNormalEvent += value;
			}
			remove => _onNormalEvent -= value;
		}
#endregion Button Event
	}
}
