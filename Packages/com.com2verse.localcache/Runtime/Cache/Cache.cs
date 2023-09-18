using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Security = Com2Verse.Utils.Security;

namespace Com2Verse.LocalCache
{
    public static class Cache
    {
        private static readonly string LogCategory = "Local Cache";
        public const bool UseEncryption = false;
        private static int _requestCount = 0;
        private static int _activeLoadCacheCount = 0;
        private static readonly int MaxCacheLoad = 10;

        private static readonly string[] ImageExtensions = new[]
        {
            ".png",
            ".jpg",
        };

#region Request
        public class Request
        {
            private string _fileName;
            private string _url;
            private Callbacks.LoadCacheCallbacks _loadCacheCallbacks;
            private Callbacks.DownloadCallbacks _downloadCallbacks;
            private Callbacks.DownloadRequestCallbacks _downloadRequestCallbacks;
            private DownloadHandler _handler;
            private Request(string fileName, string url)
            {
                _fileName = fileName;
                _url = url;
            }

            [NotNull] public static Request NewRequest(string fileName, string url = "") => new(fileName, url);
            [NotNull]
            public Request WithLoadCacheCallbacks(Callbacks.LoadCacheCallbacks callbacks)
            {
                _loadCacheCallbacks = callbacks;
                return this;
            }

            [NotNull]
            public Request WithDownloadCallbacks(Callbacks.DownloadCallbacks callbacks)
            {
                _downloadCallbacks = callbacks;
                return this;
            }

            [NotNull]
            public Request WithDownloadRequestCallbacks(Callbacks.DownloadRequestCallbacks callbacks)
            {
                _downloadRequestCallbacks = callbacks;
                return this;
            }
            public async UniTask<Memory<byte>> LoadBytesAsync()
            {
                using var pool = await Cache.LoadBytesAsync(_fileName, _url, _loadCacheCallbacks, _downloadCallbacks);
                return pool.Data;
            }
            public async UniTask<Texture2D> LoadTexture2DAsync()
            {
                var result = await LoadTextureFromFileAsync(_fileName);
                if (result.Item1)
                    return result.Item2;

                var bytes = (await LoadBytesAsync()).ToArray();
                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.LoadImage(bytes);
                return tex;
            }
        }
#endregion // Request

#region Public Methods
        public static async UniTask<Texture2D> LoadTexture2DAsync(string fileName, string url = "")
        {
            // 캐시된 텍스쳐 파일 찾기
            var result = await LoadTextureFromFileAsync(fileName);

            if (result.Item1)
                return result.Item2;

            // 파일이 없거나 손상된 경우 캐시 파일 삭제 후 다운로드
            var cacheFilePath = CacheManager.Instance.GetCacheFilePath(fileName);
            await FileUtil.DeleteFileAsync(cacheFilePath);

            var success = await DownloadTextureFileAsync();
            if (success)
            {
                // 다운로드 성공시 캐시에서 텍스쳐 로드
                result = await LoadTextureFromFileAsync(fileName);
                return result.Item2;
            }

            // 다운로드 실패
            return null;

            async UniTask<bool> DownloadTextureFileAsync()
            {
                using var pool = await LoadBytesAsync(fileName, url);
                return pool != null && pool.Data.Length != 0;
            }
        }
        public static async UniTask<C2VArrayPool<byte>> LoadBytesAsync(string fileName, string url = "", Callbacks.LoadCacheCallbacks loadCacheCallbacks = default, Callbacks.DownloadCallbacks downloadCallbacks = default, Callbacks.DownloadRequestCallbacks downloadRequestCallbacks = default)
        {
            if (IsAvailable(fileName))
                return await CacheManager.Instance.LoadCacheAsync(fileName, loadCacheCallbacks);
            if (string.IsNullOrWhiteSpace(url))
                return null;

            if (downloadCallbacks.IsUndefined())
                downloadCallbacks = Callbacks.DownloadCallbacks.Empty;

            var handler = await DownloadFileAsync(fileName, url, downloadCallbacks);
            if (handler == null) return null;

            if (handler.IsSuccess())
                return await CacheManager.Instance.LoadCacheAsync(fileName, loadCacheCallbacks);

            return null;
        }

