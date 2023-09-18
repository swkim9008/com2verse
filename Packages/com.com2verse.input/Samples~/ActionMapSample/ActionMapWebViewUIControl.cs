using System;
using UnityEngine.InputSystem;
using Com2Verse.InputSystem; 

namespace Com2Verse.Project.InputSystem
{
	public class ActionMapWebViewUIControl : ActionMap
	{
		public ActionMapWebViewUIControl()
		{
			_name = "WebViewUI";

			_actionList.Add(OnConfirmStarted);
			_actionList.Add(OnConfirmPerformed);
			_actionList.Add(OnConfirmCanceled);
		}

		public override void ClearActions()
		{
			Confirm = null;
		}

#region InputActions
		public Action Confirm { get; set; }
#endregion InputActions

#region Confirm
		private void OnConfirmStarted(InputAction.CallbackContext callback) { }

		private void OnConfirmPerformed(InputAction.CallbackContext callback)
		{
			Confirm?.Invoke();
		}

		private void OnConfirmCanceled(InputAction.CallbackContext callback) { }
#endregion Confirm
	}
}
