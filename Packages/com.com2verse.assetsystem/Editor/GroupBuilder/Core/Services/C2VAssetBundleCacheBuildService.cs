/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesSource.cs
* Developer:	tlghks1009
* Date:			2023-03-28 17:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.IO;
using Com2Verse.AssetSystem;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Com2VerseEditor.AssetSystem
{
	public sealed class C2VAssetBundleCacheBuildService : C2VAddressablesGroupBuilderServiceBase, IPostProcessor
	{
		private readonly string _fixedUnityBuiltinShadersGroupName = "unitybuiltinshaders";

		private AddressableAssetSettings _settings;

		public C2VAssetBundleCacheBuildService(IServicePack servicePack) : base(servicePack)
		{
			_settings = AddressableAssetSettingsDefaultObject.Settings;
		}

		public void Execute(eEnvironment environment)
		{
			var cacheCollection = new C2VAssetBundleCacheCollection();
			cacheCollection.AddLayout(new C2VAssetBundleCacheEntity()
			{
				CacheName  = eAssetBundleType.BUILT_IN.ToString(),
				BundleName = _fixedUnityBuiltinShadersGroupName
			});

			foreach (var group in _settings.groups)
			{
				foreach (var entry in group.entries)
				{
					var labelHashSet = entry.labels;

					foreach (var label in labelHashSet)
					{
						var cacheEntity = new C2VAssetBundleCacheEntity
						{
							CacheName = label,
							BundleName = group.Name
						};

						cacheCollection.AddLayout(cacheEntity);
						break;
					}

					break;
				}
			}

			CreateCacheInfoFile(cacheCollection);
		}


		public override void Release() => _settings = null;


		private void MakeCacheInfoDirectoryIfNotExist()
		{
			if (!Directory.Exists(C2VEditorPath.BundleCacheInfoDirectoryPath))
			{
				Directory.CreateDirectory(C2VEditorPath.BundleCacheInfoDirectoryPath);
			}
		}


		private void DeleteOldCacheInfoFileIfExist()
		{
			AssetDatabase.DeleteAsset("Assets/Project/Resources/AssetBundle/AddressableCacheInfo.asset");
		}


		private void CreateCacheInfoFile(C2VAssetBundleCacheCollection cacheCollection)
		{
			MakeCacheInfoDirectoryIfNotExist();

			DeleteOldCacheInfoFileIfExist();

			var jsonData = JsonUtility.ToJson(cacheCollection);

			File.WriteAllText($"{C2VEditorPath.BundleCacheInfoDirectoryPath}/{C2VEditorPath.BundleCacheInfoFileName}", jsonData);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
}
