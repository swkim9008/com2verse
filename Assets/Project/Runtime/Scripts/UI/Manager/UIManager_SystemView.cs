/*===============================================================
* Product:		Com2Verse
* File Name:	UIManager_EssentialPopup.cs
* Developer:	tlghks1009
* Date:			2022-08-24 10:55
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace Com2Verse.UI
{
	public enum eSystemViewType
	{
		UI_POPUP_DIMMED,
		UI_LOADING_PAGE,
		UI_FADE,
		TOAST_POPUP,
		NOTIFICATION_POPUP,
		UI_SHORT_LOADING,
		WAITING_RESPONSE_POPUP,
		ANNOUNCEMENT,
	}

	public partial class UIManager
	{
		private static readonly string _systemCanvasRootName = "2DCanvasRoot_SystemView";

		private static readonly string _systemUIInfoPath           = "UI/SystemUIInfo";
		private static readonly int    _systemUICanvasSortingOrder = 1000;

		private SystemUIInfo _systemUIInfo;

		private void InitializeSystemView()
		{
			_systemCanvasRoot = new GameObject(_systemCanvasRootName);
			_systemCanvasRoot.CreateDefaultCanvas(_systemUICanvasSortingOrder);

			GameObject.DontDestroyOnLoad(_systemCanvasRoot);

			_systemUIInfo = Resources.Load<SystemUIInfo>(_systemUIInfoPath);
			_systemUIInfo.Parse();
		}


		public async UniTask LoadSystemViewAsync(eSystemViewType systemViewType)
		{
			if (_systemUIInfo.TryGetValue(systemViewType, out var systemUIData))
			{
				if (!systemUIData.GUIView.IsUnityNull())
				{
					return;
				}

				var viewName = systemUIData.ViewName;

				await CreatePopup(viewName, (guiView) =>
				{
					guiView.transform.SetParent(_systemCanvasRoot.transform, true);

					guiView.SetSortingOrder(systemUIData.SortingOrder);

					guiView.gameObject.SetActive(false);

					systemUIData.GUIView = guiView;
				}, false, true);
			}
		}


		public async UniTask LoadSystemViewListAsync()
		{
			foreach (var systemUI in _systemUIInfo.GetSystemUIs())
			{
				await LoadSystemViewAsync(systemUI.SystemViewType);
			}
		}


		public GUIView GetSystemView(eSystemViewType systemViewType)
		{
			if (_systemUIInfo.IsUnityNull())
			{
				return null;
			}

			if (!_systemUIInfo!.TryGetValue(systemViewType, out var systemUI))
			{
				return null;
			}

			return systemUI.GUIView.IsUnityNull() ? null : systemUI!.GUIView;
		}


		public bool IsLoadedSystemView(GUIView guiView)
		{
			foreach (var systemUI in _systemUIInfo.GetSystemUIs())
			{
				if (systemUI.GUIView == guiView)
				{
					return true;
				}
			}

			return false;
		}
	}
}
