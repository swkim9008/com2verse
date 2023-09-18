/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomUserViewModel_Request.cs
* Developer:	tlghks1009
* Date:			2022-11-08 16:33
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Chat;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
	public partial class MeetingRoomUserViewModel
	{
		private void RequestMeetingForcedOut()
		{
			Commander.Instance.RequestMeetingForcedOutAsync(MeetingReservationProvider.EnteredMeetingInfo.MeetingId, UserViewModel.UserId, response =>
			{
				// TODO : UserViewModel.UserId가 100% 신뢰할 수 없는 정보라 response에서 받아서 넘겨야 함
				var data = new ChatManager.ForcedOutNotifyData
				{
					TargetUser = UserViewModel.UserId,
				};
				ChatManager.Instance.BroadcastCustomData(ChatManager.CustomDataType.FORCED_OUT_NOTIFY, data);
				CloseDropDown();
			}, error =>
			{
				CloseDropDown();
				RefreshPermissionView();
			}).Forget();
		}
	}
}
