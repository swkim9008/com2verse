/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingReservationViewModel.cs
* Developer:	tlghks1009
* Date:			2022-09-02 10:15
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.AssetSystem;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.WebApi.Service;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;

namespace Com2Verse.UI
{
    public static class MeetingReservationUIString
    {
        public static readonly string UIMeetingAppMeetingRoomTypeFailDescription = Localization.Instance.GetString("UI_MeetingAppReserve_Popup_Fail_Desc_MeetingRoomType");
        public static readonly string UIMeetingAppAddExplanationMeetingInfoDescription = Localization.Instance.GetString("UI_MeetingAppReserve_AddExplanation_MeetingInfo");
        public static string UIMeetingAppBtnReserveDescription => Localization.Instance.GetString("UI_MeetingAppReserve_Btn_Reserve");
        public static string UIMeetingAppBtnModifyDescription => Localization.Instance.GetString("UI_MeetingAppReserve_Btn_Modify");
    }

    [ViewModelGroup("MeetingReservation")]
    public partial class MeetingReservationViewModel : ViewModelDataBase<MeetingReservationModel>
    {
        private StackRegisterer _reservationGUIViewRegister;

        public StackRegisterer ReservationGUIViewRegister
        {
            get => _reservationGUIViewRegister;
            set
            {
                _reservationGUIViewRegister             =  value;
                _reservationGUIViewRegister.WantsToQuit += OnCommand_CloseReservationPopup;
            }
        }
        private struct TimeOption
        {
            public int Index;
            public int Hour;
            public int Minute;

            public bool IsTimeEquals(int hour, int minute) => Hour == hour && Minute == minute;
        }

        private readonly List<TimeOption> _reservationStartTimeOptions = new();

        private readonly int    _meetingStartHour                      = 0;
        private readonly int    _meetingEndHour                        = 24;
        private readonly int    _meetingStartTimeInterval              = 10;
        private readonly int    _meetingReservationTimeInterval        = 30;
        private readonly int    _meetingReservationTimeIntervalEndHour = 4;
        private readonly int    _meetingDefaultMinute                  = 60;
        private readonly int    _titleNameMaximumLength                = 30;
        private readonly int    _descriptionMaximumLength              = 100;
        //private readonly string _meetingNumberAvailableColor           = "#C1C4D3";
        //private readonly string _meetingEmployeeSelectColor            = "#06C757";

        private bool _isNewReservation;
        private bool _setActive = true;

        //private int _numberOfInvitees = 0;
        private int _numberOfParticipants;

        private UIInfo _uiInfoAccess;
        private UIInfo _uiInfoReservationUnit;
        private UIInfo _uiInfoReservationConfirm;

        private Vector2 _scrollReset;

        private TMP_Dropdown.DropdownEvent _dropDownEventOfStartTime;
        private TMP_Dropdown.DropdownEvent _dropDownEventOfInterval;
        private TMP_Dropdown.DropdownEvent _dropDownEventOfAccess;

        private List<TMP_Dropdown.OptionData> _meetingStartTimeOptions;
        private List<TMP_Dropdown.OptionData> _meetingIntervalOptions;
        private List<TMP_Dropdown.OptionData> _meetingAccessOptions;

        private MeetingCalendarViewModel _meetingCalendarViewModel;
        private MeetingCalendar _meetingCalendar;

        private Collection<MeetingReservationInfoViewModel>  _meetingReservationInfoCollection = new();
        private Collection<MeetingReservationSpaceViewModel> _meetingReservationSpaceCollection           = new();

        private MeetingInfoType MeetingInfo => base.Model.MeetingInfo;
        private DateTime NowDateTime => base.Model.NowDateTime;

        private GUIView                 _guiView;
        private Action                  _closeCallback;
        private Action<MeetingInfoType> _reservationCallback;
        private Action<MeetingInfoType> _reservationChangeCallback;

        private string _reservationEndTime;

        private long _selectedTemplateId;
        private int  _maxUserLimit;

        // ReSharper disable InconsistentNaming

        public CommandHandler<bool> Command_SetActiveCalendar              { get; }
        public CommandHandler       Command_ReservationClick               { get; }
        public CommandHandler       Command_OpenParticipantManagementPopup { get; }
        public CommandHandler       Command_CloseReservationPopup          { get; }
        public CommandHandler       Command_OpenOrganizationPopup          { get; }
        // ReSharper restore InconsistentNaming



