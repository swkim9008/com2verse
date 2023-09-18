/*===============================================================
* Product:		Com2Verse
* File Name:	C2VDownloadDependenciesAsyncState.cs
* Developer:	tlghks1009
* Date:			2023-02-17 17:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.AssetSystem
{
    public class C2VDownloadDependenciesAsyncState : C2VAddressablesStateHandler<C2VAddressablesDownloader>
    {
        private C2VDownloadAssetProviderOperation.AssetBundleCollection _assetBundleCollection;

        private C2VDownloadStatus _downloadStatus;

        private long _downloadedSize;

        public C2VDownloadDependenciesAsyncState(C2VDownloadAssetProviderOperation.AssetBundleCollection assetBundleCollection) => _assetBundleCollection = assetBundleCollection;


        public override void OnStateEnter()
        {
            base.OnStateEnter();

            _downloadedSize = 0;
            _downloadStatus = C2VDownloadStatus.Create(_assetBundleCollection.TotalCount, _assetBundleCollection.TotalSize);

            DownloadDependenciesTask().Forget();
        }

        public override void OnStateExit()
        {
            base.OnStateExit();

            _assetBundleCollection = null;
        }


        private async UniTask DownloadDependenciesTask()
        {
            var originalCache = Caching.currentCacheForWriting;

            foreach (var assetBundleEntry in _assetBundleCollection.Bundles)
            {
                if (assetBundleEntry.Size <= 0)
                    continue;

                if (!C2VCaching.TryGetBundleCache(assetBundleEntry.LabelType, out var cache))
                {
                    C2VDebug.LogErrorCategory("AssetBundle", $"Can't find CacheDirectory. {assetBundleEntry.LabelType}");

                    Downloader.RaiseAssetBundleDownloadResponseCode(eResponseCode.BUNDLE_DOWNLOAD_ERROR);

                    StateMachine.Dispose();
                    return;
                }

                Caching.currentCacheForWriting = cache;

                if (!await DownloadDependenciesAsync(assetBundleEntry))
                {
                    Downloader.RaiseAssetBundleDownloadResponseCode(eResponseCode.BUNDLE_DOWNLOAD_ERROR);

                    StateMachine.Dispose();
                    return;
                }
            }

            if (Downloader.DownloadCts == null || Downloader.DownloadCts.IsCancellationRequested)
                return;

            await UniTask.Delay(1000, cancellationToken: Downloader.DownloadCts.Token);

            Caching.currentCacheForWriting = originalCache;

            C2VDebug.LogCategory("AssetBundle", "AssetBundle DownloadCompleted!");

            RaiseAssetBundleDownloadCompleted();
        }


        private async UniTask<bool> DownloadDependenciesAsync(C2VDownloadAssetProviderOperation.AssetBundleEntry assetBundleEntry)
        {
            var downloadHandle  = Downloader.DownloadDependenciesAsync(assetBundleEntry.PrimaryKey, false);
            var assetBundleName = assetBundleEntry.Bundle.ToString();

            long downloadedBytes = 0;

            while (!downloadHandle.IsDone)
            {
                if (downloadHandle.Handle.OperationException != null)
                    return false;

                if (downloadHandle.DownloadStatus.DownloadedBytes > downloadedBytes)
                {
                    _downloadStatus.DownloadedSize += downloadHandle.DownloadStatus.DownloadedBytes - downloadedBytes;
                    _downloadStatus.Name =  assetBundleName;

                    downloadedBytes = downloadHandle.DownloadStatus.DownloadedBytes;

                    RaiseDownloadStatus();
                }

                await UniTask.Yield(cancellationToken: Downloader.DownloadCts.Token);
            }

            downloadHandle.Release();

            _downloadedSize += assetBundleEntry.Size;
            _downloadStatus.DownloadedSize =  _downloadedSize;

            RaiseDownloadStatus();

            return true;
        }

        private void RaiseAssetBundleDownloadCompleted() => Downloader.RaiseRemoteAssetBundlesDownloadCompleted();

        private void RaiseDownloadStatus() => Downloader.RaiseRemoteAssetBundleDownloadStatus(_downloadStatus);
    }
}
