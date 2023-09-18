/*===============================================================
* Product:		Com2Verse
* File Name:	VideoWebViewModel.cs
* Developer:	ikyoung
* Date:			2023-05-26 15:01
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Vuplex.WebView;

namespace Com2Verse.UI
{
	public sealed class VideoWebViewModel : ViewModelBase
	{
        private CanvasWebViewWrapper _canvasWebViewWrapper;

		private RectTransform  _webViewRoot;
		private Vector2        _webViewSize;
		private string         _webViewURL  = string.Empty;
		private string         _webViewHtml = string.Empty;
		private Texture        _videoScreenTex;
		private RectTransform  _videoScreenRoot;
		public  CommandHandler VideoWebViewCloseButtonClick { get; }

		public VideoWebViewModel()
		{
			VideoWebViewCloseButtonClick = new CommandHandler(OnCloseButtonClick);
		}
		
		// Property
        public RectTransform VideoWebViewRoot
		{
			get => _webViewRoot;
			set => _webViewRoot = value;
		}

		public Vector2 VideoWebViewSize
		{
			get => _webViewSize;
			set
			{
				_webViewSize = value;
				base.InvokePropertyValueChanged(nameof(VideoWebViewSize), value);
			}
		}
		
		public string VideoWebViewURL
		{
			get => _webViewURL;
			set
			{
				_webViewURL = value;
				base.InvokePropertyValueChanged(nameof(VideoWebViewURL), value);
			}
		}

		public string VideoWebViewHtml
		{
			get => _webViewHtml;
			set
			{
				_webViewHtml = value;
				InvokePropertyValueChanged(nameof(VideoWebViewHtml), value);
			}
		}

		public Texture VideoScreenTex
		{
			get => _videoScreenTex;
			set
			{
				_videoScreenTex = value;
                InvokePropertyValueChanged(nameof(VideoScreenTex), value);
            }
        }

		public RectTransform VideoScreenRoot
        {
			get => _videoScreenRoot;
			set
			{
                _videoScreenRoot = value;
                InvokePropertyValueChanged(nameof(VideoScreenRoot), value);
            }
        }

		// Method
		public async UniTask LoadUrlAsync(string url, bool showTimeline = true) 
			=> await LoadAsync(() =>
			{
				//_canvasWebViewWrapper.WebView.LoadUrl(url);
				_canvasWebViewWrapper.LoadUrl(url).ContinueWith(() => _canvasWebViewWrapper.SetVideoTimelineVisible(showTimeline));
            });
		public async UniTask LoadHtmlAsync(string html) => await LoadAsync(() => _canvasWebViewWrapper.WebView.LoadHtml(html));
		private async UniTask LoadAsync(Action onLoad)
		{
			await UniTask.WaitUntil(() => !VideoWebViewRoot.IsReferenceNull());

            if (_canvasWebViewWrapper == null)
            {
                _canvasWebViewWrapper = new();
                await _canvasWebViewWrapper.Init(this.VideoWebViewRoot, this.VideoScreenRoot, "");
            }

			onLoad?.Invoke();
			await _canvasWebViewWrapper.WebView.WaitForNextPageLoadToFinish();
		}
		
		public void OnCloseButtonClick()
		{
            _canvasWebViewWrapper?.Dispose();
            _canvasWebViewWrapper = null;
        }
    }
}
