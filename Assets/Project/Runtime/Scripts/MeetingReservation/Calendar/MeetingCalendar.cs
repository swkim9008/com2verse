/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomCalendar.cs
* Developer:	tlghks1009
* Date:			2022-08-29 10:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Network;
using Protocols;
using Protocols.OfficeMeeting;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using MeetingIdType = System.Int64;

namespace Com2Verse.MeetingReservation
{
    public sealed class MeetingCalendar
    {
        public class DayInfo
        {
            private List<MeetingInfoType> _meetingInfoList;

            public int DayNumber { get; private set; }
            public int Year { get; private set; }
            public int Month { get; private set; }
            public IReadOnlyList<MeetingInfoType> MeetingInfoList => _meetingInfoList;

            public DayInfo(int year, int month, int day)
            {
                Year = year;
                Month = month;
                DayNumber = day;

                _meetingInfoList = new List<MeetingInfoType>();
            }

            public DayInfo Clone()
            {
                var dayInfo = new DayInfo(Year, Month, DayNumber)
                {
                    _meetingInfoList = _meetingInfoList,
                };
                return dayInfo;
            }


            public void AddMeetingInfo(MeetingInfoType newMeetingInfo)
            {
                foreach (var meetingInfo in _meetingInfoList)
                {
                    if (meetingInfo.MeetingId == newMeetingInfo.MeetingId)
                        return;
                }

                _meetingInfoList.Add(newMeetingInfo);
            }

            public void RemoveMeetingInfo(MeetingInfoType willRemovedMeetingInfo)
            {
                MeetingInfoType findMeetingInfo = null;
                foreach (var meetingInfo in _meetingInfoList)
                {
                    if (meetingInfo.MeetingId == willRemovedMeetingInfo.MeetingId)
                    {
                        findMeetingInfo = meetingInfo;
                        break;
                    }
                }

                if (findMeetingInfo == null) return;

                _meetingInfoList.Remove(findMeetingInfo);
            }


            public void ClearMeetingInfo()
            {
                _meetingInfoList.Clear();
            }

            public ProtoDateTime ToProtoDateTime()
            {
                var protoDateTime = new ProtoDateTime
                {
                    Year = Year,
                    Month = Month,
                    Day = DayNumber,
                };
                return protoDateTime;
            }

            public DateTime ToDateTime() => new DateTime(Year, Month, DayNumber);

            public bool IsToday()
            {
                var currentDate = MetaverseWatch.NowDateTime;

                return currentDate.Year == Year && currentDate.Month == Month && currentDate.Day == DayNumber;
            }

            public bool CanReserveMeetingRoom()
            {
                if (IsToday())
                {
                    if (MetaverseWatch.NowDateTime.Hour > 24)
                        return false;
                }
                else
                {
                    var dateTime = ToDateTime();
                    var nowDateTime = MetaverseWatch.NowDateTime;
                    var yearLater = nowDateTime.AddYears(1);

                    if (dateTime < nowDateTime) return false;
                    if (dateTime > yearLater) return false;
                }

                return true;
            }

            public void UpdateMeetingInfo(MeetingIdType meetingId, MeetingInfoType newMeetingInfo)
            {
                var meetingInfo = GetMeetingInfo(meetingId);
                if (meetingInfo == null)
                    return;

                RemoveMeetingInfo(meetingInfo);
                AddMeetingInfo(newMeetingInfo);
            }


            public MeetingInfoType GetMeetingInfo(long meetingId)
            {
                foreach (var meetingInfo in _meetingInfoList)
                {
                    if (meetingInfo.MeetingId == meetingId)
                        return meetingInfo;
                }

                return null;
            }
        }


        private readonly List<DayInfo> _days;
        private DayInfo _selectedDayInfo;

        private int _startDayOfWeek;
        public int TotalNumberOfDays { get; private set; }

        public MeetingCalendar()
        {
            _days = new List<DayInfo>();
        }

        public void UpdateCalendar(int year, int month)
        {
            _days.Clear();

            _startDayOfWeek = GetDayOfWeek(year, month);
            TotalNumberOfDays = GetTotalNumberOfDays(year, month);

            for (int col = 0; col < 6; col++)
            {
                for (int row = 0; row < 7; row++)
                {
                    int currentNum = (col * 7) + row;
                    var day = new DayInfo(year, month, (currentNum - _startDayOfWeek) + 1);

                    _days.Add(day);
                }
            }
        }

        public DayInfo AddDayInfo(int year, int month, int day)
        {
            var dayInfo = new DayInfo(year, month, day);
            _days.Add(dayInfo);

            return dayInfo;
        }

        public void SetSelectedDay(int year, int month, int day)
        {
            var selectedDayInfo = GetDayInfo(year, month, day) ?? AddDayInfo(year, month, day);
            _selectedDayInfo = selectedDayInfo.Clone();
        }

        public void SetSelectedDay(DayInfo dayInfo)
        {
            _selectedDayInfo = dayInfo.Clone();
        }

        public DayInfo LastSelectedDayInfo => _selectedDayInfo;

        public bool TryAddMeetingInfo(int year, int month, int day, MeetingInfoType newMeetingInfo)
        {
            var dayInfo = GetDayInfo(year, month, day);
            if (dayInfo != null)
            {
                dayInfo.AddMeetingInfo(newMeetingInfo);
                return true;
            }

            return false;
        }

        public DayInfo GetDayInfo(ProtoDateTime dateTime)
        {
            return GetDayInfo(dateTime.Year, dateTime.Month, dateTime.Day);
        }
        public DayInfo GetDayInfo(DateTime dateTime) => GetDayInfo(dateTime.Year, dateTime.Month, dateTime.Day);
        public DayInfo GetDayInfo(int year, int month, int day)
        {
            return _days.Find(x => x.Year == year && x.Month == month && x.DayNumber == day);
        }

        public DayInfo GetDayInfo(long meetingId)
        {
            foreach (var dayInfo in _days)
            {
                foreach (var meetingInfo in dayInfo.MeetingInfoList)
                {
                    if (meetingInfo.MeetingId == meetingId)
                        return dayInfo;
                }
            }

            return null;
        }

        public void ClearAllMeetingInfo()
        {
            foreach (var day in _days)
            {
                day.ClearMeetingInfo();
            }
        }

        public DayInfo GetToday()
        {
            foreach (var dayInfo in _days)
            {
                if (dayInfo.IsToday())
                    return dayInfo;
            }

            return null;
        }

        public List<DayInfo> GetDays() => _days;

        public int GetTotalNumberOfDays(int year, int month) => DateTime.DaysInMonth(year, month);

        private int GetDayOfWeek(int year, int month)
        {
            DateTime temp = new DateTime(year, month, 1);

            // Sun 0 ~ Sat 6
            return (int) temp.DayOfWeek;
        }
    }
}
