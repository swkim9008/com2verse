/*===============================================================
* Product:		Com2Verse
* File Name:	C2VCleanBundleCacheOperation.cs
* Developer:	tlghks1009
* Date:			2023-06-19 10:47
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Com2Verse.Logger;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;

namespace Com2Verse.AssetSystem
{
	public class C2VCleanBundleCacheOperation
	{
		public static AsyncOperationHandle<bool> TryCleanBundleCache()
		{
			if (!Caching.ready)
				return Addressables.ResourceManager.CreateCompletedOperation<bool>(false, "Caching is not ready.");

			var cleanBundleCacheOperation = new C2VCleanBundleCacheOperation();
			var cacheDirectoriesInUse = cleanBundleCacheOperation.GetCacheDirectoriesInUse();
			var cacheDirectoriesNotInUse = cleanBundleCacheOperation.DetermineCacheDirectoriesNotInUse(cacheDirectoriesInUse);

			cleanBundleCacheOperation.RemoveCacheEntries(cacheDirectoriesNotInUse);

			return Addressables.ResourceManager.CreateCompletedOperation<bool>(true, string.Empty);
		}


		private void RemoveCacheEntries(List<string> cacheDirectoriesNotInUse)
		{
			foreach (var cacheDirectory in cacheDirectoriesNotInUse)
			{
				string bundleName = Path.GetFileName(cacheDirectory);
				Caching.ClearAllCachedVersions(bundleName);

				C2VDebug.LogCategory("AssetBundle", $"Remove an obsolete bundle. BundleName : {bundleName}");
			}
		}


		private List<string> DetermineCacheDirectoriesNotInUse(HashSet<string> cacheDirectoriesInUse)
		{
			var cacheDirectoriesForRemoval = new List<string>();
			var allCachePaths              = new List<string>();
			Caching.GetAllCachePaths(allCachePaths);

			foreach (var cachePath in allCachePaths)
			{
				if (!Directory.Exists(cachePath))
				{
					continue;
				}

				foreach (var cacheDirectory in Directory.EnumerateDirectories(cachePath, "*", SearchOption.TopDirectoryOnly))
				{
					var bundleName = Path.GetFileName(cacheDirectory);

					if (!cacheDirectoriesInUse.Contains(bundleName))
					{
						cacheDirectoriesForRemoval.Add(bundleName);
					}
				}
			}
			return cacheDirectoriesForRemoval;
		}


		private HashSet<string> GetCacheDirectoriesInUse()
		{
			var cacheDirectoriesInUse = new HashSet<string>();

			var locators = Addressables.ResourceLocators;

			foreach (var locator in locators)
			{
				var locationMap = locator as ResourceLocationMap;

				if (locationMap == null)
				{
					continue;
				}

				foreach (var locationList in locationMap.Locations.Values)
				{
					foreach (var location in locationList)
					{
						if (location.Data is AssetBundleRequestOptions options)
						{
							if (TryGetLoadInfo(location, out AssetBundleResource.LoadType loadType, out string path))
							{
								if (loadType == AssetBundleResource.LoadType.Web)
								{
									cacheDirectoriesInUse.Add(options.BundleName);
								}
							}
						}
					}
				}
			}

			return cacheDirectoriesInUse;
		}


		private bool TryGetLoadInfo(IResourceLocation location, out AssetBundleResource.LoadType loadType, out string path)
		{
			var options = location?.Data as AssetBundleRequestOptions;
			if (options == null)
			{
				loadType = AssetBundleResource.LoadType.None;
				path     = string.Empty;
				return false;
			}

			path = Addressables.ResourceManager.TransformInternalId(location);
			if (ResourceManagerConfig.ShouldPathUseWebRequest(path))
			{
				loadType = AssetBundleResource.LoadType.Web;
			}
			else if (options.UseUnityWebRequestForLocalBundles)
			{
				path     = $"file:///{Path.GetFullPath(path)}";
				loadType = AssetBundleResource.LoadType.Web;
			}
			else
			{
				loadType = AssetBundleResource.LoadType.Local;
			}

			return true;
		}
	}
}
