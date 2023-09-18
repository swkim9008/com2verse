/*===============================================================
* Product:		Com2Verse
* File Name:	UITweenImageFillAmount.cs
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
	[AddComponentMenu("[Tween]/Image/[CVUI] TweenImageFillAmount")]
	public sealed class UITweenImageFillAmount : TweenBase
	{
		[Header("Animation Settings")]
		[SerializeField, Range(0, 1)] private float _initialFillAmount;
		[SerializeField, Range(0, 1)] private float _targetFillAmount;

		private Image  Image => _image ??= GetComponent<Image>()!;
		private Image? _image;

		private void Reset() => _initialFillAmount = Image.fillAmount;

		protected override DG.Tweening.Tweener? Tween(float   duration) => Image.DOFillAmount(_targetFillAmount,  duration);
		protected override DG.Tweening.Tweener? Restore(float duration) => Image.DOFillAmount(_initialFillAmount, duration);

		protected override void TweenImmediately()   => Image.fillAmount = _targetFillAmount;
		protected override void RestoreImmediately() => Image.fillAmount = _initialFillAmount;
	}
}
