/*===============================================================
* Product:		Com2Verse
* File Name:	MyPadViewModel.cs
* Developer:	tlghks1009
* Date:			2022-08-23 10:34
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Utils;
using Com2VerseEditor;
using Newtonsoft.Json;

namespace Com2Verse.UI
{
    [ViewModelGroup("MyPad")]
    public sealed class MyPadViewModel : ViewModelBase
    {
        private static readonly string DeletedAppKey = "DELETED_APPID_MYPAD";

        private          int       _maxPage      = 0;
        private readonly int       _maxItemCount = 28;
        private          bool      _isFirstPage  = true;
        private          bool      _isLastPage   = false;
        private          bool      _isVisibleScrollSnap;
        private          string    _currentTime;
        private          string    _currentDay;
        private          int       _currentPage = 0;

        private List<string> _myPadDeletedAppList = new();
        private Collection<MyPadPageViewModel> _myPadPageCollection = new();

        public CommandHandler CloseButtonClicked   { get; }
        public CommandHandler ProfileButtonClicked { get; }
        public CommandHandler AvatarButtonClicked  { get; }
        public CommandHandler ChatBotButtonClicked { get; }
        public CommandHandler SettingButtonClicked { get; }
        public CommandHandler LogoutButtonClicked  { get; }
        public CommandHandler QuitButtonClicked    { get; }

        public MyPadViewModel()
        {
            // TODO : 삭제한 앱 리스트 불러오기 (임시)
            // var jsonData = LocalSave.Temp.LoadString(DeletedAppKey);
            // if (!string.IsNullOrEmpty(jsonData))
            // {
            //     _myPadDeletedAppList = JsonConvert.DeserializeObject<List<string>>(jsonData);
            // }
            
            CloseButtonClicked   = new CommandHandler(OnCloseButtonClicked);
            ProfileButtonClicked = new CommandHandler(OnProfileButtonClicked);
            AvatarButtonClicked  = new CommandHandler(OnAvatarButtonClicked);
            ChatBotButtonClicked = new CommandHandler(OnChatBotButtonClicked);
            SettingButtonClicked = new CommandHandler(OnSettingButtonClicked);
            LogoutButtonClicked  = new CommandHandler(OnLogoutButtonClicked);
            QuitButtonClicked    = new CommandHandler(OnQuitButtonClicked);

            SetupMyPadPages();
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
            MyPadManager.Instance.OnCurrentTimeEvent += OnTimeChecked;
            _myPadPageCollection.Value[0].RefreshMyPadNotice();
        }
        
        public override void OnRelease()
        {
            // TODO : 삭제한 앱 리스트 저장 (임시)
            // var toJson = JsonConvert.SerializeObject(_myPadDeletedAppList);
            // LocalSave.Temp.SaveString(DeletedAppKey, toJson);
            
            base.OnRelease();
        }

#region Property
        public string Time
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        public string Day
        {
            get => _currentDay;
            set => SetProperty(ref _currentDay, value);
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => RefreshPageTrigger(value);
        }

        public bool IsFirstPage
        {
            get => _isFirstPage;
            set => SetProperty(ref _isFirstPage, value);
        }
                
        public bool IsLastPage
        {
            get => _isLastPage;
            set => SetProperty(ref _isLastPage, value);
        }
                
        public bool IsVisibleScrollSnap
        {
            get => _isVisibleScrollSnap;
            set => SetProperty(ref _isVisibleScrollSnap, value);
        }

        public Collection<MyPadPageViewModel> MyPadPageCollection
        {
            get => _myPadPageCollection;
            set => SetProperty(ref _myPadPageCollection, value);
        }
#endregion // Property

#region Command
        private void ClickMyPadItemEvent(string id)
        {
            MyPadManager.Instance.PlayAction(id);
        }

        private void RemoveMyPadItemEvent(string id)
        {
            MyPadManager.Instance.CheckBeforeRemoveItem(id);
        }
                
        public void AddMyPadItem()
        {
            foreach (var viewModel in _myPadPageCollection.Value)
                viewModel.AddMyPadItem();
            _myPadPageCollection.Reset();
                 
            _myPadDeletedAppList.Clear();
            SetupMyPadPages();
        }
                
        public void RefreshMyPadItem()
        {
            foreach (var viewModel in _myPadPageCollection.Value)
                viewModel.RefreshMyPadItem();
        }
                
        public void RemoveMyPadItem(string id)
        {
            foreach (var viewModel in _myPadPageCollection.Value)
                viewModel.RemoveMyPadItem(id);
            _myPadDeletedAppList.Add(id);
        }

        public void ResetMyPadItem()
        {
            foreach (var viewModel in _myPadPageCollection.Value)
                viewModel.ResetMyPadItem();
        }

        private void OnCloseButtonClicked()
        {
            
        }
        
        private void OnProfileButtonClicked()
        {
            
        }
        
        private void OnAvatarButtonClicked()
        {
            MyPadManager.Instance.PlayAction(MyPadManager.eBasicApp.AVATAR);
        }
        
        private void OnChatBotButtonClicked()
        {
            MyPadManager.Instance.PlayAction(MyPadManager.eBasicApp.CHATBOT);
        }
        
        private void OnSettingButtonClicked()
        {
            MyPadManager.Instance.PlayAction(MyPadManager.eBasicApp.SETTING);
        }

        private void OnLogoutButtonClicked()
        {
            UIManager.Instance.ShowPopupYesNo(MyPadString.LogoutPopupTitle,
                                              MyPadString.LogoutPopupContext,
                                              (view) =>
                                              {
                                                  LoginManager.Instance.Logout();
                                              }, null, null, 
                                              MyPadString.CommonPopupYes,
                                              MyPadString.CommonPopupNo);
        }
        
        private void OnQuitButtonClicked()
        {
            UIManager.Instance.ShowPopupYesNo(MyPadString.QuitPopupTitle,
                                              MyPadString.QuitPopupContext,
                                              (view) =>
                                              {
#if UNITY_EDITOR
                                                  EditorApplicationUtil.ExitPlayMode();
#else
                                                  Application.Quit();
#endif
                                              }, null, null, 
                                              MyPadString.CommonPopupYes,
                                              MyPadString.CommonPopupNo);
        }
#endregion // Command

#region Utils
        private void SetupMyPadPages()
        {
            var myPadItemList = new List<MyPad>();
            MyPadManager.Instance.AddAllMyPadItemInList(myPadItemList);

            var sortItemList = myPadItemList?.OrderBy(myPadItem => myPadItem.Number).ToList();
            var toCreatePageCount = (int) (sortItemList.Count / _maxItemCount) + 1;
            _maxPage = toCreatePageCount;
            _isLastPage = (toCreatePageCount == 1);

            int startIndex = 0;
            int endIndex = _maxItemCount;

            var currentPage = 1;
            while (currentPage <= toCreatePageCount)
            {
                C2VDebug.LogCategory("MyPad", $"{nameof(SetupMyPadPages)} : {currentPage}");
                _myPadPageCollection.AddItem(new MyPadPageViewModel(currentPage, _myPadDeletedAppList, sortItemList, startIndex, endIndex, ClickMyPadItemEvent, RemoveMyPadItemEvent));
                
                startIndex += endIndex;
                endIndex += endIndex + 1;

                currentPage++;
            }
        }

        private void RefreshPageTrigger(int page)
        {
            IsFirstPage = (page == 0);
            IsLastPage = (page == _maxPage - 1);
        }
        
        private void OnTimeChecked(string timer, string day)
        {
            if (string.IsNullOrEmpty(timer)) return;
            if (string.IsNullOrEmpty(day)) return;
            
            var format = timer.Split(' ');
            if (format.Length < 4) return;

            Day  = MyPadString.DisplayDateFormat(format[0], format[1], format[2], day);
            Time = format[3];
        }
#endregion // Utils
    }
}
