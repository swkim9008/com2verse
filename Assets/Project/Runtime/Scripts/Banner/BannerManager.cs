/*===============================================================
* Product:		Com2Verse
* File Name:	BannerManager.cs
* Developer:	mikeyid77
* Date:			2023-08-28 17:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using Localization = Com2Verse.UI.Localization;

namespace Com2Verse.Banner
{
	public sealed class BannerManager : Singleton<BannerManager>, IDisposable
	{
		private BannerRootViewModel _bannerViewModel = null;
		private bool                _needShowBanner  = true;

		private const float RefreshCycle = 10f;
		private       float _timeLeft    = 10f;
		private       bool  _forcePause  = false;
		private       bool  _forceStop   = false;

		public void ShowBanner(bool isWorldScene)
		{
			if (isWorldScene)
			{
				var targetBannerList = GetTargetList();
				_bannerViewModel = MasterViewModel.Get<BannerRootViewModel>();
				_bannerViewModel?.InitializeBannerRoot(_needShowBanner, targetBannerList);

				if (_needShowBanner && targetBannerList?.Count > 1)
					OnTimeUpdate().Forget();
			}
			else
			{
				_bannerViewModel = MasterViewModel.Get<BannerRootViewModel>();
				_bannerViewModel?.InitializeBannerRoot(false, null);
			}
		}

		public void ResetBanner()
		{
			_bannerViewModel?.DisposeDoTween();
			_bannerViewModel = null;
			_forceStop       = true;
		}

		public void ResetBanner(bool needShowBanner)
		{
			_needShowBanner = needShowBanner;
			ResetBanner();
		}

		public void InvokeBanner(BannerInfo bannerInfo)
		{
			if (bannerInfo == null) return;

			PauseUpdate();
			UIManager.Instance.ShowPopupYesNo(
				BannerString.BannerPopupTitle, BannerString.BannerPopupContext(bannerInfo.Title),
				(yesAction) =>
				{
					if (CheckBannerTime(bannerInfo))
					{
						if (bannerInfo.WarpID < 0)
						{
							C2VDebug.LogWarningCategory("Banner", $"Invalid WarpId : {bannerInfo.WarpID}");
							NetworkUIManager.Instance.ShowCommonErrorMessage();
						}
						else
						{
							C2VDebug.LogCategory("Banner", $"Warp to Target : {bannerInfo.WarpID}");
							Commander.Instance.RequestWarpPosition(bannerInfo.WarpID);
						}
					}
					else
					{
						UIManager.Instance.SendToastMessage(BannerString.BannerToastNotAvailable);
					}
				}, null,
				(guiView) =>
				{
					guiView.OnClosedEvent += (_) =>
					{
						RefreshUpdate();
					};
				},
				BannerString.BannerPopupYes, BannerString.BannerPopupNo);
		}

		public void Dispose()
		{
			ResetBanner(true);
		}

#region TableData
		private List<BannerInfo> GetTargetList()
		{
			var targetList = new List<BannerInfo>();
			var bannerTable = TableDataManager.Instance.Get<TableBannerInfo>()?.Datas?.Values?.ToList();
			foreach (var bannerData in bannerTable)
			{
				if (!bannerData?.IsDisplay ?? false) continue;
				if (!CheckBannerTime(bannerData)) continue;
				targetList.Add(bannerData);
			}
			return targetList.OrderBy(info => info.SortOrder).ToList();
		}

		private bool CheckBannerTime(BannerInfo bannerData)
		{
			var targetFormat = "yyyy-MM-dd HH:mm:ss";
			var target = MetaverseWatch.NowDateTime;
			var startDate    = DateTime.ParseExact(bannerData.StartData, targetFormat, null);
			var endDate      = DateTime.ParseExact(bannerData.EndData,   targetFormat, null);
			return startDate <= target && target <= endDate;
		}
#endregion // TableData

#region Refresh
		public void RefreshUpdate()
		{
			_timeLeft   = RefreshCycle;
			_forcePause = false;
		}

		public void PauseUpdate()
		{
			_forcePause = true;
		}

		private async UniTask OnTimeUpdate()
		{
			_timeLeft   = RefreshCycle;
			_forcePause = false;
			_forceStop  = false;

			while (!_forceStop)
			{
				if (!_forcePause)
				{
					_timeLeft -= Time.deltaTime;
					if (_timeLeft < 0)
					{
						_bannerViewModel?.OnNextBannerButtonClicked();
						_timeLeft = RefreshCycle;
					}
				}

				await UniTask.Yield();
			}
			await UniTask.Yield();
		}
#endregion // Refresh

#region Utils
		private BannerManager() { }

		class BannerString
		{
			public static string BannerPopupTitle                  => Localization.Instance.GetString("UI_Common_Notice_Popup_Title");
			public static string BannerPopupContext(string target) => Localization.Instance.GetString("UI_Banner_Popup_Desc", Localization.Instance.GetString(target));
			public static string BannerPopupYes                    => Localization.Instance.GetString("UI_Common_Btn_Move");
			public static string BannerPopupNo                     => Localization.Instance.GetString("UI_Common_Btn_Cancel");
			public static string BannerToastNotAvailable           => Localization.Instance.GetString("UI_Banner_Toast_Msg_NotAvailable");
		}
#endregion // Utils
	}
}
