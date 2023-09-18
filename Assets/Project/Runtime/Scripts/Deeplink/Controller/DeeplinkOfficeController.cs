/*===============================================================
* Product:		Com2Verse
* File Name:	DeeplinkOfficeController.cs
* Developer:	mikeyid77
* Date:			2023-08-16 18:43
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Net;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.WebApi.Service;
using UnityEngine;
using JetBrains.Annotations;

namespace Com2Verse.Deeplink
{
	[UsedImplicitly]
	[Service(eServiceType.OFFICE)]
	public sealed class DeeplinkOfficeController : DeeplinkBaseController
	{
		public override void Initialize()
		{
			Name = nameof(DeeplinkOfficeController);
			TargetList?.TryAdd(0, LoadTarget<DeeplinkTeamRoom>());
			TargetList?.TryAdd(1, LoadTarget<DeeplinkMeetingRoom>());
		}

		protected override void TryCheckParam(EngagementInfo info) => TryCheckParamAsync(info);

		private async void TryCheckParamAsync(EngagementInfo info)
		{
			// TODO : 추후 DeeplinkType으로 구분하는 플로우로 변경

			if (info?.Param == "0")
				TargetList?[0]?.InvokeAsync(info.Param, FinishInvokeAction);
			else
			{
				var request = new Components.DeeplinkParamParseRequest
				{
					DeeplinkType  = Components.DeeplinkType.Meeting,
					DeeplinkParam = info?.Param
				};

				var response = await Api.Common.PostCommonDeeplinkParamParse(request);
				if (response == null)
				{
					C2VDebug.LogErrorCategory("Deeplink", $"DeeplinkParamParseResponse is NULL");
					ShowWebApiErrorMessage(0);
				}
				else if (response.StatusCode == HttpStatusCode.OK)
				{
					if (response.Value == null)
					{
						C2VDebug.LogErrorCategory("Deeplink", $"DeeplinkParamParseResponse Value is NULL");
						ShowWebApiErrorMessage(0);
					}
					else if (response.Value.Data == null)
					{
						C2VDebug.LogErrorCategory("Deeplink", $"DeeplinkParamParseResponse Data is NULL");
						ShowWebApiErrorMessage(0);
					}
					else
					{
						switch (response.Value.Code)
						{
							case Components.OfficeHttpResultCode.Success:
								var deeplinkValue = JsonUtility.FromJson<DeeplinkOfficeMeeting>(response.Value.Data.DeeplinkValue);
								if (deeplinkValue == null)
								{
									C2VDebug.LogErrorCategory("Deeplink", $"DeeplinkParam Json Error");
									ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
								}
								else
								{
									TargetList?[1]?.InvokeAsync(deeplinkValue.MeetingId, FinishInvokeAction);
								}
								break;
							default:
								C2VDebug.LogErrorCategory("Deeplink", $"DeeplinkParamParse Error : {response.Value.Code.ToString()}");
								ShowWebApiErrorMessage(response.Value.Code);
								break;
						}
					}
				}
				else
				{
					C2VDebug.LogErrorCategory("Deeplink", $"DeeplinkParamParse Fail : {response.StatusCode.ToString()}");
					ShowHttpErrorMessage(response.StatusCode);
				}
			}
		}

#region Utils
		private class DeeplinkOfficeMeeting
		{
			public string MeetingId;
		}
#endregion
	}
}
