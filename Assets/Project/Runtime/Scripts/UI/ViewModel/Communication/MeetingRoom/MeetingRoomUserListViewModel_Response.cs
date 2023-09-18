/*===============================================================
 * Product:		Com2Verse
 * File Name:	MeetingRoomUserListViewModel.cs
 * Developer:	urun4m0r1
 * Date:		2022-12-13 14:57
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.MeetingReservation;
using Com2Verse.Organization;
using Protocols.OfficeMeeting;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using ResponseMeetingInfo = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityResponseFormat>;

namespace Com2Verse.UI
{
	public partial class MeetingRoomUserListViewModel
	{
		// private void OnResponseMeetingInfo(MeetingInfoResponse response)
		// {
		// 	Network.Communication.PacketReceiver.Instance.MeetingInfoResponse -= OnResponseMeetingInfo;
		//
		// 	// TODO : NEW_ORGANIZATION WEB API로 대체 (임시)
		// 	// MeetingReservationProvider.SetMeetingInfo(response.MeetingInfo);
		// 	MeetingReservationProvider.SetMeetingInfo(response.MeetingInfo.Convert());
		// 	UIManager.Instance.HideWaitingResponsePopup();
		// }
		private void OnResponseMeetingInfo(ResponseMeetingInfo response)
		{
			// Network.Communication.PacketReceiver.Instance.MeetingInfoResponse -= OnResponseMeetingInfo;

			// TODO : NEW_ORGANIZATION WEB API로 대체 (RequestMeetingInfo)
			MeetingReservationProvider.SetMeetingInfo(response.Value.Data);
			UIManager.Instance.HideWaitingResponsePopup();
		}
	}
}
