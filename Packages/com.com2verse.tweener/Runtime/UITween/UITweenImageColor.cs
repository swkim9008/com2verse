/*===============================================================
* Product:		Com2Verse
* File Name:	UITweenImageColor.cs
* Developer:	hyj
* Date:			2022-04-28 15:03
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.Tweener
{
	[RequireComponent(typeof(Image))]
	[AddComponentMenu("[Tween]/Image/[CVUI] TweenImageColor")]
	public sealed class UITweenImageColor : TweenBase
	{
		[Header("Animation Settings")]
		[SerializeField] private Color _initialColor;
		[SerializeField] private Color _targetColor;

		private Image  Image => _image ??= GetComponent<Image>()!;
		private Image? _image;

		private void Reset() => _initialColor = Image.color;

		protected override DG.Tweening.Tweener? Tween(float   duration) => Image.DOColor(_targetColor,  duration);
		protected override DG.Tweening.Tweener? Restore(float duration) => Image.DOColor(_initialColor, duration);

		protected override void TweenImmediately()   => Image.color = _targetColor;
		protected override void RestoreImmediately() => Image.color = _initialColor;
	}
}
