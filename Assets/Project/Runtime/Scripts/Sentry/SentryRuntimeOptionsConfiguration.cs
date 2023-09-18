using System.Collections.Generic;
using Com2Verse.BuildHelper;
using Sentry;
using UnityEngine;
using Sentry.Unity;

[CreateAssetMenu(fileName = "Assets/Resources/Sentry/SentryRuntimeOptionsConfiguration.asset", menuName = "Sentry/SentryRuntimeOptionsConfiguration", order = 999)]
public class SentryRuntimeOptionsConfiguration : Sentry.Unity.SentryRuntimeOptionsConfiguration
{
    /// See base class for documentation.
    /// Learn more at https://docs.sentry.io/platforms/unity/configuration/options/#programmatic-configuration
    public override void Configure(SentryUnityOptions options)
    {
        var enable = AppInfo.Instance.Data.IsForceEnableSentry;
        options.Enabled = enable;
#if UNITY_EDITOR
        options.CaptureInEditor = enable;
        options.Il2CppLineNumberSupportEnabled = false;
        options.Debug = true;
#else
        // build
        options.CaptureInEditor = false;
        options.Il2CppLineNumberSupportEnabled = AppInfo.Instance.Data.ScriptingBackend == "IL2CPP";
        options.Debug = AppInfo.Instance.Data.IsDebug;
#endif
        options.AddBreadcrumbsForLogType[LogType.Assert]    = false;
        options.AddBreadcrumbsForLogType[LogType.Log]       = false;
        options.AddBreadcrumbsForLogType[LogType.Warning]   = false;
        options.AddBreadcrumbsForLogType[LogType.Error]     = false;
        options.AddBreadcrumbsForLogType[LogType.Exception] = true;
  
        options.DiagnosticLevel             = SentryLevel.Debug;
        options.Environment                 = GetEnvironmentStr();
        options.Release                     = GetReleaseStr();
        options.AttachStacktrace            = true;
        options.EnableLogDebouncing         = true;

        if (AppInfo.Instance.Data.Environment == eBuildEnv.PRODUCTION)
            options.Dsn = "http://3bc644eb7d94465c87a08570473b6743@34.64.48.153/2";
        else
            options.Dsn = "http://928c7984e3504c589b891e701928908f@34.64.227.27/3";

#region Sampling & Performance monitoring
        options.SampleRate       = 0.25f;
        options.TracesSampleRate = 1.0f;
        options.TracesSampler = context =>
        {
            if (context.TransactionContext.IsParentSampled != null)
                return context.TransactionContext.IsParentSampled.Value ? 1.0f : 0.0f;

            return context.CustomSamplingContext.GetValueOrDefault("url") switch
            {
                "/payment" => 0.5f,
                "/search"  => 0.01f,
                "/health"  => 0.0f,
                _          => 0.1f,
            };
        };
#endregion // Sampling & Performance monitoring
        options.BeforeSend = sentryEvent =>
        {
            UpdateConfigureScope();
            return sentryEvent;
        };
    }

    public static void UpdateConfigureScope()
    {
        SentrySdk.ConfigureScope(scope =>
        {
            scope.SetTag("commits", AppInfo.Instance.Data.GetCommitHashURL());
            scope.Contexts["user info"] = GetUserInfo();
        });
    }


    private static string GetEnvironmentStr()
    {
        var environmentStr = AppInfo.Instance != null ? AppInfo.Instance.Data.Environment.ToString().ToLower() : "none";
#if UNITY_EDITOR
        environmentStr = "editor";
#endif // UNITY_EDITOR
        return environmentStr;
    }


    private static string GetReleaseStr()
    {
        var releaseStr = AppInfo.Instance != null ? AppInfo.Instance.Data.GetVersionInfo() : Application.version;
        return releaseStr;
    }

    public class UserInfoContext
    {
        public long   Id         { get; set; }
        public string Name       { get; set; }
        public string MemberName { get; set; }
    }

    private static readonly UserInfoContext _userInfo = new() {Id = 0, Name = string.Empty, MemberName = string.Empty};

    private static UserInfoContext GetUserInfo()
    {
        if (Com2Verse.Network.User.InstanceExists)
        {
            var id         = Com2Verse.Network.User.Instance.CurrentUserData.ID;
            var name       = Com2Verse.Network.User.Instance.CurrentUserData.UserName;
            var memberName = "";
            if (Com2Verse.Organization.DataManager.InstanceExists)
            {
                var member = Com2Verse.Organization.DataManager.Instance.GetMember(id);
                if (member != null)
                {
                    memberName = member.Member.MemberName;
                }
            }

            _userInfo.Id         = id;
            _userInfo.Name       = name;
            _userInfo.MemberName = memberName;
        }

        return _userInfo;
    }
}
