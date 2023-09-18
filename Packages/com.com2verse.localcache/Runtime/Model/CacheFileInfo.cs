using System;
using System.IO;

namespace Com2Verse.LocalCache
{
    [Serializable]
    internal class CacheFileInfo
    {
        public string FileName;
        public string Url;
        public string Hash;
        public long FileSize;
        public DateTime CreateDate;
        private CacheFileInfo() { }

#region Static Functions
        public static CacheFileInfo CreateNew(string fileName, string url, string hash, long length) =>
            new()
            {
                FileName = fileName,
                Url = url,
                Hash = hash,
                FileSize = length,
                CreateDate = DateTime.Now,
            };
        public static int CompareTo(CacheFileInfo left, CacheFileInfo right) => left.FileName.CompareTo(right.FileName);
        public static string GetFilePath(string fileName, eCacheType type, MetadataSettingInfo settings) => Path.Combine(settings.CachePath, type.GetName(), fileName);
#endregion // Static Functions
    }
}