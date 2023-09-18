/*===============================================================
* Product:		Com2Verse
* File Name:	C2VDownloadSizeState.cs
* Developer:	tlghks1009
* Date:			2023-02-17 17:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Logger;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Com2Verse.AssetSystem
{
    public class C2VDownloadSizeState : C2VAddressablesStateHandler<C2VAddressablesDownloader>
    {
        private List<IResourceLocator> _allResourceLocators;

        public C2VDownloadSizeState(List<IResourceLocator> allResourceLocators = null) => _allResourceLocators = allResourceLocators;


        public override void OnStateEnter()
        {
            base.OnStateEnter();

            DownloadSizeAsync();
        }


        public override void OnStateExit()
        {
            base.OnStateExit();

            _allResourceLocators?.Clear();
            _allResourceLocators = null;
        }


        private void DownloadSizeAsync()
        {
            var handle = Downloader.GetDownloadSizeAsync(_allResourceLocators);

            handle.OnCompleted += OnDownloadSizeCompleted;
        }


        private void OnDownloadSizeCompleted(C2VAsyncOperationHandle<C2VDownloadAssetProviderOperation.AssetBundleCollection> handle)
        {
            handle.OnCompleted -= OnDownloadSizeCompleted;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                MoveToNextState(handle.Result);
            }
            else
            {
                C2VDebug.LogErrorCategory("AssetBundle", "Download Size Error.");

                Downloader.RaiseAssetBundleDownloadResponseCode(eResponseCode.BUNDLE_SIZE_CHECK_ERROR);

                StateMachine.Dispose();
            }

            handle.Release();
        }


        private void MoveToNextState(C2VDownloadAssetProviderOperation.AssetBundleCollection assetBundleCollection)
        {
            C2VDebug.LogCategory("AssetBundle",$"Download Size Completed : {assetBundleCollection.TotalSize}");

            var handle = Downloader.RaiseDownloadSizeCompleted(assetBundleCollection.TotalSize);

            handle.OnCompleted += (result) =>
            {
                if (result)
                {
                    StateMachine.ChangeState(new C2VCheckForCacheSizeState(assetBundleCollection));
                }
                else
                {
                    // 다운로드 사이즈가 없을 때 또는 거절 시 처리
                    StateMachine.Dispose();
                }

                handle.Dispose();
            };
        }
    }
}
