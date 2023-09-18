/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingReservationViewModel_Request.cs
* Developer:	tlghks1009
* Date:			2022-09-02 16:29
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using MeetingUserType = Com2Verse.WebApi.Service.Components.MeetingMemberEntity;
using MemberType = Com2Verse.WebApi.Service.Components.MemberType;
using AttendanceCode = Com2Verse.WebApi.Service.Components.AttendanceCode;
using AuthorityCode = Com2Verse.WebApi.Service.Components.AuthorityCode;

namespace Com2Verse.UI
{
    public partial class MeetingReservationViewModel
    {
        private void RequestMeetingReservation(MeetingReservationCreditViewModel.ePaymentType paymentType)
        {
            var startDateTime    = ApplyTimeOption(_reservationStartTimeOptions, _indexOfStartTime);
            var endDateTime = startDateTime.AddMinutes((_indexOfInterval + 1) * _meetingReservationTimeInterval);

            var memberModel = DataManager.Instance.GetMember(User.Instance.CurrentUserData.ID);

            if (memberModel == null)
            {
                return;
            }

            if (!CanMakeReservation(startDateTime, endDateTime))
                return;

            var meetingInfo = MakeMeetingInfo(startDateTime, endDateTime, memberModel);

            Commander.Instance.RequestReservationAsync(meetingInfo, (Components.GroupAssetType)paymentType, OnResponseMeetingReservation, error => { }).Forget();
        }


        private void RequestMeetingReservationChange(DateTime startDateTime, DateTime endDateTime)
        {
            var memberModel = DataManager.Instance.GetMember(User.Instance.CurrentUserData.ID);


            if (memberModel == null)
            {
                UIManager.Instance.HideWaitingResponsePopup();

                return;
            }

            var meetingInfo = MakeMeetingInfo(startDateTime, endDateTime, memberModel);

            meetingInfo.MeetingId = MeetingInfo.MeetingId;
            meetingInfo.ChannelId = MeetingInfo.ChannelId;
            meetingInfo.FieldId   = MeetingInfo.FieldId;

            Commander.Instance.RequestMeetingReservationChangeAsync(meetingInfo, response =>
            {
                UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Reservation_ModifiedSetting_Toast"));

                ResetCloseReservationPopup();
                SetActive = false;
            }, error =>
            {
                ResetCloseReservationPopup();
                SetActive = false;
            }).Forget();
        }

        private bool CanMakeReservation(DateTime startDateTime, DateTime endDateTime)
        {
            var nowDateTime = MetaverseWatch.NowDateTime;

            if (startDateTime >= endDateTime)
            {
                UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Common_CheckReservationTimeIndex_Toast"));
                return false;
            }

            if (nowDateTime > endDateTime)
            {
                UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Common_CheckReservationTimeIndex_Toast"));
                return false;
            }

            if (nowDateTime > startDateTime)
            {
                UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Common_CheckReservationTimeIndex_Toast"));
                // refresh
                return false;
            }

            if (_meetingAllUserMemberId.Count > MeetingReservationProvider.MaxNumberOfParticipants)
            {
                UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_ConnectingApp_Reservation_ExceededInvitation_Toast"));
                return false;
            }

            return true;
        }


        private MeetingInfoType MakeMeetingInfo(DateTime startDateTime, DateTime endDateTime, MemberModel memberModel)
        {
            //FIXME : test key.
            var meetingInfo = new MeetingInfoType
            {
                MeetingName = string.IsNullOrEmpty(MeetingName) ? Localization.Instance.GetString("UI_ConnectingApp_Reservation_ConnectingName_Hint", HostName) : MeetingName,
                MeetingDescription = Description,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                CancelYn = "N",
                ChatNoteYn = WillUseStt ? "Y" : "N",
                VoiceRecordYn = WillUseVoiceRecording ? "Y" : "N",
                MeetingNoteYn = "N",
                PublicYn = IsPublic == 1 ? "Y" : "N",
                TemplateId = _selectedTemplateId,
                MaxUsersLimit = _maxUserLimit,
            };

            var participants = new List<MeetingUserType>();
            var organizerUserInfo = new MeetingUserType
            {
                AccountId = User.Instance.CurrentUserData.ID,
                MemberType = MemberType.CompanyEmployee,
                AttendanceCode = AttendanceCode.Join,
                AuthorityCode = AuthorityCode.Organizer,
                MemberName = memberModel.Member.MemberName,
            };
            participants.Add(organizerUserInfo);

            if (User.Instance.CurrentUserData is OfficeUserData userData)
            {
                foreach (var tagViewModel in _meetingTagCollection.Value)
                {
                    if (tagViewModel.MemberId == userData.ID)
                        continue;

                    var attendanceCode = AttendanceCode.JoinReceive;

                    // 이미 참여 중이었던 유저인지 확인
                    foreach (var userInfo in _meetingUserInfoList)
                    {
                        if (tagViewModel.MemberId == userInfo.AccountId)
                        {
                            attendanceCode = userInfo.AttendanceCode;
                            break;
                        }
                    }

                    var member = DataManager.Instance.GetMember(tagViewModel.MemberId);
                    var meetingUserInfo = new MeetingUserType
                    {
                        AccountId      = member.Member.AccountId,
                        MemberType     = MemberType.CompanyEmployee,
                        AttendanceCode = attendanceCode,
                        AuthorityCode  = AuthorityCode.Presenter,
                        MemberName     = member.Member.MemberName,
                    };

                    participants.Add(meetingUserInfo);
                }

                meetingInfo.MeetingMembers = participants.ToArray();
            }
            return meetingInfo;
        }

        private DateTime ApplyTimeOption(List<TimeOption>        options, int              indexOfTime)              => ApplyTimeOption(base.Model.SelectedDay, options, indexOfTime);
        private DateTime ApplyTimeOption(MeetingCalendar.DayInfo dayInfo, List<TimeOption> options, int indexOfTime) => ApplyTimeOption(dayInfo.ToDateTime(),   options, indexOfTime);
        private DateTime ApplyTimeOption(DateTime dateTime, List<TimeOption> options, int indexOfTime)
        {
            if (options[indexOfTime].Hour >= 24)
            {
                dateTime = dateTime.AddDays(1);
                dateTime.SetZeroHour();
            }
            else
            {
                dateTime.SetHour(options[indexOfTime].Hour);
            }

            dateTime.SetMinute(options[indexOfTime].Minute);
            dateTime.SetZeroSecond();
            return dateTime;
        }
    }
}
