/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressableRepository.cs
* Developer:	tlghks1009
* Date:			2023-03-03 17:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Com2Verse.AssetSystem;
using UnityEditor.IMGUI.Controls;

namespace Com2VerseEditor.AssetSystem
{
	public class C2VAddressablesGroupBuilderStorageService : C2VAddressablesGroupBuilderServiceBase
	{
		private readonly string _filePath;
		private readonly string _fileName;

		private C2VAssetDataBaseAdapter _assetDataBaseAdapter;

		private C2VAddressablesGroupRule _addressableGroupRule;

		public C2VAddressablesGroupRule AddressableGroupRuleData => _addressableGroupRule;

		public C2VAddressableGroupRuleEntry[] GroupRuleEntries => AddressableGroupRuleData.GroupRuleEntries;

		private string DirectoryPath => $"Assets/{C2VPaths.GroupBuilderDirectoryPath}";
		private string FilePath      => $"{DirectoryPath}/{C2VPaths.GroupBuilderFileName}";


		public C2VAddressablesGroupBuilderStorageService(IServicePack servicePack) : base(servicePack)
		{
			_assetDataBaseAdapter = servicePack.GetAdapterPack().AssetDataBaseAdapter;

			LoadGroupRuleItems();
		}


		public T CreateScriptableObjectInstance<T>() where T : ScriptableObject => ScriptableObject.CreateInstance<T>();


		public void Save(IEnumerable<TreeViewItem> items)
		{
			var groupRulesEntries = new List<C2VAddressableGroupRuleEntry>();

			foreach (var treeViewItem in items)
			{
				var item  = (C2VAddressableGroupRuleTreeItem) treeViewItem;
				var entry = new C2VAddressableGroupRuleEntry(item);

				groupRulesEntries.Add(entry);
			}

			_addressableGroupRule = _assetDataBaseAdapter.LoadAssetAtPath<C2VAddressablesGroupRule>(FilePath);

			if (_addressableGroupRule == null)
			{
				MakeDirectoryIfNotExist(DirectoryPath);

				_addressableGroupRule = CreateScriptableObjectInstance<C2VAddressablesGroupRule>();

				_assetDataBaseAdapter.CreateAsset(_addressableGroupRule, FilePath);
			}

			_addressableGroupRule.GroupRuleEntries = groupRulesEntries.ToArray();

			SaveAssets(_addressableGroupRule);

			LoadGroupRuleItems();
		}


		public void SaveOption() => SaveAssets(_addressableGroupRule);


		private void LoadGroupRuleItems()
		{
			_addressableGroupRule = _assetDataBaseAdapter.LoadAssetAtPath<C2VAddressablesGroupRule>(FilePath);

			if (_addressableGroupRule == null)
			{
				Debug.LogError("Addressable Group Builder Window를 다시 오픈해주세요.");
			}
		}


		public bool IsAutoPackaging()
		{
			if (_addressableGroupRule == null)
			{
				return true;
			}

			return _addressableGroupRule.AutoPackaging;
		}


		private void MakeDirectoryIfNotExist(string directoryPath)
		{
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}
		}


		public override void Release()
		{
			_assetDataBaseAdapter = null;
			_addressableGroupRule = null;
		}
	}
}
