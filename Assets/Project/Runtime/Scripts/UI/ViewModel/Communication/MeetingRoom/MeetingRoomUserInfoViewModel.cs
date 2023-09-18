/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomUserInfoViewModel.cs
* Developer:	tlghks1009
* Date:			2022-11-08 15:06
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Linq;
using System.Threading;
using Com2Verse.AvatarAnimation;
using Com2Verse.Chat;
using Com2Verse.Communication;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Protocols.OfficeMeeting;
using MeetingUserType = Com2Verse.WebApi.Service.Components.MeetingMemberEntity;
using AuthorityCode = Com2Verse.WebApi.Service.Components.AuthorityCode;
using ResponseMeetingInfo = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityResponseFormat>;
using User = Com2Verse.Network.User;

namespace Com2Verse.UI
{
    public class MeetingRoomUserInfoModel : DataModel
    {
        public readonly Collection<MeetingRoomUserViewModel> Collection = new();

        public readonly Collection<MeetingRoomUserViewModel> OrganizerGroupCollection = new();
        public readonly Collection<MeetingRoomUserViewModel> PresenterGroupCollection = new();
        public readonly Collection<MeetingRoomUserViewModel> MemberGroupCollection = new();

        public Collection<MeetingRoomUserViewModel> GuestGroupCollection => MemberGroupCollection;

        public void AddUser(Collection<MeetingRoomUserViewModel> groupCollection, CommunicationUserViewModel user)
        {
            var meetingRoomUserViewModel = new MeetingRoomUserViewModel(user);

            meetingRoomUserViewModel.OnClickProfile         += OnClickProfile;
            meetingRoomUserViewModel.OnClickAuthorityChange += RequestAuthorityChange;

            Collection.AddItem(meetingRoomUserViewModel);
            groupCollection.AddItem(meetingRoomUserViewModel);
        }

        public void RemoveUser(Collection<MeetingRoomUserViewModel> groupCollection, CommunicationUserViewModel user)
        {
            foreach (var meetingRoomUserViewModel in groupCollection.Value)
            {
                if (meetingRoomUserViewModel.UserViewModel == user)
                {
                    meetingRoomUserViewModel.OnClickProfile         -= OnClickProfile;
                    meetingRoomUserViewModel.OnClickAuthorityChange -= RequestAuthorityChange;

                    Collection.RemoveItem(meetingRoomUserViewModel);
                    groupCollection.RemoveItem(meetingRoomUserViewModel);
                    break;
                }
            }
        }

        public void ChangeUser(Collection<MeetingRoomUserViewModel> oldGroupCollection, Collection<MeetingRoomUserViewModel> newGroupCollection, CommunicationUserViewModel user)
        {
            foreach (var viewModel in oldGroupCollection.Value)
            {
                if (viewModel.UserViewModel == user)
                {
                    oldGroupCollection.RemoveItem(viewModel);
                    break;
                }
            }

            foreach (var viewModel in Collection.Value)
            {
                if (viewModel.UserViewModel.UserId == user.UserId)
                {
                    newGroupCollection.AddItem(viewModel);
                    viewModel.RefreshPermissionView();
                    break;
                }
            }
        }

        public void AuthorityChange(long accountId, AuthorityCode oldAuthorityCode, AuthorityCode newAuthorityCode)
        {
            var userManager = ViewModelManager.InstanceOrNull?.Get<CommunicationUserManagerViewModel>();

            foreach (var item in userManager.ViewModelMap)
            {
                if (item.Key == accountId)
                {
                    var oldGroupCollection = GetGroupCollection(oldAuthorityCode);
                    var newGroupCollection = GetGroupCollection(newAuthorityCode);

                    ChangeUser(oldGroupCollection, newGroupCollection, item.Value);
                    break;
                }
            }
        }

        public void ResetGroupCollection(Collection<MeetingRoomUserViewModel> groupCollection) => groupCollection.Reset();