        public MeetingReservationViewModel()
        {
            Command_SetActiveCalendar                  = new CommandHandler<bool>(OnCommand_CalendarClicked);
            Command_ReservationClick                   = new CommandHandler(OnCommand_ReservationClicked);
            Command_OpenParticipantManagementPopup     = new CommandHandler(OnCommand_OpenParticipantManagementPopup);
            Command_CloseReservationPopup              = new CommandHandler(OnCommand_CloseReservationPopup);
            Command_OpenOrganizationPopup              = new CommandHandler(OnCommand_OpenOrganization);

            _meetingStartTimeOptions = new List<TMP_Dropdown.OptionData>();
            _meetingIntervalOptions  = new List<TMP_Dropdown.OptionData>();
            _meetingAccessOptions    = new List<TMP_Dropdown.OptionData>();
        }

        public override void OnInitialize()
        {
            base.OnInitialize();

            base.Model.NowDateTime = MetaverseWatch.NowDateTime;

            _setActive = true;

            SetDropDownOptions();
        }

        private void SetDropDownOptions()
        {
            _meetingAccessOptions.Clear();
            _meetingIntervalOptions.Clear();
            var accessOptionData = new TMP_Dropdown.OptionData(Localization.Instance.GetString("UI_ConnectingApp_Reservation_Privacy_Hint_2"));
            _meetingAccessOptions.Add(accessOptionData);
            accessOptionData = new TMP_Dropdown.OptionData(Localization.Instance.GetString("UI_ConnectingApp_Reservation_Privacy_Hint_1"));
            _meetingAccessOptions.Add(accessOptionData);

            for (var i = 0; i <= _meetingReservationTimeIntervalEndHour; i++)
            {
                TMP_Dropdown.OptionData timeOptionData;
                if (i != 0)
                {
                    timeOptionData = new TMP_Dropdown.OptionData($"{i:00}:{0:00}");
                    _meetingIntervalOptions.Add(timeOptionData);
                }

                if (i != _meetingReservationTimeIntervalEndHour)
                {
                    timeOptionData = new TMP_Dropdown.OptionData($"{i:00}:{30:00}");
                    _meetingIntervalOptions.Add(timeOptionData);
                }
            }
            InvokePropertyValueChanged(nameof(MeetingIntervalOptions), MeetingIntervalOptions);
            InvokePropertyValueChanged(nameof(MeetingAccessOptions), MeetingAccessOptions);
        }

