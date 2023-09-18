/*===============================================================
* Product:		Com2Verse
* File Name:	WarpSpaceItemViewModel.cs
* Developer:	jhkim
* Date:			2023-06-30 20:53
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
	public sealed class WarpSpaceItemViewModel : ViewModelBase
	{
#region Variable
		private readonly WarpSpaceItemController _controller;

		private string _label;
		private bool _isWork;
		private bool _isRest;
		private bool _isSelected;
		private bool _isDisabled;

		public delegate void OnSelected(int pid);
		private readonly OnSelected _onSelected;
#endregion // Variable

#region Properties
		[UsedImplicitly]
		public string Label
		{
			get => _label;
			set => SetProperty(ref _label, value);
		}

		[UsedImplicitly]
		public bool IsWork
		{
			get => _isWork;
			set => SetProperty(ref _isWork, value);
		}

		[UsedImplicitly]
		public bool IsRest
		{
			get => _isRest;
			set => SetProperty(ref _isRest, value);
		}

		[UsedImplicitly]
		public bool IsSelected
		{
			get => _isSelected;
			set => SetProperty(ref _isSelected, value);
		}

		[UsedImplicitly]
		public bool IsDisabled
		{
			get => _isDisabled;
			set => SetProperty(ref _isDisabled, value);
		}

		[UsedImplicitly] public CommandHandler Select { get; private set; }

		public int Pid => _controller.Pid;
#endregion // Properties

#region Initialize
		public WarpSpaceItemViewModel(WarpSpaceItemModel model, OnSelected onSelect)
		{
			_controller = new WarpSpaceItemController(model, OnStateChanged);
			_onSelected = onSelect;

			Select = new CommandHandler(OnSelect, null);

			IsWork = model.Type is WarpSpaceItemModel.eType.WORK or WarpSpaceItemModel.eType.LOBBY;
			IsRest = model.Type == WarpSpaceItemModel.eType.REST;
			IsDisabled = model.State == WarpSpaceItemModel.eState.DISABLED;

			Label = model.Label;
		}
#endregion // Initialize
		private void OnStateChanged(WarpSpaceItemModel.eState prevState, WarpSpaceItemModel.eState newState)
		{
			IsSelected = newState == WarpSpaceItemModel.eState.SELECTED;

			if (newState == WarpSpaceItemModel.eState.SELECTED)
				_onSelected?.Invoke(_controller.Pid);
		}

#region Binding Events
		private void OnSelect()
		{
			if (_controller?.State == WarpSpaceItemModel.eState.DISABLED) return;

			_controller?.SetState(WarpSpaceItemModel.eState.SELECTED);
		}
#endregion // Binding Events

		public void Deselect()
		{
			if (_controller?.State == WarpSpaceItemModel.eState.DESELECTED) return;

			_controller?.SetState(WarpSpaceItemModel.eState.DESELECTED);
		}
	}
}
