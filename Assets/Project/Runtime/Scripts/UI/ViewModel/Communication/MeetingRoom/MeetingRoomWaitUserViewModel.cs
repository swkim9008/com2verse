/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomUserViewModel.cs
* Developer:	tlghks1009
* Date:			2022-11-08 15:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.MeetingReservation;
using Com2Verse.Organization;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using MemberIdType = System.Int64;
using AttendanceCode = Com2Verse.WebApi.Service.Components.AttendanceCode;

namespace Com2Verse.UI
{
	public class MeetingRoomWaitUserModel : DataModel
	{
		public string EmployeeName;
		public string EmployeePosition;
		public Texture EmployeeProfileTexture;
	}

	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed partial class MeetingRoomWaitUserViewModel : ViewModelDataBase<MeetingRoomWaitUserModel>
	{
		public CommandHandler      CommandInviteCancelButtonClick              { get; }
		public CommandHandler      CommandInviteAcceptButtonClick              { get; }
		public CommandHandler      CommandInviteRejectButtonClick              { get; }

		private bool _isInviteRequestActive;
		private bool _isJoined;
		private long _meetingId;

		public event Action RefreshList;

		public MeetingRoomWaitUserViewModel(MemberModel employeePayload, AttendanceCode attendanceCode)
		{
			CommandInviteCancelButtonClick              = new CommandHandler(OnCommand_InviteCancelButtonClicked);
			CommandInviteAcceptButtonClick              = new CommandHandler(OnCommand_InviteAcceptButtonClicked);
			CommandInviteRejectButtonClick              = new CommandHandler(OnCommand_InviteRejectButtonClicked);

			_meetingId = MeetingReservationProvider.EnteredMeetingInfo.MeetingId;

			MemberId = employeePayload.Member.AccountId;
			EmployeePosition = employeePayload.GetPositionLevelTeamStr();
			EmployeeName = employeePayload.Member.MemberName;

			IsInviteRequestActive = attendanceCode == AttendanceCode.JoinRequest;
			IsJoined              = attendanceCode == AttendanceCode.Join;

			DownloadEmployeeProfileTexture(employeePayload);
		}

#region Properties
		public MemberIdType MemberId { get; private set; }
		public string EmployeeName
		{
			get => base.Model.EmployeeName;
			set => SetProperty(ref base.Model.EmployeeName, value);
		}

		public string EmployeePosition
		{
			get => base.Model.EmployeePosition;
			set => SetProperty(ref base.Model.EmployeePosition, value);
		}

		public Texture EmployeeProfileTexture
		{
			get => base.Model.EmployeeProfileTexture;
			set => SetProperty(ref base.Model.EmployeeProfileTexture, value);
		}
		
		public bool IsInviteRequestActive
		{
			get => _isInviteRequestActive;
			set => SetProperty(ref _isInviteRequestActive, value);
		}

		public bool IsJoined
		{
			get => _isJoined;
			set => SetProperty(ref _isJoined, value);
		}
#endregion
		private void DownloadEmployeeProfileTexture(MemberModel memberModel)
		{
			Util.DownloadTexture(memberModel.Member.PhotoPath, (success, texture) => EmployeeProfileTexture = texture).Forget();
		}

		private void OnCommand_InviteCancelButtonClicked()
		{
			RequestInviteCancel();
		}

		private void OnCommand_InviteAcceptButtonClicked()
		{
			RequestWaitListAccept();
		}

		private void OnCommand_InviteRejectButtonClicked()
		{
			RequestWaitListReject();
		}
	}
}
