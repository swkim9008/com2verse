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
using System.Linq;
using Com2Verse.AssetSystem;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.UI;
using Com2Verse.WebApi.Service;
using Cysharp.Text;
using UnityEngine;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using MeetingStatus = Com2Verse.WebApi.Service.Components.MeetingStatus;
using AttendanceCode = Com2Verse.WebApi.Service.Components.AttendanceCode;

namespace Com2Verse
{
#region Model
	public class MeetingInfoListModel : DataModel
	{
		public string MeetingName;         // 회의 이름
		public DateTime MeetingStartDate;
		public DateTime MeetingEndDate;
		public string MeetingDate;      // 회의 기간
		public string MeetingOrganizer; // 주최자
		public string Participating;    // 참여중인 인원
		public int ParticipantCount;         // 참여인원
		public string MyState;          // 내 커넥팅, 참여대기
	}
#endregion Model

	[ViewModelGroup("MeetingInquiry")]
	public sealed class MeetingInquiryViewModel : ViewModelDataBase<MeetingInfoListModel>
	{
		public enum eStateType
		{
			NONE,
			MY_CONNECTING,
			WAIT_JOIN_REQUEST,
			WAIT_JOIN_RECEIVE,
		}
		// ReSharper disable InconsistentNaming
		public CommandHandler Command_OnButtonClick { get; set; }
		// ReSharper restore InconsistentNaming


		private MeetingInfoType          _meetingInfo;
		private Color                    _tagColor;
		private bool                     _isStateActive;
		private bool                     _isPublic;
		private eStateType               _myStateType;
		private bool                     _isButtonClickActive;
		private ColorScriptableObject    _colorInfo;
		private Color                    _backgroundColor;
		private MeetingStatus            _meetingStatus;
		private Action<MeetingInfoType>  _onClickDetailPage;
		private Action<long, eStateType> _onClickInviteRequest;
		private Action<bool>             _closeInquiryPopup;
#region Properties
		public string MeetingName
		{
			get => base.Model.MeetingName;
			set => SetProperty(ref base.Model.MeetingName, value);
		}

		public DateTime MeetingStartDate
		{
			get => base.Model.MeetingStartDate;
			set => SetProperty(ref base.Model.MeetingStartDate, value);
		}

		public DateTime MeetingEndDate
		{
			get => base.Model.MeetingEndDate;
			set => SetProperty(ref base.Model.MeetingEndDate, value);
		}

		public string MeetingDate
		{
			get => base.Model.MeetingDate;
			set => SetProperty(ref base.Model.MeetingDate, value);
		}

		public string MeetingOrganizer
		{
			get => base.Model.MeetingOrganizer;
			set => SetProperty(ref base.Model.MeetingOrganizer, value);
		}

		public string Participating
		{
			get => base.Model.Participating;
			set => SetProperty(ref base.Model.Participating, value);
		}

		public int ParticipantCount
		{
			get => base.Model.ParticipantCount;
			set => SetProperty(ref base.Model.ParticipantCount, value);
		}

		public string MyState
		{
			get => base.Model.MyState;
			set => SetProperty(ref base.Model.MyState, value);
		}

		public Color TagColor
		{
			get => _tagColor;
			set
			{
				_tagColor = value;
				InvokePropertyValueChanged(nameof(TagColor), value);
			}
		}

		public bool IsStateActive
		{
			get => _isStateActive;
			set
			{
				_isStateActive = value;
				InvokePropertyValueChanged(nameof(IsStateActive), value);
			}
		}

		public bool IsButtonClickActive
		{
			get => _isButtonClickActive;
			set
			{
				_isButtonClickActive = value;
				InvokePropertyValueChanged(nameof(IsButtonClickActive), value);
			}
		}

		public Color BackgroundColor
		{
			get => _backgroundColor;
			set
			{
				_backgroundColor = value;
				InvokePropertyValueChanged(nameof(BackgroundColor), value);
			}
		}
#endregion

