/*===============================================================
* Product:		Com2Verse
* File Name:	CheckEmployeeSearchResultViewModel.cs
* Developer:	jhkim
* Date:			2022-10-18 12:25
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using MemberIdType = System.Int64;

namespace Com2Verse.UI
{
	public sealed class CheckEmployeeSearchResultViewModel : EmployeeSearchResultViewModel
	{
#region Variables
		private bool                       _isChecked;
		private bool                       _isInteractable;
		public  CommandHandler             Click { get; }
		private Action<MemberIdType, bool> _onclick;
		private Action<MemberIdType, bool> _onValueChanged;
		private bool                       _isOn;
#endregion // Variables

#region Properties
		public bool IsChecked
		{
			get => _isChecked;
			set
			{
				_isChecked = value;
				if (_isOn != value)
					_onValueChanged?.Invoke(MemberId, value);
				_isOn = _isChecked;
				InvokePropertyValueChanged(nameof(IsChecked), value);
			}
		}

		public bool IsInteractable
		{
			get => _isInteractable;
			set
			{
				_isInteractable = value;
				InvokePropertyValueChanged(nameof(IsInteractable), value);
			}
		}
#endregion // Properties

#region Initialization
		public CheckEmployeeSearchResultViewModel(MemberIdType employeeNo, bool isChecked, bool isInteractable, Action<MemberIdType, bool> onClick = null) : base(employeeNo)
		{
			Click = new CommandHandler(OnClick);

			IsChecked = _isOn = isChecked;
			IsInteractable = isInteractable;

			SetOnclick(onClick);
		}
#endregion // Initialization

#region Binding Events
		private void OnClick()
		{
			var value = !IsChecked;
			_onclick?.Invoke(MemberId, value);
			IsChecked = value;
		}
#endregion // Binding Events
		public void SetOnclick(Action<MemberIdType, bool> onClick) => _onclick = onClick;
		public void SetOnValueChanged(Action<MemberIdType, bool> onValueChanged) => _onValueChanged = onValueChanged;
	}
}
