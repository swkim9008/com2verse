// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	ChatAreaProcessor.cs
//  * Developer:	yangsehoon
//  * Date:		2023-06-14 오후 4:02
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.Data;
using Com2Verse.Network;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.CHAT_AREA)]
	public class ChatAreaProcessor : BaseLogicTypeProcessor
	{
		public override void OnZoneEnter(ServerZone zone, int callbackIndex)
		{
			Commander.Instance.ZoneEnter(zone.ZoneId);
		}

		public override void OnZoneExit(ServerZone zone, int callbackIndex)
		{
			Commander.Instance.ZoneExit(zone.ZoneId);
		}
	}
}
