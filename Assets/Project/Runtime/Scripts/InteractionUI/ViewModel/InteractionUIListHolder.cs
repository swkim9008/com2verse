// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	InteractionUIListHolder.cs
//  * Developer:	yangsehoon
//  * Date:		2023-05-15 오후 4:38
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("EventTrigger")]
	public class InteractionUIListHolder : ViewModelBase
	{
		public Collection<InteractionUIListViewModel> InteractionListCollection { get; set; } = new();
		public RectTransform InteractionCanvas { get; set; }
	}
}
