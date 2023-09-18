/*===============================================================
* Product:		Com2Verse
* File Name:	SimpleScroll.cs
* Developer:	eugene9721
* Date:			2022-10-06 15:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.EnhancedScroller;

namespace Com2Verse.RecyclableScroll
{
	public sealed class SimpleScroll : RecyclableScrollTypeBase
	{
		/// <summary>
		/// Gets the cell to be displayed. You can have numerous cell types, allowing variety in your list.
		/// Some examples of this would be headers, footers, and other grouping cells.
		/// </summary>
		/// <param name="dataIndex">The index of the data that the scroller is requesting</param>
		/// <param name="cellIndex">The index of the list. This will likely be different from the dataIndex if the scroller is looping</param>
		/// <returns>The cell for the scroller to use</returns>
		public override EnhancedScrollerCellView GetCellView(int dataIndex, int cellIndex)
		{
			// first, we get a cell from the scroller by passing a prefab.
			// if the scroller finds one it can recycle it will do so, otherwise
			// it will create a new cell.
			RecyclableCell cellView = Scroller.GetCellView(Scroller.CellViewPrefab) as RecyclableCell;

			// set the name of the game object to the cell's data index.
			// this is optional, but it helps up debug the objects in 
			// the scene hierarchy.
			SetCell(cellView, dataIndex);

			// return the cell to the scroller
			return cellView;
		}

		public SimpleScroll(RecyclableScrollRectBase scroller) : base(scroller) { }
	}
}