        private void OnClickProfile(CommunicationUserViewModel user)
        {
            //var employeePayload = user.OrganizationUserViewModel.MemberModel;
            var employeePayload = DataManager.Instance.GetMember(user.UserId);
            UIManager.Instance.CreatePopup("UI_Connecting_UserProfilePopup", guiView =>
            {
                guiView.Show();
                var viewModel = guiView.ViewModelContainer.GetViewModel<MeetingRoomProfileViewModel>();
                viewModel.GUIView = guiView;
                if (employeePayload == null)
                {
                    viewModel.SetGuestProfile(user.UserName);
                }
                else
                {
                    viewModel.SetProfile(employeePayload).Forget();
                }
            }).Forget();
        }

        private void RequestAuthorityChange(long accountId, AuthorityCode oldAuthorityCode, AuthorityCode newAuthorityCode)
        {
            var userInfo = GetMeetingUserInfo(accountId);
            if (userInfo != null)
            {
                userInfo.AuthorityCode = newAuthorityCode;
                Commander.Instance.RequestMeetingAuthorityChangeAsync(MeetingReservationProvider.EnteredMeetingInfo.MeetingId, userInfo, response =>
                {
                    var data = new ChatManager.AuthorityChangedNotifyData
                    {
                        ChangedUser  = accountId,
                        OldAuthority = (int)oldAuthorityCode,
                        NewAuthority = (int)newAuthorityCode,
                    };
                    ChatManager.Instance.BroadcastCustomData(ChatManager.CustomDataType.AUTHORITY_CHANGE_NOTIFY, data);
                }, error => { }).Forget();
            }
        }

        private MeetingUserType GetMeetingUserInfo(long accountId)
        {
            var currentMeetingInfo = MeetingReservationProvider.EnteredMeetingInfo;

            return currentMeetingInfo?.MeetingMembers?.FirstOrDefault(meetingUserInfo => meetingUserInfo?.AccountId == accountId);
        }

        public void SetHandsUpState(bool isHandsUp, long accountId)
        {
            foreach (var viewModel in Collection.Value)
            {
                if (viewModel.UserViewModel.UserId == accountId)
                {
                    viewModel.IsEmotionActive = isHandsUp;
                    viewModel.IsHandsUp       = isHandsUp;
                    break;
                }
            }
        }

        public Collection<MeetingRoomUserViewModel> GetGroupCollection(AuthorityCode? authorityCode)
        {
            switch (authorityCode)
            {
                case AuthorityCode.Participant: return MemberGroupCollection;

                case AuthorityCode.Organizer: return OrganizerGroupCollection;

                case AuthorityCode.Presenter: return PresenterGroupCollection;
            }

            return GuestGroupCollection;
        }
    }

    [ViewModelGroup("Communication")]
    public sealed class MeetingRoomUserInfoViewModel : ViewModelDataBase<MeetingRoomUserInfoModel>, IDisposable
    {
        private string _organizerGroupTitle;
        private string _presenterGroupTitle;
        private string _memberGroupTitle;
        private string _allJoinedUserCountTitle;
        private bool   _isUserLayout;
        private bool   _isRequesting;
        private bool   _isRequested;

        private MeetingInfo _meetingInfo;

        private CancellationTokenSource _cts;

        public Collection<MeetingRoomUserViewModel> AllGroupCollection => base.Model.Collection;
        public Collection<MeetingRoomUserViewModel> OrganizerGroupCollection => base.Model.OrganizerGroupCollection;

        public Collection<MeetingRoomUserViewModel> PresenterGroupCollection => base.Model.PresenterGroupCollection;

        public Collection<MeetingRoomUserViewModel> MemberGroupCollection => base.Model.MemberGroupCollection;

        private Collection<MeetingRoomUserViewModel> GetGroupCollection(AuthorityCode? authorityCode) => base.Model.GetGroupCollection(authorityCode);

        [UsedImplicitly] public bool IsOrganizer => MeetingReservationProvider.IsOrganizer(User.Instance.CurrentUserData.ID);

