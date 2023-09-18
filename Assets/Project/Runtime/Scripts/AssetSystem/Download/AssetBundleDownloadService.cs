/*===============================================================
* Product:		Com2Verse
* File Name:	AssetBundleDownloader.cs
* Developer:	tlghks1009
* Date:			2023-03-17 14:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;


namespace Com2Verse.AssetSystem
{
    public class AssetBundleDownloadService : Singleton<AssetBundleDownloadService>, IDisposable
    {
        public class AssetBundleRequestResult
        {
            public string Error { get; set; }
        }


        private static readonly Dictionary<eRequestAssetBundleType, eAssetBundleType[]> AssetBundleDownloadDictionary = new()
        {
            {eRequestAssetBundleType.LOGIN, new[] {eAssetBundleType.OFFICE, eAssetBundleType.COMMON, eAssetBundleType.MICE, eAssetBundleType.WORLD}},
            {eRequestAssetBundleType.OFFICE, new[] {eAssetBundleType.OFFICE, eAssetBundleType.COMMON}},
            {eRequestAssetBundleType.MICE, new[] {eAssetBundleType.MICE, eAssetBundleType.COMMON}},
        };

        private Action<AssetBundleRequestResult> _onAssetBundleRequestResultCallback;

        private C2VAddressablesDownloader _downloader;

        [UsedImplicitly] private AssetBundleDownloadService() { }

        public C2VAddressablesDownloader CreateDownloader(eRequestAssetBundleType requestAssetBundleType, string assetBundleVersion)
        {
            var assetBundleLoadInfo = CreateAssetBundleLoadInfo(requestAssetBundleType, assetBundleVersion);

            return C2VAddressablesDownloader.Create(assetBundleLoadInfo);
        }


        public void RequestAssetBundle(eRequestAssetBundleType requestAssetBundleType, string assetBundleVersion, Action<AssetBundleRequestResult> resultCallback)
        {
            _onAssetBundleRequestResultCallback = resultCallback;

            _downloader = CreateDownloader(requestAssetBundleType, assetBundleVersion);
            _downloader.OnRemoteAssetBundlesSizeDownloadCompleted.AddListener(OnAssetBundleSizeDownloadCompleted);

            _downloader.Download();
        }


        private void OnAssetBundleSizeDownloadCompleted(C2VAddressablesEvent<bool> answer, long size)
        {
            _downloader?.OnRemoteAssetBundlesSizeDownloadCompleted.RemoveListener(OnAssetBundleSizeDownloadCompleted);

            if (size > 0)
            {
                // FIXME : String Table.
                var downloadSize   = C2VAddressablesDownloadUtility.GetConvertedByteString(size);
                var downloadString = $"추가 다운로드 파일이 있습니다. (약 {downloadSize}) 다운로드를 진행하시겠습니까?";

                UIManager.Instance.ShowPopupYesNo(string.Empty, downloadString,
                                                  (guiView) =>
                                                  {
                                                      guiView.OnClosedEvent += OnClosedYesNoPopup;
                                                  },
                                                  (guiView) =>
                                                  {
                                                      answer?.Invoke(false);
                                                      _onAssetBundleRequestResultCallback?.Invoke(new AssetBundleRequestResult() {Error = "Canceled"});
                                                  },
                                                  null, "다운로드", "종료");

                void OnClosedYesNoPopup(GUIView guiView)
                {
                    guiView.OnClosedEvent -= OnClosedYesNoPopup;

                    _downloader?.OnRemoteAssetBundlesDownloadCompleted.AddListener(OnAssetBundleDownloadCompleted);

                    UIManager.Instance.CreatePopup("UI_Popup_Progress", (guiView) =>
                    {
                        guiView.Show();
                        answer?.Invoke(true);
                    }).Forget();
                }
            }
            else
            {
                answer?.Invoke(false);

                _onAssetBundleRequestResultCallback?.Invoke(new AssetBundleRequestResult() {Error = string.Empty});
            }
        }


        private void OnAssetBundleDownloadCompleted()
        {
            _downloader?.OnRemoteAssetBundlesDownloadCompleted.RemoveListener(OnAssetBundleDownloadCompleted);

            _onAssetBundleRequestResultCallback?.Invoke(new AssetBundleRequestResult() {Error = string.Empty});
        }


        private static C2VAddressableAssetBundleLoadInfo CreateAssetBundleLoadInfo(eRequestAssetBundleType requestAssetBundleType, string assetBundleVersion)
        {
            if (AssetBundleDownloadDictionary.TryGetValue(requestAssetBundleType, out var assetBundleTypes))
            {
                return new C2VAddressableAssetBundleLoadInfo
                (
                    AppInfo.Instance.Data.BuildTarget,
                    AppInfo.Instance.Data.Version,
                    assetBundleVersion,
                    AppInfo.Instance.Data.Environment.ToString(),
                    assetBundleTypes
                );
            }

            return null;
        }

        public void Dispose()
        {
            _downloader?.Dispose();
            _downloader = null;
        }
    }
}
