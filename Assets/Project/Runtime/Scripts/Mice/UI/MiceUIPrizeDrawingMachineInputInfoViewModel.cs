/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUIPrizeDrawingMachineInputInfoViewModel.cs
* Developer:	seaman2000
* Date:			2023-07-14 10:22
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Com2Verse.UI;
using System;
using Com2Verse.Logger;
using TMPro;
using static Com2Verse.Mice.MiceWebClient;
using System.Text;

namespace Com2Verse.Mice
{
    [ViewModelGroup("Mice")]
    public sealed class MiceUIPrizeDrawingMachineInputInfoViewModel : MiceViewModel
    {
        private static readonly string UI_ASSET = "UI_Popup_DrawingMachine_InputInfo";

        private string _recipientWinnerName;
        private string _recipientPhoneNumber;
        private string _recipientAddress;
        private string _recipientAddressDetail;
        private string _recipientEmail;
        private string _recipientEmailDomain;

        // 이름, 휴대전화 , 이메일 , 집주소
        private bool _showName;
        private bool _showPhoneNumber;
        private bool _showAddress;
        private bool _showAddressDetail;
        private bool _showEmail;

        private string _InfoText;

        // toggle
        private bool _consentToProvidePersonalInfomation;
        private bool _consentToProvideToThirdParty;

        //// email dropdown
        private List<TMP_Dropdown.OptionData> _emailDropDownOption;
        private TMP_Dropdown.DropdownEvent _emailDropDownValueChangeEvent = new TMP_Dropdown.DropdownEvent();
        private int _emailDropDownIndex;

        private Network.PrizeInfo _prizeinfo;
        private GUIView _myView;

        public CommandHandler CommandButtonAddress { get; }
        public CommandHandler CommandButtonConfirm { get; }
        public CommandHandler CommandToggleConsentToProvidePersonalInfomation { get; }
        public CommandHandler CommandToggleConsentToProvideToThirdParty { get; }

        public CommandHandler CommandButtonShowPrivacy { get; }
        public CommandHandler CommandButtonShowProvide { get; }



        public string RecipientWinnerName
        {
            get => _recipientWinnerName;
            set
            {
                _recipientWinnerName = value;
                base.InvokePropertyValueChanged(nameof(RecipientWinnerName), RecipientWinnerName);
            }
        }
        public string RecipientPhoneNumber
        {
            get => _recipientPhoneNumber;
            set
            {
                _recipientPhoneNumber = value;
                base.InvokePropertyValueChanged(nameof(RecipientPhoneNumber), RecipientPhoneNumber);
            }
        }

        public string RecipientAddress
        {
            get => _recipientAddress;
            set
            {
                _recipientAddress = value;
                base.InvokePropertyValueChanged(nameof(RecipientAddress), RecipientAddress);
            }
        }

        public string RecipientAddressDetail
        {
            get => _recipientAddressDetail;
            set
            {
                _recipientAddressDetail = value;
                base.InvokePropertyValueChanged(nameof(RecipientAddressDetail), RecipientAddressDetail);
            }
        }

        public string RecipientEmail
        {
            get => _recipientEmail;
            set
            {
                _recipientEmail = value;
                base.InvokePropertyValueChanged(nameof(RecipientEmail), RecipientEmail);
            }
        }
       
        public bool ConsentToProvidePersonalInfomation
        {
            get => _consentToProvidePersonalInfomation;
            set
            {
                _consentToProvidePersonalInfomation = value;
                base.InvokePropertyValueChanged(nameof(ConsentToProvidePersonalInfomation), ConsentToProvidePersonalInfomation);
            }
        }

        public bool ConsentToProvideToThirdParty
        {
            get => _consentToProvideToThirdParty;
            set
            {
                _consentToProvideToThirdParty = value;
                base.InvokePropertyValueChanged(nameof(ConsentToProvideToThirdParty), ConsentToProvideToThirdParty);
            }
        }

        public bool ShowName
        {
            get => _showName;
            set
            {
                _showName = value;
                base.InvokePropertyValueChanged(nameof(ShowName), ShowName);
            }
        }

