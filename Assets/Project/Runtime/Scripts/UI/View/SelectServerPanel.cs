/*===============================================================
* Product:		Com2Verse
* File Name:	UISelectServerView.cs
* Developer:	tlghks1009
* Date:			2022-06-14 19:47
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.BuildHelper;
using Com2Verse.Chat;
using Com2Verse.Network;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class SelectServerPanel : MonoBehaviour
	{
		[SerializeField] private        TMP_Dropdown    _dropDown;
		[SerializeField] private        TMP_Text        _serverAddress;
		[SerializeField] private        TMP_Text        _serverPort;
		[SerializeField] private        TMP_Text        _targetDomain;
		[SerializeField] private        TMP_Text        _argoStatus;
		[SerializeField] private        MetaverseButton _refreshButton;
		[SerializeField] private        MetaverseButton _loadButton;
		[NotNull]        private static Configurator    Config => Configurator.Instance;

		private static readonly bool SkipFileServerConfig = true;

		private void Awake()
		{
			RefreshUI();
		}

		void RefreshUI()
		{
			gameObject.SetActive(AppInfo.Instance.Data.IsDebug);

			switch (AppInfo.Instance.Data.Environment)
			{
				case eBuildEnv.DEV:
				case eBuildEnv.DEV_INTEGRATION:
				{
					gameObject.SetActive(true);
					RefreshDropdown(Configurator.Instance.Config);
				}
					break;

				case eBuildEnv.QA:
#if UNITY_EDITOR
				{
					gameObject.SetActive(true);
					RefreshDropdown(Configurator.Instance.Config);
				}
#else
				{
					gameObject.SetActive(false);
					LoadConfigJson();
				}
#endif
					break;

				case eBuildEnv.STAGING:
				case eBuildEnv.PRODUCTION:
				{
					gameObject.SetActive(false);
					LoadConfigJson();
				}
					break;

				default:
					break;
			}
		}

		private void RefreshDropdown(Configuration configurations = null)
		{
			_dropDown.onValueChanged.AddListener(OnDropDownValueChanged);
			_dropDown.options.Clear();
			_dropDown.options.AddRange(GetServerNames(configurations));
			_dropDown.RefreshShownValue();

			_dropDown.value = Config.ServerType;
			OnDropDownValueChanged(_dropDown.value);
#if UNITY_EDITOR
			if (!SkipFileServerConfig)
			{
				Config.OnNeedRefresh += () =>
				{
					_dropDown.options.Clear();
					_dropDown.options.AddRange(GetServerNames());
					_dropDown.RefreshShownValue();
					OnDropDownValueChanged(_dropDown.value);
				};
				Config.LoadFileServerConfigAsync().Forget();
			}
#endif // UNITY_EDITOR
		}

		private IEnumerable<TMP_Dropdown.OptionData> GetServerNames(Configuration configurations = null)
		{
			var configs = configurations == null ? Config.ServerConfigurations?.DevConfigurations?[0]?.ServerLists : configurations.ServerLists;
			if (configs == null) yield break;

			foreach (var config in configs)
				if (config != null)
					yield return new TMP_Dropdown.OptionData(config.Name);
		}

		private void OnDropDownValueChanged(int dropdownValue)
		{
			Config.ServerType   = dropdownValue;
			_serverAddress.text = Config.ConfigServer?.Address;
			_serverPort.text    = Config.ConfigServer?.Port.ToString();
			_targetDomain.text  = Config.Config.WorldAuth;

			ChatManager.Instance.SetChatUrl(Config.Config?.WorldChat);
			WebApi.Service.Api.ApiUrl = Configurator.Instance.Config?.OfficeServerAddress;
		}

		public async void ForceRefreshServerListFromArgo()
		{
			_refreshButton.interactable = false;
			var success = await LoginServerList.RefreshServerListAsync(status => { _argoStatus.text = status; });
			_argoStatus.text = string.Empty;
			_refreshButton.interactable = true;
			if (success)
				RefreshServerListFromArgo();
		}

		public void LoadServerListFromFile()
		{
			if (LoginServerList.IsSaveExist())
				LoginServerList.Load(success => RefreshServerListFromArgo());
		}

		public void PrintServerList()
		{
			LoginServerList.PrintServerInfoAsync(status => _argoStatus.text = status).Forget();
		}

		private void RefreshServerListFromArgo()
		{
			Config.ServerConfigurations = new ServerConfigurations
			{
				DevConfigurations            = LoginServerList.GetDevConfigurations(),
				DevIntegrationConfigurations = LoginServerList.GetDevIntegrationConfigurations(),
				ProductionConfigurations     = LoginServerList.GetProductionConfigurations(),
				StagingConfigurations        = LoginServerList.GetStagingConfigurations(),
				QaConfigurations             = LoginServerList.GetQaConfigurations(),
			};
			Configurator.PrintServerConfigurations(Config.ServerConfigurations, "FromArgo");
			RefreshUI();
		}

		private void OnApplicationQuit()
		{
			LoginServerList.Cancel();
		}

		public void LoadConfigJson(bool refreshUi = false)
		{
			var success = Config.LoadConfigJson();
			if (success)
				_argoStatus.text = "Config.json 로드 성공";
			else
				_argoStatus.text = "Config.json 로드 실패";
			if (refreshUi)
				RefreshUI();
		}
	}
}
