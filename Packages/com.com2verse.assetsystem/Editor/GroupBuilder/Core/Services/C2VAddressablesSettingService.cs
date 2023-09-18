/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesSettingService.cs
* Developer:	tlghks1009
* Date:			2023-03-31 15:28
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Com2VerseEditor.AssetSystem
{
	public sealed class C2VAddressablesSettingService : C2VAddressablesGroupBuilderServiceBase, IPostProcessor, IEnvironmentSetting
	{
		readonly struct BuildAndLoadPath
		{
			public string LoadPath { get; }
			public string BuildPath { get; }

			public BuildAndLoadPath(string profileName)
			{
				LoadPath  = $"{profileName}.LoadPath";
				BuildPath = $"{profileName}.BuildPath";
			}
		}

		private readonly AddressableAssetSettings _settings;


		public C2VAddressablesSettingService(IServicePack servicePack) : base(servicePack)
		{
			_settings = AddressableAssetSettingsDefaultObject.Settings;
		}


		public void Execute(eEnvironment environment) => ApplyTo(environment);


		public void ApplyTo(eEnvironment environment)
		{
			SetupCatalogSetting(environment);

			SetupUpdatePreviousBuildSetting(environment);

			SetupBuildSetting();

			base.SaveAssets(_settings);
		}


		private void SetupCatalogSetting(eEnvironment environment)
		{
			var assetBundleVersion  = C2VEditorPath.AssetBundleVersion;
			var buildEnvironment    = C2VEditorPath.BuildEnvironment;

			Debug.Log($"[AssetBundle] AssetBundleVersion : {assetBundleVersion}");

			_settings.OverridePlayerVersion         = $"Com2Verse_{buildEnvironment}_{assetBundleVersion}";
			_settings.BundleLocalCatalog            = false;
			_settings.BuildRemoteCatalog            = true;
			_settings.OptimizeCatalogSize           = false;
			_settings.DisableCatalogUpdateOnStartup = true;
			_settings.CatalogRequestsTimeout        = 30;

			var buildAndLoadPath = new BuildAndLoadPath($"{FindProfileName(environment)}Catalog");

			_settings.RemoteCatalogBuildPath.SetVariableByName(_settings, buildAndLoadPath.BuildPath);
			_settings.RemoteCatalogLoadPath.SetVariableByName(_settings, buildAndLoadPath.LoadPath);
		}


		private void SetupUpdatePreviousBuildSetting(eEnvironment environment)
		{
			_settings.CheckForContentUpdateRestrictionsOption = CheckForContentUpdateRestrictionsOptions.ListUpdatedAssetsWithRestrictions;
			_settings.ContentStateBuildPath = _settings.profileSettings.GetValueByName(_settings.activeProfileId,
			                                                                           $"{FindProfileName(environment)}ContentState");
		}


		private void SetupBuildSetting()
		{
			_settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;
			_settings.IgnoreUnsupportedFilesInBuild = false;
			_settings.UniqueBundleIds = true;
			_settings.ContiguousBundles = true;
			_settings.NonRecursiveBuilding = true;
			_settings.ShaderBundleNaming = ShaderBundleNaming.ProjectName;
			_settings.MonoScriptBundleNaming = MonoScriptBundleNaming.Disabled;
			_settings.DisableVisibleSubAssetRepresentations = false;
		}


		private string FindProfileName(eEnvironment environment)
		{
			return environment switch
			{
				eEnvironment.LOCAL         => "Local",
				eEnvironment.REMOTE        => "Remote",
				eEnvironment.EDITOR_HOSTED => "EditorHosted",
				_                          => throw new ArgumentOutOfRangeException(nameof(environment), environment, null)
			};
		}
	}
}
