/*===============================================================
* Product:		Com2Verse
* File Name:	Notification.cs
* Developer:	tlghks1009
* Date:			2022-10-06 16:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Protocols.Notification;

namespace Com2Verse.Notification
{
	public class NotificationInfo
	{
		public enum NotificationState
		{
			UNREAD,
			READ,
			ANSWER,
		}
		
		private CancellationTokenSource _cancellationTokenSource;

		private Action<NotificationInfo> _onNotificationCompleteUse;

		public event Action<NotificationInfo> OnNotificationCompleteUse
		{
			add
			{
				_onNotificationCompleteUse -= value;
				_onNotificationCompleteUse += value;
			}
			remove => _onNotificationCompleteUse -= value;
		}

		public NotificationNotify NotificationData { get; }

		public bool IsPopUpNotification { get; set; }
		public bool IsSelectButtonState { get; set; }

		public Action<NotificationNotify, int> SelectAction;
		public Action<NotificationInfo> WebViewAction;
		public Action<NotificationInfo> MoveAction;
		public NotificationInfo(NotificationNotify notificationData) => NotificationData = notificationData;

		public async UniTask CreateTimer(int time)
		{
			_cancellationTokenSource = new CancellationTokenSource();

			await UniTask.Delay(time * 1000, DelayType.Realtime, cancellationToken: _cancellationTokenSource.Token);

			OnTimeFinished();
		}

		public void Reset()
		{
			if (_cancellationTokenSource != null)
			{
				_cancellationTokenSource.Cancel();
				_cancellationTokenSource = null;
			}

			_onNotificationCompleteUse = null;
		}

		public void ForceTimeOut()
		{
			InvokeEventWhenTimeFinished();
			Reset();
		}

		private void OnTimeFinished()
		{
			InvokeEventWhenTimeFinished();
			Reset();
		}

		private void InvokeEventWhenTimeFinished()
		{
			_onNotificationCompleteUse?.Invoke(this);
		}
	}
}