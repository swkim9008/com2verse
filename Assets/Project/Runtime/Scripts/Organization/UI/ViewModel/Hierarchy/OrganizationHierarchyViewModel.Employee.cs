/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationHierarchyViewModel.Employee.cs
* Developer:	jhkim
* Date:			2022-08-01 17:47
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Organization;
using UnityEngine;
using OrganizationTreeModel = Com2Verse.Organization.HierarchyTreeModel<long>;
using MemberIdType = System.Int64;
using TeamIdType = System.Int64;

namespace Com2Verse.UI
{
	// Employee
	public partial class OrganizationHierarchyViewModel
	{
#region Variables
		// TODO : 무한스크롤 적용 후 제거
		private static readonly int MaxContainSubDepartment = 70;
		private static readonly float ScrollViewHeightGap = 20;

		// Check Employee
		private Collection<OrganizationCheckEmployeeListViewModel> _checkMemberList = new();
		private bool _uiCheckAll;
		private bool _hasSubDepartment;
		private bool _containSubDepartment;
		private bool _containSubDepartmentWithoutNotify;
		private string _textTeamName;
		private string _employeeCount;
		private float _defaultScrollViewHeight;
		private float _scrollViewHeight;
		private float _checkEmployeeDefaultHeight;
		private float _checkEmployeeHeight;
		private float _checkEmployeeSearchHeight;
		private RectTransform _popupEmployeeDetailRectTransform;
		public CommandHandler ClickCheckAll { get; private set; }
		public CommandHandler InclideSubGroup { get; private set; }

		private IEnumerable<CheckMemberListModel> _employeeListModels;
		private HashSet<MemberIdType> _checkedMembers;
		private HashSet<MemberIdType> _blockedMembers;
#endregion // Variables

#region Properties
		public Collection<OrganizationCheckEmployeeListViewModel> CheckEmployeeList
		{
			get => _checkMemberList;
			set
			{
				_checkMemberList = value;
				InvokePropertyValueChanged(nameof(CheckEmployeeList), value);
			}
		}

		public bool UICheckAll
		{
			get => _uiCheckAll;
			set
			{
				_uiCheckAll = value;
				InvokePropertyValueChanged(nameof(UICheckAll), value);
			}
		}
		public bool HasSubDepartment
		{
			get => _hasSubDepartment;
			set
			{
				_hasSubDepartment = value;
				InvokePropertyValueChanged(nameof(HasSubDepartment), value);
			}
		}
		public bool ContainSubDepartment
		{
			get => _containSubDepartment;
			set
			{
				if (_containSubDepartment == value) return;
				_containSubDepartment = value;
				RefreshEmployeeUI(_prevSelected);
				InvokePropertyValueChanged(nameof(ContainSubDepartment), value);
			}
		}

		public bool ContainSubDepartmentWithoutNotify
		{
			get => _containSubDepartmentWithoutNotify;
			set
			{
				_containSubDepartmentWithoutNotify = value;
				_containSubDepartment = value;
				InvokePropertyValueChanged(nameof(ContainSubDepartmentWithoutNotify), value);
			}
		}
		public string TeamName
		{
			get => _textTeamName;
			set
			{
				_textTeamName = value;
				InvokePropertyValueChanged(nameof(TeamName), value);
			}
		}

		public string EmployeeCount
		{
			get => _employeeCount;
			set
			{
				_employeeCount = value;
				InvokePropertyValueChanged(nameof(EmployeeCount), value);
			}
		}
		public float DefaultScrollViewHeight
		{
			get => _defaultScrollViewHeight;
			set
			{
				_defaultScrollViewHeight = value;
				InvokePropertyValueChanged(nameof(DefaultScrollViewHeight), value);
			}
		}

		public float ScrollViewHeight
		{
			get => _scrollViewHeight;
			set
			{
				_scrollViewHeight = value;
				InvokePropertyValueChanged(nameof(ScrollViewHeight), value);
			}
		}

		public float CheckEmployeeDefaultHeight
		{
			get => _checkEmployeeDefaultHeight;
			set
			{
				if (_checkEmployeeDefaultHeight == 0f)
				{
					_checkEmployeeDefaultHeight = value;
					InvokePropertyValueChanged(nameof(CheckEmployeeDefaultHeight), value);
				}
			}
		}
		public float CheckEmployeeHeight
		{
			get => _checkEmployeeHeight;
			set
			{
				_checkEmployeeHeight = value;
				InvokePropertyValueChanged(nameof(CheckEmployeeHeight), value);
			}
		}

