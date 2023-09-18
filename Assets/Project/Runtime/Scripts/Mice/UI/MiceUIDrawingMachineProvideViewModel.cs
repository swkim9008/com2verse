/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUIDrawingMachinePrivacyViewModel.cs
* Developer:	seaman2000
* Date:			2023-08-30 14:28
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.UI;
using Com2Verse.Mice;
using Cysharp.Threading.Tasks;
using Com2Verse.Network;
using System;

namespace Com2Verse
{
    [ViewModelGroup("Mice")]
    public sealed class MiceUIDrawingMachineProvideViewModel : MiceViewModel
    {
        private static readonly string UI_ASSET = "UI_Popup_DrawingMachine_Provide";

        private PrizeInfo _prizeInfo;


        public MiceUIDrawingMachineProvideViewModel()
        {
        }


        public static async UniTask<GUIView> ShowView(PrizeInfo info, Action<GUIView> onShow = null, Action<GUIView> onHide = null)
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

            if (view.ViewModelContainer.TryGetViewModel(typeof(MiceUIDrawingMachineProvideViewModel), out var viewModel))
            {
                var prizeViewModel = viewModel as MiceUIDrawingMachineProvideViewModel;
                prizeViewModel?.SyncData(info);
            }

            return view;
        }


        public void SyncData(PrizeInfo prizeInfo)
        {
            _prizeInfo = prizeInfo;
        }
    }
}
