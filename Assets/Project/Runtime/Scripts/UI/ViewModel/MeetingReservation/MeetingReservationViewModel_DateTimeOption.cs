/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingReservationViewModel_DateTimeOption.cs
* Developer:	tlghks1009
* Date:			2022-10-25 13:40
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using TMPro;

namespace Com2Verse.UI
{
    public partial class MeetingReservationViewModel
    {
        private int _indexOfStartTime = 0;
        private int _indexOfInterval = 0;
        private int _isPublic = 0;

        public int IndexOfStartTime
        {
            get => _indexOfStartTime;
            set
            {
                _indexOfStartTime = value;
                RefreshReservationEndTime();
                base.InvokePropertyValueChanged(nameof(IndexOfStartTime), value);
            }
        }

        public int IndexOfInterval
        {
            get => _indexOfInterval;
            set
            {
                _indexOfInterval      = value;
                RefreshReservationEndTime();
                base.InvokePropertyValueChanged(nameof(IndexOfInterval), value);
            }
        }

        public int IsPublic
        {
            get => _isPublic;
            set
            {
                _isPublic = value;
                InvokePropertyValueChanged(nameof(IsPublic), value);
            }
        }


        public TMP_Dropdown.DropdownEvent DropDownEventOfStartTime
        {
            get => _dropDownEventOfStartTime;
            set => _dropDownEventOfStartTime = value;
        }


        public TMP_Dropdown.DropdownEvent DropDownEventOfInterval
        {
            get => _dropDownEventOfInterval;
            set => _dropDownEventOfInterval = value;
        }

        public TMP_Dropdown.DropdownEvent DropDownEventOfAccess
        {
            get => _dropDownEventOfAccess;
            set => _dropDownEventOfAccess = value;
        }

        public List<TMP_Dropdown.OptionData> MeetingStartTimeOptions
        {
            get => _meetingStartTimeOptions;
            set => SetProperty(ref _meetingStartTimeOptions, value);
        }

        public List<TMP_Dropdown.OptionData> MeetingIntervalOptions
        {
            get => _meetingIntervalOptions;
            set => SetProperty(ref _meetingIntervalOptions, value);
        }

        public List<TMP_Dropdown.OptionData> MeetingAccessOptions
        {
            get => _meetingAccessOptions;
            set => SetProperty(ref _meetingAccessOptions, value);
        }


        private void InitializeTimeIndexes()
        {
            var startDateTime = NowDateTime;
            var endDateTime = startDateTime.AddMinutes(_meetingDefaultMinute);
            // var endMinute = endDateTime.Minute >= _meetingMinuteInterval ? _meetingMinuteInterval : 0;

            IndexOfStartTime = 0;
            IndexOfInterval  = 1;
        }


        private void UpdateTimeOptions(List<TMP_Dropdown.OptionData> optionData, List<TimeOption> timeOptions, int newHour, int newMinute)
        {
            timeOptions.Clear();
            optionData.Clear();

            for (int hour = newHour; hour < _meetingEndHour; hour++)
            {
                for (int minute = newMinute; minute < 60; minute += _meetingStartTimeInterval)
                {
                    var timeOptionData = new TMP_Dropdown.OptionData($"{hour:00}:{minute:00}");
                    optionData.Add(timeOptionData);

                    var timeOption = new TimeOption
                    {
                        Index = timeOptions.Count,
                        Hour = hour,
                        Minute = minute,
                    };
                    timeOptions.Add(timeOption);
                }

                newMinute = 0;
            }

            base.InvokePropertyValueChanged(nameof(MeetingStartTimeOptions), MeetingStartTimeOptions);
        }


        private void InitializeTimeOptions(DateTime dateTime)
        {
            //var startMinute = dateTime.Minute > _meetingMinuteInterval ? _meetingMinuteInterval : 0;
            var startMinute = (dateTime.Minute / _meetingStartTimeInterval + 1) * _meetingStartTimeInterval;

            if (dateTime.Hour >= _meetingEndHour)
            {
                var nextDateTime = dateTime.AddDays(1);
                dateTime = new DateTime(nextDateTime.Year, nextDateTime.Month, nextDateTime.Day, _meetingStartHour, 0, 0);
                startMinute = 0;
            }

            //var endDateTime = dateTime.AddMinutes(_meetingMinuteInterval);
            //var endMinute = endDateTime.Minute >= _meetingMinuteInterval ? _meetingMinuteInterval : 0;

            UpdateTimeOptions( _meetingStartTimeOptions, _reservationStartTimeOptions, dateTime.Hour, startMinute);
        }


        private void UpdateTimeOptionsWhenDaySelected(MeetingCalendar.DayInfo dayInfo)
        {
            if (dayInfo.IsToday())
            {
                InitializeTimeOptions(MetaverseWatch.NowDateTime);
                InitializeTimeIndexes();
                return;
            }

            var previousStartTimeOption = _reservationStartTimeOptions[IndexOfStartTime];

            UpdateTimeOptions( _meetingStartTimeOptions, _reservationStartTimeOptions, _meetingStartHour, 0);

            IndexOfStartTime = FindIndexOfTimeOption(_reservationStartTimeOptions, previousStartTimeOption.Hour, previousStartTimeOption.Minute);
        }


        private void UpdateTimeOptions(MeetingCalendar.DayInfo dayInfo)
        {
            if (dayInfo.IsToday())
            {
                InitializeTimeOptions(MetaverseWatch.NowDateTime);
            }
            else
            {
                UpdateTimeOptions( _meetingStartTimeOptions, _reservationStartTimeOptions, _meetingStartHour, 0);
            }
        }

        private void RegisterDropdownAddListener()
        {
            _dropDownEventOfStartTime.AddListener((index) =>
            {
                _indexOfStartTime = index;
                RefreshReservationEndTime();
            });

            _dropDownEventOfInterval.AddListener((index) =>
            {
                _indexOfInterval = index;
                RefreshReservationEndTime();
            });
            
            _dropDownEventOfAccess.AddListener(index => { _isPublic = index; });
        }

        private void UnregisterDropdownListener()
        {
            _dropDownEventOfStartTime.RemoveAllListeners();
            _dropDownEventOfInterval.RemoveAllListeners();
            _dropDownEventOfAccess.RemoveAllListeners();
        }
    }
}
