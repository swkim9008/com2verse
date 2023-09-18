/*===============================================================
* Product:		Com2Verse
* File Name:	YoutubeWebModel.cs
* Developer:	jhkim
* Date:			2022-10-31 20:35
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Extension;
using UnityEngine;
using Vuplex.WebView;

namespace Com2Verse.WebView
{
	public sealed class YoutubeWebModel
	{
#region Variables
		private Settings _settings;
		private IWebView _webView;
#endregion // Variables

#region Properties
		public Settings Setting => _settings;
		public eQuality Quality
		{
			get => _settings.Quality;
			set => _settings.Quality = value;
		}

		public IWebView WebView => _webView;
#endregion // Properties

#region Initialization
		public YoutubeWebModel(Settings settings)
		{
			SetSettings(settings);
		}
#endregion // Initialization

#region Public Functions
		public void SetWebView(IWebView webView) => _webView = webView;
		public void ApplyRenderer(Renderer renderer)
		{
			if (!renderer.IsUnityNull())
				renderer.material = _webView?.CreateMaterial();
		}
		public void SetVideoID(string videoId) => _settings.VideoID = videoId;
		public void Dispose()
		{
			if (_webView != null)
			{
				_webView?.Dispose();
				_webView = null;
			}
		}

		public void SetOnMessageEmitted(EventHandler<EventArgs<string>> onMessageEmitted)
		{
			_webView.MessageEmitted -= onMessageEmitted;
			_webView.MessageEmitted += onMessageEmitted;
		}
#endregion // Public Functions

#region Private Functions
		private void SetSettings(Settings settings)
		{
			_settings = settings;
		}
#endregion // Private Functions

#region Data
		[Serializable]
		public enum eQuality
		{
			LOW_144,
			LOW_240,
			LOW_360,
			MEDIUM_480,
			HIGH_720,
			FHD_1080,
		}

		[Serializable]
		public struct Settings
		{
			public Vector2Int ScreenResolution;
			public string VideoID;
			public eQuality Quality;
			public bool Mute;
			public bool AutoPlay;
			public bool Controls;
			public bool DisableKeyboards;
			public bool ClickEnabled;
			public bool FullScreen;
			public bool Loop;
			public float Volume;
			public bool EnableJsApi;
			public static Settings Default => new Settings()
			{
				ScreenResolution = new Vector2Int(1920, 1080),
				VideoID          = string.Empty,
				Quality          = eQuality.MEDIUM_480,
				Mute             = true,
				AutoPlay         = true,
				Controls         = false,
				DisableKeyboards = true,
				ClickEnabled     = false,
				FullScreen       = true,
				Loop             = true,
				EnableJsApi      = true,
			};
			public static Settings NoMuteDefault => new Settings()
			{
				ScreenResolution = new Vector2Int(1920, 1080),
				VideoID          = string.Empty,
				Quality          = eQuality.MEDIUM_480,
				Mute             = false,
				AutoPlay         = true,
				Controls         = false,
				DisableKeyboards = true,
				ClickEnabled     = false,
				FullScreen       = true,
				Loop             = true,
				EnableJsApi      = true,
			};
		}

		public enum eParam
		{
			MUTE,
			AUTOPLAY,
			HIDE_CONTROLS,
			DISABLE_KEYBOARD_CONTROL,
			FULLSCREEN,
			PLAYLIST,
			LOOP,
			START,
		};

		public static Dictionary<eParam, string> ParamMap = new Dictionary<eParam, string>()
		{
			{eParam.MUTE, "mute"},
			{eParam.AUTOPLAY, "autoplay"},
			{eParam.HIDE_CONTROLS, "controls"},
			{eParam.DISABLE_KEYBOARD_CONTROL, "disablekb"},
			{eParam.FULLSCREEN, "fs"},
			{eParam.PLAYLIST, "playlist"},
			{eParam.LOOP, "loop"},
			{eParam.START, "start"},
		};
#endregion // Data

#region Settings
		public float Volume
		{
			get => _settings.Volume;
			set => _settings.Volume = value;
		}
		public bool Mute
		{
			get => _settings.Mute;
			set => _settings.Mute = value;
		}

		public bool Loop
		{
			get => _settings.Loop;
			set => _settings.Loop = value;
		}
#endregion // Settings
	}
}
