/*===============================================================
* Product:		Com2Verse
* File Name:	NotificationJson_Version1.cs
* Developer:	tlghks1009
* Date:			2022-10-28 15:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Organization;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using Protocols.Notification;

namespace Com2Verse.Notification
{
    [Serializable]
    public sealed class NotificationJsonMeeting_Version1 : BaseNotificationJsonData
    {
        public struct NotificationDateTime
        {
            public int year;
            public int month;
            public int day;
            public DayOfWeek dayOfWeek;
            public int hour;
            public int minute;

            public string GetLocalizationKeyOfDayOfWeek() => DateTimeExtension.GetLocalizationKeyOfDayOfWeek(dayOfWeek);
            public string Time => $"{hour:00}:{minute:00}";
        }

#region Text String
        private static readonly string NOTIFICATION_MEETING_START_MEETING_MSG = "UI_Notification_Meeting_StartMeeting_Msg";
        private static readonly string NOTIFICATION_MEETING_CANCEL_RESERVATION_MSG = "UI_Notification_Meeting_CancelReservation_Msg";
        private static readonly string NOTIFICATION_MEETING_CHANGE_DATE_MSG = "UI_Notification_Meeting_ChangeDate_Msg";
        private static readonly string NOTIFICATION_MEETING_INVITE_MSG = "UI_Notification_Meeting_Invite_Msg";
        private static readonly string NOTIFICATION_MEETING_CANCEL_PARTICIPATION_MSG = "UI_Notification_Meeting_CancelParticipation_Msg";
        private static readonly string NOTIFICATION_MEETING_CHANGE_HOST_MSG = "UI_Notification_Meeting_ChangeHost_Msg";
#endregion Text String

        public string organizer_account_id;
        public string meeting_name;
        public string start_tick;
        public string prev_start_tick;

        public override async UniTask<string> GetDescription(NotificationInfo notificationInfo)
        {
            // switch (notificationInfo.SubTypeMeeting)
            // {
            //     case SubTypeMeeting.ReminderMeetingStart:
            //     {
            //         var startDateTime = StartDateTime;
            //         return Localization.Instance.GetString(NOTIFICATION_MEETING_START_MEETING_MSG,
            //                                                startDateTime.day.ToString(),
            //                                                startDateTime.GetLocalizationKeyOfDayOfWeek(),
            //                                                startDateTime.Time, MeetingName);
            //     }
            //     case SubTypeMeeting.CancelReservedMeeting:
            //     {
            //         var startDateTime = StartDateTime;
            //         return Localization.Instance.GetString(NOTIFICATION_MEETING_CANCEL_RESERVATION_MSG,
            //                                                startDateTime.day.ToString(),
            //                                                startDateTime.GetLocalizationKeyOfDayOfWeek(),
            //                                                startDateTime.Time,
            //                                                MeetingName);
            //     }
            //
            //     case SubTypeMeeting.ChangeMeetingTime:
            //     {
            //         var prevDateTime = PrevDateTime;
            //         var startDateTime = StartDateTime;
            //         return Localization.Instance.GetString(NOTIFICATION_MEETING_CHANGE_DATE_MSG,
            //                                                prevDateTime.day.ToString(),
            //                                                prevDateTime.GetLocalizationKeyOfDayOfWeek(),
            //                                                prevDateTime.Time,
            //                                                MeetingName,
            //                                                startDateTime.day.ToString(),
            //                                                startDateTime.GetLocalizationKeyOfDayOfWeek(), startDateTime.Time);
            //     }
            //
            //     case SubTypeMeeting.ReserveMember:
            //     {
            //         var organizerName = await GetEmployeeNameAsync(organizer_account_id);
            //         var startDateTime = StartDateTime;
            //
            //         return Localization.Instance.GetString(NOTIFICATION_MEETING_INVITE_MSG,
            //                                                organizerName,
            //                                                startDateTime.day.ToString(),
            //                                                startDateTime.GetLocalizationKeyOfDayOfWeek(),
            //                                                startDateTime.Time,
            //                                                MeetingName);
            //     }
            //     case SubTypeMeeting.DeleteMember:
            //     {
            //         var startDateTime = StartDateTime;
            //         return Localization.Instance.GetString(NOTIFICATION_MEETING_CANCEL_PARTICIPATION_MSG,
            //                                                startDateTime.day.ToString(),
            //                                                startDateTime.GetLocalizationKeyOfDayOfWeek(),
            //                                                startDateTime.Time,
            //                                                MeetingName);
            //     }
            //
            //     case SubTypeMeeting.ChangeOrganizer:
            //     {
            //         var organizerName = await GetEmployeeNameAsync(organizer_account_id);
            //         var startDateTime = StartDateTime;
            //
            //         return Localization.Instance.GetString(NOTIFICATION_MEETING_CHANGE_HOST_MSG,
            //                                                organizerName,
            //                                                startDateTime.day.ToString(),
            //                                                startDateTime.GetLocalizationKeyOfDayOfWeek(),
            //                                                startDateTime.Time, MeetingName);
            //     }
            // }

            return string.Empty;
        }


        public NotificationDateTime PrevDateTime => ConvertToNotificationDateTime(prev_start_tick);
        public NotificationDateTime StartDateTime => ConvertToNotificationDateTime(start_tick);

        public override string GetTitleName(NotificationInfo notificationInfo)
        {
            // switch (notificationInfo.SubTypeMeeting)
            // {
            //     case SubTypeMeeting.ReminderMeetingStart:  return Localization.Instance.GetString("UI_Notification_Meeting_StartMeeting_Title");
            //     case SubTypeMeeting.CancelReservedMeeting: return Localization.Instance.GetString("UI_Notification_Meeting_CancelReservation_Title");
            //     case SubTypeMeeting.ChangeMeetingTime:     return Localization.Instance.GetString("UI_Notification_Meeting_ChangeDate_Title");
            //     case SubTypeMeeting.ReserveMember:         return Localization.Instance.GetString("UI_Notification_Meeting_Invite_Title");
            //     case SubTypeMeeting.DeleteMember:          return Localization.Instance.GetString("UI_Notification_Meeting_CancelParticipation_Title");
            //     case SubTypeMeeting.ChangeOrganizer:       return Localization.Instance.GetString("UI_Notification_Meeting_ChangeHost_Title");
            // }

            return string.Empty;
        }

        public string MeetingName => meeting_name;

        private NotificationDateTime ConvertToNotificationDateTime(string tick)
        {
            long value = Int64.Parse(tick!, System.Globalization.NumberStyles.HexNumber);

            var dateTime = new DateTime(value).ToLocalTime();

            var notificationDateTime = new NotificationDateTime
            {
                year = dateTime.Year,
                month = dateTime.Month,
                day = dateTime.Day,
                dayOfWeek = dateTime.DayOfWeek,
                hour = dateTime.Hour,
                minute = dateTime.Minute,
            };

            return notificationDateTime;
        }

        private async UniTask<string> GetEmployeeNameAsync(string accountIdTick)
        {
            var accountId = Int64.Parse(accountIdTick!, System.Globalization.NumberStyles.HexNumber);

            var memberModel = await DataManager.Instance.GetMemberAsync(accountId);

            return memberModel == null ? string.Empty : memberModel.Member.MemberName;
        }
    }
}
