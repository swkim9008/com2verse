/*===============================================================
* Product:		Com2Verse
* File Name:	TweenLoopController.cs
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
	[AddComponentMenu("[Tween]/[CVUI] TweenLoopController")]
	public sealed class TweenLoopController : TweenController
	{
		[SerializeField] private bool _isLooping;

		private void Start()
		{
			ApplyState(_isLooping);
		}

		[UsedImplicitly]
		public bool IsLooping
		{
			get => _isLooping;
			set => ToggleLoop(value);
		}

		[UsedImplicitly]
		public bool IsNotLooping
		{
			get => !_isLooping;
			set => ToggleLoopInversed(value);
		}

		[UsedImplicitly]
		public void ToggleLoop(bool value)
		{
			if (_isLooping == value)
				return;

			_isLooping = value;
			ApplyState(value);
		}

		[UsedImplicitly]
		public void ToggleLoopInversed(bool value)
		{
			ToggleLoop(!value);
		}

		private void ApplyState(bool value)
		{
			if (value)
			{
				StartLoop();
			}
			else
			{
				StopLoop();
			}
		}
	}
}
