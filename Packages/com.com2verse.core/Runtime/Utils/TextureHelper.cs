/*===============================================================
* Product:		Com2Verse
* File Name:	TextureHelper.cs
* Developer:	urun4m0r1
* Date:			2022-07-18 16:51
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Com2Verse.Utils
{
	public static class TextureHelper
	{
#region TextureUtils
		public static readonly byte  Color32Max         = byte.MaxValue;
		public static readonly float Color32MaxInversed = 1f / Color32Max;

		public static readonly PlayerLoopTiming TextureUpdateTiming = PlayerLoopTiming.LastPostLateUpdate;

		public static bool IsSameSize(Texture lhs, Texture rhs) => lhs.width == rhs.width && lhs.height == rhs.height;

		public static bool IsLinearColorSpace => QualitySettings.activeColorSpace == ColorSpace.Linear;
		public static bool IsGammaColorSpace  => QualitySettings.activeColorSpace == ColorSpace.Gamma;
#endregion // TextureUtils

#region TextureCopy
		public static void Copy(this Texture source, Texture2D target, Color32[]? cache = null)
		{
			switch (source)
			{
				case Texture2D texture2D:
					texture2D.Copy(target);
					break;
				case RenderTexture renderTexture:
					renderTexture.Copy(target);
					break;
				case WebCamTexture webCamTexture:
					webCamTexture.Copy(target, cache);
					break;
				default:
					throw new NotSupportedException();
			}
		}

		public static void Copy(this Texture2D source, Texture2D target)
		{
			Graphics.CopyTexture(source, 0, 0, target, 0, 0);
		}

		public static void Copy(this RenderTexture source, Texture2D target)
		{
			var active = RenderTexture.active;
			RenderTexture.active = source;
			{
				var rect = new Rect(0, 0, source.width, source.height);
				target.ReadPixels(rect, 0, 0);
				target.Apply();
			}
			RenderTexture.active = active;
		}

		public static void Copy(this WebCamTexture source, Texture2D target, Color32[]? cache = null)
		{
			if (!source.isPlaying)
			{
				return;
			}

			cache ??= new Color32[source.width * source.height];

			var pixels = source.GetPixels32(cache);
			if (pixels == null)
			{
				return;
			}

			target.SetPixels32(pixels);
			target.Apply();
		}
#endregion // TextureCopy

#region RenderTexture
		public static void CopyTextureCenter(this Texture src, Texture dst)
		{
			if (IsSameSize(src, dst))
			{
				Graphics.CopyTexture(
					src, srcElement: default, srcMip: default
				  , dst, dstElement: default, dstMip: default);
			}
			else
			{
				var offsetX = Mathf.FloorToInt((dst.width  - src.width)  * 0.5f);
				var offsetY = Mathf.FloorToInt((dst.height - src.height) * 0.5f);

				var srcX = offsetX < 0 ? -offsetX : default;
				var srcY = offsetY < 0 ? -offsetY : default;
				var dstX = offsetX > 0 ? offsetX : default;
				var dstY = offsetY > 0 ? offsetY : default;

				var srcWidth  = offsetX < 0 ? dst.width : src.width;
				var srcHeight = offsetY < 0 ? dst.height : src.height;

				Graphics.CopyTexture(
					src, srcElement: default, srcMip: default, srcX, srcY, srcWidth, srcHeight
				  , dst, dstElement: default, dstMip: default, dstX, dstY);
			}
		}

		public static void Clear(this RenderTexture? dst)
		{
			dst.Clear(Color.clear);
		}

		public static void Clear(this RenderTexture? dst, Color color)
		{
			var active = RenderTexture.active;
			RenderTexture.active = dst;
			{
				GL.Clear(clearDepth: true, clearColor: true, color);
			}
			RenderTexture.active = active;
		}
#endregion // RenderTexture

#region TextureResize
		public static async UniTask<bool> Resize(this Texture2D src, Texture2D dst, bool isCrop, CancellationTokenSource? token, Color32[]? dstCache = null)
		{
			var prevContext = SynchronizationContext.Current;
			if (!await UniTaskHelper.TrySwitchToMainThread(prevContext, token))
				return false;

			if (IsSameSize(src, dst))
			{
				src.Copy(dst);
				return true;
			}

			var srcWidth  = src.width;
			var srcHeight = src.height;

			var dstWidth  = dst.width;
			var dstHeight = dst.height;

			var srcPixels = src.GetPixels32();
			if (srcPixels == null)
				return false;

			dstCache ??= new Color32[dstWidth * dstHeight];

			await UniTask.SwitchToThreadPool();
			{
				var srcAspect  = (float)srcWidth / srcHeight;
				var dstAspect  = (float)dstWidth / dstHeight;
				var srcXOffset = 0;
				var srcYOffset = 0;

				float srcScaleFactor;
				if (srcAspect > dstAspect)
				{
					if (isCrop)
					{
						srcScaleFactor = (float)dstHeight / srcHeight;
						srcXOffset     = (int)((srcWidth - srcHeight * dstAspect) * 0.5f);
					}
					else
					{
						srcScaleFactor = (float)dstWidth / srcWidth;
						srcYOffset     = (int)((srcHeight - srcWidth / dstAspect) * 0.5f);
					}
				}
				else
				{
					if (isCrop)
					{
						srcScaleFactor = (float)dstWidth / srcWidth;
						srcYOffset     = (int)((srcHeight - srcWidth / dstAspect) * 0.5f);
					}
					else
					{
						srcScaleFactor = (float)dstHeight / srcHeight;
						srcXOffset     = (int)((srcWidth - srcHeight * dstAspect) * 0.5f);
					}
				}

				for (var dstY = 0; dstY < dstHeight; dstY++)
				{
					var srcY = srcYOffset + dstY / srcScaleFactor;
					for (var dstX = 0; dstX < dstWidth; dstX++)
					{
						var srcX  = srcXOffset + dstX / srcScaleFactor;
						var color = srcPixels.GetBilinearInterpolatedColor(srcX, srcY, srcWidth, srcHeight);
						dstCache.SetColor(dstX, dstY, dstWidth, color);
					}
				}
			}

			var isSuccess = false;
			if (await UniTaskHelper.TrySwitchToMainThread(prevContext, token) && !dst.IsUnityNull())
			{
				dst.SetPixels32(dstCache);
				dst.Apply();
				isSuccess = true;
			}

			await UniTaskHelper.TrySwitchToSynchronizationContext(prevContext);
			return isSuccess;
		}
#endregion // TextureResize

#region TextureColor
		private static Color32 GetBilinearInterpolatedColor(this IReadOnlyList<Color32> target, float px, float py, int width, int height, Color32 fallback = default)
		{
			if (px < 0 || px > width - 1 || py < 0 || py > height - 1)
			{
				return fallback;
			}

			var p = new Vector2(px, py);

			var x1 = Mathf.FloorToInt(p.x);
			var x2 = Mathf.CeilToInt(p.x);
			var y1 = Mathf.FloorToInt(p.y);
			var y2 = Mathf.CeilToInt(p.y);

			var h1 = width * y1;
			var h2 = width * y2;

			var c11 = target[x1 + h1];
			var c12 = target[x1 + h2];
			var c21 = target[x2 + h1];
			var c22 = target[x2 + h2];

			var a = Color32.Lerp(c11, c12, p.y);
			var b = Color32.Lerp(c21, c22, p.y);
			var c = Color32.Lerp(a,   b,   p.x);

			return c;
		}

		private static void SetColor(this IList<Color32> target, int x, int y, int width, Color32 color)
		{
			target[x + (y * width)] = color;
		}
#endregion // TextureColor

#region TextureMask
		/// <summary>
		/// 마스크 채널
		/// </summary>
		public enum eMaskChannel
		{
			/// <summary>
			/// RGBA 채널을 모두 사용합니다.
			/// </summary>
			PASS,

			/// <summary>
			/// Human Matting API에 적절한 채널을 사용합니다.
			/// </summary>
			HUMAN_MATTING,
		}

		/// <summary>
		/// 마스크 텍스쳐를 적용합니다.
		/// </summary>
		/// <param name="target">결과물을 저장할 텍스쳐</param>
		/// <param name="source">원본 텍스쳐</param>
		/// <param name="mask">마스크 텍스쳐 (검정: 배경, 흰색: 전경)</param>
		/// <param name="channel">마스크 채널 모드 <see cref="eMaskChannel"/></param>
		/// <param name="background">배경 텍스쳐 (마스크가 검정인 영역에 적용됨)</param>
		/// <param name="tokenSource">비동기 작업 취소 토큰</param>
		public static async UniTask ApplyMask(this Texture2D target, Texture2D source, Texture2D mask, eMaskChannel channel, Texture2D? background = null, CancellationTokenSource? tokenSource = null)
		{
			var prevContext = SynchronizationContext.Current;
			if (!await UniTaskHelper.TrySwitchToMainThread(prevContext, tokenSource))
				return;

			using var targetPixels = target.GetRawTextureData<Color32>();
			using var sourcePixels = source.GetRawTextureData<Color32>();
			using var maskPixels   = mask.GetRawTextureData<Color32>();

			try
			{
				if (background is not null)
				{
					using var backgroundPixels = background.GetRawTextureData<Color32>();

					await UniTask.SwitchToThreadPool();
					targetPixels.ApplyMask(sourcePixels, maskPixels, channel, backgroundPixels);
				}
				else
				{
					await UniTask.SwitchToThreadPool();
					targetPixels.ApplyMask(sourcePixels, maskPixels, channel);
				}
			}
			catch (ObjectDisposedException e)
			{
				C2VDebug.LogWarningMethod(nameof(TextureHelper), $"Object disposed: {e.Message}");
				await UniTaskHelper.TrySwitchToSynchronizationContext(prevContext);
				return;
			}

			if (!await UniTaskHelper.TrySwitchToMainThread(prevContext, tokenSource))
				return;

			if (!target.IsUnityNull())
			{
				if (!await UniTaskHelper.TrySwitchToMainThread(prevContext, tokenSource))
					return;

				target.Apply();
			}

			await UniTaskHelper.TrySwitchToSynchronizationContext(prevContext);
		}

		private static void ApplyMask(this NativeArray<Color32> target, NativeArray<Color32> source, NativeArray<Color32> mask, eMaskChannel channel, NativeArray<Color32> background)
		{
			for (var i = 0; i < source.Length; i++)
				target[i] = source[i].ApplyMask(mask[i], channel, background[i]);
		}

		private static void ApplyMask(this NativeArray<Color32> target, NativeArray<Color32> source, NativeArray<Color32> mask, eMaskChannel channel)
		{
			for (var i = 0; i < source.Length; i++)
				target[i] = source[i].ApplyMask(mask[i], channel);
		}

		private static Color32 ApplyMask(this Color32 source, Color32 mask, eMaskChannel channel, Color32 background = default)
		{
			return channel switch
			{
				eMaskChannel.PASS          => ApplyMaskPass(source, mask, background),
				eMaskChannel.HUMAN_MATTING => ApplyMaskHumanMatting(source, mask, background),
				_                          => throw new ArgumentOutOfRangeException(nameof(channel), channel, null!),
			};

			// RGBA 채널을 모두 사용합니다.
			static Color32 ApplyMaskPass(Color32 source, Color32 mask, Color32 background = default) => new(
				ApplyMask(source.r, mask.r, background.r),
				ApplyMask(source.g, mask.g, background.g),
				ApplyMask(source.b, mask.b, background.b),
				ApplyMask(source.a, mask.a, background.a)
			);

			// Human Matting API에 적절한 채널을 사용합니다. (RVM v1.0.0 기준)
			static Color32 ApplyMaskHumanMatting(Color32 source, Color32 mask, Color32 background = default) => new(
				ApplyMask(source.r, mask.b, background.r),
				ApplyMask(source.g, mask.b, background.g),
				ApplyMask(source.b, mask.b, background.b),
				ApplyMask(source.a, mask.a, background.a)
			);
		}

		private static byte ApplyMask(byte source, byte mask, byte background = default)
		{
			var maskValue        = mask       * Color32MaxInversed;
			var maskedSource     = source     * maskValue;
			var maskedBackground = background * (1f - maskValue);
			return (byte)(maskedSource + maskedBackground);
		}
#endregion // TextureMask
	}
}
