/*===============================================================
* Product:		Com2Verse
* File Name:	TweenToggleController.cs
* Developer:	urun4m0r1
* Date:			2022-10-21 14:12
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Tweener
{
	[DisallowMultipleComponent]
	[AddComponentMenu("[Tween]/[CVUI] TweenToggleController")]
	public sealed class TweenToggleController : TweenController
	{
		[SerializeField] private bool _isTweened;

		private void Start()
		{
			if (_isTweened)
			{
				TweenImmediately();
			}
			else
			{
				RestoreImmediately();
			}
		}

		[UsedImplicitly]
		public bool IsTweened
		{
			get => _isTweened;
			set => ToggleState(value);
		}

		[UsedImplicitly]
		public bool IsRestored
		{
			get => !_isTweened;
			set => ToggleStateInversed(value);
		}

		[UsedImplicitly]
		public void ToggleState(bool value)
		{
			if (_isTweened == value)
				return;

			_isTweened = value;
			ApplyState(value);
		}

		[UsedImplicitly]
		public void ToggleStateInversed(bool value)
		{
			ToggleState(!value);
		}

		private void ApplyState(bool value)
		{
			if (value)
			{
				Tween();
			}
			else
			{
				Restore();
			}
		}

		[ContextMenu("ApplyToggle")]
		public void ApplyToggle() => ApplyState(_isTweened);
	}
}
