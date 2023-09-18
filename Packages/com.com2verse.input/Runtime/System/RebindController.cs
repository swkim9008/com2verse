/*===============================================================
* Product:    Com2Verse
* File Name:  RebindController.cs
* Developer:  mikeyid77
* Date:       2022-02-25 16:32
* History:    2022-02-25 - Init
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Logger;
using UnityEngine.InputSystem;

namespace Com2Verse.InputSystem
{
	public sealed class RebindController : MonoSingleton<RebindController>
	{
#region Fields
		private InputActionMap _inputActionMap;
		private Device         _device;
		private Action<bool>   _changeState;
		private Action<string, string, int>    _checkDuplicate;

		private          InputActionRebindingExtensions.RebindingOperation _rebindingOperation;
		private readonly List<string>                                      _currentModifier = new();
		private          InputAction                                       _targetAction;
		private          InputBinding                                      _targetParent;
		private          string                                            _targetPath;
		private          int                                               _targetIndex;
		
		public event Action<string, string, int> CheckDuplicate
		{
			add
			{
				_checkDuplicate -= value;
				_checkDuplicate += value;
			}
			remove => _checkDuplicate -= value;
		}
#endregion Fields

#region Rebinding
		public void SetRebinding(int index, InputActionMap inputActionMap, Device device, Action<bool> changeState)
		{
			_inputActionMap = inputActionMap;
			_device         = device;
			_changeState    = changeState;
			
			var binding = inputActionMap.bindings[index];
			_targetAction = inputActionMap[binding.action];

			if (binding.isPartOfComposite)
			{
				var target = binding.action;
				var bindings = inputActionMap.bindings;
				for (int i = 0; i < bindings.Count; i++)
				{
					if (bindings[i].isComposite && bindings[i].action == target)
					{
						_targetParent = bindings[i];
						_targetIndex = index - i;
						break;
					}
				}
			}
			else
			{
				_targetParent = binding;
				_targetIndex = 0;
			}
			
			StartRebinding(0);
		}
		
		private void StartRebinding(int count)
		{
			_rebindingOperation = _targetAction.PerformInteractiveRebinding(_targetIndex)
			                                   .WithControlsExcluding(_device.ExcludingControl)
			                                   .WithCancelingThrough(_device.EscapeBinding)
			                                   .OnCancel(rebind => FailedRebinding())
			                                   .OnComplete(rebind => CheckRebinding(count))
			                                   .Start();
		}

		private void CheckRebinding(int count)
		{
			_targetPath = _targetAction.bindings[_targetIndex].effectivePath;

			if (!_device.TargetModifier.Contains(_targetPath))
			{
				CheckOverlapped();
			}
			else
			{
				if (_targetParent.path.Contains("2DVector"))
					FailedRebinding();
				else
				{
					if (count < 2)
					{
						_currentModifier.Add(_targetPath);
						_rebindingOperation.Dispose();
						StartRebinding(++count);
					}
					else
						FailedRebinding();
				}
			}
		}

		private void CheckOverlapped()
		{
			var result = true;
			var bindings = _inputActionMap.bindings;
			for (int index = 0; index < bindings.Count; index++)
			{
				if (bindings[index].isComposite) continue;
				if (bindings[index].groups != _device.CurrentScheme) continue;
				if (bindings[index].id == _targetAction.bindings[_targetIndex].id) continue;

				if (bindings[index].name != null && bindings[index].name.Contains("modifier", StringComparison.OrdinalIgnoreCase))
				{
					_device.FindModifier(bindings[index].effectivePath);
					continue;
				}

				if (bindings[index].effectivePath == _targetPath && _device.CheckModifier(_currentModifier))
				{
					result = false;
					var name = (bindings[index].isPartOfComposite) ? bindings[index].name.ToLower() : bindings[index].action.ToLower();
					if (!(RebindActionHelper.RebindActionDict.ContainsKey(name) && !RebindActionHelper.RebindActionDict[name].CanRebind))
						_checkDuplicate?.Invoke(_device.PrintModifier(bindings[index].effectivePath).ToLower(), name, index);
					break;
				}

				_device.ResetModifier();
			}

			if (result) CompleteRebinding();
		}

		public void ApplyDuplicateBind(int index)
		{
			_inputActionMap.ApplyBindingOverride(index, new InputBinding{overridePath = ""});
			CompleteRebinding();
		}

		private void CompleteRebinding()
		{
			C2VDebug.Log("Complete Rebind");

			if (_currentModifier.Count == 0)
			{
				if (_targetParent.path?.Contains("2DVector") ?? false)
				{
					int          vectorIndex   = _targetAction.ChangeBindingWithId(_targetParent.id).bindingIndex;
					InputBinding vectorBinding = _inputActionMap.bindings[vectorIndex];
					int          vectorTarget  = _targetAction.GetBindingIndex(vectorBinding);

					// TODO : 동적으로 찾아서 바꿀수 있을까?
					string[] bindingPath = new string[8];
					for (int i = 0; i < 8; i++)
					{
						bindingPath[i] = _targetAction.bindings[vectorTarget + i + 1].effectivePath;
					}

					_targetAction.ChangeBinding(_targetParent).Erase();

					int index =
						_targetAction.AddCompositeBinding("2DVector(mode=1)")
						             .With("up", bindingPath[0], _device.CurrentScheme)
						             .With("down", bindingPath[1], _device.CurrentScheme)
						             .With("left", bindingPath[2], _device.CurrentScheme)
						             .With("right", bindingPath[3], _device.CurrentScheme)
						             .With("up", bindingPath[4], _device.CurrentScheme)
						             .With("right", bindingPath[5], _device.CurrentScheme)
						             .With("left", bindingPath[6], _device.CurrentScheme)
						             .With("down", bindingPath[7], _device.CurrentScheme)
						             .bindingIndex;

					_targetAction.ChangeBinding(index).WithGroups(_device.CurrentScheme);
				}
				else
				{
					_targetAction.ChangeBinding(_targetParent).Erase();

					_targetAction.AddBinding()
					             .WithPath(_targetPath)
					             .WithGroups(_device.CurrentScheme);
				}
			}
			else if (_currentModifier.Count == 1)
			{
				_targetAction.ChangeBinding(_targetParent).Erase();

				int index =
					_targetAction.AddCompositeBinding("OneModifier")
					             .With("modifier", _currentModifier[0], _device.CurrentScheme)
					             .With("binding", _targetPath, _device.CurrentScheme)
					             .bindingIndex;

				_targetAction.ChangeBinding(index).WithGroups(_device.CurrentScheme);
			}
			else
			{
				_targetAction.ChangeBinding(_targetParent).Erase();

				int index =
					_targetAction.AddCompositeBinding("TwoModifiers")
					             .With("modifier1", _currentModifier[0], _device.CurrentScheme)
					             .With("modifier2", _currentModifier[1], _device.CurrentScheme)
					             .With("binding", _targetPath, _device.CurrentScheme)
					             .bindingIndex;

				_targetAction.ChangeBinding(index).WithGroups(_device.CurrentScheme);
			}

			EndRebinding(true);
		}

		public void FailedRebinding()
		{
			C2VDebug.LogWarning("Failed Rebind");

			_targetAction.RemoveBindingOverride(_targetIndex);
			EndRebinding(false);
		}

		private void EndRebinding(bool result)
		{
			_currentModifier.Clear();
			_rebindingOperation.Dispose();
			_changeState?.Invoke(result);
		}
#endregion Rebinding
	}
}
