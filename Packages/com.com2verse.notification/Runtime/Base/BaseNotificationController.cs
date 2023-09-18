/*===============================================================
* Product:		Com2Verse
* File Name:	BaseNotificationController.cs
* Developer:	tlghks1009
* Date:			2022-10-25 11:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;

namespace Com2Verse.Notification
{
	public abstract class BaseNotificationController : MonoBehaviour
	{
		protected Action<NotificationInfo> _onNotificationWillBeRemovedEvent;
		public event Action<NotificationInfo> OnNotificationWillBeRemovedEvent
		{
			add
			{
				_onNotificationWillBeRemovedEvent -= value;
				_onNotificationWillBeRemovedEvent += value;
			}
			remove => _onNotificationWillBeRemovedEvent -= value;
		}

		public abstract void Initialize();

		public abstract void AddNotification(NotificationInfo notificationInfo);

		public abstract bool HasNotification();

		public virtual void Release()
		{
			_onNotificationWillBeRemovedEvent = null;
		}

		public abstract void SameNotificationButtonStateChange(string id);
	}
}
