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
using Protocols.OfficeMeeting;

namespace Com2Verse.Network.Communication
{
	// PacketReceiver_MeetingRoom
	public partial class PacketReceiver
	{
#region Event
		public event Action<JoinChannelResponse>?            JoinOfficeChannelResponse;
		public event Action<LeaveChannelResponse>?           LeaveOfficeChannelResponse;
		public event Action<MeetingAuthorityChangeResponse>? MeetingAuthorityChangeResponse;
		public event Action<MeetingForcedOutResponse>?       MeetingForcedOutResponse;
		public event Action<MeetingEndResponse>?             MeetingEndResponse;
		public event Action<MeetingEndNotify>?               MeetingEndNotify;
		public event Action<MeetingAuthorityChangeNotify>?   MeetingAuthorityChangeNotify;

#endregion // Event

#region Response
		public void RaiseJoinChannelResponse(JoinChannelResponse response)
		{
			LogPacketReceived(response.ToString());
			JoinOfficeChannelResponse?.Invoke(response);
		}
		public void RaiseLeaveChannelResponse(LeaveChannelResponse response)
		{
			LogPacketReceived(response.ToString());
			LeaveOfficeChannelResponse?.Invoke(response);
		}
		public void RaiseMeetingAuthorityChangeResponse(MeetingAuthorityChangeResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingAuthorityChangeResponse?.Invoke(response);
		}

		public void RaiseMeetingForcedOutResponse(MeetingForcedOutResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingForcedOutResponse?.Invoke(response);
		}

		public void RaiseMeetingEndResponse(MeetingEndResponse response)
		{
			LogPacketReceived(response.ToString());
			MeetingEndResponse?.Invoke(response);
		}

		public void RaiseMeetingEndNotify(MeetingEndNotify response)
		{
			LogPacketReceived(response.ToString());
			MeetingEndNotify?.Invoke(response);
		}

		public void RaiseMeetingAuthorityChangeNotify(MeetingAuthorityChangeNotify response)
		{
			LogPacketReceived(response.ToString());
			MeetingAuthorityChangeNotify?.Invoke(response);
		}
#endregion // Response

		private void DestroyMeetingRoom()
		{
			JoinOfficeChannelResponse      = null;
			LeaveOfficeChannelResponse     = null;
			MeetingAuthorityChangeResponse = null;
			MeetingForcedOutResponse       = null;
			MeetingEndResponse             = null;
			MeetingEndNotify               = null;
			MeetingAuthorityChangeNotify   = null;
		}
	}
}
