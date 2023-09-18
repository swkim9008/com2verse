/*===============================================================
* Product:		Com2Verse
* File Name:	NotificationManager.cs
* Developer:	tlghks1009
* Date:			2023-01-16 10:48
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Text;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Option;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Protocols.Notification;
using UnityEngine;

namespace Com2Verse.Notification
{
	public sealed partial class NotificationManager : Singleton<NotificationManager>, IDisposable
	{
		[UsedImplicitly] private NotificationManager() { }

		public static int NotificationHoldingTime = 5;

		public static int NotificationMaxCount { get => OptionController.Instance.GetOption<AccountOption>().AlarmCountIndex + 1; } 
		private readonly NotificationCore _notificationCore = new();

		public NotificationController NotificationController { get; set; }

		public void Initialize()
		{
			_notificationCore.Initialize();
			SetNotificationListener();
			
			Commander.Instance.RequestNotificationGetList();
		}

		private void SetNotificationListener()
		{
			AddListener(new NotificationWebViewListener(NotificationCore.eUserWebViewType.WEBVIEW, ShowPopupWebView));
		}

		private void ShowPopupWebView(string url)
		{
			UIManager.Instance.ShowPopupWebView(true, new Vector2(1300, 800), url);
		}
		
		private void Release()
		{
			if (NotificationController != null)
			{
				NotificationController.Release();
				NotificationController = null;
			}
			
			if (NotificationPopUpController != null)
			{
				NotificationPopUpController.Release();
				NotificationPopUpController = null;
			}
			
			if (_notifyList != null)
			{
				_notifyList.Clear();
				_notifyList = null;
			}
			
			ClearListener();
		}
		
		public void OnResponseShowNotificationNotify(NotificationNotify response)
		{
			C2VDebug.LogMethod(nameof(OnResponseShowNotificationNotify), response?.ToString());

			var notificationInfo = new NotificationInfo(response);
			notificationInfo.SelectAction = SelectButtonAction;
			notificationInfo.WebViewAction = WebViewButtonAction;
			notificationInfo.MoveAction = MoveButtonAction;
			notificationInfo.IsPopUpNotification = false;
			notificationInfo.IsSelectButtonState = true;
			NotifyNotification(notificationInfo);
			
			var notificationPopUpInfo = new NotificationInfo(response);
			notificationPopUpInfo.SelectAction = SelectButtonAction;
			notificationPopUpInfo.WebViewAction = WebViewButtonAction;
			notificationPopUpInfo.MoveAction = MoveButtonAction;
			notificationPopUpInfo.IsPopUpNotification = true;
			notificationPopUpInfo.IsSelectButtonState = true;
			NotifyNotificationPopUpItem(notificationPopUpInfo);
		}
		
		private void NotifyNotification(NotificationInfo notificationInfo)
		{
			if (NotificationController.IsReferenceNull())
			{
				C2VDebug.LogError("[Notification] Can't find notificationController.");
				return;
			}

			NotificationController?.AddNotification(notificationInfo);
		}
		
		public void Dispose() => Release();

		private void Notify(NotificationNotifyType notificationType, int interaction, NotificationInfo info) => _notificationCore.Notify(notificationType, interaction, info);
		private void AddListener(INotificationListener listener) => _notificationCore.AddListener(listener);
		public void RemoveListener(NotificationNotifyType listener) => _notificationCore.RemoveListener(listener);
		private void ClearListener() => _notificationCore.ClearAllListener();

		public void NotificationReplyAfterAction(string id)
		{
			NotificationController?.SameNotificationButtonStateChange(id);
			NotificationPopUpController?.SameNotificationButtonStateChange(id);

			for (int i = 0; i < _notifyList.Count; i++)
			{
				if (_notifyList[i].NotificationData.NotificationId == id)
				{
					_notifyList[i].NotificationData.StateCode = (int)NotificationInfo.NotificationState.ANSWER;
					break;
				}
			}
		}
		
		public static string TrimStringWithLength(string desc, int stringLength)
		{
			if (stringLength > desc.Length)
				return desc;
			
			var sb = new StringBuilder(desc.Substring(0, stringLength));
			return sb.Append("...").ToString();
		}

		private void SelectButtonAction(NotificationNotify notify, int select)
		{
			Commander.Instance.RequestNotificationReply(notify.NotificationId, select);
		}

		private void WebViewButtonAction(NotificationInfo info)
		{
			Notify(info.NotificationData.NotificationType, 0, info);
		}

		private void MoveButtonAction(NotificationInfo info)
		{
			Commander.Instance.RequestNotificationReply(info.NotificationData.NotificationId, 0);
		}

		public void PopUpClosedAction() => _onNotificationPopUpClosed?.Invoke();
		
#region Event
		private Action _onNotificationPopUpOpened;
		private Action _onNotificationPopUpClosed;

		public event Action OnNotificationPopUpOpenedEvent
		{
			add
			{
				_onNotificationPopUpOpened -= value;
				_onNotificationPopUpOpened += value;
			}
			remove => _onNotificationPopUpOpened -= value;
		}
		
		public event Action OnNotificationPopUpClosedEvent
		{
			add
			{
				_onNotificationPopUpClosed -= value;
				_onNotificationPopUpClosed += value;
			}
			remove => _onNotificationPopUpClosed -= value;
		}
#endregion Event

#if ENABLE_CHEATING
		public async UniTask TestNotification(int notificationCount, int intervalMilliseconds)
		{
			for (int i = 0; i < notificationCount; i++)
			{
				NotificationNotify noti = new NotificationNotify();
				noti.AlarmIcon = "UI_Icon_Notification_Office";
				noti.NotificationId = "0";
				noti.AlarmRegisterTime = DateTime.Now.ToProtoDateTime();

				NotificationInfo info = new NotificationInfo(noti);
				
				if(info != null)
					NotifyNotification(info);

				await UniTask.Delay(intervalMilliseconds);
			}
		}
#endif
	}
}