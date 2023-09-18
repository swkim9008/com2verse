/*===============================================================
 * Product:		Com2Verse
 * File Name:	MaskableRectVisibilityListener
 * .cs
 * Developer:	urun4m0r1
 * Date:		2023-04-18 16:03
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.UIExtension
{
	/// <summary>
	/// Mask에 의해 가려질 수 있는 RectTransform의 가시성을 알려주는 스크립트.
	/// </summary>
	[RequireComponent(typeof(RectTransform))]
	public class MaskableRectVisibilityListener : MonoBehaviour
	{
		private RectTransform RectTransform => (transform as RectTransform)!;

		[SerializeField]
		private int _maskMargin = 10;

		[SerializeField]
		private UnityEvent<bool>? _visibilityChanged;

		[SerializeField, ReadOnly]
		private bool isVisible;

		private readonly Vector3[] _rectCorners = new Vector3[4];
		private readonly Vector3[] _maskCorners = new Vector3[4];

		public bool IsVisible
		{
			get => isVisible;
			private set
			{
				if (isVisible == value)
					return;

				isVisible = value;
				_visibilityChanged?.Invoke(isVisible);
			}
		}

		public void InvokePositionUpdated(RectTransform mask)
		{
			IsVisible = IsAnyCornersInsideMask(RectTransform, mask);
		}

		private bool IsAnyCornersInsideMask(RectTransform rect, RectTransform mask)
		{
			rect.GetWorldCorners(_rectCorners);
			mask.GetWorldCorners(_maskCorners);

			var maskRect = new Rect(_maskCorners[0], _maskCorners[2] - _maskCorners[0]);

			maskRect.xMin -= _maskMargin;
			maskRect.xMax += _maskMargin;
			maskRect.yMin -= _maskMargin;
			maskRect.yMax += _maskMargin;

			return maskRect.Contains(_rectCorners[0]) ||
			       maskRect.Contains(_rectCorners[1]) ||
			       maskRect.Contains(_rectCorners[2]) ||
			       maskRect.Contains(_rectCorners[3]);
		}
	}
}
