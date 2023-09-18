/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUIPrizeDrawingMachineResultViewModel.cs
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
    public sealed class MiceUIPrizeDrawingMachineResultViewModel : MiceViewModel
    {
        private static readonly string UI_ASSET = "UI_Popup_DrawingMachine_Result";

        private bool _hasPrize;
        private Texture _prizeImage;
        private string _prizeName;
        private Network.PrizeInfo _prizeInfo;


        public CommandHandler CommandButtonConfirm { get; }


        public bool HasPrize
        {
            get => _hasPrize;
            set
            {
                _hasPrize = value;
                base.InvokePropertyValueChanged(nameof(HasPrize), HasPrize);
            }
        }

        public Texture PrizeImage
        {
            get => _prizeImage;
            set => SetProperty(ref _prizeImage, value);
        }

        public String PrizeName
        {
            get => _prizeName;
            set => SetProperty(ref _prizeName, value);
        }


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

            if (view.ViewModelContainer.TryGetViewModel(typeof(MiceUIPrizeDrawingMachineResultViewModel), out var viewModel))
            {
                var prizeViewModel = viewModel as MiceUIPrizeDrawingMachineResultViewModel;
                prizeViewModel?.SyncData(prizeInfo);
            }

            return view;
        }

        public MiceUIPrizeDrawingMachineResultViewModel()
        {
            CommandButtonConfirm = new CommandHandler(OnClickConfirm);
        }

        void OnClickConfirm()
        {
            if (_prizeInfo == null) return;

            // 배송정보를 요청한다.
            if(_prizeInfo.PersonalInfoNeeded)
            {
                string str = Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Win_Connecting_Popup_Msg.ToLocalizationString();

                // 이번행사 적용임 (None이라면 Spaxe타입 상품이다.)
                if (_prizeInfo.ReceiveType == MiceWebClient.eMicePrizeReceiveTypeCode.RECEIVE_NONE)
                {
                    MiceUIPrizeDrawingMachineResultInfoViewModel.ShowView(this._prizeInfo).Forget();
                }
                else
                {
                    MiceUIPrizeDrawingMachineInputInfoViewModel.ShowView(this._prizeInfo).Forget();
                }
                
            }
        }

        public void SyncData(Network.PrizeInfo prizeInfo)
        {
            _prizeInfo = prizeInfo;
            HasPrize = prizeInfo.HasPrize;

            if (HasPrize)
            {
                if(!string.IsNullOrEmpty(prizeInfo.ItemPhoto))
                {
                    PrizeImage = PrizeImage.GetOrDownloadTexture(prizeInfo.ItemPhoto, tex => PrizeImage = tex);
                }

                PrizeName = $"- {prizeInfo.ItemName} -";
            }

            Sound.SoundManager.Instance.PlayUISound(HasPrize ? "SE_MICE_Gacha_Win.wav" : "SE_MICE_Gacha_Fail.wav");
        }
    }
}
