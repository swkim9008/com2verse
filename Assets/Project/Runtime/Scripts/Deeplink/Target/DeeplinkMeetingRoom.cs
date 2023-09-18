/*===============================================================
* Product:		Com2Verse
* File Name:	DeeplinkMeetingRoom.cs
* Developer:	mikeyid77
* Date:			2023-08-17 15:03
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Net;
using Com2Verse.Chat;
using Com2Verse.Communication;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.WebApi.Service;
using User = Com2Verse.Network.User;

namespace Com2Verse.Deeplink
{
	public sealed class DeeplinkMeetingRoom : DeeplinkBaseTarget
	{
		public override async void InvokeAsync(string param, Action finishInvoke)
		{
			C2VDebug.LogCategory("Deeplink", $"Invoke {nameof(DeeplinkMeetingRoom)}");
			FinishInvokeAction = finishInvoke;

			var meetingId = (long)Convert.ToInt32(param);
			var response  = await Commander.Instance.RequestMeetingInfoAsync(meetingId);
			if (response == null)
			{
				C2VDebug.LogErrorCategory("Deeplink", $"MeetingInfoResponse is NULL");
				ShowWebApiErrorMessage(0);
			}
			else if (response.StatusCode == HttpStatusCode.OK)
			{
				if (response.Value == null)
				{
					C2VDebug.LogErrorCategory("Deeplink", $"MeetingInfoResponse Value is NULL");
					ShowWebApiErrorMessage(0);
				}
				else if (response.Value.Data == null)
				{
					C2VDebug.LogErrorCategory("Deeplink", $"MeetingInfoResponse Data is NULL");
					ShowWebApiErrorMessage(0);
				}
				else
				{
					switch (response.Value.Code)
					{
						case Components.OfficeHttpResultCode.Success:
							CheckMeetingInfo(response.Value.Data);
							break;
						default:
							C2VDebug.LogErrorCategory("Deeplink", $"MeetingInfoResponse Error : {response.Value.Code.ToString()}");
							ShowWebApiErrorMessage(response.Value.Code);
							break;
					}
				}
			}
			else
			{
				C2VDebug.LogErrorCategory("Deeplink", $"MeetingInfoResponse Fail : {response.StatusCode.ToString()}");
				ShowHttpErrorMessage(response.StatusCode);
			}
		}

		private void CheckMeetingInfo(Components.MeetingEntity entity)
		{
			var canJoin = false;
			foreach (var meetingMember in entity.MeetingMembers)
			{
				if (meetingMember == null) continue;
				if (User.Instance.CurrentUserData.ID == meetingMember.AccountId)
				{
					canJoin = true;
					break;
				}
			}

			if (canJoin)
			{
				MeetingReservationProvider.SetMeetingInfo(entity);
				TryGetOrganizationChart(entity.MeetingId);
			}
			else
			{
				C2VDebug.LogWarningCategory("Deeplink", $"Invalid MeetingInfo");
				ShowWebApiErrorMessage(Components.OfficeHttpResultCode.MeetingNotExistsMember);
			}
		}

		private async void TryGetOrganizationChart(long meetingId)
		{
			var request = new Components.LoginShareOfficeRequest
			{
				DeviceType = Components.DeviceType.C2VClient
			};

			var response = await Api.Common.PostCommonLoginShareOffice(request);
			if (response == null)
			{
				C2VDebug.LogErrorCategory("Deeplink", $"OrganizationChartResponse is NULL");
				ShowWebApiErrorMessage(0);
			}
			else if (response.StatusCode == HttpStatusCode.OK)
			{
				if (response.Value == null)
				{
					C2VDebug.LogErrorCategory("Deeplink", $"OrganizationChartResponse Value is NULL");
					ShowWebApiErrorMessage(0);
				}
				else if (response.Value.Data == null)
				{
					C2VDebug.LogErrorCategory("Deeplink", $"OrganizationChartResponse Data is NULL");
					ShowWebApiErrorMessage(0);
				}
				else
				{
					switch (response.Value.Code)
					{
						case Components.OfficeHttpResultCode.Success:
							DataManager.Instance.SetData(response.Value.Data.OrganizationChart);
							MeetingReservationProvider.SetMeetingTemplates(response.Value.Data.AvailableMeetingTemplate);
							TryJoinChannelAsync(meetingId);
							break;
						case Components.OfficeHttpResultCode.EmptyMemberResult:
						case Components.OfficeHttpResultCode.CannotMoveUndefinedTeam:
							C2VDebug.LogWarningCategory("Deeplink", $"OrganizationChart Warning : {response.Value.Code.ToString()}");
							ShowWebApiErrorMessage(response.Value.Code);
							break;
						default:
							C2VDebug.LogErrorCategory("Deeplink", $"OrganizationChart Error : {response.Value.Code.ToString()}");
							ShowWebApiErrorMessage(response.Value.Code);
							break;
					}
				}
			}
			else
			{
				C2VDebug.LogErrorCategory("Deeplink", $"OrganizationChart Fail : {response.StatusCode.ToString()}");
				ShowHttpErrorMessage(response.StatusCode);
			}
		}

		private async void TryJoinChannelAsync(long meetingId)
		{
			await Commander.Instance.RequestRoomJoinAsync(
				meetingId, async onResponse =>
				{
					if (onResponse == null)
					{
						C2VDebug.LogErrorCategory("Deeplink", $"JoinChannelResponse is NULL");
						ShowWebApiErrorMessage(0);
					}
					else if (onResponse.Value == null)
					{
						C2VDebug.LogErrorCategory("Deeplink", $"JoinChannelResponse Value is NULL");
						ShowWebApiErrorMessage(0);
					}
					else if (onResponse.Value.Data == null)
					{
						C2VDebug.LogErrorCategory("Deeplink", $"JoinChannelResponse Data is NULL");
						ShowWebApiErrorMessage(0);
					}
					else
					{
						C2VDebug.LogCategory("Deeplink", $"Teleport to MeetingRoom : {meetingId.ToString()}");
						var memberModel = await DataManager.Instance.GetMyselfAsync();
						var extraInfo = new ExtraInfo
						{
							Uid      = User.Instance.CurrentUserData.ID.ToString()!,
							Job      = memberModel.Member.Level,
							Name     = memberModel.Member.MemberName,
							Position = memberModel.Member.Position,
							Team     = memberModel.TeamName,
							Token    = "",
						};
						ChannelManagerHelper.AddChannel(onResponse.Value.Data, MeetingReservationProvider.DisconnectRequestFromMediaChannel, extraInfo.ToString());
						MeetingReservationProvider.RoomId = onResponse.Value.Data.RoomId;
						ChatManager.Instance.SetAreaMove(onResponse.Value.Data.GroupId);
						User.Instance.OnTeleportCompletion += OnTeleportCompletion;
					}
				},
				onError =>
				{
					C2VDebug.LogErrorCategory("Deeplink", $"JoinChannelResponse Error : {onError?.OfficeResultCode.ToString()}");
					ShowWebApiErrorMessage(onError?.OfficeResultCode ?? Components.OfficeHttpResultCode.Fail);
				});
		}

#region Utils
		private void OnTeleportCompletion()
		{
			User.Instance.OnTeleportCompletion -= OnTeleportCompletion;
			FinishInvokeAction?.Invoke();
		}
#endregion
	}
}