		public float CheckEmployeeSearchHeight
		{
			get => _checkEmployeeSearchHeight;
			set
			{
				_checkEmployeeSearchHeight = value;
				InvokePropertyValueChanged(nameof(CheckEmployeeSearchHeight), value);
			}
		}

		public RectTransform PopupEmployeeDetailRectTransform
		{
			get => _popupEmployeeDetailRectTransform;
			set
			{
				_popupEmployeeDetailRectTransform = value;
				InvokePropertyValueChanged(nameof(PopupEmployeeDetailRectTransform), value);
			}
		}
#endregion // Properties

#region Initialize
		private void InitEmployee()
		{
			_checkedMembers = new HashSet<MemberIdType>();
			_blockedMembers = new HashSet<MemberIdType>();
			ClickCheckAll = new CommandHandler(OnClickCheckAll);
			InclideSubGroup = new CommandHandler(OnIncludeSubGroup);
		}

		private void ResetEmployee()
		{
			UICheckAll = false;
		}
#endregion // Initialize

#region Binding Events
		private void OnClickCheckAll()
		{
			UICheckAll = !UICheckAll;
			if (UICheckAll) // Off -> On
			{
				SetCheckAllEmployee(UICheckAll);
			}
			else // On -> Off
			{
				if (IsEmployeeAllChecked())
					SetCheckAllEmployee(UICheckAll);
			}
		}

		private void OnIncludeSubGroup()
		{
			C2VDebug.LogWarning("OnIncludeSubGroup");
		}
#endregion // Binding Events

#region Employee
		void SetCheckAllEmployee(bool check)
		{
			foreach (var employee in CheckEmployeeList.Value)
			{
				if (!IsCheckAvailable(employee)) continue;
				employee.SetChecked(check);
			}
			OnCheckAllClicked(check, _employeeListModels);
			bool IsCheckAvailable(OrganizationCheckEmployeeListViewModel employee) => !employee.IsMine && !_blockedMembers.Contains(employee.Model.Info.Member.AccountId);
		}

		void AddCheckEmployeeItem(IEnumerable<CheckMemberListModel> employees)
		{
			foreach (var model in employees)
				CheckEmployeeList.AddItem(new OrganizationCheckEmployeeListViewModel(model, PopupEmployeeDetailRectTransform, OnEmployeeCheckClicked));
		}
		private void OnEmployeeCheckClicked(CheckMemberListModel model)
		{
			if (_viewModel != null)
				_viewModel.UICheckAll = model.IsChecked && _viewModel.IsEmployeeAllChecked();
			if (model.IsChecked) AddGroupInvite(model);
			else RemoveGroupInvite(model);
		}
		private void OnCheckAllClicked(bool check, IEnumerable<CheckMemberListModel> models)
		{
			if (models == null) return;
			if (CheckEmployeeList.CollectionCount == 0) return;

			if (_viewModel != null)
				_viewModel.UICheckAll = _viewModel.IsEmployeeAllChecked();

			var model = GetModel(models);
			model.IsChecked = check;
			if (check)
				AddGroupInvite(models);
			else
				RemoveGroupInvite(models);

			CheckMemberListModel GetModel(IEnumerable<CheckMemberListModel> items)
			{
				var enumerator = items.GetEnumerator();
				do
				{
					if (enumerator.Current.Info != null)
						return enumerator.Current;
				}
				while (enumerator.MoveNext());
				return default;
			}
		}


		bool IsEmployeeAllChecked()
		{
			if (CheckEmployeeList.Value.Count == 0)
				return false;

			foreach (var employee in CheckEmployeeList.Value)
			{
				if (employee.IsMine)
					continue;

				if (!employee.UiIsChecked)
					return false;
			}
			return true;
		}

		void UncheckEmployee(CheckMemberListModel info) => SetCheckEmployee(info, false);
		void SetCheckEmployee(CheckMemberListModel info, bool check)
		{
			var found = CheckEmployeeList.Value.FirstOrDefault(viewModel => viewModel.Model.Info.Equals(info));
			if (found != null)
				found.IsChecked = check;
		}
#endregion // Employee

#region Group Invite
		void AddGroupInvite(params CheckMemberListModel[] models) => AddGroupInvite(models as IEnumerable<CheckMemberListModel>);

