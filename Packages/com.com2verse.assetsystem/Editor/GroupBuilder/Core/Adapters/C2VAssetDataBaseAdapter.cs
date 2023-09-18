/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAssetDataBaseAdapter.cs
* Developer:	tlghks1009
* Date:			2023-03-03 14:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.AssetSystem
{
	public sealed class C2VAssetDataBaseAdapter
	{
		public IEnumerable<string> GetAllAssetPaths() => AssetDatabase.GetAllAssetPaths();

		public string GetGuid(string assetPath) => AssetDatabase.AssetPathToGUID(assetPath);

		public void SaveAssets() => AssetDatabase.SaveAssets();

		public T LoadAssetAtPath<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);

		public void CreateAsset(Object groupRule, string path) => AssetDatabase.CreateAsset(groupRule, path);

		public bool DeleteAsset(string path) => AssetDatabase.DeleteAsset(path);

		public void Refresh() => AssetDatabase.Refresh();
	}
}
