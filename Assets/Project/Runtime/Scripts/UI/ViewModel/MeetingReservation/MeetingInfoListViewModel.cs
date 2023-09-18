/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingInfoViewModel.cs
* Developer:	tlghks1009
* Date:			2022-09-02 13:37
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Net;
using Com2Verse.Data;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.WebApi.Service;
using UnityEngine;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;

namespace Com2Verse.UI
{
#region Model
	public class MeetingInfoListModel : DataModel
	{
		public string UseVoiceRecordingAndStt;
		public string MeetingName;
		public string MeetingID;
		public string MeetingDescription;
		public string MeetingDate;
		public string MeetingType;
		public string MeetingOrganizerName;
		public string MeetingParticipantText;    // 참여 인원
		public string MeetingParticipantMembers; // 참여 인원 수 {0} 명
		public string MeetingParticipantDetailText;
		public string MeetingWaitListText; // 대기자 인원
		public string MeetingWaitListMembers; // 대기 인원 수 {0} 명

		public bool IsVisibleReservationChangeButton;
		public bool IsVisibleReservationCancelButton;
		public bool IsVisibleMoveToMeetingRoomButton;

		public Texture SpaceTexture;
		public string  SpaceTitle;
		public string  SpaceDescription;
	}
#endregion Model

	[ViewModelGroup("MeetingReservation")]
	public partial class MeetingInfoListViewModel : ViewModelDataBase<MeetingInfoListModel>
	{
		private Collection<MeetingInfoViewModel> _meetingInfoCollection = new();

		private bool _setActive = true;
		private bool _setActiveDetailPage;
		private bool _setActiveCalendarPopup;
		private bool _hasMeetingInfo;

		private float _scrollRectYPos = -100f;
		private bool _contentSizeFitterEnabled;
		private MeetingCalendar.DayInfo _selectedDayInfo;
		private string _selectedDateText;
		private string _selectedDateDayOfWeek;

		private MeetingCalendarViewModel _meetingCalendarViewModel;
		private MeetingCalendar _meetingCalendar;

		public GUIView GuiView;

		public event Action OnInitializeCompleted;

		// ReSharper disable InconsistentNaming
		public CommandHandler       Command_DeactivateDetailPageClick         { get; }
		public CommandHandler       Command_CloseCalendar                     { get; }
		public CommandHandler       Command_MeetingCancelButtonClick          { get; }
		public CommandHandler       Command_OpenReservationPopup              { get; }
		public CommandHandler       Command_OpenReservationInquiryPopup       { get; }
		public CommandHandler       Command_CloseMeetingReservation           { get; }
		public CommandHandler       Command_ParticipatingPopupOpenButtonClick { get; }
		public CommandHandler       Command_CopyMeetingCode                   { get; }
		public CommandHandler       Command_CopyInviteInfoClicked             { get; }
		public CommandHandler<bool> Command_ConnectingAppCalendarActive       { get; }
		public CommandHandler<bool> Command_ChangeCalendarOneDay              { get; }
		public CommandHandler       Command_CloseConnectingApp                { get; }
		// ReSharper restore InconsistentNaming

		public MeetingInfoListViewModel()
		{
			Command_OpenParticipantsList              = new CommandHandler(OnCommand_OpenParticipantsList);
			Command_CloseParticipantsList             = new CommandHandler(OnCommand_CloseParticipantsList);
			Command_OpenWaitUserList                  = new CommandHandler(OnCommand_OpenWaitUserList);
			Command_CloseWaitUserList                 = new CommandHandler(OnCommand_CloseWaitUserList);
			Command_OpenReservationChangePopup        = new CommandHandler(OnCommand_OpenReservationChangePopup);
			Command_OpenReservationPopup              = new CommandHandler(OnCommand_OpenReservationPopup);
			Command_OpenReservationInquiryPopup       = new CommandHandler(OnCommand_OpenReservationInquiryPopup);
			Command_DeactivateDetailPageClick         = new CommandHandler(OnCommand_CloseDetailPage);
			Command_CloseCalendar                     = new CommandHandler(OnCommand_CloseCalendar);
			Command_MeetingCancelButtonClick          = new CommandHandler(OnCommand_MeetingCancelButtonClicked);
			Command_CloseMeetingReservation           = new CommandHandler(OnCommand_CloseMeetingReservation);
			Command_MoveToMeetingRoomButtonClick      = new CommandHandler(OnCommand_MoveToMeetingRoomButtonClicked);
			Command_ParticipatingPopupOpenButtonClick = new CommandHandler(OnCommand_ParticipatingPopupOpenButtonClicked);
			Command_CopyMeetingCode                   = new CommandHandler(OnCommand_CopyMeetingCode);
			Command_CopyInviteInfoClicked             = new CommandHandler(OnCommand_CopyInviteInfoClicked);
			Command_ConnectingAppCalendarActive       = new CommandHandler<bool>(OnCommand_ConnectingAppCalendarActive);
			Command_ChangeCalendarOneDay              = new CommandHandler<bool>(OnCommand_ChangeCalendarOneDayButtonClicked);
			Command_CloseConnectingApp                = new CommandHandler(CloseConnectingApp);
			Command_OpenMeetingMinutesPopup           = new CommandHandler(OnCommand_OpenMeetingMinutesPopup);
		}

