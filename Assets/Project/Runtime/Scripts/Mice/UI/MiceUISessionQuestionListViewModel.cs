/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUISessionQuestionListViewModel.cs
* Developer:	sprite
* Date:			2023-05-17 16:08
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Cysharp.Threading.Tasks;
using Com2Verse.Logger;
using System;
using Com2Verse.UI;
using JetBrains.Annotations;
using Com2Verse.Extension;

namespace Com2Verse.Mice
{
    [ViewModelGroup("Mice")]
    public sealed partial class MiceUISessionQuestionListViewModel : MiceViewModel  // Main
    {
        public enum PopupAssets
        {
            UI_SessionQuestionList,
        }

        private MiceSessionQuestionInfo _info;

        public MiceUISessionQuestionListViewModel(MiceSessionQuestionInfo info)
        {
            _info = info;
        }

        private static void PrepareViewModel(GUIView view)
        {
            var info = MiceInfoManager.Instance.SessionQuestionInfo;

            view.ViewModelContainer
                .UniqueAddViewModel
                (
                    () =>
                    {
                        var infoViewModel = new MiceSessionQuestionInfoViewModel(info);
                        infoViewModel.RegisterNestedViewModels<MiceSessionQuestionInfo.Item>(data => new MiceUISessionQuestionListItemViewModel(data));
                        return infoViewModel;
                    }
                )
                .UniqueAddViewModel(() => new MiceUISessionQuestionListViewModel(info));
        }

        public static async UniTask<GUIView> ShowView(Action<GUIView> onShow = null, Action<GUIView> onHide = null, PopupAssets asset = PopupAssets.UI_SessionQuestionList)
        {
            await MiceInfoManager.Instance.SessionQuestionInfo.Sync();

            if (MiceInfoManager.Instance.SessionQuestionInfo.IsEmpty)
            {
                // 팝업 오픈 조건이 아닌 경우, 툴바의 해당 버튼을 끄도록 한다.
                ViewModelManager.Instance.GetOrAdd<MiceToolBarViewModel>().IsQuestionListVisible = false;

                Data.Localization.eKey.MICE_UI_SessionHall_PreQuestion_Msg_NoData.ShowAsToast();
                return null;
            }

            return await MiceViewModel.ShowView(asset.ToString(), MiceUISessionQuestionListViewModel.PrepareViewModel, onShow, onHide);
        }
    }

    public class MiceSessionQuestionListManager : Singleton<MiceSessionQuestionListManager>
    {
        private GUIView _sessionQuestionListView;
        private MiceUISessionQuestionListViewModel.PopupAssets _popupAsset;

        [UsedImplicitly] private MiceSessionQuestionListManager() { }

        private bool _isBusy = false;

        private async UniTask Show(MiceUISessionQuestionListViewModel.PopupAssets asset)
        {
            if (!_sessionQuestionListView.IsUnityNull())
            {
                if (_popupAsset == asset) return;
                this.Hide();
            }

            if (_isBusy) return;

            try
            {
                _isBusy = true;

                _sessionQuestionListView = await MiceUISessionQuestionListViewModel
                    .ShowView
                    (
                        onHide: _ =>
                        {
                            this.Hide();

                            // 팝업을 직접 닫는 경우, 툴바의 해당 버튼을 끄도록 한다.
                            ViewModelManager.Instance.GetOrAdd<MiceToolBarViewModel>().IsQuestionListVisible = false;
                        },
                        asset: asset
                    );
                _popupAsset = asset;
            }
            finally
            {
                _isBusy = false;
            }
        }

        public UniTask Show() => this.Show(MiceUISessionQuestionListViewModel.PopupAssets.UI_SessionQuestionList);

        public void Hide()
        {
            if (!_sessionQuestionListView.IsUnityNull())
            {
                _sessionQuestionListView.Hide();
                _sessionQuestionListView = null;
            }
        }
    }
}