		void AddGroupInvite(IEnumerable<CheckMemberListModel> models)
		{
			foreach (var model in models)
			{
				if (!IsInviteAvailable(model)) continue;
				_checkedMembers.TryAdd(model.Info.Member.AccountId);
			}

			AddGroupInviteEmployee(models);
		}
		void OnGroupInviteRemoveEmployee(CheckMemberListModel model) => RemoveGroupInvite(model);
		void RemoveGroupInvite(params CheckMemberListModel[] models) => RemoveGroupInvite(models as IEnumerable<CheckMemberListModel>);

		void RemoveGroupInvite(IEnumerable<CheckMemberListModel> models)
		{
			foreach (var model in models)
			{
				if (!IsInviteAvailable(model)) continue;
				RemoveCheckedItem(model);
			}

			RemoveGroupInviteEmployee(models);
		}
		bool IsInviteAvailable(CheckMemberListModel model) => !model.Info.IsMine() && !_blockedMembers.Contains(model.Info.Member.AccountId);
		void RemoveCheckedItem(CheckMemberListModel model) => RemoveCheckedItem(model.Info.Member.AccountId);
		void RemoveCheckedItem(MemberIdType employeeNo)
		{
			if (_checkedMembers.Contains(employeeNo))
				_checkedMembers.Remove(employeeNo);
		}
#endregion // Group Invite

#region UI
		// 직원정보 목록 갱신
		private void RefreshEmployeeUI(HierarchyTree<OrganizationTreeModel> selected)
		{
			if (selected == null) return;

			var employees = GetMembersFromTeam(selected.Value.ID, ContainSubDepartment);
			var models = CreateModels(employees);
			var allChecked = IsAllChecked(employees, models);

			_employeeListModels = models;
			CheckEmployeeList.Reset();
			AddCheckEmployeeItem(models);

			HasSubDepartment = selected.HasChildren;

			// TODO : 겸직 표시
			EmployeeCount = GetMemberCountStr(employees.Length, 0);
			UICheckAll = allChecked;
			RefreshScrollViewHeight();

			SendRequestUserState(employees);
		}

		private void RefreshScrollViewHeight()
		{
			ScrollViewHeight = IsShowGroupInvite ? DefaultScrollViewHeight - GroupInviteViewHeight + ScrollViewHeightGap : DefaultScrollViewHeight;
			SearchScrollViewHeight = IsShowGroupInvite ? DefaultSearchScrollViewheight - GroupInviteViewHeight : DefaultSearchScrollViewheight;
		}
		// 체크박스 관한 변경사항만 갱신
		private void RefreshEmployeeCheckUI(TeamIdType teamId)
		{
			if (teamId < 0) return;

			var employees = GetMembersFromTeam(teamId, ContainSubDepartment);
			var models = CreateModels(employees);
			var allChecked = IsAllChecked(employees, models);

			foreach (var item in CheckEmployeeList.Value)
			{
				item.IsChecked = _checkedMembers.Contains(item.Model.Info.Member.AccountId);
			}

			UICheckAll = allChecked;
		}

