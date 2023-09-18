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
    public sealed class MiceUIDrawingMachinePrivacyViewModel : MiceViewModel
    {
        private static readonly string UI_ASSET = "UI_Popup_DrawingMachine_Privacy";

        private string _collectionPurpose;
        private string _collectionItems;
        private string _collectionPeriod;
        private bool _layoutRebuild;

        private PrizeInfo _prizeInfo;

        public string CollectionPurpose
        {
            get => _collectionPurpose;
            set => SetProperty(ref _collectionPurpose, value);
        }

        public string CollectionItems
        {
            get => _collectionItems;
            set => SetProperty(ref _collectionItems, value);
        }

        public string CollectionPeriod
        {
            get => _collectionPeriod;
            set => SetProperty(ref _collectionPeriod, value);
        }

        public bool LayoutRebuild
        {
            get => _layoutRebuild;
            set => SetProperty(ref _layoutRebuild, value);
        }
        

        public MiceUIDrawingMachinePrivacyViewModel()
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

            if (view.ViewModelContainer.TryGetViewModel(typeof(MiceUIDrawingMachinePrivacyViewModel), out var viewModel))
            {
                var prizeViewModel = viewModel as MiceUIDrawingMachinePrivacyViewModel;
                prizeViewModel?.SyncData(info);
            }

            return view;
        }


        public void SyncData(PrizeInfo prizeInfo)
        {
            _prizeInfo = prizeInfo;

            switch(_prizeInfo.PrizePrivacyAgreeType)
            {
                case MiceWebClient.eMicePrizePrivacyAgreeTypeCode.PRIVACY_TYPE_1: // 타입1
                    this.CollectionPurpose = Data.Localization.eKey.MICE_UI_Popup_CapsuleDrawingMachine_Terms_Privacy_SubMsg_Purpose.ToLocalizationString();
                    this.CollectionItems = Data.Localization.eKey.MICE_UI_Popup_CapsuleDrawingMachine_Terms_Privacy_SubMsg_Type1.ToLocalizationString(); ;
                    this.CollectionPeriod = Data.Localization.eKey.MICE_UI_Popup_CapsuleDrawingMachine_Terms_Privacy_SubMsg_6.ToLocalizationString(); ;
                    break;
                case MiceWebClient.eMicePrizePrivacyAgreeTypeCode.PRIVACY_TYPE_3: // 타입3
                    this.CollectionPurpose = Data.Localization.eKey.MICE_UI_Popup_CapsuleDrawingMachine_Terms_Privacy_SubMsg_Purpose.ToLocalizationString();
                    this.CollectionItems = Data.Localization.eKey.MICE_UI_Popup_CapsuleDrawingMachine_Terms_Privacy_SubMsg_Type3.ToLocalizationString(); ;
                    this.CollectionPeriod = Data.Localization.eKey.MICE_UI_Popup_CapsuleDrawingMachine_Terms_Privacy_SubMsg_6.ToLocalizationString(); ;
                    break;
                case MiceWebClient.eMicePrizePrivacyAgreeTypeCode.PRIVACY_TYPE_4: // 타입4
                    this.CollectionPurpose = Data.Localization.eKey.MICE_UI_Popup_CapsuleDrawingMachine_Terms_Privacy_SubMsg_Purpose.ToLocalizationString();
                    this.CollectionItems = Data.Localization.eKey.MICE_UI_Popup_CapsuleDrawingMachine_Terms_Privacy_SubMsg_Type4.ToLocalizationString(); ;
                    this.CollectionPeriod = Data.Localization.eKey.MICE_UI_Popup_CapsuleDrawingMachine_Terms_Privacy_SubMsg_6.ToLocalizationString(); ;
                    break;
            }
        }
    }
}
