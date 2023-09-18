/*===============================================================
 * Product:		Com2Verse
 * File Name:	ScreenShareLayoutViewModel.cs
 * Developer:	urun4m0r1
 * Date:		2023-04-19 15:30
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Runtime.CompilerServices;
using Com2Verse.ScreenShare;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("ScreenShare")]
	public sealed class ScreenShareLayoutViewModel : ViewModelBase, IDisposable
	{
		public event Action<string, bool>? LayoutChanged;

		[UsedImplicitly] public CommandHandler<bool> SetDesktopLayout { get; }
		[UsedImplicitly] public CommandHandler<bool> SetWindowLayout  { get; }

		[UsedImplicitly] public CommandHandler ToggleDesktopLayout { get; }
		[UsedImplicitly] public CommandHandler ToggleWindowLayout  { get; }

		[UsedImplicitly] public Collection<ScreenInfoViewModel> DesktopItems { get; } = new();
		[UsedImplicitly] public Collection<ScreenInfoViewModel> WindowItems  { get; } = new();

		public ScreenInfoViewModel? SelectedViewModel => IsDesktopLayout
			? _desktopViewModelFactory.SelectedItem
			: _windowViewModelFactory.SelectedItem;

		private readonly ScreenInfoViewModelFactory _desktopViewModelFactory;
		private readonly ScreenInfoViewModelFactory _windowViewModelFactory;

		private bool _isDesktopLayout = true;
		private bool _isWindowLayout  = false;

		public ScreenShareLayoutViewModel()
		{
			SetDesktopLayout = new CommandHandler<bool>(value => IsDesktopLayout = value);
			SetWindowLayout  = new CommandHandler<bool>(value => IsWindowLayout  = value);

			ToggleDesktopLayout = new CommandHandler(() => IsDesktopLayout ^= true);
			ToggleWindowLayout  = new CommandHandler(() => IsWindowLayout  ^= true);

			_desktopViewModelFactory = new ScreenInfoViewModelFactory(screenInfo => screenInfo.ScreenType == eScreenType.DESKTOP);
			_windowViewModelFactory  = new ScreenInfoViewModelFactory(screenInfo => screenInfo.ScreenType == eScreenType.WINDOW);

			_desktopViewModelFactory.ViewModelCreated  += OnDesktopViewModelCreated;
			_windowViewModelFactory.ViewModelCreated   += OnWindowViewModelCreated;
			_desktopViewModelFactory.ViewModelDisposed += OnDesktopViewModelDisposed;
			_windowViewModelFactory.ViewModelDisposed  += OnWindowViewModelDisposed;

			foreach (var viewModel in _desktopViewModelFactory.ViewModels.Values)
				OnDesktopViewModelCreated(viewModel);

			foreach (var viewModel in _windowViewModelFactory.ViewModels.Values)
				OnWindowViewModelCreated(viewModel);

			_desktopViewModelFactory.ViewModelSelected += OnDesktopViewModelSelected;
			_windowViewModelFactory.ViewModelSelected  += OnWindowViewModelSelected;

			OnDesktopViewModelSelected(_desktopViewModelFactory.SelectedItem);
			OnWindowViewModelSelected(_windowViewModelFactory.SelectedItem);
		}

		public void Dispose()
		{
			_desktopViewModelFactory.ViewModelCreated  -= OnDesktopViewModelCreated;
			_windowViewModelFactory.ViewModelCreated   -= OnWindowViewModelCreated;
			_desktopViewModelFactory.ViewModelDisposed -= OnDesktopViewModelDisposed;
			_windowViewModelFactory.ViewModelDisposed  -= OnWindowViewModelDisposed;

			foreach (var viewModel in _desktopViewModelFactory.ViewModels.Values)
				OnDesktopViewModelDisposed(viewModel);

			foreach (var viewModel in _windowViewModelFactory.ViewModels.Values)
				OnWindowViewModelDisposed(viewModel);

			_desktopViewModelFactory.ViewModelSelected -= OnDesktopViewModelSelected;
			_windowViewModelFactory.ViewModelSelected  -= OnWindowViewModelSelected;

			OnDesktopViewModelSelected(null);
			OnWindowViewModelSelected(null);

			_desktopViewModelFactory.Dispose();
			_windowViewModelFactory.Dispose();
		}

		private void OnDesktopViewModelCreated(ScreenInfoViewModel  viewModel) => DesktopItems.AddItem(viewModel);
		private void OnWindowViewModelCreated(ScreenInfoViewModel   viewModel) => WindowItems.AddItem(viewModel);
		private void OnDesktopViewModelDisposed(ScreenInfoViewModel viewModel) => DesktopItems.RemoveItem(viewModel);
		private void OnWindowViewModelDisposed(ScreenInfoViewModel  viewModel) => WindowItems.RemoveItem(viewModel);

		private void OnDesktopViewModelSelected(ScreenInfoViewModel? viewModel)
		{
			RaisePropertyValueChanged(nameof(IsDesktopItemSelected), IsDesktopItemSelected);
			UpdateSelectionDependentProperty();
		}

		private void OnWindowViewModelSelected(ScreenInfoViewModel? viewModel)
		{
			RaisePropertyValueChanged(nameof(IsWindowItemSelected), IsWindowItemSelected);
			UpdateSelectionDependentProperty();
		}

		private void UpdateSelectionDependentProperty()
		{
			RaisePropertyValueChanged(nameof(IsCurrentLayoutItemSelected), IsCurrentLayoutItemSelected);
		}

#region ViewModelProperties
		/// <summary>
		/// 데스크톱 창 선택 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsDesktopLayout
		{
			get => _isDesktopLayout;
			set => UpdateProperty(ref _isDesktopLayout, value);
		}

		/// <summary>
		/// 윈도우 창 선택 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsWindowLayout
		{
			get => _isWindowLayout;
			set => UpdateProperty(ref _isWindowLayout, value);
		}

		/// <summary>
		/// 데스크톱 창이 선택 되었는지 여부
		/// </summary>
		public bool IsDesktopItemSelected => _desktopViewModelFactory.SelectedItem != null;

		/// <summary>
		///	윈도우 창이 선택 되었는지 여부
		/// </summary>
		public bool IsWindowItemSelected => _windowViewModelFactory.SelectedItem != null;

		/// <summary>
		/// 현재 열린 레이아웃에 아이템이 선택되었는지 여부
		/// </summary>
		public bool IsCurrentLayoutItemSelected => IsDesktopLayout ? IsDesktopItemSelected : IsWindowItemSelected;
#endregion // ViewModelProperties

		private void UpdateProperty(ref bool storage, bool value, [CallerMemberName] string propertyName = "")
		{
			if (storage == value)
				return;

			SetProperty(ref storage, value, propertyName);
			LayoutChanged?.Invoke(propertyName, value);

			CloseLayoutsOnOthers(propertyName);

			UpdateLayoutDependentProperty();
		}

		private void RaisePropertyValueChanged(string propertyName, bool value)
		{
			InvokePropertyValueChanged(propertyName, value);
			LayoutChanged?.Invoke(propertyName, value);
		}

		private void UpdateLayoutDependentProperty()
		{
			RaisePropertyValueChanged(nameof(IsCurrentLayoutItemSelected), IsCurrentLayoutItemSelected);
		}

		/// <summary>
		/// 창 선택 레이아웃은 동시에 하나만 열릴 수 있도록 한다.
		/// <br/>또한, 모든 레이아웃이 동시에 닫힐 수 없도록 한다.
		/// </summary>
		private void CloseLayoutsOnOthers(string propertyName)
		{
			switch (propertyName)
			{
				case nameof(IsDesktopLayout) when IsDesktopLayout:
					IsWindowLayout = false;
					break;
				case nameof(IsWindowLayout) when IsWindowLayout:
					IsDesktopLayout = false;
					break;
			}

			if (!IsDesktopLayout && !IsWindowLayout)
				IsDesktopLayout = true;
		}
	}
}
