/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingInfoListViewModel_Detail.cs
* Developer:	tlghks1009
* Date:			2022-09-12 03:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.AssetSystem;
using Com2Verse.Data;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using MeetingUserType = Com2Verse.WebApi.Service.Components.MeetingMemberEntity;
using MemberType = Com2Verse.WebApi.Service.Components.MemberType;
using AttendanceCode = Com2Verse.WebApi.Service.Components.AttendanceCode;
using AuthorityCode = Com2Verse.WebApi.Service.Components.AuthorityCode;
using MeetingStatus = Com2Verse.WebApi.Service.Components.MeetingStatus;
using MemberIdType = System.Int64;

namespace Com2Verse.UI
{
	public partial class MeetingInfoListViewModel
	{
		// ReSharper disable InconsistentNaming
		private static readonly string UI_MeetingAppReserve_Desc_VoiceRec = "UI_MeetingAppReserve_Desc_VoiceRec";
		private static readonly string UI_MeetingAppReserve_Desc_Dialog = "UI_MeetingAppReserve_Desc_Dialog";
		// ReSharper restore InconsistentNaming

		private readonly int _showingUserListCapacity = 7;	// 카드 리스트에 참여자 프로필 아이콘 보여주는 최대 숫자 8명 부터는 +@ 숫자로 표시
		private UIInfo _uiInfo;

		private string _detailPageMeetingStatus;
		private Color  _detailPageMeetingTagColor;
		private bool   _openParticipantsList;
		private bool   _openWaitUserList;
		private string _detailPageOrganizerCountText;
		private string _detailPageParticipantCountText;
		private string _detailPageWaitListCountText;
		private string _accessText;
		private string _meetingCode;
		private bool   _isExpired;
		private bool   _isOrganizer;
		private bool   _isWaitListView;
		private bool   _statusTagActive;
		private bool   _isMeetingMinutesActive;

		private MeetingInfoType _selectedMeetingInfo;
		private MeetingInfoViewModel _selectedMeetingInfoViewModel;
		private MeetingInfoType _attendanceCancelMeetingInfo;

		private Collection<MeetingParticipantInfoViewModel>         _meetingParticipantInfoCollection          = new();
		private Collection<MeetingEmployeeProfileViewModel>         _meetingEmployeeProfileCollectionOrganizer = new();
		private Collection<MeetingEmployeeProfileViewModel>         _meetingEmployeeProfileCollection          = new();
		private Collection<MeetingEmployeeProfileWaitListViewModel> _meetingEmployeeProfileCollectionWaitList  = new();

		private SpaceTemplate _spaceTemplateData;

		// ReSharper disable InconsistentNaming
		public CommandHandler Command_OpenReservationChangePopup { get; }
		public CommandHandler Command_OpenParticipantsList       { get; }
		public CommandHandler Command_CloseParticipantsList      { get; }
		public CommandHandler Command_OpenWaitUserList           { get; }
		public CommandHandler Command_CloseWaitUserList          { get; }

		public CommandHandler Command_MoveToMeetingRoomButtonClick { get; }
		public CommandHandler Command_OpenMeetingMinutesPopup      { get; }
		// ReSharper restore InconsistentNaming

#region MeetingInfoDetailProperties
		public string UseVoiceRecordingAndStt
		{
			get => base.Model.UseVoiceRecordingAndStt;
			set => SetProperty(ref base.Model.UseVoiceRecordingAndStt, value);
		}

		public string MeetingOrganizerName
		{
			get => base.Model.MeetingOrganizerName;
			set => SetProperty(ref base.Model.MeetingOrganizerName, value);
		}

		public string MeetingType
		{
			get => base.Model.MeetingType;
			set => SetProperty(ref base.Model.MeetingType, value);
		}

		public string MeetingName
		{
			get => base.Model.MeetingID;
			set => SetProperty(ref base.Model.MeetingID, value);
		}