		public override void OnInitialize()
		{
			base.OnInitialize();

			_setActive = true;
			ScrollReset();
			CloseDetailPage();
			InitializeMeetingCalendar();
		}

		public void OnOpenedGUIView(GUIView guiView)
		{
			// TODO : 1. 계정 바인딩이 안 된 상태 (현재는 서비스 인증이 되지 않은 상태), 2. Guest인 상태 두 가지 경우 GUIView 강제 종료
			if (CurrentScene.ServiceType is not eServiceType.OFFICE)
				guiView.Hide();
			OnInitialize();
		}


		public Collection<MeetingInfoViewModel> MeetingInfoCollection
		{
			get => _meetingInfoCollection;
			set
			{
				_meetingInfoCollection = value;
				base.InvokePropertyValueChanged(nameof(MeetingInfoCollection), value);
			}
		}

		public bool SetActiveCalendarPopup
		{
			get => _setActiveCalendarPopup;
			set => SetProperty(ref _setActiveCalendarPopup, value);
		}

		public string SelectedDateText
		{
			get => _selectedDateText;
			set
			{
				_selectedDateText = value;
				InvokePropertyValueChanged(nameof(SelectedDateText), value);
			}
		}

		public string SelectedDateDayOfWeek
		{
			get => _selectedDateDayOfWeek;
			set
			{
				_selectedDateDayOfWeek = value;
				InvokePropertyValueChanged(nameof(SelectedDateDayOfWeek), value);
			}
		}

		public MeetingCalendarViewModel MeetingCalendarViewModel
		{
			get => _meetingCalendarViewModel;
			set => SetProperty(ref _meetingCalendarViewModel, value);
		}

		public float ScrollRectYPos
		{
			get => _scrollRectYPos;
			set => SetProperty(ref _scrollRectYPos, value);
		}

		public bool HasMeetingInfo
		{
			get => _hasMeetingInfo;
			set => SetProperty(ref _hasMeetingInfo, value);
		}


		public bool SetActiveDetailPage
		{
			get => _setActiveDetailPage;
			set => SetProperty(ref _setActiveDetailPage, value);
		}

		public bool SetActive
		{
			get => _setActive;
			set
			{
				ViewModelManager.Instance.Get<InteractionUIListViewModel>()?.Show(!value);

				SetProperty(ref _setActive, value);
			}
		}

		private void InitializeMeetingCalendar()
		{
			var nowDateTime = MetaverseWatch.NowDateTime;

			_meetingCalendar = MeetingReservationProvider.MeetingCalendar;

			_meetingCalendar.UpdateCalendar(nowDateTime.Year, nowDateTime.Month);

			_meetingCalendar.SetSelectedDay(nowDateTime.Year, nowDateTime.Month, nowDateTime.Day);

			SetSelectedDayInfo(_meetingCalendar.LastSelectedDayInfo);

			SetActiveCalendarPopup = false;

			// UIManager.Instance.ShowWaitingResponsePopup();

			// Network.Communication.PacketReceiver.Instance.MeetingMyListResponse += OnResponseMyMeetingListWhenInitialized;

			RequestMonthMyMeetingList(OnResponseMyMeetingListWhenInitialized);
		}


		private void OnNotifyInitializeCompleted()
		{
			OnInitializeCompleted?.Invoke();
			OnInitializeCompleted = null;
		}

#region Open/Close
		private void OpenMeetingReservationPopup()
		{
			SetActive = true;
		}

		private void CloseMeetingReservationPopup()
		{
			SetActive = false;

			CloseCalendarComplete();
		}

