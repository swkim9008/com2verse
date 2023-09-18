/*===============================================================
* Product:		Com2Verse
* File Name:	MiceInfoManager_Ticket.cs
* Developer:	ikyoung
* Date:			2023-04-13 15:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Mice
{
	public sealed partial class MiceInfoManager
	{
		public List<MiceTicketInfo> TicketInfos = new List<MiceTicketInfo>();
		
		public async UniTask SyncTicketInfo()
		{
			TicketInfos.Clear();
			var result = await MiceWebClient.User.TicketsGet();
			if (result)
			{
				var datas =  result.Data;
				if (datas != null)
				{
					foreach (var ticketEntity in datas)
					{
						var ticketInfo = new MiceTicketInfo();
						ticketInfo.Sync(ticketEntity);
						TicketInfos.Add(ticketInfo);
					}
				}
			}
			await UniTask.CompletedTask;
		}

		public bool HasTicket(MiceWebClient.eMiceAuthorityCode checkType, long eventID)
		{
			if (!EventInfos.TryGetValue(eventID, out var eventInfo)) return false;

			foreach (var sessionInfo in eventInfo.SessionInfoList)
			{
				if (HasTicketWithSessionID(checkType, sessionInfo.ID)) return true;
			}
			return false;
		}
		
		public bool HasTicketWithSessionID(MiceWebClient.eMiceAuthorityCode checkType, long sessionID)
		{
			for (int i = 0; i < TicketInfos.Count; i++)
			{
				var ticket = TicketInfos[i].TicketEntity;
				if (ticket.AuthorityCode == checkType && sessionID == ticket.SessionId)
					return true;
			}
			return false;
		}

		public MiceWebClient.eMiceAuthorityCode SelectProperUserAuthorityWithSessionID(long sessionID)
		{
			if (HasTicketWithSessionID(MiceWebClient.eMiceAuthorityCode.SPEAKER, sessionID)) return MiceWebClient.eMiceAuthorityCode.SPEAKER;
			else if (HasTicketWithSessionID(MiceWebClient.eMiceAuthorityCode.STAFF, sessionID)) return MiceWebClient.eMiceAuthorityCode.STAFF;
			else if (HasTicketWithSessionID(MiceWebClient.eMiceAuthorityCode.OPERATOR, sessionID)) return MiceWebClient.eMiceAuthorityCode.OPERATOR;
			return MiceWebClient.eMiceAuthorityCode.NORMAL;
		}
	}
}
