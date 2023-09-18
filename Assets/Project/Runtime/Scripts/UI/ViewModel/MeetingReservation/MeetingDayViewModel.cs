/*===============================================================
* Product:		Com2Verse
* File Name:	MettingRoomDayViewModel.cs
* Developer:	tlghks1009
* Date:			2022-08-29 11:24
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using UnityEngine;

namespace Com2Verse.UI
{
    [ViewModelGroup("MeetingReservation")]
    public sealed class MeetingDayViewModel : ViewModelBase
    {
        public MeetingCalendar.DayInfo DayInfo { get; private set; }
        private MeetingCalendar _meetingCalendar;

        private Action<MeetingDayViewModel> _onClicked;

        private readonly Color COLOR_UNABLE;
        private readonly Color COLOR_SELECTED;
        private readonly Color COLOR_MEETING;
        private readonly Color COLOR_TODAY;
        private readonly Color COLOR_DEFAULT_TEXT;
        private readonly Color COLOR_TODAY_TEXT = Color.black;

        private int _dayNumber;

        private MeetingCalendarViewModel.eLocationType _locationType;

        private bool _enable;
        private bool _activeTextColor, _activeRedDot;
        private bool _selected, _isToday, _hasMeeting, _unabled;

        private Color _color;
        private Color _colorText;

        public bool IsToday
        {
            get => _isToday;
            set
            {
                _isToday = value;
                OnTodayState();
            }
        }

        public bool IsSelect
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        public bool HasMeeting
        {
            get => _hasMeeting;
            set => SetProperty(ref _hasMeeting, value);
        }

        public CommandHandler Command_ButtonClick { get; private set; }


        public MeetingDayViewModel(MeetingCalendarViewModel.eLocationType locationType)
        {
            _locationType = locationType;

            ColorUtility.TryParseHtmlString("#01c654", out COLOR_TODAY);
            ColorUtility.TryParseHtmlString("#d9dce4", out COLOR_UNABLE);
            ColorUtility.TryParseHtmlString("#FFA500", out COLOR_MEETING);
            ColorUtility.TryParseHtmlString("#3C6CB3", out COLOR_SELECTED);
            ColorUtility.TryParseHtmlString("#33363D", out COLOR_DEFAULT_TEXT);
        }

        public void Set(MeetingCalendar.DayInfo dayInfo, MeetingCalendar meetingCalendar, Action<MeetingDayViewModel> onClicked)
        {
            Command_ButtonClick = new CommandHandler(OnClicked);

            _meetingCalendar = meetingCalendar;
            _onClicked = onClicked;
            DayInfo = dayInfo;

            DayNumber = DayInfo.DayNumber;
            Enable = DayInfo.DayNumber > 0 && DayInfo.DayNumber <= meetingCalendar.TotalNumberOfDays;

            _isToday = dayInfo.IsToday();
            _selected = IsSelected();
            _hasMeeting = DayInfo.MeetingInfoList.Count != 0;
            _unabled = false;
            _colorText = COLOR_DEFAULT_TEXT;

            if (Enable && _locationType == MeetingCalendarViewModel.eLocationType.RESERVATION)
                _unabled = !dayInfo.CanReserveMeetingRoom();

            SetActiveRedDot    = false;
            SetActiveTextColor = false;

            RefreshState();
        }

        public void RefreshState()
        {
            if (_unabled) OnUnableState();
            if (_hasMeeting) OnMeetingState();
            if (_selected) OnSelectedState();
            if (_isToday) OnTodayState();
        }

        public int DayNumber
        {
            get => _dayNumber;
            set => SetProperty(ref _dayNumber, value);
        }

        public bool Enable
        {
            get => _enable;
            set => SetProperty(ref _enable, value);
        }

        public bool SetActiveTextColor
        {
            get => _activeTextColor;
            set
            {
                if (!value)
                {
                    if (_isToday) return;
                    if (_selected) return;
                    if (_hasMeeting) return;
                }

                SetProperty(ref _activeTextColor, value);
            }
        }

        public Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public Color ColorText
        {
            get => _colorText;
            set => SetProperty(ref _colorText, value);
        }

        public bool Selected
        {
            get => _selected;
            set
            {
                if (_unabled) return;

                _selected = value;
                if (_selected)
                {
                    _onClicked?.Invoke(this);
                }
                SetProperty(ref _selected, value);
            }
        }


        public bool SetActiveRedDot
        {
            get => _activeRedDot;
            set => SetProperty(ref _activeRedDot, value);
        }

        public bool Equals(int year, int month, int day) => DayInfo.Year == year && DayInfo.Month == month && DayInfo.DayNumber == day;

        private void OnClicked()
        {
            if (!Enable) return;
            if (_unabled) return;

            OnSelectedState();
        }

        private void OnSelectedState()
        {
            if (!_isToday)
            {
                SetActiveTextColor = true;
                Color = COLOR_SELECTED;
            }
            Selected = true;
        }

        private void OnTodayState()
        {
            SetActiveTextColor = true;
            Color = COLOR_TODAY;
            ColorText = COLOR_TODAY_TEXT;
        }

        private void OnMeetingState()
        {
            _hasMeeting = true;
            SetActiveTextColor = true;
            Color = COLOR_MEETING;
            ColorText = COLOR_DEFAULT_TEXT;
        }

        private void OnUnableState()
        {
            _unabled = true;
            SetActiveTextColor = false;
            ColorText = COLOR_UNABLE;
        }

        private bool IsSelected()
        {
            var meetingCalendar = _meetingCalendar;
            var selectedDayInfo = meetingCalendar.LastSelectedDayInfo;

            if (meetingCalendar == null) return false;
            if (selectedDayInfo == null) return false;

            return DayInfo.Year == selectedDayInfo.Year && DayInfo.Month == selectedDayInfo.Month && DayInfo.DayNumber == selectedDayInfo.DayNumber;
        }
    }
}
