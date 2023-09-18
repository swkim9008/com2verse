#if !METAVERSE_RELEASE
/*===============================================================
* Product:		Com2Verse
* File Name:	SceneDebug.cs
* Developer:	eugene9721
* Date:			2023-02-24 11:06
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.Data;
using Com2Verse.Option;
using Com2Verse.SoundSystem;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using Localization = Com2Verse.UI.Localization;

namespace Com2Verse
{
	/// <summary>
	/// 에디터에서 Play 모드로 진입했을 때, SceneSplash가 아닌 경우 해당 Scene로 로드됩니다.
	/// </summary>
	public sealed class SceneDebug : SceneBase
	{
		protected override void OnLoadingCompleted() { }

		protected override void OnExitScene(SceneBase nextScene) { }

		protected override void RegisterLoadingTasks(Dictionary<string, UniTask> loadingTasks)
		{
			loadingTasks.Add("Addressable",    UniTask.Defer(InitializeAddressableSystem));
			loadingTasks.Add("UISystem",       UniTask.Defer(InitializeUISystem));
			loadingTasks.Add("ProjectManager", UniTask.Defer(InitializeProjectAsync));
			loadingTasks.Add("SystemView",     UniTask.Defer(LoadSystemViewListAsync));
		}

		private static async UniTask InitializeAddressableSystem()
		{
			AssetBundleManager.Instance.Initialize();

			await C2VAddressables.InitializeAsync();
		}

		private static async UniTask InitializeUISystem()
		{
			await UIManager.Instance.Initialize();

			ViewModelManager.Instance.Initialize();

			await TableDataManager.Instance.InitializeSettingTableAsync();
			OptionController.Instance.InitializeAfterSplash();
			SoundManager.Initialize();

			Localization.Instance.Initialize(OptionController.Instance.GetOption<LanguageOption>().GetLanguage());

			await UniTask.CompletedTask;
		}

		private async UniTask InitializeProjectAsync()
		{
			await ProjectManager.Instance.TryInitializeAsync();
		}
	}
}
#endif // !METAVERSE_RELEASE
