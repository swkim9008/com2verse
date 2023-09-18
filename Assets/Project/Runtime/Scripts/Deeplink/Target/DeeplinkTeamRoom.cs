/*===============================================================
* Product:		Com2Verse
* File Name:	DeeplinkTeamRoom.cs
* Developer:	mikeyid77
* Date:			2023-08-17 15:03
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Net;
using Com2Verse.Chat;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.PlayerControl;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Deeplink
{
	public sealed class DeeplinkTeamRoom : DeeplinkBaseTarget
	{
		public override async void InvokeAsync(string param, Action finishInvoke)
		{
			C2VDebug.LogCategory("Deeplink", $"Invoke {nameof(DeeplinkTeamRoom)}");
			FinishInvokeAction = finishInvoke;

			var request = new Components.LoginShareOfficeRequest
			{
				DeviceType = Components.DeviceType.C2VClient
			};

			var response = await Api.Common.PostCommonLoginShareOffice(request);
			if (response == null)
			{
				C2VDebug.LogErrorCategory("Deeplink", $"ServiceLoginResponse is NULL");
				ShowWebApiErrorMessage(0);
			}
			else if (response.StatusCode == HttpStatusCode.OK)
			{
				if (response.Value == null)
				{
					C2VDebug.LogErrorCategory("Deeplink", $"ServiceLoginResponse Value is NULL");
					ShowWebApiErrorMessage(0);
				}
				else if (response.Value.Data == null)
				{
					C2VDebug.LogErrorCategory("Deeplink", $"ServiceLoginResponse Data is NULL");
					ShowWebApiErrorMessage(0);
				}
				else
				{
					switch (response.Value.Code)
					{
						case Components.OfficeHttpResultCode.Success:
							DataManager.Instance.SetData(response.Value.Data.OrganizationChart);
							MeetingReservationProvider.SetMeetingTemplates(response.Value.Data.AvailableMeetingTemplate);
							TryTeleportMyTeamSpaceAsync().Forget();
							break;
						case Components.OfficeHttpResultCode.EmptyMemberResult:
						case Components.OfficeHttpResultCode.CannotMoveUndefinedTeam:
							C2VDebug.LogWarningCategory("Deeplink", $"ServiceLogin Warning : {response.Value.Code.ToString()}");
							ShowWebApiErrorMessage(response.Value.Code);
							break;
						default:
							C2VDebug.LogErrorCategory("Deeplink", $"ServiceLogin Error : {response.Value.Code.ToString()}");
							ShowWebApiErrorMessage(response.Value.Code);
							break;
					}
				}
			}
			else
			{
				C2VDebug.LogErrorCategory("Deeplink", $"ServiceLogin Fail : {response.StatusCode.ToString()}");
				ShowHttpErrorMessage(response.StatusCode);
			}
		}

		private async UniTaskVoid TryTeleportMyTeamSpaceAsync()
		{
			// 이동중 OSR/Input 막기
			PlayerController.Instance.SetStopAndCannotMove(true);
			User.Instance.DiscardPacketBeforeStandBy();

			var serviceType = SceneManager.Instance.CurrentScene.ServiceType;
			var serviceId   = eServiceID.SAMPLE;
			switch (serviceType)
			{
				case eServiceType.WORLD:  serviceId = eServiceID.WORLD; break;
				case eServiceType.OFFICE: serviceId = eServiceID.SPAXE; break;
				case eServiceType.MICE:   serviceId = eServiceID.MICE;  break;
			}

			var request = new Components.WarpGroupMyTeamSpaceRequest()
			{
				CurrentServiceId = (long)serviceId
			};

			var response = await Api.Organization.PostWarpGroupMyTeamSpace(request);
			if (response == null)
			{
				C2VDebug.LogErrorCategory("Deeplink", $"WarpGroupMyTeamSpaceResponse is NULL");
				WebApiErrorInvoke(0);
			}
			else if (response.StatusCode == HttpStatusCode.OK)
			{
				if (response.Value == null)
				{
					C2VDebug.LogErrorCategory("Deeplink", $"WarpGroupMyTeamSpaceResponse Value is NULL");
					WebApiErrorInvoke(0);
				}
				else if (response.Value.Data == null)
				{
					C2VDebug.LogErrorCategory("Deeplink", $"WarpGroupMyTeamSpaceResponse Data is NULL");
					WebApiErrorInvoke(0);
				}
				else
				{
					switch (response.Value.Code)
					{
						case Components.OfficeHttpResultCode.Success:
							C2VDebug.LogCategory("Deeplink", $"Teleport to TeamRoom");
							ChatManager.Instance.SetAreaMove(response.Value.Data?.ChatGroupId);
							User.Instance.OnTeleportCompletion += OnTeleportCompletion;
							break;
						default:
							C2VDebug.LogErrorCategory("Deeplink", $"WarpGroupMyTeamSpace Error : {response.Value.Code.ToString()}");
							WebApiErrorInvoke(response.Value.Code);
							break;
					}
				}
			}
			else
			{
				C2VDebug.LogErrorCategory("Deeplink", $"WarpGroupMyTeamSpace Fail : {response.StatusCode.ToString()}");
				HttpErrorInvoke(response.StatusCode);
			}

			void WebApiErrorInvoke(Components.OfficeHttpResultCode code)
			{
				PlayerController.Instance.SetStopAndCannotMove(false);
				User.Instance.RestoreStandBy();
				ShowWebApiErrorMessage(code);
			}

			void HttpErrorInvoke(HttpStatusCode code)
			{
				PlayerController.Instance.SetStopAndCannotMove(false);
				User.Instance.RestoreStandBy();
				ShowHttpErrorMessage(code);
			}
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
