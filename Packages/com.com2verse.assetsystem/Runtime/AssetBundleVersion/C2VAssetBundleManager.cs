/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAssetBundleVersionInfo.cs
* Developer:	tlghks1009
* Date:			2023-04-24 12:25
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.IO;
using System.Net;
using System.Threading;
using Com2Verse.BuildHelper;
using Com2Verse.HttpHelper;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Com2Verse.AssetSystem
{
    public enum eResponseCode
    {
        SUCCESS                     = 0,
        PATCH_SERVER_REQUEST_FAILED = -1,
        PATCH_SERVER_EMPTY_DATA     = 9999,
        RESPONSE_NULL               = 101,
        CANCELED                    = 100,
        CACHE_ERROR                 = 102,
        TIME_OUT                    = 103,
        CACHE_SPACE_ERROR           = 104,
        CATALOG_UPDATE_ERROR        = 201,
        UPDATE_CATALOG_ERROR        = 202,
        CLEAN_BUNDLE_ERROR          = 203,
        BUNDLE_SIZE_CHECK_ERROR     = 204,
        BUNDLE_DOWNLOAD_ERROR       = 205,
        INTERNAL_SERVER_ERROR       = 500,
    }

    public class C2VAssetBundleManager : Singleton<C2VAssetBundleManager>
    {
        private static readonly long   _serviceId = 11;
        private static readonly string _assetType = "AssetBundle";

        private string _assetBundlePatchVersion = "10020";
        private string _appBuildEnvironment;
        private string _appVersion;
        private string _appBuildTarget;
        private string _buildDistributeType;

        private eHiveEnvType    _hiveEnvType;
        private eAssetBuildType _assetBuildType;

        public string TargetAssetBundleVersion => _assetBundlePatchVersion;
        public string AppBuildEnvironment      => _appBuildEnvironment;
        public string AppVersion               => _appVersion;
        public string BuildTarget              => _appBuildTarget;


        public eAssetBuildType AssetBuildType => _assetBuildType;

        public string AssetBundleCacheInfoFileName      => $"cacheInfo_{AppBuildEnvironment}_{BuildTarget}_{TargetAssetBundleVersion}.json";
        public string AssetBundleLocalCacheInfoFullPath => $"{C2VPaths.BundleCacheFolderPath}/{AppBuildEnvironment}/{AssetBundleCacheInfoFileName}";
        public string AssetBundleRemotePath             => $"{C2VPaths.RemoteAssetBundleUrl}/{AppBuildEnvironment}/{BuildTarget}/{TargetAssetBundleVersion}";

        private CancellationTokenSource _cts;

        [UsedImplicitly] private C2VAssetBundleManager() { }


        public void Initialize(string buildTarget, string buildEnvironment, string appVersion, string assetBundleVersion, eAssetBuildType assetBuildType, eHiveEnvType hiveEnvType, CancellationTokenSource cts)
        {
            _assetBuildType          = assetBuildType;
            _appVersion              = appVersion;
            _appBuildEnvironment     = buildEnvironment;
            _appBuildTarget          = buildTarget;
            _cts                     = cts;
            _hiveEnvType             = hiveEnvType;
            _assetBundlePatchVersion = assetBundleVersion;
        }


        public async UniTask<eResponseCode> TryRequestAssetBundlePatchVersion()
        {
            if (_assetBuildType == eAssetBuildType.REMOTE)
            {
                var result = await RequestAssetBundlePatchVersion();
                if (result == eResponseCode.SUCCESS)
                {
                    AddInternalIdTransformFuncHandler();

                    if (!await C2VCaching.DownloadCacheData())
                        return eResponseCode.CACHE_ERROR;
                }

                return result;
            }

            if (!await C2VCaching.DownloadCacheData())
                return eResponseCode.CACHE_ERROR;

            return eResponseCode.SUCCESS;
        }


        private async UniTask<eResponseCode> RequestAssetBundlePatchVersion()
        {
            ResponseBase<WebApi.Open.Components.AssetResult> response = null;

            switch (_appBuildEnvironment)
            {
                case "PRODUCTION":
                {
                    response = await WebApi.Open.Live.Api.Asset.GetAsset(_serviceId, _assetType, _appBuildTarget, _appVersion, _cts);
                }
                    break;
                default:
                    response = await WebApi.Open.Dev.Api.Asset.GetAsset(_serviceId, _assetType, _appBuildTarget, _appVersion, _cts);
                    break;
            }

            if (_cts == null || _cts.IsCancellationRequested)
                return eResponseCode.CANCELED;

            if (response == null)
                return eResponseCode.RESPONSE_NULL;

            switch (response.StatusCode)
            {
                case HttpStatusCode.RequestTimeout:
                    return eResponseCode.TIME_OUT;
                case HttpStatusCode.OK or HttpStatusCode.Accepted:
                {
                    var assetResult = response.Value;

                    if (assetResult.RsltCd == ((int) eResponseCode.PATCH_SERVER_EMPTY_DATA).ToString())
                    {
                        C2VDebug.LogErrorCategory("AssetBundle", $"Empty Data. AppVersion : {_appVersion}, AppBuildTarget : {_appBuildTarget}");
                        return eResponseCode.PATCH_SERVER_EMPTY_DATA;
                    }


                    _assetBundlePatchVersion = assetResult.PatchVersion;
                    C2VDebug.LogCategory("AssetBundle", $"PatchVersion : {Instance._assetBundlePatchVersion}");

                    return eResponseCode.SUCCESS;
                }
                case HttpStatusCode.InternalServerError:
                    return eResponseCode.INTERNAL_SERVER_ERROR;
                default:
                    C2VDebug.LogErrorCategory("AssetBundle", $"ErrorCode : {response.StatusCode}");
                    return eResponseCode.PATCH_SERVER_REQUEST_FAILED;
            }
        }


        public static void AddInternalIdTransformFuncHandler()
        {
            Addressables.InternalIdTransformFunc -= InternalIdTransformFunc;
            Addressables.InternalIdTransformFunc += InternalIdTransformFunc;
        }

        public static void RemoveInternalIdTransformFuncHandler()
        {
            Addressables.InternalIdTransformFunc -= InternalIdTransformFunc;
        }


        private static string InternalIdTransformFunc(IResourceLocation location)
        {
            var fileName = Path.GetFileName(location.InternalId);

            if (location.InternalId.StartsWith("https://") && fileName.StartsWith("catalog_Com2Verse"))
            {
                var extension                = Path.GetExtension(location.InternalId);
                var buildEnvironment         = Instance.AppBuildEnvironment;
                var targetAssetBundleVersion = Instance.TargetAssetBundleVersion;

                var internalId = $"{Instance.AssetBundleRemotePath}/catalog_Com2Verse_{buildEnvironment}_{targetAssetBundleVersion}{extension}";

                return internalId;
            }

            return location.InternalId;
        }
    }
}
