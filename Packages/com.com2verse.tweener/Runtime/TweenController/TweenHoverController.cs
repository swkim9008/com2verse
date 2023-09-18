/*===============================================================
* Product:		Com2Verse
* File Name:	TweenHoverController.cs
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
	[AddComponentMenu("[Tween]/[CVUI] TweenHoverController")]
	public sealed class TweenHoverController : TweenController, IPointerEnterHandler, IPointerExitHandler
	{
		public void OnPointerEnter(PointerEventData? _)
		{
			Tween();
		}

		public void OnPointerExit(PointerEventData? _)
		{
			Restore();
		}
	}
}
