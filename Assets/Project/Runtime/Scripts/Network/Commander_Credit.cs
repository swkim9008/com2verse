/*===============================================================
* Product:		Com2Verse
* File Name:	Commander_Credit.cs
* Developer:	ksw
* Date:			2023-07-19 15:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Logger;
using Com2Verse.UI;
using Com2Verse.WebApi.Service;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using ServiceComponents = Com2Verse.WebApi.Service.Components;
using ResponseCreditInfo = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.CreditInfoResponseResponseFormat>;

using DelegateResponseCreditInfo = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.CreditInfoResponseResponseFormat>>;

using DelegateErrorResponse = System.Action<Com2Verse.Network.Commander.ErrorResponseData>;

namespace Com2Verse.Network
{
	public sealed partial class Commander
	{
		public async UniTask<ResponseCreditInfo> RequestGetCreditInfo(long groupId, Components.GroupAssetType groupAssetType = Components.GroupAssetType.AssetTypeNone,
		                                                              DelegateResponseCreditInfo onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new ServiceComponents.CreditInfoRequest
			{
				GroupId        = groupId,
				GroupAssetType = groupAssetType,
			};

			var response = await Com2Verse.WebApi.Service.Api.Credit.PostCreditGetUserCredit(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(0);
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