		private void OnCommand_ConnectingAppCalendarActive(bool active)
		{
			if (active)
			{
				var nowDateTime = _selectedDayInfo.ToDateTime();

				_meetingCalendar = new MeetingCalendar();
				_meetingCalendar.UpdateCalendar(nowDateTime.Year, nowDateTime.Month);
				_meetingCalendar.SetSelectedDay(nowDateTime.Year, nowDateTime.Month, nowDateTime.Day);

				OpenCalendar();
			}
			else
			{
				CloseCalendar();
			}
		}

		private void OnCommand_ChangeCalendarOneDayButtonClicked(bool isNext)
		{
			var newDateTime = isNext ? _selectedDayInfo.ToDateTime().AddDays(1) : _selectedDayInfo.ToDateTime().AddDays(-1);
			_selectedDayInfo = new MeetingCalendar.DayInfo(newDateTime.Year, newDateTime.Month, newDateTime.Day);
			SelectedDateText = DateTimeExtension.GetDateTimeOnlyDate(_selectedDayInfo.ToDateTime());
			SelectedDateDayOfWeek = DateTimeExtension.GetDateTimeOnlyDayOfWeek(_selectedDayInfo.ToDateTime());

			_meetingCalendar.SetSelectedDay(_selectedDayInfo);

			OnRequestMeetingDayChanged(_selectedDayInfo);
			if (MeetingCalendarViewModel != null)
			{
				MeetingCalendarViewModel.ChangeCalendarDay(newDateTime);
			}
		}

		private void CompleteReservation()
		{
			_meetingCalendar.SetSelectedDay(_selectedDayInfo);

			OnRequestMeetingDayChanged(_selectedDayInfo);
			if (MeetingCalendarViewModel != null)
			{
				MeetingCalendarViewModel.ChangeCalendarDay(_selectedDayInfo.ToDateTime());
			}
		}

		private void OpenCalendar()
		{
			MeetingCalendarViewModel = new MeetingCalendarViewModel(MeetingCalendarViewModel.eLocationType.INFO_LIST, _meetingCalendar);

			MeetingCalendarViewModel.OnDaySelectedEvent     += OnMeetingCalendarViewModelEventDayClickedByHomeCalendar;
			MeetingCalendarViewModel.OnCalendarChangedEvent += OnMeetingCalendarViewModelEventCalendarChanged;

			SetActiveCalendarPopup = true;

			// UIManager.Instance.ShowWaitingResponsePopup();

			// Network.Communication.PacketReceiver.Instance.MeetingMyListResponse += OnResponseMyMeetingListWhenInitialized;

			RequestMonthMyMeetingList(OnResponseMyMeetingListWhenInitialized);
		}

		private void CloseCalendar()
		{
			if (MeetingCalendarViewModel == null) return;
			MeetingCalendarViewModel.OnDaySelectedEvent     -= OnMeetingCalendarViewModelEventDayClickedByHomeCalendar;
			MeetingCalendarViewModel.OnCalendarChangedEvent -= OnMeetingCalendarViewModelEventCalendarChanged;
			MeetingCalendarViewModel                        =  null;

			SetActiveCalendarPopup = false;
		}

		private void CloseCalendarComplete()
		{
			CloseCalendar();

			ClearMeetingInfoList();
		}

		private void OpenDetailPage()
		{
			if (_selectedMeetingInfo == null)
				return;

			SetActiveDetailPage  = true;
			OpenParticipantsList = false;
			OpenWaitUserList     = false;
		}

		private void CloseDetailPage()
		{
			_selectedMeetingInfo = null;
			if (_selectedMeetingInfoViewModel != null)
			{
				_selectedMeetingInfoViewModel.OnMoveToMeetingRoomButtonVisibleStateChanged -= OnMoveToMeetingRoomButtonVisibleStateChanged;
				_selectedMeetingInfoViewModel = null;
			}

			_meetingParticipantInfoCollection.Reset();
			_meetingEmployeeProfileCollection.Reset();

			OpenParticipantsList = false;
			OpenWaitUserList     = false;
			SetActiveDetailPage  = false;
		}

		private void SetSelectedDayInfo(MeetingCalendar.DayInfo dayInfo)
		{
			_selectedDayInfo = dayInfo;
			SelectedDateText = DateTimeExtension.GetDateTimeOnlyDate(_selectedDayInfo.ToDateTime());
			SelectedDateDayOfWeek = DateTimeExtension.GetDateTimeOnlyDayOfWeek(_selectedDayInfo.ToDateTime());
			_meetingCalendar.SetSelectedDay(_selectedDayInfo);
		}

