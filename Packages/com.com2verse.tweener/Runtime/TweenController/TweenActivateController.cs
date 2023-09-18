/*===============================================================
* Product:		Com2Verse
* File Name:	TweenActivateController.cs
* Developer:	urun4m0r1
* Date:			2022-10-21 14:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;

namespace Com2Verse.Tweener
{
	[DisallowMultipleComponent]
	[AddComponentMenu("[Tween]/[CVUI] TweenActivateController")]
	public sealed class TweenActivateController : TweenController
	{
		private void OnEnable()
		{
			Tween();
		}

		private void OnDisable()
		{
			Restore();
		}
	}
}