        public bool ShowPhoneNumber
        {
            get => _showPhoneNumber;
            set
            {
                _showPhoneNumber = value;
                base.InvokePropertyValueChanged(nameof(ShowPhoneNumber), ShowPhoneNumber);
            }
        }

        public bool ShowAddress
        {
            get => _showAddress;
            set
            {
                _showAddress = value;
                base.InvokePropertyValueChanged(nameof(ShowAddress), ShowAddress);
            }
        }


        public bool ShowAddressDetail
        {
            get => _showAddressDetail;
            set
            {
                _showAddressDetail = value;
                base.InvokePropertyValueChanged(nameof(ShowAddressDetail), ShowAddressDetail);
            }
        }

        public bool ShowEmail
        {
            get => _showEmail;
            set
            {
                _showEmail = value;
                base.InvokePropertyValueChanged(nameof(ShowEmail), ShowEmail);
            }
        }

        public string Infotext
        {
            get => _InfoText;
            set => SetProperty(ref _InfoText, value);
        }
       
        public List<TMP_Dropdown.OptionData> EmailDropDownOption
        {
            get => _emailDropDownOption;
            set => SetProperty(ref _emailDropDownOption, value);
        }


        public TMP_Dropdown.DropdownEvent EmailDropDownValueChangeEvent
        {
            get => _emailDropDownValueChangeEvent;
            set => SetProperty(ref _emailDropDownValueChangeEvent, value);
        }

        public int EmailDropDownIndex
        {
            get => _emailDropDownIndex;
            set
            {
                _emailDropDownIndex = value;
                base.InvokePropertyValueChanged(nameof(EmailDropDownIndex), EmailDropDownIndex);
            }
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

            if (view.ViewModelContainer.TryGetViewModel(typeof(MiceUIPrizeDrawingMachineInputInfoViewModel), out var viewModel))
            {
                var prizeViewModel = viewModel as MiceUIPrizeDrawingMachineInputInfoViewModel;
                prizeViewModel?.SyncData(view, prizeInfo);
            }

            return view;
        }


        public MiceUIPrizeDrawingMachineInputInfoViewModel()
        {
            CommandToggleConsentToProvidePersonalInfomation = new CommandHandler(() => ConsentToProvidePersonalInfomation ^= true);
            CommandToggleConsentToProvideToThirdParty = new CommandHandler(() => ConsentToProvideToThirdParty ^= true);

            CommandButtonAddress = new CommandHandler(OnClickPostCode);
            CommandButtonConfirm = new CommandHandler(OnClickConfirm );

            EmailDropDownOption = new List<TMP_Dropdown.OptionData>();
            EmailDropDownValueChangeEvent.RemoveListener(OnDropDownValueChange);
            EmailDropDownValueChangeEvent.AddListener(OnDropDownValueChange);

            CommandButtonShowPrivacy = new CommandHandler(OnClickPrivacy);
            CommandButtonShowProvide = new CommandHandler(OnClickProvide);
        }

        void OnClickPrivacy()
        {
            MiceUIDrawingMachinePrivacyViewModel.ShowView(_prizeinfo).Forget();
        }

        void OnClickProvide()
        {
            MiceUIDrawingMachineProvideViewModel.ShowView(_prizeinfo).Forget();
        }

        async void OnClickPostCode()
        {
            var data = await UIManager.Instance.ShowPopupPostCodeWebView();

            if (!string.IsNullOrEmpty(data.postcode))
            {
                RecipientAddress = $"({data.postcode}) {data.address}";
                if (!string.IsNullOrEmpty(data.extraAddress))
                {
                    RecipientAddress += $"{data.extraAddress}";
                }
                this.ShowAddressDetail = data.isValid;
            }

            C2VDebug.LogCategory("TestPostCode", $"(valid:{data.isValid}) postcode:'{data.postcode}', address:'{data.address}', extra:'{data.extraAddress}'");
        }

        async void OnClickConfirm()
        {
            if (_prizeinfo == null) return;

            // 조건을 체크한다.
            if (!PassValidFieldDatasWithNotice()) return;

            var result = await SendPersonalInfo();
            
            if (!result) return;

            // 정상 입력 완료 팝업
            UIManager.Instance.ShowPopupConfirm
            (
                Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_Title_RequiredDone.ToLocalizationString(),
                Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_Msg_RequiredDone.ToLocalizationString(),
                null,
                Data.Localization.eKey.MICE_UI_Common_Btn_Ok.ToLocalizationString()
            );

            _myView?.Hide();

        }

