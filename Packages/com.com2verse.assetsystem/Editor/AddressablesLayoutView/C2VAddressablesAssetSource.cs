/*===============================================================
* Product:		Com2Verse
* File Name:	C2VResourceDiagnostic.cs
* Developer:	tlghks1009
* Date:			2023-03-21 12:15
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using Com2Verse.AssetSystem;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Com2VerseEditor.AssetSystem
{
	[Serializable]
	public class C2VAddressableAssetInfo
	{
		[field: SerializeField] public string ParentGroup { get; set; }
		[field: SerializeField] public  string Guid        { get; private set; }

		[field: SerializeField] private List<string> _referenceBy;
		[field: SerializeField] private List<string> _referenceTo;

		public IReadOnlyList<string> ReferenceBy => _referenceBy;
		public IReadOnlyList<string> ReferenceTo => _referenceTo;

		public C2VAddressableAssetInfo(string guid)
		{
			Guid          = guid;
			_referenceBy  = new List<string>();
			_referenceTo  = new List<string>();
		}

		public void AddReferenceBy(string reference)
		{
			if (!_referenceBy.Contains(reference))
			{
				_referenceBy.Add(reference);
			}
		}


		public void AddReferenceBy(IEnumerable<string> referenceList)
		{
			foreach (var reference in referenceList)
			{
				AddReferenceBy(reference);
			}
		}

		public void AddReferenceTo(string reference)
		{
			if (!_referenceTo.Contains(reference))
			{
				_referenceTo.Add(reference);
			}
		}

		public void AddReferenceTo(IEnumerable<string> referenceList)
		{
			foreach (var reference in referenceList)
			{
				AddReferenceTo(reference);
			}
		}
	}


	[Serializable]
	public class C2VAddressableAssetInfos
	{
		[field: SerializeField] private List<C2VAddressableAssetInfo> _bundles;
		public IReadOnlyList<C2VAddressableAssetInfo> Bundles => _bundles;

		public C2VAddressableAssetInfos()
		{
			_bundles = new List<C2VAddressableAssetInfo>();
		}

		public void Add(C2VAddressableAssetInfo info)
		{
			_bundles.Add(info);
		}
	}


	public static class C2VAddressablesAssetSource
	{
		private static Dictionary<string, C2VAddressableAssetInfo> _addressableAssetInfos;

		private static Dictionary<string, List<C2VAddressableAssetInfo>> _addressableGroupInfos;

		private static AddressableAssetSettings _settings;

		private static string _filePath = $"Assets/{C2VPaths.BundleLayoutDirectoryPath}/{C2VPaths.BundleLayoutFileName}";

		private static readonly string _pngExtensionName   = ".png";
		private static readonly string _atlasExtensionName = ".spriteatlasv2";

		public static bool TryLoad()
		{
			if (!File.Exists(_filePath))
			{
				return false;
			}

			_addressableAssetInfos = new Dictionary<string, C2VAddressableAssetInfo>();
			_addressableGroupInfos = new Dictionary<string, List<C2VAddressableAssetInfo>>();

			var json                  = File.ReadAllText(_filePath);
			var addressableAssetInfos = JsonUtility.FromJson<C2VAddressableAssetInfos>(json);

			foreach (var info in addressableAssetInfos.Bundles)
			{
				if (_addressableGroupInfos.TryGetValue(info.ParentGroup, out var infos))
				{
					infos.Add(info);
				}
				else
				{
					infos = new List<C2VAddressableAssetInfo> {info};

					_addressableGroupInfos.Add(info.ParentGroup, infos);
				}

				_addressableAssetInfos.Add(info.Guid, info);
			}

			return true;
		}


		/// <summary>
		/// 에셋 번들 전체 Dependency 재설정
		/// </summary>
		public static void Refresh()
		{
			_settings = AddressableAssetSettingsDefaultObject.Settings;

			_addressableAssetInfos = new Dictionary<string, C2VAddressableAssetInfo>();
			_addressableGroupInfos = new Dictionary<string, List<C2VAddressableAssetInfo>>();

			var spriteDictionary = FindAssetBundleNameOfSprites();
			var addressableAssetTotalInfo = new C2VAddressableAssetInfos();
			var progressStatus            = new C2VAddressablesEditorDisplayProgress() {TotalCount = _settings.groups.Count};

			foreach (var group in _settings.groups)
			{
				progressStatus.Name = group.name;

				var addressableAssetInfoList = new List<C2VAddressableAssetInfo>();

				foreach (var assetEntry in group.entries)
				{
					var assetGuid = assetEntry.guid;
					var assetPath = assetEntry.AssetPath;

					if (!_addressableAssetInfos.TryGetValue(assetGuid, out var info))
					{
						info = new C2VAddressableAssetInfo(assetGuid);
						info.ParentGroup = group.name;

						_addressableAssetInfos.Add(assetGuid, info);
					}

					addressableAssetInfoList.Add(info);
					addressableAssetTotalInfo.Add(info);

					foreach (var dependencyOfAssetEntry in AssetDatabase.GetDependencies(assetPath))
					{
						var assetGuidOfDependency = AssetDatabase.GUIDFromAssetPath(dependencyOfAssetEntry).ToString();

						MakeAddressableAssetInfoIfNotExist(assetGuidOfDependency, out var dependencyInfo);

						var assetEntryOfDependency = _settings.FindAssetEntry(assetGuidOfDependency);

						if (assetEntryOfDependency == null)
						{
							if (spriteDictionary.TryGetValue(dependencyOfAssetEntry, out var dependencyGuid))
							{
								assetEntryOfDependency = _settings.FindAssetEntry(dependencyGuid);

								assetGuidOfDependency = dependencyGuid;

								MakeAddressableAssetInfoIfNotExist(assetGuidOfDependency, out dependencyInfo);
							}
							else
							{
								continue;
							}
						}

						dependencyInfo.ParentGroup = assetEntryOfDependency.parentGroup.name;

						_addressableAssetInfos[assetGuidOfDependency].AddReferenceBy(assetEntry.parentGroup.name);
						_addressableAssetInfos[assetGuid].AddReferenceTo(assetEntryOfDependency.parentGroup.name);
					}
				}

				_addressableGroupInfos.Add(group.name, addressableAssetInfoList);

				progressStatus.Current++;
				progressStatus.DisplayProgressBar();
			}

			void MakeAddressableAssetInfoIfNotExist(string path, out C2VAddressableAssetInfo assetInfo)
			{
				if (!_addressableAssetInfos.TryGetValue(path, out var result))
				{
					assetInfo = new C2VAddressableAssetInfo(path);

					_addressableAssetInfos.Add(path, assetInfo);

					return;
				}

				assetInfo = result;
			}

			SaveFile(JsonUtility.ToJson(addressableAssetTotalInfo));
		}


		/// <summary>
		/// Key : SpriteGuid, Value : AssetBundleName
		/// </summary>
		/// <returns></returns>
		private static Dictionary<string, string> FindAssetBundleNameOfSprites()
		{
			var spriteDictionary = new Dictionary<string, string>();

			foreach (var assetEntry in _settings.GetEntries())
			{
				var entryGuid      = assetEntry.guid;
				var assetPath      = assetEntry.AssetPath;
				var assetExtension = Path.GetExtension(assetPath);

				if (assetExtension != _atlasExtensionName)
				{
					continue;
				}

				foreach (var dependencyOfAssetEntry in AssetDatabase.GetDependencies(assetPath))
				{
					var assetGuidOfDependency  = AssetDatabase.GUIDFromAssetPath(dependencyOfAssetEntry).ToString();
					var assetEntryOfDependency = _settings.FindAssetEntry(assetGuidOfDependency);

					if (assetEntryOfDependency == null)
					{
						var extension = Path.GetExtension(dependencyOfAssetEntry);
						if (extension == _pngExtensionName)
						{
							if (!spriteDictionary.ContainsKey(dependencyOfAssetEntry))
							{
								spriteDictionary.Add(dependencyOfAssetEntry, entryGuid);
							}
						}
					}
				}
			}

			return spriteDictionary;
		}


		public static bool TryGetAddressableAssetInfo(string guid, out C2VAddressableAssetInfo info)
		{
			if (string.IsNullOrEmpty(guid))
			{
				info = null;
				return false;
			}

			if (_addressableAssetInfos.TryGetValue(guid, out var addressableAssetInfo))
			{
				info = addressableAssetInfo;
				return true;
			}

			info = null;
			return false;
		}


		public static bool TryGetAddressableAssetInfos(string bundleName, out List<C2VAddressableAssetInfo> infos)
		{
			if (string.IsNullOrEmpty(bundleName))
			{
				infos = null;
				return false;
			}

			if (_addressableGroupInfos.TryGetValue(bundleName, out var addressableAssetInfos))
			{
				infos = addressableAssetInfos;
				return true;
			}

			infos = null;
			return false;
		}


		private static void SaveFile(string json)
		{
			if (File.Exists(_filePath))
			{
				File.Delete(_filePath);
			}
			File.WriteAllText(_filePath, json);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
}
