/*===============================================================
* Product:		Com2Verse
* File Name:	BannerInfoViewModel.cs
* Developer:	mikeyid77
* Date:			2023-08-28 18:02
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.AssetSystem;
using Com2Verse.Banner;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.LruObjectPool;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class BannerInfoViewModel : ViewModelBase
	{
		private BannerInfo _bannerInfo;
		private string     _bannerDate;
		private Texture    _bannerTexture;

		public CommandHandler EnterBannerButtonClicked { get; }

		public string BannerDate
		{
			get => _bannerDate;
			set => SetProperty(ref _bannerDate, value);
		}

		public Texture BannerTexture
		{
			get => _bannerTexture;
			set => SetProperty(ref _bannerTexture, value);
		}

		public BannerInfoViewModel()
		{
			EnterBannerButtonClicked = new CommandHandler(OnEnterBannerButtonClicked);
		}

		public void InitializeBannerInfo(BannerInfo bannerInfo)
		{
			if (bannerInfo == null)
			{
				C2VDebug.LogErrorCategory("Banner", $"BannerInfo is NULL");
				NetworkUIManager.Instance.ShowCommonErrorMessage();
			}
			else
			{
				_bannerInfo = bannerInfo;
				BannerDate  = Localization.Instance.GetString(bannerInfo.Desc);
				SetTextureAsync(bannerInfo.Res);
			}
		}

		private async void SetTextureAsync(string target)
		{
			var imagePath = $"Banner_{target}_All.png";
			if (!C2VAddressables.AddressableResourceExists<Texture>(imagePath))
			{
				var langCode = Localization.Instance.CurrentLanguage.ToString();
				imagePath = $"Banner_{target}_{langCode}.png";
			}
			if (!C2VAddressables.AddressableResourceExists<Texture>(imagePath))
			{
				C2VDebug.LogWarningCategory("Banner", $"Image {imagePath} is doesn't exist asset!");
				return;
			}

			//var texture = await C2VAddressables.LoadAsset<Texture>(imagePath).ToUniTask();
			var texture = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<Texture>(imagePath);
			if (texture == null)
			{
				C2VDebug.LogWarningCategory("Banner", $"{imagePath} Texture is NULL");
			}
			else
			{
				BannerTexture = texture;
			}
		}

		private void OnEnterBannerButtonClicked()
		{
			BannerManager.Instance.InvokeBanner(_bannerInfo);
		}
	}
}
