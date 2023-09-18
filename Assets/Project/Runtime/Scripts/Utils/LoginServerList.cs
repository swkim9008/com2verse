/*===============================================================
* Product:		Com2Verse
* File Name:	LoginServerList.cs
* Developer:	jhkim
* Date:			2022-10-24 12:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#define SERVER_CONFIG_LOG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using Com2Verse.Logger;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Com2Verse.Utils
{
	public static class LoginServerList
	{
		private static readonly string ServerNameDev            = "dev";
		private static readonly string ServerNameStaging        = "staging";
		private static readonly string ServerNameQa             = "qa";
		private static readonly string ServerNameDevIntegration = "devintegration";
		// private static readonly string ServerPrefixFeature = "feature-";
		// private static readonly string ServerPrefixDummy = "dummytest";
		private static readonly int PortNo = 11111;
		private static readonly string DomainAuthUrl = "https://public-dev-auth.com2verse.com/api/domain/validate";
		private static readonly string LoginAuthUrl = "https://public-dev-auth.com2verse.com/signin?idpType=";
		private static readonly string StagingDomainAuthUrl = "https://staging-auth.com2verse.com/api/domain/validate";
		private static readonly string StagingLoginAuthUrl = "https://staging-auth.com2verse.com/signin?idpType=";

		private static readonly string SaveKey = "LoginServerList";

		private static HttpClient _client;
		private static readonly Dictionary<string, ServerMapInfo> _serverInfoMap = new();
		private static ServerList _tmpServerList = new();

		public static IReadOnlyDictionary<string, ServerMapInfo> ServerInfoMap => _serverInfoMap;
		private static CancellationTokenSource _tcs;
#region Server Info
		
		//TODO: 분리 작업 or jenkins 로.
		// staging
		private static ArgoInfo _publicServer = new ArgoInfo
		{
			ServerType = eServerType.PUBLIC,
			Address = "http://34.64.178.182",		// dev
			// Address = "http://34.64.39.53",		// staging
			AuthInfo = new AuthInfo
			{
				username = "admin",
				password = "Z6dr4082fUrkWzEE",		// dev
				// password = "wghihthb56FBOIzp",	// staging
			},
			Prefix = "c2v-server-public-",
		};

		private static ArgoInfo _privateServer = new ArgoInfo
		{
			ServerType = eServerType.PRIVATE,
			Address = "http://34.64.39.161",		// dev
			// Address = "http://34.64.87.107",		// staging
			AuthInfo = new AuthInfo
			{
				username = "admin",
				password = "GsFd-yuHFH5ypUZf",		// dev
				// password = "uBw5PxNuKsLUeTCR",	// staging
			},
			Prefix = "c2v-server-private-",
		};
#endregion // Server Info

#region Public Functions
		public static Configuration[] GetProductionConfigurations()
		{
			Configurator.ServerConfigLog("GetProductionConfigurations called");
			var result = new List<Configuration>();
			if (ServerInfoMap.ContainsKey(ServerNameDev))
			{
				var info = ServerInfoMap[ServerNameDev];
				result.Add(CreateConfiguration(info));
			}
			return result.ToArray();
		}

		public static Configuration[] GetStagingConfigurations()
		{
			Configurator.ServerConfigLog("GetStagingConfigurations called");
			var result = new List<Configuration>();
			if (ServerInfoMap.ContainsKey(ServerNameStaging))
			{
				var info = ServerInfoMap[ServerNameStaging];
				result.Add(CreateConfiguration(info));
			}
			return result.ToArray();
		}

		public static Configuration[] GetQaConfigurations()
		{
			Configurator.ServerConfigLog("GetQaConfigurations called");
			var result = new List<Configuration>();
			if (ServerInfoMap.ContainsKey(ServerNameQa))
			{
				var info = ServerInfoMap[ServerNameQa];
				result.Add(CreateConfiguration(info));
			}
			return result.ToArray();
		}

		public static Configuration[] GetDevIntegrationConfigurations()
		{
			Configurator.ServerConfigLog("GetDevIntegrationConfigurations called");
			var result = new List<Configuration>();
			if (ServerInfoMap.ContainsKey(ServerNameDevIntegration))
			{
				var info = ServerInfoMap[ServerNameDevIntegration];
				result.Add(CreateConfiguration(info));
			}

			return result.ToArray();
		}

		public static Configuration[] GetDevConfigurations()
		{
			Configurator.ServerConfigLog("GetDevConfigurations called");
			var result = new List<Configuration>();
			// result.AddRange(GetReleaseConfigurations());

			var featureInfos = ServerInfoMap.Values.Where(info => IsValidServerName(info.Name)).ToArray();
			foreach (var info in featureInfos)
				result.Add(CreateConfiguration(info));
			return result.ToArray();

			bool IsValidServerName(string name) => true; // name.StartsWith(ServerPrefixFeature) || name.StartsWith(ServerPrefixDummy);
		}

		public static async UniTask<bool> RefreshServerListAsync(Action<string> onStatusUpdateCallback = null)
		{
			try
			{
				_tcs = new CancellationTokenSource();
				_tmpServerList.Infos.Clear();
				_tmpServerList.Infos.AddRange((await LoadArgoServerAsync(_publicServer, onStatusUpdateCallback).AttachExternalCancellation(_tcs.Token)).Infos);
				_tmpServerList.Infos.AddRange((await LoadArgoServerAsync(_privateServer, onStatusUpdateCallback).AttachExternalCancellation(_tcs.Token)).Infos);

				if (_tcs.IsCancellationRequested)
				{
					_tmpServerList.Infos.Clear();
					return false;
				}

				CreateServerListMap(_tmpServerList);
				Save();
				_tmpServerList.Infos.Clear();
				return true;
			}
			catch (Exception e)
			{
				C2VDebug.LogWarning($"LoginServerList Failed...\n{e}");
				return false;
			}
		}

		public static void Cancel()
		{
			if (_tcs != null)
				_tcs.Cancel();
		}

		public static async UniTaskVoid PrintServerInfoAsync(Action<string> onStatusUpdateCallback = null)
		{
			StringBuilder sb = new StringBuilder();
			await PrintServerListAsync(_publicServer);
			await PrintServerListAsync(_privateServer);
			onStatusUpdateCallback?.Invoke(string.Empty);
			async UniTask PrintServerListAsync(ArgoInfo argoInfo)
			{
				sb.Clear();
				onStatusUpdateCallback?.Invoke(LogStr(argoInfo, "서버 목록 요청"));
				var client = await GetAuthorizedClient(argoInfo);
				var list = await GetListResponseAsync(client, argoInfo);
				sb.AppendLine($"Load ServerList = {argoInfo.ServerType}");
				for (var i = 0; i < list.Length; i++)
					sb.AppendLine($" [{i}] = {list[i]}");

				C2VDebug.Log(sb.ToString());
			}
		}
#endregion // Public Functions

#region Get ServerList from Argo
		private static async UniTask<ServerList> LoadArgoServerAsync(ArgoInfo argoInfo, Action<string> onStatusUpdateCallback = null)
		{
			onStatusUpdateCallback?.Invoke(LogStr(argoInfo, "서버 목록 요청"));
			var client = await GetAuthorizedClient(argoInfo, onStatusUpdateCallback);
			var list = await GetListResponseAsync(client, argoInfo, onStatusUpdateCallback);
			var serverList = await GetServerInfoAsync(argoInfo, client, list, onStatusUpdateCallback);
			return serverList;
		}
		private static async UniTask<HttpClient> GetAuthorizedClient(ArgoInfo argoInfo, Action<string> onStatusUpdateCallback = null)
		{
			var client = GetClient();
			var response = await client.PostAsync(argoInfo.AuthUrl, new StringContent(argoInfo.AuthInfo.ToJson()));
			response.EnsureSuccessStatusCode();

			var data = await response.Content.ReadAsStringAsync();
			var authToken = JObject.Parse(data)["token"].Value<string>();
			var authorization = new AuthenticationHeaderValue("Bearer", authToken);
			client.DefaultRequestHeaders.Authorization = authorization;
			onStatusUpdateCallback?.Invoke(LogStr(argoInfo, "인증 완료"));
			return client;
		}
		private static async UniTask<string[]> GetListResponseAsync(HttpClient client, ArgoInfo argoInfo, Action<string> onStatusUpdateCallback = null)
		{
			var response = await client.GetAsync(argoInfo.ListUrl);
			response.EnsureSuccessStatusCode();

			var data = await response.Content.ReadAsStringAsync();
			var listResponse = JObject.Parse(data)["items"] as JArray;
			var results = new string[listResponse.Count];
			for (var i = 0; i < listResponse.Count; i++)
				results[i] = listResponse[i]["metadata"]["name"].Value<string>();
			onStatusUpdateCallback?.Invoke(LogStr(argoInfo, $"서버 목록 확인"));
			return results;
		}

		private static async UniTask<ServerList> GetServerInfoAsync(ArgoInfo argoInfo, HttpClient client, string[] listNames, Action<string> onStatusUpdateCallback = null)
		{
			var serverList = new ServerList();

			for (var i = 0; i < listNames.Length; i++)
			{
				var item = listNames[i];
				var url = argoInfo.GetIpAddrUrl(item);
				var response = await client.GetAsync(url);
				response.EnsureSuccessStatusCode();

				var data = await response.Content.ReadAsStringAsync();
				var jsonRoot = JObject.Parse(data);
				var jItems = jsonRoot["items"] as JArray;

				foreach (var jItem in jItems)
				{
					var name = jItem["name"].Value<string>();
					var serverName = argoInfo.GetServerName(name);
					var isValid = argoInfo.IsValidName(name);
					if (!isValid) continue;

					var log = LogStr(argoInfo, $"서버목록 조회\n{serverName}");
					onStatusUpdateCallback?.Invoke(log);
					try
					{
						var jLiveState = JObject.Parse(jItem["liveState"].Value<string>());
						var loadBalancers = jLiveState["status"]["loadBalancer"]["ingress"] as JArray;
						var ip = loadBalancers[0]["ip"];
						serverList.Infos.Add(new ServerInfo
						{
							Type = argoInfo.ServerType,
							Name = serverName,
							IP = ip.Value<string>(),
						});
					}
					catch (Exception) { }
				}
			}

			return serverList;
		}
		private static HttpClient GetClient()
		{
			if (_client != null) return _client;
			
			_client ??= new HttpClient(GetHandler(), disposeHandler: false);
			_client.DefaultRequestHeaders.UserAgent.TryParseAdd($"{Application.productName}/{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}");
			return _client;

			HttpClientHandler GetHandler()
			{
				var	currentHandler = new HttpClientHandler
				{
					AllowAutoRedirect = true,
					AutomaticDecompression = DecompressionMethods.None,
					Proxy = null,
					UseProxy = false,
					MaxAutomaticRedirections = 50,
				};
				return currentHandler;
			}
		}

		private static string LogStr(ArgoInfo info, string message) => $"[{info.ServerType}]\n{message}";
#endregion // Get ServerList from Argo

#region ServerList Map
		private static void CreateServerListMap(ServerList serverInfos)
		{
			_serverInfoMap.Clear();
			foreach (var server in serverInfos.Infos)
			{
				// C2VDebug.Log($"[ServerConfig] server.IP - {server.IP}");
				// C2VDebug.Log($"[ServerConfig] server.Name - {server.Name}");
				// C2VDebug.Log($"[ServerConfig] server.Type - {server.Type}");
				if (_serverInfoMap.ContainsKey(server.Name))
					UpdateItem(server);
				else
					AddNewItem(server);
			}

			void UpdateItem(ServerInfo server)
			{
				var serverInfo = _serverInfoMap[server.Name];
				switch (server.Type)
				{
					case eServerType.PUBLIC:
						serverInfo.PublicIP = server.IP;
						break;
					case eServerType.PRIVATE:
						serverInfo.PrivateIP = server.IP;
						break;
					default:
						break;
				}

				_serverInfoMap[server.Name] = serverInfo;
			}

			void AddNewItem(ServerInfo server)
			{
				var newInfo = new ServerMapInfo();
				newInfo.Name = server.Name;
				switch (server.Type)
				{
					case eServerType.PUBLIC:
						newInfo.PublicIP = server.IP;
						break;
					case eServerType.PRIVATE:
						newInfo.PrivateIP = server.IP;
						break;
					default:
						break;
				}

				_serverInfoMap.Add(server.Name, newInfo);
			}
		}
#endregion // ServerList Map

#region Local Save
		public static bool IsSaveExist() => LocalSave.Temp.IsExist(SaveKey);

		private static void Save()
		{
			LocalSave.Temp.SaveJson(SaveKey, _tmpServerList);
		}

		public static void Load(Action<bool> onLoaded)
		{
			var result = LocalSave.Temp.LoadJson<ServerList>(SaveKey);
			if (result != null)
				CreateServerListMap(result);
			onLoaded?.Invoke(result != null);
		}
#endregion // Local Save

#region Data
		public enum eServerType
		{
			PUBLIC,
			PRIVATE,
		}
		private struct ArgoInfo
		{
			public eServerType ServerType;
			public string Address;
			public AuthInfo AuthInfo;
			public string Prefix;
			public readonly static string Suffix = "-proxy";
			public string AuthUrl => $"{Address}/api/v1/session";
			public string ListUrl => $"{Address}/api/v1/applications";
			public string GetIpAddrUrl(string appName) => $"{ListUrl}/{appName}/managed-resources";
			public bool IsValidName(string name) => name.StartsWith(Prefix) && name.EndsWith(Suffix);
			public string GetServerName(string fullName) =>
				fullName.Replace(Prefix, string.Empty)
				        .Replace(Suffix, string.Empty)
				        .ToUpper();
		}

		[Serializable]
		private class AuthInfo
		{
			public string username;
			public string password;

			public string ToJson() => JsonConvert.SerializeObject(this);
		}

		public struct ServerMapInfo
		{
			public string Name;
			public string PublicIP;
			public string PrivateIP;
		}

		[Serializable]
		public class ServerInfo
		{
			public string Name;
			public eServerType Type;
			public string IP;
		}

		[Serializable]
		public class ServerList
		{
			public List<ServerInfo> Infos;

			public ServerList()
			{
				Infos = new();
			}
		}

		private static Configuration CreateConfiguration(ServerMapInfo info) => new Configuration
		{
			// ServerConfigLog("CreateConfiguration called");
			// FIXME : argoCD 를 통해 가져올때 이 분기가 올바르지 않음.
			WorldAuth = IsStagingServer(info.Name) ? StagingLoginAuthUrl : LoginAuthUrl,
			ServerLists = new[]
			{
				new Network.ServerList
				{
					Name    = info.Name.ToUpper(),
					Address = info.PublicIP,
					Port    = PortNo
				}
			}
		};

		private static bool IsStagingServer(string name) => name.ToLower().Equals(ServerNameStaging);
#endregion // Data
	}
}
