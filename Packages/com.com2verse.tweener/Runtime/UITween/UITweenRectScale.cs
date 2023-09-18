/*===============================================================
* Product:		Com2Verse
* File Name:	UITweenRectScale.cs
* Developer:	hyj
* Date:			2022-04-28 15:08
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using DG.Tweening;
using UnityEngine;

namespace Com2Verse.Tweener
{
	[AddComponentMenu("[Tween]/RectTransform/[CVUI] TweenRectScale")]
	public sealed class UITweenRectScale : TweenBase
	{
		[Header("Animation Settings")]
		[SerializeField] private Vector3 _initialScale;
		[SerializeField] private Vector3 _targetScale;

		public Vector3 InitialScale
		{
			get => _initialScale;
			set => _initialScale = value;
		}

		public Vector3 TargetScale
		{
			get => _targetScale;
			set => _targetScale = value;
		}

		private RectTransform  Rect => _rect ??= GetComponent<RectTransform>()!;
		private RectTransform? _rect;

		private void Reset() => _initialScale = Rect.localScale;

		protected override DG.Tweening.Tweener? Tween(float   duration) => Rect.DOScale(_targetScale,  duration);
		protected override DG.Tweening.Tweener? Restore(float duration) => Rect.DOScale(_initialScale, duration);

		protected override void TweenImmediately()   => Rect.localScale = _targetScale;
		protected override void RestoreImmediately() => Rect.localScale = _initialScale;
	}
}
