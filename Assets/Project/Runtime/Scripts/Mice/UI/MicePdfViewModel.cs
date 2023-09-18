// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	MicePdfViewModel.cs
//  * Developer:	seaman2000
//  * Date:		2023-04-24 오전 11:51
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

	public class MicePdfViewModel : ViewModelBase
	{
        private Texture2D _coverImage;
        private int _screenID;

        private string _coverImageURL;
        private string _pdfURL;


        public CommandHandler OnClickCoverView { get; }


		public Texture2D CoverImage
		{
			get => _coverImage;
			set => SetProperty(ref _coverImage, value);
		}

		
		public MicePdfViewModel()
		{
			OnClickCoverView = new CommandHandler(()=>ShowPdfView().Forget());
        }

		public void SetScreenID(int screenID)
		{
            _screenID = screenID;
            SyncScreenDisplayInfo().Forget();
            UpdateCoverImage().Forget();
        }


        public async UniTask ShowPdfView()
		{
            await UniTask.WaitWhile(() => string.IsNullOrEmpty(_pdfURL));

            // test url...
            //"https://nss-seoul.s3.ap-northeast-2.amazonaws.com/nss/temp/BehaviorDesignerDocumentation.pdf";

            // TODO : 스펙이 정해지면 정리해야 함
            var screenSize = new Vector2(1700, 1000); //GetScreenSize();
            UIManager.Instance.ShowPopupWebView(false,
                                                screenSize, //new Vector2(1300, 800),
                                                _pdfURL,
                                                downloadProgressAction: (eventArgs =>
                                                {
                                                    if (eventArgs.Type == Vuplex.WebView.ProgressChangeType.Finished)
                                                    {
                                                        var text = Data.Localization.eKey.MICE_UI_SessionHall_FileDownload_Msg_Success.ToLocalizationString();

                                                        UIManager.Instance.ShowPopupCommon(text, //$"DUMMY : PDF 파일 다운로드가 완료되었습니다.",
                                                                                           () => { OpenInFileBrowser.Open(eventArgs.FilePath); });
                                                        //File.Move(eventArgs.FilePath, someOtherLocation);
                                                    }
                                                    else if (eventArgs.Type == Vuplex.WebView.ProgressChangeType.Updated)
                                                    {
                                                        C2VDebug.Log($">>>>>>>>>> {eventArgs.Progress}");
                                                    }
                                                }));
        }


		async UniTask UpdateCoverImage()
		{
            await UniTask.WaitWhile(() => string.IsNullOrEmpty(_coverImageURL));
            CoverImage = await TextureCache.Instance.GetOrDownloadTextureAsync(_coverImageURL);
        }


        async UniTask SyncScreenDisplayInfo()
        {
            // await UniTask.WaitWhile(() => MiceInfoManager.Instance.GetEvent(MiceService.Instance.EventID) == null);
            // var curEvent = MiceInfoManager.Instance.GetEvent(MiceService.Instance.EventID);
            //
            // await UniTask.WaitWhile(() => curEvent.GetScreenContent<MiceLeafletScreenContentInfo>(_screenID) == null);
            // var curScreenContent = curEvent.GetScreenContent<MiceLeafletScreenContentInfo>(_screenID);
            //
            // await UniTask.WaitWhile(() => curScreenContent.GetDisplayInfo(1) == null);
            // var displayInfo = curScreenContent.GetDisplayInfo(1);
            // _coverImageURL = displayInfo.CoverImageURL;
            //
            // await UniTask.WaitWhile(() => curScreenContent.GetDisplayInfo(2) == null);
            // displayInfo = curScreenContent.GetDisplayInfo(2);
            // _pdfURL = displayInfo.DownloadURL;
            await UniTask.CompletedTask;
        }

    }
}