		public string MeetingID
		{
			get => base.Model.MeetingName;
			set => SetProperty(ref base.Model.MeetingName, value);
		}

		public string MeetingDate
		{
			get => base.Model.MeetingDate;
			set => SetProperty(ref base.Model.MeetingDate, value);
		}

		public string MeetingDescription
		{
			get => base.Model.MeetingDescription;
			set => SetProperty(ref base.Model.MeetingDescription, value);
		}

		public string MeetingParticipantText
		{
			get => base.Model.MeetingParticipantText;
			private set => SetProperty(ref base.Model.MeetingParticipantText, value);
		}

		public string MeetingParticipantMembers
		{
			get => base.Model.MeetingParticipantMembers;
			private set => SetProperty(ref base.Model.MeetingParticipantMembers, value);
		}

		public string MeetingParticipantDetailText
		{
			get => base.Model.MeetingParticipantDetailText;
			private set => SetProperty(ref base.Model.MeetingParticipantDetailText, value);
		}

		public string MeetingWaitListText
		{
			get => base.Model.MeetingWaitListText;
			private set => SetProperty(ref base.Model.MeetingWaitListText, value);
		}

		public string MeetingWaitListMembers
		{
			get => base.Model.MeetingWaitListMembers;
			private set => SetProperty(ref base.Model.MeetingWaitListMembers, value);
		}

		public string MeetingCode
		{
			get => _meetingCode;
			private set => SetProperty(ref _meetingCode, value);
		}

		public bool IsVisibleReservationChangeButton
		{
			get => base.Model.IsVisibleReservationChangeButton;
			set => SetProperty(ref base.Model.IsVisibleReservationChangeButton, value);
		}

		public bool IsVisibleMoveToMeetingRoomButton
		{
			get => base.Model.IsVisibleMoveToMeetingRoomButton;
			set => SetProperty(ref base.Model.IsVisibleMoveToMeetingRoomButton, value);
		}


		public bool IsVisibleReservationCancelButton
		{
			get => base.Model.IsVisibleReservationCancelButton;
			set => SetProperty(ref base.Model.IsVisibleReservationCancelButton, value);
		}

		public Texture SpaceTexture
		{
			get => base.Model.SpaceTexture;
			set => SetProperty(ref base.Model.SpaceTexture, value);
		}

		public string SpaceTitle
		{
			get => base.Model.SpaceTitle;
			set => SetProperty(ref base.Model.SpaceTitle, value);
		}

		public string SpaceDescription
		{
			get => base.Model.SpaceDescription;
			set => SetProperty(ref base.Model.SpaceDescription, value);
		}

		public bool OpenEmployeeProfilePopup
		{
			get => false;
			set
			{
				if (_meetingEmployeeProfileCollection.CollectionCount == 0)
					return;

				base.InvokePropertyValueChanged(nameof(OpenEmployeeProfilePopup), value);
			}
		}

		public bool CloseEmployeeProfilePopup
		{
			get => false;
			set => base.InvokePropertyValueChanged(nameof(CloseEmployeeProfilePopup), value);
		}

		public string DetailPageMeetingStatus
		{
			get => _detailPageMeetingStatus;
			set => SetProperty(ref _detailPageMeetingStatus, value);
		}

		public string DetailPageOrganizerCountText
		{
			get => _detailPageOrganizerCountText;
			set => SetProperty(ref _detailPageOrganizerCountText, value);
		}

		public string DetailPageParticipantCountText
		{
			get => _detailPageParticipantCountText;
			set => SetProperty(ref _detailPageParticipantCountText, value);
		}

		public string DetailPageWaitListCountText
		{
			get => _detailPageWaitListCountText;
			set => SetProperty(ref _detailPageWaitListCountText, value);
		}

		public string AccessText
		{
			get => _accessText;
			set => SetProperty(ref _accessText, value);
		}

