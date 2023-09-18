/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingReservationViewModel_Room.cs
* Developer:	tlghks1009
* Date:			2022-08-29 16:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Linq;
using Com2Verse.Logger;
using Protocols.OfficeMeeting;

namespace Com2Verse.UI
{
    public sealed partial class MeetingCalendarViewModel
    {
        private MeetingInfo _willSelectMeetingInfo;

        private void OnResponseMeetingReservation(MeetingReservationResponse response)
        {
            var startDateTime = response.MeetingInfo.StartDateTime;

            ChangeCalendar(startDateTime.Year, startDateTime.Month, startDateTime.Day);

            _willUpdateCalendar = true;

            _willSelectMeetingInfo = response.MeetingInfo;
        }


        private void OnResponseMeetingReservationChange(MeetingReservationChangeResponse response)
        {
            var startDateTime = response.MeetingInfo.StartDateTime;

            ChangeCalendar(startDateTime.Year, startDateTime.Month, startDateTime.Day);

            _willUpdateCalendar = true;

            _willSelectMeetingInfo = response.MeetingInfo;
        }

        // private void TryAddMeetingInfo(MeetingMyListResponse response)
        // {
        //     _meetingCalendar.ClearAllMeetingInfo();
        //
        //     foreach (var meetingInfo in response.MyMeetingInfo?.ToList())
        //     {
        //         var startDateTime = meetingInfo.StartDateTime;
        //
        //         _meetingCalendar.TryAddMeetingInfo(startDateTime.Year, startDateTime.Month, startDateTime.Day, meetingInfo);
        //     }
        // }

        private void SelectMeetingInfoWhenMeetingReservation()
        {
            if (_willSelectMeetingInfo != null)
            {
                var startDateTime = _willSelectMeetingInfo.StartDateTime;

                SelectMeetingDay(startDateTime.Year, startDateTime.Month, startDateTime.Day);

                _willSelectMeetingInfo = null;
            }
        }


        private void SelectMeetingDay(int year, int month, int day)
        {
            var meetingDayViewModel = GetMeetingDayViewModel(year, month, day);

            if (meetingDayViewModel == null)
                return;

            if (meetingDayViewModel.Selected)
                meetingDayViewModel.Selected = false;

            meetingDayViewModel.Selected = true;
        }
    }
}
