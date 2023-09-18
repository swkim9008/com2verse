/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebView.cs
* Developer:	sprite
* Date:			2023-07-13 11:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

//#define SHOW_WEBVIEW_CONSOLE_MESSAGE

using Com2Verse.Network;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using Protocols.Mice;
using Vuplex.WebView;
using TMPro;

namespace Com2Verse.Mice
{
    public partial class MiceWebView : MonoBehaviour
        , INamedLogger<NamedLoggerTag.Sprite>
        , IMapObjectPoolingEvent
    {
        [SerializeField] private Transform _webViewRoot;
        [SerializeField] public Renderer Renderer;
        [Range(1, 200)]
        [SerializeField] private float _resolution = 100;

        [Tooltip("사용자 정의 웹뷰 입력 처리 컴포넌트")]
        [SerializeField] private MiceWebViewPointerInputDetector _miceWebViewPointerInputDetector;
        [SerializeField] private Material _webViewMaterial;

        [Header("화면 해상도 강제 설정")]
        [Tooltip("화면 해상도를 강제로 설정 할 지 여부")]
        [SerializeField] private bool _forceSetSize = false;
        [Tooltip("강제로 설정 할 화면 해상도")]
        [SerializeField] private Vector2Int _forceSize = new(1920, 1080);

        [Header("곡면 설정")]
        [Tooltip("화면이 곡면인지 여부")]
        [SerializeField] private bool _isCurved = false;
        [Tooltip("화면 곡률 상수(0 이면 평면, 적당한 값을 넣어준다)")]
        [SerializeField] private float _curvature = 0.0f;
        
        [Header("UI")]
        [Tooltip("마우스 포인터가 Over 될 때 표시되는 UI")]
        [SerializeField] private GameObject _hoverUI;
        [Tooltip("스크린 전체 화면 전환")]
        [SerializeField] private Button _buttonZoomScreen;
        [Tooltip("외부 브라우저로 웹 페이지 열기")]
        [SerializeField] private Button _buttonOpenUrl;
        [SerializeField] private TextMeshProUGUI _tmpButton;

        public bool IsWebViewValid => _webView != null && _webView && _webView.WebView != null && _webView.WebView.IsInitialized;
        public bool IsWebPageLoaded { get; private set; } = false;
        public Texture WebViewTexture => _webView.WebView.Texture;

        private WebViewPrefab _webView;
        private bool _isBusyByInitializing;
        private DefaultPointerInputDetector _previousPointerDetector;
        private ISurface _surface;
        private bool _isValidSurface => _surface != null && _surface.IsValid;

        /// <summary>
        /// 강제 이동 중 인지 여부.
        /// </summary>
        private bool _isTeleporting = false;

        /// <summary>
        /// 튜토리얼 컷신 연출 중 인지 여부.
        /// </summary>
        private bool _isTutorialCutscenePlaying => MiceCutSceneManager.Instance != null && MiceCutSceneManager.Instance && MiceCutSceneManager.Instance.IsPlaying();

        /// <summary>
        /// 현재 동영상 총 길이(초)
        /// </summary>
        public float CurrentVideoDuration { get; private set; } = 0;
        public int CurrentVideoDurationMinutes { get; private set; } = 0;
        public int CurrentVideoDurationSeconds { get; private set; } = 0;
        public string DisplayCurrentVideoDuration => $"{this.CurrentVideoDurationMinutes:00}:{this.CurrentVideoDurationSeconds:00}";

        /// <summary>
        /// 현재 동영상 플레이 시간(초)
        /// </summary>
        public float CurrentVideoTime { get; private set; } = 0;
        public int CurrentVideoTimeMinutes { get; private set; } = 0;
        public int CurrentVideoTimeSeconds { get; private set; } = 0;
        public string DisplayCurrentVideoTime => $"{this.CurrentVideoTimeMinutes:00}:{this.CurrentVideoTimeSeconds:00}";

        public float CurrentVideoNormalizedTime => this.CurrentVideoTime / this.CurrentVideoDuration;

        public float CurrentVideoVolume => _lastVideoVolume;
        public float CurrentVideoPlaybackRate => _lastVideoPlaybackRate;

        partial void PartialInitScreenStateHandlers();
        partial void PartialInitUIDimmedPopupCanvas();

        protected virtual void Awake()
        {
            this.Log("Creating...");

            this.PartialInitScreenStateHandlers();
            this.PartialInitUIDimmedPopupCanvas();

            WebHelper.TrySetAutoplayEnabled(true);

            _isBusyByInitializing = false;
            _isTeleporting = false;

            this.BindPointerEvents();

            if (_hoverUI != null && _hoverUI)
            {
                _hoverUI.gameObject.SetActive(false);
            }

            if (_buttonZoomScreen != null && _buttonZoomScreen)
            {
                _buttonZoomScreen.onClick.AddListener(() =>
                {
                    this.OpenZoomScreen().Forget();
                });
            }

            if (_buttonOpenUrl != null && _buttonOpenUrl)
            {
                _buttonOpenUrl.onClick.AddListener(() =>
                {
                    Application.OpenURL(_webView.WebView.Url);
                });
            }

            PacketReceiver.Instance.OnMiceRoomNotifyEvent += this.OnMiceRoomNotify;
            MiceService.Instance.OnMiceStateChangedEvent += this.OnMiceServiceStateChange;

            this.SubscribeMapObjectPoolingEvent();

            this.RefreshButtonText();
        }

        partial void PartialTestStart();

        protected virtual void Start()
        {
            this.PartialTestStart();

            this.AppendToMainScreen();

            this.RefreshButtonText();
        }

        private void OnDestroy()
        {
            this.Log("Destroying...");

            this.ClearWebView();

            this.ReleasePointerEvents();

            PacketReceiver.Instance.OnMiceRoomNotifyEvent -= this.OnMiceRoomNotify;
            MiceService.Instance.OnMiceStateChangedEvent -= this.OnMiceServiceStateChange;

            this.UnsubscribeMapObjectPoolingEvent();

            if (this == MiceWebView.CurrentVideoPlayer)
            {
                MiceWebView.CurrentVideoPlayer = null;
            }
        }

        private void RefreshButtonText()
        {
            Data.Localization.eKey localKey = Data.Localization.eKey.MICE_UI_SessionHall_Btn_Zoom;

            if (_buttonZoomScreen != null && _buttonZoomScreen)
            {
                localKey = Data.Localization.eKey.MICE_UI_SessionHall_Btn_Zoom;
            }
            else if (_buttonOpenUrl != null && _buttonOpenUrl)
            {
                localKey = Data.Localization.eKey.MICE_UI_SessionHall_Btn_More;
            }

            if (_tmpButton != null && _tmpButton)
            {
                _tmpButton.text = localKey.ToLocalizationString();
            }
        }

        private async UniTask<bool> InitWebView()
        {
            if (_isBusyByInitializing)
            {
                this.Log("Already Initializing! Wait...");

                await UniTask.WaitWhile(() => _isBusyByInitializing);
                
                this.Log("Wait Done.(Initialized)");

                return true;
            }

            if (_webView != null && _webView)
            {
                var webViewIsValid = _webView.WebView != null && !_webView.WebView.IsDisposed;

                this.Log($"Already Initialized! (Validation:{webViewIsValid})");

                return webViewIsValid;
            }

            bool result = true;

            try
            {
                _isBusyByInitializing = true;

                var size = await this.InitSurface();

                Vector2 surfaceSize = size;
                if (_forceSetSize)
                {
                    surfaceSize = new(_forceSize.x / _resolution, _forceSize.y / _resolution);
                }
                else if (_isCurved)
                {
                    var baseLength = (size.x * 0.5f);
                    var heightLength = (size.z);
                    var slopeLength = (new Vector2(baseLength, heightLength)).magnitude;
                    surfaceSize.x = (slopeLength * 2.0f) * (1.0f + _curvature);
                }

                var aspectRatio = surfaceSize.x / surfaceSize.y;
                this.Log($"WebView Size: {Mathf.RoundToInt(surfaceSize.x * _resolution)}x{Mathf.RoundToInt(surfaceSize.y * _resolution)} (Aspect Ratio - {aspectRatio}:1)");

                _webView = WebViewPrefab.Instantiate(surfaceSize.x, surfaceSize.y);
                _webView.Resolution = _resolution;
                _webView.DragMode = DragMode.DragToScroll;
                _webView.transform.SetParent(_webViewRoot, false);
                _webView.transform.localPosition = Vector3.zero;

                await _webView.WaitUntilInitialized();

                this.Log("Wait for WebView...");
                await UniTask.WaitUntil(() => _webView.WebView != null);
                        
                _webView.WebView.MessageEmitted += this.OnMessageEmitted;
                _webView.WebView.UrlChanged += this.OnUrlChanged;
                _webView.WebView.LoadProgressChanged += this.OnLoadProgressChanged;
#if SHOW_WEBVIEW_CONSOLE_MESSAGE
                _webView.WebView.ConsoleMessageLogged += this.OnConsoleMessageLogged;
#endif

                _webView.Visible = false;

                _previousPointerDetector = _webView.GetComponentInChildren<DefaultPointerInputDetector>();

                this.ResetPointerInputDetector();

                this.ResetSurface();
            }
            catch (Exception e)
            {
                this.LogError(e);
                result = false;
            }
            finally
            {
                _isBusyByInitializing = false;

                this.Log(result ? "Initialized!" : "Failed!");
            }

            return result;
        }

        public async UniTask<bool> LoadUrl(string url)
        {
            this.Log($"Target Url: '{url}'");

            if (!await this.InitWebView())
            {
                this.LogWarning("Done. <color=red>Failed to initialize WebVew!</color>");
                return false;
            }

            this.IsWebPageLoaded = false;

            _webView.Visible = false;

            this.Log($"Loading... (isDisposed:{_webView.WebView.IsDisposed})");
            _webView.WebView.LoadUrl(url);            
            await _webView.WebView.WaitForNextPageLoadToFinish();

            this.IsWebPageLoaded = true;

            this.Log($"Done.");

            return this.IsWebPageLoaded;
        }

        private void ClearWebView()
        {
            if (_webView != null && _webView)
            {
                this.Video_Stop().Forget();
                this.Video_HLSDestroy();

                // 웹뷰 입력 컨트롤을 변경했었다면 원상복구 시킨다.
                this.ResetPointerInputDetector();
                _previousPointerDetector = null;

                _webView.WebView.MessageEmitted -= this.OnMessageEmitted;
                _webView.WebView.UrlChanged -= this.OnUrlChanged;
                _webView.WebView.LoadProgressChanged -= this.OnLoadProgressChanged;
#if SHOW_WEBVIEW_CONSOLE_MESSAGE
                _webView.WebView.ConsoleMessageLogged -= this.OnConsoleMessageLogged;
#endif

                _webView.Destroy();
                _webView = null;

                IsWebPageLoaded = false;
            }
        }

#region Event Handlers
        void IMapObjectPoolingEvent.OnAllocMapObject()
        {
            this.Log();

            this.InitWebView().Forget();

            _isTeleporting = false;

            this.RefreshButtonText();
        }

        void IMapObjectPoolingEvent.OnFreeMapObject()
        {
            this.Log();

            if (_webView != null && _webView && this.IsVideoState)
            {
                this.Log("Video Stop.");
                this.Video_Stop().Forget();
            }
        }

        private void OnMiceRoomNotify(MiceRoomNotify response)
        {
            if (response.MiceType == MiceType.ConferenceSession && response.NotiEvent == NotifyEvent.Close && !_isTeleporting)
            {
                this.CloseZoomScreen();
                _isTeleporting = true;
            }
        }

        private eMiceServiceState _currentMiceServiceState
        {
            get
            {
                if (!MiceService.InstanceExists) return eMiceServiceState.NONE;
                return MiceService.Instance.CurrentStateType;
            }
        }

        public bool CanPlayingLectureVideo => MiceService.Instance.CanPlayingLectureVideo(); 
        public bool IsMiceServiceCutScene => _currentMiceServiceState == eMiceServiceState.PLAYING_CUTSCENE;

        /// <summary>
        /// Mice Service 상태가 CutScene 일 경우 끝 날 때까지 대기한다.
        /// </summary>
        /// <returns></returns>
        private async UniTask WaitForMiceServiceStateCutScene([System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = null)
        {
            this.Log($"MiceServiceState = {_currentMiceServiceState}");

            if (!this.IsMiceServiceCutScene) return;

            this.Log($"({callerMemberName}) Wait For CutScene Finished.");
            await UniTask.WaitWhile(() => this.IsMiceServiceCutScene);
            this.Log($"({callerMemberName}) Wait Done.");
        }

        private void CheckMiceServiceStateForVideo(bool force = false)
        {
            this.Log($"MiceServiceState = {_currentMiceServiceState}");

            if (!force && !this.IsVideoState)
            {
                this.Log($"Not a Video State ({this.CurrentState})");
                return;
            }

            var state = _currentMiceServiceState;

            if (state == eMiceServiceState.PLAYING_CUTSCENE || !CanPlayingLectureVideo)
            {
                this.Log($"Video Muted & Paused because (MiceServiceState is {state} or CanPlayingLectureVideo is {CanPlayingLectureVideo}).");
                this.Video_Mute(true)
                    .ContinueWith(_ => this.Video_Stop())
                    .Forget();
            }
            else
            {
                if (_isPaused)
                {
                    this.Log($"Video Play & Unmuted. ({state})");
                    this.Video_Play()
                        .ContinueWith(_ => this.Video_Mute(false))
                        .Forget();
                }

                this.Log($"Video Unmuted. ({state})");
                this.Video_Mute(false).Forget();
            }

            /*
            switch (state)
            {
                case eMiceServiceState.PLAYING_CUTSCENE:
                {
                    this.Log($"Video Muted & Paused because MiceServiceState is {state}.");
                    this.Video_Mute(true)
                        .ContinueWith(_ => this.Video_Stop())
                        .Forget();
                    break;
                }

                default:
                {
                    if (_isPaused)
                    {
                        this.Log($"Video Play & Unmuted. ({state})");
                        this.Video_Play()
                            .ContinueWith(_ => this.Video_Mute(false))
                            .Forget();
                    }

                    this.Log($"Video Unmuted. ({state})");
                    this.Video_Mute(false).Forget();
                    break;
                }
            }
            */
        }

        private eMiceServiceState _lastMiceServiceState = eMiceServiceState.NONE;

        private void OnMiceServiceStateChange()
        {
            var state = MiceService.Instance.CurrentStateType;

            if (_lastMiceServiceState == state) return;

            _lastMiceServiceState = state;

            this.CheckMiceServiceStateForVideo();
        }

        private void OnMessageEmitted(object sender, EventArgs<string> eventArgs)
        {
            //this.Log(eventArgs.Value);

            if (this.CurrentState == eScreenState.WEB_PAGE)
            {
                bool result;
                try
                {
                    var kioskMsg = JsonUtility.FromJson<MiceKioskWebViewMessage>(eventArgs.Value);

                    if (Enum.TryParse(kioskMsg.MessageType, out eMiceKioskWebViewMessageType value) && value == eMiceKioskWebViewMessageType.OpenUrl)
                    {
                        this.Log($"OpenUrl => '{kioskMsg.Url}'");
                        Application.OpenURL(kioskMsg.Url);

                        result = true;
                    }
                    else
                    {
                        result = false;
                        this.LogError($"Invalid Kiosk Message Type. Only 'OpenUrl' is allowed. ({kioskMsg.MessageType})");
                    }
                }
                catch
                {
                    result = false;
                }

                if (result) return;
            }

            var msg = MiceWebViewMessage.Parse(eventArgs.Value, _webView.WebView);
            if (msg != null && (msg is not MiceWebViewMessage.VideoEvent ve || ve.EventType != MiceWebViewMessage.VideoEvent.Evt.timeupdate))
            {
                this.Log(msg);
            }

            if (msg is MiceWebViewMessage.VideoRes videoRes)
            {
                this.Log($"Resize Video to {videoRes.resolution.width}x{videoRes.resolution.height} (Duration:{videoRes.resolution.duration})");

                _webView.WebView.Resize(videoRes.resolution.width, videoRes.resolution.height);
                _webView.Resolution = 100;
                _resolution = 100;
                this.CurrentVideoDuration = videoRes.resolution.duration;
                this.CurrentVideoDurationMinutes = Mathf.FloorToInt(this.CurrentVideoDuration / 60.0f);
                this.CurrentVideoDurationSeconds = Mathf.RoundToInt(this.CurrentVideoDuration % 60.0f);
                this.CurrentVideoTime = 0;
                this.CurrentVideoTimeMinutes = 0;
                this.CurrentVideoTimeSeconds = 0;
            }
            else if (msg is MiceWebViewMessage.VideoEvent videoEvent)
            {
                switch (videoEvent.EventType)
                {
                    case MiceWebViewMessage.VideoEvent.Evt.volumechange:
                    case MiceWebViewMessage.VideoEvent.Evt.playing:
                    {
                        if (videoEvent.EventType == MiceWebViewMessage.VideoEvent.Evt.playing && _isPaused)
                        {
                            _isPaused = false;

                            this.Log($"Video Paused={_isPaused}");
                        }

                        this.CheckMiceServiceStateForVideo();

                        if (videoEvent.EventType == MiceWebViewMessage.VideoEvent.Evt.volumechange && !_isMuted)
                        {
                            this.Video_GetVolume()
                                .ContinueWith
                                (
                                    value =>
                                    {
                                        _lastVideoVolume = value;
                                        this.Log($"({videoEvent.EventType}) Current Video Volume = {_lastVideoVolume}");
                                    }
                                );

                            this.Video_GetPlaybackRate()
                                .ContinueWith
                                (
                                    value =>
                                    {
                                        _lastVideoPlaybackRate = value;
                                        this.Log($"({videoEvent.EventType}) Current Video Playback Rate = x{_lastVideoPlaybackRate}");
                                    }
                                );
                        }

                        break;
                    }

                    case MiceWebViewMessage.VideoEvent.Evt.timeupdate:
                    {
                        var jobj = Newtonsoft.Json.Linq.JObject.Parse(videoEvent.body);

                        if (this.CurrentVideoDuration == 0)
                        {
                            this.CurrentVideoDuration = SafeGetValueFromJToken<float>(jobj["duration"]);
                            this.CurrentVideoDurationMinutes = Mathf.FloorToInt(this.CurrentVideoDuration / 60.0f);
                            this.CurrentVideoDurationSeconds = Mathf.RoundToInt(this.CurrentVideoDuration % 60.0f);
                        }

                        this.CurrentVideoTime = SafeGetValueFromJToken<float>(jobj["currentTime"]);
                        this.CurrentVideoTimeMinutes = Mathf.FloorToInt(this.CurrentVideoTime / 60.0f);
                        this.CurrentVideoTimeSeconds = Mathf.RoundToInt(this.CurrentVideoTime % 60.0f);

                        if (!CanPlayingLectureVideo && this.IsVideoState)
                        {
                            this.Log("(CanPlayingLectureVideo) Stop Immediately!");

                            this.Video_Stop().Forget();
                        }
                        break;
                    }
                }

                static T SafeGetValueFromJToken<T>(Newtonsoft.Json.Linq.JToken jtoken)
                    => jtoken != null && jtoken.HasValues ? jtoken.ToObject<T>() : default;
            }
            else if (msg is MiceWebViewMessage.WebLink webLink)
            {
                Application.OpenURL(webLink.data.linkUrl);
            }
            else
            {
                this.Log(eventArgs.Value);
            }
        }

        private void OnUrlChanged(object sender, UrlChangedEventArgs args)
        {
            this.Log(args.Url);
        }

        private void OnLoadProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            this.Log($"{args.Progress * 100.0f:0.00}%");
        }

#if SHOW_WEBVIEW_CONSOLE_MESSAGE
        private void OnConsoleMessageLogged(object sender, ConsoleMessageEventArgs args)
        {
            this.Log($"[JS_Console]<{args.Level}> {args.Message} ({args.Source}:{args.Line})");
        }
#endif

#endregion  // Event Handlers

        #region Control PointerInputDetector
        /// <summary>
        /// 유효한 PointerInputDetector를 가져온다.
        /// </summary>
        /// <param name="value">false이면 기본값을 가져온다</param>
        /// <returns></returns>
        private IPointerInputDetector GetValidPointerInputDetector(bool value)
        {
            IPointerInputDetector pointerInputDetector = (_miceWebViewPointerInputDetector != null && _miceWebViewPointerInputDetector) 
                ? _miceWebViewPointerInputDetector 
                : _previousPointerDetector;

            return value ? pointerInputDetector : _previousPointerDetector;
        }

        /// <summary>
        /// 주어진 Transform에서 IPointerInputDetector를 찾아 설정한다.
        /// 찾지 못한 경우 아무것도 하지 않는다.
        /// </summary>
        /// <param name="transform"></param>
        private void SetPointerInputDetectorFrom(Transform transform)
        {
            // 웹뷰 입력 컨트롤을 찾는다.
            if (transform == null || !transform || !transform.TryGetComponent(out IPointerInputDetector pointerInputDetector)) return;

            this.SetPointerInputDetector(pointerInputDetector: pointerInputDetector);
        }

        /// <summary>
        /// PointerInputDetector를 설정한다.
        /// </summary>
        /// <param name="value">false인 경우 기본값으로 설정한다.</param>
        /// <param name="pointerInputDetector">이 값이 유효한 경우 value 값에 상관없이 이 값으로 설정한다</param>
        private void SetPointerInputDetector(bool value = true, IPointerInputDetector pointerInputDetector = null)
            => _webView.SetPointerInputDetector(pointerInputDetector != null ? pointerInputDetector : this.GetValidPointerInputDetector(value));

        /// <summary>
        /// PointerInputDetector를 기본값으로 설정한다.
        /// </summary>
        private void ResetPointerInputDetector()
            => _webView.SetPointerInputDetector(this.GetValidPointerInputDetector(false));

        private void BindPointerEvents()
        {
            if (_miceWebViewPointerInputDetector == null || !_miceWebViewPointerInputDetector) return;

            _miceWebViewPointerInputDetector.PointerEntered += this.OnPointerEntered;
            _miceWebViewPointerInputDetector.PointerExited += this.OnPointerExited;
        }

        private void ReleasePointerEvents()
        {
            if (_miceWebViewPointerInputDetector == null || !_miceWebViewPointerInputDetector) return;

            _miceWebViewPointerInputDetector.PointerEntered -= this.OnPointerEntered;
            _miceWebViewPointerInputDetector.PointerExited -= this.OnPointerExited;
        }

        private void OnPointerEntered(object sender, EventArgs eventArgs)
        {
            //if (MiceService.Instance.UserInteractionState != eMiceUserInteractionState.WithWorldObject)
            if (this.IsDimmedPopupVisible)
            {
                this.Log($"Dimmed UI is visibled.");
                return;
            }

            //Project.InputSystem.InputSystemManagerHelper
            if (_isTutorialCutscenePlaying || _isTeleporting || !_isAbleToZoomableState)
            {
                this.Log("Not allow now.");
                return;
            }

            if (this.CurrentState == eScreenState.WEB_PAGE && !_hasMoreInfoUrl)
            {
                this.Log("Has Not Infomation URL.");
                return;
            }

            if (_hoverUI != null && _hoverUI && !_hoverUI.activeSelf)
            {
                _hoverUI.SetActive(true);
            }
        }

        private void OnPointerExited(object sender, EventArgs eventArgs)
        {
            if (_hoverUI != null && _hoverUI && _hoverUI.activeSelf)
            {
                _hoverUI.SetActive(false);
            }
        }
#endregion  // Control PointerInputDetector

#region Control Surface
        private UniTask<Vector3> InitSurface() => this.CreateSurface(default(Component));

        private async UniTask<Vector3> CreateSurface<T>(T source)
            where T : Component
        {
            _surface = source != null && source ? ISurface.Factory.CreateFrom(source) : ISurface.Factory.CreateFrom(this.Renderer);
            UnityEngine.Assertions.Assert.IsNotNull(_surface, "_surface is null!");

            await _surface.Init();

            return _surface.Size;
        }

        /// <summary>
        /// 출력 화면을 설정한다.
        /// </summary>
        /// <param name="texture">사용자 정의 이미지 텍스쳐</param>
        /// <param name="flip">상하 반전 여부</param>
        /// <param name="pointerInput">마우스 입력 사용 여부</param>
        /// <returns></returns>
        private bool SetSurface(Texture texture = null, bool flip = false, bool pointerInput = false, bool applyTexCoord = false, Rect texCoord = default)
        {
            bool result = true;

            if (_isValidSurface && _surface.IsValid)
            {
                if (texture == null || !texture) texture = this.WebViewTexture;

                if (texture != null && texture)
                {
                    this.Log($"Change Texture. ({texture.width}x{texture.height}, {texture.graphicsFormat})");
                }

                _surface.SetMaterial(_webViewMaterial);
                _surface.SetTexture(texture);
                _surface.Flip(flip);
                this.Log($"Flip ({flip})");
                _surface.SetTexCoord(applyTexCoord, texCoord);
                if (applyTexCoord) this.Log($"TexCoord: {texCoord}");
            }
            else
            {
                this.Log("Surface is invalid!");
                result = false;
            }

            if (_webView != null && _webView)
            {
                _webView.Resolution = _resolution;

                this.SetPointerInputDetector(pointerInput);
                this.Log($"Resolution ({_resolution}), Pointer Input ({pointerInput})");
            }
            else
            {
                this.Log("WebView is invalid!");
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 출력 화면을 원래 설정으로 복구한다.
        /// </summary>
        /// <returns></returns>
        private bool ResetSurface()
        {
            bool result = true;

            if (_isValidSurface && _surface.IsValid)
            {
                _surface.SetTexture(default);
                _surface.Flip(false);
                _surface.ResetMaterial();
            }
            else
            {
                this.Log("Surface is invalid!");
                result = false;
            }

            if (_webView != null && _webView)
            {
                _webView.Resolution = _resolution;

                this.ResetPointerInputDetector();
            }
            else
            {
                this.Log("WebView is invalid!");
                result = false;
            }

            return result;
        }
#endregion  // Control Surface
    }

    public partial class MiceWebView : IUpdateTag<WebScreenTag>
    {
        /**
         * UpdateTag가 여러번 호출되는 경우를 대비하여 동기화 객체 적용.
         * (마지막 UpdateTag 호출이 최종적으로 적용된다)
         */

        private volatile System.Threading.SemaphoreSlim _semaphore = null;

        /// <summary>
        /// 웹 페이지를 표시 하고 있을 때, 추가 정보 페이지(URL)가 존재하는지 여부.
        /// </summary>
        private bool _hasMoreInfoUrl = false;
        private string _lastDisplayUrl = string.Empty;

        void IUpdateTag<WebScreenTag>.UpdateTag(WebScreenTag tag)
        {
            if (string.Equals(_lastDisplayUrl, tag.DisplayingPageUrl))
            {
                this.Log($"Already displaying. ({tag.DisplayingPageUrl})");
                return;
            }

            var dispUrl = tag.DisplayingPageUrl;
            var moreUrl = tag.MoreInfoPageUrl;

            _lastDisplayUrl = dispUrl;

            this.Log($"Display URL = '{dispUrl}', MoreInfo URL = '{moreUrl}'");

            var token = this.GetCancellationTokenOnDestroy();

            UniTask.Void(async () =>
            {
                try
                {
                    if (_semaphore == null)
                    {
                        this.Log("Create Semaphore.");
                    }
                    else
                    {
                        this.Log("Use Semaphore.");
                    }

                    _semaphore ??= new(1, 1);
                    
                    this.Log($"Wait for signal... ({dispUrl})");

                    await _semaphore.WaitAsync(cancellationToken: token);
                    {
                        this.Log($"Signal received! ({dispUrl})");
                        this.Log($"Loading... ({dispUrl})");

                        await this.SetScreenState(eScreenState.WEB_PAGE, dispUrl, token);

                        this.Log($"Load {(this.IsWebPageLoaded ? "Completed." : "Failed.")} ({dispUrl})");

                        if (this.IsWebPageLoaded && _buttonOpenUrl != null && _buttonOpenUrl)
                        {
                            _hasMoreInfoUrl = false;
                            _buttonOpenUrl.onClick.RemoveAllListeners();
                            if (!string.IsNullOrEmpty(moreUrl))
                            {
                                _hasMoreInfoUrl = true;
                                _buttonOpenUrl.onClick.AddListener(() => Application.OpenURL(moreUrl));
                            }
                        }
                    }

                    _semaphore.Release();
                }
                finally
                {
                    if (_semaphore != null)
                    {
                        if (_semaphore.CurrentCount == 0)
                        {
                            _semaphore.Dispose();
                            _semaphore = null;

                            this.Log("Semaphore disposed.");
                        }
                        else
                        {
                            this.Log($"Semaphore released. (Remain:{_semaphore.CurrentCount})");
                        }
                    }
                }
            });            
        }
    }
}

namespace Com2Verse.Mice
{
    public partial class MiceWebView
    {
        private Canvas _dimmedPopupCanvas;
        public bool IsDimmedPopupVisible => _dimmedPopupCanvas?.isActiveAndEnabled ?? false;

        partial void PartialInitUIDimmedPopupCanvas()
        {
            var dimmedPopup = UIManager.Instance.GetSystemView(eSystemViewType.UI_POPUP_DIMMED);
            if (dimmedPopup != null || dimmedPopup)
            {
                _dimmedPopupCanvas = dimmedPopup.GetComponent<Canvas>();
            }
        }
    }
}

namespace Com2Verse.Mice
{
    public partial class MiceWebView
    {
        public static MiceWebView CurrentVideoPlayer { get; private set; } = null;
    }
}
