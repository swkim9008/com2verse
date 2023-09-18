/*===============================================================
 * Product:		Com2Verse
 * File Name:	SceneInstanceContainer.cs
 * Developer:	urun4m0r1
 * Date:		2023-05-31 14:56
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Linq;
using Com2Verse.AssetSystem;
using Com2Verse.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Com2Verse
{
	public sealed class SceneInstanceContainer
	{
		public Scene?         NonAddressableSceneInstance { get; private set; }
		public SceneInstance? AddressableSceneInstance    { get; private set; }

		public bool IsSceneLoaded               => IsNonAddressableSceneLoaded || IsAddressableSceneLoaded;
		public bool IsNonAddressableSceneLoaded => NonAddressableSceneInstance?.isLoaded    ?? false;
		public bool IsAddressableSceneLoaded    => AddressableSceneInstance?.Scene.isLoaded ?? false;

		public async UniTask UnloadAsync()
		{
			if (AddressableSceneInstance != null)
			{
				if (IsAddressableSceneLoaded)
					await C2VAddressables.UnloadSceneAsync(AddressableSceneInstance.Value)!.ToUniTask();

				AddressableSceneInstance = null;
			}

			if (NonAddressableSceneInstance != null)
			{
				if (IsNonAddressableSceneLoaded)
					await UnitySceneManager.UnloadSceneAsync(NonAddressableSceneInstance.Value)!.ToUniTask();

				NonAddressableSceneInstance = null;
			}
		}

		public async UniTask LoadAsync(string sceneName, LoadSceneMode sceneLoadMode)
		{
			await UnloadAsync();

			if (Define.NonAddressableSceneNames.Contains(sceneName))
			{
				if (!UnitySceneManager.GetSceneByName(sceneName).isLoaded)
					await UnitySceneManager.LoadSceneAsync(sceneName, sceneLoadMode)!.ToUniTask();

				NonAddressableSceneInstance = UnitySceneManager.GetSceneByName(sceneName);
			}
			else
			{
				var addressableSceneName = GetAddressableSceneName(sceneName);
				AddressableSceneInstance = await C2VAddressables.LoadSceneAsync(addressableSceneName, sceneLoadMode)!.ToUniTask();
			}
		}

		public static string GetAddressableSceneName(string sceneName) => $"{sceneName}.unity";
	}
}
