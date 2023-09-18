/*===============================================================
* Product:    Com2Verse
* File Name:  SceneManager.cs
* Developer:  jehyun
* Date:       2022-03-08 11:18
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.SceneManagement;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;

namespace Com2Verse
{
	public sealed class SceneManager : Singleton<SceneManager>
	{
		public event Action<SceneBase, SceneBase>? BeforeSceneChanged;
		public event Action<SceneBase, SceneBase>? CurrentSceneChanged;

		private SceneBase _currentScene = SceneEmpty.Empty;

		public SceneBase CurrentScene
		{
			get => _currentScene;
			private set
			{
				var prevScene = _currentScene;
				if (prevScene == value)
					return;

				_currentScene = value;

				LogMethod(ZString.Format("\"{0}\" -> \"{1}\"", prevScene.SceneName, _currentScene.SceneName));
				CurrentSceneChanged?.Invoke(prevScene, _currentScene);
			}
		}

#region Initialize
		public SceneProperty? WorldSceneProperty { get; private set; }

		public TableSpaceOptionSetting? SpaceOptionSettings { get; private set; }

		public bool IsInitializing { get; private set; }
		public bool IsInitialized  { get; private set; }

		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private SceneManager() { }

		public void TryInitialize()
		{
			if (IsInitialized || IsInitializing)
				return;

			IsInitializing = true;
			{
				SpaceOptionSettings = TableDataManager.Instance.Get<TableSpaceOptionSetting>();
				WorldSceneProperty  = SceneProperty.CreateWorldSceneProperty();
			}
			IsInitializing = false;
			IsInitialized  = true;
		}
#endregion // Initialize

#region LoadingScene
		private readonly SceneInstanceContainer _loadingSceneInstanceContainer = new();

		public async UniTask LoadLoadingSceneAsync()
		{
			await _loadingSceneInstanceContainer.LoadAsync(Define.SceneLoadingName, LoadSceneMode.Additive);
		}

		public async UniTask UnloadLoadingSceneAsync()
		{
			await _loadingSceneInstanceContainer.UnloadAsync();
		}
#endregion // LoadingScene

#region Methods
		/// <summary>
		/// 씬 전환과 동시에 필요한 스크립트를 로드하고 리소스를 정리합니다.
		/// <br/>해당 메서드는 로딩 화면을 표시하지 않습니다.
		/// </summary>
		public async UniTask ChangeSceneAsync<T>(SceneProperty? sceneProperty = null) where T : SceneBase
		{
			var typeName  = typeof(T).Name;
			var sceneName = sceneProperty?.GetSceneName() ?? typeof(T).Name;
			LogMethod(ZString.Format("{0} (\"{1}\")", typeName, sceneName));

			var nextScene = Activator.CreateInstance<T>();
			if (nextScene == null)
			{
				LogErrorMethod(ZString.Format("Failed to create instance of \"{0}\"", typeName));
				return;
			}
			nextScene.SetSceneProperty(sceneProperty);
			
			BeforeSceneChanged?.Invoke(CurrentScene, nextScene);

			await nextScene.ChangeScene(CurrentScene);

			CurrentScene = nextScene;
		}
#endregion // Methods

#region Debug
		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, DebuggerStepThrough, StackTraceIgnore]
		private static void LogMethod(string? message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogMethod(nameof(SceneManager), message, caller);
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, DebuggerStepThrough, StackTraceIgnore]
		private static void LogErrorMethod(string? message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogErrorMethod(nameof(SceneManager), message, caller);
		}
#endregion // Debug
	}
}
