/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationDataTeamWorkListViewModel.cs
* Developer:	jhkim
* Date:			2022-10-11 11:50
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.UI
{
	[ViewModelGroup("Organization")]
	public sealed class OrganizationDataTeamWorkListViewModel : ViewModelBase
	{
#region Variables
		private string _label;
		public           CommandHandler           Click { get; }
		private          Action                   _onClick;
#endregion // Variables

#region Properties
		public string Label
		{
			get => _label;
			set
			{
				_label = value;
				InvokePropertyValueChanged(nameof(Label), value);
			}
		}
#endregion // Properties

#region Initialization
		public OrganizationDataTeamWorkListViewModel(Action onClick)
		{
			_onClick = onClick;
			Click = new CommandHandler(OnClick);
		}
#endregion // Initialization

#region Binding Events
		private void OnClick() => _onClick?.Invoke();
#endregion // Binding Events
	}
}
