/*===============================================================
* Product:		Com2Verse
* File Name:	C2VCaching.cs
* Developer:	tlghks1009
* Date:			2023-03-28 15:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using Com2Verse.BuildHelper;
using UnityEngine;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace Com2Verse.AssetSystem
{
	public static class C2VCaching
	{
		private static C2VAssetBundleCacheCollection _cacheCollection;

		private static List<Cache> _managedCaches;

		public static void Initialize() => _managedCaches = new List<Cache>();

		public static Cache AddCache(string dir)
		{
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			var activeCache = Caching.GetCacheByPath(dir);
			if (!activeCache.valid)
				activeCache = Caching.AddCache(dir);

			_managedCaches.Add(activeCache);

			return activeCache;
		}

		public static C2VAssetBundleCacheEntity GetCacheEntity(string bundleName)
		{
			if (_cacheCollection == null)
			{
				Debug.LogError("[C2VCaching] CacheInfos is null.");
				return null;
			}

			foreach (var cacheInfo in _cacheCollection.Entities)
			{
				if (bundleName.Contains(cacheInfo.BundleName))
				{
					return cacheInfo;
				}
			}

			C2VDebug.LogErrorCategory("AssetBundle", $"Can't find cacheEntity. {bundleName}");
			return null;
		}


		public static bool TryGetBundleCache(eAssetBundleType assetBundleType, out Cache bundleCache)
		{
			var buildEnvironment = C2VAssetBundleManager.Instance.AppBuildEnvironment;
			var dir = $"{Application.temporaryCachePath}/Bundles/{buildEnvironment}/{assetBundleType}";

			var cache = Caching.GetCacheByPath(dir);
			if (!cache.valid)
			{
				bundleCache = default;
				return false;
			}

			bundleCache = cache;
			return true;
		}

		public static bool CheckCachingAvailability(long toDownloadSize)
		{
			if (_managedCaches == null)
				return false;

			long totalSpaceFree = 0;
			foreach (var managedCache in _managedCaches)
				totalSpaceFree += managedCache.spaceFree;

			return toDownloadSize <= totalSpaceFree;
		}

		public static async UniTask<bool> DownloadCacheData()
		{
			if (C2VAssetBundleManager.Instance.AssetBuildType != eAssetBuildType.REMOTE)
			{
				ParseDownloadedCacheData();
				return true;
			}

			if (File.Exists($"{C2VAssetBundleManager.Instance.AssetBundleLocalCacheInfoFullPath}"))
			{
				ParseDownloadedCacheData();
				return true;
			}


			var url = $"{C2VAssetBundleManager.Instance.AssetBundleRemotePath}/{C2VAssetBundleManager.Instance.AssetBundleCacheInfoFileName}";

			var request = UnityWebRequest.Get(url);
			try
			{
				await request.SendWebRequest();

				if (request.result == UnityWebRequest.Result.Success)
				{
					MakeBundleCacheDirectoryIfNotExists();
					DeleteOldCacheInfoFileIfExist();

					var jsonData = request.downloadHandler.text;

					await File.WriteAllTextAsync($"{C2VAssetBundleManager.Instance.AssetBundleLocalCacheInfoFullPath}", jsonData);

					ParseDownloadedCacheData();

					return true;
				}

				C2VDebug.LogErrorCategory("AssetBundle", $"UnityWebRequest Url : {url}, Error : {request.error}");
				return false;
			}
			catch (Exception e)
			{
				C2VDebug.LogErrorCategory("AssetBundle", $"UnityWebRequest Url : {url}, Error : {request.error}");
				return false;
			}
		}


		public static void RemoveCachedCatalogFiles()
		{
			var appVersionSaveKey = "AppVersion";
			var appVersion = C2VAssetBundleManager.Instance.AppVersion;

			if (C2VAssetBundleManager.Instance.AssetBuildType is eAssetBuildType.LOCAL or eAssetBuildType.EDITOR_HOSTED)
			{
				DeleteCatalogFiles();

				PlayerPrefs.SetString(appVersionSaveKey, appVersion);
				return;
			}


			if (!PlayerPrefs.HasKey(appVersionSaveKey))
			{
				DeleteCatalogFiles();

				PlayerPrefs.SetString(appVersionSaveKey, appVersion);
				return;
			}

			var cachedAppVersion  = PlayerPrefs.GetString(appVersionSaveKey, appVersion);
			var currentAppVersion = appVersion;

			if (cachedAppVersion != currentAppVersion)
			{
				DeleteCatalogFiles();

				PlayerPrefs.SetString(appVersionSaveKey, currentAppVersion);
			}

			void DeleteCatalogFiles()
			{
				var localCatalogDirectoryPath = $"{Application.persistentDataPath}/com.unity.addressables";

				if (!Directory.Exists(localCatalogDirectoryPath))
					return;

				foreach (var catalogFile in Directory.GetFiles(localCatalogDirectoryPath))
					File.Delete(catalogFile);
			}
		}


		private static void MakeBundleCacheDirectoryIfNotExists()
		{
			if (!Directory.Exists(C2VPaths.BundleCacheFolderPath))
			{
				Directory.CreateDirectory(C2VPaths.BundleCacheFolderPath);
			}
		}


		private static void DeleteOldCacheInfoFileIfExist()
		{
			var files = Directory.GetFiles($"{C2VPaths.BundleCacheFolderPath}/{C2VAssetBundleManager.Instance.AppBuildEnvironment}");

			foreach (var inFile in files)
			{
				if (Path.GetFileName(inFile).StartsWith("cacheInfo") && Path.GetExtension(inFile) == ".json")
				{
					File.Delete(inFile);
				}
			}
		}


		private static void ParseDownloadedCacheData()
		{
			C2VAssetBundleCacheCollection cacheCollection = null;

			if (C2VAssetBundleManager.Instance.AssetBuildType == eAssetBuildType.REMOTE)
			{
				if (!File.Exists(C2VAssetBundleManager.Instance.AssetBundleLocalCacheInfoFullPath))
				{
					C2VDebug.LogErrorCategory("AssetBundle", $"Can't find cacheInfo File. path : {C2VAssetBundleManager.Instance.AssetBundleLocalCacheInfoFullPath}");
					return;
				}

				var jsonData = File.ReadAllText(C2VAssetBundleManager.Instance.AssetBundleLocalCacheInfoFullPath);

				cacheCollection = JsonUtility.FromJson<C2VAssetBundleCacheCollection>(jsonData);
			}
			else
			{
				var jsonData = Resources.Load<TextAsset>("AssetBundle/assetBundleCacheInfo");

				if (jsonData == null)
				{
					C2VDebug.LogErrorCategory("AssetBundle", "Please click the 'R' button!!!!!");
					return;
				}

				cacheCollection = JsonUtility.FromJson<C2VAssetBundleCacheCollection>(jsonData.text);
			}

			if (cacheCollection?.Entities == null || cacheCollection.Entities.Count == 0)
			{
				C2VDebug.LogErrorCategory("AssetBundle", "Can't find cacheInfo.");
				return;
			}

			_cacheCollection = cacheCollection;
		}
	}
}
