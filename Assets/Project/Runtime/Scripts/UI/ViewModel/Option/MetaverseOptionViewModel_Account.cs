/*===============================================================
* Product:		Com2Verse
* File Name:	MetaverseOptionViewModel_Account.cs
* Developer:	mikeyid77
* Date:			2023-04-18 15:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Hive;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Option;
using Cysharp.Threading.Tasks;
using hive;
using JetBrains.Annotations;
using UnityEngine;
using User = Com2Verse.Communication.User;

namespace Com2Verse.UI
{
	public partial class MetaverseOptionViewModel
	{
		private string WithdrawalTitle   => Localization.Instance.GetString("UI_Common_Notice_Popup_Title");
		private string WithdrawalContent => Localization.Instance.GetString("UI_Setting_Account_PersonalData_Withdrawal_Popup_Desc");
		private string WithdrawalCancel  => Localization.Instance.GetString("UI_Common_Btn_Cancel");
		private string WithdrawalConnect => Localization.Instance.GetString("UI_Setting_Account_PersonalData_Withdrawal_Popup_Btn");

		public enum eProviderType
		{
			Google,
			HIVE,
			Facebook,
			Apple
		}

		private AccountOption                       _accountOption;
		private Collection<ProviderViewModel>       _providerCollection       = new();
		private Collection<CustomProviderViewModel> _customProviderCollection = new();
		public  CommandHandler<int>                 AlertCountToggleClicked     { get; private set; }
		public  CommandHandler                      CopyCsCodeButtonClicked     { get; private set; }
		public  CommandHandler                      ServiceButtonClicked        { get; private set; }
		public  CommandHandler                      AlertButtonClicked          { get; private set; }
		public  CommandHandler                      WithdrawalButtonClicked     { get; private set; }
		public  CommandHandler                      PasswordChangeButtonClicked { get; private set; }
		public  CommandHandler                      PersonalInfoButtonClicked   { get; private set; }
		public  CommandHandler                      InquiryButtonClicked        { get; private set; }
		public  CommandHandler                      TermsButtonClicked          { get; private set; }
		public  CommandHandler                      PersonalTermsButtonClicked  { get; private set; }

		private bool _isAccountOn;
		public bool IsAccountOn
		{
			get => _isAccountOn;
			set
			{
				_isAccountOn = value;
				InvokePropertyValueChanged(nameof(IsAccountOn), IsAccountOn);
			}
		}

		public Collection<ProviderViewModel> ProviderCollection
		{
			get => _providerCollection;
			set
			{
				_providerCollection = value;
				base.InvokePropertyValueChanged(nameof(ProviderCollection), ProviderCollection);
			}
		}

		public Collection<CustomProviderViewModel> CustomProviderCollection
		{
			get => _customProviderCollection;
			set
			{
				_customProviderCollection = value;
				base.InvokePropertyValueChanged(nameof(CustomProviderCollection), CustomProviderCollection);
			}
		}

		public string CsCode
		{
			get => Network.User.Instance.CurrentUserData.PlayerId.ToString();
			set => C2VDebug.LogWarningCategory("AccountOption", $"Can't Set CsCode");
		}

		[UsedImplicitly]
		public int AlertCountIndex
		{
			get => _accountOption.AlarmCountIndex;
			set
			{
				if (_accountOption == null) return;
				_accountOption.AlarmCountIndex = value;
				base.InvokePropertyValueChanged(nameof(AlertCountIndex), AlertCountIndex);
			}
		}

		private void InitializeAccountOption()
		{
			_isAccountOn                = true;
			_accountOption              = OptionController.Instance.GetOption<AccountOption>();
			AlertCountToggleClicked     = new CommandHandler<int>(OnAlertCountToggleClicked);
			CopyCsCodeButtonClicked     = new CommandHandler(OnCopyCsCodeButtonClicked);
			ServiceButtonClicked        = new CommandHandler(OnServiceButtonClicked);
			AlertButtonClicked          = new CommandHandler(OnAlertButtonClicked);
			WithdrawalButtonClicked     = new CommandHandler(OnWithdrawalButtonClicked);
			PasswordChangeButtonClicked = new CommandHandler(OnPasswordChangeButtonClicked);
			PersonalInfoButtonClicked   = new CommandHandler(OnPersonalInfoButtonClicked);
			InquiryButtonClicked        = new CommandHandler(OnInquiryButtonClicked);
			TermsButtonClicked          = new CommandHandler(OnTermsButtonClicked);
			PersonalTermsButtonClicked  = new CommandHandler(OnPersonalTermsButtonClicked);
			AlertCountIndex             = _accountOption.AlarmCountIndex;

			foreach (eProviderType target in Enum.GetValues(typeof(eProviderType)))
			{
				_providerCollection.AddItem(new ProviderViewModel(target));
			}
		}

		private void OnAlertCountToggleClicked(int index)
		{
			if (AlertCountIndex != index) AlertCountIndex = index;
		}

		private void OnCopyCsCodeButtonClicked()
		{
			GUIUtility.systemCopyBuffer = CsCode;
			UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Setting_Account_CSCode_Copy_Msg"));
		}

		private void OnServiceButtonClicked()
		{
			if (!LoginManager.Instance.IsHiveLogin()) return;

			UIManager.Instance.CreatePopup("UI_Option_Account_ServicesList", (guiView) =>
			{
				guiView.Show();
			}).Forget();
		}

		private void OnAlertButtonClicked()
		{
			if (!LoginManager.Instance.IsHiveLogin()) return;

			UIManager.Instance.CreatePopup("UI_Option_Account_NotificationSettings", (guiView) =>
			{
				guiView.Show();
			}).Forget();
		}

		private void OnWithdrawalButtonClicked()
		{
			var popupAddress = "UI_Option_Withdrwal_Notice";

			UIManager.Instance.CreatePopup(popupAddress, (createdGuiView) =>
			{
				createdGuiView.Show();

				var viewModel = createdGuiView.ViewModelContainer.GetViewModel<CommonPopupYesNoViewModel>();
				viewModel.GuiView    = createdGuiView;
				viewModel.OnYesEvent = (guiView) => OnInquiryButtonClicked();
			}).Forget();
		}

		private void OnPasswordChangeButtonClicked()
		{
			C2VDebug.LogCategory("AccountOption", $"{nameof(OnPasswordChangeButtonClicked)}");
		}

		private void OnPersonalInfoButtonClicked()
		{
			C2VDebug.LogCategory("AccountOption", $"{nameof(OnPersonalInfoButtonClicked)}");
		}

		private void OnInquiryButtonClicked()
		{
			C2VDebug.LogCategory("AccountOption", $"{nameof(OnInquiryButtonClicked)}");
			Application.OpenURL("mailto:com2versecs@com2us.com");
		}

		private void OnTermsButtonClicked()
		{
			C2VDebug.LogCategory("AccountOption", $"{nameof(OnTermsButtonClicked)}");
			HiveSDKHelper.ShowTerms();
		}

		private void OnPersonalTermsButtonClicked()
		{
			C2VDebug.LogCategory("AccountOption", $"{nameof(OnPersonalTermsButtonClicked)}");
			HiveSDKHelper.ShowCustomView("300254");
			// TODO : 환경별로 CustomViewId 수정 필요
		}
	}
}
