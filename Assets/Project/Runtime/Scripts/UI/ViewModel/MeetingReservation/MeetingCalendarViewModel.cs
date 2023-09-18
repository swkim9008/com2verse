/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomReservationViewModel.cs
* Developer:	tlghks1009
* Date:			2022-08-29 13:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Protocols;

namespace Com2Verse.UI
{
#region Model
    public class MeetingCalendarModel : DataModel
    {
        public string CurrentDateText;

        public bool IsVisibleBackground;
        public bool IsVisibleCalendar;
        public bool IsVisibleTodayButton;
    }
#endregion Model

    [ViewModelGroup("MeetingReservation")]
    public sealed partial class MeetingCalendarViewModel : ViewModelDataBase<MeetingCalendarModel>
    {
        public enum eLocationType
        {
            RESERVATION,
            INFO_LIST,
        }

        private readonly List<MeetingDayViewModel> _meetingDayViewModelPool = new(40);
        private readonly List<MeetingDayViewModel> _usedMeetingDayViewModelPool = new();

#region Collection
        private Collection<MeetingDayViewModel> _meetingCalendarCollection = new();
#endregion Collection

#region Command
        // ReSharper disable InconsistentNaming
        public CommandHandler       Command_TodayButtonClick       { get; }
        public CommandHandler<bool> Command_ChangeCalendar         { get; }
        // ReSharper restore InconsistentNaming
        
#endregion Command
        private readonly eLocationType      _locationType;
        private readonly MeetingCalendar    _meetingCalendar;

        private DateTime _currentDate;

        private bool _willUpdateCalendar;
        private bool _isUpdating;
        public  bool IsDateChosenByForce { get; private set; } = true;

        public MeetingCalendarViewModel(eLocationType locationType, MeetingCalendar meetingCalendar)
        {
            RegisterEvents();

            Command_TodayButtonClick    = new CommandHandler(OnCommand_TodayButtonClicked);
            Command_ChangeCalendar      = new CommandHandler<bool>(OnCommand_ChangeCalendarButtonClicked);

            _locationType = locationType;
            _meetingCalendar = meetingCalendar; 
        }

        public override void OnInitialize()
        {
            base.OnInitialize();

            _currentDate = _meetingCalendar.LastSelectedDayInfo?.ToDateTime() ?? _meetingCalendar.GetToday().ToDateTime();

            _willUpdateCalendar = true;

            SetActiveBackground = true;
            SetActiveTodayButton    = false;

            CurrentDateText = DateTimeExtension.GetDateTimeOnlyDate(_currentDate);

            CreateMeetingViewModelPool();

            UpdateCalendar();
        }

        public Collection<MeetingDayViewModel> MeetingCalendarCollection
        {
            get => _meetingCalendarCollection;
            set
            {
                _meetingCalendarCollection = value;
                base.InvokePropertyValueChanged(nameof(MeetingCalendarCollection), value);
            }
        }

        public string CurrentDateText
        {
            get => base.Model.CurrentDateText;
            set => SetProperty(ref base.Model.CurrentDateText, value);
        }

        public bool SetActiveBackground
        {
            get => base.Model.IsVisibleBackground;
            set => SetProperty(ref base.Model.IsVisibleBackground, value);
        }

        public bool SetActiveTodayButton
        {
            get => base.Model.IsVisibleTodayButton;
            set => SetProperty(ref base.Model.IsVisibleTodayButton, value);
        }

        public bool SetActiveCalendar
        {
            get => base.Model.IsVisibleCalendar;
            set
            {
                base.Model.IsVisibleCalendar = value;
                if (!value)
                    UnregisterEvents();
                else
                    RegisterEvents();

                base.InvokePropertyValueChanged(nameof(SetActiveCalendar), value);
            }
        }

#region CommandHandler
        private void OnCommand_TodayButtonClicked()
        {
            SetActiveTodayButton = false;

            foreach (var meetingDay in _meetingCalendarCollection.Value)
            {
                if (meetingDay.IsToday)
                {
                    meetingDay.Selected = true;

                    UpdateDays(meetingDay);

                    SelectDayInfo(meetingDay.DayInfo);
                    return;
                }
            }

            var nowDateTime = MetaverseWatch.NowDateTime;

            ChangeCalendar(nowDateTime, nowDateTime.Day);
        }

