/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomViewModel.cs
* Developer:	tlghks1009
* Date:			2022-08-29 13:21
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com2Verse.AssetSystem;
using Com2Verse.Chat;
using Com2Verse.Communication;
using Com2Verse.Data;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.PlayerControl;
using Com2Verse.WebApi.Service;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Protocols;
using Protocols.OfficeMeeting;
using UnityEngine;
using User = Com2Verse.Network.User;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using MeetingUserType = Com2Verse.WebApi.Service.Components.MeetingMemberEntity;
using AuthorityCode = Com2Verse.WebApi.Service.Components.AuthorityCode;
using AttendanceCode = Com2Verse.WebApi.Service.Components.AttendanceCode;
using MeetingStatusType = Com2Verse.WebApi.Service.Components.MeetingStatus;
using MemberType = Com2Verse.WebApi.Service.Components.MemberType;
using ResponseRoomJoin = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.RoomJoinResponseResponseFormat>;

namespace Com2Verse.UI
{
#region Model
	public class MeetingInfoModel : DataModel
	{
		public bool IsVisibleMeetingStatus;
		public bool IsVisibleOrganizerLabel;
		public bool IsVisibleMoveToRoomButton;
		public bool IsVisibleParticipatingText;

		public Color  BackgroundColor;
		public Color  MeetingTagColor;
		public string MeetingName;
		public string MeetingDescription;
		public string MeetingDate;
		public string MeetingType;
		public string MeetingStatus;
		public string MeetingOrganizerName;
		public string MeetingCurrentParticipant;
		public string MeetingMaxParticipant;
	}
#endregion Model

	[ViewModelGroup("MeetingReservation")]
	public sealed class MeetingInfoViewModel : ViewModelDataBase<MeetingInfoModel>
	{
		// ReSharper disable InconsistentNaming
		public static readonly string INFORMATION_TITLE_ACCESS_INFO = "Information_Title_Meeting_Info";

		public static readonly string INFORMATION_TEXT_ACCESS_INFO = "Information_Text_Meeting_Info";
		// ReSharper restore InconsistentNaming

		public enum eMeetingStatus
		{
			NONE,
			EXPIRED,
			CANCELED,
			ONGOING,
			STARTED_SOON,	// 0 ~ 10분
			WAIT_JOIN_REQUEST,
			WAIT_JOIN_RECEIVED,
			START_IN,		// 10분 ~ 3시간
			WAIT_START,		// 3시간 ~
		}

		public event Action<bool> OnMoveToMeetingRoomButtonVisibleStateChanged;

		private readonly Action<MeetingInfoViewModel, MeetingInfoType> _onDetailClickedEvent;
		private readonly Action                                    _onMoveToMeetingRoomClicked;

		private readonly MeetingInfoType       _meetingInfo;
		private readonly List<MeetingUserType> _meetingUserInfoList;
		private          MeetingUserType       _myMeetingUserInfo;

		private readonly DateTime _startDateTime;
		private readonly DateTime _endDateTime;

		public eMeetingStatus MeetingStatusType = eMeetingStatus.NONE;

		private          float         _elapsedTime    = 0f;
		private readonly StringBuilder _dateTimeString = new();
		private          UIInfo        _uiInfo;

		private Collection<MeetingParticipantInfoViewModel> _meetingEnteredEmployeeInfoCollection = new();
		private Collection<MeetingEmployeeProfileViewModel> _meetingEmployeeProfileCollection     = new();

		private readonly int _capacity = 5;

		private bool _isMeetingReadyTime; // 시작 ~ 10분 전
		private bool _waitSetting = true;

		private readonly int _tagViewingHour = 3;

		private string _accessText;

		private ColorScriptableObject _colorInfo;

		// ReSharper disable InconsistentNaming
		public CommandHandler Command_DetailClick          { get; }
		public CommandHandler Command_MoveToRoom           { get; }

