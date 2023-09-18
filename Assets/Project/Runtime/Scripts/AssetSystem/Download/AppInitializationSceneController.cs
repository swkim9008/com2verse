/*===============================================================
* Product:		Com2Verse
* File Name:	AssetBundleDownloadUIController.cs
* Developer:	tlghks1009
* Date:			2023-04-26 12:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.AssetSystem;
using Com2Verse.Network;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse
{
	public sealed class AppInitializationSceneController : Singleton<AppInitializationSceneController>
	{
		[UsedImplicitly]
		private AppInitializationSceneController() { }

		public class AppInitializeStatus
		{
			public string Context              { get; set; }
			public bool   IsVisibleProgressBar { get; set; }
			public string AdditionalData       { get; set; }
			public float  Percent              { get; set; }
			public bool   IsVisibleLoadingIcon { get; set; }
		}

		private AppInitializeStatus _appInitializeStatus;

		public event Action<AppInitializeStatus> OnInitializeStatus;
		public event Action OnInitialized;

		private bool _isInitialized;

		public async UniTask InitializeAppAsync()
		{
			if (_isInitialized)
			{
				OnAppInitializeCompleted();
				return;
			}

			_isInitialized = false;

			_appInitializeStatus = new AppInitializeStatus();

			if (!await CheckValidOs())
				return;

			SetContext(LocalizationKey("UI_World_AssetBundle_Msg_InitializationModule"), string.Empty, 0, false, true);
			if (!await UniTaskHelper.DelayFrame(1))
				return;

			if (!await InitializeLoginManager())
				return;
			SetContext(LocalizationKey("UI_World_AssetBundle_Msg_InitializationModuleComplete"), string.Empty, 1, false, true);


			SetContext(LocalizationKey("UI_World_AssetBundle_Msg_InitializationRes"), string.Empty, 0, false, true);
			if (!await UniTaskHelper.DelayFrame(1))
				return;

			if (!await InitializeAssetBundle())
				return;
			SetContext(LocalizationKey("UI_World_AssetBundle_Msg_InitializationResComplete"), string.Empty, 1, false, true);


			SetContext(LocalizationKey("UI_World_AssetBundle_Msg_InitializationApp"), string.Empty, 0, true, true);
			if (!await UniTaskHelper.DelayFrame(1))
				return;

			if (!await InitializeProjectManager())
				return;
			SetContext(LocalizationKey("UI_World_AssetBundle_Msg_InitializationAppComplete"), string.Empty, 1, true, false);

			await UniTaskHelper.Delay(500);

			OnAppInitializeCompleted();
		}

#region osCheck
		private async UniTask<bool> CheckValidOs()
		{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			// Windows 10 미만은 팝업 출력 후 강제 종료
			OperatingSystem os = Environment.OSVersion;
			if (os.Version == null) return ShowExitPopup();
			if (os.Version.Major < 10) return ShowExitPopup();
			return true;
#else
			return true;
#endif

			bool ShowExitPopup()
			{
				UIManager.Instance.ShowPopupConfirm(LocalizationKey("UI_Title_Popup_Title_Notice"),
				                                    LocalizationKey("UI_Title_Popup_Desc_UnsupportedVersion"), null,
				                                    LocalizationKey("UI_Title_Popup_Btn_Exit"), false, false,
				                                    (guiView) =>
				                                    {
					                                    guiView.OnClosedEvent += (_) =>
					                                    {
#if UNITY_EDITOR
						                                    Com2VerseEditor.EditorApplicationUtil.ExitPlayMode();
#else
															UnityEngine.Application.Quit();
#endif
					                                    };
				                                    });
				return false;
			}
		}
#endregion

#region login
		private async UniTask<bool> InitializeLoginManager()
		{
			var isLoginManagerInitialized = false;
			LoginManager.Instance.Initialize((result) => isLoginManagerInitialized = result);

			while (!isLoginManagerInitialized)
			{
				if (!await UniTaskHelper.DelayFrame(1))
					return false;
			}

			return true;
		}
#endregion

#region AssetBundle
		private async UniTask<bool> InitializeAssetBundle()
		{
			var assetBundleManager = AssetBundleManager.Instance;

			if (!await assetBundleManager.RequestAssetBundlePatchVersion())
				return false;

			if (!await assetBundleManager.IsDownloadRequired())
			{
				assetBundleManager.Release();
				return true;
			}

			var assetBundleDownloadComplete = false;

			assetBundleManager.ShowSelectionPopup(
				() =>
				{
					assetBundleManager.Downloader?.OnRemoteAssetBundleDownloadStatus.AddListener(OnAssetBundleDownloadStatus);
					assetBundleManager.Downloader?.OnRemoteAssetBundlesDownloadCompleted.AddListener(OnAssetBundleDownloadCompleted);
				});

			void OnAssetBundleDownloadStatus(C2VDownloadStatus status)
			{
				var downloadedSize    = C2VAddressablesDownloadUtility.GetConvertedByteString(status.DownloadedSize);
				var totalDownloadSize = C2VAddressablesDownloadUtility.GetConvertedByteString(status.TotalSize);

				var sizeString = Localization.Instance.GetString("UI_World_AssetBundle_Msg_DownloadData", downloadedSize, totalDownloadSize);
				var context = Localization.Instance.GetString("UI_World_AssetBundle_Msg_DownloadGuide",
				                                              C2VAssetBundleManager.Instance.TargetAssetBundleVersion,
				                                              (int) (status.Percent * 100));
				var percent = status.Percent;

				SetContext(context, sizeString, percent, true, false);
			}

			void OnAssetBundleDownloadCompleted()
			{
				assetBundleManager.Downloader?.OnRemoteAssetBundleDownloadStatus.RemoveListener(OnAssetBundleDownloadStatus);
				assetBundleManager.Downloader?.OnRemoteAssetBundlesDownloadCompleted.RemoveListener(OnAssetBundleDownloadCompleted);

				assetBundleManager.Release();

				assetBundleDownloadComplete = true;
			}


			return await UniTaskHelper.WaitUntil(() => assetBundleDownloadComplete);
		}
#endregion

#region ProjectManager
		private async UniTask<bool> InitializeProjectManager()
		{
			ProjectManager.Instance.TryInitializeAsync().Forget();

			var initializationAppTextKey = LocalizationKey("UI_World_AssetBundle_Msg_InitializationApp");
			while (!ProjectManager.Instance.IsInitialized)
			{
				var progress = ProjectManager.Instance.CompleteTaskCount / (float) ProjectManager.Instance.TotalTaskCount;
				SetContext(initializationAppTextKey, string.Empty, progress, true, true);

				if (!await UniTaskHelper.DelayFrame(1))
					return false;
			}

			return true;
		}
#endregion

		private void OnAppInitializeCompleted()
		{
			OnInitialized?.Invoke();

			_isInitialized = true;
		}


		private string LocalizationKey(string key) => Localization.Instance.GetString(key);


		private void SetContext(string context, string additionalData, float percent, bool isActiveProgressbar, bool isActiveLoadingIcon)
		{
			_appInitializeStatus.Context              = context;
			_appInitializeStatus.AdditionalData       = additionalData;
			_appInitializeStatus.Percent              = percent;
			_appInitializeStatus.IsVisibleProgressBar = isActiveProgressbar;
			_appInitializeStatus.IsVisibleLoadingIcon = isActiveLoadingIcon;

			OnInitializeStatus?.Invoke(_appInitializeStatus);
		}
	}
}
