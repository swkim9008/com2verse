/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingInfoListViewModel_Request.cs
* Developer:	tlghks1009
* Date:			2022-09-02 19:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DelegateResponseMyList = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityIEnumerableResponseFormat>>;
using ResponseMyList = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityIEnumerableResponseFormat>;

namespace Com2Verse.UI
{
	public partial class MeetingInfoListViewModel
	{
		private void RequestMeetingMyMeetList(DateTime startDate, DateTime endDate, DelegateResponseMyList onResponse)
		{
			Commander.Instance.RequestMyListAsync(startDate, endDate, onResponse, error =>
			{
				
			}).Forget();
		}

		private void RequestMyMeetListRefresh(DateTime startDate, DateTime endDate)
		{
			// Network.Communication.PacketReceiver.Instance.MeetingMyListResponse += OnResponseMyMeetingListRefresh;

			RequestMeetingMyMeetList(startDate, endDate, OnResponseMyMeetingListRefresh);
		}

		private void RequestMeetingInfo(long meetingId)
		{
			Commander.Instance.RequestMeetingInfoAsync(meetingId, OnResponseMeetingInfo, error =>
			{
				_selectedMeetingInfoViewModel.OnMoveToMeetingRoomButtonVisibleStateChanged -= OnMoveToMeetingRoomButtonVisibleStateChanged;
				UpdateConnectingListWhenInvite();
			}).Forget();
		}

		// private void RequestMonthMyMeetingList()
		// {
		// 	var firstDateTime = MeetingReservationTimeHelper.FirstDateTime(0);
		// 	var lastDateTime = MeetingReservationTimeHelper.LastDateTime(firstDateTime);
		//
		// 	Commander.Instance.RequestMyMeetList(firstDateTime, lastDateTime);
		// }

		private void RequestMonthMyMeetingList(DelegateResponseMyList onResponse, Action<Commander.ErrorResponseData> onError = null)
		{
			var firstDateTime = MeetingReservationTimeHelper.FirstDateTime(0);
			var lastDateTime = MeetingReservationTimeHelper.LastDateTime(firstDateTime).AddDays(-1);

			Commander.Instance.RequestMyListAsync(firstDateTime, lastDateTime, onResponse, error =>
			{
				onError?.Invoke(error);
			}).Forget();
		}
		private void RequestMeetingCancel(long meetingId)
		{
			Commander.Instance.RequestMeetingReservationCancelAsync(meetingId, response =>
			{
				RequestMeetingMyMeetList(_selectedMeetingInfo.StartDateTime, _selectedMeetingInfo.EndDateTime, OnResponseMyMeetingListWhenReservationCancel);
			}, error =>
			{
				RequestMeetingMyMeetList(_selectedMeetingInfo.StartDateTime, _selectedMeetingInfo.EndDateTime, OnResponseMyMeetingListWhenReservationCancel);
			}).Forget();
		}

		private void OnRequestMeetingDayChanged(MeetingCalendar.DayInfo dayInfo)
		{
			// Network.Communication.PacketReceiver.Instance.MeetingMyListResponse += OnResponseChangedMeetingDay;

			// UIManager.Instance.ShowWaitingResponsePopup();
			RequestMeetingMyMeetList(dayInfo.ToDateTime(), dayInfo.ToDateTime(), OnResponseChangedMeetingDay);
		}

		private void RequestMeetingAttendanceCancel(GUIView _)
		{
			Commander.Instance.RequestMeetingAttendanceCancelAsync(_attendanceCancelMeetingInfo.MeetingId, ResponseMeetingAttendanceCancel, error =>
			{
				
			}).Forget();
		}

		// 참여자가 주최자로부터 온 초대 거절
		private void RequestConnectingInviteReject(long meetingId)
		{
			UIManager.Instance.ShowPopupCommon("현재 사용 불가능한 기능입니다(WebAPI 변경중)");
			return;
			//UIManager.Instance.ShowWaitingResponsePopup();
			//Network.Communication.PacketReceiver.Instance.MeetingInviteRejectResponse += OnResponseMeetingInviteReject;
			//Network.Communication.PacketReceiver.Instance.ErrorCodeResponse           += OnErrorConnectingInviteReject;
			//if (User.Instance.CurrentUserData is OfficeUserData userData)
			//	Commander.Instance.RequestConnectingInviteReject(meetingId, userData.EmployeeID);
		}

		// 회의 초대 취소
		private void RequestInviteCancel(long meetingId, long cancelAccountId)
		{
			Commander.Instance.RequestMeetingInviteCancelAsync(meetingId, cancelAccountId, OnResponseInviteCancel, error =>
			{
				
			}).Forget();
		}

		private void RequestWaitListAccept(long meetingId, long waitAccountId)
		{
			Commander.Instance.RequestMeetingWaitListAcceptAsync(meetingId, waitAccountId, OnResponseInviteAccept, error =>
			{
				
			}).Forget();
		}

		private void RequestWaitListReject(long meetingId, long waitAccountId)
		{
			Commander.Instance.RequestMeetingWaitListReject(meetingId, waitAccountId, OnResponseInviteReject, error =>
			{
				
			}).Forget();
		}
	}
}