        public async UniTask Set(MeetingInfoType meetingInfo, Action closeCallback, Action<MeetingInfoType> reservationResponse, Action<MeetingInfoType> reservationChangeResponse, GUIView guiView)
        {
            Model.MeetingInfo = meetingInfo;
            await Model.RefreshReservationInfo();

            IsNewReservation = base.Model.MeetingInfo == null;

            MemberId = -1;
            TitleName = IsNewReservation
                ? Localization.Instance.GetString("UI_ConnectingApp_Reservation_Title_Text")
                : Localization.Instance.GetString("UI_ConnectingApp_Detail_Setting_ConnectingSettingTitle_Text");
            MeetingName               = base.Model.MeetingName;
            MeetingNamePlaceHolder    = base.Model.MeetingNamePlaceHolder;
            Description               = base.Model.Description;
            DescriptionPlaceHolder    = base.Model.DescriptionPlaceHolder;
            MeetingDate               = base.Model.MeetingDate;
            ReservationText           = base.Model.ReservationText;
            WillUseStt                = base.Model.WillUseStt;
            WillUseVoiceRecording     = base.Model.WillUseVoiceRecording;
            HostName                  = base.Model.HostName;
            HostAddress               = base.Model.HostAddress;
            HostPositionLevelDeptInfo = base.Model.HostPositionLevelDeptInfo;
            HostProfile               = base.Model.HostProfile;
            

            if (IsNewReservation)
            {
                InitializeTimeOptions(NowDateTime);
                InitializeTimeIndexes();
            }
            else
            {
                SetupMeetingUserInfo(meetingInfo);

                var startDateTime   = meetingInfo.StartDateTime;
                var endDateTime     = meetingInfo.EndDateTime;
                var interval        = endDateTime - startDateTime;

                UpdateTimeOptions(_meetingStartTimeOptions, _reservationStartTimeOptions, _meetingStartHour, 0);

                IndexOfStartTime = FindIndexOfTimeOption(_reservationStartTimeOptions, startDateTime.Hour, startDateTime.Minute);
                IndexOfInterval  = (int)interval.TotalMinutes / _meetingReservationTimeInterval - 1;
                IsPublic         = meetingInfo.PublicYn == "Y" ? 1 : 0;
            }

            RegisterDropdownAddListener();

            RefreshViewParticipants();

            CloseCalendar();

            SetActiveHelpInfoPage = false;
            SetActiveReservationInfo = false;

            _guiView                   = guiView;
            _closeCallback             = closeCallback;
            _reservationCallback       = reservationResponse;
            _reservationChangeCallback = reservationChangeResponse;

            _meetingReservationSpaceCollection.Reset();

            var meetingTemplates = MeetingReservationProvider.MeetingTemplates;
            if (meetingTemplates == null || meetingTemplates.Count == 0)
            {
                C2VDebug.LogError("MeetingTemplate is null! Set Default Template");
                var template = new Components.MeetingTemplate
                {
                    MaxUserLimit      = 30,
                    MeetingTemplateId = 30300501000,
                };
                var defaultSpace = new MeetingReservationSpaceViewModel();
                defaultSpace.Initialize(template, OnCommand_MeetingTypeClicked, true);
                _meetingReservationSpaceCollection.AddItem(defaultSpace);
                return;
            }

            foreach (var meetingTemplate in meetingTemplates)
            {
                var space = new MeetingReservationSpaceViewModel();
                if (_meetingReservationSpaceCollection.CollectionCount == 0)
                    space.Initialize(meetingTemplate, OnCommand_MeetingTypeClicked, true);
                else
                    space.Initialize(meetingTemplate, OnCommand_MeetingTypeClicked);
                _meetingReservationSpaceCollection.AddItem(space);
            }
        }

        public string TitleName
        {
            get => base.Model.TitleName;
            set => SetProperty(ref base.Model.TitleName, value);
        }
        public string MeetingName
        {
            get => base.Model.MeetingName;
            set
            {
                SetProperty(ref base.Model.MeetingName, value);
                base.InvokePropertyValueChanged(nameof(TitleNameLength), TitleNameLength);
            }
        }

        public string MeetingNamePlaceHolder
        {
            get => base.Model.MeetingNamePlaceHolder;
            set => SetProperty(ref base.Model.MeetingNamePlaceHolder, value);
        }

        public string Description
        {
            get => base.Model.Description;
            set
            {
                SetProperty(ref base.Model.Description, value);
                base.InvokePropertyValueChanged(nameof(DescriptionLength), DescriptionLength);
            }
        }

        public string DescriptionPlaceHolder
        {
            get => base.Model.DescriptionPlaceHolder;
            set => SetProperty(ref base.Model.DescriptionPlaceHolder, value);
        }

        public string ReservationText
        {
            get => base.Model.ReservationText;
            set => SetProperty(ref base.Model.ReservationText, value);
        }

        public string TitleNameLength => $"{MeetingName.Length}/{_titleNameMaximumLength}";

        public string DescriptionLength => $"{Description.Length}/{_descriptionMaximumLength}";

        public string ReservationTimeNotificationText
        {
            get => base.Model.ReservationTimeNotificationText;
            set => SetProperty(ref base.Model.ReservationTimeNotificationText, value);
        }

        public bool SetActive
        {
            get => _setActive;
            set
            {
                _setActive = value;

                if (!value) 
                    OnPopupClosed();

                base.InvokePropertyValueChanged(nameof(SetActive), value);
            }
        }

        public bool WillUseVoiceRecording
        {
            get => base.Model.WillUseVoiceRecording;
            set => SetProperty(ref base.Model.WillUseVoiceRecording, value);
        }

        public bool WillUseStt
        {
            get => base.Model.WillUseStt;
            set => SetProperty(ref base.Model.WillUseStt, value);
        }

