/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationDataViewModel.EmployeeDetail.cs
* Developer:	jhkim
* Date:			2022-08-31 17:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using Com2Verse.Organization;
using TeamIdType = System.Int64;

namespace Com2Verse.UI
{
	// Employee
	public partial class OrganizationDataViewModel
	{
#region Variables
		private string _memberName;
		private Collection<OrganizationDataButtonListViewModel> _memberList = new();
		private Collection<OrganizationDataInfoListViewModel> _memberInfoItemList = new();
#endregion // Variables

#region Properties
		public string EmployeeName
		{
			get => _memberName;
			set
			{
				_memberName = value;
				InvokePropertyValueChanged(nameof(EmployeeName), value);
			}
		}

		public Collection<OrganizationDataButtonListViewModel> EmployeeList
		{
			get => _memberList;
			set
			{
				_memberList = value;
				InvokePropertyValueChanged(nameof(EmployeeList), value);
			}
		}

		public Collection<OrganizationDataInfoListViewModel> EmployeeInfoItemList
		{
			get => _memberInfoItemList;
			set
			{
				_memberInfoItemList = value;
				InvokePropertyValueChanged(nameof(EmployeeInfoItemList), value);
			}
		}
#endregion // Properties

		void SetMemberList(TeamIdType teamId)
		{
			var dataManager = DataManager.Instance;
			var memberIds = dataManager.GetMemberIdsFromTeam(teamId);

			ClearMemberList();
			foreach (var memberId in memberIds)
			{
				var memberModel = dataManager.GetMember(memberId);
				EmployeeList.AddItem(new OrganizationDataButtonListViewModel(memberModel.Member.MemberName, () =>
				{
					C2VDebug.Log($"On Click Employee = {memberModel.Member.MemberName}, {memberModel.Member.AccountId}");
					SetMemberInfo(memberModel);
				}));
			}
		}

		void SetMemberInfo(MemberModel memberModel)
		{
			ClearEmployeeInfo();

			EmployeeName = memberModel.Member.MemberName;

			Add(nameof(memberModel.Member.Position), memberModel.Member.Position);
			Add("Primary Work", memberModel.TeamName);
			Add(nameof(memberModel.Member.MemberName), memberModel.Member.MemberName);
			Add(nameof(memberModel.Member.AccountId), Convert.ToString(memberModel.Member.AccountId));
			Add(nameof(memberModel.Member.MailAddress), memberModel.Member.MailAddress);
			Add(nameof(memberModel.Member.PhotoPath), memberModel.Member.PhotoPath);
			Add(nameof(memberModel.Member.TelNo), Convert.ToString(memberModel.Member.TelNo));
			Add("Formatted TelNo", memberModel.GetFormattedTelNo());

			void Add(string key, string value, Action onSelect = null) => EmployeeInfoItemList.AddItem(new OrganizationDataInfoListViewModel(key, value, onSelect));
		}

		void ClearMemberList() => EmployeeList.Reset();

		void ClearEmployeeInfo()
		{
			EmployeeName = string.Empty;
			EmployeeInfoItemList.Reset();
		}
	}
}
