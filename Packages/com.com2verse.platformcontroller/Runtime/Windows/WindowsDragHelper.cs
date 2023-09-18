/*===============================================================
* Product:		Com2Verse
* File Name:	WindowsDragHelper.cs
* Developer:	mikeyid77
* Date:			2023-02-10 15:00
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Com2Verse.PlatformControl.Windows
{
	internal class WindowsDragHelper : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
	    public delegate void OutAction<T1, T2>(out T1 left, out T2 top);

	    private RectTransform _targetObject = null;
	    private RectTransform _objectRoot;
	    private float _attach = 50f;
	    private float _widthRatio, _heightRatio;
        
        private Vector2 _targetVector;
        private float _windowHeightDefault;
        private OutAction<int, int> _getWindowAnchor = null;
        private Action<int, int> _setWindowPosition = null;
        private Action _setCurrentDisplay = null;

        public void Initialize(RectTransform targetObject, RectTransform objectRoot, OutAction<int, int> getWindowAnchor, Action<int, int> setWindowPosition, Action setCurrentDisplay)
        {
	        _targetObject = targetObject;
	        _objectRoot = objectRoot;
	        _getWindowAnchor = getWindowAnchor;
	        _setWindowPosition = setWindowPosition;
	        _setCurrentDisplay = setCurrentDisplay;
        }

        public void Terminate()
        {
	        var dragHelper = gameObject.GetComponent<WindowsDragHelper>();
	        Destroy(dragHelper);
        }
        
        public void OnBeginDrag(PointerEventData eventData)
		{
			//Cursor.SetCursor(_mousePointer, new Vector2(_mousePointer.width, _mousePointer.height) / 2, CursorMode.Auto);
			if (PlatformController.Instance.IsWorkspaceState())
			{
#if !UNITY_EDITOR
				_targetVector = eventData.position;
#endif
			}
			else
			{
				_widthRatio = Screen.width / _targetObject.sizeDelta.x;
				_heightRatio = Screen.height / _targetObject.sizeDelta.y;
				_targetVector = _objectRoot.anchoredPosition - MousePointerToObjectPosition(eventData.position);
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (PlatformController.Instance.IsWorkspaceState())
			{
#if !UNITY_EDITOR
				Vector2 currentVector = eventData.position - _targetVector;
				
				_getWindowAnchor(out int left, out int top);
				var x = left + (int)currentVector.x;
				var y = top - (int)currentVector.y;

				// if (x < _attach)
				// 	x = 0;
				// else if (x + Screen.width > WindowsController.CurrentDisplay.Width - _attach)
				// 	x = WindowsController.CurrentDisplay.Width - Screen.width;
				//
				// if (y < _attach)
				// 	y = 0;
				// else if (y + _windowHeightDefault > WindowsController.CurrentDisplay.Height - _attach)
				// 	y = WindowsController.CurrentDisplay.Height - (int)(_windowHeightDefault);

				_setWindowPosition?.Invoke(x, y);
#endif
			}
			else
			{
				var currentVector = _targetVector + MousePointerToObjectPosition(eventData.position);
				var x = currentVector.x;
				var y = currentVector.y;

				if (x < _attach)
					x = 0;
				else if (x + _objectRoot.sizeDelta.x > (_targetObject.sizeDelta.x - _attach))
					x = _targetObject.sizeDelta.x - _objectRoot.sizeDelta.x;
				
				if (y - _objectRoot.sizeDelta.y < _attach - _targetObject.sizeDelta.y)
					y = _objectRoot.sizeDelta.y - _targetObject.sizeDelta.y;
				else if (y > -_attach)
					y = 0;

				_objectRoot.anchoredPosition = new Vector2(x, y);
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			//Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			if (PlatformController.Instance.IsWorkspaceState())
			{
#if !UNITY_EDITOR
				_targetVector = Vector2.zero;
				_setCurrentDisplay?.Invoke();
#endif
			}
			else
			{
				_targetVector = Vector2.zero;
				_widthRatio = _heightRatio = 0f;
			}
		}
		
		private Vector2 MousePointerToObjectPosition(Vector2 mousePosition)
		{
			var width = mousePosition.x / _widthRatio;
			var height = mousePosition.y / _heightRatio - _targetObject.sizeDelta.y;
			return new Vector2(width, height);
		}
    }
}