        public string HostName
        {
            get => base.Model.HostName;
            set => SetProperty(ref base.Model.HostName, value);
        }

        public string HostPositionLevelDeptInfo
        {
            get => base.Model.HostPositionLevelDeptInfo;
            set => SetProperty(ref base.Model.HostPositionLevelDeptInfo, value);
        }

        public string HostAddress
        {
            get => base.Model.HostAddress;
            set => SetProperty(ref base.Model.HostAddress, value);
        }

        public bool IsNewReservation
        {
            get => _isNewReservation;
            set => SetProperty(ref _isNewReservation, value);
        }

        public Texture HostProfile
        {
            get => base.Model.HostProfile;
            set => SetProperty(ref base.Model.HostProfile, value);
        }

        public string MeetingDate
        {
            get => base.Model.MeetingDate;
            set => SetProperty(ref base.Model.MeetingDate, value);
        }

        public string ParticipantsText
        {
            get => base.Model.ParticipantsText;
            set => SetProperty(ref base.Model.ParticipantsText, value);
        }

        public string ParticipantsDetailText
        {
            get => base.Model.ParticipantsDetailText;
            set => SetProperty(ref base.Model.ParticipantsDetailText, value);
        }

        public string MeetingTypeNotificationText => MeetingReservationUIString.UIMeetingAppMeetingRoomTypeFailDescription;

        public bool SetActiveHelpInfoPage
        {
            get => base.Model.IsVisibleHelpInfoPage;
            set => SetProperty(ref base.Model.IsVisibleHelpInfoPage, value);
        }

        public bool SetActiveReservationInfo
        {
            get => base.Model.IsVisibleReservationInfo;
            set => SetProperty(ref base.Model.IsVisibleReservationInfo, value);
        }

        public bool SetActiveCalendarPopup
        {
            get => base.Model.IsVisibleCalendarPopup;
            set => SetProperty(ref base.Model.IsVisibleCalendarPopup, value);
        }

        public bool SetActiveDropDown
        {
            get => base.Model.IsVisibleDropdown;
            set => SetProperty(ref base.Model.IsVisibleDropdown, value);
        }

        public bool IsVisibleReservationTimeNotification
        {
            get => base.Model.IsVisibleReservationTimeNotification;
            set => SetProperty(ref base.Model.IsVisibleReservationTimeNotification, value);
        }

        public bool IsVisibleParticipantsText
        {
            get => base.Model.IsVisibleParticipantsText;
            set => SetProperty(ref base.Model.IsVisibleParticipantsText, value);
        }

        public UIInfo UIInfoAccessViewModel
        {
            get
            {
                if (_uiInfoAccess == null)
                    InitializeUIInfoAccess();
                return _uiInfoAccess;
            }
        }

        public UIInfo UIInfoReservationUnitViewModel
        {
            get
            {
                if (_uiInfoReservationUnit == null)
                    InitializeUIInfoReservationUnit();
                return _uiInfoReservationUnit;
            }
        }

        public UIInfo UIInfoReservationConfirmViewModel
        {
            get
            {
                if (_uiInfoReservationConfirm == null)
                    InitializeUIInfoReservationConfirm();
                return _uiInfoReservationConfirm;
            }
        }

        public float ScrollReset
        {
            get => 0;
            set => base.InvokePropertyValueChanged(nameof(ScrollReset), value);
        }

        public string ReservationEndTime
        {
            get => _reservationEndTime;
            set => SetProperty(ref _reservationEndTime, value);
        }

        public MeetingCalendarViewModel MeetingCalendarViewModel
        {
            get => _meetingCalendarViewModel;
            set
            {
                _meetingCalendarViewModel = value;
                base.InvokePropertyValueChanged(nameof(MeetingCalendarViewModel), value);
            }
        }

        public Collection<MeetingReservationInfoViewModel> MeetingReservationInfoCollection
        {
            get => _meetingReservationInfoCollection;
            set
            {
                _meetingReservationInfoCollection = value;
                base.InvokePropertyValueChanged(nameof(MeetingReservationInfoCollection), value);
            }
        }

        public Collection<MeetingReservationSpaceViewModel> MeetingReservationSpaceCollection
        {
            get => _meetingReservationSpaceCollection;
            set => SetProperty(ref _meetingReservationSpaceCollection, value);
        }


