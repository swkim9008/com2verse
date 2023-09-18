using System;
using System.Buffers;
using System.IO;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Security = Com2Verse.Utils.Security;

namespace Com2Verse.LocalCache
{
	internal class CacheManager
	{
		private MetadataInfo _metadataInfo;

#region Singleton
		private static Lazy<CacheManager> _instance = new(() => new CacheManager());
		public static CacheManager Instance => _instance.Value;
#endregion // Singleton

#region Initialization
		private CacheManager()
		{
			_metadataInfo = GetMetaData();
		}
#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Reset()
		{
			_instance = new(() => new CacheManager());
		}
#endif // UNITY_EDITOR
#endregion // Initialization

#region Metadata
		private MetadataInfo GetMetaData()
		{
			var newMetadata = MetadataInfo.LoadMetaData();
			if(_metadataInfo != null)
				newMetadata.SetCacheType(_metadataInfo.CurrentCacheType);
			return newMetadata;
		}

		private void ReloadMetaData() => _metadataInfo = GetMetaData();
		public void SaveMetaData()
		{
			MetadataInfo.SaveMetaData(_metadataInfo);
			// ReloadMetaData();
		}
		public void SetCacheType(eCacheType type) => _metadataInfo.SetCacheType(type);
		public void Print()
		{
			SaveMetaData();
			var json = JsonConvert.SerializeObject(_metadataInfo, Formatting.Indented);
			C2VDebug.Log(json);
		}
#endregion // Metadata

#region Cache File
		public bool IsCacheExist(string fileName) => _metadataInfo.IsCacheExist(fileName) && File.Exists(GetCacheFilePath(fileName));

		public bool IsCacheValid(string fileName)
		{
			var filePath = GetCacheFilePath(fileName);
			if (File.Exists(filePath))
			{
				var cacheInfo = _metadataInfo.GetCacheFileInfo(fileName);
				var fileLength = new FileInfo(filePath).Length;
				return cacheInfo.FileSize == fileLength;
			}

			return false;
		}

		public async UniTask SaveCacheAsync(string fileName, byte[] bytes)
		{
			await _metadataInfo.SaveCacheFileAsync(fileName, bytes);
			SaveMetaData();
		}

		public string GetCacheFilePath(string fileName) => GetCacheFilePath(fileName, _metadataInfo.CurrentCacheType);
		private string GetCacheFilePath(string fileName, eCacheType type) => CacheFileInfo.GetFilePath(fileName, type, MetadataInfo.Settings);
		public async UniTask<C2VArrayPool<byte>> LoadCacheAsync(string fileName, Callbacks.LoadCacheCallbacks loadCacheCallbacks = default) => await _metadataInfo.LoadCacheAsync(fileName, loadCacheCallbacks);
		public void AddCacheFileInfo(string fileName, string url, string hash, long length) => _metadataInfo.AddCacheFile(fileName, url, hash, length);
		private void RemoveCacheFileInfo(string fileName) => RemoveCacheFileInfo(fileName, _metadataInfo.CurrentCacheType);
		private void RemoveCacheFileInfo(string fileName, eCacheType type) => _metadataInfo.RemoveCacheFile(fileName, type);
		public void DeleteCacheFile(string fileName)
		{
			DeleteCache(fileName);
			SaveMetaData();
		}
		public void DeleteCacheByType(eCacheType type)
		{
			DeleteCache(type);
			SaveMetaData();
		}
		public void DeleteAllCacheFiles()
		{
			foreach (var key in _metadataInfo.CacheFileMap.Keys)
				DeleteCache(key);
			SaveMetaData();
		}
		private void DeleteCache(eCacheType type)
		{
			var infos = _metadataInfo.CacheFileMap[type].ToArray();
			foreach (var info in infos)
			{
				var filePath = GetCacheFilePath(info.FileName, type);
				FileUtil.DeleteFile(filePath);
				RemoveCacheFileInfo(info.FileName, type);
			}
		}
		private void DeleteCache(string fileName)
		{
			var filePath = GetCacheFilePath(fileName);
			FileUtil.DeleteFile(filePath);
			RemoveCacheFileInfo(fileName);
		}
#endregion // Cache File

#region Temp File
		public bool IsTempFileExist(string fileName) => _metadataInfo.IsTempFileExist(fileName);
		public string GetTempFilePath(string fileName) => GetTempFilePath(fileName, _metadataInfo.CurrentCacheType);
		private string GetTempFilePath(string fileName, eCacheType type) => TempFileInfo.GetFilePath(fileName, type, MetadataInfo.Settings);

		public TempFileInfo GetTempFileInfo(string fileName)
		{
			var map = _metadataInfo.TempFileMap[_metadataInfo.CurrentCacheType];
			var idx = map.FindIndex(item => item.FileName.Equals(fileName));
			return idx > 0 ? map[idx] : null;
		}

		public void AddTempFileInfo(string fileName, string url) => AddTempFileInfo(fileName, url, _metadataInfo.CurrentCacheType);
		private void AddTempFileInfo(string fileName, string url, eCacheType type) => _metadataInfo.AddTempFile(fileName, url, type);
		public void RemoveTempFileInfo(string fileName) => RemoveTempFileInfo(fileName, _metadataInfo.CurrentCacheType);
		private void RemoveTempFileInfo(string fileName, eCacheType type) => _metadataInfo.RemoveTempFile(fileName, type);
		public void DeleteTempFile(string fileName)
		{
			DeleteTemp(fileName);
			SaveMetaData();
		}

		public void DeleteTempByType(eCacheType type)
		{
			DeleteTemp(type);
			SaveMetaData();
		}
		public void DeleteAllTempFiles()
		{
			foreach (var key in _metadataInfo.TempFileMap.Keys)
				DeleteTemp(key);
			SaveMetaData();
		}

		private void DeleteTemp(eCacheType type)
		{
			var infos = _metadataInfo.TempFileMap[type].ToArray();
			foreach (var info in infos)
			{
				var filePath = GetTempFilePath(info.FileName, type);
				FileUtil.DeleteFile(filePath);
				RemoveTempFileInfo(info.FileName, type);
			}
		}
		private void DeleteTemp(string fileName)
		{
			var filePath = GetTempFilePath(fileName);
			FileUtil.DeleteFile(filePath);
			RemoveTempFileInfo(fileName);
		}

		public async UniTask<long> MoveTempFileToCacheAsync(string fileName)
		{
			var tempFilePath = GetTempFilePath(fileName);
			var cacheFilePath = GetCacheFilePath(fileName);

			if (!File.Exists(tempFilePath)) return -1;

			if (!Cache.UseEncryption)
			{
				if (File.Exists(cacheFilePath))
					File.Delete(cacheFilePath);

				var tmpFileInfo = new FileInfo(tempFilePath);
				var tmpFileLen = tmpFileInfo.Length;
				File.Move(tempFilePath, cacheFilePath);
				return tmpFileLen;
			}
			else
			{
				var bytes = await File.ReadAllBytesAsync(tempFilePath);
				try
				{
					File.Delete(tempFilePath);
					if (File.Exists(cacheFilePath))
						File.Delete(cacheFilePath);
				}
				catch (IOException _) { }

				FileStream stream = null;
				try
				{
					stream = new FileStream(cacheFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
				}
				catch (IOException _)
				{
					return -1;
				}

				var encrypted = await Security.Instance.EncryptAesAsync(bytes);
				await using var sw = new StreamWriter(stream);
				await sw.WriteAsync(encrypted);
				return encrypted.Length;
			}
		}
#endregion // Temp File
	}
}
