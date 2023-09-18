/*===============================================================
* Product:		Com2Verse
* File Name:	EditorApplicationUtil.cs
* Developer:	tlghks1009
* Date:			2023-05-24 17:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#if UNITY_EDITOR

using Cysharp.Threading.Tasks;
using UnityEditor;

namespace Com2VerseEditor
{
	public static class EditorApplicationUtil
	{
		public static void ExitPlayMode() => ExitPlayModeToUniTask().Forget();

		private static async UniTask ExitPlayModeToUniTask()
		{
			await UniTask.Delay(1000);

			EditorApplication.isPlaying = false;
		}
	}
}

#endif