/*===============================================================
* Product:		Com2Verse
* File Name:	ApplicationController.cs
* Developer:	mikeyid77
* Date:			2023-02-10 15:00
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.PlatformControl
{
    public abstract class ApplicationController
    {
        protected TargetInfo TargetInfo = new();
        protected Action<bool> ChangeScreenModeAction;
        
        private Action _startEnterWorkspaceAction;
        private Action _endEnterWorkspaceAction;
        private Action _startRestoreApplicationAction;
        private Action _endRestoreApplicationAction;
        private eResizeState _currentResizeState = eResizeState.APPLICATION;
        
        public bool IsWorkspaceState() => _currentResizeState == eResizeState.WORKSPACE;
        public bool IsResizingState() => _currentResizeState == eResizeState.RESIZING;
        public bool IsApplicationState() => _currentResizeState == eResizeState.APPLICATION;
        protected bool TargetIsNull() => TargetInfo.IsNull();

        public event Action StartEnterWorkspaceEvent
        {
            add
            {
                _startEnterWorkspaceAction -= value;
                _startEnterWorkspaceAction += value;
            }
            remove => _startEnterWorkspaceAction -= value;
        }
        
        public event Action EndEnterWorkspaceEvent
        {
            add
            {
                _endEnterWorkspaceAction -= value;
                _endEnterWorkspaceAction += value;
            }
            remove => _endEnterWorkspaceAction -= value;
        }
        
        public event Action StartRestoreApplicationEvent
        {
            add
            {
                _startRestoreApplicationAction -= value;
                _startRestoreApplicationAction += value;
            }
            remove => _startRestoreApplicationAction -= value;
        }
        
        public event Action EndRestoreApplicationEvent
        {
            add
            {
                _endRestoreApplicationAction -= value;
                _endRestoreApplicationAction += value;
            }
            remove => _endRestoreApplicationAction -= value;
        }
        
        public event Action<bool> ChangeScreenModeEvent
        {
            add
            {
                ChangeScreenModeAction -= value;
                ChangeScreenModeAction += value;
            }
            remove => ChangeScreenModeAction -= value;
        }
        
        protected void OnStartEnterWorkspace()
        {
            C2VDebug.LogCategory("PlatformController", "Start Enter Workspace");

            _currentResizeState = eResizeState.RESIZING;
            _startEnterWorkspaceAction?.Invoke();
        }
        
        protected void OnEndEnterWorkspace()
        {
            C2VDebug.LogCategory("PlatformController", "End Enter Workspace");

            _currentResizeState = eResizeState.WORKSPACE;
            _endEnterWorkspaceAction?.Invoke();
        }
        
        protected void OnStartRestoreApplication()
        {
            C2VDebug.LogCategory("PlatformController", $"Start Restore Application");
            
            _currentResizeState = eResizeState.RESIZING;
            _startRestoreApplicationAction?.Invoke();
        }
        
        protected void OnEndRestoreApplication()
        {
            C2VDebug.LogCategory("PlatformController", $"End Restore Application");
            
            _currentResizeState = eResizeState.APPLICATION;
            _endRestoreApplicationAction?.Invoke();
        }

        public void SetTargetInfo(RectTransform targetUI, RectTransform rootCanvas)
        {
            TargetInfo.Initialize(targetUI, rootCanvas);
        }

        public void ResetTargetInfo()
        {
            TargetInfo.Reset();
        }

        public abstract void Initialize();
        public abstract void OnUpdate();
        public abstract void Terminate();
        public virtual void ToggleScreenMode() { }
        public virtual void SetScreenModeAsync(bool isFullScreen) { }
        public virtual void SetScreenResolutionAsync(int index) { }
        public virtual void AddDragManager() { }
        public virtual void EnterWorkspace(bool isFullScreen = false) { }
        public virtual void RestoreApplication() { }
    }
    
    public class TargetInfo
    {
        private RectTransform _targetUI;
        private RectTransform _rootCanvas;

        public TargetInfo() { Reset(); }
        public void Initialize(RectTransform targetUI, RectTransform rootCanvas)
        {
            _targetUI = targetUI;
            _rootCanvas = rootCanvas;
        }
        public void Reset()
        {
            _targetUI = null;
            _rootCanvas = null;
        }

        public bool IsNull() => _targetUI == null;
        public GameObject Target => IsNull() ? null : _targetUI.gameObject;
        public RectTransform TargetRect => IsNull() ? null : _targetUI;
        public Vector2 TargetSize
        {
            get => IsNull() ? Vector2.zero : _targetUI.sizeDelta;
            set
            {
                if (IsNull()) return;
                _targetUI.sizeDelta = value;
            }
        }
        public Vector2 TargetPosition 
        {
            get => IsNull() ? Vector2.zero : _targetUI.anchoredPosition;
            set
            {
                if (IsNull()) return;
                _targetUI.anchoredPosition = value;
            }
        }
        public Vector3 TargetScale 
        {
            get => IsNull() ? Vector3.zero : _targetUI.localScale;
            set
            {
                if (IsNull()) return;
                _targetUI.localScale = value;
            }
        }
        public RectTransform RootRect => IsNull() ? null : _rootCanvas;
        public Vector2 RootSize => IsNull() ? Vector2.zero : _rootCanvas.sizeDelta;
        public Vector2 RootResolution => IsNull() ? Vector2.zero : _rootCanvas.GetComponent<CanvasScaler>().referenceResolution;
        public float RootAlpha 
        {
            get => IsNull() ? 0f : _rootCanvas.GetComponent<CanvasGroup>().alpha;
            set
            {
                if (IsNull()) return;
                _rootCanvas.GetComponent<CanvasGroup>().alpha = value;
            }
        }
    }
}

