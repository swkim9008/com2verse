/*===============================================================
* Product:		Com2Verse
* File Name:	UIHelper.cs
* Developer:	tlghks1009
* Date:			2023-05-11 14:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Com2Verse
{
	public static class UIHelper
	{
		public static void CreateDefaultCanvas(this GameObject thisGameObject, int sortingOrder)
		{
			var rootCanvas = thisGameObject.AddComponent<Canvas>();
			rootCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
			rootCanvas.sortingOrder = sortingOrder;

			var rootCanvasScaler = thisGameObject.AddComponent<CanvasScaler>();
			rootCanvasScaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			rootCanvasScaler.referenceResolution = new Vector2(1920, 1080);

			var rootGraphicRaycaster = thisGameObject.AddComponent<GraphicRaycaster>();
			rootGraphicRaycaster.blockingMask = 1 << LayerMask.NameToLayer("UI");
		}


		public static void CreateDefaultCanvas(this GameObject thisGameObject)
		{
			thisGameObject.CreateDefaultCanvas(0);
		}
	}
}
