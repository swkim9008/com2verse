/*===============================================================
 * Product:		Com2Verse
 * File Name:	NoticeItemViewModel.cs
 * Developer:	yangsehoon
 * Date:		2022-12-07 오전 11:51
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.AssetSystem;
using Com2Verse.Data;
using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("Notice")]
	public class NoticeItemViewModel : ViewModelBase
	{
		// private NoticeBoard _postItem;
		private bool _tabActive;

		public bool TabActive
		{
			get => _tabActive;
			set
			{
				_tabActive = value;
				InvokePropertyValueChanged(nameof(TabActive), TabActive);
			}
		}

		// public NoticeBoard PostItem
		// {
		// 	set
		// 	{
		// 		_postItem = value;
		// 		Organization = _postItem.Organization;
		// 		PostTitle = _postItem.PostTitle;
		// 		NTag = _postItem.NTag;
		// 		AuthorName = _postItem.AuthorName;
		// 		AuthorTitle = _postItem.AuthorTitle;
		// 		AuthorOrganization = _postItem.AuthorOrganization;
		// 		DateTimeFull = _postItem.DateTime;
		// 		DateTimeOnlyDate = _postItem.DateTime.Split(' ')[0];
		// 		BodyText1 = _postItem.BodyText1?.Replace(@"\n", "\n");
		// 		BodyText2 = _postItem.BodyText2?.Replace(@"\n", "\n");
		// 		if (!string.IsNullOrEmpty(_postItem.BodyImgPath))
		// 		{
		// 			var handle = C2VAddressables.LoadAssetAsync<Sprite>(_postItem.BodyImgPath);
		// 			handle.OnCompleted += (op) =>
		// 			{
		// 				BodyImg = op.Result;
		// 				InvokePropertyValueChanged(nameof(BodyImg));
		// 			};
		// 		}
		//
		//
		// 		InvokePropertyValueChanged(nameof(Organization));
		// 		InvokePropertyValueChanged(nameof(PostTitle));
		// 		InvokePropertyValueChanged(nameof(NTag));
		// 		InvokePropertyValueChanged(nameof(AuthorName));
		// 		InvokePropertyValueChanged(nameof(AuthorTitle));
		// 		InvokePropertyValueChanged(nameof(AuthorOrganization));
		// 		InvokePropertyValueChanged(nameof(DateTimeOnlyDate));
		// 		InvokePropertyValueChanged(nameof(DateTimeFull));
		// 		InvokePropertyValueChanged(nameof(BodyText1));
		// 		InvokePropertyValueChanged(nameof(BodyText2));
		// 		InvokePropertyValueChanged(nameof(HasBodyText1));
		// 		InvokePropertyValueChanged(nameof(HasBodyText2));
		// 		InvokePropertyValueChanged(nameof(DetailName));
		// 		InvokePropertyValueChanged(nameof(HasBodyImg));
		// 	}
		// }

		public string Organization { get; private set; } = string.Empty;
		public string PostTitle { get; private set; } = string.Empty;
		public bool NTag { get; private set; } = false;
		public string AuthorName { get; private set; } = string.Empty;
		public string AuthorTitle { get; private set; } = string.Empty;
		public string AuthorOrganization { get; private set; } = string.Empty;
		public string DateTimeOnlyDate { get; private set; } = string.Empty;
		public string DateTimeFull { get; private set; } = string.Empty;
		public string BodyText1 { get; private set; } = string.Empty;
		public string BodyText2 { get; private set; } = string.Empty;
		public bool HasBodyText1 => !string.IsNullOrEmpty(BodyText1);
		public bool HasBodyText2 => !string.IsNullOrEmpty(BodyText2);
		public bool HasBodyImg => BodyImg != null;
		public Sprite BodyImg { get; private set; } = null;
		public string DetailName => $"{AuthorName} {AuthorTitle} / {AuthorOrganization}";
		public Vector2 ScrollToTop { get; } = Vector2.zero;

		public CommandHandler CommandOnClickNoticeItem { get; }
		public CommandHandler CommandOnClickBack { get; }

		private void OnClickNoticeItem()
		{
			// NoticeManager.Instance.ShowNotice(_postItem.PK);
		}

		private void OnClickBack()
		{
			// NoticeManager.Instance.ShowNoticeList(_postItem.MapId, true);
		}

		public void InitializeScroll()
		{
			InvokePropertyValueChanged(nameof(ScrollToTop), ScrollToTop);
		}
		
		public NoticeItemViewModel()
		{
			CommandOnClickNoticeItem = new(OnClickNoticeItem);
			CommandOnClickBack = new(OnClickBack);
		}
	}
}
