/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesAssetProvider.cs
* Developer:	tlghks1009
* Date:			2023-02-27 11:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Logger;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Com2Verse.AssetSystem
{
    public sealed class C2VDownloadAssetProviderOperation
    {
        public sealed class AssetBundleEntry
        {
            public IResourceLocation Bundle { get; set; }

            public string PrimaryKey { get; set; }

            public long Size { get; set; }

            public eAssetBundleType LabelType { get; set; }
        }

        public sealed class AssetBundleCollection : IDisposable
        {
            public long TotalSize => _assetBundleCollection?.Sum(bundle => bundle.Size) ?? 0;

            public int TotalCount => _assetBundleCollection?.Count(bundle => bundle.Size > 0) ?? 0;


            private List<AssetBundleEntry> _assetBundleCollection;

            public IReadOnlyList<AssetBundleEntry> Bundles => _assetBundleCollection;


            public AssetBundleCollection() => _assetBundleCollection = new List<AssetBundleEntry>();

            public void AddEntry(AssetBundleEntry entry) => _assetBundleCollection.Add(entry);

            public void Dispose()
            {
                _assetBundleCollection.Clear();
                _assetBundleCollection = null;
            }
        }


        internal static AsyncOperationHandle<AssetBundleCollection> GetDownloadSizeAsync(eAssetBundleType key)
        {
            return GetDownloadSizeAsync(Addressables.ResourceLocators.ToList(), new[] {key});
        }


        internal static AsyncOperationHandle<AssetBundleCollection> GetDownloadSizeAsync(List<IResourceLocator> locators, eAssetBundleType[] targetKeys)
        {
            var downloadAssetProvider = new C2VDownloadAssetProviderOperation();

            return downloadAssetProvider.ComputeSizeAsync(locators, targetKeys);
        }


        private AsyncOperationHandle<AssetBundleCollection> ComputeSizeAsync(IEnumerable<IResourceLocator> locators, eAssetBundleType[] targetKeys = null)
        {
            locators ??= Addressables.ResourceLocators;

            var allLocations = new List<IResourceLocation>();
            var assetBundleEntries = new AssetBundleCollection();

            foreach (var key in targetKeys)
            {
                foreach (var location in locators.LocateAll(key))
                {
                    if (location.HasDependencies)
                    {
                        allLocations.AddRange(location.Dependencies);
                    }
                }
            }


            foreach (var location in allLocations.Distinct())
            {
                if (location.Data is AssetBundleRequestOptions requestOptions)
                {
                    var size = requestOptions.ComputeSize(location, Addressables.ResourceManager);

                    if (size > 0)
                    {
                        var cacheInfo = C2VCaching.GetCacheEntity(location.PrimaryKey);

                        if (cacheInfo == null)
                            return Addressables.ResourceManager.CreateCompletedOperation<AssetBundleCollection>(null, "CacheInfo not found.");

                        var assetBundleEntry = new AssetBundleEntry
                        {
                            Bundle = location,
                            Size = size,
                            PrimaryKey = location.PrimaryKey,
                            LabelType = Enum.Parse<eAssetBundleType>(cacheInfo.CacheName),
                        };

                        assetBundleEntries.AddEntry(assetBundleEntry);
                    }
                }
            }

            return Addressables.ResourceManager.CreateCompletedOperation(assetBundleEntries, string.Empty);
        }
    }
}
