/*===============================================================
* Product:		Com2Verse
* File Name:	TagController.cs
* Developer:	tlghks1009
* Date:			2022-09-01 16:11
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class TagLayoutController : MonoBehaviour
	{
		[SerializeField] private float _spacing = 10f;
		[SerializeField] private float _leftPadding = 10f;
		[SerializeField] private float _topPadding = -10f;
		[SerializeField] private float _contentHeightSize = 40f;

		private float _totalWidth;
		private Transform _transform;
		private RectTransform _rectTransform;

		private void Awake()
		{
			_rectTransform = gameObject.GetComponent<RectTransform>();


			_totalWidth = _rectTransform.rect.width;
			_transform = this.transform;

			UIManager.Instance.AddUpdateListener(OnUpdate);
		}

		private void OnUpdate()
		{
			float addedContentWidth = 0;
			float height = 0;

			foreach (RectTransform childTransform in _transform)
			{
				if (!childTransform.gameObject.activeSelf)
				{
					continue;
				}

				if (addedContentWidth + childTransform.rect.width + _spacing > _totalWidth)
				{
					height = height - _contentHeightSize - _spacing;

					addedContentWidth = 0;
				}

				childTransform.anchoredPosition = new Vector2(_leftPadding + addedContentWidth, _topPadding + height);

				addedContentWidth += childTransform.rect.width + _spacing;

				_rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x,
				                                       -childTransform.anchoredPosition.y + Math.Abs(_topPadding) + _contentHeightSize);
			}
		}
	}
}
