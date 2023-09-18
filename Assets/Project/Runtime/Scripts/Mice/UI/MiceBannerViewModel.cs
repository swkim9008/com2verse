// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	MiceBannerViewModel.cs
//  * Developer:	seaman2000
//  * Date:		2023-04-28 오후 4:59
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.Logger;
using Com2Verse.Mice;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("Mice")]

	public class MiceBannerViewModel : ViewModelBase
	{
        private Texture2D _bannerImage;
        private int _screenID;

        private string _bannerImageURL;


        //public CommandHandler OnClickCoverView { get; }


		public Texture2D BannerImage
		{
			get => _bannerImage;
			set => SetProperty(ref _bannerImage, value);
		}

		
		public MiceBannerViewModel()
		{
			//OnClickCoverView = new CommandHandler(()=>ShowPdfView().Forget());
        }

		public void SetScreenID(int screenID)
		{
            _screenID = screenID;
            SyncScreenDisplayInfo().Forget();
            UpdateBannerImage().Forget();
        }


  //      public async UniTask ShowPdfView()
		//{
  //          await UniTask.WaitWhile(() => string.IsNullOrEmpty(_pdfURL));

  //          // test url...
  //          //"https://nss-seoul.s3.ap-northeast-2.amazonaws.com/nss/temp/BehaviorDesignerDocumentation.pdf";

  //          // TODO : 스펙이 정해지면 정리해야 함
  //          var screenSize = new Vector2(1700, 1000); //GetScreenSize();
  //          UIManager.Instance.ShowPopupWebView(false,
  //                                              screenSize, //new Vector2(1300, 800),
  //                                              _pdfURL,
  //                                              downloadProgressAction: (eventArgs =>
  //                                              {
  //                                                  if (eventArgs.Type == Vuplex.WebView.ProgressChangeType.Finished)
  //                                                  {
  //                                                      UIManager.Instance.ShowPopupCommon($"DUMMY : PDF 파일 다운로드가 완료되었습니다.",
  //                                                                                         () => { OpenInFileBrowser.Open(eventArgs.FilePath); });
  //                                                      //File.Move(eventArgs.FilePath, someOtherLocation);
  //                                                  }
  //                                                  else if (eventArgs.Type == Vuplex.WebView.ProgressChangeType.Updated)
  //                                                  {
  //                                                      C2VDebug.Log($">>>>>>>>>> {eventArgs.Progress}");
  //                                                  }
  //                                              }));
  //      }


		async UniTask UpdateBannerImage()
		{
            await UniTask.WaitWhile(() => string.IsNullOrEmpty(_bannerImageURL));
            BannerImage = await TextureCache.Instance.GetOrDownloadTextureAsync(_bannerImageURL);
        }


        async UniTask SyncScreenDisplayInfo()
        {
            await UniTask.WaitWhile(() => MiceInfoManager.Instance.GetEventInfo(MiceService.Instance.EventID) == null);
            var curEvent = MiceInfoManager.Instance.GetEventInfo(MiceService.Instance.EventID);

            // TODO : 변경해야 함
            _bannerImageURL = "";

            //await UniTask.WaitWhile(() => curScreenContent.GetDisplayInfo(2) == null);
            //displayInfo = curScreenContent.GetDisplayInfo(2);
            //_pdfURL = displayInfo.DownloadURL;
        }

    }
}
