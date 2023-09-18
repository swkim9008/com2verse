/*===============================================================
* Product:		Com2Verse
* File Name:	CustomizeMenuViewModel.cs
* Developer:	eugene9721
* Date:			2023-05-03 16:47
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[ViewModelGroup("AvatarCustomize")]
	public sealed class CustomizeMenuViewModel : ViewModelBase
	{
#region Fields
		private string _menuTypeKey = string.Empty;
		private string _menuTextKey = string.Empty;

		private bool _isSelected;

		private Action<CustomizeMenuViewModel>? _onSelectedEvent;
#endregion Fields

#region Field Properites
		[UsedImplicitly]
		public string MenuTypeKey
		{
			get => _menuTypeKey;
			set => SetProperty(ref _menuTypeKey, value);
		}

		[UsedImplicitly]
		public string MenuTextKey
		{
			get => _menuTextKey;
			set
			{
				SetProperty(ref _menuTextKey, value);
				InvokePropertyValueChanged(nameof(MenuName), MenuName);
			}
		}

		[UsedImplicitly]
		public bool IsSelected
		{
			get => _isSelected;
			set => SetProperty(ref _isSelected, value);
		}

		public string MenuName => Localization.Instance.GetString(MenuTextKey);

		public event Action<CustomizeMenuViewModel> OnSelectedEvent
		{
			add
			{
				_onSelectedEvent -= value;
				_onSelectedEvent += value;
			}
			remove => _onSelectedEvent -= value;
		}
#endregion Field Properites

#region Command Properties
		[UsedImplicitly] public CommandHandler MenuClicked { get; }
#endregion Command Properties

		public CustomizeMenuViewModel()
		{
			MenuClicked = new CommandHandler(OnMenuClicked);
		}

		private void OnMenuClicked()
		{
			_onSelectedEvent?.Invoke(this);
		}
	}
}
