/*===============================================================
* Product:		Com2Verse
* File Name:	TweenStyle.cs
* Developer:	urun4m0r1
* Date:			2022-10-19 17:02
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Utils;
using DG.Tweening;
using UnityEngine;

namespace Com2Verse.Tweener
{
	[Serializable]
	public struct TweenStyle
	{
		[field: SerializeField] public bool  UseRestoreDuration { get; private set; }
		[field: SerializeField] public float TweeningDuration   { get; private set; }
		[field: SerializeField] public Ease  TweeningType       { get; private set; }

		[SerializeField, DrawIf(nameof(UseRestoreDuration), true)] private float _restoringDuration;
		[SerializeField, DrawIf(nameof(UseRestoreDuration), true)] private Ease  _restoringType;

		public float RestoringDuration
		{
			readonly get => UseRestoreDuration ? _restoringDuration : TweeningDuration;
			private set => _restoringDuration = value;
		}

		public Ease RestoringType
		{
			readonly get => UseRestoreDuration ? _restoringType : TweeningType;
			private set => _restoringType = value;
		}

		public static TweenStyle Default = new()
		{
			UseRestoreDuration = false,
			TweeningDuration   = 0.2f,
			TweeningType       = Ease.InSine,
			RestoringDuration  = 0.2f,
			RestoringType      = Ease.OutSine,
		};
	}
}
