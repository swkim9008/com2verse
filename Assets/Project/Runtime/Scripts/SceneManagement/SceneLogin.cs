/*===============================================================
* Product:    Com2Verse
* File Name:  SceneLogin.cs
* Developer:  jehyun
* Date:       2022-03-10 10:17
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Com2Verse.LruObjectPool;
using Com2Verse.SoundSystem;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;

namespace Com2Verse
{
	public sealed class SceneLogin : SceneBase
	{
		protected override void OnLoadingCompleted()
		{
			AppInitializationSceneController.Instance.InitializeAppAsync().Forget();

			UIManager.Instance.HideToastPopup();
			SoundManager.PlayBGM(eSoundIndex.BGM__TITLE, 3, 0);
		}

		protected override void OnExitScene(SceneBase nextScene)
		{
			RuntimeObjectManager.Instance.Clear();
		}

		protected override void RegisterLoadingTasks(Dictionary<string, UniTask> loadingTasks)
		{
			loadingTasks.Add("BuiltInLocalization", UniTask.Defer(LoadBuiltInTableAsync));
			loadingTasks.Add("LoadDimmedPopup", UniTask.Defer(LoadDimmedPopupAsync));
			//loadingTasks.Add("BadgeHierarchyTable", UniTask.Defer(LoadBadgeTableAsync)); // 배찌 테이블 로드
			loadingTasks.Add("UICanvasRoot", UniTask.Defer(LoadUICanvasRootAsync));
		}


		private async UniTask LoadBuiltInTableAsync()
		{
			await Localization.Instance.LoadBuiltInTableAsync();
		}


		private async UniTask LoadDimmedPopupAsync()
		{
			await UIManager.Instance.LoadSystemViewAsync(eSystemViewType.UI_POPUP_DIMMED);
		}


		private async UniTask LoadBadgeTableAsync()
		{
			await BadgeHierarchyTable.LoadTableAsync();
		}
	}
}
