/*===============================================================
* Product:		Com2Verse
* File Name:	StubActionMapUI.cs
* Developer:	tlghks1009
* Date:			2022-05-25 10:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

// #define SONG_TEST

using System;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.UI;
using UnityEngine;
using UnityEngine.InputSystem;

#if SONG_TEST
using Com2Verse.AssetSystem; 
#endif // SONG_TEST

namespace Com2Verse.Project.InputSystem
{
	public sealed class ActionMapUIControl : ActionMap
	{
		public ActionMapUIControl()
		{
			_name = "UI";

			_actionList.Add(OnConfirmStarted);
			_actionList.Add(OnConfirmPerformed);
			_actionList.Add(OnConfirmCanceled);

			// Mouse Wheel
			_actionList.Add(OnZoomStarted);
			_actionList.Add(OnZoomPerformed);
			_actionList.Add(OnZoomCanceled);

			// Drag - Type: Value, Vector2
			_actionList.Add(OnLeftDragStarted);
			_actionList.Add(OnLeftDragPerformed);
			_actionList.Add(OnLeftDragCanceled);

			// Drag - Type: Value, Vector2
			_actionList.Add(OnRightDragStarted);
			_actionList.Add(OnRightDragPerformed);
			_actionList.Add(OnRightDragCanceled);

			// Minimap - Type: Button
			_actionList.Add(OnMinimapUIStarted);
			_actionList.Add(OnMinimapUIPerformed);
			_actionList.Add(OnMinimapUICanceled);
			
			// Escape - Type: Button
			_actionList.Add(OnEscapeUIStarted);
			_actionList.Add(OnEscapeUIPerformed);
			_actionList.Add(OnEscapeUICanceled);
		}

		public override void ClearActions()
		{
			Confirm         = null;
			ZoomAction      = null;
			LeftDragAction  = null;
			RightDragAction = null;
			MinimapAction   = null;
			EscapeAction    = null;
		}

#region InputActions
		public Action          Confirm         { get; set; }
		public Action<float>   ZoomAction      { get; set; }
		public Action<Vector2> LeftDragAction  { get; set; }
		public Action<Vector2> RightDragAction { get; set; }
		public Action          MinimapAction   { get; set; }
		public Action          EscapeAction    { get; set; }
#endregion InputActions

#region Confirm
		private void OnConfirmStarted(InputAction.CallbackContext callback)
		{
#if SONG_TEST
			AssetSystemManager.Instance.TestAll();
#endif // SONG_TEST
		}

		private void OnConfirmPerformed(InputAction.CallbackContext callback)
		{
			Confirm?.Invoke();
		}

		private void OnConfirmCanceled(InputAction.CallbackContext callback) { }
#endregion Confirm

		private void OnZoomStarted(InputAction.CallbackContext callback) { }

		private void OnZoomPerformed(InputAction.CallbackContext callback)
		{
			ZoomAction?.Invoke(callback.action.ReadValue<float>());
		}

		private void OnZoomCanceled(InputAction.CallbackContext callback) { }

#region DRAG
		private void OnLeftDragStarted(InputAction.CallbackContext callback) { }

		private void OnLeftDragPerformed(InputAction.CallbackContext callback)
		{
			LeftDragAction?.Invoke(callback.action.ReadValue<Vector2>());
		}

		private void OnLeftDragCanceled(InputAction.CallbackContext callback)
		{
			LeftDragAction?.Invoke(Vector2.zero);
		}

		private void OnRightDragStarted(InputAction.CallbackContext callback) { }

		private void OnRightDragPerformed(InputAction.CallbackContext callback)
		{
			RightDragAction?.Invoke(callback.action.ReadValue<Vector2>());
		}

		private void OnRightDragCanceled(InputAction.CallbackContext callback)
		{
			RightDragAction?.Invoke(Vector2.zero);
		}
#endregion DRAG

#region MINIMAP
		private void OnMinimapUIStarted(InputAction.CallbackContext callback)
		{
			if (_canClick && InputSystemManager.CanClick())
			{
				StartTimerWhenKeyboardPress();
				MinimapAction?.Invoke();
			}
		}

		private void OnMinimapUIPerformed(InputAction.CallbackContext callback) { }

		private void OnMinimapUICanceled(InputAction.CallbackContext callback) { }
#endregion MINIMAP

#region MINIMAP
		private void OnEscapeUIStarted(InputAction.CallbackContext callback)
		{
			if (_canClick && InputSystemManager.CanClick())
			{
				StartTimerWhenKeyboardPress();
				EscapeAction?.Invoke();
			}
		}

		private void OnEscapeUIPerformed(InputAction.CallbackContext callback) { }

		private void OnEscapeUICanceled(InputAction.CallbackContext callback) { }
#endregion MINIMAP

		private bool _canClick = true;

		private void StartTimerWhenKeyboardPress()
		{
			if (InputSystemManager.BlockInterval > 0)
			{
				_canClick = false;
				InputSystemManager.RequestKeyboardBlock();
				UIManager.Instance.StartTimer(InputSystemManager.BlockInterval,
				                              () =>
				                              {
					                              _canClick = true;
					                              InputSystemManager.RequestKeyboardUnblock();
				                              });
			}
		}
	}
}
