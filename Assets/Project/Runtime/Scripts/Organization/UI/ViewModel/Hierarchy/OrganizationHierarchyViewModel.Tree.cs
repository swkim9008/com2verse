/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationHierarchyViewModel.Tree.cs
* Developer:	jhkim
* Date:			2022-08-01 17:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Extension;
using Com2Verse.Organization;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;
using OrganizationTreeModel = Com2Verse.Organization.HierarchyTreeModel<long>;
using OrganizationTreeItem = Com2Verse.Organization.HierarchyTree<Com2Verse.Organization.HierarchyTreeModel<long>>;
using MemberIdType = System.Int64;

namespace Com2Verse.UI
{
	// Tree
	public partial class OrganizationHierarchyViewModel
	{
#region Variables
		// Hierarchy Tree
		private Collection<OrganizationHierarchyListViewModel> _hierarchyList = new();
		private Dictionary<int, int> _hierarchyIndexMap = new();
		private OrganizationTreeItem _prevSelected = null;
		private ObjectPool<OrganizationHierarchyListViewModel> _treeItemPool;

		// Pool
		private HierarchyTree<OrganizationTreeModel> _poolRequestTree;
		private OrganizationHierarchyListViewModel.OnClicked _poolRequestOnClicked;
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

#region Initialize
		private void InitTree()
		{
			_treeItemPool = new ObjectPool<OrganizationHierarchyListViewModel>(CreateTreeItem, GetTreeItem, ReleaseTreeItem, DestroyTreeItem);
		}
#endregion // Initialize

#region Hierarchy Set Item(s)
		private void RefreshHierarchyTreeData()
		{
			ReleaseHierarchyList();
			_hierarchyIndexMap.Clear();
			if (!DataManager.Instance.IsReady) return;

			var companies = DataManager.Instance.GetHierarchyRoot();
			var groupIdx = 0;
			var hierarchyIndex = 0;
			var addItemCnt = 0;

			foreach (var company in companies)
			{
				var index = 0;
				foreach (var tree in company.Root.GetForwardEnumerator())
				{
					var model = tree.Value;

					// 최상위 노드는 항상 보여짐
					if (index == 0)
						tree.Visible = true;

					model.Index = index++;
					model.GroupIndex = groupIdx;
					tree.Value = model;

					if (tree.Visible)
					{
						AddHierarchyItem(tree, OnClickItem);
						addItemCnt++;
					}
				}

				_hierarchyIndexMap.Add(groupIdx, hierarchyIndex);
				hierarchyIndex += index;
				groupIdx++;
			}
		}

		void AddHierarchyItem(HierarchyTree<OrganizationTreeModel> item, OrganizationHierarchyListViewModel.OnClicked onClicked)
		{
			var viewModel = GetTreeItem(item, onClicked);
			HierarchyList?.AddItem(viewModel);
		}

		void OnClickItem(int groupIdx, int idx)
		{
			var companies = DataManager.Instance.GetHierarchyRoot();
			if (groupIdx >= companies.Length) return;
			if (idx >= companies[groupIdx].Length) return;

			var selected = companies.GetChild(groupIdx, idx);
			if (selected == null) return;

			RefreshTitleInfo(selected);
			RefreshSubNodeVisible(selected);
			SelectTreeItem(groupIdx, idx);

			if (IsSelectAvailable(selected.Value.ID)) return;

			_prevSelected = selected;

			RefreshEmployeeUI(selected);
			SetCheatTreeIndex(groupIdx, idx);

			ClearSearchText();
			SetScrollToTop();

			bool IsSelectAvailable(MemberIdType id) => _prevSelected != null && _prevSelected.Value.ID == id;
		}
#endregion // Hierarchy Set Item(s)

#region Hierarchy Refresh UI
		public void SelectTreeItem(int groupIdx, int index)
		{
			for (int i = 0; i < HierarchyList.CollectionCount; ++i)
			{
				var item = HierarchyList.Value[i];
				var isSelected = item.Index == index && item.GroupIndex == groupIdx;
				item.SetSelected(isSelected);
			}
		}

		void RefreshTitleInfo(HierarchyTree<OrganizationTreeModel> selected)
		{
			if (selected != null && selected.Value.ID != _prevSelected?.Value.ID)
			{
				TeamName = selected.Value.Name;
				ContainSubDepartmentWithoutNotify = false;
			}
		}

		void RefreshSubNodeVisible(HierarchyTree<OrganizationTreeModel> selected)
		{
			if (selected != null)
			{
				var descentEnumerator = selected.GetDescentEnumerator();
				var invisibleIdxSet = new HashSet<int>();
				var isDirty = false;
				foreach (var childNode in descentEnumerator)
				{
					CheckChildrenInvisible(childNode);

					if (childNode.Equals(selected)) continue;
					var childItem = GetItemByHierarchyIndex(childNode.Value.GroupIndex, childNode.Value.Index);
					childNode.Visible = IsVisible(childNode);
					if (childItem == null)
						isDirty = true;
				}

				if (isDirty)
					RefreshHierarchyTreeData();

				void CheckChildrenInvisible(HierarchyTree<OrganizationTreeModel> parent)
				{
					if (parent.HasChildren && parent.IsFold)
					{
						foreach (var t in parent.GetDescentEnumerator())
						{
							if (parent.Equals(t)) continue;
							invisibleIdxSet.TryAdd(t.Value.Index);
						}
					}
				}

				bool IsVisible(HierarchyTree<OrganizationTreeModel> tree) => selected.Visible && !invisibleIdxSet.Contains(tree.Value.Index);
			}
		}

