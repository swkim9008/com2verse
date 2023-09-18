/*===============================================================
* Product:		Com2Verse
* File Name:	NotificationMenuController.cs
* Developer:	ydh
* Date:			2023-04-24 16:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using Com2Verse.UI;

namespace Com2Verse.Notification
{
	public sealed class NotificationPopUpController : BaseNotificationController
	{
		private NotificationPopUpViewModel _notificationPopupViewModel;
		public override void Initialize() { }

		public void OnEnable()
		{
			_notificationPopupViewModel = ViewModelManager.Instance.GetOrAdd<NotificationPopUpViewModel>();
			NotificationManager.Instance.NotificationPopUpController = this;
		}

		public void ViewActive(bool active)
		{
			if (_notificationPopupViewModel == null)
				return;

			if (active)
			{
				_notificationPopupViewModel.OpenPopUp();
				_notificationPopupViewModel.PopupOnState = true;
			}
			else
			{
				_notificationPopupViewModel.ClosePopUp();
			}
		}

		public override void Release()
		{
			base.Release();
			if (_notificationPopupViewModel != null)
			{
				_notificationPopupViewModel.Reset();
				_notificationPopupViewModel = null;
			}
		}

		public override void SameNotificationButtonStateChange(string id)
		{
			foreach (var data in _notificationPopupViewModel.NotificationItemCollection.Value)
			{
				if(data == null || data.NotificationInfo == null)
					continue;
				
				if (id == data.NotificationInfo.NotificationData.NotificationId)
				{
					data.IsSelectButtonState = false;
					break;
				}
			}
		}

		public override void AddNotification(NotificationInfo notificationInfo) => _notificationPopupViewModel.AddNotification(notificationInfo);
		public override bool HasNotification() => _notificationPopupViewModel.NotificationItemCollection.CollectionCount > 0;
		public NotificationInfo GetNotificationInfo(string id) => _notificationPopupViewModel.GetItemNotificationInfo(id);
		public void ClosePopUp() => _notificationPopupViewModel.ClosePopUp();
		public void ClosePopUpBackButton() => NotificationManager.Instance.PopUpClosedAction();
		public bool IsGuiViewActive() => _notificationPopupViewModel.PopupOnState;
		public void RemoveItem(NotificationItemViewModel viewModel) => _notificationPopupViewModel.RemoveNotification(viewModel);
		public void RemoveAllItem() => _notificationPopupViewModel.Reset();
		public void CountSet(int count) => _notificationPopupViewModel.CurrentCount(count);
	}
}