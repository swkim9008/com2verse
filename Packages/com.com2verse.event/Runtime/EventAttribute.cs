/*===============================================================
* Product:		Com2Verse
* File Name:	EventAttribute.cs
* Developer:	ydh
* Date:			2023-01-13 09:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.EventListen
{
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	public class EventKeyAttribute : Attribute
	{
		private string _key;

		public EventKeyAttribute(string key)
		{
			_key = key;
		}

		public string Key => _key;
	}
}