		public CommandHandler Command_ParticipatingPopupOpenButtonClick { get; }
		// ReSharper restore InconsistentNaming

		public MeetingInfoViewModel(MeetingInfoType meetingInfo, Action onMoveToMeetingRoomClicked, Action<MeetingInfoViewModel, MeetingInfoType> onDetailClickedEventListener)
		{
			Command_DetailClick                       = new CommandHandler(OnCommand_OnDetailClicked);
			Command_MoveToRoom                        = new CommandHandler(OnCommand_MoveToRoomButtonClicked);
			Command_ParticipatingPopupOpenButtonClick = new CommandHandler(OnCommand_ParticipatingPopupOpenButtonClicked);

			_onDetailClickedEvent       = onDetailClickedEventListener;
			_onMoveToMeetingRoomClicked = onMoveToMeetingRoomClicked;

			_meetingInfo         = meetingInfo;
			_meetingUserInfoList = meetingInfo.MeetingMembers?.ToList();
			_startDateTime       = meetingInfo.StartDateTime;
			_endDateTime         = meetingInfo.EndDateTime;

			//InitializeMeetingStatus();
			CheckExpirationStatus();

			SetActiveMoveToRoomButton = false;
			SetActiveOrganizerLabel   = MeetingReservationProvider.IsOrganizer(_meetingUserInfoList);

			MeetingName        = _meetingInfo.MeetingName;
			MeetingDescription = _meetingInfo.MeetingDescription;
			MeetingDate        = GetMeetingDateString();
			
			var zString = new ZStringWriter();
			foreach (var meetingUserInfo in _meetingInfo.MeetingMembers)
			{
				if (meetingUserInfo.AuthorityCode == AuthorityCode.Organizer)
				{
					zString.Write(MeetingReservationProvider.GetMemberNameInOrganization(meetingUserInfo.AccountId));
					zString.Write(" ");
				}

				if (meetingUserInfo.AccountId == User.Instance.CurrentUserData.ID)
					_myMeetingUserInfo = meetingUserInfo;
			}

			MeetingOrganizerName = zString.ToString();

			MeetingCurrentParticipant = _meetingInfo.MeetingMembers.Length.ToString();
			MeetingMaxParticipant     = MeetingReservationProvider.MaxNumberOfParticipants.ToString();

			IsMeetingReadyTime = false;
			BackgroundColor    = Color.white;

			InitializeEnteredUserInfoList();

			AccessText = meetingInfo.PublicYn == "Y" ? Localization.Instance.GetString("UI_ConnectingApp_Card_Privacy_Text_1")
				: Localization.Instance.GetString("UI_ConnectingApp_Card_Privacy_Text_2");
			var handle = C2VAddressables.LoadAssetAsync<ColorScriptableObject>("ColorData.asset");
			handle.OnCompleted += (opHandle) =>
			{
				_colorInfo = opHandle.Result;
				InitializeMeetingStatus();
				RefreshTag();
				_waitSetting = false;
				handle.Release();
			};
		}

#region Properties
		public MeetingInfoType MeetingInfo => _meetingInfo;
		public long        MeetingId   => _meetingInfo.MeetingId;

		public Color BackgroundColor
		{
			get => base.Model.BackgroundColor;
			set
			{
				SetProperty(ref Model.BackgroundColor, value);
			}
		}

		public Color MeetingTagColor
		{
			get => base.Model.MeetingTagColor;
			set
			{
				base.Model.MeetingTagColor = value;
				base.InvokePropertyValueChanged(nameof(MeetingTagColor), value);
			}
		}

		public string AccessText
		{
			get => _accessText;
			set
			{
				_accessText = value;
				InvokePropertyValueChanged(nameof(AccessText), value);
			}
		}

		public bool IsMeetingReadyTime
		{
			get => _isMeetingReadyTime;
			set
			{
				_isMeetingReadyTime = value;
				InvokePropertyValueChanged(nameof(IsMeetingReadyTime), value);
			}
		}


