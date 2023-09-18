/*===============================================================
* Product:		Com2Verse
* File Name:	UITweenRectPosition.cs
* Developer:	hyj
* Date:			2022-04-28 15:05
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using DG.Tweening;
using UnityEngine;

namespace Com2Verse.Tweener
{
	[AddComponentMenu("[Tween]/RectTransform/[CVUI] TweenAnchoredPosition")]
	public sealed class UITweenAnchoredPosition : TweenBase
	{
		[Header("Animation Settings")]
		[SerializeField] private bool _isLocal = true;
		[SerializeField] private Vector2 _initialPosition;
		[SerializeField] private Vector2 _targetPosition;

		private Vector2 _canvasInitialPosition;
		private Vector2 _canvasTargetPosition;

		private RectTransform  Rect => _rect ??= GetComponent<RectTransform>()!;
		private RectTransform? _rect;

		private void Reset() => _initialPosition = _isLocal ? Vector2.zero : Rect.anchoredPosition;

		protected override void Initialize()
		{
			if (_isLocal)
			{
				_canvasInitialPosition = Rect.anchoredPosition  + _initialPosition;
				_canvasTargetPosition  = _canvasInitialPosition + (_targetPosition - _initialPosition);
			}
			else
			{
				_canvasInitialPosition = _initialPosition;
				_canvasTargetPosition  = _targetPosition;
			}
		}

		protected override DG.Tweening.Tweener? Tween(float   duration) => Rect.DOAnchorPos(_canvasTargetPosition,  duration);
		protected override DG.Tweening.Tweener? Restore(float duration) => Rect.DOAnchorPos(_canvasInitialPosition, duration);

		protected override void TweenImmediately()   => Rect.anchoredPosition = _canvasTargetPosition;
		protected override void RestoreImmediately() => Rect.anchoredPosition = _canvasInitialPosition;
	}
}
