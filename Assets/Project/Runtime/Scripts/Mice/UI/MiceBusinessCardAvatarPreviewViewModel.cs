/*===============================================================
* Product:		Com2Verse
* File Name:	MiceBusinessCardImageAvatarViewModel.cs
* Developer:	wlemon
* Date:			2023-04-06 12:55
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Mice;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("Mice")]
	public class MiceBusinessCardAvatarPreviewViewModel : ViewModelBase
	{
		public static readonly string ResName = "UI_Popup_BusinessCard_AvatarPreview";

#region Variables
		private Texture2D _avatarImage;

		public CommandHandler Apply { get; }

		public Action<Texture2D> OnImageSelected { get; set; }
#endregion

#region Properties
		public Texture2D AvatarImage
		{
			get => _avatarImage;
			set => SetProperty(ref _avatarImage, value);
		}
#endregion

#region Initialize
		public MiceBusinessCardAvatarPreviewViewModel()
		{
			Apply = new CommandHandler(OnApply);
		}
#endregion

#region Binding Events
		private void OnApply()
		{
			OnImageSelected?.Invoke(AvatarImage);
		}
#endregion

		public static async UniTask ShowView(Action<Texture2D> onImageSelected)
		{
			UIManager.Instance.ShowWaitingResponsePopup();

			var texture    = default(Texture2D);
			var avatarInfo = User.Instance.AvatarInfo ?? null;
			if (avatarInfo != null)
			{
				texture = await BusinessCardRT.CreateAsync(avatarInfo);
			}
			await UIManager.Instance.CreatePopup(ResName, async (guiView) =>
			{
				guiView.Show();

				var viewModel = guiView.ViewModelContainer.GetViewModel<MiceBusinessCardAvatarPreviewViewModel>();
				viewModel.AvatarImage     = texture;
				viewModel.OnImageSelected = onImageSelected;
			});

			UIManager.Instance.HideWaitingResponsePopup();

		}
	}
}