		public string MeetingName
		{
			get => base.Model.MeetingName;
			set => SetProperty(ref base.Model.MeetingName, value);
		}

		public string MeetingDescription
		{
			get => base.Model.MeetingDescription;
			set => SetProperty(ref base.Model.MeetingDescription, value);
		}

		public string MeetingStatus
		{
			get => base.Model.MeetingStatus;
			set => SetProperty(ref base.Model.MeetingStatus, value);
		}

		public string MeetingDate
		{
			get => base.Model.MeetingDate;
			set => SetProperty(ref base.Model.MeetingDate, value);
		}

		public string MeetingOrganizerName
		{
			get => base.Model.MeetingOrganizerName;
			set => SetProperty(ref base.Model.MeetingOrganizerName, value);
		}

		public string MeetingCurrentParticipant
		{
			get => base.Model.MeetingCurrentParticipant;
			set => SetProperty(ref base.Model.MeetingCurrentParticipant, value);
		}

		public string MeetingMaxParticipant
		{
			get => base.Model.MeetingMaxParticipant;
			set => SetProperty(ref base.Model.MeetingMaxParticipant, value);
		}

		public bool SetActiveMoveToRoomButton
		{
			get => base.Model.IsVisibleMoveToRoomButton;
			set
			{
				if (value)
				{
					if (CurrentScene.SpaceCode is eSpaceCode.MEETING)
					{
						SetProperty(ref base.Model.IsVisibleMoveToRoomButton, false);
						InvokePropertyValueChanged(nameof(SetActiveMoveToRoomButton), false);
						return;
					}
				}

				SetProperty(ref base.Model.IsVisibleMoveToRoomButton, value);
				InvokePropertyValueChanged(nameof(SetActiveMoveToRoomButton), value);

				OnMoveToMeetingRoomButtonVisibleStateChanged?.Invoke(value);
			}
		}

		public bool SetActiveOrganizerLabel
		{
			get => base.Model.IsVisibleOrganizerLabel;
			set => SetProperty(ref base.Model.IsVisibleOrganizerLabel, value);
		}

		public bool SetActiveMeetingStatus
		{
			get => base.Model.IsVisibleMeetingStatus;
			set => SetProperty(ref base.Model.IsVisibleMeetingStatus, value);
		}

		public bool OpenEmployeeProfilePopup
		{
			get => false;
			set
			{
				if (_meetingEmployeeProfileCollection.CollectionCount == 0)
					return;

				base.InvokePropertyValueChanged(nameof(OpenEmployeeProfilePopup), OpenEmployeeProfilePopup);
			}
		}

		public bool CloseEmployeeProfilePopup
		{
			get => false;
			set => base.InvokePropertyValueChanged(nameof(CloseEmployeeProfilePopup), CloseEmployeeProfilePopup);
		}

		public bool IsVisibleParticipatingText
		{
			get => base.Model.IsVisibleParticipatingText;
			set => SetProperty(ref base.Model.IsVisibleParticipatingText, value);
		}

		public Collection<MeetingParticipantInfoViewModel> MeetingEnteredEmployeeInfoCollection
		{
			get => _meetingEnteredEmployeeInfoCollection;
			set
			{
				_meetingEnteredEmployeeInfoCollection = value;
				base.InvokePropertyValueChanged(nameof(MeetingEnteredEmployeeInfoCollection), value);
			}
		}

		public Collection<MeetingEmployeeProfileViewModel> MeetingEmployeeProfileCollection
		{
			get => _meetingEmployeeProfileCollection;
			set
			{
				_meetingEmployeeProfileCollection = value;
				base.InvokePropertyValueChanged(nameof(MeetingEmployeeProfileCollection), value);
			}
		}
#endregion Properties

		private void OnCommand_ParticipatingPopupOpenButtonClicked()
		{
			OpenEmployeeProfilePopup = true;
		}

