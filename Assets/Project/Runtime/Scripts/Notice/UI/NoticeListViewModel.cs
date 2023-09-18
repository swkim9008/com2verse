/*===============================================================
 * Product:		Com2Verse
 * File Name:	NoticeListViewModel.cs
 * Developer:	yangsehoon
 * Date:		2022-12-07 오전 11:51
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Data;
using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("Notice")]
	public class NoticeListViewModel : ViewModel
	{
		private bool _tabActive;
		private long _mapId;

		public bool TabActive
		{
			get => _tabActive;
			set
			{
				_tabActive = value;
				InvokePropertyValueChanged(nameof(TabActive), TabActive);
			}
		}

		public string TeamName
		{
			get
			{
				var result = OfficeInfo.Instance.GetFloorInfo(_mapId);
				if (result != null)
				{
					return Localization.Instance.GetString(result.DeptName);
				}

				return string.Empty;
			}
		}

		public string TitleBarName
		{
			get
			{
				// (FIXME) Remove these features.
				if (_mapId == NoticeManager.LobbyMapId)
					return Localization.Instance.GetString("UI_NoticeBoard_Title_CompanyNews");

				return Localization.Instance.GetString("UI_NoticeBoard_Title_Notice");
			}
		}

		public bool ShowTeamLabel
		{
			get
			{
				return _mapId != NoticeManager.LobbyMapId;
			}
		}

		public Vector2 ScrollToTop { get; } = Vector2.zero;

		public Collection<NoticeItemViewModel> PostList { get; private set; } = new Collection<NoticeItemViewModel>();

		public NoticeListViewModel()
		{
			InvokePropertyValueChanged(nameof(TitleBarName), TitleBarName);
		}

		public void InitializeScroll()
		{
			InvokePropertyValueChanged(nameof(ScrollToTop), ScrollToTop);
		}

		// public void SetPosts(long mapId, List<NoticeBoard> posts)
		// {
		// 	PostList.Reset();
		//
		// 	if (posts != null)
		// 	{
		// 		foreach (var post in posts)
		// 		{
		// 			NoticeItemViewModel itemViewModel = new NoticeItemViewModel();
		// 			itemViewModel.PostItem = post;
		// 			PostList.AddItem(itemViewModel);
		// 		}
		// 	}
		//
		// 	if (mapId != -1)
		// 		_mapId = mapId;
		//
		// 	InvokePropertyValueChanged(nameof(TitleBarName));
		// 	InvokePropertyValueChanged(nameof(PostList));
		// 	InvokePropertyValueChanged(nameof(TeamName));
		// 	InvokePropertyValueChanged(nameof(ShowTeamLabel));
		// }
	}
}