		public MeetingInquiryViewModel(MeetingInfoType meetingInfo, Action<MeetingInfoType> onClickDetailPage, Action<long, eStateType> onClickInviteRequest, Action<bool> closeInquiryPopup)
		{
			Command_OnButtonClick = new CommandHandler(OnClickButton);
			_meetingInfo          = meetingInfo;
			MeetingName           = meetingInfo.MeetingName;
			MeetingStartDate      = meetingInfo.StartDateTime;
			MeetingEndDate        = meetingInfo.EndDateTime;
			MeetingDate           = GetMeetingDateString();
			Participating         = GetParticipatingCount();
			_isPublic             = meetingInfo.PublicYn == "Y";
			_myStateType          = GetMyStateType();
			foreach (var member in meetingInfo.MeetingMembers)
			{
				if (member.AuthorityCode == Components.AuthorityCode.Organizer)
				{
					var organizer = DataManager.Instance.GetMember(member.AccountId);
					if (organizer == null)
					{
						MeetingOrganizer = Localization.Instance.GetString("UI_ConnectingApp_GroupMember_Deleted_Text");
						break;
					}

					MeetingOrganizer = organizer.Member?.MemberName;
					break;
				}
			}
			if (meetingInfo.MeetingStatus is MeetingStatus.MeetingExpired or MeetingStatus.MeetingPassed)
				IsButtonClickActive = _myStateType == eStateType.MY_CONNECTING;
			else
				IsButtonClickActive = true;
			_meetingStatus          = meetingInfo.MeetingStatus;
			_onClickDetailPage      = onClickDetailPage;
			_onClickInviteRequest   = onClickInviteRequest;
			_closeInquiryPopup      = closeInquiryPopup;

			var handle = C2VAddressables.LoadAssetAsync<ColorScriptableObject>("ColorData.asset");
			handle.OnCompleted += (internalHandle) =>
			{
				_colorInfo      = internalHandle.Result;
				BackgroundColor = GetBackgroundColor();
				SetTag();
				handle.Release();
			};
		}

		public MeetingInquiryViewModel Clone()
		{
			MeetingInquiryViewModel cloneObj = new MeetingInquiryViewModel(_meetingInfo, _onClickDetailPage, _onClickInviteRequest, _closeInquiryPopup);
			return cloneObj;
		}

		private string TextColorSetting(string text, eStateType type)
		{
			string color = null;

			switch (type)
			{
				case eStateType.MY_CONNECTING:
					color = ColorUtility.ToHtmlStringRGB(_colorInfo.MyConnectingColor);
					break;
				case eStateType.WAIT_JOIN_REQUEST:
					color = ColorUtility.ToHtmlStringRGB(_colorInfo.WaitJoinInquiryColor);
					break;
				case eStateType.WAIT_JOIN_RECEIVE:
					color = ColorUtility.ToHtmlStringRGB(_colorInfo.WaitJoinReceivedColor);
					break;
				default:
					break;
			}

			return ZString.Format("<color=#{0}>{1}</color>", color, text);
		}

		private Color GetColor(eStateType type)
		{
			switch (type)
			{
				case eStateType.WAIT_JOIN_REQUEST:
					return _colorInfo.WaitJoinInquiryBgColor;
				case eStateType.WAIT_JOIN_RECEIVE:
					return _colorInfo.WaitJoinReceivedBgColor;
				case eStateType.MY_CONNECTING:
				default:
					return _colorInfo.MyConnectingBgColor;
			}
		}

		private string GetMeetingDateString() =>
			ZString.Format("{0}.{1:00}.{2:00} {3:00}:{4:00} ~ {5}.{6:00}.{7:00} {8:00}:{9:00}", _meetingInfo.StartDateTime.Year, _meetingInfo.StartDateTime.Month, _meetingInfo.StartDateTime.Day,
			               _meetingInfo.StartDateTime.Hour, _meetingInfo.StartDateTime.Minute, _meetingInfo.EndDateTime.Year, _meetingInfo.EndDateTime.Month, _meetingInfo.EndDateTime.Day,
			               _meetingInfo.EndDateTime.Hour, _meetingInfo.EndDateTime.Minute);