		public void OnCommand_OnDetailClicked()
		{
			// TODO : 초대 수락/거절이 되지 않아 임시코드
			if (MeetingStatusType is eMeetingStatus.CANCELED or eMeetingStatus.WAIT_JOIN_RECEIVED)
				return;

			_onDetailClickedEvent?.Invoke(this, _meetingInfo);
		}

		public void OnCommand_MoveToRoomButtonClicked()
		{
			// 이동중 OSR/Input 막기
			PlayerController.Instance.SetStopAndCannotMove(true);
			User.Instance.DiscardPacketBeforeStandBy();
			Commander.Instance.RequestRoomJoinAsync(_meetingInfo.MeetingId, OnJoinChannelResponse).Forget();
			//OpenMeetingRoomYesNoPopup();
		}

		private void OpenMeetingRoomYesNoPopup()
		{
			UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString("UI_Common_Notice_Popup_Title"),
			                                  Localization.Instance.GetString("UI_MeetingApp_Btn_Direct_Popup_Direct", MeetingName),
			                                  (guiView) =>
			                                  {
				                                  Commander.Instance.RequestRoomJoinAsync(_meetingInfo.MeetingId, OnJoinChannelResponse).Forget();
			                                  });
		}

		private void OnErrorJoinChannel(MessageTypes messageTypes, ErrorCode errorCode)
		{
			if (messageTypes != MessageTypes.MeetingJoinResponse)
			{
				C2VDebug.LogError("Wrong MessageType ErrorCode!");
				return;
			}

			switch (errorCode)
			{
				// 회의가 종료됐을 경우
				case ErrorCode.NotMeetingStartReadyTimeBetweenEndReadyTime:
					UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Reservation_AlreadyConnectingDone_Toast"));
					break;
				default:
					UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_Common_UnknownProblemError_Popup_Text", ZString.Format("{0} : {1}", "ErrorCode", (int)errorCode)));
					C2VDebug.LogError("OnErrorMeetingReservation ErrorCode : " + errorCode);
					break;
			}
		}


		private void OnUpdate()
		{
			if (_waitSetting)
				return;
			if (_meetingInfo == null)
				return;

			if (MeetingStatusType is eMeetingStatus.EXPIRED or eMeetingStatus.CANCELED)
				return;

			if (_myMeetingUserInfo == null)
				return;
			if (_myMeetingUserInfo.AttendanceCode is AttendanceCode.JoinRequest or AttendanceCode.JoinReceive)
				return;

			_elapsedTime += Time.deltaTime;
			if (_elapsedTime > 1)
			{
				CheckExpirationStatus();

				BackgroundColor = GetBackgroundColor();
				MeetingDate     = GetMeetingDateString();

				RefreshTag();

				_elapsedTime = 0;
			}
		}

		private string GetMeetingDateString() =>
			ZString.Format("{0}.{1:00}.{2:00} {3:00}:{4:00} ~ {5}.{6:00}.{7:00} {8:00}:{9:00}", _meetingInfo.StartDateTime.Year, _meetingInfo.StartDateTime.Month, _meetingInfo.StartDateTime.Day,
			               _meetingInfo.StartDateTime.Hour, _meetingInfo.StartDateTime.Minute, _meetingInfo.EndDateTime.Year, _meetingInfo.EndDateTime.Month, _meetingInfo.EndDateTime.Day,
			               _meetingInfo.EndDateTime.Hour, _meetingInfo.EndDateTime.Minute);

