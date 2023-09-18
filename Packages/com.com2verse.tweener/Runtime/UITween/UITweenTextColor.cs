/*===============================================================
* Product:		Com2Verse
* File Name:	UITweenTextColor.cs
* Developer:	urun4m0r1
* Date:			2022-04-28 15:03
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Com2Verse.Tweener
{
	[RequireComponent(typeof(TMP_Text))]
	[AddComponentMenu("[Tween]/Text/[CVUI] TweenTextColor")]
	public sealed class UITweenTextColor : TweenBase
	{
		[Header("Animation Settings")]
		[SerializeField] private Color _initialColor;
		[SerializeField] private Color _targetColor;

		private TMP_Text  Text => _image ??= GetComponent<TMP_Text>()!;
		private TMP_Text? _image;

		private void Reset() => _initialColor = Text.color;

		protected override DG.Tweening.Tweener? Tween(float   duration) => Text.DOColor(_targetColor,  duration);
		protected override DG.Tweening.Tweener? Restore(float duration) => Text.DOColor(_initialColor, duration);

		protected override void TweenImmediately()   => Text.color = _targetColor;
		protected override void RestoreImmediately() => Text.color = _initialColor;
	}
}
