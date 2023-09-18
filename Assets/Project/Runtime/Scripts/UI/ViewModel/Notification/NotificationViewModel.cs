/*===============================================================
* Product:		Com2Verse
* File Name:	NotificationViewModel.cs
* Developer:	tlghks1009
* Date:			2022-10-06 15:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Notification;

namespace Com2Verse.UI
{
	public class NotificationModel : DataModel
	{
		public Collection<NotificationItemViewModel> NotificationItemCollection;

		public NotificationModel()
		{
			NotificationItemCollection = new Collection<NotificationItemViewModel>();
		}

		public void AddNotification(NotificationItemViewModel notificationItemViewModel)
		{
			if (NotificationItemCollection.CollectionCount >= NotificationManager.NotificationMaxCount)
			{
				NotificationItemCollection.RemoveItem(0);
			}
			
			NotificationItemCollection.AddItem(notificationItemViewModel);
		}

		public void RemoveNotification(NotificationItemViewModel notificationItemViewModel) => NotificationItemCollection.RemoveItem(notificationItemViewModel);

		public void RemoveNotification(int index) => NotificationItemCollection.RemoveItem(index);

		public NotificationItemViewModel GetNotificationItemViewModel(NotificationInfo notificationInfo)
		{
			foreach (var notificationItemView in NotificationItemCollection.Value)
			{
				if (notificationItemView.NotificationInfo == notificationInfo)
				{
					return notificationItemView;
				}
			}

			return null;
		}


		public NotificationItemViewModel GetNotificationItemViewModel(int index) => NotificationItemCollection.Value[index];


		public void Reset()
		{
			NotificationItemCollection.Reset();
		}
	}

	[ViewModelGroup("Notification")]
	public sealed class NotificationViewModel : ViewModelDataBase<NotificationModel>
	{
		private string _unreadNotificationCount;
		public string UnreadNotificationCount
		{
			get => _unreadNotificationCount;
			set => SetProperty(ref _unreadNotificationCount, value);
		}
		
		private bool _notificationRedDotEnable;
		public bool NotificationRedDotEnable
		{
			get => _notificationRedDotEnable;
			set => SetProperty(ref _notificationRedDotEnable, value);
		}
		
		public CommandHandler Command_NotificationCloseAll { get; }

		public NotificationViewModel()
		{
			Command_NotificationCloseAll = new CommandHandler(OnCommand_NotificationCloseAll);
		}

		public Collection<NotificationItemViewModel> NotificationItemCollection => base.Model.NotificationItemCollection;

		public void AddNotification(NotificationInfo notificationInfo)
		{
			var notificationItemViewModel = new NotificationItemViewModel();

			notificationItemViewModel.Initialize(notificationInfo);

			notificationItemViewModel.Activate();

			base.Model.AddNotification(notificationItemViewModel);
		}


		public void RemoveNotification(NotificationInfo notificationInfo, Action onRemovedComplete)
		{
			var notificationItemViewModel = base.Model.GetNotificationItemViewModel(notificationInfo);

			if (notificationItemViewModel == null)
			{
				return;
			}

			notificationInfo.Reset();

			notificationItemViewModel.OnNotificationTweenFinished += () =>
			{
				base.Model.RemoveNotification(notificationItemViewModel);

				onRemovedComplete?.Invoke();
			};

			notificationItemViewModel.Deactivate();
		}


		public NotificationItemViewModel GetNotificationItemViewModel(int index) => base.Model.GetNotificationItemViewModel(index);

		public void RemoveNotification(int index) => base.Model.RemoveNotification(index);

		public bool HasNotification => NotificationItemCollection.CollectionCount != 0;

		public void Reset()
		{
			CloseAllNotification();

			base.Model.Reset();
		}


		private void OnCommand_NotificationCloseAll()
		{
			CloseAllNotification();
		}

		private void CloseAllNotification()
		{
			foreach (var notificationItemViewModel in base.Model.NotificationItemCollection.Value)
			{
				notificationItemViewModel.NotificationInfo.ForceTimeOut();
			}
		}
		
		public NotificationInfo GetItemNotificationInfo(string id)
		{
			foreach (var item in NotificationItemCollection.Value)
			{
				if (id == item.NotificationInfo.NotificationData.NotificationId)
					return item.NotificationInfo;
			}	
			
			return null;
		}
	}
}
