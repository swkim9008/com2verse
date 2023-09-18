/*===============================================================
* Product:		Com2Verse
* File Name:	BannerRootViewModel.cs
* Developer:	mikeyid77
* Date:			2023-08-28 17:52
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using UnityEngine;
using Com2Verse.Banner;
using Com2Verse.Data;
using Com2Verse.Logger;
using DG.Tweening;

namespace Com2Verse.UI
{
	public sealed class BannerRootViewModel : ViewModelBase
	{
		private Collection<BannerInfoViewModel> _bannerInfoCollection = new();

		private RectTransform _targetContents        = null;
		private float         _targetContentsWidth   = 358f;
		private int           _targetContentIndex    = 0;
		private float         _targetConetnsDuration = 0.5f;

		private bool _showBannerRoot       = true;
		private bool _showBannerButton     = false;

		public CommandHandler CloseBannerButtonClicked    { get; }
		public CommandHandler PreviousBannerButtonClicked { get; }
		public CommandHandler NextBannerButtonClicked     { get; }

#region Property
		public Collection<BannerInfoViewModel> BannerInfoCollection
		{
			get => _bannerInfoCollection;
			set => SetProperty(ref _bannerInfoCollection, value);
		}

		public RectTransform TargetContents
		{
			get => _targetContents;
			set => SetProperty(ref _targetContents, value);
		}

		public bool ShowBannerRoot
		{
			get => _showBannerRoot;
			set => SetProperty(ref _showBannerRoot, value);
		}

		public bool ShowBannerButton
		{
			get => _showBannerButton;
			set => SetProperty(ref _showBannerButton, value);
		}
#endregion // Property

		public BannerRootViewModel()
		{
			CloseBannerButtonClicked    = new CommandHandler(OnCloseBannerButtonClicked);
			PreviousBannerButtonClicked = new CommandHandler(OnPreviousBannerButtonClicked);
			NextBannerButtonClicked     = new CommandHandler(OnNextBannerButtonClicked);
		}

		public void InitializeBannerRoot(bool needShowBanner, List<BannerInfo> targetList)
		{
			if (needShowBanner)
			{
				C2VDebug.LogCategory("Banner", $"Initialize Banner UI");
				foreach (var target in targetList)
				{
					var bannerInfoViewModel = new BannerInfoViewModel();
					bannerInfoViewModel?.InitializeBannerInfo(target);
					_bannerInfoCollection?.AddItem(bannerInfoViewModel);
				}

				C2VDebug.LogCategory("Banner", $"Banner Contents Count : {_bannerInfoCollection?.Value?.Count}");
				if (_bannerInfoCollection?.Value?.Count == 0)
				{
					ShowBannerRoot   = false;
					ShowBannerButton = false;
				}
				else
				{
					ShowBannerRoot   = true;
					ShowBannerButton = _bannerInfoCollection?.Value?.Count > 1;
				}
			}
			else
			{
				ShowBannerRoot   = false;
				ShowBannerButton = false;
			}
		}

		public void DisposeDoTween()
		{
			TargetContents.DOKill();
		}

		private void OnCloseBannerButtonClicked()
		{
			C2VDebug.LogCategory("Banner", $"Close Banner UI");
			ShowBannerRoot = false;
			BannerManager.Instance.ResetBanner(false);
		}

		private void OnPreviousBannerButtonClicked()
		{
			if (!ShowBannerButton) return;

			_targetContentIndex--;
			if (_targetContentIndex < 0) _targetContentIndex = _bannerInfoCollection.Value.Count - 1;

			TargetContents.DOAnchorPos(new Vector2(_targetContentsWidth * -_targetContentIndex, 0), _targetConetnsDuration);
			BannerManager.Instance.RefreshUpdate();
		}

		public void OnNextBannerButtonClicked()
		{
			if (!ShowBannerButton) return;

			_targetContentIndex++;
			if (_targetContentIndex >= _bannerInfoCollection.Value.Count) _targetContentIndex = 0;

			TargetContents.DOAnchorPos(new Vector2(_targetContentsWidth * -_targetContentIndex, 0), _targetConetnsDuration);
			BannerManager.Instance.RefreshUpdate();
		}
	}
}
