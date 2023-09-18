/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingInfo.cs
* Developer:	tlghks1009
* Date:			2022-09-02 19:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.WebApi.Service;
using Cysharp.Text;
using Protocols;
using Protocols.OfficeMeeting;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using MeetingStatus = Com2Verse.WebApi.Service.Components.MeetingStatus;
using AttendanceCode = Com2Verse.WebApi.Service.Components.AttendanceCode;
using ResponseMyList = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityIEnumerableResponseFormat>;
using ResponseMeetingInfo = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityResponseFormat>;
using ResponseInviteCancel = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.InviteCancelResponseResponseFormat>;
using ResponseAttendanceCancel = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>;
using ResponseWaitListAccept = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>;
using ResponseWaitListReject = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>;

namespace Com2Verse.UI
{
    public partial class MeetingInfoListViewModel
    {
        private void OnResponseReservation(MeetingInfoType response)
        {
            // TODO : NEW_ORGANIZATION WEB API로 대체 (임시)
            // _selectedMeetingInfo = response.MeetingInfo;
            _selectedMeetingInfo = response;
            CompleteReservation();
        }

        private void OnResponseMeetingReservationChange(MeetingInfoType response)
        {
            _selectedMeetingInfo = response;
            RequestMeetingMyMeetList(_selectedMeetingInfo.StartDateTime, _selectedMeetingInfo.EndDateTime, OnResponseMyMeetingListWhenReservationStateChanged);
        }

        private void OnResponseMeetingUserDelete(MeetingUserDeleteResponse response)
        {
            // Network.Communication.PacketReceiver.Instance.MeetingMyListResponse += OnResponseMyMeetingListWhenReservationStateChanged;

            // TODO : NEW_ORGANIZATION WEB API로 대체 (RequestMyList)
            RequestMeetingMyMeetList(_selectedMeetingInfo.StartDateTime, _selectedMeetingInfo.EndDateTime, OnResponseMyMeetingListWhenReservationStateChanged);
        }

        private void OnResponseMeetingOrganizerChange(MeetingOrganizerChangeResponse response)
        {
            // Network.Communication.PacketReceiver.Instance.MeetingMyListResponse += OnResponseMyMeetingListWhenReservationStateChanged;

            // TODO : NEW_ORGANIZATION WEB API로 대체 (RequestMyList)
            RequestMeetingMyMeetList(_selectedMeetingInfo.StartDateTime, _selectedMeetingInfo.EndDateTime, OnResponseMyMeetingListWhenReservationStateChanged);
        }

        private void OnMeetingCalendarViewModelEventCalendarChanged(DateTime startDateTime, DateTime endDateTime)
        {
            // Network.Communication.PacketReceiver.Instance.MeetingMyListResponse += OnResponseMyMeetingListWhenCalendarChanged;

            // UIManager.Instance.ShowWaitingResponsePopup();

            MeetingCalendarViewModel.OnDaySelectedEvent -= OnMeetingCalendarViewModelEventDayClicked;

            RequestMeetingMyMeetList(startDateTime, endDateTime, OnResponseMyMeetingListWhenCalendarChanged);
        }


        private void OnMeetingCalendarViewModelEventDayClicked(MeetingCalendar.DayInfo dayInfo)
        {
            // Network.Communication.PacketReceiver.Instance.MeetingMyListResponse += OnResponseMyMeetingListWhenDayClicked;

            // UIManager.Instance.ShowWaitingResponsePopup();

            RequestMeetingMyMeetList(dayInfo.ToDateTime(), dayInfo.ToDateTime(), OnResponseMyMeetingListWhenDayClicked);

            SetSelectedDayInfo(dayInfo);
        }

        private void OnMeetingCalendarViewModelEventDayClickedByHomeCalendar(MeetingCalendar.DayInfo dayInfo)
        {
            // Network.Communication.PacketReceiver.Instance.MeetingMyListResponse += OnResponseMyMeetingListWhenDayClickedByHome;

            // UIManager.Instance.ShowWaitingResponsePopup();
            
            RequestMonthMyMeetingList(OnResponseMyMeetingListWhenDayClickedByHome);

            SetSelectedDayInfo(dayInfo);
        }

#region AfterProcess
        private void ResponseMeetingAttendanceCancel(ResponseAttendanceCancel response)
        {
            UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Card_CancelParticipation_Toast"));

            _attendanceCancelMeetingInfo = null;
            InitializeMeetingCalendar();
        }

        private void OnResponseMeetingInviteReject(MeetingInviteRejectResponse meetingInviteResponse)
        {
            Network.Communication.PacketReceiver.Instance.MeetingInviteRejectResponse -= OnResponseMeetingInviteReject;
            Network.Communication.PacketReceiver.Instance.ErrorCodeResponse           -= OnErrorConnectingInviteReject;

            RefreshDetailList();

            UIManager.Instance.HideWaitingResponsePopup();
            UIManager.Instance.SendToastMessage("UI_ConnectingApp_Card_DeclineInvitation_Toast");
        }

