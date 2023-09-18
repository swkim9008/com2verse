/*===============================================================
* Product:		Com2Verse
* File Name:	AssetSystemManager.cs
* Developer:	masteage
* Date:			2022-04-13 16:23
* History:    
* Documents:	https://jira.com2us.com/wiki/display/C2U2VR/Asset+System
*				Addressables 1.19.19
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

// ADDRESSABLES_LOG_ALL

// #define ENABLE_LOG
// #define DEBUG
// #define UNITY_EDITOR
// #define DEVELOPMENT_BUILD
#if !METAVERSE_RELEASE
#define ASSET_SYSTEM_LIFE_CYCLE_LOG
// #define ASSET_SYSTEM_LOG
// #define DETAIL_ASSET_SYSTEM_LOG
#endif	// !METAVERSE_RELEASE
// #define SONG_TEST

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif// UNITY_EDITOR

namespace Com2Verse.AssetSystem
{
	public sealed class AssetSystemManager : MonoSingleton<AssetSystemManager>
	{
		private static readonly int DEFAULT_COMPANY_ID = 1;
		private static readonly char URL_DELIMITER = Path.AltDirectorySeparatorChar;
		
		private struct AssetData
		{
			public AsyncOperationHandle Handle { get; }
			public bool IsInstantiated { get; }
			public AssetData(AsyncOperationHandle handle , bool isInstantiated)
			{
				Handle = handle;
				IsInstantiated = isInstantiated;
			}
		}
		
		private bool _isInitialized;// = false;
		private static bool _useInternalIdTransformFunc;// = false;
		private static HttpClient _httpClient;
		private static List<PatchUrlData> _patchUrlData;
		private static int _companyId = DEFAULT_COMPANY_ID;
		private static string _path = string.Empty;//"https://metaverse-platform-fn.qpyou.cn/metaverse-platform/Test/AssetBundle/StandaloneWindows64/0.1.0/0.0.1";
		
		//TODO: Mr.Song - ObjectPool 사용 고려.
		// key (AssetGUID/AddressableName), Data
		private Dictionary<string, AssetData> _assetDictionary;
		// key, is timeout
		private Dictionary<string, bool> _assetLoadingPool;
		
		//////////////////////////////////////////
		// Mono

#region Mono
		protected override void OnDestroyInvoked() => Destroy();
		protected override void OnApplicationQuitInvoked() => Destroy();
		
		private void Destroy()
		{
			AssetSystemLifeCycleLog("Destroy called");
			ReleaseAssetRefAll();
			_isInitialized = false;
			_useInternalIdTransformFunc = false;
			_companyId = DEFAULT_COMPANY_ID;
			_path = string.Empty;
			_httpClient = null;
			_patchUrlData?.Clear();
			_patchUrlData = null;
			AssetSystemLifeCycleLog("Destroy End");
		}
#endregion	// Mono
		
		//////////////////////////////////////////
		// Public
		
#region Public
		
#region Init
		public void Initialize()
		{
			AssetSystemLifeCycleLog("Initialize called");
			InitializeHttpClient();
			if (_isInitialized == false)
			{
				// Caching.ClearCache();
				var initializeAsyncHandle = Addressables.InitializeAsync(true);
				initializeAsyncHandle.Completed += (handle) =>
				{
					AssetSystemLifeCycleLog("Addressables.InitializeAsync - Completed");
					if (handle.Status == AsyncOperationStatus.Succeeded)
					{
						AssetSystemLifeCycleLog("Addressables.InitializeAsync - Succeeded");
						// UpdateCatalog();
						_isInitialized = true;
					}
					else if (handle.Status == AsyncOperationStatus.Failed)
					{
						AssetSystemLifeCycleLog("Addressables.InitializeAsync - Failed");
					}
				};
				// await initializeAsyncHandle.Task;
				// initializeAsyncHandle.WaitForCompletion();
				_assetDictionary?.Clear();
				_assetDictionary = new Dictionary<string, AssetData>();
				_assetLoadingPool?.Clear();
				_assetLoadingPool = new Dictionary<string, bool>();
#if SONG_TEST
				_testFlow = 0;
				_instantiatedObject = null;
#endif // SONG_TEST
			}
		}

		private void UpdateCatalog()
		{
			//TODO: Mr.Song - code 추가 정리 및 테스트 필요.
			AssetSystemLog("UpdateCatalog called");
			Addressables.CheckForCatalogUpdates().Completed += handle =>
			{
				AssetSystemLog("Addressables.CheckForCatalogUpdates - Completed");
				if (handle.Status == AsyncOperationStatus.Succeeded)
				{
					AssetSystemLog("Addressables.CheckForCatalogUpdates - Succeeded");
					if (handle.Result is not null && handle.Result.Count > 0)
					{
						DetailAssetSystemLog($"handle.Result.Count : {handle.Result.Count}");
						foreach (var item in handle.Result)
						{
							DetailAssetSystemLog($"item : {item}");
						}
						
						// UpdateCatalogs
						// - all : null or handle.Result
						// - select : item..
						// Addressables.UpdateCatalogs(handle.Result).Completed += updates =>
						// Addressables.UpdateCatalogs(true).Completed += updates =>
						Addressables.UpdateCatalogs().Completed += updateCatalogsHandle =>
						{
							AssetSystemLog("Addressables.UpdateCatalogs - completed");
							// updateCatalogsHandle.Result;

							if (updateCatalogsHandle.Status == AsyncOperationStatus.Succeeded)
							{
								AssetSystemLog("Addressables.UpdateCatalogs - Succeeded");
							}
							else if (updateCatalogsHandle.Status == AsyncOperationStatus.Failed)
							{
								AssetSystemLog("Addressables.UpdateCatalogs - Failed");
							}
						};
					}
					else
					{
						AssetSystemLog($"Addressables.UpdateCatalogs - no items");
						if (handle.Result != null)
						{
							DetailAssetSystemLog($"handle.Result.Count : {handle.Result.Count}");	
						}
					}
				}
				else if(handle.Status == AsyncOperationStatus.Failed)
				{
					AssetSystemLog("Addressables.CheckForCatalogUpdates - Failed");
					if (handle.OperationException != null) return;
					var e = handle.OperationException;
					var exceptionType = e.GetType().ToString();
					DetailAssetSystemLog($"e.GetType() : {exceptionType}");
					DetailAssetSystemLog($"e.Message : {e.Message}");
				}
			};
			// await UniTask.WaitUntil(() => completed);
			// return await UniTask.FromResult(downloadSize);
		}
		
#endregion	// Init

#region Get

		public object GetAsset(AssetReference assetRef)
		{
			AssetSystemLog("GetAsset (AssetReference) called");
			if (assetRef == null) { return null; }
			DetailAssetSystemLog($"AssetGUID : {assetRef.AssetGUID}");
			return assetRef.OperationHandle.Result;
			// return GetAssetHandle(assetRef.AssetGUID).Result;
		}

		public object GetAsset(string addressableName)
		{
			AssetSystemLog("GetAsset (AssetReference) called");
			if (string.IsNullOrEmpty(addressableName)) { return null; }
			string key = addressableName;
			DetailAssetSystemLog($"key (AssetGUID/AddressableName) : {key}");
			return GetAssetHandle(key)?.Result;
		}
#endregion	// Get
		
#region Load
		public bool IsLoaded(AssetReference assetRef)
		{
			AssetSystemLog("IsLoaded (AssetReference) called");
			bool isAssetLoaded = false;
			if (assetRef != null)
			{
				DetailAssetSystemLog($"AssetGUID : {assetRef.AssetGUID}");
				isAssetLoaded = IsLoaded(assetRef.OperationHandle);
			}
			return isAssetLoaded;
		}

		public bool IsLoaded(string addressableName)
		{
			AssetSystemLog("IsLoaded (addressableName) called");
			if (string.IsNullOrEmpty(addressableName)) {return false;}
			string key = addressableName;
			DetailAssetSystemLog($"key (AssetGUID/AddressableName) : {key}");
			bool isAssetLoaded = false;
			if (_assetDictionary != null && _assetDictionary.TryGetValue(key, out AssetData data))
			{
				isAssetLoaded = IsLoaded(data.Handle);
			}
			return isAssetLoaded;
		}

		[Obsolete("instead of 'C2VAddressable.LoadAsset'")]
		public T LoadAssetSync<T>(string addressableName, bool isInstantiated = false) where T : UnityEngine.Object
		{
			AssetSystemLog("LoadAssetSync called");
			return LoadAssetHandleSync<T>(addressableName, isInstantiated);
		}

		[Obsolete("instead of 'C2VAddressable.LoadAssetAsync'")]
		public async void LoadAssetAsync<T>(AssetReference assetRef, Action<T> completed, bool isInstantiated = false) where T : UnityEngine.Object
		{
			AssetSystemLog("LoadAssetAsync (AssetReference) called");
			await LoadAssetAsyncTask(assetRef, completed, isInstantiated);
		}


		[Obsolete("instead of 'C2VAddressable.LoadAssetAsync'")]
		public void LoadAssetAsync<T>(string addressableName, Action<T> completed, bool isInstantiated = false) where T : UnityEngine.Object
		{
			var handle = C2VAddressables.LoadAssetAsync<T>(addressableName);
			handle.OnCompleted += (opHandle) => completed?.Invoke(opHandle.Result);
			AssetSystemLog("LoadAssetAsync (addressableName) called");
			//await LoadAssetAsyncTask(addressableName, completed, isInstantiated);
		}

		[Obsolete("instead of 'C2VAddressable.LoadAssetAsync'")]
		public async UniTask<T> LoadAssetAsyncTask<T>(AssetReference assetRef, [CanBeNull] Action<T> completed = null, bool isInstantiated = false) where T : UnityEngine.Object
		{
			AssetSystemLog("LoadAssetAsyncTask (AssetReference) called");
#if UNITY_EDITOR
			DetailAssetSystemLog($"path : {AssetDatabase.GUIDToAssetPath(assetRef?.AssetGUID)}");
#endif // UNITY_EDITOR
			DetailAssetSystemLog($"AssetGUID : {assetRef?.AssetGUID}");
			return await LoadAssetHandle(assetRef?.AssetGUID, completed, isInstantiated);
		}

		[Obsolete("instead of 'C2VAddressable.LoadAssetAsync'")]
		public async UniTask<T> LoadAssetAsyncTask<T>(string addressableName, [CanBeNull] Action<T> completed = null, bool isInstantiated = false) where T : UnityEngine.Object
		{
			C2VDebug.LogError("로드가 되지 않았습니다. C2VAddressable.LoadAssetAsync를 사용해주세요");
			AssetSystemLog("LoadAssetAsyncTask (AddressableName) called");
			DetailAssetSystemLog($"addressableName : {addressableName}");
// #if UNITY_EDITOR
// 			DetailAssetSystemLog($"AssetGUID : {AssetDatabase.AssetPathToGUID(addressableName)}");
// #endif // UNITY_EDITOR
			return await LoadAssetHandle(addressableName, completed, isInstantiated);
		}
#endregion	// Load

#region Release
		
		public void Release()
		{
			AssetSystemLog("Release called");
			ReleaseAssetRefAll();
			_assetLoadingPool?.Clear();
			_assetLoadingPool = null;
		}
		
		public void ReleaseAssetRefAll()
		{
			AssetSystemLog("ReleaseAssetRefAll called");
			if (_assetDictionary != null)
			{
				foreach (var asset in _assetDictionary)
				{
					DetailAssetSystemLog($"key (AssetGUID/AddressableName) : {asset.Key}");
					ReleaseAssetHandleData(asset.Value);
				}	
			}
			_assetDictionary?.Clear();
			_assetDictionary = null;
		}
		
		public void ReleaseAssetRef(AssetReference assetRef)
		{
			AssetSystemLog("ReleaseAssetRef called");
			ReleaseAssetHandle(assetRef?.AssetGUID);
		}
		
		public void ReleaseAssetAddressableName(string addressableName)
		{
			AssetSystemLog("ReleaseAssetAddressableName called");
			ReleaseAssetHandle(addressableName);
		}

#endregion	// Release

#region CDN

		private void LoadContentCatalogAsync(string catalogPath)
		{
			AssetSystemLog($"catalogPath : {catalogPath}");
			AssetSystemLog($"Addressables.ResourceLocators.Count : {Addressables.ResourceLocators.Count()}");
			Addressables.LoadContentCatalogAsync(catalogPath, true).Completed += handle =>
			{
				AssetSystemLog("Addressables.LoadContentCatalogAsync - Completed");
				if (handle.Status == AsyncOperationStatus.Succeeded)
				{
					AssetSystemLog("Addressables.LoadContentCatalogAsync - Succeeded");
					var resourceLocator = handle.Result;
					if (resourceLocator != null)
					{
						AssetSystemLog($"LocatorId : {resourceLocator.LocatorId}");
						// foreach (var key in resourceLocator.Keys)
						// {
						// 	AssetSystemLog($"key : {key}");
						// }
						AssetSystemLog($"Addressables.ResourceLocators.Count : {Addressables.ResourceLocators.Count()}");
					}
				}
				else if (handle.Status == AsyncOperationStatus.Failed)
				{
					AssetSystemLog("Addressables.LoadContentCatalogAsync - Failed");
				}
			};
		}
		private void ClearCacheAll()
		{
			AssetSystemLog("ClearCacheAll called");
			//TODO: Mr.Song - 해당 api 정리 및 사용 고려
			// Unloads all currently loaded AssetBundles
			// AssetBundle.UnloadAllAssetBundles(true);
			
			// 참고
			// 다운로드 받은 에셋 번들을 강제로 삭제 한다.
			// Unity 시스템 전체 cache 이기 때문에 다른 부분도 영향 미침.
			// 메모리에 올라와 있는 경우에는 삭제가 안되는듯 함. (최초 실행시점에는 정상 동작함)
			Caching.ClearCache();
			// Caching.ClearCache((int) Time.realtimeSinceStartup + 10);
		}

		private void ClearCache(string key)
		{
			AssetSystemLog("ClearCache called");
			AssetSystemLog($"key : {key}");
			//TODO: Mr.Song - 해당 api 정리 및 사용 고려
			
			// cache file path (Editor 기준)
			// ex) C:\Users\user\AppData\LocalLow\Unity\Com2Verse_MetaversePlatform\~~~
			
			// ClearDependencyCacheAsync 호출 전에, 사용중인 res를(해당 group 군) 모두 release 해야 정상 동작함.
			
			// Unloads all currently loaded AssetBundles
			// AssetBundle.UnloadAllAssetBundles(true);
			// ReleaseAssetAddressableName(key);
			try
			{
				var clearDependencyCacheHandle = Addressables.ClearDependencyCacheAsync(key, true);
				clearDependencyCacheHandle.Completed += (handle) =>
				{
					AssetSystemLog("ClearCache - Completed");
					if (handle.Status == AsyncOperationStatus.Succeeded)
					{
						AssetSystemLog("ClearCache - Succeeded");
						AssetSystemLog($"handle.Result : {handle.Result}");	// bool
					}
					else if (handle.Status == AsyncOperationStatus.Failed)
					{
						AssetSystemLog("ClearCache - Failed");
#if UNITY_EDITOR
						if (handle.OperationException != null)
						{
							var e = handle.OperationException;
							var exceptionType = e.GetType().ToString();
							DetailAssetSystemLog($"e.GetType() : {exceptionType}");
							DetailAssetSystemLog($"e.Message : {e.Message}");
						}
#endif	// UNITY_EDITOR
					}
				};
			}
			catch (Exception e)
			{
				AssetSystemLog($"Exception : {e}");
			}
		}

		private async UniTask<long> GetDownloadSize(string key)
		{
			AssetSystemLog("GetDownloadSize called");
			// "0" is already downloaded
			long downloadSize = 0;
			if (string.IsNullOrEmpty(key)) { return await UniTask.FromResult(downloadSize); }
			DetailAssetSystemLog($"key : {key}");
			bool completed = false;
			AsyncOperationHandle<long> getDownloadSizeHandle = Addressables.GetDownloadSizeAsync(key);
			getDownloadSizeHandle.Completed += (handle) =>
			{
				AssetSystemLog("GetDownloadSize - Completed");
				if (handle.Status == AsyncOperationStatus.Succeeded)
				{
					AssetSystemLog("GetDownloadSize - Succeeded");
					downloadSize = handle.Result;
					AssetSystemLog($"downloadSize : {downloadSize}");
					// AssetSystemLog($"DebugName : {handle.DebugName}");
					DetailAssetSystemLog($"IsDone : {handle.IsDone}");
					DetailAssetSystemLog($"PercentComplete : {handle.PercentComplete}");
					
					var downloadStatus = handle.GetDownloadStatus();
					// The number of bytes downloaded by the operation and all of its dependencies.
					DetailAssetSystemLog($"DownloadStatus::TotalBytes : {downloadStatus.TotalBytes}");
					// The total number of bytes needed to download by the operation and dependencies.
					DetailAssetSystemLog($"DownloadStatus::DownloadedBytes : {downloadStatus.DownloadedBytes}");
					// Is the operation completed.  This is used to determine if the computed Percent should be 0 or 1 when TotalBytes is 0.
					DetailAssetSystemLog($"DownloadStatus::IsDone : {downloadStatus.IsDone}");
					// Returns the computed percent complete as a float value between 0 &amp; 1.  If TotalBytes == 0, 1 is returned.
					DetailAssetSystemLog($"DownloadStatus::Percent : {downloadStatus.Percent}");
					
					if (downloadStatus.IsDone)
					{
						completed = true;
					}
				}
				else if (handle.Status == AsyncOperationStatus.Failed)
				{
					//TODO: Mr.Song - 예외처리 보강 필요.
					AssetSystemLog("GetDownloadSize - Failed");
#if UNITY_EDITOR
					if (handle.OperationException != null)
					{
						var e = handle.OperationException;
						var exceptionType = e.GetType().ToString();
						DetailAssetSystemLog($"e.GetType() : {exceptionType}");
						DetailAssetSystemLog($"e.Message : {e.Message}");
					}
#endif	// UNITY_EDITOR
				}
			};
			await UniTask.WaitUntil(() => completed);
			Addressables.Release(getDownloadSizeHandle);
			return await UniTask.FromResult(downloadSize);
		}
		
		private async UniTask Download(string key)
		{
			AssetSystemLog("Download called");
			if (string.IsNullOrEmpty(key)) { return; }
			DetailAssetSystemLog($"key : {key}");
			bool completed = false;
			AsyncOperationHandle downloadHandle  = Addressables.DownloadDependenciesAsync(key, true);
			downloadHandle.Completed += (handle) =>
			{
				AssetSystemLog("Download - Completed");
				if (handle.Status == AsyncOperationStatus.Succeeded)
				{
					AssetSystemLog("Download - Succeeded");
					// DetailAssetSystemLog($"Result : {handle.Result}");
					// DetailAssetSystemLog($"DebugName : {handle.DebugName}");
					DetailAssetSystemLog($"IsDone : {handle.IsDone}");
					DetailAssetSystemLog($"PercentComplete : {handle.PercentComplete}");
					
					var downloadStatus = handle.GetDownloadStatus();
					// The number of bytes downloaded by the operation and all of its dependencies.
					DetailAssetSystemLog($"DownloadStatus::TotalBytes : {downloadStatus.TotalBytes}");
					// The total number of bytes needed to download by the operation and dependencies.
					DetailAssetSystemLog($"DownloadStatus::DownloadedBytes : {downloadStatus.DownloadedBytes}");
					// Is the operation completed.  This is used to determine if the computed Percent should be 0 or 1 when TotalBytes is 0.
					DetailAssetSystemLog($"DownloadStatus::IsDone : {downloadStatus.IsDone}");
					// Returns the computed percent complete as a float value between 0 &amp; 1.  If TotalBytes == 0, 1 is returned.
					DetailAssetSystemLog($"DownloadStatus::Percent : {downloadStatus.Percent}");
					
					if (downloadStatus.IsDone)
					{
						completed = true;
					}
				}
				else if (handle.Status == AsyncOperationStatus.Failed)
				{
					AssetSystemLog("Download - Failed");
#if UNITY_EDITOR
					if (handle.OperationException != null)
					{
						var e = handle.OperationException;
						var exceptionType = e.GetType().ToString();
						DetailAssetSystemLog($"e.GetType() : {exceptionType}");
						DetailAssetSystemLog($"e.Message : {e.Message}");
					}
#endif	// UNITY_EDITOR
				}
			};
			await UniTask.WaitUntil(() => completed);
			// Addressables.Release(downloadHandle);	// auto release
		}
#endregion	// CDN

#endregion	// Public
		
		//////////////////////////////////////////
		// Internal

#region Internal

#region Log
		[Conditional("ASSET_SYSTEM_LIFE_CYCLE_LOG"), Conditional("SONG_TEST")]
		private static void AssetSystemLifeCycleLog(string msg)
		{
			C2VDebug.Log("[AssetSystem] " + msg);
		}
		[Conditional("ASSET_SYSTEM_LOG"), Conditional("SONG_TEST")]
		private static void AssetSystemLog(string msg)
		{
			C2VDebug.Log("[AssetSystem] " + msg);
		}
		
		[Conditional("DETAIL_ASSET_SYSTEM_LOG"), Conditional("SONG_TEST")]
		private static void DetailAssetSystemLog(string msg)
		{
			C2VDebug.Log("[AssetSystem] " + msg);
		}
#endregion	// Log
		
#region Init
		
		[RuntimeInitializeOnLoadMethod]
		private static void SetInternalIdTransform()
		{
			AssetSystemLifeCycleLog("SetInternalIdTransform called");
			
			//FIXME: Mr.Song - appinfo 로 서버 타입에 따라 사용 여부 처리를 해야함. (remote)
			// if (AppInfo.Instance == null || AppInfo.Instance.Data.AssetBuildType == eAssetBuildType.LOCAL) return;
			// AssetSystemLifeCycleLog("InternalIdTransformFunc use");
			// Addressables.InternalIdTransformFunc = InternalIdTransformFunc;
			
			// Addressables.WebRequestOverride = EditWebRequestURL;
		}
		
		private static string InternalIdTransformFunc(IResourceLocation location)
		{
			// DetailAssetSystemLog("InternalIdTransformFunc called");
			if (!_useInternalIdTransformFunc || string.IsNullOrEmpty(_path) || _path.Split(URL_DELIMITER).Length < 2) return location.InternalId;
			var oldLocation = location.InternalId;
			var newLocation = oldLocation;
			if (oldLocation.StartsWith("http"))
			{
				//TODO: Mr.Song - remote, remote test 구분 하여 작업 고려. - AppInfo.Instance.Data.AssetBuildType
				// var patchVersionUrl = "https://metaverse-platform-fn.qpyou.cn/metaverse-platform/Test/AssetBundle/StandaloneWindows64/0.1.0/0.0.1";
				// var appVersionUrl = "https://metaverse-platform-fn.qpyou.cn/metaverse-platform/Test/AssetBundle/StandaloneWindows64/0.1.0";
				var patchVersionUrl = _path;
				var appVersionUrl = _path.Substring(0, _path.LastIndexOf(URL_DELIMITER));
				DetailAssetSystemLog($"patchVersionUrl - {patchVersionUrl}");
				DetailAssetSystemLog($"appVersionUrl - {appVersionUrl}");
				var fileName = Path.GetFileName(oldLocation);
				var extension = Path.GetExtension(fileName);
				
				// catalog
				if(fileName.Contains("catalog_") && (extension.Equals(".json") || extension.Equals(".hash")))
				{
					// https://metaverse-platform-fn.qpyou.cn/metaverse-platform/Test/AssetBundle/StandaloneWindows64/0.1.0/0.0.1/catalog_metaverse_remote_test.hash
					// https://metaverse-platform-fn.qpyou.cn/metaverse-platform/Test/AssetBundle/StandaloneWindows64/0.1.0/0.0.1/catalog_metaverse_remote_test.json
					newLocation = patchVersionUrl + URL_DELIMITER + fileName;
				}
				// asset bundle
				else if (location.ResourceType == typeof(IAssetBundleResource))
				{
					// https://metaverse-platform-fn.qpyou.cn/metaverse-platform/Test/AssetBundle/StandaloneWindows64/0.1.0/100001_artasset_atlas_assets_all_6c78ad660da2e0ca5ff5befe93118ec8.bundle
					newLocation = appVersionUrl + URL_DELIMITER + fileName;
				}
			}
						
			DetailAssetSystemLog($"InternalIdTransformFunc - oldLocation : {oldLocation}");
			DetailAssetSystemLog($"InternalIdTransformFunc - newLocation : {newLocation}");
			return newLocation;
		}
		
		private static void EditWebRequestURL(UnityWebRequest request)
		{
			//TODO: Mr.Song - 코드 정리 및 주석 정리 필요.
			// 사용 여부도 고려 필요.
			AssetSystemLog($"request.url : {request.url}");
			if (request.url.EndsWith(".bundle"))
			{
			}
			else if (request.url.EndsWith(".json") || request.url.EndsWith(".hash"))
			{
			}
		}
#endregion	// Init
		
#region Asset
		private void ReleaseAssetHandle(string key)
		{
			AssetSystemLog("ReleaseAssetHandle called");
			if (string.IsNullOrEmpty(key)) {return;}
			DetailAssetSystemLog($"key : {key}");
			if (_assetDictionary != null && _assetDictionary.TryGetValue(key, out AssetData data))
			{
				ReleaseAssetHandleData(data);
				_assetDictionary.Remove(key);
				DetailAssetSystemLog($"assetRefDictionary.Count : {_assetDictionary.Count}");
			}
			else
			{
				DetailAssetSystemLog("ReleaseAssetHandle fail case");
			}
		}

		private void ReleaseAssetHandleData(AssetData data)
		{
			DetailAssetSystemLog("ReleaseAssetHandleData called");
			if (data.Handle.IsValid())
			{
				if (data.IsInstantiated)
				{
					DetailAssetSystemLog($"Addressables.ReleaseInstance call");
					Addressables.ReleaseInstance(data.Handle);
				}
				else
				{
					DetailAssetSystemLog($"Addressables.Release call");
					Addressables.Release(data.Handle);	
				}
			}
			else
			{
				DetailAssetSystemLog($"handle is null/invalid");
			}
		}

		private static readonly float TIMEOUT_LOAD = 2;
		private IEnumerator TimeoutLoad(string key)
		{
			yield return new WaitForSeconds(TIMEOUT_LOAD);
			if (_assetLoadingPool != null && _assetLoadingPool.ContainsKey(key))
			{
				_assetLoadingPool[key] = true;
			}
		}

		private T LoadAssetHandleSync<T>(string key, bool isInstantiated = false) where T : UnityEngine.Object
		{
			AssetSystemLog("LoadAssetHandleSync called");
			AssetSystemLog($"isInstantiated : {isInstantiated}");
			// if (!_isInitialized) { await UniTask.WaitUntil(() => _isInitialized); }
			if (string.IsNullOrEmpty(key) || _assetLoadingPool == null || _assetDictionary == null) { return null; }
			DetailAssetSystemLog($"key : {key}");

			// already included
			if (_assetDictionary.TryGetValue(key, out AssetData data))
			{
				AssetSystemLog("LoadAssetHandleSync - case : already included");
				data.Handle.WaitForCompletion();
				return (T) data.Handle.Result;
			}
			// add
			else
			{
				AssetSystemLog("LoadAssetHandleSync - case : add");
				DetailAssetSystemLog($"_assetLoadingPool.Count : {_assetLoadingPool?.Count}");
				if (_assetLoadingPool?.ContainsKey(key) != true)
				{
					// add
					_assetLoadingPool?.Add(key, false);

					// laod
					AsyncOperationHandle handle;
					// - load (with download) + Instantiate
					if (isInstantiated) { handle = Addressables.InstantiateAsync(key); }
					// - load (with download)
					else { handle = Addressables.LoadAssetAsync<T>(key); }
					handle.Completed += (asyncOperationHandle) =>
					{
						AssetSystemLog("LoadAssetAsync - Completed");
						if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
						{
							AssetSystemLog("LoadAssetAsync - Succeeded");
							if (_assetDictionary.TryAdd(key, new AssetData(asyncOperationHandle, isInstantiated)))
							{
								AssetSystemLog($"_assetDictionary.Add Success");
								DetailAssetSystemLog($"assetRefDictionary.Count : {_assetDictionary.Count}");
							}
							else
							{
								AssetSystemLog($"_assetDictionary.Add Fail");
							}

							_assetLoadingPool?.Remove(key);
						}
						else if (asyncOperationHandle.Status == AsyncOperationStatus.Failed)
						{
							AssetSystemLog("LoadAssetAsync - Failed");
							_assetLoadingPool?.Remove(key);
#if UNITY_EDITOR
							if (asyncOperationHandle.OperationException != null)
							{
								DetailAssetSystemLog("OperationException case");
								var e = asyncOperationHandle.OperationException;
								var exceptionType = e.GetType().ToString();
								DetailAssetSystemLog($"e.GetType() : {exceptionType}");
								DetailAssetSystemLog($"e.Message : {e.Message}");
								if (exceptionType.Equals("UnityEngine.AddressableAssets.InvalidKeyException"))
								{
									//TODO: Mr.Song - 예외 처리 추가 고려.
									// ShowPopupCommon 에서 필요한 asset 이 없을 경우
									// "Assets/Project/Bundles/Com2us/ArtAsset/Prefabs/UI_Popup_Confirm.prefab"
									var msg = "Addressables Group 에 포함되지 않은 Asset load\n";
									msg += "Metaverse -> AssetSystem -> Group Recreate 를 통해 추가 가능\n";
									msg += $"Error Msg : {e.Message}\n";
									// UIManager.Instance.ShowPopupCommon(msg);
									C2VDebug.LogError("[AssetSystem] : " + msg);
								}
							}
#endif // UNITY_EDITOR
						}
					};
					handle.WaitForCompletion();
					return (T)handle.Result;
				}
			}
			return null;
		}

		private async UniTask<T> LoadAssetHandle<T>(string key, [CanBeNull] Action<T> completed = null, bool isInstantiated = false) where T : UnityEngine.Object
		{
			AssetSystemLog("LoadAssetHandle called");
			AssetSystemLog($"isInstantiated : {isInstantiated}");
			if (!_isInitialized) { await UniTask.WaitUntil(() => _isInitialized); }
			if (string.IsNullOrEmpty(key)) {return await UniTask.FromResult<T>(null);}
			if (_assetLoadingPool == null)
			{
				// bug flow.
				AssetSystemLog($"_assetLoadingPool - null case");
				return await UniTask.FromResult<T>(null);
			}
			DetailAssetSystemLog($"key : {key}");

			if (_assetDictionary != null)
			{
				// already included
				if (_assetDictionary.TryGetValue(key, out AssetData data))
				{
					AssetSystemLog("LoadAssetHandle - case : already included");
					if (data.Handle.IsValid())
					{
						DetailAssetSystemLog($"handleOut.IsValid() : {data.Handle.IsValid()}");
						DetailAssetSystemLog($"handleOut.IsDone : {data.Handle.IsDone}");
						DetailAssetSystemLog($"handleOut.Status : {data.Handle.Status}");
					}
					completed?.Invoke((T) data.Handle.Result);
					return await UniTask.FromResult((T) data.Handle.Result);
				}
				// add
				else
				{
					AssetSystemLog("LoadAssetHandle - case : add");
					DetailAssetSystemLog($"_assetLoadingPool.Count : {_assetLoadingPool?.Count}");
					// foreach (var loadPoolAsset in _assetLoadingPool) {AssetSystemLog($"guid : {loadPoolAsset}");}
					if (_assetLoadingPool?.ContainsKey(key) != true)
					{
						// add
						_assetLoadingPool?.Add(key, false);
						
						// laod
						AsyncOperationHandle handle;
						// - load (with download) + Instantiate
						if (isInstantiated) { handle = Addressables.InstantiateAsync(key); }
						// - load (with download)
						else { handle = Addressables.LoadAssetAsync<T>(key); }
						handle.Completed += (asyncOperationHandle) =>
						{
							AssetSystemLog("LoadAssetAsync - Completed");
							if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
							{
								AssetSystemLog("LoadAssetAsync - Succeeded");
								if (_assetDictionary.TryAdd(key, new AssetData(asyncOperationHandle, isInstantiated)))
								{
									AssetSystemLog($"_assetDictionary.Add Success");
									DetailAssetSystemLog($"assetRefDictionary.Count : {_assetDictionary.Count}");
								}
								else
								{
									AssetSystemLog($"_assetDictionary.Add Fail");
								}
								completed?.Invoke((T)handle.Result);
								_assetLoadingPool?.Remove(key);
							}
							else if (asyncOperationHandle.Status == AsyncOperationStatus.Failed)
							{
								AssetSystemLog("LoadAssetAsync - Failed");
								completed?.Invoke((T)handle.Result);
								_assetLoadingPool?.Remove(key);
#if UNITY_EDITOR
								if (asyncOperationHandle.OperationException != null)
								{
									DetailAssetSystemLog("OperationException case");
									var e = asyncOperationHandle.OperationException;
									var exceptionType = e.GetType().ToString();
									DetailAssetSystemLog($"e.GetType() : {exceptionType}");
									DetailAssetSystemLog($"e.Message : {e.Message}");
									if (exceptionType.Equals("UnityEngine.AddressableAssets.InvalidKeyException"))
									{
										//TODO: Mr.Song - 예외 처리 추가 고려.
										// ShowPopupCommon 에서 필요한 asset 이 없을 경우
										// "Assets/Project/Bundles/Com2us/ArtAsset/Prefabs/UI_Popup_Confirm.prefab"
										var msg = "Addressables Group 에 포함되지 않은 Asset load\n";
										msg += "Metaverse -> AssetSystem -> Group Recreate 를 통해 추가 가능\n";
										msg += $"Error Msg : {e.Message}\n";
										// UIManager.Instance.ShowPopupCommon(msg);
										C2VDebug.LogError("[AssetSystem] : " + msg);
									}
								}
#endif	// UNITY_EDITOR
							}
						};
						return (T)(await handle.Task.AsUniTask());
					}
					else
					{
						AssetSystemLog("LoadAssetAsync - Load Wait");
						// load wait
						StartCoroutine(TimeoutLoad(key));
						await UniTask.WaitUntil(() =>
						{
							var hasDict = _assetDictionary.ContainsKey(key);
							var hasPool = _assetLoadingPool.ContainsKey(key);
							// case : load success
							if (hasDict) { return true; }
							// case : load fail, timeout
							else if (hasPool && _assetLoadingPool.TryGetValue(key,out bool isTimeout)) { return isTimeout; }
							// case : removed or bug
							else if (!hasPool) { return true; }
							return false;
						});
						
						if (_assetDictionary.TryGetValue(key, out AssetData data2))
						{
							AssetSystemLog("LoadAssetAsync - Load after ");
							if (data2.Handle.IsValid())
							{
								DetailAssetSystemLog($"handleOut.IsValid() : {data2.Handle.IsValid()}");
								DetailAssetSystemLog($"handleOut.IsDone : {data2.Handle.IsDone}");
								DetailAssetSystemLog($"handleOut.Status : {data2.Handle.Status}");
							}
							completed?.Invoke((T) data2.Handle.Result);
							return await UniTask.FromResult((T) data2.Handle.Result);
						}
					}
				}
			}
			AssetSystemLog("LoadAssetHandle - fail case");
			return await UniTask.FromResult<T>(null);
		}

		private bool IsLoaded(AsyncOperationHandle handle)
		{
			AssetSystemLog("IsLoaded (AsyncOperationHandle) called");
			bool isAssetLoaded = handle.IsValid() && handle.IsDone && (handle.Status == AsyncOperationStatus.Succeeded);
			if (handle.IsValid())
			{
				DetailAssetSystemLog($"handle.IsValid() : {handle.IsValid()}");
				DetailAssetSystemLog($"handle.IsDone : {handle.IsDone}");
				DetailAssetSystemLog($"handle.Status : {handle.Status}");
			}
			DetailAssetSystemLog($"isAssetLoaded : {isAssetLoaded}");
			return isAssetLoaded;
		}

		private AsyncOperationHandle? GetAssetHandle(string key)
		{
			AssetSystemLog("GetAssetHandle called");
			if (string.IsNullOrEmpty(key)) { return null; }
			DetailAssetSystemLog($"key (AssetGUID/AddressableName) : {key}");
			if (_assetDictionary != null && _assetDictionary.TryGetValue(key, out AssetData data))
			{
				return data.Handle;
			}
			return null;
		}
#endregion	// Asset

#region CDN
		
		//////////////////////////////////////////
		// CDN URL Change

		private enum eResultCode
		{
			SUCCESS = 0,
		};

		private class PatchUrlData
		{
			public string _id;
			public string _name;
			public string _path;
			public PatchUrlData(string id, string name, string path)
			{
				_id = id;
				_name = name;
				_path = path;
			}
		}

		[Serializable]
		private class CompanyData
		{
			// [
			// {
			// 	"METAVERSE_ID": 11,
			// 	"METAVERSE_NAME": "컴투버스",
			// 	"USE_YN": "Y"
			// }
			// ]
			[JsonProperty(PropertyName = "METAVERSE_ID")]
			public string _id;
			[JsonProperty(PropertyName = "METAVERSE_NAME")]
			public string _name;
			[JsonProperty(PropertyName = "USE_YN")]
			public string _use;
		}
		
		[Serializable]
		private class PatchVersionData
		{
			// {
			// 	"MetaversePath": "https://metaverse-platform-fn.qpyou.cn/metaverse-platform/Test/AssetBundle/StandaloneWindows64/0.1.0/0.0.1",
			// 	"PatchVersion": "0.0.1",
			// 	"RSLT_CD": "0000",
			// 	"RSLT_MSG": "성공"
			// }
			// {
			// 	"MetaversePath": null,
			// 	"PatchVersion": null,
			// 	"RSLT_CD": "9999",
			// 	"RSLT_MSG": "데이터가 없습니다."
			// }
			// https://metaverse-platform-fn.qpyou.cn/metaverse-platform/{ASSET_TYPE}/{BUILD_TARGET}/{APP_VERSION}/{PATCH_VERSION}
			// {ASSET_TYPE} : AssetBundle
			// {BUILD_TARGET} : StandaloneWindows64
			// {APP_VERSION} : 1.0.0
			// {PATCH_VERSION} : 1.0.0
			[JsonProperty(PropertyName = "MetaversePath")]
			public string _path;
			[JsonProperty(PropertyName = "PatchVersion")]
			public string _version;
			[JsonProperty(PropertyName = "RSLT_CD")]
			public string _resultCode;
			[JsonProperty(PropertyName = "RSLT_MSG")]
			public string _resultMessage;
		}

		[Serializable]
		private class GetRequestAssetInfoData
		{
			// {
			// 	"METAVERSE_ID": "1",
			// 	"BUILD_TARGET": "StandaloneWindows64",
			// 	"USE_YN": "Y",
			// 	"PAGE_SIZE": 1000,
			// 	"PAGE_NUM": 1
			// }
			[JsonProperty(PropertyName = "METAVERSE_ID")]
			public string _id;
			[JsonProperty(PropertyName = "BUILD_TARGET")]
			public string _target;
			[JsonProperty(PropertyName = "USE_YN")]
			public string _use;
			public GetRequestAssetInfoData(string id, string target, string use)
			{
				_id = id;
				_target = target;
				_use = use;
			}
		}
		
		[Serializable]
		private class PutRequestAssetInfoData
		{
			// {
			// 	"METAVERSE_PATH": "https://metaverse-platform-fn.qpyou.cn/metaverse-platform/Test/{ASSET_TYPE}/{BUILD_TARGET}/{APP_VERSION}/{PATCH_VERSION}",
			// 	"USE_YN": "Y"
			// }
			[JsonProperty(PropertyName = "METAVERSE_PATH")]
			public string _path;
			[JsonProperty(PropertyName = "USE_YN")]
			public string _use;
			public PutRequestAssetInfoData(string path, string use)
			{
				_path = path;
				_use = use;
			}
		}

		[Serializable]
		private class AssetInfoData
		{
			// [
			// 	{
			// 		"ASSET_ID": 35,
			// 		"METAVERSE_ID": 1,
			// 		"METAVERSE_NAME": "컴투스",
			// 		"METAVERSE_PATH": "https://metaverse-platform-fn.qpyou.cn/metaverse-platform/Test/{ASSET_TYPE}/{BUILD_TARGET}/{APP_VERSION}/{PATCH_VERSION}",
			// 		"ASSET_TYPE": "AssetBundle",
			// 		"BUILD_TARGET": "StandaloneWindows64",
			// 		"APP_VERSION": "0.1.0",
			// 		"PATCH_VERSION": "0.0.1",
			// 		"USE_YN": "Y",
			// 		"CREATE_DATETIME": "2022-10-20T12:03:08",
			// 		"UPDATE_DATETIME": "2022-10-24T17:53:23",
			// 		"ERROR_CODE": "",
			// 		"ERROR_MESSAGE": ""
			// 	},
			// ]
			//TODO: Mr.Song - error code enum ?
			// "ERROR_CODE": "1062",
			// "ERROR_MESSAGE": "duplicate keys on asset column metaverse_id, asset_type, build_target, app_version, patch_version found"
			// ERROR_CODE : 1644
			// ERROR_MESSAGE : 10000 : metaverse_id not found in metaverse table
			[JsonProperty(PropertyName = "ASSET_ID")]
			public string _assetId;
			[JsonProperty(PropertyName = "METAVERSE_ID")]
			public string _id;
			[JsonProperty(PropertyName = "METAVERSE_NAME")]
			public string _name;
			[JsonProperty(PropertyName = "METAVERSE_PATH")]
			public string _path;
			[JsonProperty(PropertyName = "ASSET_TYPE")]
			public string _assetType;
			[JsonProperty(PropertyName = "BUILD_TARGET")]
			public string _buildTarget;
			[JsonProperty(PropertyName = "APP_VERSION")]
			public string _appVersion;
			[JsonProperty(PropertyName = "PATCH_VERSION")]
			public string _patchVersion;
			[JsonProperty(PropertyName = "USE_YN")]
			public string _use;
			[JsonProperty(PropertyName = "CREATE_DATETIME")]
			public string _createDateTime;
			[JsonProperty(PropertyName = "UPDATE_DATETIME")]
			public string _updateDateTime;
			[JsonProperty(PropertyName = "ERROR_CODE")]
			public string _errorCode;
			[JsonProperty(PropertyName = "ERROR_MESSAGE")]
			public string _errorMessage;
		}
		
		[Serializable]
		private class PostRequestAssetInfoData
		{
			[JsonProperty(PropertyName = "METAVERSE_ID")]
			public string _id;
			[JsonProperty(PropertyName = "METAVERSE_PATH")]
			public string _path;
			[JsonProperty(PropertyName = "ASSET_TYPE")]
			public string _assetType;
			[JsonProperty(PropertyName = "BUILD_TARGET")]
			public string _buildTarget;
			[JsonProperty(PropertyName = "APP_VERSION")]
			public string _appVersion;
			[JsonProperty(PropertyName = "PATCH_VERSION")]
			public string _patchVersion;
			[JsonProperty(PropertyName = "USE_YN")]
			public string _use;
		}

		[Serializable]
		private class BodyItems
		{
			public int BodyId;
			public int BodyKey;
			public int BodyValue;
			public int BodyR;
			public int BodyG;
			public int BodyB;
		}

		[Serializable]
		private class FashionItems
		{
			public int FashionId;
			public int FashionR;
			public int FashionG;
			public int FashionB;
		}
		
		[Serializable]
		private class AvatarInfoData
		{
			// [
			// {
			// 	"AvatarId": 96,
			// 	"AvatarType": 15,
			// 	"BodyItems": [
			// 	{
			// 		"BodyId": 1515001,
			// 		"BodyKey": 0,
			// 		"BodyValue": 50,
			// 		"BodyR": 137,
			// 		"BodyG": 137,
			// 		"BodyB": 137
			// 	},
			// 	],
			// 	"FashionItems": [
			// 	{
			// 		"FashionId": 1506000,
			// 		"FashionR": 0,
			// 		"FashionG": 0,
			// 		"FashionB": 0
			// 	}
			// 	],
			// 	"RSLT_CD": "0000",
			// 	"RSLT_MSG": "성공"
			// }
			// ]
			
			
			[JsonProperty(PropertyName = "AvatarId")]
			public int _avatarId;
			[JsonProperty(PropertyName = "AvatarType")]
			public int _avatarType;
			[JsonProperty(PropertyName = "BodyItems")]
			public List<BodyItems> _bodyItems;
			[JsonProperty(PropertyName = "FashionItems")]
			public List<FashionItems> _fashionItems;
			[JsonProperty(PropertyName = "RSLT_CD")]
			public string _resultCode;
			[JsonProperty(PropertyName = "RSLT_MSG")]
			public string _resultMessage;
		}
		
		private static string GetUse(bool use) => use ? "Y" : "N";
		
		private static async void InitializeHttpClient(bool updateCompanyDataList = true)
		{
			AssetSystemLog("InitializeHttpClient called");
			if (_httpClient != null) { return; }
			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type","application/json");
			_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("charset","utf-8");
			if (updateCompanyDataList)
			{
				await UpdateCompanyDataList();	
			}
		}

		private static void UpdatePatchInfo()
		{
			AssetSystemLog("UpdatePatchInfo called");
			
			// company id
			//TODO: Mr.song - update current companyID
			// _companyId = GetCurrentCompanyCode() ?? DEFAULT_COMPANY_ID;
			_companyId = DEFAULT_COMPANY_ID;
			AssetSystemLog($"_companyId : {_companyId}");

			// path
			// ex) https://metaverse-platform-fn.qpyou.cn/metaverse-platform/Test/AssetBundle/StandaloneWindows64/0.1.0/0.0.1;
			if (_patchUrlData?.Count <= 0) return;
			var data = _patchUrlData?.Find(x => x._id == _companyId.ToString());
			if (data == null || string.IsNullOrEmpty(data._path)) return;
			_path = data._path.TrimEnd(URL_DELIMITER);
			AssetSystemLog($"_path : {_path}");
			_useInternalIdTransformFunc = true;
		}
		
		private static string GetServerApi()
		{
			// https://public-dev-service.com2verse.com/api/swagger/index.html
			// https://public-dev-service.com2verse.com/asset
			
			//TODO: Mr.Song - url - config file (Config.json 고려)
			var url = "https://public-dev-service.com2verse.com/api/";
			
			//FIXME: Mr.Song - appinfo 에 따라 base url 다르게 처리 (외부로 빼던지 해야 할듯.) 
			// if (AppInfo.Instance != null)
			// {
			// 	switch (AppInfo.Instance.Data.Environment)
			// 	{
			// 		case AppInfoData.Env.DEV:
			// 			url = "https://public-dev-service.com2verse.com/api/";
			// 			break;
			// 		case AppInfoData.Env.QA:
			// 			url = "https://qa-api.com2verse.com/api/";
			// 			break;
			// 		case AppInfoData.Env.STAGING:
			// 			url = "https://staging-api.com2verse.com/api/";
			// 			break;
			// 		case AppInfoData.Env.PRODUCTION:
			// 			url = "https://alpha-api.com2verse.com/api/";
			// 			break;
			// 	}
			// }
			return url;
		}
		
		private static string GetCommonServerApi()
		{
			return GetServerApi() + "Common/";
		}
		
		private static string GetAssetServerApi()
		{
			return GetServerApi() + "Asset/";
		}
		
		private static string GetAvatarServerApi()
		{
			return GetServerApi() + "Avatar/";
		}

		private static string GetCompanyDataListServerApi()
		{
			// get : /api/Common/Metaverse/List
			// https://public-dev-service.com2verse.com/api/Common/Metaverse/List
			return GetCommonServerApi() + "Metaverse/List";
		}
		
		private static string GetAssetListServerApi()
		{
			// post : /api/Asset/List
			// https://public-dev-service.com2verse.com/api/Asset/list
			return GetAssetServerApi() + "List";
		}
		
		private static string GetPatchUrlServerApi(string companyId)
		{
			// get : /api/Asset/{MetaversId}/{AssetType}/{BuildTarget}/{AppVersion}
			DetailAssetSystemLog("UpdateCompanyDataList called");
			DetailAssetSystemLog($"companyId : {companyId}");
			// https://public-dev-service.com2verse.com/api/Asset/1/AssetBundle/StandaloneWindows64/0.1.0
			var assetType = "AssetBundle";
			
			//FIXME: Mr.Song - appinfo 정보에 따라 처리 필요.
			// var buildTarget = AppInfo.Instance != null ? AppInfo.Instance.Data.BuildTarget : "StandaloneWindows64";
			// var appVersion = AppInfo.Instance != null ? AppInfo.Instance.Data.Version : Application.version;
			var buildTarget = "StandaloneWindows64";
			var appVersion = Application.version;
			var url = GetAssetServerApi() + $"{companyId}/{assetType}/{buildTarget}/{appVersion}";
			DetailAssetSystemLog($"url : {url}");
			return url;
		}

		// private static async UniTask<int?> GetCompanyCode(long userId)
		// {
		// 	return (await DataManager.Instance.GetEmployeeByAccountIdAsync(userId)).GetPrimaryWork().Department.CompanyCode;
		// }
		
		// private static async UniTask<int?> GetCurrentCompanyCode()
		// {
		// 	return Network.User.Instance ? await GetCompanyCode(Network.User.Instance.ID) : null;
		// }

		private static async UniTask UpdateCompanyDataList()
		{
			AssetSystemLog("UpdateCompanyDataList called");
			_patchUrlData?.Clear();
			_patchUrlData = new List<PatchUrlData>();
			try
			{
				var response = await _httpClient.GetAsync(GetCompanyDataListServerApi());
				if (response.StatusCode == HttpStatusCode.OK)
				{
					DetailAssetSystemLog("UpdateCompanyDataList : success case");
					var responseBody = await response.Content.ReadAsStringAsync();
					if (!string.IsNullOrEmpty(responseBody))
					{
						DetailAssetSystemLog($"responseBody : {responseBody}");
						var companyDataList = JsonConvert.DeserializeObject<List<CompanyData>>(responseBody);
						if (companyDataList != null)
						{
							foreach (var companyData in companyDataList)
							{
								DetailAssetSystemLog($"id : {companyData._id}");
								DetailAssetSystemLog($"name : {companyData._name}");
								DetailAssetSystemLog($"use : {companyData._use}");
								var companyId = companyData._id;
								var companyName = companyData._name;
								var path = await GetPatchUrl(companyId);
								_patchUrlData.TryAdd(new PatchUrlData(companyId, companyName, path));
								DetailAssetSystemLog($"path : {path}");
							}
						}	
					}
				}
				else
				{
					// fail case
					DetailAssetSystemLog("UpdateCompanyDataList : fail case");
					DetailAssetSystemLog($"response.ReasonPhrase : {response.ReasonPhrase}");
					DetailAssetSystemLog($"response.StatusCode : {response.StatusCode}");
				}
			}
			catch (Exception e)
			{
				DetailAssetSystemLog($"Exception : {e}");
			}
		}
		
		private static async UniTask<string> GetPatchUrl(string companyId)
		{
			AssetSystemLog("GetPatchUrl called");
			AssetSystemLog($"companyId : {companyId}");
			var patchUrl = "";
			try
			{
				var response = await _httpClient.GetAsync(GetPatchUrlServerApi(companyId));
				switch (response.StatusCode)
				{
					case HttpStatusCode.OK:
					{
						DetailAssetSystemLog("GetPatchUrl : success case");
						var responseBody = await response.Content.ReadAsStringAsync();
						if (string.IsNullOrEmpty(responseBody)) { return patchUrl; }
						DetailAssetSystemLog($"responseBody : {responseBody}");
						var patchVersionData = JsonConvert.DeserializeObject<PatchVersionData>(responseBody);
						if (patchVersionData == null) { return patchUrl; }
						var resultCode = Int32.Parse(patchVersionData._resultCode);
						DetailAssetSystemLog($"path : {patchVersionData._path}");
						DetailAssetSystemLog($"version : {patchVersionData._version}");
						DetailAssetSystemLog($"resultCode : {patchVersionData._resultCode}");
						DetailAssetSystemLog($"resultCode (int32) : {resultCode}");
						DetailAssetSystemLog($"resultMessage : {patchVersionData._resultMessage}");
						if ((eResultCode)resultCode == eResultCode.SUCCESS)
						{
							patchUrl = patchVersionData._path;
						}
					}
						break;

					default:
					{
						// fail case
						DetailAssetSystemLog("GetPatchUrl : fail case");
						DetailAssetSystemLog($"response.ReasonPhrase : {response.ReasonPhrase}");
						DetailAssetSystemLog($"response.StatusCode : {response.StatusCode}");
					}
						break;
				}
			}
			catch (Exception e)
			{
				DetailAssetSystemLog($"GetPatchUrl : Exception : {e}");
			}
			return patchUrl;
		}

		private static async UniTask<bool> AddAssetInfoData(PostRequestAssetInfoData postRequestAssetInfoData)
		{
			AssetSystemLog("AddAssetInfoData called");
			PrintPostRequestAssetInfoData(postRequestAssetInfoData);
			var url = GetAssetServerApi();
			AssetSystemLog($"url : {url}");
			try
			{
				var jsonPostRequestAssetInfoData = JsonConvert.SerializeObject(postRequestAssetInfoData);
				var content = new StringContent(jsonPostRequestAssetInfoData, Encoding.UTF8, @"application/json");
				var response = await _httpClient.PostAsync(url, content);
				switch (response.StatusCode)
				{
					case HttpStatusCode.OK:
					{
						DetailAssetSystemLog("AddAssetInfoData : success case");
						var responseBody = await response.Content.ReadAsStringAsync();
						if (string.IsNullOrEmpty(responseBody)) { return false; }
						DetailAssetSystemLog($"responseBody : {responseBody}");
						var assetInfoData = JsonConvert.DeserializeObject<AssetInfoData>(responseBody);
						if (assetInfoData == null) { return false; }
						PrintAssetInfoData(assetInfoData);
						if (string.IsNullOrEmpty(assetInfoData._errorCode) && string.IsNullOrEmpty(assetInfoData._errorMessage)) { return true; }
						return (Int32.Parse(assetInfoData._errorCode) == 0);
					}
						// break;
				
					default:
					{
						// fail case
						DetailAssetSystemLog("AddAssetInfoData : fail case");
						DetailAssetSystemLog($"response.ReasonPhrase : {response.ReasonPhrase}");
						DetailAssetSystemLog($"response.StatusCode : {response.StatusCode}");
					}
						break;
				}
			}
			catch (Exception e)
			{
				DetailAssetSystemLog($"Exception : {e}");
			}

			return false;
		}
		
		private static async void GetAssetInfoDataList(string companyId, string buildTarget, bool use = true)
		{
			AssetSystemLog("GetAssetInfoDataList called");
			AssetSystemLog($"companyId : {companyId}");
			AssetSystemLog($"buildTarget : {buildTarget}");
			AssetSystemLog($"use : {use}");
			var url = GetAssetListServerApi();
			AssetSystemLog($"url : {url}");
			try
			{
				var getRequestAssetInfoData = new GetRequestAssetInfoData(companyId, buildTarget, GetUse(use));
				var jsonGetRequestAssetInfoData = JsonConvert.SerializeObject(getRequestAssetInfoData);
				var content = new StringContent(jsonGetRequestAssetInfoData, Encoding.UTF8, @"application/json");
				var response = await _httpClient.PostAsync(url, content);
				switch (response.StatusCode)
				{
					case HttpStatusCode.OK:
					{
						DetailAssetSystemLog("GetAssetInfoDataList : success case");
						var responseBody = await response.Content.ReadAsStringAsync();
						if (string.IsNullOrEmpty(responseBody)) { return; }
						DetailAssetSystemLog($"responseBody : {responseBody}");
						var assetInfoDataList = JsonConvert.DeserializeObject<List<AssetInfoData>>(responseBody);
						if (assetInfoDataList == null) { return; }
						foreach (var assetInfoData in assetInfoDataList)
						{
							PrintAssetInfoData(assetInfoData);
						}
					}
						break;

					default:
					{
						// fail case
						DetailAssetSystemLog("GetAssetInfoDataList : fail case");
						DetailAssetSystemLog($"response.ReasonPhrase : {response.ReasonPhrase}");
						DetailAssetSystemLog($"response.StatusCode : {response.StatusCode}");
					}
						break;
				}
			}
			catch (Exception e)
			{
				DetailAssetSystemLog($"Exception : {e}");
			}
		}
		
		private static async UniTask<bool> GetAssetInfoData(string assetId)
		{
			// get : /api/Asset/List/{AssetId}
			// https://public-dev-service.com2verse.com/api/Asset/38
			AssetSystemLog("GetAssetInfoData called");
			AssetSystemLog($"assetId : {assetId}");
			var url = GetAssetServerApi() + assetId;
			AssetSystemLog($"url : {url}");
			try
			{
				var response = await _httpClient.GetAsync(url);
				switch (response.StatusCode)
				{
					case HttpStatusCode.OK:
					{
						DetailAssetSystemLog("GetAssetInfoData : success case");
						var responseBody = await response.Content.ReadAsStringAsync();
						if (string.IsNullOrEmpty(responseBody)) { return false; }
						DetailAssetSystemLog($"responseBody : {responseBody}");
						var assetInfoData = JsonConvert.DeserializeObject<AssetInfoData>(responseBody);
						if (assetInfoData == null) { return false; }
						PrintAssetInfoData(assetInfoData);
						if (string.IsNullOrEmpty(assetInfoData._errorCode) && string.IsNullOrEmpty(assetInfoData._errorMessage)) { return true; }
						return (Int32.Parse(assetInfoData._errorCode) == 0);
					}
						// break;
					
					case HttpStatusCode.NoContent:
					{
						DetailAssetSystemLog("GetAssetInfoData : no content case");
					}
						break;
					
					default:
					{
						// fail case
						DetailAssetSystemLog("GetAssetInfoData : fail case");
						DetailAssetSystemLog($"response.ReasonPhrase : {response.ReasonPhrase}");
						DetailAssetSystemLog($"response.StatusCode : {response.StatusCode}");
					}
						break;
				}
			}
			catch (Exception e)
			{
				DetailAssetSystemLog($"Exception : {e}");
			}
			return false;
		}
		
		private static async UniTask<bool> UpdateAssetInfoData(string assetId, string path, bool use)
		{
			// put : /api/Asset/List/{AssetId}
			// https://public-dev-service.com2verse.com/api/Asset/37
			AssetSystemLog("UpdateAssetInfoData called");
			AssetSystemLog($"assetId : {assetId}");
			AssetSystemLog($"path : {path}");
			AssetSystemLog($"use : {use}");
			var url = GetAssetServerApi() + assetId;
			AssetSystemLog($"url : {url}");
			try
			{
				var putRequestAssetInfoData = new PutRequestAssetInfoData(path, GetUse(use));
				var jsonPutRequestAssetInfoData = JsonConvert.SerializeObject(putRequestAssetInfoData);
				var content = new StringContent(jsonPutRequestAssetInfoData, Encoding.UTF8, @"application/json");
				var response = await _httpClient.PutAsync(url, content);
				switch (response.StatusCode)
				{
					case HttpStatusCode.OK:
					{
						DetailAssetSystemLog("UpdateAssetInfoData : success case");
						var responseBody = await response.Content.ReadAsStringAsync();
						if (string.IsNullOrEmpty(responseBody)) { return false; }
						DetailAssetSystemLog($"responseBody : {responseBody}");
						var assetInfoData = JsonConvert.DeserializeObject<AssetInfoData>(responseBody);
						if (assetInfoData == null) { return false; }
						PrintAssetInfoData(assetInfoData);
						if (string.IsNullOrEmpty(assetInfoData._errorCode) && string.IsNullOrEmpty(assetInfoData._errorMessage)) { return true; }
						return (Int32.Parse(assetInfoData._errorCode) == 0);
					}
						// break;

					case HttpStatusCode.NoContent:
					{
						DetailAssetSystemLog("UpdateAssetInfoData : no content case");
					}
						break;
					
					default:
					{
						// fail case
						DetailAssetSystemLog("UpdateAssetInfoData : fail case");
						DetailAssetSystemLog($"response.ReasonPhrase : {response.ReasonPhrase}");
						DetailAssetSystemLog($"response.StatusCode : {response.StatusCode}");		
					}
						break;
				}
			}
			catch (Exception e)
			{
				DetailAssetSystemLog($"Exception : {e}");
			}
			return false;
		}
		
		private static async UniTask<bool> RemoveAssetInfoData(string assetId)
		{
			// delete : /api/Asset/List/{AssetId}
			// https://public-dev-service.com2verse.com/api/Asset/37
			AssetSystemLog("RemoveAssetInfoData called");
			AssetSystemLog($"assetId : {assetId}");
			var url = GetAssetServerApi() + assetId;
			AssetSystemLog($"url : {url}");
			try
			{
				var response = await _httpClient.DeleteAsync(url);
				switch (response.StatusCode)
				{
					case HttpStatusCode.OK:
					{
						DetailAssetSystemLog("RemoveAssetInfoData : success case");
						var responseBody = await response.Content.ReadAsStringAsync();
						if (string.IsNullOrEmpty(responseBody)) { return false; }
						DetailAssetSystemLog($"responseBody : {responseBody}");
						var assetInfoData = JsonConvert.DeserializeObject<AssetInfoData>(responseBody);
						if (assetInfoData == null) { return false; }
						PrintAssetInfoData(assetInfoData);
						if (string.IsNullOrEmpty(assetInfoData._errorCode) && string.IsNullOrEmpty(assetInfoData._errorMessage)) { return true; }
						return (Int32.Parse(assetInfoData._errorCode) == 0);
					}
						// break;

					default:
					{
						// fail case
						DetailAssetSystemLog("RemoveAssetInfoData : fail case");
						DetailAssetSystemLog($"response.ReasonPhrase : {response.ReasonPhrase}");
						DetailAssetSystemLog($"response.StatusCode : {response.StatusCode}");
					}
						break;
				}
			}
			catch (Exception e)
			{
				DetailAssetSystemLog($"Exception : {e}");
			}
			return false;
		}

		private static async UniTask<bool> GetAvatarInfoDataList(string accountId)
		{
			// get : /api/avatar/{account_id}
			// http://public-dev-service.com2verse.com/api/avatar/100
			AssetSystemLog("GetAvatarInfoDataList called");
			AssetSystemLog($"accountId : {accountId}");
			var url = GetAvatarServerApi() + accountId;
			AssetSystemLog($"url : {url}");
			
			try
			{
				var response = await _httpClient.GetAsync(url);
				switch (response.StatusCode)
				{
					case HttpStatusCode.OK:
					{
						DetailAssetSystemLog("GetAvatarInfoDataList : success case");
						var responseBody = await response.Content.ReadAsStringAsync();
						if (string.IsNullOrEmpty(responseBody)) { return false; }
						DetailAssetSystemLog($"responseBody : {responseBody}");
						var avatarInfoDataList = JsonConvert.DeserializeObject<List<AvatarInfoData>>(responseBody);
						if (avatarInfoDataList == null || avatarInfoDataList.Count <= 0) { return false; }
						foreach (var avatarInfoData in avatarInfoDataList)
						{
							PrintAvatarInfoData(avatarInfoData);
						}
						// if (string.IsNullOrEmpty(assetInfoData._errorCode) && string.IsNullOrEmpty(assetInfoData._errorMessage)) { return true; }
						// return (Int32.Parse(assetInfoData._errorCode) == 0);
						return true;
					}

					default:
					{
						// fail case
						DetailAssetSystemLog("GetAvatarInfoDataList : fail case");
						DetailAssetSystemLog($"response.ReasonPhrase : {response.ReasonPhrase}");
						DetailAssetSystemLog($"response.StatusCode : {response.StatusCode}");
					}
						break;
				}
			}
			catch (Exception e)
			{
				AssetSystemLog($"Exception : {e}");
			}
			return false;
		}

		private static async UniTask<bool> GetAvatarInfoData(string accountId, string avatarId)
		{
			// get : /api/avatar/{account_id}/{avatar_id}
			// http://public-dev-service.com2verse.com/api/avatar/100/96
			AssetSystemLog("GetAvatarInfoData called");
			AssetSystemLog($"accountId : {accountId}");
			AssetSystemLog($"avatarId : {avatarId}");
			var url = GetAvatarServerApi() + $"{accountId}/{avatarId}";
			AssetSystemLog($"url : {url}");
			
			try
			{
				var response = await _httpClient.GetAsync(url);
				switch (response.StatusCode)
				{
					case HttpStatusCode.OK:
					{
						DetailAssetSystemLog("GetAvatarInfoData : success case");
						var responseBody = await response.Content.ReadAsStringAsync();
						if (string.IsNullOrEmpty(responseBody)) { return false; }
						DetailAssetSystemLog($"responseBody : {responseBody}");
						var avatarInfoData = JsonConvert.DeserializeObject<AvatarInfoData>(responseBody);
						if (avatarInfoData == null) { return false; }
						PrintAvatarInfoData(avatarInfoData);
						// if (string.IsNullOrEmpty(avatarInfoData._resultCode) && string.IsNullOrEmpty(avatarInfoData._resultMessage)) { return true; }
						return (Int32.Parse(avatarInfoData._resultCode) == 0);
					}

					default:
					{
						// fail case
						DetailAssetSystemLog("GetAvatarInfoData : fail case");
						DetailAssetSystemLog($"response.ReasonPhrase : {response.ReasonPhrase}");
						DetailAssetSystemLog($"response.StatusCode : {response.StatusCode}");
					}
						break;
				}
			}
			catch (Exception e)
			{
				AssetSystemLog($"Exception : {e}");
			}
			return false;
		}
		
		[Conditional("ASSET_SYSTEM_LOG"), Conditional("DETAIL_ASSET_SYSTEM_LOG"), Conditional("SONG_TEST")]
		private static void PrintAssetInfoData(AssetInfoData infoData)
		{
			if (infoData == null) { return; }
			var info = "@ PrintAssetInfoData\n\n";
			info += $" * assetId : {infoData._assetId}\n";
			info += $" * id : {infoData._id}\n";
			info += $" * name : {infoData._name}\n";
			info += $" * path : {infoData._path}\n";
			info += $" * assetType : {infoData._assetType}\n";
			info += $" * buildTarget : {infoData._buildTarget}\n";
			info += $" * appVersion : {infoData._appVersion}\n";
			info += $" * patchVersion : {infoData._patchVersion}\n";
			info += $" * use : {infoData._use}\n";
			info += $" * createDateTime : {infoData._createDateTime}\n";
			info += $" * updateDateTime : {infoData._updateDateTime}\n";
			info += $" * errorCode : {infoData._errorCode}\n";
			info += $" * errorMessage : {infoData._errorMessage}\n";
			DetailAssetSystemLog($"{info}");
		}
		
		[Conditional("ASSET_SYSTEM_LOG"), Conditional("DETAIL_ASSET_SYSTEM_LOG"), Conditional("SONG_TEST")]
		private static void PrintPostRequestAssetInfoData(PostRequestAssetInfoData infoData)
		{
			if (infoData == null) { return; }
			var info = "@ PrintPostRequestAssetInfoData\n\n";
			info += $" * id : {infoData._id}\n";
			info += $" * path : {infoData._path}\n";
			info += $" * assetType : {infoData._assetType}\n";
			info += $" * buildTarget : {infoData._buildTarget}\n";
			info += $" * appVersion : {infoData._appVersion}\n";
			info += $" * patchVersion : {infoData._patchVersion}\n";
			info += $" * use : {infoData._use}\n";
			DetailAssetSystemLog($"{info}");
		}
		
		[Conditional("ASSET_SYSTEM_LOG"), Conditional("DETAIL_ASSET_SYSTEM_LOG"), Conditional("SONG_TEST")]
		private static void PrintAvatarInfoData(AvatarInfoData infoData)
		{
			if (infoData == null) { return; }
			var info = "@ AvatarInfoData\n\n";
			info += $" * id : {infoData._avatarId}\n";
			info += $" * type : {infoData._avatarType}\n";
			var resultCode = Int32.Parse(infoData._resultCode);
			info += $" * resultCode : {infoData._resultCode}\n";
			info += $" * resultCode (int32) : {resultCode}\n";
			info += $" * resultMessage : {infoData._resultMessage}\n";
			info += "\n * bodyItems : \n";
			if (infoData._bodyItems != null)
			{
				foreach (var bodyItem in infoData._bodyItems)
				{
					info += $"  + BodyId : {bodyItem.BodyId}\n";
					info += $"  + BodyKey : {bodyItem.BodyKey}\n";
					info += $"  + BodyValue : {bodyItem.BodyValue}\n";
					info += $"  + BodyR : {bodyItem.BodyR}\n";
					info += $"  + BodyG : {bodyItem.BodyG}\n";
					info += $"  + BodyB : {bodyItem.BodyB}\n\n";
				}
			}
			info += "\n * fashionItems : \n";
			if (infoData._fashionItems != null)
			{
				foreach (var fashionItems in infoData._fashionItems)
				{
					info += $"  + FashionId : {fashionItems.FashionId}\n";
					info += $"  + FashionR : {fashionItems.FashionR}\n";
					info += $"  + FashionG : {fashionItems.FashionG}\n";
					info += $"  + FashionB : {fashionItems.FashionB}\n";
				}
			}
			DetailAssetSystemLog($"{info}");
		}
#endregion	// CDN
#endregion	// Internal

#if SONG_TEST
#region TEST
		// private static bool _testFlag = false;
		private static int _testFlow = 0;
		private static GameObject _instantiatedObject = null;
		private static string _addressableName = "UI_Popup_YN.prefab";
		
#if UNITY_EDITOR
		[MenuItem("Com2Verse/AssetSystem/Test %F8")]
		public static void TestAllMenu()
		{
			AssetSystemLog("TestAllMenu called:");
			InitializeHttpClient(false);
			TestCDN_ALL();
			// TestAvatar_ALL();
		}
#endif	// UNITY_EDITOR
		
		public void TestAll()
		{
			AssetSystemLog("TestAll called:");
			// ClearCacheAll();
			TestCDN_ALL();
			// TestInstantiate_ALL();
			// TestCDN_Now();
			// TestAvatar_ALL();
		}

		private static void TestAvatar_ALL()
		{
			AssetSystemLog("TestAvatar_ALL called:");
			// Test_GetAvatarInfoDataList();
			Test_GetAvatarInfoData();
		}
		
		private static async void Test_GetAvatarInfoDataList()
		{
			AssetSystemLog("Test_GetAvatarInfoDataList called");
			string accountId = "100";
			// accountId = "9999999";
			var result = await GetAvatarInfoDataList(accountId);
			AssetSystemLog($"GetAvatarInfoDataList - result : {result}");
		}
		
		private static async void Test_GetAvatarInfoData()
		{
			AssetSystemLog("Test_GetAvatarInfoData called");
			string accountId = "100";
			// accountId = "9999999";
			string avatarId = "96";
			var result = await GetAvatarInfoData(accountId, avatarId);
			AssetSystemLog($"GetAvatarInfoData - result : {result}");
		}
		
		private async void TestCDN_Now()
		{
			AssetSystemLog("TestCDN_Now called:");
			UpdatePatchInfo();
			AssetSystemLog("LoadAssetAsync - true");
			await LoadAssetAsyncTask<GameObject>(_addressableName, null, true);
			// TestUpdateCatalog_01();
		}
		
		private static void TestCDN_ALL()
		{
			// TestCache();
			// await TestDownload_01();
			// await TestDownload_02();
			// await TestDownload_03();
			// TestDownload_04();
			// TestDownload_05();
			// TestUpdateCatalog_01();
			// TestLoadContentCatalog_01();
			TestCdnServerAPI();
		}

		private static void TestCdnServerAPI()
		{
			Test_CompanyCode();
			// Test_CompanyCode_02();
			// Test_GetPatchUrl();
			
			// await UpdateCompanyDataList();
			// Test_PrintPatchUrlData();
			// Test_GetAssetDataInfo();
			
			// Test_GetAssetInfoDataList();

			// Test_GetAssetInfoData();
			// Test_UpdateAssetInfoData();
			// Test_RemoveAssetInfoData();
			// Test_AddAssetInfoData();
		}

		private static async void Test_AddAssetInfoData()
		{
			AssetSystemLog("Test_AddAssetInfoData called");
			PostRequestAssetInfoData assetInfoData = new PostRequestAssetInfoData();
			assetInfoData._id = "3";
			assetInfoData._path = "https://metaverse-platform-fn.qpyou.cn/metaverse-platform/Test/{ASSET_TYPE}/{BUILD_TARGET}/{APP_VERSION}/{PATCH_VERSION}";
			assetInfoData._assetType = "AssetBundle";
			assetInfoData._buildTarget = "StandaloneWindows64";
			assetInfoData._appVersion = "0.1.0";
			assetInfoData._patchVersion = "0.0.1";
			assetInfoData._use = GetUse(true);
			var result = await AddAssetInfoData(assetInfoData);
			AssetSystemLog($"AddAssetInfoData - result : {result}");
		}
		private static void Test_GetAssetInfoDataList()
		{
			AssetSystemLog("Test_GetAssetInfoDataList called");
			var companyId = "1";
			// var buildTarget = AppInfo.Instance != null ? AppInfo.Instance.Data.BuildTarget : "StandaloneWindows64";
			var buildTarget = "StandaloneWindows64";
			GetAssetInfoDataList(companyId, buildTarget);
		}
		
		private static async void Test_GetAssetInfoData()
		{
			AssetSystemLog("Test_GetAssetInfoData called");
			string assetId = "41";
			var result = await GetAssetInfoData(assetId);
			AssetSystemLog($"GetAssetInfoData - result : {result}");
		}
		
		private static async void Test_UpdateAssetInfoData()
		{
			AssetSystemLog("Test_UpdateAssetInfoData called");
			string assetId = "41";
			// string path = "https://metaverse-platform-fn.qpyou.cn/metaverse-platform/{ASSET_TYPE}/{BUILD_TARGET}/{APP_VERSION}/{PATCH_VERSION}";
			string path = "https://metaverse-platform-fn.qpyou.cn/metaverse-platform/Test/{ASSET_TYPE}/{BUILD_TARGET}/{APP_VERSION}/{PATCH_VERSION}";
			var result = await UpdateAssetInfoData(assetId, path, false);
			AssetSystemLog($"GetAssetInfoData - result : {result}");
		}
		
		private static async void Test_RemoveAssetInfoData()
		{
			AssetSystemLog("Test_RemoveAssetInfoData called");
			string assetId = "49";
			var result = await RemoveAssetInfoData(assetId);
			AssetSystemLog($"RemoveAssetInfoData - result : {result}");
		}
		
		private static void Test_PrintPatchUrlData()
		{
			AssetSystemLog("Test_PrintPatchUrlData called");
			if (0 < _patchUrlData?.Count)
			{
				foreach (var data in _patchUrlData)
				{
					AssetSystemLog($"companyId : {data._id}");
					AssetSystemLog($"companyName : {data._name}");
					AssetSystemLog($"path : {data._path}");
				}
			}
		}
		
		private static async void Test_CompanyCode()
		{
			//TODO: Mr.Song
			// (1) company id, code 어느걸로 할지 정식 명칭을..
			// (2) 제대로된 목록 가져와야 할듯함.
			
			// company id - companyModel
			// 0 - 컴투스홀딩스
			// 1 - 컴투스
			// 2 - 컴투스플랫폼
			// 3 - 하나은행
			// 11 - 컴투버스
			// [{"METAVERSE_ID":0,"METAVERSE_NAME":"컴투스홀딩스","USE_YN":"Y"},{"METAVERSE_ID":1,"METAVERSE_NAME":"컴투스","USE_YN":"Y"},{"METAVERSE_ID":2,"METAVERSE_NAME":"컴투스플랫폼","USE_YN":"Y"},{"METAVERSE_ID":3,"METAVERSE_NAME":"하나은행","USE_YN":"Y"},{"METAVERSE_ID":11,"METAVERSE_NAME":"컴투버스","USE_YN":"Y"}]

			// 문제 - 데이터가 정상적이지 않다.
			// (1) CompanyName 은 비워져 있고 DeptName 은 있는 형태.
			// "CompanyName": ""
			// "CompanyCode": 1,
			// "DeptName": "컴투스"
			
			// [AssetSystem] info : { "DeptCode": "2", "ParentCode": "0", "IndexCode": "1100", "DeptName": "컴투스홀딩스", "DeptEnglishName": "Com2uS-Holdings", "DeptNickname": "컴투스홀딩스", "DeptDescription": "컴투스홀딩스", "DeptMailAddress": "gamevilnotices" }
			// [AssetSystem] info : { "CompanyCode": 1, "DeptCode": "00001", "ParentCode": "0", "IndexCode": "1200", "DeptName": "컴투스", "DeptEnglishName": "Com2us", "DeptNickname": "컴투스 ", "DeptDescription": "컴투스", "DeptMailAddress": "everyone" }
			// [AssetSystem] info : { "CompanyCode": 2, "DeptCode": "550000", "ParentCode": "0", "IndexCode": "1300", "DeptName": "컴투스플랫폼", "DeptEnglishName": "Com2usPlatform", "DeptNickname": "컴투스플랫폼 ", "DeptDescription": "컴투스플랫폼 ", "DeptMailAddress": "GCP" }
			// [AssetSystem] info : { "CompanyCode": 11, "DeptCode": "00011", "ParentCode": "0", "IndexCode": "1400", "DeptName": "컴투버스", "DeptEnglishName": "Com2Verse", "DeptNickname": "컴투버스 ", "DeptDescription": "컴투버스", "DeptMailAddress": "C2V" }

			// (쓸수 없을듯) login after - company list
			// var companyId = 2;
			// if (Network.User.Instance != null && DataManager.Instance != null)
			// {
			// 	AssetSystemLog($"user id : {Network.User.Instance.ID}");
			// 	var employeePayload = await DataManager.Instance.GetEmployeeByAccountIdAsync(Network.User.Instance.ID);
			// 	if (employeePayload != null)
			// 	{
			// 		var work = employeePayload.GetPrimaryWork();
			// 		AssetSystemLog($"CompanyCode : {work.Department.CompanyCode}");
			// 		AssetSystemLog($"CompanyName : {work.Department.CompanyName}");
			// 		AssetSystemLog($"DeptCode : {work.Department.DeptCode}");
			// 		AssetSystemLog($"DeptName : {work.Department.DeptName}");
			// 	}
			// }
			//
			// // (쓸수 없을듯) login after - company list
			// if (DataManager.Instance != null)
			// {
			// 	var companyModel = DataManager.Instance.GetCompanyModel();
			// 	if (companyModel != null)
			// 	{
			// 		foreach (var company in companyModel.Companies)
			// 		{
			// 			var info = company.Info;
			// 			AssetSystemLog($"info : {info}");
			// 			AssetSystemLog($"CompanyCode : {info.CompanyCode}");
			// 			AssetSystemLog($"CompanyName : {info.CompanyName}");
			// 			AssetSystemLog($"DeptCode : {info.DeptCode}");
			// 			AssetSystemLog($"DeptName : {info.DeptName}");
			// 		}	
			// 	}
			// }
			//
			// //TODO: Mr.Song - company code
			// var companyCode = await GetCurrentCompanyCode();
			// AssetSystemLog($"companyCode : {companyCode}");
			
			// var patchUrl = GetPatchUrl(companyId);
			// AssetSystemLog($"patchUrl : {patchUrl}");
		}

		private async void Test_CompanyCode_02()
		{
			// public api company list
			try
			{
				var response = await _httpClient.GetAsync(GetCompanyDataListServerApi());
				if (response.StatusCode == HttpStatusCode.OK)
				{
					DetailAssetSystemLog("Test_CompanyCode_02 : success case");
					var responseBody = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(responseBody)) { return; }
					DetailAssetSystemLog($"responseBody : {responseBody}");
					var companyDataList = JsonConvert.DeserializeObject<List<CompanyData>>(responseBody);
					if (companyDataList == null) { return; }
					foreach (var companyData in companyDataList)
					{
						DetailAssetSystemLog($"id : {companyData._id}");
						DetailAssetSystemLog($"name : {companyData._name}");
						DetailAssetSystemLog($"use : {companyData._use}");
					}
				}
				else
				{
					// fail case
					DetailAssetSystemLog("Test_CompanyCode_02 : fail case");
					DetailAssetSystemLog($"response.ReasonPhrase : {response.ReasonPhrase}");
					DetailAssetSystemLog($"response.StatusCode : {response.StatusCode}");
				}
			}
			catch (Exception e)
			{
				DetailAssetSystemLog($"Exception : {e}");
			}
		}
		
		private static void Test_GetPatchUrl()
		{
			//TODO: Mr.Song - company id / code
			// company id
			// 0 - 컴투스홀딩스
			// 1 - 컴투스
			// 2 - 컴투스플랫폼
			// 3 - 하나은행
			// 11 - 컴투버스
			var companyId = "1";
			
			// //TODO: Mr.Song - company code
			// var companyCode = GetCurrentCompanyCode();
			// AssetSystemLog($"companyCode : {companyCode}");
			
			var patchUrl = GetPatchUrl(companyId);
			AssetSystemLog($"patchUrl : {patchUrl}");
		}

		private void TestCache()
		{
			if (_testFlow == 0)
			{
				AssetSystemLog($"LoadAssetAsync");
				// await AssetSystemManager.Instance.TestDownload_02();
				// SoundManager.Instance.PlayUISound("Title.ogg");
				// SoundManager.Instance.PlayUISound("ButtonClick1.wav");
				LoadAssetAsync<AudioClip>("Title.ogg",null);
				// LoadAssetAsync<AudioClip>("ButtonClick1.wav",null);
                
				// SoundManager.Instance.PlayUISound("powerUp-2.wav");
				// LoadAssetAsync<AudioClip>("Title.ogg",null);
			}

			if (_testFlow == 1)
			{
				AssetSystemLog($"ReleaseAssetAddressableName");
				// AssetBundle.UnloadAllAssetBundles(true);
				ReleaseAssetAddressableName("Title.ogg");
			}
            
			if (_testFlow == 2)
			{
				AssetSystemLog($"TestClear_02");
				// TestClearCache("Title.ogg");
				TestClearCache("ButtonClick1.wav");
				// TestClearCache("powerUp-2.wav");
				// TestClearCacheAll();
			}

			_testFlow++;
			if (_testFlow == 3)
			{
				_testFlow = 0;
			}
		}

		private void TestClearCache(string key)
		{
			ClearCache(key);
		}

		private void TestClearCacheAll()
		{
			ClearCacheAll();
		}
		
		private void Test_Catalog()
		{
			// catalog
			// https://docs.unity3d.com/Packages/com.unity.addressables@1.20/api/UnityEngine.AddressableAssets.Addressables.UpdateCatalogs.html
			// https://docs.unity3d.com/Packages/com.unity.addressables@1.20/api/UnityEngine.AddressableAssets.Addressables.CheckForCatalogUpdates.html
			// https://docs.unity3d.com/Packages/com.unity.addressables@1.15/manual/UpdateCatalogs.html
			
			// Addressables.CleanBundleCache();
			// Addressables.CheckForCatalogUpdates()
			// Addressables.UpdateCatalogs()
		}

		private async UniTask TestDownload_01()
		{
			string key = "";
			// string key = "waterDive.mp3";
			// string key = "UI_Popup_Coin.prefab";
			// string key = "UI_Popup_Confirm.prefab";

			// 실제 : 1.55GB (1,669,286,388 바이트)
			// asset file : 49.8MB (52,309,377 바이트)
			// download : 183,865,133
			// com2us_artasset - 183865133
			key = "UI_Popup_Confirm.prefab";
			AssetSystemLog($"(1) {key} : {await GetDownloadSize(key)}");
			
			// com2us_artasset - 183865133
			key = "Deco_safeborder_002.prefab";
			AssetSystemLog($"(2) {key} : {await GetDownloadSize(key)}");
			
			// 실제 : 3.92MB (4,112,014 바이트)
			// asset file : 1.03MB (1,080,424 바이트)
			// download : 426,186
			// com2us_sound - 426186
			key = "Title.ogg";
			AssetSystemLog($"(3) {key} : {await GetDownloadSize(key)}");

			// 실제 : 13.1MB (13,810,966 바이트)
			// asset file : 3.40MB (3,572,248 바이트)
			// download : 153,820
			// demoresource - 153820
			key = "waterDive.mp3";
			AssetSystemLog($"(4) {key} : {await GetDownloadSize(key)}");
			
			// demoresource - 152150(down)
			// 왜 다르지?
			key = "waterDive.mp3";
			AssetSystemLog($"(5) {key} : before");
			await Download(key);
			AssetSystemLog($"(5) {key} : after");
		}
		
		private async UniTask TestDownload_02()
		{
			string key = "";
			// key = "UI_Popup_Confirm.prefab";
			// key = "waterDive.mp3";
			// key = "demoresource";
			
			// 1,080,424
			key = "Title.ogg";
			
			// 실제 : 13.1MB (13,810,966 바이트)
			// asset file : 3.40MB (3,572,248 바이트)
			// download : 153,820
			// demoresource - 153820
			AssetSystemLog($"(4) {key} : {await GetDownloadSize(key)}");
			
			// demoresource - 152150(down)
			// 왜 다르지?
			// key = "waterDive.mp3";
			// key = "demoresource";
			AssetSystemLog($"(5) {key} : before");
			await Download(key);
			AssetSystemLog($"(5) {key} : after");
		}

		private async UniTask TestDownload_03()
		{
			string key = "";
			key = "waterDive.mp3";
			bool bLoadResourceLocationsHandle = false;
			var loadResourceLocationsHandle = Addressables.LoadResourceLocationsAsync(key);
			loadResourceLocationsHandle.Completed += (handle) =>
			{
				// AssetSystemLog($"LoadResourceLocationsAsync : {handle.Result}");
				var downloadStatus = handle.GetDownloadStatus();
				AssetSystemLog($"DownloadStatus-TotalBytes : {downloadStatus.TotalBytes}");
				AssetSystemLog($"DownloadStatus-DownloadedBytes : {downloadStatus.DownloadedBytes}");
				AssetSystemLog($"DownloadStatus-IsDone : {downloadStatus.IsDone}");
				AssetSystemLog($"DownloadStatus-Percent : {downloadStatus.Percent}");
				AssetSystemLog($"PercentComplete : {handle.PercentComplete}");
				Addressables.Release(loadResourceLocationsHandle);
				// Addressables.Release(handle);
				bLoadResourceLocationsHandle = true;
			};
			await UniTask.WaitUntil(() => bLoadResourceLocationsHandle);
		}

		private async void TestDownload_04()
		{
			TestClearCache("Title.ogg");
			await GetDownloadSize("powerUp-2.wav");
			await GetDownloadSize("waterDive.mp3");
			await GetDownloadSize("");
		}

		// private async void TestDownload_05()
		// {
		// 	//TODO: Mr.Song
		// 	// // label string test
		// 	// string key = "";
		// 	// AssetSystemLog($"D");
		// 	// await GetDownloadSize(key);
		// 	// AssetSystemLog($"D");
		// 	// await Download(key);
		// 	// AssetSystemLog($"D");
		// }

		private async void TestUpdateCatalog_01()
		{
			if (_testFlow == 0)
			{
				// _testFlag = false;
				AssetSystemLog("LoadAssetAsync - true");
				await LoadAssetAsyncTask<GameObject>(_addressableName, null, true);
			}
			else if (_testFlow == 1)
			{
				// _testFlag = false;
				AssetSystemLog("ReleaseAssetAddressableName");
				ReleaseAssetAddressableName(_addressableName);
			}
			else if (_testFlow == 2)
			{
				// _testFlag = true;
				UpdateCatalog();
			}
			else if (_testFlow == 3)
			{
				// _testFlag = false;
				AssetSystemLog("LoadAssetAsync - true");
				await LoadAssetAsyncTask<GameObject>(_addressableName, null, true);
			}
			
			_testFlow++;
			if (3 < _testFlow)
			{
				_testFlow = 0;
			}
		}
		
		private async void TestLoadContentCatalog_01()
		{
			if (_testFlow == 0)
			{
				var catalogPath = "https://metaverse-platform-fn.qpyou.cn/metaverse-platform/Test/AssetBundles/0.1.5/1.0.0/catalog_metaverse_remote_test.json";
				LoadContentCatalogAsync(catalogPath);
			}
			if (_testFlow == 1)
			{
				await Download(_addressableName);
			}
			if (_testFlow == 2)
			{
				AssetSystemLog("LoadAssetAsync - true");
				await LoadAssetAsyncTask<GameObject>(_addressableName, null, true);
			}
			else if (_testFlow == 3)
			{
				AssetSystemLog("ReleaseAssetAddressableName");
				ReleaseAssetAddressableName(_addressableName);
			}
			
			_testFlow++;
			if (3 < _testFlow)
			{
				_testFlow = 0;
			}
		}
		private void TestInstantiate_ALL()
		{
			// TestInstantiate_00();
			// TestInstantiate_01();
			// TestInstantiate_02();
			TestInstantiate_03();
		}
		
		private void TestInstantiate_00()
		{
			LoadAssetAsync<GameObject>(_addressableName, obj =>
			{
				// UIManager.Instance.ShowPopupCommon("test");
			});
		}
		
		private void TestInstantiate_01()
		{
			// Load + Release
			if (_testFlow == 0)
			{
				AssetSystemLog("LoadAssetAsync - false");
				LoadAssetAsync<GameObject>(_addressableName,null);
			}
			else if (_testFlow == 1)
			{
				AssetSystemLog("ReleaseAssetAddressableName");
				ReleaseAssetAddressableName(_addressableName);
			}
			
			_testFlow++;
			if (1 < _testFlow)
			{
				_testFlow = 0;
			}
		}
		
		private async void TestInstantiate_02()
		{
			// Load + Instantiate + Destroy + Release
			if (_testFlow == 0)
			{
				AssetSystemLog("LoadAssetAsync - false");
				_instantiatedObject = Instantiate(await LoadAssetAsyncTask<GameObject>(_addressableName,null));
			}
			else if (_testFlow == 1)
			{
				AssetSystemLog("Destroy");
				Destroy(_instantiatedObject);
			}
			else if (_testFlow == 2)
			{
				AssetSystemLog("ReleaseAssetAddressableName");
				ReleaseAssetAddressableName(_addressableName);
			}
			
			_testFlow++;
			if (2 < _testFlow)
			{
				_testFlow = 0;
			}
		}

		private async void TestInstantiate_03()
		{
			// Load(Instantiate) + Release (Destroy)
			if (_testFlow == 0)
			{
				AssetSystemLog("LoadAssetAsync - true");
				await LoadAssetAsyncTask<GameObject>(_addressableName, null, true);
			}
			else if (_testFlow == 1)
			{
				AssetSystemLog("ReleaseAssetAddressableName");
				ReleaseAssetAddressableName(_addressableName);
			}
			_testFlow++;
			if (1 < _testFlow)
			{
				_testFlow = 0;
			}
		}
		
		private void TestSound()
		{
			// SoundManager.Instance.PlayUISound("Assets/Project/DemoResource/Sound/sfx/game/powerUp-2.wav");
			// SoundManager.Instance.PlayUISound("Assets/Project/DemoResource/Sound/sfx/game/waterDive.mp3");
		}
#endregion	// TEST
#endif	// SONG_TEST
	}
}
