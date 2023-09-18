/*===============================================================
* Product:		Com2Verse
* File Name:	GUIView_SortingOrder.cs
* Developer:	tlghks1009
* Date:			2022-08-19 14:50
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;

namespace Com2Verse.UI
{
	public abstract partial class GUIView
	{
		public void SetSortingOrder(int sortingOrder)
		{
			if (!_canvas.IsUnityNull())
			{
				_canvas.overrideSorting = true;
				_canvas.sortingOrder    = sortingOrder;
			}
		}

		public int GetSortingOrder() => _canvas.IsUnityNull() ? Utils.Define.POPUP_SORTING_ORDER : _canvas!.sortingOrder;

		public void SetOverrideSorting(bool enable)
		{
			if (!_canvas.IsUnityNull())
				_canvas.overrideSorting = enable;
		}
	}
}
