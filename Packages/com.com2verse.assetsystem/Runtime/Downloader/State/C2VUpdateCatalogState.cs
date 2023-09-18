/*===============================================================
* Product:		Com2Verse
* File Name:	C2VUpdateCatalogState.cs
* Developer:	tlghks1009
* Date:			2023-02-17 17:21
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Logger;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
#endif
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Com2Verse.AssetSystem
{
    public class C2VUpdateCatalogState : C2VAddressablesStateHandler<C2VAddressablesDownloader>
    {
        private readonly List<string> _catalogs;

        enum eActivePlayModeType
        {
            USE_ASSET_DATABASE = 0,
        }

        public C2VUpdateCatalogState(List<string> catalogs) => _catalogs = catalogs;

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            UpdateCatalogAsync();
        }

        private void UpdateCatalogAsync()
        {
            if (_catalogs == null)
            {
                Downloader.RaiseAssetBundleDownloadResponseCode(eResponseCode.UPDATE_CATALOG_ERROR);
                StateMachine.Dispose();

                return;
            }

#if UNITY_EDITOR
            if (AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilderIndex == (int) eActivePlayModeType.USE_ASSET_DATABASE)
            {
                var handle = Downloader.RaiseDownloadSizeCompleted(0);

                handle.OnCompleted += (result) =>
                {
                    handle.Dispose();

                    StateMachine.Dispose();
                };
                return;
            }
#endif

            if (_catalogs.Count > 0)
            {
                C2VDebug.LogCategory("AssetBundle", $"Catalog update required. Count : {_catalogs.Count}");

                var updateCatalogsHandle = Downloader.UpdateCatalogs(_catalogs, false);

                updateCatalogsHandle.OnCompleted += OnUpdateCatalogsCompleted;
            }
            else
            {
                MoveToNextState();
            }
        }


        private void OnUpdateCatalogsCompleted(C2VAsyncOperationHandle<List<IResourceLocator>> handle)
        {
            handle.OnCompleted -= OnUpdateCatalogsCompleted;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                C2VDebug.LogCategory("AssetBundle", "Catalog Update Completed.");
                MoveToNextState(handle.Result);
            }
            else
            {
                Downloader.RaiseAssetBundleDownloadResponseCode(eResponseCode.UPDATE_CATALOG_ERROR);

                StateMachine.Dispose();
            }

            handle.Release();
        }


        private void MoveToNextState(List<IResourceLocator> resourceLocations = null)
        {
            StateMachine.ChangeState(new C2VCleanBundleCacheState(resourceLocations));
        }
    }
}
