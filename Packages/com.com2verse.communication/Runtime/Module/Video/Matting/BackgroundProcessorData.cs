/*===============================================================
* Product:		Com2Verse
* File Name:	BackgroundProcessorData.cs
* Developer:	urun4m0r1
* Date:			2022-10-07 18:17
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Threading;
using com.com2verse.humanmattingLight;
using Com2Verse.Extension;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace Com2Verse.Communication.Matting
{
	public sealed class BackgroundProcessorData : IDisposable
	{
		public int Width  { get; }
		public int Height { get; }

		public Texture2D Source { get; }
		public Texture2D Target { get; }
		public Texture2D Mask   { get; }

		public Texture2D? Background { get; private set; }

		private readonly Texture2D     _backgroundTexture2D;
		private readonly RenderTexture _sourceRenderTexture;
		private readonly RenderTexture _maskRenderTexture;
		private readonly Color32[]     _cache;

		public BackgroundProcessorData(int width, int height)
		{
			Width  = width;
			Height = height;

			Source = CreateTexture();
			Target = CreateTexture();
			Mask   = CreateTexture();

			_backgroundTexture2D = CreateTexture();
			_sourceRenderTexture = CreateRenderTexture();
			_maskRenderTexture   = CreateRenderTexture();

			_cache = new Color32[width * height];

			Texture2D CreateTexture()
			{
				return new Texture2D(Width, Height, Define.WebRtcSupportedGraphicsFormat, TextureCreationFlags.None);
			}

			RenderTexture CreateRenderTexture()
			{
				var renderTexture = new RenderTexture(Width, Height, 0, Define.WebRtcSupportedGraphicsFormat);
				renderTexture.Create();
				return renderTexture;
			}
		}

		public async UniTask UpdateTargetTextureAsync(HumanMatting? predictor, Texture source, CancellationTokenSource? tokenSource)
		{
			if (_disposed || tokenSource == null || tokenSource.IsCancellationRequested)
			{
				return;
			}

			Graphics.Blit(source, _sourceRenderTexture);
			var maskRenderTexture = predictor?.ProcessImage(_sourceRenderTexture);
			if (maskRenderTexture.IsUnityNull() || _disposed || tokenSource.IsCancellationRequested)
			{
				return;
			}

			Graphics.Blit(maskRenderTexture!, _maskRenderTexture);
			source.Copy(Source);
			_maskRenderTexture.Copy(Mask);
			await Target.ApplyMask(Source, Mask, TextureHelper.eMaskChannel.HUMAN_MATTING, Background, tokenSource);
		}

		public async UniTask ChangeBackgroundTextureAsync(Texture2D? background, CancellationTokenSource? tokenSource)
		{
			if (_disposed)
			{
				return;
			}

			if (background == null)
			{
				Background = null;
			}
			else
			{
				await background.Resize(_backgroundTexture2D, true, tokenSource, _cache);
				Background = _backgroundTexture2D;
			}
		}

#region IDisposable
		private bool _disposed;

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_sourceRenderTexture.Release();
			_maskRenderTexture.Release();

			Object.Destroy(Source);
			Object.Destroy(Target);
			Object.Destroy(Mask);
			Object.Destroy(_backgroundTexture2D);
			Object.Destroy(_sourceRenderTexture);
			Object.Destroy(_maskRenderTexture);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
