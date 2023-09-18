/*===============================================================
* Product:		Com2Verse
* File Name:	PostCodeWebViewModel.cs
* Developer:	sprite
* Date:			2023-07-10 16:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Vuplex.WebView;
using Cysharp.Threading.Tasks;
using Com2Verse.Logger;
using System;
using Com2Verse.Network;

namespace Com2Verse.UI
{
	public sealed class PostCodeWebViewModel : ViewModelBase
    {
        private CanvasWebViewPrefab                         _canvasWebView;
        private RectTransform    _webViewRoot;

        public CommandHandler CloseButtonClick { get; }

        public RectTransform WebViewRoot
        {
            get => _webViewRoot;
            set => _webViewRoot = value;
        }

        private Action<CanvasWebViewWrapper.PostCode.Data> _onCompleted;

        public PostCodeWebViewModel()
        {
            this.CloseButtonClick = new CommandHandler(OnCloseButtonClick);
        }

        /*
        private static readonly string[] POSTCODE_URLS= new[]
        {
            @$"{Configurator.Instance.Config.MiceServerAddress}/postcode.html",
            @"https://nss-seoul.s3.ap-northeast-2.amazonaws.com/nss/mice/postcode.html",
            @"streaming-assets://postcode.html",
        };
        */

        public async UniTask Init(GUIView guiView, Action<CanvasWebViewWrapper.PostCode.Data> onCompleted)
        {
            await UniTask.WaitUntil(() => this.WebViewRoot != null && this.WebViewRoot);

            if (_canvasWebView == null || !_canvasWebView)
            {
                _canvasWebView ??= CanvasWebViewPrefab.Instantiate();
                _canvasWebView.DragMode = DragMode.DragWithinPage;
                _canvasWebView.transform.SetParent(this.WebViewRoot, false);

                await _canvasWebView.WaitUntilInitialized();

                _canvasWebView.WebView.MessageEmitted += this.OnWebViewMessageEmitted;
                _canvasWebView.WebView.FocusedInputFieldChanged += this.OnFocusedInputFieldChanged;
                _canvasWebView.WebView.ConsoleMessageLogged += this.OnConsoleMessageLogged;
                _canvasWebView.WebView.PageLoadFailed += this.OnPageLoadFailed;

                _canvasWebView.WebView.Resize((int)this.WebViewRoot.sizeDelta.x, (int)this.WebViewRoot.sizeDelta.y - 10);
            }

            //
            // daumPostCode 는 호스팅 서버가 필요함 (https://github.com/daumPostcode/QnA/issues/642)
            //

            //var url = "streaming-assets://postcode.html";
            //var url = "https://nss-seoul.s3.ap-northeast-2.amazonaws.com/nss/mice/postcode.html";
            var url = $"{Configurator.Instance.Config.MiceServerAddress}/postcode.html";

            C2VDebug.Log($"[PostCodeWebView] URL='{url}'");
            if (!await this.TestUrl(url))
            {
                C2VDebug.LogError($"[PostCodeWebView] Invalid Url! '{url}'");
                guiView.Hide();
                return;
            }

            //for (int i = 0, cnt = POSTCODE_URLS.Length; i < cnt; i++)
            //{
            //    var target = POSTCODE_URLS[i];
            //    C2VDebug.Log($"[PostCodeWebView] URL='{target}'");
            //    if (await this.TestUrl(target))
            //    {
            //        url = target;
            //        break;
            //    }
            //}
            //
            //if (string.IsNullOrEmpty(url))
            //{
            //    C2VDebug.Log("No valid Url found!");
            //    this.OnCloseButtonClick();
            //    return;
            //}

            _canvasWebView.WebView.LoadUrl(url);
            await _canvasWebView.WebView.WaitForNextPageLoadToFinish();

            this._onCompleted = onCompleted;
        }

        private void OnWebViewMessageEmitted(object sender, EventArgs<string> eventArgs)
        {
            var msg = CanvasWebViewWrapper.WebViewMessage.Parse(eventArgs.Value, _canvasWebView.WebView);
            C2VDebug.Log($"<color=cyan>[PostCodeWebView Message]</color>: {msg}");

            if (msg is CanvasWebViewWrapper.PostCode postCode && this._onCompleted != null)
            {
                this._onCompleted(postCode.data);

                this.OnCloseButtonClick();
            }
        }

        private void OnFocusedInputFieldChanged(object sender, FocusedInputFieldChangedEventArgs args)
        {
            if (args.Type == FocusedInputFieldType.Text)
            {
                Input.imeCompositionMode = IMECompositionMode.On;
            }
            else
            {
                Input.imeCompositionMode = IMECompositionMode.Auto;
            }
        }

        private void OnConsoleMessageLogged(object sender, ConsoleMessageEventArgs args)
        {
            C2VDebug.Log($"[PostCodeWebView]<{args.Level}> {args.Message} ({args.Source}:{args.Line})");
        }

        private void OnPageLoadFailed(object sender, EventArgs args)
        {
            C2VDebug.Log($"[PostCodeWebView] Load Failed! ({_canvasWebView.WebView.Url})");
        }

        public void OnCloseButtonClick()
        {
            if (_canvasWebView != null && _canvasWebView)
            {
                _canvasWebView.WebView.MessageEmitted -= this.OnWebViewMessageEmitted;
                _canvasWebView.WebView.FocusedInputFieldChanged -= this.OnFocusedInputFieldChanged;
                _canvasWebView.WebView.ConsoleMessageLogged -= this.OnConsoleMessageLogged;
                _canvasWebView.Destroy();
            }
            _canvasWebView = null;
        }

        private async UniTask<bool> TestUrl(string url)
        {
            bool result;
            UnityEngine.Networking.UnityWebRequest uwr = null;

            try
            {
                uwr = new UnityEngine.Networking.UnityWebRequest(url);
                await uwr.SendWebRequest().ToUniTask();

                result = true;
            }
            catch
            {
                result = false;
            }
            
            C2VDebug.Log($"[PostCodeWebView] Result = {result}, UWR Result = {uwr.result}, Response Code = {uwr.responseCode}, Error Message = '{uwr.error}'");

            return result;
        }
    }
}
