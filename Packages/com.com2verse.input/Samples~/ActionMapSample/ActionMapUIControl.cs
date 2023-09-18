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

using UnityEngine.InputSystem;
using System;
using Com2Verse.InputSystem; 

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
		}

		public override void ClearActions()
		{
			Confirm    = null;
			ZoomAction = null;
		}

#region InputActions
		public Action        Confirm    { get; set; }
		public Action<float> ZoomAction { get; set; }
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
	}
}