		private MemberModel[] GetMembersFromTeam(TeamIdType teamId, bool containSubTeam)
		{
			if (containSubTeam)
			{
				var result = new List<MemberModel>();
				var team = DataManager.Instance.GetTeam(teamId);
				if (team == null) return Array.Empty<MemberModel>();

				C2VDebug.Log($"조직도 GetMembersFromTeam = {team.Info.TeamName} ({Convert.ToString(team.Info.TeamId)})");
				AddAllMembersInTeam(result, team);
				return result.ToArray();
			}

			return GetMembersFromTeamInternal(teamId);

			void AddAllMembersInTeam(List<MemberModel> list, TeamModel teamModel)
			{
				if (list.Count > MaxContainSubDepartment) return;

				list.AddRange(GetMembersFromTeamInternal(teamModel.Info.TeamId));

				foreach (var subTeamId in teamModel.SubTeamIds)
				{
					var subTeam = DataManager.Instance.GetTeam(subTeamId);
					if (subTeam == null)
						continue;

					AddAllMembersInTeam(list, subTeam);
				}
			}

			MemberModel[] GetMembersFromTeamInternal(TeamIdType subTeamId)
			{
				var memberModelIds = DataManager.Instance.GetMemberIdsFromTeam(subTeamId);
				return DataManager.Instance.GetMembers(memberModelIds);
			}
		}
		// private EmployeePayload[] GetEmployeesFromDepartment(string deptCode, bool containSubDepartment)
		// {
		// 	if (containSubDepartment)
		// 	{
		// 		var result = new List<EmployeePayload>();
		// 		var departmant = DataManager.Instance.GetDepartment(deptCode);
		// 		AddAllEmployeesInDepartment(result, departmant);
		// 		return result.ToArray();
		// 	}
		// 	return GetEmployeesFromDepartment(deptCode);
		//
		// 	void AddAllEmployeesInDepartment(List<EmployeePayload> list, DepartmentModel department)
		// 	{
		// 		// TODO : 무한스크롤 적용 후 제거
		// 		if (list.Count > MaxContainSubDepartment) return;
		// 		list.AddRange(GetEmployeesFromDepartment(department.Info.DeptCode));
		// 		foreach (var subDepartmentId in department.SubDepartmentIds)
		// 			AddAllEmployeesInDepartment(list, DataManager.Instance.GetDepartment(subDepartmentId));
		// 	}
		// }

		// private EmployeePayload[] GetEmployeesFromDepartment(string deptCode)
		// {
		// 	var employeeIds = DataManager.Instance.GetEmployeeIdsFromDepartment(deptCode);
		// 	return DataManager.Instance.GetEmployees(employeeIds);
		// }

		private IEnumerable<CheckMemberListModel> CreateModels(MemberModel[] memberModels)
		{
			return (memberModels.OrderBy(MemberModel.Compare) ?? Array.Empty<MemberModel>())
			   .Select(memberModel => new CheckMemberListModel
				{
					Info = memberModel,
					IsChecked = _checkedMembers.Contains(memberModel.Member.AccountId),
					IsInteractable = !memberModel.IsMine() && !_blockedMembers.Contains(memberModel.Member.AccountId),
				});
		}

		private bool IsAllChecked(MemberModel[] memberModels, IEnumerable<CheckMemberListModel> models) => memberModels.Length > 0 && !models.Any(item => !item.IsChecked && !item.Info.IsMine());

		private bool IsAllChecked(TeamIdType teamId)
		{
			var employeeIds = DataManager.Instance.GetMemberIdsFromTeam(teamId);

			foreach (var id in employeeIds)
				if (!_checkedMembers.Contains(id))
					return false;

			return true;
		}
		private bool IsAllCheckedIncludeSubDepartment(TeamIdType teamId)
		{
			return IsAllCheckedSubDepartment(teamId);

			bool IsAllCheckedSubDepartment(TeamIdType subTeamId)
			{
				if (!IsAllChecked(subTeamId)) return false;

				var team = DataManager.Instance.GetTeam(subTeamId);

				foreach (var subDept in team.SubTeamIds)
					if (!IsAllCheckedSubDepartment(subDept))
						return false;

				return true;
			}
		}
		private string GetMemberCountStr(int total, int duplicateJob) =>
			$"{Localization.Instance.GetString(Constant.TextKey.AllMember)} {Convert.ToString(total)}  {Localization.Instance.GetString(Constant.TextKey.DuplicateJob)} {Convert.ToString(duplicateJob)}";

		private void RefreshCheckEmployeeHeight(bool isSearching)
		{
			CheckEmployeeHeight = isSearching ? CheckEmployeeSearchHeight : CheckEmployeeDefaultHeight;
		}
#endregion // UI

#region User State
		private void SendRequestUserState(MemberModel[] members)
		{
			var accountIds = members.Select(memberModel => memberModel.Member.AccountId);
			UserState.Info.Instance.SendLogOnOffUserInfoRequest(accountIds);
		}
#endregion // User State
		private void DisposeEmployee()
		{
			_employeeListModels = null;
			_checkedMembers.Clear();
			_blockedMembers.Clear();

			CheckEmployeeList.DestroyAll();
		}
	}
}