        private void OnEmployeeRemoveButtonClicked(MeetingTagViewModel meetingTagViewModel)
        {
            RemoveMeetingTagElement(meetingTagViewModel);
        }


        private void OnCommand_CalendarClicked(bool enable)
        {
            if (enable)
            {
                var nowDateTime = base.Model.SelectedDay.ToDateTime();

                _meetingCalendar = new MeetingCalendar();
                _meetingCalendar.UpdateCalendar(nowDateTime.Year, nowDateTime.Month);
                _meetingCalendar.SetSelectedDay(nowDateTime.Year, nowDateTime.Month, nowDateTime.Day);

                MeetingCalendarViewModel = new MeetingCalendarViewModel(MeetingCalendarViewModel.eLocationType.RESERVATION, _meetingCalendar);
                MeetingCalendarViewModel.OnDaySelectedEvent += OnReservationDaySelected;

                OpenCalendar();
            }
            else
            {
                MeetingCalendarViewModel.OnDaySelectedEvent -= OnReservationDaySelected;

                CloseCalendar();
            }
        }

        private void OnCommand_OpenParticipantManagementPopup()
        {
            RegisterEmployeeStateChangedEvent();

            MeetingReservationProvider.OpenParticipantManagementPopup(MeetingInfo);
        }


        public void OnCommand_CloseReservationPopup()
        {
            ResetCloseReservationPopup();
            SetActive = false;
        }

        private void OnCommand_ReservationClicked()
        {
            if (IsNewReservation)
            {
                MeetingReservationProvider.OpenMeetingReservationCreditPopup(_indexOfInterval + 1, RequestMeetingReservation);
                //RequestMeetingReservation();
            }
            else
            {
                // UIManager.Instance.ShowWaitingResponsePopup();

                var startDateTime = ApplyTimeOption(_reservationStartTimeOptions, _indexOfStartTime);
                var endDateTime   = startDateTime.AddMinutes((_indexOfInterval + 1) * _meetingReservationTimeInterval);

                var remainTime = MeetingInfo.StartDateTime - MetaverseWatch.NowDateTime;
                if (remainTime.TotalMinutes < MeetingReservationProvider.AdmissionTime)
                {
                    UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Reservation_CantSetting_Toast"));
                    // UIManager.Instance.HideWaitingResponsePopup();
                    OnCommand_CloseReservationPopup();
                    return;
                }
                
                if (!CanMakeReservation(startDateTime, endDateTime))
                {
                    // UIManager.Instance.HideWaitingResponsePopup();

                    return;
                }

                //RemoveEmployeeFromMeetingInfo();

                RequestMeetingReservationChange(startDateTime, endDateTime);
            }
        }

        private void OnCommand_MeetingTypeClicked(long templateId, int maxUserLimit)
        {
            //MeetingType = (MeetingTypeType) meetingType;
            foreach (var viewModel in _meetingReservationSpaceCollection.Value)
            {
                viewModel.SpaceSelected = false;
            }

            _selectedTemplateId = templateId;
            _maxUserLimit       = maxUserLimit;
            
            MeetingReservationProvider.MaxNumberOfParticipants = _maxUserLimit;
        }

        private void OnReservationDaySelected(MeetingCalendar.DayInfo dayInfo)
        {
            if (!dayInfo.CanReserveMeetingRoom())
                return;

            if (!MeetingCalendarViewModel.IsDateChosenByForce)
                CloseCalendar();

            base.Model.SelectedDay = dayInfo;

            MeetingDate = DateTimeExtension.GetDateTimeFullName(base.Model.SelectedDay.ToDateTime());
            
            UpdateTimeOptionsWhenDaySelected(base.Model.SelectedDay);
        }

        private int FindIndexOfTimeOption(List<TimeOption> timeOptions, int hour, int minute) => (from timeOption in timeOptions where timeOption.IsTimeEquals(hour, minute) select timeOption.Index).FirstOrDefault();

