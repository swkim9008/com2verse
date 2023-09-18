/*===============================================================
* Product:		Com2Verse
* File Name:	IEnhancedScrollerDelegate.cs
* Developer:	eugene9721
* Date:			2022-10-06 16:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.EnhancedScroller;

namespace Com2Verse.RecyclableScroll
{
	/// <summary>
	/// All scripts that handle the scroller's callbacks should inherit from this interface
	/// </summary>
	public interface IEnhancedScrollerDelegate
	{
		/// <summary>
		/// Gets the number of cells in a list of data
		/// </summary>
		/// <returns></returns>
		int GetNumberOfCells();

		/// <summary>
		/// Gets the size of a cell view given the index of the data set.
		/// This allows you to have different sized cells
		/// </summary>
		/// <param name="dataIndex"></param>
		/// <returns></returns>
		float GetCellViewSize(int dataIndex);

		/// <summary>
		/// Gets the cell view that should be used for the data index. Your implementation
		/// of this function should request a new cell from the scroller so that it can
		/// properly recycle old cells.
		/// </summary>
		/// <param name="dataIndex"></param>
		/// <param name="cellIndex"></param>
		/// <returns></returns>
		EnhancedScrollerCellView GetCellView(int dataIndex, int cellIndex);
	}
}
