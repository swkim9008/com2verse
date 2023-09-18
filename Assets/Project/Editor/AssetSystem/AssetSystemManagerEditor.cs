/*===============================================================
* Product:		Com2Verse
* File Name:	AssetSystemManagerEditor.cs
* Developer:	masteage
* Date:			2022-05-25 13:32
* History:		
* Documents:	https://jira.com2us.com/wiki/display/C2U2VR/Asset+System
*				Addressables 1.19.19
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

// ADDRESSABLES_LOG_ALL

// #define ENABLE_LOG
// #define DEBUG
// #define UNITY_EDITOR
// #define DEVELOPMENT_BUILD
#define ASSET_SYSTEM_LOG
// #define DETAIL_ASSET_SYSTEM_LOG
// #define SONG_TEST
// #define CDN_TEST
#define GROUP_CREATE_LOGIC_V2
// #undef DETAIL_ASSET_SYSTEM_LOG
// #undef ASSET_SYSTEM_LOG

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Com2Verse.BuildHelper;
using Com2Verse.Logger;
using Com2Verse.Utils;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
// using UnityEditor.AddressableAssets.Build.Layout;
// using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;
#if SONG_TEST
using UnityEngine.AddressableAssets;
#endif // SONG_TEST
using UnityEngine.ResourceManagement.ResourceProviders;
using static UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema;
using static Com2Verse.AssetSystem.AssetSystemManager;
using Debug = UnityEngine.Debug;

namespace Com2VerseEditor.AssetSystem
{
	public class AddressableProfileEtc
	{
#if ENV_DEV
		public static string ENV_PATH = "Dev";
#elif ENV_STAGING
		public static string ENV_PATH = "Staging";
#elif ENV_PRODUCTION
		public static string ENV_PATH = "Production";
#else	// default
		public static string ENV_PATH = "Dev";
#endif	// ENV_DEV
	}
	
	public sealed class AssetSystemManagerEditor
	{
		private static AddressableAssetSettings _settings = AddressableAssetSettingsDefaultObject.Settings;
		private static AddressableAssetGroupTemplate _template = _settings.GetGroupTemplateObject(0) as AddressableAssetGroupTemplate;
		private static AddressableAssetProfileSettings _profileSettings = _settings.profileSettings;
		// private static AddressableAssetBuildSettings _buildSettings = _settings.buildSettings;
		private static Type[] _types = _template.GetTypes();

		//TODO: Mr.Song - 추후에 name 변경 가능성 고려.
		// not remove group
		private static readonly string BUILT_IN_DATA = "Built In Data";
		private static readonly string DEFAULT_LOCAL_GROUP = _settings.DefaultGroup.Name;	// "Default Local Group"
		
		// Asset Root Path Length
		private static readonly int ASSET_ROOT_PATH_LENGTH = DirectoryUtil.DataRoot.Length - "Assets".Length;
		private static readonly string ROOT_PATH = Path.GetFullPath("Assets/Project/Bundles");
		private static AssetRecreateInfo _assetRecreateInfo;
		
#region Menu
		[MenuItem("Com2Verse/AssetSystem/Obsolete/Info %#F1")]
		public static void Info()
		{
			AssetSystemLog("AssetSystemManagerEditor::Info called");
			ProfileInfo();
			DataBuilderInfo();
			GroupInfo();
			CheckAllFileExtension();
			CheckDuplicateFile(ROOT_PATH);
		}

		private static void CreateTableDataGroup()
		{
			var tableDataPackageName = "com.com2verse.tabledata";
			var tablePath = $"{AssetExternalDatabase.FindDirectory(Path.GetFullPath(Path.Combine(Application.dataPath!, @"../Library/PackageCache")), tableDataPackageName)}/Runtime/Data";

			var groupKey = "tabledatas";
			var label = $"{groupKey}_label";

			var group = _settings.FindGroup(groupKey);
			if (group == null)
			{
				group = _settings.CreateGroup(groupKey, false, false, false, null, typeof(BundledAssetGroupSchema));
			}

			_settings.AddLabel(label);

			var (guids, assetNames) = AssetExternalDatabase.FindAssets(tablePath);
			var addressableAssetEntries = new List<AddressableAssetEntry>();

			var i = 0;
			foreach (var guid in guids)
			{
				var entry = _settings.CreateOrMoveEntry(guid, group);
				entry.address = assetNames[i];
				entry.SetLabel(label, true);

				addressableAssetEntries.Add(entry);

				i++;
			}

			group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, addressableAssetEntries, false, true);
			_settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, addressableAssetEntries, true, false);

			EditorUtility.SetDirty(_settings);
		}


		[MenuItem("Com2Verse/AssetSystem/Obsolete/Group Recreate")]
		public static void RecreateGroup()
		{
			AssetSystemLog("AssetSystemManagerEditor::RecreateGroup called");
			if (!CheckDuplicateFile(ROOT_PATH))
			{
				AssetSystemLog("Has a Duplicate File !!!");
				return;
			}
			TimeCheckStart();

			// Remove Groups
			RemoveGroups(GetRemoveExcludeGroupList(ROOT_PATH));
			
			//TODO: Mr.Song - labels remove
			// - 전체 label 관리. (제거된 경우에 대한 처리 필요.)
			// _settings.GetLabels()
			
			//Create Groups
			CreateGroups(ROOT_PATH);        

			// Create TableData Group
			CreateTableDataGroup();
			
			// default group
			var label = "default";
			var defaultGroup = _settings.FindGroup(DEFAULT_LOCAL_GROUP);
			if (defaultGroup)
			{
				foreach (var entry in defaultGroup.entries)
				{
					entry.labels.Clear();
					entry?.SetLabel(label, true);
				}
			}
			
			// save
			AssetDatabase.SaveAssets();
			EditorUtility.ClearProgressBar();
			TimeCheckEnd();
			
			
		}
		
		[MenuItem("Com2Verse/AssetSystem/Obsolete/LocalBuild")]
		public static bool LocalBuild()
		{
			// Clean();
			return Build(eAssetBuildType.LOCAL);
		}
		
		[MenuItem("Com2Verse/AssetSystem/Obsolete/RemoteBuild %#F4")]
		public static bool RemoteBuild()
		{
			// Clean();
			return Build(eAssetBuildType.REMOTE);
		}
		
		[MenuItem("Com2Verse/AssetSystem/Obsolete/RemoteTestBuild %#F5")]
		public static bool RemoteTestBuild()
		{
			// Clean();
			return Build(eAssetBuildType.REMOTE_TEST);
		}
		
		[MenuItem("Com2Verse/AssetSystem/Obsolete/EditorHostedBuild %#F6")]
		public static bool EditorHostedBuild()
		{
			// Clean();
			return Build(eAssetBuildType.EDITOR_HOSTED);
		}
		
		[MenuItem("Com2Verse/AssetSystem/Obsolete/LocalBuildSetting %#F7")]
		public static void LocalBuildSetting()
		{
			BuildSetting(eAssetBuildType.LOCAL, true);
		}
		
		[MenuItem("Com2Verse/AssetSystem/Obsolete/RemoteBuildSetting %#F8")]
		public static void RemoteBuildSetting()
		{
			BuildSetting(eAssetBuildType.REMOTE, true);
		}
		
		[MenuItem("Com2Verse/AssetSystem/Obsolete/RemoteTestBuildSetting %#F9")]
		public static void RemoteTestBuildSetting()
		{
			BuildSetting(eAssetBuildType.REMOTE_TEST, true);
		}
		
		[MenuItem("Com2Verse/AssetSystem/Obsolete/EditorHostedBuildSetting %#F10")]
		public static void EditorHostedBuildSetting()
		{
			BuildSetting(eAssetBuildType.EDITOR_HOSTED, true);
		}
		
		public static bool CleanBuild()
		{
			AssetSystemLog("AssetSystemManagerEditor::CleanBuild called");
			Clean();
			return Build();
		}
		
		public static bool Build()
		{
			// 40.42 sec (22/06/15)
			AssetSystemLog("AssetSystemManagerEditor::Build called");
			TimeCheckStart();
			
			//TODO: Mr.Song - contents 지정 기능 확인 테스트
			// AddressableAssetSettings.PlayerBuildOption
			// BuildPlayerOptions options;
			// BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
			// BuildPlayerWindow.RegisterBuildPlayerHandler(BuildPlayerHandler);
			
			AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult buildResult);
			DetailAssetSystemLog($"ActivePlayerDataBuilder : {_settings.ActivePlayerDataBuilder}");
			DetailAssetSystemLog($"ActivePlayModeDataBuilder : {_settings.ActivePlayModeDataBuilder}");
			if (buildResult != null)
			{
				if (string.IsNullOrEmpty(buildResult.Error))
				{
					AssetSystemLog("Build Success");
					DetailAssetSystemLog($"Duration : {buildResult.Duration}");
					DetailAssetSystemLog($"FileRegistry : {buildResult.FileRegistry}");
					DetailAssetSystemLog($"LocationCount : {buildResult.LocationCount}");
					DetailAssetSystemLog($"OutputPath : {buildResult.OutputPath}");
					TimeCheckEnd();
					return true;
				}
				else
				{
					AssetSystemLog("Build Fail");
					DetailAssetSystemLog($"Error : {buildResult.Error}");
				}	
			}
			TimeCheckEnd();
			return false;
		}
		
#region Test
		
#if SONG_TEST
		static public int testIndex = 0;
		[MenuItem("Com2Verse/AssetSystem/Test ASM_Editor %#F7")]
		public static void TestAll()
		{
			eAssetBuildType type = eAssetBuildType.REMOTE_TEST;
			ChangeSetting(type);
			ChangeGroups(type);

			// LocalBuild();
			// RemoteBuild();
			// RemoteTestBuild();
			// EditorHostedBuild();

			// TestAssetPathKeyword();
			// TestAssetPath();
			// TestAssetPathEtc();
			// TestDataBuilder();
			// TestBuild();
			// TestCdnUrlChange();
		}
		
		private static void TestAssetPathKeyword()
		{
			AssetSystemLog($"GetBuildPathName(eAssetBuildType.LOCAL) : {GetBuildPathName(eAssetBuildType.LOCAL)}");
			AssetSystemLog($"GetBuildPathName(eAssetBuildType.REMOTE) : {GetBuildPathName(eAssetBuildType.REMOTE)}");
			AssetSystemLog($"GetBuildPathName(eAssetBuildType.REMOTE_TEST) : {GetBuildPathName(eAssetBuildType.REMOTE_TEST)}");
			AssetSystemLog($"GetBuildPathName(eAssetBuildType.EDITOR_HOSTED) : {GetBuildPathName(eAssetBuildType.EDITOR_HOSTED)}");
			AssetSystemLog($"GetLoadPathName(eAssetBuildType.LOCAL) : {GetLoadPathName(eAssetBuildType.LOCAL)}");
			AssetSystemLog($"GetLoadPathName(eAssetBuildType.REMOTE) : {GetLoadPathName(eAssetBuildType.REMOTE)}");
			AssetSystemLog($"GetLoadPathName(eAssetBuildType.REMOTE_TEST) : {GetLoadPathName(eAssetBuildType.REMOTE_TEST)}");
			AssetSystemLog($"GetLoadPathName(eAssetBuildType.EDITOR_HOSTED) : {GetLoadPathName(eAssetBuildType.EDITOR_HOSTED)}");
			AssetSystemLog($"GetContentStateBuildPathName(eAssetBuildType.LOCAL) : {GetContentStateBuildPathName(eAssetBuildType.LOCAL)}");
			AssetSystemLog($"GetContentStateBuildPathName(eAssetBuildType.REMOTE) : {GetContentStateBuildPathName(eAssetBuildType.REMOTE)}");
			AssetSystemLog($"GetContentStateBuildPathName(eAssetBuildType.REMOTE_TEST) : {GetContentStateBuildPathName(eAssetBuildType.REMOTE_TEST)}");
			AssetSystemLog($"GetContentStateBuildPathName(eAssetBuildType.EDITOR_HOSTED) : {GetContentStateBuildPathName(eAssetBuildType.EDITOR_HOSTED)}");
			AssetSystemLog($"GetContentStateBuildPath(eAssetBuildType.LOCAL) : {GetContentStateBuildPath(eAssetBuildType.LOCAL)}");
			AssetSystemLog($"GetContentStateBuildPath(eAssetBuildType.REMOTE) : {GetContentStateBuildPath(eAssetBuildType.REMOTE)}");
			AssetSystemLog($"GetContentStateBuildPath(eAssetBuildType.REMOTE_TEST) : {GetContentStateBuildPath(eAssetBuildType.REMOTE_TEST)}");
			AssetSystemLog($"GetContentStateBuildPath(eAssetBuildType.EDITOR_HOSTED) : {GetContentStateBuildPath(eAssetBuildType.EDITOR_HOSTED)}");
			AssetSystemLog($"GetContentStateBuildFullPath(eAssetBuildType.LOCAL) : {GetContentStateBuildFullPath(eAssetBuildType.LOCAL)}");
			AssetSystemLog($"GetContentStateBuildFullPath(eAssetBuildType.REMOTE) : {GetContentStateBuildFullPath(eAssetBuildType.REMOTE)}");
			AssetSystemLog($"GetContentStateBuildFullPath(eAssetBuildType.REMOTE_TEST) : {GetContentStateBuildFullPath(eAssetBuildType.REMOTE_TEST)}");
			AssetSystemLog($"GetContentStateBuildFullPath(eAssetBuildType.EDITOR_HOSTED) : {GetContentStateBuildFullPath(eAssetBuildType.EDITOR_HOSTED)}");
		}
		
		private static void TestAssetPath()
		{
			foreach (eAssetBuildType type in Enum.GetValues(typeof(eAssetBuildType)))
			{
				ChangeSetting(type);
				// AssetSystemLog($"Build Path : {_settings.profileSettings.GetValueByName(_settings.activeProfileId,  GetBuildPathName(type))}");
				// AssetSystemLog($"Load Path : {_settings.profileSettings.GetValueByName(_settings.activeProfileId,  GetLoadPathName(type))}");
			}
		}
		
		private static void TestAssetPathEtc()
		{
			// etc
			// Default
			var appVersion = Application.version;
			AssetSystemLog($"AppVersion : {appVersion}");
			var buildTarget = EditorUserBuildSettings.activeBuildTarget;
			AssetSystemLog($"BuildTarget : {buildTarget}");
			// - Library/com.unity.addressables/aa/Windows
			AssetSystemLog($"Addressables.BuildPath : {Addressables.BuildPath}"); 
			AssetSystemLog($"Local.BuildPath : {Addressables.BuildPath}/{buildTarget}");
			AssetSystemLog($"Local.LoadPath : {Addressables.RuntimePath}/{buildTarget}");
			AssetSystemLog($"Remote.BuildPath : ServerData/{appVersion}/Remote/{buildTarget}");
			AssetSystemLog($"Remote.LoadPath : http://[PrivateIpAddress]:[HostingServicePort]");
			
			// Addressables
			// - AddressablesRuntimeBuildLog
			AssetSystemLog($"Addressables.kAddressablesRuntimeBuildLogPath : {Addressables.kAddressablesRuntimeBuildLogPath}"); 
			// - AddressablesRuntimeDataPath
			AssetSystemLog($"Addressables.kAddressablesRuntimeDataPath : {Addressables.kAddressablesRuntimeDataPath}");
			// - C:/Users/user/Desktop/Com2Verse/Projects/C2VClient_Sample/Assets/StreamingAssets/aa
			AssetSystemLog($"Addressables.PlayerBuildDataPath : {Addressables.PlayerBuildDataPath}");
			// - Library/com.unity.addressables/aa/Windows
			AssetSystemLog($"Addressables.RuntimePath : {Addressables.RuntimePath}");
			// - Library/com.unity.addressables/
			AssetSystemLog($"Addressables.LibraryPath : {Addressables.LibraryPath}");
			
			// - C:\Users\user\Desktop\Com2Verse\Projects\C2VClient_Sample
			AssetSystemLog($"CurrentDirectory - {Directory.GetCurrentDirectory()}");
			// - C:/Users/user/Desktop/Com2Verse/Projects/C2VClient_Sample/Assets
			AssetSystemLog($"Application.dataPath - {Application.dataPath}");
			// - C:\Users\user\Desktop\Com2Verse\Projects\C2VClient_Sample\Assets\Project\Bundles
			AssetSystemLog($"bundles - {ROOT_PATH}");
			
			eAssetBuildType type = eAssetBuildType.REMOTE_TEST;
			// - Windows
			AssetSystemLog($"platformPathSubFolder - {PlatformMappingService.GetPlatformPathSubFolder()}");
			// - AddressableContents/1.0.0/RemoteTest
			AssetSystemLog($"contentStateBuildPathName - {GetContentStateBuildPathName(type)}");
			AssetSystemLog($"contentStateBuildPath - {GetContentStateBuildPath(type)}");
			
			// - C:\Users\user\Desktop\Com2Verse\Projects\C2VClient_Sample\AddressableContents/1.0.0/RemoteTest\Windows
			// var test01 = Path.Combine(Directory.GetCurrentDirectory(), GetContentStateBuildPathName(type), PlatformMappingService.GetPlatformPathSubFolder());
			var test01 = Path.Combine(Directory.GetCurrentDirectory(), GetContentStateBuildPath(type));
			AssetSystemLog($"test_01 - {test01}");
			if (Directory.Exists(test01))
			{
				AssetSystemLog($"test01 - directory is exists");
				Directory.Delete(test01,true);
			}
			
			// - C:/Users/user/Desktop/Com2Verse/Projects/C2VClient_Sample/Assets\../AddressableContents/1.0.0/RemoteTest\Windows
			// var test02 = Path.Combine(Application.dataPath, "../", GetContentStateBuildPathName(type), PlatformMappingService.GetPlatformPathSubFolder());
			var test02 = Path.Combine(Application.dataPath, "../", GetContentStateBuildPath(type));
			AssetSystemLog($"test02 - {test02}");
			if (Directory.Exists(test02))
			{
				AssetSystemLog($"test02 - directory is exists");
				Directory.Delete(test02,true);
			}
		}
		
		private static void TestDataBuilder()
		{
			DataBuilderInfo();
			ChangeActivePlayerDataBuilder(eBuildScriptMode.BuildScriptPackedPlayMode);
			ChangeActivePlayModeDataBuilder(eBuildScriptMode.BuildScriptPackedPlayMode);
			DataBuilderInfo();
		}
		
		private static void TestBuild()
		{
			// LocalBuild();
			// RemoteBuild();
			RemoteTestBuild();
			// EditorHostedBuild();
			
			// if(testIndex == 0) { ChangeSetting(eAssetBuildType.LOCAL); }
			// else if(testIndex == 1) { ChangeSetting(eAssetBuildType.REMOTE); }
			// else if(testIndex == 2)  { ChangeSetting(eAssetBuildType.REMOTE_TEST); }
			// else if(testIndex == 3)  { ChangeSetting(eAssetBuildType.EDITOR_HOSTED); }
			// if(testIndex == 0) { LocalBuild(); }
			// else if(testIndex == 1) { RemoteBuild(); }
			// else if(testIndex == 2)  { RemoteTestBuild(); }
			// else if(testIndex == 3)  { EditorHostedBuild(); }
			// testIndex++;
			// if (3 < testIndex) { testIndex = 0; }
			
			// Clean();

			// ChangeGroups(eAssetBuildType.LOCAL);
			// ChangeGroups(eAssetBuildType.REMOTE);
			// ChangeGroups(eAssetBuildType.REMOTE_TEST);
			// ChangeGroups(eAssetBuildType.EDITOR_HOSTED);
			
			// ChangeActiveProfile("Default");
			// ChangeActiveProfile("default");
			// ChangeActiveProfile(eAssetBuildType.LOCAL.ToString());
			// ChangeActiveProfile(eAssetBuildType.REMOTE.ToString());
			// ChangeActiveProfile(eAssetBuildType.REMOTE_TEST.ToString());
			// ChangeActiveProfile(eAssetBuildType.EDITOR_HOSTED.ToString());

			// Build(eAssetBuildType.LOCAL);
			// Build(eAssetBuildType.REMOTE);
			// Build(eAssetBuildType.REMOTE_TEST);
			// Build(eAssetBuildType.EDITOR_HOSTED);

			// ChangeActivePlayerDataBuilder(eBuildScriptMode.BuildScriptVirtualMode);
			// ChangeActivePlayModeDataBuilder(eBuildScriptMode.BuildScriptVirtualMode);

			// clean
			// AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder;
			// AddressableAssetSettings.CleanPlayerContent();
			// AddressableAssetSettings.CleanPlayerContent(_settings.ActivePlayerDataBuilder);
			// AddressableAssetSettings.CleanPlayerContent(_settings.ActivePlayModeDataBuilder);
			// BuildCache.PurgeCache(false);
			
			// option
			// AddressableAssetProfileSettings profileSettings = _settings.profileSettings;
			// AssetSystemLog("AssetSystemManagerEditor::Build called");
			// AddressableAssetSettings.PlayerBuildOption aaa;
			// aaa = AddressableAssetSettings.PlayerBuildOption.PreferencesValue;
			// aaa = AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer;
			// aaa = AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;
			// AddressableAssetSettings.BuildPlayerContent();
			
			// asset delete
			// if (AssetDatabase.DeleteAsset("Assets/Project/Bundles/Com2us/Sound/SE/applause_01.wav"))
			// {
			// 	AssetSystemLog("AssetDatabase.DeleteAsset - Success");
			// }
			// else
			// {
			// 	AssetSystemLog("AssetDatabase.DeleteAsset - Fail");
			// }
		}

		private static void TestCdnUrlChange()
		{
			var oldLocation = "https://metaverse-platform-fn.qpyou.cn/metaverse-platform/Test/AssetBundles/1.1.0/catalog_metaverse_remote_test.json";
			var newLocation = oldLocation;
			if (oldLocation.StartsWith("http"))
			{
				var delimiter = Path.AltDirectorySeparatorChar;
				// var appVersion = Application.version;
				// var patchVersion = "2.1.3";
				// var hostPath = Path.GetDirectoryName(oldLocation);
				var frontPath = oldLocation.Substring(0, oldLocation.LastIndexOf(delimiter));
				var fileName = Path.GetFileName(oldLocation);
				var extension = Path.GetExtension(fileName);
				
				// catalog
				if (fileName.Contains("catalog_") && (extension.Equals(".json") || extension.Equals(".hash")))
				{
					// newLocation = frontPath + delimiter + patchVersion + delimiter + fileName;
					newLocation = frontPath + delimiter + fileName;
				}
			}
			AssetSystemLog($"newLocation : {newLocation}");
			
			// var oldLocation = "https://metaverse-platform-fn.qpyou.cn/metaverse-platform/Test/AssetBundles/1.1.0/catalog_metaverse_remote_test.hash";
			// // var oldLocation = "https://metaverse-platform-fn.qpyou.cn/metaverse-platform/AssetBundle/StandaloneWindows64/com2us_artasset_assets_all_cd21145184c33834a5463daa929ed6a9.bundle";
			// var newLocation = oldLocation;
			// if (oldLocation.StartsWith("http"))
			// {
			// 	var delimiter = Path.DirectorySeparatorChar;//Path.AltDirectorySeparatorChar;
			// 	var appVersion = Application.version;
			// 	var patchVersion = "2.1.3";
			// 	var dirName = Path.GetDirectoryName(oldLocation);
			// 	var fileName = Path.GetFileName(oldLocation);
			// 	var extension = Path.GetExtension(oldLocation);
			// 	
			// 	// catalog
			// 	if(fileName.Contains("catalog_") &&
			// 	   (extension.Equals(".json") || extension.Equals(".hash")))
			// 	{
			// 		// https:\metaverse-platform-fn.qpyou.cn\metaverse-platform\AssetBundles\StandaloneWindows64\0.1.0\2.1.3\catalog_2022.07.13.07.48.32.hash
			// 		newLocation = dirName + delimiter + appVersion + delimiter + patchVersion + delimiter + fileName;
			// 	}
			// 	else
			// 	{
			// 		// https:\metaverse-platform-fn.qpyou.cn\metaverse-platform\AssetBundle\StandaloneWindows64\0.1.0\com2us_artasset_assets_all_cd21145184c33834a5463daa929ed6a9.bundle
			// 		newLocation = dirName + delimiter + appVersion + delimiter + fileName;
			// 	}
			// }
			// AssetSystemLog($"newLocation : {newLocation}");
		}
#endif	// SONG_TEST
#endregion	// Test
#endregion	// Menu
		
#region Internal

#region Util
		[Conditional("ASSET_SYSTEM_LOG"), Conditional("SONG_TEST")]
		private static void AssetSystemLog(string msg)
		{
			C2VDebug.Log("[AssetSystemEditor] " + msg);
		}
		
		[Conditional("DETAIL_ASSET_SYSTEM_LOG"), Conditional("SONG_TEST")]
		private static void DetailAssetSystemLog(string msg)
		{
			C2VDebug.Log("[AssetSystemEditor] " + msg);
		}

		// private static readonly string SERVER_DATA_PATH = "ServerData";
		// private static readonly string CONTENTS_PATH = "AddressableContents";
		
		private static string GetBuildTypePath(eAssetBuildType type) =>
			type switch
			{
				eAssetBuildType.LOCAL => "Local",
				eAssetBuildType.REMOTE => "Remote",
				eAssetBuildType.REMOTE_TEST => "RemoteTest",
				eAssetBuildType.EDITOR_HOSTED => "EditorHosted",
				_ => "",
			};
		
		private static string GetBuildPathName(eAssetBuildType type) =>
			type switch
			{
				// "Local.BuildPath"
				// - ~/Library/com.unity.addressables/aa/Windows/StandaloneWindows64/
				// - [UnityEngine.AddressableAssets.Addressables.BuildPath]/[BuildTarget]
				eAssetBuildType.LOCAL => AddressableAssetSettings.kLocalBuildPath,
				// "Remote.BuildPath"
				// - ServerData/[AppVersion]/Remote/[BuildTarget]
				eAssetBuildType.REMOTE => AddressableAssetSettings.kRemoteBuildPath,
				// "RemoteTest.BuildPath"
				// - ServerData/[AppVersion]/RemoteTest/[BuildTarget]
				eAssetBuildType.REMOTE_TEST => $"{GetBuildTypePath(type)}.{AddressableAssetSettings.kBuildPath}",
				// "EditorHosted.BuildPath"
				// - ServerData/[AppVersion]/EditorHosted/[BuildTarget]
				eAssetBuildType.EDITOR_HOSTED => $"{GetBuildTypePath(type)}.{AddressableAssetSettings.kBuildPath}",
				// default
				_ => AddressableAssetSettings.kLocalBuildPath,
			};

		private static string GetLoadPathName(eAssetBuildType type) =>
			type switch
			{
				// "Local.LoadPath"
				// - {UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget]
				eAssetBuildType.LOCAL => AddressableAssetSettings.kLocalLoadPath,
				// "Remote.LoadPath"
				// - https://metaverse-platform-fn.qpyou.cn/metaverse-platform/AssetBundles/
				eAssetBuildType.REMOTE => AddressableAssetSettings.kRemoteLoadPath,
				// "RemoteTest.LoadPath"
				// - https://metaverse-platform-fn.qpyou.cn/metaverse-platform/Test/AssetBundles/1.0.7
				eAssetBuildType.REMOTE_TEST => $"{GetBuildTypePath(type)}.{AddressableAssetSettings.kLoadPath}",
				// "EditorHosted.LoadPath"
				// - http://[PrivateIpAddress]:[HostingServicePort]
				eAssetBuildType.EDITOR_HOSTED => $"{GetBuildTypePath(type)}.{AddressableAssetSettings.kLoadPath}",
				// default
				_ => AddressableAssetSettings.kLocalLoadPath,
			};
		
		private static string GetContentStateBuildPathName(eAssetBuildType type) =>
			type switch
			{
				// ex) ~/AddressableContents/0.1.0/Local/addressables_content_state.bin
				eAssetBuildType.LOCAL => "LocalContentState",
				eAssetBuildType.REMOTE => "RemoteContentState",
				eAssetBuildType.REMOTE_TEST => "RemoteTestContentState",
				eAssetBuildType.EDITOR_HOSTED => "EditorHostedContentState",
				// default
				_ => "LocalContentState",
			};

		private static string GetContentStateBuildPath(eAssetBuildType type)
		{
			return _settings.profileSettings.GetValueByName(_settings.activeProfileId, GetContentStateBuildPathName(type));
		}
		
		// old version.
		// private static string GetContentStateBuildPath(eAssetBuildType type) =>
		// 	type switch
		// 	{
		// 		// ""
		// 		// -- ex) ~/Assets/Project/AddressableAssetsData/Windows/addressables_content_state.bin
		// 		// $"{CONTENTS_PATH}/{Application.version}/{GetBuildTypePath(type)}"
		// 		// -- ex) ~/AddressableContents/0.1.0/Remote/Windows
		// 		// eAssetBuildType.LOCAL => "",
		// 		// eAssetBuildType.REMOTE => $"{CONTENTS_PATH}/{Application.version}/{GetBuildTypePath(type)}",
		// 		// eAssetBuildType.REMOTE_TEST => $"{CONTENTS_PATH}/{Application.version}/{GetBuildTypePath(type)}",
		// 		// eAssetBuildType.EDITOR_HOSTED => $"{CONTENTS_PATH}/{Application.version}/{GetBuildTypePath(type)}",
		// 		
		// 		// - ex) ~/AddressableContents/0.1.0/Remote/Windows
		// 		_ => $"{CONTENTS_PATH}/{Application.version}/{GetBuildTypePath(type)}"
		// 	};
		
		//TODO: Mr.Song - 해당 folder 경로 gitignore 여부 고려.
		private static string GetContentStateBuildFullPath(eAssetBuildType type) =>
			type switch
			{
				// addressables_content_state.bin
				// ex) C:\Users\user\Desktop\Com2Verse\Projects\C2VClient_Sample\AddressableContents\1.0.0\RemoteTest\~~
				_ => Path.Combine(Directory.GetCurrentDirectory(), GetContentStateBuildPath(type)),
				// ex) C:\Users\user\Desktop\Com2Verse\Projects\C2VClient_Sample\AddressableContents\1.0.0\RemoteTest\Windows/~~
				// _ => Path.Combine(Directory.GetCurrentDirectory(), GetContentStateBuildPath(type), PlatformMappingService.GetPlatformPathSubFolder()),
				// _ => Path.Combine(Application.dataPath, "../", GetContentStateBuildPath(type), PlatformMappingService.GetPlatformPathSubFolder(),
			};
		
		private static string GetDefaultOverridePlayerVersion(eAssetBuildType type) =>
			type switch
			{
				//TODO: Mr.Song - company name category.
				// catalog name
				eAssetBuildType.LOCAL => "metaverse_local",
				eAssetBuildType.REMOTE => "metaverse_remote",
				eAssetBuildType.REMOTE_TEST => "metaverse_remote_test",
				eAssetBuildType.EDITOR_HOSTED => "metaverse_local_remote",
				_ => "metaverse_local",
			};
#endregion	// Util
		
#region Setting

		// private static void InitSettingsEvent()
		// {
		// 	AssetSystemLog($"AssetSystemManagerEditor::InitSettingsEvent called");
		// 	_settings.OnModification -= OnModification;
		// 	_settings.OnModification += OnModification;
		// }
		//
		// private static void OnModification(AddressableAssetSettings settings, AddressableAssetSettings.ModificationEvent modificationEvent, object obj)
		// {
		// 	AssetSystemLog($"AssetSystemManagerEditor::OnModification called");
		// 	AssetSystemLog($"modificationEvent : {modificationEvent}");
		// }
		
		private static void ChangeSetting(eAssetBuildType type)
		{
			//TODO: Mr.Song - 아래 내용 고려 및 수정
			AssetSystemLog("AssetSystemManagerEditor::ChangeSetting called");
			AssetSystemLog($"type : {type}");
			if (type == eAssetBuildType.LOCAL)
			{
				// Diagnostics
				// m_SendProfilerEvents
				// m_LogRuntimeExceptions
				
				// Catalog
				_settings.BundleLocalCatalog = false;
				_settings.OptimizeCatalogSize = false;
			
				// Content Update
				_settings.DisableCatalogUpdateOnStartup = true;
				_settings.BuildRemoteCatalog = true;
				
				// Downloads
				
				// Build
				_settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;
				
				// Build and Play Mode Scripts
				ChangeActivePlayerDataBuilder(eBuildScriptMode.BuildScriptPackedMode);
				ChangeActivePlayModeDataBuilder(eBuildScriptMode.BuildScriptFastMode);
			}
			else if(type == eAssetBuildType.REMOTE)
			{
				// Diagnostics
				// m_SendProfilerEvents
				// m_LogRuntimeExceptions
				
				// Catalog
				_settings.BundleLocalCatalog = false;
				_settings.OptimizeCatalogSize = false;
			
				// Content Update
				_settings.DisableCatalogUpdateOnStartup = true;
				_settings.BuildRemoteCatalog = true;
				
				// Downloads
				
				// Build
				_settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;
				
				// Build and Play Mode Scripts
				ChangeActivePlayerDataBuilder(eBuildScriptMode.BuildScriptPackedMode);
				ChangeActivePlayModeDataBuilder(eBuildScriptMode.BuildScriptPackedPlayMode);
			}
			else if(type == eAssetBuildType.REMOTE_TEST)
			{
				// Diagnostics
				// m_SendProfilerEvents
				// m_LogRuntimeExceptions
				
				// Catalog
				_settings.BundleLocalCatalog = false;
				_settings.OptimizeCatalogSize = false;
			
				// Content Update
				_settings.DisableCatalogUpdateOnStartup = true;
				_settings.BuildRemoteCatalog = true;
				
				// Downloads
				
				// Build
				_settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;
				
				// Build and Play Mode Scripts
				ChangeActivePlayerDataBuilder(eBuildScriptMode.BuildScriptPackedMode);
				ChangeActivePlayModeDataBuilder(eBuildScriptMode.BuildScriptPackedPlayMode);
			}
			else if(type == eAssetBuildType.EDITOR_HOSTED)
			{
				// Diagnostics
				// m_SendProfilerEvents
				// m_LogRuntimeExceptions
				
				// Catalog
				_settings.BundleLocalCatalog = false;
				_settings.OptimizeCatalogSize = false;
			
				// Content Update
				_settings.DisableCatalogUpdateOnStartup = true;
				_settings.BuildRemoteCatalog = true;
				
				// Downloads
				
				// Build
				_settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;
				
				// Build and Play Mode Scripts
				ChangeActivePlayerDataBuilder(eBuildScriptMode.BuildScriptPackedMode);
				ChangeActivePlayModeDataBuilder(eBuildScriptMode.BuildScriptVirtualMode);
			}
			
			// build type dependency
			// - Catalog
			_settings.OverridePlayerVersion = GetDefaultOverridePlayerVersion(type);
			// - Content Update
			_settings.CheckForContentUpdateRestrictionsOption = CheckForContentUpdateRestrictionsOptions.ListUpdatedAssetsWithRestrictions;
			_settings.ContentStateBuildPath = GetContentStateBuildPath(type);
			_settings.RemoteCatalogBuildPath.SetVariableByName(_settings, GetBuildPathName(type));
			_settings.RemoteCatalogLoadPath.SetVariableByName(_settings, GetLoadPathName(type));
			DetailAssetSystemLog($"Content State Build Path : {GetContentStateBuildPath(type)}");
			DetailAssetSystemLog($"Build Path : {_settings.profileSettings.GetValueByName(_settings.activeProfileId,  GetBuildPathName(type))}");
			DetailAssetSystemLog($"Load Path : {_settings.profileSettings.GetValueByName(_settings.activeProfileId,  GetLoadPathName(type))}");
		}
#endregion	// Setting

#region Profile
		private static void ProfileInfo()
		{
			AssetSystemLog("AssetSystemManagerEditor::ProfileInfo called");
			var profileNames = _profileSettings.GetAllProfileNames();
			var variableNames= _profileSettings.GetVariableNames();
			var activeProfileId = _settings.activeProfileId;
			var log = "@ ProfileInfo\n\n";
			foreach (var profileName in profileNames)
			{
				var profileId = _profileSettings.GetProfileId(profileName);
				log += $" * profileName : {profileName}";
				if (activeProfileId.Equals(profileId))
				{
					log += " (Active)";
				}
				log += "\n";
				log += $"  + profileId : {profileId}\n";
				foreach (var variableName in variableNames)
				{
					var variable = _profileSettings.GetValueByName(profileId, variableName);
					log += $"  + {variableName} : {variable}\n";
				}
				log += "\n";
			}
			AssetSystemLog($"{log}");
		}
		
		private static void ChangeActiveProfile(string profileName)
		{
			AssetSystemLog($"AssetSystemManagerEditor::ChangeActiveProfile called");
			// AssetSystemLog($"type : {type}");
			// var profileName = type.ToString();
			// if (type == eAssetBuildType.LOCAL) { profileName = "Default"; }
			AssetSystemLog($"profileName : {profileName}");
			var id = _profileSettings.GetProfileId(profileName);
			if (!string.IsNullOrEmpty(id))
			{
				_settings.activeProfileId = id;
				AssetSystemLog($"ChangeActiveProfile Success");
				DetailAssetSystemLog($"profileId : {id}");
			}
			else
			{
				AssetSystemLog($"ChangeActiveProfile Fail");
			}
		}
#endregion	// Profile

#region PlayModeScript

		private static Dictionary<eBuildScriptMode, int> _dataBuilderIndex;
		
		private enum eBuildScriptMode
		{
			// Use Asset Database (fastest)
			// UnityEditor.AddressableAssets.Build.DataBuilders.BuildScriptFastMode
			// editor only
			BuildScriptFastMode,
			
			// Simulate Groups (advanced)
			// UnityEditor.AddressableAssets.Build.DataBuilders.BuildScriptVirtualMode
			// editor only
			BuildScriptVirtualMode,
			
			// Use Existing Build (requires built groups)
			// UnityEditor.AddressableAssets.Build.DataBuilders.BuildScriptPackedPlayMode
			BuildScriptPackedPlayMode,
			
			// Default Build Script
			// UnityEditor.AddressableAssets.Build.DataBuilders.BuildScriptPackedMode
			BuildScriptPackedMode,		// default
		}
		
		private static void DataBuilderInfo()
		{
			AssetSystemLog("AssetSystemManagerEditor::DataBuilderInfo called");
			RefreshDataBuilderIndex();
			var log = "@ DataBuilderInfo\n\n";
			var activePlayerDataBuilder = _settings.ActivePlayerDataBuilder;
			log += $" * ActivePlayerDataBuilder\n";
			log += $"  + {activePlayerDataBuilder}\n";
			log += $"  + Name : {activePlayerDataBuilder.Name}\n";
			log += $"  + GetType() : {activePlayerDataBuilder.GetType()}\n";
			log += "\n";
			
			var activePlayModeDataBuilder = _settings.ActivePlayModeDataBuilder;
			log += $" * ActivePlayModeDataBuilder\n";
			log += $"  + {activePlayModeDataBuilder}\n";
			log += $"  + Name : {activePlayModeDataBuilder.Name}\n";
			log += $"  + GetType() : {activePlayModeDataBuilder.GetType()}\n";
			log += "\n";
			
// 			log += $" * Select Able List\n";
// 			var dataBuilders = _settings.DataBuilders;
// 			foreach (var dataBuilder in dataBuilders)
// 			{
// 				log += $"  + dataBuilder : {dataBuilder}\n";
// 				log += $"  + dataBuilder.name : {dataBuilder.name}\n";
// 				log += $"  + dataBuilder.GetType() : {dataBuilder.GetType()}\n";
// 				log += "\n";
// 			}
			AssetSystemLog($"{log}");
		}

		private static void RefreshDataBuilderIndex()
		{
			DetailAssetSystemLog("AssetSystemManagerEditor::RefreshDataBuilderIndex called");
			_dataBuilderIndex ??= _dataBuilderIndex = new Dictionary<eBuildScriptMode, int>();
			if (_dataBuilderIndex != null)
			{
				// not cache , setting - order change able.. 
				_dataBuilderIndex?.Clear();
				int index = 0;
				var log = $"@ Data Builder Type\n\n";
				foreach (var dataBuilder in _settings.DataBuilders)
				{
					if (Enum.TryParse(dataBuilder.name, out eBuildScriptMode type))
					{
						log += $" + dataBuilder.name : {dataBuilder.name}\n";
						log += $" + Type : {type}\n";
						log += $" + Index : {index}\n\n";
						_dataBuilderIndex?.Add(type, index++);	
					}
				}
				DetailAssetSystemLog($"{log}");
			}
		}
		
		private static void ChangeActivePlayerDataBuilder(eBuildScriptMode type)
		{
			// Player
			// - data builder for player data.
			AssetSystemLog("AssetSystemManagerEditor::ChangeActivePlayerDataBuilder called");
			AssetSystemLog($"type : {type}");
			RefreshDataBuilderIndex();
			if (_dataBuilderIndex.TryGetValue(type, out int index))
			{
				DetailAssetSystemLog($"ActivePlayerDataBuilder (before) : {_settings.ActivePlayerDataBuilder}");
				_settings.ActivePlayerDataBuilderIndex = index;
				DetailAssetSystemLog($"ActivePlayerDataBuilder (current) : {_settings.ActivePlayerDataBuilder}");
			}
		}

		private static void ChangeActivePlayModeDataBuilder(eBuildScriptMode type)
		{
			// Play Mode
			// - data builder for editor play mode data.
			AssetSystemLog("AssetSystemManagerEditor::ChangeActivePlayModeDataBuilder called");
			AssetSystemLog($"type : {type}");
			RefreshDataBuilderIndex();
			if (_dataBuilderIndex.TryGetValue(type, out int index))
			{
				DetailAssetSystemLog($"ActivePlayModeDataBuilder (before) : {_settings.ActivePlayModeDataBuilder}");
				_settings.ActivePlayModeDataBuilderIndex = index;
				DetailAssetSystemLog($"ActivePlayModeDataBuilder (current) : {_settings.ActivePlayModeDataBuilder}");
			}
		}
#endregion	// PlayModeScript
		
#region AssetGroup

		private static void ChangeGroups(eAssetBuildType type)
		{
			AssetSystemLog("AssetSystemManagerEditor::ChangeGroups called");
			AssetSystemLog($"type : {type}");
			if (_settings == null || _settings.groups == null) { return; }
			foreach (var group in _settings.groups)
			{
				if (group == null) { continue; }
				ChangeGroupSchema(group, type);
				// if (group.IsDefaultGroup()) { ChangeGroupSchema(group); }
				// else { ChangeGroupSchema(group, type); }
			}
		}
		
		private static void GroupInfo()
		{
			AssetSystemLog("AssetSystemManagerEditor::GroupInfo called");
			var log = "@ GroupInfo\n\n";
			// log += $" * DefaultGroup.Name : {DEFAULT_LOCAL_GROUP}\n";
			log += $" * Group list\n";
			if (_settings == null || _settings.groups == null) { return; }
			foreach (var group in _settings.groups)
			{
				if (group == null || group.entries == null) { continue; }
				log += $"  + group.Name : {group.Name}";
				if (group.IsDefaultGroup())
				{
					log += " (Default)";
				}
				log += "\n";
				log += $"  + group.Guid : {group.Guid}\n";
				log += $"  + group.Schemas\n";
				foreach (var schema in group.Schemas)
				{
					log += $"    - schema.name : {schema.name}\n";
				}
				
				log += $"  + BundledAssetGroupSchema\n";
				BundledAssetGroupSchema bundledAssetGroupSchema = group.GetSchema<BundledAssetGroupSchema>();
				if (bundledAssetGroupSchema != null)
				{
					log += $"    - bundledAssetGroupSchema.name : {bundledAssetGroupSchema.name}\n";
					log += $"    - bundledAssetGroupSchema.IncludeInBuild : {bundledAssetGroupSchema.IncludeInBuild}\n";
					log += $"    - bundledAssetGroupSchema.Compression : {bundledAssetGroupSchema.Compression}\n";
					log += $"    - bundledAssetGroupSchema.BundleMode : {bundledAssetGroupSchema.BundleMode}\n";
					log += $"    - bundledAssetGroupSchema.BuildPath : {bundledAssetGroupSchema.BuildPath.GetValue(_settings)}\n";
					log += $"    - bundledAssetGroupSchema.LoadPath : {bundledAssetGroupSchema.LoadPath.GetValue(_settings)}\n";
					log += $"    - bundledAssetGroupSchema.Timeout : {bundledAssetGroupSchema.Timeout}\n";
					log += $"    - bundledAssetGroupSchema.RetryCount : {bundledAssetGroupSchema.RetryCount}\n";
				}

				log += $"  + ContentUpdateGroupSchema\n";
				ContentUpdateGroupSchema contentUpdateGroupSchema = group.GetSchema<ContentUpdateGroupSchema>();
				if (contentUpdateGroupSchema != null)
				{
					log += $"    - contentUpdateGroupSchema.name : {contentUpdateGroupSchema.name}\n";
					// log += $"    - contentUpdateGroupSchema.Group : {contentUpdateGroupSchema.Group}\n";
					log += $"    - contentUpdateGroupSchema.StaticContent : {contentUpdateGroupSchema.StaticContent}\n";
					// contentUpdateGroupSchema.HideAndDontSaveFlags
				}
				
				log += $"  + group.entries\n";
				foreach (var entry in group.entries)
				{
					if (entry == null) { continue; }
					log += $"    - entry.guid : {entry.guid}\n";
					log += $"    - entry.address : {entry.address}\n";
					log += $"    - entry.AssetPath : {entry.AssetPath}\n";
					log += $"    - entry.labels : {entry.labels}\n";
					// log += $"    - entry.IsScene : {entry.IsScene}\n";
					// log += $"    - entry.IsFolder : {entry.IsFolder}\n";
				}
				log += "\n";
			}
			AssetSystemLog($"{log}");
		}
		
		private static string GetAssetPath(string path)
		{
			if (String.IsNullOrEmpty(path) || path.Length <= ASSET_ROOT_PATH_LENGTH) { return ""; }
			return path.Substring(ASSET_ROOT_PATH_LENGTH);
		}
		
		private static string MakeGroupName_V1(string path, int depth = 1)
		{
			if (String.IsNullOrEmpty(path)) { return "";}
			DetailAssetSystemLog("AssetSystemManagerEditor::MakeGroupName_V1 called");
			DetailAssetSystemLog($"path : {path}");
			DetailAssetSystemLog($"depth : {depth}");
			DirectoryInfo directoryInfo = new DirectoryInfo(path);
			if (depth == 1)
			{
				// ex) ~/Scene -> "scene"
				DetailAssetSystemLog($"Name : {directoryInfo.Name}");
				return directoryInfo.Name.ToLower();
			}
			else if (depth == 2)
			{
				// ex) ~/Common/ArtAsset -> "common_artasset"
				if (directoryInfo.Parent != null)
				{
					DetailAssetSystemLog($"Parent.Name : {directoryInfo.Parent.Name}");
					DetailAssetSystemLog($"Name : {directoryInfo.Name}");
					return (directoryInfo.Parent.Name + "_" + directoryInfo.Name).ToLower();
				}
			}
			DetailAssetSystemLog("error case");
			return "";
		}

		private static string MakeGroupName_V2(string rootPath, string path)
		{
			if (String.IsNullOrEmpty(rootPath) || String.IsNullOrEmpty(path)) { return "";}
			DetailAssetSystemLog("AssetSystemManagerEditor::MakeGroupName_V2 called");
			DetailAssetSystemLog($"rootPath : {rootPath}");
			DetailAssetSystemLog($"path : {path}");
			var aag_suffix_length = IsTableData(path) ? 0 : SUFFIX_LENGTH_AAG;
			// ex)
			// rootPath : C:\Users\user\Desktop\Com2Verse\Projects\C2VClient_Sample\Assets\Project\Bundles
			// path : C:\Users\user\Desktop\Com2Verse\Projects\C2VClient_Sample\Assets\Project\Bundles\Com2us\ArtAsset\Animations_AAG
			// -> Com2us\ArtAsset\Animations_AAG
			var rootPathLenght = rootPath.Length + 1;
			var subString = path.Substring(rootPathLenght, path.Length - rootPathLenght - aag_suffix_length);
			// -> com2us_artasset_animations
			return subString.Replace(Path.DirectorySeparatorChar, '_').ToLower();
		}
		
		private static void CreateGroupAndEntry(string path, string groupName)
		{
			// 6.20 sec (22/06/10)
			// CreateGroupAndEntry_V1(path, groupName);
			
			// 7.25 ~ 8.26 sec (22/06/10)
			// 610 sec (22/11/18)
			// CreateGroupAndEntry_V2(path, groupName);
			
			// 68.81 sec (22/11/18)
			// CreateGroupAndEntry_V3(path, groupName);
			
			// 14.77 sec (22/11/18)
			CreateGroupAndEntry_V4(path, groupName);
		}
		
		/// <summary>
		/// Logic v1
		/// folder 통째로 addressable group 에 추가 하는 Logic
		/// </summary>
		private static void CreateGroupAndEntry_V1(string rootPath, string groupName)
		{
			DetailAssetSystemLog("CreateGroupAndEntry_V1::CreateGroupAndEntry called");
			DetailAssetSystemLog($"rootPath : {rootPath}");
			DetailAssetSystemLog($"groupName : {groupName}");
			if (_settings == null || _template == null) { return; }
			
			// create
			DetailAssetSystemLog("CreateGroup");
			var newGroup = _settings.FindGroup(groupName);
			if (newGroup == null)
			{
				newGroup = _settings.CreateGroup(
					groupName,
					false,
					false,
					false,
					null,
					_types);
				_template.ApplyToAddressableAssetGroup(newGroup);
			}
			
			// create entry
			var guid = AssetDatabase.AssetPathToGUID(rootPath);
			DetailAssetSystemLog("CreateOrMoveEntry");
			DetailAssetSystemLog($"guid : {guid}");
			_settings.CreateOrMoveEntry(guid, newGroup);
			
			// set default group schema
			SetDefaultGroupSchema(newGroup);
		}
		
		/// <summary>
		/// Logic v2
		/// folder 의 파일 하나하나 addressable group 에 추가 하는 방식.
		/// </summary>
		private static void CreateGroupAndEntry_V2(string rootPath, string groupName)
		{
			DetailAssetSystemLog("CreateGroupAndEntry_V2::CreateGroupAndEntry called");
			DetailAssetSystemLog($"rootPath : {rootPath}");
			DetailAssetSystemLog($"groupName : {groupName}");
			if (_settings == null || _template == null) { return; }
			
			// create
			DetailAssetSystemLog("CreateGroup");
			var newGroup = _settings.FindGroup(groupName);
			if (newGroup == null)
			{
				newGroup = _settings.CreateGroup(
					groupName,
					false,
					false,
					false,
					null,
					_types);
				_template.ApplyToAddressableAssetGroup(newGroup);
			}
			
			// label
			var label = $"{groupName}_label";
			_settings.AddLabel(label);
			
			// addressable
			string[] guids = AssetDatabase.FindAssets("", new[] {rootPath});
			if (guids == null) { return; }

			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				if (Directory.Exists(path)) { continue; }
				var extension = Path.GetExtension(path);
				if (!AssetAbleExtension(extension)) { continue; }
				// create entry
				var fileGuid = AssetDatabase.AssetPathToGUID(path);
				DetailAssetSystemLog("CreateOrMoveEntry");
				DetailAssetSystemLog($"guid : {guid}");
				var entry = _settings.CreateOrMoveEntry(fileGuid, newGroup);
				if (entry != null)
				{
					// Addressable Name
					// - ex) "Btn_Common.controller"
					entry.address = Path.GetFileName(path);
					
					// Label
					// - ex) "com2us_artasset_label"
					entry.labels.Clear();
					entry.SetLabel(label,true);
				}
			}
			
			// set default group schema
			SetDefaultGroupSchema(newGroup);
		}
		
		/// <summary>
		/// Logic v3
		/// folder 통째로 addressable group 에 추가 하는 방식
		/// file 하나하나 name 설정 바꾸는 방식으로.
		/// </summary>
		private static void CreateGroupAndEntry_V3(string rootPath, string groupName)
		{
			DetailAssetSystemLog("CreateGroupAndEntry_V3::CreateGroupAndEntry called");
			DetailAssetSystemLog($"rootPath : {rootPath}");
			DetailAssetSystemLog($"groupName : {groupName}");
			if (_settings == null || _template == null) { return; }
			
			// create
			DetailAssetSystemLog("CreateGroup");
			var newGroup = _settings.FindGroup(groupName);
			if (newGroup == null)
			{
				newGroup = _settings.CreateGroup(
					groupName,
					false,
					false,
					false,
					null,
					_types);
				_template.ApplyToAddressableAssetGroup(newGroup);
			}
			
			// label
			var label = $"{groupName}_label";
			_settings.AddLabel(label);
			
			// create entry
			var rootGuid = AssetDatabase.AssetPathToGUID(rootPath);
			DetailAssetSystemLog("CreateOrMoveEntry");
			DetailAssetSystemLog($"rootGuid : {rootGuid}");
			var rootEntry = _settings.CreateOrMoveEntry(rootGuid, newGroup);
			if (rootEntry != null)
			{
				rootEntry.labels.Clear();
				rootEntry.SetLabel(label,true);
				foreach (var entry in newGroup.entries)
				{
					// Addressable Name
					// - ex) "Btn_Common.controller"
					entry.SetAddress(Path.GetFileName(entry.AssetPath));

					// Label
					// - ex) "com2us_artasset_label"
					entry.labels.Clear();
					entry.SetLabel(label, true);
					
					_assetRecreateInfo.AssetName = entry.address;
					_assetRecreateInfo.NextAsset();
					_assetRecreateInfo.ShowProgress();
					if (_assetRecreateInfo.IsCancelled)
						return;
				}
			}

			// set default group schema
			SetDefaultGroupSchema(newGroup);
		}
		
		/// <summary>
		/// Logic v4
		/// 수정이 필요한것만 api 호출 하도록 변경.
		/// </summary>
		private static void CreateGroupAndEntry_V4(string rootPath, string groupName)
		{
			DetailAssetSystemLog("CreateGroupAndEntry_V4::CreateGroupAndEntry called");
			DetailAssetSystemLog($"rootPath : {rootPath}");
			DetailAssetSystemLog($"groupName : {groupName}");
			if (_settings == null || _template == null) { return; }
			
			// create
			DetailAssetSystemLog("CreateGroup");
			var group = _settings.FindGroup(groupName);
			if (group == null)
			{
				group = _settings.CreateGroup(
					groupName,
					false,
					false,
					false,
					null,
					_types);
				_template.ApplyToAddressableAssetGroup(group);
			}
			
			// label
			var label = $"{groupName}_label";
			_settings.AddLabel(label);
			
			// addressable
			string[] guids = AssetDatabase.FindAssets("", new[] {rootPath});
			if (guids == null) { return; }
			
			// entry parent check.
			List<AddressableAssetEntry> moveEntries = new List<AddressableAssetEntry>();
			foreach (var guid in guids)
			{
				var entry = _settings.FindAssetEntry(guid);
				// new - create
				if (entry == null)
				{
					_settings.CreateOrMoveEntry(guid, group);
				}
				// exist
				else
				{
					// group different
					if (entry.parentGroup != group)
					{
						moveEntries.Add(entry);
					}
					_assetRecreateInfo.AssetName = entry.address;
				}
				
				_assetRecreateInfo.NextAsset();
				_assetRecreateInfo.ShowProgress();
				if (_assetRecreateInfo.IsCancelled)
					return;
			}
			
			if (0 < moveEntries.Count)
			{
				_settings.MoveEntries(moveEntries,group);
			}
			moveEntries.Clear();
			
			// entry info refresh
			foreach (var entry in group.entries)
			{
				// Addressable Name
				// - ex) "Btn_Common.controller"
				var address = Path.GetFileName(entry.AssetPath);
				if (entry.address != address)
				{
					entry.SetAddress(address);
				}
				
				// Label
				// - ex) "com2us_artasset_label"
				if (!entry.labels.Contains(label))
				{
					entry.labels.Clear();
					entry.SetLabel(label,true);
				}
			}
			
			// set default group schema
			SetDefaultGroupSchema(group);
		}

		private static void RemoveGroupEntries(string[] excludeList)
		{
			AssetSystemLog($"RemoveGroupEntries");
			if (_settings == null || _settings.groups == null || excludeList == null) { return; }
			var removeGroups = _settings.groups.ToArray();
			foreach (var group in removeGroups)
			{
				if (group == null || string.IsNullOrEmpty(group.name) || excludeList.Contains(group.name)) { continue; }
				DetailAssetSystemLog($"group.Name : {group.Name}");
				var entries = group.entries.ToArray();
				foreach (var entry in entries)
				{
					DetailAssetSystemLog($"entry : {entry}");
					DetailAssetSystemLog($"entry.guid : {entry.guid}");
					group.RemoveAssetEntry(entry);
				}
			}
		}
		private static void RemoveGroups(List<string> excludeList)
		{
			AssetSystemLog($"Remove Groups");
			if (_settings == null || _settings.groups == null || excludeList == null) { return; }
			var removeGroups = _settings.groups.ToArray();
			foreach (var group in removeGroups)
			{
				if (group == null || string.IsNullOrEmpty(group.name) || excludeList.Contains(group.name)) { continue; }
				DetailAssetSystemLog($"group.Name : {group.Name}");
				_settings.RemoveGroup(group);
			}
		}

		private static List<string> GetRemoveExcludeGroupList(string rootPath)
		{
#if GROUP_CREATE_LOGIC_V2
			return GetRemoveExcludeGroupList_V2(rootPath);
#else
			return GetRemoveExcludeGroupList_V1(rootPath);
#endif	// GROUP_CREATE_LOGIC_V2
		}
		
		/// <summary>
		/// group 생성 기준 - 2depth
		/// </summary>
		private static List<string> GetRemoveExcludeGroupList_V1(string rootPath)
		{
			DetailAssetSystemLog($"GetRemoveExcludeGroupList_V1 called");
			DetailAssetSystemLog($"rootPath : {rootPath}");
			List<string> removeExcludeGroupList = new List<string>();
			removeExcludeGroupList.Add(BUILT_IN_DATA);
			removeExcludeGroupList.Add(DEFAULT_LOCAL_GROUP);
			var mainDirs = Directory.GetDirectories(rootPath);
			foreach (var mainDir in mainDirs)
			{
				if (mainDir == null) { continue; }
				var subDirs = Directory.GetDirectories(mainDir);
				if (subDirs.Length == 0)
				{
					removeExcludeGroupList.Add(MakeGroupName_V1(mainDir));
				}
				else
				{
					foreach (var subDir in subDirs)
					{
						removeExcludeGroupList.Add(MakeGroupName_V1(subDir, 2));
					}
				}
			}
#if DETAIL_ASSET_SYSTEM_LOG
			var log = " @ Remove Exclude Group List V1\n";
			foreach (var group in removeExcludeGroupList)
			{
				log += $"{group}\n";
			}
			DetailAssetSystemLog(log);
#endif	// DETAIL_ASSET_SYSTEM_LOG
			return removeExcludeGroupList;
		}
		
		/// <summary>
		/// group 생성 기준 - keyword
		/// </summary>
		private static readonly string SUFFIX_AAG = "_AAG";
		private static readonly int SUFFIX_LENGTH_AAG = 4;
		private static List<string> GetRemoveExcludeGroupList_V2(string rootPath)
		{
			DetailAssetSystemLog($"GetRemoveExcludeGroupList_V2 called");
			DetailAssetSystemLog($"rootPath : {rootPath}");
			List<string> removeExcludeGroupList = new List<string>();
			removeExcludeGroupList.Add(BUILT_IN_DATA);
			removeExcludeGroupList.Add(DEFAULT_LOCAL_GROUP);
			var dirs = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);
			foreach (var dir in dirs)
			{
				if (CreateAbleAag(GetAssetPath(dir)))
				{
					removeExcludeGroupList.Add(MakeGroupName_V2(rootPath, dir));
				}
			}
#if DETAIL_ASSET_SYSTEM_LOG
			var log = " @ Remove Exclude Group List V2\n";
			foreach (var group in removeExcludeGroupList)
			{
				log += $"{group}\n";
			}
			DetailAssetSystemLog(log);
#endif	// DETAIL_ASSET_SYSTEM_LOG
			return removeExcludeGroupList;
		}
		
		private static void CreateGroupsBundle(string rootPath)
		{
			AssetSystemLog($"Create Groups Bundle");
			AssetSystemLog($"rootPath : {rootPath}");
			CreateGroupAndEntry_V1(GetAssetPath(rootPath), MakeGroupName_V1(rootPath));
			// CreateGroupAndEntry_V2(GetAssetPath(rootPath), MakeGroupName(rootPath));
		}
		
		private static void CreateGroupsEach(string rootPath)
		{
			AssetSystemLog($"Create Groups Each");
			AssetSystemLog($"rootPath : {rootPath}");
			CreateGroupAndEntry_V2(GetAssetPath(rootPath), MakeGroupName_V1(rootPath));
		}

		private static void CreateGroups(string rootPath)
		{
#if GROUP_CREATE_LOGIC_V2
			CreateGroups_V2(rootPath);
#else
			CreateGroups_V1(rootPath);
#endif	// GROUP_CREATE_LOGIC_V2
		}
		
		/// <summary>
		/// group 생성 기준 - 2depth
		/// </summary>
		private static void CreateGroups_V1(string rootPath)
		{
			DetailAssetSystemLog($"Create Groups V1");
			DetailAssetSystemLog($"rootPath : {rootPath}");
			var mainDirs = Directory.GetDirectories(rootPath);
			foreach (var mainDir in mainDirs)
			{
				if (mainDir == null)
				{
					DetailAssetSystemLog($"mainDir invalid");
					continue;
				}
				
				DetailAssetSystemLog($"mainDir : {mainDir}");
				var subDirs = Directory.GetDirectories(mainDir);
				if (subDirs.Length == 0)
				{
					// 1 depth path case
					// ex) ~/Assets/Project/Bundles/Scene
					// -> ~/Scene
					CreateGroupAndEntry(GetAssetPath(mainDir), MakeGroupName_V1(mainDir));
				}
				else
				{
					foreach (var subDir in subDirs)
					{
						// 2 depth path case
						// ex) ~/Assets/Project/Bundles/Common/ArtAsset
						// -> ~/Common/ArtAsset
						CreateGroupAndEntry(GetAssetPath(subDir), MakeGroupName_V1(subDir, 2));
					}
				}
			}
		}
		
		/// <summary>
		/// group 생성 기준 - keyword
		/// </summary>
		private static void CreateGroups_V2(string rootPath)
		{
			DetailAssetSystemLog($"Create Groups V2");
			DetailAssetSystemLog($"rootPath : {rootPath}");
			var dirs = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);
			_assetRecreateInfo = AssetRecreateInfo.CreateNew();
			_assetRecreateInfo.SetAssetsInfo(rootPath, dirs);
			foreach (var dir in dirs)
			{
				_assetRecreateInfo.SetDirectoryName(dir);
				if (CreateAbleAag(GetAssetPath(dir)))
				{
					CreateGroupAndEntry(GetAssetPath(dir), MakeGroupName_V2(rootPath, dir));
				}
				_assetRecreateInfo.IndexDir++;
				if (_assetRecreateInfo.IsCancelled)
					break;
			}
			_assetRecreateInfo.HideProgress();
			_assetRecreateInfo = null;
		}

		private static void SetDefaultGroupSchema(AddressableAssetGroup group)
		{
			// AssetSystemLog($"AssetSystemManagerEditor::SetDefaultGroupSchema called");
			ChangeGroupSchema(group);
		}
		
		private static void ChangeGroupSchema(AddressableAssetGroup group, eAssetBuildType type = eAssetBuildType.LOCAL)
		{
			DetailAssetSystemLog($"AssetSystemManagerEditor::ChangeGroupSchema called");
			DetailAssetSystemLog($"group : {group.Name}");
			DetailAssetSystemLog($"type : {type}");
			if (BUILT_IN_DATA.Equals(group.name)) { return; }
			// if (DEFAULT_LOCAL_GROUP.Equals(group.name)) { return; }
			
			// BundledAssetGroupSchema
			BundledAssetGroupSchema bundledAssetGroupSchema = group.GetSchema<BundledAssetGroupSchema>();
			if (bundledAssetGroupSchema == null)
			{
				group.RemoveSchema<BundledAssetGroupSchema>();
				bundledAssetGroupSchema = ScriptableObject.CreateInstance<BundledAssetGroupSchema>();
				group.AddSchema(bundledAssetGroupSchema);
			}
			
			//TODO: Mr.Song - 회사,종류(월드,오피스)별 포함 리소스 고려. (IncludeInBuild, IncludeAddressInCatalog)...
			// - others.
			bundledAssetGroupSchema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
			bundledAssetGroupSchema.IncludeInBuild = true;
			bundledAssetGroupSchema.UseAssetBundleCache = true;
			bundledAssetGroupSchema.IncludeAddressInCatalog = true;
			bundledAssetGroupSchema.IncludeGUIDInCatalog = true;
			bundledAssetGroupSchema.IncludeLabelsInCatalog = true;
			bundledAssetGroupSchema.InternalIdNamingMode = AssetNamingMode.FullPath;
			bundledAssetGroupSchema.InternalBundleIdMode = BundleInternalIdMode.GroupGuidProjectIdHash;
			bundledAssetGroupSchema.AssetBundledCacheClearBehavior = CacheClearBehavior.ClearWhenWhenNewVersionLoaded;
			bundledAssetGroupSchema.BundleMode = BundlePackingMode.PackTogether;
			bundledAssetGroupSchema.AssetLoadMode = AssetLoadMode.RequestedAssetAndDependencies;
			// - build type dependency
			bundledAssetGroupSchema.BuildPath.SetVariableByName(_settings, GetBuildPathName(type));
			bundledAssetGroupSchema.LoadPath.SetVariableByName(_settings, GetLoadPathName(type));

			// ContentUpdateGroupSchema
			ContentUpdateGroupSchema contentUpdateGroupSchema = group.GetSchema<ContentUpdateGroupSchema>();
			if (contentUpdateGroupSchema == null)
			{
				group.RemoveSchema<ContentUpdateGroupSchema>();
				contentUpdateGroupSchema = ScriptableObject.CreateInstance<ContentUpdateGroupSchema>();
				group.AddSchema(contentUpdateGroupSchema);
			}
			// Prevent Updates
			// ContentType current = m_StaticContent ? ContentType.CannotChangePostRelease : ContentType.CanChangePostRelease;
			contentUpdateGroupSchema.StaticContent = false;
			
			EditorUtility.SetDirty(group);
		}

		private static bool CreateAbleAag(string path)
		{
			if (path.EndsWith(SUFFIX_AAG))
			{
				// DetailAssetSystemLog($"path : {path}");
				var matchItems =
					from folderName in path.Split(Path.DirectorySeparatorChar)
					where folderName.EndsWith(SUFFIX_AAG)
					select folderName;
				
				// success
				if (matchItems.Count() == 1)
				{
					return true;
				}
#if DETAIL_ASSET_SYSTEM_LOG
				// fail
				if (1 < matchItems.Count())
				{
					DetailAssetSystemLog($"aag dir error : {path}");
					foreach (var item in matchItems)  
					{  
						DetailAssetSystemLog($"item : {item}");
					} 
				}
#endif	// DETAIL_ASSET_SYSTEM_LOG
			}
			else if(IsTableData(path))
			{
				return true;
			}
			return false;
		}

		private static bool IsTableData(string path)
		{
			//FIXME: Mr.Song - data 경로 hard coding 부분 처리 필요.
			var list = path.Split(Path.DirectorySeparatorChar);
			var strData = list[^1];
			var strTableData = list[^2];
			if (strData.ToLower().Equals("data") && strTableData.ToLower().Equals("tabledata"))
			{
				// DetailAssetSystemLog($"path : {path}");
				return true;
			}
			return false;
		}
#endregion // AssetGroup

#region Build
		public static void Clean()
		{
			AssetSystemLog("AssetSystemManagerEditor::Clean called");
			// TimeCheckStart();
			AddressableAssetSettings.CleanPlayerContent();
			// AddressableAssetSettings.CleanPlayerContent(_settings.ActivePlayerDataBuilder);
			// _settings.ActivePlayerDataBuilder.ClearCachedData();
			// _settings.ActivePlayModeDataBuilder.ClearCachedData();
			BuildCache.PurgeCache(false);
			// TimeCheckEnd();
		}

		private static void BuildSetting(eAssetBuildType type, bool update = false)
		{
			AssetSystemLog($"AssetSystemManagerEditor::BuildSetting called");
			AssetSystemLog($"type : {type}");
			// InitSettingsEvent();
			ChangeSetting(type);
			// ChangeActiveProfile(type);
			ChangeGroups(type);
			
			//FIXME: Mr.Song - appinfo file update 처리.
			// if (update && AppInfo.Instance != null)
			// {
			// 	AppInfo.Instance.UpdateAssetBuildType(type);
			// }
		}
		
		public static bool Build(eAssetBuildType type, bool update = false)
		{
			AssetSystemLog($"AssetSystemManagerEditor::Build (type) called");
			AssetSystemLog($"type : {type}");
			//BuildSetting(type, update);
			return Build();
		}
#endregion	// Build

#region Check

		[NotNull] private static string[] _extensionIncludeList =
		{
			".bytes",
			".json",
			".signal",
			".controller",
			".anim",
			".spriteatlasv2",
			".prefab",
			".shader",
			".shadergraph",
			".asset",
			".mat",
			".lighting",
			".fbx",
			".FBX",
			".png",
			".PNG",
			// ".psd",
			// ".tif",
			".jpg",
			".mask",
			".overrideController",
			".unity",
			".playable",
			".tga",
			".otf",
			".ogg",
			".wav",
			".mixer",
			// ".cs",
			// ".asmdef",
			// ".csv",
			// ".dat",
			".exr",
			".mp3",
			".mp4",
		};
		
		[NotNull] private static string[] _extensionExcludeList =
		{
			".psd",		// Art
			".tif",		// Art
			".cs",		// TableData
			".asmdef",	// TableData
			".csv",		// TableData
			".dll",		// TableData
			".dat",		// TableData - Asset Build unsupported
			".py",		// for art script - "converter.py"
			// ".meta",
		};

		private static bool MissingExtension(string extension)
		{
			return !_extensionIncludeList.Contains(extension) && !_extensionExcludeList.Contains(extension);
		}
		
		private static bool AssetAbleExtension(string extension)
		{
			return !_extensionExcludeList.Contains(extension);
		}
		
		private static void CheckAllFileExtension()
		{
			AssetSystemLog("AssetSystemManagerEditor::CheckAllFileExtension called");
			List<string> missingExtensionList = new List<string>();
			List<string> missingExtensionFileNameList = new List<string>();
			string[] guids = AssetDatabase.FindAssets("", new[] {GetAssetPath(ROOT_PATH)});
			if (guids == null) { return; }
			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				if (Directory.Exists(path)) { continue; }
				// var extension = Path.GetExtension(path).ToLower();
				var extension = Path.GetExtension(path);
				if (MissingExtension(extension))
				{
					if (!missingExtensionList.Contains(extension))
					{
						missingExtensionList.Add(extension);	
					}
					missingExtensionFileNameList.Add(Path.GetFileName(path));
				}
			}
			
			// make log
			var logMissingExtension = " @ Missing File Extension List\n\n";
			foreach (var extension in missingExtensionList)
			{
				logMissingExtension += $"\"{extension}\",\n";
			}
			var logMissingExtensionFile = " @ Missing File Extension File List\n\n";
			foreach (var fileName in missingExtensionFileNameList)
			{
				logMissingExtensionFile += $"\"{fileName}\",\n";
			}
			
			AssetSystemLog($"{logMissingExtension}");
			AssetSystemLog($"{logMissingExtensionFile}");
			missingExtensionList.Clear();
			missingExtensionFileNameList.Clear();
			// missingExtensionList = null;
			// missingExtensionFileNameList = null;
		}
		
		private static bool CheckDuplicateFile(string rootPath)
		{
			AssetSystemLog("AssetSystemManagerEditor::CheckDuplicateFile called");
			List<string> fileNameList = new List<string>();
			List<string> duplicateFileNameList = new List<string>();
			string[] guids = AssetDatabase.FindAssets("", new[] {GetAssetPath(rootPath)});
			if (guids == null) { return false; }
			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				if (Directory.Exists(path)) { continue; }
				// var fileName = Path.GetFileName(path).ToLower();	// 대소문자 구분하여 다른 파일로 처리됨.
				var fileName = Path.GetFileName(path);
				if (fileNameList.Contains(fileName))
				{
					duplicateFileNameList.Add(fileName);
				}
				else
				{
					fileNameList.Add(fileName);
				}
			}

			// make log
			var log = " @ Duplicate File List\n\n";
			foreach (var duplicateFileName in duplicateFileNameList)
			{
				log += $"\"{duplicateFileName}\",\n";
			}

			if (duplicateFileNameList.Count == 0)
			{
				log += $"\n Success (not duplicate)!!\n\n";
			}
			else
			{
				log += "\n";
			}

			AssetSystemLog($"{log}");
			bool hasDuplicateFile = (duplicateFileNameList.Count == 0);
			duplicateFileNameList.Clear();
			fileNameList.Clear();
			return hasDuplicateFile;
		}
		
		private static bool _isStart;
		private static double _timeStart;
		private static void TimeCheckStart()
		{
			if (_isStart)
			{
				AssetSystemLog("Not Good !!!");
			}
			else
			{
				_isStart = true;
				_timeStart = EditorApplication.timeSinceStartup;
			}
		}

		private static void TimeCheckEnd()
		{
			if (!_isStart)
			{
				AssetSystemLog("Not Good !!!");
			}
			else
			{
				var timeEnd = EditorApplication.timeSinceStartup;
				var timeSpend = timeEnd - _timeStart;
				var log = "@ TimeCheck\n\n";
				log += $" + timeStart : {_timeStart:F2}\n";
				log += $" + timeSpend : {timeSpend:F2}\n";
				log += $" + timeEnd : {timeEnd:F2}\n\n";
				AssetSystemLog($"{log}");
				_isStart = false;
				_timeStart = 0;
			}
		}
#endregion	// Check
		
#region AssetRecreateInfo
		class AssetRecreateInfo
		{
			public int IndexDir;
			private int _indexAsset;
			public string AssetName;

			private int _totalDirCount;
			private int _totalAssetCount;
			private string _dirName;
			private bool _isCancelled;

			private float _startTime;
			public bool IsCancelled
			{
				get => _isCancelled;
				set
				{
					_isCancelled = value;
					if (_isCancelled)
						HideProgress();
				}
			}
			private AssetRecreateInfo()
			{
				_isCancelled = false;
			}
			public static AssetRecreateInfo CreateNew() => new();
			public void SetDirectoryName(string path)
			{
				var idx = path.LastIndexOf(Path.DirectorySeparatorChar);
				_dirName = idx == -1 ? path : path.Substring(idx);
			}

			public void ShowProgress() => IsCancelled = EditorUtility.DisplayCancelableProgressBar(GetProgressTitle(), GetProgressInfo(), GetProgress());
			public void HideProgress() => EditorUtility.ClearProgressBar();

			private string GetProgressTitle() => $"Group Recreate ({IndexDir + 1}/{_totalDirCount}) - {_dirName}";
			private string GetProgressInfo() => $"({_indexAsset + 1}/{_totalAssetCount}, {GetProgressPct()}%) [{GetEstimateTime()}] {AssetName}";
			private string GetProgressPct() => (((_indexAsset + 1) / (float) _totalAssetCount) * 100f).ToString("F2");
			private float GetProgress() => _indexAsset / (float) _totalAssetCount;

			private string GetEstimateTime()
			{
				var elapsedTime = Time.realtimeSinceStartup - _startTime;
				var progressedAssetCount = _indexAsset + 1;
				var elapsedPerItem = elapsedTime / progressedAssetCount;
				var remainItems = _totalAssetCount - progressedAssetCount;
				var estimateTime = Mathf.RoundToInt(elapsedPerItem * remainItems);
				var min = (estimateTime / 60).ToString("00");
				var sec = (estimateTime % 60).ToString("00");
				return $"{min}분 {sec}초";
			}

			public void SetAssetsInfo(string rootDir, params string[] dirs)
			{
				_totalDirCount = dirs.Length;
				_totalAssetCount = 0;
				IndexDir = 0;
				foreach (var dir in dirs)
				{
					var assetsDir = GetAssetPath(dir);
					if (CreateAbleAag(assetsDir))
					{
						var groupName = MakeGroupName_V2(rootDir, dir);
						var guids = AssetDatabase.FindAssets("", new[] {assetsDir});
						foreach (var guid in guids)
						{
							var assetPath = AssetDatabase.GUIDToAssetPath(guid);
							var ext = Path.GetExtension(assetPath);
							if (Directory.Exists(assetPath)) continue;
							if (!AssetAbleExtension(ext)) continue;
							_totalAssetCount++;
						}
					}
				}
				ResetTimer();
			}

			public void NextAsset()
			{
				_indexAsset++;

				if (_indexAsset == _totalAssetCount)
					HideProgress();
			}

			private void ResetTimer() => _startTime = Time.realtimeSinceStartup;
		}
#endregion	// AssetRecreateInfo
		
#endregion	// Internal
	}
}

#endif	// UNITY_EDITOR
