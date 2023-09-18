/*===============================================================
* Product:		Com2Verse
* File Name:	DraggableWebView.cs
* Developer:	mikeyid77
* Date:			2022-08-22 16:32
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.EventSystems;

namespace Com2Verse.WebView
{
	public sealed class DraggableWebView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[SerializeField] private RectTransform _panel;
		private Vector2 lastMousePosition;
		private bool _canChangePos;

		public bool CanChangePos
		{
			get => _canChangePos;
			set => _canChangePos = value;
		}

		public void OnDrag(PointerEventData data)
		{
			if (_canChangePos)
			{
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
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
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
	}
}