        private async UniTask<bool> SendPersonalInfo()
        {
            if (_prizeinfo == null) return false;

            // 주소를 합친다.
            string totalAddress = "";
            if(!string.IsNullOrEmpty(this.RecipientAddressDetail))
                totalAddress = $"{this.RecipientAddress} {this.RecipientAddressDetail}";
            else
                totalAddress = this.RecipientAddress;

            var response = await MiceWebClient.Prize.PersonalInfoPost(new MiceWebClient.Entities.PrizePersonalInfo()
            {
                PrizeId = _prizeinfo.PrizeId,
                PrizeItemId = (long)_prizeinfo.PrizeItemId,
                PrizeItemSeq = (int)_prizeinfo.PrizeItemIdSeq,
                WinnerName = this.RecipientWinnerName,
                PhoneNum = this.RecipientPhoneNumber,
                Email = $"{this.RecipientEmail}@{_recipientEmailDomain}",
                Address = totalAddress
            });

            // 정보 동기화
            if(response)
            {
                _prizeinfo.PersonalInfoSendComplete();
                return true;
            }

            // TODO : 추후 에러메세지가 확정되면 작업
            //// error process
            //switch (response.Result.MiceStatusCode)
            //{
            //    case eMiceHttpErrorCode.PRIZE_PERSONAL_NEED_NAME:
            //        UIManager.Instance.SendToastMessage("PRIZE_PERSONAL_NEED_NAME", 3f, UIManager.eToastMessageType.WARNING);
            //        return false;
            //    case eMiceHttpErrorCode.PRIZE_PERSONAL_NEED_PHONE:
            //        UIManager.Instance.SendToastMessage("PRIZE_PERSONAL_NEED_PHONE", 3f, UIManager.eToastMessageType.WARNING);
            //        return false;
            //    case eMiceHttpErrorCode.PRIZE_PERSONAL_NEED_EMAIL:
            //        UIManager.Instance.SendToastMessage("PRIZE_PERSONAL_NEED_EMAIL", 3f, UIManager.eToastMessageType.WARNING);
            //        return false;
            //    case eMiceHttpErrorCode.PRIZE_PERSONAL_NEED_ADDRESS:
            //        UIManager.Instance.SendToastMessage("PRIZE_PERSONAL_NEED_ADDRESS", 3f, UIManager.eToastMessageType.WARNING);
            //        return false;
            //}

            response.ShowErrorMessage();

            return false;
        }

