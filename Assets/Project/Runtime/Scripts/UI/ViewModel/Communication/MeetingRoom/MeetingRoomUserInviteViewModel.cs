/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomUserInviteViewModel.cs
* Developer:	ksw
* Date:			2023-04-17 14:36
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Network;
using Com2Verse.UI;
using System.Linq;
using Com2Verse.Data;
using Com2Verse.MeetingReservation;
using Com2Verse.Organization;
using Cysharp.Text;
using JetBrains.Annotations;
using UnityEngine;
using MemberIdType = System.Int64;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using MemberType = Com2Verse.WebApi.Service.Components.MemberType;
using AuthorityCode = Com2Verse.WebApi.Service.Components.AuthorityCode;
using Localization = Com2Verse.UI.Localization;
using String = System.String;

namespace Com2Verse
{
	[UsedImplicitly] [ViewModelGroup("Communication")]
	public sealed partial class MeetingRoomUserInviteViewModel : ViewModelBase
	{
		private bool            _isVisibleInvitePopup;
		private MeetingInfoType _meetingInfo;
		private int             _possibleInviteNumbers;
		private string          _possibleInviteNumbersText;
		private bool            _isUserLayout;

		private List<MemberIdType> _meetingAllUserMemberId = new();
		private List<MemberIdType> _meetingBlockedMemberId = new();

		private Collection<MeetingRoomInviteTagViewModel> _inviteTagCollection = new();

		public CommandHandler CommandCloseButtonClick  { get; }
		public CommandHandler CommandOpenOrganization  { get; }
		public CommandHandler CommandCopyMeetingCode     { get; }
		public CommandHandler CommandCopyInviteCode    { get; }
		public CommandHandler CommandInviteButton      { get; }
		public CommandHandler CommandInvitePopupButton { get; }

#region Properties
		public bool IsVisibleInvitePopup
		{
			get => _isVisibleInvitePopup;
			set
			{
				if (value)
					RequestMeetingInfo();

				SetProperty(ref _isVisibleInvitePopup, value);
			}
		}

		public int PossibleInviteNumbers
		{
			get => _possibleInviteNumbers;
			set => SetProperty(ref _possibleInviteNumbers, value);
		}

		public string PossibleInviteNumbersText
		{
			get => _possibleInviteNumbersText;
			set => SetProperty(ref _possibleInviteNumbersText, value);
		}

		public Collection<MeetingRoomInviteTagViewModel> InviteTagCollection
		{
			get => _inviteTagCollection;
			set
			{
				_inviteTagCollection = value;
				base.InvokePropertyValueChanged(nameof(InviteTagCollection), value);
			}
		}

		public bool IsUserLayout
		{
			get => _isUserLayout;
			set
			{
				if (!value)
					OnClick_CloseButton();

				_isUserLayout = value;
			}
		}

		public string MeetingCode => _meetingInfo?.MeetingCode;
#endregion

		public MeetingRoomUserInviteViewModel()
		{
			CommandCloseButtonClick  = new CommandHandler(OnClick_CloseButton);
			CommandOpenOrganization  = new CommandHandler(OnClick_OpenOrganization);
			CommandCopyMeetingCode     = new CommandHandler(OnClick_CopyMeetingCode);
			CommandCopyInviteCode    = new CommandHandler(OnClick_CopyInviteCode);
			CommandInviteButton      = new CommandHandler(OnClick_InviteButton);
			CommandInvitePopupButton = new CommandHandler(OnClick_InvitePopupButton);

			_isVisibleInvitePopup = false;
			_meetingInfo          = MeetingReservationProvider.EnteredMeetingInfo;

			if (_meetingInfo != null)
			{
				PossibleInviteNumbers = MeetingReservationProvider.MaxNumberOfParticipants - _meetingInfo.MeetingMembers.Length;
			}

			SetupMeetingUserInfo();
		}

		private async void OnClick_OpenOrganization()
		{
			// 조직도 정보 Refresh
			await DataManager.TryOrganizationRefreshAsync(eOrganizationRefreshType.CONNECTING_APP, DataManager.Instance.GroupID);
			var info = OrganizationHierarchyViewModel.HierarchyViewInfo.Create(_meetingAllUserMemberId, _meetingBlockedMemberId);
			OrganizationHierarchyViewModel.ShowView(OrganizationHierarchyViewModel.ePopupType.INVITE_MEMBER,
			                                        User.Instance.CurrentUserData.ID,
			                                        info,
			                                        (popupType, employeeNoList) =>
			                                        {
				                                        OrganizationHierarchyViewModel.HideView();
				                                        foreach (var employeeNo in employeeNoList)
					                                        AddMember(employeeNo);
			                                        });
		}

		private void SetUpOrganizationDataInfo()
		{
			_meetingAllUserMemberId.Clear();
			_meetingBlockedMemberId.Clear();
			foreach (var meetingUserInfo in MeetingReservationProvider.EnteredMeetingInfo.MeetingMembers)
			{
				if (meetingUserInfo.MemberType == MemberType.OutsideParticipant)
					continue;
				_meetingAllUserMemberId.Add(meetingUserInfo.AccountId);
				_meetingBlockedMemberId.Add(meetingUserInfo.AccountId);
			}
		}

		private void OnClick_CopyMeetingCode()
		{
			GUIUtility.systemCopyBuffer = MeetingCode;
			UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_Common_UserList_Invitation_SpaceCodeCopied_Toast"));
		}

