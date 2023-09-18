/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomWaitUserViewModel_Response.cs
* Developer:	ksw
* Date:			2023-04-18 12:43
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.MeetingReservation;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using ResponseMeetingInfo = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityResponseFormat>;
using ResponseInviteCancel = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.InviteCancelResponseResponseFormat>;
using ResponseWaitListAccept = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>;
using ResponseWaitListReject = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>;

namespace Com2Verse.UI
{
	public sealed partial class MeetingRoomWaitUserViewModel
	{
		private void OnResponseWaitListAccept(ResponseWaitListAccept response)
		{
			RequestMeetingInfo();
		}


		private void OnResponseWaitListReject(ResponseWaitListAccept response)
		{
			RequestMeetingInfo();
		}

		private void OnResponseMeetingInfo(ResponseMeetingInfo response)
		{
			// Network.Communication.PacketReceiver.Instance.MeetingInfoResponse -= OnResponseMeetingInfo;

			// TODO : NEW_ORGANIZATION WEB API로 대체 (RequestMeetingInfo)
			MeetingReservationProvider.SetMeetingInfo(response.Value.Data);
			RefreshList?.Invoke();
		}

		private void OnResponseInviteCancel(ResponseInviteCancel response)
		{
			RequestMeetingInfo();
		}
	}
}
