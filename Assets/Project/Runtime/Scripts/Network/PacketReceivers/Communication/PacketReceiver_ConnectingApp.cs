/*===============================================================
* Product:		Com2Verse
* File Name:	PacketReceiver_MeetingRoom.cs
* Developer:	eugene9721
* Date:			2022-10-31 17:32
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Protocols;
using Protocols.OfficeMeeting;

namespace Com2Verse.Network.Communication
{
	// PacketReceiver_MeetingRoom
	public partial class PacketReceiver
	{
#region Event
		//public event Action<MeetingMyListResponse>?            MeetingMyListResponse;
		public event Action<MeetingReservationResponse>?       MeetingReservationResponse;
		public event Action<MeetingReservationStatusResponse>? MeetingReservationStatusResponse;
		public event Action<MeetingReservationCancelResponse>? MeetingReservationCancelResponse;
		public event Action<MeetingReservationChangeResponse>? MeetingReservationChangeResponse;
		public event Action<MeetingUserDeleteResponse>?        MeetingUserDeleteResponse;
		public event Action<MeetingSearchBySpaceCodeResponse>? MeetingSearchBySpaceCodeResponse;
		public event Action<MeetingSearchByDetailResponse>?    MeetingSearchByDetailResponse;
		public event Action<MeetingOrganizerChangeResponse>?   MeetingOrganizerChangeResponse;
		public event Action<MeetingInfoResponse>?              MeetingInfoResponse;
		public event Action<MeetingInviteResponse>?            MeetingInviteResponse;
		public event Action<MeetingInviteNotify>?              MeetingInviteNotify;
		public event Action<MeetingInviteAcceptResponse>?      MeetingInviteAcceptResponse;
		public event Action<MeetingInviteCancelResponse>?      MeetingInviteCancelResponse;
		public event Action<MeetingInviteCancelNotify>?        MeetingInviteCancelNotify;
		public event Action<MeetingInviteRejectResponse>?      MeetingInviteRejectResponse;
		public event Action<MeetingWaitlistAcceptResponse>?    MeetingWaitListAcceptResponse;
		public event Action<MeetingWaitlistAcceptNotify>?      MeetingWaitListAcceptNotify;
		public event Action<MeetingWaitlistRejectResponse>?    MeetingWaitListRejectResponse;
		public event Action<MeetingWaitlistRejectNotify>?      MeetingWaitListRejectNotify;
		public event Action<MeetingAttendanceResponse>?        MeetingAttendanceResponse;
		public event Action<MeetingAttendanceNotify>?          MeetingAttendanceNotify;
		public event Action<MeetingAttendanceCancelResponse>?  MeetingAttendanceCancelResponse;
		public event Action<MeetingAttendanceCancelNotify>?    MeetingAttendanceCancelNotify;
		public event Action<MessageTypes, ErrorCode>?          ErrorCodeResponse;
#endregion // Event

#region Response
		// public void RaiseMeetingMyListResponse(MeetingMyListResponse response)
		// {
		// 	LogPacketReceived(response.ToString());
		// 	MeetingMyListResponse?.Invoke(response);
		// }

		public void RaiseMeetingReservationResponse(MeetingReservationResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingReservationResponse?.Invoke(response);
		}

		public void RaiseMeetingReservationStatusResponse(MeetingReservationStatusResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingReservationStatusResponse?.Invoke(response);
		}

		public void RaiseMeetingReservationCancelResponse(MeetingReservationCancelResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingReservationCancelResponse?.Invoke(response);
		}

		public void RaiseMeetingReservationChangeResponse(MeetingReservationChangeResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingReservationChangeResponse?.Invoke(response);
		}

		public void RaiseMeetingUserDeleteResponse(MeetingUserDeleteResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingUserDeleteResponse?.Invoke(response);
		}

		public void RaiseMeetingSearchBySpaceCodeResponse(MeetingSearchBySpaceCodeResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingSearchBySpaceCodeResponse?.Invoke(response);
		}

		public void RaiseMeetingSearchByDetailResponse(MeetingSearchByDetailResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingSearchByDetailResponse?.Invoke(response);
		}

		public void RaiseMeetingOrganizerChangeResponse(MeetingOrganizerChangeResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingOrganizerChangeResponse?.Invoke(response);
		}

		public void RaiseMeetingInfoResponse(MeetingInfoResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingInfoResponse?.Invoke(response);
		}

		public void RaiseMeetingInviteResponse(MeetingInviteResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingInviteResponse?.Invoke(response);
		}

		public void RaiseMeetingInviteNotify(MeetingInviteNotify response)
		{
			LogPacketReceived(response.ToString());
			MeetingInviteNotify?.Invoke(response);
		}

		public void RaiseMeetingInviteAcceptResponse(MeetingInviteAcceptResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingInviteAcceptResponse?.Invoke(response);
		}

		public void RaiseMeetingInviteCancelResponse(MeetingInviteCancelResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingInviteCancelResponse?.Invoke(response);
		}

		public void RaiseMeetingInviteCancelNotify(MeetingInviteCancelNotify response)
		{
			LogPacketReceived(response.ToString());
			MeetingInviteCancelNotify?.Invoke(response);
		}

		public void RaiseMeetingInviteRejectResponse(MeetingInviteRejectResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingInviteRejectResponse?.Invoke(response);
		}

		public void RaiseMeetingWaitListAcceptResponse(MeetingWaitlistAcceptResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingWaitListAcceptResponse?.Invoke(response);
		}

		public void RaiseMeetingWaitListAcceptNotify(MeetingWaitlistAcceptNotify response)
		{
			LogPacketReceived(response.ToString());
			MeetingWaitListAcceptNotify?.Invoke(response);
		}

		public void RaiseMeetingWaitListRejectResponse(MeetingWaitlistRejectResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingWaitListRejectResponse?.Invoke(response);
		}

		public void RaiseMeetingWaitListRejectNotify(MeetingWaitlistRejectNotify response)
		{
			LogPacketReceived(response.ToString());
			MeetingWaitListRejectNotify?.Invoke(response);
		}

		public void RaiseMeetingAttendanceResponse(MeetingAttendanceResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingAttendanceResponse?.Invoke(response);
		}

		public void RaiseMeetingAttendanceNotify(MeetingAttendanceNotify response)
		{
			LogPacketReceived(response.ToString());
			MeetingAttendanceNotify?.Invoke(response);
		}

		public void RaiseMeetingAttendanceCancelResponse(MeetingAttendanceCancelResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingAttendanceCancelResponse?.Invoke(response);
		}

		public void RaiseMeetingAttendanceCancelNotify(MeetingAttendanceCancelNotify response)
		{
			LogPacketReceived(response.ToString());
			MeetingAttendanceCancelNotify?.Invoke(response);
		}

		public void RaiseErrorCodeResponse(MessageTypes messageTypes, ErrorCode errorCode)
		{
			ErrorCodeResponse?.Invoke(messageTypes, errorCode);
		}

#endregion // Response

		private void DestroyConnectingApp()
		{
			// MeetingMyListResponse            = null;
			MeetingReservationStatusResponse = null;
			MeetingReservationResponse       = null;
			MeetingReservationCancelResponse = null;
			MeetingReservationChangeResponse = null;
			MeetingUserDeleteResponse        = null;
			MeetingSearchBySpaceCodeResponse = null;
			MeetingSearchByDetailResponse    = null;
			MeetingInfoResponse              = null;
			MeetingOrganizerChangeResponse   = null;
			MeetingInviteResponse            = null;
			MeetingInviteAcceptResponse      = null;
			MeetingInviteCancelResponse      = null;
			MeetingInviteCancelNotify        = null;
			MeetingInviteRejectResponse      = null;
			MeetingWaitListAcceptResponse    = null;
			MeetingWaitListAcceptNotify      = null;
			MeetingWaitListRejectResponse    = null;
			MeetingWaitListRejectNotify      = null;
			MeetingInviteNotify              = null;
			MeetingAttendanceResponse        = null;
			MeetingAttendanceCancelResponse  = null;
			MeetingAttendanceNotify          = null;
			MeetingAttendanceCancelNotify    = null;
			ErrorCodeResponse                = null;
		}
	}
}
