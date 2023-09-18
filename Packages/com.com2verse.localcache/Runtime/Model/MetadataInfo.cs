using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Security = Com2Verse.Utils.Security;

namespace Com2Verse.LocalCache
{
    [Serializable]
    internal class MetadataInfo
    {
        private readonly int _bufferSize = 0x10000;

#region Model
        public static readonly MetadataSettingInfo Settings = MetadataSettingInfo.Default;
        private Dictionary<eCacheType, List<CacheFileInfo>> _cacheFileMap;
        private Dictionary<eCacheType, List<TempFileInfo>> _tempFileMap;
        public IReadOnlyDictionary<eCacheType, List<CacheFileInfo>> CacheFileMap => _cacheFileMap;
        public IReadOnlyDictionary<eCacheType, List<TempFileInfo>> TempFileMap => _tempFileMap;
        private eCacheType _currentCacheType = eCacheType.DEFAULT;
        private Dictionary<string, Callbacks.LoadCacheCallbacks> _activeLoadCacheCallbacks;

        public eCacheType CurrentCacheType
        {
            get => _currentCacheType;
            private set => _currentCacheType = value;
        }
#endregion // Model

#region Initialization
        private MetadataInfo()
        {
            _cacheFileMap = new Dictionary<eCacheType, List<CacheFileInfo>>();
            _tempFileMap = new Dictionary<eCacheType, List<TempFileInfo>>();
            _activeLoadCacheCallbacks = new ();
            var cacheTypes = Enum.GetValues(typeof(eCacheType)) as eCacheType[];
            foreach (var type in cacheTypes)
            {
                _cacheFileMap.Add(type, new List<CacheFileInfo>());
                _tempFileMap.Add(type, new List<TempFileInfo>());
            }
        }
#endregion // Initialization

#region Metadata
        public static MetadataInfo LoadMetaData()
        {
            if (IsMetaDataExist)
            {
                try
                {
                    return JsonConvert.DeserializeObject<MetadataInfo>(File.ReadAllText(MetaDataPath));
                }
                catch (Exception _)
                {
                    // ignored
                }
            }
            return SaveMetaData();
        }
        public static MetadataInfo SaveMetaData(MetadataInfo info = null)
        {
            info ??= new MetadataInfo();

            CreateDirectoryFromFilePath(MetaDataPath);
            if (IsMetaDataExist) File.Delete(MetaDataPath);
            SortMetaInfo(info);
            File.WriteAllText(MetaDataPath, JsonConvert.SerializeObject(info, Formatting.Indented));
            return info;
        }
        public static readonly string MetaDataPath = Path.Combine(Settings.CachePath, "CacheInfo.metadata");
        private static bool IsMetaDataExist => File.Exists(MetaDataPath);
        private static void SortMetaInfo(MetadataInfo info)
        {
            foreach (var cacheFileInfos in info.CacheFileMap.Values)
                cacheFileInfos.Sort(CacheFileInfo.CompareTo);
            foreach (var tempFileInfos in info.TempFileMap.Values)
                tempFileInfos.Sort(TempFileInfo.CompareTo);
        }

        public void SetCacheType(eCacheType type) => CurrentCacheType = type;
#endregion // Metadata

#region Cache File
        public bool IsCacheExist(string fileName) => CacheFileMap[CurrentCacheType].Exists(item => item.FileName.Equals(fileName));

