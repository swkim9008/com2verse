/*===============================================================
* Product:		Com2Verse
* File Name:	DeeplinkParser.cs
* Developer:	mikeyid77
* Date:			2023-08-16 18:38
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.WebApi.Service;
using hive;

namespace Com2Verse.Deeplink
{
	public static class DeeplinkParser
	{
		private static bool _engagementReady = false;

		public static void Initialize()
		{
			C2VDebug.LogCategory("Deeplink", $"Initialize Parser");
			_engagementReady = false;
		}

		public static void OnEngagementReady()
		{
			if (!_engagementReady)
			{
				_engagementReady = true;
				Boolean   isReady = true;
				ResultAPI ret     = Promotion.setEngagementReady(isReady);
				C2VDebug.LogCategory("Deeplink", $"setEngagementReady : {ret.toString()}");
			}
			else
			{
				C2VDebug.LogCategory("Deeplink", $"Already Set Engagement");
			}
		}

		public static void OnEngagementCB(ResultAPI result, EngagementEventType engagementEventType, EngagementEventState engagementEventState, JSONObject jsonObject)
		{
			if (!_engagementReady) return;
			if (engagementEventType == EngagementEventType.EVENT && engagementEventState == EngagementEventState.START)
			{
				string param = "";
				jsonObject?.GetField(ref param, "param");

				if (string.IsNullOrEmpty(param))
				{
					C2VDebug.LogErrorCategory("Deeplink", $"Wrong Param");
				}
				else
				{
					try
					{
						// param은 ABC 순서대로 들어옴
						// C2VDebug.LogCategory("QuickConnect", $"Param : {param}");
						var tempParam1    = param.Split($"&deeplink_param=");
						var targetString  = tempParam1?[1]?.Split($"&envi=");
						var tempParam2    = targetString?[1]?.Split($"&service=");
						var targetService = tempParam2?[1]?.Split($"&start_point=");

						var info = new EngagementInfo();
						info.ServiceType  = (eServiceType)Enum.Parse(typeof(eServiceType), targetService[0].ToUpper());
						info.DeeplinkType = Components.DeeplinkType.Meeting;
						info.Param        = targetString[0];
						info.IsCheat      = false;

						if (info.IsValid())
						{
							C2VDebug.LogCategory("Deeplink", $"Get {info.ServiceType.ToString()} Engagement (Target : {info.Param})");
							DeeplinkManager.InvokeEngagement(info);
						}
						else
						{
							C2VDebug.LogErrorCategory("Deeplink", $"Wrong Info");
						}
					}
					catch (Exception e)
					{
						C2VDebug.LogErrorCategory("Deeplink", $"{e}");
					}
				}
			}
			else
			{
				C2VDebug.LogCategory("Deeplink", $"Invalid Engagement : {engagementEventType.ToString()}({engagementEventState.ToString()})");
			}
		}

#region Cheat
		public static async void SetDeeplinkParamAsync(string type, string param)
		{
			C2VDebug.LogCategory("Deeplink", "Set Cheat Engagement");

			if (param == "0")
			{
				SendCheatMessage(type, param);
			}
			else
			{
				var request = new Components.DeeplinkParamGetRequest()
				{
					DeeplinkType  = Components.DeeplinkType.Meeting,
					DeeplinkValue = param
				};

				var response = await Api.Common.PostCommonDeeplinkParamGet(request);
				if (response?.Value?.Code == Components.OfficeHttpResultCode.Success)
				{
					SendCheatMessage(type, response.Value.Data?.DeeplinkParam);
				}
				else
				{
					C2VDebug.LogWarningCategory("Deeplink", $"DeeplinkParamGetResponse Fail");
				}
			}
		}

		private static void SendCheatMessage(string type, string param)
		{
			try
			{
				var info = new EngagementInfo();
				info.ServiceType  = (eServiceType)Enum.Parse(typeof(eServiceType), type.ToUpper());
				info.DeeplinkType = Components.DeeplinkType.Meeting;
				info.Param        = param;
				info.IsCheat      = true;

				if (info.IsValid())
				{
					C2VDebug.LogCategory("Deeplink", $"Get {info.ServiceType.ToString()} Cheat Engagement (Target : {info.Param})");
					DeeplinkManager.InvokeEngagement(info);
				}
				else
				{
					C2VDebug.LogErrorCategory("Deeplink", $"Wrong Info");
				}
			}
			catch (Exception e)
			{
				C2VDebug.LogErrorCategory("Deeplink", $"{e}");
			}
		}
#endregion
	}
}
