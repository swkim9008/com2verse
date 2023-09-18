/*===============================================================
* Product:    Com2Verse
* File Name:  SceneSplash.cs
* Developer:  yangsehoon
* Date:       2023-02-20 10:45
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Com2Verse.Builder;
using Com2Verse.Extension;
using Com2Verse.SoundSystem;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse
{
	public sealed class SceneBuilder : SceneBase
	{
		protected override void OnLoadingCompleted()
		{
			SoundManager.PlayBGM(eSoundIndex.NONE, 2, 0, 0);
		}

		protected override void OnExitScene(SceneBase nextScene)
		{
			var cameraMouseController = Object.FindObjectOfType<CameraMouseController>();
			if (!cameraMouseController.IsReferenceNull())
			{
				Object.Destroy(cameraMouseController);
			}
		}

		protected override void RegisterLoadingTasks(Dictionary<string, UniTask> loadingTasks)
		{
			loadingTasks.Add("UICanvasRoot", UniTask.Defer(LoadUICanvasRootAsync));
			loadingTasks.Add("SpaceData", UniTask.Defer(LoadSpaceData));
			loadingTasks.Add("InventoryData", UniTask.Defer(LoadInventoryData));
		}

		private async UniTask LoadInventoryData()
		{
			BuilderAssetManager.Instance.Initialize();
			await BuilderInventoryManager.Instance.FetchInventoryData();
		}

		private async UniTask LoadSpaceData()
		{
			SpaceManager.Instance.LoadSpaceData();
			await UniTask.Yield();
		}
	}
}
