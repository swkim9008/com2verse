/*===============================================================
* Product:		Com2Verse
* File Name:	MiceBusinessCardListItemViewModel.cs
* Developer:	wlemon
* Date:			2023-04-06 12:55
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Mice;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("Mice")]
	public class MiceBusinessCardListItemViewModel : ViewModelBase
	{
#region Variables
		private Texture      _image;
		private string       _name;
		private string       _affiliation;
		private bool         _isExchanged;
		private bool         _isSelected;
		private MiceUserInfo _userInfo;

		public CommandHandler Select        { get; }
		public CommandHandler ShowCard      { get; }
#endregion

#region Properties
		public Texture Image
		{
			get => _image;
			set
			{
				SetProperty(ref _image, value);
				InvokePropertyValueChanged(nameof(IsImageValid), IsImageValid);
			}
		}

		public bool IsImageValid
		{
			get => Image != null;
		}

		public string Name
		{
			get => _name;
			set => SetProperty(ref _name, value);
		}

		public string Affiliation
		{
			get => _affiliation;
			set => SetProperty(ref _affiliation, value);
		}

		public bool IsExchanged
		{
			get => _isExchanged;
			set => SetProperty(ref _isExchanged, value);
		}

		public bool IsSelected
		{
			get => _isSelected;
			set => SetProperty(ref _isSelected, value);
		}

		public MiceUserInfo UserInfo => _userInfo;
#endregion

#region Initialize
		public MiceBusinessCardListItemViewModel(MiceUserInfo userInfo, Action<MiceBusinessCardListItemViewModel> onClick, Action<MiceBusinessCardListItemViewModel> onShowCard)
		{
			_userInfo     = userInfo;
			Name          = userInfo.Name;
			Affiliation   = userInfo.Affiliation;
			IsExchanged   = userInfo.ExchangeCode == MiceWebClient.eMiceAccountCardExchangeCode.MUTUAL_FOLLOW;
			IsSelected    = false;
			Select        = new CommandHandler(() => onClick?.Invoke(this));
			ShowCard = new CommandHandler(() => onShowCard?.Invoke(this));

			if (string.IsNullOrEmpty(_userInfo.PhotoThumbnailUrl))
				Image = default(Texture);
			else
				TextureCache.Instance.GetOrDownloadTextureAsync(_userInfo.PhotoThumbnailUrl, (result, texture) => { Image = result ? texture : default(Texture); }).Forget();
		}
#endregion
	}
}