        public static async UniTask SaveBytesAsync(string fileName, byte[] bytes)
        {
            if (string.IsNullOrWhiteSpace(fileName) || bytes == null) return;

            PurgeCache(fileName);
            await CacheManager.Instance.SaveCacheAsync(fileName, bytes);
        }
        public static void PurgeCache(string fileName) => CacheManager.Instance.DeleteCacheFile(fileName);
        public static void PurgeCache(eCacheType type) => CacheManager.Instance.DeleteCacheByType(type);
        public static void DeleteAllCache() => CacheManager.Instance.DeleteAllCacheFiles();
        public static void PurgeTemp(string fileName) => CacheManager.Instance.DeleteTempFile(fileName);
        public static void PurgeTemp(eCacheType type) => CacheManager.Instance.DeleteTempByType(type);
        public static void DeleteAllTemp() => CacheManager.Instance.DeleteAllTempFiles();
        public static void SetCacheType(eCacheType cacheType) => CacheManager.Instance.SetCacheType(cacheType);
        public static void PrintInfo() => CacheManager.Instance.Print();
        public static bool IsExist(string fileName) => IsAvailable(fileName);
#endregion // Public Methods

#region Private Methods
        private static bool IsAvailable(string fileName)
        {
            var cacheManager = CacheManager.Instance;
            return cacheManager.IsCacheExist(fileName) && cacheManager.IsCacheValid(fileName);
        }

