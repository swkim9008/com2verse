#if ENABLE_CHEATING

/*===============================================================
* Product:		Com2Verse
* File Name:	CheatKey_Editor.cs
* Developer:	mikeyid77
* Date:			2023-06-14 15:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#if UNITY_EDITOR
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Cheat
{
	public partial class CheatKey
	{
#region BannedWord
		private static readonly string BannedWordUrlStaging = "https://staging-api-gspus.qpyou.cn/gateway.php";
		private static readonly string BannedWordUrlReal = "https://api-gspus.qpyou.cn/gateway.php";
		private const string DefaultAppId = "com.com2us.aaa"; // "com.andriod";
		private const string DefaultGameName = "default"; // "sgsongtestserver";
		private const string DefaultSaveFileName = "bannedWorlds.txt";

		// Check
		[MetaverseCheat("Cheat/HttpHelper/BannedWord/Staging/Check")] [HelpText("AppID", "GameName", "Revision")]
		private static async UniTask TestBannedWordStaging(string appId = DefaultAppId, string game = DefaultGameName, string revision = "0") => await TestBannedWordAsync(BannedWordUrlStaging, appId, game, revision);
		[MetaverseCheat("Cheat/HttpHelper/BannedWord/Real/Check")] [HelpText("AppID", "GameName", "Revision")]
		private static async UniTask TestBannedWordReal(string appId = DefaultAppId, string game = DefaultGameName, string revision = "0") => await TestBannedWordAsync(BannedWordUrlReal, appId, game, revision);

		// Download
		[MetaverseCheat("Cheat/HttpHelper/BannedWord/Staging/Download")] [HelpText("AppID", "GameName", "Revision")]
		private static async UniTask TestBannedWordDownloadStaging(string appId = DefaultAppId, string game = DefaultGameName, string revision = "0") => await TestBannedWordDownloadAsync(BannedWordUrlStaging, appId, game, revision);
		[MetaverseCheat("Cheat/HttpHelper/BannedWord/Real/Download")] [HelpText("AppID", "GameName", "Revision")]
		private static async UniTask TestBannedWordDownloadReal(string appId = DefaultAppId, string game = DefaultGameName, string revision = "0") => await TestBannedWordDownloadAsync(BannedWordUrlReal, appId, game, revision);

		// Run
		[MetaverseCheat("Cheat/HttpHelper/BannedWord/Staging/Run")] [HelpText("AppID", "GameName", "Revision")]
		private static async UniTask TestBannedWordRunStaging(string appId = DefaultAppId, string game = DefaultGameName, string revision = "0") => await TestBannedWordRunAsync(BannedWordUrlStaging, appId, game, revision);
		[MetaverseCheat("Cheat/HttpHelper/BannedWord/Real/Run")] [HelpText("AppID", "GameName", "Revision")]
		private static async UniTask TestBannedWordRunReal(string appId = DefaultAppId, string game = DefaultGameName, string revision = "0") => await TestBannedWordRunAsync(BannedWordUrlReal, appId, game, revision);

		// Save
		[MetaverseCheat("Cheat/HttpHelper/BannedWord/Staging/Save")] [HelpText("AppID", "GameName", "Revision", "SaveFileName")]
		private static async UniTask TestBannedWordSaveStaging(string appId = DefaultAppId, string game = DefaultGameName, string revision = "0", string saveFileName = DefaultSaveFileName) => await TestBannedWordSaveAsync(BannedWordUrlStaging, appId, game, revision, saveFileName);

		[MetaverseCheat("Cheat/HttpHelper/BannedWord/Real/Save")] [HelpText("AppID", "GameName", "Revision", "SaveFileName")]
		private static async UniTask TestBannedWordSaveReal(string appId = DefaultAppId, string game = DefaultGameName, string revision = "0", string saveFileName = DefaultSaveFileName) => await TestBannedWordSaveAsync(BannedWordUrlReal, appId, game, revision, saveFileName);

		private static async UniTask<WordFilterGetBundleInfoResponse> TestBannedWordAsync(string url, string appId, string game, string revisionStr)
		{
			if (!int.TryParse(revisionStr, out var revision))
				revision = 0;

			var content = JsonUtility.ToJson(WordFilterGetBundleInfoRequest.CreateNew(appId, game, revision));
			var responseString = await HttpHelper.Client.POST.RequestStringAsync(url, content);
			C2VDebug.Log(responseString);
			var response = JsonUtility.FromJson<WordFilterGetBundleInfoResponse>(responseString.Value);
			if (response != null)
				response.Print();
			return response;
		}

		private static async UniTask<(bool, byte[])> TestBannedWordDownloadAsync(string url, string appId, string game, string revisionStr)
		{
			if (!int.TryParse(revisionStr, out var revision))
				revision = 0;

			var response = await TestBannedWordAsync(url, appId, game, revisionStr);
			if (response == null)
				return (false, null);

			if (response.bundleInfo.revision > revision)
			{
				var downloadUrl = response.bundleInfo.url;
				byte[] buffer = null;
				await using (var responseStream = await HttpHelper.Client.GET.RequestStreamAsync(downloadUrl))
				{
					using var ms = new MemoryStream();
					await responseStream.Value.CopyToAsync(ms);
					buffer = ms.ToArray();
				}

				using var md5 = MD5.Create();
				var hash = md5.ComputeHash(buffer);
				var md5Str = ToHex(hash);
				var isValid = response.bundleInfo.md5.Equals(md5Str);
				C2VDebug.Log($"HASH     = {response.bundleInfo.md5}\nDOWNLOAD = {md5Str}");
				C2VDebug.Log($"IS VALID = {isValid}");
				if (isValid)
					return (true, buffer);
			}
			else
			{
				C2VDebug.Log($"request Version is newer");
			}

			return (false, null);
		}

		private static async UniTask<string> TestBannedWordRunAsync(string url, string appId, string game, string revisionStr)
		{
			var result = await TestBannedWordDownloadAsync(url, appId, game, revisionStr);
			if (!result.Item1) return string.Empty;

			var bytes = result.Item2;
			return Decrypt(bytes).Item2;
		}

		private static async UniTask TestBannedWordSaveAsync(string url, string appId, string game, string revisionStr, string saveFileName)
		{
			var result = await TestBannedWordRunAsync(url, appId, game, revisionStr);
			if (string.IsNullOrWhiteSpace(result)) return;

			if (string.IsNullOrWhiteSpace(saveFileName))
				saveFileName = DefaultSaveFileName;

			await File.WriteAllTextAsync(saveFileName, result);
		}

		private static (bool, string) Decrypt(byte[] bytes)
		{
			string key = "8318749066388490";

			try
			{
				RijndaelManaged aes = new RijndaelManaged();
				aes.KeySize = key.Length * 8;
				aes.BlockSize = 128;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;
				aes.Key = Encoding.UTF8.GetBytes(key);
				aes.IV = new byte[aes.BlockSize / 8];
				Array.Clear(aes.IV, 0, aes.IV.Length);
				ICryptoTransform decrypto = aes.CreateDecryptor();
				byte[] compressedData = decrypto.TransformFinalBlock(bytes, 0, bytes.Length);

				var data = Ionic.Zlib.ZlibStream.UncompressBuffer(compressedData);
				var dataStr = Encoding.UTF8.GetString(data);
				C2VDebug.Log($"Decrypted = {dataStr}");
				return (true, dataStr);
			}
			catch (Exception e)
			{
				C2VDebug.LogWarning($"Decrypt failed\n{e}");
				return (false, string.Empty);
			}
		}
		// https://stackoverflow.com/questions/2435695/converting-a-md5-hash-byte-array-to-a-string
		private static string ToHex(byte[] bytes, bool upperCase = false)
		{
			StringBuilder result = new StringBuilder(bytes.Length * 2);

			for (int i = 0; i < bytes.Length; i++)
				result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

			return result.ToString();
		}

#region Data
		[Serializable]
		private class WordFilterGetBundleInfoRequest
		{
			public string type;
			public string appId;
			public string game;
			public int revision;

			public static WordFilterGetBundleInfoRequest CreateNew(string appId, string game, int revision) =>
				new WordFilterGetBundleInfoRequest
				{
					type = "RequestWordfilterGetBundleInfo",
					appId = appId,
					game = game,
					revision = revision,
				};
		}

		[Serializable]
		private class WordFilterGetBundleInfoResponse
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
#endregion
	}
}
#endif // ENABLE_CHEATING
#endif // UNITY_EDITOR