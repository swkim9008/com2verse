/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesGroupSettings.cs
* Developer:	tlghks1009
* Date:			2023-03-30 14:11
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using Com2Verse.AssetSystem;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace Com2VerseEditor.AssetSystem
{
	public sealed class C2VAddressablesGroupSettingService : C2VAddressablesGroupBuilderServiceBase, IPostProcessor, IEnvironmentSetting
	{
		private AddressableAssetSettings _settings;

		private string _defaultLocalGroupName = "defaultLocalGroup";


		public C2VAddressablesGroupSettingService(IServicePack servicePack) : base(servicePack)
		{
			_settings = AddressableAssetSettingsDefaultObject.Settings;
		}


		public void Execute(eEnvironment environment) => ApplyTo(environment);


		public void ApplyTo(eEnvironment environment)
		{
			var groupTemplate = _settings.GetGroupTemplateObject((int) environment) as AddressableAssetGroupTemplate;
			var builtInInGroupTemplate = _settings.GetGroupTemplateObject((int) eEnvironment.LOCAL) as AddressableAssetGroupTemplate;

			foreach (var group in _settings.groups)
			{
				if (group.name == _defaultLocalGroupName)
				{
					builtInInGroupTemplate.ApplyToAddressableAssetGroup(group);
					EditorUtility.SetDirty(group);
					continue;
				}

				if (group.entries.Count > 0)
				{
					var entry = group.entries.ToList()[0];
					foreach (var label in entry.labels)
					{
						if (label == eAssetBundleType.BUILT_IN.ToString())
						{
							builtInInGroupTemplate.ApplyToAddressableAssetGroup(group);
						}
						else
						{
							groupTemplate.ApplyToAddressableAssetGroup(group);
						}

						EditorUtility.SetDirty(group);
						break;
					}
				}
			}

			var adapterPack = base.ServicePack.GetAdapterPack();
			adapterPack.AddressablesEditorAdapter.RemoveMissingReferences();

			base.SaveAssets();
		}


		public override void Release()
		{
			_settings = null;
		}
	}
}