		public Color DetailPageMeetingTagColor
		{
			get => _detailPageMeetingTagColor;
			set => SetProperty(ref _detailPageMeetingTagColor, value);
		}

		public bool OpenParticipantsList
		{
			get => _openParticipantsList;
			set => SetProperty(ref _openParticipantsList, value);
		}

		public bool OpenWaitUserList
		{
			get => _openWaitUserList;
			set => SetProperty(ref _openWaitUserList, value);
		}

		public bool IsExpired
		{
			get => _isExpired;
			set => SetProperty(ref _isExpired, value);
		}

		public bool IsMeetingMinutesActive
		{
			get => _isMeetingMinutesActive;
			set => SetProperty(ref _isMeetingMinutesActive, value);
		}


		public bool IsOrganizer
		{
			get => _isOrganizer;
			set => SetProperty(ref _isOrganizer, value);
		}

		public bool IsWaitListView
		{
			get => _isWaitListView;
			set => SetProperty(ref _isWaitListView, value);
		}

		public bool StatusTagActive
		{
			get => _statusTagActive;
			set => SetProperty(ref _statusTagActive, value);
		}

		[UsedImplicitly]
		public UIInfo UIInfoViewModel
		{
			get
			{
				if (_uiInfo == null)
				{
					InitializeUIInfo();
				}

				return _uiInfo;
			}
		}


		public Collection<MeetingParticipantInfoViewModel> MeetingParticipantInfoCollection
		{
			get => _meetingParticipantInfoCollection;
			set => SetProperty(ref _meetingParticipantInfoCollection, value);
		}

		public Collection<MeetingEmployeeProfileViewModel> MeetingEmployeeProfileCollection
		{
			get => _meetingEmployeeProfileCollection;
			set => SetProperty(ref _meetingEmployeeProfileCollection, value);
		}

		public Collection<MeetingEmployeeProfileViewModel> MeetingEmployeeProfileCollectionOrganizer
		{
			get => _meetingEmployeeProfileCollectionOrganizer;
			set => SetProperty(ref _meetingEmployeeProfileCollectionOrganizer, value);
		}

		public Collection<MeetingEmployeeProfileWaitListViewModel> MeetingEmployeeProfileCollectionWaitList
		{
			get => _meetingEmployeeProfileCollectionWaitList;
			set => SetProperty(ref _meetingEmployeeProfileCollectionWaitList, value);
		}
#endregion MeetingInfoDetailProperties

		private void OnCommand_MoveToMeetingRoomButtonClicked() => _selectedMeetingInfoViewModel.OnCommand_MoveToRoomButtonClicked();

		private void OnCommand_ParticipatingPopupOpenButtonClicked() => OpenEmployeeProfilePopup = true;

		private void OnCommand_CopyMeetingCode()
		{
			GUIUtility.systemCopyBuffer = MeetingCode;
			UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_Common_UserList_Invitation_SpaceCodeCopied_Toast"));
		}

		private void OnCommand_CopyInviteInfoClicked()
		{
			GUIUtility.systemCopyBuffer = Localization.Instance.GetString("UI_MeetingRoom_Common_ConnectingInfo_Invitation_Content_Text",
			                                                              MeetingOrganizerName, MeetingDate, MeetingName, MeetingCode);
			UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_Common_UserList_Invitation_InvitationCopied_Toast"));
		}

		private void OnCommand_OpenParticipantsList()
		{
			OpenParticipantsList = true;
		}

		private void OnCommand_CloseParticipantsList()
		{
			OpenParticipantsList = false;
		}

		private void OnCommand_OpenWaitUserList()
		{
			OpenWaitUserList = true;
		}

		private void OnCommand_CloseWaitUserList()
		{
			OpenWaitUserList = false;
		}

