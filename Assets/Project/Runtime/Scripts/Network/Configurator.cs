/*===============================================================
* Product:    Com2Verse
* File Name:  Configurator.cs
* Developer:  haminjeong
* Date:       2022-05-09 14:38
* History:    
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#define SERVER_CONFIG_LOG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Com2Verse.BuildHelper;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Cache = Com2Verse.LocalCache.Cache;

namespace Com2Verse.Network
{
	[Serializable]
	public sealed class ServerList
	{
		public string Name;
		public string Address;
		public int    Port;
	}
 
	[Serializable]
	public sealed class Configuration
	{
		public string       WorldAuth;
		public string       WorldChat;
		public string       OfficeServerAddress;
		public string       MiceServerAddress;
		public ServerList[] ServerLists;
	}

	[Serializable]
	public sealed class ServerConfigurations
	{
		public Configuration[] ProductionConfigurations;
		public Configuration[] StagingConfigurations;
		public Configuration[] QaConfigurations;
		public Configuration[] DevIntegrationConfigurations;
		public Configuration[] DevConfigurations;
	}

	public sealed class Configurator : MonoSingleton<Configurator>
	{
		// Property
		private int _serverType;
		private string _configFile = "Config.json";
		private string _serverTypeKey = "Configurator_ServerPanelIndex";
#if UNITY_EDITOR
		private string _configJsonUrl = "http://10.36.0.87:8081/config.json";
#endif // UNITY_EDITOR
		private ServerConfigurations _serverConfigurations;

		//fixme: class name 과 get/setter 함수가 같음.
		public ServerConfigurations ServerConfigurations
		{
			get => _serverConfigurations;
			set => _serverConfigurations = value;
		}

		public event Action OnNeedRefresh = () => { };

		public int ServerType
		{
			get
			{
				switch (AppInfo.Instance.Data.Environment)
				{
					case eBuildEnv.DEV:
						_serverType = PlayerPrefs.GetInt(_serverTypeKey, 0);
						return _serverType;
					default:
						return 0;
				}
			}
			set
			{
				switch (AppInfo.Instance.Data.Environment)
				{
					case eBuildEnv.DEV:
						_serverType = value;
						PlayerPrefs.SetInt(_serverTypeKey, _serverType);
						break;
					default:
						_serverType = 0;
						break;
				}
			}
		}

		public Configuration Config
		{
			get
			{
				switch (AppInfo.Instance.Data.Environment)
				{
					case eBuildEnv.QA:
						return _serverConfigurations?.QaConfigurations?[0];
					case eBuildEnv.STAGING:
						return _serverConfigurations?.StagingConfigurations?[0];
					case eBuildEnv.PRODUCTION:
						return _serverConfigurations?.ProductionConfigurations?[0];
					case eBuildEnv.DEV_INTEGRATION:
						return _serverConfigurations?.DevIntegrationConfigurations?[0];
					default:
					case eBuildEnv.DEV:
						return _serverConfigurations?.DevConfigurations?[0];
				}
			}
		}

		public ServerList ConfigServer
		{
			get
			{
				switch (AppInfo.Instance.Data.Environment)
				{
					case eBuildEnv.QA:
						return _serverConfigurations?.QaConfigurations?[0]?.ServerLists?[0];
					case eBuildEnv.STAGING:
						return _serverConfigurations?.StagingConfigurations?[0]?.ServerLists?[0];
					case eBuildEnv.PRODUCTION:
						return _serverConfigurations?.ProductionConfigurations?[0]?.ServerLists?[0];
					case eBuildEnv.DEV_INTEGRATION:
						return _serverConfigurations?.DevIntegrationConfigurations?[0]?.ServerLists?[0];
					default:
					case eBuildEnv.DEV:
						var index = (ServerType < _serverConfigurations?.DevConfigurations?[0]?.ServerLists?.Length) ? ServerType : 0;
						return _serverConfigurations?.DevConfigurations?[0]?.ServerLists?[index];
				}
			}
		}

		// Init
		protected override void AwakeInvoked()
		{
			LoadConfigJson();
		}

		public bool LoadConfigJson()
		{
			C2VDebug.Log("Starting configurator");
			try
			{
				var configFile = File.ReadAllText(DirectoryUtil.GetStreamingAssetPath(_configFile));
				ServerConfigLog($"configFile : {configFile}");
				ServerConfigurations = JsonConvert.DeserializeObject<ServerConfigurations>(configFile);
				PrintServerConfigurations(ServerConfigurations, "LoadConfigJson");
				return true;
			}
			catch (Exception e)
			{
				C2VDebug.LogError(e.Message);
			}

			return false;
		}

		public async UniTask LoadFileServerConfigAsync()
		{
#if UNITY_EDITOR
			try
			{
				Cache.PurgeCache(_configFile);
				Cache.PurgeTemp(_configFile);
				using var pool = (await Cache.LoadBytesAsync(_configFile, _configJsonUrl));
				var json = System.Text.Encoding.UTF8.GetString(pool.Data);
				ServerConfigLog($"json : {json}");
				ServerConfigurations = JsonConvert.DeserializeObject<ServerConfigurations>(json);
				PrintServerConfigurations(ServerConfigurations, "LoadFileServerConfigAsync");
				OnNeedRefresh?.Invoke();
			}
			catch (Exception e)
			{
				C2VDebug.LogError(e);
			}
#endif // UNITY_EDITOR
		}

		public void HardCodingForStaging()
		{
			//fixme: hard coding...
			//TODO: remove.
			ServerConfigLog("HardCodingForStaging called");
			ServerConfigurations = new ServerConfigurations();
			var config = new Configuration();
			// config.ConfigurationName = "STAGING";
			// config.ServerAddress     = "34.64.227.101";
			// config.ServerPort        = 11111;
			// config.TargetAuth        = "https://staging-auth.com2verse.com";
			var stageConfigurations = new List<Configuration>();
			stageConfigurations.Add(config);
			ServerConfigurations.StagingConfigurations = stageConfigurations.ToArray();
		}

#region Util
		[Conditional("SERVER_CONFIG_LOG"), Conditional("SONG_TEST")]
		public static void ServerConfigLog(string msg)
		{
			C2VDebug.Log("[ServerConfig] " + msg);
		}

		[Conditional("UNITY_EDITOR"), Conditional("SONG_TEST")]
		public static void PrintServerConfigurations(ServerConfigurations config, string addName = "")
		{
			if (config == null) return;
			var log = $"@ PrintServerConfigurations  - {addName}\n\n";
			AddLogs(config.DevConfigurations,            "Dev");
			AddLogs(config.DevIntegrationConfigurations, "DevIntegration");
			AddLogs(config.QaConfigurations,             "Qa");
			AddLogs(config.StagingConfigurations,        "Staging");
			AddLogs(config.ProductionConfigurations,     "Production");
			void AddLogs(Configuration[] list, string type = "")
			{
				if (list != null && 0 < list.Length)
				{
					log += $" + {type} :\n";
					foreach (var item in list)
					{
						log += $" ---------- \n";
						log += $"  - TargetLoginAuth : {item.WorldAuth}\n";
						log += $"  - TargetLoginAuth : {item.WorldChat}\n";
						log += $"  - TargetLoginAuth : {item.OfficeServerAddress}\n";
						log += $"  - MiceServerAddress : {item.MiceServerAddress}\n";
						log += $"  - ServerList : [";
						foreach (var server in item.ServerLists)
						{
							log += "        {\n";
							log += $"          - Name : {server.Name}\n";
							log += $"          - Address : {server.Address}\n";
							log += $"          - Port : {server.Port}\n";
							log += "        }\n";
						}
						log += "     ]\n";
						log += " ---------- \n";
					}
					log += "\n";
				}
			}
			ServerConfigLog($"{log}");
		}

		public static void PrintCurrentConfigInfo()
		{
			//TODO: Mr.Song - log 중복 코드 추후에 하나로.
			var log = $"@ PrintCurrentConfigInfo\n\n";
			var item = Instance.Config;
			log += $" ---------- \n";
			log += $"  - TargetLoginAuth : {item.WorldAuth}\n";
			log += $"  - TargetLoginAuth : {item.WorldChat}\n";
			log += $"  - TargetLoginAuth : {item.OfficeServerAddress}\n";
			log += $"  - MiceServerAddress : {item.MiceServerAddress}\n";
			log += $"  - ServerList : [";
			foreach (var server in item.ServerLists)
			{
				log += "        {\n";
				log += $"          - Name : {server.Name}\n";
				log += $"          - Address : {server.Address}\n";
				log += $"          - Port : {server.Port}\n";
				log += "        }\n";
			}
			log += "     ]\n";
			log += " ---------- \n";
			ServerConfigLog($"{log}");
		}
#endregion	// Util
	}
}
