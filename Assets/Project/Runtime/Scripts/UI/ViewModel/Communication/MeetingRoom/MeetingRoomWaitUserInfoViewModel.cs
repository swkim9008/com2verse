/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomWaitUserViewModel.cs
* Developer:	ksw
* Date:			2023-04-18 11:29
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.UI;
using Protocols.OfficeMeeting;
using AttendanceCode = Com2Verse.WebApi.Service.Components.AttendanceCode;
using MemberIdType = System.Int64;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;

namespace Com2Verse
{
    public class MeetingRoomWaitUserInfoModel : DataModel
    {
        public readonly Collection<MeetingRoomWaitUserViewModel> WaitInviteAcceptGroupCollection = new();
        public readonly Collection<MeetingRoomWaitUserViewModel> NotJoinedGroupCollection        = new();

        public void AddUser(MemberIdType memberId, AttendanceCode attendanceCode, Action refreshUserList)
        {
            var groupCollection = GetGroupCollection(attendanceCode);
            if (groupCollection == null)
                return;

            var memberModel = DataManager.Instance.GetMember(memberId);

            if (memberModel == null)
                return;

            var meetingRoomUserViewModel = new MeetingRoomWaitUserViewModel(memberModel, attendanceCode);
            meetingRoomUserViewModel.RefreshList += refreshUserList;

            groupCollection.AddItem(meetingRoomUserViewModel);
        }

        public Collection<MeetingRoomWaitUserViewModel> GetGroupCollection(AttendanceCode attendanceCode)
        {
            switch (attendanceCode)
            {
                case AttendanceCode.Join:
                    return NotJoinedGroupCollection;
                case AttendanceCode.JoinRequest:
                case AttendanceCode.JoinReceive:
                    return WaitInviteAcceptGroupCollection;
            }

            return null;
        }

        public void ResetGroupCollection(Collection<MeetingRoomWaitUserViewModel> groupCollection) => groupCollection.Reset();
    }

    [ViewModelGroup("Communication")]
    public sealed class MeetingRoomWaitUserInfoViewModel : ViewModelDataBase<MeetingRoomWaitUserInfoModel>, IDisposable
    {
        private string _waitInviteAcceptGroupTitle;
        private string _notJoinedGroupTitle;
        private string _allWaitingUserCountTitle;
        private bool   _isOpenWaitLayout;

        public Collection<MeetingRoomWaitUserViewModel> WaitInviteAcceptGroupCollection => base.Model.WaitInviteAcceptGroupCollection;

        public Collection<MeetingRoomWaitUserViewModel> NotJoinedGroupCollection => base.Model.NotJoinedGroupCollection;

        public string WaitInviteAcceptGroupTitle
        {
            get => _waitInviteAcceptGroupTitle;
            set => SetProperty(ref _waitInviteAcceptGroupTitle, value);
        }

        public string NotJoinedGroupTitle
        {
            get => _notJoinedGroupTitle;
            set => SetProperty(ref _notJoinedGroupTitle, value);
        }

        public string AllWaitingUserCountTitle
        {
            get => _allWaitingUserCountTitle;
            set => SetProperty(ref _allWaitingUserCountTitle, value);
        }

        public MeetingRoomWaitUserInfoViewModel()
        {
            MeetingReservationProvider.OnMeetingInfoChanged += MeetingInfoChanged;

            RefreshUsers();
        }

        public void Dispose()
        {
            MeetingReservationProvider.OnMeetingInfoChanged -= MeetingInfoChanged;
        }

        private void RefreshUsers()
        {
            Clear();

            var meetingInfo = MeetingReservationProvider.EnteredMeetingInfo;
            if (meetingInfo != null)
            {
                foreach (var userInfo in meetingInfo.MeetingMembers)
                {

                    if (userInfo.AttendanceCode == AttendanceCode.Join)
                    {
                        if (userInfo.IsEnter)
                            continue;
                        if (userInfo.AccountId == User.Instance.CurrentUserData.ID)
                            continue;
                    }

                    base.Model.AddUser(userInfo.AccountId, userInfo.AttendanceCode, RefreshUsers);
                }
            }

            SetTitleString();
        }

        private void Clear()
        {
            base.Model?.ResetGroupCollection(WaitInviteAcceptGroupCollection);
            base.Model?.ResetGroupCollection(NotJoinedGroupCollection);
        }

        private void SetTitleString()
        {
            WaitInviteAcceptGroupTitle = Localization.Instance.GetString("UI_MeetingRoom_UserList_Invitation_Invitation_Count_Text", WaitInviteAcceptGroupCollection.CollectionCount);
            NotJoinedGroupTitle        = Localization.Instance.GetString("UI_MeetingRoom_UserList_Invitation_Offline_Count_Text", NotJoinedGroupCollection.CollectionCount);
            AllWaitingUserCountTitle   = Localization.Instance.GetString("UI_MeetingRoom_UserList_Waiting_Count_Text",
                                                                         WaitInviteAcceptGroupCollection.CollectionCount + NotJoinedGroupCollection.CollectionCount);
        }

        private void MeetingInfoChanged()
        {
            RefreshUsers();
        }

        public override void OnLanguageChanged()
        {
            base.OnLanguageChanged();
            SetTitleString();
        }
    }
}