        private void RegisterEmployeeStateChangedEvent()
        {
            UnregisterEmployeeStateChangedEvent();

            Network.Communication.PacketReceiver.Instance.MeetingOrganizerChangeResponse += OnResponseMeetingOrganizerChange;
            Network.Communication.PacketReceiver.Instance.MeetingUserDeleteResponse += OnResponseEmployeeDeleted;
        }


        private void UnregisterEmployeeStateChangedEvent()
        {
            Network.Communication.PacketReceiver.Instance.MeetingOrganizerChangeResponse -= OnResponseMeetingOrganizerChange;
            Network.Communication.PacketReceiver.Instance.MeetingUserDeleteResponse -= OnResponseEmployeeDeleted;
        }


        private void OnPopupClosed()
        {
            _guiView.Hide();
            _closeCallback.Invoke();

            UnregisterDropdownListener();

            UnregisterEmployeeStateChangedEvent();
        }


        private void OpenCalendar() => SetActiveCalendarPopup = true;
        private void CloseCalendar() => SetActiveCalendarPopup = false;

        private void InitializeUIInfoAccess()
        {
            var titleString   = Localization.Instance.GetString("UI_ConnectingApp_Detail_Setting_Privacy_Tooltip_Title");
            var messageString = Localization.Instance.GetString("UI_ConnectingApp_Detail_Setting_Privacy_Tooltip_Text");

            _uiInfoAccess = new UIInfo(true);
            _uiInfoAccess.Set(UIInfo.eInfoType.INFO_TYPE_ACCESS_INFO, UIInfo.eInfoLayout.INFO_LAYOUT_DOWN, titleString, messageString);
        }

        private void InitializeUIInfoReservationUnit()
        {
            var titleString   = Localization.Instance.GetString("UI_ConnectingApp_Reservation_TimeUnitInfo_Tooltip_Text_1");
            var messageString = Localization.Instance.GetString("UI_ConnectingApp_Reservation_TimeUnitInfo_Tooltip_Text_2");

            _uiInfoReservationUnit = new UIInfo(true);
            _uiInfoReservationUnit.Set(UIInfo.eInfoType.INFO_TYPE_RESERVATION_UNIT_INFO, UIInfo.eInfoLayout.INFO_LAYOUT_UP, titleString, messageString);
        }

        private void InitializeUIInfoReservationConfirm()
        {
            var titleString   = Localization.Instance.GetString("UI_ConnectingApp_Reservation_PaymentPerTime_Tooltip_Text_1");
            var messageString = ZString.Format("{0}\n{1}", Localization.Instance.GetString("UI_ConnectingApp_Reservation_PaymentPerTime_Tooltip_Text_2"),
                                               Localization.Instance.GetString("UI_ConnectingApp_Reservation_PaymentPerTime_Tooltip_Text_3"));

            _uiInfoReservationConfirm = new UIInfo(true);
            _uiInfoReservationConfirm.Set(UIInfo.eInfoType.INFO_TYPE_RESERVATION_CONFIRM_INFO, UIInfo.eInfoLayout.INFO_LAYOUT_UP, titleString, messageString);
        }

        private void ResetCloseReservationPopup()
        {
            MeetingTagCollection.Reset();
            _meetingAllUserMemberId.Clear();
            _meetingBlockedMemberId.Clear();
            _meetingUserInfoList.Clear();

            _numberOfParticipants = 0;
            IsPublic              = 0;
        }

        private void RefreshReservationEndTime()
        {
            var startDateTime = ApplyTimeOption(_reservationStartTimeOptions, _indexOfStartTime);
            var endDateTime   = startDateTime.AddMinutes((_indexOfInterval + 1) * _meetingReservationTimeInterval);
            ReservationEndTime = ZString.Format("{0}.{1:00}.{2:00} {3:00}:{4:00} ~ {5}.{6:00}.{7:00} {8:00}:{9:00}",
                                                startDateTime.Year, startDateTime.Month,
                                                startDateTime.Day, startDateTime.Hour, startDateTime.Minute,
                                                endDateTime.Year, endDateTime.Month,
                                                endDateTime.Day, endDateTime.Hour, endDateTime.Minute);
        }

        public override void OnLanguageChanged()
        {
            base.OnLanguageChanged();
            InitializeUIInfoAccess();
            InitializeUIInfoReservationUnit();
            InitializeUIInfoReservationConfirm();
        }
    }
}