		private void RefreshTag()
		{
			if (MeetingStatusType is eMeetingStatus.EXPIRED or eMeetingStatus.CANCELED)
				return;
			if (_myMeetingUserInfo?.AttendanceCode is AttendanceCode.JoinReceive or AttendanceCode.JoinRequest)
				return;

			var nowDateTime = MetaverseWatch.NowDateTime;
			//if (nowDateTime.Date != _meetingInfo.StartDateTime.ToDateTime().Date)
			//	return;

			var startDateTime = _startDateTime;
			var remainingTime = startDateTime - nowDateTime;

			if (remainingTime.TotalHours >= 3)
				return;

			SetActiveMeetingStatus     = true;
			IsVisibleParticipatingText = true;

			if (remainingTime.TotalMinutes < 0)
			{
				SetActiveMoveToRoomButton  = true;
				IsMeetingReadyTime         = true;
				MeetingStatusType         = eMeetingStatus.ONGOING;

				MeetingStatus   = ColorSetting(Localization.Instance.GetString("UI_ConnectingApp_Common_CardTag_Proceeding_Text"), eMeetingStatus.ONGOING);
				MeetingTagColor = GetColor(MeetingStatusType);
				return;
			}

			if (remainingTime.TotalMinutes <= MeetingReservationProvider.AdmissionTime)
			{
				SetActiveMoveToRoomButton  = true;
				IsMeetingReadyTime         = true;
				MeetingStatusType         = eMeetingStatus.STARTED_SOON;

				MeetingStatus   = ColorSetting(Localization.Instance.GetString("UI_ConnectingApp_Common_CardTag_Soon_Text"), eMeetingStatus.STARTED_SOON);
				MeetingTagColor = GetColor(MeetingStatusType);
				return;
			}

			MeetingStatusType         = eMeetingStatus.START_IN;
			MeetingStatus = ColorSetting(remainingTime.TotalHours switch
			{
				< 1 => Localization.Instance.GetString("UI_ConnectingApp_Common_CardTag_AfterMinute_Text", ((int)remainingTime.TotalMinutes).ToString()),
				> 1 => Localization.Instance.GetString("UI_ConnectingApp_Common_CardTag_AfterHour_Text",   ((int)remainingTime.TotalHours).ToString()),
				_   => string.Empty
			}, eMeetingStatus.START_IN);
			MeetingTagColor = GetColor(MeetingStatusType);
		}


		private void InitializeMeetingStatus()
		{
			SetActiveMeetingStatus = true;
			BackgroundColor        = GetBackgroundColor();

			if (_meetingInfo.MeetingStatus is Components.MeetingStatus.MeetingExpired or Components.MeetingStatus.MeetingPassed)
			{
				MeetingStatusType = eMeetingStatus.EXPIRED;

				//MeetingStatus = Localization.Instance.GetString("UI_MeetingApp_Desc_Expire");
				SetActiveMeetingStatus     = false;
				IsVisibleParticipatingText = false;
				BackgroundColor            = GetBackgroundColor();
				return;
			}

			if (_meetingInfo.CancelYn == "Y")
			{
				MeetingStatusType = eMeetingStatus.CANCELED;

				MeetingStatus              = ColorSetting(Localization.Instance.GetString("UI_ConnectingApp_Common_CardTag_Cancellation_Text"), MeetingStatusType);
				MeetingTagColor            = GetColor(MeetingStatusType);
				SetActiveMeetingStatus     = true;
				IsVisibleParticipatingText = false;
				BackgroundColor            = GetBackgroundColor();
				return;
			}

			// TODO : 초대 수락/거절 기능이 되지 않아 임시 코드
			if (_myMeetingUserInfo?.AttendanceCode is AttendanceCode.JoinReceive)
			{
				MeetingStatusType          = eMeetingStatus.WAIT_JOIN_RECEIVED;
				MeetingStatus              = ColorSetting(Localization.Instance.GetString("UI_ConnectingApp_Common_CardTag_WaitAcceptance_Text"), MeetingStatusType);

				MeetingTagColor            = GetColor(MeetingStatusType);
				SetActiveMeetingStatus     = true;
				IsVisibleParticipatingText = false;
				BackgroundColor            = GetBackgroundColor();
				return;
			}

			if (_myMeetingUserInfo?.AttendanceCode is AttendanceCode.JoinRequest or AttendanceCode.JoinReceive)
			{
				if (_myMeetingUserInfo?.AttendanceCode == AttendanceCode.JoinRequest)
				{
					MeetingStatusType = eMeetingStatus.WAIT_JOIN_REQUEST;
					MeetingStatus     = ColorSetting(Localization.Instance.GetString("UI_ConnectingApp_Common_CardTag_Waiting_Text"), MeetingStatusType);
				}
				else
				{
					MeetingStatusType = eMeetingStatus.WAIT_JOIN_RECEIVED;
					MeetingStatus = ColorSetting(Localization.Instance.GetString("UI_ConnectingApp_Common_CardTag_WaitAcceptance_Text"), MeetingStatusType);
				}
				MeetingTagColor            = GetColor(MeetingStatusType);
				SetActiveMeetingStatus     = true;
				IsVisibleParticipatingText = false;
				return;
			}

			if ((_meetingInfo.StartDateTime - MetaverseWatch.NowDateTime).TotalHours >= _tagViewingHour)
			{
				MeetingStatusType         = eMeetingStatus.WAIT_START;
				SetActiveMeetingStatus     = false;
				IsVisibleParticipatingText = false;
				return;
			}

			if (_meetingInfo.MeetingStatus == Components.MeetingStatus.MeetingOngoing)
			{
				MeetingStatusType = eMeetingStatus.ONGOING;

				MeetingStatus              = ColorSetting(Localization.Instance.GetString("UI_ConnectingApp_Common_CardTag_Proceeding_Text"), MeetingStatusType);
				MeetingTagColor            = GetColor(MeetingStatusType);
				SetActiveMeetingStatus     = true;
				IsVisibleParticipatingText = true;
				SetActiveMoveToRoomButton  = true;
				IsMeetingReadyTime         = true;
				return;
			}
		}

