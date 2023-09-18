/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationGroupInviteSelectedRowViewModel.cs
* Developer:	jhkim
* Date:			2022-07-29 17:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Linq;
using Com2Verse.Organization;
using UnityEngine.Pool;

namespace Com2Verse.UI
{
	[ViewModelGroup("Organization")]
	public sealed class OrganizationGroupInviteSelectedRowViewModel : ViewModelBase
	{
#region Variables
		private Collection<OrganizationGroupInviteSelectedItemViewModel> _selected = new();
		private float _maxWidth;
		private float _remainWidth;
		private Action<CheckMemberListModel> _onRemove;
		private ObjectPool<OrganizationGroupInviteSelectedItemViewModel> _itemPool;
#endregion // Variables

#region Properties
		public Collection<OrganizationGroupInviteSelectedItemViewModel> Selected
		{
			get => _selected;
			set
			{
				_selected = value;
				InvokePropertyValueChanged(nameof(Selected), value);
			}
		}
		public float MaxWidth
		{
			set
			{
				_maxWidth = value;
				_remainWidth = _maxWidth;
			}
		}
#endregion // Properties

#region Initialize
		public OrganizationGroupInviteSelectedRowViewModel() { }

		public OrganizationGroupInviteSelectedRowViewModel(float maxWidth, Action<CheckMemberListModel> onRemove)
		{
			_maxWidth = _remainWidth = maxWidth;
			_onRemove = onRemove;
			_itemPool = new ObjectPool<OrganizationGroupInviteSelectedItemViewModel>(CreateItem, GetItem, ReleaseItem, DestroyItem);
		}
#endregion // Initialize

		public bool IsAvailableAdd(float itemWidth) => _remainWidth - itemWidth > 0;
		public void AddItem(CheckMemberListModel employee, float itemWidth)
		{
			_remainWidth -= itemWidth;

			var item = _itemPool.Get();
			item.SetModel(employee);
			item.Width = itemWidth;
			Selected.AddItem(item);
		}

		public void CleanUp()
		{
			_remainWidth = _maxWidth;
			ReleaseAllItems();
			Selected.Reset();
		}

		private void ReleaseAllItems()
		{
			var items = Selected.Value.ToArray();
			foreach (var item in items)
				_itemPool.Release(item);
		}
#region Pooling
		OrganizationGroupInviteSelectedItemViewModel CreateItem() => new(_onRemove);

		void GetItem(OrganizationGroupInviteSelectedItemViewModel item)
		{
		}

		void ReleaseItem(OrganizationGroupInviteSelectedItemViewModel item)
		{
			if (item != null)
			{
				if (Selected.Value.Contains(item))
					Selected.RemoveItem(item);
			}
		}

		void DestroyItem(OrganizationGroupInviteSelectedItemViewModel item)
		{
			if (item != null)
			{
				if(Selected.Value.Contains(item))
					Selected.RemoveItem(item);
			}
		}
#endregion // Pooling

		public void Dispose()
		{
			ReleaseAllItems();
			_itemPool.Dispose();
		}
	}
}
