/*===============================================================
* Product:		Com2Verse
* File Name:	AssetBundleWebAPIBuilder.cs
* Developer:	tlghks1009
* Date:			2023-06-26 10:52
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Text;
using Com2Verse.BuildHelper;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Com2VerseEditor.Build
{
	public sealed class AssetBundleWebAPIBuilder
	{
		private static string _apiDevUrl  = "https://dev-api.com2verse.com";
		private static string _apiLiveUrl = "https://api.com2verse.com";

		private static string GetApiUrl(eBuildEnv buildEnv, eHiveEnvType hiveEnvType)
		{
			switch (buildEnv)
			{
				case eBuildEnv.QA:
				case eBuildEnv.DEV:
				case eBuildEnv.STAGING:
				case eBuildEnv.DEV_INTEGRATION:
					return _apiDevUrl;
				case eBuildEnv.PRODUCTION:
					return hiveEnvType == eHiveEnvType.LIVE ? _apiLiveUrl : _apiDevUrl;
			}

			return string.Empty;
		}

		private static UnityWebRequest CreateUnityWebRequest(string url, string jsonData)
		{
			var request = new UnityWebRequest(url, "POST");
			request.SetRequestHeader("Content-Type", "application/json");
			request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));

			return request;
		}


		public static bool PutAsset(eBuildEnv buildEnv, eHiveEnvType hiveEnvType, string assetBundlePath, string buildTarget, string assetBundleVersion, string appVersion)
		{
			Debug.Log("[AssetBundle] PatchVersion Upload Start");
			var url = GetApiUrl(buildEnv, hiveEnvType);
			var api = $"{url}/api/asset";

			AssetEntity assetEntity = null;
			switch (buildEnv)
			{
				case eBuildEnv.QA:
				case eBuildEnv.DEV:
				case eBuildEnv.STAGING:
				case eBuildEnv.DEV_INTEGRATION:
					assetEntity = CreateAssetEntity(assetBundlePath, buildTarget, appVersion, assetBundleVersion, "Y");
					break;
				case eBuildEnv.PRODUCTION:
				{
					var use = "Y";
					if (hiveEnvType == eHiveEnvType.LIVE)
						use = "N";

					assetEntity = CreateAssetEntity(assetBundlePath, buildTarget, appVersion, assetBundleVersion, use);
				}
					break;
			}

			var toJson  = JsonUtility.ToJson(assetEntity);
			var request = CreateUnityWebRequest(api, toJson);

			try
			{
				var asyncOperation = request.SendWebRequest();
				while (!asyncOperation.isDone) { }

				bool result = true;
				if (request.result != UnityWebRequest.Result.Success)
				{
					Debug.LogError($"[AssetBundle] Open api error. {request.error}");
					result = false;
				}

				request.Dispose();
				Debug.Log("[AssetBundle] PatchVersion Upload Success");

				return result;
			}
			catch (Exception e)
			{
				request.Dispose();
				Debug.LogError($"[AssetBundle] Patch Version Upload API call Error. Message : {request.error}");
				return false;
			}
		}


		public static AssetEntity[] PostAssetList(eBuildEnv buildEnv, eHiveEnvType hiveEnvType, string buildTarget)
		{
			var url = GetApiUrl(buildEnv, hiveEnvType);
			var api = $"{url}/api/asset/list";

			var assetRequestBody = new AssetRequestBody
			{
				MetaverseId = 11,
				BuildTarget = buildTarget,
				UseYn       = "Y",
				PageSize    = 100000,
				PageNum     = 1,
			};
			var toJson  = JsonUtility.ToJson(assetRequestBody);
			var request = CreateUnityWebRequest(api, toJson);
			request.downloadHandler = new DownloadHandlerBuffer();

			try
			{
				var asyncOperation = request.SendWebRequest();

				while (!asyncOperation.isDone) { }

				AssetEntity[] assetsData = null;

				if (request.result == UnityWebRequest.Result.Success)
				{
					var jsonData    = "{\"items\": " + request.downloadHandler.text + "}";
					var assetEntity = JsonUtility.FromJson<JsonWrapper<AssetEntity>>(jsonData);
					assetsData = assetEntity.items;
				}
				else
					Debug.LogError($"[AssetBundle] Open api error. {request.error}");

				request.Dispose();
				return assetsData;
			}
			catch (Exception e)
			{
				Debug.LogError($"[AssetBundle] Patch Version API call Error. Message : {request.error}");
				request.Dispose();
				return null;
			}
		}


		private static AssetEntity CreateAssetEntity(string assetBundlePath, string buildTarget, string appVersion, string assetBundleVersion, string use)
		{
			var assetEntity = new AssetEntity
			{
				METAVERSE_ID   = 11,
				METAVERSE_PATH = assetBundlePath,
				ASSET_TYPE     = "AssetBundle",
				BUILD_TARGET   = buildTarget,
				APP_VERSION    = appVersion,
				PATCH_VERSION  = assetBundleVersion,
				USE_YN         = use,
			};
			return assetEntity;
		}
	}


	[Serializable]
	public class AssetEntity
	{
		public int    METAVERSE_ID;
		public string METAVERSE_PATH;
		public string ASSET_TYPE;
		public string BUILD_TARGET;
		public string APP_VERSION;
		public string PATCH_VERSION;
		public string USE_YN;
	}

	[Serializable]
	public class AssetRequestBody
	{
		[JsonProperty("METAVERSE_ID")]
		public long MetaverseId { get; set; }

		[JsonProperty("BUILD_TARGET")]
		public string BuildTarget { get; set; }

		[JsonProperty("USE_YN")]
		public string UseYn { get; set; }

		[JsonProperty("PAGE_SIZE")]
		public int PageSize { get; set; }

		[JsonProperty("PAGE_NUM")]
		public int PageNum { get; set; }
	}

	[Serializable]
	public class JsonWrapper<T>
	{
		public T[] items;
	}
}
