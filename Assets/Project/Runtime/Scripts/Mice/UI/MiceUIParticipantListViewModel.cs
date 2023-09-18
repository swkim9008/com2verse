/*===============================================================
* Product:		Com2Verse
* File Name:	MiceParticipantListViewModel.cs
* Developer:	sprite
* Date:			2023-04-12 11:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Cysharp.Threading.Tasks;
using Com2Verse.Logger;
using System;
using Com2Verse.UI;
using Com2Verse.Extension;
using JetBrains.Annotations;
using Com2Verse.Network;
using System.Collections.Generic;
using System.Threading;

namespace Com2Verse.Mice
{
	[ViewModelGroup("Mice")]
	public sealed partial class MiceUIParticipantListViewModel : MiceViewModel  // Main
    {
        public enum PopupAssets
        {
            UI_ParticipantList,
            UI_Conference_ParticipantList
        }

        partial void InitProperties();
        partial void InitCommandHandler();

        public MiceUIParticipantListViewModel(MiceParticipantInfo info)
        {
            _info = info;
            _info.OnDataChange += this.OnDataChange;

            this.InitProperties();
            this.InitCommandHandler();

            //this.InitUpdateRequestProcess();
        }

        public override void OnRelease()
        {
            base.OnRelease();

            _info.OnDataChange -= this.OnDataChange;

            this.ClearSearch();

            //this.ClearUpdateRequestProcess();
        }

        private void OnDataChange()
        {
            InvokePropertyValueChanged(nameof(Title), Title);
            InvokePropertyValueChanged(nameof(UserCount), UserCount);
        }

        private static void PrepareViewModel(GUIView view)
        {
            var info = MiceInfoManager.Instance.ParticipantInfo;

            view.ViewModelContainer
                .UniqueAddViewModel
                (
                    () =>
                    {
                        var infoViewModel = new MiceParticipantInfoViewModel(info);
                        infoViewModel.RegisterNestedViewModels<MiceParticipantInfo.Item>(data => new MiceUIParticipantListItemViewModel(data));
                        return infoViewModel;
                    }
                )
                .UniqueAddViewModel(() => new MiceUIParticipantListViewModel(info));

            var vm = view.ViewModelContainer.GetViewModel<MiceUIParticipantListViewModel>();
            vm.ClearSearch();

            //var self = view.ViewModelContainer.GetViewModel<MiceUIParticipantListViewModel>();
            //self._guiViewToken = view.GetCancellationTokenOnDestroy();
        }

        /// <summary>
        /// 참여자 목록 표시.
        /// </summary>
        /// <returns></returns>
        public static async UniTask<GUIView> ShowView(Action<GUIView> onShow = null, Action<GUIView> onHide = null, PopupAssets asset = PopupAssets.UI_ParticipantList)
        {
            await MiceInfoManager.Instance.ParticipantInfo.Sync();

            GUIView view = await asset.AsGUIView();

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

            MiceUIParticipantListViewModel.PrepareViewModel(view);

            view.Show();

            return view;
        }
       
    }

    public sealed partial class MiceUIParticipantListViewModel    // Properties
    {
        /// <summary>
        /// 뷰 제목.
        /// </summary>
        public string Title => Data.Localization.eKey.MICE_UI_SessionHall_Attendance_Popup_Title.ToLocalizationString(_info.ContentsCount, _info.MaxContentsCount);
        /// <summary>
        /// 사용자 명수.
        /// </summary>
        public string UserCount => $"{_info.ContentsCount}/{_info.MaxContentsCount}";
        /// <summary>
        /// 입력된 찾기 텍스트.
        /// </summary>
        public string InputSearchText 
        {
            get => _inputSearchText;
            set
            {
                SetProperty(ref _inputSearchText, value);
                InvokePropertyValueChanged(nameof(IsExistSearchText), IsExistSearchText);

                C2VDebug.Log($"[InputField]<ValueChanged> '{_inputSearchText}'");
            }
        }
        /// <summary>
        /// 입력 텍스트가 존재하는지 여부.
        /// </summary>
        public bool IsExistSearchText => !string.IsNullOrWhiteSpace(InputSearchText);

        private MiceParticipantInfo _info;
        private string _title;
        private string _inputSearchText;

        partial void InitProperties()
        {
            this.InputSearchText = string.Empty;
        }
    }

    public sealed partial class MiceUIParticipantListViewModel    // Command Handlers
    {
        public CommandHandler InputFieldClearButton { get; private set; }
        public CommandHandler InputFieldSearchButton { get; private set; }
        public CommandHandler MoreContentsButton { get; private set; }
        public CommandHandler RefreshContentsButton { get; private set; }

        partial void InitCommandHandler()
        {
            this.InputFieldClearButton = new CommandHandler(this.OnClickInputFieldClearButton);
            this.InputFieldSearchButton = new CommandHandler(this.OnClickInputFieldSearchButton);
            this.MoreContentsButton = new CommandHandler(this.OnClickMoreContentsButton);
            this.RefreshContentsButton = new CommandHandler(this.OnClickRefreshContentsButton);
        }

        private void OnClickInputFieldClearButton()
        {
            this.ClearSearch();
        }

        private void OnClickInputFieldSearchButton()
        {
            _info.SearchBy(this.InputSearchText).Forget();
        }

        private void OnClickMoreContentsButton()
        {
            this.RefreshContentsAndMore().Forget();
        }

        private void OnClickRefreshContentsButton()
        {
            this.RefreshContents().Forget();
        }

        private void ClearSearch()
        {
            InputSearchText = string.Empty;
            _info.SearchBy(null).Forget();
        }

        private bool _busyByRefreshContents = false;

        private async UniTask RefreshContents(Action preRefreh = null, Action postRefresh = null)
        {
            if (_busyByRefreshContents)
            {
                C2VDebug.LogCategory("RefreshContents", "Already refreshing!");
                return;
            }

            try
            {
                _busyByRefreshContents = true;

                C2VDebug.LogCategory("RefreshContents", "Refreshing...");

                preRefreh?.Invoke();

                await MiceInfoManager.Instance.ParticipantInfo.Sync();

                postRefresh?.Invoke();
            }
            finally
            {
                C2VDebug.LogCategory("RefreshContents", "Done.");

                _busyByRefreshContents = false;
            }
        }

        private UniTask RefreshContentsAndMore()
        {
            if (_busyByRefreshContents) return UniTask.CompletedTask;

            int lastContentsCount = _info.ContentsCount;
            return this.RefreshContents(postRefresh: () =>
            {
                C2VDebug.LogCategory("RefreshContentsAndMore", $"Last Contents Count = {lastContentsCount}");
                _info.More(lastContentsCount);
            });
        }
    }

    public class MiceParticipantListManager : Singleton<MiceParticipantListManager>
    {
        private GUIView _participantListView;
        private MiceUIParticipantListViewModel.PopupAssets _popupAsset;

        [UsedImplicitly] private MiceParticipantListManager() { }

        private async UniTask Show(MiceUIParticipantListViewModel.PopupAssets asset)
        {
            if (!_participantListView.IsUnityNull())
            {
                if (_popupAsset == asset) return;
                this.Hide();
            }

            _participantListView = await MiceUIParticipantListViewModel
                .ShowView
                (
                    asset: asset,
                    onHide: _ =>
                    {
                        this.Hide();

                        // 팝업을 직접 닫는 경우, 툴바의 해당 버튼을 끄도록 한다.
                        ViewModelManager.Instance.GetOrAdd<ToolBarViewModel>().IsParticipantLayout = false;
                        ViewModelManager.Instance.GetOrAdd<MiceToolBarViewModel>().IsParticipantListVisible = false;
                    }
                );
            _popupAsset = asset;
        }

        public UniTask ShowAsNormal() => this.Show(MiceUIParticipantListViewModel.PopupAssets.UI_ParticipantList);
        public UniTask ShowAsConference() => this.Show(MiceUIParticipantListViewModel.PopupAssets.UI_Conference_ParticipantList);

        public void Hide()
        {
            if (!_participantListView.IsUnityNull())
            {
                _participantListView.Hide();
                _participantListView = null;
            }
        }
    }
}