        private void OnErrorConnectingInviteReject(MessageTypes messageTypes, ErrorCode errorCode)
        {
            if (messageTypes != MessageTypes.MeetingInviteRejectResponse)
            {
                C2VDebug.LogError("Wrong MessageType ErrorCode!");
                return;
            }
            Network.Communication.PacketReceiver.Instance.MeetingInviteRejectResponse -= OnResponseMeetingInviteReject;
            Network.Communication.PacketReceiver.Instance.ErrorCodeResponse           -= OnErrorConnectingInviteReject;

            switch (errorCode)
            {
                // 요청 거절 했을 때, 회의가 종료됐을 경우
                case ErrorCode.NotMeetingStartReadyTimeBetweenEndReadyTime:
                    UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Reservation_AlreadyConnectingDone_Toast"));
                    break;
                // 요청 거절 했을 때, 회의가 취소된 경우
                case ErrorCode.PassedMeetingReadyTime:
                    UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCanceled_Toast"));
                    break;
                // 요청 거절 했을 때, 이미 주최자가 초대 취소한 경우 또는 이미 자신이 수락하거나 거절했을 경우
                case ErrorCode.MeetingNotExistsWaitlist:
                    UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCompleted_Toast"));
                    break;
                default:
                    UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_Common_UnknownProblemError_Popup_Text", ZString.Format("{0} : {1}", "ErrorCode", (int)errorCode)));
                    C2VDebug.LogError("OnErrorConnectingInviteReject ErrorCode : " + errorCode);
                    break;
            }

            RefreshDetailList();
            UIManager.Instance.HideWaitingResponsePopup();
        }

        
#endregion AfterProcess

        private void UpdateMeetingInfoList(List<MeetingInfoType> meetingInfoList)
        {
            ClearMeetingInfoList();

            var selectedDayInfo = _meetingCalendar.LastSelectedDayInfo;
            SetSelectedDayInfo(selectedDayInfo);

            var sortedMeetingInfoList = meetingInfoList?.OrderBy((x) =>
                                                         {
                                                             var nowDateTime = MetaverseWatch.NowDateTime;
                                                             var endDateTime = x.EndDateTime;
                                                             if (endDateTime > nowDateTime)
                                                             {
                                                                 return -1;
                                                             }

                                                             return 1;
                                                         })
                                                        .ThenBy(x => x.StartDateTime.Hour)
                                                        .ThenBy(x => x.StartDateTime.Minute)
                                                        .ThenBy(x => x.MeetingName).ToList();

            HasMeetingInfo = false;

            if (User.Instance.CurrentUserData is not OfficeUserData userData) return;
            
            foreach (var meetingInfo in sortedMeetingInfoList)
            {
                bool isWaiting = false;
                // 회의가 만료되거나 취소되었는데 내가 초대or참여 요청중인 상태라면 List 추가 안함
                if (meetingInfo.MeetingStatus is MeetingStatus.MeetingExpired or MeetingStatus.MeetingCancelAfterDelete or MeetingStatus.MeetingPassed)
                {
                    foreach (var meetingUserInfo in meetingInfo.MeetingMembers)
                    {
                        if (meetingUserInfo.AccountId == User.Instance.CurrentUserData.ID)
                        {
                            if (meetingUserInfo.AttendanceCode is AttendanceCode.JoinReceive or AttendanceCode.JoinRequest)
                                isWaiting = true;
                            break;
                        }
                    }
                }
                if (isWaiting)
                    continue;

                if (meetingInfo.StartDateTime.Year  == selectedDayInfo.Year  &&
                    meetingInfo.StartDateTime.Month == selectedDayInfo.Month &&
                    meetingInfo.StartDateTime.Day   == selectedDayInfo.DayNumber)
                {
                    UpdateMeetingInfo(meetingInfo);

                    HasMeetingInfo = true;
                }
            }


            //if (!HasMeetingInfo)
            //{
            //    UpdateMeetingInfo(null);
            //}
        }