        public string OrganizerGroupTitle
        {
            get => _organizerGroupTitle;
            set => SetProperty(ref _organizerGroupTitle, value);
        }

        public string PresenterGroupTitle
        {
            get => _presenterGroupTitle;
            set => SetProperty(ref _presenterGroupTitle, value);
        }

        public string MemberGroupTitle
        {
            get => _memberGroupTitle;
            set => SetProperty(ref _memberGroupTitle, value);
        }

        public string AllJoinedUserCountTitle
        {
            get => _allJoinedUserCountTitle;
            set => SetProperty(ref _allJoinedUserCountTitle, value);
        }

        public bool IsUserLayout
        {
            get => _isUserLayout;
            set => _isUserLayout = value;
        }

        public MeetingRoomUserInfoViewModel()
        {
            MeetingReservationProvider.OnMeetingInfoChanged += OnMeetingInfoChanged;

            var userManager = ViewModelManager.Instance.GetOrAdd<CommunicationUserManagerViewModel>();
            userManager.ViewModelAdded             += OnUserViewModelAdded;
            userManager.ViewModelRemoved           += OnUserViewModelRemoved;

            ChatManager.Instance.OnHandsUpNotify          += OnHandsUpNotify;
            ChatManager.Instance.OnHandsDownNotify        += OnHandsDownNotify;
            ChatManager.Instance.OnForcedOutNotify        += OnForcedOutNotify;
            ChatManager.Instance.OnHandsStateRequest      += OnHandsStateRequest;
            ChatManager.Instance.OnAuthorityChangedNotify += OnAuthorityChangedNotify;
            ChatManager.Instance.OnRecordStartNotify      += OnRecordStartNotify;
            ChatManager.Instance.OnRecordEndNotify        += OnRecordEndNotify;
            SetTitleString();

            RefreshUsers();
            User.Instance.OnTeleportCompletion += WaitTeleportComplete;
        }

        private void OnMeetingInfoChanged()
        {
            InvokePropertyValueChanged(nameof(IsOrganizer), IsOrganizer);
        }

        private void WaitTeleportComplete()
        {
            User.Instance.OnTeleportCompletion -= WaitTeleportComplete;
            var activeObject = MapController.Instance.GetObjectByUserID(User.Instance.CurrentUserData.ID) as ActiveObject;
            if (!activeObject.IsUnityNull() && !activeObject.BaseBodyTransform.IsUnityNull())
            {
                var animatorController = activeObject.BaseBodyTransform.GetComponent<AvatarAnimatorController>();
                if (!animatorController.IsUnityNull())
                {
                    animatorController.IsOnHandsUpState = false;
                }
            }
            ChatManager.Instance.BroadcastCustomNotify(ChatManager.CustomDataType.HANDS_STATE_REQUEST);
        }

