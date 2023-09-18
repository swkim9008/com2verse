/*===============================================================
* Product:		Com2Verse
* File Name:	TweenClickController.cs
* Developer:	urun4m0r1
* Date:			2022-10-21 14:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;
using UnityEngine.EventSystems;

namespace Com2Verse.Tweener
{
	[DisallowMultipleComponent]
	[AddComponentMenu("[Tween]/[CVUI] TweenClickController")]
	public sealed class TweenClickController : TweenController, IPointerDownHandler, IPointerUpHandler
	{
		public enum eClickTrigger
		{
			DOWN,
			UP,
			DOWN_UP,
		}

		[SerializeField] private eClickTrigger _clickTrigger = eClickTrigger.DOWN;

		public void OnPointerDown(PointerEventData? _)
		{
			if (_clickTrigger == eClickTrigger.DOWN)
			{
				TweenOnce();
			}
			else if (_clickTrigger == eClickTrigger.DOWN_UP)
			{
				Tween();
			}
		}

		public void OnPointerUp(PointerEventData? _)
		{
			if (_clickTrigger == eClickTrigger.UP)
			{
				TweenOnce();
			}
			else if (_clickTrigger == eClickTrigger.DOWN_UP)
			{
				Restore();
			}
		}
	}
}
