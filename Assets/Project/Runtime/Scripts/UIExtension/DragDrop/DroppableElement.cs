/*===============================================================
* Product:    Com2Verse
* File Name:  DroppableElement.cs
* Developer:  hyj
* Date:       2022-04-06 12:50
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Com2Verse.UIExtension
{
	[AddComponentMenu("[CVUI]/[CVUI] DroppableElement")]
	[RequireComponent(typeof(CanvasGroup))]
	public sealed class DroppableElement : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
	{		
		[SerializeField] private string _key;
		[SerializeField, Range(0, 1f)] private float _alphaBeginDrag = .6f;
		
		private RectTransform _rectTransform;
		private CanvasGroup _canvasGroup;
		
		private float _prevAlpha = 1f;

		public string Key => _key;

		private void Awake()
		{
			_rectTransform = GetComponent<RectTransform>();
			_canvasGroup = GetComponent<CanvasGroup>();
		}

		public void OnDrop(PointerEventData eventData)
		{
			bool isRectTransform = eventData.pointerDrag.TryGetComponent(out RectTransform eventRect);
			bool isDraggableElement = eventData.pointerDrag.TryGetComponent(out DraggableElement draggableElement);
			
			if (isRectTransform && isDraggableElement && draggableElement.IsDraging && IsMatched(draggableElement)) 
			{
				SetDropPosition(eventRect);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			var pressGameObject = eventData.pointerPressRaycast.gameObject;
			bool hasPressObject = ReferenceEquals(pressGameObject, null);
			if (hasPressObject) return;

			DraggableElement draggableElement;
			bool hasDraggableElement = pressGameObject.TryGetComponent(out draggableElement);
			
			// if DraggableElement is Canvas Gameobject
			if (!hasDraggableElement)
			{
				Transform parentTr = pressGameObject.transform;
				bool hasParentCanvas = false;
				while (!hasParentCanvas)
				{
					parentTr = parentTr.parent;
					hasParentCanvas = parentTr.TryGetComponent(out Canvas parentCanvas);
					if(hasParentCanvas)
						hasDraggableElement = parentCanvas.TryGetComponent(out draggableElement);
				}
			}
			
			if (hasDraggableElement && draggableElement.IsDraging && IsMatched(draggableElement))
			{
				_prevAlpha = _canvasGroup.alpha;
				_canvasGroup.alpha = _alphaBeginDrag;
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_canvasGroup.alpha = _prevAlpha;
		}

		private void SetDropPosition(RectTransform draggableRect)
		{
			Vector3[] droppableCorners = _rectTransform.GetCorners();
			Vector3[] draggableCorners = draggableRect.GetCorners();
			Vector2 draggablePivot = draggableRect.pivot;

			float droppableMaxY = droppableCorners[1].y;
			float droppableMinY = droppableCorners[0].y;
			float droppableMaxX = droppableCorners[2].x;
			float droppableMinX = droppableCorners[0].x;

			float draggableSizeX = draggableCorners[2].x - draggableCorners[0].x;
			float draggableSizeY = draggableCorners[1].y - draggableCorners[0].y;
			
			draggableRect.position = new Vector3(
				droppableMinX + (droppableMaxX - droppableMinX) * 0.5f - (0.5f - draggablePivot.x) * draggableSizeX,
				droppableMinY + (droppableMaxY - droppableMinY) * 0.5f - (0.5f - draggablePivot.y) * draggableSizeY,
				_rectTransform.position.z
			);
		}

		private bool IsMatched(DraggableElement draggableElement) => draggableElement.Key == _key;
	}
}
