/*===============================================================
* Product:		Com2Verse
* File Name:	NotificationItemViewModel.cs
* Developer:	tlghks1009
* Date:			2022-10-06 17:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Notification;
using Newtonsoft.Json;
using Protocols.Notification;
using UnityEngine;

namespace Com2Verse.UI
{
    [ViewModelGroup("Notification")]
    public sealed class NotificationItemViewModel : ViewModel
    {
        private const string ATLAS_NAME = "Atlas_MyPad";
        
        private Action _onNotificationTweenFinished;
        private bool _isVisible;
        private int _viewSDescStringLength = 150;
        private NotificationInfo _notificationInfo;
        public NotificationInfo NotificationInfo
        {
            get => _notificationInfo;
            set => _notificationInfo = value;
        }
        public bool RefreshViewBg { get; set; }
        public CommandHandler Command_TweenFinished       { get; }
        public CommandHandler Command_CloseButtonClick    { get; }
        public CommandHandler Command_AcceptButtonClick   { get; }
        public CommandHandler Command_RefuseButtonClick   { get; }
        public CommandHandler Command_WebViewButtonClick { get; }
        public CommandHandler Command_MoveButtonClick     { get; }

        public NotificationItemViewModel()
        {
            Command_TweenFinished = new CommandHandler(OnCommand_TweenFinished);
            Command_CloseButtonClick = new CommandHandler(OnCommand_CloseButtonClicked);
            Command_AcceptButtonClick = new CommandHandler(OnCommand_AcceptButtonClicked);
            Command_RefuseButtonClick = new CommandHandler(OnCommand_RefuseButtonClicked);
            Command_WebViewButtonClick = new CommandHandler(OnCommand_WebViewButtonClicked);
            Command_MoveButtonClick = new CommandHandler(OnCommand_MoveButtonClicked);
        }

        public void Initialize(NotificationInfo notificationInfo)
        {
            NotificationInfo = notificationInfo;
            RefreshView();
        }

        public void Activate() => IsVisibleState = true;

        public void Deactivate() => IsVisibleState = false;

        public bool IsVisibleState
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        private string _notificationTitle;
        public string NotificationTitle
        {
            get => _notificationTitle;
            set => SetProperty(ref _notificationTitle, value);
        }
        
        private string _description;
        public string NotificationDescription
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
        
        private Sprite _notificationicon;
        public Sprite NotificationIcon
        {
            get => _notificationicon;
            set => SetProperty(ref _notificationicon, value);
        }

        private bool _isSelectButtonState;

        public bool IsSelectButtonState
        {
            get => _isSelectButtonState;
            set => SetProperty(ref _isSelectButtonState, value);
        }
        
        private bool _isSelectButtonOn;

        public bool IsSelectButtonOn
        {
            get => _isSelectButtonOn;
            set => SetProperty(ref _isSelectButtonOn, value);
        }
        
        private bool _isWebViewButtonOn;

        public bool IsWebViewButtonOn
        {
            get => _isWebViewButtonOn;
            set => SetProperty(ref _isWebViewButtonOn, value);
        }
        
        private bool _isMoveButtonOn;

        public bool IsMoveButtonOn
        {
            get => _isMoveButtonOn;
            set => SetProperty(ref _isMoveButtonOn, value);
        }

        private string _acceptButtonText;
        public string AcceptButtonText
        {
            get => _acceptButtonText;
            set => SetProperty(ref _acceptButtonText, value);
        }
        
        private string _refuseButtonText;
        public string RefuseButtonText
        {
            get => _refuseButtonText;
            set => SetProperty(ref _refuseButtonText, value);
        }
        
        private string _webViewButtonText;
        public string WebViewButtonText
        {
            get => _webViewButtonText;
            set => SetProperty(ref _webViewButtonText, value);
        }
        
        private string _moveButtonText;
        public string MoveButtonText
        {
            get => _moveButtonText;
            set => SetProperty(ref _moveButtonText, value);
        }

        public string NotificationReceivedDateTime => NotificationInfo.NotificationData.AlarmRegisterTime.ToLocalTime().ToString("MM-dd tt hh:mm");
        public bool IsVisibleUserInteractionButton => NotificationInfo.NotificationData.IsFeedBack();

        private void RefreshView()
        {
            InvokePropertyValueChanged(nameof(NotificationReceivedDateTime), NotificationReceivedDateTime);
            InvokePropertyValueChanged(nameof(IsVisibleUserInteractionButton), IsVisibleUserInteractionButton);

            RefreshViewBg = true;
            RefreshViewData();
        }

        private void RefreshViewData()
        {
            NotificationDescription = GetDescription();
            NotificationTitle = GetTitleName();
            NotificationIcon = GetIcon();
            
            switch (NotificationInfo.NotificationData.NotificationType)
            {
                case NotificationNotifyType.Select:
                    IsSelectButtonOn = true;
                    if (!_notificationInfo.NotificationData.NotificationDetail[0].Button.Equals(""))
                    {
                        var buttonString = _notificationInfo.NotificationData.NotificationDetail[0].Button.Replace(" ", string.Empty).Split(',');
                        if(buttonString == null)
                            break;

                        if (buttonString.Length == 1)
                        {
                            AcceptButtonText = buttonString[0];
                            RefuseButtonText = buttonString[0];
                        }
                        else
                        { 
                            AcceptButtonText = buttonString[0];
                            RefuseButtonText = buttonString[1];
                        }
                    }
                    break;
                case NotificationNotifyType.MoveBuilding:
                case NotificationNotifyType.MoveObject:
                case NotificationNotifyType.MoveSpace:
                    IsMoveButtonOn = true;
                    MoveButtonText = _notificationInfo.NotificationData.NotificationDetail[0].Button;
                    break;
                case NotificationNotifyType.Webview:
                    IsWebViewButtonOn = true;
                    WebViewButtonText = _notificationInfo.NotificationData.NotificationDetail[0].Button;
                    break;
            }

            IsSelectButtonState = NotificationInfo.IsSelectButtonState;
        }

        private void OnCommand_CloseButtonClicked()
        {
            NotificationInfo.ForceTimeOut();
        }

        private void OnCommand_TweenFinished()
        {
            _onNotificationTweenFinished?.Invoke();
            _onNotificationTweenFinished = null;
        }

        private void OnCommand_AcceptButtonClicked()
        {
            _notificationInfo.SelectAction?.Invoke(_notificationInfo.NotificationData, 0);
        }

        private void OnCommand_RefuseButtonClicked()
        {
            _notificationInfo.SelectAction?.Invoke(_notificationInfo.NotificationData, 1);
        }

        private void OnCommand_WebViewButtonClicked()
        {
            _notificationInfo.WebViewAction?.Invoke(_notificationInfo);
        }

        private void OnCommand_MoveButtonClicked()
        {
            _notificationInfo.MoveAction?.Invoke(_notificationInfo);
        }
        
        private Sprite GetIcon() => NotificationIcon = SpriteAtlasManager.Instance.GetSprite(ATLAS_NAME, _notificationInfo.NotificationData.AlarmIcon);
        private string GetTitleName() => NotificationTitle = _notificationInfo.NotificationData.NotificationDetail[0].Title;
        private string GetDescription()
        {
            if (!_notificationInfo.IsPopUpNotification)
                return NotificationManager.TrimStringWithLength(_notificationInfo.NotificationData.NotificationDetail[0].TextMessage, _viewSDescStringLength);
            else
                return _notificationInfo.NotificationData.NotificationDetail[0].TextMessage;
        }

        #region Event
        public event Action OnNotificationTweenFinished
        {
            add
            {
                _onNotificationTweenFinished -= value;
                _onNotificationTweenFinished += value;
            }
            remove => _onNotificationTweenFinished -= value;
        }
#endregion Event
    }
}