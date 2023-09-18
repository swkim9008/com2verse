using System;
using System.IO;
using Com2Verse.BuildHelper;
using Com2Verse.Serialization;
using UnityEngine;
using static Com2Verse.AssetSystem.AssetSystemManager;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR


[Serializable]
public struct AppInfoData
{
	public string          AppName;
	public string          BuildTime;
	public string          Version;
	public string          AssetBundleVersion;
	public string          GitBuildBranch;
	public string          GitCommitHash;
	public string          GitRevisionCount;
	public eBuildEnv       Environment;
	public string          BuildTarget;
	public string          AppId;
	public eAssetBuildType AssetBuildType;
	public string          ScriptingBackend;
	public bool            IsDebug;
	public bool            IsForceSingleInstance;
	public bool            IsForceEnableAppInfo;
	public bool            IsForceEnableSentry;
	public ePackageType    PackageType;
	public eHiveEnvType    HiveEnvType;

	public static AppInfoData Default => new AppInfoData
	{
		AppName               = Application.productName,
		BuildTime             = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
		Version               = Application.version,
		AssetBundleVersion    = string.Empty,
		GitBuildBranch        = string.Empty,
		GitCommitHash         = string.Empty,
		GitRevisionCount      = string.Empty,
		AppId                 = string.Empty,
		Environment           = eBuildEnv.DEV,
		BuildTarget           = "StandaloneWindows64", // BuildTarget.StandaloneWindows64
		AssetBuildType        = eAssetBuildType.LOCAL,
		ScriptingBackend      = "Mono2x", // ScriptingImplementation.Mono2x
		IsDebug               = true,
		IsForceSingleInstance = false,
		IsForceEnableAppInfo  = false,
		IsForceEnableSentry   = false,
		PackageType           = ePackageType.NOT,
		HiveEnvType           = eHiveEnvType.NOT
	};

	public string GetCommitHashLink() => $"[{GitCommitHash}]({GetCommitHashURL()})";
	public string GetCommitHashURL()  => $"https://meta-bitbucket.com2us.com/projects/C2VERSE/repos/c2vclient/commits/{GitCommitHash}";
	public static string GetEnvShortStr(eBuildEnv ev)
	{
		switch (ev)
		{
			case eBuildEnv.QA:              return "q";
			case eBuildEnv.STAGING:         return "s";
			case eBuildEnv.PRODUCTION:      return "p";
			case eBuildEnv.DEV:             return "d";
			case eBuildEnv.DEV_INTEGRATION: return "di";
			default:                        return "d";
		}
	}

	public string GetVersionInfo()
	{
		// ex) 0.1.11630_d (4449ed86f)
		var versionInfo = Application.version;
		if (!string.IsNullOrEmpty(GitCommitHash))
		{
			versionInfo += $" ({GitCommitHash})";
		}

		return versionInfo;
	}

	public string GetAppInfoDataLog()
	{
		var log = "@ AppInfoData\n\n";
		log += $" * info data\n";
		log += $"  + AppName : {AppName}\n";
		log += $"  + BuildTime : {BuildTime}\n";
		log += $"  + Version : {Version}\n";
		log += $"  + AssetBundleVersion : {AssetBundleVersion}\n";
		log += $"  + GitBuildBranch : {GitBuildBranch}\n";
		log += $"  + GitCommitHash : {GitCommitHash}\n";
		log += $"  + GitRevisionCount : {GitRevisionCount}\n";
		log += $"  + Environment : {Environment}\n";
		log += $"  + BuildTarget : {BuildTarget}\n";
		log += $"  + AssetBuildType : {AssetBuildType}\n";
		log += $"  + ScriptingBackend : {ScriptingBackend}\n";
		log += $"  + IsDebug : {IsDebug}\n";
		log += $"  + IsForceSingleInstance : {IsForceSingleInstance}\n";
		log += $"  + IsForceEnableAppInfo : {IsForceEnableAppInfo}\n";
		log += $"  + IsForceEnableSentry : {IsForceEnableSentry}\n";
		log += $"  + PackageType : {PackageType}\n";
		log += $"  + HiveEnvType : {HiveEnvType}\n";
		log += $"  + AppId : {AppId}\n";
		log += "\n";
		return log;
	}
}

public class AppInfo : ModelBase<AppInfo, AppInfoData>, IInitModel<AppInfoData>
{
	public AppInfoData Data => data;

	public static void FixModelPath()
	{
		ModelPath = "Assets/Project/Resources";
	}

	public override void Init(AppInfoData newData)
	{
		base.Init(newData);
		Save();
	}

	public void UpdateAssetBuildType(eAssetBuildType type)
	{
		data.AssetBuildType = type;
		Save();
	}


	public void UpdateAssetBundleVersion(string version)
	{
		data.AssetBundleVersion = version;
		Save();
	}


	private static AppInfo _instance;
	private static string GetModelPath() => Path.Combine(ModelPath, $"{typeof(AppInfo).Name}.asset");

	public new static AppInfo Instance
	{
		get
		{
			if (!_instance)
			{
				FixModelPath();
				_instance = Resources.Load<AppInfo>(typeof(AppInfo).Name);
#if UNITY_EDITOR
				if (!_instance)
				{
					var modelPath = GetModelPath();
					var newModel = CreateInstance<AppInfo>();
					newModel.data = AppInfoData.Default;
					_instance = newModel;
					var dir = Path.GetDirectoryName(modelPath);
					Util.CreateDirectory(dir);
					if (!File.Exists(modelPath))
						AssetDatabase.CreateAsset(_instance, modelPath);
					AssetDatabase.SaveAssets();
				}
#endif // UNITY_EDITOR
			}

			return _instance;
		}
	}

	public void Save()
	{
#if UNITY_EDITOR
		EditorUtility.SetDirty(this);
		AssetDatabase.SaveAssetIfDirty(this);
#endif // UNITY_EDITOR
	}
}
