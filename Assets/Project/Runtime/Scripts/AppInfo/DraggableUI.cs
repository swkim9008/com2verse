/*===============================================================
* Product:		Com2Verse
* File Name:	DraggableUI.cs
* Developer:	jhkim
* Date:			2022-06-17 14:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Com2Verse
{
	public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[Header("Draggable UI")]
		[SerializeField] private RectTransform _panel;
		[SerializeField] private bool _allowOutOfScreen;
		private bool _disabled = false;
		public bool Disabled
		{
			get => _disabled;
			set => _disabled = value;
		}

#region Drag
		// http://gyanendushekhar.com/2019/11/11/move-canvas-ui-mouse-drag-unity-3d-drag-drop-ui/
		private Vector2 lastMousePosition;

		public void OnDrag(PointerEventData data)
		{
			if (Mouse.current.rightButton.isPressed) return;
			if (Disabled) return;
			Vector2 currentMousePosition = data.position;
			Vector2 diff = currentMousePosition - lastMousePosition;
			var rect = _panel;

			Vector3 newPosition = rect.position + new Vector3(diff.x, diff.y, transform.position.z);
			Vector3 oldPos = rect.position;
			rect.position = newPosition;
			if (!IsRectTransformInsideSreen(rect))
			{
				rect.position = oldPos;
			}

			lastMousePosition = currentMousePosition;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (Mouse.current.rightButton.isPressed) return;
			if (Disabled) return;
			lastMousePosition = eventData.position;
		}

		public void OnEndDrag(PointerEventData eventData) { }

		private Vector3[] GetWorldCorners(RectTransform rectTransform)
		{
			Vector3[] corners = new Vector3[4];
			rectTransform.GetWorldCorners(corners);
			return corners;
		}

		private bool IsRectTransformInsideSreen(RectTransform rectTransform)
		{
			if (_allowOutOfScreen) return true;
			bool isInside = false;
			var corners = GetWorldCorners(rectTransform);
			int visibleCorners = 0;
			Rect rect = new Rect(0, 0, Screen.width, Screen.height);
			foreach (Vector3 corner in corners)
			{
				if (rect.Contains(corner))
				{
					visibleCorners++;
				}
			}

			if (visibleCorners == 4)
			{
				isInside = true;
			}

			return isInside;
		}
#endregion
	}

}
