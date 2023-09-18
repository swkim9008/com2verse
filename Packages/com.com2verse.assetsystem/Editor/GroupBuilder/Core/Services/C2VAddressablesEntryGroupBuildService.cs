/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesEntryGroupBuildService.cs
* Developer:	tlghks1009
* Date:			2023-03-07 11:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Text.RegularExpressions;

namespace Com2VerseEditor.AssetSystem
{
	public class C2VAddressablesEntryGroupBuildService : C2VAddressablesGroupBuilderServiceBase
	{
		private C2VAddressablesEditorAdapter _addressableEditorAdapter;

		private C2VAddressablesPathGenerator _addressablePathGenerator;

		private C2VAssetDataBaseAdapter _assetDataBaseAdapter;

		public C2VAddressablesEntryGroupBuildService(IServicePack servicePack) : base(servicePack)
		{
			_addressablePathGenerator = new C2VAddressablesPathGenerator();

			_addressableEditorAdapter = servicePack.GetAdapterPack().AddressablesEditorAdapter;

			_assetDataBaseAdapter = servicePack.GetAdapterPack().AssetDataBaseAdapter;
		}


		public C2VAssetDataBaseAdapter AssetDataBaseAdapter => _assetDataBaseAdapter;

		public C2VAddressablesPathGenerator AddressablePathGenerator => _addressablePathGenerator;

		public C2VAddressablesEditorAdapter AddressableEditorAdapter => _addressableEditorAdapter;


		public C2VAddressableEntryCreateInfo BuildCreateEntryOperationInfoByRule(string assetPath, C2VAddressableGroupRuleEntry rule)
		{
			var addressablePath = _addressablePathGenerator.GenerateFromAssetPath(rule.GetProjectRootPath(), rule.RootPath, assetPath);

			if (string.IsNullOrEmpty(addressablePath))
			{
				return null;
			}

			var regex = new Regex(rule.PathRule!);
			var match = regex.Match(assetPath!);

			if (match.Success)
			{
				var address = _addressablePathGenerator.GeneratePathByAddressType(rule.GetAddressType(), assetPath);
				var guid = _assetDataBaseAdapter.GetGuid(assetPath);
				var addressableAssetEntry = _addressableEditorAdapter.FindAssetEntry(guid);
				var label = rule.LabelEnumValue;

				var groupName = regex.Replace(assetPath, rule.GroupName);
				groupName = groupName.Replace("/", "_").ToLower();
				groupName = groupName.Replace("assets_project_bundles_", "");

				return new C2VAddressableEntryCreateInfo(assetPath, address, guid, groupName, label, addressableAssetEntry);
			}

			return null;
		}


		public C2VAddressableEntryRemoveInfo BuildRemoveEntryOperationInfo(string assetPath, C2VAddressableGroupRuleEntry rule)
		{
			if (rule != null)
			{
				return BuildRemoveEntryOperationInfoByRule(assetPath, rule);
			}

			var guid = _assetDataBaseAdapter.GetGuid(assetPath);
			var groupName = _addressableEditorAdapter.FindGroupName(guid);
			var addressableAssetEntry = _addressableEditorAdapter.FindAssetEntry(guid);

			return new C2VAddressableEntryRemoveInfo(assetPath, guid, groupName, addressableAssetEntry);
		}


		public C2VAddressableEntryRemoveInfo BuildRemoveEntryOperationInfoByRule(string assetPath, C2VAddressableGroupRuleEntry rule)
		{
			var addressablePath = _addressablePathGenerator.GenerateFromAssetPath(rule.GetProjectRootPath(), rule.RootPath, assetPath);

			if (string.IsNullOrEmpty(addressablePath))
			{
				return null;
			}

			var regex = new Regex(rule.PathRule!);
			var match = regex.Match(assetPath!);

			if (match.Success)
			{
				var guid = _assetDataBaseAdapter.GetGuid(assetPath);
				var groupName = _addressableEditorAdapter.FindGroupName(guid);
				var addressableAssetEntry = _addressableEditorAdapter.FindAssetEntry(guid);

				return new C2VAddressableEntryRemoveInfo(assetPath, guid, groupName, addressableAssetEntry);
			}

			return null;
		}


		public C2VAddressableGroupRemoveInfo BuildRemoveGroupOperationInfoByRule(string assetPath, C2VAddressableGroupRuleEntry rule)
		{
			var addressablePath = _addressablePathGenerator.GenerateFromAssetPath(rule.GetProjectRootPath(), rule.RootPath, assetPath);

			if (string.IsNullOrEmpty(addressablePath))
			{
				return null;
			}

			var regex = new Regex(rule.PathRule!);
			var match = regex.Match(assetPath!);

			if (match.Success)
			{
				var guid = _assetDataBaseAdapter.GetGuid(assetPath);
				var groupName = _addressableEditorAdapter.FindGroupName(guid);

				return new C2VAddressableGroupRemoveInfo(groupName);
			}

			return null;
		}


		public override void Release()
		{
			_addressableEditorAdapter = null;

			_addressablePathGenerator = null;

			_assetDataBaseAdapter = null;
		}
	}
}



