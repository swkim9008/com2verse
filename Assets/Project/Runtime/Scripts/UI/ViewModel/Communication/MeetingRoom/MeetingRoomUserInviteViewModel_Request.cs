/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomUserInviteViewModel.cs
* Developer:	ksw
* Date:			2023-04-17 14:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.UI;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;
using MeetingUserType = Com2Verse.WebApi.Service.Components.MeetingMemberEntity;
using MemberType = Com2Verse.WebApi.Service.Components.MemberType;
using AttendanceCode = Com2Verse.WebApi.Service.Components.AttendanceCode;
using AuthorityCode = Com2Verse.WebApi.Service.Components.AuthorityCode;

namespace Com2Verse
{
	public sealed partial class MeetingRoomUserInviteViewModel
	{
		private void RequestConnectingInvite()
		{
			var inviteUsers = new List<MeetingUserType>();

			foreach (var viewModel in _inviteTagCollection.Value)
			{
				var meetingUserInfo = new MeetingUserType
				{
					AccountId      = viewModel.InviteEmployeeNo,
					MemberType     = MemberType.CompanyEmployee,
					AttendanceCode = AttendanceCode.JoinReceive,
					AuthorityCode  = AuthorityCode.Presenter,
					IsEnter        = false,
					MemberName     = viewModel.InviteEmployeeName,
				};
				inviteUsers.Add(meetingUserInfo);
			}

			Commander.Instance.RequestMeetingInviteAsync(_meetingInfo.MeetingId, inviteUsers, OnResponseConnectingInvite, error =>
			{
				
			}).Forget();
		}

		private void RequestMeetingInfo()
		{
			Commander.Instance.RequestMeetingInfoAsync(MeetingReservationProvider.EnteredMeetingInfo.MeetingId, OnResponseMeetingInfo, error =>
			{
				
			}).Forget();
		}
	}
}
