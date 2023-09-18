/*===============================================================
* Product:    Com2Verse
* File Name:  GUIView.cs
* Developer:  tlghks1009
* Date:       2022-03-03 14:19
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using Com2Verse.Logger;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	public abstract partial class GUIView : MonoBehaviour
	{
		public IUpdateOrganizer UpdateOrganizer { get; set; }

		private Canvas       _canvas;
		private Canvas[]     _childCanvases;
		private CanvasScaler _canvasScaler;
		private CanvasGroup  _canvasGroup;

		public CanvasGroup CanvasGroup
		{
			get
			{
				if (_canvasGroup.IsUnityNull())
				{
					_canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
				}

				return _canvasGroup;
			}
		}

		public Canvas       Canvas       => _canvas;
		public CanvasScaler CanvasScaler => _canvasScaler;

		private void Awake()
		{
			if (_dontDestroyOnLoad)
			{
				DontDestroyOnLoad(this.gameObject);
			}

			AttachUIFocusController();
			FindInitialComponentsIfNotAssigned();
		}

		public GUIView Show()
		{
			FindInitialComponentsIfNotAssigned();
			OnFocused();

			if (_visibleState is eVisibleState.OPENED or eVisibleState.OPENING)
			{
				return this;
			}

			UpdateOrganizer?.AddUpdateListener(OnUpdate);

			Activate();
			Bind();
			OnActivated?.Invoke();
			return this;
		}


		public GUIView Hide()
		{
			FindInitialComponentsIfNotAssigned();

			if (_visibleState is eVisibleState.CLOSED or eVisibleState.CLOSING)
			{
				return this;
			}

			UpdateOrganizer?.RemoveUpdateListener(OnUpdate);

			Deactivate();
			OnInactivated?.Invoke();
			return this;
		}

		public GUIView SetActive(bool active) => active ? Show() : Hide();

		public GUIView SetDefaultActive()
		{
			switch (_defaultActiveState)
			{
				case eDefaultActiveState.SHOW:
					SetActive(true);
					break;
				case eDefaultActiveState.HIDE:
					SetActive(false);
					break;
			}

			return this;
		}

		protected virtual void OnUpdate() { }

		private void OnDestroy()
		{
			ForceUnbind();

			ResetDataBinding();

			_onDestroyedEvent?.Invoke(this);
			UnregisterEvents();
		}

		private void AttachUIFocusController()
		{
			var thisTransform = transform;

			if (thisTransform.childCount == 0) return;

			var childTransform = thisTransform.GetChild(0).gameObject.GetComponent<UIFocusNotifier>();
			if (childTransform.IsUnityNull())
				thisTransform.GetChild(0).gameObject.AddComponent<UIFocusNotifier>();
		}


		public void PositionInitialization()
		{
			var rectTransform = this.GetComponent<RectTransform>();
			rectTransform.anchorMin        = Vector2.zero;
			rectTransform.anchorMax        = Vector2.one;
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.offsetMin        = Vector2.zero;
			rectTransform.offsetMax        = Vector2.zero;
			rectTransform.localScale       = Vector3.one;
		}


		private void FindInitialComponentsIfNotAssigned()
		{
			if (_canvas.IsUnityNull())
			{
				_canvas = gameObject.GetOrAddComponent<Canvas>();
			}

			if (_childCanvases == null)
			{
				_childCanvases = gameObject.GetComponentsInChildren<Canvas>();
			}

			if (_canvasScaler.IsUnityNull())
			{
				_canvasScaler = gameObject.GetComponentInParent<CanvasScaler>();
			}
		}

		public void DeactivateForce()
		{
			FindInitialComponentsIfNotAssigned();
			OnClosedState();
		}
	}
}
