/*===============================================================
* Product:    Com2Verse
* File Name:  ActionMap.cs
* Developer:  mikeyid77
* Date:       2022-02-25 09:23
* History:    2022-02-25 - Init
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Logger;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Com2Verse.InputSystem
{
	public sealed class ActionMapFactory
	{
#region Fields
		private readonly Dictionary<Type, ActionMap> _factory = new();
#endregion Fields

#region Methods
		public T GetActionMap<T>() where T : ActionMap, new()
		{
			if (_factory.ContainsKey(typeof(T)))
				return _factory[typeof(T)] as T;

			C2VDebug.Log($"create {typeof(T).Name}");
			var ret = new T();
			_factory.Add(typeof(T), ret);
			return ret;
		}

		public void ClearActionMaps()
		{
			foreach (var keyValuePair in _factory)
			{
				keyValuePair.Value.ClearActions();
			}
		}
#endregion Methods
	}


	public abstract class ActionMap
	{
		private enum eMouseEventType
		{
			PRESSED = 0,
			DRAG,
			CLICK,
			RELEASE
		}

#region Fields
		private eMouseEventType _mouseLeftEventType  = eMouseEventType.RELEASE;
		private eMouseEventType _mouseRightEventType = eMouseEventType.RELEASE;

		protected Action MouseLeftButtonPressed;
		protected Action MouseLeftButtonDrag;
		protected Action MouseLeftButtonClick;
		protected Action MouseLeftButtonRelease;
		protected Action MouseRightButtonPressed;
		protected Action MouseRightButtonDrag;

		protected Action MouseRightButtonClick;
		protected Action MouseRightButtonRelease;

		private static readonly float DragThreshold = 5f;

		private Vector2 _clickLeftStartPosition;
		private Vector2 _clickRightStartPosition;

		protected readonly List<Action<InputAction.CallbackContext>> _actionList   = new();
		protected readonly List<string>                              _targetAction = new();
		protected          string                                    _name;
#endregion Fields

#region Properties
		public List<Action<InputAction.CallbackContext>> ActionList   => _actionList;
		public List<string>                              TargetAction => _targetAction;
		public string                                    Name         => _name;
#endregion Properties


		public virtual void OnUpdate()
		{
			UpdateMouseEventType(true, Mouse.current.leftButton.wasPressedThisFrame, Mouse.current.leftButton.isPressed);
			UpdateMouseEventType(false, Mouse.current.rightButton.wasPressedThisFrame, Mouse.current.rightButton.isPressed);
		}

		private void UpdateMouseEventType(bool isLeft, bool wasPressed, bool isPressed)
		{
			switch (isLeft ? _mouseLeftEventType : _mouseRightEventType)
			{
				case eMouseEventType.RELEASE:
					if (wasPressed)
					{
						if (isLeft)
						{
							// TODO: Input System 자체 기능으로 처리 가능한지 조사
							if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
								return;
							_clickLeftStartPosition = Mouse.current.position.ReadValue();
							_mouseLeftEventType     = eMouseEventType.PRESSED;
							MouseLeftButtonPressed?.Invoke();
						}
						else
						{
							_clickRightStartPosition = Mouse.current.position.ReadValue();
							_mouseRightEventType     = eMouseEventType.PRESSED;
							MouseRightButtonPressed?.Invoke();
						}
					}

					break;
				case eMouseEventType.PRESSED:
					if (isPressed)
					{
						if (isLeft)
						{
							if (Vector2.Distance(_clickLeftStartPosition, Mouse.current.position.ReadValue()) >= DragThreshold)
							{
								_mouseLeftEventType = eMouseEventType.DRAG;
								MouseLeftButtonDrag?.Invoke();
							}
						}
						else
						{
							if (Vector2.Distance(_clickRightStartPosition, Mouse.current.position.ReadValue()) >= DragThreshold)
							{
								_mouseRightEventType = eMouseEventType.DRAG;
								MouseRightButtonDrag?.Invoke();
							}
						}
					}
					else
					{
						if (isLeft)
						{
							_mouseLeftEventType = eMouseEventType.CLICK;
							MouseLeftButtonClick?.Invoke();
						}
						else
						{
							_mouseRightEventType = eMouseEventType.CLICK;
							MouseRightButtonClick?.Invoke();
						}
					}

					break;
				case eMouseEventType.DRAG:
					if (isPressed)
					{
						if (isLeft)
							MouseLeftButtonDrag?.Invoke();
						else
							MouseRightButtonDrag?.Invoke();
					}
					else
					{
						if (isLeft)
						{
							_mouseLeftEventType = eMouseEventType.RELEASE;
							MouseLeftButtonRelease?.Invoke();
						}
						else
						{
							_mouseRightEventType = eMouseEventType.RELEASE;
							MouseRightButtonRelease?.Invoke();
						}
					}

					break;
				case eMouseEventType.CLICK:
					if (isLeft)
					{
						_mouseLeftEventType = eMouseEventType.RELEASE;
						MouseLeftButtonRelease?.Invoke();
					}
					else
					{
						_mouseRightEventType = eMouseEventType.RELEASE;
						MouseRightButtonRelease?.Invoke();
					}

					break;
			}
		}

		public virtual  void UpdateOnSyncTime() { }
		public abstract void ClearActions();
	}
}
