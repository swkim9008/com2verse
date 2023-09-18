/*===============================================================
* Product:		Com2Verse
* File Name:	ScreenCaptureController_UtilityMethods.cs
* Developer:	urun4m0r1
* Date:			2022-09-23 16:26
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

// TODO: SDK 버그 수정 후 주석 해제를 통해 메모리 절약 가능
// #define ALLOW_SCREEN_TEXTURE_DESTROY

#nullable enable

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Com2Verse.Extension;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Com2Verse.ScreenShare
{
	public partial class ScreenCaptureController
	{
#region UtilityMethods - List
		private bool IsScreenInfoExists(ScreenId id)
		{
			return _screenInfos.ContainsKey(id);
		}

		private bool TryFindScreenInfo(ScreenId id, [NotNullWhen(true)] out ScreenInfo? result)
		{
			return _screenInfos.TryGetValue(id, out result);
		}
#endregion // UtilityMethods - List

#region UtilityMethods - Capture
		private bool IsCapturingOrRequestedScreen(ScreenId id)
		{
			return IsCapturingScreen(id) || IsRequestedScreen(id);
		}

		private bool IsCapturingScreen(ScreenId id)
		{
			return _currentScreen != null && _currentScreen.Id == id;
		}

		private bool IsRequestedScreen(ScreenId id)
		{
			return _requestedScreen != null && _requestedScreen.Id == id;
		}

		private void UpdateScreenSize(ScreenInfo value)
		{
			if (!IsCapturingScreen(value.Id))
				return;

			var screenSize = CalculateScreenSize(value.Id);
			_screenCaptureManager.SetCaptureReStart(value.Id, screenSize);
		}
#endregion // UtilityMethods - Capture

#region UtilityMethods - Metadata
		private void UpdateTitle(ScreenInfo value)
		{
			value.Title = _screenCaptureManager.GetTitle(value.Id);
		}

		private void UpdateVisibility(ScreenInfo value, bool isVisible)
		{
			value.IsVisible = isVisible;

			if (IsCapturingOrRequestedScreen(value.Id))
			{
				if (!isVisible)
					StopCapture(eScreenShareSignal.CAPTURE_STOPPED_BY_VISIBILITY);
			}
			else
			{
				UpdateThumbnail(value);
			}
		}

		private void UpdateIcon(ScreenInfo value)
		{
			if (value.Icon.IsReferenceNull())
				return;

			RequestIcon(value);
		}

		private void UpdateThumbnail(ScreenInfo value)
		{
			if (value.Thumbnail.IsReferenceNull())
				return;

			RequestThumbnail(value);
		}

		private void RequestIcon(ScreenInfo value)
		{
			_screenCaptureManager.GetIcon(value.Id);
		}

		private void RequestThumbnail(ScreenInfo value)
		{
			if (IsCapturingOrRequestedScreen(value.Id))
			{
				ReplaceThumbnail(value, value.Screen);
				return;
			}

			var screenSize = CalculateThumbnailSize(value.Id);
			_screenCaptureManager.GetThumbnail(value.Id, screenSize);
		}

		private void ReplaceIcon(ScreenInfo screenInfo, Texture2D? texture)
		{
			if (screenInfo.Icon == texture)
				return;

			DestroyIcon(screenInfo);
			screenInfo.Icon = texture;
		}

		private void ReplaceThumbnail(ScreenInfo screenInfo, Texture2D? texture)
		{
			if (screenInfo.Thumbnail == texture)
				return;

			DestroyThumbnail(screenInfo);
			screenInfo.Thumbnail = texture;
		}

		private void ReplaceScreen(ScreenInfo screenInfo, Texture2D? texture)
		{
			if (screenInfo.Screen == texture)
				return;

			DestroyTexture(screenInfo.Screen);
			screenInfo.Screen = texture;

			UpdateThumbnail(screenInfo);
		}

		private void DestroyResources(ScreenInfo screenInfo)
		{
			DestroyIcon(screenInfo);
			DestroyThumbnail(screenInfo);
			DestroyScreen(screenInfo);
		}

		private void DestroyIcon(ScreenInfo screenInfo)
		{
			DestroyTexture(screenInfo.Icon);
			screenInfo.Icon = null;
		}

		private void DestroyThumbnail(ScreenInfo screenInfo)
		{
			if (screenInfo.Thumbnail != screenInfo.Screen)
				DestroyTexture(screenInfo.Thumbnail);

			screenInfo.Thumbnail = null;
		}

		private void DestroyScreen(ScreenInfo screenInfo)
		{
			if (IsCapturingScreen(screenInfo.Id) && !screenInfo.Screen.IsReferenceNull())
				throw new InvalidOperationException("Cannot destroy screen while capturing");

			if (screenInfo.Thumbnail == screenInfo.Screen)
				UpdateThumbnail(screenInfo);

			DestroyTexture(screenInfo.Screen);
			screenInfo.Screen = null;
		}

		/// <summary>
		/// 사용하지 않는 화면 텍스쳐를 파괴합니다.<br/>
		/// <remarks>
		/// com.com2verse.screencapture 패키지 1.1.0 버전 기준 SDK 내부 오류로 인해 텍스쳐를 파괴할 경우 다시 생성되지 않거나 Crash가 발생할 수 있습니다.
		/// </remarks>
		/// </summary>
		[Conditional("ALLOW_SCREEN_TEXTURE_DESTROY")]
		private static void DestroyTexture(Object? texture)
		{
			if (texture.IsReferenceNull())
				return;

			Object.Destroy(texture!);
		}
#endregion // UtilityMethods - Metadata
	}
}
