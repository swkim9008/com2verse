/*===============================================================
* Product:		Com2Verse
* File Name:	C2VSftpClient.cs
* Developer:	tlghks1009
* Date:			2023-05-09 14:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using UnityEditor;

namespace Com2VerseEditor.AssetSystem
{
    public class C2VSftpClient : IDisposable
    {
        private SftpClient _sftpClient;

        private string _localFilePath;
        private string _lastVersion;

        private int _port = 22;

        private bool _isConnected;


        public static C2VSftpClient Create() => new();

        private C2VSftpClient()
        {
            _isConnected = false;
        }


        public bool IsConnected() => _isConnected;

        public eResult Connect()
        {
            try
            {
                if (_isConnected)
                {
                    return eResult.SUCCESS;
                }

                var sftpKeyFilePath = Path.GetFullPath(C2VEditorPath.SftpAuthKeyPath);
                if (string.IsNullOrEmpty(sftpKeyFilePath))
                {
                    sftpKeyFilePath = EditorUtility.OpenFilePanel("SftpKey", string.Empty, "key");
                }

                var keyAuthenticationMethod = new PrivateKeyAuthenticationMethod(C2VEditorPath.SftpUserName, new PrivateKeyFile(sftpKeyFilePath));
                var connectionInfo          = new ConnectionInfo(C2VEditorPath.SftpRemotePath, _port, C2VEditorPath.SftpUserName, keyAuthenticationMethod);

                _sftpClient = new SftpClient(connectionInfo);
                _sftpClient.Connect();

                _isConnected = true;
                Debug.Log("[SFTP] Connection Success.");

                return eResult.SUCCESS;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError("[SFTP] Connection Failed.");

                return eResult.FAILED;
            }
        }


        public IEnumerable<SftpFile> ListDirectory(string directoryPath)
        {
            if (!_sftpClient.Exists(directoryPath))
            {
                return null;
            }

            return _sftpClient?.ListDirectory(directoryPath);
        }


        public void UploadFile(string remoteUploadPath, string localFilePath, string fileName = "")
        {
            using (var inFile = File.Open(localFilePath, FileMode.Open))
            {
                if (string.IsNullOrEmpty(fileName))
                    fileName = Path.GetFileName(inFile.Name);

                _sftpClient.UploadFile(inFile, $"{remoteUploadPath}/{fileName}");
                Debug.Log($"[SFTP] File Upload Completed Path : {remoteUploadPath}/{fileName}");
            }
        }


        public void MakeDirectoryIfNotExist(string directoryPath)
        {
            if (!_sftpClient.Exists(directoryPath))
            {
                _sftpClient.CreateDirectory(directoryPath);
                Debug.Log($"[SFTP] Directory Create Completed. Path : {directoryPath}");
            }
        }


        public T DownloadCatalogJsonFile<T>(string remoteDirectoryPath, string pathToDownload)
        {
            T instance = default;

            if (!_sftpClient.Exists(remoteDirectoryPath))
            {
                return instance;
            }

            foreach (var sftpFile in ListDirectory(remoteDirectoryPath))
            {
                if (Path.GetFileName(sftpFile.FullName).StartsWith("catalog") && Path.GetExtension(sftpFile.FullName) == ".json")
                {
                    var fileName = Path.Combine(pathToDownload, sftpFile.Name);

                    using (var fileStream = File.Create(fileName))
                    {
                        _sftpClient.DownloadFile(sftpFile.FullName, fileStream);
                    }

                    instance = JsonUtility.FromJson<T>(File.ReadAllText(fileName));
                    Debug.Log($"[SFTP] Json File Download Completed. FileName : {fileName}");

                    break;
                }
            }

            return instance;
        }

        public void Disconnect()
        {
            _sftpClient?.Disconnect();
            _sftpClient?.Dispose();
            _sftpClient = null;

            _isConnected = false;
            Debug.Log("[SFTP] Disconnect");
        }


        public void Dispose() => Disconnect();
    }
}
