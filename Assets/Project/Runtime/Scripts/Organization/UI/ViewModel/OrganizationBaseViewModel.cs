/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationBaseViewModel.cs
* Developer:	jhkim
* Date:			2022-10-04 15:25
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Extension;
using Com2Verse.Organization;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.UI
{
	public class OrganizationBaseViewModel : ViewModelBase
	{
#region Variables
		private static readonly float SearchDelayTime = 0.12f;
		private static Dictionary<string, GUIView> _views;
		private static HashSet<string> _requests;
		private static Dictionary<string, OrganizationBaseViewModel> _resViewModelMap;
		private static Dictionary<string, Action> _onShowEventMap;
		private static Dictionary<string, Action> _onHideEventMap;
		private static UIManager.Timer _searchTimer;

		private bool _isShow = true;
#endregion // Variables

#region Properties
		public bool IsShow
		{
			get => _isShow;
			set
			{
				_isShow = value;
				InvokePropertyValueChanged(nameof(IsShow), value);
			}
		}
		private static bool IsViewExist(string resName) => _views.ContainsKey(resName);
		private static GUIView GetView(string resName) => ValidateView(resName) ? _views[resName] : null;
		private static bool ValidateView(string resName)
		{
			if (IsViewExist(resName))
			{
				if (_views[resName].IsUnityNull())
				{
					_views.Remove(resName);
					if (_requests.Contains(resName))
						_requests.Remove(resName);
					return false;
				}
				return true;
			}
			return false;
		}
		protected static bool HasRequest(string resName) => _requests.Contains(resName);
#endregion // Properties

#region Initialize
		static OrganizationBaseViewModel()
		{
			_views = new Dictionary<string, GUIView>();
			_requests = new HashSet<string>();
			_resViewModelMap = new Dictionary<string, OrganizationBaseViewModel>();
			_onShowEventMap = new Dictionary<string, Action>();
			_onHideEventMap = new Dictionary<string, Action>();
		}

		protected OrganizationBaseViewModel(string resName)
		{
			if (_resViewModelMap.ContainsKey(resName))
				_resViewModelMap.Remove(resName);
			_resViewModelMap.Add(resName, this);
		}
#endregion // Initialize

#region View
		protected static void ShowView(string resName, Action onLoaded = null, Action onLoadedEnd = null)
		{
			var view = GetView(resName);
			var request = HasRequest(resName);
			if (view.IsReferenceNull())
			{
				if (request) return;
				_requests.Add(resName);
				UIManager.Instance.CreatePopup(resName, root =>
				{
					if (root.IsUnityNull()) return;
					view = root.Show();
					_views.Add(resName, view);
					_requests.Remove(resName);
					onLoaded?.Invoke();
					AddShowEvent(view, onLoadedEnd);
				}).Forget();
			}
			else
			{
				view.Show();
				SetVisible(resName, true);
				onLoaded?.Invoke();
			}
		}
		protected static void ShowView(string resName, MemberModel memberModel, Action onLoaded = null, Action onLoadedEnd = null)
		{
			UIManager.Instance.CreatePopup(resName, guiView =>
			{
				var view = guiView.Show();
				var viewModel = guiView.ViewModelContainer.GetViewModel<MeetingRoomProfileViewModel>();
				viewModel.GUIView = guiView;
				viewModel.SetProfile(memberModel).Forget();
				onLoaded?.Invoke();
				AddShowEvent(view, onLoadedEnd);
			}).Forget();
		}

		protected static void HideView(string resName, Action onHided = null, Action onHidedEnd = null)
		{
			var view = GetView(resName);
			if (view.IsReferenceNull()) return;
			SetVisible(resName, false);
			view.Hide();
			onHided?.Invoke();
			AddHideEvent(view, onHidedEnd);
		}

		private static void SetVisible(string resName, bool visible)
		{
			if (_resViewModelMap.ContainsKey(resName))
				_resViewModelMap[resName].IsShow = visible;
		}
#endregion // View

#region Event
		private static void AddShowEvent(GUIView view, Action onLoaded)
		{
			if (!_onShowEventMap.TryAdd(view.name, onLoaded))
				_onShowEventMap[view.name] = onLoaded;
			view.OnOpenedEvent -= OnShowEnd;
			view.OnOpenedEvent += OnShowEnd;
		}

		private static void AddHideEvent(GUIView view, Action onHided)
		{
			if (!_onHideEventMap.TryAdd(view.name, onHided))
				_onHideEventMap[view.name] = onHided;
			view.OnClosedEvent -= OnHideEnd;
			view.OnClosedEvent += OnHideEnd;
		}
		private static void OnShowEnd(GUIView view)
		{
			if (_onShowEventMap.ContainsKey(view.name))
				_onShowEventMap[view.name]?.Invoke();
		}

		private static void OnHideEnd(GUIView view)
		{
			if (_onHideEventMap.ContainsKey(view.name))
				_onHideEventMap[view.name]?.Invoke();
		}
#endregion // Event

#region Timer
	public void SetSearchTimer(Action onTimerEnd)
	{
		var timer = GetSearchTimer();
		UIManager.Instance.StartTimer(timer, SearchDelayTime, onTimerEnd);
	}

	public void ResetSearchTimer() => _searchTimer?.Reset();
	private UIManager.Timer GetSearchTimer() => _searchTimer ??= new UIManager.Timer();
#endregion // Timer
#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod]
		private static void ClearAll()
		{
			_views.Clear();
			_requests.Clear();
		}
#endif // UNITY_EDITOR
	}
}
