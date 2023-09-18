/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarSelectionFaceViewModel_Edit.cs
* Developer:	eugene9721
* Date:			2023-03-29 19:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Avatar;
using Com2Verse.Data;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	// Phase 4-4: 얼굴 편집
	public partial class AvatarSelectionFaceViewModel
	{
		private const string PresetMenuKey = "UI_AvatarCreate_Face_Menu_Preset";
		private const string FaceMenuKey   = "UI_AvatarCreate_Face_Menu_Face";
		private const string MakeUpMenuKey = "UI_AvatarCreate_Face_Menu_MakeUp";
		private const string HairMenuKey   = "UI_AvatarCreate_Face_Menu_Hair";

#region Fields
		private Collection<CustomizeSubMenuViewModel>    _faceSubMenuList = new();
		private Collection<CustomizeFaceOptionViewModel> _faceOptionList  = new();

		private bool _hasSubMenu;
#endregion Fields

#region Field Properties
		[UsedImplicitly]
		public Collection<CustomizeSubMenuViewModel> FaceSubMenuList
		{
			get => _faceSubMenuList;
			set => SetProperty(ref _faceSubMenuList, value);
		}

		[UsedImplicitly]
		public Collection<CustomizeFaceOptionViewModel> FaceOptionList
		{
			get => _faceOptionList;
			set => SetProperty(ref _faceOptionList, value);
		}

		[UsedImplicitly]
		public bool HasSubMenu
		{
			get => _hasSubMenu;
			set => SetProperty(ref _hasSubMenu, value);
		}

		[UsedImplicitly] public CommandHandler ClickSetForceLayoutRebuildNextFrame { get; set; }
#endregion Field Properties

#region AvatarSelectionViewModelBase
		private void OnShowEdit()
		{
			IsSelectMethodPhase  = false;
			IsTakePicturePhase   = false;
			IsUploadPicturePhase = false;

			IsOnTakePictureButton      = false;
			IsOnConfirmGeneratedAvatar = false;
			IsOnAvatarGenerateSlider   = false;
			IsOnEditImage              = false;

			IsOnVideoImage  = false;
			IsOnAvatarImage = false;

			CleanUploadTexture();
			AddMenuItems();

			if (Owner == null) return;

			if (Owner.ItemMenuList.Value is { Count: > 0 })
				OnMenuClicked(Owner.ItemMenuList.FirstItem()!);

			Owner.SubTitleTextKey = AvatarSelectionManagerViewModel.FaceEditFaceSubTitleTextKey;
		}

		private void OnHideEdit()
		{
			HasSubMenu = false;
		}

		private void OnClearEdit()
		{
		}
#endregion AvatarSelectionViewModelBase

#region Handlers
		private void OnFaceMenuChanged(eFaceMenu menu)
		{
			ClearSubMenuCollection();
			OnFaceMenuClick(menu);
		}

		private void OnFaceMenuClick(eFaceMenu menu)
		{
			HasSubMenu = CheckHasSubMenu(menu);
			ClearSubMenuCollection();
			if (HasSubMenu)
				SetSubMenuCollection(menu);

			SetFaceOptionCollection(GetFirstSubMenu(menu));
		}

		public void OnClickSetForceLayoutRebuildNextFrame()
		{
			ForceLayoutRebuildNextFrameNextFrame().Forget();
		}

		private async UniTask ForceLayoutRebuildNextFrameNextFrame()
		{
			await UniTask.NextFrame();
			InvokePropertyValueChanged(nameof(SetForceLayoutRebuild), SetForceLayoutRebuild);
		}

		private void OnReGenerateAiButtonClick()
		{
			var closet      = AvatarMediator.Instance.AvatarCloset;
			var currentType = closet.AvatarEditData.AvatarType;
			closet.ClearAvatar();
			closet.Controller?.OnClear();
			closet.AvatarEditData.AvatarType = currentType;

			IsSelectMethodPhase = true;
			Owner?.RefreshButtons();
		}
#endregion Handlers

#region MenuUtil
		private bool CheckHasSubMenu(eFaceMenu menu) => AvatarTable.CheckHasSubMenu(menu);

		private eFaceSubMenu GetFirstSubMenu(eFaceMenu menu) => AvatarTable.GetFirstSubMenu(menu);
#endregion MenuUtil

#region Private Methods
		private void GoToFaceEditPhase()
		{
			if (Owner == null) return;

			Owner.ShowPreviewAvatarAsync(() =>
			{
				IsEditPhase = true;
				Owner.RefreshButtons();
			}).Forget();
		}
#endregion Private Methods
	}
}
