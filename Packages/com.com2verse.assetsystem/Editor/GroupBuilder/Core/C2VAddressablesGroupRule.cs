/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressableGroupRuleTreeItem.cs
* Developer:	tlghks1009
* Date:			2023-03-03 10:27
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.AssetSystem;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Com2VerseEditor.AssetSystem
{
	public enum eProjectRootPath
	{
		ASSETS,
		PACKAGES
	}


	public enum eAddressType
	{
		ASSET_NAME,
		ASSET_NAME_WITHOUT_EXTENSIONS,
		ADDRESSABLE_PATH,
		ADDRESSABLE_PATH_WITHOUT_EXTENSIONS,
	}

	public enum eEnvironment
	{
		LOCAL = 0,
		EDITOR_HOSTED = 1,
		REMOTE = 2,
	}

	public sealed class C2VAddressableGroupRuleTreeItem : TreeViewItem
	{
		public C2VAddressableGroupRuleEntry GroupRule { get; set; }

		public eProjectRootPath ProjectRootPath
		{
			get => GroupRule.GetProjectRootPath();
			set => GroupRule.ProjectRootPath = value.ToString();
		}

		public string RootPath
		{
			get => GroupRule.RootPath;
			set => GroupRule.RootPath = value;
		}
		public string PathRule
		{
			get => GroupRule.PathRule;
			set => GroupRule.PathRule = value;
		}

		public string GroupName
		{
			get => GroupRule.GroupName;
			set => GroupRule.GroupName = value;
		}

		public eAddressType AddressType
		{
			get => GroupRule.GetAddressType();
			set => GroupRule.AddressType = value.ToString();
		}

		public eAssetBundleType Label
		{
			get => (eAssetBundleType) GroupRule.LabelEnumValue;
			set => GroupRule.LabelEnumValue = (int) value;
		}
	}

	[Serializable]
	public class C2VAddressableGroupRuleEntry
	{
		[field: SerializeField] public string ProjectRootPath { get; set; }

		[field: SerializeField] public string RootPath { get; set; }

		[field: SerializeField] public string PathRule { get; set; }

		[field: SerializeField] public string GroupName { get; set; }

		[field: SerializeField] public string AddressType { get; set; }

		[field: SerializeField] public int LabelEnumValue { get; set; }

		public eProjectRootPath GetProjectRootPath()
		{
			if (string.IsNullOrEmpty(ProjectRootPath))
			{
				return eProjectRootPath.ASSETS;
			}

			return (eProjectRootPath) Enum.Parse(typeof(eProjectRootPath), ProjectRootPath!);
		}


		public eAddressType GetAddressType()
		{
			if (string.IsNullOrEmpty(AddressType))
			{
				return eAddressType.ASSET_NAME;
			}

			return (eAddressType) Enum.Parse(typeof(eAddressType), AddressType!);
		}

		public C2VAddressableGroupRuleEntry() { }

		public C2VAddressableGroupRuleEntry(C2VAddressableGroupRuleTreeItem treeItem)
		{
			ProjectRootPath = treeItem.GroupRule.ProjectRootPath;
			RootPath        = treeItem.RootPath;
			PathRule        = treeItem.PathRule;
			GroupName       = treeItem.GroupName;
			AddressType     = treeItem.GroupRule.AddressType;
			LabelEnumValue  = treeItem.GroupRule.LabelEnumValue;
		}
	}


	[Serializable]
	public class C2VAddressablesGroupRule : ScriptableObject
	{
		[field: SerializeField] public C2VAddressableGroupRuleEntry[] GroupRuleEntries { get; set; }

		[field: SerializeField] public bool AutoPackaging { get; set; }

		[field: SerializeField] public eEnvironment Environment { get; set; }
	}
}
