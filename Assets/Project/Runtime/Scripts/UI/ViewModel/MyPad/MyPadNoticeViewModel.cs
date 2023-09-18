/*===============================================================
* Product:		Com2Verse
* File Name:	MyPadNoticeViewModel.cs
* Developer:	mikeyid77
* Date:			2023-04-11 10:38
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.AssetSystem;
using Com2Verse.Logger;
using Com2Verse.LruObjectPool;
using Protocols;
using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("MyPad")]
	public sealed class MyPadNoticeViewModel : ViewModelBase
	{
		private MyPadAdvertisement _target = null;
		private Texture _advImage;
		private bool _isAdvertisementRequestPage = false;
		public CommandHandler NoticeButtonClicked { get; }

		public MyPadAdvertisement Target
		{
			get => _target;
			set
			{
				_target = value;
				_isAdvertisementRequestPage = false;
				SetAdvertisementSpriteAsync(value.DispalyContents);
			}
		}
		
		public Texture AdvImage
		{
			get => _advImage;
			set => SetProperty(ref _advImage, value);
		}

		public MyPadNoticeViewModel()
		{
			NoticeButtonClicked = new CommandHandler(OnNoticeButtonClicked);
		}

		private void OnNoticeButtonClicked()
		{
			if (_isAdvertisementRequestPage)
			{
				// TODO : 광고 문의 페이지 출력
				// C2VDebug.LogCategory("MyPad", $"{nameof(OnNoticeButtonClicked)} : RequestAdvertisePage");
				// Application.OpenURL("https://www.com2verse.com/");

				Application.OpenURL("https://company.com2verse.com/media/iframe_notice.php?ptype=view&idx=132&page=1&code=notice_spaxe");
			}
			else
			{
				// if (Target == null) return;
				// if (string.IsNullOrEmpty(Target.LinkMessage)) return;
				//
				// C2VDebug.LogCategory("MyPad", $"{nameof(OnNoticeButtonClicked)} : {Target.DispalyContents}");
				// Application.OpenURL(Target.LinkMessage);
			}
		}

		public void SetAdvertisementRequestPage()
		{
			// TODO : 광고 문의 페이지 리소스`
			_isAdvertisementRequestPage = true;
			SetAdvertisementSpriteAsync("UI_Ad_MyPad_Empty");
		}

		public void SetDummyTexture(string target, bool isFront)
		{
			_isAdvertisementRequestPage = isFront;
			SetAdvertisementSpriteAsync(target);
		}

		private async void SetAdvertisementSpriteAsync(string target)
		{
			var image = $"{target}.png";
			//var texture = await C2VAddressables.LoadAssetAsync<Texture>(image).ToUniTask();
			//씬 변경시 로드한 어셋 자동 삭제하기 위해 아래 함수로 수정함
			var texture = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<Texture>(image);
			if (texture == null)
			{
				C2VDebug.LogWarningCategory("MyPad", $"Can't Find Texture");
			}
			else
			{
				AdvImage = texture;
			}
		}
	}
}
