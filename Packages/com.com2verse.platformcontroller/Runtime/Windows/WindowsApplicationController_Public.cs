/*===============================================================
* Product:		Com2Verse
* File Name:	WindowsApplicationController_Public.cs
* Developer:	mikeyid77
* Date:			2023-02-10 15:00
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Logger;
using UnityEngine;
using Cysharp.Threading.Tasks;
using static Com2Verse.PlatformControl.Windows.Win32API;

namespace Com2Verse.PlatformControl.Windows
{
    public partial class WindowsApplicationController : ApplicationController
    {
        public override void Initialize()
        {
            SetCurrentWindow();
            SetDisplayInfo();
#if !UNITY_EDITOR
            //SetWindowEventHook();
            //HitTestInitialize();
            //InitializeBuild();
#endif
            var lastWidth = 0;
            var maxResolutionIndex = Screen.resolutions.Length;
            for (int i = 0; i < maxResolutionIndex; i++)
            {
                // if (Screen.resolutions[i].width == Screen.resolutions[maxResolutionIndex].width) continue;
                if (Screen.resolutions[i].width == lastWidth) continue;
                
                lastWidth = Screen.resolutions[i].width;
                _resolutionList.Insert(0, Screen.resolutions[i]);
            }
        }

        private void InitializeBuild()
        {
            MoveSizeStartEvent += CheckBeforeSystemResize;
            MoveSizeEndEvent += SystemResize;
        }

        public override void OnUpdate()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Return))
            {
                ToggleScreenMode();
            }
        }
        
        public override void Terminate()
        {
            
        }

        public override void ToggleScreenMode()
        {
            if (IsApplicationState()) SetScreenModeAsync(!Screen.fullScreen);
        }

        public override async void SetScreenModeAsync(bool isFullScreen)
        {
#if !UNITY_EDITOR
            if (isFullScreen)
            {
                ChangeScreenModeAction?.Invoke(true);
                Screen.fullScreen = true;
                
                var index = Screen.resolutions.Length - 1;
                var largestResolution = Screen.resolutions[index];
                Screen.SetResolution(largestResolution.width, largestResolution.height, true);
            }
            else
            {
                ChangeScreenModeAction?.Invoke(false);
                Screen.fullScreen = false;
                await UniTask.DelayFrame(1);
                
                SetCurrentDisplay(Screen.width, Screen.height);
                SetTopmost(false);
                SetScreenResolutionAsync(_currentResolutionIndex);
            }
#endif
        }

        public override async void SetScreenResolutionAsync(int index)
        {
#if !UNITY_EDITOR
            if (index < 0)
            {
                _currentResolutionIndex = 0;
                return;
            }

            if (index >= _resolutionList?.Count)
            {
                _currentResolutionIndex = _resolutionList.Count - 1;
            }

            if (!Screen.fullScreen)
            {
                _currentResolutionIndex = index;
                await UniTask.DelayFrame(1);

                SetCurrentDisplay(Screen.width, Screen.height);
                SetTopmost(false);
                if (index == 0)
                {
                    SetWindowPosition(CurrentDisplay.Min.x, CurrentDisplay.Min.y, WindowWidth, WindowHeight);
                }
                else
                {
                    Screen.SetResolution(_resolutionList[_currentResolutionIndex].width,
                        _resolutionList[_currentResolutionIndex].height, false);
                }
                
            }
#endif
        }
        
        public override void AddDragManager()
        {
            var dragHelper = TargetInfo.Target.AddComponent<WindowsDragHelper>();
            dragHelper.Initialize(
                TargetInfo.RootRect,
                TargetInfo.TargetRect,
                GetWindowLeftTopAnchor,
                (x, y) => SetWindowPosition(x, y, Screen.width, Screen.height, eOrderFlagType.CHANGE_POSITION),
                () => SetCurrentDisplay(Screen.width, Screen.height));
        } 

        public override void EnterWorkspace(bool isFullScreen = false)
        {
            if (IsApplicationState())
            {
#if !UNITY_EDITOR
                EnterWorkspaceAsync(isFullScreen);
#else
                OnStartEnterWorkspace();
                OnEndEnterWorkspace();
#endif
            }
        }
        
        private async void EnterWorkspaceAsync(bool isFullscreen)
        {
            MinimizeStartEvent += SystemMinimizeStart;
            MinimizeEndEvent += SystemMinimizeEnd;
            _resizeApplicationWindowAction = (isFullscreen) ? ResizeApplicationScreen : ResizeApplicationTarget;

            OnStartEnterWorkspace();
            //_targetInfo.RootAlpha = 0f;
		                
            SaveWindowSettings(Screen.fullScreenMode == FullScreenMode.FullScreenWindow, Screen.width, Screen.height);
		                
            if (Screen.fullScreen)
            {
                ChangeWindowState((int)WindowCommand.SW_SHOWMINIMIZED);
            }
            else
            {
                C2VDebug.LogCategory("PlatformController", $"Start Minimized Process");

                // 리사이징을 위한 초기 설정
                await UniTask.WaitUntil(() => InitWindow() != 0);
						
                SetTransparentWindow(true);
                SetTopmost(true);
		                
                _resizeApplicationWindowAction?.Invoke();
                OnEndEnterWorkspace();
                HitTestAsync().Forget(); 
                // DOTween.To(() => 0f, (value) => _targetInfo.RootAlpha = value, 1f, 0.2f)
                //     .OnComplete(() => 
                //     { 
                //         OnEndEnterWorkspace();
                //         HitTestAsync().Forget(); 
                //     });
            }
        }
        
        public override void RestoreApplication()
        {
            if (IsWorkspaceState())
            {
                OnStartRestoreApplication();
#if !UNITY_EDITOR
                RestoreApplicationCheck();
#endif
                OnEndRestoreApplication();
            }
        }
        
        private void RestoreApplicationCheck()
        {
            SetTransparentWindow(false);
            SetTopmost(false);
            RepositionAssetObject();
        }
    }
}
