/*===============================================================
* Product:		Com2Verse
* File Name:	BannedWords.cs
* Developer:	jhkim
* Date:			2023-03-08 16:35
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;

namespace Com2Verse.BannedWords
{
	public static class BannedWords
	{
		public enum eFilterType
		{
			ALL,
			NAME,
			SENTENCE,
		}

#region Variables
		private static readonly string BannedWordUrlStaging = "https://staging-api-gspus.qpyou.cn/gateway.php";
		private static readonly string BannedWordUrlReal = "https://api-gspus.qpyou.cn/gateway.php";
		private static readonly int ResultSuccess = 100;
		private static readonly int ResultIsNew = 0;
		private static readonly int ResultIsOld = 1;
		private static readonly string DecompressKey = "8318749066388490";

		public const string All = "all";
		public const string UsageName = "name";
		public const string UsageSentence = "sentence";

		private static BannedWordsInfo _info = null;

		private static readonly Data Data = new();
		private static readonly Define Define = new();

		private static bool _isRequest = false;
#endregion // Variables

#region Properties
		public static bool IsReady => _info is {WordInfoMap: {Count: > 0}};
		public static string LanguageCode => _info?.LanguageCode;
		public static string CountryCode => _info?.CountryCode;
		public static string Usage => _info?.Usage;
#endregion // Properties

#region Public Methods
		public static async UniTask<bool> CheckAndUpdateAsync(AppDefine appDefine)
		{
			var preRequested = false;
			if (_isRequest)
			{
				preRequested = true;
				await UniTask.WaitUntil(() => _isRequest);
			}
			if (preRequested) return IsReady;

			_isRequest = true;

			await Define.InitializeAsync();
			if (!Data.IsValidAppDefine(appDefine))
			{
				C2VDebug.LogError("Invalid app define");
				_isRequest = false;
				return false;
			}

			var isCached = Data.IsCached(appDefine);
			var url = appDefine.IsStaging ? BannedWordUrlStaging : BannedWordUrlReal;

			var revision = Define.GetRevision(appDefine);
			appDefine.Revision = revision;

			var info = await RequestInfoAsync(url, appDefine);
			if (info.resultCode == ResultSuccess)
			{
				if (!isCached || info.bundleInfo.resultType == ResultIsOld)
				{
					await DownloadAndCache(appDefine, info.bundleInfo.url);

					appDefine.Revision = Convert.ToString(info.bundleInfo.revision);
					Define.UpdateAppDefine(appDefine);
				}

				_isRequest = false;
				return true;
			}

			C2VDebug.LogError($"Invalid response = {info.resultMessage} ({info.resultCode}");
			_isRequest = false;
			return false;
		}

		public static async UniTask<BannedWordsInfo> LoadAsync(AppDefine appDefine)
		{
			if (!Data.IsValidAppDefine(appDefine))
			{
				C2VDebug.LogError("Invalid app define");
				return null;
			}

			if (!Data.IsCached(appDefine))
			{
				C2VDebug.LogError("캐시 된 금칙어 데이터가 없습니다. CheckAndUpdateAsync로 금칙어 데이터를 다운받아주세요");
				Data.RemoveCache(appDefine);
				return null;
			}

			var data = await Data.LoadCacheAsync(appDefine);
			if (string.IsNullOrWhiteSpace(data))
			{
				C2VDebug.LogError("Load cache failed.");
				return null;
			}

			_info = BannedWordsInfo.Create(data);
			return _info;
		}

		public static string ApplyFilter(string text, string replace, bool matchNumberOfFiltered = false) => _info?.ApplyFilter(text, replace, matchNumberOfFiltered);
		public static async UniTask<string> ApplyFilterAsync(string text, string replace, bool matchNumberOfFiltered = false)
		{
			await PrepareBannedWordAsync();
			return ApplyFilter(text, replace, matchNumberOfFiltered);
		}
		public static bool HasBannedWords(string text) => _info?.HasBannedWords(text) ?? false;
		public static async UniTask<bool> HasBannedWordAsync(string text)
		{
			await PrepareBannedWordAsync();
			return HasBannedWords(text);
		}
		public static void SetLanguageAll() => SetLanguageCode(All);
		public static void SetLanguageCode(string lang) => _info?.SetLanguageCode(lang);
		public static void SetCountryAll() => SetCountryCode(All);
		public static void SetCountryCode(string country) => _info?.SetCountryCode(country);
		public static void SetUsageName() => SetUsage(UsageName);
		public static void SetUsageSentence() => SetUsage(UsageSentence);
		public static void SetUsage(string usage) => _info?.SetUsage(usage);
#endregion // Public Methods

#region Private Methods
		private static async UniTask PrepareBannedWordAsync()
		{
			if (!IsReady)
			{
				var available = await CheckAndUpdateAsync(AppDefine.Default);
				if (available)
				{
					await LoadAsync(AppDefine.Default);
					SetLanguageAll();
					SetCountryAll();
					SetUsageName();
				}
			}
		}
#endregion // Private Methods

#region Request
		private static async UniTask<GetInfoResponse> RequestInfoAsync(string url, AppDefine appDefine)
		{
			if (!int.TryParse(appDefine.Revision, out var revision))
				revision = 0;

			var content = JsonUtility.ToJson(GetInfoRequest.CreateNew(appDefine.AppId, appDefine.Game, revision));
			var responseString = await HttpHelper.Client.POST.RequestStringAsync(url, content);
			var response = JsonUtility.FromJson<GetInfoResponse>(responseString?.Value);
			return response;
		}
		private static async UniTask DownloadAndCache(AppDefine appDefine, string url)
		{
			if (string.IsNullOrWhiteSpace(url)) return;

			try
			{
				byte[] encrypted;
				await using (var response = await HttpHelper.Client.GET.RequestStreamAsync(url))
				{
					if (response?.Value == null)
					{
						C2VDebug.LogError($"금칙어 다운로드 실패");
						return;
					}
					using var ms = new MemoryStream();
					await response.Value.CopyToAsync(ms);
					encrypted = ms.ToArray();
				}

				var result = await DecryptAsync(encrypted);
				if (result.Item1)
				{
					await Data.SaveCacheAsync(appDefine, result.Item2);
					return;
				}

				C2VDebug.LogError($"금칙어 복호화 실패");
			}
			catch (Exception e)
			{
				C2VDebug.LogError(e);
			}
		}
#endregion // Request

#region Response
		private static async UniTask<(bool, string)> DecryptAsync(byte[] bytes)
		{
			try
			{
				RijndaelManaged aes = new RijndaelManaged();
				aes.KeySize = DecompressKey.Length * 8;
				aes.BlockSize = 128;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;
				aes.Key = Encoding.UTF8.GetBytes(DecompressKey);
				aes.IV = new byte[aes.BlockSize / 8];
				Array.Clear(aes.IV, 0, aes.IV.Length);

				var decrypto = aes.CreateDecryptor();
				var compressedData = decrypto.TransformFinalBlock(bytes, 0, bytes.Length);

				byte[] decompressed = Zip.ZlibStream.UncompressBuffer(compressedData);
				if (decompressed == null)
				{
					C2VDebug.LogError("압축 해제 실패");
					return (false, string.Empty);
				}

				var dataStr = Encoding.UTF8.GetString(decompressed);
				return (true, dataStr);
			}
			catch (Exception e)
			{
				C2VDebug.LogWarning($"복호화 실패\n{e}");
				return (false, string.Empty);
			}
		}
#endregion // Response

#region Data
		[Serializable]
		private class GetInfoRequest
		{
			public string type;
			public string appId;
			public string game;
			public int revision;

			public static GetInfoRequest CreateNew(string appId, string game, int revision) =>
				new GetInfoRequest
				{
					type = "RequestWordfilterGetBundleInfo",
					appId = appId,
					game = game,
					revision = revision,
				};
		}

		[Serializable]
		private class GetInfoResponse
		{
			public BundleInfo bundleInfo;
			public string type;
			public int resultCode;
			public string resultMessage;

			public void Print()
			{
				C2VDebug.Log($"Result : {resultMessage} ({resultCode})");
				C2VDebug.Log($"{type}");
				bundleInfo.Print();
			}
		}

		[Serializable]
		private class BundleInfo
		{
			public string game;
			public int revision;
			public string modDate;
			public int resultType;
			public string url;
			public int fileSize;
			public string md5;

			public void Print()
			{
				C2VDebug.Log($"Game: {game} ({revision})");
				C2VDebug.Log($"Date: {modDate}, ResultType: {resultType}");
				C2VDebug.Log($"{url}");
				C2VDebug.Log($"{fileSize}");
				C2VDebug.Log($"HASH: {md5}");
			}
		}
#endregion // Data

#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Reset()
		{
			_info?.Dispose();
			_info = null;
		}
#endif // UNITY_EDITOR
	}
}
