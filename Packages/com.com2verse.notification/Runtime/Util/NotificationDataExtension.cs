/*===============================================================
* Product:		Com2Verse
* File Name:	NotificationDataExtension.cs
* Developer:	tlghks1009
* Date:			2022-11-16 14:24
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Protocols.Notification;

namespace Com2Verse.Notification
{
	public static class NotificationDataExtension
	{
		public static bool IsFeedBack(this NotificationNotify NotificationData)
		{
			switch (NotificationData.NotificationType)
			{
				case NotificationNotifyType.Normal:
					return false;
				case NotificationNotifyType.Select:
				case NotificationNotifyType.MoveBuilding:
				case NotificationNotifyType.MoveObject:
				case NotificationNotifyType.MoveSpace:
				case NotificationNotifyType.Webview:
					return true;
				
				default:
					return false;
			}
		}
	}
}
	