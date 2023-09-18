/*===============================================================
* Product:    Com2Verse
* File Name:  SceneCharacterSelection.cs
* Developer:  jehyun
* Date:       2022-03-08 10:46
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Com2Verse.Avatar;
using Com2Verse.Data;
using Com2Verse.Project.InputSystem;
using Com2Verse.SoundSystem;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using Com2Verse.LruObjectPool;

namespace Com2Verse
{
	public sealed class SceneAvatarSelection : SceneBase
	{
		protected override void OnLoadingCompleted()
		{
			InputSystemManagerHelper.ChangeState(eInputSystemState.UI);
		}

		protected override void OnExitScene(SceneBase nextScene)
		{
			RuntimeObjectManager.Instance.Clear();
			SoundManager.PlayBGM(eSoundIndex.NONE, 2, 0, 0);
			AvatarMediator.Instance.AvatarCloset.Clear();
		}

		protected override void RegisterLoadingTasks(Dictionary<string, UniTask> loadingTasks)
		{
			loadingTasks.Add("UserLogin",              UniTask.Defer(WaitForUserLoginAsync));
			loadingTasks.Add("LoadAvatarCreateAsset",  UniTask.Defer(LoadAvatarCreateAsset));
			loadingTasks.Add("UICanvasRoot",           UniTask.Defer(LoadUICanvasRootAsync));
			loadingTasks.Add("AvatarPartsSpriteAtlas", UniTask.Defer(LoadAvatarPartsSpriteAtlasAsync));
		}

		private async UniTask LoadAvatarCreateAsset()
		{
			await AvatarCreateManager.Instance.LoadResources();
		}

		//FIXME : TestCode 입니다. 추후 제거 예정입니다.
		// protected override async UniTask ExecuteLoadingTasks()
		// {
		// 	bool downloadComplete = false;
		// 	AddressableSystemManager.Instance.Initialize(() =>
		// 	{
		// 		AddressableSystemManager.Instance.DownloadAssets(() =>
		// 		{
		// 			downloadComplete = true;
		// 		});
		// 	});
		//
		// 	await UniTask.WaitUntil(() => downloadComplete);
		//
		// 	LoadingManager.Instance.Reset();
		//
		// 	base.ExecuteLoadingTasks().Forget();
		// }

		private async UniTask LoadAvatarPartsSpriteAtlasAsync()
		{
			bool spriteAtlasLoaded = false;
			SpriteAtlasManager.Instance.LoadSpriteAtlasAsync("Atlas_PartsItem", _ => { spriteAtlasLoaded = true; }, true);
			await UniTask.WaitUntil(() => spriteAtlasLoaded);
		}
	}
}
