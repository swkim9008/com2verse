/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesDownloader.cs
* Developer:	tlghks1009
* Date:			2023-02-17 17:22
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Com2Verse.BuildHelper;
using JetBrains.Annotations;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace Com2Verse.AssetSystem
{
    public class C2VDownloadStatus
    {
        public string Name               { get; set; }
        public long   DownloadedSize     { get; set; }
        public int    TotalDownloadCount { get; set; }
        public long   TotalSize          { get; set; }

        public float Percent => DownloadedSize / (float) TotalSize;

        public static C2VDownloadStatus Create() => new();

        public static C2VDownloadStatus Create(int totalDownloadCount, long totalSize) => new(totalDownloadCount, totalSize);

        private C2VDownloadStatus(int totalDownloadCount, long totalSize)
        {
            TotalDownloadCount = totalDownloadCount;
            TotalSize          = totalSize;
        }

        private C2VDownloadStatus() { }
    }


    public class C2VAddressableAssetBundleLoadInfo
    {
        public eAssetBundleType[] AssetBundleTypes { get; }

        public string BuildTarget    { get; }
        public string AppVersion     { get; }
        public string BundleVersion  { get; }
        public string AppEnvironment { get; }

        public C2VAddressableAssetBundleLoadInfo(string buildTarget, string appVersion, string bundleVersion, string appEnvironment, [NotNull] params eAssetBundleType[] assetBundleTypes)
        {
            BuildTarget      = buildTarget;
            AppVersion       = appVersion;
            BundleVersion    = bundleVersion;
            AppEnvironment   = appEnvironment;
            AssetBundleTypes = assetBundleTypes.ToArray();
        }
    }


    public class C2VAddressablesDownloader : IDisposable
    {
        public readonly C2VAddressablesEvent<eResponseCode> OnRemoteAssetBundleDownloadFailed = new();
        public readonly C2VAddressablesEvent<C2VAddressablesEvent<bool>, long> OnRemoteAssetBundlesSizeDownloadCompleted = new();
        public readonly C2VAddressablesEvent OnRemoteAssetBundlesDownloadCompleted = new();
        public readonly C2VAddressablesEvent<C2VDownloadStatus> OnRemoteAssetBundleDownloadStatus = new();


        private readonly IStateMachine<C2VAddressablesDownloader> _stateMachine;

        private C2VAddressableAssetBundleLoadInfo _assetBundleLoadInfo;
        public  C2VAddressableAssetBundleLoadInfo LoadInfo => _assetBundleLoadInfo;


        private CancellationTokenSource _downloadCts;
        public  CancellationTokenSource DownloadCts => _downloadCts;

        public static C2VAddressablesDownloader Create(C2VAddressableAssetBundleLoadInfo assetBundleLoadInfo) => new(assetBundleLoadInfo);

        private C2VAddressablesDownloader(C2VAddressableAssetBundleLoadInfo assetBundleLoadInfo)
        {
            _downloadCts = new CancellationTokenSource();

            _assetBundleLoadInfo = assetBundleLoadInfo;

            _stateMachine = C2VAddressablesStateMachine<C2VAddressablesDownloader>.Create(this);
        }

        public void Download() => _stateMachine.StartMachine(new C2VCheckForCatalogUpdateState());


        public C2VAddressablesEvent<bool> RaiseDownloadSizeCompleted(long downloadSize)
        {
            var answerEvent = new C2VAddressablesEvent<bool>();

            OnRemoteAssetBundlesSizeDownloadCompleted?.Invoke(answerEvent, downloadSize);

            return answerEvent;
        }

        public void RaiseRemoteAssetBundlesDownloadCompleted() => OnRemoteAssetBundlesDownloadCompleted?.Invoke();

        public void RaiseRemoteAssetBundleDownloadStatus(C2VDownloadStatus status) => OnRemoteAssetBundleDownloadStatus?.Invoke(status);

        public void RaiseAssetBundleDownloadResponseCode(eResponseCode responseCode) => OnRemoteAssetBundleDownloadFailed?.Invoke(responseCode);


        public C2VAsyncOperationHandle DownloadDependenciesAsync(object key, bool autoReleaseHandle = false)
        {
            return new C2VAsyncOperationHandle(Addressables.DownloadDependenciesAsync(key, autoReleaseHandle));
        }


        public C2VAsyncOperationHandle<IResourceLocator> LoadContentCatalogAsync(string catalogPath, bool autoReleaseHandle = true)
        {
            return new C2VAsyncOperationHandle<IResourceLocator>(Addressables.LoadContentCatalogAsync(catalogPath, autoReleaseHandle));
        }


        public C2VAsyncOperationHandle<C2VDownloadAssetProviderOperation.AssetBundleCollection> GetDownloadSizeAsync(List<IResourceLocator> locators)
        {
            return new C2VAsyncOperationHandle<C2VDownloadAssetProviderOperation.AssetBundleCollection>(
                C2VDownloadAssetProviderOperation.GetDownloadSizeAsync(locators, _assetBundleLoadInfo.AssetBundleTypes));
        }


        public C2VAsyncOperationHandle<List<string>> CheckForCatalogUpdate(bool autoReleaseHandle = true)
        {
            return new C2VAsyncOperationHandle<List<string>>(Addressables.CheckForCatalogUpdates(autoReleaseHandle));
        }


        public C2VAsyncOperationHandle<List<IResourceLocator>> UpdateCatalogs(IEnumerable<string> catalogs = null, bool autoReleaseHandle = true)
        {
            return new C2VAsyncOperationHandle<List<IResourceLocator>>(Addressables.UpdateCatalogs(catalogs, autoReleaseHandle));
        }

        public C2VAsyncOperationHandle<bool> TryCleanBundleCache()
        {
            return new C2VAsyncOperationHandle<bool>(C2VCleanBundleCacheOperation.TryCleanBundleCache());
        }


        public void Dispose()
        {
            _stateMachine?.Dispose();

            OnRemoteAssetBundleDownloadFailed.Dispose();
            OnRemoteAssetBundlesSizeDownloadCompleted.Dispose();
            OnRemoteAssetBundlesDownloadCompleted.Dispose();
            OnRemoteAssetBundleDownloadStatus.Dispose();

            _assetBundleLoadInfo = null;

            _downloadCts?.Cancel();
            _downloadCts?.Dispose();
            _downloadCts = null;
        }
    }
}
