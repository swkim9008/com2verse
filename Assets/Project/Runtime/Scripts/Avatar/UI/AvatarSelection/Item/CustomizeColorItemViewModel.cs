/*===============================================================
* Product:		Com2Verse
* File Name:	CustomizeColorItemViewModel.cs
* Developer:	eugene9721
* Date:			2023-05-02 18:11
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[ViewModelGroup("AvatarCustomize")]
	public sealed class CustomizeColorItemViewModel : ViewModelBase
	{
#region Fields
		private int    _itemId;
		private bool   _isSelected;
		private int    _colorIndex;
		private string _colorToHex;
		private Color  _color;

		private Action<CustomizeColorItemViewModel> _onColorClickedEvent;
#endregion Fields

#region Field Properties
		[UsedImplicitly]
		public int ItemId
		{
			get => _itemId;
			set => SetProperty(ref _itemId, value);
		}

		[UsedImplicitly]
		public bool IsSelected
		{
			get => _isSelected;
			set => SetProperty(ref _isSelected, value);
		}

		[UsedImplicitly]
		public int ColorIndex
		{
			get => _colorIndex;
			set => SetProperty(ref _colorIndex, value);
		}

		[UsedImplicitly]
		public string ColorToHex
		{
			get => _colorToHex;
			set
			{
				SetProperty(ref _colorToHex, value);
				InvokePropertyValueChanged(nameof(Color), Color);
			}
		}

		[UsedImplicitly]
		public Color Color => ColorUtility.TryParseHtmlString(_colorToHex, out var color) ? color : Color.black;

		public event Action<CustomizeColorItemViewModel> OnColorClickedEvent
		{
			add
			{
				_onColorClickedEvent -= value;
				_onColorClickedEvent += value;
			}
			remove => _onColorClickedEvent -= value;
		}
#endregion Field Properties

#region Command Properties
		[UsedImplicitly] public CommandHandler ColorClicked { get; }
#endregion Command Properties

		public CustomizeColorItemViewModel()
		{
			ColorClicked = new CommandHandler(OnColorClicked);
		}

		private void OnColorClicked()
		{
			_onColorClickedEvent?.Invoke(this);
			IsSelected = true;
		}
	}
}
