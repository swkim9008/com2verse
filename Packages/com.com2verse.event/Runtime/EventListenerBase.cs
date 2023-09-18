/*===============================================================
* Product:		Com2Verse
* File Name:	EventListenerBase.cs
* Developer:	ydh
* Date:			2023-01-13 09:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.EventListen
{
	public class EventListenerBase<T> : Singleton<T> where T : class
	{
		private readonly string _eventKey;

		protected EventListenerBase()
		{
			var type = GetType();
			var eventAttr = Attribute.GetCustomAttribute(type, typeof(EventKeyAttribute)) as EventKeyAttribute;
			if (eventAttr != null)
			{
				_eventKey = eventAttr.Key; 
			}
		}

		public virtual void OnEvent() { }
		public void OnEnter() => EventManager.Instance.OnEnter(_eventKey);
		public void SubScribe(Action action) => EventManager.Instance.OnSubscribe(_eventKey, action);	
		public void UnSubScribe(Action action) => EventManager.Instance.OnUnSubscribe(_eventKey, action);
	}
}