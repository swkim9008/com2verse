/*===============================================================
* Product:		Com2Verse
* File Name:	MiceService_Staff.cs
* Developer:	wlemon
* Date:			2023-08-07 18:39
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Avatar;
using Com2Verse.Data;
using Com2Verse.Network;

namespace Com2Verse.Mice
{
	//TODO: 인벤토리 관련 정식 플로우 적용 시 제거 필요
	public sealed partial class MiceService
	{
		private static readonly int[] StaffFashionItemIds =
		{
			16011043,
			15011039
		};

		public void ApplyStaffFashionItems()
		{
			if (!HasCurrentEventTicket(MiceWebClient.eMiceAuthorityCode.STAFF)) return;
			
			var newAvatarInfo = User.Instance.AvatarInfo.Clone();
			foreach (var itemId in StaffFashionItemIds!)
			{
				var avatarFashionItem = AvatarTable.GetFashionItem(itemId);
				if (avatarFashionItem            == null) continue;
				if (avatarFashionItem.AvatarType != newAvatarInfo.AvatarType) continue;

				newAvatarInfo.UpdateFashionItem(new FashionItemInfo(itemId));
			}

			Commander.Instance.RequestUpdateAvatar(newAvatarInfo, true);
		}
	}
}
