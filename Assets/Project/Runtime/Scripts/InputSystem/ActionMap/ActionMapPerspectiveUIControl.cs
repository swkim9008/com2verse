/*===============================================================
* Product:		Com2Verse
* File Name:	ActionMapPerspectiveUIControl.cs
* Developer:	mikeyid77
* Date:			2023-06-16 18:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

// #define SONG_TEST

using System;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using UnityEngine;
using UnityEngine.InputSystem;

#if SONG_TEST
using Com2Verse.AssetSystem; 
#endif // SONG_TEST

namespace Com2Verse.Project.InputSystem
{
	public sealed class ActionMapPerspectiveUIControl : ActionMap
	{
		public ActionMapPerspectiveUIControl()
		{
			_name = "PerspectiveUI";

			_actionList.Add(OnConfirmStarted);
			_actionList.Add(OnConfirmPerformed);
			_actionList.Add(OnConfirmCanceled);

			// Mouse Wheel
			_actionList.Add(OnZoomStarted);
			_actionList.Add(OnZoomPerformed);
			_actionList.Add(OnZoomCanceled);

			// Drag - Type: Value, Vector2
			_actionList.Add(OnRightDragStarted);
			_actionList.Add(OnRightDragPerformed);
			_actionList.Add(OnRightDragCanceled);
		}

		public override void ClearActions()
		{
			Confirm         = null;
			ZoomAction      = null;
			RightDragAction = null;
		}

#region InputActions
		public Action          Confirm         { get; set; }
		public Action<float>   ZoomAction      { get; set; }
		public Action<Vector2> RightDragAction { get; set; }
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
	}
}
