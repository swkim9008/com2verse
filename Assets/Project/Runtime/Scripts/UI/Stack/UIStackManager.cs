/*===============================================================
* Product:		Com2Verse
* File Name:	UIStackManager.cs
* Developer:	mikeyid77
* Date:			2023-05-30 13:10
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using UnityEngine;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.InputSystem;
using Com2Verse.Loading;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Project.InputSystem;

namespace Com2Verse.UI
{
	public sealed class UIStackManager : Singleton<UIStackManager>, IDisposable
	{
		private class TargetInfo
		{
			public GameObject        GameObject;
			public string            Name;
			public string            AppId;
			public Action            CloseAction;
			public eInputSystemState TargetInputState;
			public bool              NeedEscControl;

			public void Invoke() => CloseAction?.Invoke();
		}

		private bool _isWaitingPopupActive = false;
		private readonly List<TargetInfo> _targetList = new();

		public void Initialize()
		{
			NetworkManager.Instance.OnDisconnected += Dispose;

			var actionMapCharacterControl = InputSystemManager.Instance.GetActionMap<ActionMapCharacterControl>();
			if (actionMapCharacterControl == null)
			{
				C2VDebug.LogErrorCategory("UIStackManager", $"Player ActionMap Not Found");
			}
			else
			{
				actionMapCharacterControl.EscapeAction += Invoke;
			}

			var actionMapUIControl = InputSystemManager.Instance.GetActionMap<ActionMapUIControl>();
			if (actionMapUIControl == null)
			{
				C2VDebug.LogErrorCategory("UIStackManager", $"UI ActionMap Not Found");
			}
			else
			{
				actionMapUIControl.EscapeAction += Invoke;
			}
		}

		public void Invoke()
		{
			if (LoadingManager.Instance.IsLoading) return;
			if (_isWaitingPopupActive) return;
			if (_targetList?.Count != 0)
			{
				if (_targetList?[0]?.NeedEscControl ?? false)
				{
					C2VDebug.LogCategory("UIStackManager", $"[{_targetList?.Count}] Invoke ({_targetList?[0]?.Name}) Action");
					_targetList?[0]?.Invoke();
				}
				else
				{
					C2VDebug.LogCategory("UIStackManager", $"[{_targetList?.Count}] ({_targetList?[0]?.Name}) Can't Close with ESC");
				}
			}
			else
			{
				if ( _targetList.Count <= 0)
				{
					C2VDebug.LogCategory("UIStackManager", $"[0] Empty List");
					SceneManager.Instance.CurrentScene.OnEscapeAction();
				}
			}
		}

		public void Dispose()
		{
			_targetList?.Clear();
			_isWaitingPopupActive = false;
		}

		public string TopMostViewName => Count > 0 ? _targetList?[0]!.Name : string.Empty;
		public int    Count           => _targetList?.Count ?? 0;

#region Add
		public void AddByObject(GameObject gameObject, string name, Action action, eInputSystemState inputState, bool needEscControl)
		{
			if (gameObject.IsUnityNull()) return;
			if (string.IsNullOrEmpty(name)) return;
			if (action == null) return;
			if (CheckContentsByObject(gameObject)) return;

			C2VDebug.LogCategory("UIStackManager", $"Add ({name})");
			_targetList?.Insert(0, new TargetInfo
			{
				GameObject       = gameObject,
				Name             = name,
				AppId            = "",
				CloseAction      = action,
				TargetInputState = inputState,
				NeedEscControl   = needEscControl
			});
			SetInputActionMap();
		}

		public void AddByName(string name, Action action, eInputSystemState inputState, bool needEscControl)
		{
			if (string.IsNullOrEmpty(name)) return;
			if (action == null) return;
			if (CheckContentsByName(name)) return;

			C2VDebug.LogCategory("UIStackManager", $"Add ({name})");
			_targetList?.Insert(0, new TargetInfo
			{
				GameObject       = null,
				Name             = name,
				AppId            = "",
				CloseAction      = action,
				TargetInputState = inputState,
				NeedEscControl   = needEscControl
			});
			SetInputActionMap();
		}
#endregion // Add

#region Set
		public void SetCustomInfoByObject(GameObject gameObject, string appId, bool isActivation)
		{
			if (gameObject.IsUnityNull()) return;
			if (!CheckContentsByObject(gameObject)) return;

			var target = GetContentsByObject(gameObject);
			if (target != null)
			{
				C2VDebug.LogCategory("UIStackManager", $"Refresh ({target.Name})");
				target.AppId            = appId;
				target.TargetInputState = (isActivation) ? eInputSystemState.CHARACTER_CONTROL : eInputSystemState.UI;
				SetInputActionMap();
			}
			else
			{
				C2VDebug.LogWarningCategory("UIStackManager", $"No Target in List");
			}
		}

		public void SetTopmostByObject(GameObject gameObject)
		{
			if (gameObject.IsUnityNull()) return;
			if (!CheckContentsByObject(gameObject)) return;

			var target = GetContentsByObject(gameObject);
			if (target != null)
			{
				C2VDebug.LogCategory("UIStackManager", $"Set Topmost ({target.Name})");
				_targetList.Remove(target);
				_targetList.Insert(0, target);
				SetInputActionMap();
			}
			else
			{
				C2VDebug.LogWarningCategory("UIStackManager", $"No Target in List");
			}
		}
#endregion // Set

#region Remove
		public void RemoveByObject(GameObject gameObject)
		{
			if (gameObject.IsUnityNull()) return;
			if (!CheckContentsByObject(gameObject)) return;

			var target = GetContentsByObject(gameObject);
			if (target != null)
			{
				C2VDebug.LogCategory("UIStackManager", $"Remove ({target.Name})");
				_targetList?.Remove(target);
				SetInputActionMap();
			}
		}

		public void RemoveByName(string name)
		{
			if (string.IsNullOrEmpty(name)) return;
			if (!CheckContentsByName(name)) return;

			var target = GetContentsByName(name);
			if (target != null)
			{
				C2VDebug.LogCategory("UIStackManager", $"Remove ({target.Name})");
				_targetList?.Remove(target);
				SetInputActionMap();
			}
		}

		public void RemoveAll()
		{
			C2VDebug.LogCategory("UIStackManager", $"Remove All");
			for (int i = 0; i < _targetList?.Count;)
				_targetList[i]?.Invoke();
			SetInputActionMap();
		}
#endregion // Remove

#region Utils
		private bool       CheckContentsByObject(GameObject gameObject) => _targetList?.Find(info => info?.GameObject == gameObject) != null;
		private bool       CheckContentsByName(string name)             => _targetList?.Find(info => info?.Name       == name)       != null;
		private TargetInfo GetContentsByObject(GameObject gameObject)   => _targetList?.Find(info => info?.GameObject == gameObject);
		private TargetInfo GetContentsByName(string name)               => _targetList?.Find(info => info?.Name       == name);

		public void ResetStack()
		{
			C2VDebug.LogCategory("UIStackManager", $"Reset Stack");
			_targetList?.Clear();
		}

		public void SetWaitingPopup(bool isActive)
		{
			_isWaitingPopupActive = isActive;
			SetInputActionMap();
		}

		private void SetInputActionMap()
		{
			if (SceneManager.InstanceOrNull?.CurrentScene is not SceneSpace) return;
			if (_isWaitingPopupActive)
			{
				InputSystemManagerHelper.ChangeState(eInputSystemState.WAITING_BLOCK);
			}
			else
			{
				if (_targetList?.Count == 0)
					InputSystemManagerHelper.ChangeState(eInputSystemState.CHARACTER_CONTROL);
				else
					InputSystemManagerHelper.ChangeState(_targetList?[0]?.TargetInputState ?? eInputSystemState.CHARACTER_CONTROL);
			}
		}

		private UIStackManager() { }
#endregion // Utils
	}
}
