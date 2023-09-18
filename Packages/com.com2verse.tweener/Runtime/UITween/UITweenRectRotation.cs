/*===============================================================
* Product:		Com2Verse
* File Name:	UITweenRectRotation.cs
* Developer:	hyj
* Date:			2022-04-28 15:07
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using DG.Tweening;
using UnityEngine;

namespace Com2Verse.Tweener
{
	[AddComponentMenu("[Tween]/RectTransform/[CVUI] TweenRectRotation")]
	public sealed class UITweenRectRotation : TweenBase
	{
		[Header("Animation Settings")]
		[SerializeField] private RotateMode _rotateMode;
		[SerializeField] private Vector3 _initialRotation;
		[SerializeField] private Vector3 _targetRotation;

		private RectTransform  Rect => _rect ??= GetComponent<RectTransform>()!;
		private RectTransform? _rect;

		private void Reset() => _initialRotation = Rect.rotation.eulerAngles;

		protected override DG.Tweening.Tweener? Tween(float   duration) => Rect.DORotate(_targetRotation,  duration, _rotateMode);
		protected override DG.Tweening.Tweener? Restore(float duration) => Rect.DORotate(_initialRotation, duration, _rotateMode);

		protected override void TweenImmediately()   => Rect.rotation = Quaternion.Euler(_targetRotation);
		protected override void RestoreImmediately() => Rect.rotation = Quaternion.Euler(_initialRotation);
	}
}