        private static async UniTask<DownloadHandler> DownloadFileAsync(string fileName, string url, Callbacks.DownloadCallbacks downloadCallbacks = default, Callbacks.DownloadRequestCallbacks downloadRequestCallbacks = default)
        {
            long offset = 0;
            var cacheManager = CacheManager.Instance;
            if (cacheManager == null)
                throw new NullReferenceException();

            var tempFilePath = cacheManager.GetTempFilePath(fileName);
            if (cacheManager.IsTempFileExist(fileName))
            {
                await FileUtil.DeleteFileAsync(tempFilePath);
                // offset = new FileInfo(tempFilePath).Length;
            }

            cacheManager.AddTempFileInfo(fileName, url);
            cacheManager.SaveMetaData();

            DownloadHandler handler = null;
            var isDownloadComplete = false;
            var request = DownloadFileInternal(tempFilePath, url, downloadCallbacks, OnDownloadHandler, offset);
            downloadRequestCallbacks.OnDownloadRequest?.Invoke(request);

            await UniTask.WaitUntil(() => isDownloadComplete);

            return handler;

            void OnDownloadHandler(DownloadHandler downloadHandler)
            {
                if (downloadHandler == null)
                {
                    isDownloadComplete = true;
                    return;
                }

                handler = downloadHandler;
                downloadHandler.SetOnFailed(code =>
                {
                    isDownloadComplete = true;
                });
                downloadHandler.AddOnDownloadComplete(async (hash, length) =>
                {
                    var cachedFileLength = await cacheManager.MoveTempFileToCacheAsync(fileName);
                    cacheManager.RemoveTempFileInfo(fileName);
                    if (cachedFileLength != -1)
                        cacheManager.AddCacheFileInfo(fileName, url, hash, cachedFileLength);
                    cacheManager.SaveMetaData();

                    await UniTask.WaitUntil(() => cacheManager.IsCacheExist(fileName));

                    isDownloadComplete = true;
                });
            }
        }
        private static DownloadRequest DownloadFileInternal(string filePath, string url, Callbacks.DownloadCallbacks downloadCallbacks, Action<DownloadHandler> onDownloadHandler, long offset = 0)
        {
            downloadCallbacks.OnReadBytesToStreamAsync = async (stream, bytes) => { await stream.WriteAsync(bytes); };

            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return DownloadManager.Instance.NewRequest(() =>
            {
                try
                {
                    var handler = DownloadManager.Instance.NewDownload(url, filePath, downloadCallbacks, offset);
                    downloadCallbacks.OnDownloadHandler?.Invoke(handler);
                    handler.StartDownload();
                    onDownloadHandler?.Invoke(handler);
                }
                catch (Exception e)
                {
                    C2VDebug.LogWarningCategory(LogCategory, e);
                    onDownloadHandler?.Invoke(null);
                }
            });
        }
#endregion // Private Methods

#region Util
        private static async UniTask<(bool, Texture2D)> LoadTextureFromFileAsync(string fileName)
        {

            fileName = RemoveImageExtension(fileName);
            var cacheFilePath = CacheManager.Instance.GetCacheFilePath(fileName);
            if (IsAvailable(fileName))
            {
                var filePath = Path.Combine("file://", cacheFilePath);
                if (!UseEncryption)
                {
                    var isValidTexture = TextureUtil.CheckFileIsValidTexture(filePath);
                    if (isValidTexture)
                    {
                        var textureSize = TextureUtil.GetTextureSize(filePath);
                        if (textureSize == (-1, -1))
                            return (false, null);

                        try
                        {
                            using var request = UnityWebRequestTexture.GetTexture(filePath);
                            await request.SendWebRequest();
                            if (request.result == UnityWebRequest.Result.Success)
                            {
                                var tex = DownloadHandlerTexture.GetContent(request);
                                isValidTexture = tex.width > 8 && tex.height > 8;
                                return (isValidTexture, tex);
                            }
                            C2VDebug.LogErrorCategory(LogCategory, $"LOAD TEXTURE FROM FILE FAILED. RESULT = {request.result}");
                        }
                        catch (Exception e)
                        {
                            C2VDebug.LogErrorCategory(LogCategory, $"LOAD TEXTURE FROM FILE ERROR\n{e}");
                        }
                    }
                    return (false, null);
                }
                else
                {
                    // TODO: 사용전에 UnityWebRequest로 변경 필요
                    var www = UnityWebRequest.Get(filePath);
                    await www.SendWebRequest();
                    Texture2D texture = null;
                    var success = false;

                    if (www.isDone && www.result == UnityWebRequest.Result.Success)
                    {
                        try
                        {
                            byte[] data = null;
                            if (UseEncryption)
                            {
                                var encrypted = www.downloadHandler.data;
                                var result = await Security.Instance.DecryptToBytesAsync(Encoding.UTF8.GetString(encrypted));
                                if (result.Status == Security.eDecryptStatus.SUCCESS)
                                    data = result.Value;
                                else
                                    return (false, null);
                            }
                            else
                            {
                                data = www.downloadHandler.data;

                                if (!TextureUtil.IsValidTexture(data))
                                    return (false, null);
                            }

                            texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                            success = texture.LoadImage(data);
                        }
                        catch (Exception e)
                        {
                            C2VDebug.LogWarningCategory(LogCategory, e);
                            success = false;

                            await FileUtil.DeleteFileAsync(cacheFilePath);
                        }
                    }

                    return (success, texture);
                }
            }

            return (false, null);
        }

        public static string RemoveImageExtension(string path)
        {
            foreach (var imageExtension in ImageExtensions)
            {
                if (path.EndsWith(imageExtension))
                    return path.Substring(0, path.Length - imageExtension.Length);
            }

            return path;
        }
#endregion // Util
    }

#if UNITY_EDITOR
namespace Com2Verse.LocalCache
{
    public class CacheTest
    {
        [MenuItem("Com2Verse/LocalCache/Open Temp Folder")]
        static void OpenMetaData()
        {
            var dirPath = Path.GetDirectoryName(MetadataInfo.MetaDataPath);
            EditorUtility.RevealInFinder(dirPath);
        }
    }
}
#endif
}
