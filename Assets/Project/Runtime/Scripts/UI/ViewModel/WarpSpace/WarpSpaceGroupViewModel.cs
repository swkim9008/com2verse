/*===============================================================
* Product:		Com2Verse
* File Name:	WarpSpaceGroupViewModel.cs
* Developer:	jhkim
* Date:			2023-06-30 20:50
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Office.WarpSpace;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[ViewModelGroup("WarpSpace")]
	public sealed class WarpSpaceGroupViewModel : ViewModelBase
	{
#region Variable
		private static readonly int ItemPerRow = 3;
		private static readonly float BaseHeight = 30f;
		private static readonly float HeightPerRow = 60f;

		private string _label;
		private string _subLabel;
		private float _height;

		private Collection<WarpSpaceItemViewModel> _items = new();

		private readonly WarpSpaceItemViewModel.OnSelected _onSelectItem;
#endregion // Variable

#region Properties
		[UsedImplicitly]
		public string Label
		{
			get => _label;
			set => SetProperty(ref _label, value);
		}

		[UsedImplicitly]
		public string SubLabel
		{
			get => _subLabel;
			set => SetProperty(ref _subLabel, value);
		}

		[UsedImplicitly]
		public Collection<WarpSpaceItemViewModel> Items
		{
			get => _items;
			set
			{
				_items = value;
				SetProperty(ref _items, value);
			}
		}

		[UsedImplicitly]
		public float Height
		{
			get => _height;
			set => SetProperty(ref _height, value);
		}
#endregion // Properties

#region Initialize
		public WarpSpaceGroupViewModel(WarpSpaceGroupModel model, WarpSpaceItemViewModel.OnSelected onSelectItem)
		{
			_onSelectItem = onSelectItem;

			SetData(model);
		}
#endregion // Initialize
		private void SetData(WarpSpaceGroupModel model)
		{
			Label = model.Label;
			SubLabel = model.SubLabel;

			Items.Reset();

			foreach (var item in model.Items)
				Items.AddItem(new WarpSpaceItemViewModel(item, _onSelectItem));

			UpdateHeight();
		}

		public void DeselectItemsExcept(int pid)
		{
			foreach (var item in Items.Value)
			{
				if (item.Pid != pid)
					item.Deselect();
			}
		}
		private void UpdateHeight()
		{
			var rowCount = Items.CollectionCount / ItemPerRow;
			if (Items.CollectionCount % ItemPerRow != 0)
				rowCount++;

			Height = BaseHeight + rowCount * HeightPerRow;
		}
	}
}
