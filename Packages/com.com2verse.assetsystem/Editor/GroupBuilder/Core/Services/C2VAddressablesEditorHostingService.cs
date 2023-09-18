/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesHostingService.cs
* Developer:	tlghks1009
* Date:			2023-08-29 15:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using Com2Verse.BuildHelper;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.HostingServices;
using UnityEditor.AddressableAssets.Settings;

namespace Com2VerseEditor.AssetSystem
{
	public sealed class C2VAddressablesEditorHostingService : C2VAddressablesGroupBuilderServiceBase, IPreProcessor, IEnvironmentSetting
	{
		private readonly AddressableAssetSettings _settings;

		private readonly int USE_ASSET_DATABASE = 0;
		private readonly int USE_EXISTING_BUILD = 2;

		public C2VAddressablesEditorHostingService(IServicePack servicePack) : base(servicePack)
		{
			_settings = AddressableAssetSettingsDefaultObject.Settings;
		}


		public void Execute(eEnvironment environment)
		{
			if (environment == eEnvironment.EDITOR_HOSTED)
			{
				StartHostingService();

				SetActivePlayModeScript(USE_EXISTING_BUILD);

				SetAppInfo(eAssetBuildType.EDITOR_HOSTED);
			}
		}

		public void ApplyTo(eEnvironment environment)
		{
			if (environment == eEnvironment.LOCAL)
			{
				StopHostingService();

				SetActivePlayModeScript(USE_ASSET_DATABASE);

				SetAppInfo(eAssetBuildType.LOCAL);
			}
		}


		private void SetAppInfo(eAssetBuildType assetBuildType) => AppInfo.Instance.UpdateAssetBuildType(assetBuildType);

		private void SetActivePlayModeScript(int index) => AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilderIndex = index;


		private void StartHostingService()
		{
			if (_settings == null)
				return;

			RemoveAllHostingService();
			AddHostingService();

			var hostingServices = _settings.HostingServicesManager.HostingServices.ToList();
			var firstHostingService = hostingServices[0];

			firstHostingService.StartHostingService();
		}


		private void StopHostingService()
		{
			if (_settings == null)
				return;

			if (_settings.HostingServicesManager.HostingServices.Count != 0)
				_settings.HostingServicesManager.StopAllServices();
		}


		private void AddHostingService()
		{
			var hostingName = "C2V Editor Hosting";

			_settings.HostingServicesManager.AddHostingService(typeof(HttpHostingService), hostingName);
		}


		private void RemoveAllHostingService()
		{
			if (_settings == null)
				return;

			if (_settings.HostingServicesManager.HostingServices.Count != 0)
			{
				var hostingServices = new List<IHostingService>(_settings.HostingServicesManager.HostingServices);

				foreach (var hostingService in hostingServices)
				{
					_settings.HostingServicesManager.RemoveHostingService(hostingService);
				}
			}
		}
	}
}
