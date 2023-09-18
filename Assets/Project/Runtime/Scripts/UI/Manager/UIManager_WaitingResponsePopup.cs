/*===============================================================
* Product:		Com2Verse
* File Name:	UIManager_WaitingResponsePopup.cs
* Developer:	tlghks1009
* Date:			2022-11-04 13:09
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.UI
{
	public partial class UIManager
	{
		private Timer _waitingResponsePopupTimer;

		public void ShowWaitingResponsePopup(float time = 1f)
		{
			var view = GetSystemView(eSystemViewType.WAITING_RESPONSE_POPUP);

			view.Show();


			var waitingResponsePopupViewModel = view.ViewModelContainer.GetViewModel<WaitingResponsePopupViewModel>();

			waitingResponsePopupViewModel.IsVisibleLoadingView = false;

			UIStackManager.Instance.SetWaitingPopup(true);

			_waitingResponsePopupTimer = new Timer();

			StartTimer(_waitingResponsePopupTimer, time, () =>
			{
				waitingResponsePopupViewModel.IsVisibleLoadingView = true;
			});
		}


		public void HideWaitingResponsePopup()
		{
			if (_waitingResponsePopupTimer != null)
			{
				_waitingResponsePopupTimer.Reset();


				var view = GetSystemView(eSystemViewType.WAITING_RESPONSE_POPUP);

				view.Hide();
			}

			UIStackManager.InstanceOrNull?.SetWaitingPopup(false);
			_waitingResponsePopupTimer = null;
		}
	}
}