        private void OnCommand_ChangeCalendarButtonClicked(bool isNext)
        {
            var newDateTime = isNext ? _currentDate.AddMonths(1) : _currentDate.AddMonths(-1);
            var selectedDayInfo = _meetingCalendar.LastSelectedDayInfo;
            var nowDateTime = MetaverseWatch.NowDateTime;
            int newDay = (nowDateTime.Year == newDateTime.Year && nowDateTime.Month == newDateTime.Month) ? nowDateTime.Day : 1;
            if (selectedDayInfo != null)
            {
                bool hasSelectedDay = selectedDayInfo.Year == newDateTime.Year && selectedDayInfo.Month == newDateTime.Month;
                if (hasSelectedDay) newDay = selectedDayInfo.DayNumber;
            }

            IsDateChosenByForce = true;

            ChangeCalendar(newDateTime, newDay);
        }

        public void ChangeCalendarDay(DateTime newDateTime)
        {
            IsDateChosenByForce = true;

            ChangeCalendar(newDateTime);
        }
#endregion CommandHandler

        private void ChangeCalendar(DateTime newDateTime, int newDay) => ChangeCalendar(newDateTime.Year, newDateTime.Month, newDay);
        private void ChangeCalendar(DateTime newDateTime) => ChangeCalendar(newDateTime.Year, newDateTime.Month, newDateTime.Day);
        private void ChangeCalendar(int year, int month, int newDay)
        {
            {
                _currentDate = new DateTime(year, month, newDay);

                _meetingCalendar.UpdateCalendar(year, month);
                _meetingCalendar.ClearAllMeetingInfo();
                _meetingCalendar.SetSelectedDay(year, month, newDay);
                _willUpdateCalendar = true;

                UpdateCalendar();
            }

            {
                var startDateTime = new DateTime(year: _currentDate.Year, month: _currentDate.Month, day: 1);
                var endDateTime = new DateTime(year: _currentDate.Year, month: _currentDate.Month, day: _meetingCalendar.TotalNumberOfDays);
                _onCalendarChangedEvent?.Invoke(startDateTime, endDateTime);
            }
        }

        private void UpdateCalendar()
        {
            if (!_willUpdateCalendar)
                return;
            _willUpdateCalendar = false;
            _isUpdating         = true;
            if (_meetingCalendarCollection.CollectionCount != 0)
            {
                AllReturnMeetingDayViewModel();
            }


            CurrentDateText = DateTimeExtension.GetDateTimeOnlyDate(_currentDate);

            foreach (var dayInfo in _meetingCalendar.GetDays())
            {
                var meetingDayViewModel = RentMeetingDayViewModelFromPool();

                meetingDayViewModel.Set(dayInfo, _meetingCalendar, OnDayClicked);

                _meetingCalendarCollection.AddItem(meetingDayViewModel);
            }

            _isUpdating = false;
        }

        private MeetingDayViewModel GetMeetingDayViewModel(int year, int month, int day)
        {
            foreach (var meetingDayViewModel in _meetingCalendarCollection.Value)
            {
                if (meetingDayViewModel.Equals(year, month, day))
                {
                    return meetingDayViewModel;
                }
            }

            return null;
        }

