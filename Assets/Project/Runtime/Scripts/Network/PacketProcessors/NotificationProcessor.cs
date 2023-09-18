/*===============================================================
* Product:		Com2Verse
* File Name:	NotificationProcessor.cs
* Developer:	tlghks1009
* Date:			2022-10-11 16:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Linq;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.Notification;
using Com2Verse.UI;
using JetBrains.Annotations;
using Protocols;
using Protocols.Notification;

namespace Com2Verse.Network
{
	[UsedImplicitly]
	[Channel(Protocols.Channels.Notification)]
	public sealed class NotificationProcessor : BaseMessageProcessor
	{
		public override void Initialize()
		{
			SetMessageProcessCallback((int) MessageTypes.NotificationOptionResponse,
									static payload => NotificationOptionResponse.Parser?.ParseFrom(payload),
									static message =>
									{
										if (message is NotificationOptionResponse response)
										{
											C2VDebug.Log($"NotificationOptionResponse : {message}");
										}
									});
			
			SetMessageProcessCallback((int) MessageTypes.NotificationNotify,
			                          static payload => NotificationNotify.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is NotificationNotify response)
				                          {
					                          C2VDebug.Log($"NotificationNotify : {message}");

					                          NotificationManager.Instance.OnResponseShowNotificationNotify(response);
				                          }
			                          });
			
			SetMessageProcessCallback((int)MessageTypes.NotificationGetListResponse, 
									static payload => NotificationGetListResponse.Parser?.ParseFrom(payload),
									static message =>
									{
										if (message is NotificationGetListResponse response)
										{
											C2VDebug.Log($"NotificationGetListResponse : {message}");

											NotificationManager.Instance.NotificationNotifyReset();
											
											var reverseList = response.NotificationList.Reverse();
											foreach (var notificationItem in reverseList)
											{
												NotificationManager.Instance.AddNotifyData(notificationItem);
											}
											
											NotificationManager.Instance.NotifyReadDataCheck();
										}
									});
			
			SetMessageProcessCallback((int) MessageTypes.NotificationGetOptionResponse,
				static payload => NotificationGetOptionResponse.Parser?.ParseFrom(payload),
				static message =>
				{
					if (message is NotificationGetOptionResponse response)
					{
						C2VDebug.Log($"NotificationGetOptionResponse : {message}");
					}
				});
			
			SetMessageProcessCallback((int) MessageTypes.NotificationCloseResponse,
				static payload => NotificationCloseResponse.Parser?.ParseFrom(payload),
				static message =>
				{
					if (message is NotificationCloseResponse response)
					{
						C2VDebug.Log($"NotificationCloseResponse : {message}");
					}
				});
			
			SetMessageProcessCallback((int) MessageTypes.NotificationReplyResponse,
				static payload => NotificationReplyResponse.Parser?.ParseFrom(payload),
				static message =>
				{
					if (message is NotificationReplyResponse response)
					{
						C2VDebug.Log($"NotificationReplyResponse : {message}");
						
						NotificationManager.Instance.NotificationReplyAfterAction(response.NotificationId);
						NotificationManager.Instance.FindNotifyAndNotifyAction(response.NotificationId);
					}
				});
			
			SetMessageProcessCallback((int) MessageTypes.NotificationReadResponse,
				static payload => NotificationReadResponse.Parser?.ParseFrom(payload),
				static message =>
				{
					if (message is NotificationReadResponse response)
					{
						C2VDebug.Log($"NotificationReadResponse : {message}");
					}
				});

			SetMessageProcessCallback((int)MessageTypes.AnnouncementNotify,
			                          payload => AnnouncementNotify.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is AnnouncementNotify response)
					                          GameLogic.PacketReceiver.Instance.RaiseSystemNoticeResponse(response);
			                          });
		}

		public override void ErrorProcess(Channels channel, int command, ErrorCode errorCode)
		{
			if (command == (int)MessageTypes.NotificationReplyResponse)
			{
				UIManager.Instance.HideWaitingResponsePopup();
				C2VDebug.LogError($"NotificationReplyResponse : {errorCode}");

				switch (errorCode)
				{
					case ErrorCode.MeetingEnd:
						UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_ConnectingApp_Reservation_AlreadyConnectingDone_Toast"));
						break;
					default:
						base.ErrorProcess(channel, command, errorCode);
						break;
				}
				return;
			}
			base.ErrorProcess(channel, command, errorCode);
		}
	}
}
