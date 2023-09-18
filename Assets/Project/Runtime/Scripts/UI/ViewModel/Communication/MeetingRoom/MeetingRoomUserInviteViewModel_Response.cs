/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomUserInviteViewModel.cs
* Developer:	ksw
* Date:			2023-04-17 14:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.UI;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using ResponseMeetingInfo = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityResponseFormat>;
using ResponseMeetingInvite = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.InviteResponseResponseFormat>;

namespace Com2Verse
{
	public sealed partial class MeetingRoomUserInviteViewModel
	{
#region Web API
		// private void OnResponseMeetingInfo(MeetingInfoResponse response)
		// {
		// 	Network.Communication.PacketReceiver.Instance.MeetingInfoResponse -= OnResponseMeetingInfo;
		//
		// 	// TODO : NEW_ORGANIZATION WEB API로 대체 (임시)
		// 	RefreshViewParticipants();
		// 	UIManager.Instance.HideWaitingResponsePopup();
		// }

		private void OnResponseMeetingInfo(ResponseMeetingInfo response)
		{
			// Network.Communication.PacketReceiver.Instance.MeetingInfoResponse -= OnResponseMeetingInfo;

			// TODO : NEW_ORGANIZATION WEB API로 대체 (임시)
			MeetingReservationProvider.SetMeetingInfo(response.Value.Data);
			RefreshViewParticipants();
		}

		private void OnResponseConnectingInvite(ResponseMeetingInvite response)
		{
			ResponseInvite();
		}
#endregion // Web API
	}
}
