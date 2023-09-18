/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomWaitUserViewModel_Request.cs
* Developer:	ksw
* Date:			2023-04-18 12:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
	public sealed partial class MeetingRoomWaitUserViewModel
	{
		private void RequestInviteCancel()
		{
			Commander.Instance.RequestMeetingInviteCancelAsync(_meetingId, MemberId, OnResponseInviteCancel, error =>
			{
				
			}).Forget();
		}

		private void RequestWaitListAccept()
		{
			Commander.Instance.RequestMeetingWaitListAcceptAsync(_meetingId, MemberId, OnResponseWaitListAccept, error =>
			{
				RequestMeetingInfo();
			}).Forget();
		}

		private void RequestWaitListReject()
		{
			Commander.Instance.RequestMeetingWaitListReject(_meetingId, MemberId, OnResponseWaitListReject, error =>
			{
				RequestMeetingInfo();
			}).Forget();
		}

		private void RequestMeetingInfo()
		{
			Commander.Instance.RequestMeetingInfoAsync(MeetingReservationProvider.EnteredMeetingInfo.MeetingId, OnResponseMeetingInfo, error =>
			{
				
			}).Forget();
		}
	}
}
