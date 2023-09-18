/*===============================================================
* Product:		Com2Verse
* File Name:	UITweenCanvasGroupAlpha.cs
* Developer:	hyj
* Date:			2022-04-28 15:01
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using DG.Tweening;
using UnityEngine;

namespace Com2Verse.Tweener
{
	[RequireComponent(typeof(CanvasGroup))]
	[AddComponentMenu("[Tween]/CanvasGroup/[CVUI] TweenCanvasGroupAlpha")]
	public sealed class UITweenCanvasGroupAlpha : TweenBase
	{
		[Header("Animation Settings")]
		[SerializeField] private float _initialAlpha;
		[SerializeField] private float _targetAlpha;

		private CanvasGroup  CanvasGroup => _canvasGroup ??= GetComponent<CanvasGroup>()!;
		private CanvasGroup? _canvasGroup;

		private void Reset() => _initialAlpha = CanvasGroup.alpha;

		protected override DG.Tweening.Tweener? Tween(float   duration) => CanvasGroup.DOFade(_targetAlpha,  duration);
		protected override DG.Tweening.Tweener? Restore(float duration) => CanvasGroup.DOFade(_initialAlpha, duration);

		protected override void TweenImmediately()   => CanvasGroup.alpha = _targetAlpha;
		protected override void RestoreImmediately() => CanvasGroup.alpha = _initialAlpha;
	}
}
