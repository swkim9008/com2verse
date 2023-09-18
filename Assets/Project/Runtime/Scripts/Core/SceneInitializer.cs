/*===============================================================
* Product:		Com2Verse
* File Name:	SceneInitializer.cs
* Developer:	tlghks1009
* Date:			2023-04-28 10:11
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Logger;
using Com2Verse.SceneManagement;
using Cysharp.Threading.Tasks;

namespace Com2Verse
{
	public sealed class SceneInitializer : MonoSingleton<SceneInitializer>
	{
		/// <summary>
		/// UniTask 등 프로젝트 전반적으로 사용하는 시스템은 최초 Start 이후 타이밍에서 실행하는것이 안전.
		/// </summary>
		private void Start()
		{
			InitializeScene().Forget();
		}

		private static async UniTask InitializeScene()
		{
			var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
			if (sceneName?.Equals(Define.SceneSplashName) is true)
			{
				await SceneManager.Instance.ChangeSceneAsync<SceneSplash>();
			}
			else
			{
#if METAVERSE_RELEASE
				throw new System.InvalidOperationException("Build scene is not SceneSplash.");
#else
				C2VDebug.LogErrorMethod(nameof(SceneInitializer), "You are bypassing scene splash. You may not be able to use the system properly.");
				await SceneManager.Instance.ChangeSceneAsync<SceneDebug>();
#endif // METAVERSE_RELEASE
			}
		}
	}
}
