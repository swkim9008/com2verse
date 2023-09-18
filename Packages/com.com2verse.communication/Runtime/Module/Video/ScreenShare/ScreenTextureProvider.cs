/*===============================================================
* Product:		Com2Verse
* File Name:	ScreenTextureProvider.cs
* Developer:	urun4m0r1
* Date:			2022-08-30 00:16
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Threading;
using Com2Verse.Extension;
using Com2Verse.ScreenShare;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Com2Verse.Communication
{
	public sealed class ScreenTextureProvider : IVideoTextureProvider, IDisposable
	{
#region IModule
		public event Action<bool>? StateChanged;

		private bool _isRunning;

		public bool IsRunning
		{
			get => _isRunning;
			set
			{
				var prevValue = _isRunning;
				if (prevValue == value)
					return;

				_isRunning = value;
				StateChanged?.Invoke(value);

				UpdateScreenTexture();
			}
		}
#endregion // IModule

#region IVideoTextureProvider
		public event Action<Texture?>? TextureChanged;

		private Texture? _texture;

		public Texture? Texture
		{
			get => _texture;
			private set
			{
				var prevValue = _texture;
				if (prevValue == value)
					return;

				_texture = value;
				TextureChanged?.Invoke(value);
			}
		}
#endregion // IVideoTextureProvider

		private RenderTexture? _frameTexture;

		private CancellationTokenSource? _updateTokenSource;

		private readonly ScreenCaptureController _controller;

		private readonly VideoSettings _requestedSettings;

		public ScreenTextureProvider(ScreenCaptureController controller, VideoSettings requestedSettings)
		{
			_controller = controller;

			_controller.MinScreenSize = new(VideoProperty.SdkMinLimit.Width, VideoProperty.SdkMinLimit.Height);
			_controller.MaxScreenSize = new(VideoProperty.SdkMaxLimit.Width, VideoProperty.SdkMaxLimit.Height);

			_requestedSettings = requestedSettings;

			_requestedSettings.SettingsChanged += OnRequestedSettingsChanged;

			OnRequestedSettingsChanged(_requestedSettings);
		}

		private void UpdateScreenTexture()
		{
			if (IsRunning)
			{
				_updateTokenSource ??= new CancellationTokenSource();
				UpdateTextureAsync().Forget();
			}
			else
			{
				Texture = null;
				DisposeUpdateToken();
			}
		}

		private void OnRequestedSettingsChanged(IReadOnlyVideoSettings settings)
		{
			_controller.Fps = settings.Fps;

			_controller.RequestedScreenSize    = new(_requestedSettings.Width, _requestedSettings.Height);
			_controller.RequestedThumbnailSize = GetThumbnailSize();
			_controller.UpdateRequestedSize();
		}

		private async UniTask UpdateTextureAsync()
		{
			while (await UniTaskHelper.Delay(1000 / _controller.Fps, _updateTokenSource, TextureHelper.TextureUpdateTiming))
			{
				var sourceTexture = _controller.CurrentScreen?.Screen;
				if (sourceTexture.IsUnityNull())
				{
					Texture = null;
					continue;
				}

				if (_frameTexture.IsUnityNull() || (_frameTexture!.width != _requestedSettings.Width || _frameTexture.height != _requestedSettings.Height))
				{
					Texture = null;

					if (!await CreateFrameTexture(_requestedSettings))
						break;
				}

				_frameTexture.Clear();
				sourceTexture!.CopyTextureCenter(_frameTexture!);
				_frameTexture!.name = sourceTexture!.name;

				Texture = _frameTexture;
			}
		}

		private async UniTask<bool> CreateFrameTexture(IReadOnlyVideoSettings settings)
		{
			var waitBeforeRenderTextureCreation = !_frameTexture.IsUnityNull();
			DestroyFrameTexture();

			if (waitBeforeRenderTextureCreation)
			{
				if (!await UniTaskHelper.Delay(Define.RenderTextureReCreateDelay, _updateTokenSource))
					return false;
			}

			_frameTexture = new RenderTexture(settings.Width, settings.Height, 0, Define.WebRtcSupportedGraphicsFormat);
			_frameTexture.Create();

			return true;
		}

		private static Vector2Int GetThumbnailSize()
		{
			var table = VideoResolution.InstanceOrNull?.Table;
			if (table == null || !table.TryGetValue(Define.DefaultTableIndex, out var resolution) || resolution == null)
			{
				return new(VideoProperty.Fallback.Width, VideoProperty.Fallback.Height);
			}

			return new(resolution.ScreenThumbnailWidth, resolution.ScreenThumbnailHeight);
		}

		public void Dispose()
		{
			_requestedSettings.SettingsChanged -= OnRequestedSettingsChanged;

			DisposeUpdateToken();
			DestroyFrameTexture();
		}

		private void DisposeUpdateToken()
		{
			_updateTokenSource?.Cancel();
			_updateTokenSource?.Dispose();
			_updateTokenSource = null;
		}

		private void DestroyFrameTexture()
		{
			if (!_frameTexture.IsUnityNull())
			{
				_frameTexture!.Release();
				Object.Destroy(_frameTexture);
			}
		}
	}
}
