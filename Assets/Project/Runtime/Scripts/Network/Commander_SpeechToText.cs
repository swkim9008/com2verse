/*===============================================================
* Product:		Com2Verse
* File Name:	Commander_SpeechToText.cs
* Developer:	ksw
* Date:			2023-07-21 10:34
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#region Request
using RequestRecordStart = Com2Verse.WebApi.Service.Components.SpeechToTextRecordRequest;
using RequestRecordStop = Com2Verse.WebApi.Service.Components.SpeechToTextRecordRequest;
using RequestRecordUsage = Com2Verse.WebApi.Service.Components.RecordUsageRequest;
using RequestRecordInfo = Com2Verse.WebApi.Service.Components.SpeechToTextRecordInfoRequest;
using RequestTranscription = Com2Verse.WebApi.Service.Components.SpeechToTextTranscriptionStartRequest;
using RequestFileDownloadURL = Com2Verse.WebApi.Service.Components.FileDownloadUrlRequest;
#endregion

#region Response
using ResponseRecordStart = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.SpeechToTextRecordResponseResponseFormat>;
using ResponseRecordStop = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.SpeechToTextRecordStopResponseResponseFormat>;
using ResponseRecordUsage = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.RecordUsageResponseResponseFormat>;
using ResponseRecordInfo = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.SpeechToTextRecordInfoResponseResponseFormat>;
using ResponseTranscription = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.SpeechToTextRecordInfoResponseResponseFormat>;
using ResponseFileDownloadURL = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.FileDownloadUrlResponseResponseFormat>;
#endregion

#region DelegateResponse
using DelegateResponseRecordStart = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.SpeechToTextRecordResponseResponseFormat>>;
using DelegateResponseRecordStop = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.SpeechToTextRecordStopResponseResponseFormat>>;
using DelegateResponseRecordUsage = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.RecordUsageResponseResponseFormat>>;
using DelegateResponseRecordInfo = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.SpeechToTextRecordInfoResponseResponseFormat>>;
using DelegateResponseTranscription = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.SpeechToTextRecordInfoResponseResponseFormat>>;
using DelegateResponseFileDownloadURL = System.Action<Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.FileDownloadUrlResponseResponseFormat>>;
#endregion

using Com2Verse.Logger;
using Com2Verse.UI;
using Com2Verse.WebApi.Service;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DelegateErrorResponse = System.Action<Com2Verse.Network.Commander.ErrorResponseData>;

namespace Com2Verse.Network
{
	public sealed partial class Commander
	{
		public async UniTask<ResponseRecordStart> RequestRecordStartAsync(long meetingId, DelegateResponseRecordStart onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new RequestRecordStart
			{
				MeetingId = meetingId,
			};

			var response = await Com2Verse.WebApi.Service.Api.SpeechToText.PostSpeechToTextRecordStart(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
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

		public async UniTask<ResponseRecordStop> RequestRecordStopAsync(long meetingId, DelegateResponseRecordStop onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new RequestRecordStop
			{
				MeetingId = meetingId,
			};

			var response = await Com2Verse.WebApi.Service.Api.SpeechToText.PostSpeechToTextRecordStop(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
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

		public async UniTask<ResponseRecordUsage> RequestRecordUsageAsync(long meetingId, DelegateResponseRecordUsage onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new RequestRecordUsage
			{
				MeetingId = meetingId,
			};

			var response = await Com2Verse.WebApi.Service.Api.SpeechToText.PostSpeechToTextRecordUsage(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
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

		public async UniTask<ResponseRecordInfo> RequestRecordInfoAsync(long meetingId, DelegateResponseRecordInfo onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new RequestRecordInfo
			{
				MeetingId = meetingId,
			};

			var response = await Com2Verse.WebApi.Service.Api.SpeechToText.PostSpeechToTextRecordInfo(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
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

		public async UniTask<ResponseTranscription> RequestTranscriptionAsync(long meetingId, int language = -1, DelegateResponseTranscription onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new RequestTranscription
			{
				MeetingId   = meetingId,
				LanguageType = language,
			};

			var response = await Com2Verse.WebApi.Service.Api.SpeechToText.PostSpeechToTextTranscriptionStart(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
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

		public async UniTask<ResponseFileDownloadURL> RequestFileDownloadURL(long meetingId, string[] fileNames, DelegateResponseFileDownloadURL onResponse = null, DelegateErrorResponse onError = null)
		{
			var request = new RequestFileDownloadURL
			{
				MeetingId = meetingId,
				FileName = fileNames,
			};

			var response = await Com2Verse.WebApi.Service.Api.SpeechToText.PostSpeechToTextFileDownloadUrl(request);
			var error    = OnResponseErrorHandling(response, response?.Value?.Code, onError);

			if (!error.IsValidResponse)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
				return null;
			}

			if (error.HasError)
			{
				// TODO : 검증 필요
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
