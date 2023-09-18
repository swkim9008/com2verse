/*===============================================================
* Product:		Com2Verse
* File Name:	NotificationResponser.cs
* Developer:	tlghks1009
* Date:			2022-10-06 16:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Data;
using Protocols.Notification;
using Com2Verse.Network;

namespace Com2Verse.Notification
{
    public sealed class NotificationCore
    {
        public enum eUserSelectResponseType
        {
            NONE,
            UNKNOWN,
            CONNECTING_INVITE = 100,            // 주최자가 초대 요청
            CONNECTING_INVITE_REJECT,
            CONNECTING_PARTICIPATION,           // 사용자가 참여 요청
            CONNECTING_PARTICIPATION_REJECT,
        }

        public enum eUserWebViewType
        {
            WEBVIEW,
        }
        
        public enum eUserMoveBuildingType
        {
            MOVE,
        }
        
        public enum eUserMoveSpaceType
        {
            MOVE,
        }
        
        public enum eUserMoveObjectType
        {
            MOVE,
        }

        private readonly Dictionary<NotificationNotifyType, INotificationListener> _notificationListenerDict = new();

        public void Initialize()
        {
            _notificationListenerDict.Clear();
        }

        public void AddListener(INotificationListener listener)
        {
            if(!_notificationListenerDict.ContainsKey(listener.GetTypeToReceiveNotification()))
                _notificationListenerDict.Add(listener.GetTypeToReceiveNotification(), listener);
        }

        public void RemoveListener(NotificationNotifyType listener)
        {
            _notificationListenerDict.Remove(listener);
        }

        public void ClearAllListener()
        {
            _notificationListenerDict.Clear();
        }

        public void Notify(NotificationNotifyType notificationType, int interaction, NotificationInfo info)
        {
            if (_notificationListenerDict.TryGetValue(notificationType, out var listener))
            {
                listener.OnUserResponseToNotification(interaction, info);
            }
        }
    }
}
