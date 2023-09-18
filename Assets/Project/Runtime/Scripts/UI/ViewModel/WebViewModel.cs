/*===============================================================
* Product:		Com2Verse
* File Name:	WebViewModel.cs
* Developer:	haminjeong
* Date:			2022-07-22 13:15
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Extension;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using Vuplex.WebView;

namespace Com2Verse.UI
{
	public sealed class WebViewModel : ViewModelBase
	{
		CanvasWebViewPrefab                      _canvasWebView;
		private Action                           _autoCloseAction;
		private Action<string>                   _messageEmittedAction;
		private Action<DownloadChangedEventArgs> _downloadProgressAction;
		private Action                           _onTerminated;
		private Action                           _onClicked;
		private Dictionary<string, string>       _additionalHttpHeaders = new();
		private bool                             _userChangedURL        = false;
		private TMP_InputField                   _webViewIME;
		private bool                             _isLoginView = false;
		private Action<WebViewHandler>           _onUseHandler;
		private Action _onPageLoaded;

		private RectTransform _webViewRoot;
		private Vector2       _webViewSize;
		private string        _webViewURL   = string.Empty;
		private string        _webViewHtml = string.Empty;
		private bool          _canChangeURL = false;
		private float _alpha = 1.0f;
		public           CommandHandler                  CloseButtonClick    { get; }
		public           CommandHandler                  BackwardButtonClick { get; }
		public           CommandHandler                  ForwardButtonClick  { get; }
		public           CommandHandler                  ChangeWebViewURL    { get; }

		private WebViewHandler _handler;
		
		public WebViewModel()
		{
			CloseButtonClick = new CommandHandler(OnCloseButtonClick);
			BackwardButtonClick = new CommandHandler(OnBackwardButtonClick);
			ForwardButtonClick = new CommandHandler(OnForwardButtonClick);
			ChangeWebViewURL = new CommandHandler(OnChangeWebViewURL);
		}

		// Property
		public event Action AutoCloseAction
		{
			add
			{
				_autoCloseAction -= value;
				_autoCloseAction += value;
			}
			remove => _autoCloseAction -= value;
		}

		public event Action<string> MessageEmittedAction
		{
			add
			{
				_messageEmittedAction -= value;
				_messageEmittedAction += value;
			}
			remove => _messageEmittedAction -= value;
		}

		public event Action<DownloadChangedEventArgs> DownloadProgressAction
		{
			add
			{
				_downloadProgressAction -= value;
				_downloadProgressAction += value;
			}
			remove => _downloadProgressAction -= value;
		}

		public event Action OnTerminated
		{
			add
			{
				_onTerminated -= value;
				_onTerminated += value;
			}
			remove => _onTerminated -= value;
		}

		public event Action OnClicked
		{
			add
			{
				_onClicked -= value;
				_onClicked += value;
			}
			remove => _onClicked -= value;
		}

		public RectTransform WebViewRoot
		{
			get => _webViewRoot;
			set => _webViewRoot = value;
		}

		public RectTransform WebViewIME
		{
			get => null;
			set
			{
				if (_webViewIME.IsReferenceNull())
				{
					_webViewIME = value.GetComponent<TMP_InputField>();
					_webViewIME.Select();
					_webViewIME.interactable = false;
				}
			}
		}

		public Dictionary<string, string> AdditionalHttpHeaders => _additionalHttpHeaders;

		public Vector2 WebViewSize
		{
			get => _webViewSize;
			set
			{
				_webViewSize = value;
				base.InvokePropertyValueChanged(nameof(WebViewSize), WebViewSize);
			}
		}

		public string WebViewURL
		{
			get => _webViewURL;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					_webViewURL = string.Empty;
					return;
				}

				if (string.IsNullOrEmpty(_webViewURL) || _canvasWebView == null)
				{
					LoadUrlAsync(value).Forget();
				}
				else if (_userChangedURL && _webViewURL != value)
				{
					ChangeCanvasWebView(value);
				}

				_webViewURL = value;
				base.InvokePropertyValueChanged(nameof(WebViewURL), WebViewURL);
			}
		}

		public string WebViewHtml
		{
			get => _webViewHtml;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					_webViewHtml = string.Empty;
					return;
				}

				if (string.IsNullOrWhiteSpace(_webViewHtml) || string.CompareOrdinal(_webViewHtml, value) != 0 || _canvasWebView.IsReferenceNull())
				{
					LoadHtmlAsync(value).Forget();
				}
				else if (string.CompareOrdinal(_webViewHtml, value) != 0)
				{
					ChangeCanvasWebViewHtml(value);
				}

				_webViewHtml = value;
				InvokePropertyValueChanged(nameof(WebViewHtml), WebViewHtml);
			}
		}
		public bool CanChangeURL
		{
			get => _canChangeURL;
			set
			{
				_canChangeURL = value;
				base.InvokePropertyValueChanged(nameof(CanChangeURL), CanChangeURL);
			}
		}

		public bool IsLoginView
		{
			get => _isLoginView;
			set => _isLoginView = value;
		}

		[UsedImplicitly]
		public float Alpha
		{
			get => _alpha;
			set => SetProperty(ref _alpha, value);
		}

		public DragMode DragMode { get; set; } = DragMode.DragWithinPage;

        // Method
        private async UniTask LoadUrlAsync(string url) => await LoadAsync(() => _canvasWebView.WebView.LoadUrl(url, _additionalHttpHeaders != null && _additionalHttpHeaders.Count > 0 ? _additionalHttpHeaders : null));
        private async UniTask LoadHtmlAsync(string html) => await LoadAsync(() => _canvasWebView.WebView.LoadHtml(html));
		private async UniTask LoadAsync(Action onLoad)
		{
			await UniTask.WaitUntil(() => !WebViewRoot.IsReferenceNull());

			try
			{
				_canvasWebView          ??= CanvasWebViewPrefab.Instantiate();
				_canvasWebView.DragMode =   DragMode;
				_canvasWebView.transform.SetParent(WebViewRoot, false);

				await _canvasWebView.WaitUntilInitialized();
				onLoad?.Invoke();
				_onPageLoaded?.Invoke();

				_canvasWebView.Clicked                -= OnWebViewClicked;
				_canvasWebView.Clicked                += OnWebViewClicked;
				_canvasWebView.WebView.UrlChanged     -= UrlChanged;
				_canvasWebView.WebView.UrlChanged     += UrlChanged;
				_canvasWebView.WebView.MessageEmitted -= OnWebViewMessageEmitted;
				_canvasWebView.WebView.MessageEmitted += OnWebViewMessageEmitted;

				var webViewWithDownloads = _canvasWebView.WebView as IWithDownloads;
				if (webViewWithDownloads != null)
				{
					webViewWithDownloads.SetDownloadsEnabled(true);

					webViewWithDownloads.DownloadProgressChanged -= OnWebViewDownloadProgressChanged;
					webViewWithDownloads.DownloadProgressChanged += OnWebViewDownloadProgressChanged;
				}
			}
			catch (NullReferenceException e)
			{
				C2VDebug.LogErrorCategory("WebView", e);
			}
		}

		public void SetOnPageLoaded(Action onPageLoaded) => _onPageLoaded = onPageLoaded;
		public void OnUseHandlerAction()
		{
			if (_onUseHandler != null)
			{
				_handler ??= new WebViewHandler(_canvasWebView.WebView);
				_onUseHandler?.Invoke(_handler);
			}
		}

		private void ChangeCanvasWebView(string url)
		{
			if (_canvasWebView.IsReferenceNull()) return;
			if (_canvasWebView.WebView == null) return;
			_userChangedURL = false;
			_canvasWebView.WebView.LoadUrl(url, _additionalHttpHeaders != null && _additionalHttpHeaders.Count > 0 ? _additionalHttpHeaders : null);
        }

		private void ChangeCanvasWebViewHtml(string html)
		{
			if (_canvasWebView.IsReferenceNull()) return;
			if (_canvasWebView.WebView == null) return;
			_canvasWebView.WebView.LoadHtml(html);
		}
#region Event Setter
		public void SetAutoClose(Action onAutoClose) => _autoCloseAction = onAutoClose;
		public void SetMessageEmitted(Action<string> onMessageEmitted) => _messageEmittedAction = onMessageEmitted;
		public void SetDownloadProgress(Action<DownloadChangedEventArgs> onDownloadProgress) => _downloadProgressAction = onDownloadProgress;
		public void SetTerminated(Action onTerminated) => _onTerminated = onTerminated;
		public void SetClicked(Action onClicked) => _onClicked = onClicked;
		public void SetUseHandler(Action<WebViewHandler> onUseHandler) => _onUseHandler = onUseHandler;
#endregion // Event Setter

		public void OnCloseButtonClick()
		{
			if (!_canvasWebView.IsReferenceNull())
			{
				_canvasWebView.Clicked                -= OnWebViewClicked;
				if (_canvasWebView.WebView != null)
				{
					_canvasWebView.WebView.UrlChanged     -= UrlChanged;
					_canvasWebView.WebView.MessageEmitted -= OnWebViewMessageEmitted;
				}
				_canvasWebView.Destroy();
				_canvasWebView = null;
			}
			_autoCloseAction = null;
			_handler = null;

			_onTerminated?.Invoke();
		}

		public void PostMessage(string message)
		{
			_canvasWebView.WebView.PostMessage(message);
		}

		private void OnBackwardButtonClick()
		{
			if (_canvasWebView.IsReferenceNull()) return;
			if (_canvasWebView.WebView == null) return;
			_canvasWebView.WebView.GoBack();
		}

		private void OnForwardButtonClick()
		{
			if (_canvasWebView.IsReferenceNull()) return;
			if (_canvasWebView.WebView == null) return;
			_canvasWebView.WebView.GoForward();
		}

		private void OnChangeWebViewURL()
		{
			_userChangedURL = true;
		}

		// Action
		private void UrlChanged(object sender, UrlChangedEventArgs eventArgs)
		{
			_userChangedURL = false;
			WebViewURL = eventArgs.Url;
			if (IsLoginView && WebViewURL.Contains(LoginManager.ServerURL.AddressServiceOAuth)) GetHtmlBodyAsync();
		}

		private void OnWebViewDownloadProgressChanged(object sender, DownloadChangedEventArgs eventArgs)
		{
			_downloadProgressAction?.Invoke(eventArgs);
			//_messageEmittedAction?.Invoke(eventArgs.Value);
		}

		private void OnWebViewMessageEmitted(object sender, EventArgs<string> eventArgs)
		{
			_messageEmittedAction?.Invoke(eventArgs.Value);
		}

		private void OnWebViewClicked(object sender, ClickedEventArgs eventArgs)
		{
			_onClicked?.Invoke();
		}

		private async void GetHtmlBodyAsync()
		{
			if (_canvasWebView.IsReferenceNull()) return;
			if (_canvasWebView.gameObject.IsReferenceNull()) return;
			if (_canvasWebView.WebView == null) return;
			_canvasWebView.gameObject.SetActive(false);

			var html = await _canvasWebView.WebView.ExecuteJavaScript("document.getElementsByTagName('pre')[0].innerText");
			// LoginManager.Instance.GetServiceUserData(html);

			_autoCloseAction?.Invoke();
		}

#region Render
		public Material CreateMaterial() => _canvasWebView?.WebView.CreateMaterial();
#endregion // Render

#region Handler
		public class WebViewHandler
		{
			private IWebView _webView;
			public void Reload() => _webView.Reload();
			public WebViewHandler(IWebView view) => _webView = view;
		}
#endregion Handler
	}
}
