/*===============================================================
* Product:		Com2Verse
* File Name:	TTSEventManager.cs
* Developer:	ydh
* Date:			2022-11-10 17:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Com2Verse.EventListen
{
	public sealed class EventManager : Singleton<EventManager>
	{
		private Dictionary<string, Action> _events = new();
		
		[UsedImplicitly] private EventManager() { }

		public void OnEnter(string key)
		{
			if(_events.ContainsKey(key))
				_events[key]?.Invoke();
		}

		public void OnSubscribe(string key, Action action)
		{
			if (_events.ContainsKey(key))
			{
				_events[key] -= action;
				_events[key] += action;
			}
			else
			{
				_events.Add(key, action);
			}
		}

		public void OnUnSubscribe(string key, Action action)
		{
			if (_events.ContainsKey(key))
			{
				_events[key] -= action;
			}

			if (_events.ContainsKey(key))
			{
				_events.Remove(key);
			}
		}
	}
}
