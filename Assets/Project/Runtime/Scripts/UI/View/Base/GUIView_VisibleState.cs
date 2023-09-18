/*===============================================================
* Product:    Com2Verse
* File Name:  GUIView_ActiveState.cs
* Developer:  tlghks1009
* Date:       2022-04-08 15:03
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Sound;
using Com2Verse.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	public partial class GUIView
	{
		public enum eActiveTransitionType
		{
			NONE,
			ANIMATION,
			FADE
		}

		public enum eVisibleState
		{
			NONE,
			OPENING,
			OPENED,
			CLOSING,
			CLOSED
		}

		public enum eDefaultActiveState
		{
			SHOW,
			HIDE,
			NONE
		}

		private eVisibleState _visibleState = eVisibleState.NONE;

		[HideInInspector] [SerializeField]            private UnityEvent            OnActivated;
		[HideInInspector] [SerializeField]            private UnityEvent            OnInactivated;
		[HideInInspector] [SerializeField]            private UnityEvent<bool>      OnActiveChanged;
		[HideInInspector] [SerializeField]            private UnityEvent<bool>      OnClosed;
		[HideInInspector] [SerializeField]            private eActiveTransitionType _transitionType = eActiveTransitionType.NONE;
		[HideInInspector] [SerializeField]            private eDefaultActiveState   _defaultActiveState;
		[HideInInspector] [SerializeField]            private AnimationPlayer       _animationPlayer;
		[HideInInspector] [SerializeField]            private float                 _fadeSpeed = 1f;
		[HideInInspector] [SerializeField]            private bool                  _isPlaySound;
		[HideInInspector] [SerializeField]            private AssetReference        _audioFileWhenActivated;
		[HideInInspector] [SerializeField]            private AssetReference        _audioFileWhenInactivated;
		[HideInInspector] [ReadOnly] [SerializeField] private string                _viewName;

		public bool IsStatic { get; set; }

		public eVisibleState VisibleState
		{
			get => _visibleState;
			set => _visibleState = value;
		}

		public AnimationPlayer AnimationPlayer => _animationPlayer;

		public float FadeSpeed => _fadeSpeed;

		public eActiveTransitionType ActiveTransitionType => _transitionType;

		public string ViewName => _viewName;


		public void OnFocused()
		{
			_onFocusEvent?.Invoke(this);
		}

		protected virtual void Activate()
		{
			ActiveOn();

			SetVisibleState(eVisibleState.OPENING);

			PlayUISound(_audioFileWhenActivated);
		}

		protected virtual void Deactivate()
		{
			SetVisibleState(eVisibleState.CLOSING);

			PlayUISound(_audioFileWhenInactivated);
		}


		private void OnOpeningState()
		{
			_visibleState = eVisibleState.OPENING;

			OnActiveChanged?.Invoke(true);
			_onOpeningEvent?.Invoke(this);

			GuiViewActionController.PlayAction(this, () =>
			{
				SetVisibleState(eVisibleState.OPENED);
			});
		}


		private void OnOpenedState()
		{
			_visibleState = eVisibleState.OPENED;

			_onOpenedEvent?.Invoke(this);
			InvokeCompletedEvent();
		}


		private void OnClosingState()
		{
			_visibleState = eVisibleState.CLOSING;

			OnActiveChanged?.Invoke(false);
			_onClosingEvent?.Invoke(this);

			GuiViewActionController.PlayAction(this, () =>
			{
				SetVisibleState(eVisibleState.CLOSED);
			});
		}

		private void OnClosedState()
		{
			_visibleState = eVisibleState.CLOSED;

			ActiveOff();

			OnClosed?.Invoke(true);
			_onClosedEvent?.Invoke(this);
			InvokeCompletedEvent();

			Unbind();

			if (_allowDuplicate)
				GameObject.Destroy(this.gameObject);
		}


		private void InvokeCompletedEvent()
		{
			_onCompletedEvent?.Invoke(this);
			_onCompletedEvent = null;
		}


		private void SetVisibleState(eVisibleState state)
		{
			_visibleState = state;

			switch (_visibleState)
			{
				case eVisibleState.CLOSED:
					OnClosedState();
					break;

				case eVisibleState.CLOSING:
					OnClosingState();
					break;

				case eVisibleState.OPENED:
					OnOpenedState();
					break;

				case eVisibleState.OPENING:
					OnOpeningState();
					break;
			}
		}


		private void PlayUISound(AssetReference audioFile)
		{
			if (_isPlaySound)
			{
				if (audioFile != null && !string.IsNullOrEmpty(audioFile.AssetGUID))
					SoundManager.Instance.PlayUISound(audioFile);
			}
		}


		private void ActiveOn()
		{
			if (!gameObject.activeSelf)
			{
				gameObject.SetActive(true);
			}

			foreach (var childCanvas in _childCanvases)
			{
				childCanvas.enabled = true;
			}
		}

		private void ActiveOff()
		{
			foreach (var childCanvas in _childCanvases)
			{
				childCanvas.enabled = false;
			}
		}

#region EventProperties
		private Action<GUIView> _onFocusEvent;
		private Action<GUIView> _onOpeningEvent;
		private Action<GUIView> _onOpenedEvent;
		private Action<GUIView> _onClosingEvent;
		private Action<GUIView> _onClosedEvent;
		private Action<GUIView> _onCompletedEvent;
		private Action<GUIView> _onDestroyedEvent;

		public event Action<GUIView> OnFocusEvent
		{
			add
			{
				_onFocusEvent -= value;
				_onFocusEvent += value;
			}
			remove => _onFocusEvent -= value;
		}

		public event Action<GUIView> OnOpeningEvent
		{
			add
			{
				_onOpeningEvent -= value;
				_onOpeningEvent += value;
			}
			remove => _onOpeningEvent -= value;
		}

		public event Action<GUIView> OnOpenedEvent
		{
			add
			{
				_onOpenedEvent -= value;
				_onOpenedEvent += value;
			}
			remove => _onOpenedEvent -= value;
		}

		public event Action<GUIView> OnClosingEvent
		{
			add
			{
				_onClosingEvent -= value;
				_onClosingEvent += value;
			}
			remove => _onClosingEvent -= value;
		}

		public event Action<GUIView> OnClosedEvent
		{
			add
			{
				_onClosedEvent -= value;
				_onClosedEvent += value;
			}
			remove => _onClosedEvent -= value;
		}

		public event Action<GUIView> OnCompletedEvent
		{
			add
			{
				_onCompletedEvent -= value;
				_onCompletedEvent += value;
			}
			remove => _onCompletedEvent -= value;
		}

		public event Action<GUIView> OnDestroyedEvent
		{
			add
			{
				_onDestroyedEvent -= value;
				_onDestroyedEvent += value;
			}
			remove => _onDestroyedEvent -= value;
		}

		private void UnregisterEvents()
		{
			_onFocusEvent = null;
			_onOpenedEvent = null;
			_onOpeningEvent = null;
			_onClosedEvent = null;
			_onClosingEvent = null;
			_onDestroyedEvent = null;
		}
#endregion EventProperties
	}
}
