/*===============================================================
* Product:		Com2Verse
* File Name:	MiceBusinessCardViewModel.cs
* Developer:	wlemon
* Date:			2023-04-06 12:55
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.IO;
using Com2Verse.Mice;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using SimpleFileBrowser;
using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("Mice")]
	public class MiceBusinessCardViewModel : ViewModelBase
	{
		public static readonly string ResName = "UI_Popup_BusinessCard";

		public enum eViewMode
		{
			DEFAULT,
			CARD_ONLY,
		}

#region Variables
		private Texture      _image;
		private string       _name;
		private string       _englishName;
		private string       _affiliation;
		private string       _email;
		private string       _phone;
		private Texture      _qrCode;
		private string       _free1;
		private string       _free2;
		private eViewMode    _viewMode;
		private Transform    _cardRootTransform;
		private bool         _isExchangable;
		private MiceUserInfo _userInfo;

		public CommandHandler SaveImage { get; }
		public CommandHandler Exchange  { get; }
#endregion

#region Properties
		public Texture Image
		{
			get => _image;
			set
			{
				SetProperty(ref _image, value);
				InvokePropertyValueChanged(nameof(IsImageValid), IsImageValid);
			}
		}

		public string Name
		{
			get => _name;
			set => SetProperty(ref _name, value);
		}

		public string EnglishName
		{
			get => _englishName;
			set => SetProperty(ref _englishName, value);
		}

		public string Affiliation
		{
			get => _affiliation;
			set
			{
				SetProperty(ref _affiliation, value);
				InvokePropertyValueChanged(nameof(IsAffiliationValid), IsAffiliationValid);
			}
		}

		public string Email
		{
			get => _email;
			set
			{
				SetProperty(ref _email, value);
				InvokePropertyValueChanged(nameof(IsEmailValid), IsEmailValid);
			}
		}

		public string Phone
		{
			get => _phone;
			set
			{
				SetProperty(ref _phone, value);
				InvokePropertyValueChanged(nameof(IsPhoneValid), IsPhoneValid);
			}
		}

		public Texture QrCode
		{
			get => _qrCode;
			set => SetProperty(ref _qrCode, value);
		}

		public string Free1
		{
			get => _free1;
			set
			{
				SetProperty(ref _free1, value);
				InvokePropertyValueChanged(nameof(IsFree1Valid), IsFree1Valid);
			}
		}

		public string Free2
		{
			get => _free2;
			set
			{
				SetProperty(ref _free2, value);
				InvokePropertyValueChanged(nameof(IsFree2Valid), IsFree2Valid);
			}
		}

		public bool IsExchangable
		{
			get => _isExchangable;
			set => SetProperty(ref _isExchangable, value);
		}

		public bool IsAffiliationValid => !string.IsNullOrEmpty(_affiliation);

		public bool IsEmailValid => !string.IsNullOrEmpty(_email);

		public bool IsPhoneValid => !string.IsNullOrEmpty(_phone);

		public bool IsFree1Valid => !string.IsNullOrEmpty(_free1);

		public bool IsFree2Valid => !string.IsNullOrEmpty(_free2);

		public eViewMode ViewMode
		{
			get => _viewMode;
			set => SetProperty(ref _viewMode, value);
		}

		public bool IsImageValid => Image != null;

		public Transform CardRootTransform
		{
			get => _cardRootTransform;
			set => SetProperty(ref _cardRootTransform, value);
		}
#endregion

#region Initialize
		public MiceBusinessCardViewModel()
		{
			SaveImage = new CommandHandler(OnSaveImage);
			Exchange  = new CommandHandler(OnExchange);
			_viewMode = eViewMode.DEFAULT;
		}
#endregion


		public void Set(MiceUserInfo userInfo, Texture2D photoTexture)
		{
			InternalSet(userInfo);
			Image = photoTexture;
		}

		public void Set(MiceUserInfo userInfo)
		{
			InternalSet(userInfo);

			if (string.IsNullOrEmpty(userInfo.PhotoUrl))
				Image = null;
			else
				TextureCache.InstanceOrNull.GetOrDownloadTextureAsync(userInfo.PhotoUrl, ((b, texture) => Image = texture)).Forget();
		}

		private void InternalSet(MiceUserInfo userInfo)
		{
			_userInfo   = userInfo;
			Name        = userInfo.Name;
			EnglishName = userInfo.AdditionalName;
			Affiliation = userInfo.Affiliation;
			Email       = userInfo.Email;
			Phone       = userInfo.Phone;
			Free1       = userInfo.GetFree(0);
			Free2       = userInfo.GetFree(1);
			QrCode      = userInfo.GenerateQrCode();
			IsExchangable = !MiceInfoManager.Instance.IsMyUser(userInfo) &&
			                (userInfo.ExchangeCode == MiceWebClient.eMiceAccountCardExchangeCode.UNKNOWN || userInfo.ExchangeCode == MiceWebClient.eMiceAccountCardExchangeCode.NONE);
		}

		public void OnSaveImage()
		{
			SaveImageAsync().Forget();
		}

		public void OnExchange()
		{
			UIManager.Instance.ShowPopupYesNo(Data.Localization.eKey.MICE_UI_BC_Popup_Title_SaveBC.ToLocalizationString(),
			                                  Data.Localization.eKey.MICE_UI_BC_Popup_Desc_SaveBC.ToLocalizationString(),
			                                  (_) => { ExchangeAsync().Forget(); });
		}

		private async UniTask SaveImageAsync()
		{
			if (CardRootTransform == null) return;

			var directory = Mice.Utils.Path.Downloads;
			var savePath  = Mice.Utils.GenerateUniquePath(Path.Combine(directory, $"BusinessCard_{this.Name}.png"));
			ViewMode = eViewMode.CARD_ONLY;
			var texture    = await MiceUICapture.Capture(CardRootTransform.gameObject);
			var imageBytes = texture.EncodeToPNG();

			if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
			await System.IO.File.WriteAllBytesAsync(savePath, imageBytes);
			ViewMode = eViewMode.DEFAULT;

			UIManager.Instance.ShowPopupConfirm(Data.Localization.eKey.MICE_UI_Popup_Title_Info_BCSave.ToLocalizationString(),
			                                    Data.Localization.eKey.MICE_UI_Popup_Msg_Info_BCSave.ToLocalizationString(),
			                                    () => OpenInFileBrowser.Open(directory));
		}

		private async UniTask ExchangeAsync()
		{
			if (await MiceInfoManager.Instance.ExchangeBusinessCard(_userInfo))
			{
				IsExchangable = false;
			}
		}

		public static void ShowView(MiceUserInfo userInfo, Texture2D photoTexture)
		{
			UIManager.Instance.CreatePopup(ResName, (guiView) =>
			{
				var viewModel = guiView.ViewModelContainer.GetOrAddViewModel<MiceBusinessCardViewModel>();
				viewModel.Set(userInfo, photoTexture);

				guiView.Show();
			}).Forget();
		}

		public static void ShowView(MiceUserInfo userInfo)
		{
			UIManager.Instance.CreatePopup(ResName, (guiView) =>
			{
				var viewModel = guiView.ViewModelContainer.GetOrAddViewModel<MiceBusinessCardViewModel>();
				viewModel.Set(userInfo);

				guiView.Show();
			}).Forget();
		}
	}
}
