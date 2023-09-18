/*===============================================================
* Product:		Com2Verse
* File Name:	ViewDrivenCellSizeScroll.cs
* Developer:	eugene9721
* Date:			2022-10-24 15:34
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.EnhancedScroller;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.RecyclableScroll;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.Project.RecyclableScroll
{
	public sealed class ViewDrivenCellSizeScroll : RecyclableScrollTypeBase
	{
		private readonly RectTransform _scrollerRect;
		private          bool          _calculateLayout;

		public ViewDrivenCellSizeScroll(RecyclableScrollRectBase scroller) : base(scroller)
		{
			_scrollerRect = scroller.GetComponent<RectTransform>();
			ResizeScroller();
		}

		private void ResizeScroller()
		{
			// capture the scroller dimensions so that we can reset them when we are done
			var size = _scrollerRect.sizeDelta;
			_scrollerRect.sizeDelta = new Vector2(size.x, float.MaxValue);

			// First Pass: reload the scroller so that it can populate the text UI elements in the cell view.
			// The content size fitter will determine how big the cells need to be on subsequent passes.
			_calculateLayout = true;
			Scroller.ReloadData();

			// reset the scroller size back to what it was originally
			_scrollerRect.sizeDelta = size;

			// Second Pass: reload the data once more with the newly set cell view sizes and scroller content size
			_calculateLayout = false;
		}

		public override void OnReloadRequest()
		{
			var scrollPosition = Scroller.ScrollPosition;
			ResizeScroller();
			Scroller.ScrollPosition = scrollPosition;
		}

		private void SetCellSize(int dataIndex, ScrollDirectionEnum scrollDirection, RectTransform cellViewRect)
		{
			var data = Data[dataIndex];
			if (data == null) return;
			if (data.ContentRect.IsReferenceNull()) return;

			// force update the canvas so that it can calculate the size needed for the content immediately
			// Canvas.ForceUpdateCanvases();
			foreach (var layoutGroup in cellViewRect.GetComponentsInChildren<LayoutElement>())
				LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());

			if (scrollDirection == ScrollDirectionEnum.VERTICAL)
			{
				var cellSize = data.ContentRect!.rect.height;
				data.SizeDelta = new Vector2(data.SizeDelta.x, cellSize);
			}
			else
			{
				var cellSize = data.ContentRect!.rect.width;
				data.SizeDelta = new Vector2(cellSize, data.SizeDelta.y);
			}
		}

		public override EnhancedScrollerCellView GetCellView(int dataIndex, int cellIndex)
		{
			var cellView = Scroller.GetCellView(Scroller.CellViewPrefab) as RecyclableCell;
			if (cellView.IsReferenceNull())
			{
				C2VDebug.LogError($"[{nameof(ViewDrivenCellSizeScroll)}] cellView is null");
				return null;
			}

			var cellViewRect = cellView!.GetComponent<RectTransform>();
			if (cellViewRect.IsReferenceNull())
			{
				C2VDebug.LogError($"[{nameof(ViewDrivenCellSizeScroll)}] cellViewRect is null");
				return null;
			}

			SetCell(cellView, dataIndex);
			if (_calculateLayout)
				SetCellSize(dataIndex, Scroller.ScrollDirection, cellViewRect);
			return cellView;
		}
	}
}