		private void OnCommand_OpenMeetingMinutesPopup()
		{
			// TODO : 해당 커넥팅에 회의록이 없다면 return. 현재 어떤식으로 넘겨줄 지 몰라 처리 x
			UIManager.Instance.CreatePopup("UI_ConnectingApp_MeetingMinutes_Popup", (guiView) =>
			{
				guiView.Show();

				var meetingMinutesViewModel = guiView.ViewModelContainer.GetViewModel<MeetingMinutesDetailPopupViewModel>();
				meetingMinutesViewModel.Initialize(guiView, _selectedMeetingInfo);
			}).Forget();
		}

		private void InitializeUIInfo()
		{
			var titleString   = Localization.Instance.GetString("UI_ConnectingApp_Detail_Setting_Privacy_Tooltip_Title");
			var messageString = Localization.Instance.GetString("UI_ConnectingApp_Detail_Setting_Privacy_Tooltip_Text");

			_uiInfo = new UIInfo(true);
			_uiInfo.Set(UIInfo.eInfoType.INFO_TYPE_ACCESS_INFO, UIInfo.eInfoLayout.INFO_LAYOUT_UP, titleString, messageString);
		}

		private void UpdateDetailPage()
		{
			if (_selectedMeetingInfo == null)
			{
				return;
			}

			foreach (var meetingInfoViewModel in _meetingInfoCollection.Value)
			{
				if (meetingInfoViewModel.MeetingInfo == null)
					continue;

				if (meetingInfoViewModel.MeetingId == _selectedMeetingInfo.MeetingId)
				{
					meetingInfoViewModel.OnCommand_OnDetailClicked();
					break;
				}
			}
		}

		private void OnMeetingInfoDetailButtonClicked(MeetingInfoViewModel meetingInfoViewModel, MeetingInfoType meetingInfo)
		{
			if (!meetingInfoViewModel.IsPossibleViewDetailPage())
				return;
			if (meetingInfoViewModel.IsJoinRequestWaiting())
			{
				_attendanceCancelMeetingInfo = meetingInfo;
				UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString("UI_Common_Popup_Title_Text"), Localization.Instance.GetString("UI_ConnectingApp_CardPopup_CancelJoin_Text"), RequestMeetingAttendanceCancel,
				                                  yes: Localization.Instance.GetString("UI_Common_Btn_Yes"), no: Localization.Instance.GetString("UI_Common_Btn_No"));
				return;
			}

			if (meetingInfoViewModel.IsJoinReceiveWaiting())
			{
				// TODO : Reject를 Accept로
				UIManager.Instance.ShowPopupYesNoCancel(Localization.Instance.GetString("UI_Common_Notice_Popup_Title"),
				                                        Localization.Instance.GetString("UI_ConnectingApp_Card_UserAccept_Popup_Text"),
				                                        ok => RequestConnectingInviteReject(meetingInfo.MeetingId),
				                                        no => RequestConnectingInviteReject(meetingInfo.MeetingId),
				                                        yes: Localization.Instance.GetString("UI_Common_Accept_Btn"), no: Localization.Instance.GetString("UI_Common_Reject_Btn"));
				return;
			}

			UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString("UI_Common_Popup_Title_Text"), Localization.Instance.GetString("UI_ConnectingApp_CardPopup_GoDetail_Text"),
			                                  ok =>
			                                  {
				                                  _selectedMeetingInfoViewModel = meetingInfoViewModel;

				                                  _selectedMeetingInfoViewModel.OnMoveToMeetingRoomButtonVisibleStateChanged += OnMoveToMeetingRoomButtonVisibleStateChanged;
				                                  RequestMeetingInfo(meetingInfo.MeetingId);
			                                  },
			                                  yes: Localization.Instance.GetString("UI_Common_Btn_Yes"), no: Localization.Instance.GetString("UI_Common_Btn_No"));
		}

		private void OnMeetingInfoDetailButtonClickedByInquiryMyConnecting(MeetingInfoViewModel meetingInfoViewModel, MeetingInfoType meetingInfo)
		{
			if (!meetingInfoViewModel.IsPossibleViewDetailPage())
				return;

			_selectedMeetingInfoViewModel = meetingInfoViewModel;

			_selectedMeetingInfoViewModel.OnMoveToMeetingRoomButtonVisibleStateChanged += OnMoveToMeetingRoomButtonVisibleStateChanged;
			RequestMeetingInfo(meetingInfo.MeetingId);
		}

		private void OnInitializeDetailPage(MeetingInfoType meetingInfo)
		{
			// UIManager.Instance.HideWaitingResponsePopup();

			_selectedMeetingInfo = meetingInfo;

			var useVoiceRecording       = meetingInfo.VoiceRecordYn == "Y" ? "ON" : "OFF";
			var useStt                  = meetingInfo.ChatNoteYn    == "Y" ? "ON" : "OFF";
			int meetingJoinUserCount    = 0;
			int meetingWaitingUserCount = 0;
			foreach (var meetingUserInfo in meetingInfo.MeetingMembers)
			{
				// 사용자가 게스트일 경우 무시
				if (meetingUserInfo.MemberType == MemberType.OutsideParticipant)
					continue;
				if (meetingUserInfo.AttendanceCode == AttendanceCode.Join)
					meetingJoinUserCount++;
				if (meetingUserInfo.AttendanceCode is AttendanceCode.JoinReceive or AttendanceCode.JoinRequest)
					meetingWaitingUserCount++;
			}

			UseVoiceRecordingAndStt = $"{Localization.Instance.GetString(UI_MeetingAppReserve_Desc_VoiceRec)} {useVoiceRecording} / " +
			                          $"{Localization.Instance.GetString(UI_MeetingAppReserve_Desc_Dialog)} {useStt}";
			MeetingName                      = meetingInfo.MeetingName;
			MeetingID                        = meetingInfo.MeetingId.ToString();
			MeetingDate                      = _selectedMeetingInfoViewModel.MeetingDate;
			MeetingDescription               = meetingInfo.MeetingDescription;
			MeetingParticipantText           = Localization.Instance.GetString("UI_ConnectingApp_Detail_InvitationMemberTitle_Text", meetingJoinUserCount); // 초대 ({0})
			MeetingParticipantDetailText     = Localization.Instance.GetString("UI_MeetingRoom_UserList_Online_Count_Text",          meetingJoinUserCount); // 참여 {0}
			MeetingWaitListText              = Localization.Instance.GetString("UI_ConnectingApp_Detail_WaitMemberTitle_Text",       meetingWaitingUserCount); // 대기 ({0})
			MeetingParticipantMembers        = Localization.Instance.GetString("UI_ConnectingApp_Detail_InvitationMemberCount_Text", meetingJoinUserCount); // {0} 명
			MeetingWaitListMembers           = Localization.Instance.GetString("UI_ConnectingApp_Detail_WaitMemberCount_Text",       meetingWaitingUserCount); // {0} 명
			IsExpired                        = meetingInfo.MeetingStatus is MeetingStatus.MeetingExpired or MeetingStatus.MeetingPassed;
			IsMeetingMinutesActive = meetingInfo.MeetingStatus is MeetingStatus.MeetingPassed;
			IsWaitListView = meetingInfo.MeetingStatus == MeetingStatus.MeetingBeforeStart;
			IsVisibleMoveToMeetingRoomButton = CurrentScene.SpaceCode != eSpaceCode.MEETING && _selectedMeetingInfoViewModel.SetActiveMoveToRoomButton;
			DetailPageMeetingTagColor        = _selectedMeetingInfoViewModel.MeetingTagColor;
			DetailPageMeetingStatus          = _selectedMeetingInfoViewModel.MeetingStatus;
			AccessText = meetingInfo.PublicYn == "Y" ? Localization.Instance.GetString("UI_ConnectingApp_Detail_Privacy_Hint_1") : Localization.Instance.GetString("UI_ConnectingApp_Detail_Privacy_Hint_2");
			MeetingCode = meetingInfo.MeetingCode;

			if (_isExpired)
			{
				StatusTagActive = false;
			}
			else
			{
				StatusTagActive = _selectedMeetingInfoViewModel.MeetingStatusType != MeetingInfoViewModel.eMeetingStatus.WAIT_START;
			}

			C2VAsyncOperationHandle<Texture> handle;

			if (meetingInfo.TemplateId == 0)
				meetingInfo.TemplateId = 30300501000; // Default
			
			handle = C2VAddressables.LoadAssetAsync<Texture>(NetworkUIManager.Instance.SpaceTemplates[meetingInfo.TemplateId].ImgRes);
			handle.OnCompleted += (internalHandle) =>
			{
				SpaceTexture     = internalHandle.Result;
				SpaceTitle       = Localization.Instance.GetString(NetworkUIManager.Instance.SpaceTemplates[meetingInfo.TemplateId].Title);
				SpaceDescription = Localization.Instance.GetString(NetworkUIManager.Instance.SpaceTemplates[meetingInfo.TemplateId].Description);
				handle.Release();
			};

			OpenDetailPage();

			SetupOrganizer();

			InitializeEnteredEmployeeInfoList();

			if (IsMeetingMinutesActive)
			{
				Commander.Instance.RequestRecordInfoAsync(meetingInfo.MeetingId, response =>
				{
					if (response.Value.Data.SoundFiles.Length == 0)
					{
						IsMeetingMinutesActive = false;
					}
				}).Forget();
			}
			
			Canvas.ForceUpdateCanvases();
		}

		private void OnMoveToMeetingRoomButtonVisibleStateChanged(bool visibleState) => IsVisibleMoveToMeetingRoomButton = visibleState;

		private void SetupOrganizer()
		{
			IsVisibleReservationChangeButton = false;
			IsVisibleReservationCancelButton = false;

			foreach (var meetingUserInfo in _selectedMeetingInfo.MeetingMembers)
			{
				if (meetingUserInfo.AuthorityCode == AuthorityCode.Organizer)
				{
					if (meetingUserInfo.AccountId == User.Instance.CurrentUserData.ID)
					{
						IsOrganizer = true;

						if (_selectedMeetingInfo.MeetingStatus != MeetingStatus.MeetingOngoing)
						{
							if (_selectedMeetingInfo.MeetingStatus != MeetingStatus.MeetingReadyTime)
							{
								IsVisibleReservationChangeButton = true;
								IsVisibleReservationCancelButton = true;
								IsVisibleMoveToMeetingRoomButton = false;
							}
							else
							{
								IsVisibleMoveToMeetingRoomButton = true;
							}
						}
						else
						{
							IsVisibleMoveToMeetingRoomButton = true;
						}
					}

					//MeetingOrganizerName = meetingUserInfo.EmployeeName;
					MeetingOrganizerName = DataManager.Instance.GetMember(meetingUserInfo.AccountId).Member.MemberName;
				}
			}

			if (CurrentScene.SpaceCode == eSpaceCode.MEETING)
				IsVisibleMoveToMeetingRoomButton = false;

			if (_selectedMeetingInfo.MeetingStatus == MeetingStatus.MeetingExpired)
			{
				IsVisibleReservationChangeButton = false;
				IsVisibleReservationCancelButton = false;
				IsVisibleMoveToMeetingRoomButton = false;
			}
		}

		private void InitializeEnteredEmployeeInfoList()
		{
			_meetingParticipantInfoCollection.Reset();
			_meetingEmployeeProfileCollection.Reset();
			_meetingEmployeeProfileCollectionOrganizer.Reset();
			_meetingEmployeeProfileCollectionWaitList.Reset();

			var meetingUserList = new List<MeetingUserType>();
			foreach (var meetingUserInfo in _selectedMeetingInfo.MeetingMembers)
			{
				// 게스트 제거
				if (meetingUserInfo.MemberType == MemberType.OutsideParticipant)
					continue;
				meetingUserList.Add(meetingUserInfo);
			}
			var sortedMeetingUserList = meetingUserList?.OrderBy(x =>
			{
				if (x.AuthorityCode == AuthorityCode.Organizer)
					return -1;

				return 1;
			}).ToList();

			var enteredUserCount = 0;
			foreach (var meetingUserInfo in sortedMeetingUserList)
			{
				if (!meetingUserInfo.IsEnter)
					continue;
				if (meetingUserInfo.AttendanceCode != AttendanceCode.Join)
					continue;

				enteredUserCount++;
			}

			var loopCount = 0;
			foreach (var meetingUserInfo in sortedMeetingUserList)
			{
				if (!meetingUserInfo.IsEnter)
					continue;
				if (meetingUserInfo.AttendanceCode != AttendanceCode.Join)
					continue;

				var meetingParticipantInfoViewModel = new MeetingParticipantInfoViewModel(meetingUserInfo);

				_meetingParticipantInfoCollection.AddItem(meetingParticipantInfoViewModel);
				meetingParticipantInfoViewModel.IsOrganizer = meetingUserInfo.AuthorityCode == AuthorityCode.Organizer;

				loopCount++;

				if (loopCount > _showingUserListCapacity)
				{
					meetingParticipantInfoViewModel.InvitedUserCount = $"+{(enteredUserCount - _showingUserListCapacity)}";
					break;
				}
			}

			foreach (var meetingUserInfo in sortedMeetingUserList)
			{
				if (meetingUserInfo.AttendanceCode != AttendanceCode.Join)
				{
					var meetingEmployeeProfileWaitListViewModel = new MeetingEmployeeProfileWaitListViewModel(meetingUserInfo, _isOrganizer, 
					                                                                                          OnClick_InviteRequestCancel,
					                                                                                          OnClick_InviteRequestAccept,
					                                                                                          OnClick_InviteRequestReject);
					_meetingEmployeeProfileCollectionWaitList.AddItem(meetingEmployeeProfileWaitListViewModel);
					continue;
				}

				var meetingEmployeeProfileViewModel = new MeetingEmployeeProfileViewModel(meetingUserInfo);

				if (meetingUserInfo.AuthorityCode == AuthorityCode.Organizer)
				{
					_meetingEmployeeProfileCollectionOrganizer.AddItem(meetingEmployeeProfileViewModel);
				}
				else
				{
					_meetingEmployeeProfileCollection.AddItem(meetingEmployeeProfileViewModel);
				}
			}

			DetailPageOrganizerCountText   = Localization.Instance.GetString("UI_ConnectingApp_Detail_Organizer_Text", _meetingEmployeeProfileCollectionOrganizer.CollectionCount);
			DetailPageParticipantCountText = Localization.Instance.GetString("UI_ConnectingApp_Detail_Invitation_Text", _meetingEmployeeProfileCollection.CollectionCount);
			DetailPageWaitListCountText    = Localization.Instance.GetString("UI_ConnectingApp_Detail_WaitMemberTitle_Text", _meetingEmployeeProfileCollectionWaitList.CollectionCount);
		}

		private void OnClick_InviteRequestCancel(MemberIdType memberId)
		{
			RequestInviteCancel(_selectedMeetingInfo.MeetingId, memberId);
		}

		private void OnClick_InviteRequestAccept(MemberIdType memberId)
		{
			RequestWaitListAccept(_selectedMeetingInfo.MeetingId, memberId);
		}

		private void OnClick_InviteRequestReject(MemberIdType memberId)
		{
			RequestWaitListReject(_selectedMeetingInfo.MeetingId, memberId);
		}

		private void RefreshDetailList()
		{
			RequestMeetingInfo(_selectedMeetingInfo.MeetingId);
		}

		public override void OnLanguageChanged()
		{
			base.OnLanguageChanged();
			InitializeUIInfo();
		}
	}
}