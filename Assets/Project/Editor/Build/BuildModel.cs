/*===============================================================
* Product:		Com2Verse
* File Name:	BuildModel.cs
* Developer:	jhkim
* Date:			2022-06-02 17:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.BuildHelper;
using Com2Verse.Serialization;
using UnityEditor;
using UnityEngine;
using static Com2Verse.AssetSystem.AssetSystemManager;

namespace Com2VerseEditor.Build
{
    //TODO: Mr.song - BuildInfo , AppInfoData 중복 제거 or 하나로 통일. (data 가 거의 동일함)
    [Serializable]
    public struct BuildInfo
    {
        public BuildTarget             BuildTarget;
        public BuildTargetGroup        BuildTargetGroup;
        public ScriptingImplementation ScriptingBackend;
        public bool                    ExecuteAfterBuild;
        public bool                    IsDebug;
        public string                  Version;
        public string                  AssetBundleVersion;
        public bool                    AppCenterUpload;
        public string                  AppCenterComment;
        public string                  AppCenterAppName;
        public string                  BuildFilePath;
        public DateTime                BuildTime;
        public bool                    EnableCodeSign;
        public string                  GitBuildBranch;
        public string                  GitCommitHash;
        public string                  GitRevisionCount;
        public string                  AppId;
        public eBuildEnv               env;
        public eAssetBuildType         AssetBuildType;
        public bool                    IsForceSingleInstance;
        public bool                    IsForceEnableAppInfo;
        public bool                    IsForceEnableSentry;
        public bool                    EnableLogging;
        public ePackageType            PackageType;
        public eHiveEnvType            HiveEnvType;

        public override string ToString() => $"{BuildScript.ToFileFormat(BuildTime)}_{BuildScript.AppName}_{Version}_{AssetBundleVersion}_{BuildScript.GetDistributeStr()}";
    }

    public sealed class BuildModel : ModelBase<BuildModel, BuildInfo>, IInitModel<BuildInfo>
    {
        public bool IsDebug => data.IsDebug;
        public string Version => data.Version;
        public void SetBuildTargetGroup(BuildTargetGroup value) => data.BuildTargetGroup = value;
        public void SetBuildTarget(BuildTarget value) => data.BuildTarget = value;

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(BuildModel))]
    public class BuildModelEditor : Editor
    {
        private BuildModel _model;

        private void Awake()
        {
            _model = target as BuildModel;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Save"))
                _model.Save();
        }
    }
#endif // UNITY_EDITOR
}
