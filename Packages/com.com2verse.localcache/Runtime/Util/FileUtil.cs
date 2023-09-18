using System;
using System.IO;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;

namespace Com2Verse.LocalCache
{
    internal class FileUtil
    {
        public static bool DeleteFile(string filePath) => DeleteFileInternalAsync(filePath).GetAwaiter().GetResult();
        public static async UniTask<bool> DeleteFileAsync(string filePath) => await DeleteFileInternalAsync(filePath);
        private static async UniTask<bool> DeleteFileInternalAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1, FileOptions.DeleteOnClose | FileOptions.Asynchronous);
                    return true;
                }
                catch (Exception e)
                {
                    C2VDebug.LogWarning(e);
                    return false;
                }
            }

            return false;
        }
    }
}
