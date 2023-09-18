/*===============================================================
* Product:		Com2Verse
* File Name:	DefineSymbolsManager.cs
* Developer:	pjhara
* Date:			2023-03-20 17:35
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using Com2Verse.BuildHelper;
using UnityEditor;

namespace Com2VerseEditor.BuildHelper
{
	public static class DefineSymbols
	{
		private static void AddScriptingDefineSymbols(List<string> defines, string define)
		{
			BuildLog.DetailBuildScriptLog("AddScriptingDefineSymbols called");
			BuildLog.DetailBuildScriptLog($"define : {define}");
			if (!defines.Contains(define))
			{
				defines.Add(define);
			}
		}

		private static void RemoveScriptingDefineSymbols(List<string> defines, string define)
		{
			BuildLog.DetailBuildScriptLog("RemoveScriptingDefineSymbols called");
			BuildLog.DetailBuildScriptLog($"define : {define}");
			if (defines.Contains(define))
				defines.Remove(define);
		}

		private static void CommonDefineSymbols(List<string> defines)
		{
			// urp
			AddScriptingDefineSymbols(defines, "USING_URP");

			// vuplex
			AddScriptingDefineSymbols(defines, "VUPLEX_CCU");
			AddScriptingDefineSymbols(defines, "VUPLEX_STANDALONE");
			AddScriptingDefineSymbols(defines, "VUPLEX_ALLOW_LARGE_WEBVIEWS");
		}

		public static void UpdateDefineSymbols(bool isLocalBuild, BuildTargetGroup targetGroup, bool isDebug, bool enableLogging, bool isForceEnableAppInfo, ScriptingImplementation scriptingBackend, eBuildEnv buildEnv)
		{
			BuildLog.BuildScriptLog("UpdateDefineSymbols called");
			var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup).Split(';').Select(d => d.Trim()).ToList();
			BuildLog.BuildScriptLog($" @ DefineSymbols(before) : {string.Join(";", defines.ToArray())}");

			// see : https://jira.com2us.com/wiki/display/C2U2VR/Define+Symbols

			CommonDefineSymbols(defines);

			// test
			RemoveScriptingDefineSymbols(defines, "SONG_TEST");

			// debug
			RemoveScriptingDefineSymbols(defines, "MEDIASDK_ENABLE_LOGGING");
			RemoveScriptingDefineSymbols(defines, "ENABLE_LOGGING");
			RemoveScriptingDefineSymbols(defines, "ENABLE_CHEATING");
			RemoveScriptingDefineSymbols(defines, "METAVERSE_RELEASE");
			if (isDebug)
			{
				if (enableLogging)
				{
					AddScriptingDefineSymbols(defines, "MEDIASDK_ENABLE_LOGGING");
					AddScriptingDefineSymbols(defines, "ENABLE_LOGGING");
				}
				AddScriptingDefineSymbols(defines, "ENABLE_CHEATING");
			}
			// release
			else
			{
				AddScriptingDefineSymbols(defines, "METAVERSE_RELEASE");
				if (isForceEnableAppInfo)
				{
					AddScriptingDefineSymbols(defines, "ENABLE_CHEATING");
				}

				if (enableLogging)
				{
					AddScriptingDefineSymbols(defines, "MEDIASDK_ENABLE_LOGGING");
					AddScriptingDefineSymbols(defines, "ENABLE_LOGGING");
				}
			}

			// il2cpp
			RemoveScriptingDefineSymbols(defines, "IL2CPP_ADDITIONAL_ARGS");
			if (scriptingBackend == ScriptingImplementation.IL2CPP)
			{
				AddScriptingDefineSymbols(defines, "IL2CPP_ADDITIONAL_ARGS");
			}

			// env
			RemoveScriptingDefineSymbols(defines, "ENV_DEV");
			RemoveScriptingDefineSymbols(defines, "ENV_QA");
			RemoveScriptingDefineSymbols(defines, "ENV_STAGING");
			RemoveScriptingDefineSymbols(defines, "ENV_PRODUCTION");
			RemoveScriptingDefineSymbols(defines, "ENV_DEV_INTEGRATION");
			switch (buildEnv)
			{
				case eBuildEnv.DEV: {AddScriptingDefineSymbols(defines, "ENV_DEV");} break;
				case eBuildEnv.QA: {AddScriptingDefineSymbols(defines, "ENV_QA");} break;
				case eBuildEnv.DEV_INTEGRATION:{AddScriptingDefineSymbols(defines, "ENV_DEV_INTEGRATION");}break;
				case eBuildEnv.STAGING: {AddScriptingDefineSymbols(defines, "ENV_STAGING");} break;
				case eBuildEnv.PRODUCTION: {AddScriptingDefineSymbols(defines, "ENV_PRODUCTION");} break;
				default: {AddScriptingDefineSymbols(defines, "ENV_DEV");} break;
			}

			// Hot-Reload
			RemoveScriptingDefineSymbols(defines, "LiveScriptReload_Enabled");
			RemoveScriptingDefineSymbols(defines, "LiveScriptReload_IncludeInBuild_Enabled");

			if (isDebug)
			{
				if (isLocalBuild)
				{
					AddScriptingDefineSymbols(defines, "LiveScriptReload_Enabled");
					if (scriptingBackend == ScriptingImplementation.Mono2x)
					{
						AddScriptingDefineSymbols(defines, "LiveScriptReload_IncludeInBuild_Enabled");
					}
				}
			}

			string definesStr = string.Join(";", defines.ToArray());
			BuildLog.BuildScriptLog($" @ DefineSymbols(after) : {definesStr}");
			PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, definesStr);
			BuildLog.BuildScriptLog("UpdateDefineSymbols setting after");
		}
	}
}
