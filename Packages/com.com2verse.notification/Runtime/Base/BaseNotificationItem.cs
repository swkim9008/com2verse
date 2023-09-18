/*===============================================================
* Product:		Com2Verse
* File Name:	BaseNotificationItem.cs
* Developer:	tlghks1009
* Date:			2022-10-28 10:48
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.Notification
{
	public abstract class BaseNotificationItem : MonoBehaviour
	{
		[HideInInspector] public UnityEvent _onTweenFinished;

		protected NotificationInfo _notificationInfo;

		[UsedImplicitly]
		public bool IsVisibleState
		{
			get => this.gameObject.activeSelf;
			set => SetVisibleState(value, _onTweenFinished);
		}

		public NotificationInfo NotificationInfo
		{
			get => _notificationInfo;
			set => _notificationInfo = value;
		}

		protected abstract void SetVisibleState(bool visible, UnityEvent onTweenFinished);
	}
}
