/*===============================================================
* Product:    Com2Verse
* File Name:  User.cs
* Developer:  haminjeong
* Date:       2022-05-09 14:38
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Com2Verse.Avatar;
using Com2Verse.AvatarAnimation;
using Com2Verse.CameraSystem;
using Com2Verse.Chat;
using Com2Verse.Data;
using Com2Verse.Director;
using Com2Verse.Extension;
using Com2Verse.InputSystem;
using Com2Verse.Loading;
using Com2Verse.Logger;
using Com2Verse.Pathfinder;
using Com2Verse.PlayerControl;
using Com2Verse.Project.InputSystem;
using Com2Verse.Sound;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Protocols;
using UnityEngine;
using UnityEngine.InputSystem;
using Localization = Com2Verse.UI.Localization;
using Vector3 = UnityEngine.Vector3;

namespace Com2Verse.Network
{
    public sealed class User : Singleton<User>, IDisposable
    {
        public UserCharacterStateViewModel CharacterStateViewModel { get; private set; }
#if ENABLE_CHEATING
        [field: SerializeField, ReadOnly] public string CheatType { get; set; } = string.Empty;
#endif // ENABLE_CHEATING
        public enum eConnectionState
        {
            UNKNOWN,
            CONNECTED,
            LOGIN_COMPLETE,
            STANDBY,
        }

        public bool Connected => _connectionState == eConnectionState.CONNECTED || _connectionState == eConnectionState.LOGIN_COMPLETE || _connectionState == eConnectionState.STANDBY;

        public bool LoginComplete => _connectionState == eConnectionState.LOGIN_COMPLETE || _connectionState == eConnectionState.STANDBY;

        public bool Standby => _connectionState == eConnectionState.STANDBY;

        private eConnectionState _connectionState = eConnectionState.UNKNOWN;

        private eConnectionState ConnectionState
        {
            get => _connectionState;
            set
            {
                _connectionState = value;

                if (ChatManager.InstanceExists)
                    ChatManager.Instance.SetUserStandby(Standby);
            }
        }

        public ActiveObject CharacterObject { get; private set; }

        public UserFunctionUI UserFunctionUI { get; private set; }

        public AvatarInfo AvatarInfo { get; private set; }

#if ENABLE_CHEATING
#if !METAVERSE_RELEASE
        public NetworkDebugViewModel NetworkDebugViewModel;
#endif
#endif // ENABLE_CHEATING


        private Action _onTeleportCompletion;

        public event Action OnTeleportCompletion
        {
            add
            {
                _onTeleportCompletion -= value;
                _onTeleportCompletion += value;
            }
            remove => _onTeleportCompletion -= value;
        }

        private long                           _prevServiceType    = -1;
        private long                           _currentServiceType = (long)eServiceID.WORLD;
        private Dictionary<long, BaseUserData> _userDataDictionary;
        public  long                           CurrentServiceType => _currentServiceType;
        public  long                           PrevServiceType    => _prevServiceType;
        
        [NotNull] public BaseUserData CurrentUserData => _userDataDictionary!.ContainsKey(_currentServiceType) ? _userDataDictionary![_currentServiceType]! : new();
        [NotNull] public BaseUserData DefaultUserData => _userDataDictionary![(long)eServiceID.WORLD]!;

        private User() { }

        public void Initialize()
        {
            ConnectionState     = eConnectionState.UNKNOWN;
            UserFunctionUI      = new UserFunctionUI();
            _userDataDictionary = new();

            NetworkManager.Instance.OnDisconnected += SetDisconnected;

            if (MapController.InstanceExists)
            {
                MapController.Instance.OnUpdateEvent += UpdateMapController;
                var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(DefinitionAttribute)));
                MapController.Instance.RegisterObjectCreators(types);
                types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(TagObjectTypeAttribute)));
                TagProcessorManager.Instance.RegisterTagProcessors(types);
                types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(ServiceIDAttribute)));
                RegisterUserDatas(types);
            }
            
            UpdateSession();
        }

        private void RegisterUserDatas(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                C2VDebug.Log($"-> {type.Name}");
                BaseUserData userData = Activator.CreateInstance(type) as BaseUserData;
                _userDataDictionary!.TryAdd(type.GetCustomAttribute<ServiceIDAttribute>().ServiceID, userData);
                C2VDebug.Log($"-> {type.Name}...DONE");
            }
        }

        private void UpdateMapController()
        {
#if ENABLE_CHEATING
#if !METAVERSE_RELEASE
            if (NetworkDebugViewModel != null)
            {
                NetworkDebugViewModel.RTTText         = $"RTT {NetworkManager.Instance.GetRtt().ToString()}ms";
                var mapController = MapController.InstanceOrNull;
                NetworkDebugViewModel.ObjectCountText = !mapController.IsUnityNull() && mapController!.IsEnableObjectCountLog ? $"Total:{mapController.TotalObjectCount}, Static:{mapController.StaticObjectCount}" : string.Empty;
            }
#endif
#endif // ENABLE_CHEATING
        }

        public void Dispose()
        {
            if (NetworkManager.InstanceExists)
                NetworkManager.Instance.OnDisconnected -= SetDisconnected;

            if (MapController.InstanceExists)
                MapController.Instance.OnUpdateEvent -= UpdateMapController;

            _userDataDictionary?.Clear();
            _userDataDictionary   = null;
            _onTeleportCompletion = null;
            _isRunningSession     = false;
            _isShownWarning       = false;
            if (InputSystemManager.InstanceExists)
                InputSystemManager.Instance.AnyButtonPressEvent -= OnAnyButtonPressed;
            
            SetDisconnected();
        }

        /// <summary>
        /// 현재 UserData를 바라보는 키 값을 변경합니다. 서비스 전환이 일어나면 반드시 호출되어야 합니다.
        /// </summary>
        /// <param name="serviceID">서비스 아이디</param>
        public void ChangeUserData(long serviceID)
        {
            if (serviceID == 0) return; // HACK : 서버 노티로 서비스 전환 값이 0으로 넘어오는 경우가 있음

            _prevServiceType    = _currentServiceType;
            _currentServiceType = serviceID;
        }

         
        public void DefaultTimeoutProcess(Action afterCallback = null)
        {
            var context = $"{Localization.Instance.GetString("UI_Error_Timeout_Popup_Desc")}\n[Timeout]";
            UIManager.Instance.HideWaitingResponsePopup();
            UIManager.Instance.ShowPopupConfirm(Localization.Instance.GetString("UI_Common_Notice_Popup_Title"), context,
                                                null,
                                                Localization.Instance.GetString("UI_Common_Btn_OK"), true, false,
                                                guiView =>
                                                {
                                                    void OnClosedAction(GUIView view)
                                                    {
                                                        guiView.OnClosedEvent -= OnClosedAction;
                                                        afterCallback?.Invoke();
                                                        LoadingManager.Instance.ChangeScene<SceneLogin>();
                                                    }

                                                    NetworkManager.Instance.Disconnect(false);
                                                    guiView.OnClosedEvent += OnClosedAction;
                                                });
        }

        /// <summary>
        /// 서버와 연결이 되면 상태를 바꿉니다.
        /// </summary>
        public void SetConnected()
        {
            C2VDebug.Log("Starting login process");
            C2VDebug.Log($"AccountID : {CurrentUserData.ID}");

            // 이미 연결 된 혹은 연결 중인 상태인 경우, Error 표시
            if (ConnectionState != eConnectionState.UNKNOWN)
            {
                // throw new System.Exception("Invalid user state");
                C2VDebug.LogError("Invalid User State");
                return;
            }

            ConnectionState = eConnectionState.CONNECTED;
        }

        public void OnLoginComplete()
        {
            //C2VDebug.Log($"Login complete: {loginComplete.Result}");
            ConnectionState = eConnectionState.LOGIN_COMPLETE;
        }

#region Logout
        public void OnLogout()
        {
            UpdateSession();
            _userDataDictionary?.Clear();
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(ServiceIDAttribute)));
            RegisterUserDatas(types);
            ChangeUserData((long)eServiceID.WORLD);
        }
#endregion

        public void OnClockSynced()
        {
            C2VDebug.Log("Clock synced");
            if (ConnectionState != eConnectionState.LOGIN_COMPLETE)
                C2VDebug.LogWarning($"Current user state {ConnectionState}");
        }

        /// <summary>
        /// 물리서버에게 동기화를 할 준비가 되었음을 알린다
        /// </summary>
        public void ReadyToReceive()
        {
            ConnectionState = eConnectionState.LOGIN_COMPLETE;
            if (NetworkUIManager.InstanceExists)
            {
                if (NetworkUIManager.Instance.TeleportTargetID == 0)
                    Commander.Instance.UsePortalRequestFinish();
                else
                    Commander.Instance.TeleportToUserPositionFinishNotify();
            }
            Commander.Instance.TeleportUserSceneLoadingCompletionNotify();
        }

        /// <summary>
        /// 커넥션 상태를 바꿔 그동안 CSU를 무시하도록 합니다.
        /// </summary>
        public void DiscardPacketBeforeStandBy()
        {
            ConnectionState = eConnectionState.LOGIN_COMPLETE;
        }

        public void RestoreStandBy()
        {
            ConnectionState = eConnectionState.STANDBY;
        }

        public void ReleaseMapObject(MapObject mapObject)
        {
            DetachComponents();
        }

        private void DetachComponents()
        {
            var playerController = PlayerController.InstanceOrNull;
            if (!playerController.IsUnityNull())
            {
                playerController!.EnableControl = false;
                playerController!.GestureHelper.Disable();
            }

            CharacterObject = null;

            UserFunctionUI?.Disable();
        }

        /// <summary>
        /// 연결이 끊기면 커넥션 상태를 초기상태로 바꿉니다.
        /// </summary>
        public void SetDisconnected()
        {
            DetachComponents();
            ConnectionState = eConnectionState.UNKNOWN;
        }

        public void OnSetCharacter(Protocols.WorldState.SetCharacter setCharacter)
        {
            OnSetCharacter(setCharacter.ObjectId);
        }

        public void OnSetCharacter(long objectId, bool isDebug = false)
        {
            C2VDebug.Log($"SetCharacter id {objectId.ToString()}");
            SetCharacter(objectId, isDebug).Forget();
        }

        private async UniTaskVoid SetCharacter(long objectId, bool isDebug = false)
        {
            CurrentUserData.ObjectID = objectId;

            var cameraManager   = CameraManager.Instance;
            var metaverseCamera = cameraManager.MetaverseCamera;
            if (metaverseCamera.IsUnityNull())
            {
                C2VDebug.LogErrorCategory(GetType().Name, "MetaverseCamera is null");
                return;
            }

            DetachComponents();
            InitBeforeCreateOtherAvatars();

            var mapController = MapController.Instance;
            var temp = mapController[CurrentUserData.ObjectID];
            while (temp.IsReferenceNull())
            {
                await UniTask.Yield();
                if (CurrentUserData == null) return;
                temp = mapController[CurrentUserData.ObjectID];
            }

            CharacterObject = temp.GetComponent<ActiveObject>();
            if (CharacterObject.IsReferenceNull())
            {
                C2VDebug.LogErrorCategory(GetType().Name, "CharacterObject is null");
                return;
            }

            cameraManager.UserAvatarTarget = CharacterObject;
            var hudCullingGroup = metaverseCamera.GetOrAddCullingGroupProxy(eCullingGroupType.HUD);
            hudCullingGroup.DistanceReferencePoint = CharacterObject!.transform;

            ModeManager.Instance.SetMode(eModeType.NORMAL);
            InitAfterSetCharacter(CharacterObject);

            await UniTask.Delay(TimeSpan.FromMilliseconds(ServerTime.SyncInterval), DelayType.Realtime);
            await UniTask.WaitUntil(() => UserDirector.Instance.IsInitialized);

            ConnectionState = eConnectionState.STANDBY;
            C2VDebug.Log($"SetCharacter Complete: {CurrentUserData.ID}");

            CameraManager.Instance.UpdateCameraView().Forget();

#if ENABLE_CHEATING
            if (isDebug) return;
#endif // ENABLE_CHEATING

            if (CharacterObject.transform.Find(AudioListenerManager.Instance.name) == null)
                SetAudioListener();

            var loadingPage = UIManager.Instance.GetSystemView(eSystemViewType.UI_LOADING_PAGE);
            if (!loadingPage.IsUnityNull())
                await UniTask.WaitUntil(() => loadingPage!.VisibleState == GUIView.eVisibleState.CLOSING);
            await UserDirector.Instance.WaitingForWarpEffect(true, null);

            Commander.Instance.TeleportActionCompletionNotify();

            SetPlayerMovement();

            MapController.Instance.StatePublisher = new ActiveObjectStatePublisher();
            MapController.Instance.StatePublisher.SetMyObject(CharacterObject);

            _onTeleportCompletion?.Invoke();

#if ENABLE_CHEATING
#if !METAVERSE_RELEASE
            UIManager.Instance.CreatePopup("UI_Cheat_Network_DebugInfo", (guiView) =>
            {
                guiView.Show();
                var newRoot = new GameObject("UIDebugRoot");
                guiView.gameObject.transform.SetParent(newRoot.transform);
                NetworkDebugViewModel = ViewModelManager.Instance.GetOrAdd<NetworkDebugViewModel>();
            }, true).Forget();
#endif
#endif // ENABLE_CHEATING
        }

        private void InitBeforeCreateOtherAvatars()
        {
        }

        private void InitAfterSetCharacter(ActiveObject characterObject)
        {
            var cameraManager = CameraManager.Instance;
            cameraManager.ChangeTarget(characterObject.transform);
            cameraManager.InitializeValueCurrentCamera();

            AvatarCreatorQueue.Instance.SetMyAvatar(CharacterObject.gameObject);

            UserFunctionUI.Enable();
            PlayerController.Instance.GestureHelper.Enable();
            AnimationManager.Instance.EnableEvents();

            CharacterStateViewModel = ViewModelManager.Instance.GetOrAdd<UserCharacterStateViewModel>();
            AvatarInfo = characterObject.AvatarController?.Info;
            CharacterStateViewModel.SetCharacterState(CharacterState.IdleWalkRun);
        }

        private void SetPlayerMovement()
        {
            if (CharacterObject.IsUnityNull()) return;
            CharacterObject!.IsNavigating = false;

            var playerController = PlayerController.Instance;
            playerController.SetMovementState(ClientPathFinding.Instance.enabled ? ePlayerMovementType.ONLY_CLIENT : ePlayerMovementType.WITH_SERVER);
            playerController.SetComponents(CharacterObject);
            playerController.IsControlledByServer = true;
            playerController.EnableControl        = true;
        }

        private void SetAudioListener()
        {
            var listenerObj = AudioListenerManager.Instance.ListenerObject;

            if (CharacterObject.IsUnityNull()) return;
            listenerObj.transform.SetParent (CharacterObject!.transform);
            listenerObj.transform.localPosition = new Vector3 (0, CharacterObject.ObjectHeight, 0);
            listenerObj.transform.localRotation = Quaternion.Euler (0, 180f, 0);
            listenerObj.transform.localScale    = Vector3.one;
        }

#region ServerAddress
        public void SetupServerAddress()
        {
            // TODO : Server Panel 지운 후 자동으로 Release 서버 + Domain 서버 연결되도록 변경하기
            var defaultConfig = Configurator.Instance.ConfigServer;

            if (defaultConfig != null)
            {
                C2VDebug.Log($"Default config is not null");
                var serverAddress = defaultConfig.Address;
                var serverPort = defaultConfig.Port;
                C2VDebug.Log($"Server Address = {serverAddress}:{serverPort}");
                if (GeneralData.General != null)
                {
                    NetworkManager.Instance.SetTimeoutTime(GeneralData.General.SocketTimeout);
                    _sessionTimeOutAlert = GeneralData.General.SessionTimeOutAlert * 1000;
                }
                NetworkManager.Instance.SetupServerAddress(serverAddress, serverPort, CurrentUserData.ID);
            }
            else
            {
                C2VDebug.Log("Default config is null");
            }

            {
                var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(ChannelAttribute)));
                NetworkManager.Instance.RegisterMessageProcessor(types);
                MapController.Instance.OnUpdateEvent -= UpdateMapController;
                MapController.Instance.OnUpdateEvent += UpdateMapController;
                types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(DefinitionAttribute)));
                MapController.Instance.RegisterObjectCreators(types);
                types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(TagObjectTypeAttribute)));
                TagProcessorManager.Instance.RegisterTagProcessors(types);
                types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(ServiceIDAttribute)));
                RegisterUserDatas(types);
            }
            
            UpdateSession();
        }
#endregion

#region Session
        private static readonly int LoopingTime          = 1000;
        private static          int _sessionTimeOut      = 1800 * 1000;
        private static          int _sessionTimeOutAlert = 180  * 1000;

        public void SetUserSessionTimeoutTime(int time)
        {
            _isCheckSession = time != -1;
            if (time > 0)
                _sessionTimeOut = time * 1000;
        }
        
        private long _expireSessionTime        = 0;
        private bool _isShownWarning           = false;
        private bool _isRunningSession         = false;
        private bool _isCheckSession           = true;
        public  bool ForcedDisableCheckSession = false;
        public void UpdateSession()
        {
            _expireSessionTime = MetaverseWatch.Realtime + _sessionTimeOut;
            _isShownWarning    = false;
            if (!_isRunningSession)
            {
                _isRunningSession = true;
                if (InputSystemManager.InstanceExists)
                    InputSystemManager.Instance.AnyButtonPressEvent += OnAnyButtonPressed;
                WaitingForIdleInput().Forget();
            }
        }

        private void OnAnyButtonPressed(InputControl control) => UpdateSession();

        private async UniTaskVoid WaitingForIdleInput()
        {
            while (_isRunningSession)
            {
                if (!Standby || !_isCheckSession || ForcedDisableCheckSession)
                {
                    await UniTask.Delay(LoopingTime, true);
                    UpdateSession();
                    continue;
                }
                if (!_isShownWarning && _expireSessionTime - _sessionTimeOutAlert < MetaverseWatch.Realtime)
                {
                    _isShownWarning = true;
                    UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_WorldSessionTimeOutAlert_Toast", _sessionTimeOut / (LoopingTime * 60)),
                                                        toastMessageType: UIManager.eToastMessageType.WARNING);
                }
                else if (_expireSessionTime < MetaverseWatch.Realtime)
                {
                    UIManager.Instance.ShowPopupConfirm(Localization.Instance.GetString("UI_Common_Notice_Popup_Title"),
                                                        Localization.Instance.GetString("UI_WorldSessionTimeOut_Popup_Desc"),
                                                        null,
                                                        Localization.Instance.GetString("UI_Common_Btn_OK"), true, false,
                                                        guiView =>
                                                        {
                                                            void OnClosedAction(GUIView view)
                                                            {
                                                                guiView.OnClosedEvent -= OnClosedAction;
                                                                LoadingManager.Instance.ChangeScene<SceneLogin>();
                                                            }

                                                            NetworkManager.Instance.Disconnect(true);
                                                            guiView.OnClosedEvent += OnClosedAction;
                                                        });
                    _isRunningSession = false;
                    _isShownWarning   = false;
                    InputSystemManager.Instance.AnyButtonPressEvent -= OnAnyButtonPressed;
                }
                await UniTask.Delay(LoopingTime, true);
            }
        }
#endregion
    }
}
