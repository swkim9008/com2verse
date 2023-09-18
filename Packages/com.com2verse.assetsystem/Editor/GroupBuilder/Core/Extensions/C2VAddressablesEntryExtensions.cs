/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesEntryExtensions.cs
* Developer:	tlghks1009
* Date:			2023-03-14 18:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using System;
using System.Collections.Generic;
using Com2Verse.AssetSystem;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Com2VerseEditor.AssetSystem
{
	public static class C2VAddressablesEntryExtensions
	{
		public static void SetLabelsByLabelEnumValue(this AddressableAssetEntry entry, int labelEnumValue)
		{
			if (!entry.SetLabel(((eAssetBundleType) labelEnumValue).ToString(), true, true))
			{
				Debug.LogError($"[Addressables] Unable to add label. Address : {entry.address}, LabelType : {((eAssetBundleType) labelEnumValue).ToString()}");
			}
		}

		public static bool HasLabel(this AddressableAssetEntry entry, int labelEnumValue)
		{
			var labels = entry.labels;
			var assetBundleType = (eAssetBundleType) labelEnumValue;

			return labels.Contains(assetBundleType.ToString());
		}


		public static IEnumerable<AddressableAssetEntry> GetEntries(this AddressableAssetSettings thisSettings)
		{
			foreach (var group in thisSettings.groups)
			{
				foreach (var assetEntry in group.entries)
				{
					yield return assetEntry;
				}
			}
		}


		public static bool Equals(this AddressableAssetEntry entry, string address, string groupName, int labelEnumValue)
		{
			if (entry.address != address)
			{
				return false;
			}

			if (entry.parentGroup.name != groupName)
			{
				return false;
			}

			if (!entry.HasLabel(labelEnumValue))
			{
				return false;
			}

			return true;
		}
	}
}
