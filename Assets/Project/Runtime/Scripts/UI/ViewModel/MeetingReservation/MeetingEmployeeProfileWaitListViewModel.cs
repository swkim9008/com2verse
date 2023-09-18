/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingEmployeeProfileWaitListViewModel.cs
* Developer:	ksw
* Date:			2023-04-20 16:10
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Organization;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MeetingUserType = Com2Verse.WebApi.Service.Components.MeetingMemberEntity;
using MemberIdType = System.Int64;
using AttendanceCode = Com2Verse.WebApi.Service.Components.AttendanceCode;

namespace Com2Verse.UI
{
	public class MeetingEmployeeProfileWaitListModel : DataModel
	{
		public string  EmployeeName;
		public Texture EmployeeProfileTexture;
		public string  EmployeePositionLevelDeptInfo;
		public bool    IsRequestReceived;
		public bool    IsOrganizer;
	}


	[ViewModelGroup("MeetingReservation")]
	public sealed class MeetingEmployeeProfileWaitListViewModel : ViewModelDataBase<MeetingEmployeeProfileWaitListModel>
	{
		public CommandHandler CommandRequestCancel { get; }
		public CommandHandler CommandRequestAccept { get; }
		public CommandHandler CommandRequestReject { get; }

		private Action<MemberIdType> _onClickCancel;
		private Action<MemberIdType> _onClickAccept;
		private Action<MemberIdType> _onClickReject;

		private MemberIdType _memberId;

		public MeetingEmployeeProfileWaitListViewModel(MeetingUserType meetingUserInfo, bool isOrganizer, Action<MemberIdType> onClickCancel, Action<MemberIdType> onClickAccept, Action<MemberIdType> onClickReject)
		{
			CommandRequestCancel = new CommandHandler(OnClick_RequestInviteCancel);
			CommandRequestAccept = new CommandHandler(OnClick_RequestInviteAccept);
			CommandRequestReject = new CommandHandler(OnClick_RequestInviteReject);

			_onClickCancel = onClickCancel;
			_onClickAccept = onClickAccept;
			_onClickReject = onClickReject;

			IsOrganizer = isOrganizer;

			var memberModel = DataManager.Instance.GetMember(meetingUserInfo.AccountId);
			if (memberModel != null)
			{
				EmployeeName                  = memberModel.Member.MemberName;
				_memberId                     = memberModel.Member.AccountId;
				EmployeePositionLevelDeptInfo = memberModel.GetPositionLevelTeamStr();
				if (!string.IsNullOrEmpty(memberModel.Member.PhotoPath))
				{
					Util.DownloadTexture(memberModel.Member.PhotoPath,(success, texture) => EmployeeProfileTexture = texture).Forget();
				}
			}

			if (meetingUserInfo.AttendanceCode == AttendanceCode.JoinRequest)
			{
				IsRequestReceived = false;
			}
			else
			{
				IsRequestReceived = true;
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

		public bool IsRequestReceived
		{
			get => base.Model.IsRequestReceived;
			set => SetProperty(ref Model.IsRequestReceived, value);
		}

		public bool IsOrganizer
		{
			get => base.Model.IsOrganizer;
			set => SetProperty(ref Model.IsOrganizer, value);
		}

		private void OnClick_RequestInviteCancel()
		{
			_onClickCancel?.Invoke(_memberId);
		}

		private void OnClick_RequestInviteAccept()
		{
			_onClickAccept?.Invoke(_memberId);
		}

		private void OnClick_RequestInviteReject()
		{
			_onClickReject?.Invoke(_memberId);
		}
	}
}
