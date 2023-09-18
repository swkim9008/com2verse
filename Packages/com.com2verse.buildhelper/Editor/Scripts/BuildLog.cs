/*===============================================================
* Product:		Com2Verse
* File Name:	BuildLog.cs
* Developer:	pjhara
* Date:			2023-03-20 18:07
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#define BUILD_SCRIPT_LOG

using System.Diagnostics;

namespace Com2VerseEditor.BuildHelper
{
	public static class BuildLog
	{
		[Conditional("BUILD_SCRIPT_LOG"), Conditional("SONG_TEST")]
		public static void BuildScriptLog(string msg)
		{
			UnityEngine.Debug.Log("[BuildScriptLog] " + msg);
		}

		[Conditional("DETAIL_BUILD_SCRIPT_LOG"), Conditional("SONG_TEST")]
		public static void DetailBuildScriptLog(string msg)
		{
			UnityEngine.Debug.Log("[BuildScriptLog] " + msg);
		}

		[Conditional("BUILD_SCRIPT_LOG"), Conditional("SONG_TEST")]
		public static void BuildScriptLogError(string msg)
		{
			UnityEngine.Debug.LogError("[BuildScriptLog] " + msg);
		}
	}
}
