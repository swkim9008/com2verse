/*===============================================================
* Product:		Com2Verse
* File Name:	WindowsApplicationController_Private.cs
* Developer:	mikeyid77
* Date:			2023-02-10 15:00
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Com2Verse.Logger;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using static Com2Verse.PlatformControl.Windows.Win32API;

namespace Com2Verse.PlatformControl.Windows
{
    public partial class WindowsApplicationController : ApplicationController
    {
#region DEVICE
		private int _currentResolutionIndex;
		private readonly List<Resolution> _resolutionList = new();

		private void SetCurrentDisplay(int width, int height)
		{
			GetWindowLeftTopAnchor(out int left, out int top);
			var halfWidth = width / 2;
			var halfHeight = height / 2;
					
			foreach (var displayInfo in DisplayInfoList)
			{
				if (displayInfo.Min.x - halfWidth <= left && left <= displayInfo.Max.x - halfWidth
				                                          && displayInfo.Min.y - halfHeight <= top && top <= displayInfo.Max.y - halfHeight)
				{
					CurrentDisplayIndex = DisplayInfoList.FindIndex(target => target == displayInfo);
					C2VDebug.LogCategory("PlatformController", $"CurrentDisplay : {CurrentDisplay.Min} {CurrentDisplay.Max}");
					break;
				}
			}
		}
#endregion // DEVICE

#region WINDOW
		private IntPtr _topmostFlag = HWND_TOPMOST;
		private bool _isFullScreen = true;
		private int _beforeWidth = 0;
		private int _beforeHeight = 0;
		
		private enum eOrderFlagType
		{
			CURRENT,
			CHANGE_ORDER,
			CHANGE_SIZE,
			CHANGE_POSITION
		}
		
		private void SaveWindowSettings(bool isFullScreen, int width, int height)
		{
			_isFullScreen = isFullScreen;
			_beforeWidth = width;
			_beforeHeight = height;
		}

		private int InitWindow()
		{
			// Set Topmost
			//_topmostFlag = HWND_TOPMOST;
			
			// Borderless
			SetWindowLong(HWnd, (int)OptionOffset.GWL_STYLE, (uint)OptionStyle.WS_POPUP | (uint)OptionStyle.WS_VISIBLE);

			// Acrylic
			SetWindowLong(HWnd, (int)OptionOffset.GWL_EXSTYLE, (uint)OptionExStyle.WS_EX_LAYERED);
			return SetLayeredWindowAttributes(HWnd, SetColor(0, 0, 0), 255, (uint)TransparentFlag.LWA_ALPHA);

			// FullScreen Window
			
			//await UniTask.WaitUntil(() => SetWindowPos(_hWnd, _topmostFlag, 0, 0,  MaxWidth - 1, MaxHeight, SWP_FRAMECHANGED));
			//return SetWindowPos(_hWnd, _topmostFlag, 0, 0, CurrentDisplay.Width, CurrentDisplay.Height, SWP_FRAMECHANGED);
			
			// TODO : SubProcess
			// HWnd = GetActiveWindow();
			// 
			// // Borderless
			// SetWindowLong(HWnd, (int)OptionOffset.GWL_STYLE, (uint)OptionStyle.WS_POPUP | (uint)OptionStyle.WS_VISIBLE);
			//
			// // Acrylic & Hide Icon from Taskbar
			// SetWindowLong(HWnd, (int)OptionOffset.GWL_EXSTYLE, (uint)OptionExStyle.WS_EX_LAYERED | (uint)OptionExStyle.WS_EX_TOOLWINDOW);
			// SetLayeredWindowAttributes(HWnd, SetColor(0, 0, 0), 255, (uint)TransparentFlag.LWA_ALPHA);
			//
			// // Transparent
			// Margins margins = new Margins { left = -1 };
			// DwmExtendFrameIntoClientArea(HWnd, ref margins);
			//
			// OnEndEnterWorkspace();
			// return SetWindowPos(HWnd, _topmostFlag, 0, 0, (int)TargetInfo.TargetSize.x, (int)TargetInfo.TargetSize.y, (uint)WindowFlag.SWP_FRAMECHANGED);
		}
		
		private void SetCurrentWindow()
		{
			HWnd = GetActiveWindow();
			C2VDebug.LogCategory("PlatformController", $"CurrentWindow : {HWnd}");
		}
		
		private void ChangeWindowState(int cmdShow)
		{
			// 임시
			if(HWnd == IntPtr.Zero)
				HWnd = GetActiveWindow();
					
			C2VDebug.LogCategory("PlatformController", $"command : {cmdShow}, _hWnd : {HWnd}, IntPtr.Zero : {IntPtr.Zero}");
					
			if (HWnd != IntPtr.Zero)
				ShowWindow(HWnd, cmdShow);
			else
				C2VDebug.LogErrorCategory("PlatformController", $"_hWnd is IntPtr.Zero");
		}
		
		private void GetWindowLeftTopAnchor(out int left, out int top)
		{
			var windowRect = GetWindowRect(HWnd, out WinRect winRect);
			left = winRect.left;
			top = winRect.top;
		}

		private bool SetWindowPosition(int x, int y, int width, int height, eOrderFlagType order = eOrderFlagType.CURRENT)
		{
			switch (order)
			{
				case eOrderFlagType.CURRENT:
					return SetWindowPos(HWnd, _topmostFlag, x, y, width, height, (uint)WindowFlag.SWP_FRAMECHANGED);
				case eOrderFlagType.CHANGE_ORDER:
					return SetWindowPos(HWnd, _topmostFlag, x, y, width, height, (uint)WindowFlag.SWP_FRAMECHANGED | (uint)WindowFlag.SWP_NOSIZE | (uint)WindowFlag.SWP_NOMOVE);
				case eOrderFlagType.CHANGE_SIZE:
					return SetWindowPos(HWnd, _topmostFlag, x, y, width, height, (uint)WindowFlag.SWP_FRAMECHANGED | (uint)WindowFlag.SWP_NOMOVE);
				case eOrderFlagType.CHANGE_POSITION:
					return SetWindowPos(HWnd, _topmostFlag, x, y, width, height, (uint)WindowFlag.SWP_FRAMECHANGED | (uint)WindowFlag.SWP_NOSIZE);
			}
			return false;
		}
		
		private void SetTopmost(bool topmost)
		{
			_topmostFlag = (topmost) ? HWND_TOPMOST : HWND_NOTOPMOST;
		}  

		private void SetTransparentWindow(bool transparent)
		{
			if (transparent)
			{
				Margins margins = new Margins { left = -1 };
				DwmExtendFrameIntoClientArea(HWnd, ref margins);
				// SetLayeredWindowAttributes(_hWnd, Color(255, 0, 255), 255, LWA_COLORKEY);
			}
			else
			{
				Margins margins = new Margins { left = 0, right = 0, top = 0, bottom = 0 };
				DwmExtendFrameIntoClientArea(HWnd, ref margins);
				// SetLayeredWindowAttributes(_hWnd, Color(255, 0, 255), 255, LWA_ALPHA);
				_topmostFlag = HWND_TOPMOST;
				// SetWindowPos(_hWnd, _topmostFlag, CurrentDisplay.Min.x, CurrentDisplay.Min.y,
				// 	CurrentDisplay.Width, CurrentDisplay.Height, SWP_FRAMECHANGED);

				if (_isFullScreen)
				{
					var index = Screen.resolutions.Length - 1;
					var largestResolution = Screen.resolutions[index];
					Screen.SetResolution(largestResolution.width,  largestResolution.height,true);
				}
				else
				{
					Screen.SetResolution(_beforeWidth, _beforeHeight, false);
				}
			}
		}

		private void SetWindowExStyle(bool trigger)
		{
			if (trigger)
			{
				long exstyle = GetWindowLong(HWnd, (int)OptionOffset.GWL_EXSTYLE);
				exstyle &= ~(uint)OptionExStyle.WS_EX_TRANSPARENT;
				SetWindowLong(HWnd, (int)OptionOffset.GWL_EXSTYLE, (uint)exstyle);
			}
			else
			{
				long exstyle = GetWindowLong(HWnd, (int)OptionOffset.GWL_EXSTYLE);
				exstyle |= (uint)OptionExStyle.WS_EX_TRANSPARENT;
				exstyle |= (uint)OptionExStyle.WS_EX_LAYERED;
				SetWindowLong(HWnd, (int)OptionOffset.GWL_EXSTYLE, (uint)exstyle);
			}
		}
#endregion // WINDOW

#region HIT_TEST
		private Camera _baseCamera;
		private PointerEventData _pointerEventData;
		private int _hitTestLayerMask;
		private bool _isObjectExists;
		
		private void HitTestInitialize()
		{
			_pointerEventData = new PointerEventData(EventSystem.current);
			_hitTestLayerMask = ~LayerMask.GetMask("Ignore Raycast");
			_baseCamera = Camera.main;
		}
				
		private async UniTask HitTestAsync()
		{
			while (IsWorkspaceState())
			{
				await UniTask.DelayFrame(1);
				HitTestByRaycast();
				SetWindowExStyle(_isObjectExists);
			}

			await UniTask.DelayFrame(1);
			SetWindowExStyle(true);
		}

		private void HitTestByRaycast()
		{
			var position = Mouse.current.position.ReadValue();

			var raycastResults = new List<RaycastResult>();
			_pointerEventData.position = position;
			EventSystem.current.RaycastAll(_pointerEventData, raycastResults);
			foreach (var result in raycastResults)
			{
				if (((1 << result.gameObject.layer) & _hitTestLayerMask) > 0)
				{
					_isObjectExists = true;
					return;
				}
			}

			if (_baseCamera && _baseCamera.isActiveAndEnabled)
			{
				Ray ray = _baseCamera.ScreenPointToRay(position);

				var rayHit2D = Physics2D.GetRayIntersection(ray);
				UnityEngine.Debug.DrawRay(ray.origin, ray.direction, Color.blue, 2f, false);
				if (rayHit2D.collider != null)
				{
					_isObjectExists = true;
					return;
				}
			}
			else
			{
				_baseCamera = Camera.main;
			}

			_isObjectExists = false;
		}
#endregion // HIT_TEST

#region EVENT
		private IntPtr _windowEventHook = IntPtr.Zero;
		private Action _resizeApplicationWindowAction;

		private void SetWindowEventHook()
		{
			if (_windowEventHook == IntPtr.Zero)
			{
				_windowEventHook = SetWinEventHook(
					(int)WinEvent.EVENT_MIN,     // eventMin
					(int)WinEvent.EVENT_AIA_END, // eventMax
					HWnd,                       // hmodWinEventProc
					WindowEventCallback,         // lpfnWinEventProc
					0,                           // idProcess
					0,                           // idThread
					(int)WinEventFlags.WINEVENT_OUTOFCONTEXT);

				if (_windowEventHook == IntPtr.Zero)
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
			}
		}
#endregion // EVENT

#region RESIZE
		private RectTransform _rootCanvasSize;
		private Vector2 _rootCanvasResolution;
		private int _beforeResizeWidth;
		private int _beforeResizeHeight;
		private float _targetRatio;
		private float _currentWindowRatio => (float)CurrentDisplay.Width / (float)CurrentDisplay.Height;
		private float _checkWindowRatio => Math.Min(_currentWindowRatio / _targetRatio, 1f);
		private bool _isTargetMode = false;
		private static int WindowWidth => GetSystemMetrics((int)OptionSystemMetrics.SM_CXFULLSCREEN) + GetSystemMetrics((int)OptionSystemMetrics.SM_CXSIZEFRAME) * 2;
		private static int WindowHeight => GetSystemMetrics((int)OptionSystemMetrics.SM_CYFULLSCREEN) + GetSystemMetrics((int)OptionSystemMetrics.SM_CYSIZEFRAME) * 2 + GetSystemMetrics((int)OptionSystemMetrics.SM_CYSIZE);
		
        private void ResizeApplicationScreen()
        {
	        C2VDebug.LogCategory("PlatformController", $"Resize Application Window to Full Screen");
            
            SetCurrentDisplay(Screen.width, Screen.height);
            var x = CurrentDisplay.Min.x;
            var y = CurrentDisplay.Min.y;
            var width = CurrentDisplay.Width;
            var height = CurrentDisplay.Height;
            _isTargetMode = false;
			
            SetWindowPosition(x, y, width, height);
        }

        private void ResizeApplicationTarget()
        {
	        C2VDebug.LogCategory("PlatformController", $"Resize Application Window to Target");
            
            SetCurrentDisplay(Screen.width, Screen.height);
            var windowRatio = CurrentDisplay.Height / TargetInfo.RootResolution.y * _checkWindowRatio;
            var x = (int)(TargetInfo.TargetPosition.x * windowRatio) + CurrentDisplay.Min.x;
            var y = -(int)(TargetInfo.TargetPosition.y * windowRatio) + CurrentDisplay.Min.y;
            var width = (int)Math.Ceiling(TargetInfo.TargetSize.x * windowRatio);
            var height = (int)Math.Ceiling(TargetInfo.TargetSize.y * windowRatio);
            
            TargetInfo.TargetScale = Vector3.one * (TargetInfo.RootResolution.y / TargetInfo.TargetSize.y);
            TargetInfo.TargetPosition = Vector2.zero;
            _isTargetMode = true;
			
            SetWindowPosition(x, y, width, height);
        }
		
		private async void SystemMinimizeStart()
		{
			if (IsResizingState())
			{
				// 창 모드로 변경
				if (Screen.fullScreen)
				{
					C2VDebug.LogCategory("PlatformController", $"Change Window Mode");
					Screen.fullScreenMode = FullScreenMode.Windowed;
					await UniTask.DelayFrame(1);
				}

				C2VDebug.LogCategory("PlatformController", $"Start Minimized Process");

				// 리사이징을 위한 초기 설정
				await UniTask.WaitUntil(() => InitWindow() != 0);
				
				SetTransparentWindow(true);
				SetTopmost(true);
				ChangeWindowState((int)WindowCommand.SW_SHOWNORMAL);
			}
		}

		private void SystemMinimizeEnd()
		{
			if (IsResizingState())
			{
				_resizeApplicationWindowAction?.Invoke();
				OnEndEnterWorkspace();
				HitTestAsync().Forget(); 
				// DOTween.To(() => 0f, (value) => TargetInfo.RootAlpha = value, 1f, 0.2f)
				// 	.OnComplete(() => 
				// 	{ 
				// 		OnEndEnterWorkspace();
				// 		HitTestAsync().Forget(); 
				// 	});
			}
		}

		private void CheckBeforeSystemResize()
		{
			_beforeResizeWidth = Screen.width;
			_beforeResizeHeight = Screen.height;
		}

		private void SystemResize()
		{
			if (_beforeResizeWidth != Screen.width)
			{
				SetWindowPosition(0, 0, Screen.width, (int)(Screen.width / _targetRatio), eOrderFlagType.CHANGE_SIZE);
			}
			else if (_beforeResizeHeight != Screen.height)
			{
				SetWindowPosition(0, 0, (int)(Screen.height * _targetRatio), Screen.height, eOrderFlagType.CHANGE_SIZE);
			}
		}

		private void RepositionAssetObject()
		{
			if (_isTargetMode)
			{
				GetWindowLeftTopAnchor(out var l, out var t);
				var windowRatio = CurrentDisplay.Height / TargetInfo.RootResolution.y * _checkWindowRatio;
				var left = (float)(l - CurrentDisplay.Min.x) / windowRatio;
				var top = -(float)(t - CurrentDisplay.Min.y) / windowRatio;
				TargetInfo.TargetPosition = new Vector2(left, top);
				TargetInfo.TargetScale = Vector3.one;
			}
			// var x = TargetInfo.TargetPosition.x;
			// var y = TargetInfo.TargetPosition.y;
			//
			// if (x < 0)
			// 	x = 0;
			// else if (x + TargetInfo.TargetSize.x > TargetInfo.RootSize.x)
			// 	x = TargetInfo.RootSize.x - TargetInfo.TargetSize.x;
			//
			// if (y - TargetInfo.TargetSize.y < -TargetInfo.RootSize.y)
			// 	y = TargetInfo.TargetSize.y - TargetInfo.RootSize.y;
			// else if (y > -0)
			// 	y = 0;
			//
			// TargetInfo.TargetScale = Vector3.one;
			// TargetInfo.TargetPosition = new Vector2(x, y);
		}
#endregion // RESIZE
    }
}
