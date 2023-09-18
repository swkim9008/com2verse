#define BUILD_SCRIPT_LOG
// #define DETAIL_BUILD_SCRIPT_LOG
// #define SONG_TEST

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Com2Verse.AssetSystem;
using Com2Verse.BuildHelper;
using Com2Verse.Utils;
using Com2VerseEditor.AssetSystem;
using Com2VerseEditor.BuildHelper;
using Com2VerseEditor.Serialization;
using Cysharp.Threading.Tasks;
using Sentry.Unity;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Com2VerseEditor.Build
{
	public static class BuildScript
	{
		private static readonly string VERSION_SEPARATOR = ".";
		private static readonly string BuildPath = Path.Combine("Build", "Executable");
		private static string _executableDir = BuildPath;
		private static string _appcenterExecutable;
		public static string AppName = Application.productName;
		private static ProcessUtil.ProcessInfo _process;

		enum BuildArgType
		{
			DISTRIBUTE_TYPE,		// "debug", "release"
			SCRIPTING_BACKEND,		// "il2cpp", "mono"
			ENV,					// dev, staging, production
			ASSET_BUILD_TYPE,		// local, remote, remote test, editor hosted
			CLEAN_BUILD,
			APP_CENTER_UPLOAD,
			APP_CENTER_COMMENT,
			APP_CENTER_APP_NAME,
			APP_CENTER_DISTRIBUTE_GROUP,
			CODESIGN_WINDOWS,
			GIT_BUILD_BRANCH,
			FORCE_SINGLE_INSTANCE,
			FORCE_ENABLE_APPINFO,
			FORCE_ENABLE_SENTRY,
			PACKAGE_TYPE,			// zip, msi, launcher, asset, not
			HIVE_ENV,
			ENABLE_LOGGING,
			APP_ID
		}

		private static Dictionary<BuildArgType, string> _buildArgsMap = new()
		{
			{BuildArgType.DISTRIBUTE_TYPE, "-distributeType"},
			{BuildArgType.SCRIPTING_BACKEND, "-scriptingBackend"},
			{BuildArgType.ENV, "-env"},
			{BuildArgType.ASSET_BUILD_TYPE, "-assetBuildType"},
			{BuildArgType.CLEAN_BUILD, "-cleanBuild"},
			{BuildArgType.APP_CENTER_UPLOAD, "-appCenterUpload"},
			{BuildArgType.APP_CENTER_COMMENT, "-appCenterComment"},
			{BuildArgType.APP_CENTER_APP_NAME, "-appCenterAppName"},
			{BuildArgType.APP_CENTER_DISTRIBUTE_GROUP, "-appCenterDistributeGroup"},
			{BuildArgType.CODESIGN_WINDOWS, "-codesignWindows"},
			{BuildArgType.GIT_BUILD_BRANCH, "-gitBuildBranch"},
			{BuildArgType.FORCE_SINGLE_INSTANCE, "-forceSingleInstance"},
			{BuildArgType.FORCE_ENABLE_APPINFO, "-forceEnableAppInfo"},
			{BuildArgType.FORCE_ENABLE_SENTRY, "-forceEnableSentry"},
			{BuildArgType.PACKAGE_TYPE, "-packageType"},
			{BuildArgType.HIVE_ENV, "-hiveEnv"},
			{BuildArgType.ENABLE_LOGGING, "-enableLogging"},
			{BuildArgType.APP_ID, "-appId"}
		};

		private static string GetOX(bool check) => check ? "O" : "X";

#region Jenkins
		public static async void BuildWindowsBeforeForJenkins()
		{
			BuildLog.BuildScriptLog("BuildWindowsBeforeForJenkins called");

			var builder = Builder.New(GetBuildTargetFromOSFamily())
				!.SetBuildBefore(true);
			_process = ProcessUtil.New();
			builder!.UpdateGitInfoAsync();
			builder = builder!.SetAppInfo();
				//!.SetSentryOption();


			var result = builder.BuildAssetBundle();
			if (result == eResult.FAILED)
			{
				EditorUtility.ClearProgressBar();
				EditorApplication.Exit(1);
				return;
			}

			EditorUtility.ClearProgressBar();
			EditorApplication.Exit(builder == null ? 1 : 0);

			_process = null;
		}


		public static async void BuildWindowsForJenkins()
		{
			BuildLog.BuildScriptLog("BuildWindowsForJenkins called");
			var builder = Builder.New(GetBuildTargetFromOSFamily())
				!.SetBuildBefore(false);
			_process = ProcessUtil.New();
			// await builder!.UpdateGitInfoAsync();
			// builder = builder!.SetAppInfo()
			// 	!.SetSentryOption()
			// 	!.BuildAssetBundle();
			await builder!.BuildAsync(true);

			EditorUtility.ClearProgressBar();
			EditorApplication.Exit(builder == null ? 1 : 0);
			_process = null;
		}


		public static async void UploadAssetBundleForJenkins()
		{
			BuildLog.BuildScriptLog("BuildWindowsForJenkins called");
			var builder = Builder.New(GetBuildTargetFromOSFamily())
				!.SetBuildBefore(false);

			var uploadAssetBundleResult = builder.UploadAssetBundle();
			if (uploadAssetBundleResult == eResult.FAILED)
			{
				EditorUtility.ClearProgressBar();
				EditorApplication.Exit(1);
				return;
			}
			var uploadAssetBundleVersionResult = builder.UploadAssetBundleVersion();
			if (uploadAssetBundleVersionResult == eResult.FAILED)
			{
				EditorUtility.ClearProgressBar();
				EditorApplication.Exit(1);
				return;
			}

			EditorUtility.ClearProgressBar();
			EditorApplication.Exit(0);
		}
#endregion // Jenkins

#region Build
		[MenuItem("Com2Verse/Build/Default AppInfo Make #F1")]
		public static async UniTask MakeAppInfo()
		{
			_process = ProcessUtil.New();
			var builder = Builder.New(GetCurrentBuildTarget())
				!.SetDebug(true)
				!.SetScriptingBackend(GeCurrentScriptingBackend())
				!.SetAssetBuildType()
				!.SetEnv()
				!.SetPackageType();
			builder!.UpdateGitInfoAsync();
			builder!.SetAppInfo();
			EditorUtility.ClearProgressBar();
			_process = null;
		}
		
		[MenuItem("Com2Verse/Build/Open Build Folder #F2")]
		public static void OpenBuildFolder()
		{
			OpenFolder(BuildPath);
		}
		
		[MenuItem("Com2Verse/Build/Build - Current Appinfo #F3")]
		static async void BuildCurrentAppInfo()
		{
			await Build(AppInfo.Instance.Data);
			OpenFolder(_executableDir);
		}

		static async UniTask Build(AppInfoData appInfoData,bool execute = false, bool editorExit = false, bool isLogging = true)
		{
			BuildLog.BuildScriptLog("Build called");
			var buildTarget = GetBuildTargetFromString(appInfoData.BuildTarget);
			var scriptingBackend = GetScriptingBackendFromString(appInfoData.ScriptingBackend);
			var builder = Builder.New(buildTarget)
				!.SetDebug(appInfoData.IsDebug)
				!.SetAppId(appInfoData.AppId)
				!.SetEnableLogging(isLogging)
				!.SetCopyPDBFilesOption(true)
				!.SetScriptingBackend(scriptingBackend)
				!.SetAssetBuildType(appInfoData.AssetBuildType)
				!.SetEnv(appInfoData.Environment)
				!.SetHiveEnv(appInfoData.HiveEnvType)
				!.SetPackageType(appInfoData.PackageType)
				!.SetAdditionalIl2CppArgs()
				!.SetCacheServerInfo()
				!.UpdateDefineSymbolsBuilder(true)
				!.SetExecuteAfterBuild(execute)
				!.SetIncrementalBuild()
				!.SetCodeSign(false)
				!.SetAppCenterUpload(false)
				!.SetAppCenterComment($"Build via UnityEditor ({buildTarget.ToString()} - {scriptingBackend.ToString()})")
				!.SetAppCenterAppName("")
				!.SetForceSingleInstance(appInfoData.IsForceSingleInstance)
				!.SetForceEnableAppInfo(appInfoData.IsForceEnableAppInfo)
				!.SetForceEnableSentry(appInfoData.IsForceEnableSentry)
				!.UpdateAppIcon();

			builder!.UpdateGitInfoAsync();
			builder!.SetAppInfo();
			//builder!.SetSentryOption();

			var result = builder!.BuildAssetBundle();
			if (result == eResult.FAILED)
				return;

			await builder!.BuildAsync(editorExit);

			EditorUtility.ClearProgressBar();
		}

		static async UniTask Build(
			BuildTarget buildTarget,
			ScriptingImplementation backend,
			bool execute,
			bool editorExit,
			bool isDebug = true,
			bool isForceEnableAppInfo = false,
			bool isLogging = true)
		{
			AppInfoData appInfoData = AppInfoData.Default;
			appInfoData.BuildTarget = buildTarget.ToString();
			appInfoData.ScriptingBackend = backend.ToString();
			appInfoData.IsDebug = isDebug;
			appInfoData.IsForceEnableAppInfo = isForceEnableAppInfo;
			await Build(appInfoData,execute,editorExit, isLogging);
		}
#endregion	// Build
		
#region Test
		
#if SONG_TEST
		[MenuItem("Com2Verse/Build/Build Test")]
		static async UniTask BuildTest()
		{
			BuildLog.BuildScriptLog("BuildTest called");
			var builder = Builder.New(BuildTarget.StandaloneWindows64)
				!.SetDebug(false)
				!.SetScriptingBackend(ScriptingImplementation.Mono2x)
				!.SetAssetBuildType()
				!.SetEnv()
				!.SetPackageType()
				!.SetAdditionalIl2CppArgs()
				!.SetCacheServerInfo()
				!.UpdateDefineSymbolsBuilder()
				!.SetExecuteAfterBuild(false)
				!.SetIncrementalBuild()
				!.SetCodeSign(false)
				!.SetAppCenterUpload(false)
				!.SetAppCenterComment("Build via UnityEditor (mono)")
				!.SetAppCenterAppName("")
				!.SetForceSingleInstance(true)
				!.SetForceEnableAppInfo(true)
				!.SetForceEnableSentry(true)
				!.UpdateAppIcon();
			_process = ProcessUtil.New();
			await builder!.UpdateGitInfoAsync();
			builder!.SetAppInfo();
			builder!.SetSentryOption();
			builder!.BuildAssetBundle();
			await builder!.BuildAsync(false);
			EditorUtility.ClearProgressBar();
			_process = null;
			OpenFolder(_executableDir);
			BuildLog.BuildScriptLog("BuildTest End");
		}
#endif // SONG_TEST

#endregion	// Test
		
#region OSX

		static async UniTask BuildOsxIl2Cpp(bool execute, bool editorExit, bool isDebug = true, bool isForceEnableAppInfo = false)
		{
			await Build(BuildTarget.StandaloneOSX, ScriptingImplementation.IL2CPP, execute, editorExit, isDebug, isForceEnableAppInfo);
		}
		
		[MenuItem("Com2Verse/Build/OSX/Build/IL2CPP - debug")]
		static async void BuildAndOpenOsxIl2CppDebug()
		{
			await BuildOsxIl2Cpp(false, false);
			OpenFolder(_executableDir);
		}

		[MenuItem("Com2Verse/Build/OSX/Build/IL2CPP - release")]
		static async void BuildAndOpenOsxIl2CppRelease()
		{
			await BuildOsxIl2Cpp(false, false,false);
			OpenFolder(_executableDir);
		}
		
		[MenuItem("Com2Verse/Build/OSX/Build and run/IL2CPP - debug")]
		static async void BuildAndRunOsxIl2CppDebug()
		{
			await BuildOsxIl2Cpp(true, false);
		}

		[MenuItem("Com2Verse/Build/OSX/Build and run/IL2CPP - release")]
		static async void BuildAndRunOsxIl2CppRelease()
		{
			await BuildOsxIl2Cpp(true, false,false);
		}
		
#endregion	// OSX

#region Windows 64
		
		static async UniTask BuildWindowsMono(bool execute, bool editorExit, bool isDebug = true, bool isForceEnableAppInfo = false, bool isLogging = true)
		{
			await Build(BuildTarget.StandaloneWindows64, ScriptingImplementation.Mono2x, execute, editorExit, isDebug, isForceEnableAppInfo, isLogging);
		}
		
		static async UniTask BuildWindowsIl2Cpp(bool execute, bool editorExit, bool isDebug = true, bool isForceEnableAppInfo = false)
		{
			await Build(BuildTarget.StandaloneWindows64, ScriptingImplementation.IL2CPP, execute, editorExit, isDebug, isForceEnableAppInfo);
		}
		
		[MenuItem("Com2Verse/Build/Windows64/Build/Mono - debug #F4")]
		static async void BuildAndOpenWindowsMono()
		{
			await BuildWindowsMono(false, false);
			OpenFolder(_executableDir);
		}

		[MenuItem("Com2Verse/Build/Windows64/Build/Mono - debug(No Log)")]
		static async void BuildAndOpenWindowsMonoNoLog()
		{
			await BuildWindowsMono(false, false, true, false, false);
			OpenFolder(_executableDir);
		}
		
		[MenuItem("Com2Verse/Build/Windows64/Build/Mono - release #F5")]
		static async void BuildAndOpenWindowsMonoRelease()
		{
			await BuildWindowsMono(false, false, false);
			OpenFolder(_executableDir);
		}
		
		[MenuItem("Com2Verse/Build/Windows64/Build/Mono - release (Test) #F6")]
		static async void BuildAndOpenWindowsMonoReleaseTest()
		{
			await BuildWindowsMono(false, false, false, true);
			OpenFolder(_executableDir);
		}
		
		[MenuItem("Com2Verse/Build/Windows64/Build and run/Mono - debug")]
		static async void BuildAndRunWindowsMono()
		{
			await BuildWindowsMono(true, false);
		}
		
		[MenuItem("Com2Verse/Build/Windows64/Build and run/Mono - release")]
		static async void BuildAndRunWindowsMonoRelease()
		{
			await BuildWindowsMono(true, false, false);
		}

		[MenuItem("Com2Verse/Build/Windows64/Build and run/IL2CPP - debug")]
		static async void BuildAndRunWindowsIl2Cpp()
		{
			await BuildWindowsIl2Cpp(true, false);
		}

		[MenuItem("Com2Verse/Build/Windows64/Build/IL2CPP - debug")]
		static async void BuildAndOpenWindowsIl2Cpp()
		{
			await BuildWindowsIl2Cpp(false, false);
			OpenFolder(_executableDir);
		}
		
		[MenuItem("Com2Verse/Build/Windows64/Build and run/IL2CPP - release")]
		static async void BuildAndRunWindowsIl2CppRelease()
		{
			await BuildWindowsIl2Cpp(true, false, false);
		}

		[MenuItem("Com2Verse/Build/Windows64/Build/IL2CPP - release")]
		static async void BuildAndOpenWindowsIl2CppRelease()
		{
			await BuildWindowsIl2Cpp(false, false, false);
			OpenFolder(_executableDir);
		}
		
		[MenuItem("Com2Verse/Build/Windows64/Build/IL2CPP - release (Test)")]
		static async void BuildAndOpenWindowsIl2CppReleaseTest()
		{
			await BuildWindowsIl2Cpp(false, false, false, true);
			OpenFolder(_executableDir);
		}
		
#endregion // Windows 64

#region Build Common

		static string[] GetAllSceneNames() => (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
		static BuildTarget GetCurrentBuildTarget()
		{
			return EditorUserBuildSettings.activeBuildTarget;
		}
		
		static BuildTarget GetBuildTargetFromOSFamily() => SystemInfo.operatingSystemFamily switch
		{
			OperatingSystemFamily.Windows => BuildTarget.StandaloneWindows64,
			OperatingSystemFamily.MacOSX => BuildTarget.StandaloneOSX,
			_ => BuildTarget.NoTarget,
		};
		
		static ScriptingImplementation GeCurrentScriptingBackend()
		{
			return PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup);
		}
		
		static void OpenFolder(string path)
		{
			BuildLog.DetailBuildScriptLog("OpenFolder called");
			BuildLog.DetailBuildScriptLog($"path : {path}");
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			Process.Start(path);
			// EditorUtility.RevealInFinder(path);
		}

#endregion // Build Common

#region Argument

		private static string GetArg(BuildArgType type) => GetArg(_buildArgsMap![type]);

		private static string GetArg(string name)
		{
			var args = System.Environment.GetCommandLineArgs();
			for (int i = 0; i < args.Length; ++i)
			{
				if (args[i] == name && args.Length > i + 1)
				{
					BuildLog.DetailBuildScriptLog($"GetArg Success: {args[i]} = {args[i + 1]}");
					return args[i + 1];
				}
			}

			return string.Empty;
		}

		private static ScriptingImplementation GetArgScriptingBackend() => GetArg(BuildArgType.SCRIPTING_BACKEND)?.ToLower() == "mono" ? ScriptingImplementation.Mono2x : ScriptingImplementation.IL2CPP;

		private static eAssetBuildType GetArgAssetBuildType()
		{
			var assetBuildTypeStr = GetArg(BuildArgType.ASSET_BUILD_TYPE)?.ToLower();
			var assetBuildType = assetBuildTypeStr switch
			{
				"remote" => eAssetBuildType.REMOTE,
				"remote_test" => eAssetBuildType.REMOTE_TEST,
				"editor_hosted" => eAssetBuildType.EDITOR_HOSTED,
				_ => eAssetBuildType.LOCAL
			};
			return assetBuildType;
		}
		
		public static string GetDistributeStr() => string.IsNullOrEmpty(GetArg(BuildArgType.DISTRIBUTE_TYPE)) ? "debug" : GetArg(BuildArgType.DISTRIBUTE_TYPE).ToLower();
		private static string GetEnvStr() => GetArg(BuildArgType.ENV)?.ToLower();
		private static bool GetArgIsDebug() => GetArg(BuildArgType.DISTRIBUTE_TYPE)?.ToLower() == "debug";
		private static bool GetArgIsAppCenterUpload() => GetArgIsTrue(BuildArgType.APP_CENTER_UPLOAD);
		private static bool GetArgIsCodeSign() => GetArgIsTrue(BuildArgType.CODESIGN_WINDOWS);
		// private static bool GetArgIsForceSingleInstance() => GetArgIsTrue(BuildArgType.FORCE_SINGLE_INSTANCE);
		private static bool   GetArgIsForceEnableAppInfo() => GetArgIsTrue(BuildArgType.FORCE_ENABLE_APPINFO);
		private static bool   GetArgIsForceEnableSentry()  => GetArgIsTrue(BuildArgType.FORCE_ENABLE_SENTRY);
		private static bool   GetArgIsCleanBuild()         => GetArgIsTrue(BuildArgType.CLEAN_BUILD);
		private static bool   GetArgEnableLogging()        => GetArgIsTrue(BuildArgType.ENABLE_LOGGING);
		public static  string GetAppId()                   => GetArg(BuildArgType.APP_ID)?.ToLower();
		private static string GetPackageTypeStr()          => GetArg(BuildArgType.PACKAGE_TYPE)?.ToLower();

		private static eHiveEnvType GetArgHiveEnv()
		{
			var hiveEnvTypeStr = GetArg(BuildArgType.HIVE_ENV)?.ToLower();
			var hiveEnvType = hiveEnvTypeStr switch
			{
				"sandbox" => eHiveEnvType.SANDBOX,
				"live"    => eHiveEnvType.LIVE,
				_         => eHiveEnvType.NOT
			};
			return hiveEnvType;
		}

		private static eBuildEnv GetArgEnv()
		{
			var env = eBuildEnv.DEV;
			switch (GetEnvStr())
			{
				case "qa":
					env = eBuildEnv.QA;
					break;
				case "staging":
					env = eBuildEnv.STAGING;
					break;
				case "dev_integration":
					env = eBuildEnv.DEV_INTEGRATION;
					break;
				case "release":
				case "production":
					env = eBuildEnv.PRODUCTION;
					break;
			}
			return env;
		}
		
		private static ePackageType GetPackageType()
		{
			var packageType = ePackageType.NOT;
			switch (GetPackageTypeStr())
			{
				case "zip":
					packageType = ePackageType.ZIP;
					break;
				case "msi":
					packageType = ePackageType.MSI;
					break;
				case "launcher":
					packageType = ePackageType.LAUNCHER;
					break;
				case "asset":
					packageType = ePackageType.ASSET;
					break;
				case "not":
					packageType = ePackageType.NOT;
					break;
			}
			return packageType;
		}
		
		private static bool GetArgIsTrue(BuildArgType type) => GetArg(type)?.ToLower() == "true";

		private static void PrintAllCommandLineArgs()
		{
			var args = System.Environment.GetCommandLineArgs();
			var log = "";
			foreach (var arg in args)
			{
				log += $"{arg}\n";
			}

			BuildLog.BuildScriptLog($" @ Command Line Arg : \n\n{log}\n\n");
		}
#endregion // Argument

#region Builder

		class Builder
		{
			private BuildInfo _info;

			private BuildInfo Info
			{
				get => _info;
				set => _info = value;
			}

			private bool _isEditorUpdate = true;
			private bool _isCleanBuild; // = false;

			public static Builder New(BuildTarget buildTarget) => new(buildTarget);

			private Builder(BuildTarget buildTarget)
			{
				BuildLog.BuildScriptLog("Builder called");
				Info = new BuildInfo();
				SetBuildTarget(buildTarget);
				_info.Version        = Application.version;
				_info.GitBuildBranch = GetArg(BuildArgType.GIT_BUILD_BRANCH);
			}

#region Builder - AssetBundle
			public eResult BuildAssetBundle()
			{
				// asset change after.
				BuildLog.BuildScriptLog("BuildAssetBundle called");
				if (!_isEditorUpdate)
					return eResult.FAILED;

				using var compositionRoot = C2VAddressablesEditorCompositionRoot.RequestInstance();
				if (compositionRoot?.ServicePack == null)
					return eResult.FAILED;

				var bundleEnvironment = _info.AssetBuildType == eAssetBuildType.LOCAL ? eEnvironment.LOCAL : eEnvironment.REMOTE;
				var buildService = compositionRoot.ServicePack.GetService<C2VAddressablesBuildService>();

				return _isCleanBuild ? buildService.CleanBuild(bundleEnvironment) : buildService.Build(bundleEnvironment);
			}


			public eResult UploadAssetBundle()
			{
				BuildLog.BuildScriptLog($"{nameof(UploadAssetBundle)} called");

				using var compositionRoot = C2VAddressablesEditorCompositionRoot.RequestInstance();
				if (compositionRoot?.ServicePack == null)
				{
					BuildLog.BuildScriptLogError($"{nameof(UploadAssetBundle)} servicePack null");
					return eResult.FAILED;
				}

				var buildService = compositionRoot.ServicePack.GetService<C2VAddressablesBuildService>();
				return buildService.Upload(eEnvironment.REMOTE);
			}


			public eResult UploadAssetBundleVersion()
			{
				BuildLog.BuildScriptLog("UploadAssetBundleVersion called");

				var appVersion  = _info.Version;
				var buildEnv    = _info.env;
				var hiveEnv     = _info.HiveEnvType;
				var buildTarget = C2VEditorPath.BuildTarget;

				if (_info.PackageType == ePackageType.ASSET)
				{
					var assetEntities = AssetBundleWebAPIBuilder.PostAssetList(buildEnv, hiveEnv, buildTarget);
					if (assetEntities == null)
					{
						// 한번 더 시도
						assetEntities = AssetBundleWebAPIBuilder.PostAssetList(buildEnv, hiveEnv, buildTarget);
						if (assetEntities == null)
							return eResult.FAILED;
					}

					appVersion = AssetBundleWebAPIHelper.GetLastAppVersion(assetEntities, _info.env);

					if (string.IsNullOrEmpty(appVersion))
						appVersion = _info.Version;
				}

				var remoteAssetBundlePath = $"{C2VPaths.RemoteAssetBundleUrl}/{C2VEditorPath.BuildEnvironment}/{buildTarget}/{C2VEditorPath.AssetBundleVersion}";

				var result = AssetBundleWebAPIBuilder.PutAsset(buildEnv, hiveEnv, remoteAssetBundlePath, buildTarget, C2VEditorPath.AssetBundleVersion, appVersion);
				if (!result)
				{
					// 한번 더 시도
					result = AssetBundleWebAPIBuilder.PutAsset(buildEnv, hiveEnv, remoteAssetBundlePath, buildTarget, C2VEditorPath.AssetBundleVersion, appVersion);
				}
				return result ? eResult.SUCCESS : eResult.FAILED;
			}
#endregion // Builder - AssetBundle

#region Builder - Get/Set

			public Builder SetBuildBefore(bool isEditorUpdate)
			{
				_isEditorUpdate = isEditorUpdate;
				_isCleanBuild = GetArgIsCleanBuild();
				BuildLog.BuildScriptLog("Builder::SetBuildBefore called");
				SetDebug(GetArgIsDebug());
				SetCopyPDBFilesOption(true);
				SetAppId(GetAppId());
				SetEnableLogging(GetArgEnableLogging());
				SetHiveEnv(GetArgHiveEnv());
				SetScriptingBackend(GetArgScriptingBackend());
				SetAssetBuildType(GetArgAssetBuildType());
				SetEnv(GetArgEnv());
				SetPackageType(GetPackageType());
				SetAdditionalIl2CppArgs();
				SetCacheServerInfo();
				UpdateDefineSymbolsBuilder(false);
				SetExecuteAfterBuild(false);
				SetIncrementalBuild();
				SetCodeSign(GetArgIsCodeSign());
				SetAppCenterUpload(GetArgIsAppCenterUpload());
				SetAppCenterComment(GetArg(BuildArgType.APP_CENTER_COMMENT));
				SetAppCenterAppName(GetArg(BuildArgType.APP_CENTER_APP_NAME));
				// SetForceSingleInstance(GetArgIsForceSingleInstance());	// argument dependency
				SetForceSingleInstance(!GetArgIsDebug());
				SetForceEnableAppInfo(GetArgIsForceEnableAppInfo());
				SetForceEnableSentry(GetArgIsForceEnableSentry());
				UpdateAppIcon();
				EditorUtility.ClearProgressBar();
				return this;
			}


			public void UpdateGitInfoAsync()
			{
				BuildLog.BuildScriptLog("UpdateGitInfo called");

				_info.GitCommitHash    = ProcessStart("git", "rev-parse --short HEAD");
				BuildLog.DetailBuildScriptLog($"GitCommitHashResult : {_info.GitCommitHash}");
				_info.GitRevisionCount = ProcessStart("git", "rev-list --count HEAD");
				BuildLog.DetailBuildScriptLog($"GitCommitHashResult : {_info.GitRevisionCount}");

				if (string.IsNullOrEmpty(_info.GitBuildBranch))
				{
					_info.GitBuildBranch = ProcessStart("git", "rev-parse --abbrev-ref HEAD");
					BuildLog.DetailBuildScriptLog($"GitBuildBranch : {_info.GitBuildBranch}");
				}


				UpdateVersionInfo();
			}

			private void UpdateVersionInfo()
			{
				BuildLog.BuildScriptLog("UpdateVersionInfo called");
				if (!_isEditorUpdate) return;
				// ex) 0.1.11630_d
				var versionStrArray = Application.version.Split(VERSION_SEPARATOR);
				versionStrArray[^1] = $"{_info.GitRevisionCount}_{AppInfoData.GetEnvShortStr(_info.env)}";
				var version = string.Join(VERSION_SEPARATOR, versionStrArray);
				BuildLog.DetailBuildScriptLog($"version : {version}");
				PlayerSettings.bundleVersion = version;
				_info.Version = version;
			}


			private string ProcessStart(string fileName, string commnad)
			{
				var startInfo = new ProcessStartInfo(fileName, commnad)
				{
					RedirectStandardOutput = true,
					UseShellExecute        = false,
					CreateNoWindow         = true
				};


				using var revisionProcess = new Process {StartInfo = startInfo};
				revisionProcess.Start();

				var outPut = revisionProcess.StandardOutput.ReadToEnd();

				revisionProcess.WaitForExit();

				return outPut.Trim();
			}


			public Builder SetAppInfo()
			{
				// UpdateGitInfoAsync call after.
				BuildLog.BuildScriptLog("SetAppInfo called");
				if (!_isEditorUpdate) return this;
				var data = new AppInfoData();
				data.AppName               = AppName;
				data.BuildTime             = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
				data.Version               = _info.Version;
				data.AssetBundleVersion    = _info.AssetBundleVersion;
				data.GitBuildBranch        = _info.GitBuildBranch;
				data.GitCommitHash         = _info.GitCommitHash;
				data.GitRevisionCount      = _info.GitRevisionCount;
				data.Environment           = _info.env;
				data.BuildTarget           = _info.BuildTarget.ToString();
				data.AssetBuildType        = _info.AssetBuildType;
				data.ScriptingBackend      = _info.ScriptingBackend.ToString();
				data.IsDebug               = _info.IsDebug;
				data.IsForceSingleInstance = _info.IsForceSingleInstance;
				data.IsForceEnableAppInfo  = _info.IsForceEnableAppInfo;
				data.IsForceEnableSentry   = _info.IsForceEnableSentry;
				data.PackageType           = _info.PackageType;
				data.AppId                 = _info.AppId;
				data.HiveEnvType           = _info.HiveEnvType;
				BuildLog.DetailBuildScriptLog($"{data.GetAppInfoDataLog()}");
				AppInfo.Instance.Init(data);
				AssetDatabase.Refresh();
				return this;
			}
			
			public Builder SetSentryOption()
			{
				// UpdateGitInfoAsync call after.
				BuildLog.BuildScriptLog("SetSentryOption called");
				if (!_isEditorUpdate) return this;
				var sentryOptions = AssetDatabase.LoadAssetAtPath<ScriptableSentryUnityOptions>("Assets/Resources/Sentry/SentryOptions.asset");
				if (sentryOptions == null)
				{
					BuildLog.BuildScriptLog("SetSentryOption Failed");
					return null;
				}

				sentryOptions.Debug                          = false;
				sentryOptions.Enabled                        = true;
				sentryOptions.EnvironmentOverride            = AppInfo.Instance.Data.Environment.ToString().ToLower(); //_info.env.ToString();
				sentryOptions.ReleaseOverride                = AppInfo.Instance.Data.GetVersionInfo();
				sentryOptions.Il2CppLineNumberSupportEnabled = (_info.ScriptingBackend == ScriptingImplementation.IL2CPP);
				sentryOptions.Dsn                            = "http://928c7984e3504c589b891e701928908f@34.64.227.27/3";
				BuildLog.BuildScriptLog($"sentryOptions.Debug : {sentryOptions.Debug}");
				BuildLog.BuildScriptLog($"sentryOptions.Enabled : {sentryOptions.Enabled}");
				BuildLog.BuildScriptLog($"sentryOptions.EnvironmentOverride : {sentryOptions.EnvironmentOverride}");
				BuildLog.BuildScriptLog($"sentryOptions.ReleaseOverride : {sentryOptions.ReleaseOverride}");
				BuildLog.BuildScriptLog($"sentryOptions.Il2CppLineNumberSupportEnabled : {sentryOptions.Il2CppLineNumberSupportEnabled}");
				EditorUtility.SetDirty(sentryOptions);
				return this;
			}
            			
			public Builder SetScriptingBackend(ScriptingImplementation backend)
			{
				BuildLog.BuildScriptLog("SetScriptingBackend called");
				BuildLog.BuildScriptLog($"backend : {backend}");
				_info.ScriptingBackend = backend;
				if (!_isEditorUpdate) return this;
				PlayerSettings.SetScriptingBackend(_info.BuildTargetGroup, backend);
				return this;
			}

			public Builder SetExecuteAfterBuild(bool execute)
			{
				BuildLog.BuildScriptLog("SetExecuteAfterBuild called");
				BuildLog.BuildScriptLog($"execute : {execute}");
				_info.ExecuteAfterBuild = execute;
				return this;
			}

			public Builder SetIncrementalBuild()
			{
				BuildLog.BuildScriptLog("SetIncrementalBuild called");
				if (!_isEditorUpdate) return this;
				bool enable = _info.ScriptingBackend == ScriptingImplementation.Mono2x;
				PlayerSettings.SetIncrementalIl2CppBuild(_info.BuildTargetGroup, enable);
				return this;
			}

			public Builder SetDebug(bool debug)
			{
				BuildLog.BuildScriptLog("SetDebug called");
				_info.IsDebug = debug;
				return this;
			}
			
			public Builder UpdateAppIcon()
			{
				BuildLog.BuildScriptLog("UpdateAppIcon called");
				if (!_isEditorUpdate) return this;
				var fileName = _info.env.ToString().ToLower() + (_info.IsDebug ? "_debug" : "");
				BuildLog.BuildScriptLog($"_info.env : {_info.env.ToString().ToLower()}");
				BuildLog.BuildScriptLog($"fileName : {fileName}");
				var sourceAppIconPath = Path.GetFullPath($"Config/{fileName}.png");
				var appIconPath = "/Project/Editor/AppIcon/appicon.png";
				var appIconAssetsPath = $"Assets{appIconPath}";
				BuildLog.BuildScriptLog($"sourceAppIconPath : {sourceAppIconPath}");
				BuildLog.BuildScriptLog($"appIconPath : {appIconPath}");
				BuildLog.BuildScriptLog($"appIconAssetsPath : {appIconAssetsPath}");
				File.Copy(sourceAppIconPath, $"{DirectoryUtil.DataRoot}{appIconPath}", true);
				AssetDatabase.Refresh();
				AssetDatabase.ImportAsset(appIconAssetsPath);
				var iconSize = PlayerSettings.GetIconsForTargetGroup(_info.BuildTargetGroup);
				var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(appIconAssetsPath);
				var icons = new Texture2D[iconSize.Length];
				for (var i = 0; i < icons.Length; ++i)
				{
					icons[i] = icon;
				}
				PlayerSettings.SetIconsForTargetGroup(_info.BuildTargetGroup, icons);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				return this;
			}
			
			public Builder SetAppCenterUpload(bool upload)
			{
				BuildLog.BuildScriptLog("SetAppCenterUpload called");
				BuildLog.BuildScriptLog($"upload : {upload}");
				_info.AppCenterUpload = upload;
				return this;
			}

			public Builder SetAppCenterComment(string comment)
			{
				BuildLog.BuildScriptLog("SetAppCenterComment called");
				BuildLog.BuildScriptLog($"comment : {comment}");
				if(string.IsNullOrEmpty(comment) || comment.ToLower().Equals("daily_build"))
				{
					// ex) Windows Mono (Debug)
					_info.AppCenterComment = $"{GetBuildTargetStr()} {_info.ScriptingBackend.ToString()} ({GetDistributeStr()})";
				}
				else
				{
					_info.AppCenterComment = comment;
				}

				BuildLog.DetailBuildScriptLog($"_info.AppCenterComment : {_info.AppCenterComment}");
				return this;
			}
			

			public Builder SetAppCenterAppName(string appCenterAppName)
			{
				BuildLog.BuildScriptLog("SetAppCenterAppName called");
				BuildLog.BuildScriptLog($"appCenterAppName : {appCenterAppName}");
				if(string.IsNullOrEmpty(appCenterAppName))
				{
					appCenterAppName = "Com2Verse";
					switch (_info.BuildTarget)
					{
						case BuildTarget.StandaloneWindows:
						case BuildTarget.StandaloneWindows64:
						case BuildTarget.StandaloneOSX:
							appCenterAppName += $"/{GetBuildTargetStr()}";
							break;
						default:
							BuildLog.DetailBuildScriptLog("appcenter not setting.");
							break;
					}
					
					if (_info.env == eBuildEnv.DEV)
					{
						// "Com2Verse/Windows_Dev"
						appCenterAppName += "_Dev";
					
						if (_info.IsDebug)
						{
							// "Com2Verse/Windows_Dev_Debug"
							appCenterAppName += "_Debug";
						}
					}
					else if (_info.env == eBuildEnv.QA)
					{
						// "Com2Verse/Windows_QA"
						appCenterAppName += "_QA";
					
						if (_info.IsDebug)
						{
							// "Com2Verse/Windows_QA_Debug"
							appCenterAppName += "_Debug";
						}
					}
					else if (_info.env == eBuildEnv.STAGING)
					{
						// "Com2Verse/Windows_Staging"
						appCenterAppName += "_Staging";
					
						if (_info.IsDebug)
						{
							// "Com2Verse/Windows_Staging_Debug"
							appCenterAppName += "_Debug";
						}
					}
					else if (_info.env == eBuildEnv.DEV_INTEGRATION)
					{
						appCenterAppName += "_Dev_Integration";

						if (_info.IsDebug)
						{
							appCenterAppName += "_Debug";
						}
					}
					else if (_info.env == eBuildEnv.PRODUCTION)
					{
						// "Com2Verse/Windows_Production"
						appCenterAppName += "_Production";
						if (_info.IsDebug)
						{
							// "Com2Verse/Windows_Production_Debug"
							appCenterAppName += "_Debug";
						}
					}
					
					// il2cpp - test
					// if (_info.ScriptingBackend == ScriptingImplementation.IL2CPP)
					// {
					// 	// "Com2Verse/Windows_IL2CPP"
					// 	appCenterAppName += "_IL2CPP";
					// }
				}
				_info.AppCenterAppName = appCenterAppName;
				BuildLog.DetailBuildScriptLog($"_info.AppCenterAppName : {_info.AppCenterAppName}");
				return this;
			}
			
			public Builder SetCodeSign(bool enable)
			{
				BuildLog.BuildScriptLog("SetCodeSign called");
				BuildLog.BuildScriptLog($"CodeSign : {enable}");
				_info.EnableCodeSign = enable;
				return this;
			}
			
			private void SetBuildTarget(BuildTarget buildTarget)
			{
				BuildLog.BuildScriptLog("SetBuildTarget called");
				BuildLog.BuildScriptLog($"buildTarget : {buildTarget}");
				_info.BuildTarget = buildTarget;
				_info.BuildTargetGroup = GetBuildTargetGroup(buildTarget);
			}
			
			public Builder SetAssetBuildType(eAssetBuildType type = eAssetBuildType.LOCAL)
			{
				BuildLog.BuildScriptLog("SetAssetBuildType called");
				BuildLog.BuildScriptLog($"AssetBuildType : {type}");
				_info.AssetBuildType = type;
				return this;
			}
			
			public Builder SetEnv(eBuildEnv buildEnv = eBuildEnv.DEV)
			{
				BuildLog.BuildScriptLog("SetEnv called");
				BuildLog.BuildScriptLog($"Env : {buildEnv}");
				_info.env = buildEnv;
				return this;
			}

			public Builder SetPackageType(ePackageType packageType = ePackageType.NOT)
			{
				BuildLog.BuildScriptLog("SetPackageType called");
				BuildLog.BuildScriptLog($"PackageType : {packageType}");
				_info.PackageType = packageType;
				return this;
			}
			
			public Builder SetForceSingleInstance(bool isForceSingleInstance)
			{
				BuildLog.BuildScriptLog("SetForceSingleInstance called");
				BuildLog.BuildScriptLog($"isForceSingleInstance : {isForceSingleInstance}");
				_info.IsForceSingleInstance = isForceSingleInstance;
				if (!_isEditorUpdate) return this;
				PlayerSettings.forceSingleInstance = isForceSingleInstance;
				return this;
			}

			public Builder SetForceEnableAppInfo(bool isForceEnableAppInfo)
			{
				BuildLog.BuildScriptLog("SetForceEnableAppInfo called");
				BuildLog.BuildScriptLog($"isForceEnableAppInfo : {isForceEnableAppInfo}");
				_info.IsForceEnableAppInfo = isForceEnableAppInfo;
				return this;
			}
			
			public Builder SetForceEnableSentry(bool isForceEnableSentry)
			{
				BuildLog.BuildScriptLog("SetForceEnableSentry called");
				BuildLog.BuildScriptLog($"isForceEnableSentry : {isForceEnableSentry}");
				_info.IsForceEnableSentry = isForceEnableSentry;
				return this;
			}


			public Builder SetHiveEnv(eHiveEnvType hiveEnv)
			{
				BuildLog.BuildScriptLog("SetHiveEnv called");
				BuildLog.BuildScriptLog($"HiveEnv : {hiveEnv}");

				_info.HiveEnvType = hiveEnv;
				return this;
			}

			public Builder SetEnableLogging(bool enable)
			{
				BuildLog.BuildScriptLog("SetEnableLogging called");
				BuildLog.BuildScriptLog($"enable : {enable}");

				_info.EnableLogging = enable;
				return this;
			}

			public Builder SetAppId(string appId)
			{
				BuildLog.BuildScriptLog("SetAppId called");
				BuildLog.BuildScriptLog($"appId : {appId}");
				_info.AppId = appId;
				return this;
			}


			public Builder SetCopyPDBFilesOption(bool enabled)
			{
				BuildLog.BuildScriptLog("SetCopyPDBFilesOption called");

				UnityEditor.EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "true");
				return this;
			}
			
			public static void SetAdditionalIl2CppArgsStaticFunc(ScriptingImplementation backend)
			{
				BuildLog.BuildScriptLog("SetAdditionalIl2CppArgsStaticFunc called");
				// if (backend == ScriptingImplementation.IL2CPP)
				{
					// https://forum.unity.com/threads/neverending-il2cpp-build.855235/
					// var args = "--compiler-flags=\"/d2ssa-cfg-jt-\"";

					// https://docs.unity3d.com/kr/2022.1/Manual/handling-IL2CPP-additional-args.html
					// var args = "--compiler-flags=\"d2ssa-cfg-jt\"";
					// -> 'd2ssa-cfg-jt': No such file or directory
					
					// il2cpp optimization option
					// var args = "--compiler-flags=\"-d2ssa-patterns-all-\"";
					
					// il2cpp option
					var args = "--compiler-flags=\"-d2ssa-cfg-jt-\"";
					BuildLog.BuildScriptLog($"Setting Additional IL2CPP Args = {args}");
					PlayerSettings.SetAdditionalIl2CppArgs(args);
					EditorUtility.ClearProgressBar();
				}
			}

			public Builder SetAdditionalIl2CppArgs()
			{
				BuildLog.BuildScriptLog("SetAdditionalIl2CppArgs called");
				if (!_isEditorUpdate) return this;
				SetAdditionalIl2CppArgsStaticFunc(_info.ScriptingBackend);
				return this;
			}

			public Builder SetCacheServerInfo()
			{
				BuildLog.BuildScriptLog("SetCacheServerInfo called");
				if (!_isEditorUpdate) return this;
				EditorSettings.cacheServerMode = CacheServerMode.Enabled;
				var cacheServerUrl = "10.36.0.87:80";
				switch (_info.BuildTarget)
				{
					case BuildTarget.StandaloneWindows:
					case BuildTarget.StandaloneWindows64:
					default:
						cacheServerUrl = "10.36.0.87:80";
						break;
					
					case BuildTarget.StandaloneOSX:
						cacheServerUrl = "10.36.0.88:80";
						break;
				}
				EditorSettings.cacheServerEndpoint = cacheServerUrl;
				BuildLog.DetailBuildScriptLog($"EditorSettings.cacheServerMode : {EditorSettings.cacheServerMode}");
				BuildLog.DetailBuildScriptLog($"EditorSettings.cacheServerEndpoint : {EditorSettings.cacheServerEndpoint}");
				return this;
			}
			
			private BuildOptions GetBuildOptions() => Info.IsDebug
				? BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.DetailedBuildReport
				: BuildOptions.None;

			private string GetBuildTargetStr() => _info.BuildTarget switch
			{
				BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64 => OperatingSystemFamily.Windows.ToString(),
				BuildTarget.Android => "Android",
				BuildTarget.iOS => "iOS",
				BuildTarget.StandaloneOSX => OperatingSystemFamily.MacOSX.ToString(),
				_ => "Unknown",
			};
			
			private string GetBuildTargetShortStr() => _info.BuildTarget switch
			{
				BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64 => "win",
				BuildTarget.Android => "aos",
				BuildTarget.iOS => "ios",
				BuildTarget.StandaloneOSX => "macos",
				_ => "unknown",
			};
			
#endregion // Builder - Get/Set

#region Builder - Execute Build

			public async UniTask BuildAsync(bool editorExit)
			{
				BuildLog.BuildScriptLog("BuildAsync called");
				BuildLog.BuildScriptLog($"editorExit : {editorExit}");
				PrintAllCommandLineArgs();
				BuildLog.BuildScriptLog(GetAppCenterCommentString(Info));
				BuildReport report = null;
				switch (Info.BuildTarget)
				{
					case BuildTarget.StandaloneWindows:
					case BuildTarget.StandaloneWindows64:
						report = await BuildWindows64Async();
						BuildLog.BuildScriptLog("BuildAsync - BuildWindows64Async after");
						break;
					case BuildTarget.StandaloneOSX:
						// report = await BuildOsxAsync();
						report = BuildOsxAsync();
						BuildLog.BuildScriptLog("BuildAsync - BuildOsxAsync after");
						break;
					default:
						break;
				}

				if (report)
				{
					BuildReportExporter.ExportToJson(report);
					BuildLog.BuildScriptLog("BuildAsync - BuildReportExporter.ExportToHtml after");
				}

				if (editorExit)
				{
					BuildLog.BuildScriptLog("BuildAsync - EditorApplication.Exit");
					EditorApplication.Exit((!report || report.summary.result != BuildResult.Succeeded) ? 1 : 0);
				}
			}

			private async UniTask<BuildReport> BuildWindows64Async()
			{
				BuildLog.BuildScriptLog("BuildWindows64Async called");
				var scenes = GetAllSceneNames();
				_info.BuildTime = DateTime.Now;
				// ex) 221115_112048_c2v_win_0.1.11630_d_debug
				var zipFileName = $"{ToFileFormat(_info.BuildTime)}_c2v_{GetBuildTargetShortStr()}_{_info.Version}_{GetDistributeStr()}";
				_executableDir = Path.Combine(BuildPath, "Windows64", GetDistributeStr());
				var executableFileName = GetExecutableFileName();
				var executablePath = Path.Combine(_executableDir, $"{executableFileName}.exe");
				DeleteDirectory(BuildPath);
				BuildLog.DetailBuildScriptLog($"zipFileName : {zipFileName}");
				BuildLog.DetailBuildScriptLog($"BuildPath : {BuildPath}");
				BuildLog.DetailBuildScriptLog($"executableDir : {_executableDir}");
				BuildLog.DetailBuildScriptLog($"executablePath : {executablePath}");
				var report = BuildPipeline.BuildPlayer(scenes, executablePath, Info.BuildTarget, GetBuildOptions());
				EditorUtility.ClearProgressBar();
				if (report != null && report.summary.result == BuildResult.Succeeded)
				{
					BuildLog.BuildScriptLog("BuildWindows64Async - BuildPlayer - Succeeded");
					if (_info.EnableCodeSign)
					{
						BuildLog.BuildScriptLog("BuildWindows64Async - CodeSign");
						await CodeSignWindows(executablePath);
						BuildLog.BuildScriptLog("BuildWindows64Async - CodeSignWindows after");
					}

					if (_info.AppCenterUpload)
					{
						BuildLog.BuildScriptLog("BuildWindows64Async - AppCenterUpload");
						ZipWindowsBuild(_executableDir, executableFileName, zipFileName);
						BuildLog.BuildScriptLog("BuildWindows64Async - ZipWindowsBuild after");
						await UploadAppcenterAsync(_info);
						BuildLog.BuildScriptLog("BuildWindows64Async - UploadAppcenterAsync after");
					}
					else if (Info.ExecuteAfterBuild)
					{	
						BuildLog.BuildScriptLog("BuildWindows64Async - ExecuteAfterBuild");
						Process proc = new Process();
						proc.StartInfo.FileName = executablePath;
						proc.Start();
						BuildLog.BuildScriptLog("BuildWindows64Async - Process.Start After");
					}
				}
				else
				{
					BuildLog.BuildScriptLog("BuildWindows64Async - BuildPlayer - Failed");
				}

				return report;
			}
			
			private BuildReport BuildOsxAsync()
			// private async UniTask<BuildReport> BuildOsxAsync()
			{
				//TODO: Mr.Song - OSX
				BuildLog.BuildScriptLog("BuildOsxAsync called");
				var scenes = GetAllSceneNames();
				_info.BuildTime = DateTime.Now;
				// ex) 221115_112048_c2v_win_0.1.11630_d_debug
				var zipFileName = $"{ToFileFormat(_info.BuildTime)}_c2v_{GetBuildTargetShortStr()}_{_info.Version}_{GetDistributeStr()}";
				_executableDir = Path.Combine(BuildPath, "OSX", GetDistributeStr());
				var executableFileName = GetExecutableFileName();
				var executablePath = Path.Combine(_executableDir, $"{executableFileName}.exe");
				DeleteDirectory(BuildPath);
				BuildLog.DetailBuildScriptLog($"zipFileName : {zipFileName}");
				BuildLog.DetailBuildScriptLog($"BuildPath : {BuildPath}");
				BuildLog.DetailBuildScriptLog($"executableDir : {_executableDir}");
				BuildLog.DetailBuildScriptLog($"executablePath : {executablePath}");
				var report = BuildPipeline.BuildPlayer(scenes, executablePath, Info.BuildTarget, GetBuildOptions());
				EditorUtility.ClearProgressBar();
				if (report != null && report.summary.result == BuildResult.Succeeded)
				{
					BuildLog.BuildScriptLog("BuildWindows64Async - BuildPlayer - Succeeded");
					//TODO: Mr.Song - OSX
					// if (_info.EnableCodeSign)
					// {
					// 	BuildScriptLog("BuildWindows64Async - CodeSign");
					// 	await CodeSignWindows(executablePath);
					// 	BuildScriptLog("BuildWindows64Async - CodeSignWindows after");
					// }
					//
					// if (_info.AppCenterUpload)
					// {
					// 	BuildScriptLog("BuildWindows64Async - AppCenterUpload");
					// 	ZipWindowsBuild(_executableDir, executableFileName, zipFileName);
					// 	BuildScriptLog("BuildWindows64Async - ZipWindowsBuild after");
					// 	await UploadAppcenterAsync(_info);
					// 	BuildScriptLog("BuildWindows64Async - UploadAppcenterAsync after");
					// }
					if (Info.ExecuteAfterBuild)
					{
						BuildLog.BuildScriptLog("BuildWindows64Async - ExecuteAfterBuild");
						Process proc = new Process();
						proc.StartInfo.FileName = executablePath;
						proc.Start();
						BuildLog.BuildScriptLog("BuildWindows64Async - Process.Start After");
					}
				}
				else
				{
					BuildLog.BuildScriptLog("BuildWindows64Async - BuildPlayer - Failed");
				}
				
				return report;
			}
			
			private async UniTask CodeSignWindows(string executablePath)
			{
				BuildLog.BuildScriptLog("CodeSignWindows called");
				var certExecutablePath = DirectoryUtil.GetDataPath("../JenkinsScripts", "CodeSign_Windows.bat");
				if (GetArgIsCodeSign())
				{
					var result = await _process.RunAsync(certExecutablePath, executablePath);
					BuildLog.BuildScriptLog(result.Success ? "CodeSign Success" : $"CodeSign Failed\n{result.ErrorMessage}");
				}
			}

			private void ZipWindowsBuild(string executableDir, string appName, string fileName)
			{
				BuildLog.BuildScriptLog("ZipWindowsBuild called");
				DeleteDirectory(Path.Combine(executableDir, $"{appName}_BackUpThisFolder_ButDontShipItWithYourGame"));
				DeleteDirectory(Path.Combine(executableDir, $"{appName}_BurstDebugInformation_DoNotShip"));

				var zipFilePath = Zip(executableDir, $"{fileName}.zip");
				_info.BuildFilePath = zipFilePath;
			}

			private string GetExecutableFileName()
			{
				// MetaversePlatform_Mono2x_Dev_Debug.exe
				// MetaversePlatform_Mono2x_Dev.exe
				// MetaversePlatform_Mono2x_QA_Debug.exe
				// MetaversePlatform_Mono2x_QA.exe
				// var fileName = $"{AppName}_{_info.ScriptingBackend.ToString()}_{_info.env.ToString()}";
				// if(_info.IsDebug)
				// {
				// 	fileName += "_Debug";
				// }
				// return fileName;
				
				// MetaversePlatform
				return AppName;
			}
#endregion // Builder - Execute Build

#region DefineSymbols

			public Builder UpdateDefineSymbolsBuilder(bool isLocalBuild)
			{
				if (!_isEditorUpdate) return this;
				BuildLog.BuildScriptLog("UpdateDefineSymbolsBuilder called");
				DefineSymbols.UpdateDefineSymbols(isLocalBuild, _info.BuildTargetGroup, _info.IsDebug, _info.EnableLogging, _info.IsForceEnableAppInfo, _info.ScriptingBackend, _info.env);
				return this;
			}

#endregion // DefineSymbols
		}

#endregion // Builder

#region App Center

		static async UniTask UploadAppcenterAsync(BuildInfo info)
		{
			//https://jira.com2us.com/jira/browse/CUHC2V-979
			//TODO: Mr.song - appcenter upload 외부로 이동
			// - Unity build script 에서 300 sec를 넘어 build fail issue 발생으로 인해 await 하지 않도록 임시 처리.
			BuildLog.BuildScriptLog("UploadAppcenterAsync called");
			if (!await ValidateAppCenterAsync(info))
			{
				BuildLog.BuildScriptLogError("Validate AppCenter failed...");
				return;
			}

			if (string.IsNullOrWhiteSpace(_appcenterExecutable))
			{
				BuildLog.BuildScriptLogError("Cannot found appcenter executable...");
				return;
			}

			BuildLog.BuildScriptLog($"_appcenterExecutable : {_appcenterExecutable}");
			if (string.IsNullOrWhiteSpace(info.BuildFilePath) || !File.Exists(info.BuildFilePath))
			{
				BuildLog.BuildScriptLogError($"Invalid BuildFilePath : [{info.BuildFilePath}]");
				return;
			}

			var appCenterDistributeParam = await GetAppCenterDistributeParamAsync(info);
			BuildLog.BuildScriptLog($"appCenterDistributeParam : {appCenterDistributeParam}");
			if (_process != null) await _process.RunAsync(_appcenterExecutable, appCenterDistributeParam);
			// var result = await _process.RunAsync(_appcenterExecutable, appCenterDistributeParam);
			// DetailBuildScriptLog($"app center upload result : {result.Success}");
			// DetailBuildScriptLog($"result.OutputMessage : {result.OutputMessage}");
			// DetailBuildScriptLog($"result.ErrorMessage : {result.ErrorMessage}");
			// DetailBuildScriptLog($"appCenterDistributeParam : {appCenterDistributeParam}");
			EditorUtility.ClearProgressBar();

			// var appCenterCommentFilePath = GetAppCenterCommentFilePath();
			// DeleteFile(appCenterCommentFilePath);
		}

#region App Center - Validation

		static async UniTask<bool> ValidateAppCenterAsync(BuildInfo info)
		{
			BuildLog.BuildScriptLog("ValidateAppCenterAsync called");
			switch (info.BuildTarget)
			{
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					return await ValidateAppCenterWindows();
				//TODO: Mr.Song - OSX
				// case BuildTarget.StandaloneOSX:
				// 	break;
				default:
					break;
			}

			return true;
		}

		static async UniTask<bool> ValidateAppCenterWindows()
		{
			BuildLog.BuildScriptLog("ValidateAppCenterWindows called");
			try
			{
				// Validate npm
				var npmExecutable = FindExecutablePath("npm.cmd");
				var npmPrefixResult = await _process.RunAsync(npmExecutable, "config", "get", "prefix");
				if (npmPrefixResult.Success)
				{
					BuildLog.BuildScriptLog("npmPrefixResult.Success case");
					// Validate appcenter-cli
					var npmPrefix = npmPrefixResult.OutputMessage.TrimEnd();
					_appcenterExecutable = Path.Combine(npmPrefix, "appcenter.cmd");
					var appCenterVersionResult = await _process.RunAsync(_appcenterExecutable, "--version");
					if (appCenterVersionResult.Success)
					{
						BuildLog.BuildScriptLog("appCenterVersionResult.Success case");
						EditorUtility.ClearProgressBar();
						return true;
					}
					else
					{
						BuildLog.BuildScriptLog("appCenterVersionResult.Fail case");
						BuildLog.BuildScriptLog($"appCenterVersionResult.ErrorMessage : {appCenterVersionResult.ErrorMessage}");
						BuildLog.BuildScriptLog(
							$"appCenterVersionResult.OutputMessage : {appCenterVersionResult.OutputMessage}");
					}

					EditorUtility.ClearProgressBar();
					return false;
				}
				else
				{
					BuildLog.BuildScriptLog("npmPrefixResult.Fail case");
					BuildLog.BuildScriptLog($"npmPrefixResult.ErrorMessage : {npmPrefixResult.ErrorMessage}");
					BuildLog.BuildScriptLog($"npmPrefixResult.OutputMessage : {npmPrefixResult.OutputMessage}");
				}

				EditorUtility.ClearProgressBar();
				return false;
			}
			catch (Exception e)
			{
				BuildLog.BuildScriptLogError(e.Message);
				EditorUtility.ClearProgressBar();
				return false;
			}
		}

#endregion // App Center - Validation

#region App Center - Parameter

		static string GetAppCenterCommentFilePath() => Path.Combine(Path.GetTempPath(), ".AppCenterComment.md");

		static string GetAppCenterCommentString(BuildInfo info)
		{
			var appInfoData = AppInfo.Instance.Data;
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"# {AppName} v{info.Version} ({GetDistributeStr()})");
			sb.AppendLine($"## {info.AppCenterComment}");
			sb.AppendLine($"### {ToAppCenterCommentFormat(info.BuildTime)}");
			sb.AppendLine("### App Info");
			sb.AppendLine($"- {info.ToString()}");
			sb.AppendLine($"- Build Target : {info.BuildTarget.ToString()}");
			sb.AppendLine($"- Build Group : {info.BuildTargetGroup.ToString()}");
			sb.AppendLine($"- Environment : {info.env}");
			sb.AppendLine($"- Scripting Backend : {info.ScriptingBackend.ToString()}");
			sb.AppendLine($"- Asset Build Type : {info.AssetBuildType.ToString()}");
			sb.AppendLine($"- Asset Bundle Version : {info.AssetBundleVersion}");
			sb.AppendLine($"- IsDebug : {GetOX(info.IsDebug)}");
			sb.AppendLine($"- IsForceSingleInstance : {GetOX(info.IsForceSingleInstance)}");
			sb.AppendLine($"- IsForceEnableAppInfo : {GetOX(info.IsForceEnableAppInfo)}");
			sb.AppendLine($"- IsForceEnableSentry : {GetOX(info.IsForceEnableSentry)}");
			sb.AppendLine("### Git Info");
			sb.AppendLine($"- Branch : {appInfoData.GitBuildBranch}");
			sb.AppendLine($"- HASH : {appInfoData.GetCommitHashLink()}");
			sb.AppendLine($"- Revision : {appInfoData.GitRevisionCount}");
			AppendPlatformSpecificComment(sb, info);
			return sb.ToString();
		}

		private static void AppendPlatformSpecificComment(StringBuilder sb, BuildInfo info)
		{
			switch (info.BuildTarget)
			{
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					AppendStandaloneWindowsComment(sb, info);
					break;
				//TODO: Mr.Song - OSX
				// case BuildTarget.StandaloneOSX:
				// 	break;
				default:
					break;
			}
		}

		private static void AppendStandaloneWindowsComment(StringBuilder sb, BuildInfo info)
		{
			sb.AppendLine("### Windows Info");
			sb.AppendLine($"- CodeSign : {GetOX(info.EnableCodeSign)}");
		}

		static async UniTask<bool> WriteAppCenterCommentToFileAsync(BuildInfo info)
		{
			try
			{
				var tempFilePath = GetAppCenterCommentFilePath();
				DeleteFile(tempFilePath);
				await using var fs = new FileStream(tempFilePath, FileMode.CreateNew);
				await using var sw = new StreamWriter(fs);
				await sw.WriteAsync(GetAppCenterCommentString(info))!;
				return true;
			}
			catch (Exception e)
			{
				BuildLog.BuildScriptLogError(e.Message);
				return false;
			}
		}
		
		static async UniTask<string> GetAppCenterDistributeParamAsync(BuildInfo info)
		{
			await WriteAppCenterCommentToFileAsync(info);
			switch (info.BuildTarget)
			{
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
				default:
					return $"distribute release -a {info.AppCenterAppName} -f \"{info.BuildFilePath}\" --group {GetArg(BuildArgType.APP_CENTER_DISTRIBUTE_GROUP)} --build-version {info.Version} -R {GetAppCenterCommentFilePath()}";
				//TODO: Mr.Song - OSX
				// case BuildTarget.StandaloneOSX:
				// 	break;
			}
		}

