/*===============================================================
* Product:		Com2Verse
* File Name:	MiceCommonViewModel.cs
* Developer:	ikyoung
* Date:			2023-04-04 15:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using UnityEngine;
using Com2Verse.Mice;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;
using Com2Verse.Logger;

namespace Com2Verse.UI
{
    [ViewModelGroup("Mice")]
    public sealed partial class MiceCommonViewModel : ViewModelBase
    {
        public CommandHandler EnterMiceLobbyFromEntry { get; private set; }
        public CommandHandler TestMicePDFView         { get; private set; }
        public CommandHandler ShowBusinessCardBook    { get; private set; }
        public CommandHandler CreateDummyAvatar       { get; private set; }

        public CommandHandler ShowMiceSessionInfoView { get; private set; }
        public CommandHandler ExitMiceService         { get; private set; }
        public CommandHandler EnterMiceService         { get; private set; }

        public string CurrentMiceAreaName
        {
            get => MiceService.Instance.CurrentAreaDisplayInfo();
        }
        public string CurrentMiceInformationMessage
        {
            get => MiceService.Instance.CurrentInformationMessage();
        }
        public string CurrentUserAuthority
        {
            get => MiceService.Instance.CurrentUserAuthority.ToString();
        }
        public bool IsInWorld
        {
            get => MiceService.Instance.CurrentAreaType == eMiceAreaType.WORLD;
        }
        public bool IsInWorldAndUserReady
        {
            get => MiceService.Instance.CurrentAreaType == eMiceAreaType.WORLD && MiceService.Instance.UserTeleportCompleted;
        }
        
        public bool IsMiceDebugMode
        {
            get
            {
                #if UNITY_EDITOR
                return true;
                #endif
                return false;
            }
        }
        private int _cameraCursor = 0;
        
        public MiceCommonViewModel()
        {
            RegisterCommandHandlers();
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
            MiceService.Instance.OnMiceStateChangedEvent += OnMiceStateChanged;
            MiceInfoManager.Instance.OnMiceInfoChangedEvent += OnMiceInfoChanged;

            InitParticipantList();
            InitSessionInfo();

            RefreshMiceServiceProperty();
            RefreshMiceInfoProperty();
            RefreshParticipantListProperty();
        }

        public override void OnRelease()
        {
            if (MiceService.InstanceExists)
                MiceService.Instance.OnMiceStateChangedEvent -= OnMiceStateChanged;
            if (MiceInfoManager.InstanceExists)
                MiceInfoManager.Instance.OnMiceInfoChangedEvent -= OnMiceInfoChanged;
            base.OnRelease();
        }

        private void OnMiceStateChanged()
        {
            RefreshMiceServiceProperty();
        }

        private void OnMiceInfoChanged()
        {
            RefreshMiceInfoProperty();
        }

        private void RefreshMiceServiceProperty()
        {
            InvokePropertyValueChanged(nameof(CurrentMiceAreaName), CurrentMiceAreaName);
            InvokePropertyValueChanged(nameof(CurrentMiceInformationMessage), CurrentMiceInformationMessage);
            InvokePropertyValueChanged(nameof(IsInWorld), IsInWorld);
            InvokePropertyValueChanged(nameof(IsInWorldAndUserReady), IsInWorldAndUserReady);
            InvokePropertyValueChanged(nameof(IsMiceDebugMode), IsMiceDebugMode);
        }
        
        private void RefreshMiceInfoProperty()
        {
            InvokePropertyValueChanged(nameof(CurrentUserAuthority), CurrentUserAuthority);
        }

        private void RegisterCommandHandlers()
        {
            EnterMiceLobbyFromEntry = new CommandHandler(OnEnterMiceLobbyClick);
            TestMicePDFView         = new CommandHandler(OnRequestShowPDFView);
            ShowBusinessCardBook    = new CommandHandler(OnShowBusinessCardBook);

            ShowMiceSessionInfoView = new CommandHandler(OnShowMiceSessionInfoView);
            CreateDummyAvatar       = new CommandHandler(OnCreateDummyAvatar);
            ExitMiceService         = new CommandHandler(OnExitMiceService);
            EnterMiceService         = new CommandHandler(OnEnterMiceService);

            RegisterParticipantListHandlers();
        }

        private void OnEnterMiceLobbyClick()
        {
            MiceService.Instance.RequestEnterMiceLobby().Forget();
        }

        private void OnRequestShowPDFView()
        {
        }

        private void OnShowBusinessCardBook()
        {
            MiceBusinessCardBookViewModel.ShowView();
        }

        private void OnCreateDummyAvatar()
        {
            var seatController = GameObject.FindObjectOfType<MiceSeatController>();
            seatController?.CreateDummyAvatars(1.0f);
        }


        private void OnShowMiceSessionInfoView()
        {
            MiceUISessionInfoViewModel.ShowView().Forget();
            var seatController = GameObject.FindObjectOfType<MiceSeatController>();
            seatController?.CreateDummyAvatars(1.0f);
        }
        
        private void OnExitMiceService()
        {
            Commander.Instance.LeaveBuildingRequest();
        }
        
        private void OnEnterMiceService()
        {
            Commander.Instance.RequestServiceChange(MiceService.MICE_CONVENTIONCENTER_ID);
        }
    }
}


namespace Com2Verse.UI
{
    public sealed partial class MiceCommonViewModel // Participant List
    {
        public string ShowParticipantListButtonCaption => _showParticipantListButtonCaption;
        public CommandHandler ShowParticipantList { get; private set; }
        public bool IsParticipantViewVisible => _viewParticipant != null;

        private GUIView _viewParticipant;
        private string _showParticipantListButtonCaption;

        private void InitParticipantList()
        {
            _viewParticipant = null;
        }

        private void RefreshParticipantListProperty()
        {
            InvokePropertyValueChanged(nameof(IsParticipantViewVisible), IsParticipantViewVisible);

            _showParticipantListButtonCaption = IsParticipantViewVisible ? "참여자 목록 감추기" : "참여자 목록 보기";
            InvokePropertyValueChanged(nameof(ShowParticipantListButtonCaption), ShowParticipantListButtonCaption);
        }

        private void RegisterParticipantListHandlers()
        {
            this.ShowParticipantList = new CommandHandler(this.OnShowParticipantList);
        }

        private void OnShowParticipantList()
        {
            if (_viewParticipant is not null)
            {
                _viewParticipant.Hide();
                _viewParticipant = null;

                RefreshParticipantListProperty();
            }
            else
            {
                MiceUIParticipantListViewModel
                    .ShowView
                    (
                        v =>
                        {
                            _viewParticipant = v;
                            RefreshParticipantListProperty();
                        },
                        _ =>
                        {
                            _viewParticipant = null;
                            RefreshParticipantListProperty();
                        }
                    )
                    .Forget();
            }
        }
    }
}


namespace Com2Verse.UI
{
    public sealed partial class MiceCommonViewModel // SessionInfo
    {
        public CommandHandler ShowSessionInfo { get; private set; }

        public void InitSessionInfo()
        {
            ShowSessionInfo = new CommandHandler(OnShowSessionInfo);
        }

        void OnShowSessionInfo()
        {
            if(MiceService.Instance.CurMiceType == MiceWebClient.MiceType.EventLounge)
            {
                // 라운지 테스트
                MiceService.Instance.ShowUIPopupSessionList().Forget();
            }
            else if (MiceService.Instance.CurMiceType > MiceWebClient.MiceType.EventLounge)
            {
                // 홀 테스트
                MiceService.Instance.ShowUIPopupSessionInfo().Forget();
            }
        }
    }
}