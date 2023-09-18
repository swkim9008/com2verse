/*===============================================================
* Product:		Com2Verse
* File Name:	Commander_OfficeMeeting.cs
* Developer:	swkim
* Date:			2023-03-22 11:03
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using Com2Verse.HttpHelper;
using Com2Verse.Logger;
using Com2Verse.PlayerControl;
using Com2Verse.UI;
using Com2Verse.WebApi.Service;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Protocols;
using Protocols.OfficeMeeting;
using EmployeeNoType = System.String;
using MemberIdType = System.Int64;
using ServiceComponents = Com2Verse.WebApi.Service.Components;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using MeetingUserInfoType = Com2Verse.WebApi.Service.Components.MeetingMemberEntity;

using RequestAttendanceCancel = Com2Verse.WebApi.Service.Components.MeetingIdRequest;
using RequestMeetingEnd = Com2Verse.WebApi.Service.Components.MeetingIdRequest;
using RequestReservationCancel = Com2Verse.WebApi.Service.Components.MeetingIdRequest;
using RequestExtendEnd = Com2Verse.WebApi.Service.Components.ExtendEndRequest;

using ResponseReservation = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.ReservationResponseResponseFormat>;
using ResponseMyList = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityIEnumerableResponseFormat>;
using ResponseMeetingInfo = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityResponseFormat>;
using ResponseRoomJoin = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.RoomJoinResponseResponseFormat>;
using ResponseRoomLeave = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.RoomLeaveResponseResponseFormat>;
using ResponseMeetingInvite = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.InviteResponseResponseFormat>;
using ResponseMeetingInviteCancel = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.InviteCancelResponseResponseFormat>;
using ResponseSearchByDetail = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityIEnumerableResponseFormat>;
using ResponseSearchByMeetingCode = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityResponseFormat>;
using ResponseAttendance = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.AttendanceResponseResponseFormat>;
using ResponseAttendanceCancel = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>;
using ResponseAuthorityChange = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>;
using ResponseWaitListAccept = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>;
using ResponseWaitListReject = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>;
using ResponseReservationChange = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.ReservationChangeResponseResponseFormat>;
using ResponseReservationCancel = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>;
using ResponseMeetingEnd = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>;
using ResponseForcedOut = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>;
using ResponseGuestCheck = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.GuestResponseResponseFormat>;
using ResponseExtendEnd = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>;

using DelegateResponseReservation = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.ReservationResponseResponseFormat>>;
using DelegateResponseMyList = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityIEnumerableResponseFormat>>;
using DelegateResponseMeetingInfo = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityResponseFormat>>;
using DelegateResponseRoomJoin = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.RoomJoinResponseResponseFormat>>;
using DelegateResponseRoomLeave = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.RoomLeaveResponseResponseFormat>>;
using DelegateResponseMeetingInvite = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.InviteResponseResponseFormat>>;
using DelegateResponseMeetingInviteCancel = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.InviteCancelResponseResponseFormat>>;
using DelegateResponseSearchByDetail = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityIEnumerableResponseFormat>>;
using DelegateResponseSearchByMeetingCode = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityResponseFormat>>;
using DelegateResponseAttendance = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.AttendanceResponseResponseFormat>>;
using DelegateResponseAttendanceCancel = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>>;
using DelegateResponseAuthorityChange = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>>;
using DelegateResponseWaitListAccept = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>>;
using DelegateResponseWaitListReject = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>>;
using DelegateResponseReservationChange = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.ReservationChangeResponseResponseFormat>>;
using DelegateResponseReservationCancel = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>>;
using DelegateResponseMeetingEnd = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>>;
using DelegateResponseForcedOut = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>>;
using DelegateResponseGuestCheck = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.GuestResponseResponseFormat>>;
using DelegateResponseExtendEnd = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>>;

using DelegateErrorResponse = System.Action<Com2Verse.Network.Commander.ErrorResponseData>;

namespace Com2Verse.Network
{
	public sealed partial class Commander
	{
#region ConnectingApp
		public void RequestMeetingOrganizerChange(long meetingId, EmployeeNoType changeEmployeeNo)
		{
			MeetingOrganizerChangeRequest meetingOrganizerChangeRequest = new()
			{
				MeetingId         = meetingId,
				ChangeEmployeeNo  = changeEmployeeNo
			};
			LogPacketSend(meetingOrganizerChangeRequest.ToString());
			NetworkManager.Instance.Send(meetingOrganizerChangeRequest, MessageTypes.MeetingOrganizerChangeRequest);
		}


		public void RequestMeetingUserDelete(long meetingId, EmployeeNoType deleteEmployeeNo)
		{
			MeetingUserDeleteRequest meetingUserDeleteRequest = new()
			{
				MeetingId    = meetingId,
				DeleteEmployeeNo = deleteEmployeeNo
			};
			LogPacketSend(meetingUserDeleteRequest.ToString());
			NetworkManager.Instance.Send(meetingUserDeleteRequest, MessageTypes.MeetingUserDeleteRequest);
		}


		/// <summary>
		/// 커넥팅 초대 요청 거절
		/// </summary>
		/// <param name="meetingId"></param>
		public void RequestConnectingInviteReject(long meetingId, string employeeNo)
		{
			MeetingInviteRejectRequest meetingInviteRejectRequest = new()
			{
				MeetingId  = meetingId,
				EmployeeNo = employeeNo,
			};
			LogPacketSend(meetingInviteRejectRequest.ToString());
			NetworkManager.Instance.Send(meetingInviteRejectRequest, MessageTypes.MeetingInviteRejectRequest);
		}
#endregion // MeetingRoom

#region Web API
#region Common
		public async UniTask<ResponseMeetingInfo> RequestMeetingInfoAsync(long meetingId, DelegateResponseMeetingInfo onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new ServiceComponents.MeetingIdRequest
			{
				MeetingId = meetingId,
			};
			LogPacketSend(request.ToString());
			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingMeetingInfo(request);
			var error = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				// TODO : Error 처리
				switch (error.OfficeResultCode)
				{
					// 이미 회의가 취소된 경우
					// case ErrorCode.PassedMeetingReadyTime:
					// 	UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCanceled_Toast"));
					// 	break;
					// 내가 회의에서 제외된 경우
					// case ErrorCode.NotExistsMember:
					// 	UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_ExcludedParticipants_Toast"));
					// 	break;
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				if (response?.Value?.Data == null)
				{
					C2VDebug.LogError("Data is null!");
					return response;
				}

				response.Value.Data.StartDateTime = response.Value.Data.StartDateTime.ToLocalTime();
				response.Value.Data.EndDateTime   = response.Value.Data.EndDateTime.ToLocalTime();
				onResponse?.Invoke(response);
			}

			return response;
		}
		public async UniTask<ResponseMeetingInviteCancel> RequestMeetingInviteCancelAsync(long meetingId, long cancelAccountId, DelegateResponseMeetingInviteCancel onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new Components.InviteCancelRequest
			{
				MeetingId       = meetingId,
				CancelAccountId = cancelAccountId,
			};
			LogPacketSend(request.ToString());

			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingInviteCancel(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
				switch (error.OfficeResultCode)
				{
					// 이미 수락되어 참여중인 경우
					//case Components.OfficeHttpResultCode.MeetingAlreadyInviteUser:
					//	UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCompleted_Toast"));
					//	break;
					// 이미 거절한 경우
					//case ErrorCode.MeetingNotExistsWaitlist:
					//case Components.OfficeHttpResultCode.NotExistMember:
					//	UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCompleted_Toast"));
					//	break;
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}
#endregion
#region ConnectingApp
		public async UniTask<ResponseReservation> RequestReservationAsync(MeetingInfoType meetingInfo, Components.GroupAssetType paymentType, DelegateResponseReservation onResponse, DelegateErrorResponse onError = null)
		{
			meetingInfo.StartDateTime = meetingInfo.StartDateTime.ToUniversalTime();
			meetingInfo.EndDateTime = meetingInfo.EndDateTime.ToUniversalTime();

			var request = new ServiceComponents.ReservationRequest
			{
				Meeting              = meetingInfo,
				UseAssetType         = paymentType,
			};
			LogPacketSend(meetingInfo.ToString());
			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingReservation(request);
			var error = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				// TODO : Error 처리
				switch (error.OfficeResultCode)
				{
					//case Components.OfficeHttpResultCode.MeetingBandwidthLevel4:
					//	UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_UserList_Invitation_CantEnter_Toast"));
					//	break;
					//case Components.OfficeHttpResultCode.MeetingAssetNotEnough:
					//	UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_Payment_NotEnoughFreePass_Toast"));
					//	break;
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}

		public async UniTask<ResponseMyList> RequestMyListAsync(DateTime startTime, DateTime endTime, DelegateResponseMyList onResponse = null, DelegateErrorResponse onError = null)
		{
			startTime.SetZeroHms();

			endTime = endTime.AddDays(1);
			endTime.SetZeroHms();

			var request = new ServiceComponents.MyListRequest
			{
				StartDateTime = startTime.ToUniversalTime(),
				EndDateTime = endTime.ToUniversalTime(),
			};
			LogPacketSend(request.ToString());
			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingMyList(request);
			var error = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				// TODO : Error 처리
				switch (error.OfficeResultCode)
				{
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				if (response.Value.Data == null)
				{
					C2VDebug.LogError("Data is null!");
					return response;
				}
				foreach (var meetingInfo in response.Value.Data)
				{
					meetingInfo.StartDateTime = meetingInfo.StartDateTime.ToLocalTime();
					meetingInfo.EndDateTime   = meetingInfo.EndDateTime.ToLocalTime();
				}

				onResponse?.Invoke(response);
			}

			return response;
		}

		private void RestoreMoveInput()
		{
			PlayerController.Instance.SetStopAndCannotMove(false);
			User.Instance.RestoreStandBy();
		}

		public async UniTask<ResponseRoomJoin> RequestRoomJoinAsync(long meetingId, DelegateResponseRoomJoin onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new ServiceComponents.RoomJoinRequest
			{
				MeetingId        = meetingId,
				CurrentServiceId = User.Instance.CurrentServiceType,
				DeviceType       = ServiceComponents.DeviceType.C2VClient,
				CurrentFieldId   = MapController.Instance.FieldID,
			};
			LogPacketSend(request.ToString());
			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingRoomJoin(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				RestoreMoveInput();
				return null;
			}

			if (error.HasError)
			{
				switch (error.OfficeResultCode)
				{
					// 회의가 종료됐을 경우
					//case Components.OfficeHttpResultCode.MeetingEnd:
					//	UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Reservation_AlreadyConnectingDone_Toast"));
					//	break;
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}

				RestoreMoveInput();
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}

		public async UniTask<ResponseRoomJoin> RequestGuestRoomJoinAsync(long meetingId, string guestName, DelegateResponseRoomJoin onResponse = null, DelegateErrorResponse onError = null)
		{
			// [게스트] 1. 여기서 자신의 게스트 닉네임 설정
			if (User.Instance.CurrentUserData is OfficeUserData userData)
				userData.GuestName = guestName;

			var request = new ServiceComponents.RoomJoinRequest
			{
				MeetingId        = meetingId,
				GuestMemberName  = guestName, // [게스트] 3. 여기서 자신의 게스트 닉네임을 서버로 전송
				CurrentServiceId = User.Instance.CurrentServiceType,
				DeviceType       = ServiceComponents.DeviceType.C2VClient,
				CurrentFieldId   = MapController.Instance.FieldID,
			};
			LogPacketSend(request.ToString());
			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingRoomJoin(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				switch (error.OfficeResultCode)
				{
					// 회의가 종료됐을 경우
					//case Components.OfficeHttpResultCode.MeetingEnd:
					//	UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Reservation_AlreadyConnectingDone_Toast"));
					//	break;
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}

		public async UniTask<ResponseReservationCancel> RequestMeetingReservationCancelAsync(long meetingId, DelegateResponseReservationCancel onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new RequestReservationCancel
			{
				MeetingId       = meetingId,
			};
			LogPacketSend(request.ToString());

			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingReservationCancel(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
				switch (error.OfficeResultCode)
				{
					/*// 예약 취소에 실패한 경우
					case ErrorCode.CancelReservationFail:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Reservation_AlreadyStartCantCancel_Toast"));
						break;*/
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}

		public async UniTask<ResponseReservationChange> RequestMeetingReservationChangeAsync(MeetingInfoType meetingInfo, DelegateResponseReservationChange onResponse = null, DelegateErrorResponse onError = null)
		{
			meetingInfo.StartDateTime = meetingInfo.StartDateTime.ToUniversalTime();
			meetingInfo.EndDateTime   = meetingInfo.EndDateTime.ToUniversalTime();

			var request = new Components.ReservationChangeRequest
			{
				Meeting = meetingInfo,
			};
			LogPacketSend(request.ToString());

			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingReservationChange(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
				switch (error.OfficeResultCode)
				{
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}

		public async UniTask<ResponseGuestCheck> RequestGuestCheckAsync(string meetingCode, DelegateResponseGuestCheck onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new Components.GuestRequest
			{
				MeetingCode = meetingCode,
			};
			LogPacketSend(request.ToString());

			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingGuestCheck(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
				switch (error.OfficeResultCode)
				{
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}
#endregion

#region Inquiry
		/// <summary>
		/// MeetingCode로 검색
		/// </summary>
		public async UniTask<ResponseSearchByMeetingCode> RequestSearchByMeetingCodeAsync(string meetingCode, DelegateResponseSearchByMeetingCode onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new Components.SearchByMeetingCodeRequest
			{
				MeetingCode = meetingCode,
			};
			LogPacketSend(request.ToString());
			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingSearchByMeetingCode(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
				switch (error.OfficeResultCode)
				{
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				if (response.Value.Data != null)
				{
					response.Value.Data.StartDateTime = response.Value.Data.StartDateTime.ToLocalTime();
					response.Value.Data.EndDateTime   = response.Value.Data.EndDateTime.ToLocalTime();
				}

				onResponse?.Invoke(response);
			}

			return response;
		}

		/// <summary>
		/// 조건으로 검색
		/// </summary>
		public async UniTask<ResponseSearchByDetail> RequestSearchByDetailAsync(DateTime startDateTime, DateTime endDateTime, string meetingName, MemberIdType organizerMemberId, MemberIdType memberId,
		                                                                        bool upcomingOrOngoing, DelegateResponseSearchByDetail onResponse = null,
		                                                                        DelegateErrorResponse onError = null)
		{
			startDateTime.SetZeroHms();

			var endTime = endDateTime.AddDays(1);
			endTime.SetZeroHms();

			var request = new Components.SearchByDetailRequest
			{
				StartDateTime        = startDateTime.ToUniversalTime(),
				EndDateTime          = endTime.ToUniversalTime(),
				MeetingName          = meetingName,
				OrganizerAccountId   = organizerMemberId,
				ParticipantAccountId = memberId,
				UpcomingOrOngoing    = upcomingOrOngoing
			};

			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingSearchByDetail(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
				switch (error.OfficeResultCode)
				{
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				foreach (var meeting in response.Value.Data)
				{
					meeting.StartDateTime = meeting.StartDateTime.ToLocalTime();
					meeting.EndDateTime   = meeting.EndDateTime.ToLocalTime();
				}
				onResponse?.Invoke(response);
			}

			return response;
		}

		/// <summary>
		/// 커넥팅 참여 요청
		/// </summary>
		/// <param name="meetingId"></param>
		public async UniTask<ResponseAttendance> RequestMeetingAttendanceAsync(long meetingId, DelegateResponseAttendance onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new Components.AttendanceRequest()
			{
				MeetingId       = meetingId,
			};
			LogPacketSend(request.ToString());
			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingAttendance(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
				switch (error.OfficeResultCode)
				{
					/*// 요청 보냈는데 유저 수가 다 찼을 경우
					case ErrorCode.OverUserCount:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Reservation_FullParticipants_Toast"));
						break;
					// 요청 보냈는데 해당 회의가 종료되었을 경우
					case ErrorCode.NotMeetingStartReadyTimeBetweenEndReadyTime:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Reservation_AlreadyConnectingDone_Toast"));
						break;
					// 요청 보냈는데 해당 회의가 취소된 회의인 경우
					case ErrorCode.PassedMeetingReadyTime:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCanceled_Toast"));
						break;
					// 요청 보냈는데 이미 초대를 받은 경우
					case ErrorCode.MeetingAlreadyInviteUser:
						// 이미 완료된 사항입니다
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCompleted_Toast"));
						break;
					*/
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}

		/// <summary>
		/// 커넥팅 참여 취소 요청
		/// </summary>
		/// <param name="meetingId"></param>
		public async UniTask<ResponseAttendanceCancel> RequestMeetingAttendanceCancelAsync(long meetingId, DelegateResponseAttendanceCancel onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new RequestAttendanceCancel()
			{
				MeetingId       = meetingId,
			};
			LogPacketSend(request.ToString());
			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingAttendanceCancel(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
				switch (error.OfficeResultCode)
				{
					/*// 요청 취소 했을 때, 회의가 종료됐을 경우
					case ErrorCode.NotMeetingStartReadyTimeBetweenEndReadyTime:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Reservation_AlreadyConnectingDone_Toast"));
						break;
					// 요청 취소 했을 때, 회의가 취소된 경우
					case ErrorCode.PassedMeetingReadyTime:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCanceled_Toast"));
						break;
					// 요청 취소 했을 때, 이미 주최자가 수락한 경우
					case ErrorCode.AlreadyJoinInMeeting:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCompleted_Toast"));
						break;
					// 요청 취소 했을 때, 이미 주최자가 거절한 경우
					case ErrorCode.MeetingAlreadyInviteUser:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCompleted_Toast"));
						break;
					*/
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}
#endregion
#region InConnecting
		public async UniTask<ResponseRoomLeave> RequestRoomLeaveAsync(long meetingId, string roomId, DelegateErrorResponse onError = null)
		{
			var request = new ServiceComponents.RoomLeaveRequest
			{
				MeetingId  = meetingId,
				RoomId     = roomId,
				DeviceType = Components.DeviceType.C2VClient,
			};
			LogPacketSend(request.ToString());
			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingRoomLeave(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				RestoreMoveInput();
				return null;
			}

			if (error.HasError)
			{
				switch (error.OfficeResultCode)
				{
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}

				RestoreMoveInput();
			}
			else
			{
				//onResponse?.Invoke(response);
			}

			return response;
		}
		/// <summary>
		/// 권한 변경 요청
		/// </summary>
		/// <param name="meetingId"></param>
		/// <param name="userInfo"></param>
		public async UniTask<ResponseAuthorityChange> RequestMeetingAuthorityChangeAsync(long meetingId, MeetingUserInfoType userInfo, DelegateResponseAuthorityChange onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new Components.AuthorityChangeRequest
			{
				MeetingId     = meetingId,
				MeetingMember = userInfo,
			};
			LogPacketSend(request.ToString());
			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingAuthorityChange(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
				switch (error.OfficeResultCode)
				{
					/*// 해당 유저가 커넥팅 내부에 존재하지 않을 경우
					case ErrorCode.MeetingNotInMeeting:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_UserList_NotCurrentlyParticipants_Toast"));
						break;
					// 해당 유저가 이미 강퇴되었을 경우
					case ErrorCode.NotExistsMember:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_UserList_NotCurrentlyParticipants_Toast"));
						break;
					// 주최자가 아닌 경우
					case ErrorCode.NotOrganizer:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Common_NotOrganizer_Popup_Text"));
						break;*/
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}

		/// <summary>
		/// 주최자가 커넥팅 참여 요청한 사람에 대한 수락
		/// </summary>
		/// <param name="meetingId"></param>
		/// <param name="waitAccountId"></param>
		public async UniTask<ResponseWaitListAccept> RequestMeetingWaitListAcceptAsync(long meetingId, long waitAccountId, DelegateResponseWaitListAccept onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new Components.WaitListAcceptRequest
			{
				MeetingId     = meetingId,
				WaitAccountId = waitAccountId,
			};
			LogPacketSend(request.ToString());
			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingWaitListAccept(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
				switch (error.OfficeResultCode)
				{
					/*// 이미 수락되어 참여중인 경우
					case ErrorCode.MeetingAlreadyInviteUser:
					UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCompleted_Toast"));
					break;
					// 이미 참여 요청을 취소한 경우
					case ErrorCode.MeetingNotExistsWaitlist:
					UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCompleted_Toast"));
					break;
					// 주최자가 아닌 경우
					case ErrorCode.NotOrganizer:
					UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Common_NotOrganizer_Popup_Text"));
					break;*/
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}

		/// <summary>
		/// 주최자가 커넥팅 참여 요청한 사람에 대한 거절
		/// </summary>
		/// <param name="meetingId"></param>
		/// <param name="waitAccountId"></param>
		public async UniTask<ResponseWaitListReject> RequestMeetingWaitListReject(long meetingId, long waitAccountId, DelegateResponseWaitListReject onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new Components.WaitListAcceptRequest
			{
				MeetingId     = meetingId,
				WaitAccountId = waitAccountId,
			};
			LogPacketSend(request.ToString());
			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingWaitListReject(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
				switch (error.OfficeResultCode)
				{
					/*// 이미 수락되어 참여중인 경우
					case ErrorCode.MeetingAlreadyInviteUser:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCompleted_Toast"));
						break;
					// 이미 참여 요청을 취소한 경우
					case ErrorCode.MeetingNotExistsWaitlist:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCompleted_Toast"));
						break;
					// 주최자가 아닌 경우
					case ErrorCode.NotOrganizer:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Common_NotOrganizer_Popup_Text"));
						break;*/
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}

		/// <summary>
		/// 모두 나가기(커넥팅 강제 종료) 기능
		/// </summary>
		/// <param name="meetingId"></param>
		public async UniTask<ResponseMeetingEnd> RequestMeetingEndAsync(long meetingId, DelegateResponseMeetingEnd onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new RequestMeetingEnd
			{
				MeetingId = meetingId,
			};

			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingEnd(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				switch (error.OfficeResultCode)
				{
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}

		/// <summary>
		/// 커넥팅 초대 (주최자 -> 참가자)
		/// </summary>
		/// <param name="meetingId"></param>
		/// <param name="meetingMembers"></param>
		/// <param name="onResponse"></param>
		/// <param name="onError"></param>
		public async UniTask<ResponseMeetingInvite> RequestMeetingInviteAsync(long meetingId, List<Components.MeetingMemberEntity> meetingMembers, DelegateResponseMeetingInvite onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new Components.InviteRequest
			{
				MeetingId = meetingId,
				MeetingMembers = meetingMembers.ToArray(),
			};
			LogPacketSend(request.ToString());

			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingInvite(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				switch (error.OfficeResultCode)
				{
					// 이미 초대된 유저일 경우
					case Components.OfficeHttpResultCode.MeetingAlreadyInviteUser:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCompleted_Toast"));
						break;
					// 참여 인원 수 초과로 초대가 불가능할 경우
					//case
					//	UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Reservation_FullParticipants_Toast"));
					//	break;
					// 권한이 주최자가 아닐 경우
					//case ErrorCode.NotOrganizer:
					//	UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Common_NotOrganizer_Popup_Text"));
					//	OnClick_CloseButton();
					//	break;
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}

		/// <summary>
		/// 해당 유저 내보내기 기능
		/// </summary>
		/// <param name="meetingId"></param>
		/// <param name="forcedOutAccountId"></param>
		public async UniTask<ResponseForcedOut> RequestMeetingForcedOutAsync(long meetingId, long forcedOutAccountId, DelegateResponseForcedOut onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new Components.ForcedOutRequest
			{
				MeetingId          = meetingId,
				ForcedOutAccountId = forcedOutAccountId,
			};
			LogPacketSend(request.ToString());

			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingForcedOut(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				switch (error.OfficeResultCode)
				{
					/*// 해당 유저가 커넥팅 내부에 존재하지 않을 경우
					case ErrorCode.MeetingNotInMeeting:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_UserList_NotCurrentlyParticipants_Toast"));
						break;
					// 해당 유저가 이미 강퇴되었을 경우
					case ErrorCode.NotExistsMember:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_UserList_NotCurrentlyParticipants_Toast"));
						break;
					// 주최자가 아닌 경우
					case ErrorCode.NotOrganizer:
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Common_NotOrganizer_Popup_Text"));
						break;*/
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}
		
		/// <summary>
		/// 종료 시간 연장
		/// </summary>
		/// <param name="meetingId"></param>
		/// <param name="extendMinute"></param>
		public async UniTask<ResponseExtendEnd> RequestExtendEndAsync(long meetingId, int extendMinute, Components.GroupAssetType paymentType, DelegateResponseExtendEnd onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new RequestExtendEnd
			{
				MeetingId    = meetingId,
				ExtendMinute = extendMinute,
				UseAssetType = paymentType,
			};
			var response = await Com2Verse.WebApi.Service.Api.Meeting.PostMeetingExtendEnd(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				ErrorString(0);
				return null;
			}

			if (error.HasError)
			{
				switch (error.OfficeResultCode)
				{
					default:
						ErrorString(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}
#endregion
#region Error Handling
		public class ErrorResponseData
		{
			public bool IsValidResponse;
			public bool HasError;
			public HttpStatusCode HttpStatusCode;
			public Components.OfficeHttpResultCode OfficeResultCode;

			public static ErrorResponseData InvalidResponse => new ErrorResponseData {IsValidResponse = false, HasError = false};
			public static ErrorResponseData SuccessResponse = new ErrorResponseData {IsValidResponse = true, HasError = false};
		}
		private ErrorResponseData OnResponseErrorHandling<T>(ResponseBase<T> response, Components.OfficeHttpResultCode? officeResultCode, DelegateErrorResponse onErrorResponse = null)
		{
			if (response == null)
			{
				var errorResponse = ErrorResponseData.InvalidResponse;
				onErrorResponse?.Invoke(errorResponse);
				return errorResponse; // TODO : 요청 실패 (네트워크 불량 등...)
			}

			if (response.StatusCode == HttpStatusCode.OK && officeResultCode == Components.OfficeHttpResultCode.Success)
				return ErrorResponseData.SuccessResponse;

			var error = new ErrorResponseData
			{
				IsValidResponse = true,
				HasError = true,
				HttpStatusCode = response.StatusCode,
				OfficeResultCode = officeResultCode ?? Components.OfficeHttpResultCode.Fail,
			};

			onErrorResponse?.Invoke(error);
			return error;
		}

		private void ErrorString(Components.OfficeHttpResultCode officeResultCode)
		{
			NetworkUIManager.Instance.ShowWebApiErrorMessage(officeResultCode);
			// var errorString = Localization.Instance.GetOfficeErrorString((int)officeResultCode);
			// if (string.IsNullOrWhiteSpace(errorString))
			// {
			// 	UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_Common_UnknownProblemError_Popup_Text", (int)officeResultCode));
			// 	C2VDebug.LogError("ErrorCode : [" + (int)officeResultCode + "] : " + officeResultCode);
			// }
			// else
			// {
			// 	UIManager.Instance.SendToastMessage(errorString);
			// }
		}
#endregion // Error Handling
#endregion // Web API
	}
}
