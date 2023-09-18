/*===============================================================
* Product:    Com2Verse
* File Name:  UIManager.cs
* Developer:  tlghks1009
* Date:       2022-04-20 10:14
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Option;
using Com2Verse.ScreenShare;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;


namespace Com2Verse.UI
{
	public partial class UIManager : Singleton<UIManager>, IUpdateOrganizer, IDisposable
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private UIManager() { }

		private readonly Dictionary<string, GUIView> _loadedGuiViewDict = new();
		private readonly List<GUIView> _guiViewList = new();
		private readonly List<GUIView> _activatedGuiViewList = new();
		private readonly UINavigationController _uiNavigationController = new();

		private readonly List<UICanvasRoot> _canvasRootList = new();

		private GameObject _systemCanvasRoot;
		private UICanvasRoot _currentCanvasRoot;

		private bool    _isInitialized;
		private GUIView _webViewGUI;

		public async UniTask Initialize()
		{
			if (_isInitialized) return;
			_isInitialized = true;

			InitializeSystemView();

			InitializeTimer();

			InitializeUINavigation();

			await AddEventSystemObject();

			AddEvents();
		}

		public void Destroy(GUIView guiView)
		{
			Object.Destroy(guiView.gameObject);

			_guiViewList.Remove(guiView);
			_activatedGuiViewList.Remove(guiView);

			string assetAddressableName = guiView.name;
			if (_loadedGuiViewDict.ContainsKey(assetAddressableName))
				_loadedGuiViewDict.Remove(assetAddressableName);
		}


		/// <summary>
		/// Contents의 Root 입니다.IsStatic = {bool} false
		/// </summary>
		/// <param name="address"></param>
		/// <param name="onCompleted"></param>
		public async UniTask LoadUICanvasRootAsync(string address, Action<UICanvasRoot> onCompleted)
		{
			string assetAddressableName = $"{address}.prefab";

			var handle = C2VAddressables.InstantiateAsync(assetAddressableName);
			if (handle == null)
			{
				return;
			}

			var loadedAsset = await handle.ToUniTask();
			if (loadedAsset.IsUnityNull())
			{
				return;
			}

			_currentCanvasRoot = loadedAsset!.GetComponent<UICanvasRoot>()!;
			_currentCanvasRoot.Initialize(this, _uiNavigationController);

			ResizeInterface();

			_canvasRootList!.Add(_currentCanvasRoot);
			onCompleted?.Invoke(_currentCanvasRoot);
		}

		public void RemoveUICanvasRoot(UICanvasRoot canvasRoot)
		{
			_canvasRootList!.Remove(canvasRoot);
		}

		private async UniTask AddEventSystemObject()
		{
			GameObject eventSystemObj;

			var eventSystemHandle = C2VAddressables.LoadAssetAsync<GameObject>("UI_EventSystem.prefab");
			if (eventSystemHandle == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "not found event system reference");
				eventSystemObj = new GameObject();
				eventSystemObj.AddComponent<EventSystem>();
			}
			else
			{
				var eventSystemPrefab = await eventSystemHandle!.ToUniTask();
				eventSystemObj = Object.Instantiate(eventSystemPrefab);
			}

			eventSystemObj!.name = "UI_EventSystem";
			Object.DontDestroyOnLoad(eventSystemObj);
		}


		private void OnBeforeSceneChanged(SceneBase previousScene, SceneBase currentScene)
		{
			Release();

			InitializeUINavigation();
		}


		private void InitializeUINavigation()
		{
			_uiNavigationController.Initialize(this);
		}


		private void Release()
		{
			_uiNavigationController.Release();

			_guiViewList.Clear();
			_loadedGuiViewDict.Clear();
			_activatedGuiViewList.Clear();

			_onUpdateEvent = null;
		}

		private void AddEvents()
		{
			SceneManager.Instance.BeforeSceneChanged               += OnBeforeSceneChanged;
			ScreenCaptureManager.Instance.ScreenShareSignalChanged += OnScreenShareSignalChanged;
		}


		private void RemoveEvents()
		{
			var sceneManager = SceneManager.InstanceOrNull;
			if (sceneManager != null)
			{
				sceneManager.BeforeSceneChanged -= OnBeforeSceneChanged;
			}

			var screenCaptureManager = ScreenCaptureManager.InstanceOrNull;
			if (screenCaptureManager != null)
			{
				screenCaptureManager.ScreenShareSignalChanged -= OnScreenShareSignalChanged;
			}
		}


		public void ResizeInterface()
		{
			var controlOption = OptionController.Instance.GetOption<ControlOption>();

			foreach (var canvasRoot in _canvasRootList!)
			{
				if (canvasRoot.IsUnityNull())
					continue;

				var allCanvas = canvasRoot!.GetComponentsInChildren<CanvasScaler>();
				if (allCanvas == null || allCanvas.Length == 0)
					continue;

				foreach (var canvas in allCanvas)
				{
					if (canvas.IsUnityNull())
						continue;

					canvas!.referenceResolution = controlOption.GetInterfaceSizeVector();
				}
			}
		}

		public void Dispose()
		{
			Release();

			RemoveEvents();
		}
#region Update
		private Action _onUpdateEvent;
		private Action _onUpdateEventCrossScene;
		private bool   _isUpdateReady;

		public void AddUpdateListener(Action onFunc, bool crossScene = false)
		{
			if (crossScene)
			{
				_onUpdateEventCrossScene -= onFunc;
				_onUpdateEventCrossScene += onFunc;
			}
			else
			{
				_onUpdateEvent -= onFunc;
				_onUpdateEvent += onFunc;
			}
		}

		public void RemoveUpdateListener(Action onFunc)
		{
			_onUpdateEventCrossScene -= onFunc;
			_onUpdateEvent -= onFunc;
		}

		public void OnUpdate()
		{
			_isUpdateReady = true;
			_onUpdateEventCrossScene?.Invoke();
			_onUpdateEvent?.Invoke();
		}
#endregion Update
	}
}
