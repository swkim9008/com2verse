/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingSearchViewModel.cs
* Developer:	ksw
* Date:			2023-03-07 11:52
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Net;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.UI;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;
using MeetingUserType = Com2Verse.WebApi.Service.Components.MeetingMemberEntity;
using MeetingIdType = System.Int64;
using MemberIdType = System.Int64;

namespace Com2Verse
{
	public sealed partial class MeetingInquiryListViewModel
	{
		// /api/Meeting/Reservation
		// 	회의 예약
		//
		// /api/Meeting/ReservationChange
		// 	예약 설정 변경(구현 중)
		//
		// /api/Meeting/ReservationCancel
		// 	예약 취소(구현 중)
		//
		// /api/Meeting/MyList
		// 	내 회의 목록

		// /api/Meeting/OrganizerChange
		// 	주최자 변경(구현 중)
		//
		// /api/Meeting/MemberDelete
		// 	참여자 삭제(구현 중)
		//
		// /api/Meeting/RoomJoin
		// 	회의실 입장
		//
		// /api/Meeting/RoomLeave
		// 	회의실 퇴장

		// /api/Meeting/MeetingInfo
		// 	특정 회의 정보 확인

		private void RequestSearchByMeetingCode(string meetingCode)
		{
			IsNotSearch = false;

			Commander.Instance.RequestSearchByMeetingCodeAsync(meetingCode, OnResponseSearchByMeetingCode, error =>
			{
				
			}).Forget();
		}

		private void RequestInquirySearchByDetail(DateTime startDateTime, DateTime endDateTime, string meetingName, MemberIdType organizerMemberId, MemberIdType participatingMemberId)
		{
			IsNotSearch = false;

			Commander.Instance.RequestSearchByDetailAsync(startDateTime, endDateTime, meetingName, organizerMemberId, participatingMemberId, UpcomingOrOngoing, OnResponseSearchByDetail, error =>
			{
				
			}).Forget();
		}

		private void RequestMeetingAttendance(long meetingId)
		{
			Commander.Instance.RequestMeetingAttendanceAsync(meetingId, OnResponseAttendance, error =>
			{
				
			}).Forget();
		}

		private void RequestMeetingAttendanceCancel(long meetingId)
		{
			Commander.Instance.RequestMeetingAttendanceCancelAsync(meetingId, OnResponseMeetingAttendanceCancel, error =>
			{
				
			}).Forget();
		}

		private void RequestMeetingInviteReject(long meetingId)
		{
			UIManager.Instance.ShowPopupCommon("현재 사용 불가능한 기능입니다(WebAPI 변경중)");
			return;
			//Network.Communication.PacketReceiver.Instance.MeetingInviteRejectResponse += OnResponseMeetingInviteReject;
			//Network.Communication.PacketReceiver.Instance.ErrorCodeResponse           += OnErrorConnectingInviteReject;

			//if (User.Instance.CurrentUserData is not OfficeUserData userData) return;
			//Commander.Instance.RequestConnectingInviteReject(meetingId, userData.EmployeeID);
		}
	}
}
