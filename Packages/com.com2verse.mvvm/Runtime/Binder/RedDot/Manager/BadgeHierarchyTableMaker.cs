/*===============================================================
* Product:		Com2Verse
* File Name:	BadgeHierarchyTable.cs
* Developer:	NGSG
* Date:			2023-05-02 15:32
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Com2Verse.AssetSystem;
using Com2Verse.Logger;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Com2Verse.UI
{
	[System.Serializable]
	public class BadgeHierarchyTableMaker : ScriptableObject
	{
		const string BadgeHierarchyTablePath = "Assets/Project/Bundles/99_Common/Badge_AAG";
		const string BadgeHierarchyTableFileName = "BadgeHierarchy.asset";
		
		public BadgeTreeNode<string> _root = null;

#if UNITY_EDITOR
		[MenuItem("Com2Verse/CreateBadgeHierarchyTable")]
		static void CreateBadgeHierarchyTable()
		{
			string fullPath = BadgeHierarchyTablePath + "/" + BadgeHierarchyTableFileName;
			var instance = AssetDatabase.LoadAssetAtPath<BadgeHierarchyTableMaker>(fullPath);
			if (instance == null)
			{
				instance = CreateInstance<BadgeHierarchyTableMaker>();
				AssetDatabase.CreateAsset(instance, fullPath);
			}

			Selection.activeObject = instance;
			AssetDatabase.Refresh();
		}
#endif
		public static void Load(System.Action<BadgeTreeNode<string>> onComplete)
		{
			string fullPath = BadgeHierarchyTableFileName;
			var loadedAsset = C2VAddressables.LoadAsset<BadgeHierarchyTableMaker>(fullPath);
			onComplete?.Invoke(loadedAsset.Result._root);
		}

		public static async UniTask<BadgeHierarchyTableMaker> LoadAsync()
		{
			string fullPath = BadgeHierarchyTableFileName;
			var loadedAsset = await C2VAddressables.LoadAssetAsync<BadgeHierarchyTableMaker>(fullPath).ToUniTask();
			if (loadedAsset == null)
			{
				Debug.LogError($"Unable to load BadgeHierarchyTableMaker. {fullPath}");
				return null;
			}

			return loadedAsset;
		}
	}
}