		private void OnClick_CopyInviteCode()
		{
			GUIUtility.systemCopyBuffer = Localization.Instance.GetString("UI_MeetingRoom_Common_ConnectingInfo_Invitation_Content_Text",
			                                                              MeetingReservationProvider.GetOrganizerName(),
			                                                              GetMeetingDateString(), _meetingInfo.MeetingName, MeetingCode);
			UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_Common_UserList_Invitation_InvitationCopied_Toast"));
		}

		private string GetMeetingDateString() =>
			ZString.Format("{0}.{1:00}.{2:00} {3:00}:{4:00} ~ {5}.{6:00}.{7:00} {8:00}:{9:00}", _meetingInfo?.StartDateTime.Year, _meetingInfo?.StartDateTime.Month, _meetingInfo?.StartDateTime.Day,
			               _meetingInfo?.StartDateTime.Hour, _meetingInfo?.StartDateTime.Minute, _meetingInfo?.EndDateTime.Year, _meetingInfo?.EndDateTime.Month, _meetingInfo?.EndDateTime.Day,
			               _meetingInfo?.EndDateTime.Hour, _meetingInfo?.EndDateTime.Minute);

		private void OnClick_InvitePopupButton()
		{
			if (MeetingReservationProvider.IsGuest())
			{
				UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Guest_MeetingRoom_GuestNotUse_Toast"));
				OnClick_CloseButton();
				return;
			}

			if (MeetingReservationProvider.EnteredMeetingInfo == null)
			{
				OnClick_CloseButton();
				return;
			}

			if (User.Instance.CurrentUserData is OfficeUserData userData)
			{
				foreach (var meetingUserInfo in MeetingReservationProvider.EnteredMeetingInfo.MeetingMembers)
				{
					if (meetingUserInfo.AccountId == userData.ID)
					{
						if (meetingUserInfo.AuthorityCode != AuthorityCode.Organizer)
							break;
						IsVisibleInvitePopup = true;
						SetUpOrganizationDataInfo();
						return;
					}
				}
			}
			OnClick_CloseButton();
			UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_UserList_Invitation_OnlyOrganizerInvite_Toast"));
		}

		private void OnClick_InviteButton()
		{
			if (_inviteTagCollection.CollectionCount == 0)
				return;

			if (User.Instance.CurrentUserData is OfficeUserData userData)
			{
				foreach (var meetingUserInfo in MeetingReservationProvider.EnteredMeetingInfo.MeetingMembers)
				{
					if (meetingUserInfo.AccountId == userData.ID)
					{
						if (meetingUserInfo.AuthorityCode != AuthorityCode.Organizer)
							break;
						RequestConnectingInvite();
						return;
					}
				}
			}

			OnClick_CloseButton();
			UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_UserList_Invitation_OnlyOrganizerInvite_Toast"));
		}
		private void SetupMeetingUserInfo()
		{
			ResetMeetingTagCollection();
		}

		private void AddMeetingTagElement(MemberIdType memberId)
		{
			_inviteTagCollection.AddItem(new MeetingRoomInviteTagViewModel(memberId, OnClickTagRemove));
			_meetingAllUserMemberId.Add(memberId);
			_meetingBlockedMemberId.Add(memberId);

			var meetingTagCollectionClone = _inviteTagCollection.Clone();
			var sortedCollection          = meetingTagCollectionClone?.OrderBy(x => x.InviteEmployeeName).ToList();

			ResetMeetingTagCollection();

			foreach (var meetingTagViewModel in sortedCollection)
			{
				_inviteTagCollection.AddItem(meetingTagViewModel);
			}

			RefreshViewParticipants();
		}


		private void RemoveMeetingTagElement(MeetingRoomInviteTagViewModel meetingRoomInviteTagViewModel)
		{
			_inviteTagCollection.RemoveItem(meetingRoomInviteTagViewModel);
			_meetingAllUserMemberId.Remove(meetingRoomInviteTagViewModel.InviteEmployeeNo);
			_meetingBlockedMemberId.Remove(meetingRoomInviteTagViewModel.InviteEmployeeNo);

			RefreshViewParticipants();
		}


		private void ResetMeetingTagCollection()
		{
			InviteTagCollection.Reset();
		}

		private void RefreshViewParticipants()
		{
			PossibleInviteNumbers = MeetingReservationProvider.MaxNumberOfParticipants - MeetingReservationProvider.EnteredMeetingInfo.MeetingMembers.Length;
			PossibleInviteNumbersText = Localization.Instance.GetString("UI_MeetingRoom_UserList_Invitation_Invite_Popup_Limit_Count_Text", PossibleInviteNumbers);
		}

		private void OnClick_CloseButton()
		{
			InviteTagCollection.Reset();
			MeetingEmployeeResults.Reset();
			_meetingAllUserMemberId.Clear();
			_meetingBlockedMemberId.Clear();

			SearchEmployee       = String.Empty;
			SetActiveDropDown    = false;
			EmployeeName         = String.Empty;
			IsVisibleInvitePopup = false;
		}

		private void OnClickTagRemove(MeetingRoomInviteTagViewModel meetingRoomInviteTagViewModel)
		{
			RemoveMeetingTagElement(meetingRoomInviteTagViewModel);
		}

		private void ResponseInvite()
		{
			OnClick_CloseButton();
			UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_UserList_Invitation_CompleteRequestInvitation_Toast"));
			// 대기 목록 갱신
			RequestMeetingInfo();
		}

		public override void OnLanguageChanged()
		{
			base.OnLanguageChanged();
			RefreshViewParticipants();
		}
	}
}
