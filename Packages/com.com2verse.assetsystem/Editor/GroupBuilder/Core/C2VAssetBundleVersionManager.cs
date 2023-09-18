/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAssetBundleVersionManager.cs
* Developer:	tlghks1009
* Date:			2023-05-09 14:56
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace Com2VerseEditor.AssetSystem
{
    public class C2VAssetBundleVersionManager
    {
        public static string DefaultVersion => "10020";

        private readonly C2VSftpClient _sftpClient;
        private string _errorString    = "error";

        public static C2VAssetBundleVersionManager Create(C2VSftpClient sftpClient) => new(sftpClient);

        private C2VAssetBundleVersionManager(C2VSftpClient sftpClient)
        {
            _sftpClient = sftpClient;
        }

        public string GetLastVersion()
        {
            if (!_sftpClient.IsConnected())
            {
                Debug.LogError("[SFTP] 연결이 되어 있지 않습니다.");
                return _errorString;
            }

            var directoryList = _sftpClient.ListDirectory(C2VEditorPath.SftpRemoteBuildTargetPath);
            if (directoryList == null)
            {
                return string.Empty;
            }

            int lastVersion = -1;

            foreach (var sftpFile in directoryList)
            {
                var remoteVersion = Convert.ToInt32(Path.GetFileName(sftpFile.FullName));

                lastVersion = Math.Max(lastVersion, remoteVersion);
            }

            Debug.Log($"[SFTP] AssetBundle Last Version : {lastVersion}");

            return lastVersion.ToString();
        }


        public string RequestNewVersion()
        {
            var lastVersion = GetLastVersion();

            if (lastVersion == _errorString)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(lastVersion))
            {
                return DefaultVersion;
            }

            var newVersion = (Convert.ToInt32(lastVersion) + 1).ToString();

            return newVersion;
        }


        public ContentCatalogData RequestRemoteCatalog(string version)
        {
            var catalogDirectoryPath = Path.GetFullPath(Path.Combine(Application.dataPath!, @"../AssetBundles/RemoteCatalog"));

            MakeLocalDirectoryIfNotExist(catalogDirectoryPath);

            if (string.IsNullOrEmpty(version)) { }
            else
            {
                var contentCatalogData = _sftpClient.DownloadCatalogJsonFile<ContentCatalogData>($"{C2VEditorPath.SftpRemoteBuildTargetPath}/{version}", catalogDirectoryPath);

                if (contentCatalogData == null)
                {
                    Debug.LogError("Catalog를 가져오지 못했습니다.");
                }

                return contentCatalogData;
            }

            return null;
        }


        private void MakeLocalDirectoryIfNotExist(string localDirectoryPath)
        {
            if (!Directory.Exists(localDirectoryPath))
            {
                Directory.CreateDirectory(localDirectoryPath);
            }
        }
    }
}
