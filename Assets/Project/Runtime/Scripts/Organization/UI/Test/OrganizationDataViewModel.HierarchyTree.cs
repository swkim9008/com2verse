/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationDataViewModel.HierarchyTree.cs
* Developer:	jhkim
* Date:			2022-09-05 15:05
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Organization;
using OrganizationTreeModel = Com2Verse.Organization.HierarchyTreeModel<long>; 

namespace Com2Verse.UI
{
	// Hierarchy Tree
	public partial class OrganizationDataViewModel
	{
#region Variables
		private Collection<OrganizationHierarchyListViewModel> _hierarchyList = new();
		private int _selectedGroupIdx;
		private int _selectedTreeIdx;
#endregion // Variables

#region Properties
		public Collection<OrganizationHierarchyListViewModel> HierarchyList
		{
			get => _hierarchyList;
			set
			{
				_hierarchyList = value;
				InvokePropertyValueChanged(nameof(HierarchyList), value);
			}
		}
#endregion // Properties

		void SetHierarchyItem()
		{
			HierarchyList.Reset();
			var hierarchyRoot = DataManager.Instance.GetHierarchyRoot();
			if (hierarchyRoot == null) return;

			for (int i = 0; i < hierarchyRoot.Length; ++i)
			{
				var company = hierarchyRoot[i];
				int idx = 0;
				foreach (var tree in company.GetForwardEnumerator())
				{
					var model = tree.Value;
					tree.Visible = idx == 0;
					model.Index = idx++;
					model.GroupIndex = i;
					tree.Value = model;
					AddHierarchyItem(tree, OnClickItem);
				}
			}
		}

		private void OnClickItem(int groupIdx, int idx)
		{
			_selectedGroupIdx = groupIdx;
			_selectedTreeIdx = idx;
			// C2VDebug.Log($"HierarchyTree OnClick. Group = {groupIdx}, Idx = {idx}");
			SelectItem(groupIdx, idx);
		}

		private void SelectItem(int groupIdx, int idx)
		{
			bool match = false;
			foreach (var item in HierarchyList.Value)
			{
				match = item.GroupIndex == groupIdx && item.Index == idx;
				item.SetSelected(match);
			}
		}
		private void AddHierarchyItem(HierarchyTree<OrganizationTreeModel> item, OrganizationHierarchyListViewModel.OnClicked onClicked)
		{
			// C2VDebug.Log($"Add Hierarchy Item = {item.Value.Name}, Index = {item.Index}, Depth = {item.Depth}");
			var viewModel = new OrganizationHierarchyListViewModel(item, onClicked)
			{
				ShowArrow = item?.HasChildren ?? false,
			};
			HierarchyList.AddItem(viewModel);
		}
	}
}