		private OrganizationHierarchyListViewModel GetItemByHierarchyIndex(int groupIdx, int index)
		{
			foreach (var item in HierarchyList.Value)
			{
				if (item.GroupIndex == groupIdx && item.Index == index)
					return item;
			}

			return null;
		}
#endregion // Hierarchy Refresh UI

#region Hierarchy Pick Item
		private void PickItem(int index)
		{
			var pair = GetGroupIndex(index);
			var groupIdx = pair.Item1;
			var idx = pair.Item2;

			PickItem(groupIdx, idx);
		}

		private ValueTuple<int, int> GetGroupIndex(int hierarchyIdx)
		{
			var groupIdx = 0;
			var index = 0;
			foreach (var (gIdx, count) in _hierarchyIndexMap)
			{
				if (hierarchyIdx - count <= 0)
				{
					groupIdx = gIdx;
					index += count - hierarchyIdx;
					break;
				}

				hierarchyIdx -= count;
			}

			return (groupIdx, index);
		}
		private void PickItem(int groupIdx, int idx)
		{
			var companies = DataManager.Instance.GetHierarchyRoot();
			if (groupIdx >= companies.Length) return;

			var depts = companies[groupIdx];
			if (idx >= depts.Length) return;

			for (int i = 0; i < companies.Length; ++i)
			{
				if (i == groupIdx)
				{
					PickItemInternal(groupIdx, idx);
					break;
				}
			}
		}

		private void PickItemInternal(int groupIdx, int idx)
		{
			FoldAll();

			var department = DataManager.Instance.GetHierarchyRoot()[groupIdx];
			var selected = HierarchyTree<OrganizationTreeModel>.Pick(department, idx);

			RefreshTitleInfo(selected);
			RefreshSubNodeVisible(selected);
			// SelectTreeItem(groupIdx, idx);

			_prevSelected = selected;

			RefreshEmployeeUI(selected);
			SetCheatTreeIndex(groupIdx, idx);
		}

		private async UniTask<(int, int)> PickItemByAccountIdAsync(long accountId)
		{
			var memberModel = await DataManager.Instance.GetMemberAsync(accountId);
			return PickItemByEmployee(memberModel);
		}

		private (int, int) PickItemByEmployee(MemberModel memberModel)
		{
			if (memberModel == null) return (-1, -1);

			var teamId = memberModel.Member.TeamId;
			var companies = DataManager.Instance.GetHierarchyRoot();
			var groupIdx = 0;
			var idx = 0;
			var found = false;
			foreach (var company in companies)
			{
				idx = 0;
				foreach (var dept in company.Root.GetForwardEnumerator())
				{
					if (dept.Value.ID.Equals(teamId))
					{
						found = true;
						break;
					}

					idx++;
				}

				if (found)
					break;
				groupIdx++;
			}

			if (found)
			{
				PickItemInternal(groupIdx, idx);
				return (groupIdx, idx);
			}
			return (-1, -1);
		}

		private void FoldAll()
		{
			var roots = DataManager.Instance.GetHierarchyRoot();
			foreach (var root in roots)
				HierarchyTree<OrganizationTreeModel>.FoldAll(root);
		}
#endregion // Hierarchy Pick Item

#region Hierarchy Cheat
		private void SetCheatTreeIndex(int groupIdx, int idx)
		{
			CheatGroupIdx = Convert.ToString(groupIdx);
			CheatIdx = Convert.ToString(idx);
		}
#endregion // Hierarchy Cheat

#region Dispose
		private void DisposeTree()
		{
			_prevSelected = null;
			ResetHierarchy();
			ReleaseHierarchyList();
			// HierarchyList.DestroyAll();
		}
		private void ReleaseHierarchyList()
		{
			foreach (var item in HierarchyList.Value)
				_treeItemPool.Release(item);
			HierarchyList.Reset();
		}
		private void ResetHierarchy()
		{
			_hierarchyIndexMap.Clear();
			var roots = DataManager.Instance.GetHierarchyRoot();
			foreach (var department in roots)
				department.ResetFoldStateInChildren();
		}
#endregion // Dispose

#region Pooling
		private OrganizationHierarchyListViewModel GetTreeItem(HierarchyTree<OrganizationTreeModel> item, OrganizationHierarchyListViewModel.OnClicked onClicked)
		{
			var viewModel = _treeItemPool.Get();
			viewModel.ShowArrow = item?.HasChildren ?? false;
			viewModel.SetData(item, onClicked);
			return viewModel;
		}
		OrganizationHierarchyListViewModel CreateTreeItem() => new();
		void GetTreeItem(OrganizationHierarchyListViewModel item) { }

		void ReleaseTreeItem(OrganizationHierarchyListViewModel item)
		{
			item.Selected = false;
		}
		void DestroyTreeItem(OrganizationHierarchyListViewModel item) { }
#endregion // Pooling
	}
}
