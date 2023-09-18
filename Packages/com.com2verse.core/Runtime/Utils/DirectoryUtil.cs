/*===============================================================
* Product:		Com2Verse
* File Name:	DirectoryUtil.cs
* Developer:	jhkim
* Date:			2022-11-18 10:47
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Com2Verse.Logger;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Utils
{
    public static class DirectoryUtil
    {
#region Variables
        public static readonly string TempRoot = Application.temporaryCachePath;
        public static readonly string PersistentDataRoot = Application.persistentDataPath;
        public static readonly string StreamingAssetRoot = Application.streamingAssetsPath;
        public static readonly string DataRoot = Application.dataPath;
        public static readonly string LibraryRoot = Path.GetFullPath(Path.Combine("Library", "PackageCache"));
        public static readonly string SysTempRoot = Path.GetTempPath();
#endregion // Variables

#region Getter
        [NotNull] public static string GetTempPath(string subDir) => Path.Combine(TempRoot, subDir ?? string.Empty);
        [NotNull] public static string GetPersistentDataPath(string subDir) => Path.Combine(PersistentDataRoot, subDir ?? string.Empty);
        [NotNull] public static string GetStreamingAssetPath(string subDir) => Path.Combine(StreamingAssetRoot, subDir ?? string.Empty);
        [NotNull] public static string GetDataPath(string subDir) => Path.Combine(DataRoot, subDir ?? string.Empty);
        [NotNull] public static string GetLibraryPath(string subDir) => Path.Combine(LibraryRoot, subDir ?? string.Empty);
        [NotNull] public static string GetSysTempPath(string subDir) => Path.Combine(SysTempRoot, subDir ?? string.Empty);

        [NotNull] public static string GetTempPath(params string[] subDirs) => Path.Combine(TempRoot, CombinePath(subDirs));
        [NotNull] public static string GetPersistentDataPath(params string[] subDirs) => Path.Combine(PersistentDataRoot, CombinePath(subDirs));
        [NotNull] public static string GetStreamingAssetPath(params string[] subDirs) => Path.Combine(StreamingAssetRoot, CombinePath(subDirs));
        [NotNull] public static string GetDataPath(params string[] subDirs) => Path.Combine(DataRoot, CombinePath(subDirs));
        [NotNull] public static string GetLibraryPath(params string[] subDirs) => Path.Combine(LibraryRoot, CombinePath(subDirs));
        [NotNull] public static string GetSysTempPath(params string[] subDirs) => Path.Combine(SysTempRoot, CombinePath(subDirs));
#endregion // Getter

        [NotNull]
        public static string CombinePath(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return string.Empty;
            if (paths.Length == 1)
                return paths[0] ?? string.Empty;

            return paths.Aggregate((l, r) =>
            {
                if (string.IsNullOrWhiteSpace(l)) l = string.Empty;
                if (string.IsNullOrWhiteSpace(r)) r = string.Empty;
                return Path.Combine(l, r);
            }) ?? string.Empty;
        }

        public static void DirectoryCopy(string fromDir, string toDir, string searchPattern, bool recurse = true, string[] ignoreFileNames = null, params string[] ignoreDirs)
        {
            if (string.IsNullOrWhiteSpace(fromDir) || string.IsNullOrWhiteSpace(toDir)) return;

            searchPattern ??= string.Empty;

            CreateDirectory(toDir);
            var files = Directory.GetFiles(fromDir, searchPattern);
            var subDirs = Directory.GetDirectories(fromDir);

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                if (ignoreFileNames?.Contains(fileName) == true)
                    continue;

                try
                {
                    File.Copy(Path.Combine(fromDir, fileName), Path.Combine(toDir, fileName), true);
                }
                catch (IOException e)
                {
                    C2VDebug.LogWarning($"Directory Copy - FileCopy Failed\n{e}");
                }
            }

            if (recurse)
            {
                foreach (var subDir in subDirs)
                {
                    var dirName = Path.GetFileName(subDir);
                    if (string.IsNullOrWhiteSpace(dirName)) continue;
                    if (ignoreDirs == null || ignoreDirs.Contains(dirName)) continue;

                    DirectoryCopy(Path.Combine(fromDir, dirName), Path.Combine(toDir, dirName), searchPattern, true, ignoreFileNames, ignoreDirs);
                }
            }
        }

        private static void CreateDirectory(string dirPath)
        {
            if (string.IsNullOrWhiteSpace(dirPath)) return;

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
        }

        public static void FileCopy(IEnumerable<string> files, string toDir)
        {
            if (files == null) return;
            if (string.IsNullOrWhiteSpace(toDir)) return;

            CreateDirectory(toDir);
            foreach (var filePath in files)
            {
                if (string.IsNullOrWhiteSpace(filePath)) continue;

                var fileName = Path.GetFileName(filePath);
                File.Copy(filePath, Path.Combine(toDir, fileName), true);
            }
        }
    }
}
