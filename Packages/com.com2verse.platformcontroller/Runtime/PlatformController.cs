/*===============================================================
* Product:		Com2Verse
* File Name:	PlatformController.cs
* Developer:	mikeyid77
* Date:			2023-02-10 15:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using UnityEngine;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using Com2Verse.PlatformControl.Windows;
#endif

namespace Com2Verse.PlatformControl
{
    public class PlatformController : Singleton<PlatformController>, IDisposable
    {
        private ApplicationController _applicationController = null;
        private ProcessController _processController = null;

        private PlatformController() { }
        
        public void CreateController() => CreateNewController(out _applicationController, out _processController);
        
        private void CreateNewController(out ApplicationController application, out ProcessController process)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            application = new WindowsApplicationController();
#endif
            process = new ProcessController();
        }

        private bool CheckApplicationController(bool enterLog = true)
        {
            if (_applicationController == null)
            {
                if (enterLog) C2VDebug.LogWarningCategory("PlatformController", "ApplicationController is not Exists");
                return false;
            }
            return true;
        }

        private bool CheckProcessController(bool enterLog = true)
        {
            if (_processController == null)
            {
                if (enterLog) C2VDebug.LogWarningCategory("PlatformController", "ProcessController is not Exists");
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            if (CheckApplicationController())
            {
                _applicationController.Terminate();
                _applicationController = null;
            }
            
            if (CheckProcessController())
            {
                _processController.Terminate();
                _processController = null;
            }
        }
        
#region APPLICATION
        public void InitializeApplicationController() => _applicationController?.Initialize();
        public void OnUpdateApplicationController() => _applicationController?.OnUpdate();
        public bool IsWorkspaceState() => _applicationController?.IsWorkspaceState() ?? false;
        public void ToggleScreenMode() => _applicationController?.ToggleScreenMode();
        public void SetScreenMode(bool isFullScreen) => _applicationController?.SetScreenModeAsync(isFullScreen);
        public void SetScreenResolution(int index) => _applicationController?.SetScreenResolutionAsync(index);
        
        public void AddDragManager(RectTransform targetUI, RectTransform rootCanvas)
        {
            if (!CheckApplicationController()) return;
            
            _applicationController.SetTargetInfo(targetUI, rootCanvas);
            _applicationController.AddDragManager();
        }

        public void EnterWorkspace(RectTransform targetUI, RectTransform rootCanvas, bool isFullScreen = false)
        {
            if (!CheckApplicationController()) return;
            
            _applicationController.SetTargetInfo(targetUI, rootCanvas);
            _applicationController.EnterWorkspace(isFullScreen);
        }

        public void RestoreApplication()
        {
            if (!CheckApplicationController()) return;
            
            _applicationController.RestoreApplication();
        }
        
        public void AddEvent(eApplicationEventType type, Action action)
        {
            if (!CheckApplicationController()) return;
            
            switch (type)
            {
                case eApplicationEventType.START_ENTER:
                    _applicationController.StartEnterWorkspaceEvent += action;
                    break;
                case eApplicationEventType.END_ENTER:
                    _applicationController.EndEnterWorkspaceEvent += action;
                    break;
                case eApplicationEventType.START_RESTORE:
                    _applicationController.StartRestoreApplicationEvent += action;
                    break;
                case eApplicationEventType.END_RESTORE:
                    _applicationController.EndRestoreApplicationEvent += action;
                    break;
            }
        }

        public void AddEvent(eApplicationEventType type, Action<bool> action)
        {
            if (!CheckApplicationController()) return;
            
            switch (type)
            {
                case eApplicationEventType.CHANGE_SCREEN_MODE:
                    _applicationController.ChangeScreenModeEvent += action;
                    break;
            }
        }
        
        public void RemoveEvent(eApplicationEventType type, Action action)
        {
            if (!CheckApplicationController()) return;
            
            switch (type)
            {
                case eApplicationEventType.START_ENTER:
                    _applicationController.StartEnterWorkspaceEvent -= action;
                    break;
                case eApplicationEventType.END_ENTER:
                    _applicationController.EndEnterWorkspaceEvent -= action;
                    break;
                case eApplicationEventType.START_RESTORE:
                    _applicationController.StartRestoreApplicationEvent -= action;
                    break;
                case eApplicationEventType.END_RESTORE:
                    _applicationController.EndRestoreApplicationEvent -= action;
                    break;
            }
        }
        
        public void RemoveEvent(eApplicationEventType type, Action<bool> action)
        {
            if (!CheckApplicationController()) return;
            
            switch (type)
            {
                case eApplicationEventType.CHANGE_SCREEN_MODE:
                    _applicationController.ChangeScreenModeEvent -= action;
                    break;
            }
        }
#endregion // APPLICATION

#region PROCESS
        public void InitializeProcessController() => _processController?.Initialize();
        public void OnUpdateProcessController() => _processController?.OnUpdate();

        public void AddEvent(eReceiveMessage type, Action<string, string> action)
        {
            if (!CheckProcessController()) return;

            switch (type)
            {
                case eReceiveMessage.NORMAL:
                case eReceiveMessage.ALL:
                    _processController.ReceiveMessageEvent += action;
                    break;
                case eReceiveMessage.QUICK_CONNECT:
                    _processController.ReceiveQuickConnectEvent += action;
                    break;
            }
        }

        public void RemoveEvent(eReceiveMessage type, Action<string, string> action)
        {
            if (!CheckProcessController()) return;

            switch (type)
            {
                case eReceiveMessage.NORMAL:
                case eReceiveMessage.ALL:
                    _processController.ReceiveMessageEvent -= action;
                    break;
                case eReceiveMessage.QUICK_CONNECT:
                    _processController.ReceiveQuickConnectEvent -= action;
                    break;
            }
        }

        public bool IsDaemonProcessNotExist() => _processController?.IsDaemonNotFound ?? true;
#endregion // PROCESS
    }
}