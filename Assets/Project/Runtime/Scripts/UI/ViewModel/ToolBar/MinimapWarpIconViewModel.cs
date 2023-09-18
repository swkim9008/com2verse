/*===============================================================
* Product:		Com2Verse
* File Name:	MinimapWarpIconViewModel.cs
* Developer:	haminjeong
* Date:			2023-08-29 15:39
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Network;
using Com2Verse.PlayerControl;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class MinimapWarpIconViewModel : ViewModelBase
	{
		private MinimapPopupViewModel _parentViewModel;
		private WarpPosition          _warpIconData;

		private string _iconSpriteName;
		public string IconSpriteName
		{
			get => _iconSpriteName;
			set => SetProperty(ref _iconSpriteName, value);
		}

		private Vector2 _iconPosition;
		public Vector2 IconPosition
		{
			get => _iconPosition;
			set
			{
				float xRatio = (value.x - _parentViewModel.MapData.LeftBottom.x) / _parentViewModel.MapData.Size.x;
				float yRatio = (value.y - _parentViewModel.MapData.LeftBottom.y) / _parentViewModel.MapData.Size.y;

				var position = new Vector2(_parentViewModel.MapImage.texture.width * xRatio, _parentViewModel.MapImage.texture.height * yRatio);
				SetProperty(ref _iconPosition, position);
			}
		}

		public CommandHandler OnWarpButton { get; }

		public MinimapWarpIconViewModel()
		{
			OnWarpButton = new CommandHandler(OnWarpButtonHandler);
		}

		private void OnWarpButtonHandler()
		{
			UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString("UI_Common_Notice_Popup_Title"),
			                                  Localization.Instance.GetString("UI_Map_Warp_Popup_Desc"),
			                                  _ =>
			                                  {
				                                  Commander.Instance.RequestWarpPosition(_warpIconData.ID);
				                                  var playerController = PlayerController.InstanceOrNull;
				                                  if (!playerController.IsUnityNull())
					                                  playerController!.OnMinimapClose();
			                                  });
		}

		public void SetWarpIcon(MinimapPopupViewModel parent, WarpPosition iconData)
		{
			_parentViewModel = parent;
			_warpIconData    = iconData;
			IconSpriteName   = _warpIconData.IconRes;
			IconPosition     = new Vector2(_warpIconData.SpawnLocation.x, _warpIconData.SpawnLocation.z);
		}
	}
}
