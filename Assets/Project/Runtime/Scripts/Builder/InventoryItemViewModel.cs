// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	InventoryItemViewModel.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-08 오전 11:20
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.Builder;
using Com2Verse.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse
{
	public class InventoryItemViewModel : ViewModel
	{
		public ScrollRect ParentScrollView { get; set; }
		public RectTransform CanvasRoot { get; set; }
		public BuilderInventoryItem Data { get; set; }
		public Sprite Image => Data.Thumbnail;
	}
}