        public void Dispose()
        {
            MeetingReservationProvider.OnMeetingInfoChanged -= OnMeetingInfoChanged;

            var userManager = ViewModelManager.InstanceOrNull?.Get<CommunicationUserManagerViewModel>();
            if (userManager != null)
            {
                userManager.ViewModelAdded   -= OnUserViewModelAdded;
                userManager.ViewModelRemoved -= OnUserViewModelRemoved;
            }

            if (ChatManager.InstanceExists)
            {
                ChatManager.Instance.OnForcedOutNotify        -= OnForcedOutNotify;
                ChatManager.Instance.OnHandsUpNotify          -= OnHandsUpNotify;
                ChatManager.Instance.OnHandsDownNotify        -= OnHandsDownNotify;
                ChatManager.Instance.OnHandsStateRequest      -= OnHandsStateRequest;
                ChatManager.Instance.OnAuthorityChangedNotify -= OnAuthorityChangedNotify;
                ChatManager.Instance.OnRecordStartNotify      -= OnRecordStartNotify;
                ChatManager.Instance.OnRecordEndNotify        -= OnRecordEndNotify;
            }

            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        private void RefreshUsers()
        {
            Clear();

            var userManager = ViewModelManager.Instance.Get<CommunicationUserManagerViewModel>();
            if (userManager == null)
                return;

            foreach (var item in userManager.ViewModelMap)
            {
                AddUser(item.Value!).Forget();
            }
        }

        private void Clear()
        {
            base.Model?.ResetGroupCollection(OrganizerGroupCollection);
            base.Model?.ResetGroupCollection(PresenterGroupCollection);
            base.Model?.ResetGroupCollection(MemberGroupCollection);
        }

        private void OnUserViewModelAdded(Uid uid, CommunicationUserViewModel user)
        {
            AddUser(user).Forget();
        }

        private void OnUserViewModelRemoved(Uid uid, CommunicationUserViewModel user)
        {
            RemoveUser(user);
        }

        private async UniTask AddUser(CommunicationUserViewModel user)
        {
            if (user?.Value == null)
                return;
            
            var meetingUserInfo = GetMeetingUserInfo(user.UserId);
            // 게스트 유저나 신규 초대 유저는 List에 없을 수 있으므로 MeetingInfo 다시 셋팅
            if (meetingUserInfo == null)
            {
                if (!await GetMeetingInfo())
                    return;
                meetingUserInfo = GetMeetingUserInfo(user.UserId);
                // 커넥팅에 속해있지 않은 유저
                if (meetingUserInfo == null)
                {
                    C2VDebug.LogError("Not Include Meeting User : " + user.UserId);
                    return;
                }
            }
            // 참여한 유저인데 IsEnter가 false면 최신 MeetingInfo 업데이트
            if (!meetingUserInfo.IsEnter)
            {
                if (!await GetMeetingInfo())
                    return;
            }
            var groupCollection = GetGroupCollection(meetingUserInfo?.AuthorityCode);
            if (groupCollection == null)
            {
                return;
            }

            base.Model.AddUser(groupCollection, user);
            SetTitleString();
        }


        private void RemoveUser(CommunicationUserViewModel user)
        {
            if (user?.Value == null)
                return;

            var meetingUserInfo = GetMeetingUserInfo(user.UserId);
            var groupCollection = GetGroupCollection(meetingUserInfo?.AuthorityCode);
            if (groupCollection == null)
            {
                return;
            }

            base.Model.RemoveUser(groupCollection, user);
            SetTitleString();
            GetMeetingInfo().Forget();
        }

        private async UniTask<bool> GetMeetingInfo()
        {
            if (_isRequesting)
            {
                return await UniTaskHelper.WaitUntil(() => _isRequested == true, _cts);
            }
            _cts ??= new CancellationTokenSource();
            RequestMeetingInfo();
            if (!await UniTaskHelper.WaitUntil(() => _isRequesting == false, _cts))
                return false;
            return true;
        }

        private MeetingUserType GetMeetingUserInfo(Uid uid)
        {
            var currentMeetingInfo = MeetingReservationProvider.EnteredMeetingInfo;
            return currentMeetingInfo?.MeetingMembers?.FirstOrDefault(meetingUserInfo => meetingUserInfo?.AccountId == uid);
        }

        private void SetTitleString()
        {
            OrganizerGroupTitle = Localization.InstanceOrNull?.GetString("UI_MeetingRoom_UserList_Organizer_Count_Text",   OrganizerGroupCollection.CollectionCount);
            PresenterGroupTitle = Localization.InstanceOrNull?.GetString("UI_MeetingRoom_UserList_Presenter_Count_Text",   PresenterGroupCollection.CollectionCount);
            MemberGroupTitle    = Localization.InstanceOrNull?.GetString("UI_MeetingRoom_UserList_Participant_Count_Text", MemberGroupCollection.CollectionCount);
            AllJoinedUserCountTitle = Localization.InstanceOrNull?.GetString("UI_MeetingRoom_UserList_Online_Count_Text",
                                                                             OrganizerGroupCollection.CollectionCount + PresenterGroupCollection.CollectionCount + MemberGroupCollection.CollectionCount);
        }

        private void RequestMeetingInfo()
        {
            if (MeetingReservationProvider.EnteredMeetingInfo == null)
            {
                C2VDebug.LogErrorMethod(nameof(MeetingRoomExitViewModel), "EnteredMeetingInfo is null.");
                return;
            }

            if (_isRequesting)
                return;

            _isRequesting = true;
            _isRequested  = false;
            Commander.InstanceOrNull?.RequestMeetingInfoAsync(MeetingReservationProvider.EnteredMeetingInfo.MeetingId, OnResponseMeetingInfo, error =>
            {
                _isRequesting = false;
                _isRequested  = true;
            }).Forget();
        }

        private void OnResponseMeetingInfo(ResponseMeetingInfo response)
        {
            // TODO : NEW_ORGANIZATION WEB API로 대체 (RequestMeetingInfo)
            MeetingReservationProvider.SetMeetingInfo(response.Value.Data);
            _isRequesting = false;
            _isRequested = true;
        }

        private void OnForcedOutNotify(long sender, long target)
        {
            C2VDebug.Log($"OnForcedOutNotify: {sender} -> {target}");
        }

        private void OnHandsUpNotify(long accountId)
        {
            SetHandsUpState(accountId, true);
            base.Model.SetHandsUpState(true, accountId);
        }

        private void OnHandsDownNotify(long accountId)
        {
            SetHandsUpState(accountId, false);
            base.Model.SetHandsUpState(false, accountId);
        }

        private void SetHandsUpState(long accountId, bool isHandsUp)
        {
            var activeObject = MapController.Instance.GetObjectByUserID(accountId) as ActiveObject;
            if (!activeObject.IsUnityNull() && !activeObject!.BaseBodyTransform.IsUnityNull())
            {
                var avatarAnimatorController = activeObject!.BaseBodyTransform!.GetComponent<AvatarAnimatorController>();
                if (!avatarAnimatorController.IsUnityNull())
                {
                    avatarAnimatorController!.IsOnHandsUpState = isHandsUp;
                    ChatManager.Instance.SetHandsUpEmoticon(accountId, isHandsUp);
                }
            }
        }

        private void OnHandsStateRequest()
        {
            var activeObject = MapController.Instance.GetObjectByUserID(User.Instance.CurrentUserData.ID) as ActiveObject;
            if (!activeObject.IsUnityNull() && !activeObject.BaseBodyTransform.IsUnityNull())
            {
                var animatorController = activeObject.BaseBodyTransform.GetComponent<AvatarAnimatorController>();
                if (!animatorController.IsUnityNull())
                {
                    ChatManager.Instance.BroadcastCustomNotify(
                        animatorController.IsOnHandsUpState
                            ? ChatManager.CustomDataType.HANDS_UP
                            : ChatManager.CustomDataType.HANDS_DOWN);
                }
            }
        }

        private async void OnAuthorityChangedNotify(long accountId, int oldAuthority, int newAuthority)
        {
            C2VDebug.Log($"OnAuthorityChangedNotify : {accountId}  {(AuthorityCode)oldAuthority}  {(AuthorityCode)newAuthority}");
            await GetMeetingInfo();
            base.Model.AuthorityChange(accountId, (AuthorityCode)oldAuthority, (AuthorityCode)newAuthority);
            SetTitleString();
            if (accountId == User.Instance.CurrentUserData.ID)
            {
                switch (newAuthority)
                {
                    case (int)AuthorityCode.Presenter:
                        UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_UserList_GotPresenterAuthor_Toast"));
                        break;
                    case (int)AuthorityCode.Participant:
                        UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_UserList_GotParticipantAuthor_Toast"));
                        break;
                }
            }
        }

        private void OnRecordStartNotify()
        {
            UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_OtherMenu_Stt_RecordingStarted_Toast"));
        }

        private void OnRecordEndNotify()
        {
            UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_OtherMenu_Stt_RecordingStopped_Toast"));
        }

        public override void OnLanguageChanged()
        {
            base.OnLanguageChanged();
            SetTitleString();
        }
    }
}
