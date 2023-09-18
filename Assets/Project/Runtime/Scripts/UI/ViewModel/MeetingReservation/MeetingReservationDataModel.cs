/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingReservationDataModel.cs
* Developer:	tlghks1009
* Date:			2022-10-18 16:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;

namespace Com2Verse.UI
{
    public class MeetingReservationModel : DataModel
    {
        public string      TitleName;
        public string      MeetingName;
        public string      MeetingNamePlaceHolder;
        public string      Description;
        public string      DescriptionPlaceHolder;
        public string      HostName;
        public string      HostPositionLevelDeptInfo;
        public string      HostAddress;
        public Texture     HostProfile;
        public string      MeetingDate = MetaverseWatch.NowDateTime.ToString("yyyy.MM.dd dddd");
        public string      ReservationTimeNotificationText;

        public string SearchMember;
        public string ReservationText;
        public string MemberName;
        public string ParticipantsText;
        public string ParticipantsDetailText;

        public string SchedulerDateText;
        public string SchedulerMeetingTypeText;

        public bool WillUseRepeatReservation;
        public bool WillUseVoiceRecording;
        public bool WillUseStt;

        public bool IsVisibleHelpInfoPage;
        public bool IsVisibleReservationInfo;
        public bool IsVisibleInvitePopup;
        public bool IsVisibleCalendarPopup;
        public bool IsVisibleDropdown;
        public bool IsVisibleReservationTimeNotification;
        public bool IsVisibleParticipantsText;

        public DateTime NowDateTime;
        public MeetingCalendar.DayInfo SelectedDay { get; set; }

        private MeetingInfoType _meetingInfo;

        public MeetingInfoType MeetingInfo
        {
            get => _meetingInfo;
            set
            {
                _meetingInfo = value;
                //RefreshReservationInfo();
            }
        }

        private bool HasMeetingInfo => _meetingInfo != null;

        public async UniTask RefreshReservationInfo()
        {
            SelectedDay = HasMeetingInfo
                ? MeetingReservationProvider.MeetingCalendar.GetDayInfo(MeetingInfo.StartDateTime)
                : MeetingReservationProvider.MeetingCalendar.GetToday() ??
                  MeetingReservationProvider.MeetingCalendar.AddDayInfo(NowDateTime.Year, NowDateTime.Month, NowDateTime.Day);

            MeetingName              = HasMeetingInfo ? _meetingInfo.MeetingName : string.Empty;
            MeetingNamePlaceHolder   = HasMeetingInfo ? string.Empty : Localization.Instance.GetString("UI_ConnectingApp_Reservation_ConnectingName_Text");
            Description            = HasMeetingInfo ? _meetingInfo.MeetingDescription : string.Empty;
            DescriptionPlaceHolder = HasMeetingInfo ? string.Empty : MeetingReservationUIString.UIMeetingAppAddExplanationMeetingInfoDescription;
            MeetingDate            = HasMeetingInfo ? DateTimeExtension.GetDateTimeFullName(_meetingInfo.StartDateTime) : DateTimeExtension.GetDateTimeFullName(NowDateTime);
            ReservationText = HasMeetingInfo
                ? Localization.Instance.GetString("UI_ConnectingApp_Detail_Setting_Modify_Btn")
                : Localization.Instance.GetString("UI_ConnectingApp_Reservation_Proceed_Btn");
            WillUseStt            = HasMeetingInfo && _meetingInfo.ChatNoteYn    == "Y";
            WillUseVoiceRecording = HasMeetingInfo && _meetingInfo.VoiceRecordYn == "Y";

            await RefreshViewAsync();
        }

        private async UniTask RefreshViewAsync()
        {
            var memberModel = await MeetingReservationProvider.GetOrganizerMemberModel(User.Instance.CurrentUserData.ID);

            MeetingNamePlaceHolder = HasMeetingInfo
                ? string.Empty
                : Localization.Instance.GetString("UI_ConnectingApp_Reservation_ConnectingName_Text");

            HostName = HasMeetingInfo
                ? MeetingReservationProvider.GetOrganizerName(_meetingInfo)
                : memberModel?.Member?.MemberName;

            HostAddress = memberModel?.Member?.MailAddress;

            HostPositionLevelDeptInfo = memberModel?.GetPositionLevelTeamStr();

            if (!string.IsNullOrEmpty(memberModel?.Member?.PhotoPath))
            {
                await Util.DownloadTexture(memberModel?.Member?.PhotoPath, (success, texture) => { HostProfile = texture; });
            }
        }
    }
}
