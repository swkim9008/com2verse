/*===============================================================
* Product:		Com2Verse
* File Name:	NotificationJsonDataBase.cs
* Developer:	tlghks1009
* Date:			2022-10-28 15:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using Cysharp.Threading.Tasks;

namespace Com2Verse.Notification
{
	public abstract class BaseNotificationJsonData
	{
		public abstract string GetTitleName(NotificationInfo notificationInfo);

		public abstract UniTask<string> GetDescription(NotificationInfo notificationInfo);
	}
}
