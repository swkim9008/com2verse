/*===============================================================
* Product:		Com2Verse
* File Name:	C2VCleanBundleCacheState.cs
* Developer:	tlghks1009
* Date:			2023-06-19 12:05
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.IO;
using Com2Verse.Logger;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Com2Verse.AssetSystem
{
    public sealed class C2VCleanBundleCacheState : C2VAddressablesStateHandler<C2VAddressablesDownloader>
    {
        private List<IResourceLocator> _allResourceLocators;

        public C2VCleanBundleCacheState(List<IResourceLocator> allResourceLocators) => _allResourceLocators = allResourceLocators;


        public override void OnStateEnter()
        {
            CleanCatalogCache();

            CleanBundleCache();
        }


        private void CleanCatalogCache()
        {
            var playerSettingsLocation = Addressables.ResolveInternalId(Addressables.RuntimePath + "/settings.json");
            if (string.IsNullOrEmpty(playerSettingsLocation))
                return;

            var text = File.ReadAllText(playerSettingsLocation);
            if (string.IsNullOrEmpty(text))
                return;

            var rtdRuntime = JsonUtility.FromJson<ResourceManagerRuntimeData>(text);
            if (rtdRuntime == null || rtdRuntime.CatalogLocations.Count == 0)
                return;

            var remoteCatalogLocation = rtdRuntime.CatalogLocations[0];
            var currentRemoteCatalogName = Path.GetFileNameWithoutExtension(remoteCatalogLocation.InternalId);

            var localCatalogDirectoryPath = $"{Application.persistentDataPath}/com.unity.addressables";

            if (!Directory.Exists(localCatalogDirectoryPath))
                return;


            foreach (var catalogFile in Directory.GetFiles(localCatalogDirectoryPath))
            {
                var catalogFileNameWithoutExtension = Path.GetFileNameWithoutExtension(catalogFile);

                if (currentRemoteCatalogName == catalogFileNameWithoutExtension)
                {
                    continue;
                }

                File.Delete(catalogFile);
                C2VDebug.LogCategory("AssetBundle", $"Delete CatalogFile. FileName : {catalogFile}");
            }
        }


        private void CleanBundleCache()
        {
            var handle = Downloader.TryCleanBundleCache();

            handle.OnCompleted += OnCleanBundleCacheCompleted;
        }


        private void OnCleanBundleCacheCompleted(C2VAsyncOperationHandle<bool> handle)
        {
            handle.OnCompleted -= OnCleanBundleCacheCompleted;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (!handle.Result)
                {
                    Downloader.RaiseAssetBundleDownloadResponseCode(eResponseCode.CLEAN_BUNDLE_ERROR);

                    StateMachine.Dispose();
                }
                else
                {
                    MoveToNextState();
                }
            }
            else
            {
                Downloader.RaiseAssetBundleDownloadResponseCode(eResponseCode.CLEAN_BUNDLE_ERROR);

                StateMachine.Dispose();
            }

            handle.Release();
        }


        private void MoveToNextState()
        {
            StateMachine.ChangeState(new C2VDownloadSizeState(_allResourceLocators));
        }
    }
}
