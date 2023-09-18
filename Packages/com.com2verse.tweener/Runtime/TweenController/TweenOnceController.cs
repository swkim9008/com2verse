/*===============================================================
* Product:		Com2Verse
* File Name:	TweenOnceController.cs
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
	[AddComponentMenu("[Tween]/[CVUI] TweenOnceController")]
	public sealed class TweenOnceController : TweenController
	{
		[UsedImplicitly]
		public void Fire()
		{
			TweenOnce();
		}
	}
}