        public CacheFileInfo GetCacheFileInfo(string fileName)
        {
            var idx = GetCacheFileIdx(fileName);
            return idx < 0 ? null : CacheFileMap[CurrentCacheType][idx];
        }
        private int GetCacheFileIdx(string fileName) => CacheFileMap[CurrentCacheType].FindIndex(item => item.FileName.Equals(fileName));
        public async UniTask<C2VArrayPool<byte>> LoadCacheAsync(string fileName, Callbacks.LoadCacheCallbacks loadCacheCallbacks = default, CancellationToken cancellationToken = default)
        {
            var cacheFileInfo = GetCacheFileInfo(fileName);
            if (cacheFileInfo == null)
                return null;

            FileStream fs = null;
            // IMemoryOwner<byte> memoryPool = null;
            C2VArrayPool<byte> arrayPool = null;
            try
            {
                if (!_activeLoadCacheCallbacks.ContainsKey(fileName))
                {
                    var filePath = CacheFileInfo.GetFilePath(cacheFileInfo.FileName, CurrentCacheType, Settings);
                    if (File.Exists(filePath))
                    {
                        fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        _activeLoadCacheCallbacks.Add(fileName, loadCacheCallbacks);
                    }
                }
            }
            catch (IOException e)
            {
                loadCacheCallbacks.OnLoadComplete?.Invoke(false, 0, null);
                if (_activeLoadCacheCallbacks.ContainsKey(fileName))
                    _activeLoadCacheCallbacks.Remove(fileName);
                if (fs != null)
                {
                    await fs.DisposeAsync();
                    fs = null;
                }

                return null;
            }

            // 이미 동일파일을 읽고있는중
            // activeLoadCacheCallbacks에 콜백을 등록해서 다운로드 완료 시점에 결과만 받아 처리
            if (fs == null)
            {
                if (!_activeLoadCacheCallbacks.ContainsKey(fileName)) return null;

                var callbacks = _activeLoadCacheCallbacks[fileName];
                bool isDone = false;
                callbacks.OnProgress += loadCacheCallbacks.OnProgress;
                callbacks.OnLoadComplete += loadCacheCallbacks.OnLoadComplete;
                callbacks.OnLoadComplete += (size, total, bytes) =>
                {
                    arrayPool = C2VArrayPool<byte>.Rent(bytes.Length);
                    bytes.CopyTo(arrayPool.Data);
                    isDone = true;
                };
                _activeLoadCacheCallbacks[fileName] = callbacks;
                await UniTask.WaitUntil(() => isDone);
                // return readBytes.IsEmpty ? null : readBytes.ToArray();
                return arrayPool;
            }

            using var memoryBuffer = MemoryPool<byte>.Shared.Rent(_bufferSize);
            arrayPool = C2VArrayPool<byte>.Rent(fs.Length);
            var readLen = 0;
            long totalReadLen = 0;
            do
            {
                readLen = await fs.ReadAsync(memoryBuffer.Memory, cancellationToken);
                memoryBuffer.Memory.Span.Slice(0, readLen).CopyTo(arrayPool.Data.AsSpan().Slice(Convert.ToInt32(fs.Position - readLen), readLen));
                if (_activeLoadCacheCallbacks[fileName].OnProgress == null) continue;
                totalReadLen += readLen;
                _activeLoadCacheCallbacks[fileName].OnProgress.Invoke(readLen, totalReadLen, arrayPool.Data.Length);
            }
            while (readLen > 0);
            await fs.DisposeAsync();

            _activeLoadCacheCallbacks[fileName].OnLoadComplete?.Invoke(true, totalReadLen, arrayPool.Data);
            if (_activeLoadCacheCallbacks.ContainsKey(fileName))
                _activeLoadCacheCallbacks.Remove(fileName);

            try
            {
                if (Cache.UseEncryption)
                {
                    // var base64Encrypted = Encoding.UTF8.GetString(result);
                    // return decrypted;

                    // var base64Encrypted = Encoding.UTF8.GetString(memoryPool.Memory.Span);
                    // var decrypted = await Security.Instance.DecryptToBytesAsync(base64Encrypted);
                    // memoryPool.Dispose();
                    // memoryPool = MemoryPool<byte>.Shared.Rent(decrypted.Length);
                    // decrypted.CopyTo(memoryPool.Memory);
                    // return memoryPool;

                    var base64Encrypted = Encoding.UTF8.GetString(arrayPool.Data);
                    var result = await Security.Instance.DecryptToBytesAsync(base64Encrypted);
                    arrayPool.Dispose();
                    if (result.Status == Security.eDecryptStatus.SUCCESS)
                    {
                        var decrypted = result.Value;
                        arrayPool = C2VArrayPool<byte>.Rent(decrypted.Length);
                        decrypted.CopyTo(arrayPool.Data.AsSpan());
                    }
                    return arrayPool;
                }
                else
                {
                    return arrayPool;
                }
            }
            catch (Exception e)
            {
                C2VDebug.LogWarning(e);
                return null;
            }
        }

        public async UniTask SaveCacheFileAsync(string fileName, byte[] bytes)
        {
            var filePath = CacheFileInfo.GetFilePath(fileName, _currentCacheType, Settings);
            var length = 0;
            if (Cache.UseEncryption)
            {
                var encrypted = await Security.Instance.EncryptAesAsync(bytes);
                await File.WriteAllTextAsync(filePath, encrypted);
                length = encrypted.Length;
            }
            else
            {
                CreateDirectoryFromFilePath(filePath);

                var base64String = Convert.ToBase64String(bytes);
                await File.WriteAllTextAsync(filePath, base64String);
                length = base64String.Length;
            }
            RemoveCacheFile(fileName, _currentCacheType);
            AddCacheFile(fileName, string.Empty, string.Empty, length);
        }
        public void AddCacheFile(string fileName, string url, string hash, long length)
        {
            var idx = GetCacheFileIdx(fileName);
            if (idx != -1)
            {
                CacheFileMap[CurrentCacheType][idx].Url = url;
                CacheFileMap[CurrentCacheType][idx].Hash = hash;
                CacheFileMap[CurrentCacheType][idx].FileSize = length;
                return;
            }

            var newCacheFile = CacheFileInfo.CreateNew(fileName, url, hash, length);
            CacheFileMap[CurrentCacheType].Add(newCacheFile);
        }

        public void RemoveCacheFile(string fileName, eCacheType type)
        {
            var map = CacheFileMap[type];
            var idx = map.FindIndex(item => item.FileName.Equals(fileName));
            if (idx >= 0)
                map.RemoveAt(idx);
        }
#endregion // Cache File

#region Temp File
        public bool IsTempFileExist(string fileName) => TempFileMap[CurrentCacheType].Exists(item => item.FileName.Equals(fileName));
        private TempFileInfo GetTempFileInfo(string fileName, eCacheType type) => TempFileMap[type].Find(item => item.FileName.Equals(fileName));
        public TempFileInfo AddTempFile(string fileName, string url, eCacheType type)
        {
            var tempFile = GetTempFileInfo(fileName, type);
            if (tempFile != null)
                return tempFile;

            var newTempFile = TempFileInfo.CreateNew(fileName, url);
            TempFileMap[type].Add(newTempFile);
            return newTempFile;
        }

        public void RemoveTempFile(string fileName, eCacheType type)
        {
            var map = TempFileMap[type];
            var idx = map.FindIndex(item => item.FileName.Equals(fileName));
            if (idx >= 0)
                map.RemoveAt(idx);
        }
#endregion // Temp File

#region Util
        private static void CreateDirectoryFromFilePath(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }
#endregion // Util
    }
}
