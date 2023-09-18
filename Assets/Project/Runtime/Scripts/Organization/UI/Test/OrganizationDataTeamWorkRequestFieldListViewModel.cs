/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationDataTeamWorkRequestFieldListViewModel.cs
* Developer:	jhkim
* Date:			2022-10-11 14:11
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections;

namespace Com2Verse.UI
{
	[ViewModelGroup("Organization")]
	public class OrganizationDataTeamWorkRequestFieldListViewModel : ViewModelBase
	{
#region Variables
		private string _name;
		private bool _isArrayParam;
		// Data Field
		private string _value;

		// Array Field
		public Collection<OrganizationDataTeamWorkArrayElementListViewModel> _items = new();
		public CommandHandler Add { get; }

		public Action<string> OnValueChanged;
		private Action<eNotifyCollectionChangedAction, IList, int> _onListChanged;
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
		public bool IsArrayParam
		{
			get => _isArrayParam;
			set
			{
				_isArrayParam = value;
				InvokePropertyValueChanged(nameof(IsArrayParam), value);
			}
		}

		public string Value
		{
			get => _value;
			set
			{
				_value = value;
				OnValueChanged?.Invoke(value);
				InvokePropertyValueChanged(nameof(Value), value);
			}
		}

		public Collection<OrganizationDataTeamWorkArrayElementListViewModel> Items
		{
			get => _items;
			set
			{
				_items = value;
				InvokePropertyValueChanged(nameof(Items), value);
			}
		}

		public Action<eNotifyCollectionChangedAction, IList, int> OnListChanged
		{
			get => _onListChanged;
			set
			{
				_onListChanged = value;
				if (_onListChanged == null) return;

				// Items.RemoveEvent(_onListChanged);
				Items.AddEvent(_onListChanged);
			}
		}
#endregion // Properties

		public OrganizationDataTeamWorkRequestFieldListViewModel()
		{
			Add = new CommandHandler(OnAdd);
		}

		private void OnAdd()
		{
			var idx = Items.CollectionCount;
			var newItem = new OrganizationDataTeamWorkArrayElementListViewModel(new OrganizationDataTeamWorkArrayElementListViewModel.EventHandler
			{
				OnRemove = OnRemoveItem,
				OnValueChanged = value => OnListItemChanged(idx, value),
			});
			Items.AddItem(newItem);
		}

		private void OnRemoveItem(OrganizationDataTeamWorkArrayElementListViewModel item) => Items.RemoveItem(item);

		private void OnListItemChanged(int idx, string value)
		{
			OnListChanged(eNotifyCollectionChangedAction.ADD, Items.ItemsSource as IList, idx);
		}
	}
}
