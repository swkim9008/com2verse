/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesEntryOperationINfo.cs
* Developer:	tlghks1009
* Date:			2023-03-07 12:27
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using UnityEditor.AddressableAssets.Settings;

namespace Com2VerseEditor.AssetSystem
{
	public class C2VAddressablesEntryOperationInfo
	{
		public C2VAddressableEntryCreateInfo EntryCreateInfo { get; set; }

		public C2VAddressableEntryRemoveInfo EntryRemoveInfo { get; set; }

		public C2VAddressableGroupRemoveInfo GroupRemoveInfo { get; set; }

		public C2VAddressablesEntryOperationInfo(C2VAddressableEntryCreateInfo createInfo, C2VAddressableEntryRemoveInfo removeInfo, C2VAddressableGroupRemoveInfo removeGroupInfo)
		{
			EntryCreateInfo = createInfo;
			EntryRemoveInfo = removeInfo;
			GroupRemoveInfo = removeGroupInfo;
		}
	}


	public class C2VAddressableEntryInfo
	{
		public bool IsValid { get; set; }
	}


	public class C2VAddressableEntryRemoveInfo : C2VAddressableEntryInfo
	{
		public string AssetPath { get; }
		public string Guid { get; }
		public string GroupName { get; }
		public AddressableAssetEntry AddressableAssetEntry { get; }

		public C2VAddressableEntryRemoveInfo(string assetPath, string guid, string groupName, AddressableAssetEntry addressableAssetEntry)
		{
			AssetPath = assetPath;
			Guid = guid;
			GroupName = groupName;
			AddressableAssetEntry = addressableAssetEntry;
		}
	}


	public class C2VAddressableEntryCreateInfo : C2VAddressableEntryInfo
	{
		public string AssetPath { get; }
		public string Address { get; }
		public string Guid { get; }
		public string GroupName { get; }
		public int LabelEnumValue { get; }
		public AddressableAssetEntry AddressableAssetEntry { get; }


		public C2VAddressableEntryCreateInfo(string assetPath, string address, string guid, string groupName, int labelEnumValue, AddressableAssetEntry addressableAssetEntry)
		{
			AssetPath = assetPath;
			Address = address;
			Guid = guid;
			GroupName = groupName;
			LabelEnumValue = labelEnumValue;
			AddressableAssetEntry = addressableAssetEntry;
		}
	}


	public class C2VAddressableGroupRemoveInfo : C2VAddressableEntryInfo
	{
		public string GroupName { get; }

		public C2VAddressableGroupRemoveInfo(string groupName)
		{
			GroupName = groupName;
		}
	}
}
