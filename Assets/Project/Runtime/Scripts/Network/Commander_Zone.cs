// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	Commander_Zone.cs
//  * Developer:	yangsehoon
//  * Date:		2023-06-15 오후 3:06
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

namespace Com2Verse.Network
{
	public partial class Commander
	{
		public void ZoneEnter(long zoneId)
		{
			Protocols.GameLogic.EnterZoneRequest request = new()
			{
				ZoneId = zoneId
			};
			NetworkManager.Instance.Send(request, Protocols.GameLogic.MessageTypes.EnterZoneRequest);
		}

		public void ZoneExit(long zoneId)
		{
			Protocols.GameLogic.ExitZoneRequest request = new()
			{
				ZoneId = zoneId
			};
			NetworkManager.Instance.Send(request, Protocols.GameLogic.MessageTypes.ExitZoneRequest);
		}
	}
}
