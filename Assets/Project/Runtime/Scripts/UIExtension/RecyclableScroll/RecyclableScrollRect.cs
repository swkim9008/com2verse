/*===============================================================
* Product:		Com2Verse
* File Name:	RecyclableScrollRect.cs
* Developer:	eugene9721
* Date:			2023-01-11 17:46
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.RecyclableScroll;

namespace Com2Verse.Project.RecyclableScroll
{
	public sealed class RecyclableScrollRect : RecyclableScrollRectBase
	{
		protected override RecyclableScrollTypeBase GetRecyclableScrollController()
		{
			switch (_scrollType)
			{
				case eRecyclableScrollType.SIMPLE:
					return new SimpleScroll(this);
				case eRecyclableScrollType.VIEW_DRIVEN_CELL_SIZE:
					return new ViewDrivenCellSizeScroll(this);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
