/*===============================================================
* Product:		Com2Verse
* File Name:	NotificationMenuManager.cs
* Developer:	ydh
* Date:			2023-04-24 15:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Extension;
using Com2Verse.Network;
using Com2Verse.UI;
using Protocols.Notification;

namespace Com2Verse.Notification
{
	public sealed partial class NotificationManager
	{
		public NotificationPopUpController NotificationPopUpController { get; set; }
		private List<NotificationInfo> _notifyList = new();
		private static string _notificationPopUp = "UI_Notification_PopUp";

		public void HidePopUp()
		{
			NotificationPopUpController?.ViewActive(false);
		}

		private void NotifyNotificationPopUpItem(NotificationInfo notificationInfo)
		{
			_notifyList.Add(notificationInfo);

			if (IsOpenedPopUp())
			{
				for (int i = 0; i < _notifyList.Count; i++)
				{
					AddNotificationData(_notifyList[i]);	
				}

				Commander.Instance.RequestNotificationRead(_notifyList[0].NotificationData.AlarmRegisterTime);

				_notifyList.Clear();
			}

			int unreadNotify = 0;
			for (int i = 0; i < _notifyList.Count; i++)
			{
				if (_notifyList[i].NotificationData.StateCode == (int)NotificationInfo.NotificationState.UNREAD)
					unreadNotify++;
			}
			NotificationController.NotificationRedDotCount(unreadNotify);
		}

		public void NotificationPopUpOpen()
		{
			if (NotificationPopUpController == null)
				return;
			
			for (int i = 0; i < _notifyList.Count; i++)
			{
				AddNotificationData(_notifyList[i]);
			}
			
			NotificationPopUpController.ViewActive(true);
			NotificationController?.NotificationRedDotCount(0);
			_onNotificationPopUpOpened?.Invoke();
			
			if(_notifyList.Count > 0)
				Commander.Instance.RequestNotificationRead(_notifyList.LastItem().NotificationData.AlarmRegisterTime);
			
			_notifyList.Clear();
		}

		private bool IsOpenedPopUp()
		{
			if (NotificationPopUpController == null || !NotificationPopUpController.IsGuiViewActive())
				return false;
			
			return true;
		}
		
		public void AddNotifyData(NotificationNotify notify)
		{
			var notificationInfo = new NotificationInfo(notify);
			notificationInfo.IsPopUpNotification = true;
			notificationInfo.SelectAction = SelectButtonAction;
			notificationInfo.WebViewAction = WebViewButtonAction;
			notificationInfo.MoveAction = MoveButtonAction;
			_notifyList.Add(notificationInfo);
		}

		public void NotifyReadDataCheck()
		{
			int unreadNotify = 0;
			for (int i = 0; i < _notifyList.Count; i++)
			{
				if (_notifyList[i].NotificationData.StateCode == (int)NotificationInfo.NotificationState.UNREAD)
					unreadNotify++;
			}

			if (NotificationController.IsReferenceNull())
				return;
			
			NotificationController.NotificationRedDotCount(unreadNotify);
		}

		public void FindNotifyAndNotifyAction(string id)
		{
			NotificationInfo notificationInfo = NotificationController?.GetNotificationInfo(id);

			if (notificationInfo != null)
			{
				NotificationNotify(notificationInfo);
			}
			else
			{
				var notificationPopUpItemInfo = NotificationPopUpController?.GetNotificationInfo(id);
				if (notificationPopUpItemInfo != null)
				{
					NotificationNotify(notificationPopUpItemInfo);
				}
			}
		}

		private void NotificationNotify(NotificationInfo info)
		{
			if (info.NotificationData.NotificationType == NotificationNotifyType.Select)
			{
				if (info.NotificationData.NotificationDetail[0].LinkAddress.Equals(string.Empty))
					return;
				
				var linkAddress = info.NotificationData.NotificationDetail[0].LinkAddress.Replace(" ", string.Empty).Split(',');
				Notify(info.NotificationData.NotificationType, Convert.ToInt32(linkAddress[0]), info);
			}
			else if (info.NotificationData.NotificationType == NotificationNotifyType.MoveBuilding)
			{
			}
		}

		private void AddNotificationData(NotificationInfo notificationInfo) => NotificationPopUpController.AddNotification(notificationInfo);
		public void ClosePopUp() => NotificationPopUpController?.ClosePopUp();

		public void NotificationNotifyReset()
		{
			if (NotificationPopUpController == null) return;
			NotificationPopUpController.CountSet(0);
			NotificationPopUpController.RemoveAllItem();
			_notifyList.Clear();
		}
	}
}