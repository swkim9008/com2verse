/*===============================================================
* Product:		Com2Verse
* File Name:	NotificationPopUpNavigator.cs
* Developer:	ydh
* Date:			2023-05-16 10:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.Notification
{
	public sealed class NotificationPopUpNavigator : MonoBehaviour
	{
		public static void NotificationPopUpOpen()
		{
			NotificationManager.Instance.NotificationPopUpOpen();
		}
	}
}
