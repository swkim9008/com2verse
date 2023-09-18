/*===============================================================
* Product:		Com2Verse
* File Name:	Commander_Notification.cs
* Developer:	ydh
* Date:			2023-05-09 12:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Communication;
using Protocols;
using Protocols.Notification;
using MessageTypes = Protocols.Notification.MessageTypes;

namespace Com2Verse.Network
{
	public sealed partial class Commander
	{
		public void RequestNotificationOption(int accessServiceId, int accessStateType, bool access, int receivedServiceId, int receivedStateType, bool received)
		{
			NotificationOption accessOption = new NotificationOption();
			accessOption.ServiceId = accessServiceId;
			accessOption.StateType = accessStateType;
			accessOption.StateCode = access;
			
			NotificationOption receivedOption = new NotificationOption();
			receivedOption.ServiceId = receivedServiceId;
			receivedOption.StateType = receivedStateType;
			receivedOption.StateCode = received;
			
			NotificationOptionRequest notificationOptionRequest = new()
			{
				NotificationOptions = { accessOption , receivedOption }
			};
			LogPacketSend(notificationOptionRequest.ToString());
			NetworkManager.Instance.Send(notificationOptionRequest, MessageTypes.NotificationOptionRequest);
		}
		
		public void RequestNotificationGetOption()
		{
			NotificationGetOptionRequest notificationGetOptionRequest = new() { };
			LogPacketSend(notificationGetOptionRequest.ToString());
			NetworkManager.Instance.Send(notificationGetOptionRequest, MessageTypes.NotificationGetOptionRequest);
		}

		public void RequestNotificationReply(string notificationId, int acceptOrRepuse)
		{
			NotificationReplyRequest notificationReplyRequest = new()
			{
				NotificationId = notificationId,
				ButtonIdx = acceptOrRepuse,
			};
			LogPacketSend(notificationReplyRequest.ToString());
			NetworkManager.Instance.Send(notificationReplyRequest, MessageTypes.NotificationReplyRequest);
		}

		public void RequestNotificationClose(string notificationId)
		{
			NotificationCloseRequest notificationCloseRequest = new()
			{
				NotificationId = notificationId
			};
			LogPacketSend(notificationCloseRequest.ToString());
			NetworkManager.Instance.Send(notificationCloseRequest, MessageTypes.NotificationCloseRequest);
		}
		
		public void RequestNotificationGetList()
		{
			NotificationGetListRequest notificationGetListRequest = new() { };
			LogPacketSend(notificationGetListRequest.ToString());
			NetworkManager.Instance.Send(notificationGetListRequest, MessageTypes.NotificationGetListRequest);
		}

		public void RequestNotificationRead(ProtoDateTime time)
		{
			NotificationReadRequest notificationReadRequest = new()
			{
				LastAlarmDatetime = time
			};
			LogPacketSend(notificationReadRequest.ToString());
			NetworkManager.Instance.Send(notificationReadRequest, MessageTypes.NotificationReadRequest);
		}

#region  CheatTest
		public void RequestNotificationCreate(int type)
		{
			NotificationNotifyType notifytype = NotificationNotifyType.Normal;
			string btn = "버튼";
			if (type == 0)
			{
				notifytype = NotificationNotifyType.Normal;
			}
			else if (type == 1)
			{
				notifytype = NotificationNotifyType.Select;
				btn = "버튼,버튼";
			}
			else if (type == 2)
			{
				notifytype = NotificationNotifyType.Webview;
			}
			
			var request = new NotificationCheatRequest()
	        {
		        NotificationCreateRequest = new NotificationCreateRequest()
		        {
			        ButtonFunc = "ButtonFunc",
			        SendAccountId = User.Instance.CurrentUserData.ID,

			        NotificationNotify = new NotificationNotify()
			        {
				        AlarmIcon = "알람 아이콘 이름",
				        AlarmRegisterTime = DateTime.Now.ToProtoDateTime(),
				        
				        NotificationType = notifytype,
				        StateCode = 0,
				        NotificationDetail = 
				        { 
					        new NotificationDetail()
					        {
						        LanguageCode = 0,
						        Title = "Normal",
						        TextMessage = "내용",
						        Button = btn,
					        },
					        new NotificationDetail()
					        {
						        LanguageCode = 1,
						        Title = "Normal",
						        TextMessage = "Message",
						        Button = btn,
					        },
				        },
			        }
		        }
	        };
			
			LogPacketSend(request.ToString());
			NetworkManager.Instance.Send(request, MessageTypes.NotificationCheatRequest);
		}
#endregion CheatTest
	}
}