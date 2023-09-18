/*===============================================================
* Product:    Com2Verse
* File Name:  InputSystemManager.cs
* Developer:  mikeyid77
* Date:       2022-03-08 16:34
* History:    
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Com2Verse.Logger;
using Com2Verse.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Com2Verse.InputSystem
{
	public enum eSAVE
	{
		AUTO,
		CUSTOM,
	}

	public sealed class InputSystemManager : Singleton<InputSystemManager>, IDisposable
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private InputSystemManager() { }

#region Fields
		private ActionMapStateMachine _stateMachine;
		private ActionMapFactory      _actionMapFactory;
		private DeviceFactory         _deviceFactory;
		private ActionMap             _actionMap, _previousActionMap;
		private Device                _device;

		private IInputActionCollection2 _inputActionCollection;
		private InputActionAsset        _inputActionAsset;
		private string                  _inputActionInit;

		private InputActionMap     _playerActionMap;
		private InputControlScheme _playerControlScheme;

		// TODO: dev 머지 후 AUTO로 타입 변경
		private eSAVE _saveType = eSAVE.AUTO;
		private Action _refreshBindingUiAction = null;

		private float       _syncTimer     = 0;
		private bool        _isInitialized = false;
		private IDisposable _anyButtonPressSubscription;
#endregion Fields

#region Properites
		public LayerMask GroundMask { get; set; }
		public LayerMask ObjectMask { get; set; }
		public string    SaveName   { get; set; } = "/ActionMap.json";

		private float SyncInterval { get; set; } = 0.05f;

		public bool AnalogMovement { get; set; }

		public ActionMapStateMachine StateMachine    => _stateMachine;
		public InputActionMap        TargetActionMap => _inputActionAsset.FindActionMap("Player");
		public InputActionMap        PlayerActionMap => _playerActionMap;
		public ActionMap             ActionMap       => _actionMap;
		public Device                Device          => _device;
		public int                   Index;

		public event Action<string, string, int> CheckDuplicateBindEvent
		{
			add => RebindController.Instance.CheckDuplicate += value;
			remove => RebindController.Instance.CheckDuplicate -= value;
		}
		public event Action RefreshBindingUiEvent
		{
			add
			{
				_refreshBindingUiAction -= value;
				_refreshBindingUiAction += value;
			}
			remove => _refreshBindingUiAction -= value;
		}
#endregion Properites

		public void Initialize(
			IInputActionCollection2 inputActionCollection,
			InputActionAsset inputActionAsset,
			LayerMask groundMask = default,
			LayerMask objectMask = default,
			string saveName = "/ActionMap.json")
		{
			if (_isInitialized) return;

			_inputActionCollection = inputActionCollection;
			_inputActionAsset      = inputActionAsset;
			_inputActionInit       = _inputActionAsset.ToJson();

			ResetAllButtonBlock();
			GroundMask = groundMask;
			ObjectMask = objectMask;

			_stateMachine     = new ActionMapStateMachine();
			_actionMapFactory = new ActionMapFactory();
			_deviceFactory    = new DeviceFactory();
			_device           = _deviceFactory.SetDevice<DeviceKeyboard>();

			SaveName = saveName;

			_stateMachine.Init(_saveType == eSAVE.AUTO ? eSTATE.LOAD : eSTATE.SET);

			_anyButtonPressSubscription = UnityEngine.InputSystem.InputSystem.onAnyButtonPress.Call(OnAnyButtonPressEvent);

			_isInitialized = true;
		}

		public void OnUpdate()
		{
			if (_actionMap == null) return;

			_actionMap.OnUpdate();

			_syncTimer += Time.deltaTime;
			if (_syncTimer >= SyncInterval)
			{
				_syncTimer = 0;
				_actionMap.UpdateOnSyncTime();
			}
		}

#region InputCheck
		/// <summary>
		/// 사용자의 Device(마우스, 키보드 등)에서 Press Event가 발생시 호출
		/// </summary>
		public event Action<InputControl> AnyButtonPressEvent;
		public event Action<InputControl> MousePressEvent;
		public event Action<InputControl> KeyboardPressEvent;

		public const   float BlockInterval         = 0.4f;
		private static bool  _mouseBlockRequest    = false;
		private static bool  _keyboardBlockRequest = false;
		private static bool  AnyButtonBlockRequest => _mouseBlockRequest || _keyboardBlockRequest;

		public static void RequestMouseBlock()      => _mouseBlockRequest = true;
		public static void RequestKeyboardBlock()   => _keyboardBlockRequest = true;
		public static void RequestMouseUnblock()    => _mouseBlockRequest = false;
		public static void RequestKeyboardUnblock() => _keyboardBlockRequest = false;
		public static void ResetAllButtonBlock()    => _mouseBlockRequest = _keyboardBlockRequest = false;
		public static bool CanClick()               => !AnyButtonBlockRequest;

		private void OnAnyButtonPressEvent(InputControl inputControl)
		{
			if (inputControl == null) return;

			AnyButtonPressEvent?.Invoke(inputControl);
			switch (inputControl.device)
			{
				case Mouse:
					MousePressEvent?.Invoke(inputControl);
					break;
				case Keyboard:
					KeyboardPressEvent?.Invoke(inputControl);
					break;
				default:
					break;
			}
		}
#endregion // InputCheck

#region ActionMapSetting
		public void DisableActionMap()
		{
			_inputActionCollection.Disable();
		}

		public T GetActionMap<T>() where T : ActionMap, new()
		{
			return _actionMapFactory?.GetActionMap<T>();
		}

		public void SetSaveMode(eSAVE saveMode)
		{
			_saveType = saveMode;
		}

		public void ChangeActionMap<T>() where T : ActionMap, new()
		{
			if (!_isInitialized) return;

			var currentActionMap = _actionMapFactory.GetActionMap<T>();

			if (_actionMap != null && _actionMap != currentActionMap)
				_previousActionMap = _actionMap;

			_actionMap = currentActionMap;
			StateMachine.ChangeState(eSTATE.SET);
		}

		public void ChangePreviousActionMap()
		{
			if (_actionMap == null || _previousActionMap == null) return;
			_actionMap = _previousActionMap;
			StateMachine.ChangePreviousState();
		}

		public void SetActionMap(Action changeState = null)
		{
			if (_actionMap == null) return;
			_inputActionCollection.Disable();
			_playerActionMap = _inputActionAsset.FindActionMap(_actionMap.Name);
			_playerActionMap.Enable();

			int index = 0;
			foreach (InputAction action in _playerActionMap)
			{
				if (index == _actionMap.ActionList.Count) break;

				if (action.type == InputActionType.PassThrough)
				{
					action.performed += _actionMap.ActionList[index++];
				}
				else
				{
					action.started   += _actionMap.ActionList[index++];
					action.performed += _actionMap.ActionList[index++];
					action.canceled  += _actionMap.ActionList[index++];
				}
			}

			changeState?.Invoke();
		}

		public void SaveActionMap(Action changeState)
		{
			C2VDebug.LogCategory("InputSystemManager", $"{nameof(SaveActionMap)}");
			var action = _inputActionAsset.ToJson();
			using (StreamWriter output = new StreamWriter(Path.Combine(Application.persistentDataPath, SaveName)))
			{
				foreach (char line in action)
				{
					output.Write(line);
				}
			}

			changeState?.Invoke();
		}

		public void LoadActionMap(Action changeState)
		{
			C2VDebug.LogCategory("InputSystemManager", $"{nameof(LoadActionMap)}");
			try
			{
				_inputActionCollection.Disable();
				var action = File.ReadAllText(Path.Combine(Application.persistentDataPath, SaveName));

				var dummyActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
				dummyActionAsset.LoadFromJson(action);

				var checkActionMapChanged = CheckActionMapChanged(dummyActionAsset);
				if (checkActionMapChanged)
				{
					C2VDebug.LogWarningCategory("InputSystemManager", $"ActionMap Changed in Editor");
				}
				else
				{
					_inputActionAsset.LoadFromJson(action);
				}
				changeState();
			}
			catch (FileNotFoundException)
			{
				// 지정된 경로에 파일이 없는 경우
				C2VDebug.LogWarningCategory("InputSystemManager", $"Can't Find Save ActionMap File");
			}
			catch (Exception e)
			{
				C2VDebug.LogErrorCategory("InputSystemManager", $"{e.Message}\n{e.StackTrace}");
			}
		}

		private bool CheckActionMapChanged(InputActionAsset target)
		{
			if (_inputActionAsset == null || target == null) return false;

			// 초기 값과 저장된 값의 ActionMap 갯수가 다를 경우
			if (_inputActionAsset.actionMaps.Count != target.actionMaps.Count) return true;

			// 각각의 ActionMap 안의 Action의 수가 다를 경우
			for (int i = 0; i < _inputActionAsset.actionMaps.Count; i++)
			{
				if (_inputActionAsset.actionMaps[i].actions.Count != target.actionMaps[i].actions.Count) 
					return true;
			}

			return false;
		}

		public void ResetActionMap(Action changeState)
		{
			C2VDebug.LogCategory("InputSystemManager", $"{nameof(ResetActionMap)}");
			_inputActionCollection.Disable();
			_inputActionAsset.LoadFromJson(_inputActionInit);
			changeState?.Invoke();
		}
#endregion ActionMapSetting

#region BindCheck
		public void ApplyDuplicateBinding(int index)
		{
			RebindController.Instance.ApplyDuplicateBind(index);
		}

		public void CancelBinding()
		{
			RebindController.Instance.FailedRebinding();
		}

		public void RefreshBindingUi()
		{
			_refreshBindingUiAction?.Invoke();
		}
#endregion BindCheck

#region ViewTarget
		public class ViewTarget
		{
			public int Index;
			public string ActionName;
			public string BindName;
			public string BindPath;
			public bool CanRebind;
		}

		public List<ViewTarget> GetTargets()
		{
			List<ViewTarget> resultList = new();
			var bindings = TargetActionMap.bindings.ToList();
			var targetList = RebindActionHelper.RebindActionDict;
			foreach (var target in targetList)
			{
				Device.ResetModifier();
				var targetName = target.Key;
				var targetInfo = target.Value;
				for (int index = 0; index < bindings.Count; index++)
				{
					if (bindings[index].isComposite) continue;
					if (bindings[index].groups != Device.CurrentScheme) continue;
					var name = (bindings[index].isPartOfComposite) ? bindings[index].name.ToLower() : bindings[index].action.ToLower();
					if (name.Contains("modifier"))
					{
						Device.FindModifier(bindings[index].effectivePath);
						continue;
					}
					if (name.Contains("binding") && bindings[index].action.ToLower() != targetName)
					{
						Device.ResetModifier();
						continue;
					}
							
					if (name == targetName || (name.Contains("binding") && bindings[index].action.ToLower() == targetName))
					{
						resultList.Add(new ViewTarget()
						{
							Index = index,
							ActionName = targetName,
							BindName = targetInfo.Name,
							BindPath 
								= (string.IsNullOrEmpty(bindings[index].effectivePath))
									? string.Empty 
									: Device.PrintModifier(bindings[index].effectivePath).ToLower(),
							CanRebind = targetInfo.CanRebind
						});
						Device.ResetModifier();
						break;
					}
				}
			}
			return resultList;
		}
#endregion

#region Dispose
		public void Dispose()
		{
			if (_saveType == eSAVE.AUTO)
				_stateMachine?.ChangeState(eSTATE.SAVE);

			_isInitialized = false;
			_actionMapFactory?.ClearActionMaps();

			_anyButtonPressSubscription?.Dispose();
			_anyButtonPressSubscription = null;
			_mouseBlockRequest          = false;
			_keyboardBlockRequest       = false;
		}
#endregion Dispose

#region Constant
		public static class Constant
		{
			public const string ArrowUp    = "upArrow";
			public const string ArrowDown  = "downArrow";
			public const string ArrowLeft  = "leftArrow";
			public const string ArrowRight = "rightArrow";
		}
#endregion // Constant

#region EventSystem
		public T SetOverrideInput<T>() where T : BaseInput
		{
			var currentEventSystem = EventSystem.current;
			if (currentEventSystem == null || currentEventSystem.currentInputModule == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "EventSystem or inputModule is Null");
				return null;
			}

			var overrideInput = Util.GetOrAddComponent<T>(currentEventSystem.gameObject);
			currentEventSystem.currentInputModule.inputOverride = overrideInput;
			return overrideInput;
		}
#endregion EventSystem
	}
}