		private void OnCommand_CloseMeetingReservation()
		{
			SetActive = false;

			CloseCalendarComplete();
		}

		private void OnCommand_CloseCalendar() => CloseCalendarComplete();

		private void OnCommand_OpenReservationPopup()        => MeetingReservationProvider.OpenMeetingReservationPopup(null, UpdateConnectingListWhenInvite, OnResponseReservation,
		                                                                                                               OnResponseMeetingReservationChange);
		private async void OnCommand_OpenReservationInquiryPopup()
		{
			// 조직도 정보 Refresh
			await DataManager.TryOrganizationRefreshAsync(eOrganizationRefreshType.CONNECTING_APP, DataManager.Instance.GroupID);
			MeetingReservationProvider.OpenMeetingReservationInquiryPopup(OnClickDetailPage, UpdateConnectingListWhenInvite);
		}

		private void OnClickDetailPage(MeetingInfoType meetingInfo)
		{
			_selectedMeetingInfo = meetingInfo;
			var meetingViewModel = new MeetingInfoViewModel(meetingInfo, CloseMeetingReservationPopup, OnMeetingInfoDetailButtonClickedByInquiryMyConnecting);

			if (meetingInfo != null)
				meetingViewModel.RegisterUpdateEvent();
			OnMeetingInfoDetailButtonClickedByInquiryMyConnecting(meetingViewModel, meetingInfo);
		}

		private void UpdateConnectingListWhenInvite()
		{
			RequestMyMeetListRefresh(_meetingCalendar.LastSelectedDayInfo.ToDateTime(), _meetingCalendar.LastSelectedDayInfo.ToDateTime());
		}

		private void OnCommand_CloseDetailPage()
		{
			RequestMyMeetListRefresh(_meetingCalendar.LastSelectedDayInfo.ToDateTime(), _meetingCalendar.LastSelectedDayInfo.ToDateTime());
		}

		//private void OnCommand_OpenParticipantManagementPopup() => MeetingReservationProvider.OpenParticipantManagementPopup(_selectedMeetingInfo);

		private void OnCommand_MeetingCancelButtonClicked()
		{
			if (!MeetingReservationHelper.CanChangeOption(_selectedMeetingInfo))
			{
				return;
			}

			UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString("UI_Common_Popup_Title_Text"), Localization.Instance.GetString("UI_ConnectingApp_Detail_CancelReservationPopup_Text"),
			                                  (guiView) => { RequestMeetingCancel(_selectedMeetingInfo.MeetingId); },
			                                  yes: Localization.Instance.GetString("UI_Common_Btn_Yes"), no: Localization.Instance.GetString("UI_Common_Btn_No"));
		}

		private void OnCommand_OpenReservationChangePopup()
		{
			if (!MeetingReservationHelper.CanChangeOption(_selectedMeetingInfo))
			{
				return;
			}

			MeetingReservationProvider.OpenMeetingReservationPopup(_selectedMeetingInfo, UpdateConnectingListWhenInvite, OnResponseReservation, OnResponseMeetingReservationChange);
		}
#endregion Open/Close

		private void UpdateMeetingInfo(MeetingInfoType meetingInfo)
		{
			var meetingViewModel = new MeetingInfoViewModel(meetingInfo, CloseMeetingReservationPopup, OnMeetingInfoDetailButtonClicked);

			if (meetingInfo != null)
				meetingViewModel.RegisterUpdateEvent();

			_meetingInfoCollection.AddItem(meetingViewModel);
		}


		private void ClearMeetingInfoList()
		{
			foreach (var meetingInfoViewModel in _meetingInfoCollection.Value)
			{
				meetingInfoViewModel.UnregisterUpdateEvent();
			}

			_meetingInfoCollection.Reset();
		}


		private void RemoveEmptyMeetingInfo()
		{
			foreach (var meetingInfoViewModel in _meetingInfoCollection.Value)
			{
				if (meetingInfoViewModel.IsVisibleParticipatingText)
				{
					_meetingInfoCollection.RemoveItem(meetingInfoViewModel);
					return;
				}
			}
		}

		public MeetingInfoViewModel GetMeetingInfoViewModel(long meetingId)
		{
			foreach (var meetingInfoViewModel in _meetingInfoCollection.Value)
			{
				if (meetingInfoViewModel.MeetingId == meetingId)
				{
					return meetingInfoViewModel;
				}
			}

			return null;
		}

		private void CloseConnectingApp()
		{
			GuiView.Hide();
		}
	}
}
