/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationDataTeamWorkArrayElementListViewModel.cs
* Developer:	jhkim
* Date:			2022-10-11 14:19
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.UI
{
	[ViewModelGroup("Organization")]
	public sealed class OrganizationDataTeamWorkArrayElementListViewModel : ViewModelBase
	{
		public struct EventHandler
		{
			public Action<OrganizationDataTeamWorkArrayElementListViewModel> OnRemove;
			public Action<string> OnValueChanged;
		}

#region Variables
		private string _value;
		public CommandHandler Remove { get; }

		private EventHandler _handler;
#endregion // Variables

#region Properties
		public string Value
		{
			get => _value;
			set
			{
				_value = value;
				_handler.OnValueChanged?.Invoke(value);
				InvokePropertyValueChanged(nameof(Value), value);
			}
		}
#endregion // Properties

#region Initialization
		public OrganizationDataTeamWorkArrayElementListViewModel(EventHandler handler)
		{
			_handler = handler;
			Value = string.Empty;
			Remove = new CommandHandler(OnRemove);
		}

		private void OnRemove() => _handler.OnRemove?.Invoke(this);
#endregion // Initialization
	}
}
