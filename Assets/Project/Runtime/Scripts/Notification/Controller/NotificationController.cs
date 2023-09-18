/*===============================================================
* Product:		Com2Verse
* File Name:	NotificationController.cs
* Developer:	tlghks1009
* Date:			2022-10-06 16:01
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Notification;

namespace Com2Verse.UI
{
	public class NotificationController : BaseNotificationController
	{
		private NotificationViewModel _notificationViewModel;
		private readonly int _maxViewCount = 100;

		public override void Initialize() { }
		
		public void OnEnable()
		{
			_notificationViewModel = ViewModelManager.Instance.GetOrAdd<NotificationViewModel>();
			NotificationManager.Instance.NotificationController = this;
		}

		public override void Release()
		{
			base.Release();

			if (_notificationViewModel != null)
			{
				_notificationViewModel.Reset();

				_notificationViewModel = null;
			}
		}
		
		public override void AddNotification(NotificationInfo notificationInfo)
		{
			notificationInfo.OnNotificationCompleteUse += OnNotificationCompleteUse;

			if (!notificationInfo.IsPopUpNotification)
			{
				notificationInfo.CreateTimer(NotificationManager.NotificationHoldingTime);
			}

			_notificationViewModel.AddNotification(notificationInfo);
		}

		private void OnNotificationCompleteUse(NotificationInfo notificationInfo)
		{
			notificationInfo.OnNotificationCompleteUse -= OnNotificationCompleteUse;

			_notificationViewModel?.RemoveNotification(notificationInfo, () =>
			{
				base._onNotificationWillBeRemovedEvent?.Invoke(notificationInfo);
			});
		}

		public override void SameNotificationButtonStateChange(string id)
		{
			foreach (var data in _notificationViewModel.NotificationItemCollection.Value)
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
		
		public override bool HasNotification() => _notificationViewModel.HasNotification;
		public int NumberOfNotification => _notificationViewModel.NotificationItemCollection.CollectionCount;
		public NotificationInfo GetNotificationInfo(string id) => _notificationViewModel.GetItemNotificationInfo(id);

		public void NotificationRedDotCount(int value)
		{
			_notificationViewModel.NotificationRedDotEnable = value > 0;

			_notificationViewModel.UnreadNotificationCount = value > _maxViewCount ? _maxViewCount.ToString() : value.ToString();
		}	
	}
}