        public void SyncData(GUIView view, Network.PrizeInfo prizeInfo)
        {
            _myView = view;
            _prizeinfo = prizeInfo;

            // toogle 초기화
            this.ConsentToProvidePersonalInfomation = false;
            this.ConsentToProvideToThirdParty = false;

            this.ShowAddressDetail = false;

            this.RecipientWinnerName = "";
            this.RecipientPhoneNumber = "";
            this.RecipientEmail = "";
            this.RecipientAddress = "";
            this.RecipientAddressDetail = "";

            // 비트연산.ㅋ
            this.ShowName = 0 < (prizeInfo.ReceiveType & MiceWebClient.eMicePrizeReceiveTypeCode.RECEIVE_NAME);
            this.ShowPhoneNumber = 0 < (prizeInfo.ReceiveType & MiceWebClient.eMicePrizeReceiveTypeCode.RECEIVE_PHONE);
            this.ShowEmail = 0 < (prizeInfo.ReceiveType & MiceWebClient.eMicePrizeReceiveTypeCode.RECEIVE_EMAIL);
            this.ShowAddress = 0 < (prizeInfo.ReceiveType & MiceWebClient.eMicePrizeReceiveTypeCode.RECEIVE_ADDRESS);

            var emailDomainList = MiceBusinessCardBookViewModel.GetEmailDomainList();

            EmailDropDownOption.Clear();
            foreach(var entry in emailDomainList)
            {
                EmailDropDownOption.Add(new TMP_Dropdown.OptionData(entry));
            }

            // 초기화한다.
            EmailDropDownIndex = 0;
            OnDropDownValueChange(0);

            // 인포 텍스트 출력
            StringBuilder sb = new StringBuilder();

            switch(prizeInfo.PrizePrivacyAgreeType)
            {
                case eMicePrizePrivacyAgreeTypeCode.PRIVACY_TYPE_1:
                    sb.AppendLine(Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_SubMsg_AdditionalInfo_1.ToLocalizationString());
                    sb.AppendLine(Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_SubMsg_AdditionalInfo_2.ToLocalizationString());
                    sb.AppendLine(Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_AdditionalInfoPopup_Notice_RealThing.ToLocalizationString());
                    sb.AppendLine(Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_AdditionalInfoPopup_Notice_Taxes_1.ToLocalizationString());
                    sb.AppendLine(Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_AdditionalInfoPopup_Notice_Taxes_2.ToLocalizationString());
                    sb.AppendLine(Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_AdditionalInfoPopup_Notice_Taxes_3.ToLocalizationString());
                    break;
                case eMicePrizePrivacyAgreeTypeCode.PRIVACY_TYPE_3:
                    sb.AppendLine(Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_SubMsg_AdditionalInfo_1.ToLocalizationString());
                    sb.AppendLine(Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_SubMsg_AdditionalInfo_2.ToLocalizationString());
                    sb.AppendLine(Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_AdditionalInfoPopup_Notice_RealThing.ToLocalizationString());
                    break;
                case eMicePrizePrivacyAgreeTypeCode.PRIVACY_TYPE_4:
                    sb.AppendLine(Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_SubMsg_AdditionalInfo_1.ToLocalizationString());
                    sb.AppendLine(Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_SubMsg_AdditionalInfo_2.ToLocalizationString());
                    break;
            }

            this.Infotext = sb.ToString();
        }

        void OnDropDownValueChange(int index)
        {
            _recipientEmailDomain = _emailDropDownOption[index].text;
        }

        bool PassValidFieldDatasWithNotice()
        {
            bool validInfo = true;

            // 데이타.
            if(this.ShowName)
            {
                if (string.IsNullOrEmpty(_recipientWinnerName)) validInfo = false;
            }

            if (this.ShowPhoneNumber)
            {
                if (string.IsNullOrEmpty(_recipientPhoneNumber)) validInfo = false;
            }

            if(this.ShowEmail)
            {
                if (string.IsNullOrEmpty(_recipientEmail)) validInfo = false;
                if (string.IsNullOrEmpty(_recipientEmailDomain)) validInfo = false;
            }

            if (this.ShowAddress)
            {
                if (string.IsNullOrEmpty(_recipientAddress)) validInfo = false;
                if (string.IsNullOrEmpty(_recipientAddressDetail)) validInfo = false;
            }

            if(!validInfo)
            {
                UIManager.Instance.ShowPopupConfirm
                (
                    Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_Title_Required.ToLocalizationString(),
                    Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_Msg_Required.ToLocalizationString(),
                    null,
                    Data.Localization.eKey.MICE_UI_Common_Btn_Ok.ToLocalizationString()
                );
                return false;
            }


            // 개인 정보 제공에 동의
            if (!ConsentToProvidePersonalInfomation)
            {
                UIManager.Instance.ShowPopupConfirm
                (
                    Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_Title_AgreeTerms.ToLocalizationString(),
                    Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_Msg_PrivacyPolicy_1.ToLocalizationString(),
                    null,
                    Data.Localization.eKey.MICE_UI_Common_Btn_Ok.ToLocalizationString()
                );

                return false;
            }

            // 3자 정보 제공에 동의
            if (!ConsentToProvideToThirdParty)
            {
                UIManager.Instance.ShowPopupConfirm
                (
                    Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_Title_AgreeTerms.ToLocalizationString(),
                    Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_Msg_PrivacyPolicy_2.ToLocalizationString(),
                    null,
                    Data.Localization.eKey.MICE_UI_Common_Btn_Ok.ToLocalizationString()
                );
                return false;
            }


            return true;
        }
    }
}
