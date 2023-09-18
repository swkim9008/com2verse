/*===============================================================
* Product:		Com2Verse
* File Name:	ResizeWindowViewModel.cs
* Developer:	mikeyid77
* Date:			2022-12-05 09:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.PlatformControl;
using Com2Verse.Extension;
using Com2Verse.Logger;
using DG.Tweening;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class ResizeWindowViewModel : ViewModel
	{
		private RectTransform _targetObject = null;
		private RectTransform _rootCanvasSize;
		private Vector2 _rootCanvasResolution;
		private float _targetAlpha = 1f;
		private bool _isWorkspace;
		private bool _isMinimized;
		private bool _isAlarmActivated = false;

		public CommandHandler EnterWorkSpace { get; private set; }
		public CommandHandler ExitWorkspace { get; private set; }
		public CommandHandler ChangeFullscreenMode { get; private set; }

		public ResizeWindowViewModel()
		{
			EnterWorkSpace = new CommandHandler(OnEnterWorkspace);
			ExitWorkspace = new CommandHandler(OnExitWorkspace);
			ChangeFullscreenMode = new CommandHandler(OnChangeFullscreenMode);
			//PlatformController.Instance.AddEvent(eApplicationEventType.END_ENTER, EndEnterWorkspaceAction);
			//Application.runInBackground = true;
		}

#region PROPERTY
		public RectTransform TargetObject
		{
			get => _targetObject;
			set
			{
				if (_targetObject.IsReferenceNull())
				{
					_targetObject = value;
					InvokePropertyValueChanged(nameof(TargetObject), TargetObject);
				}
			}
		}

		public RectTransform RootCanvasSize
		{
			get => _rootCanvasSize;
			set => _rootCanvasSize = value;
		}

		public Vector2 RootCanvasResolution
		{
			get => _rootCanvasResolution;
			set => _rootCanvasResolution = value;
		}

		public float TargetAlpha
		{
			get => _targetAlpha;
			set
			{
				_targetAlpha = value;
				InvokePropertyValueChanged(nameof(TargetAlpha), TargetAlpha);
			}
		}

		public bool IsWorkspace
		{
			get => _isWorkspace;
			set
			{
				_isWorkspace = value;
				InvokePropertyValueChanged(nameof(IsWorkspace), IsWorkspace);
			}
		}

		public bool IsMinimized
		{
			get => _isMinimized;
			set
			{
				_isMinimized = value;
				InvokePropertyValueChanged(nameof(IsMinimized), IsMinimized);
			}
		}

		public bool IsAlarmActivated
		{
			get => _isAlarmActivated;
			set
			{
				if (_isAlarmActivated != value)
				{
					_isAlarmActivated = value;
					InvokePropertyValueChanged(nameof(IsAlarmActivated), IsAlarmActivated);
				}
			}
		}
#endregion // PROPERTY

#region METHOD
		public void OnEnterWorkspace()
		{
			if (!IsWorkspace)
			{
				C2VDebug.LogCategory("ResizeWindow", $"Start Enter Workspace");

				IsWorkspace = true;
				TargetObject.anchoredPosition = Vector2.zero;
				TargetAlpha = 0f;
				PlatformController.Instance.EnterWorkspace(TargetObject, RootCanvasSize, true);
			}
		}

		public void OnExitWorkspace()
		{
			if (IsWorkspace)
			{
				C2VDebug.LogCategory("ResizeWindow", $"Start Exit Workspace");

				IsWorkspace = false;
				PlatformController.Instance.RestoreApplication();
			}
		}

		private void OnChangeFullscreenMode()
		{
			C2VDebug.LogCategory("ResizeWindow", $"Toggle Window Mode");

			PlatformController.Instance.ToggleScreenMode();
		}

		private void EndEnterWorkspaceAction()
		{
			DOTween.To(() => 0f, (value) => TargetAlpha = value, 1f, 0.2f);
		}
#endregion // METHOD

#region CHEAT
		private string _fps;
		private bool _miniModeCanvasOutline;

		public string FPS
		{
			get => _fps;
			set
			{
				_fps = value;
				InvokePropertyValueChanged(nameof(FPS), FPS);
			}
		}

		public bool MiniModeCanvasOutline
		{
			get => _miniModeCanvasOutline;
			set
			{
				_miniModeCanvasOutline = value;
				InvokePropertyValueChanged(nameof(MiniModeCanvasOutline), MiniModeCanvasOutline);
			}
		}
#endregion
	}
}
