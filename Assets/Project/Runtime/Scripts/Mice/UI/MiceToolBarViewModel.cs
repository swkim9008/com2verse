/*===============================================================
* Product:		Com2Verse
* File Name:	MiceToolBarViewModel.cs
* Developer:	seaman2000
* Date:			2023-05-31 15:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/
using Com2Verse.Mice;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Notification;
using Com2Verse.PlayerControl;
using JetBrains.Annotations;
using Com2Verse.Communication.Unity;
using UnityEngine;

namespace Com2Verse.UI
{
    [ViewModelGroup("Mice")]
    public sealed partial class MiceToolBarViewModel : ViewModelBase, IDisposable
    {
        public CommandHandler GoBack     { get; private set; }

        [UsedImplicitly] public CommandHandler<bool> SetMicLayout { get; }
        [UsedImplicitly] public CommandHandler<bool> SetCameraLayout { get; }

        public CommandHandler CloseCameraUI { get; private set; }
        public CommandHandler CloseChatUI { get; private set; }


        public CommandHandler      ToggleMicLayout       { get; private set; }
        public CommandHandler      ToggleCameraLayout    { get; private set; }
        public CommandHandler      ToggleEmotionLayout   { get; private set; }
        public CommandHandler      ToggleCameraChange    { get; private set; }
        public CommandHandler      ToggleChat            { get; private set; }
        public CommandHandler      ToggleSessionInfo     { get; private set; }
        public CommandHandler      ToggleParticipantList { get; private set; }
        public CommandHandler      ToggleNotification    { get; private set; }
        public CommandHandler      ToggleSetting         { get; private set; }
        public CommandHandler      ToggleDataDownload    { get; private set; }
        public CommandHandler      TogglePhotoShoot      { get; private set; }
        public CommandHandler      ToggleQuestionList    { get; private set; }
        public CommandHandler      ToggleAskQuestion     { get; private set; }

        public CommandHandler<int> ChangeCamera          { get; private set; }

        public CommandHandler ApplyStaffFahsionItem { get; private set; }

        private string _memberCount;
        private string _userName;

        private bool _isEmotionLayout;
        private bool _isMicLayout;
        private bool _isCameraLayout;

        private bool _isCameraChangeVisible;
        private bool _isChatVisible;
        private bool _isParticipantListVisible;
        private bool _isSessionInfoVisible;
        private bool _isNotificationVisible;
        private bool _isSettingVisible;
        private bool _isDataDownloadVisible;
        private bool _isPhotoShootVisible;

        private bool _isAskQuestionVisible;
        private bool _isQuestionListVisible;

        private bool _isClosingPopupLayouts;

        // 연사 프로필 작업
        private Texture _userProfileImage;
        private bool _hasMyProfileIcon;
        
        public string MemberCount
        {
            get => _memberCount;
            set => SetProperty(ref _memberCount, value);
        }
        public string UserName
        {
            get => User.Instance.CurrentUserData.UserName;
        }
       
        public bool IsEmotionLayout
        {
            get => _isEmotionLayout;
            set => UpdateProperty(ref _isEmotionLayout, value);
        }

        public bool IsMicLayout
        {
            get => _isMicLayout;
            set => UpdateProperty(ref _isMicLayout, value);
        }

        public bool IsCameraLayout
        {
            get => _isCameraLayout;
            set => UpdateProperty(ref _isCameraLayout, value);
        }

        public bool IsSessionInfoVisible
        {
            get => _isSessionInfoVisible;
            set => UpdateProperty(ref _isSessionInfoVisible, value);
        }

        public bool IsCameraChangeVisible
        {
            get => _isCameraChangeVisible;
            set => UpdateProperty(ref _isCameraChangeVisible, value);
        }

        public bool IsChatVisible
        {
            get => _isChatVisible;
            set => UpdateProperty(ref _isChatVisible, value);
        }

        public bool IsParticipantListVisible
        {
            get => _isParticipantListVisible;
            set => UpdateProperty(ref _isParticipantListVisible, value);
        }

        public bool IsNotificationVisible
        {
            get => _isNotificationVisible;
            set => UpdateProperty(ref _isNotificationVisible, value);
        }

        public bool IsSettingVisible
        {
            get => _isSettingVisible;
            set => UpdateProperty(ref _isSettingVisible, value);
        }

        public bool IsDataDownloadVisible
        {
            get => _isDataDownloadVisible;
            set => UpdateProperty(ref _isDataDownloadVisible, value);
        }

        public bool IsPhotoShootVisible
        {
            get => _isPhotoShootVisible;
            set => UpdateProperty(ref _isPhotoShootVisible, value);
        }

        public bool IsAskQuestionVisible
        {
            get => _isAskQuestionVisible;
            set => UpdateProperty(ref _isAskQuestionVisible, value);
        }

        public bool IsQuestionListVisible
        {
            get => _isQuestionListVisible;
            set => UpdateProperty(ref _isQuestionListVisible, value);
        }

        // 연사 프로필 관련 데이타
        public bool HasMyProfileIcon
        {
            get => _hasMyProfileIcon;
            set => UpdateProperty(ref _hasMyProfileIcon, value);
        }

        public Texture UserProfileImage
        {
            get => _userProfileImage;
            set => SetProperty(ref _userProfileImage, value);
        }

        // 사전 질문 목록 보기
        public bool IsShowEnableQuestionList 
        {
            get => MiceService.Instance.GetCurrentSessionInfo()?.IsUsePreliminaryQuestion ?? true;
        }

        // 실시간 질문하기
        public bool IsShowEnableAskQuestion 
        {
            // 세지포에서는 보이지 않게 해야한다.
            get => false;
        }

        public bool HasStaffAuthority   => MiceService.Instance.HasCurrentEventTicket(MiceWebClient.eMiceAuthorityCode.STAFF);
        public bool IsCameraView1       => CheckCamera(eMiceCameraJigKey.MICE_HALL_MAIN_SCREEN_VIEW);
        public bool IsCameraView2       => CheckCamera(eMiceCameraJigKey.MICE_HALL_ALL_SCREEN_VIEW);
        public bool IsCameraView3       => CheckCamera(eMiceCameraJigKey.MICE_HALL_LEFT_SCREEN_VIEW);
        public bool IsCameraView4       => CheckCamera(eMiceCameraJigKey.MICE_HALL_RIGHT_SCREEN_VIEW);
        public bool IsCameraView5       => CheckCamera(eMiceCameraJigKey.MICE_HALL_WIDE_VIEW);
        public bool IsSpeaker           => MiceService.Instance.CurrentUserAuthority == MiceWebClient.eMiceAuthorityCode.SPEAKER;
        public bool IsOperator          => MiceService.Instance.CurrentUserAuthority == MiceWebClient.eMiceAuthorityCode.OPERATOR;
        public bool MiceUIShouldVisible => MiceService.Instance.MiceUIShouldVisible();

        public MiceToolBarViewModel()
        {
            SetMicLayout = new CommandHandler<bool>(value => IsMicLayout = value);
            SetCameraLayout = new CommandHandler<bool>(value => IsCameraLayout = value);

            // command binder
            GoBack = new CommandHandler(OnClickGoBack);

            ToggleMicLayout = new CommandHandler(() =>
            {
                IsMicLayout ^= true;
                ModuleManager.Instance.Voice.IsRunning ^= true;
            });

            ToggleCameraLayout = new CommandHandler(() =>
            {
                IsCameraLayout ^= true;
                ModuleManager.Instance.Camera.IsRunning ^= true;
            });

            CloseCameraUI = new CommandHandler(() => IsCameraChangeVisible = false);
            CloseChatUI = new CommandHandler(() => IsChatVisible = false);

            ToggleEmotionLayout = new CommandHandler(() => IsEmotionLayout ^= true);
            ToggleCameraChange = new CommandHandler(() => IsCameraChangeVisible ^= true);
            ToggleChat = new CommandHandler(() => IsChatVisible ^= true);
            ToggleParticipantList = new CommandHandler(() => IsParticipantListVisible ^= true);
            ToggleSessionInfo = new CommandHandler(() => IsSessionInfoVisible ^= true);
            ToggleNotification = new CommandHandler(() => IsNotificationVisible ^= true);
            ToggleSetting = new CommandHandler(() => IsSettingVisible ^= true);
            ToggleDataDownload = new CommandHandler(() => IsDataDownloadVisible ^= true);
            TogglePhotoShoot = new CommandHandler(() => IsPhotoShootVisible ^= true);
            ToggleQuestionList = new CommandHandler(() => IsQuestionListVisible ^= true);
            ToggleAskQuestion = new CommandHandler(() => IsAskQuestionVisible ^= true);
            ChangeCamera = new CommandHandler<int>(OnChangeCamera);
            ApplyStaffFahsionItem = new CommandHandler(() => MiceService.Instance.ApplyStaffFashionItems());
            
            var playerController = PlayerController.InstanceOrNull;
            if (!playerController.IsReferenceNull() && playerController.GestureHelper != null)
            {
                playerController.GestureHelper.OnUIOpenedEvent += OnEmotionUIOpened;
                playerController.GestureHelper.OnUIClosedEvent += OnEmotionUIClosed;
            }

            MiceService.Instance.OnMiceStateChangedEvent += OnMiceServiceStateChanged;

            // 참가자 데이타 갱신
            MiceInfoManager.Instance.ParticipantInfo.OnDataChange += OnMiceParticipantDataChanged;
            MiceInfoManager.Instance.ParticipantInfo.Sync().Forget();

            var notificationManager = NotificationManager.InstanceOrNull;
            if (notificationManager != null)
            {
                NotificationManager.Instance.OnNotificationPopUpOpenedEvent += OnNotificaionOpend;
                NotificationManager.Instance.OnNotificationPopUpClosedEvent += OnNotificaionClosed;
            }

            // user thumbnail
            //this.HasMyProfileIcon = false;
            //InitThumbnail().Forget();

            //UpdateUI();
        }

        //async UniTask InitThumbnail()
        //{
        //    var userInfo = MiceInfoManager.Instance.MyUserInfo;
        //    if (userInfo == null) return;

        //    this.HasMyProfileIcon = !string.IsNullOrEmpty(userInfo.PhotoUrl);

        //    if (!this.HasMyProfileIcon)
        //    {
        //        await UniTask.Yield();
        //        UserProfileImage = default(Texture);
        //    }
        //    else
        //    {
        //        await TextureCache.Instance.GetOrDownloadTextureAsync(userInfo.PhotoUrl, (result, texture) => { UserProfileImage = result ? texture : default(Texture); });
        //    }
        //}


        public void Dispose()
        {
            var playerController = PlayerController.InstanceOrNull;
            if (!playerController.IsReferenceNull() && playerController.GestureHelper != null)
            {
                playerController.GestureHelper.OnUIOpenedEvent -= OnEmotionUIOpened;
                playerController.GestureHelper.OnUIClosedEvent -= OnEmotionUIClosed;
            }
            if (MiceService.InstanceExists)
                MiceService.Instance.OnMiceStateChangedEvent -= OnMiceServiceStateChanged;

            var notificationManager = NotificationManager.InstanceOrNull;
            if (notificationManager != null)
            {
                notificationManager.OnNotificationPopUpOpenedEvent -= OnNotificaionOpend;
                notificationManager.OnNotificationPopUpClosedEvent -= OnNotificaionClosed;
            }

            if (MiceInfoManager.InstanceOrNull?.ParticipantInfo != null)
            {
                MiceInfoManager.Instance.ParticipantInfo!.OnDataChange -= OnMiceParticipantDataChanged;
            }
        }

        private void OnNotificaionOpend() => IsNotificationVisible = true;
        private void OnNotificaionClosed() => IsNotificationVisible = false;

        private void OnEmotionUIOpened() => IsEmotionLayout = true;
        private void OnEmotionUIClosed() => IsEmotionLayout = false;

        private void UpdateProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "") where T : unmanaged, IConvertible
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return;

            SetProperty(ref storage, value, propertyName);

            ClosePopupLayoutsOnAny(propertyName);

            RefreshLayoutDependencies();
        }

        private void RefreshLayoutDependencies()
        {
            if (IsSessionInfoVisible)
                MiceService.Instance.ShowUIPopupSessionInfo(null, (view) => IsSessionInfoVisible = false).Forget();
            else
                MiceService.Instance.HideUISessionInfo();

            if (IsParticipantListVisible)
                MiceParticipantListManager.Instance.ShowAsConference();
            else
                MiceParticipantListManager.Instance.Hide();

            if (IsNotificationVisible)
                NotificationManager.Instance.NotificationPopUpOpen();
            else
                NotificationManager.Instance.HidePopUp();

            if (IsQuestionListVisible)
                MiceSessionQuestionListManager.Instance.Show();
            else
                MiceSessionQuestionListManager.Instance.Hide();

            if (IsAskQuestionVisible)
            {
                // TODO : 추후 network프로세서가 확인되면 추가 구현해야 함
                //bool isActiveMic = ModuleManager.Instance.Voice.IsRunning;
                bool hasAudioDevice = DeviceManager.Instance.AudioRecorder.Count > 0;
                hasAudioDevice = false;

                if (!hasAudioDevice)
                {
                    var title = Data.Localization.eKey.UI_Common_Notice_Popup_Title.ToLocalizationString();
                    var desc = Data.Localization.eKey.MICE_UI_SessionHall_Asking_CheckMIC_Popup_Desc.ToLocalizationString();
                    var ok = Data.Localization.eKey.MICE_UI_Common_Btn_Check.ToLocalizationString();
                    var cancel = Data.Localization.eKey.MICE_UI_Common_Btn_Cancel.ToLocalizationString();

                    UIManager.Instance.ShowPopupYesNo(title, desc,
                                  (_) => OnClick_SettingButton(),
                                  (_) => IsAskQuestionVisible = false,
                                  null, ok, cancel, false, false);

                    return;
                }
            }

            if (IsSettingVisible)
                MiceService.Instance.ShowUIPopupOption(null, (guiView) => { IsSettingVisible = false; }).Forget();
            else
                MiceService.Instance.HideUIPopupOption();

            if (IsDataDownloadVisible)
            {
                if (MiceService.Instance.HasDataDownloadFiles())
                {
                    MiceService.Instance.ShowUISessionDataDownload(null, (view) => IsDataDownloadVisible = false).Forget();
                }
                else
                {
                    var text = Data.Localization.eKey.MICE_UI_SessionHall_FileDownload_Msg_NoData.ToLocalizationString();
                    UIManager.Instance.SendToastMessage(text, 3f, UIManager.eToastMessageType.NORMAL);
                }
            }
            else
            {
                MiceService.Instance.HideUISessionDataDownload();
            }

            MiceUIConferencePhotoShootViewModel.ToggleView(IsPhotoShootVisible, () => IsPhotoShootVisible = false);

            var playerController = PlayerController.InstanceOrNull;
            if (!playerController.IsReferenceNull())
            {
                if (IsEmotionLayout)
                    playerController!.GestureHelper.OpenEmotionUI();
                else
                    playerController!.GestureHelper.CloseEmotionUI();
            }
        }

        private void ClosePopupLayoutsOnAny(string propertyName)
        {
            if (_isClosingPopupLayouts)
                return;

            _isClosingPopupLayouts = true;
            {
                if (propertyName != nameof(IsMicLayout)) IsMicLayout = false;
                if (propertyName != nameof(IsCameraLayout)) IsCameraLayout = false;

                if (propertyName != nameof(IsEmotionLayout)) IsEmotionLayout = false;
                if (propertyName != nameof(IsSessionInfoVisible)) IsSessionInfoVisible = false;
                if (propertyName != nameof(IsCameraChangeVisible)) IsCameraChangeVisible = false;
                if (propertyName != nameof(IsChatVisible)) IsChatVisible = false;
                if (propertyName != nameof(IsParticipantListVisible)) IsParticipantListVisible = false;
                if (propertyName != nameof(IsNotificationVisible)) IsNotificationVisible = false;
                if (propertyName != nameof(IsSettingVisible)) IsSettingVisible = false;
                if (propertyName != nameof(IsDataDownloadVisible)) IsDataDownloadVisible = false;
                if (propertyName != nameof(IsPhotoShootVisible)) IsPhotoShootVisible = false;
                if (propertyName != nameof(IsQuestionListVisible)) IsQuestionListVisible = false;
                if (propertyName != nameof(IsAskQuestionVisible)) IsAskQuestionVisible = false;

            }
            _isClosingPopupLayouts = false;
        }

        public void CloseAllUI()
        {
            ClosePopupLayoutsOnAny(string.Empty);
            RefreshLayoutDependencies();
        }

        void OnClickGoBack()
        {
            MiceService.Instance.ShowHallExitPopup();
        }

        private void OnClick_SettingButton()
        {
            MiceService.Instance.ShowUIPopupOption(null, (guiView)=>
            {
                IsAskQuestionVisible = false;
            }, true).Forget();
        }

        private void OnChangeCamera(int index)
        {
            MiceService.Instance.ChangeCamera(index);
            IsCameraChangeVisible = false;
            RefreshCameraView();
        }

        private bool CheckCamera(eMiceCameraJigKey key)
        {
            if (CameraManager.Instance.CurrentState == eCameraState.FIXED_CAMERA)
                return FixedCameraManager.Instance.CurrentJigKey == key.ToString();
            else
                return key == eMiceCameraJigKey.MICE_HALL_MAIN_SCREEN_VIEW;
        }

        private void RefreshCameraView()
        {
            InvokePropertyValueChanged(nameof(IsCameraView1), IsCameraView1);
            InvokePropertyValueChanged(nameof(IsCameraView2), IsCameraView2);
            InvokePropertyValueChanged(nameof(IsCameraView3), IsCameraView3);
            InvokePropertyValueChanged(nameof(IsCameraView4), IsCameraView4);
            InvokePropertyValueChanged(nameof(IsCameraView5), IsCameraView5);
        }

        private void OnMiceServiceStateChanged()
        {
            InvokePropertyValueChanged(nameof(IsSpeaker),           IsSpeaker);   
            InvokePropertyValueChanged(nameof(IsOperator),          IsOperator);
            InvokePropertyValueChanged(nameof(HasStaffAuthority),   HasStaffAuthority);
            InvokePropertyValueChanged(nameof(MiceUIShouldVisible), MiceUIShouldVisible);
        }

        private void OnMiceParticipantDataChanged()
        {
            // 전체 인원도 함께 표시
            //var info = MiceService.Instance.GetCurrentSessionInfo();
            //if (info == null) { this.MemberCount = ""; return; }
            //int curCount = MiceInfoManager.Instance.ParticipantInfo.Data.Count;
            //this.MemberCount = $"{curCount} / {info.MaxMemeberCount}";

            // 현재 인원 표시
            int curCount = MiceInfoManager.Instance.ParticipantInfo.TotalData.Count;
            this.MemberCount = $"{curCount}";
        }
    }
}
