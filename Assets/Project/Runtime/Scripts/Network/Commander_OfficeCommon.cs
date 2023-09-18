/*===============================================================
* Product:		Com2Verse
* File Name:	Commander_OfficeCommon.cs
* Developer:	ksw
* Date:			2023-08-14 11:03
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#region Request
using RequestGetInfoByTeamAttribute = Com2Verse.WebApi.Service.Components.GetInfoByTeamAttributeRequest;
using RequestLoginShareOffice = Com2Verse.WebApi.Service.Components.LoginShareOfficeRequest;
using RequestDeeplinkParamGet = Com2Verse.WebApi.Service.Components.DeeplinkParamGetRequest;
using RequestDeeplinkParamParse = Com2Verse.WebApi.Service.Components.DeeplinkParamParseRequest;
#endregion

#region Response
using ResponseGetInfoByTeamAttribute = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.GetInfoByTeamAttributeResponseResponseFormat>;
using ResponseLoginConsole = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.LoginConsoleResponseResponseFormat>;
using ResponseLoginShareOffice = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.LoginShareOfficeResponseResponseFormat>;
using ResponseDeeplinkParamGet = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.DeeplinkParamGetResponseResponseFormat>;
using ResponseDeeplinkParamParse = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.DeeplinkParamParseResponseResponseFormat>;
#endregion

#region DelegateResponse
using DelegateResponseGetInfoByTeamAttribute = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.GetInfoByTeamAttributeResponseResponseFormat>>;
using DelegateResponseLoginConsole = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.LoginConsoleResponseResponseFormat>>;
using DelegateResponseLoginShareOffice = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.LoginShareOfficeResponseResponseFormat>>;
using DelegateResponseDeeplinkParamGet = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.DeeplinkParamGetResponseResponseFormat>>;
using DelegateResponseDeeplinkParamParse = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.DeeplinkParamParseResponseResponseFormat>>;
#endregion

using DelegateErrorResponse = System.Action<Com2Verse.Network.Commander.ErrorResponseData>;

using Com2Verse.UI;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Network
{
	public sealed partial class Commander
	{
		public async UniTask<ResponseGetInfoByTeamAttribute> RequestGetInfoByTeamAttribute(long groupId, long teamId = 0, string spaceId = null, DelegateResponseGetInfoByTeamAttribute onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new RequestGetInfoByTeamAttribute
			{
				GroupId        = groupId,
				TeamId         = teamId,
				SpaceId        = spaceId,
				IsJoinAreaChat = false,
			};

			var response = await Com2Verse.WebApi.Service.Api.Common.PostCommonGetInfoByTeamAttribute(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
				return null;
			}

			if (error.HasError)
			{
				switch (error.OfficeResultCode)
				{
					default:
						NetworkUIManager.Instance.ShowWebApiErrorMessage(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}

		public async UniTask<ResponseLoginConsole> RequestLoginConsole(DelegateResponseLoginConsole onResponse = null, DelegateErrorResponse onError = null)
		{
			var response = await Com2Verse.WebApi.Service.Api.Common.PostCommonLoginConsole();
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
				return null;
			}

			if (error.HasError)
			{
				switch (error.OfficeResultCode)
				{
					default:
						NetworkUIManager.Instance.ShowWebApiErrorMessage(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}

		public async UniTask<ResponseLoginShareOffice> RequestLoginShareOffice(DelegateResponseLoginShareOffice onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new RequestLoginShareOffice
			{
				DeviceType = Components.DeviceType.C2VClient,
			};

			var response = await Com2Verse.WebApi.Service.Api.Common.PostCommonLoginShareOffice(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
				return null;
			}

			if (error.HasError)
			{
				switch (error.OfficeResultCode)
				{
					default:
						NetworkUIManager.Instance.ShowWebApiErrorMessage(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}

		public async UniTask<ResponseDeeplinkParamGet> RequestDeeplinkParamGet(Components.DeeplinkType deeplinkType, string deeplinkValue, DelegateResponseDeeplinkParamGet onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new RequestDeeplinkParamGet
			{
				DeeplinkType  = deeplinkType,
				DeeplinkValue = deeplinkValue,
			};

			var response = await Com2Verse.WebApi.Service.Api.Common.PostCommonDeeplinkParamGet(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
				return null;
			}

			if (error.HasError)
			{
				switch (error.OfficeResultCode)
				{
					default:
						NetworkUIManager.Instance.ShowWebApiErrorMessage(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}

		public async UniTask<ResponseDeeplinkParamParse> RequestDeeplinkParamParse(Components.DeeplinkType deeplinkType, string deeplinkParam, DelegateResponseDeeplinkParamParse onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new RequestDeeplinkParamParse
			{
				DeeplinkType  = deeplinkType,
				DeeplinkParam = deeplinkParam,
			};

			var response = await Com2Verse.WebApi.Service.Api.Common.PostCommonDeeplinkParamParse(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
				return null;
			}

			if (error.HasError)
			{
				switch (error.OfficeResultCode)
				{
					default:
						NetworkUIManager.Instance.ShowWebApiErrorMessage(error.OfficeResultCode);
						break;
				}
			}
			else
			{
				onResponse?.Invoke(response);
			}

			return response;
		}
	}
}
