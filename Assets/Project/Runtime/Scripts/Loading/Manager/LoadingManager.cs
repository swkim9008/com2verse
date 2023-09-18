/*===============================================================
* Product:		Com2Verse
* File Name:	LoadingManager.cs
* Developer:	tlghks1009
* Date:			2022-06-07 18:45
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.UI;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Loading
{
	public enum eLoadingType
	{
		SHORT,
		FADE,
		PROGRESS,
	}

	public sealed class LoadingManager : Singleton<LoadingManager>
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private LoadingManager() { }

		public event Action         OnLoadingResetEvent;
		public event Action         OnInitLoadingEvent;
		public event Action<long>   OnStartTaskProcEvent;
		public event Action<string> OnCurrentLoadingEvent;
		public event Action<long>   OnLoadingCompletedEvent;
		public event Action         OnLoadingFinishedEvent;

		public bool LoadingFinished { get; private set; }
		public bool IsLoading       { get; private set; }

		private GUIView      _shortLoadingView;
		private float        _taskStartTime;
		private string       _currentTaskName;
		private eLoadingType _currentLoadingType;

		private SceneProperty _sceneProperty;
		public  SceneProperty LoadingSceneProperty => _sceneProperty;

		[NotNull] private readonly List<object> _loadingRequesters = new();

		public void ChangeScene<T>([CanBeNull] SceneProperty sceneProperty = null, eLoadingType loadingType = eLoadingType.PROGRESS) where T : SceneBase
		{
			IsLoading = true;
			var sceneName = sceneProperty?.GetSceneName() ?? typeof(T).Name;
			_sceneProperty = sceneProperty;
			C2VDebug.LogMethod(GetType().Name, ZString.Format("{0} (\"{1}\") [{2}]", typeof(T).Name, sceneName, loadingType));
			Show(this, loadingType, () => SceneManager.Instance.ChangeSceneAsync<T>(sceneProperty).Forget());
		}

		public void Show([CanBeNull] object requester, [CanBeNull] Action onOpened = null)
		{
			Show(requester, eLoadingType.SHORT, onOpened);
		}

		public void Show([CanBeNull] object requester, eLoadingType type, Action onOpened)
		{
			_loadingRequesters.TryAdd(requester);

			LoadingFinished     = false;
			_currentLoadingType = type;

			var view = GetView(type);

			if (view != null)
			{
				view.OnOpenedEvent += OnLoadingPageOpened;
				view.OnClosedEvent += OnLoadingPageClosed;
				view.Show();

				OnInitLoadingEvent?.Invoke();

				void OnLoadingPageOpened(GUIView _)
				{
					view.OnOpenedEvent -= OnLoadingPageOpened;
					onOpened?.Invoke();
				}

				void OnLoadingPageClosed(GUIView _)
				{
					view.OnClosedEvent -= OnLoadingPageClosed;
					LoadingFinished    =  true;
					OnLoadingFinishedEvent?.Invoke();
				}
			}
		}

		public void Hide([CanBeNull] object requester) => Hide(requester, _currentLoadingType);

		public void Hide([CanBeNull] object requester, eLoadingType type)
		{
			_loadingRequesters.Remove(requester);

			if (_loadingRequesters.Count > 0)
				return;

			ReleaseEvents();

			GetView(type)?.Hide();

			_sceneProperty = null;

			IsLoading = false;
		}


		public void Reset()
		{
			Log($"Loading Reset.");
			OnLoadingResetEvent?.Invoke();
		}


		private void ReleaseEvents()
		{
			OnLoadingCompletedEvent = null;
			OnLoadingResetEvent     = null;
			OnStartTaskProcEvent    = null;
			OnCurrentLoadingEvent   = null;
		}

		private static GUIView GetView(eLoadingType loadingType) => loadingType switch
		{
			eLoadingType.SHORT    => UIManager.Instance.GetSystemView(eSystemViewType.UI_SHORT_LOADING),
			eLoadingType.FADE     => UIManager.Instance.GetSystemView(eSystemViewType.UI_FADE),
			eLoadingType.PROGRESS => UIManager.Instance.GetSystemView(eSystemViewType.UI_LOADING_PAGE),
			_                     => throw new ArgumentOutOfRangeException(nameof(loadingType), loadingType, null),
		};

		public void SetTotalTaskCount(long toLoadCount)
		{
			Log($"Processing {toLoadCount.ToString()} tasks.");
			OnStartTaskProcEvent?.Invoke(toLoadCount);
		}

		public void BeginTask(string taskName)
		{
			_taskStartTime   = Time.realtimeSinceStartup;
			_currentTaskName = taskName;

			Log($"Loading {taskName}...");
			OnCurrentLoadingEvent?.Invoke(taskName);
		}

		public void EndTask() => EndTask(1);

		public void EndTask(long loadedCount)
		{
			var totalTime = Time.realtimeSinceStartup - _taskStartTime;

			Log($"Loading {_currentTaskName} completed. ({totalTime:F2}sec)");
			OnLoadingCompletedEvent?.Invoke(loadedCount);
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, DebuggerStepThrough, StackTraceIgnore]
		private void Log(string message)
		{
			C2VDebug.LogCategory(nameof(LoadingManager), message);
		}
	}
}
