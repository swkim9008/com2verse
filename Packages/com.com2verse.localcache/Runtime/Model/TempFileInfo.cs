using System;
using System.IO;

namespace Com2Verse.LocalCache
{
    [Serializable]
    internal class TempFileInfo
    {
        public string FileName;
        public string Url;
        private TempFileInfo() { }

#region Static Functions
        public static TempFileInfo CreateNew(string fileName, string url) =>
            new()
            {
                FileName = fileName,
                Url = url,
            };
        public static string TempFilePrefix = "_";
        public static string TempFileExt = ".tmp";
        public static string GetTempFileName(string fileName) => $"{TempFilePrefix}{fileName}.{TempFileExt}";
        public static string GetFileName(string tempFileName)
        {
            int startIdx = 0;
            int endIdx = tempFileName.Length - 1;
            if (tempFileName.StartsWith(TempFilePrefix))
                startIdx++;
            if (tempFileName.EndsWith(TempFileExt))
                endIdx -= TempFileExt.Length + 1;
            return tempFileName.Substring(startIdx, endIdx);
        }
        public static int CompareTo(TempFileInfo left, TempFileInfo right) => left.FileName.CompareTo(right.FileName);
        public static string GetFilePath(string fileName, eCacheType type, MetadataSettingInfo settings) =>
            Path.Combine(settings.CachePath, type.GetName(), $"{TempFilePrefix}{fileName}{TempFileExt}");
#endregion // Static Functions
    }
}
