/*===============================================================
* Product:		Com2Verse
* File Name:	INotificationListener.cs
* Developer:	tlghks1009
* Date:			2022-11-16 13:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Protocols.Notification;

namespace Com2Verse.Notification
{
	public interface INotificationListener
	{
		/// <summary>
		/// 알림을 받을 타입 정의
		/// </summary>
		/// <returns></returns>
		NotificationNotifyType GetTypeToReceiveNotification();

		/// <summary>
		/// 알림에 대한 유저 응답이 왔을 때
		/// </summary>
		/// <param name="userResponse"></param>
		void OnUserResponseToNotification(int userResponse, NotificationInfo info);
	}
	
	public class NotificationSelectButtonListener : INotificationListener, IDisposable
	{
		public NotificationSelectButtonListener(NotificationCore.eUserSelectResponseType type, Action action) => _dictionaryAction.Add(type, action);
			
		private NotificationNotifyType _types = NotificationNotifyType.Select;
		public NotificationNotifyType GetTypeToReceiveNotification() => _types;

		private Dictionary<NotificationCore.eUserSelectResponseType, Action> _dictionaryAction = new();

		public void OnUserResponseToNotification(int userResponse, NotificationInfo info)
		{
			switch ((NotificationCore.eUserSelectResponseType)userResponse)
			{
				case NotificationCore.eUserSelectResponseType.CONNECTING_INVITE:
					_dictionaryAction[(NotificationCore.eUserSelectResponseType)userResponse]?.Invoke();
					break;
				case NotificationCore.eUserSelectResponseType.CONNECTING_INVITE_REJECT:
					_dictionaryAction[(NotificationCore.eUserSelectResponseType)userResponse]?.Invoke();
					break;
			}
		}

		public void AddNotificationAction(NotificationCore.eUserSelectResponseType type, Action action) => _dictionaryAction.Add(type, action);

		public void Dispose()
		{
			_dictionaryAction.Clear();
			_dictionaryAction = null;
		}
	}
	
	public class NotificationWebViewListener : INotificationListener, IDisposable
	{
		public NotificationWebViewListener(NotificationCore.eUserWebViewType type, Action<string> action) =>_dictionaryAction.Add(type, action);
		
		private NotificationNotifyType _types =NotificationNotifyType.Webview;
		public NotificationNotifyType GetTypeToReceiveNotification() => _types;
		
		private Dictionary<NotificationCore.eUserWebViewType, Action<string>> _dictionaryAction = new();
		
		public void OnUserResponseToNotification(int userResponse, NotificationInfo info)
		{
			switch ((NotificationCore.eUserWebViewType)userResponse)
			{
				case NotificationCore.eUserWebViewType.WEBVIEW:
					_dictionaryAction[(NotificationCore.eUserWebViewType)userResponse]?.Invoke(info.NotificationData.NotificationDetail[0].LinkAddress);
					break;
			}
		}

		public void Dispose()
		{
			_dictionaryAction.Clear();
			_dictionaryAction = null;
		}
	}
	
	public class NotificationMoveBuildingListener : INotificationListener, IDisposable
	{
		public NotificationMoveBuildingListener(NotificationCore.eUserMoveBuildingType type, Action<int> action) =>_dictionaryAction.Add(type, action);
		
		private NotificationNotifyType _types = NotificationNotifyType.MoveBuilding;
		public NotificationNotifyType GetTypeToReceiveNotification() => _types;
		private Dictionary<NotificationCore.eUserMoveBuildingType, Action<int>> _dictionaryAction = new();
		
		public void OnUserResponseToNotification(int userResponse, NotificationInfo info)
		{
			switch ((NotificationCore.eUserMoveBuildingType)userResponse)
			{
				case NotificationCore.eUserMoveBuildingType.MOVE:
					_dictionaryAction[(NotificationCore.eUserMoveBuildingType)userResponse]?.Invoke(Convert.ToInt32(info.NotificationData.NotificationDetail[0].LinkAddress));
					break;
			}
		}
		
		public void Dispose()
		{
			_dictionaryAction.Clear();
			_dictionaryAction = null;
		}
	}
	
	public class NotificationMoveSpaceListener : INotificationListener, IDisposable
	{
		public NotificationMoveSpaceListener(NotificationCore.eUserMoveSpaceType type, Action<int> action) =>_dictionaryAction.Add(type, action);
		
		private NotificationNotifyType _types = NotificationNotifyType.MoveSpace;
		public NotificationNotifyType GetTypeToReceiveNotification() => _types;
		private Dictionary<NotificationCore.eUserMoveSpaceType, Action<int>> _dictionaryAction = new();
		
		public void OnUserResponseToNotification(int userResponse, NotificationInfo info)
		{
			switch ((NotificationCore.eUserMoveSpaceType)userResponse)
			{
				case NotificationCore.eUserMoveSpaceType.MOVE:
					_dictionaryAction[(NotificationCore.eUserMoveSpaceType)userResponse]?.Invoke(Convert.ToInt32(info.NotificationData.NotificationDetail[0].LinkAddress));
					break;
			}
		}
		
		public void Dispose()
		{
			_dictionaryAction.Clear();
			_dictionaryAction = null;
		}
	}
	
	public class NotificationMoveObjectListener : INotificationListener, IDisposable
	{
		public NotificationMoveObjectListener(NotificationCore.eUserMoveObjectType type, Action<int> action) =>_dictionaryAction.Add(type, action);
		
		private NotificationNotifyType _types = NotificationNotifyType.MoveObject;
		public NotificationNotifyType GetTypeToReceiveNotification() => _types;
		private Dictionary<NotificationCore.eUserMoveObjectType, Action<int>> _dictionaryAction = new();
		
		public void OnUserResponseToNotification(int userResponse, NotificationInfo info)
		{
			switch ((NotificationCore.eUserMoveObjectType)userResponse)
			{
				case NotificationCore.eUserMoveObjectType.MOVE:
					_dictionaryAction[(NotificationCore.eUserMoveObjectType)userResponse]?.Invoke(Convert.ToInt32(info.NotificationData.NotificationDetail[0].LinkAddress));
					break;
			}
		}
		
		public void Dispose()
		{
			_dictionaryAction.Clear();
			_dictionaryAction = null;
		}
	}
}