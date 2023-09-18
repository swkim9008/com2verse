/*===============================================================
* Product:		Com2Verse
* File Name:	MiceBusinessCardImageSelectionViewModel.cs
* Developer:	wlemon
* Date:			2023-04-06 12:55
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Cysharp.Threading.Tasks;
using SimpleFileBrowser;
using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("Mice")]
	public class MiceBusinessCardImageSelectionViewModel : ViewModelBase
	{
		public static readonly string ResName = "UI_Popup_BusinessCard_ImageExchange";
		public static readonly string[] ImageExtensions = new string[] { "png", "jpg" };
		
#region Variables
		private bool    _setVisible;
		
		public CommandHandler SelectMyAvatar { get; }
		public CommandHandler UploadImage { get; }

		public Action<Texture2D> OnImageSelected { get; set; }
#endregion

#region Properties
		public bool SetVisible
		{
			get => _setVisible;
			set => SetProperty(ref _setVisible, value);
		}
#endregion

#region Initialize
		public MiceBusinessCardImageSelectionViewModel()
		{
			SelectMyAvatar = new CommandHandler(() => OnSelectMyAvatar().Forget());
			UploadImage = new CommandHandler(() => OnUploadImage().Forget());
		}

		public override void OnInitialize()
		{
			base.OnInitialize();
			_setVisible = true;
		}
#endregion
		
#region Binding Events
		private async UniTask OnSelectMyAvatar()
		{
			await UniTask.Yield();
			SetVisible = false;
			MiceBusinessCardAvatarPreviewViewModel.ShowView(OnImageSelected).Forget();
		}

		private async UniTask OnUploadImage()
		{
			await UniTask.Yield();
			SetVisible = false;

			FileBrowser.ShowLoadDialog((string[] path) => { MiceBusinessCardImagePreviewViewModel.ShowView(path[0], OnImageSelected); }, null, FileBrowser.PickMode.Files, false,
			                           onBeforeShow: () => { FileBrowser.SetFilters(false, ImageExtensions); });
		}
#endregion

		public static void ShowView(Action<Texture2D> onImageSelected)
		{
			UIManager.Instance.CreatePopup(ResName, (guiView) =>
			{
				guiView.Show();

				var viewModel = guiView.ViewModelContainer.GetViewModel<MiceBusinessCardImageSelectionViewModel>();
				viewModel.OnImageSelected = onImageSelected;
			}).Forget();
		}
	}
}
