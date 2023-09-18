/*===============================================================
* Product:		Com2Verse
* File Name:	UITweenRectSize.cs
* Developer:	hyj
* Date:			2022-04-28 15:04
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using DG.Tweening;
using UnityEngine;

namespace Com2Verse.Tweener
{
	[AddComponentMenu("[Tween]/RectTransform/[CVUI] TweenRectSize")]
	public sealed class UITweenRectSize : TweenBase
	{
		[Header("Animation Settings")]
		[SerializeField] private Vector2 _initialSize;
		[SerializeField] private Vector2 _targetSize;

		private RectTransform  Rect => _rect ??= GetComponent<RectTransform>()!;
		private RectTransform? _rect;

		private void Reset() => _initialSize = Rect.sizeDelta;

		protected override DG.Tweening.Tweener? Tween(float   duration) => Rect.DOSizeDelta(_targetSize,  duration);
		protected override DG.Tweening.Tweener? Restore(float duration) => Rect.DOSizeDelta(_initialSize, duration);

		protected override void TweenImmediately()   => Rect.sizeDelta = _targetSize;
		protected override void RestoreImmediately() => Rect.sizeDelta = _initialSize;
	}
}
