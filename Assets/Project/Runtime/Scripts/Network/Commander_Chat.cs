/*===============================================================
* Product:		Com2Verse
* File Name:	Commander_Chat.cs
* Developer:	haminjeong
* Date:			2022-07-25 12:03
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.Network
{
	public sealed partial class Commander
	{
		public void SendChatMessage(string sender, string message, string time, long id = -1)
		{
			if (!User.Instance.Standby) return;
			Protocols.Chat.FieldChattingRequest areaChatting = new()
			{
				SenderName = sender,
				Message = message,
				TimeString = time,
			};
			NetworkManager.Instance.Send(areaChatting, Protocols.Chat.MessageTypes.FieldChattingRequest);
		}

		public void SendChatMessage(int groupId, string message)
		{
			if (!User.Instance.Standby) return;
			Protocols.GameLogic.GroupChatRequest groupChatRequest = new()
			{
				GroupId = groupId,
				Msg = message,
			};

			NetworkManager.Instance.Send(groupChatRequest, Protocols.GameLogic.MessageTypes.GroupChatRequest);
		}

		public void SendChatMessage(long companyCode, string deptCode, string message)
		{
			if (!User.Instance.Standby) return;

			Protocols.GameLogic.OrganizationChatRequest organizationChatRequest = new()
			{
				CompanyCode = companyCode,
				DeptCode = deptCode,
				Msg = message,
			};

			NetworkManager.Instance.Send(organizationChatRequest, Protocols.GameLogic.MessageTypes.OrganizationChatRequest);
		}

		public void RequestAreaUserInfo()
		{
			if (!User.Instance.Standby) return;
			Protocols.Chat.FieldUserListRequest areaUserList = new();
			NetworkManager.Instance.Send(areaUserList, Protocols.Chat.MessageTypes.FieldUserlistRequest);
		}
	}
}
