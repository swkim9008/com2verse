/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationDataViewModel.DepartmentDetail.cs
* Developer:	jhkim
* Date:			2022-08-31 17:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Linq;
using Com2Verse.Logger;
using Com2Verse.Organization;
using TeamIdType = System.Int64;

namespace Com2Verse.UI
{
	// Department
	public partial class OrganizationDataViewModel
	{
#region Variables
		private string _departmentName;
		private Collection<OrganizationDataButtonListViewModel> _subDepartmentList = new();
		private Collection<OrganizationDataInfoListViewModel> _departmentInfoItemList = new();
#endregion // Variables

#region Properties
		public string DepartmentName
		{
			get => _departmentName;
			set
			{
				_departmentName = value;
				InvokePropertyValueChanged(nameof(DepartmentName), value);
			}
		}

		public Collection<OrganizationDataButtonListViewModel> SubDepartmentList
		{
			get => _subDepartmentList;
			set
			{
				_subDepartmentList = value;
				InvokePropertyValueChanged(nameof(SubDepartmentList), value);
			}
		}

		public Collection<OrganizationDataInfoListViewModel> DepartmentInfoItemList
		{
			get => _departmentInfoItemList;
			set
			{
				_departmentInfoItemList = value;
				InvokePropertyValueChanged(nameof(DepartmentInfoItemList), value);
			}
		}
#endregion // Properties

		void SetTeam(GroupModel groupModel)
		{
			DepartmentName = "회사 선택";
			if (groupModel == null) return;
			var group = groupModel.Group;
			var teamModels = group.Teams.Select(team => team.TeamId);
			SetSubTeams(teamModels.ToArray());
		}
		void SetTeam(TeamModel teamModel)
		{
			if (teamModel == null)
			{
				OnResetSelect();
				return;
			}

			DepartmentName = teamModel.Info.TeamName;
			SetSubTeams(teamModel.SubTeamIds);
			SetDepartmentInfo(teamModel);
			SetMemberList(teamModel.Info.TeamId);
		}

		void SetSubTeams(params TeamModel[] teams) => SetSubTeams(teams.Select(teamModel => teamModel.Info.TeamId).ToArray());

		void SetSubTeams(params TeamIdType[] subTeamIds)
		{
			ClearSubDepartmentList();
			foreach (var teamId in subTeamIds)
			{
				var subTeam = DataManager.Instance.GetTeam(teamId);
				if(subTeam == null) continue;

				SubDepartmentList.AddItem(new OrganizationDataButtonListViewModel(subTeam.Info.TeamName, () =>
				{
					C2VDebug.Log($"On Click Team = {subTeam.Info.TeamName}");
					ClearEmployeeInfo();
					SetTeam(subTeam);
				}));
			}
		}

		void SetDepartmentInfo(TeamModel teamModel)
		{
			ClearDepartmentInfo();

			// privateCommon.proto - DepartmentInfo
			Add("Department Info", teamModel.Info.TeamName);
			Add(nameof(teamModel.Info.TeamId), Convert.ToString(teamModel.Info.TeamId));
			Add(nameof(teamModel.Info.TeamName), teamModel.Info.TeamName);
			Add(nameof(teamModel.Info.SpaceId), teamModel.Info.SpaceId);
			Add(nameof(teamModel.Info.ParentTeamId), Convert.ToString(teamModel.Info.ParentTeamId), () => SetTeam(DataManager.Instance.GetTeam(teamModel.Info.ParentTeamId)));

			void Add(string key, string value, Action onSelect = null) => DepartmentInfoItemList.AddItem(new OrganizationDataInfoListViewModel(key, value, onSelect));
		}

		void ClearSubDepartmentList() => SubDepartmentList.Reset();
		void ClearDepartmentInfo() => DepartmentInfoItemList.Reset();
	}
}
