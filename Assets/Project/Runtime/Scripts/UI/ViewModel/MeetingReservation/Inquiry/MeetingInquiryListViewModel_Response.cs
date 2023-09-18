/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingSearchViewModel.cs
* Developer:	ksw
* Date:			2023-03-07 11:52
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.UI;
using Cysharp.Text;
using Protocols;
using Protocols.OfficeMeeting;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using MeetingStatus = Com2Verse.WebApi.Service.Components.MeetingStatus;
using ResponseSearchByMeetingCode = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityResponseFormat>;
using ResponseSearchByDetail = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingEntityIEnumerableResponseFormat>;
using ResponseAttendance = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.AttendanceResponseResponseFormat>;
using ResponseAttendanceCancel = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.MeetingNullResponseResponseFormat>;

namespace Com2Verse
{
	public sealed partial class MeetingInquiryListViewModel
	{
		private void OnResponseSearchByMeetingCode(ResponseSearchByMeetingCode response)
		{
			MeetingInquiryCollection.Reset();
			ShowingInquiryCollection.Reset();

			if (response.Value.Data != null)
			{
				switch (response.Value.Data.MeetingStatus)
				{
					case MeetingStatus.MeetingCancelAfterDelete:
						C2VDebug.Log("Search Result is Canceled!");
						IsResultNull = true;
						break;
					case MeetingStatus.MeetingExpired:
					{
						C2VDebug.Log("Search Result is Expired!");
						IsResultNull = true;
						foreach (var userInfo in response.Value.Data.MeetingMembers)
						{
							if (userInfo.AccountId == User.Instance.CurrentUserData.ID)
							{
								AddItem();
								C2VDebug.Log("Search Result is Expired! But is MyConnecting");
								break;
							}
						}
						break;
					}
					default:
					{
						AddItem();
						break;
					}
				}
			}
			else
			{
				C2VDebug.Log("Search Result is null!");
				IsResultNull = true;
			}

			void AddItem()
			{
				var item = new MeetingInquiryViewModel(response.Value.Data, _onClickMoveDetailPage, _onClickInviteRequest, CloseInquiryPopup);
				MeetingInquiryCollection.AddItem(item);
				ShowingInquiryCollection.AddItem(item);
				IsResultNull = false;
			}
		}

		private void OnResponseSearchByDetail(ResponseSearchByDetail response)
		{
			MeetingInquiryCollection.Reset();
			ShowingInquiryCollection.Reset();
			if (response.Value.Data.Length != 0)
			{
				foreach (var meetingInfo in response.Value.Data)
				{
					switch (meetingInfo.MeetingStatus)
					{
						case MeetingStatus.MeetingCancelAfterDelete:
							C2VDebug.Log("Search Result is Canceled!");
							break;
						case MeetingStatus.MeetingExpired:
							C2VDebug.Log("Search Result is Expired!");
							if (User.Instance.CurrentUserData is not OfficeUserData userData) return;
							foreach (var member in meetingInfo.MeetingMembers)
							{
								if (member.AccountId == userData.ID)
								{
									AddItem(meetingInfo);
									C2VDebug.Log("Search Result is Expired! But is MyConnecting");
									break;
								}
							}
							break;
						default:
							AddItem(meetingInfo);
							break;
					}
				}
				_maxPage = (MeetingInquiryCollection.CollectionCount - 1) / _numberOfModelOnPage + 1;
				_currentPage = 1;
				RefreshInquiryPage();
				IsResultNull = false;

				void AddItem(MeetingInfoType meetingInfo)
				{
					var item = new MeetingInquiryViewModel(meetingInfo, _onClickMoveDetailPage, _onClickInviteRequest, CloseInquiryPopup);
					MeetingInquiryCollection.AddItem(item);
				}
			}
			else
			{
				C2VDebug.Log("Search Result is null!");
				IsResultNull = true;
			}
		}

		private void OnResponseAttendance(ResponseAttendance meetingAttendanceResponse)
		{
			if (_searchTypeIsDetail)
				OnClickSearch();
			else
				OnClickSearchMeetingCode();
		}

		private void OnResponseMeetingAttendanceCancel(ResponseAttendanceCancel meetingInviteResponse)
		{
			if (_searchTypeIsDetail)
				OnClickSearch();
			else
				OnClickSearchMeetingCode();
		}
		
		private void OnResponseMeetingInviteReject(MeetingInviteRejectResponse meetingInviteResponse)
		{
			Network.Communication.PacketReceiver.Instance.MeetingInviteRejectResponse -= OnResponseMeetingInviteReject;
			Network.Communication.PacketReceiver.Instance.ErrorCodeResponse           -= OnErrorConnectingInviteReject;

			if (_searchTypeIsDetail)
				OnClickSearch();
			else
				OnClickSearchMeetingCode();

			UIManager.Instance.HideWaitingResponsePopup();
			UIManager.Instance.SendToastMessage("UI_ConnectingApp_Card_DeclineInvitation_Toast");
		}

		private void OnErrorConnectingInviteReject(MessageTypes messageTypes, ErrorCode errorCode)
		{
			if (messageTypes != MessageTypes.MeetingInviteRejectResponse)
			{
				C2VDebug.LogError("Wrong MessageType ErrorCode!");
				return;
			}
			Network.Communication.PacketReceiver.Instance.MeetingInviteRejectResponse -= OnResponseMeetingInviteReject;
			Network.Communication.PacketReceiver.Instance.ErrorCodeResponse           -= OnErrorConnectingInviteReject;

			switch (errorCode)
			{
				// 요청 거절 했을 때, 회의가 종료됐을 경우
				case ErrorCode.NotMeetingStartReadyTimeBetweenEndReadyTime:
					UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Reservation_AlreadyConnectingDone_Toast"));
					break;
				// 요청 거절 했을 때, 회의가 취소된 경우
				case ErrorCode.PassedMeetingReadyTime:
					UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCanceled_Toast"));
					break;
				// 요청 거절 했을 때, 이미 주최자가 초대 취소한 경우 또는 이미 자신이 수락하거나 거절했을 경우
				case ErrorCode.MeetingNotExistsWaitlist:
					UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_ConnectingApp_Detail_AlreadyCompleted_Toast"));
					break;
				default:
					UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_Common_UnknownProblemError_Popup_Text", ZString.Format("{0} : {1}", "ErrorCode", (int)errorCode)));
					C2VDebug.LogError("OnErrorConnectingInviteReject ErrorCode : " + errorCode);
					break;
			}

			if (_searchTypeIsDetail)
				OnClickSearch();
			else
				OnClickSearchMeetingCode();
			UIManager.Instance.HideWaitingResponsePopup();
		}
	}
}
