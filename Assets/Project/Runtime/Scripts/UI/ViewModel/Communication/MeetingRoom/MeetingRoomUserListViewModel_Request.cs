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
using Com2Verse.Network;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
	public partial class MeetingRoomUserListViewModel
	{
		private void RequestMeetingInfo()
		{
			if (MeetingReservationProvider.EnteredMeetingInfo == null)
				return;

			Commander.Instance.RequestMeetingInfoAsync(MeetingReservationProvider.EnteredMeetingInfo.MeetingId, OnResponseMeetingInfo, error =>
			{
				
			}).Forget();
		}
	}
}
