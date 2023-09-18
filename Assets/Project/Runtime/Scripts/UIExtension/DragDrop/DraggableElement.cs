/*===============================================================
* Product:    Com2Verse
* File Name:  DraggableElement.cs
* Developer:  hyj
* Date:       2022-04-06 12:49
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Com2Verse.UIExtension
{
	[AddComponentMenu("[CVUI]/[CVUI] DraggableElement")]
	[RequireComponent(typeof(CanvasGroup))]
	public sealed class DraggableElement : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
	{
		[SerializeField] private string _key;
		[SerializeField, Range(0, 1f)] private float _alphaBeginDrag = .6f;
		[SerializeField] private bool _isOnlyOnDroppable;

		private Canvas _canvas;
		private RectTransform _rectTransform;
		private CanvasGroup _canvasGroup;
		
		private Vector2 _prevPosition;
		private float _prevAlpha = 1f;
		private bool _isDraging;

		public string Key => _key;

		public bool IsDraging => _isDraging;
		
		private void Awake()
		{
			_canvas = GetComponentInParent<Canvas>();
			_rectTransform = GetComponent<RectTransform>();
			_canvasGroup = GetComponent<CanvasGroup>();
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			_prevPosition = _rectTransform.anchoredPosition;
			_prevAlpha = _canvasGroup.alpha;
			_canvasGroup.alpha = _alphaBeginDrag;
			_canvasGroup.blocksRaycasts = false;
			_isDraging = true;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (!_isDraging) return;
			if (ReferenceEquals(eventData.pointerEnter, null)) OnEndDrag(eventData);
			_rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
			PreventLeavingScreen();
		}

		public void OnEndDrag(PointerEventData eventData) 
		{
			_canvasGroup.alpha = _prevAlpha;
			_canvasGroup.blocksRaycasts = true;
			_isDraging = false;

			if (!_isOnlyOnDroppable) return;
			bool isOnDroppable = 
				eventData.pointerCurrentRaycast.gameObject.TryGetComponent(out DroppableElement droppableElement);
			if (!isOnDroppable || (_key != droppableElement.Key))
				_rectTransform.anchoredPosition = _prevPosition;
		}
		
		/// <summary>
		/// Prevents elements from leaving the screen.
		/// </summary>
		private void PreventLeavingScreen()
		{
			Vector3[] rectCorners = _rectTransform.GetCorners();
			float minX = rectCorners[0].x;
			float maxX = rectCorners[2].x;
			float minY = rectCorners[0].y;
			float maxY = rectCorners[2].y;
			
			if (minX < 0)
				_rectTransform.position -= new Vector3(minX, 0, 0);
			if (maxX > Screen.width)
				_rectTransform.position -= new Vector3(maxX - Screen.width, 0, 0);
			if (minY < 0)
				_rectTransform.position -= new Vector3(0, minY, 0);
			if (maxY > Screen.height)
				_rectTransform.position -= new Vector3(0, maxY - Screen.height, 0);
		}
	}
}
