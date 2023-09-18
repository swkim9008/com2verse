/*===============================================================
* Product:		Com2Verse
* File Name:	UIManager_Popup.cs
* Developer:	tlghks1009
* Date:			2022-08-18 15:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.Extension;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Vuplex.WebView;
using Object = UnityEngine.Object;
using System.Threading;
using Com2Verse.LruObjectPool;

namespace Com2Verse.UI
{
    public partial class UIManager
    {
        private static class Constant
        {
            public static readonly string PopupWebView = "UI_Popup_WebView";
            public static readonly string PopupVideoWebView = "UI_Popup_VideoWebView";
            public static readonly string PopupMiceKioskWebView = "UI_Popup_MiceKioskWebView";
            public static readonly string PopupPostCodeWebView = "UI_Popup_PostCodeWebView";
        }

        public void ShowPopupCommon(string context, Action okAction = null, string yes = null, bool isActivation = false, bool allowCloseArea = true, Action<GUIView> onShowAction = null)
        {
            ShowPopupConfirm(Define.String.UI_Common_Notice_Popup_Title, context, okAction, yes, isActivation, allowCloseArea, onShowAction);
        }

        public void ShowPopupConfirm(string title, string context, Action okAction = null, string yes = null, bool isActivation = false, bool allowCloseArea = true, Action<GUIView> onShowAction = null)
        {
            var popupAddress = "UI_Popup_Confirm_Title";

            CreatePopup(popupAddress, (createdGuiView) =>
            {
                createdGuiView.Show();

                var commonPopupViewModel = createdGuiView.ViewModelContainer.GetViewModel<CommonPopupViewModel>();
                commonPopupViewModel.Title          = title;
                commonPopupViewModel.Context        = context;
                commonPopupViewModel.OnYesEvent     = okAction;
                commonPopupViewModel.Yes            = yes;
                commonPopupViewModel.AllowCloseArea = allowCloseArea;
                
                onShowAction?.Invoke(createdGuiView);
            }).Forget();
        }

        public void ShowPopupYesNo(string title, string context, Action<GUIView> okAction, Action<GUIView> noAction = null, Action<GUIView> guiCallback = null, string yes = null, string no = null, bool isActivation = false, bool allowCloseArea = true)
        {
            var popupAddress = "UI_Popup_YN_Title";

            CreatePopup(popupAddress, (createdGuiView) =>
            {
                createdGuiView.Show();

                var commonPopupViewModel = createdGuiView.ViewModelContainer.GetViewModel<CommonPopupYesNoViewModel>();
                commonPopupViewModel.GuiView        = createdGuiView;
                commonPopupViewModel.Context        = context;
                commonPopupViewModel.Title          = string.IsNullOrEmpty(title) ? Define.String.UI_Common_Notice_Popup_Title : title;
                commonPopupViewModel.Yes            = yes;
                commonPopupViewModel.No             = no;
                commonPopupViewModel.OnYesEvent     = okAction;
                commonPopupViewModel.OnNoEvent      = noAction;
                commonPopupViewModel.AllowCloseArea = allowCloseArea;

                guiCallback?.Invoke(createdGuiView);
            }).Forget();
        }

        public void ShowPopupYesNoCancel(string title, string context, Action<GUIView> okAction, Action<GUIView> noAction = null, Action<GUIView> cancelAction = null, Action<GUIView> guiCallback = null, string yes = null, string no = null,
                                         bool   isActivation = false, bool allowCloseArea = true)
        {
            var popupAddress = "UI_Popup_YNClose";

            CreatePopup(popupAddress, (createdGuiView) =>
            {
                createdGuiView.Show();

                var commonPopupViewModel = createdGuiView.ViewModelContainer.GetViewModel<CommonPopupYesNoViewModel>();
                commonPopupViewModel.GuiView        = createdGuiView;
                commonPopupViewModel.Context        = context;
                commonPopupViewModel.Title          = string.IsNullOrEmpty(title) ? Define.String.UI_Common_Notice_Popup_Title : title;
                commonPopupViewModel.Yes            = yes;
                commonPopupViewModel.No             = no;
                commonPopupViewModel.OnYesEvent     = okAction;
                commonPopupViewModel.OnNoEvent      = noAction;
                commonPopupViewModel.OnCancelEvent  = cancelAction;
                commonPopupViewModel.AllowCloseArea = allowCloseArea;

                guiCallback?.Invoke(createdGuiView);
            }).Forget();
        }

        public void ShowPopUpNotice(string title, string context, Action closeAction = null, bool isActivation = false, bool allowCloseArea = true)
        {
            var popupAddress = string.IsNullOrEmpty(title) ? "UI_Popup_Info" : "UI_Popup_Info_Title";

            CreatePopup(popupAddress, (createdGuiView) =>
            {
                createdGuiView.Show();

                var commonPopupViewModel = createdGuiView.ViewModelContainer.GetViewModel<CommonPopupNoticeViewModel>();
                commonPopupViewModel.Context        = context;
                commonPopupViewModel.Title          = title;
                commonPopupViewModel.AllowCloseArea = allowCloseArea;
                commonPopupViewModel.OnCloseEvent   = closeAction;
            }).Forget();
        }

        public void ShowPopUpEvent(string popupAddress, Action closeAction = null, bool allowCloseArea = true)
        {
            CreatePopup(popupAddress, (createdGuiView) =>
            {
                createdGuiView.Show();

                var commonPopupViewModel = createdGuiView.ViewModelContainer.GetViewModel<CommonPopupEventViewModel>();
                commonPopupViewModel.AllowCloseArea = allowCloseArea;
                commonPopupViewModel.OnCloseEvent   = closeAction;
            }).Forget();
        }

        public void ShowPopupScreenResolution(Action cancel)
        {
            var popupAddress = "UI_Popup_Option_Pannel_GraphicsPopup";
            
            CreatePopup(popupAddress, (createdGuiView) =>
            {
                createdGuiView.Show();

                var commonPopupViewModel = createdGuiView.ViewModelContainer.GetViewModel<ScreenResolutionCheckViewModel>();
                commonPopupViewModel.SetPopup(cancel);
            }).Forget();
        }

        public void ShowOrganizationYesNoPopup(string message, Action<GUIView, bool> onClick, string yes = null, string no = null, Vector3 popupPosition = default)
        {
            var popupAddress = "UI_Organization_Popup";
            CreatePopup(popupAddress, guiView =>
            {
                guiView.Show();

                var viewModel = guiView.ViewModelContainer.GetViewModel<OrganizationPopupViewModel>();
                viewModel.Text = message;
                viewModel.Yes = yes;
                viewModel.No = no;
                viewModel.SetOnClick(guiView, onClick);
                viewModel.PopupPosition = popupPosition;
            }).Forget();
        }


        public void ShowRebindPopup()
        {
            var popupAddress = "UI_Option_Control_HotkeySettings";

            CreatePopup(popupAddress, guiView =>
            {
                guiView.Show();
                var viewModel = guiView.ViewModelContainer.GetViewModel<RebindActionViewModel>();
                viewModel.CurrentView = guiView;
            }).Forget();
        }

        public void ShowNotificationPermissionPopUp()
        {
            var popupAddress = "UI_Notification_Permission_Popup";
            
            CreatePopup(popupAddress, guiView =>
            {
                guiView.Show();
                var notificationPermissionPopUpViewModel = guiView.ViewModelContainer.GetViewModel<NotificationPermissionPopUpViewModel>();
                notificationPermissionPopUpViewModel.CloseAction = () => { guiView.Hide(); };

            }).Forget();
        }

        public async UniTask CreatePopup(string addressableName, Action<GUIView> onLoadCompleted, bool needNewRoot = false, bool dontDestory = false)
        {
            // var assetAddressableName = $"{addressableName}.prefab";
            // var loadHandle           = C2VAddressables.LoadAssetAsync<GameObject>(assetAddressableName);
            // if (loadHandle == null)
            // {
            //     return;
            // }
            // var loadedAsset = await loadHandle.ToUniTask();

            var assetAddressableName = $"{addressableName}.prefab";
            var loadedAsset = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<GameObject>(assetAddressableName, null, dontDestory);
            InstantiateAndInitialize(loadedAsset, onLoadCompleted, needNewRoot);
        }


        public async UniTask CreatePopup(AssetReference assetReference, Action<GUIView> onLoadCompleted, bool needNewRoot = false)
        {
            // var loadHandle = C2VAddressables.LoadAssetAsync<GameObject>(assetReference);
            // if (loadHandle == null)
            // {
            //     return;
            // }
            // var loadedAsset = await loadHandle.ToUniTask();

            var loadedAsset = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<GameObject>(assetReference);
            InstantiateAndInitialize(loadedAsset, onLoadCompleted, needNewRoot);
        }


        public async UniTask LoadAsync(AssetReference assetReference, Action onLoadCompleted)
        {
            // var loadHandle = C2VAddressables.LoadAssetAsync<GameObject>(assetReference);
            // if (loadHandle == null)
            // {
            //     return;
            // }
            // var loadedAsset = await loadHandle.ToUniTask();

            var loadedAsset = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<GameObject>(assetReference);
            onLoadCompleted?.Invoke();
        }


        private void InstantiateAndInitialize(GameObject loadedAsset, Action<GUIView> onLoadCompleted, bool needNewRoot = false)
        {
            if (loadedAsset.IsReferenceNull())
            {
                return;
            }

            if (!needNewRoot && _loadedGuiViewDict.TryGetValue(loadedAsset.name, out var view))
            {
                if (!view.AllowDuplicate)
                {
                    view.UpdateOrganizer = this;
                    onLoadCompleted?.Invoke(view);
                    return;
                }
            }

            var guiViewObject = Object.Instantiate(loadedAsset);
            var guiView       = guiViewObject.GetComponent<GUIView>();

            guiView.UpdateOrganizer = this;
            guiView.IsStatic        = false;

            if (guiView.IsSystemView)
            {
                guiView.transform.SetParent(_systemCanvasRoot.transform);

                guiView.PositionInitialization();
            }
            else if (!guiView.DontDestroyOnLoad)
            {
                if (_currentCanvasRoot.IsUnityNull())
                {
                    Destroy(guiView);
                    return;
                }
                guiView.transform.SetParent(_currentCanvasRoot!.PopupLayer!.transform);

                guiView.PositionInitialization();
            }


            _uiNavigationController.RegisterEvent(guiView);
            _guiViewList.Add(guiView);

            if (!_loadedGuiViewDict.ContainsKey(loadedAsset.name) && !needNewRoot)
                _loadedGuiViewDict.Add(loadedAsset.name, guiView);

            onLoadCompleted?.Invoke(guiView);
        }

        public void SetGuiViewActive(bool isActive, params string[] ignoreList)
        {
            foreach (var canvasRoot in _canvasRootList!)
            {
                if (canvasRoot.IsUnityNull())
                    continue;

                canvasRoot!.SetChildActive(isActive, ignoreList);
            }
        }

        public void SetPopupLayerGuiViewActive(bool isActive, bool isSaveView, GUIView exceptView = null)
        {
            foreach (var canvasRoot in _canvasRootList!)
            {
                if (canvasRoot.IsUnityNull())
                    continue;

                canvasRoot.SetPopupLayerActive(isActive, isSaveView, exceptView);
            }
        }

#region WebView
        public void ShowPopupWebView(bool canChangeURL,
                                     Vector2 size,
                                     string url,
                                     Action<GUIView> webViewCreated = null,
                                     bool isLoginView = false,
                                     Action<string> messageEmitted = null,
                                     Action<DownloadChangedEventArgs> downloadProgressAction = null,
                                     Action<GUIView> onClosed = null)
        {
            ShowPopupWebView(new WebViewData
            {
                IsLoginView = isLoginView,
                WebViewSize = size,
                Url = url,
                CanChangeUrl = canChangeURL,
                OnMessageEmitted = messageEmitted,
                OnDownloadProgress = downloadProgressAction,
            }, OnWebViewCreated, onClosed);

            void OnWebViewCreated(GUIView guiView) => webViewCreated?.Invoke(guiView);
        }

        public void ShowPopupWebViewWithHeaders(bool canChangeURL, Vector2 size, string url, IEnumerable<KeyValuePair<string, string>> headers, Action<GUIView> onCompleted, Action<string> emittedAction)
        {
            ShowPopupWebView(new WebViewData
            {
                AdditionalHeaders = headers,
                DragMode = DragMode.DragToScroll,
                WebViewSize = size,
                Url = url,
                CanChangeUrl = canChangeURL,
                OnMessageEmitted = emittedAction
            }, OnLoadCompleted);

            void OnLoadCompleted(GUIView guiView) => onCompleted?.Invoke(guiView);
        }
        
        public void ShowPopupVideoWebView(Vector2 size, string url, bool showTimeline = true)
        {
            CreatePopup(Constant.PopupVideoWebView, guiView =>
            {
                guiView.Show();

                var vm = guiView.ViewModelContainer.GetViewModel<VideoWebViewModel>();
                vm.VideoWebViewSize = size;

                vm.VideoWebViewURL = url;
                vm.LoadUrlAsync(url, showTimeline).Forget();

                guiView.OnClosingEvent += OnClosingEvent;

                void OnClosingEvent(GUIView guiView)
                {
                    vm.OnCloseButtonClick();
                    guiView.OnClosedEvent -= OnClosingEvent;
                }
            }).Forget();
        }
        
        public void ShowPopupWebViewWithHtml(Vector2 size, string html)
        {
            ShowPopupWebView(new WebViewData
            {
                WebViewSize = size,
                Html = html,
            });
        }
        public void ShowPopupMiceKioskWebView(Vector2 size, string url, Action<GUIView> onLoadCompleted = null, Action<string> messageEmitted = null)
        {
            CreatePopup(Constant.PopupMiceKioskWebView, guiView =>
            {
                WebViewData data = new WebViewData()
                {
                    Url = url,
                    WebViewSize = size,
                    OnMessageEmitted = messageEmitted
                };
                
                guiView.Show();
                SetWebViewModel(guiView, data);
                onLoadCompleted?.Invoke(guiView);
            }).Forget();
        }
        public void ShowPopupWebView(WebViewData data, Action<GUIView> onLoadCompleted = null, Action<GUIView> onClosed = null)
        {
            CreatePopup(Constant.PopupWebView, guiView =>
            {
                _webViewGUI = guiView;
                guiView.Show();
                guiView.OnClosedEvent +=   onClosed;
                data.OnAutoClosed     ??= OnAutoClosed;
                data.OnClicked        ??= OnClicked;
                data.OnUseHandler     ??= OnUseHandler;
                SetWebViewModel(guiView, data);
                
                void OnAutoClosed()
                {
                    guiView.Hide();
                }
                void OnClicked()
                {
                    guiView.OnFocused();
                }

                void OnUseHandler(WebViewModel.WebViewHandler webViewHandler)
                {
                    webViewHandler.Reload();
                }

                onLoadCompleted?.Invoke(guiView);
            }).Forget();
        }

        public void UseHandlerAction()
        {
            if(_webViewGUI.ViewModelContainer.TryGetViewModel(typeof(WebViewModel), out var vm))
            {
                if (vm is WebViewModel webViewModel)
                {
                    webViewModel.OnUseHandlerAction();
                }
            }
        }
        
        private void SetWebViewModel(GUIView view, WebViewData data)
        {
            if (view.ViewModelContainer.TryGetViewModel(typeof(WebViewModel), out var vm))
            {
                if (vm is WebViewModel webViewModel)
                {
                    webViewModel.IsLoginView = data.IsLoginView.HasValue && data.IsLoginView.Value;
                    webViewModel.CanChangeURL = data.CanChangeUrl.HasValue && data.CanChangeUrl.Value;

                    webViewModel.WebViewSize = data.WebViewSize.HasValue ? data.WebViewSize.Value : new Vector2(1300, 800);
                    webViewModel.DragMode = data.DragMode.HasValue ? data.DragMode.Value : DragMode.DragWithinPage;

                    webViewModel.WebViewURL = data.Url;
                    webViewModel.WebViewHtml = data.Html;

                    webViewModel.SetAutoClose(data.OnAutoClosed);
                    webViewModel.SetMessageEmitted(data.OnMessageEmitted);
                    webViewModel.SetDownloadProgress(data.OnDownloadProgress);
                    webViewModel.SetTerminated(data.OnTerminated);
                    webViewModel.SetClicked(data.OnClicked);
                    webViewModel.SetUseHandler(data.OnUseHandler);

                    webViewModel.AdditionalHttpHeaders.Clear();
                    if (data.AdditionalHeaders != null)
                    {
                        foreach (var (key, value) in data.AdditionalHeaders)
                            webViewModel.AdditionalHttpHeaders.Add(key, value);
                    }
                    view.OnClosingEvent += (guiView) => webViewModel.OnCloseButtonClick();
                }
            }
        }
        
        public struct WebViewData
        {
            public bool? IsLoginView;
            public Vector2? WebViewSize;
            public bool? CanChangeUrl;
            public DragMode? DragMode;
            public bool? HasVideoPlayer;

            public Action<WebViewModel.WebViewHandler> OnUseHandler;
            // 주의 : URL, HTML 중 하나만 사용 !
            [CanBeNull] public string Url;
            [CanBeNull] public string Html;
            [CanBeNull] public Action OnAutoClosed;
            [CanBeNull] public Action<string> OnMessageEmitted;
            [CanBeNull] public Action<DownloadChangedEventArgs> OnDownloadProgress;
            [CanBeNull] public Action OnTerminated;
            [CanBeNull] public Action OnClicked;
            [CanBeNull] public IEnumerable<KeyValuePair<string, string>> AdditionalHeaders;
        }

        public UniTask<CanvasWebViewWrapper.PostCode.Data> ShowPopupPostCodeWebView()
        {
            UniTaskCompletionSource<CanvasWebViewWrapper.PostCode.Data> tcs = new();

            CreatePopup
            (
                Constant.PopupPostCodeWebView,
                guiView =>
                {
                    var lastIMEMode = Input.imeCompositionMode;

                    guiView.Show();
                    guiView.OnOpenedEvent += OnOpened;
                    guiView.OnClosedEvent += OnClosed;

                    void OnOpened(GUIView _)
                    {
                        Input.imeCompositionMode = IMECompositionMode.On;

                        StartMaintenanceICM(cancellationToken: guiView.GetCancellationTokenOnDestroy()).Forget();

                        guiView.OnOpenedEvent -= OnOpened;
                    }

                    void OnClosed(GUIView _)
                    {
                        StopMaintenanceICM();

                        Input.imeCompositionMode = lastIMEMode;

                        guiView.OnClosedEvent -= OnClosed;
                        guiView.Hide();

                        tcs.TrySetResult(default);
                    }

                    var vm = guiView.ViewModelContainer.GetViewModel<PostCodeWebViewModel>();
                    if (vm != null)
                    {
                        vm.Init
                        (
                            guiView,
                            data =>
                            {
                                guiView.OnClosedEvent -= OnClosed;
                                guiView.Hide();

                                tcs.TrySetResult(data);
                            }
                        )
                        .Forget();
                    }
                }
            )
            .Forget();

            return tcs.Task;
        }

        #region IMECompositionMode 값 유지용.
        private static CancellationTokenSource _tcsPollICM;
        private static void StopMaintenanceICM() => _tcsPollICM?.Cancel();

        private static async UniTaskVoid StartMaintenanceICM
        (
            IMECompositionMode maintenanceModeValue = IMECompositionMode.On,
            IMECompositionMode originalModeValue = IMECompositionMode.Auto,
            CancellationToken cancellationToken = default
        )
        {
            if (_tcsPollICM != null) return;

            try
            {
                Logger.C2VDebug.Log($"[MaintenanceICM] Start...");

                _tcsPollICM = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var token = _tcsPollICM.Token;

                Input.imeCompositionMode = maintenanceModeValue;

                while (!token.IsCancellationRequested)
                {
                    if (Input.imeCompositionMode != maintenanceModeValue)
                    {
                        Logger.C2VDebug.Log($"[MaintenanceICM] Detected Ime Composition Mode change. ({Input.imeCompositionMode} => {maintenanceModeValue})");
                        Input.imeCompositionMode = maintenanceModeValue;
                    }

                    await UniTask.Delay(500, true, cancellationToken: token);

                    token.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException)
            {
                Logger.C2VDebug.Log($"[MaintenanceICM] Canceled!");
            }
            finally
            {
                _tcsPollICM?.Dispose();
                _tcsPollICM = null;

                Input.imeCompositionMode = originalModeValue;

                Logger.C2VDebug.Log($"[MaintenanceICM] Done.");
            }
        }
        #endregion // IMECompositionMode 값 유지용.

        #endregion // WebView
    }
}
