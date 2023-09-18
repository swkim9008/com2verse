/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationDataButtonListViewModel.cs
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
	public sealed class OrganizationDataButtonListViewModel : ViewModelBase
	{
#region Variables
		private Action _onSelect;
		private string _name;
		public CommandHandler Select { get; }
#endregion // Variables

#region Properties
		public string Name
		{
			get => _name;
			set
			{
				_name = value;
				InvokePropertyValueChanged(nameof(Name), value);
			}
		}
#endregion // Properties

#region Initialize
		public OrganizationDataButtonListViewModel(string name, Action onSelect)
		{
			_onSelect = onSelect;
			Name = name;
			Select = new CommandHandler(OnSelect);
		}
#endregion // Initialize

#region Binding Events
		private void OnSelect() => _onSelect?.Invoke();
#endregion // Binding Events
	}
}
