/*===============================================================
* Product:		Com2Verse
* File Name:	RecyclableScrollType.cs
* Developer:	eugene9721
* Date:			2022-10-05 22:06
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.EnhancedScroller;
using Com2Verse.UI;
using UnityEngine;

namespace Com2Verse.RecyclableScroll
{
	public enum eRecyclableScrollType
	{
		SIMPLE,
		VIEW_DRIVEN_CELL_SIZE,
	}

	public abstract class RecyclableScrollTypeBase : IEnhancedScrollerDelegate
	{
		protected RecyclableScrollRectBase Scroller { get; private set; }

		public SmallList<RecyclableCellViewModel> Data { get; set; } = new();

		public abstract EnhancedScrollerCellView GetCellView(int dataIndex, int cellIndex);

		public virtual void OnReloadRequest() { }

		public virtual float GetCellViewSize(int dataIndex)
		{
			if (Scroller.ScrollDirection == ScrollDirectionEnum.VERTICAL)
			{
				float height = Data[dataIndex].SizeDelta.y;
				if (Mathf.Approximately(height, 0))
				{
					height = Scroller.CellViewPrefab.GetComponent<RectTransform>().sizeDelta.y;
				}

				return height;
			}
			else
			{
				float width = Data[dataIndex].SizeDelta.x;
				if (Mathf.Approximately(width, 0))
				{
					width = Scroller.CellViewPrefab.GetComponent<RectTransform>().sizeDelta.x;
				}

				return width;
			}
		}

		public virtual int GetNumberOfCells()
		{
			return Data.Count;
		}

		public virtual void Initialize() { }

		protected void SetCell(RecyclableCell cell, int index)
		{
			if (index < 0 || index >= Data.Count)
			{
				cell.gameObject.SetActive(false);
				return;
			}

			if (!cell.gameObject.activeSelf) cell.gameObject.SetActive(true);

			cell.ViewModelContainer.ClearAll();
			cell.ViewModelContainer.AddViewModel(Data[index]);
			cell.Bind();
		}

		protected RecyclableScrollTypeBase(RecyclableScrollRectBase scroller)
		{
			Scroller = scroller;
		}
	}
}
