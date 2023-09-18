/*===============================================================
* Product:		Com2Verse
* File Name:	PacketReceiver_Chat.cs
* Developer:	eugene9721
* Date:			2023-05-26 18:44
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Protocols.GameLogic;
using Protocols.Notification;

namespace Com2Verse.Network.GameLogic
{
	public partial class PacketReceiver
	{
		public event Action<AnnouncementNotify> SystemNoticeResponse;

		public void RaiseSystemNoticeResponse(AnnouncementNotify response)
		{
			SystemNoticeResponse?.Invoke(response);
		}

		private void DisposeChat()
		{
			SystemNoticeResponse = null;
		}
	}
}
