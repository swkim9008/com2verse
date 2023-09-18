/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUIPrizeDrawingMachineGiftInfoViewModel.cs
* Developer:	seaman2000
* Date:			2023-07-12 15:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Com2Verse.UI;
using System;
using Com2Verse.Network;

namespace Com2Verse.Mice
{
    [ViewModelGroup("Mice")]
    public sealed class MiceUIGachaMachineGiftInfoViewModel : MiceViewModel
    {
        private static readonly string UI_ASSET = "UI_Popup_DrawingMachine_GiftInfo";

        private Collection<UI_DrawingMachine_GiftList> _prizeCollection = new();

        public Collection<UI_DrawingMachine_GiftList> PrizeCollection
        {
            get => _prizeCollection;
            set => SetProperty(ref _prizeCollection, value);
        }


        public static async UniTask<GUIView> ShowView(List<PrizeInfo> infos, Action<GUIView> onShow = null, Action<GUIView> onHide = null)
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

            if (view.ViewModelContainer.TryGetViewModel(typeof(MiceUIGachaMachineGiftInfoViewModel), out var viewModel))
            {
                var prizeViewModel = viewModel as MiceUIGachaMachineGiftInfoViewModel;
                prizeViewModel?.SyncData(infos);
            }

            return view;
        }

        public MiceUIGachaMachineGiftInfoViewModel()
        {
        }

        public void SyncData(List<PrizeInfo> prizeInfoList)
        {
            this.PrizeCollection.Reset();

            foreach(var entry in prizeInfoList)
            {
                var item = new UI_DrawingMachine_GiftList();
                item.SyncData(entry).Forget();
                this.PrizeCollection.AddItem(item);
            }
        }
    }
}
