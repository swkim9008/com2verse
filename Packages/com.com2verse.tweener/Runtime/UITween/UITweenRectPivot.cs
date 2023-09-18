/*===============================================================
* Product:		Com2Verse
* File Name:	UITweenRectPivot.cs
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
	[AddComponentMenu("[Tween]/RectTransform/[CVUI] TweenRectPivot")]
	public sealed class UITweenRectPivot : TweenBase
	{
		[Header("Animation Settings")]
		[SerializeField] private Vector2 _initialPivot;
		[SerializeField] private Vector2 _targetPivot;

		private RectTransform  Rect => _rect ??= GetComponent<RectTransform>()!;
		private RectTransform? _rect;

		private void Reset() => _initialPivot = Rect.pivot;

		protected override DG.Tweening.Tweener? Tween(float   duration) => Rect.DOPivot(_targetPivot,  duration);
		protected override DG.Tweening.Tweener? Restore(float duration) => Rect.DOPivot(_initialPivot, duration);

		protected override void TweenImmediately()   => Rect.pivot = _targetPivot;
		protected override void RestoreImmediately() => Rect.pivot = _initialPivot;
	}
}
