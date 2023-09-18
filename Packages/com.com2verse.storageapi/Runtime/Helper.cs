/*===============================================================
* Product:		Com2Verse
* File Name:	Helper.cs
* Developer:	jhkim
* Date:			2023-06-01 11:11
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Com2Verse.HttpHelper;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Com2Verse.StorageApi
{
	public static class Helper
	{
#region Variables
		public enum eEnvironment
		{
			TEST,
		}

		private enum eRequest
		{
			GET_UPLOAD_URL,
			GET_DOWNLOAD_URL,
		}

		private static readonly Dictionary<eEnvironment, string> StorageApiUrlMap = new Dictionary<eEnvironment, string>
		{
			{eEnvironment.TEST, "https://test-api2.com2verse.com/file/"},
		};

		private static readonly Dictionary<eRequest, string> RequestApiMap = new Dictionary<eRequest, string>
		{
			{eRequest.GET_UPLOAD_URL, "api/v1/file/"},
			{eRequest.GET_DOWNLOAD_URL, "api/v1/file/download"},
		};
#endregion // Variables

#region Properties
		public static string ServiceName { get; set; }
		public static string StoragePath { get; set; }
		public static int ExpiredSecond { get; set; } = 360000;
		public static eEnvironment Environment { get; set; } = eEnvironment.TEST;
#endregion // Properties

#region Debug
		public static string DebugAuthToken
		{
			set => WebApi.Util.Instance.AccessToken = value;
		}
#endregion // Debug

#region Public Methods
		public static async UniTask<bool> UploadAsync(string fileName, AudioClip clip)
		{
			if (!ValidateCommon()) return false;

			var bytes = Util.GetBytes(clip);
			if (bytes == null)
			{
				C2VDebug.LogWarning("AudioClip이 유효하지 않습니다.");
				return false;
			}

			if (!Util.TryGenerateHash(bytes, out var hash)) return false;

			var request = new GetUploadUrlRequest
			{
				ServiceName = ServiceName,
				Path = Util.GetValidStoragePath(StoragePath),
				FileName = fileName,
				FileSize = bytes.Length,
				Md5Hash = hash,
				ExpiredSecond = ExpiredSecond,
			};

			return await RequestUploadAsync(request, bytes);
		}

		public static async UniTask<bool> UploadAsync(string filename, byte[] bytes)
		{
			if (!ValidateCommon()) return false;
			if (!Util.TryGenerateHash(bytes, out var hash)) return false;

			var request = new GetUploadUrlRequest
			{
				ServiceName = ServiceName,
				Path = Util.GetValidStoragePath(StoragePath),
				FileName = filename,
				FileSize = bytes.Length,
				Md5Hash = hash,
				ExpiredSecond = ExpiredSecond,
			};

			return await RequestUploadAsync(request, bytes);
		}

		public static async UniTask<bool> UploadFileAsync(string url, string fileName, AudioClip clip)
		{
			if (!ValidateCommon()) return false;

			var bytes = Util.GetBytes(clip);
			if (bytes == null)
			{
				C2VDebug.LogWarning("AudioClip이 유효하지 않습니다.");
				return false;
			}

			var result = await WebRequestUploadFileAsync(url, fileName, bytes);
			return await ValidateResponseAsync(result, "AudioClip 업로드 요청");
		}

		public static async UniTask<bool> UploadFileAsync(string url, string fileName, byte[] bytes)
		{
			if (!ValidateCommon()) return false;

			var result = await WebRequestUploadFileAsync(url, fileName, bytes);
			return await ValidateResponseAsync(result, "파일 업로드 요청");
		}

		public static async UniTask<(bool, string)> GetDownloadUrlAsync(string fileName)
		{
			if (!ValidateCommon()) return (false, string.Empty);

			var request = new GetDownloadUrlRequest
			{
				ServiceName = ServiceName,
				Path = Util.GetValidStoragePath(StoragePath),
				FileName = fileName,
			};

			var result = await RequestDownloadUrlAsync(request);
			return result;
		}
#endregion // Public Methods

#region File Upload
		private static async UniTask<bool> RequestUploadAsync(GetUploadUrlRequest urlRequest, byte[] bytes)
		{
			var result = await WebRequestGetUploadUrlAsync(urlRequest);
			if (!await ValidateResponseAsync(result, "Upload URL 요청")) return false;

			var response = JsonConvert.DeserializeObject<GetUploadUrlResponse>(result.Value);
			var url = response?.UrlInfo?.URL;

			result = await WebRequestUploadFileAsync(url, urlRequest.FileName, bytes);
			return await ValidateResponseAsync(result, "파일 업로드 요청");
		}

		private static async UniTask<ResponseBase<string>> WebRequestGetUploadUrlAsync(GetUploadUrlRequest urlRequest)
		{
			var url = GetApiUrl(eRequest.GET_UPLOAD_URL);
			var builder = HttpRequestBuilder.CreateNew(Client.eRequestType.POST, url);

			var requestJson = JsonConvert.SerializeObject(urlRequest);
			builder.SetContent(requestJson);
			builder.SetContentType(Client.Constant.ContentJson);

			var result = await Client.Message.RequestStringAsync(builder.Request);
			return result;
		}

		public static async UniTask<ResponseBase<string>> WebRequestUploadFileAsync(string url, string fileName, AudioClip clip)
		{
			var bytes = Util.GetBytes(clip);
			return await WebRequestUploadFileAsync(url, fileName, bytes);
		}
		public static async UniTask<ResponseBase<string>> WebRequestUploadFileAsync(string url, string fileName, byte[] bytes)
		{
			Util.TryGenerateHash(bytes, out var hash);

			var builder = HttpRequestBuilder.CreateNew(Client.eRequestType.PUT, url);
			var content = new ByteArrayContent(bytes);
			builder.SetContent(content);
			builder.Request.Content.Headers.Add("Content-Type", Client.Constant.ContentOctetStream);
			builder.SetMD5ContentHeader(Convert.FromBase64String(hash));
			var result = await Client.Message.RequestStringAsync(builder.Request, null, RequestOption.Default);
			return result;
		}
#endregion // File Upload

#region File Download
		private static async UniTask<(bool, string)> RequestDownloadUrlAsync(GetDownloadUrlRequest request)
		{
			var result = await WebRequestGetDownloadUrlAsync(request);
			if (!await ValidateResponseAsync(result, "Download URL 요청")) return (false, string.Empty);

			var response = JsonConvert.DeserializeObject<GetDownloadUrlResponse>(result.Value);
			var url = response?.UrlInfo?.URL;
			return (true, url);
		}
		private static async UniTask<ResponseBase<string>> WebRequestGetDownloadUrlAsync(GetDownloadUrlRequest request)
		{
			var url = GetApiUrl(eRequest.GET_DOWNLOAD_URL);
			url = $"{url}?service-name={request.ServiceName}&path={request.Path}&file={request.FileName}";

			var builder = HttpRequestBuilder.CreateNew(Client.eRequestType.GET, url);
			var result = await Client.Message.RequestStringAsync(builder.Request);
			return result;
		}
#endregion // File Download

#region Private Methods
		private static string GetApiUrl(eRequest request)
		{
			if (!StorageApiUrlMap.ContainsKey(Environment))
			{
				C2VDebug.LogWarning($"등록된 환경 주소가 없습니다 = {Environment}");
				return string.Empty;
			}
			if (!RequestApiMap.ContainsKey(request))
			{
				C2VDebug.LogWarning($"등록된 API 정보가 없습니다 = {request}");
				return string.Empty;
			}

			var apiUrl = StorageApiUrlMap[Environment];
			var requestApi = RequestApiMap[request];
			return $"{apiUrl}{requestApi}";
		}
#endregion // Private Methods

#region Validate
		private static bool ValidateCommon() => ValidateInfo() && ValidateAuthToken();
		private static bool ValidateInfo()
		{
			if (string.IsNullOrWhiteSpace(ServiceName))
			{
				C2VDebug.LogWarning("서비스 이름이 지정되지 않았습니다.");
				return false;
			}

			if (string.IsNullOrWhiteSpace(StoragePath))
			{
				C2VDebug.LogWarning("경로가 지정되지 않았습니다.");
				return false;
			}

			return true;
		}
		private static bool ValidateAuthToken()
		{
			if (!WebApi.Util.Instance.TrySetAuthToken())
			{
				C2VDebug.LogWarning("인증 실패. 토큰이 유효하지 않습니다.");
				return false;
			}

			return true;
		}
		private static async UniTask<bool> ValidateResponseAsync<T>(ResponseBase<T> result, string comment)
		{
			if (!result.Response.IsSuccessStatusCode)
			{
				var responseString = await result.Response.Content.ReadAsStringAsync();
				C2VDebug.LogWarning($"{comment} 실패, {responseString}");
				return false;
			}

			return true;
		}
#endregion // Validate

#region Requests
		[Serializable]
		private class GetUploadUrlRequest
		{
			[JsonProperty("serviceName")]
			public string ServiceName;

			[JsonProperty("path")]
			public string Path;

			[JsonProperty("file")]
			public string FileName;

			[JsonProperty("size")]
			public int FileSize;

			[JsonProperty("md5")]
			public string Md5Hash;

			[JsonProperty("expiredSec")]
			public int ExpiredSecond;
		}

		[Serializable]
		private class GetDownloadUrlRequest
		{
			[JsonProperty("service-name")]
			public string ServiceName;

			[JsonProperty("path")]
			public string Path;

			[JsonProperty("file")]
			public string FileName;
		}
#endregion // Requests

#region Responses
		[Serializable]
		private class ResponseBase
		{
			[JsonProperty("code")]
			public int ResponseCode;

			[JsonProperty("msg")]
			public string ResponseMessage;
		}

		[Serializable]
		private class GetUploadUrlResponse : ResponseBase
		{
			[JsonProperty("data")]
			public UrlInfo UrlInfo;
		}

		[Serializable]
		private class GetDownloadUrlResponse : ResponseBase
		{
			[JsonProperty("data")]
			public UrlInfo UrlInfo;
		}

		[Serializable]
		private class UrlInfo
		{
			[JsonProperty("url")]
			public string URL;
		}
#endregion // Responses
	}
}
