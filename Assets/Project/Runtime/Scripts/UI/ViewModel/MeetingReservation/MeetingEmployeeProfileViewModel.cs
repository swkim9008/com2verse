/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingEmployeeProfileViewModel.cs
* Developer:	tlghks1009
* Date:			2022-09-13 19:34
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Organization;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MeetingUserType = Com2Verse.WebApi.Service.Components.MeetingMemberEntity;

namespace Com2Verse.UI
{
	public class MeetingEmployeeProfileModel : DataModel
	{
		public string  EmployeeName;
		public Texture EmployeeProfileTexture;
		public string  EmployeePositionLevelDeptInfo;
		public bool    IsMine;
	}


	[ViewModelGroup("MeetingReservation")]
	public sealed class MeetingEmployeeProfileViewModel : ViewModelDataBase<MeetingEmployeeProfileModel>
	{
		public MeetingEmployeeProfileViewModel(MeetingUserType meetingUserInfo)
		{
			var memberModel = DataManager.Instance.GetMember(meetingUserInfo.AccountId);
			if (memberModel != null)
			{
				EmployeeName                  = memberModel.Member.MemberName;
				EmployeePositionLevelDeptInfo = memberModel.GetPositionLevelTeamStr();
				IsMine                        = memberModel.IsMine();
				if (!string.IsNullOrEmpty(memberModel.Member.PhotoPath))
				{
					Util.DownloadTexture(memberModel.Member.PhotoPath, (success, texture) => EmployeeProfileTexture = texture).Forget();
				}
			}
			else
			{
				// 게스트가 아닌데 조직도에 검색이 안되는 사용자는 탈퇴된 사용자
				EmployeeName                  = Localization.Instance.GetString("UI_ConnectingApp_GroupMember_Deleted_Text");
				EmployeePositionLevelDeptInfo = string.Empty;
				IsMine                        = false;
			}
		}


		public string EmployeeName
		{
			get => base.Model.EmployeeName;
			set => SetProperty(ref base.Model.EmployeeName, value);
		}


		public Texture EmployeeProfileTexture
		{
			get => base.Model.EmployeeProfileTexture;
			set => SetProperty(ref base.Model.EmployeeProfileTexture, value);
		}

		public string EmployeePositionLevelDeptInfo
		{
			get => base.Model.EmployeePositionLevelDeptInfo;
			set => SetProperty(ref base.Model.EmployeePositionLevelDeptInfo, value);
		}

		public bool IsMine
		{
			get => base.Model.IsMine;
			set => SetProperty(ref Model.IsMine, value);
		}
	}
}
