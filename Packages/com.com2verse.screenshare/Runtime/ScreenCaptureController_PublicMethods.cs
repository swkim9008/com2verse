/*===============================================================
* Product:		Com2Verse
* File Name:	ScreenCaptureController_PublicMethods.cs
* Developer:	urun4m0r1
* Date:			2022-09-23 16:26
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com2Verse.ScreenShare
{
	public partial class ScreenCaptureController
	{
#region PublicMethods - Settings
		/// <summary>
		/// 화면 캡쳐 빈도를 설정합니다.
		/// </summary>
		public int Fps
		{
			get => _screenCaptureManager.GetFps();
			set => _screenCaptureManager.SetFps(value);
		}

		/// <summary>
		/// 최소 허용 화면 크기를 설정합니다.
		/// </summary>
		public Vector2Int MinScreenSize { get; set; } = Define.DefaultMinScreenSize;

		/// <summary>
		/// 최대 허용 화면 크기를 설정합니다.
		/// </summary>
		public Vector2Int MaxScreenSize { get; set; } = Define.DefaultMaxScreenSize;

		/// <summary>
		/// 요구 썸네일 크기를 설정합니다.
		/// </summary>
		public Vector2Int RequestedThumbnailSize { get; set; } = Define.DefaultRequestedThumbnailSize;

		/// <summary>
		/// 요구 화면 크기를 설정합니다.
		/// </summary>
		public Vector2Int RequestedScreenSize { get; set; } = Define.DefaultRequestedScreenSize;

		/// <summary>
		/// 화면 캡쳐 해상도를 재설정합니다.
		/// </summary>
		public void UpdateRequestedSize()
		{
			if (_currentScreen != null)
				UpdateScreenSize(_currentScreen);

			UpdateAllMetadata();
		}
#endregion // PublicMethods - Settings

#region PublicMethods - List
		/// <summary>
		/// 화면 목록에 아이템이 추가 될 때 호출됩니다.
		/// </summary>
		public event Action<IReadOnlyScreenInfo>? ScreenInfoAdded;

		/// <summary>
		/// 화면 목록에 아이템이 삭제 될 때 호출됩니다.
		/// </summary>
		public event Action<IReadOnlyScreenInfo>? ScreenInfoRemoved;

		/// <summary>
		/// 화면 목록입니다.
		/// </summary>
		public IReadOnlyCollection<IReadOnlyScreenInfo> ScreenInfos => _screenInfos.Values;

		/// <summary>
		/// 화면 캡쳐 대상 목록을 갱신합니다.
		/// </summary>
		public void RequestScreenInfoList() => RequestScreenInfoListImpl();

		/// <summary>
		/// 대상 화면이 목록에 존재하는지 확인합니다.
		/// </summary>
		public bool IsScreenInfoExists(IReadOnlyScreenInfo? value)
		{
			return value != null && IsScreenInfoExists(value.Id);
		}
#endregion // PublicMethods - List

#region PublicMethods - Capture
		/// <summary>
		/// 캡쳐를 요청한 화면의 텍스쳐 레퍼런스가 변경될 때 호출됩니다.
		/// </summary>
		public event Action<Texture2D?>? CapturedImageChanged;

		/// <summary>
		/// 캡쳐를 요청한 화면의 레퍼런스가 변경될 때 호출됩니다.
		/// </summary>
		public event Action<IReadOnlyScreenInfo?>? RequestedScreenChanged;

		/// <summary>
		/// 현재 캡쳐중인 화면 정보를 반환합니다.
		/// </summary>
		public IReadOnlyScreenInfo? CurrentScreen => _currentScreen;

		/// <summary>
		/// 마지막으로 캡쳐를 요청한 화면 정보를 반환합니다.
		/// </summary>
		public IReadOnlyScreenInfo? RequestedScreen => _requestedScreen;

		/// <summary>
		/// 실제 화면 캡쳐 여부를 반환합니다
		/// </summary>
		public bool IsCapturing => _currentScreen != null;

		/// <summary>
		/// 화면 캡쳐 시도 여부를 반환합니다.
		/// </summary>
		public bool IsCaptureRequested => _requestedScreen != null;

		/// <summary>
		/// 화면 캡쳐 시도 여부 또는 캡쳐중인지 확인합니다.
		/// </summary>
		public bool IsCaptureRequestedOrCapturing => IsCaptureRequested || IsCapturing;

		/// <summary>
		/// 대상 화면이 캡쳐중인지 확인합니다.
		/// </summary>
		public bool IsCapturingScreen(IReadOnlyScreenInfo? value)
		{
			return value != null && IsCapturingScreen(value.Id);
		}

		/// <summary>
		/// 대상 화면이 캡쳐 요청중인지 확인합니다.
		/// </summary>
		public bool IsRequestedScreen(IReadOnlyScreenInfo? value)
		{
			return value != null && IsRequestedScreen(value.Id);
		}

		/// <summary>
		/// 대상 화면이 사용중인지 확인합니다.
		/// </summary>
		public bool IsCapturingOrRequestedScreen(IReadOnlyScreenInfo? value)
		{
			return IsCapturingScreen(value) || IsRequestedScreen(value);
		}

		/// <summary>
		/// 최소화되어있는 화면 크기를 복원합니다.
		/// </summary>
		public void SetScreenVisible(IReadOnlyScreenInfo target) => _screenCaptureManager.SetWakeUpWindow(target.Id);

		/// <summary>
		/// 지정한 화면을 캡쳐합니다. 캡쳐 시작시 <see cref="CapturedImageChanged"/> 이벤트가 호출됩니다.
		/// </summary>
		public bool StartCapture(IReadOnlyScreenInfo target, eScreenShareSignal reason) => StartCaptureImpl(target.Id, reason);

		/// <summary>
		/// 캡쳐를 중지합니다. 캡쳐 중지시 <see cref="CapturedImageChanged"/> 이벤트가 호출됩니다.
		/// </summary>
		public void StopCapture(eScreenShareSignal reason) => StopCaptureImpl(reason);
#endregion // PublicMethods - Capture

#region PublicMethods - Metadata
		/// <summary>
		/// 화면 목록에서 메타데이터가 존재하는 경우, 해당 메타데이터를 업데이트합니다.
		/// </summary>
		public void UpdateAllMetadata()
		{
			foreach (var screenInfo in _screenInfos.Values)
				UpdateMetadata(screenInfo);
		}

		/// <summary>
		/// 화면 목록에서 메타데이터가 존재 여부와 상관없이, 모든 메타데이터를 업데이트합니다.
		/// </summary>
		public void RequestAllMetadata()
		{
			foreach (var screenInfo in _screenInfos.Values)
				RequestMetadata(screenInfo);
		}

		/// <summary>
		/// 화면 목록의 모든 메타데이터 자원을 해제합니다.
		/// </summary>
		public void DestroyAllMetadata()
		{
			foreach (var value in _screenInfos.Values)
				DestroyMetadata(value);
		}

		/// <summary>
		/// 메타데이터가 존재하는 경우, 해당 메타데이터를 업데이트합니다.
		/// </summary>
		public void UpdateMetadata(IReadOnlyScreenInfo value)
		{
			if (TryFindScreenInfo(value.Id, out var screenInfo))
			{
				UpdateTitle(screenInfo);
				UpdateIcon(screenInfo);
				UpdateThumbnail(screenInfo);
			}
		}

		/// <summary>
		/// 메타데이터가 존재 여부와 상관없이, 해당 메타데이터를 업데이트합니다.
		/// </summary>
		public void RequestMetadata(IReadOnlyScreenInfo value)
		{
			if (TryFindScreenInfo(value.Id, out var screenInfo))
			{
				UpdateTitle(screenInfo);
				RequestIcon(screenInfo);
				RequestThumbnail(screenInfo);
			}
		}

		/// <summary>
		/// 메타데이터 자원을 해제합니다.
		/// </summary>
		public void DestroyMetadata(IReadOnlyScreenInfo value)
		{
			if (TryFindScreenInfo(value.Id, out var screenInfo))
			{
				UpdateTitle(screenInfo);
				DestroyIcon(screenInfo);
				DestroyThumbnail(screenInfo);
			}
		}
#endregion // PublicMethods - Metadata
	}
}
