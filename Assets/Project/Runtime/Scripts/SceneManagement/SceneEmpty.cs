/*===============================================================
 * Product:		Com2Verse
 * File Name:	SceneEmpty.cs
 * Developer:	urun4m0r1
 * Date:		2023-07-12 21:02
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse
{
	public class SceneEmpty : SceneBase
	{
		public static readonly SceneBase Empty = new SceneEmpty();

		protected override void OnLoadingCompleted() { }

		protected override void OnExitScene(SceneBase nextScene) { }

		protected override void RegisterLoadingTasks(System.Collections.Generic.Dictionary<string, Cysharp.Threading.Tasks.UniTask> loadingTasks) { }
	}
}
