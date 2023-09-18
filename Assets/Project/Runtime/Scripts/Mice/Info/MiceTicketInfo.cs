/*===============================================================
* Product:		Com2Verse
* File Name:	MiceTicketInfo.cs
* Developer:	ikyoung
* Date:			2023-04-14 17:11
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.Mice
{
	public sealed partial class MiceTicketInfo : MiceBaseInfo
	{
		public MiceWebClient.Entities.TicketEntity TicketEntity { get; private set; }

		public void Sync(MiceWebClient.Entities.TicketEntity ticketEntity)
		{
			TicketEntity = ticketEntity;
		}
	}
}
