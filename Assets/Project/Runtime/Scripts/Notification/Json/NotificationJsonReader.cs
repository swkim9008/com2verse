/*===============================================================
* Product:		Com2Verse
* File Name:	NotificationJsonReader.cs
* Developer:	tlghks1009
* Date:			2022-10-28 15:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Newtonsoft.Json;
using Protocols.Notification;

namespace Com2Verse.Notification
{
	public static class NotificationJsonReader
	{
		// public static BaseNotificationJsonData Deserialize(NotificationInfo notificationInfo)
		// {
		// 	switch (notificationInfo.NotificationData.Type)
		// 	{
		// 		case NotificationType.PartyTalk: return DeserializePartyTalk(notificationInfo);
		// 		case NotificationType.Meeting:   return DeserializeMeeting(notificationInfo);
		// 		case NotificationType.System:
		// 		case NotificationType.TeamWork:
		// 		case NotificationType.Knock:
		// 			return null;
		// 		default:
		// 			throw new ArgumentOutOfRangeException();
		// 	}
		// }
		//
		// private static BaseNotificationJsonData DeserializeMeeting(NotificationInfo notificationInfo)
		// {
		// 	switch (notificationInfo.NotificationData.Version)
		// 	{
		// 		case 1: return JsonConvert.DeserializeObject<NotificationJsonMeeting_Version1>(notificationInfo.NotificationData?.Data!);
		// 	}
		//
		// 	return null;
		// }
		//
		// private static BaseNotificationJsonData DeserializePartyTalk(NotificationInfo notificationInfo)
		// {
		// 	switch (notificationInfo.NotificationData.Version)
		// 	{
		// 		case 1: return JsonConvert.DeserializeObject<NotificationJsonPartyTalk_Version1>(notificationInfo.NotificationData?.Data!);
		// 	}
		//
		// 	return null;
		// }
	}
}
