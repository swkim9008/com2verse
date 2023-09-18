/*===============================================================
* Product:		Com2Verse
* File Name:	UserFunctionViewModel.cs
* Developer:	eugene9721
* Date:			2022-10-07 15:58
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Chat;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.PlayerControl;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
    [ViewModelGroup("CommonUI")]
    public sealed class UserFunctionViewModel : ViewModel
    {
#region Variables
        private long _ownerId;
        private bool _isOnUserMenuUI;
        private int  _userState; // TODO: 임시 데이터, 이후 아바타별 상태로 변경

        private bool _isOnAnotherUserUiOnCommunication;

        private long _selectedUserIdOnCommunication;

        [UsedImplicitly] public CommandHandler       OpenUserStateUI { get; }
        [UsedImplicitly] public CommandHandler       OpenChattingUI  { get; }
        [UsedImplicitly] public CommandHandler       OpenGestureUI   { get; }
        [UsedImplicitly] public CommandHandler       OpenOptionUI    { get; }
        [UsedImplicitly] public CommandHandler<bool> OpenUserMenuUI  { get; }
        [UsedImplicitly] public CommandHandler<int>  SetUserState    { get; }

        // Another User Function
        [UsedImplicitly] public CommandHandler<long> FollowUserAsLeader  { get; }
        [UsedImplicitly] public CommandHandler<long> FollowMeRequest     { get; }
        [UsedImplicitly] public CommandHandler<long> OpenChattingWhisper { get; }
        [UsedImplicitly] public CommandHandler<long> UserInformation     { get; }

        // Another User Function On Communication
        [UsedImplicitly] public CommandHandler FollowUserAsLeaderOnCommunication  { get; }
        [UsedImplicitly] public CommandHandler FollowMeRequestOnCommunication     { get; }
        [UsedImplicitly] public CommandHandler OpenChattingWhisperOnCommunication { get; }
        [UsedImplicitly] public CommandHandler UserInformationOnCommunication     { get; }

        [UsedImplicitly] public CommandHandler<long> AnotherUserUiOnCommunication      { get; }
        [UsedImplicitly] public CommandHandler       CloseAnotherUserUiOnCommunication { get; }
#endregion Variables

#region Properties
        [UsedImplicitly]
        public long OwnerId
        {
            get => _ownerId;
            set
            {
                _ownerId = value;
                InvokePropertyValueChanged(nameof(OwnerId), OwnerId);
            }
        }

        [UsedImplicitly]
        public bool IsOnAvatarInfoUI
        {
            get
            {
                if (!User.InstanceExists) return false;
                var userCharacter = User.Instance.CharacterObject;
                if (userCharacter.IsUnityNull()) return false;

                // 회의실인 경우 아바타 UI 비활성화
                if (CurrentScene.CommunicationType is eSpaceOptionCommunication.MEETING) return false;

                return true;
            }
        }

        [UsedImplicitly]
        public bool IsOnUserMenuUI
        {
            get => _isOnUserMenuUI;
            set
            {
                if (_isOnUserMenuUI == value) return;
                _isOnUserMenuUI = value;
                InvokePropertyValueChanged(nameof(IsOnUserMenuUI), IsOnUserMenuUI);
            }
        }

        [UsedImplicitly]
        public int UserState
        {
            get => _userState;
            set
            {
                if (_userState == value) return;
                _userState = value;
                InvokePropertyValueChanged(nameof(UserState), UserState);
            }
        }

        [UsedImplicitly]
        public bool IsOnAnotherUserUiOnCommunication
        {
            get => _isOnAnotherUserUiOnCommunication;
            set
            {
                if (_isOnAnotherUserUiOnCommunication == value) return;
                _isOnAnotherUserUiOnCommunication = value;

                InvokePropertyValueChanged(nameof(IsOnAnotherUserUiOnCommunication), IsOnAnotherUserUiOnCommunication);

                if (value) return;
                _selectedUserIdOnCommunication = 0;
                if (User.InstanceExists) User.Instance.UserFunctionUI?.OffAnotherUserMenuUI();
            }
        }

        [UsedImplicitly]
        public string SelectedUserNameOnCommunication
        {
            get
            {
                if (_selectedUserIdOnCommunication == 0) return string.Empty;

                var activeObjectManagerViewModel = ViewModelManager.Instance.Get<ActiveObjectManagerViewModel>();
                if (activeObjectManagerViewModel == null) return string.Empty;

                if (activeObjectManagerViewModel.TryGet(_selectedUserIdOnCommunication, out var activeObjectViewModel))
                    return activeObjectViewModel.Name;

                return string.Empty;
            }
        }
#endregion Properties

        public UserFunctionViewModel()
        {
            OpenUserStateUI = new CommandHandler(OnOpenUserStateUI);
            OpenChattingUI  = new CommandHandler(OnOpenChattingUI);
            OpenGestureUI   = new CommandHandler(OnOpenGestureUI);
            OpenOptionUI    = new CommandHandler(OnOpenOptionUI);

            OpenUserMenuUI = new CommandHandler<bool>(OnOpenUserMenuUI);
            SetUserState   = new CommandHandler<int>(OnSetUserState);

            FollowUserAsLeader  = new CommandHandler<long>(OnFollowUserAsLeader);
            FollowMeRequest     = new CommandHandler<long>(OnFollowMeRequest);
            OpenChattingWhisper = new CommandHandler<long>(OnOpenChattingWhisper);
            UserInformation     = new CommandHandler<long>(OnUserInformation);

            FollowUserAsLeaderOnCommunication  = new CommandHandler(OnFollowUserAsLeaderOnCommunication);
            FollowMeRequestOnCommunication     = new CommandHandler(OnFollowMeRequestOnCommunication);
            OpenChattingWhisperOnCommunication = new CommandHandler(OnOpenChattingWhisperOnCommunication);
            UserInformationOnCommunication     = new CommandHandler(OnUserInformationOnCommunication);

            AnotherUserUiOnCommunication      = new CommandHandler<long>(OnAnotherUserUiOnCommunication);
            CloseAnotherUserUiOnCommunication = new CommandHandler(OnCloseAnotherUserUiOnCommunication);
        }

        private void OffAnotherUserMenuUI(long ownerId)
        {
            var activeObjectManagerViewModel = ViewModelManager.Instance.Get<ActiveObjectManagerViewModel>();
            if (activeObjectManagerViewModel == null) return;
            if (activeObjectManagerViewModel.TryGet(ownerId, out var activeObjectViewModel))
                activeObjectViewModel.IsOnAnotherUserMenuUI = false;
        }

        public void RefreshAvatarInfoUI()
        {
            InvokePropertyValueChanged(nameof(IsOnAvatarInfoUI), IsOnAvatarInfoUI);
        }

#region CommandHandler Method
        private void OnOpenUserStateUI()
        {
            IsOnUserMenuUI = false;

            //TODO: 유저 상태 UI 열기
            C2VDebug.LogCategory(nameof(UserFunctionViewModel), "사용자 기능 - 따라가기");
        }

        private void OnOpenChattingUI()
        {
            IsOnUserMenuUI = false;

            ChatManager.Instance.OpenChatUI();
            C2VDebug.LogCategory(nameof(UserFunctionViewModel), "사용자 기능 - 채팅");
        }

        private void OnOpenGestureUI()
        {
            IsOnUserMenuUI = false;

            var playerController = PlayerController.InstanceOrNull;
            if (!playerController.IsUnityNull())
                playerController!.GestureHelper.OpenEmotionUI();
            C2VDebug.LogCategory(nameof(UserFunctionViewModel), "사용자 기능 - 제스처");
        }

        private void OnOpenOptionUI()
        {
            IsOnUserMenuUI = false;

            UIManager.Instance.CreatePopup("UI_Popup_Option", (guiView) => guiView.Show()).Forget();
            C2VDebug.LogCategory(nameof(UserFunctionViewModel), "사용자 기능 - 옵션");
        }

        private void OnOpenUserMenuUI(bool isOpen)
        {
            IsOnUserMenuUI                   = isOpen;
            IsOnAnotherUserUiOnCommunication = false;
        }

        private void OnSetUserState(int userState)
        {
            UserState = userState;
        }

        private void OnFollowUserAsLeader(long ownerId)
        {
            if (ownerId == 0) return;
            OffAnotherUserMenuUI(ownerId);

            //TODO: 따라가기
            C2VDebug.LogCategory(nameof(UserFunctionViewModel), $"{ownerId.ToString()} 따라가기");
        }

        private void OnFollowMeRequest(long ownerId)
        {
            if (ownerId == 0) return;
            OffAnotherUserMenuUI(ownerId);

            //TODO: 따라오기
            C2VDebug.LogCategory(nameof(UserFunctionViewModel), $"{ownerId.ToString()} 따라오기");
        }

        private void OnOpenChattingWhisper(long ownerId)
        {
            if (ownerId == 0) return;
            OffAnotherUserMenuUI(ownerId);

            //TODO: 1:1 채팅
            // ChatManager.Instance.OpenWhisperChat(ownerId);
            C2VDebug.LogCategory(nameof(UserFunctionViewModel), $"{ownerId.ToString()} 1:1 채팅");
        }

        private void OnUserInformation(long ownerId)
        {
            if (ownerId == 0) return;
            OffAnotherUserMenuUI(ownerId);

            //TODO: 정보
            C2VDebug.LogCategory(nameof(UserFunctionViewModel), $"{ownerId.ToString()} 정보");
        }

        private void OnFollowUserAsLeaderOnCommunication()
        {
            OnFollowUserAsLeader(_selectedUserIdOnCommunication);
            IsOnAnotherUserUiOnCommunication = false;
        }

        private void OnFollowMeRequestOnCommunication()
        {
            OnFollowMeRequest(_selectedUserIdOnCommunication);
            IsOnAnotherUserUiOnCommunication = false;
        }

        private void OnOpenChattingWhisperOnCommunication()
        {
            OnOpenChattingWhisper(_selectedUserIdOnCommunication);
            IsOnAnotherUserUiOnCommunication = false;
        }

        private void OnUserInformationOnCommunication()
        {
            OnUserInformation(_selectedUserIdOnCommunication);
            IsOnAnotherUserUiOnCommunication = false;
        }

        private void OnAnotherUserUiOnCommunication(long ownerId)
        {
            if (User.InstanceExists) User.Instance.UserFunctionUI?.OffAnotherUserMenuUI();
            IsOnUserMenuUI                   = false;
            IsOnAnotherUserUiOnCommunication = true;
            _selectedUserIdOnCommunication   = ownerId;
            InvokePropertyValueChanged(nameof(SelectedUserNameOnCommunication), SelectedUserNameOnCommunication);
        }

        private void OnCloseAnotherUserUiOnCommunication()
        {
            IsOnAnotherUserUiOnCommunication = false;
        }
#endregion CommandHandler Method
    }
}