#endregion // App Center - Parameter

#endregion // App Center

#region EditorCoroutine
		// FIXME : groovy에서 ExecuteMethod 실행시 async, await가 되지 않아 해당 클래스로 수정
		private class EditorCoroutine
		{
			private IEnumerator routine;

			private EditorCoroutine(IEnumerator routine)
			{
				this.routine = routine;
			}

			public static EditorCoroutine Start(IEnumerator routine)
			{
				EditorCoroutine coroutine = new EditorCoroutine(routine);
				coroutine.Start();
				return coroutine;
			}

			private void Start()
			{
				UnityEditor.EditorApplication.update += Update;
			}

			public void Stop()
			{
				UnityEditor.EditorApplication.update -= Update;
			}

			private void Update()
			{
				if (!routine.MoveNext())
				{
					Stop();
				}
			}
		}
#endregion EditorCoroutine
#region Util
		static void DeleteFile(string path)
		{
			BuildLog.DetailBuildScriptLog($"DeleteFile : {path}");
			if (File.Exists(path))
			{
				BuildLog.DetailBuildScriptLog($"File.Exists : {path}");
				File.Delete(path);
			}
			else
				BuildLog.DetailBuildScriptLog($"File not exists : {path}");
		}

		static void DeleteDirectory(string path)
		{
			BuildLog.DetailBuildScriptLog($"DeleteDirectory : {path}");
			if (Directory.Exists(path))
			{
				BuildLog.DetailBuildScriptLog($"Directory.Exists : {path}");
				Directory.Delete(path, true);
			}
			else
				BuildLog.DetailBuildScriptLog($"Directory not exists : {path}");
		}

		static string Zip(string sourceDir, string outputFile)
		{
			var zipFilePath = Path.Combine(sourceDir, "..", outputFile);
			DeleteFile(zipFilePath);
			ZipFile.CreateFromDirectory(sourceDir, zipFilePath, System.IO.Compression.CompressionLevel.Optimal, true);
			return zipFilePath;
		}

		static string FindExecutablePath(string executable)
		{
			var findKeys = new string[]
			{
				"Path",
				"PATH",
			};

			List<string> paths = new List<string>();
			CollectEnvironmentVariables(paths, EnvironmentVariableTarget.Machine, findKeys);
			CollectEnvironmentVariables(paths, EnvironmentVariableTarget.Process, findKeys);
			CollectEnvironmentVariables(paths, EnvironmentVariableTarget.User, findKeys);
			Environment.SetEnvironmentVariable("PATH", paths.Distinct().Aggregate((l, r) => $"{l};{r}"));

			var findPath = paths.Distinct().FirstOrDefault(path =>
			{
				var fullPath = Path.Combine(path, executable);
				return File.Exists(fullPath);
			});

			return string.IsNullOrWhiteSpace(findPath) ? executable : Path.Combine(findPath, executable);

			void CollectEnvironmentVariables(List<string> list, EnvironmentVariableTarget target,
				params string[] findKeys)
			{
				var variables = Environment.GetEnvironmentVariables(target);
				foreach (var key in findKeys)
				{
					if (variables[key] is string value)
						list.AddRange(value.Split(";"));
				}
			}
		}
		
		public static string ToFileFormat(DateTime dateTime) => dateTime.ToString("yyMMdd_HHmmss");
		private static string ToAppCenterCommentFormat(DateTime dateTime) => dateTime.ToString("yy/MM/dd HH:mm:ss");

		private static BuildTargetGroup GetBuildTargetGroup(BuildTarget buildTarget) => buildTarget switch
		{
			BuildTarget.StandaloneWindows | BuildTarget.StandaloneWindows64 => BuildTargetGroup.Standalone,
			BuildTarget.StandaloneOSX => BuildTargetGroup.Standalone,
			BuildTarget.iOS => BuildTargetGroup.iOS,
			BuildTarget.Android => BuildTargetGroup.Android,
			_ => BuildTargetGroup.Standalone,
		};

		private static BuildTarget GetBuildTargetFromString(string buildTargetStr)
		{
			if (Enum.TryParse(buildTargetStr, out BuildTarget buildTarget)) { return buildTarget; }
			return BuildTarget.StandaloneWindows64;
		}
		
		private static ScriptingImplementation GetScriptingBackendFromString(string scriptingBackendStr)
		{
			if (Enum.TryParse(scriptingBackendStr, out ScriptingImplementation scriptingBackend)) { return scriptingBackend; }
			return ScriptingImplementation.Mono2x;
		}
#endregion // Util
	}
}

#endif // UNITY_EDITOR
