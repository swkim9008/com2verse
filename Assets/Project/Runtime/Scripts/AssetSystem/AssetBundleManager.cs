/*===============================================================
* Product:		Com2Verse
* File Name:	SceneAppInitialization.cs
* Developer:	tlghks1009
* Date:			2023-03-17 14:09
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Threading;
using Com2Verse.Logger;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.AssetSystem
{
	public sealed class AssetBundleManager : Singleton<AssetBundleManager>, IDisposable
	{
		private readonly int _maxRetryCount = 3;

		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private AssetBundleManager() { }

		private C2VAddressablesDownloader  _downloader;
		private C2VAddressablesEvent<bool> _answer;

		private CancellationTokenSource _downloadWaitCts;


		private bool _isInitialized;

		private long _downloadSize;

		private AppInfoData AppData => AppInfo.Instance.Data;

		public C2VAddressablesDownloader Downloader => _downloader;


		public void Initialize()
		{
			_downloadWaitCts = new CancellationTokenSource();

			C2VAssetBundleManager.Instance.Initialize(AppData.BuildTarget,
			                                          AppData.Environment.ToString(),
			                                          AppData.Version,
			                                          AppData.AssetBundleVersion,
			                                          AppData.AssetBuildType,
			                                          AppData.HiveEnvType,
			                                          _downloadWaitCts);

			C2VCaching.RemoveCachedCatalogFiles();
		}


		public async UniTask<bool> RequestAssetBundlePatchVersion()
		{
			var currentRetryCount = 0;
			while (true)
			{
				var result = await C2VAssetBundleManager.Instance.TryRequestAssetBundlePatchVersion();
				if (result == eResponseCode.SUCCESS)
					return true;

				currentRetryCount++;

				if (currentRetryCount < _maxRetryCount)
					continue;

				currentRetryCount = 0;
				if (!await ShowRetryPopup(result))
				{
					QuitApp();
					return false;
				}
			}
		}


		public async UniTask<bool> IsDownloadRequired()
		{
			var completed          = false;
			var isDownloadRequired = false;


			void OnAssetBundleSizeDownloadCompleted(C2VAddressablesEvent<bool> answer, long size)
			{
				_answer       = answer;
				_downloadSize = size;

				isDownloadRequired = _downloadSize > 0;
				completed          = true;
			}

			_downloader = AssetBundleDownloadService.Instance.CreateDownloader(eRequestAssetBundleType.LOGIN, C2VAssetBundleManager.Instance.TargetAssetBundleVersion);
			_downloader.OnRemoteAssetBundleDownloadFailed.AddListener(OnAssetBundleDownloadFailed);
			_downloader.OnRemoteAssetBundlesSizeDownloadCompleted.AddListener(OnAssetBundleSizeDownloadCompleted);
			_downloader.Download();

			await UniTask.WaitUntil(() => completed, cancellationToken: _downloadWaitCts.Token);

			if (!isDownloadRequired)
			{
				_answer?.Invoke(false);
			}

			return isDownloadRequired;
		}


		private void OnAssetBundleDownloadFailed(eResponseCode responseCode) => ShowErrorPopup(responseCode);

		public void QuitApp()
		{
			Release();

#if UNITY_EDITOR
			Com2VerseEditor.EditorApplicationUtil.ExitPlayMode();
#else
            UnityEngine.Application.Quit();
#endif
		}

		public void ShowSelectionPopup(Action okCallback)
		{
			var downloadSize = C2VAddressablesDownloadUtility.GetConvertedByteString(_downloadSize);

			var downloadTitlePopup = Localization.Instance.GetString("UI_World_AssetBundle_Popup_Title_Download");
			var downloadString     = Localization.Instance.GetString("UI_World_AssetBundle_Popup_Desc_Download",  downloadSize);
			var downloadYesString  = Localization.Instance.GetString("UI_World_AssetBundle_Popup_Btn_Download");
			var downloadNoString   = Localization.Instance.GetString("UI_World_AssetBundle_Popup_Btn_Exit");

			UIManager.Instance.ShowPopupYesNo(downloadTitlePopup, downloadString,
			                                  (_) =>
			                                  {
				                                  okCallback?.Invoke();

				                                  _.OnClosedEvent += OnPopupClosedWhenOk;
			                                  },
			                                  (_) => QuitApp(),
			                                  null, downloadYesString, downloadNoString, false, false);

			void OnPopupClosedWhenOk(GUIView guiView)
			{
				guiView.OnClosedEvent -= OnPopupClosedWhenOk;

				_answer.Invoke(true);
			}
		}


		private async UniTask<bool> ShowRetryPopup(eResponseCode responseCode)
		{
			var complete = false;
			var isRetry  = false;

			var patchServerPopupTitleError      = GetErrorTitleMessage(responseCode);
			var patchServerPopupDescRequestFail = GetErrorMessage(responseCode);
			var patchServerPopupExit            = Localization.Instance.GetString("UI_PatchServer_Popup_Exit");
			var patchServerPopupBtnRetry        = Localization.Instance.GetString("UI_PatchServer_Popup_Btn_Retry");

			UIManager.Instance.ShowPopupYesNo(patchServerPopupTitleError, patchServerPopupDescRequestFail,
			                                  (_) => isRetry = true,
			                                  (_) => isRetry = false,
			                                  (_) => _.OnClosedEvent += OnPopupClosed,
			                                  patchServerPopupBtnRetry, patchServerPopupExit, false, false);

			void OnPopupClosed(GUIView guiView)
			{
				guiView.OnClosedEvent -= OnPopupClosed;

				complete = true;
			}

			await UniTask.WaitUntil(() => complete, cancellationToken: _downloadWaitCts.Token);

			return isRetry;
		}


		private void ShowErrorPopup(eResponseCode responseCode)
		{
			var errorTitleMessage = GetErrorTitleMessage(responseCode);
			var errorMessage      = GetErrorMessage(responseCode);
			var yesKey            = Localization.Instance.GetString("UI_World_AssetBundle_Popup_Btn_Confirm");

			C2VDebug.LogErrorCategory("AssetBundle", $"AssetBundle Error. ResponseCode : {responseCode}");

			UIManager.Instance.ShowPopupConfirm(errorTitleMessage, errorMessage,
			                                    null, yesKey, false, false, (guiView) => { guiView.OnClosedEvent += (_) => QuitApp(); });
		}


		private string GetErrorTitleMessage(eResponseCode responseCode)
		{
			var errorTitleMessage = string.Empty;
			switch (responseCode)
			{
				case eResponseCode.CACHE_SPACE_ERROR:
					errorTitleMessage = Localization.Instance.GetString("UI_World_AssetBundle_Popup_Title_LackStorage");
					break;
				case eResponseCode.CANCELED:
				case eResponseCode.PATCH_SERVER_REQUEST_FAILED:
				case eResponseCode.RESPONSE_NULL:
				case eResponseCode.CACHE_ERROR:
				case eResponseCode.INTERNAL_SERVER_ERROR:
				case eResponseCode.TIME_OUT:
				case eResponseCode.CATALOG_UPDATE_ERROR:
				case eResponseCode.UPDATE_CATALOG_ERROR:
				case eResponseCode.CLEAN_BUNDLE_ERROR:
				case eResponseCode.BUNDLE_SIZE_CHECK_ERROR:
				case eResponseCode.BUNDLE_DOWNLOAD_ERROR:
				case eResponseCode.PATCH_SERVER_EMPTY_DATA:
					errorTitleMessage = Localization.Instance.GetString("UI_PatchServer_Popup_Title_Error");
					break;
			}

			return errorTitleMessage;
		}


		private string GetErrorMessage(eResponseCode responseCode)
		{
			var errorMessage = string.Empty;
			switch (responseCode)
			{
				case eResponseCode.CACHE_SPACE_ERROR:
					errorMessage = Localization.Instance.GetString("UI_World_AssetBundle_Popup_Desc_LackStorage");
					break;
				case eResponseCode.CANCELED:
				case eResponseCode.PATCH_SERVER_REQUEST_FAILED:
				case eResponseCode.RESPONSE_NULL:
				case eResponseCode.CACHE_ERROR:
				case eResponseCode.INTERNAL_SERVER_ERROR:
				case eResponseCode.TIME_OUT:
				case eResponseCode.CATALOG_UPDATE_ERROR:
				case eResponseCode.UPDATE_CATALOG_ERROR:
				case eResponseCode.CLEAN_BUNDLE_ERROR:
				case eResponseCode.BUNDLE_SIZE_CHECK_ERROR:
				case eResponseCode.BUNDLE_DOWNLOAD_ERROR:
					errorMessage = Localization.Instance.GetString("UI_PatchServer_Popup_Desc_RequestFail");
					break;
				case eResponseCode.PATCH_SERVER_EMPTY_DATA:
					errorMessage = Localization.Instance.GetString("UI_PatchServer_Popup_Desc_DataError");
					break;
			}

			return errorMessage;
		}


		public void Dispose() => Release();

		public void Release()
		{
			C2VAssetBundleManager.RemoveInternalIdTransformFuncHandler();

			_downloadWaitCts?.Cancel();
			_downloadWaitCts?.Dispose();
			_downloadWaitCts = null;

			_answer?.Dispose();
			_answer = null;

			_downloader?.Dispose();
			_downloader = null;

			C2VDebug.LogCategory("AssetBundle", "AssetBundleDownloader Disposed");
		}
	}
}
