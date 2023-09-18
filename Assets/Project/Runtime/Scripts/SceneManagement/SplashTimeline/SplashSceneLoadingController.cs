/*===============================================================
* Product:		Com2Verse
* File Name:	SplashSceneLoadingController.cs
* Developer:	mikeyid77
* Date:			2022-09-23 15:15
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Threading;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace Com2Verse.UI
{
	[RequireComponent(typeof(PlayableDirector))]
	public sealed class SplashSceneLoadingController : MonoBehaviour
	{
		[SerializeField] private bool _skipSplash;

		[SerializeField] private PlayableDirector? _director;

		private CancellationTokenSource? _loadingWaitCts;

		private void Start()
		{
#if UNITY_EDITOR
			_skipSplash = true;
#endif // UNITY_EDITOR

			if (_skipSplash)
			{
				OnSplashSkipped();
				return;
			}

			_director ??= GetComponent<PlayableDirector>();
			if (_director == null)
				throw new NullReferenceException(nameof(_director));

			_director.Play();
		}

		public void OnSplashSkipped()
		{
			ChangeNextSceneAfterLoadingAsync().Forget();
		}

		private async UniTaskVoid ChangeNextSceneAfterLoadingAsync()
		{
			await WaitForLoadingAsync();
			await ChangeNextScene();
		}

		public void OnSplashPaused()
		{
			_director!.Pause();
			OnSplashPausedAsync().Forget();

			async UniTask OnSplashPausedAsync()
			{
				await WaitForLoadingAsync();
				_director!.Resume();
			}
		}

		private async UniTask WaitForLoadingAsync()
		{
			_loadingWaitCts = new CancellationTokenSource();
			await UniTaskHelper.WaitUntil(IsSceneLoaded, _loadingWaitCts);

			bool IsSceneLoaded()
			{
				return CurrentScene.Scene.SceneState is eSceneState.LOADED;
			}
		}

		public void OnSplashEnded()
		{
			ChangeNextScene().Forget();
		}

		private async UniTask ChangeNextScene()
		{
			await SceneManager.Instance.ChangeSceneAsync<SceneLogin>();
		}

		private void OnDestroy()         => DisposeToken();
		private void OnApplicationQuit() => DisposeToken();

		private void DisposeToken()
		{
			_loadingWaitCts?.Cancel();
			_loadingWaitCts?.Dispose();
			_loadingWaitCts = null;
		}
	}
}
