/*===============================================================
* Product:		Com2Verse
* File Name:	CommunicationProcessor.cs
* Developer:	haminjeong
* Date:			2022-06-03 10:03
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.UI;
using JetBrains.Annotations;
using Protocols;
using Protocols.OfficeMeeting;

namespace Com2Verse.Network
{
	[UsedImplicitly]
	[Channel(Channels.OfficeMeeting)]
	public sealed class OfficeMeetingProcessor : BaseMessageProcessor
	{
		public override void Initialize()
		{
			MeetingRoomCallBackInitialize();
		}

		public override void ErrorProcess(Channels channel, int command, ErrorCode errorCode)
		{
			base.ErrorProcess(channel, command, errorCode);
		}

		private void MeetingRoomCallBackInitialize()
		{
#region ConnectingApp
			// SetMessageProcessCallback((int)MessageTypes.MeetingMyListResponse,
			//                           static payload => MeetingMyListResponse.Parser?.ParseFrom(payload),
			//                           static message =>
			//                           {
			// 	                          if (message is MeetingMyListResponse response)
			// 	                          {
			// 		                          foreach (var myMeetInfo in response.MyMeetingInfo)
			// 		                          {
			// 			                          myMeetInfo.StartDateTime = myMeetInfo.StartDateTime.ToLocalProtoDateTime();
			// 			                          myMeetInfo.EndDateTime   = myMeetInfo.EndDateTime.ToLocalProtoDateTime();
			// 		                          }
			//
			// 		                          Communication.PacketReceiver.Instance.RaiseMeetingMyListResponse(response);
			// 	                          }
			//                           });
			SetMessageProcessCallback((int)MessageTypes.MeetingReservationResponse,
			                          static payload => MeetingReservationResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingReservationResponse response)
				                          {
					                          response.MeetingInfo.StartDateTime = response.MeetingInfo.StartDateTime.ToLocalProtoDateTime();
					                          response.MeetingInfo.EndDateTime   = response.MeetingInfo.EndDateTime.ToLocalProtoDateTime();

					                          Communication.PacketReceiver.Instance.RaiseMeetingReservationResponse(response);
				                          }
			                          });
			SetMessageProcessCallback((int)MessageTypes.MeetingReservationStatusResponse,
			                          static payload => MeetingReservationStatusResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingReservationStatusResponse response)
				                          {
					                          foreach (var reservationStatus in response.ReservationStatus)
					                          {
						                          reservationStatus.DateTime = reservationStatus.DateTime.ToLocalProtoDateTime();
					                          }

					                          Communication.PacketReceiver.Instance.RaiseMeetingReservationStatusResponse(response);
				                          }
			                          });
			SetMessageProcessCallback((int)MessageTypes.MeetingReservationChangeResponse,
			                          static payload => MeetingReservationChangeResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingReservationChangeResponse response)
				                          {
					                          response.MeetingInfo.StartDateTime = response.MeetingInfo.StartDateTime.ToLocalProtoDateTime();
					                          response.MeetingInfo.EndDateTime   = response.MeetingInfo.EndDateTime.ToLocalProtoDateTime();

					                          Communication.PacketReceiver.Instance.RaiseMeetingReservationChangeResponse(response);
				                          }
			                          });
			SetMessageProcessCallback((int)MessageTypes.MeetingInfoResponse,
			                          static payload => MeetingInfoResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingInfoResponse response)
				                          {
					                          response.MeetingInfo.StartDateTime = response.MeetingInfo.StartDateTime.ToLocalProtoDateTime();
					                          response.MeetingInfo.EndDateTime   = response.MeetingInfo.EndDateTime.ToLocalProtoDateTime();

					                          Communication.PacketReceiver.Instance.RaiseMeetingInfoResponse(response);
				                          }
			                          });
			SetMessageProcessCallback((int)MessageTypes.MeetingReservationCancelResponse,
			                          static payload => MeetingReservationCancelResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingReservationCancelResponse response)
					                          Communication.PacketReceiver.Instance.RaiseMeetingReservationCancelResponse(response);
			                          });
#endregion

#region Inquiry
			SetMessageProcessCallback((int)MessageTypes.MeetingSearchBySpaceCodeResponse,
			                          static payload => MeetingSearchBySpaceCodeResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingSearchBySpaceCodeResponse response)
				                          {
					                          response.MeetingInfo.StartDateTime = response.MeetingInfo.StartDateTime.ToLocalProtoDateTime();
					                          response.MeetingInfo.EndDateTime   = response.MeetingInfo.EndDateTime.ToLocalProtoDateTime();
					                          Communication.PacketReceiver.Instance.RaiseMeetingSearchBySpaceCodeResponse(response);
				                          }
			                          });

			SetMessageProcessCallback((int)MessageTypes.MeetingSearchByDetailResponse,
			                          static payload => MeetingSearchByDetailResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingSearchByDetailResponse response)
				                          {
					                          foreach (var meetingInfo in response.MeetingInfo)
					                          {
						                          meetingInfo.StartDateTime = meetingInfo.StartDateTime.ToLocalProtoDateTime();
						                          meetingInfo.EndDateTime   = meetingInfo.EndDateTime.ToLocalProtoDateTime();
					                          }

					                          Communication.PacketReceiver.Instance.RaiseMeetingSearchByDetailResponse(response);
				                          }
			                          });
			// 커넥팅 조회에서 커넥팅 참여 요청에 대한 응답
			SetMessageProcessCallback((int)MessageTypes.MeetingAttendanceResponse,
			                          static payload => MeetingAttendanceResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingAttendanceResponse response)
				                          {
					                          Communication.PacketReceiver.Instance.RaiseMeetingAttendanceResponse(response);
				                          }
			                          });
			// 커넥팅 조회에서 참여 요청을 했을 때 주최자에게 전달되는 노티
			SetMessageProcessCallback((int)MessageTypes.MeetingAttendanceNotify,
			                          static payload => MeetingAttendanceNotify.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingAttendanceNotify response)
				                          {
					                          Communication.PacketReceiver.Instance.RaiseMeetingAttendanceNotify(response);
				                          }
			                          });
			// 커넥팅 조회에서 커넥팅 참여 취소 요청에 대한 응답
			SetMessageProcessCallback((int)MessageTypes.MeetingAttendanceCancelResponse,
			                          static payload => MeetingAttendanceCancelResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingAttendanceCancelResponse response)
				                          {
					                          Communication.PacketReceiver.Instance.RaiseMeetingAttendanceCancelResponse(response);
				                          }
			                          });

			// 커넥팅 조회에서 커넥팅 참여 취소 요청을 했을 때 주최자에게 전달되는 노티
			SetMessageProcessCallback((int)MessageTypes.MeetingAttendanceCancelNotify,
			                          static payload => MeetingAttendanceCancelNotify.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingAttendanceCancelNotify response)
				                          {
					                          Communication.PacketReceiver.Instance.RaiseMeetingAttendanceCancelNotify(response);
				                          }
			                          });
#endregion

#region InConnecting
			SetMessageProcessCallback((int)MessageTypes.MeetingJoinResponse,
			                          static payload => JoinChannelResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is JoinChannelResponse response)
					                          Communication.PacketReceiver.Instance.RaiseJoinChannelResponse(response);
			                          });
			SetMessageProcessCallback((int)MessageTypes.MeetingGuestJoinRequest,
			                          static payload => JoinChannelResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is JoinChannelResponse response)
					                          Communication.PacketReceiver.Instance.RaiseJoinChannelResponse(response);
			                          });      
			SetMessageProcessCallback((int)MessageTypes.MeetingLeaveResponse,
			                          static payload => LeaveChannelResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is LeaveChannelResponse response)
					                          Communication.PacketReceiver.Instance.RaiseLeaveChannelResponse(response);
			                          });
			SetMessageProcessCallback((int)MessageTypes.MeetingAuthorityChangeResponse,
			                          static payload => MeetingAuthorityChangeResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingAuthorityChangeResponse response)
					                          Communication.PacketReceiver.Instance.RaiseMeetingAuthorityChangeResponse(response);
			                          });
			SetMessageProcessCallback((int)MessageTypes.MeetingForcedOutResponse,
			                          static payload => MeetingForcedOutResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingForcedOutResponse response)
					                          Communication.PacketReceiver.Instance.RaiseMeetingForcedOutResponse(response);
			                          });
			SetMessageProcessCallback((int)MessageTypes.MeetingEndResponse,
			                          static payload => MeetingEndResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingEndResponse response)
					                          Communication.PacketReceiver.Instance.RaiseMeetingEndResponse(response);
			                          });
			SetMessageProcessCallback((int)MessageTypes.MeetingEndNotify,
			                          static payload => MeetingEndNotify.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingEndNotify response)
					                          Communication.PacketReceiver.Instance.RaiseMeetingEndNotify(response);
			                          });

			SetMessageProcessCallback((int)MessageTypes.MeetingAuthorityChangeNotify,
			                          static payload => MeetingAuthorityChangeNotify.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingAuthorityChangeNotify response)
					                          Communication.PacketReceiver.Instance.RaiseMeetingAuthorityChangeNotify(response);
			                          });
#endregion

#region Invite
			// 주최자가 사용자 초대에 대한 응답
			SetMessageProcessCallback((int)MessageTypes.MeetingInviteResponse,
			                          static payload => MeetingInviteResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingInviteResponse response)
				                          {
					                          Communication.PacketReceiver.Instance.RaiseMeetingInviteResponse(response);
				                          }
			                          });
			// 주최자가 사용자 초대 요청에 대한 사용자 노티
			SetMessageProcessCallback((int)MessageTypes.MeetingInviteNotify,
			                          static payload => MeetingInviteNotify.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingInviteNotify response)
				                          {
					                          Communication.PacketReceiver.Instance.RaiseMeetingInviteNotify(response);
				                          }
			                          });
			// 주최자가 사용자를 초대했을 때 사용자가 수락한 것에 대한 응답
			SetMessageProcessCallback((int)MessageTypes.MeetingInviteAcceptResponse,
			                          static payload => MeetingInviteAcceptResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingInviteAcceptResponse response)
				                          {
					                          Communication.PacketReceiver.Instance.RaiseMeetingInviteAcceptResponse(response);
				                          }
			                          });
			// 주최자가 커넥팅 내부에서 사용자 초대를 취소했을 때에 대한 응답
			SetMessageProcessCallback((int)MessageTypes.MeetingInviteCancelResponse,
			                          static payload => MeetingInviteCancelResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingInviteCancelResponse response)
				                          {

					                          Communication.PacketReceiver.Instance.RaiseMeetingInviteCancelResponse(response);
				                          }
			                          });
			// 주최자가 커넥팅 내부에서 사용자 초대를 취소했을 때 사용자에게 전달되는 노티
			SetMessageProcessCallback((int)MessageTypes.MeetingInviteCancelNotify,
			                          static payload => MeetingInviteCancelNotify.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingInviteCancelNotify response)
				                          {

					                          Communication.PacketReceiver.Instance.RaiseMeetingInviteCancelNotify(response);
				                          }
			                          });
			// 주최자가 사용자를 초대했을 때 사용자가 거절한 것에 대한 응답
			SetMessageProcessCallback((int)MessageTypes.MeetingInviteRejectResponse,
			                          static payload => MeetingInviteRejectResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingInviteRejectResponse response)
				                          {
					                          Communication.PacketReceiver.Instance.RaiseMeetingInviteRejectResponse(response);
				                          }
			                          });
			// 커넥팅 초대 요청 리스트에서 초대를 수락했을 때에 대한 응답
			SetMessageProcessCallback((int)MessageTypes.MeetingWaitlistAcceptResponse,
			                          static payload => MeetingWaitlistAcceptResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingWaitlistAcceptResponse response)
				                          {

					                          Communication.PacketReceiver.Instance.RaiseMeetingWaitListAcceptResponse(response);
				                          }
			                          });
			// 커넥팅 초대 요청 리스트에서 초대를 수락했을 때 해당 사용자에게 전달되는 노티
			SetMessageProcessCallback((int)MessageTypes.MeetingWaitlistAcceptNotify,
			                          static payload => MeetingWaitlistAcceptNotify.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingWaitlistAcceptNotify response)
				                          {

					                          Communication.PacketReceiver.Instance.RaiseMeetingWaitListAcceptNotify(response);
				                          }
			                          });
			// 커넥팅 초대 요청 리스트에서 초대를 거절했을 때에 대한 응답
			SetMessageProcessCallback((int)MessageTypes.MeetingWaitlistRejectResponse,
			                          static payload => MeetingWaitlistRejectResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingWaitlistRejectResponse response)
				                          {

					                          Communication.PacketReceiver.Instance.RaiseMeetingWaitListRejectResponse(response);
				                          }
			                          });
			// 커넥팅 초대 요청 리스트에서 초대를 거절했을 때 해당 사용자에게 전달되는 노티
			SetMessageProcessCallback((int)MessageTypes.MeetingWaitlistRejectNotify,
			                          static payload => MeetingWaitlistRejectNotify.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingWaitlistRejectNotify response)
				                          {

					                          Communication.PacketReceiver.Instance.RaiseMeetingWaitListRejectNotify(response);
				                          }
			                          });
#endregion

			SetMessageProcessCallback((int)MessageTypes.MeetingOrganizerChangeResponse,
			                          static payload => MeetingOrganizerChangeResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingOrganizerChangeResponse response)
					                          Communication.PacketReceiver.Instance.RaiseMeetingOrganizerChangeResponse(response);
			                          });
			SetMessageProcessCallback((int)MessageTypes.MeetingUserDeleteResponse,
			                          static payload => MeetingUserDeleteResponse.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is MeetingUserDeleteResponse response)
					                          Communication.PacketReceiver.Instance.RaiseMeetingUserDeleteResponse(response);
			                          });


			//FIXME : Log error 30 임시 대응
			SetMessageProcessCallback((int) MessageTypes.MeetingJoinUserNotify,
			                          static payload => null,
			                          static message =>
			                          {});

			//FIXME : Log error 31 임시 대응 언젠가 사용할 수도? - 회의실 나갈 때 브로드캐스팅
			SetMessageProcessCallback((int) MessageTypes.MeetingLeaveUserNotify,
			                          static payload => null,
			                          static message => { });
		}
	}
}
