/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingReservationViewModel_EmployeeTag.cs
* Developer:	tlghks1009
* Date:			2022-10-18 16:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using MemberIdType = System.Int64;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using AuthorityCode = Com2Verse.WebApi.Service.Components.AuthorityCode;
using AttendanceCode = Com2Verse.WebApi.Service.Components.AttendanceCode;
using MeetingUsersInfo = Com2Verse.WebApi.Service.Components.MeetingMemberEntity;

namespace Com2Verse.UI
{
	public partial class MeetingReservationViewModel
	{
		private readonly List<MemberIdType> _willDeleteEmployeeNoList = new();

		// 실제 참여중인 사용자만 담은 Collection
		private Collection<MeetingTagViewModel> _meetingTagCollection = new();

		private List<MeetingUsersInfo> _meetingUserInfoList = new();
		// 대기중인 사용자도 모두 담은 string list. 조직도에 사용
		private List<MemberIdType> _meetingAllUserMemberId = new();
		private List<MemberIdType> _meetingBlockedMemberId = new();

		public Collection<MeetingTagViewModel> MeetingTagCollection
		{
			get => _meetingTagCollection;
			set
			{
				_meetingTagCollection = value;
				base.InvokePropertyValueChanged(nameof(MeetingTagCollection), value);
			}
		}


		private async void OnCommand_OpenOrganization()
		{
			// 조직도 정보 Refresh
			await DataManager.TryOrganizationRefreshAsync(eOrganizationRefreshType.CONNECTING_APP, DataManager.Instance.GroupID);
			var info = OrganizationHierarchyViewModel.HierarchyViewInfo.Create(_meetingAllUserMemberId, _meetingBlockedMemberId);
			OrganizationHierarchyViewModel.ShowView(OrganizationHierarchyViewModel.ePopupType.MEETING_RESERVATION,
			                                        User.Instance.CurrentUserData.ID,
			                                        info,
			                                        (popupType, employeeNoList) =>
			                                        {
				                                        OrganizationHierarchyViewModel.HideView();
				                                        foreach (var employeeNo in employeeNoList)
					                                        AddEmployee(employeeNo);
			                                        });
		}

		private void SetupMeetingUserInfo(MeetingInfoType meetingInfo)
		{
			ResetMeetingTagCollection();

			_willDeleteEmployeeNoList.Clear();
			_meetingAllUserMemberId.Clear();
			_meetingUserInfoList.Clear();
			_meetingBlockedMemberId.Clear();

			foreach (var meetingUserInfo in meetingInfo.MeetingMembers)
			{
				_meetingAllUserMemberId.Add(meetingUserInfo.AccountId);
				_meetingUserInfoList.Add(meetingUserInfo);
				_meetingBlockedMemberId.Add(meetingUserInfo.AccountId);
				if (meetingUserInfo.AuthorityCode == AuthorityCode.Organizer)
					continue;
				if (meetingUserInfo.AttendanceCode is AttendanceCode.JoinRequest or AttendanceCode.JoinReceive)
					continue;
				MeetingTagCollection.AddItem(new MeetingTagViewModel(meetingUserInfo.AccountId, OnEmployeeRemoveButtonClicked));
			}
			_numberOfParticipants = _meetingTagCollection.CollectionCount;
		}

		private void AddMeetingTagElement(MemberIdType memberId)
		{
			_meetingTagCollection.AddItem(new MeetingTagViewModel(memberId, OnEmployeeRemoveButtonClicked));
			_meetingAllUserMemberId.Add(memberId);
			_meetingBlockedMemberId.Add(memberId);

			var meetingTagCollectionClone = _meetingTagCollection.Clone();
			var sortedCollection = meetingTagCollectionClone?.OrderBy(x => x.EmployeeName).ToList();

			ResetMeetingTagCollection();

			foreach (var meetingTagViewModel in sortedCollection)
			{
				_meetingTagCollection.AddItem(meetingTagViewModel);
			}

			_numberOfParticipants = _meetingTagCollection.CollectionCount;

			RefreshViewParticipants();
		}


		private void RemoveMeetingTagElement(MeetingTagViewModel meetingTagViewModel)
		{
			if (!IsNewReservation)
			{
				_willDeleteEmployeeNoList.Add(meetingTagViewModel.MemberId);
			}

			_meetingTagCollection.RemoveItem(meetingTagViewModel);

			_numberOfParticipants = _meetingTagCollection.CollectionCount;

			_meetingBlockedMemberId.Remove(meetingTagViewModel.MemberId);
			_meetingAllUserMemberId.Remove(meetingTagViewModel.MemberId);
			RefreshViewParticipants();
		}


		private void ResetMeetingTagCollection()
		{
			MeetingTagCollection.Reset();

			_numberOfParticipants = 0;
		}


		private void RemoveEmployeeFromMeetingInfo()
		{
			if (_willDeleteEmployeeNoList.Count == 0)
			{
				return;
			}

			var meetingUserInfoClone = MeetingInfo.MeetingMembers.ToList();

			foreach (var memberId in _willDeleteEmployeeNoList)
			{
				foreach (var meetingUserInfo in meetingUserInfoClone)
				{
					if (memberId == meetingUserInfo.AccountId)
						MeetingInfo.MeetingMembers.TryRemove(meetingUserInfo);
				}
			}

			_willDeleteEmployeeNoList.Clear();
		}

		private MeetingTagViewModel GetMeetingTagElement(MemberIdType memberId) => MeetingTagCollection.Value?.FirstOrDefault(meetingTagViewModel => meetingTagViewModel.MemberId == memberId);


		private void RefreshViewParticipants()
		{
			IsVisibleParticipantsText = _numberOfParticipants > 0 && !IsNewReservation;

			ParticipantsText = Localization.Instance.GetString("UI_ConnectingApp_Detail_Setting_AdditionCount_Text",
			                                                   MeetingReservationProvider.MaxNumberOfParticipants - _meetingAllUserMemberId.Count - 1);

			ParticipantsDetailText = Localization.Instance.GetString("UI_MeetingAppReserve_Desc_Wait",
			                                                         (_numberOfParticipants + 1).ToString());
		}
	}
}
