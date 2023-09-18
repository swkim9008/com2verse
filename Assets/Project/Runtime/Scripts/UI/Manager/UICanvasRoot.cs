/*===============================================================
* Product:    Com2Verse
* File Name:  UICanvasRoot.cs
* Developer:  tlghks1009
* Date:       2022-05-02 14:15
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.UI
{
	public class UICanvasRoot : MonoBehaviour
	{
		public List<GUIView> _guiViewList = new();
		public Transform PopupLayer { get; private set; }

		private readonly List<GUIView> _changedGUIViews = new();

		private IUpdateOrganizer _updateOrganizer;
		private UINavigationController _navigationController;


		private void Awake()
		{
			PopupLayer = transform.Find("Content_Popup");
		}

		private void OnDestroy()
		{
			UIManager.InstanceOrNull?.RemoveUICanvasRoot(this);
		}

		public void SetChildActive(bool isActive, params string[] ignoreList)
		{
			if (_guiViewList == null)
				return;

			foreach (var guiView in _guiViewList)
			{
				if (guiView.IsUnityNull())
					continue;

				var viewName = guiView!.ViewName;
				if (viewName != null && ignoreList?.Contains(viewName) == true) continue;

				guiView.SetActive(isActive);
			}
		}

		public void SetPopupLayerActive(bool isActive, bool isSaveView, GUIView exceptView)
		{
			if (isSaveView)
				_changedGUIViews!.Clear();
			foreach (var view in PopupLayer.GetComponentsInChildren<GUIView>())
			{
				if (exceptView != null && view.Equals(exceptView)) continue;
				if (isSaveView)
				{
					if (isActive  && view.VisibleState is GUIView.eVisibleState.OPENING or GUIView.eVisibleState.OPENED) continue;
					if (!isActive && view.VisibleState is GUIView.eVisibleState.CLOSING or GUIView.eVisibleState.CLOSED) continue;
					_changedGUIViews.Add(view);
				}
				else if (!_changedGUIViews!.Contains(view))
					continue;

				view.SetActive(isActive);
			}
		}

		public void Initialize(IUpdateOrganizer updateOrganizer, UINavigationController navigationController)
		{
			_updateOrganizer = updateOrganizer;
			_navigationController = navigationController;

			InitializeGuiViews();
		}


		private void InitializeGuiViews()
		{
			var guiViewList = GetComponentsInChildren<GUIView>(true);
			foreach (var guiView in guiViewList)
			{
				guiView.IsStatic = true;
				guiView.UpdateOrganizer = _updateOrganizer;
				guiView.WillChangeInputSystem = true;
				guiView.SetDefaultActive();

				_navigationController.RegisterEvent(guiView);
				_guiViewList.Add(guiView);
			}
		}


		public new T[] GetComponentsInChildren<T>(bool includeInactive = false)
		{
			return this.transform.GetComponentsInChildren<T>(includeInactive);
		}
	}
}
