/*===============================================================
* Product:    Com2Verse
* File Name:  SceneSplash.cs
* Developer:  mikeyid77
* Date:       2022-09-23 15:05
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.Data;
using Com2Verse.Option;
using Com2Verse.Sound;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using Localization = Com2Verse.UI.Localization;
using SoundManager = Com2Verse.SoundSystem.SoundManager;

namespace Com2Verse
{
    public sealed class SceneSplash : SceneBase
    {
        protected override void OnLoadingCompleted() { }

        protected override void OnExitScene(SceneBase nextScene) { }

        protected override void RegisterLoadingTasks(Dictionary<string, UniTask> loadingTasks)
        {
            loadingTasks.Add("Addressable",  UniTask.Defer(InitializeAddressableSystem));
            loadingTasks.Add("UISystem",     UniTask.Defer(InitializeUISystem));
            loadingTasks.Add("UICanvasRoot", UniTask.Defer(LoadUICanvasRootAsync));
        }

        private static async UniTask InitializeAddressableSystem()
        {
            AssetBundleManager.Instance.Initialize();

            await C2VAddressables.InitializeAsync();
        }

        private static async UniTask InitializeUISystem()
        {
            await UIManager.Instance.Initialize();

            AudioListenerManager.Instance.Initialize();

            ViewModelManager.Instance.Initialize();

            await TableDataManager.Instance.InitializeSettingTableAsync();
            OptionController.Instance.InitializeAfterSplash();
            SoundManager.Initialize();

            Localization.Instance.Initialize(OptionController.Instance.GetOption<LanguageOption>().GetLanguage());

            await UniTask.CompletedTask;
        }
    }
}
