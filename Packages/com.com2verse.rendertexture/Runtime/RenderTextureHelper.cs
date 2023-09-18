/*===============================================================
* Product:    Com2Verse
* File Name:  RenderTextureHelper.cs
* Developer:  eugene9721
* Date:       2022-04-28 18:49
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Logger;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Com2Verse.UIExtension
{
	public static class RenderTextureHelper
	{
		private static readonly Dictionary<RenderTextureFormat, bool> SupportedRenderTextureFormats = new();

		static RenderTextureHelper()
		{
			// SystemInfo.SupportsRenderTextureFormat() generates garbage so we need to cache

			SupportedRenderTextureFormats.Clear();
			var formats = Enum.GetValues(typeof(RenderTextureFormat));
			foreach (RenderTextureFormat format in formats)
			{
				// Safe guard, negative values are deprecated stuff
				if (format < 0) continue;

				var isSupported = SystemInfo.SupportsRenderTextureFormat(format);
				SupportedRenderTextureFormats[format] = isSupported;
			}
		}

		public static RenderTexture CreateRenderTexture(
			RenderTextureFormat format            = RenderTextureFormat.Default
		  , int                 defaultWidth      = 0
		  , int                 defaultHeight     = 0
		  , float               renderScale       = 1f
		  , int                 depth             = 1,
			bool                enableRandomWrite = false)
		{
			var width  = (int)((defaultWidth  <= 0 ? Screen.width : defaultWidth)   * renderScale);
			var height = (int)((defaultHeight <= 0 ? Screen.height : defaultHeight) * renderScale);

			return new RenderTexture(width, height, depth, GetValidatedTextureFormat(format))
			{
				// dimension = dimension,
				// filterMode = FilterMode.Bilinear,
				// wrapMode = TextureWrapMode.Clamp,
				// anisoLevel = 0,
				// volumeDepth = d,
				enableRandomWrite = enableRandomWrite,
			};
		}

		public static bool IsSupported(this RenderTextureFormat format)
		{
			var hasKey = SupportedRenderTextureFormats.TryGetValue(format, out var isSupported);
			return hasKey && isSupported;
		}

		public static RenderTextureFormat GetValidatedTextureFormat(RenderTextureFormat renderTextureFormat)
		{
			if (SupportedRenderTextureFormats[renderTextureFormat])
			{
				return renderTextureFormat;
			}

			var graphicsFormat = GraphicsFormatUtility.GetGraphicsFormat(renderTextureFormat, true);
			var textureFormat  = GraphicsFormatUtility.GetTextureFormat(graphicsFormat);
			return GetValidatedTextureFormat(textureFormat);
		}

		public static RenderTextureFormat GetValidatedTextureFormat(TextureFormat textureFormat)
		{
			// Safe guard, negative values are deprecated stuff
			if (textureFormat < 0)
				return GetValidatedTextureFormatError(textureFormat);

			var graphicsFormat       = GraphicsFormatUtility.GetGraphicsFormat(textureFormat, true);
			var renderGraphicsFormat = SystemInfo.GetCompatibleFormat(graphicsFormat, FormatUsage.Render);

			if (renderGraphicsFormat is GraphicsFormat.None)
				return GetValidatedTextureFormatError(textureFormat);

			var renderTextureFormat = GraphicsFormatUtility.GetRenderTextureFormat(renderGraphicsFormat);

			// Safe guard, filtering abnormal case
			if (!SupportedRenderTextureFormats.ContainsKey(renderTextureFormat))
				return GetValidatedTextureFormatError(textureFormat);

			return renderTextureFormat;
		}

		private static RenderTextureFormat GetValidatedTextureFormatError(TextureFormat textureFormat)
		{
			C2VDebug.LogErrorCategory(nameof(RenderTextureHelper), $"Cannot find a replacement for {textureFormat.ToString()}");
			return RenderTextureFormat.Default;
		}
	}
}
