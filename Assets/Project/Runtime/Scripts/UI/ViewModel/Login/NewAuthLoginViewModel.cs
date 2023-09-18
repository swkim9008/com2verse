/*===============================================================
* Product:		Com2Verse
* File Name:	NewAuthLoginViewModel.cs
* Developer:	mikeyid77
* Date:			2023-04-04 12:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Loading;
using Com2Verse.Network;

namespace Com2Verse.UI
{
	public sealed class NewAuthLoginViewModel : ViewModelBase
	{
		private string _dummyId;
		private string _appVersion;
		private bool   _idNullError = false;
		private bool   _isVisibleUIs;

		public  CommandHandler HiveLoginButtonClicked  { get; }
		public  CommandHandler DummyLoginButtonClicked { get; }

		public bool IsVisibleUIs
		{
			get => _isVisibleUIs;
			set => SetProperty(ref _isVisibleUIs, value);
		}


		public string DummyId
		{
			get => _dummyId;
			set
			{
				_dummyId = value;
				base.InvokePropertyValueChanged(nameof(DummyId), DummyId);
			}
		}

		public string AppVersion
		{
			get => _appVersion;
			set
			{
				_appVersion = value;
				InvokePropertyValueChanged(nameof(AppVersion), AppVersion);
			}
		}

		public bool IdNullError
		{
			get => _idNullError;
			set
			{
				_idNullError = value;
				base.InvokePropertyValueChanged(nameof(IdNullError), IdNullError);
			}
		}

		public NewAuthLoginViewModel()
		{
			AppInitializationSceneController.Instance.OnInitialized += OnAppInitialized;

			HiveLoginButtonClicked  = new CommandHandler(OnHiveLoginButtonClicked);
			DummyLoginButtonClicked = new CommandHandler(OnCom2VerseLoginButtonClickedAsync);
		}


		private void OnAppInitialized()
		{
			AppInitializationSceneController.Instance.OnInitialized -= OnAppInitialized;

			IsVisibleUIs = true;
		}

		private void OnHiveLoginButtonClicked()
		{
			if (!LoginManager.Instance.IsInitialized) return;
			if (LoadingManager.Instance.IsLoading) return;

#if UNITY_EDITOR
			UIManager.Instance.ShowPopupCommon("Hive 로그인은 Editor에서는 동작하지 않습니다.");
#else
			LoginManager.Instance.RequestCom2VerseLogin(LoginManager.eCom2VerseLoginType.HIVE);
#endif
		}

		private void OnCom2VerseLoginButtonClickedAsync()
		{
			if (!LoginManager.Instance.IsInitialized) return;
			if (LoadingManager.Instance.IsLoading) return;

			if (string.IsNullOrEmpty(DummyId))
			{
				IdNullError = true;
			}
			else
			{
				IdNullError = false;
				LoginManager.Instance.RequestCom2VerseLogin(LoginManager.eCom2VerseLoginType.HIVE_DEV, DummyId);
			}
		}
	}
}
