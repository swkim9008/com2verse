/*===============================================================
* Product:    Com2Verse
* File Name:  SceneBase.cs
* Developer:  jehyun
* Date:       2022-03-08 10:22
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.InputSystem;
using Com2Verse.Loading;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.UI;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Vuplex.WebView;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Com2Verse
{
	public abstract partial class SceneBase
	{
		public event Action<SceneBase, eSceneState, eSceneState>? SceneStateChanged;

		private eSceneState _sceneState = eSceneState.NONE;

		public eSceneState SceneState
		{
			get => _sceneState;
			private set
			{
				var prevState = _sceneState;
				if (prevState == value)
					return;

				_sceneState = value;

				LogMethod(ZString.Format("{0} -> {1}", prevState, _sceneState));
				SceneStateChanged?.Invoke(this, prevState, _sceneState);
			}
		}

#region SceneProperty
		public SceneProperty SceneProperty { get; private set; } = SceneProperty.Empty;

		public bool IsWorldScene => SceneProperty == SceneManager.Instance.WorldSceneProperty;

		public string        SceneName   => SceneProperty.GetSceneName() ?? GetType().Name;
		public eServiceType? ServiceType => SceneProperty.ServiceType;

		public SpaceTemplate? SpaceTemplate => SceneProperty.SpaceTemplate;
		public eSpaceType?    SpaceType     => SpaceTemplate?.SpaceType;
		public eSpaceCode?    SpaceCode     => SpaceTemplate?.SpaceCode;

		public eSpaceOptionCommunication  CommunicationType  => SceneProperty.CommunicationType;
		public eSpaceOptionChattingUI     ChattingUIType     => SceneProperty.ChattingUIType;
		public eSpaceOptionToolbarUI      ToolbarUIType      => SceneProperty.ToolbarUIType;
		public eSpaceOptionMap            MapType            => SceneProperty.MapType;
		public eSpaceOptionGhostAvatar    GhostAvatarType    => SceneProperty.GhostAvatarType;
		public eSpaceOptionViewport       ViewportType       => SceneProperty.ViewportType;
		public eSpaceOptionMotionTracking MotionTrackingType => SceneProperty.MotionTrackingType;

		public bool IsDebug => SceneProperty.IsDebug;

		public bool UseVoiceModule => true; // 기획 의도상 CommunicationType이 NONE인 경우에도 트리거 스몰토크는 가능하므로 true로 처리
		public bool UseCameraModule => CommunicationType is not
			(eSpaceOptionCommunication.NONE
		  or eSpaceOptionCommunication.SMALL_TALK_AREA_VOICE
		  or eSpaceOptionCommunication.SMALL_TALK_DISTANCE_VOICE);
		public bool UseScreenModule => CommunicationType is eSpaceOptionCommunication.MEETING;

		public int SessionTimeout => SceneProperty.SessionTimeout;
#endregion // SceneProperty

		protected virtual eMainCameraType MainCameraType => eMainCameraType.UI;

		private readonly Dictionary<string, UniTask> _loadingTasks = new();

		private readonly SceneInstanceContainer _sceneInstance = new();

		private SceneBase _previousScene = SceneEmpty.Empty;

		public void SetSceneProperty(SceneProperty? sceneProperty)
		{
			SceneProperty = sceneProperty ?? SceneProperty.Empty;
		}
		
		public async UniTask ChangeScene(SceneBase? previousScene)
		{
			_previousScene = previousScene;
			_previousScene.ExitScene(this);
			{
				await EnterScene();
			}
			_previousScene = SceneEmpty.Empty;
		}

		private async UniTask EnterScene()
		{
			SceneState = eSceneState.ENTERING;

			_loadingTasks.Clear();

			NetworkManager.Instance.OnDisconnected += OnNetworkDisconnected;
			
			RegisterGeneralLoadingTasks();
			RegisterLoadingTasks(_loadingTasks);

			SceneState = eSceneState.LOADING;

			await ExecuteLoadingTasks();

			NetworkManager.Instance.OnDisconnected -= OnNetworkDisconnected;

			SceneState = eSceneState.LOADED;
		}

		public void ExitScene(SceneBase nextScene)
		{
			SceneState = eSceneState.EXITING;

			LogMethod(nameof(OnExitScene));
			OnExitSceneReleaseSound();
			OnExitSceneVuplex();
			OnExitScene(nextScene);

			SceneState = eSceneState.NONE;
		}

		protected abstract void OnExitScene(SceneBase nextScene);

		private void RegisterGeneralLoadingTasks()
		{
			_loadingTasks.Add("LoadScene", UniTask.Defer(LoadScene));
		}

		protected abstract void RegisterLoadingTasks(Dictionary<string, UniTask> loadingTasks);

		private async UniTask ExecuteLoadingTasks()
		{
			LogMethod(nameof(OnBeforeLoading));
			OnBeforeLoading();

			LoadingManager.Instance.SetTotalTaskCount(_loadingTasks.Count);

			foreach (var (name, task) in _loadingTasks)
			{
				LoadingManager.Instance.BeginTask(name);
				await task;
				if (_loadingTasks.Count == 0)
					break;
				LoadingManager.InstanceOrNull?.EndTask();
			}

			LogMethod(nameof(OnLoadingCompleted));
			OnLoadingCompleted();

			await UnityEngine.Resources.UnloadUnusedAssets()!.ToUniTask();

			GC.Collect();

			await UniTask.Delay(100);
			LoadingManager.InstanceOrNull?.Hide(LoadingManager.InstanceOrNull);
		}

		private async UniTask LoadScene()
		{
			if (_sceneInstance.IsSceneLoaded)
				return;

			LogMethod("Step 01 - Additive load loading scene (PreviousScene + LoadingScene)");
			await SceneManager.Instance.LoadLoadingSceneAsync();

			LogMethod("Step 02 - Unload previous scene (LoadingScene)");
			await _previousScene._sceneInstance.UnloadAsync();

			LogMethod("Step 03 - Additive load new scene (LoadingScene + NewScene)");
			await _sceneInstance.LoadAsync(SceneName, LoadSceneMode.Additive);

			LogMethod("Step 04 - Unload loading scene (NewScene)");
			await SceneManager.Instance.UnloadLoadingSceneAsync();

			LogMethod("Step 05 - Setup main camera for new scene");
			CameraManager.Instance.MainCameraType = MainCameraType;

			LogMethod("Step 06 - Setup UI Block");
			InputSystemManager.ResetAllButtonBlock();
			UIStackManager.Instance.ResetStack();
		}

		protected virtual void OnBeforeLoading() { }

		protected abstract void OnLoadingCompleted();

		protected void OnNetworkDisconnected()
		{
			_loadingTasks.Clear();
			DelayedMoveLoginScene().Forget();
		}

		private async UniTask DelayedMoveLoginScene()
		{
			await UniTask.WaitUntil(() => !LoadingManager.Instance.IsLoading);
			User.Instance.SetDisconnected();
			LoadingManager.Instance.ChangeScene<SceneLogin>();
		}

#region Debug
		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, DebuggerStepThrough, StackTraceIgnore]
		public void LogMethod(string? message = null, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogMethod(GetType().Name, message, caller);
		}
#endregion // Debug

		private void OnExitSceneReleaseSound() => Sound.SoundManager.Instance.ClearAssetHandle();
#region Vuplex WebView
		private void OnExitSceneVuplex() => ActiveViewCounter.Instance?.ForceClear();
#endregion // Vuplex WebView

		public virtual void OnEscapeAction() { }
	}
}