		private string GetParticipatingCount()
		{
			var userCount = _meetingInfo.MeetingMembers.Count(userinfo => userinfo.IsEnter);
			return userCount.ToString();
		}

		private eStateType GetMyStateType()
		{
			if (User.Instance.CurrentUserData is not OfficeUserData userData) return eStateType.NONE;
			foreach (var userinfo in _meetingInfo.MeetingMembers)
			{
				if (userinfo.AccountId == userData.ID)
				{
					return userinfo.AttendanceCode switch
					{
						AttendanceCode.Join        => eStateType.MY_CONNECTING,
						AttendanceCode.JoinRequest => eStateType.WAIT_JOIN_REQUEST,
						AttendanceCode.JoinReceive => eStateType.WAIT_JOIN_RECEIVE,
						_                          => eStateType.NONE
					};
				}
			}

			return eStateType.NONE;
		}

		private void SetTag()
		{
			if (_meetingInfo.MeetingStatus is MeetingStatus.MeetingExpired or MeetingStatus.MeetingPassed)
			{
				if (_myStateType != eStateType.MY_CONNECTING)
				{
					IsStateActive = false;
					return;
				}
			}
			if (_myStateType == eStateType.NONE)
			{
				IsStateActive = false;
				return;
			}
			TagColor = GetColor(_myStateType);
			switch (_myStateType)
			{
				case eStateType.MY_CONNECTING:
					MyState = TextColorSetting(Localization.Instance.GetString("UI_ConnectingApp_Common_CardTag_Myconnceting_Text"), _myStateType);
					break;
				case eStateType.WAIT_JOIN_RECEIVE:
					MyState       = TextColorSetting(Localization.Instance.GetString("UI_ConnectingApp_Common_CardTag_WaitAcceptance_Text"), _myStateType);
					break;
				case eStateType.WAIT_JOIN_REQUEST:
					MyState = TextColorSetting(Localization.Instance.GetString("UI_ConnectingApp_Common_CardTag_Waiting_Text"), _myStateType);
					break;
			}
			IsStateActive = true;
		}

		private void OnClickButton()
		{
			if (_meetingInfo.MeetingStatus is MeetingStatus.MeetingExpired or MeetingStatus.MeetingPassed)
			{
				if (_myStateType != eStateType.MY_CONNECTING)
					return;
			}
			
			switch (_myStateType)
			{
				case eStateType.NONE:
					RequestAttendance();
					break;
				case eStateType.MY_CONNECTING:
					UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString("UI_Common_Popup_Title_Text"), Localization.Instance.GetString("UI_ConnectingApp_Search_Popup_GoDetail_Text"), MoveDetailPage,
					                                  yes: Localization.Instance.GetString("UI_Common_Btn_Yes"), no: Localization.Instance.GetString("UI_Common_Btn_No"));
					break;
				case eStateType.WAIT_JOIN_REQUEST:
					RequestAttendance();
					break;
				case eStateType.WAIT_JOIN_RECEIVE:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void MoveDetailPage(GUIView _)
		{
			_onClickDetailPage.Invoke(_meetingInfo);
			_closeInquiryPopup.Invoke(false);
		}

		private void RequestAttendance()
		{
			_onClickInviteRequest.Invoke(_meetingInfo.MeetingId, _myStateType);
		}

		private Color GetBackgroundColor()
		{
			var color = Color.white;
			if (_meetingStatus is MeetingStatus.MeetingExpired or MeetingStatus.MeetingPassed)
			{
				return _colorInfo.ExpiredBackgroundColor;
			}
			return color;
		}
	}
}
