/*===============================================================
* Product:		Com2Verse
* File Name:	SitUpEvent.cs
* Developer:	ydh
* Date:			2023-01-12 17:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Tutorial;
using Com2Verse.UserState;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.EventListen
{
	[EventKey("EventTest")]
	public class EventTest : EventListenerBase<EventTest>
	{
		[UsedImplicitly] private EventTest() { }
		public override void OnEvent()
		{
		}
	}
}