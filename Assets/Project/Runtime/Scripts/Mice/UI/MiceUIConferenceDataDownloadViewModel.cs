/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUIConferenceDataDownloadViewModel.cs
* Developer:	seaman2000
* Date:			2023-06-02 16:05
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Cysharp.Threading.Tasks;
using System;
using Com2Verse.UI;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Com2Verse.Mice
{
    [ViewModelGroup("Mice")]
    public sealed partial class  MiceUIConferenceDataDownloadViewModel : MiceViewModel
    {
        private static readonly string UI_ASSET = "UI_Conference_DataDownload";


        private Collection<MiceUIConferenceDataDwonloadDataNameViewModel> _dataNameCollection = new();

        private bool _selectAllData = false;
        private GUIView _myView = null;
        private int _selectedCount = 0;
        private bool _selectedAllFileButtonActive;
        private bool _unselectedAllFileButtonActive;


        public CommandHandler SelectedAllFileButton { get; private set; }

        public CommandHandler UnSelectedAllFileButton { get; private set; }

        public CommandHandler SelectedFileDownloadButton { get; private set; }



        public bool SelectAllData
        {
            get => _selectAllData;
            set => SetProperty(ref _selectAllData, value);
        }

        public int SelectedCount
        {
            get => _selectedCount;
            set => SetProperty(ref _selectedCount, value);
        }

        public bool SelectedAllFileButtonActive
        {
            get => _selectedAllFileButtonActive;
            set => SetProperty(ref _selectedAllFileButtonActive, value);
        }

        public bool UnselectedAllFileButtonActive
        {
            get => _unselectedAllFileButtonActive;
            set => SetProperty(ref _unselectedAllFileButtonActive, value);
        }

        public Collection<MiceUIConferenceDataDwonloadDataNameViewModel> DataNameCollection
        {
            get => _dataNameCollection;
            set => SetProperty(ref _dataNameCollection, value);
        }



        public static async UniTask<GUIView> ShowPopup(Action<GUIView> onShow = null, Action<GUIView> onHide = null)
        {
            GUIView view = await UI_ASSET.AsGUIView();

            void OnOpenedEvent(GUIView view)
            {
                onShow?.Invoke(view);
            }

            void OnClosedEvent(GUIView view)
            {
                onHide?.Invoke(view);

                view.OnOpenedEvent -= OnOpenedEvent;
                view.OnClosedEvent -= OnClosedEvent;
            }

            view.OnOpenedEvent += OnOpenedEvent;
            view.OnClosedEvent += OnClosedEvent;

            view.Show();

            return view;
        }


        public MiceUIConferenceDataDownloadViewModel()
        {
            // InitCommandHandler
            this.SelectedAllFileButton = new CommandHandler(OnClickSelectAllFileButton);
            this.UnSelectedAllFileButton = new CommandHandler(OnClickUnSelectAllFileButton);
            this.SelectedFileDownloadButton = new CommandHandler(OnClickSelectedFileDownloadButton);
        }

        void OnClickSelectAllFileButton()
        {
            foreach (var entry in this._dataNameCollection.Value) { entry.IsSelected = true; }
            RefreshItemSelected();
        }

        void OnClickUnSelectAllFileButton()
        {
            foreach (var entry in this._dataNameCollection.Value) { entry.IsSelected = false; }
            RefreshItemSelected();
        }

        async void OnClickSelectedFileDownloadButton()
        {
            bool showNoticePopup = false;
            var datapath = Utils.Path.Downloads;

            foreach (var entry in this._dataNameCollection.Value)
            {
                if (!entry.IsSelected) continue;
                var result = await DownloadFile(datapath, entry.AttachedData.StrName, entry.AttachedData.FileUrl);

                if(result) { showNoticePopup = true; }
            }

            if(showNoticePopup)
            {
                var text = Data.Localization.eKey.MICE_UI_SessionHall_FileDownload_Msg_Success.ToLocalizationString();
                UIManager.Instance.ShowPopupCommon(text,
                () => { OpenInFileBrowser.Open(datapath); });
            }
        }

        public void SyncData(GUIView myView)
        {
            _myView = myView;

            var sessionInfo = MiceService.Instance.GetCurrentSessionInfo();
            if (sessionInfo == null) return;

            this.DataNameCollection.Reset();
            foreach (var entry in sessionInfo.AttachmentFiles)
            {
                var item = new MiceUIConferenceDataDwonloadDataNameViewModel();
                item.SetInfo(entry, this.RefreshItemSelected);
                this.DataNameCollection.AddItem(item);
            }

            RefreshItemSelected();
        }

        public void RefreshItemSelected()
        {
            int count = 0;
            foreach (var entry in this.DataNameCollection.Value)
            {
                if (entry.IsSelected) count++;
            }
            SelectedCount = count;

            this.SelectedAllFileButtonActive = count == 0;
            this.UnselectedAllFileButtonActive = count != 0;

        }

        async UniTask<bool> DownloadFile(string folderPath, string fileName, string url)
        {
            // directory 유무 체크
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // 동일 파일 유무 체크 있다면 추가로 (index)를 붙힌다.
            var onlyFileName = Path.GetFileNameWithoutExtension(fileName);
            var onlyExtension = Path.GetExtension(fileName);
            int index = 0;
            string add = "";
            string filePath = string.Empty;
            while (true)
            {
                filePath = Path.Combine(folderPath + "/", onlyFileName + add + onlyExtension);
                if (!File.Exists(filePath)) break;

                index++;
                add = $"({index})";
            }

            // 다운로드.
            var wr = UnityEngine.Networking.UnityWebRequest.Get(url);
            await wr.SendWebRequest();
            if (!wr.isDone && wr.result != UnityEngine.Networking.UnityWebRequest.Result.Success) return false;

            // 파일 저장
            File.WriteAllBytes(filePath, wr.downloadHandler.data);

            return true;
        }


    }
}

