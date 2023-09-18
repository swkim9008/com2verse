/*===============================================================
* Product:		Com2Verse
* File Name:	MiceBusinessCardBookViewModel_MyCard.cs
* Developer:	wlemon
* Date:			2023-04-12 18:04
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Mice;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.UI
{
	public partial class MiceBusinessCardBookViewModel
	{
#region Variables
		private Texture2D    _savedImage;
		private Texture2D    _image;
		private string       _firstName;
		private string       _lastName;
		private string       _englishName;
		private string       _affiliation;
		private string       _emailID;
		private string       _emailDomain;
		private List<string> _mailDomainList;
		private string       _phone;
		private bool         _isPublic;
		private string       _freeTitle1;
		private string       _freeContent1;
		private bool         _isFreeItem1Active;
		private string       _freeTitle2;
		private string       _freeContent2;
		private bool         _isFreeItem2Active;
		private bool         _isFreeItemMax;
		private bool         _isImageChanged;
		private bool         _checkValid;
		private bool         _isFree1Editing;
		private bool         _isFree2Editing;
		private MiceUserInfo _userInfo;

		public CommandHandler Save             { get; private set; }
		public CommandHandler AddFreeItem      { get; private set; }
		public CommandHandler TogglePreview      { get; private set; }
		public CommandHandler ChangeImage      { get; private set; }
		public CommandHandler RemoveFreeItem1  { get; private set; }
		public CommandHandler RemoveFreeItem2  { get; private set; }
		public CommandHandler EditFree1        { get; private set; }
		public CommandHandler EditFree2        { get; private set; }
		public CommandHandler CancelEditFree1  { get; private set; }
		public CommandHandler CancelEditFree2  { get; private set; }

		public CommandHandler SetPublic { get; private set; }

		public CommandHandler SetPrivate { get; private set; }
#endregion

#region Properties
		public Texture2D SavedImage
		{
			get => _savedImage;
			set
			{
				SetProperty(ref _savedImage, value);
				InvokePropertyValueChanged(nameof(IsSavedImageValid), IsSavedImageValid);
			}
		}

		public bool IsSavedImageValid => SavedImage != null;

		public Texture2D Image
		{
			get => _image;
			set
			{
				SetProperty(ref _image, value);
				InvokePropertyValueChanged(nameof(IsImageValid), IsImageValid);
			}
		}

		public bool IsImageValid => Image != null;
		
		public string FirstName
		{
			get => _firstName;
			set
			{
				SetProperty(ref _firstName, value);
				InvokePropertyValueChanged(nameof(IsFirstNameValid), IsFirstNameValid);
			}
		}

		public string LastName
		{
			get => _lastName;
			set
			{
				SetProperty(ref _lastName, value);
				InvokePropertyValueChanged(nameof(IsLastNameValid), IsLastNameValid);
			}
		}

		public string EnglishName
		{
			get => _englishName;
			set
			{
				SetProperty(ref _englishName, value);
				InvokePropertyValueChanged(nameof(IsEnglishNameValid), IsEnglishNameValid);
			}
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

		public string EmailID
		{
			get => _emailID;
			set
			{
				SetProperty(ref _emailID, value);
				InvokePropertyValueChanged(nameof(IsEmailIdValid), IsEmailIdValid);
			}
		}

		public string EmailDomain
		{
			get => _emailDomain;
			set
			{
				SetProperty(ref _emailDomain, value);
				InvokePropertyValueChanged(nameof(IsEmailDomainValid), IsEmailDomainValid);
			}
		}

		public List<string> EmailDomainList
		{
			get => _mailDomainList;
			set => SetProperty(ref _mailDomainList, value);
		}

		public string Phone
		{
			get => _phone;
			set => SetProperty(ref _phone, value);
		}

		public string FreeTitle1
		{
			get => _freeTitle1;
			set
			{
				SetProperty(ref _freeTitle1, value);
				InvokePropertyValueChanged(nameof(IsFreeTitle1Valid), IsFreeTitle1Valid);
			}
		}

		public string FreeContent1
		{
			get => _freeContent1;
			set
			{
				SetProperty(ref _freeContent1, value);
				InvokePropertyValueChanged(nameof(IsFreeContent1Valid), IsFreeContent1Valid);
			}
		}

		public bool IsFreeItem1Active
		{
			get => _isFreeItem1Active;
			set => SetProperty(ref _isFreeItem1Active, value);
		}

		public string FreeTitle2
		{
			get => _freeTitle2;
			set
			{
				SetProperty(ref _freeTitle2, value);
				InvokePropertyValueChanged(nameof(IsFreeTitle2Valid), IsFreeTitle2Valid);
			}
		}

		public string FreeContent2
		{
			get => _freeContent2;
			set
			{
				SetProperty(ref _freeContent2, value);
				InvokePropertyValueChanged(nameof(IsFreeContent2Valid), IsFreeContent2Valid);
			}
		}

		public bool IsFreeItem2Active
		{
			get => _isFreeItem2Active;
			set => SetProperty(ref _isFreeItem2Active, value);
		}

		public bool IsFreeItemMax
		{
			get => _isFreeItemMax;
			set => SetProperty(ref _isFreeItemMax, value);
		}

		public bool IsPublic
		{
			get => _isPublic;
			set => SetProperty(ref _isPublic, value);
		}

		public bool CheckValid
		{
			get => _checkValid;
			set => SetProperty(ref _checkValid, value);
		}

		public bool IsFree1Editing
		{
			get => _isFree1Editing;
			set
			{
				SetProperty(ref _isFree1Editing, value);
				if (!IsFree1Editing)
				{
					FreeTitle1 = string.Empty;
				}
			}
		}

		public bool IsFree2Editing
		{
			get => _isFree2Editing;
			set
			{
				SetProperty(ref _isFree2Editing, value);
				if (!IsFree2Editing)
				{
					FreeTitle2 = string.Empty;
				}
			}
		}

		public bool IsFirstNameValid    => !string.IsNullOrEmpty(FirstName);
		public bool IsLastNameValid     => !string.IsNullOrEmpty(LastName);
		public bool IsEnglishNameValid  => !string.IsNullOrEmpty(EnglishName);
		public bool IsEmailIdValid      => !string.IsNullOrEmpty(EmailID);
		public bool IsEmailDomainValid  => !string.IsNullOrEmpty(EmailDomain);
		public bool IsAffiliationValid  => !string.IsNullOrEmpty(Affiliation);
		public bool IsFreeTitle1Valid   => !string.IsNullOrEmpty(FreeTitle1);
		public bool IsFreeTitle2Valid   => !string.IsNullOrEmpty(FreeTitle2);
		public bool IsFreeContent1Valid => !string.IsNullOrEmpty(FreeContent1);
		public bool IsFreeContent2Valid => !string.IsNullOrEmpty(FreeContent2);
#endregion

#region Intialize
		public void InitializeMyUserInfo()
		{
			Save            = new CommandHandler(OnSave);
			AddFreeItem     = new CommandHandler(OnAddFreeItem);
			TogglePreview   = new CommandHandler(OnTogglePreview);
			ChangeImage     = new CommandHandler(OnChangeImage);
			RemoveFreeItem1 = new CommandHandler(OnRemoveFreeItem1);
			RemoveFreeItem2 = new CommandHandler(OnRemoveFreeItem2);
			EditFree1       = new CommandHandler(() => IsFree1Editing = true);
			EditFree2       = new CommandHandler(() => IsFree2Editing = true);
			CancelEditFree1 = new CommandHandler(() => IsFree1Editing = false);
			CancelEditFree2 = new CommandHandler(() => IsFree2Editing = false);
			SetPublic       = new CommandHandler(() => IsPublic       = true);
			SetPrivate      = new CommandHandler(() => IsPublic       = false);

			//TODO: Table
			EmailDomainList = GetEmailDomainList();
		}

		// TODO : 테이블 작업이 되면 명함과, 경품 메일 부분을 바꿔야 한다.
		public static List<string> GetEmailDomainList()
		{
			var emailDomainList = new List<string>();
			emailDomainList.Add("daum.net");
			emailDomainList.Add("gmail.com");
			emailDomainList.Add("hanmail.net");
			emailDomainList.Add("nate.com");
			emailDomainList.Add("naver.com");
			return emailDomainList;
		}

		public void SetMyUserInfo(MiceUserInfo userInfo)
		{
			_userInfo = userInfo;

			_userInfo.GetEmail(out var emailID, out var emailDomain);
			_userInfo.GetFree(0, out var freeTitle1, out var freeContent1);
			_userInfo.GetFree(1, out var freeTitle2, out var freeContent2);
			_isImageChanged = false;
			CheckValid     = false;

			if (string.IsNullOrEmpty(_userInfo.PhotoUrl))
			{
				Image        = default(Texture2D);
				SavedImage = default(Texture2D);
			}
			else
			{
				TextureCache.Instance.GetOrDownloadTextureAsync(_userInfo.PhotoUrl, (b, texture) =>
				{
					Image        = texture as Texture2D;
					SavedImage = Image;
				}).Forget();
			}

			FirstName         = _userInfo.FirstName;
			LastName          = _userInfo.LastName;
			EnglishName       = _userInfo.AdditionalName;
			Affiliation       = _userInfo.Affiliation;
			EmailID           = emailID;
			EmailDomain       = emailDomain;
			Phone             = _userInfo.Phone;
			FreeTitle1        = freeTitle1;
			FreeContent1      = freeContent1;
			FreeTitle2        = freeTitle2;
			FreeContent2      = freeContent2;
			IsFreeItem1Active = _userInfo.IsFreeActive(0);
			IsFreeItem2Active = _userInfo.IsFreeActive(1);
			IsFreeItemMax     = IsFreeItem1Active && IsFreeItem2Active;
			IsFree1Editing    = !string.IsNullOrEmpty(FreeTitle1);
			IsFree2Editing    = !string.IsNullOrEmpty(FreeTitle2);
			IsPublic          = _userInfo.IsPublic;
		}
#endregion

#region Binding Events
		private void OnSave()
		{
			CheckValid = true;
			if (!IsFirstNameValid || !IsLastNameValid || !IsEmailIdValid || !IsEmailDomainValid || !IsAffiliationValid)
				return;
			SaveAsync().Forget();
		}

		private void OnAddFreeItem()
		{
			if (IsFreeItem1Active)
			{
				if (!IsFreeItem2Active)
				{
					IsFreeItemMax     = true;
					IsFreeItem2Active = true;
				}
			}
			else
			{
				IsFreeItem1Active = true;
			}
		}

		private void OnRemoveFreeItem1()
		{
			if (IsFreeItem2Active)
			{
				FreeTitle1        = FreeTitle2;
				FreeContent1      = FreeContent2;
				FreeTitle2        = string.Empty;
				FreeContent2      = string.Empty;
				IsFreeItem2Active = false;
				IsFree2Editing    = false;
			}
			else
			{
				FreeTitle1        = string.Empty;
				FreeContent1      = string.Empty;
				IsFreeItem1Active = false;
				IsFree1Editing    = false;
			}

			IsFreeItemMax = false;
		}

		private void OnRemoveFreeItem2()
		{
			if (IsFreeItem2Active)
			{
				FreeTitle2        = string.Empty;
				FreeContent2      = string.Empty;
				IsFreeItem2Active = false;
				IsFreeItemMax     = false;
				IsFree2Editing    = false;
			}
		}

		private void OnTogglePreview()
		{
			if (IsCardViewVisible)
			{
				IsCardViewVisible = false;
			}
			else
			{
				IsCardViewVisible = true;
				_miceBusinessCardViewModel.Set(ConvertToUserInfo());
			}
		}

		private void OnChangeImage()
		{
			MiceBusinessCardImageSelectionViewModel.ShowView(OnImageSelected);
		}

		private void OnImageSelected(Texture2D texture)
		{
			Image           = texture;
			_isImageChanged = true;
		}
#endregion

		private async UniTask SaveAsync()
		{
			UIManager.Instance.ShowWaitingResponsePopup();
			var isImageUploaded = false;
			if (Image != null && _isImageChanged)
			{
				isImageUploaded = await MiceInfoManager.Instance.UploadMyCardImage(Image);
				if (isImageUploaded) SavedImage = Image;
			}

			var userInfo = ConvertToUserInfo();
			var result   = await MiceInfoManager.Instance.SaveMyUser(userInfo);
			if (result)
			{
				_miceBusinessCardViewModel.Set(MiceInfoManager.Instance.MyUserInfo);
			}
			UIManager.Instance.HideWaitingResponsePopup();
			SetVisible = false;

			if (result)
			{
				UIManager.Instance.ShowPopupConfirm(Data.Localization.eKey.MICE_UI_Popup_Title_Info_BCSave.ToLocalizationString(),
				                                    Data.Localization.eKey.MICE_UI_Popup_Msg_Info_BCSave.ToLocalizationString());
			}
		}

		private MiceUserInfo ConvertToUserInfo()
		{
			var userInfo = new MiceEditableUserInfo(_userInfo);
			userInfo.SetFirstName(FirstName);
			userInfo.SetLastName(LastName);
			userInfo.SetAdditionalName(EnglishName);
			userInfo.SetAffiliation(Affiliation);
			userInfo.SetEmail(EmailID, EmailDomain);
			userInfo.SetPhone(Phone);
			if (IsFreeItem1Active && IsFreeItem2Active)
			{
				userInfo.SetFreeListCount(2);
				userInfo.SetFree(0, FreeTitle1, FreeContent1);
				userInfo.SetFree(1, FreeTitle2, FreeContent2);
			}
			else if (IsFreeItem1Active)
			{
				userInfo.SetFreeListCount(1);
				userInfo.SetFree(0, FreeTitle1, FreeContent1);
			}
			else
			{
				userInfo.ClearFreeList();
			}

			userInfo.SetIsPublic(IsPublic);
			return userInfo;
		}
	}
}
