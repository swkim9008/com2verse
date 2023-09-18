/*===============================================================
* Product:		Com2Verse
* File Name:	ScreenCaptureController.cs
* Developer:	urun4m0r1
* Date:			2022-09-23 16:26
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Solution.ScreenCapture;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using SdkScreenId = System.Int32;

namespace Com2Verse.ScreenShare
{
	public partial class ScreenCaptureController : IDisposable
	{
		private ScreenInfo? _currentScreen;

		private ScreenInfo? _requestedScreen;

		private CancellationTokenSource? _captureToken;

		private readonly Dictionary<ScreenId, ScreenInfo> _screenInfos = new(ScreenIdComparer.Default);

		private readonly MgrScreenCapture _screenCaptureManager;

		internal ScreenCaptureController(MgrScreenCapture screenCaptureManager)
		{
			_screenCaptureManager = screenCaptureManager;

			Fps = Define.DefaultCaptureFps;
		}

#region Initialize
		/// <summary>
		/// 해당 클래스가 초기화되었는지 여부를 반환합니다.
		/// </summary>
		public bool IsInitialized { get; private set; }

		/// <summary>
		/// 클래스 초기화를 수행합니다.
		/// </summary>
		public void Initialize()
		{
			if (IsInitialized)
				return;

			ResetControllerState();

			Debug.ScreenCaptureLogMethod();
			RegisterCallbackMethods();
			RequestScreenInfoList();

			IsInitialized = true;
		}

		/// <summary>
		/// 클래스 기능을 해제합니다.
		/// </summary>
		public void Dispose()
		{
			if (!IsInitialized)
				return;

			Debug.ScreenCaptureLogMethod();
			ResetControllerState();

			IsInitialized = false;
		}

		private void ResetControllerState()
		{
			UnregisterCallbackMethods();

			SetRequestedScreen(null);
			DisposeCaptureToken();

			if (IsCapturing)
			{
				_screenCaptureManager.SetCaptureFinish();
				Debug.ScreenCaptureLogMethod($"Set capture finish. / {_currentScreen}");

				_currentScreen = null;
			}

			ScreenCaptureManager.Instance.SendSignal(eScreenShareSignal.CAPTURE_STOPPED_BY_SYSTEM);
			CapturedImageChanged?.Invoke(null);

			ResetScreenInfoList();
		}

		private void ResetScreenInfoList()
		{
			foreach (var screenInfo in _screenInfos.Values)
			{
				ScreenInfoRemoved?.Invoke(screenInfo);
				DestroyResources(screenInfo);
			}

			_screenInfos.Clear();
		}

		private void DisposeCaptureToken()
		{
			_captureToken?.Cancel();
			_captureToken?.Dispose();
			_captureToken = null;
		}

		private void RegisterCallbackMethods()
		{
			_screenCaptureManager.OnScreenAddedDelegate       += OnScreenAdded;
			_screenCaptureManager.OnScreenRemovedDelegate     += OnScreenRemoved;
			_screenCaptureManager.OnScreenVisibilityChanged   += OnScreenVisibilityChanged;
			_screenCaptureManager.OnIconCapturedDelegate      += OnIconCaptured;
			_screenCaptureManager.OnThumbnailCapturedDelegate += OnThumbnailCaptured;
			_screenCaptureManager.OnCapturedDelegate          += OnScreenCaptured;
			_screenCaptureManager.OnCaptureResizedDelegate    += OnScreenCaptureResized;
			_screenCaptureManager.ScreenManagerRun(TextureHelper.IsLinearColorSpace);
		}

		private void UnregisterCallbackMethods()
		{
			_screenCaptureManager.OnScreenAddedDelegate       -= OnScreenAdded;
			_screenCaptureManager.OnScreenRemovedDelegate     -= OnScreenRemoved;
			_screenCaptureManager.OnScreenVisibilityChanged   -= OnScreenVisibilityChanged;
			_screenCaptureManager.OnIconCapturedDelegate      -= OnIconCaptured;
			_screenCaptureManager.OnThumbnailCapturedDelegate -= OnThumbnailCaptured;
			_screenCaptureManager.OnCapturedDelegate          -= OnScreenCaptured;
			_screenCaptureManager.OnCaptureResizedDelegate    -= OnScreenCaptureResized;
			_screenCaptureManager.ScreenManagerFinish();
		}
#endregion // Initialize

#region CallbackMethods
		private void OnScreenAdded(SdkScreenId id, bool isWindow)
		{
			if (TryFindScreenInfo(id, out var screenInfo))
			{
				Debug.ScreenCaptureLogWarningMethod($"ScreenInfo already exists. / {screenInfo}");
			}
			else
			{
				screenInfo = GetOrAddScreenInfo(id, isWindow);

				LogCallbackMethod(screenInfo);
			}
		}

		private void OnScreenRemoved(SdkScreenId id, bool isWindow)
		{
			if (IsCapturingOrRequestedScreen(id))
				StopCapture(eScreenShareSignal.CAPTURE_STOPPED_BY_SCREEN_REMOVED);

			if (TryFindScreenInfo(id, out var screenInfo))
			{
				_screenInfos.Remove(id);
				ScreenInfoRemoved?.Invoke(screenInfo);
				DestroyResources(screenInfo);

				LogCallbackMethod(screenInfo);
			}
			else
			{
				LogScreenInfoNotFound(id);
			}
		}

		private void OnScreenVisibilityChanged(SdkScreenId id, bool isWindow, bool isMinimized)
		{
			if (TryFindScreenInfo(id, out var screenInfo))
			{
				UpdateVisibility(screenInfo, !isMinimized);
				LogCallbackMethod(screenInfo);
			}
			else
			{
				LogScreenInfoNotFound(id);
			}
		}

		private void OnIconCaptured(SdkScreenId id, bool isWindow, Texture2D? texture)
		{
			if (TryFindScreenInfo(id, out var screenInfo))
			{
				ReplaceIcon(screenInfo, texture);
				LogCallbackMethod(screenInfo);
			}
			else
			{
				LogScreenInfoNotFound(id);
			}
		}

		private void OnThumbnailCaptured(SdkScreenId id, bool isWindow, Texture2D? texture)
		{
			if (TryFindScreenInfo(id, out var screenInfo))
			{
				ReplaceThumbnail(screenInfo, texture);
				LogCallbackMethod(screenInfo);
			}
			else
			{
				LogScreenInfoNotFound(id);
			}
		}

		private void OnScreenCaptured(SdkScreenId id, bool isWindow, Texture2D? texture)
		{
			if (texture.IsReferenceNull())
				OnCaptureStopped(id);
			else
				OnCaptureStarted(id, texture);
		}

		private void OnCaptureStarted(SdkScreenId id, Texture2D? texture)
		{
			if (TryFindScreenInfo(id, out var screenInfo))
			{
				_currentScreen = screenInfo;
				ReplaceScreen(screenInfo, texture);

				CapturedImageChanged?.Invoke(texture);
				LogCallbackMethod(screenInfo);

				if (!IsRequestedScreen(id))
					Debug.ScreenCaptureLogWarningMethod($"Capture started but not requested. / {screenInfo}");
			}
			else
			{
				LogScreenInfoNotFound(id);
			}
		}

		private void OnCaptureStopped(SdkScreenId id)
		{
			_currentScreen = null;

			if (TryFindScreenInfo(id, out var screenInfo))
			{
				DestroyScreen(screenInfo);
				LogCallbackMethod(screenInfo);
			}
			else
			{
				LogScreenInfoNotFound(id);
			}
		}

		private void OnScreenCaptureResized(SdkScreenId id, bool isWindow)
		{
			if (TryFindScreenInfo(id, out var screenInfo))
			{
				UpdateScreenSize(screenInfo);
				LogCallbackMethod(screenInfo);
			}
			else
			{
				LogScreenInfoNotFound(id);
			}
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		private static void LogCallbackMethod(ScreenInfo value, [CallerMemberName] string? caller = null)
			=> Debug.ScreenCaptureLogMethod(value.GetInfoText(), caller);

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		private static void LogScreenInfoNotFound(SdkScreenId id, [CallerMemberName] string? caller = null)
			=> Debug.ScreenCaptureLogWarningMethod($"ScreenInfo not found. {id.ToString()}", caller);
#endregion // CallbackMethods

#region InternalLogic
		private void RequestScreenInfoListImpl()
		{
			ResetScreenInfoList();

			var screenList = _screenCaptureManager.GetScreenCaptureLst();
			if (screenList == null)
			{
				Debug.ScreenCaptureLogErrorMethod("Failed to get screen list");
				return;
			}

			foreach (var value in screenList)
				GetOrAddScreenInfo(value.Id, value.IsWindow);
		}

		private ScreenInfo GetOrAddScreenInfo(SdkScreenId id, bool isWindow)
		{
			if (TryFindScreenInfo(id, out var screenInfo))
				return screenInfo;

			var screenType = isWindow ? eScreenType.WINDOW : eScreenType.DESKTOP;
			screenInfo = new ScreenInfo(id, screenType)
			{
				Title     = _screenCaptureManager.GetTitle(id),
				IsVisible = !_screenCaptureManager.GetStatusMinimized(id),
			};

			_screenInfos.TryAdd(id, screenInfo);
			ScreenInfoAdded?.Invoke(screenInfo);
			return screenInfo;
		}

		private bool StartCaptureImpl(ScreenId id, eScreenShareSignal reason)
		{
			if (!TryGetValidatedCaptureTarget(id, out var target))
			{
				SetRequestedScreen(null);
				return false;
			}

			if (IsCapturingOrRequestedScreen(target))
			{
				Debug.ScreenCaptureLogWarningMethod($"Already requested screen. / {target}");
				return false;
			}

			SetRequestedScreen(target);

			DisposeCaptureToken();
			_captureToken ??= new CancellationTokenSource();
			StartCaptureAsync(target, reason).Forget();

			Debug.ScreenCaptureLogMethod(target.ToString());
			return true;
		}

		private async UniTaskVoid StartCaptureAsync(ScreenInfo target, eScreenShareSignal reason)
		{
			if (!await ValidateAndStartCaptureAsync(target))
			{
				SetRequestedScreen(null);
				return;
			}

			DisposeCaptureToken();
			ScreenCaptureManager.Instance.SendSignal(reason);
			Debug.ScreenCaptureLogMethod(target.ToString());
		}

		private void StopCaptureImpl(eScreenShareSignal reason)
		{
			if (!IsCaptureRequestedOrCapturing)
			{
				Debug.ScreenCaptureLogWarningMethod("Not capturing or requested screen.");
				return;
			}

			SetRequestedScreen(null);
			DisposeCaptureToken();

			if (IsCapturing)
			{
				_screenCaptureManager.SetCaptureFinish();
				Debug.ScreenCaptureLogMethod($"Set capture finish. / {_currentScreen}");
			}

			ScreenCaptureManager.Instance.SendSignal(reason);
			CapturedImageChanged?.Invoke(null);
		}

		private bool TryGetValidatedCaptureTarget(ScreenId id, [NotNullWhen(true)] out ScreenInfo? screenInfo)
		{
			screenInfo = null;

			if (!IsInitialized)
			{
				Debug.ScreenCaptureLogWarningMethod("Not initialized.");
				return false;
			}

			if (!TryFindScreenInfo(id, out screenInfo))
			{
				LogScreenInfoNotFound(id);
				return false;
			}

			return true;
		}

		private async UniTask<bool> ValidateAndStartCaptureAsync(ScreenInfo target)
		{
			if (!await WaitForCaptureFinished())
			{
				Debug.ScreenCaptureLogErrorMethod($"Failed to wait for capture finished. / {_currentScreen}");
				return false;
			}

			if (!await WaitForScreenVisible())
			{
				Debug.ScreenCaptureLogErrorMethod($"Failed to wait for screen visible. / {target}");
				return false;
			}

			if (!TryGetValidatedCaptureTarget(target.Id, out _))
			{
				Debug.ScreenCaptureLogErrorMethod($"Failed to validate capture target. / {target}");
				return false;
			}

			if (IsCapturingScreen(target))
			{
				Debug.ScreenCaptureLogErrorMethod($"Already capturing screen. / {target}");
				return false;
			}

			if (!TryStartCapture())
			{
				Debug.ScreenCaptureLogErrorMethod($"Failed to start capture. / {target}");
				return false;
			}

			return true;

			async UniTask<bool> WaitForCaptureFinished()
			{
				_screenCaptureManager.SetCaptureFinish();
				Debug.ScreenCaptureLogMethod($"Set capture finish. / {_currentScreen}");

				return await UniTaskHelper.WaitUntil(() => !IsCapturing, _captureToken);
			}

			async UniTask<bool> WaitForScreenVisible()
			{
				SetScreenVisible(target);
				Debug.ScreenCaptureLogMethod($"Set screen visible. / {target}");

				if (target.IsVisible)
					return true;

				if (!await UniTaskHelper.WaitUntil(() => target.IsVisible, _captureToken))
					return false;

				return await UniTaskHelper.Delay(Define.MinimizedScreenRestoreDelay, _captureToken);
			}

			bool TryStartCapture()
			{
				var screenSize = CalculateScreenSize(target.Id);
				return _screenCaptureManager.SetCaptureStart(target.Id, screenSize);
			}
		}

		private void SetRequestedScreen(ScreenInfo? screenInfo)
		{
			_requestedScreen = screenInfo;
			RequestedScreenChanged?.Invoke(screenInfo);
		}

		private Vector2Int CalculateThumbnailSize(ScreenId id) => CalculateOutputSize(id, RequestedThumbnailSize);
		private Vector2Int CalculateScreenSize(ScreenId    id) => CalculateOutputSize(id, RequestedScreenSize);

		private Vector2Int CalculateOutputSize(ScreenId id, Vector2Int requestSize)
		{
			var rawSize     = _screenCaptureManager.GetScreenRawSize(id);
			var widthRatio  = (float)requestSize.x / rawSize.x;
			var heightRatio = (float)requestSize.y / rawSize.y;

			var resizeRatio = Mathf.Min(widthRatio, heightRatio);

			var outputWidth  = Mathf.RoundToInt(rawSize.x * resizeRatio);
			var outputHeight = Mathf.RoundToInt(rawSize.y * resizeRatio);

			MathUtil.Clamp(ref outputWidth,  MinScreenSize.x, MaxScreenSize.x);
			MathUtil.Clamp(ref outputHeight, MinScreenSize.y, MaxScreenSize.y);

			return new Vector2Int(outputWidth, outputHeight);
		}
#endregion // InternalLogic
	}
}
