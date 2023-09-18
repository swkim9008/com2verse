/*===============================================================
* Product:		Com2Verse
* File Name:	MiceInfo.cs
* Developer:	ikyoung
* Date:			2023-03-31 11:18
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System;
using System.Text;
using System.Threading;
using Com2Verse.CameraSystem;
using JetBrains.Annotations;
using Com2Verse.UI;
using Com2Verse.Network;
using Com2Verse.Logger;
using Protocols.Mice;
using Cysharp.Threading.Tasks;
using Com2Verse.Data;
using Com2Verse.Director;
using Com2Verse.Extension;
using UnityEngine;
using Application = UnityEngine.Application;
using System.IO;
using Com2Verse.EventTrigger;
using Com2Verse.Interaction;
using Com2Verse.PlayerControl;
using Sentry;
using User = Com2Verse.Network.User;

namespace Com2Verse.Mice
{
    public enum eMiceKioskWebViewMessageType
    {
        None,
        EnterLounge, // 라운지 들어갈때 (PC)
        EnterHall, // 홀 들어갈때.(PC)
        ClosePage, // 페이지 닫을때.(PC)
        RequireBusinessCard, // 명함 필요(PC)
        RequireBusinessCardFreeLounge, // 명함 필요(PC) 체험하기.
        RefreshToken, // 토큰 갱신.(PC, Mobile)
        ChangeLanguage, // 언어 변경 hive (Mobile)
        LogOut, // 로그아웃 (Mobile)
        GoToTitle, // 타이틀로 돌아가기 (Mobile)
        EnterFreeLounge, // 무료라운지 들어갈때 (PC)
        OpenUrl, // 외부 브라우저로 url열때(PC, Mobile)
    }
    
    public enum eMiceUserInteractionState
    {
        None,
        WithWorldObject,
    }
    
    public enum eMiceAreaChangeReason
    {
        None,
        ExitButton,
        Kick,
        SessionClosed,
        SessionChanged,
    }
    
    public struct MiceKioskWebViewMessage
    {
        public string MessageType;
        public long EventID;
        public long SessionID;
        public string HiveLanguageCode;
        public string Url;

        public override string ToString()
        {
            return $"MessageType {MessageType} EventID {EventID} SessionID {SessionID} HiveLanguageCode {HiveLanguageCode} Url {Url}";
        }
    }
    
    [Serializable]
    public struct MiceKioskWebViewPostMessage
    {
        public string type;
        public string message;

        public override string ToString()
        {
            return $"type {type} message {message}";
        }
    }

    public sealed partial class MiceService : Singleton<MiceService>, IDisposable
    {
        public static readonly eMiceCameraJigKey[] CameraList = new eMiceCameraJigKey[]
        {
            eMiceCameraJigKey.MICE_HALL_MAIN_SCREEN_VIEW, eMiceCameraJigKey.MICE_HALL_ALL_SCREEN_VIEW, eMiceCameraJigKey.MICE_HALL_LEFT_SCREEN_VIEW, eMiceCameraJigKey.MICE_HALL_RIGHT_SCREEN_VIEW, eMiceCameraJigKey.MICE_HALL_WIDE_VIEW
        };

        static public readonly long MICE_CONVENTIONCENTER_ID = 4;
        public bool ServicePrepared => _servicePrepared;
        public bool ServiceEnabled => _serviceEnabled;
        public bool ServiceConnected => _serviceConnected;
        public MiceArea CurrentArea => _currentAreaCache;
        public eMiceAreaType CurrentAreaType => _currentAreaCache?.MiceAreaType ?? eMiceAreaType.NONE;
        public eMiceAreaType EnterRequestedMiceLoungeAreaType { get; private set; }
        public eMiceServiceState CurrentStateType => _currentStateCache?.ServiceStateType ?? eMiceServiceState.NONE;
        public bool IsInSession => CurrentAreaType == eMiceAreaType.HALL || CurrentAreaType == eMiceAreaType.MEET_UP; 
        public string MiceFrontEndMenuUrl { get; private set; }
        
        public string NextSpaceID { get; private set; }
        public MiceSessionID SessionID { get; set; }
        public MiceEventID EventID { get; set; }
        public eMiceUserInteractionState UserInteractionState => _userInteractionState;

        public long CurRoomID => _currentAreaCache?.GetRoomID() ?? 0;
        public MiceWebClient.MiceType CurMiceType => _currentAreaCache?.GetMiceType() ?? MiceWebClient.MiceType.Lobby;
        public Dictionary<long, SpaceTemplate> SpaceTemplateDatas => _spaceTemplateDatas;

        public MiceStreamingNoti StreamingState { get; set; }
        public eMiceAreaChangeReason LastAreaChangeReason { get; set; }

        public bool UserTeleportCompleted { get; private set; }
        public bool ReceiveTeleportStartMessage { get; private set; }
        public long EnterRequestedRoomID { get; set; }

        public MiceWebClient.eMiceAuthorityCode CurrentUserAuthority => _currentUserAuthority;

        private bool _servicePrepared;
        private bool _serviceEnabled;
        private bool _serviceConnected;
        private MiceArea _currentAreaCache;
        private MiceArea _prevAreaCache;
        private MiceServiceState _currentStateCache;
        private MiceServiceState _prevStateCache;
        private Dictionary<string, MiceArea> _miceAreas = new Dictionary<string, MiceArea>();
        private Dictionary<string, MiceServiceState> _miceStates = new Dictionary<string, MiceServiceState>();
        private Action<eMiceAreaType> _onMiceSpaceLoadComplete;
        private Action _onMiceServiceStateChanged;
        private Action<MiceStreamingNoti> _onMiceScreenStateChanged;
        private StringBuilder _requestUrlMaker = new StringBuilder();
        private Dictionary<long, SpaceTemplate> _spaceTemplateDatas = null;
        private string _platformString = "";
        private string _webLanguageString = "";
        private MiceWebClient.eMiceAuthorityCode _currentUserAuthority;
        private eMiceUserInteractionState _userInteractionState;

        // TODO 임시 사용
        private GUIView _uiSessionDataDownloadView;
        private GUIView _uiSessionInfoView;
        private GUIView _uiMetaverseOptionView;
        private GUIView _kioskEventMenuView;
        private GUIView _kioskSessionMenuView;

        private readonly string _requestEventUrlForm = "{0}?os={1}&page=main&lang={2}&token={3}";
        private readonly string _requestSessionUrlForm = "{0}?os={1}&page=event&id={2}&lang={3}&token={4}";
        private readonly string _requestNoticeUrlForm = "{0}?os={1}&page=notice&id={2}&lang={3}&token={4}";
        private readonly long MICE_SERVICE_ID = 300;
        private CancellationTokenSource _teleportErrorCheckCts;

        // 230531 nams : 현재 session에 참가중인 인원수 
        // TODO : 실제 값을 적용해야 함
        //public int CurrentSessionMemeberCount { get; private set; } = 37;
        //public int MaxSessionMemeberCount { get; private set; } = 200; 

        public event Action<eMiceAreaType> OnMiceSpaceLoadCompleteEvent
        {
            add
            {
                _onMiceSpaceLoadComplete -= value;
                _onMiceSpaceLoadComplete += value;
            }
            remove => _onMiceSpaceLoadComplete -= value;
        }

        public event Action OnMiceStateChangedEvent
        {
            add
            {
                _onMiceServiceStateChanged -= value;
                _onMiceServiceStateChanged += value;
            }
            remove => _onMiceServiceStateChanged -= value;
        }
        public event Action<MiceStreamingNoti> OnMiceScreenStateChanged
        {
            add
            {
                _onMiceScreenStateChanged -= value;
                _onMiceScreenStateChanged += value;
            }
            remove => _onMiceScreenStateChanged -= value;
        }

        /// <summary>
        /// Singleton Instance Creation
        /// </summary>
        [UsedImplicitly]
        private MiceService()
        {
        }

        public void Initialize()
        {
            this.PrepareInstance();
        }

        //IDisposable
        public void Dispose()
        {
            InitializeServiceState();
            _serviceEnabled = false;
            _serviceConnected = false;
            _servicePrepared = false;
            _miceAreas.Clear();
            _onMiceSpaceLoadComplete = null;
            _onMiceServiceStateChanged = null;

            {
                var packetReceiver = Network.GameLogic.PacketReceiver.InstanceOrNull;
                if (packetReceiver != null)
                {
                    packetReceiver.ServiceChangeResponse -= OnServiceChangeResponse;
                    packetReceiver.LeaveBuildingResponse -= OnLeaveBuildingResponse;
                    if (User.InstanceExists)
                        User.Instance.OnTeleportCompletion -= OnTeleportCompletion;
                    if (SceneManager.InstanceExists)
                        SceneManager.Instance.BeforeSceneChanged -= OnBeforeSceneChanged;
                }
            }
            if (ServiceManager.InstanceExists)
                ServiceManager.Instance.ServiceChangeEarlyNotify -= OnServiceChangeEarlyNotify;
            
            C2VDebug.LogMethod(GetType().Name);
        }

        public void PrepareInstance()
        {
            _servicePrepared = true;

            _AddMiceArea(new MiceLobbyArea(), ref _miceAreas);
            _AddMiceArea(new MiceLoungeArea(), ref _miceAreas);
            _AddMiceArea(new MiceFreeLoungeArea(), ref _miceAreas);
            _AddMiceArea(new MiceHallArea(), ref _miceAreas);
            _AddMiceArea(new MiceMeetUpArea(), ref _miceAreas);
            _AddMiceArea(new MiceWorldArea(), ref _miceAreas);

            _AddMiceState(new MiceSessionIdleState(), ref _miceStates);
            _AddMiceState(new MiceSessionPlayingState(), ref _miceStates);
            _AddMiceState(new MiceSessionQnAState(), ref _miceStates);
            _AddMiceState(new MicePlayingCutScene(), ref _miceStates);

            void _AddMiceArea(MiceArea area, ref Dictionary<string, MiceArea> containerToAdd)
            {
                area.Prepare();
                containerToAdd.Add(area.GetType().ToString(), area);
            }
            
            void _AddMiceState(MiceServiceState state, ref Dictionary<string, MiceServiceState> containerToAdd)
            {
                state.Prepare();
                containerToAdd.Add(state.GetType().ToString(), state);
            }

            _currentStateCache = GetMiceState(eMiceServiceState.SESSION_IDLE);
            _currentAreaCache = GetMiceArea(eMiceAreaType.WORLD);
            
            if (Application.platform == RuntimePlatform.Android
                || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                _platformString = "mobile";
            }
            else
            {
                _platformString = "pc";
            }

            Network.GameLogic.PacketReceiver.Instance.ServiceChangeResponse += OnServiceChangeResponse;
            Network.GameLogic.PacketReceiver.Instance.LeaveBuildingResponse += OnLeaveBuildingResponse;
            User.Instance.OnTeleportCompletion += OnTeleportCompletion;
            ServiceManager.Instance.ServiceChangeEarlyNotify += OnServiceChangeEarlyNotify;

            SceneManager.Instance.BeforeSceneChanged  += OnBeforeSceneChanged;
        }

        private MiceArea GetMiceArea(eMiceAreaType miceAreaType)
        {
            foreach (var kvp in _miceAreas)
            {
                if(kvp.Value.MiceAreaType == miceAreaType)
                {
                    return kvp.Value;
                }
            }
            return null;
        }
        
        private MiceServiceState GetMiceState(eMiceServiceState miceState)
        {
            foreach (var kvp in _miceStates)
            {
                if(kvp.Value.ServiceStateType == miceState)
                {
                    return kvp.Value;
                }
            }
            return null;
        }

        public void PrepareService()
        {
            InitializeServiceState();
            _serviceEnabled = true;
            
            MiceService.Instance.OnMiceSpaceLoadCompleteEvent += OnMiceSpaceLoadComplete;
            PacketReceiver.Instance.OnEnterMiceLobbyResponseEvent += OnEnterMiceLobbyResponse;
            PacketReceiver.Instance.OnEnterMiceLoungeResponseEvent += OnEnterMiceLoungeResponse;
            PacketReceiver.Instance.OnEnterMiceFreeLoungeResponseEvent += OnEnterMiceFreeLoungeResponse;
            PacketReceiver.Instance.OnEnterMiceHallResponseEvent += OnEnterMiceHallResponse;
            PacketReceiver.Instance.OnUsePortalResponseEvent += OnUsePortalResponse;
            PacketReceiver.Instance.OnTeleportUserStartNotifyEvent += OnTeleportUserStartNotifyResponse;
            PacketReceiver.Instance.OnMiceRoomNotifyEvent += OnMiceRoomNotify;
            PacketReceiver.Instance.OnForcedMiceTeleportNotifyEvent += OnForcedMiceTeleportNotify;
            NetworkManager.Instance.OnLogout += OnLogout;
            MapController.Instance.CheckReserveObject = CheckReserveObject;
            
            C2VDebug.LogMethod(GetType().Name);
        }

        private void StopService()
        {
            InitializeServiceState();
            _serviceEnabled = false;
            _serviceConnected = false;
            MiceService.Instance.OnMiceSpaceLoadCompleteEvent -= OnMiceSpaceLoadComplete;
            PacketReceiver.Instance.OnEnterMiceLobbyResponseEvent -= OnEnterMiceLobbyResponse;
            PacketReceiver.Instance.OnEnterMiceLoungeResponseEvent -= OnEnterMiceLoungeResponse;
            PacketReceiver.Instance.OnEnterMiceFreeLoungeResponseEvent -= OnEnterMiceFreeLoungeResponse;
            PacketReceiver.Instance.OnEnterMiceHallResponseEvent -= OnEnterMiceHallResponse;
            PacketReceiver.Instance.OnUsePortalResponseEvent -= OnUsePortalResponse;
            PacketReceiver.Instance.OnTeleportUserStartNotifyEvent -= OnTeleportUserStartNotifyResponse;
            PacketReceiver.Instance.OnMiceRoomNotifyEvent -= OnMiceRoomNotify;
            PacketReceiver.Instance.OnForcedMiceTeleportNotifyEvent -= OnForcedMiceTeleportNotify;
            NetworkManager.Instance.OnLogout -= OnLogout;
            MapController.Instance.CheckReserveObject = null;

            _uiSessionDataDownloadView = null;
            _uiSessionInfoView = null;
            _uiMetaverseOptionView = null;
            _kioskEventMenuView = null;
            _kioskSessionMenuView = null;
        
            _teleportErrorCheckCts?.Cancel();
            C2VDebug.LogMethod(GetType().Name);
        }

        private async UniTask TryingToChangeCurrentArea(long mapId)
        {
            if (_spaceTemplateDatas == null)
            {
                LoadSpaceTemplate();
            }
            
            if (_spaceTemplateDatas.TryGetValue(mapId, out var spaceTemplate))
            {
                eMiceAreaType targetArea = spaceTemplate.SpaceCode.ToMiceAreaType();
                if (targetArea == eMiceAreaType.LOUNGE && EnterRequestedMiceLoungeAreaType == eMiceAreaType.FREE_LOUNGE)
                {
                    targetArea = eMiceAreaType.FREE_LOUNGE;
                }
                ChangeCurrentArea(targetArea);
            }
        }

        public void ChangeCurrentArea(eMiceAreaType miceAreaType)
        {
            MiceArea nextArea = GetMiceArea(miceAreaType);
            ChangeCurrentArea(nextArea);
        }

        private void ChangeCurrentArea(MiceArea nextArea)
        {
            if (nextArea == null) return;
            
            eMiceAreaType prevAreaType = eMiceAreaType.NONE;
            eMiceAreaType nextAreaType = eMiceAreaType.NONE;
            {
                _currentAreaCache?.OnStop();
                prevAreaType = _currentAreaCache?.MiceAreaType ?? eMiceAreaType.NONE;
                nextAreaType = nextArea.MiceAreaType;
            }
            _prevAreaCache = _currentAreaCache;
            _currentAreaCache = nextArea;
            _currentAreaCache?.OnStart(prevAreaType);

            UpdateUserAuthority();
            
            C2VDebug.LogMethod(GetType().Name, $"{prevAreaType} to {nextAreaType}");
        }
        
        
        public void ChangeCurrentState(eMiceServiceState miceStateType)
        {
            var nextState = GetMiceState(miceStateType);
            ChangeCurrentState(nextState);
        }

        private void ChangeCurrentState(MiceServiceState nextState)
        {
            if (nextState == null) return;
            
            eMiceServiceState prevType = eMiceServiceState.NONE;
            eMiceServiceState nextType = eMiceServiceState.NONE;
            {
                _currentStateCache?.OnStop();
                prevType = _currentStateCache?.ServiceStateType ?? eMiceServiceState.NONE;
                nextType = nextState.ServiceStateType;
            }
            C2VDebug.LogMethod(GetType().Name, $"{prevType} to {nextType}");
            
            _prevStateCache = _currentStateCache;
            _currentStateCache = nextState;
            _currentStateCache?.OnStart(prevType);

            Notify();
        }

        private void InitializeServiceState()
        {
            _currentAreaCache?.OnStop();
            _currentAreaCache = null;
            _prevAreaCache = null;
            
            _currentStateCache?.OnStop();
            _currentStateCache = null;
            _prevStateCache = null;
        }

        private void OnMiceSpaceLoadComplete(eMiceAreaType miceAreaType)
        {
            C2VDebug.LogMethod(GetType().Name, $"{miceAreaType}");
        }

        private void OnEnterMiceHallResponse(Protocols.Mice.EnterHallResponse response)
        {
            C2VDebug.LogMethod(GetType().Name, $"{response} {response.EnterRequestResult}");
            InitMiceRoomEnteringState();
            
            if (response.EnterRequestResult != EnterRequestResult.Success)
            {
                _currentAreaCache?.OnError(response.EnterRequestResult);
                return;
            }

            if (string.IsNullOrEmpty(response.SpaceId))
            {
                _currentAreaCache?.OnError(EnterRequestResult.RoomNull);
                return;
            }
            
            SessionID = response.RoomId.ToMiceSessionID();
            ChangeCurrentState(eMiceServiceState.SESSION_IDLE);
            _kioskSessionMenuView?.Hide();
            _currentAreaCache?.OnEnterMiceHallResponse(response);
            InteractionManager.Instance.UnsetInteractionUIAll();
        }

        private void OnEnterMiceLoungeResponse(Protocols.Mice.EnterLoungeResponse response)
        {
            C2VDebug.LogMethod(GetType().Name, $"{response} {response.EnterRequestResult}");
            InitMiceRoomEnteringState();
            if (response.EnterRequestResult != EnterRequestResult.Success)
            {
                _currentAreaCache?.OnError(response.EnterRequestResult);
                return;
            }

            EventID = response.RoomId.ToMiceEventID();
            ChangeCurrentState(eMiceServiceState.SESSION_IDLE);
            _currentAreaCache?.OnEnterMiceLoungeResponse(response);
        }
        
        private void OnEnterMiceFreeLoungeResponse(Protocols.Mice.EnterFreeLoungeResponse response)
        {
            // 무료 라운지 구현은 행사 설정으로 변경되었습니다. 0905
            C2VDebug.LogMethod(GetType().Name, $"{response} {response.EnterRequestResult}");
            InitMiceRoomEnteringState();
            if (response.EnterRequestResult != EnterRequestResult.Success)
            {
                _currentAreaCache?.OnError(response.EnterRequestResult);
                return;
            }

            EventID = response.RoomId.ToMiceEventID();
            
            _currentAreaCache?.OnEnterMiceFreeLoungeResponse(response);
        }
        
        private void OnEnterMiceLobbyResponse(Protocols.Mice.EnterLobbyResponse response)
        {
            C2VDebug.LogMethod(GetType().Name, $"{response}");
            InitMiceRoomEnteringState();
            _currentAreaCache?.OnEnterMiceLobbyResponse(response);
        }

        private string MakeWebViewRequestForm(string sourceUrl, MiceType miceType)
        {
            _requestUrlMaker.Clear();
            
            string languageCode = "ko";
            var option = Com2Verse.Option.OptionController.Instance.GetOption<LanguageOption>();
            if (option != null)
            {
                languageCode = option.GetLanguage().ToWebLanguageCode();
            }
            
            if (miceType == MiceType.Lobby)
            {
                return _requestUrlMaker.AppendFormat(_requestEventUrlForm, sourceUrl, _platformString, languageCode, User.Instance.CurrentUserData.AccessToken).ToString();
            }
            else if (miceType == MiceType.EventLounge || miceType == MiceType.EventFreeLounge)
            {
                return _requestUrlMaker.AppendFormat(_requestSessionUrlForm, sourceUrl, _platformString, EventID.value, languageCode, User.Instance.CurrentUserData.AccessToken).ToString();
            }
            return null;
        }
        
        public string MakeMiceNoticeUrl(int noticeKey)
        {
            _requestUrlMaker.Clear();
            
            string languageCode = "ko";
            var option = Com2Verse.Option.OptionController.Instance.GetOption<LanguageOption>();
            if (option != null)
            {
                languageCode = option.GetLanguage().ToWebLanguageCode();
            }
            return _requestUrlMaker.AppendFormat(_requestNoticeUrlForm, MiceFrontEndMenuUrl, _platformString, noticeKey.ToString(), languageCode, User.Instance.CurrentUserData.AccessToken).ToString();
        }

        private void OnUsePortalResponse(Protocols.GameLogic.UsePortalResponse response)
        {
            C2VDebug.LogMethod(GetType().Name, $"{response}");
            UserTeleportCompleted = false;
            _currentAreaCache?.OnUsePortalResponse(response);
        }

        private void OnTeleportUserStartNotifyResponse(Protocols.WorldState.TeleportUserStartNotify response)
        {
            C2VDebug.LogMethod(GetType().Name, $"{response}");
            UserTeleportCompleted = false;
            ReceiveTeleportStartMessage = true;
            _currentAreaCache?.OnTeleportUserStartNotifyResponse(response);
        }

        private void OnMiceRoomNotify(Protocols.Mice.MiceRoomNotify response)
        {
            C2VDebug.LogMethod(GetType().Name, $"NotiEvent : {response.NotiEvent}, MiceType : {response.MiceType}, EventName : {response.EventName}, EventTime : {response.EventTime}");
            _currentAreaCache?.OnMiceRoomNotify(response);
            
            if (response.NotiEvent == NotifyEvent.Close)
            {
                LastAreaChangeReason = eMiceAreaChangeReason.SessionClosed;
                UIManager.Instance.SendToastMessage
                (
                    Data.Localization.eKey.MICE_UI_SessionHall_Exit_EndSession_Message.ToLocalizationString(),
                    5f,
                    UIManager.eToastMessageType.WARNING
                );
            }
            else if (response.NotiEvent == NotifyEvent.SessionChange)
            {
                LastAreaChangeReason = eMiceAreaChangeReason.SessionChanged;
                UIManager.Instance.SendToastMessage
                (
                    Data.Localization.eKey.MICE_UI_SessionHall_Exit_ChangeSession_Message.ToLocalizationString(),
                    5f,
                    UIManager.eToastMessageType.WARNING
                );
            }
            else if (response.NotiEvent == NotifyEvent.Kick)
            {
                LastAreaChangeReason = eMiceAreaChangeReason.Kick;
                UIManager.Instance.SendToastMessage
                (
                    Data.Localization.eKey.MICE_UI_SessionHall_Kickout_Message.ToLocalizationString(),
                    5f,
                    UIManager.eToastMessageType.WARNING
                );
            }
            else if (response.NotiEvent == NotifyEvent.MiceKioskUrl)
            {
                if (response.MiceKioskUrlNoti.MiceType == MiceType.Lobby)
                {
                    MiceFrontEndMenuUrl = response.MiceKioskUrlNoti.Url;
                }
                else if (response.MiceKioskUrlNoti.MiceType == MiceType.EventLounge
                         || response.MiceKioskUrlNoti.MiceType == MiceType.EventFreeLounge)
                {
                    MiceFrontEndMenuUrl = response.MiceKioskUrlNoti.Url;
                }
            }
            else if (response.NotiEvent == NotifyEvent.StreamingStart || response.NotiEvent == NotifyEvent.StreamingEnd)
            {
                StreamingState = response.MiceStreamingNoti;
                _onMiceScreenStateChanged?.Invoke(response.MiceStreamingNoti);
            }
        }

        private void ForceLeaveBuilding()
        {
            PlayerController.Instance.SetStopAndCannotMove(true);
            User.Instance.DiscardPacketBeforeStandBy();
            Commander.Instance.LeaveBuildingRequest();
        }

        private async UniTask TeleportErrorChecker()
        {
            if (_teleportErrorCheckCts != null)
            {
                return;
            }
            try
            {
                _teleportErrorCheckCts = new CancellationTokenSource();
                float timer = 0f;
                float time = 10f;
                while (true)
                {
                    await UniTask.Delay(1000, DelayType.DeltaTime, PlayerLoopTiming.Update, _teleportErrorCheckCts.Token);
                    if (ReceiveTeleportStartMessage)
                    {
                        break;
                    }

                    timer += 1f;
                    if (timer >= time)
                    {
                        ForceLeaveBuilding();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                C2VDebug.LogMethod(GetType().Name, $"{e}");
            }
            finally
            {
                _teleportErrorCheckCts?.Dispose();
                _teleportErrorCheckCts = null;
            }
        }

        private void OnLogout()
        {
            StopService();
        }

        private void OnForcedMiceTeleportNotify(Protocols.Mice.ForcedMiceTeleportNotify response)
        {
            C2VDebug.LogMethod(GetType().Name, $"{response}");
            InitMiceRoomEnteringState();

            if (response.MiceType == Protocols.Mice.MiceType.EventLounge
                || response.MiceType == Protocols.Mice.MiceType.EventFreeLounge)
            {
                EventID = response.RoomId.ToMiceEventID();
            }
            else if(response.MiceType == Protocols.Mice.MiceType.ConferenceSession)
            {
                SessionID = response.RoomId.ToMiceSessionID();
                Commander.Instance.RequestMiceRoomNotify(RequestNotifyType.ScreenState, response.MiceType, SessionID);
            }
            UserTeleportCompleted = false;
            ReceiveTeleportStartMessage = false;
            
            ChangeCurrentState(eMiceServiceState.SESSION_IDLE);
            TeleportErrorChecker().Forget();
            _currentAreaCache?.OnForcedMiceTeleportNotify(response);
        }
        
       
        public async UniTask ShowKioskMenu(KioskObject kiosk)
        {
            // TODO 태그에서 URL 받아와야 함
            if (_currentAreaCache != null)
            {
                // 툴바에 있던 UI들을 닫는다.
                ViewModelManager.Instance.Get<MiceToolBarViewModel>()?.CloseAllUI();

                await _currentAreaCache.ShowKioskMenu(kiosk.GetUrl());
            }
            await UniTask.CompletedTask;
        }

        public MiceEventInfo GetCurrentEventInfo()
        {
            if (!this.EventID.IsValid()) return null;

            return MiceInfoManager.Instance.GetEventInfo(this.EventID);
        }

        public MiceSessionInfo GetCurrentSessionInfo()
        {
            if (!this.SessionID.IsValid()) return null;

            return GetCurrentEventInfo()?.GetSessionInfo(this.SessionID) ?? null;
        }
        
        public eMiceAreaType ConvertToMiceAreaType(long spaceTemplateID)
        {
            if(_spaceTemplateDatas != null && _spaceTemplateDatas.TryGetValue(spaceTemplateID, out var spaceTemplate) && spaceTemplate != null)
            {
                return spaceTemplate.SpaceCode.ToMiceAreaType();
            }
            return eMiceAreaType.NONE;
        }

        public void SetEnterWarpEffect(eMiceAreaType type)
        {
            var userDirector = UserDirector.Instance;
            switch (type)
            {
                case eMiceAreaType.LOBBY:
                    userDirector.IsActiveWarpEffect = false;
                    break;
                case eMiceAreaType.HALL:
                    userDirector.IsActiveWarpEffect = false;
                    break;
            }
        }

        public void SetExitWarpEffect(eMiceAreaType type)
        {
            var userDirector = UserDirector.Instance;
            switch (type)
            {
                case eMiceAreaType.LOBBY:
                    userDirector.IsActiveWarpEffect = true;
                    break;
                case eMiceAreaType.HALL:
                    userDirector.IsActiveWarpEffect = true;
                    break;
            }
        }

        public async UniTask OnPrepareService(long spaceTemplateID,
            CancellationTokenSource cts = null)
        {
            if (_serviceConnected == false) return;
            eMiceAreaType miceArea = ConvertToMiceAreaType(spaceTemplateID);
            switch (miceArea)
            {
                case eMiceAreaType.LOBBY:
                    await OnPrepareMiceLobby(cts);
                    break;
                case eMiceAreaType.LOUNGE:
                case eMiceAreaType.FREE_LOUNGE:
                    await OnPrepareMiceLounge(cts);
                    break;
                case eMiceAreaType.HALL:
                    await OnPrepareMiceHall(cts);
                    break;
                case eMiceAreaType.MEET_UP:
                    await OnPrepareMiceMeetUp(cts);
                    break;
            }

            await TryingToChangeCurrentArea(spaceTemplateID);
            
            _prevAreaCache?.OnLeaveScene();
            _currentAreaCache?.OnEnterScene();
        }

        public async UniTask OnStartService(long spaceTemplateID, CancellationTokenSource cts = null)
        {
            if (_serviceConnected == false) return;
            eMiceAreaType miceArea = ConvertToMiceAreaType(spaceTemplateID);
            switch (miceArea)
            {
                case eMiceAreaType.LOBBY:
                    await OnStartMiceLobby(cts);
                    break;
                case eMiceAreaType.LOUNGE:
                case eMiceAreaType.FREE_LOUNGE:
                    await OnStartMiceLounge(cts);
                    break;
                case eMiceAreaType.HALL:
                    await OnStartMiceHall(cts);
                    break;
                case eMiceAreaType.MEET_UP:
                    await OnStartMiceMeetUp(cts);
                    break;
            }
            
            Notify();
        }
        
        private async UniTask OnStartMiceLobby(CancellationTokenSource cts = null)
        {
            C2VDebug.LogMethod(GetType().Name, MakeWebViewRequestForm(MiceFrontEndMenuUrl, MiceType.Lobby));
            await MiceInfoManager.Instance.SyncMyUser();

            await ShowKioskEventMenu(this.MiceFrontEndMenuUrl);
            
            await UniTask.CompletedTask;
        }

        private async UniTask OnStartMiceLounge(CancellationTokenSource cts = null)
        {
            C2VDebug.LogMethod(GetType().Name);

            await MiceInfoManager.Instance.SyncEventEntity();
            await MiceInfoManager.Instance.SyncTicketInfo();
            await MiceInfoManager.Instance.SyncNoticeInfo();
            // 나의 모든 패키지 정보를 가져온다.
            await MiceInfoManager.Instance.SyncMyPackages();

            if (LastAreaChangeReason == eMiceAreaChangeReason.ExitButton)
            {
                TryShowSurveyPopup();
            }
            else if (LastAreaChangeReason == eMiceAreaChangeReason.Kick)
            {
                UIManager.Instance.ShowPopupConfirm
                (
                    Data.Localization.eKey.MICE_UI_Common_Title.ToLocalizationString(),
                    Data.Localization.eKey.MICE_UI_SessionHall_KickOut_Move_Popup_Desc.ToLocalizationString(),
                    null,
                    Data.Localization.eKey.MICE_UI_Common_Btn_Ok.ToLocalizationString()
                );
            }
            else if (LastAreaChangeReason == eMiceAreaChangeReason.SessionClosed
                     || LastAreaChangeReason == eMiceAreaChangeReason.SessionChanged)
            {
                UIManager.Instance.ShowPopupConfirm
                (
                    Data.Localization.eKey.MICE_UI_Common_Title.ToLocalizationString(),
                    Data.Localization.eKey.MICE_UI_SessionHall_NoRights_Move_Popup_Desc.ToLocalizationString(),
                    TryShowSurveyPopup,
                    Data.Localization.eKey.MICE_UI_Common_Btn_Ok.ToLocalizationString()
                );
            }
            LastAreaChangeReason = eMiceAreaChangeReason.None;
            await UniTask.CompletedTask;
        }

        public void TryShowSurveyPopup()
        {
            var eventInfo = MiceInfoManager.Instance.GetEventInfo(EventID);
            if (eventInfo != null)
            {
                var program = eventInfo.GetProgramInfo(SessionID);
                if (program == null) return;
                var survey = MiceInfoManager.Instance.GetEventInfo(EventID).GetAvailableSurvey(program.ProgramEntity.ProgramId);
                if (survey != null)
                {
                    var popupAddress = "UI_Popup_Survey";
                    UIManager.Instance.CreatePopup(popupAddress, (createdGuiView) =>
                    {
                        createdGuiView.Show();
                        var vm = createdGuiView.ViewModelContainer.GetViewModel<MiceUISurveyPopupViewModel>();
                        vm.Init(survey.SurveyPath, survey.SurveyNo);
                    }).Forget();
                }
            }
        }

        private async UniTask OnStartMiceHall(CancellationTokenSource cts = null)
        {
            C2VDebug.LogMethod(GetType().Name);
            await MiceInfoManager.Instance.SyncTicketInfo();
            await UniTask.CompletedTask;
        }

        private async UniTask OnStartMiceMeetUp(CancellationTokenSource cts = null)
        {
            C2VDebug.LogMethod(GetType().Name);
            await MiceInfoManager.Instance.SyncTicketInfo();
            await UniTask.CompletedTask;
        }

        private async UniTask OnPrepareMiceLobby(CancellationTokenSource cts = null)
        {
            C2VDebug.LogMethod(GetType().Name, MakeWebViewRequestForm(MiceFrontEndMenuUrl, MiceType.Lobby));
            await UniTask.CompletedTask;
        }

        private async UniTask OnPrepareMiceLounge(CancellationTokenSource cts = null)
        {
            C2VDebug.LogMethod(GetType().Name);
            await UniTask.CompletedTask;
        }

        private async UniTask OnPrepareMiceHall(CancellationTokenSource cts = null)
        {
            C2VDebug.LogMethod(GetType().Name);
            await UniTask.CompletedTask;
        }

        private async UniTask OnPrepareMiceMeetUp(CancellationTokenSource cts = null)
        {
            C2VDebug.LogMethod(GetType().Name);
            await UniTask.CompletedTask;
        }

        public async UniTask RequestStartService(CancellationTokenSource cts = null)
        {
            if (!_serviceEnabled)
            {
                MiceService.Instance.PrepareService();
                LoadSpaceTemplate();
                _serviceEnabled = true;
            }
            await UniTask.CompletedTask;
        }

        public async UniTask RequestEnterMiceLobby(CancellationTokenSource cts = null)
        {
            C2VDebug.LogMethod(GetType().Name);
            if (_currentAreaCache != null)
            {
                await _currentAreaCache.RequestEnterMiceLobby(cts);
            }

            UserTeleportCompleted = false;
            await UniTask.CompletedTask;
        }
        public async UniTask RequestEnterMiceLounge(MiceEventID eventID, CancellationTokenSource cts = null)
        {
            C2VDebug.LogMethod(GetType().Name, eventID.ToString());
            EnterRequestedMiceLoungeAreaType = eMiceAreaType.LOUNGE;
            if (_currentAreaCache != null)
            {
                if (!HasMiceRoomEnteringRequest())
                {
                    SetMiceRoomEnteringState(eventID.value);
                    UserTeleportCompleted = false;
                    await _currentAreaCache.RequestEnterMiceLounge(eventID, cts);
                }
            }
            await UniTask.CompletedTask;
        }
        
        public async UniTask RequestEnterMiceFreeLounge(MiceEventID eventID, CancellationTokenSource cts = null)
        {
            C2VDebug.LogMethod(GetType().Name, eventID.ToString());
            EnterRequestedMiceLoungeAreaType = eMiceAreaType.FREE_LOUNGE;
            if (_currentAreaCache != null)
            {
                if (!HasMiceRoomEnteringRequest())
                {
                    SetMiceRoomEnteringState(eventID.value);
                    UserTeleportCompleted = false;
                    await _currentAreaCache.RequestEnterMiceFreeLounge(eventID, cts);
                }
            }
            await UniTask.CompletedTask;
        }

        public async UniTask RequestEnterLastMiceLounge(CancellationTokenSource cts = null)
        {
            C2VDebug.LogMethod(GetType().Name);
            UserTeleportCompleted = false;
            if (EnterRequestedMiceLoungeAreaType == eMiceAreaType.FREE_LOUNGE)
            {
                await RequestEnterMiceFreeLounge(EventID, cts);                    
            }
            else if (EnterRequestedMiceLoungeAreaType == eMiceAreaType.LOUNGE)
            {
                await RequestEnterMiceLounge(EventID, cts);
            }

            await UniTask.CompletedTask;
        }

        public async UniTask RequestEnterMiceHall(MiceSessionID sessionID, CancellationTokenSource cts = null)
        {
            C2VDebug.LogMethod(GetType().Name, sessionID.ToString());
            if (_currentAreaCache != null)
            {
                if (!HasMiceRoomEnteringRequest())
                {
                    SetMiceRoomEnteringState(sessionID.value);
                    UserTeleportCompleted = false;
                    await _currentAreaCache.RequestEnterMiceHall(sessionID, cts);                    
                }
            }
            await UniTask.CompletedTask;
        }

        public async UniTask ProcessKioskMessage(MiceKioskWebViewMessage kioskMessage, GUIView guiView)
        {
            C2VDebug.LogMethod(GetType().Name, $"{kioskMessage}");
            try
            {
                var messageType = Enum.Parse<eMiceKioskWebViewMessageType>(kioskMessage.MessageType);
                if (messageType == eMiceKioskWebViewMessageType.EnterHall)
                {
                    if (_currentAreaCache != null) await _currentAreaCache.OnKioskEnterHall(kioskMessage, guiView);
                }
                else if (messageType == eMiceKioskWebViewMessageType.EnterLounge)
                {
                    if (_currentAreaCache != null) await _currentAreaCache.OnKioskEnterLounge(kioskMessage, guiView);
                }
                else if (messageType == eMiceKioskWebViewMessageType.EnterFreeLounge)
                {
                    if (_currentAreaCache != null) await _currentAreaCache.OnKioskEnterFreeLounge(kioskMessage, guiView);
                }
                else if (messageType == eMiceKioskWebViewMessageType.RequireBusinessCard
                         || messageType == eMiceKioskWebViewMessageType.RequireBusinessCardFreeLounge)
                {
                    if (_currentAreaCache != null) await _currentAreaCache.OnKioskRequireBusinessCard(kioskMessage, guiView);
                }
                else if (messageType == eMiceKioskWebViewMessageType.RefreshToken)
                {
                    if (guiView.ViewModelContainer.TryGetViewModel(typeof(WebViewModel), out var vm))
                    {
                        if (vm is WebViewModel webViewModel)
                        {
                            await LoginManager.Instance.TryRefreshToken(() => {});
                            MiceKioskWebViewPostMessage messageObj = new MiceKioskWebViewPostMessage()
                            {
                                type = "SetAccessToken",
                                message = User.Instance.CurrentUserData.AccessToken,
                            };
                            
                            string message = JsonUtility.ToJson(messageObj);
                            webViewModel.PostMessage(message);
                        }
                    }

                    if (_currentAreaCache != null) await _currentAreaCache.OnKioskRefreshToken(kioskMessage, guiView);
                }
                else if (messageType == eMiceKioskWebViewMessageType.ClosePage)
                {
                    if (_currentAreaCache != null) await _currentAreaCache.OnKioskClosePage(kioskMessage, guiView);
                }
                else if (messageType == eMiceKioskWebViewMessageType.LogOut)
                {
                    LoginManager.Instance.Logout();
                }
                else if (messageType == eMiceKioskWebViewMessageType.OpenUrl)
                {
                    Application.OpenURL(kioskMessage.Url);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                guiView.Hide();
            }
            await UniTask.CompletedTask;
        }

        public bool ShouldHandleKioskEscButton()
        {
            if (CurrentAreaType == eMiceAreaType.LOBBY)
                return true;
            
            return false;
        }

        public void OnEscInput(StackRegisterer guiRegisterer)
        {
            if (ShouldHandleKioskEscButton())
            {
                RequestGoToWorld();
            }
            else
            {
                guiRegisterer.HideComplete();
            }
        }

        public void RequestGoToWorld()
        {
            UIManager.Instance.ShowPopupYesNo(Data.Localization.eKey.MICE_UI_Popup_Title_Lobby_Exit.ToLocalizationString(), Data.Localization.eKey.MICE_UI_Popup_Msg_Lobby_Exit.ToLocalizationString(),
                (guiView) =>
                {
                    MiceService.Instance.LastAreaChangeReason = eMiceAreaChangeReason.ExitButton;
                    Commander.Instance.LeaveBuildingRequest();
                });
        }

        public void InitMiceRoomEnteringState()
        {
            EnterRequestedRoomID = 0;
        }
        public void SetMiceRoomEnteringState(long roomIDToEnter)
        {
            EnterRequestedRoomID = roomIDToEnter;
        }
        public bool HasMiceRoomEnteringRequest()
        {
            return EnterRequestedRoomID != 0;
        }

        public void Notify()
        {
            _onMiceServiceStateChanged?.Invoke();
        }

        public string CurrentAreaDisplayInfo()
        {
            return _currentAreaCache?.CurrentAreaDisplayInfo() ?? string.Empty;
        }

        public string CurrentInformationMessage()
        {
            return _currentAreaCache?.CurrentInformationMessage() ?? string.Empty;
        }

        public long GetRoomID(MiceWebClient.MiceType miceType)
        {
            switch (miceType)
            {
                case MiceWebClient.MiceType.Conference:
                case MiceWebClient.MiceType.ConferenceSession:
                case MiceWebClient.MiceType.Exhibition:
                    return SessionID;
                case MiceWebClient.MiceType.EventLounge:
                case MiceWebClient.MiceType.EventFreeLounge:
                    return EventID;
                default:
                    return 0;
            }
        }
        
        public long GetRoomID(Protocols.Mice.MiceType miceType)
        {
            MiceWebClient.MiceType castedType = (MiceWebClient.MiceType)miceType;
            return GetRoomID(castedType);
        }

        public void OnServiceChangeEarlyNotify(Protocols.CommonLogic.ServiceChangeNotify response)
        {
            if (response.ServiceId != MICE_SERVICE_ID)
            {
                if (_serviceConnected)
                {
                    C2VDebug.LogMethod(GetType().Name, $"{response}");
                    Commander.Instance.RequestServiceExit();
                    MiceService.Instance.StopService();
                }
            }
        }
        
        public void OnServiceChangeResponse(Protocols.GameLogic.ServiceChangeResponse response)
        {
            if (response.ServiceId == MICE_SERVICE_ID)
            {
                if (!_serviceConnected)
                {
                    C2VDebug.LogMethod(GetType().Name, $"{response}");
                    MiceService.Instance.RequestStartService().Forget();
                    _serviceConnected = true;
                    _currentAreaCache?.OnServiceChangeResponse(response);
                }
            }
        }

        public void OnLeaveBuildingResponse(Protocols.GameLogic.LeaveBuildingResponse response)
        {
            if (!_serviceConnected) return;

            C2VDebug.LogMethod(GetType().Name, $"{response}");
            _currentAreaCache?.OnLeaveBuildingResponse(response);
            
            UserTeleportCompleted = false;
            StopService();
        }

        private void LoadSpaceTemplate()
        {
            var spaceData = TableDataManager.Instance.Get<TableSpaceTemplate>();
            _spaceTemplateDatas = spaceData.Datas;
        }
        
        public void ShowPDFView(string url)
        {
            string pdfUrl = url;
            if(string.IsNullOrEmpty(pdfUrl)) return;
            
            var screenSize = new Vector2(1700, 1000); //GetScreenSize();
            UIManager.Instance.ShowPopupWebView(false,
                screenSize,
                pdfUrl,
                downloadProgressAction: (eventArgs =>
                {
                    if (eventArgs.Type == Vuplex.WebView.ProgressChangeType.Finished)
                    {
                        // pdf url로 부터 파일 이름을 가져온 후 복사할 폴더에 중복파일이 있는지를 검사한후 넘버링을 붙힌 후 경로를 반환한다.
                        var newName = GetUniqueFileNameInPath(Utils.Path.Downloads, pdfUrl);

                        // 파일을 이름을 변경하면서 옮긴다.
                        System.IO.FileInfo fi = new System.IO.FileInfo(eventArgs.FilePath);
                        if (!fi.Exists) return;
                        fi.MoveTo(newName);

                        UIManager.Instance.ShowPopupCommon
                        (
                            Data.Localization.eKey.MICE_UI_SessionHall_FileDownload_Msg_Success.ToLocalizationString(),
                            () => OpenInFileBrowser.Open(Utils.Path.Downloads)
                        );
                    }
                    else if (eventArgs.Type == Vuplex.WebView.ProgressChangeType.Updated)
                    {
                        C2VDebug.Log($">>>>>>>>>> {eventArgs.Progress}");
                    }
                }));
        }


        public static string GetUniqueFileNameInPath(string folderPath, string fileName)
        {
            // 동일 파일 유무 체크 있다면 추가로 (index)를 붙힌다.
            var onlyFileName = Path.GetFileNameWithoutExtension(fileName);
            var onlyExtension = Path.GetExtension(fileName);
            int index = 0;
            string add = "";
            string filePath = string.Empty;
            while (true)
            {
                filePath = Path.Combine(folderPath + "\\", onlyFileName + add + onlyExtension);
                if (!File.Exists(filePath)) break;

                index++;
                add = $" ({index})";
            }

            return filePath;
        }



        public async UniTask ShowKioskSessionMenu(string url)
        {
            string combinedUrl = MakeWebViewRequestForm(url, MiceType.EventLounge); 
            C2VDebug.LogMethod(GetType().Name, combinedUrl);

            InitMiceRoomEnteringState();
            UIManager.Instance.ShowPopupMiceKioskWebView(new Vector2(1280, 800), combinedUrl, OnWebViewCreated, OnMessageEmitted);
            await UniTask.WaitUntil(() =>
            {
                return _kioskSessionMenuView != null;
            });
            
            void OnWebViewCreated(GUIView createdView)
            {
                _kioskSessionMenuView = createdView;
            }
	        
            void OnMessageEmitted(string message)
            {
                var kioskMessage = JsonUtility.FromJson<MiceKioskWebViewMessage>(message);
                ProcessKioskMessage(kioskMessage, _kioskSessionMenuView).Forget();
            }

            await UniTask.CompletedTask;
        }
        
        public async UniTask ShowKioskEventMenu(string url)
        {
            string combinedUrl = MakeWebViewRequestForm(url, MiceType.Lobby);
            C2VDebug.LogMethod(GetType().Name, combinedUrl);
            
            InitMiceRoomEnteringState();
            UIManager.Instance.ShowPopupMiceKioskWebView(new Vector2(1280, 800), combinedUrl, OnWebViewCreated, OnMessageEmitted);
            
            await UniTask.WaitUntil(() =>
            {
                return _kioskEventMenuView != null;
            });
            
            void OnWebViewCreated(GUIView createdView)
            {
                _kioskEventMenuView = createdView;
            }
	        
            void OnMessageEmitted(string message)
            {
                var kioskMessage = JsonUtility.FromJson<MiceKioskWebViewMessage>(message);
                ProcessKioskMessage(kioskMessage, _kioskEventMenuView).Forget();
            }
            await UniTask.CompletedTask;
        }

        public async UniTask ShowUIPopupSessionList(TriggerInEventParameter triggerInParameter = null)
        {
            await MiceInfoManager.Instance.SyncEventEntity(MiceService.Instance.EventID.value);
            
            _kioskEventMenuView = null;
            _kioskSessionMenuView = null;

            // 툴바에 있던 UI들을 닫는다.
            ViewModelManager.Instance.Get<MiceToolBarViewModel>()?.CloseAllUI();

            var view = await MiceUIEnterHallViewModel.ShowView();
            if (view.ViewModelContainer.TryGetViewModel(typeof(MiceUIEnterHallViewModel), out var viewModel))
            {
                var miceUISessionInfoViewModel = viewModel as MiceUIEnterHallViewModel;
                miceUISessionInfoViewModel?.SyncData(view, triggerInParameter);
            }
        }

        public async UniTask ShowUIPopupSessionInfo(Action<GUIView> onShow = null, Action<GUIView> onHide = null)
        {
            _uiSessionInfoView = await MiceUISessionInfoViewModel.ShowView(onShow, onHide);
            if (_uiSessionInfoView.ViewModelContainer.TryGetViewModel(typeof(MiceUISessionInfoViewModel), out var viewModel))
            {
                var miceUISessionInfoViewModel = viewModel as MiceUISessionInfoViewModel;
                miceUISessionInfoViewModel?.SyncData();
            }
        }

        public async UniTask ShowUIPopupOption(Action<GUIView> onShow = null, Action<GUIView> onHide = null, bool voiceOption = false)
        {
            await UIManager.Instance.CreatePopup("UI_Popup_Option", (guiView) =>
            {
                _uiMetaverseOptionView = guiView;

                guiView.Show();
                var viewModel = guiView.ViewModelContainer.GetViewModel<MetaverseOptionViewModel>();
                guiView.OnOpenedEvent += (guiView) =>
                {
                    viewModel.ScrollRectEnable = true;
                    if(onShow != null) { onShow(guiView); }
                };

                guiView.OnClosedEvent += (guiView) =>
                {
                    viewModel.ScrollRectEnable = false;
                    if (onHide != null) { onHide(guiView); }
                };

                if(voiceOption)
                {
                    viewModel.IsVoiceVideoOptionOn = true;
                }
            });
        }

        public void HideUIPopupOption()
        {
            if (!_uiMetaverseOptionView.IsUnityNull())
            {
                _uiMetaverseOptionView.Hide();
                _uiMetaverseOptionView = null;
            }
        }

        public void HideUISessionInfo()
        {
            if (!_uiSessionInfoView.IsUnityNull())
            {
                _uiSessionInfoView.Hide();
                _uiSessionInfoView = null;
            }
        }

        public async UniTask ShowUISessionDataDownload(Action<GUIView> onShow = null, Action<GUIView> onHide = null)
        {
            _uiSessionDataDownloadView = await MiceUIConferenceDataDownloadViewModel.ShowPopup(onShow, onHide);
            if (_uiSessionDataDownloadView.ViewModelContainer.TryGetViewModel(typeof(MiceUIConferenceDataDownloadViewModel), out var viewModel))
            {
                var dataDonwloadViewModel = viewModel as MiceUIConferenceDataDownloadViewModel;
                dataDonwloadViewModel?.SyncData(_uiSessionDataDownloadView);
            }
        }

        public bool HasDataDownloadFiles()
        {
            var sessionInfo = MiceService.Instance.GetCurrentSessionInfo();
            if (sessionInfo == null) return false;

            return sessionInfo.AttachmentFiles.Count > 0;
        }

        public void HideUISessionDataDownload()
        {
            if (!_uiSessionDataDownloadView.IsUnityNull())
            {
                _uiSessionDataDownloadView.Hide();
                _uiSessionDataDownloadView = null;
            }
        }

        public void ChangeCamera(int index)
        {
            CameraManager.Instance.ChangeState(eCameraState.FIXED_CAMERA, CameraList[index].ToString());
        }

        public void UpdateUserAuthority()
        {
            if (CurrentAreaType == eMiceAreaType.HALL)
            {
                _currentUserAuthority = MiceInfoManager.Instance.SelectProperUserAuthorityWithSessionID(SessionID);
            }
            else
            {
                _currentUserAuthority = MiceWebClient.eMiceAuthorityCode.NORMAL;
            }

            Notify();
            MiceInfoManager.Instance.Notify();
        }

        public bool HasCurrentEventTicket(MiceWebClient.eMiceAuthorityCode code)
        {
            return MiceInfoManager.Instance.HasTicket(code, EventID);
        }
        
        public void OnTeleportCompletion()
        {
            if (_serviceConnected)
            {
                C2VDebug.LogMethod(GetType().Name);
                _currentAreaCache?.OnTeleportCompletion();
                UserTeleportCompleted = true;
                Notify();
            }
        }

        public void OnBeforeSceneChanged(SceneBase prevScene, SceneBase currentScene)
        {
            if (prevScene    != null) SetExitWarpEffect(ConvertToMiceAreaType(prevScene.SceneProperty?.SpaceTemplate?.ID     ?? -1));
            if (currentScene != null) SetEnterWarpEffect(ConvertToMiceAreaType(currentScene.SceneProperty?.SpaceTemplate?.ID ?? -1));
        }

        public bool MiceUIShouldVisible()
        {
            bool miceState = _currentStateCache?.MiceUIShouldVisible() ?? true; 
            bool interactionState = _userInteractionState != eMiceUserInteractionState.WithWorldObject;
            return miceState && interactionState;
        }

        public void SetUserInteractionState(eMiceUserInteractionState stateType)
        {
            _userInteractionState = stateType;
            Notify();
        }
        
        #region 동영상 스트리밍 & 플레이용 웹페이지(video.html) 주소 생성
        
        // 비공식 값(HLS 스트리밍 구분용)
        public const string MEDIA_TYPE_CUSTOM_HLS   = "custom/hls";

        // <source> 태그의 공식 MEDIA TYPE 값
        public const string MEDIA_TYPE_VIDEO_OGG    = "video/ogg";
        public const string MEDIA_TYPE_VIDEO_MP4    = "video/mp4";
        public const string MEDIA_TYPE_VIDEO_WEBM   = "video/webm";
        public const string MEDIA_TYPE_AUDIO_OGG    = "audio/ogg";
        public const string MEDIA_TYPE_AUDIO_MPEG   = "audio/mpeg";
        
        private const string URL_FMT = "streaming-assets://{0}{1}";
        private const string URL_HTML = "video.html";
        private const string URL_QUERY_FMT = "?mediaurl='{0}'&mediatype='{1}'&showcontrols={2}&debug={3}";

        public static string GetHLSVideoWebPage(string sourceUrl, string mediaType = MEDIA_TYPE_CUSTOM_HLS, bool showControls = true, bool isDebug = false)
            => string.Format(URL_FMT, URL_HTML, string.Format(URL_QUERY_FMT, sourceUrl, mediaType, showControls ? "1" : "0", isDebug ? "1" : "0"));

#endregion  // 동영상 스트리밍 & 플레이용 웹페이지(video.html) 주소 생성


        public bool CheckReserveObject(Protocols.ObjectState objState)
        {
            return _currentAreaCache?.CheckReservedObject(objState) ?? false;
        }


        public void SetCurrentEventIntroSkip() =>
            MiceInfoManager.Instance.EventIntroSkipClickedPrefs.Click(EventID);

        public bool IsCurrentEventIntroSkip =>
            !MiceInfoManager.Instance.EventIntroSkipClickedPrefs.IsNew(EventID);

        public void SetCurrentEventTutorialSkip() =>
            MiceInfoManager.Instance.EventTutorialSkipClickedPrefs.Click(EventID);

        public bool IsCurrentEventTutorialSkip =>
            !MiceInfoManager.Instance.EventTutorialSkipClickedPrefs.IsNew(EventID);
        
        
        public void ShowHallExitPopup()
        {
            UIManager.Instance.ShowPopupYesNo(Data.Localization.eKey.MICE_UI_SessionHall_Exit_Popup_Title.ToLocalizationString(),
                                              Data.Localization.eKey.MICE_UI_SessionHall_Exit_Popup_Desc.ToLocalizationString(),
                                              (guiView) =>
                                              {
                                                  LastAreaChangeReason = eMiceAreaChangeReason.ExitButton;
                                                  RequestEnterLastMiceLounge().Forget();
                                              });
        }

        public bool CanPlayingLectureVideo()
        {
            return _currentStateCache?.CanPlayingLectureVideo() ?? false;
        }
    }
}