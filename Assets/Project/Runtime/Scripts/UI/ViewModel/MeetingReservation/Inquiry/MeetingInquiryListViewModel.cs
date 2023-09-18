/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingSearchViewModel.cs
* Developer:	ksw
* Date:			2023-03-07 11:52
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Globalization;
using System.Linq;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using MeetingUserType = Com2Verse.WebApi.Service.Components.MeetingMemberEntity;
using MemberType = Com2Verse.WebApi.Service.Components.MemberType;
using AttendanceCode = Com2Verse.WebApi.Service.Components.AttendanceCode;
using AuthorityCode = Com2Verse.WebApi.Service.Components.AuthorityCode;

namespace Com2Verse
{
	[ViewModelGroup("MeetingInquiry")]
	public sealed partial class MeetingInquiryListViewModel : ViewModelBase
	{
		private StackRegisterer _inquiryGUIViewRegister;

		public StackRegisterer InquiryGUIViewRegister
		{
			get => _inquiryGUIViewRegister;
			set
			{
				_inquiryGUIViewRegister             =  value;
				_inquiryGUIViewRegister.WantsToQuit += OnClickCloseButton;
			}
		}
		// ReSharper disable InconsistentNaming
		public CommandHandler Command_PrevPage          { get; }
		public CommandHandler Command_NextPage          { get; }
		public CommandHandler Command_Search            { get; }
		public CommandHandler Command_SearchSpaceCode   { get; }
		public CommandHandler Command_UpcomingOrOngoing { get; }
		public CommandHandler Command_Close             { get; }
		public CommandHandler Command_StartDateCalender { get; }
		public CommandHandler Command_EndDateCalender   { get; }

		public CommandHandler Command_OffCalender { get; }
		// ReSharper restore InconsistentNaming


		private Collection<MeetingInquiryViewModel> _meetingInquiryCollection = new(); // 데이터 가공 전 서버에서 가져온 컬렉션
		private Collection<MeetingInquiryViewModel> _showingInquiryCollection = new(); // 데이터 가공 후 보여지는 컬렉션

		private MeetingCalendarViewModel _meetingCalendarViewModel;
		private MeetingCalendar          _meetingCalendar;

		private bool   _setActive;
		private bool   _isResultNull;             // 검색 값이 null 일 때 true
		private bool   _isNotSearch;              // 검색을 하기 전일 때
		private bool   _isPageTextView;           // 페이지 숫자/버튼 보여주는 값
		private int    _currentPage;              // 현재 페이지
		private string _currentPageString;        // 현재 페이지 문자열
		private int    _maxPage;                  // 마지막 페이지
		private string _maxPageString;            // 마지막 페이지 문자열
		// TODO : 기획 데이터 변경
		private int    _numberOfModelOnPage = 10; // 페이지 당 보여주는 회의 갯수
		private bool   _searchTypeIsDetail;       // 참여 요청/취소 시 재검색을 위한 변수

		private string                                           _meetingCode;
		private string                                           _startDate;
		private string                                           _endDate;
		private bool                                             _isUpcomingOrOngoing;
		private string                                           _meetingName;          // 커넥팅 제목
		private string                                           _meetingOrganizer;     // 주최자(생성자)
		private string                                           _meetingParticipating; // 사용자
		private bool                                             _isActiveCalendar;
		private bool                                             _isStartDateCalender;
		private MeetingCalendar.DayInfo                          _startDayInfo;
		private MeetingCalendar.DayInfo                          _endDayInfo;
		private Action<MeetingInfoType>                          _onClickMoveDetailPage;
		private Action<long, MeetingInquiryViewModel.eStateType> _onClickInviteRequest;
		private Action                                           _inviteCallback;
		// TODO : 기획 데이터 변경
		private int                                              _maxSearchDuration = 30;

		private GUIView _guiView;

#region Properties
		public bool SetActive
		{
			get => _setActive;
			set
			{
				_setActive = value;
				if (!value)
					OnPopupClosed();
				InvokePropertyValueChanged(nameof(SetActive), value);
			}
		}

		public Collection<MeetingInquiryViewModel> MeetingInquiryCollection
		{
			get => _meetingInquiryCollection;
			set
			{
				_meetingInquiryCollection = value;
				InvokePropertyValueChanged(nameof(MeetingInquiryCollection), value);
			}
		}