        private void UpdateMeetingInfoListMonth(List<MeetingInfoType> meetingInfoList)
        {
            ClearMeetingInfoList();

            var selectedDayInfo = _meetingCalendar.LastSelectedDayInfo;
            SetSelectedDayInfo(selectedDayInfo);

            var sortedMeetingInfoList = meetingInfoList?.OrderBy((x) =>
                                                         {
                                                             var nowDateTime = MetaverseWatch.NowDateTime;
                                                             var endDateTime = x.EndDateTime;
                                                             if (endDateTime > nowDateTime)
                                                             {
                                                                 return -1;
                                                             }

                                                             return 1;
                                                         })
                                                        .ThenBy(x => x.StartDateTime.Hour)
                                                        .ThenBy(x => x.StartDateTime.Minute)
                                                        .ThenBy(x => x.MeetingName).ToList();

            _meetingCalendar.ClearAllMeetingInfo();

            HasMeetingInfo = false;

            if (User.Instance.CurrentUserData is not OfficeUserData userData) return;
            
            foreach (var meetingInfo in sortedMeetingInfoList)
            {
                bool isWaiting = false;
                // 회의가 만료되거나 취소되었는데 내가 초대or참여 요청중인 상태라면 List 추가 안함
                if (meetingInfo.MeetingStatus is MeetingStatus.MeetingExpired or MeetingStatus.MeetingCancelAfterDelete or MeetingStatus.MeetingPassed)
                {
                    foreach (var meetingUserInfo in meetingInfo.MeetingMembers)
                    {
                        if (meetingUserInfo.AccountId == User.Instance.CurrentUserData.ID)
                        {
                            if (meetingUserInfo.AttendanceCode is AttendanceCode.JoinReceive or AttendanceCode.JoinRequest)
                                isWaiting = true;
                            break;
                        }
                    }
                }
                if (isWaiting)
                    continue;

                if (meetingInfo.StartDateTime.Year  == selectedDayInfo.Year  &&
                    meetingInfo.StartDateTime.Month == selectedDayInfo.Month &&
                    meetingInfo.StartDateTime.Day   == selectedDayInfo.DayNumber)
                {
                    UpdateMeetingInfo(meetingInfo);

                    HasMeetingInfo = true;
                }

                _meetingCalendar.TryAddMeetingInfo(meetingInfo.StartDateTime.Year,
                                                   meetingInfo.StartDateTime.Month,
                                                   meetingInfo.StartDateTime.Day, meetingInfo);
            }

            _meetingCalendarViewModel?.SetConnectingInfo();

            //if (!HasMeetingInfo)
            //{
            //    UpdateMeetingInfo(null);
            //}
        }

        private void ScrollReset()
        {
            ScrollRectYPos = 0;
        }

        private void OnResponseInviteAccept(ResponseWaitListAccept response)
        {
            RefreshDetailList();
        }

        private void OnResponseInviteReject(ResponseWaitListReject response)
        {
            RefreshDetailList();
        }

#region Web API
#region MyList
        private void OnResponseMyMeetingListRefresh(ResponseMyList response)
        {
            UpdateMeetingInfoListMonth(response.Value.Data.ToList());

            CloseDetailPage();

            ScrollReset();
        }
        private void OnResponseChangedMeetingDay(ResponseMyList response)
        {
            UpdateMeetingInfoList(response.Value.Data?.ToList());

            ScrollReset();
        }
        
        private void OnResponseMyMeetingListWhenReservationCancel(ResponseMyList response)
        {
            ClearMeetingInfoList();

            UpdateMeetingInfoList(response.Value.Data?.ToList());

            CloseDetailPage();
        }
       
        private void OnResponseMyMeetingListWhenReservationStateChanged(ResponseMyList response)
        {
            ClearMeetingInfoList();

            UpdateMeetingInfoList(response.Value.Data?.ToList());

            UpdateDetailPage();

            OpenDetailPage();
        }
       
        private void OnResponseMyMeetingListWhenCalendarChanged(ResponseMyList response)
        {
            MeetingCalendarViewModel.OnDaySelectedEvent += OnMeetingCalendarViewModelEventDayClicked;

            ClearMeetingInfoList();

            UpdateMeetingInfoListMonth(response.Value.Data?.ToList());
            UpdateDetailPage();

            ScrollReset();
        }
        private void OnResponseMyMeetingListWhenDayClicked(ResponseMyList response)
        {
            UpdateMeetingInfoList(response.Value.Data?.ToList());

            CloseDetailPage();

            ScrollReset();

            OnNotifyInitializeCompleted();
        }
        private void OnResponseMyMeetingListWhenInitialized(ResponseMyList response)
        {
            ClearMeetingInfoList();

            UpdateMeetingInfoListMonth(response.Value.Data?.ToList());
            UpdateDetailPage();

            ScrollReset();

            MeetingCalendarViewModel?.ChangeCalendarDay(_selectedDayInfo.ToDateTime());
        }

        private void OnResponseMyMeetingListWhenDayClickedByHome(ResponseMyList response)
        {
            UpdateMeetingInfoListMonth(response.Value.Data?.ToList());

            CloseDetailPage();

            ScrollReset();

            OnNotifyInitializeCompleted();
        }

        private void OnResponseInviteCancel(ResponseInviteCancel response)
        {
            RefreshDetailList();
        }
#endregion // MyList

#region MeetingInfo
        // private void OnResponseMeetingInfo(MeetingInfoResponse response)
        // {
        //     Network.Communication.PacketReceiver.Instance.MeetingInfoResponse -= OnResponseMeetingInfo;
        //     Network.Communication.PacketReceiver.Instance.ErrorCodeResponse -= OnErrorMeetingInfoByDetailPage;
        //
        //     // TODO : NEW_ORGANIZATION WEB API로 대체 (임시)
        //     // OnInitializeDetailPage(response.MeetingInfo);
        //     OnInitializeDetailPage(response.MeetingInfo.Convert());
        // }

        private void OnResponseMeetingInfo(ResponseMeetingInfo response)
        {
            _selectedMeetingInfoViewModel.OnMoveToMeetingRoomButtonVisibleStateChanged -= OnMoveToMeetingRoomButtonVisibleStateChanged;

            OnInitializeDetailPage(response.Value.Data);
        }
#endregion // MeetingInfo
#endregion // Web API
    }
}