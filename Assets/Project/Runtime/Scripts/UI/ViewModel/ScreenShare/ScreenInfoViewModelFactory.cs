/*===============================================================
* Product:		Com2Verse
* File Name:	ScreenInfoViewModelFactory.cs
* Developer:	urun4m0r1
* Date:			2022-09-23 13:22
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.ScreenShare;

namespace Com2Verse.UI
{
	public sealed class ScreenInfoViewModelFactory : IDisposable
	{
		public event Action<ScreenInfoViewModel?>? ViewModelSelected;

		public event Action<ScreenInfoViewModel>? ViewModelCreated;
		public event Action<ScreenInfoViewModel>? ViewModelDisposed;

		public IReadOnlyDictionary<ScreenId, ScreenInfoViewModel> ViewModels => _viewModels;

		private readonly Dictionary<ScreenId, ScreenInfoViewModel> _viewModels = new(ScreenIdComparer.Default);

		private readonly Predicate<IReadOnlyScreenInfo>? _screenFilter;

		private ScreenInfoViewModel? _selectedItem;

		public ScreenInfoViewModelFactory(Predicate<IReadOnlyScreenInfo>? screenFilter = null)
		{
			_screenFilter = screenFilter;

			var controller = ScreenCaptureManager.Instance.Controller;
			controller.ScreenInfoAdded   += OnScreenInfoAdded;
			controller.ScreenInfoRemoved += OnScreenInfoRemoved;

			foreach (var screenInfo in controller.ScreenInfos)
				OnScreenInfoAdded(screenInfo);
		}

		public void Dispose()
		{
			if (!ScreenCaptureManager.InstanceExists)
				return;

			var controller = ScreenCaptureManager.Instance.Controller;
			controller.ScreenInfoAdded   -= OnScreenInfoAdded;
			controller.ScreenInfoRemoved -= OnScreenInfoRemoved;

			foreach (var screenInfo in controller.ScreenInfos)
				OnScreenInfoRemoved(screenInfo);
		}

		private void OnScreenInfoAdded(IReadOnlyScreenInfo screenInfo)
		{
			if (!IsTargetScreen(screenInfo))
				return;

			if (_viewModels.ContainsKey(screenInfo.Id))
				return;

			var viewModel = new ScreenInfoViewModel(screenInfo);
			viewModel.Selected += OnItemSelected;

			_viewModels.Add(screenInfo.Id, viewModel);
			ViewModelCreated?.Invoke(viewModel);

			ScreenCaptureManager.Instance.Controller.RequestMetadata(screenInfo);
		}

		private void OnScreenInfoRemoved(IReadOnlyScreenInfo screenInfo)
		{
			if (!IsTargetScreen(screenInfo))
				return;

			if (!_viewModels.TryGetValue(screenInfo.Id, out var viewModel))
				return;

			viewModel!.Selected -= OnItemSelected;
			OnItemSelected(viewModel, false);

			_viewModels.Remove(screenInfo.Id);
			ViewModelDisposed?.Invoke(viewModel);

			viewModel.Dispose();
		}

		private bool IsTargetScreen(IReadOnlyScreenInfo screenInfo) => _screenFilter?.Invoke(screenInfo) ?? true;

		private void OnItemSelected(ScreenInfoViewModel? item, bool isSelected)
		{
			if (isSelected)
			{
				if (item == SelectedItem)
					return;

				DeselectCurrent();
				SelectedItem = item;
			}
			else if (SelectedItem == item)
			{
				SelectedItem = null;
			}
		}

		private void DeselectCurrent()
		{
			if (SelectedItem == null)
				return;

			SelectedItem.IsSelected = false;
		}

		public ScreenInfoViewModel? SelectedItem
		{
			get => _selectedItem;
			private set
			{
				if (_selectedItem == value)
					return;

				_selectedItem = value;
				ViewModelSelected?.Invoke(value);
			}
		}
	}
}
