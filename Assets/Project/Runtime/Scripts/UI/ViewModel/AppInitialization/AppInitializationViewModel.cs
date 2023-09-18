/*===============================================================
* Product:		Com2Verse
* File Name:	AssetBundleDownloadViewModel.cs
* Developer:	tlghks1009
* Date:			2023-03-20 15:53
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[ViewModelGroup("AppInitialization")]
	public sealed class AppInitializationViewModel : ViewModelBase
	{
		private bool _isVisibleBuiltProgressUI;
		private bool _isVisibleProgressBar;
		private bool _isVisibleLoadingIcon;

		private string _context;
		private string _sizeString;

		private float _percent;


		public AppInitializationViewModel()
		{
			AppInitializationSceneController.Instance.OnInitializeStatus += OnAppInitializeStatus;
			AppInitializationSceneController.Instance.OnInitialized      += OnAppInitialized;

			IsVisibleBuiltProgressUI = true;
		}

		private void OnAppInitializeStatus(AppInitializationSceneController.AppInitializeStatus status)
		{
			IsVisibleLoadingIcon = status.IsVisibleLoadingIcon;
			IsVisibleProgressBar = status.IsVisibleProgressBar;
			Context              = status.Context;
			SizeString           = status.AdditionalData;
			Percent              = status.Percent;
		}


		private void OnAppInitialized()
		{
			AppInitializationSceneController.Instance.OnInitializeStatus -= OnAppInitializeStatus;
			AppInitializationSceneController.Instance.OnInitialized      -= OnAppInitialized;

			IsVisibleLoadingIcon     = false;
			IsVisibleProgressBar     = false;
			IsVisibleBuiltProgressUI = false;
		}

		[UsedImplicitly]
		public bool IsVisibleBuiltProgressUI
		{
			get => _isVisibleBuiltProgressUI;
			set => SetProperty(ref _isVisibleBuiltProgressUI, value);
		}


		[UsedImplicitly]
		public bool IsVisibleLoadingIcon
		{
			get => _isVisibleLoadingIcon;
			set => SetProperty(ref _isVisibleLoadingIcon, value);
		}

		[UsedImplicitly]
		public bool IsVisibleProgressBar
		{
			get => _isVisibleProgressBar;
			set => SetProperty(ref _isVisibleProgressBar, value);
		}

		[UsedImplicitly]
		public string Context
		{
			get => _context;
			set => SetProperty(ref _context, value);
		}

		[UsedImplicitly]
		public string SizeString
		{
			get => _sizeString;
			set => SetProperty(ref _sizeString, value);
		}

		[UsedImplicitly]
		public float Percent
		{
			get => _percent;
			set => SetProperty(ref _percent, value);
		}
	}
}