        private void UpdateDays(MeetingDayViewModel selectedMeetingDay)
        {
            CurrentDateText = DateTimeExtension.GetDateTimeOnlyDate(selectedMeetingDay.DayInfo.ToDateTime());

            _currentDate = new DateTime(selectedMeetingDay.DayInfo.Year, selectedMeetingDay.DayInfo.Month, selectedMeetingDay.DayInfo.DayNumber);

            foreach (var meetingDay in _meetingCalendarCollection.Value)
            {
                if (meetingDay == selectedMeetingDay) continue;

                meetingDay.Selected = false;
                meetingDay.SetActiveTextColor = false;

                meetingDay.RefreshState();
            }
        }


        private void OnDayClicked(MeetingDayViewModel selectedMeetingDay)
        {
            if (_isUpdating)
                return;
            SetActiveTodayButton = !selectedMeetingDay.DayInfo.IsToday();

            IsDateChosenByForce = false;

            UpdateDays(selectedMeetingDay);

            SelectDayInfo(selectedMeetingDay.DayInfo);

            _onDaySelectedEvent?.Invoke(selectedMeetingDay.DayInfo);
        }


        private void SelectDayInfo(MeetingCalendar.DayInfo dayInfo)
        {
            _meetingCalendar.SetSelectedDay(dayInfo);

            _willUpdateCalendar = false;
        }

        public void SetConnectingInfo()
        {
            _willUpdateCalendar = true;
            
            UpdateCalendar();

            SelectMeetingInfoWhenMeetingReservation();
        }

#region Events
        private Action<DateTime, DateTime> _onCalendarChangedEvent;
        public event Action<DateTime, DateTime> OnCalendarChangedEvent
        {
            add
            {
                _onCalendarChangedEvent -= value;
                _onCalendarChangedEvent += value;
            }
            remove => _onCalendarChangedEvent -= value;
        }

        private Action<MeetingCalendar.DayInfo> _onDaySelectedEvent;
        public event Action<MeetingCalendar.DayInfo> OnDaySelectedEvent
        {
            add
            {
                _onDaySelectedEvent -= value;
                _onDaySelectedEvent += value;
            }
            remove => _onDaySelectedEvent -= value;
        }

        private void RegisterEvents()
        {
            Network.Communication.PacketReceiver.Instance.MeetingReservationResponse -= OnResponseMeetingReservation;
            Network.Communication.PacketReceiver.Instance.MeetingReservationResponse += OnResponseMeetingReservation;

            Network.Communication.PacketReceiver.Instance.MeetingReservationChangeResponse -= OnResponseMeetingReservationChange;
            Network.Communication.PacketReceiver.Instance.MeetingReservationChangeResponse += OnResponseMeetingReservationChange;
        }

        private void UnregisterEvents()
        {
            Network.Communication.PacketReceiver.Instance.MeetingReservationResponse -= OnResponseMeetingReservation;

            Network.Communication.PacketReceiver.Instance.MeetingReservationChangeResponse -= OnResponseMeetingReservationChange;
        }
#endregion Events

#region Pool
        private void CreateMeetingViewModelPool()
        {
            int capacity = 40;
            int current = 0;
            while (current < capacity)
            {
                _meetingDayViewModelPool.Add(new MeetingDayViewModel(_locationType));
                current++;
            }
        }

        private MeetingDayViewModel RentMeetingDayViewModelFromPool()
        {
            if (_meetingDayViewModelPool.Count == 0)
            {
                _meetingDayViewModelPool.Add(new MeetingDayViewModel(_locationType));
            }

            var meetingDayViewModel = _meetingDayViewModelPool[0];
            _meetingDayViewModelPool.RemoveAt(0);

            _usedMeetingDayViewModelPool.Add(meetingDayViewModel);

            return meetingDayViewModel;
        }

        private void AllReturnMeetingDayViewModel()
        {
            foreach (var meetingDayViewModel in _usedMeetingDayViewModelPool)
            {
                _meetingDayViewModelPool.Add(meetingDayViewModel);
            }

            _usedMeetingDayViewModelPool.Clear();
            _meetingCalendarCollection.Reset();
        }
#endregion Pool
    }
}