		private void CheckExpirationStatus()
		{
			if (MetaverseWatch.NowDateTime > _endDateTime)
			{
				MeetingStatusType          = eMeetingStatus.EXPIRED;
				SetActiveMeetingStatus     = false;
				IsVisibleParticipatingText = false;
				SetActiveMoveToRoomButton  = false;
			}
		}

		private Color GetBackgroundColor()
		{
			var color = Color.white;
			// TODO : 초대 수락/거절이 되지 않아 임시코드
			if (MeetingStatusType is eMeetingStatus.EXPIRED or eMeetingStatus.CANCELED or eMeetingStatus.WAIT_JOIN_RECEIVED)
			{
				return _colorInfo.ExpiredBackgroundColor;
			}

			return color;
		}


		private void InitializeEnteredUserInfoList()
		{
			_meetingEnteredEmployeeInfoCollection.Reset();
			_meetingEmployeeProfileCollection.Reset();

			var meetingUserList = new List<MeetingUserType>();
			foreach (var meetingUserInfo in _meetingUserInfoList)
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

			int loopCount = 0;
			foreach (var meetingUserInfo in sortedMeetingUserList)
			{
				if (!meetingUserInfo.IsEnter)
					continue;
				if (meetingUserInfo.AttendanceCode != AttendanceCode.Join)
					continue;

				var meetingParticipantInfoViewModel = new MeetingParticipantInfoViewModel(meetingUserInfo);

				_meetingEnteredEmployeeInfoCollection.AddItem(meetingParticipantInfoViewModel);
				meetingParticipantInfoViewModel.IsOrganizer = meetingUserInfo.AuthorityCode == AuthorityCode.Organizer;

				loopCount++;

				if (loopCount > _capacity)
				{
					meetingParticipantInfoViewModel.InvitedUserCount = $"+{(enteredUserCount - _capacity)}";
					break;
				}
			}

			foreach (var meetingUserInfo in sortedMeetingUserList)
			{
				if (!meetingUserInfo.IsEnter)
					continue;

				var meetingEmployeeProfileViewModel = new MeetingEmployeeProfileViewModel(meetingUserInfo);

				_meetingEmployeeProfileCollection.AddItem(meetingEmployeeProfileViewModel);
			}
		}

