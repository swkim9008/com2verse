/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUIPrizeDrawingMachineResultInfoViewModel.cs
* Developer:	seaman2000
* Date:			2023-07-14 10:22
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Cysharp.Threading.Tasks;
using Com2Verse.UI;
using System;

namespace Com2Verse.Mice
{
    [ViewModelGroup("Mice")]
    public sealed class MiceUIPrizeDrawingMachineResultInfoViewModel : MiceViewModel
    {
        private static readonly string UI_ASSET = "UI_Popup_DrawingMachine_ResultInfo";

        public CommandHandler CommandButtonConfirm { get; }

        private Network.PrizeInfo _prizeInfo;

        public static async UniTask<GUIView> ShowView(Network.PrizeInfo prizeInfo, Action<GUIView> onShow = null, Action<GUIView> onHide = null)
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

            if (view.ViewModelContainer.TryGetViewModel(typeof(MiceUIPrizeDrawingMachineResultInfoViewModel), out var viewModel))
            {
                var prizeViewModel = viewModel as MiceUIPrizeDrawingMachineResultInfoViewModel;
                prizeViewModel?.SyncData(prizeInfo);
            }

            return view;
        }

        public MiceUIPrizeDrawingMachineResultInfoViewModel()
        {
            CommandButtonConfirm = new CommandHandler(OnClickConfirm);
        }


        public void SyncData(Network.PrizeInfo prizeInfo)
        {
            _prizeInfo = prizeInfo;

            _prizeInfo?.PersonalInfoSendComplete();
        }

        void OnClickConfirm()
        {
            if (_prizeInfo == null) return;
        }
    }
}
