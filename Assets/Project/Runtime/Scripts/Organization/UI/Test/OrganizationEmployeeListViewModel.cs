/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationEmployeeListViewModel.cs
* Developer:	jhkim
* Date:			2022-08-31 16:11
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Protocols;

namespace Com2Verse.UI
{
	[ViewModelGroup("Organization")]
	public sealed class OrganizationEmployeeListViewModel : ViewModel
	{
#region Variables
		private EmployeePayload _employee;
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
		public OrganizationEmployeeListViewModel(EmployeePayload employee, Action onSelect)
		{
			_employee = employee;
			_onSelect = onSelect;

			Select = new CommandHandler(OnSelect);
			RefreshUI();
		}
#endregion // Initialize

#region Binding Events
		private void OnSelect()
		{
			_onSelect?.Invoke();
		}
#endregion // Binding Events

		private void RefreshUI()
		{
			Name = _employee.EmployeeName;
		}
	}
}