		public Collection<MeetingInquiryViewModel> ShowingInquiryCollection
		{
			get => _showingInquiryCollection;
			set
			{
				_showingInquiryCollection = value;
				InvokePropertyValueChanged(nameof(ShowingInquiryCollection), value);
			}
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

		public bool IsResultNull
		{
			get => _isResultNull;
			set
			{
				_isResultNull = value;
				if (value)
					IsPageTextView = false;
				else
				{
					IsPageTextView = true;
				}

				InvokePropertyValueChanged(nameof(IsResultNull), value);
			}
		}

		public bool IsNotSearch
		{
			get => _isNotSearch;
			set
			{
				_isNotSearch = value;
				if (value)
					IsPageTextView = false;
				InvokePropertyValueChanged(nameof(IsNotSearch), value);
			}
		}

		public bool IsPageTextView
		{
			get => _isPageTextView;
			set
			{
				_isPageTextView = value;
				InvokePropertyValueChanged(nameof(IsPageTextView), value);
			}
		}

		public string CurrentPage
		{
			get => _currentPageString;
			set { SetProperty(ref _currentPageString, value); }
		}

		public string MaxPage
		{
			get => _maxPageString;
			set { SetProperty(ref _maxPageString, value); }
		}

		public string MeetingCode
		{
			get => _meetingCode;
			set
			{
				_meetingCode = value;
				InvokePropertyValueChanged(nameof(MeetingCode), value);
			}
		}

		public string StartDate
		{
			get => _startDate;
			set
			{
				_startDate = value;
				InvokePropertyValueChanged(nameof(StartDate), value);
			}
		}

		public string EndDate
		{
			get => _endDate;
			set
			{
				_endDate = value;
				InvokePropertyValueChanged(nameof(EndDate), value);
			}
		}

		public bool UpcomingOrOngoing
		{
			get => _isUpcomingOrOngoing;
			set
			{
				_isUpcomingOrOngoing = value;
				InvokePropertyValueChanged(nameof(UpcomingOrOngoing), value);
			}
		}

		public string MeetingName
		{
			get => _meetingName;
			set
			{
				_meetingName = value;
				InvokePropertyValueChanged(nameof(MeetingName), value);
			}
		}

		public string MeetingOrganizer
		{
			get => _meetingOrganizer;
			set
			{
				_meetingOrganizer = value;
				InvokePropertyValueChanged(nameof(MeetingOrganizer), value);
			}
		}

		public string MeetingParticipating
		{
			get => _meetingParticipating;
			set
			{
				_meetingParticipating = value;
				InvokePropertyValueChanged(nameof(MeetingParticipating), value);
			}
		}

		public bool IsActiveCalendar
		{
			get => _isActiveCalendar;
			set
			{
				_isActiveCalendar = value;
				InvokePropertyValueChanged(nameof(IsActiveCalendar), value);
			}
		}
#endregion

		public MeetingInquiryListViewModel()
		{
			Command_PrevPage          = new CommandHandler(OnClickPrevPage);
			Command_NextPage          = new CommandHandler(OnClickNextPage);
			Command_Search            = new CommandHandler(OnClickSearch);
			Command_SearchSpaceCode   = new CommandHandler(OnClickSearchMeetingCode);
			Command_UpcomingOrOngoing = new CommandHandler(() => UpcomingOrOngoing = !UpcomingOrOngoing);
			Command_Close             = new CommandHandler(OnClickCloseButton);
			Command_StartDateCalender = new CommandHandler(OnClickStartDateCalender);
			Command_EndDateCalender   = new CommandHandler(OnClickEndDateCalender);
			Command_OffCalender       = new CommandHandler(OnClickOffCalender);

			_onClickInviteRequest = OnClickMeetingAttendanceRequest;
			InitInquiry();
		}

		public override void OnInitialize()
		{
			base.OnInitialize();
			InitInquiry();
		}

		public void Set(Action<MeetingInfoType> onClickAction, Action inviteCallback, GUIView guiView)
		{
			_onClickMoveDetailPage = onClickAction;
			_inviteCallback        = inviteCallback;
			_guiView               = guiView;
			InitInquiry();
		}

		private void InitInquiry()
		{
			IsNotSearch    = true;
			IsResultNull   = false;
			IsPageTextView = false;
			CurrentPage    = "1";
			MaxPage        = "1";

			MeetingCode = string.Empty;
			StartDate = DateTimeExtension.GetDateTimeOnlyDate(MetaverseWatch.NowDateTime);
			EndDate   = DateTimeExtension.GetDateTimeOnlyDate(MetaverseWatch.NowDateTime);

			_meetingCalendar = new MeetingCalendar();
			_meetingCalendar.AddDayInfo(MetaverseWatch.NowDateTime.Year, MetaverseWatch.NowDateTime.Month, MetaverseWatch.NowDateTime.Day);
			var dayInfo = _meetingCalendar.GetDayInfo(MetaverseWatch.NowDateTime.ToProtoDateTime());
			_startDayInfo        = dayInfo;
			_endDayInfo          = dayInfo;
			UpcomingOrOngoing    = false;
			MeetingName          = string.Empty;
			MeetingOrganizer     = string.Empty;
			MeetingParticipating = string.Empty;
			MeetingInquiryCollection.Reset();
			ShowingInquiryCollection.Reset();
			IsActiveCalendar       = false;
			IsPageTextView         = false;
			SetActive              = true;
			_isStartDateCalender   = false;
			_organizerMemberId     = -1;
			_participatingMemberId = -1;
			_searchTypeIsDetail    = true;
			_meetingOrganizerSearchResults.Reset();
			_meetingParticipatingSearchResults.Reset();
			SetActiveOrganizerSearchField     = false;
			SetActiveParticipatingSearchField = false;
		}

		private void OnClickMeetingAttendanceRequest(long meetingId, MeetingInquiryViewModel.eStateType stateType)
		{
			if (stateType == MeetingInquiryViewModel.eStateType.WAIT_JOIN_REQUEST)
			{
				UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString("UI_Common_Notice_Popup_Title"), Localization.Instance.GetString("UI_ConnectingApp_CardPopup_CancelJoin_Text"), view =>
					                                  RequestMeetingAttendanceCancel(meetingId),
				                                  yes: Localization.Instance.GetString("UI_Common_Btn_Yes"), no: Localization.Instance.GetString("UI_Common_Btn_No"));
			}
			else if (stateType == MeetingInquiryViewModel.eStateType.WAIT_JOIN_RECEIVE)
			{
				UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString("UI_Common_Notice_Popup_Title"), Localization.Instance.GetString("UI_ConnectingApp_Card_DeclineInvitationRequest_Popup_Text"),
				                                  view =>
					                                  RequestMeetingInviteReject(meetingId),
				                                  yes: Localization.Instance.GetString("UI_Common_Btn_Yes"), no: Localization.Instance.GetString("UI_Common_Btn_No"));
			}
			else
			{
				//if (User.Instance.CurrentUserData is not OfficeUserData userData) return;
				//var memberModel = DataManager.Instance.GetMember(userData.ID);
				//var meetingUserInfo = new MeetingUserType
				//{
				//	AccountId      = memberModel.Member.AccountId,
				//	MemberType     = MemberType.CompanyEmployee,
				//	AttendanceCode = AttendanceCode.JoinRequest,
				//	AuthorityCode  = AuthorityCode.Presenter,
				//};
				UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString("UI_Common_Notice_Popup_Title"),
				                                  Localization.Instance.GetString("UI_ConnectingApp_Search_Popup_Join_Text"),
				                                  view =>
					                                  RequestMeetingAttendance(meetingId),
				                                  yes: Localization.Instance.GetString("UI_Common_Btn_Yes"), no: Localization.Instance.GetString("UI_Common_Btn_No"));
			}
		}

		private void OnClickPrevPage()
		{
			if (_currentPage == 1)
				return;
			_currentPage -= 1;
			RefreshInquiryPage();
		}

		private void OnClickNextPage()
		{
			if (_currentPage == _maxPage)
				return;
			_currentPage += 1;
			RefreshInquiryPage();
		}

		private void RefreshInquiryPage()
		{
			ShowingInquiryCollection.Reset();
			var copy = MeetingInquiryCollection.Value.Select(obj => obj.Clone()).ToList();

			if (_currentPage == _maxPage)
			{
				for (var i = (_currentPage - 1) * _numberOfModelOnPage; i < _currentPage * _numberOfModelOnPage; i++)
				{
					if (copy.Count <= i)
						break;
					ShowingInquiryCollection.AddItem(copy[i]);
				}
			}
			else
			{
				for (var i = (_currentPage - 1) * _numberOfModelOnPage; i < _currentPage * _numberOfModelOnPage; i++)
				{
					ShowingInquiryCollection.AddItem(copy[i]);
				}
			}

			CurrentPage = _currentPage.ToString();
			MaxPage     = _maxPage.ToString();
		}

		/// <summary>
		/// 조건으로 검색
		/// </summary>
		private void OnClickSearch()
		{
			if (string.IsNullOrWhiteSpace(MeetingOrganizer))
				_organizerMemberId = -1;
			if (string.IsNullOrWhiteSpace(MeetingParticipating))
				_participatingMemberId = -1;
			if (string.IsNullOrWhiteSpace(MeetingName))
				_meetingName = "";
			if (string.IsNullOrWhiteSpace(_meetingName) && _organizerMemberId < 0 && _participatingMemberId < 0)
			{
				return;
			}

			// 날짜 30일 조건
			var duration = _endDayInfo.ToDateTime() - _startDayInfo.ToDateTime();
			if (duration.TotalDays < 0)
			{
				UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_SearchCondition_IncorrectSearchPeriod_Toast"));
				return;
			}
			if (duration.TotalDays > _maxSearchDuration)
			{
				UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_SearchCondition_SearchMaximumPeriod_Toast"));
				return;
			}

			RequestInquirySearchByDetail(_startDayInfo.ToDateTime(), _endDayInfo.ToDateTime(), _meetingName, _organizerMemberId, _participatingMemberId);
			_searchTypeIsDetail = true;
		}

		/// <summary>
		/// 회의 코드로 검색
		/// </summary>
		private void OnClickSearchMeetingCode()
		{
			if (string.IsNullOrWhiteSpace(MeetingCode))
				return;

			RequestSearchByMeetingCode(MeetingCode);
			_searchTypeIsDetail = false;
		}

		private void OnClickCloseButton()
		{
			MeetingName = "";
			_inviteCallback?.Invoke();
			SetActive = false;
		}

		private void CloseInquiryPopup(bool isInvite = true)
		{
			MeetingName = "";
			if (isInvite)
				_inviteCallback?.Invoke();
			SetActive   = false;
		}

		private void OnPopupClosed()
		{
			_guiView.Hide();
		}

		private void OnClickStartDateCalender()
		{
			_isStartDateCalender = true;
			OpenCalender();
			IsActiveCalendar = true;
		}

		private void OnClickEndDateCalender()
		{
			_isStartDateCalender = false;
			OpenCalender();
			IsActiveCalendar = true;
		}

		private void OnClickOffCalender()
		{
			IsActiveCalendar     = false;
			_isStartDateCalender = false;
			CloseCalender();
		}

		private void OpenCalender()
		{
			var nowDateTime = _isStartDateCalender ? _startDayInfo.ToDateTime() : _endDayInfo.ToDateTime();
			_meetingCalendar         = new MeetingCalendar();
			MeetingCalendarViewModel = new MeetingCalendarViewModel(MeetingCalendarViewModel.eLocationType.INFO_LIST, _meetingCalendar);
			_meetingCalendar.UpdateCalendar(nowDateTime.Year, nowDateTime.Month);
			_meetingCalendar.SetSelectedDay(nowDateTime.Year, nowDateTime.Month, nowDateTime.Day);
			MeetingCalendarViewModel.OnDaySelectedEvent += OnInquiryDaySelected;
		}

		private void CloseCalender()
		{
			MeetingCalendarViewModel.OnDaySelectedEvent -= OnInquiryDaySelected;
		}

		private void OnInquiryDaySelected(MeetingCalendar.DayInfo dayInfo)
		{
			if (MeetingCalendarViewModel.IsDateChosenByForce)
				return;

			if (_isStartDateCalender)
			{
				_startDayInfo = dayInfo;
				StartDate     = DateTimeExtension.GetDateTimeOnlyDate(_startDayInfo.ToDateTime());
			}
			else
			{
				_endDayInfo = dayInfo;
				EndDate     = DateTimeExtension.GetDateTimeOnlyDate(_endDayInfo.ToDateTime());
			}

			OnClickOffCalender();
		}
	}
}
