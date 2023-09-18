/*===============================================================
* Product:		Com2Verse
* File Name:	YoutubeWebController.cs
* Developer:	jhkim
* Date:			2022-10-31 20:48
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Text;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Vuplex.WebView;

namespace Com2Verse.WebView
{
	public sealed class YoutubeWebController
	{
#region Variables
		private static readonly string LogCategory = "YouTube";
		private static readonly string UrlYoutube = "https://www.youtube.com/embed/";
		private static readonly string UrlParamStart = "?";
		private static readonly string UrlParamAnd = "&";

		private static readonly string RemoveOverlayElementsScript =
			@"document.querySelector('.ytp-show-cards-title')?.remove();
			  document.querySelector('.ytp-pause-overlay-container')?.remove();
			  document.querySelector('.ytp-watermark')?.remove()";

		private static readonly string GetVideoScript = "var video = document.querySelector('.html5-video-player');";
		private static readonly string PlayVideoScript =
			@$"{GetVideoScript}
			   video?.playVideo();";
		private static readonly string PauseVideoScript =
			@$"{GetVideoScript}
			   video?.pauseVideo();";
		private static readonly string SetVolumeFormatScript =
			@$"{GetVideoScript}
			   video.setVolume({{0}});";
		private static readonly string SetMuteScript =
			$@"{GetVideoScript}
				video.mute()";
		private static readonly string SetUnMuteScript =
			$@"{GetVideoScript}
				video.unmute()";
		private static readonly string SetLoopFormatScript =
			$@"{GetVideoScript}
				video.setLoop({{0}})";

		[NotNull] private readonly YoutubeWebModel _model;
#endregion // Variables

#region Initialize
		private YoutubeWebController(YoutubeWebModel.Settings settings) => _model = new YoutubeWebModel(settings);

		public static async UniTask<YoutubeWebController> CreateAsync(YoutubeWebModel.Settings settings)
		{
			var instance = new YoutubeWebController(settings);
			var webView = Web.CreateWebView();
			await webView.Init(settings.ScreenResolution.x, settings.ScreenResolution.y);
			instance.SetWebView(webView);
			return instance;
		}

		public static async UniTask CreateAsync(YoutubeWebModel.Settings settings, string videoId, Renderer renderer, Action<YoutubeWebController> onInitialized)
		{
			var instance = await CreateAsync(settings);
			await instance.PlayAsync(videoId, renderer, () => onInitialized?.Invoke(instance));
		}
#endregion // Initialize

#region WebView
		public float Resolution => _model.Quality switch
		{
			YoutubeWebModel.eQuality.LOW_144 => 144f,
			YoutubeWebModel.eQuality.LOW_240 => 240f,
			YoutubeWebModel.eQuality.LOW_360 => 360f,
			YoutubeWebModel.eQuality.MEDIUM_480 => 480f,
			YoutubeWebModel.eQuality.HIGH_720 => 720f,
			YoutubeWebModel.eQuality.FHD_1080 => 1080f,
			_ => 480f, // 기본 화질
		};

		public async UniTask PlayAsync(string videoId, Renderer renderer, Action onReady)
		{
			await PlayWithVideoIdAsync(videoId);
			ApplyRenderer(renderer);
			onReady?.Invoke();
		}

		public async UniTask PauseAsync() => await ExecuteJavaScript(PauseVideoScript);
		public async UniTask ResumeAsync() => await ExecuteJavaScript(PlayVideoScript);
		public void Dispose() => _model?.Dispose();
		private void SetWebView(IWebView webView) => _model.SetWebView(webView);
		private async UniTask PlayWithVideoIdAsync(string videoId)
		{
			_model.SetVideoID(videoId);
			await LoadVideoAsync();
			await ResumeAsync();
		}
		private async UniTask LoadVideoAsync()
		{
			try
			{
				var url = GenerateUrlFromSettings();
				_model.WebView.LoadUrl(url);
				await _model.WebView.WaitForNextPageLoadToFinish();
				await RemovePauseOverlayAsync();
				await InitializeEventAsync();
			}
			catch (Exception e)
			{
				C2VDebug.LogWarningCategory(LogCategory, e);
			}
		}
		private string GenerateUrlFromSettings()
		{
			var setting = _model.Setting;
			StringBuilder sb = new StringBuilder(UrlYoutube);
			sb.Append(setting.VideoID);
			BeginParam(YoutubeWebModel.eParam.MUTE, BoolToString(setting.Mute));
			AppendParam(YoutubeWebModel.eParam.AUTOPLAY, BoolToString(setting.AutoPlay));
			AppendParam(YoutubeWebModel.eParam.HIDE_CONTROLS, BoolToString(setting.Controls));
			AppendParam(YoutubeWebModel.eParam.DISABLE_KEYBOARD_CONTROL, BoolToString(setting.DisableKeyboards));
			AppendParam(YoutubeWebModel.eParam.FULLSCREEN, BoolToString(setting.FullScreen));
			AppendParam(YoutubeWebModel.eParam.LOOP, BoolToString(setting.Loop));
			AppendParam(YoutubeWebModel.eParam.PLAYLIST, setting.VideoID);
			return sb.ToString();

			void AddParam(YoutubeWebModel.eParam key, string value) => sb.Append($"{YoutubeWebModel.ParamMap[key]}={value}");
			void BeginParam(YoutubeWebModel.eParam key, string value)
			{
				sb.Append(UrlParamStart);
				AddParam(key, value);
			}

			void AppendParam(YoutubeWebModel.eParam key, string value)
			{
				sb.Append(UrlParamAnd);
				AddParam(key, value);
			}
			string BoolToString(bool value) => value ? "1" : "0";
		}
		private async UniTask RemovePauseOverlayAsync() => await ExecuteJavaScript(RemoveOverlayElementsScript);
		private async UniTask InitializeEventAsync()
		{
			await SetVolumeAsync(_model.Volume);
			await SetMuteAsync(_model.Mute);
			await SetLoopAsync(_model.Loop);
		}

		private async UniTask ExecuteJavaScript(string script) => await _model?.WebView?.ExecuteJavaScript(script);

		private void ClickScreen() => _model?.WebView?.Click(new Vector2(0.5f, 0.5f));
		private void ApplyRenderer(Renderer renderer) => _model?.ApplyRenderer(renderer);
#endregion // WebView

#region Settings
		public async UniTask SetVolumeAsync(float volume)
		{
			var normalized = Mathf.Clamp01(volume);
			_model.Volume = normalized * 100f;
			await ExecuteJavaScript(string.Format(SetVolumeFormatScript, _model.Volume));
		}
		public async UniTask SetMuteAsync(bool mute)
		{
			_model.Mute = mute;
			await ExecuteJavaScript(mute ? SetMuteScript : SetUnMuteScript);
		}

		public async UniTask SetLoopAsync(bool loop)
		{
			_model.Loop = loop;
			await ExecuteJavaScript(string.Format(SetLoopFormatScript, _model.Loop.ToString().ToLower()));
		}
#endregion // Settings
	}
}