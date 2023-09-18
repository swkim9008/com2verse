/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesBuildService.cs
* Developer:	tlghks1009
* Date:			2023-04-14 10:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using Com2Verse.AssetSystem;
using Com2Verse.BuildHelper;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace Com2VerseEditor.AssetSystem
{
    public enum eResult
    {
        SUCCESS,
        FAILED
    }

    public sealed class C2VAddressablesBuildService : C2VAddressablesGroupBuilderServiceBase
    {
        private static string RemoteBuildPath(eEnvironment environment)
        {
            return environment switch
            {
                eEnvironment.LOCAL         => Path.GetFullPath(Path.Combine(Application.dataPath!, $@"../{Addressables.BuildPath}/{C2VEditorPath.BuildTarget}")),
                eEnvironment.EDITOR_HOSTED => Path.GetFullPath(Path.Combine(Application.dataPath!, $@"../ServerData/{C2VEditorPath.BuildTarget}")),
                eEnvironment.REMOTE => Path.GetFullPath(Path.Combine(Application.dataPath!,
                                                                     $@"../AssetBundles/{C2VEditorPath.BuildEnvironment}/{C2VEditorPath.BuildTarget}/{Application.version}/{C2VEditorPath.AssetBundleVersion}")),
                _ => string.Empty
            };
        }

#region Uploader
        private class Uploader : IDisposable
        {
            private DirectoryInfo _localDirectoryInfo;

            private C2VSftpClient _sftpClient;

            private C2VAssetBundleVersionManager _assetBundleVersionManager;

            public static Uploader Create(string localFilePath) => new(localFilePath);


            private Uploader(string localFilePath)
            {
                _localDirectoryInfo = new DirectoryInfo(localFilePath!);

                _sftpClient = C2VSftpClient.Create();
                _assetBundleVersionManager = C2VAssetBundleVersionManager.Create(_sftpClient);
            }


            public eResult SftpConnect() => _sftpClient.Connect();


            public eResult Upload()
            {
                try
                {
                    Debug.Log($"[SFTP] File Upload Start");

                    var lastVersion           = _assetBundleVersionManager.GetLastVersion();
                    var newAssetBundleVersion = C2VEditorPath.AssetBundleVersion;
                    var remoteUploadPath      = $"{C2VEditorPath.SftpRemoteBuildTargetPath}/{newAssetBundleVersion}";

                    var lastContentCatalogData = _assetBundleVersionManager.RequestRemoteCatalog(lastVersion);
                    if (lastContentCatalogData != null)
                    {
                        var currentContentCatalogData = GetCurrentContentCatalogData();
                        if (currentContentCatalogData == null)
                        {
                            Debug.LogError("[SFTP] Unable to load CurrentContentCatalogFile.");
                            return eResult.FAILED;
                        }

                        var hasLocalBundleChanged   = HasLocalBundleChanged(lastContentCatalogData, currentContentCatalogData);
                        var updatedAssetBundleNames = FindUpdatedAssetBundleNames(lastContentCatalogData, currentContentCatalogData);
                        if (updatedAssetBundleNames.Count == 0 && !hasLocalBundleChanged)
                        {
                            AppInfo.Instance.UpdateAssetBundleVersion(lastVersion);
                            return eResult.SUCCESS;
                        }

                        var updatedAssetBundleFileInfo = FindUpdatedAssetBundlesInLocalDirectory(updatedAssetBundleNames);

                        var newContentCatalogDataJsonString = JsonUtility.ToJson(currentContentCatalogData);
                        CreateNewCatalogFile(newContentCatalogDataJsonString);

                        MakeRemoteDirectoryIfNotExist(remoteUploadPath);
                        MakeRemoteDirectoryIfNotExist(remoteUploadPath);

                        UploadAssetBundles(remoteUploadPath, updatedAssetBundleFileInfo);
                    }
                    else
                    {
                        UploadAssetBundles(remoteUploadPath, null);
                    }

                    UploadCatalogs(remoteUploadPath);
                    UploadAppInfoFile(remoteUploadPath);
                    UploadBundleCacheInfo(remoteUploadPath);

                    return eResult.SUCCESS;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SFTP] Upload Failed. {e.Message}");
                    return eResult.FAILED;
                }
            }


            private void CreateNewCatalogFile(string catalogJson)
            {
                foreach (var fileInfo in _localDirectoryInfo.GetFiles())
                {
                    if (Path.GetExtension(fileInfo.FullName) == ".json")
                    {
                        var fullName = fileInfo.FullName;

                        File.Delete(fileInfo.FullName);
                        File.WriteAllText(fullName, catalogJson);

                        Debug.Log("파일 지우고 새로 추가");
                    }
                }
            }


            private bool HasLocalBundleChanged(ContentCatalogData lastContentCatalogData, ContentCatalogData currentContentCatalogData)
            {
                var currentLocalBundleInternalIds = new List<string>();
                var lastLocalBundleInternalIds = new List<string>();


                foreach (var currentInternalFullId in currentContentCatalogData.InternalIds)
                {
                    if (!currentInternalFullId.StartsWith("https://") && Path.GetExtension(currentInternalFullId) == ".bundle")
                        currentLocalBundleInternalIds.Add(currentInternalFullId);
                }

                foreach (var lastInternalFullId in lastContentCatalogData.InternalIds)
                {
                    if (!lastInternalFullId.StartsWith("https://") && Path.GetExtension(lastInternalFullId) == ".bundle")
                        lastLocalBundleInternalIds.Add(lastInternalFullId);
                }

                if (currentLocalBundleInternalIds.Count != lastLocalBundleInternalIds.Count)
                    return true;

                foreach (var currentLocalBundleInternalId in currentLocalBundleInternalIds)
                {
                    var isSame = false;
                    foreach (var lastLocalBundleInternalId in lastLocalBundleInternalIds)
                    {
                        if (currentLocalBundleInternalId == lastLocalBundleInternalId)
                        {
                            isSame = true;
                            break;
                        }
                    }

                    if (!isSame)
                        return true;
                }

                return false;
            }


            private List<string> FindUpdatedAssetBundleNames(ContentCatalogData lastContentCatalogData, ContentCatalogData currentContentCatalogData)
            {
                var updatedAssetBundleNames = new List<string>();

                var i = 0;
                foreach (string currentInternalFullId in currentContentCatalogData.InternalIds)
                {
                    bool isSame = false;
                    if (currentInternalFullId.StartsWith("https://"))
                    {
                        foreach (var lastInternalFullId in lastContentCatalogData.InternalIds)
                        {
                            if (lastInternalFullId.StartsWith("https://"))
                            {
                                var currentInternalIds = currentInternalFullId.Split('/');
                                var lastInternalIds    = lastInternalFullId.Split('/');

                                var currentInternalId = currentInternalIds[^1];
                                var lastInternalId    = lastInternalIds[^1];

                                if (currentInternalId == lastInternalId)
                                {
                                    var buildEnvironment = C2VEditorPath.BuildEnvironment;
                                    var buildTarget      = C2VEditorPath.BuildTarget;
                                    var fileName         = Path.GetFileName(currentInternalId);
                                    var version          = lastInternalIds[^2];

                                    currentContentCatalogData.InternalIds[i] = $"{C2VPaths.RemoteAssetBundleUrl}/{buildEnvironment}/{buildTarget}/{version}/{fileName}";

                                    Debug.Log($"[새로운 이름 추가] 원본 : {lastInternalFullId} 변경 : {currentContentCatalogData.InternalIds[i]}");
                                    isSame = true;
                                    break;
                                }
                            }
                        }

                        if (!isSame)
                        {
                            updatedAssetBundleNames.Add(currentInternalFullId);
                        }
                    }

                    i++;
                }

                return updatedAssetBundleNames;
            }


            private List<FileInfo> FindUpdatedAssetBundlesInLocalDirectory(List<string> updatedAssetBundleNames)
            {
                var updatedAssetBundleFileInfos = new List<FileInfo>();

                foreach (var updatedAssetBundleName in updatedAssetBundleNames)
                {
                    foreach (var fileInfo in _localDirectoryInfo.GetFiles())
                    {
                        if (updatedAssetBundleName.Contains(Path.GetFileName(fileInfo.FullName)))
                        {
                            updatedAssetBundleFileInfos.Add(fileInfo);
                            break;
                        }
                    }
                }

                return updatedAssetBundleFileInfos;
            }


            private void UploadAppInfoFile(string remoteUploadPath)
            {
                var appInfoJson         = JsonUtility.ToJson(AppInfo.Instance.Data);
                var appInfoFileFullPath = $"{_localDirectoryInfo.FullName}/appInfo.json";

                File.WriteAllText(appInfoFileFullPath, appInfoJson);

                UploadFile(remoteUploadPath, appInfoFileFullPath);
            }


            private void UploadBundleCacheInfo(string remoteUploadPath)
            {
                if (File.Exists($"{C2VEditorPath.BundleCacheInfoDirectoryPath}/{C2VEditorPath.BundleCacheInfoFileName}"))
                {
                    var buildEnv           = C2VEditorPath.BuildEnvironment;
                    var buildTarget        = C2VEditorPath.BuildTarget;
                    var assetBundleVersion = C2VEditorPath.AssetBundleVersion;

                    var fileName = $"cacheInfo_{buildEnv}_{buildTarget}_{assetBundleVersion}.json";

                    UploadFile(remoteUploadPath, $"{C2VEditorPath.BundleCacheInfoDirectoryPath}/{C2VEditorPath.BundleCacheInfoFileName}", fileName);
                }
            }


            private void UploadCatalogs(string remoteUploadPath)
            {
                foreach (var file in _localDirectoryInfo.GetFiles())
                {
                    if (Path.GetExtension(file.FullName) == ".json" || Path.GetExtension(file.FullName) == ".hash")
                    {
                        UploadFile(remoteUploadPath, file.FullName);
                    }
                }
            }


            private void UploadAssetBundles(string remoteUploadPath, List<FileInfo> updatedAssetBundleFileInfos)
            {
                if (updatedAssetBundleFileInfos == null)
                {
                    foreach (var fileInfo in _localDirectoryInfo.GetFiles())
                    {
                        if (Path.GetExtension(fileInfo.FullName) != ".json" && Path.GetExtension(fileInfo.FullName) != ".hash")
                        {
                            UploadFile(remoteUploadPath, fileInfo.FullName);
                        }
                    }

                    return;
                }

                foreach (var fileInfo in updatedAssetBundleFileInfos)
                {
                    UploadFile(remoteUploadPath, fileInfo.FullName);
                }
            }


            private void UploadFile(string remoteUploadPath, string localFilePath, string fileName = "") => _sftpClient?.UploadFile(remoteUploadPath, localFilePath, fileName);


            private void MakeRemoteDirectoryIfNotExist(string remoteDirectoryPath) => _sftpClient?.MakeDirectoryIfNotExist(remoteDirectoryPath);


            private ContentCatalogData GetCurrentContentCatalogData()
            {
                foreach (var file in _localDirectoryInfo.GetFiles())
                {
                    if (Path.GetExtension(file.FullName) == ".json")
                    {
                        var catalogData = File.ReadAllText(file.FullName);

                        var contentCatalogData = JsonUtility.FromJson<ContentCatalogData>(catalogData);

                        return contentCatalogData;
                    }
                }

                return null;
            }


            public void Dispose()
            {
                _sftpClient?.Dispose();
                _sftpClient = null;

                _localDirectoryInfo        = null;
                _assetBundleVersionManager = null;
            }
        }
#endregion Uploader

#region Builder
        private class Builder : IDisposable
        {
            private readonly eEnvironment _environment;

            private C2VAddressablesEditorCompositionRoot _compositionRoot;

            private string RemoteBuildPath => RemoteBuildPath(_environment);

            public static Builder Create(eEnvironment environment) => new Builder(environment);

            private Builder(eEnvironment environment)
            {
                _environment = environment;
                _compositionRoot = C2VAddressablesEditorCompositionRoot.RequestInstance();
            }


            public Builder Clean()
            {
                AddressableAssetSettings.CleanPlayerContent(null);
                BuildCache.PurgeCache(false);

                var totalGroupCount = AddressableAssetSettingsDefaultObject.Settings.groups.Count;

                for (int i = 0; i < totalGroupCount; i++)
                {
                    var group = AddressableAssetSettingsDefaultObject.Settings.groups[0];
                    AddressableAssetSettingsDefaultObject.Settings.RemoveGroup(group);
                }

                return this;
            }


            public Builder UpdateVersion()
            {
                if (AppInfo.Instance.Data.AssetBuildType == eAssetBuildType.LOCAL)
                {
                    AppInfo.Instance.UpdateAssetBundleVersion(AppInfo.Instance.Data.GitRevisionCount);
                }
                else
                {
                    var sftpClient       = C2VSftpClient.Create();
                    var connectionResult = sftpClient.Connect();
                    if (connectionResult == eResult.FAILED)
                    {
                        AppInfo.Instance.UpdateAssetBundleVersion(AppInfo.Instance.Data.GitRevisionCount);
                        return this;
                    }

                    var versionManager = C2VAssetBundleVersionManager.Create(sftpClient);
                    var newVersion     = versionManager.RequestNewVersion();
                    Debug.Log($"[SFTP] New Version : {newVersion}");

                    sftpClient.Dispose();

                    AppInfo.Instance.UpdateAssetBundleVersion(newVersion);
                }

                return this;
            }


            public Builder CreateGroup()
            {
                var groupBuilderController = _compositionRoot.GroupBuilderController;

                groupBuilderController.CreateGroup(_environment);

                return this;
            }


            public eResult Build()
            {
                return NewBuild();

                // FIXME : 추 후 Update 빌드 가능성이 존재해 주석 처리 하였습니다.

                // if (_environment == eEnvironment.REMOTE)
                // {
                //     var contentStateDataDirectoryPath = Path.GetFullPath(Path.Combine(Application.dataPath!, $@"../AddressableContents/{C2VEditorPath.BuildTarget}/{C2VEditorPath.BuildEnvironment}/Remote"));
                //
                //     return !Directory.Exists(contentStateDataDirectoryPath) ? NewBuild() : PreviousBuild();
                // }
                //
                // return NewBuild();
            }

            private eResult NewBuild()
            {
                if (Directory.Exists(RemoteBuildPath))
                {
                    Directory.Delete(RemoteBuildPath, true);
                    Debug.Log("Catalog 기존 폴더 제거 완료.");
                }

                AddressableAssetSettings.BuildPlayerContent(out var buildResult);

                if (buildResult != null)
                {
                    if (string.IsNullOrEmpty(buildResult.Error))
                    {
                        Debug.Log($"[AssetBundle] AssetBundle Build Success");

                        Dispose();
                        return eResult.SUCCESS;
                    }
                }

                Debug.Log("[AssetBundle] AssetBundle Build Failed");
                Dispose();
                return eResult.FAILED;
            }

            private eResult PreviousBuild()
            {
                if (Directory.Exists(RemoteBuildPath))
                {
                    Directory.Delete(RemoteBuildPath, true);
                    Debug.Log("기존 폴더 제거 완료.");
                }

                var contentStateDataPath = ContentUpdateScript.GetContentStateDataPath(false);

                var settings    = AddressableAssetSettingsDefaultObject.Settings;
                var buildResult = ContentUpdateScript.BuildContentUpdate(settings, contentStateDataPath);


                if (buildResult != null)
                {
                    if (string.IsNullOrEmpty(buildResult.Error))
                    {
                        Debug.Log($"에셋 번들 빌드 성공");
                        Dispose();
                        return eResult.SUCCESS;
                    }
                }

                Debug.Log("에셋 번들 빌드 실패");
                Dispose();
                return eResult.FAILED;
            }


            public void Dispose()
            {
                _compositionRoot?.Dispose();
                _compositionRoot = null;
            }
        }
#endregion Builder

        public C2VAddressablesBuildService(IServicePack servicePack) : base(servicePack) { }

        public eResult CleanBuild(eEnvironment environment) => Builder.Create(environment).Clean().UpdateVersion().CreateGroup().Build();

        public eResult Build(eEnvironment environment) => Builder.Create(environment).UpdateVersion().CreateGroup().Build();

        public eResult Upload(eEnvironment environment)
        {
            Debug.Log("[SFTP] Upload called.");
            var uploader      = Uploader.Create(RemoteBuildPath(environment));
            var connectResult = uploader.SftpConnect();
            if (connectResult == eResult.FAILED)
            {
                Debug.LogError("[SFTP] Connection is Failed.");
                uploader.Dispose();
                return eResult.FAILED;
            }

            var uploadResult = uploader.Upload();
            uploader.Dispose();

            return uploadResult;
        }


        public eResult BuildAndUpload(eEnvironment environment)
        {
            Debug.LogError("[SFTP] BuildAndUpload called.");
            var buildResult = Build(environment);
            if (buildResult == eResult.SUCCESS)
            {
                Debug.LogError("[SFTP] Build Success.");
                var uploader = Uploader.Create(RemoteBuildPath(environment));
                if (uploader.SftpConnect() == eResult.FAILED)
                {
                    Debug.LogError("[SFTP] Connection is Failed.");
                    uploader.Dispose();
                    return eResult.FAILED;
                }

                var result = uploader.Upload();
                uploader.Dispose();

                return result;
            }
            Debug.LogError("[SFTP] Connection is Failed.");
            return eResult.FAILED;
        }
    }
}