		public void RegisterUpdateEvent()
		{
			UIManager.Instance.AddUpdateListener(OnUpdate);
		}

		public void UnregisterUpdateEvent()
		{
			UIManager.Instance.RemoveUpdateListener(OnUpdate);
		}

		public bool IsPossibleViewDetailPage()
		{
			return MeetingStatusType is not (eMeetingStatus.CANCELED);
		}

		public bool IsJoinRequestWaiting()
		{
			return _myMeetingUserInfo.AttendanceCode == AttendanceCode.JoinRequest;
		}

		public bool IsJoinReceiveWaiting()
		{
			return _myMeetingUserInfo.AttendanceCode == AttendanceCode.JoinReceive;
		}

		private string ColorSetting(string text, eMeetingStatus type)
		{
			string color = null;
			switch (type)
			{
				case eMeetingStatus.NONE:
				case eMeetingStatus.EXPIRED:
					break;
				case eMeetingStatus.CANCELED:
					color = ColorUtility.ToHtmlStringRGB(_colorInfo.CanceledColor);
					break;
				case eMeetingStatus.ONGOING:
					color = ColorUtility.ToHtmlStringRGB(_colorInfo.OnGoingColor);
					break;
				case eMeetingStatus.STARTED_SOON:
				case eMeetingStatus.START_IN:
					color = ColorUtility.ToHtmlStringRGB(_colorInfo.StartedSoonColor);
					break;
				case eMeetingStatus.WAIT_JOIN_REQUEST:
					color = ColorUtility.ToHtmlStringRGB(_colorInfo.WaitJoinRequestColor);
					break;
				case eMeetingStatus.WAIT_JOIN_RECEIVED:
					color = ColorUtility.ToHtmlStringRGB(_colorInfo.WaitJoinReceivedColor);
					break;
				default:
					break;
			}

			return ZString.Format("<color=#{0}>{1}</color>", color, text);
		}

		private Color GetColor(eMeetingStatus type)
		{
			switch (type)
			{
				case eMeetingStatus.NONE:
				case eMeetingStatus.EXPIRED:
					break;
				case eMeetingStatus.CANCELED:
					return _colorInfo.CanceledBgColor;
				case eMeetingStatus.ONGOING:
					return _colorInfo.OnGoingBgColor;
				case eMeetingStatus.STARTED_SOON:
				case eMeetingStatus.START_IN:
					return _colorInfo.StartedSoonBgColor;
				case eMeetingStatus.WAIT_JOIN_REQUEST:
					return _colorInfo.WaitJoinRequestBgColor;
				case eMeetingStatus.WAIT_JOIN_RECEIVED:
					return _colorInfo.WaitJoinReceivedBgColor;
				default:
					break;
			}

			return _colorInfo.CanceledBgColor;
		}

#region Web API
#region RoomJoin
		private async void OnJoinChannelResponse(ResponseRoomJoin response)
		{
			_onMoveToMeetingRoomClicked?.Invoke();

			MeetingReservationProvider.RoomId = response.Value.Data.RoomId;
			ChatManager.Instance.SetAreaMove(response.Value.Data.GroupId);

			MeetingReservationProvider.SetMeetingInfo(_meetingInfo);
			var memberModel = await DataManager.Instance.GetMyselfAsync();
			var extraInfo = new ExtraInfo
			{
				Uid      = User.Instance.CurrentUserData.ID.ToString()!,
				Job      = memberModel.Member.Level,
				Name     = memberModel.Member.MemberName,
				Position = memberModel.Member.Position,
				Team     = memberModel.TeamName,
				Token    = "",
			};
			var jsonExtraInfo = JsonConvert.SerializeObject(extraInfo);
			ChannelManagerHelper.AddChannel(response.Value.Data, MeetingReservationProvider.DisconnectRequestFromMediaChannel, jsonExtraInfo);
		}
#endregion // RoomJoin
#endregion // Web API
	}
}
