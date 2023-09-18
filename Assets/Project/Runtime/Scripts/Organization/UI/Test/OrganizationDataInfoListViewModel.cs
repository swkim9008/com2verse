/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationDataInfoListViewModel.cs
* Developer:	jhkim
* Date:			2022-08-31 17:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.UI
{
	[ViewModelGroup("Organization")]
	public sealed class OrganizationDataInfoListViewModel : ViewModelBase
	{
#region Variables
		private Action _onClick;
		private string _key;
		private string _value;

		public bool HasEvent
		{
			get => _onClick != null;
			set
			{
				InvokePropertyValueChanged(nameof(HasEvent), value);
			}
		}

		private bool _hasEvent;
		public CommandHandler Selected { get; }
#endregion // Variables

#region Properties
		public string Key
		{
			get => _key;
			set
			{
				_key = value;
				InvokePropertyValueChanged(nameof(Key), value);
			}
		}

		public string Value
		{
			get => _value;
			set
			{
				_value = value;
				InvokePropertyValueChanged(nameof(Value), value);
			}
		}
#endregion // Properties

#region Initialize
		public OrganizationDataInfoListViewModel(string key, string value, Action onClick = null)
		{
			Key = key;
			Value = value;
			_onClick = onClick;

			HasEvent = _onClick == null;
			Selected = new CommandHandler(OnSelected);
		}
#endregion // Initialize

#region Binding Events
		private void OnSelected() => _onClick?.Invoke();
#endregion // Binding Events
	}
}
