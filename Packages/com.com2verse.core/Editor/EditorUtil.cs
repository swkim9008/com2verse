/*===============================================================
* Product:		Com2Verse
* File Name:	EditorUtil.cs
* Developer:	eugene9721
* Date:			2022-07-14 14:01
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using System.IO;
using Cysharp.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Com2VerseEditor.Utils
{
	public static class EditorUtil
	{
		public static bool IsAssetAFolder(Object obj){
			if (obj == null) return false;

			var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
			if (path == null) return false;

			return path.Length > 0 && Directory.Exists(path);
		}

		public static bool IsValidateOnlyScene()
		{
			var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			var isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
			return !isValidPrefabStage;
		}

		public static List<string> SearchFiles(string dir, string pattern)
		{
			var sb = ZString.CreateStringBuilder();

			var listScenePaths = new List<string>();
			foreach (var f in Directory.GetFiles(dir, pattern, SearchOption.AllDirectories))
			{
				var directoryPath = Path.GetDirectoryName(f);
				if (directoryPath == null)
					continue;

				var dirs = directoryPath.Split(Path.DirectorySeparatorChar);
				if (dirs == null || dirs.Length == 0)
					continue;

				var checkAssetsRoot = false;
				foreach (var dirName in dirs)
				{
					if (checkAssetsRoot == false)
					{
						if (dirName.Equals("Assets"))
							checkAssetsRoot = true;
					}
					else
					{
						sb.Append($"{dirName}/");
					}
				}

				var fi = $"{sb.ToString()}{Path.GetFileNameWithoutExtension(f)}";
				sb.Clear();
				listScenePaths.Add(fi);
			}

			return listScenePaths;
		}
	}
}
