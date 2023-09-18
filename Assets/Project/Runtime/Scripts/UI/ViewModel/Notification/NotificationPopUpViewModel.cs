/*===============================================================
* Product:		Com2Verse
* File Name:	NotificationMenuViewModel.cs
* Developer:	ydh
* Date:			2023-04-24 16:22
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using Com2Verse.InputSystem;
using Com2Verse.Notification;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.UI
{
	[ViewModelGroup("Notification")]
	public sealed class NotificationPopUpViewModel :ViewModel, IDisposable
	{
		private Collection<NotificationItemViewModel> _notificationItemCollection;
		public Collection<NotificationItemViewModel> NotificationItemCollection
		{
			get => _notificationItemCollection;
		}
		
		private UnityEvent _setBlockerEvent;
		private readonly string _notificationPreviewOpenAnimationName = "Notification_Preview_Open";
		private static readonly string NotificationPreviewCloseAnimationName = "Notification_Preview_Close";
		
		private AnimationPropertyExtensions _animationPropertyExtensions;

		public AnimationPropertyExtensions AnimationPropertyExtensions
		{
			get => _animationPropertyExtensions;
			set
			{
				_animationPropertyExtensions = value;
				_animationPropertyExtensions.SetFuntion(_notificationPreviewOpenAnimationName, _setBlockerEvent);
				_animationPropertyExtensions.SetFuntion(NotificationPreviewCloseAnimationName, _setBlockerEvent);
			}
		}

		private GameObject _this;

		public GameObject This
		{
			get => _this;
			set
			{
				_this = value;
				base.InvokePropertyValueChanged(nameof(This), value);
			}
		}

		private string _notificationPopupTitle;

		public string NotificationPopupTitle
		{
			get => _notificationPopupTitle;
			set => SetProperty(ref _notificationPopupTitle, value);
		}

		public void CurrentCount(int count) => NotificationPopupTitle = string.Format(Localization.Instance.GetString("UI_Notification_Title"), count, _maxViewCount);

		public bool PopupOnState;
		public CommandHandler Command_AllClear      { get; }
		public CommandHandler Command_SettingButton { get; }
		private readonly int _maxViewCount = 100;

		public NotificationPopUpViewModel()
		{
			Command_AllClear = new CommandHandler(OnCommand_AllClear);
			Command_SettingButton = new CommandHandler(OnCommand_SettingButton);

			if (_setBlockerEvent == null)
				_setBlockerEvent = new UnityEvent();

			_notificationItemCollection ??= new();
		}

		private void OnCommand_SettingButton()
		{
			UIManager.Instance.CreatePopup("UI_Popup_Option", (guiView) =>
			{
				guiView.Show();
				var viewModel = guiView.ViewModelContainer.GetViewModel<MetaverseOptionViewModel>();
				guiView.OnOpenedEvent += (guiView) => viewModel.ScrollRectEnable = true;
				guiView.OnClosedEvent += (guiView) => viewModel.ScrollRectEnable = false;

				viewModel.IsAccountOn = true;
			}).Forget();
		}
		
		public void AddNotification(NotificationInfo notificationInfo)
		{
			var viewModel = new NotificationItemViewModel();
			viewModel.Initialize(notificationInfo);
			viewModel.Activate();
			if (notificationInfo.NotificationData.StateCode == (int)NotificationInfo.NotificationState.ANSWER)
				viewModel.IsSelectButtonState = false;
			else
				viewModel.IsSelectButtonState = true;
			
			if(NotificationItemCollection.Value.Count >= _maxViewCount)
				NotificationItemCollection.RemoveItem(NotificationItemCollection.FirstItem());

			NotificationItemCollection.AddItem(viewModel);

			CurrentCount(NotificationItemCollection.CollectionCount);
		}

		public void RemoveNotification(NotificationItemViewModel viewModel) => NotificationItemCollection.RemoveItem(viewModel);
		
		private void OnCommand_AllClear()
		{
			for (int i = 0; i < NotificationItemCollection.Value.Count; ++i)
			{
				NotificationItemCollection.Value[i].Deactivate();
			}

			Reset();
		}
		
		public void Reset() => NotificationItemCollection.Reset();

		private void OnCommand_Close() => NotificationManager.Instance.HidePopUp();

		public void ClosePopUp()
		{
			if (!PopupOnState)
				return;

			_animationPropertyExtensions.AnimationName = NotificationPreviewCloseAnimationName;
			_animationPropertyExtensions.AnimationPlay = true;
			PopupOnState = false;
			UIStackManager.Instance.RemoveByObject(_this);
		}

		public void OpenPopUp()
		{
			_animationPropertyExtensions.AnimationName = _notificationPreviewOpenAnimationName;
			_animationPropertyExtensions.AnimationPlay = true;
			UIStackManager.Instance.AddByObject(_this, nameof(NotificationPopUpViewModel), NotificationManager.Instance.PopUpClosedAction,
			                                    eInputSystemState.CHARACTER_CONTROL, true);
		}

		public void RemoveNotification(NotificationInfo notificationInfo, Action onRemovedComplete)
		{
			var notificationItemViewModel = GetNotificationItemViewModel(notificationInfo);

			if (notificationItemViewModel == null)
			{
				return;
			}

			notificationInfo.Reset();

			notificationItemViewModel.OnNotificationTweenFinished += () =>
			{
				NotificationItemCollection.RemoveItem(notificationItemViewModel);

				onRemovedComplete?.Invoke();
			};

			notificationItemViewModel.Deactivate();
		}

		private NotificationItemViewModel GetNotificationItemViewModel(NotificationInfo notificationInfo)
		{
			foreach (var notificationItemView in NotificationItemCollection.Value)
			{
				if (notificationItemView.NotificationInfo == notificationInfo)
				{
					return notificationItemView;
				}
			}

			return null;
		}
		
		public void Dispose()
		{
			_notificationItemCollection?.Reset();
			_notificationItemCollection = null;
		}

		public NotificationInfo GetItemNotificationInfo(string id)
		{
			foreach (var item in _notificationItemCollection.Value)
			{
				if (id == item.NotificationInfo.NotificationData.NotificationId)
					return item.NotificationInfo;
			}	
			
			return null;
		}
	}
}