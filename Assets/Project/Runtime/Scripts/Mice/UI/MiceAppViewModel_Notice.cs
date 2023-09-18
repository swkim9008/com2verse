/*===============================================================
* Product:		Com2Verse
* File Name:	MiceAppViewModel_Notice.cs
* Developer:	klizzard
* Date:			2023-07-17 15:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using Com2Verse.Mice;
using UnityEngine;

namespace Com2Verse.UI
{
	public partial class MiceAppViewModel //Notice
	{
#region Variables
		private Collection<MiceAppNoticeListItemViewModel> _noticeCollection = new();
		private MiceAppNoticeDetailViewModel               _noticeDetailViewModel;
#endregion

#region Properties
		public bool IsNoticeViewOn     => IsNoticeListView || IsNoticeDetailView;
		public bool IsNoticeListView   => ViewMode is eViewMode.NOTICE_LIST;
		public bool IsNoticeDetailView => ViewMode is eViewMode.NOTICE_DETAIL;

		public Collection<MiceAppNoticeListItemViewModel> NoticeCollection
		{
			get => _noticeCollection;
			set => SetProperty(ref _noticeCollection, value);
		}

		public MiceAppNoticeDetailViewModel NoticeDetailViewModel
		{
			get => _noticeDetailViewModel;
			set => SetProperty(ref _noticeDetailViewModel, value);
		}
#endregion

		partial void InitNoticeView()
		{
			NoticeDetailViewModel = new MiceAppNoticeDetailViewModel();
			NestedViewModels.Add(NoticeDetailViewModel);
		}

		partial void InvokeNoticeView()
		{
			if (IsNoticeListView)
				UpdateNoticeView();

			InvokePropertyValueChanged(nameof(IsNoticeViewOn),     IsNoticeViewOn);
			InvokePropertyValueChanged(nameof(IsNoticeListView),   IsNoticeListView);
			InvokePropertyValueChanged(nameof(IsNoticeDetailView), IsNoticeDetailView);
		}

		private void UpdateNoticeView()
		{
			NoticeCollection.Reset();
			foreach (var elem in MiceInfoManager.Instance.NoticeInfos)
			{
				var viewModel = new MiceAppNoticeListItemViewModel(elem.Value);
				viewModel.OnShowNoticeDetailView += OnShowNoticeDetailView;
				NoticeCollection.AddItem(viewModel);
			}

			InvokePropertyValueChanged(nameof(NoticeCollection), NoticeCollection);
		}

		private GUIView _miceNoticeView;
		private void OnShowNoticeDetailView(int noticeKey)
		{
			if (MiceInfoManager.Instance.NoticeInfos.TryGetValue(noticeKey, out var noticeInfo))
			{
				NoticeDetailViewModel.SetData(noticeInfo);
				//SetViewMode(eViewMode.NOTICE_DETAIL);

				MiceInfoManager.Instance.NoticeClickedPrefs.Click(noticeKey);

				Mice.RedDotManager.SetTrigger(Mice.RedDotManager.RedDotData.TriggerKey.ShowNotice);

				var url = MiceService.Instance.MakeMiceNoticeUrl(noticeKey);
				C2VDebug.LogCategory(GetType().Name, url);
				UIManager.Instance.ShowPopupMiceKioskWebView(new UnityEngine.Vector2(1300, 800), $"{url}",
					OnWebViewCreated, OnMessageEmitted);
				void OnWebViewCreated(GUIView createdView)
				{
					_miceNoticeView = createdView;
				}
	        
				void OnMessageEmitted(string message)
				{
					var kioskMessage = JsonUtility.FromJson<MiceKioskWebViewMessage>(message);
					if (Enum.TryParse<eMiceKioskWebViewMessageType>(kioskMessage.MessageType, out var messageType))
					{
						if (messageType == eMiceKioskWebViewMessageType.ClosePage)
						{
							UpdateNoticeView();
							_miceNoticeView.Hide();
						}
					}
				}
			}
		}
	}

	[ViewModelGroup("Mice")]
	public partial class MiceAppNoticeListItemViewModel : ViewModelBase
	{
#region Variables
		private int    _noticeKey;
		private string _noticeType;
		private string _noticeTitle;
		private string _noticeUpdateTime;

		private Action<int> _onShowNoticeDetailView;
#endregion

#region Properties
		public string NoticeType
		{
			get => _noticeType;
			set => SetProperty(ref _noticeType, value);
		}

		public string NoticeTitle
		{
			get => _noticeTitle;
			set => SetProperty(ref _noticeTitle, value);
		}

		public string NoticeUpdateTime
		{
			get => _noticeUpdateTime;
			set => SetProperty(ref _noticeUpdateTime, value);
		}

		public bool IsActiveRedDot => MiceInfoManager.Instance.NoticeClickedPrefs.IsNew(_noticeKey);

		public CommandHandler ShowNoticeDetailView { get; }

		public event Action<int> OnShowNoticeDetailView
		{
			add
			{
				_onShowNoticeDetailView -= value;
				_onShowNoticeDetailView += value;
			}
			remove => _onShowNoticeDetailView -= value;
		}
#endregion

#region Intialize
		public MiceAppNoticeListItemViewModel()
		{
			ShowNoticeDetailView = new CommandHandler(() => { _onShowNoticeDetailView?.Invoke(_noticeKey); });
		}

		public MiceAppNoticeListItemViewModel(MiceNoticeInfo noticeInfo) : this()
		{
			SetData(noticeInfo);
		}

		public virtual void SetData(MiceNoticeInfo noticeInfo)
		{
			_noticeKey = noticeInfo.NoticeEntity.BoardSeq;

			NoticeType       = noticeInfo.NoticeEntity.StrArticeType;
			NoticeTitle      = noticeInfo.NoticeEntity.ArticleTitle;
			NoticeUpdateTime = noticeInfo.NoticeEntity.UpdateDatetime.ToString();

			InvokePropertyValueChanged(nameof(IsActiveRedDot), IsActiveRedDot);
		}
#endregion
	}

	[ViewModelGroup("Mice")]
	public partial class MiceAppNoticeDetailViewModel : MiceAppNoticeListItemViewModel
	{
#region Variables
		private string _noticeDescription;
#endregion

#region Properties
		public string NoticeDescription
		{
			get => _noticeDescription;
			set => SetProperty(ref _noticeDescription, value);
		}
#endregion

#region Intialize
		public MiceAppNoticeDetailViewModel() : base() { }

		public MiceAppNoticeDetailViewModel(MiceNoticeInfo noticeInfo) : base(noticeInfo) { }

		public override void SetData(MiceNoticeInfo noticeInfo)
		{
			base.SetData(noticeInfo);

			NoticeDescription = noticeInfo.NoticeEntity.ArticleDescription;
		}
#endregion
	}
}
