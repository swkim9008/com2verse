/*===============================================================
* Product:		Com2Verse
* File Name:	CustomizeFaceOptionViewModel.cs
* Developer:	eugene9721
* Date:			2023-05-02 17:56
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Avatar;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("AvatarCustomize")]
	public sealed class CustomizeFaceOptionViewModel : ViewModelBase, IDisposable
	{
		private const string PresetListKey = "UI_AvatarCreate_Preset_Option_PresetList";
		private const string FaceShapeKey  = "UI_AvatarCreate_Face_Option_FaceShape";
		private const string EyeShapeKey   = "UI_AvatarCreate_Face_Option_EyeShape";
		private const string PupilTypeKey  = "UI_AvatarCreate_Face_Option_PupilType";
		private const string EyeBrowKey    = "UI_AvatarCreate_Face_Option_EyeBrowOption";
		private const string NoseShapeKey  = "UI_AvatarCreate_Face_Option_NoseShape";
		private const string MouthShapeKey = "UI_AvatarCreate_Face_Option_MouthShape";
		private const string EyeMakeUpKey  = "UI_AvatarCreate_MakeUp_Option_EyeMakeUpType";
		private const string EyeLashKey    = "UI_AvatarCreate_MakeUp_Option_EyeLash";
		private const string CheekTypeKey  = "UI_AvatarCreate_MakeUp_Option_CheekType";
		private const string LipTypeKey    = "UI_AvatarCreate_MakeUp_Option_LipType";
		private const string TattooKey     = "UI_AvatarCreate_MakeUp_Option_TattooOption";
		private const string HairStyleKey  = "UI_AvatarCreate_Hair_Option_HairStyle";

		private readonly Vector2 _facePresetCellSize = new Vector2(130, 216);
		private readonly Vector2 _faceItemCellSize   = new Vector2(102, 102);

#region Fields
		private Collection<CustomizeColorItemViewModel> _colorItems = new();
		private Collection<CustomizeItemViewModel>      _itemSlots  = new();

		private eFaceOption _faceOption;

		private bool _hasDropdown;
		private bool _hasItemSlot;
		private bool _hasColorPalette;
		private bool _isDropdownOpen;
#endregion Fields

#region Field Properites
		[UsedImplicitly]
		public Collection<CustomizeColorItemViewModel> ColorItems
		{
			get => _colorItems;
			set => SetProperty(ref _colorItems, value);
		}

		[UsedImplicitly]
		public Collection<CustomizeItemViewModel> ItemSlots
		{
			get => _itemSlots;
			set => SetProperty(ref _itemSlots, value);
		}

		[UsedImplicitly]
		public eFaceOption FaceOption
		{
			get => _faceOption;
			set
			{
				SetProperty(ref _faceOption, value);
				InitializeFaceOption(value);
				InvokePropertyValueChanged(nameof(FaceOptionName),     FaceOptionName);
				InvokePropertyValueChanged(nameof(FaceOptionCellSize), FaceOptionCellSize);
			}
		}

		[UsedImplicitly]
		public string FaceOptionName
		{
			get
			{
				switch (_faceOption)
				{
					case eFaceOption.PRESET_LIST:
						return Localization.Instance.GetString(PresetListKey);
					case eFaceOption.FACE_SHAPE:
						return Localization.Instance.GetString(FaceShapeKey);
					case eFaceOption.EYE_SHAPE:
						return Localization.Instance.GetString(EyeShapeKey);
					case eFaceOption.PUPIL_TYPE:
						return Localization.Instance.GetString(PupilTypeKey);
					case eFaceOption.EYE_BROW_OPTION:
						return Localization.Instance.GetString(EyeBrowKey);
					case eFaceOption.NOSE_SHAPE:
						return Localization.Instance.GetString(NoseShapeKey);
					case eFaceOption.MOUTH_SHAPE:
						return Localization.Instance.GetString(MouthShapeKey);
					case eFaceOption.EYE_MAKE_UP_TYPE:
						return Localization.Instance.GetString(EyeMakeUpKey);
					case eFaceOption.EYE_LASH:
						return Localization.Instance.GetString(EyeLashKey);
					case eFaceOption.CHEEK_TYPE:
						return Localization.Instance.GetString(CheekTypeKey);
					case eFaceOption.LIP_TYPE:
						return Localization.Instance.GetString(LipTypeKey);
					case eFaceOption.TATTOO_OPTION:
						return Localization.Instance.GetString(TattooKey);
					case eFaceOption.HAIR_STYLE:
						return Localization.Instance.GetString(HairStyleKey);
				}

				return string.Empty;
			}
		}

		[UsedImplicitly]
		public Vector2 FaceOptionCellSize => _faceOption == eFaceOption.PRESET_LIST ? _facePresetCellSize : _faceItemCellSize;

		[UsedImplicitly]
		public bool HasItemSlot
		{
			get => _hasItemSlot;
			set => SetProperty(ref _hasItemSlot, value);
		}

		[UsedImplicitly]
		public bool HasDropdown
		{
			get => _hasDropdown;
			set => SetProperty(ref _hasDropdown, value);
		}

		[UsedImplicitly]
		public bool HasColorPalette
		{
			get => _hasColorPalette;
			set => SetProperty(ref _hasColorPalette, value);
		}

		[UsedImplicitly]
		public bool IsDropdownOpen
		{
			get => _isDropdownOpen;
			set
			{
				SetProperty(ref _isDropdownOpen, value);
				// TODO
				InvokePropertyValueChanged(nameof(HasDropdown),     HasDropdown);
				InvokePropertyValueChanged(nameof(HasColorPalette), HasColorPalette);
			}
		}
#endregion Field Properites

#region Command Properties
		[UsedImplicitly] public CommandHandler<bool> SetDropdownOpen                     { get; }
		[UsedImplicitly] public CommandHandler       ClickSetForceLayoutRebuildNextFrame { get; set; }

#endregion Command Properties

#region Initialize
		public CustomizeFaceOptionViewModel()
		{
			SetDropdownOpen                     = new CommandHandler<bool>(OnSetDropdownOpen);
			ClickSetForceLayoutRebuildNextFrame = new CommandHandler(OnClickSetForceLayoutRebuildNextFrame);
		}

		public void Dispose()
		{
			ClearColorItems();
			ClearItemSlots();
		}

		private void ClearItemSlots()
		{
			if (_itemSlots.Value == null)
				return;

			foreach (var itemSlot in _itemSlots.Value)
				itemSlot.OnSelectedEvent -= OnItemSlotSelected;

			_itemSlots.Reset();
		}

		private void ClearColorItems()
		{
			if (_colorItems.Value == null)
				return;

			foreach (var colorItem in _colorItems.Value)
				colorItem.OnColorClickedEvent -= OnColorItemSelected;

			_colorItems.Reset();
		}
		private void InitializeFaceOption(eFaceOption faceOption)
		{
			if (!AvatarTable.FaceOptionFeatures.TryGetValue(faceOption, out var faceOptionFeature))
			{
				ClearColorItems();
				ClearItemSlots();
				HideAllOption();
				C2VDebug.LogErrorCategory(GetType().Name, $"Not found face option feature. faceOption: {faceOption}");
				return;
			}

			if (faceOptionFeature.HasEmptySlot)
				SetEmptySlot();

			SetFaceOptionColorType(faceOptionFeature.ColorType, faceOption);

			AddItemSlots(faceOption, faceOptionFeature);
		}

		private void AddItemSlots(eFaceOption faceOption, AvatarTable.FaceOptionFeature faceOptionFeature)
		{
			ClearItemSlots();
			var avatarCloset = AvatarMediator.Instance.AvatarCloset;
			var faceItemList = avatarCloset.GetFaceItemList();
			if (faceItemList == null)
				return;

			if (faceOption == eFaceOption.PRESET_LIST)
				SetAiPresetItemSlot(avatarCloset);

			foreach (var id in faceItemList)
			{
				var faceItem = AvatarTable.GetFaceItem(id);
				if (faceItem?.FaceOption != faceOption)
					continue;

				if (avatarCloset.CurrentAvatar.IsUnityNull() || avatarCloset.CurrentAvatar!.Info == null)
					continue;

				if (faceItem.AvatarType != avatarCloset.CurrentAvatar.Info.AvatarType)
					continue;

				AddItemSlot(faceItem, faceOptionFeature.ColorType);
			}
		}

		private void SetAiPresetItemSlot(AvatarCloset avatarCloset)
		{
			var parentViewModel = ViewModelManager.Instance.Get<AvatarSelectionFaceViewModel>();

			var aiCreateItem = parentViewModel?.CurrentAiCreateItem;
			if (aiCreateItem != null)
			{
				aiCreateItem.OnSelectedEvent += OnItemSlotSelected;
				_itemSlots.AddItem(aiCreateItem);
				aiCreateItem.SetAdditionalInfo(avatarCloset.Controller?.IsUseAdditionalInfoAtItem ?? false);
			}
		}

		private void AddItemSlot(FaceItem itemInfo, AvatarTable.eFaceOptionColorType colorType)
		{
			switch (colorType)
			{
				case AvatarTable.eFaceOptionColorType.NONE:
				case AvatarTable.eFaceOptionColorType.RGB:
				case AvatarTable.eFaceOptionColorType.SKIN:
					AddItemSlotAsync(itemInfo);
					break;
				case AvatarTable.eFaceOptionColorType.TEXTURE:
					// TODO: 이미 ItemKey가 있는경우 continue
					AddItemSlotAsync(itemInfo);
					break;
			}
		}

		private void AddItemSlotAsync(FaceItem itemInfo)
		{
			var itemViewModel = new CustomizeItemViewModel
			{
				ItemId            = itemInfo.id,
				CustomizeItemType = AvatarTable.eCustomizeItemType.FACE,
			};
			itemViewModel.OnSelectedEvent += OnItemSlotSelected;
			// LayoutRebuild상황을 고려하여 AddItemSlot을 먼저 호출
			_itemSlots.AddItem(itemViewModel);

			var avatarCloset = AvatarMediator.Instance.AvatarCloset;
			itemViewModel.SetAdditionalInfo(avatarCloset.Controller?.IsUseAdditionalInfoAtItem ?? false);
		}

		private void SetFaceOptionColorType(AvatarTable.eFaceOptionColorType colorType, eFaceOption faceOption)
		{
			// TODO
			// _colorType = colorType;
			switch (colorType)
			{
				case AvatarTable.eFaceOptionColorType.NONE:
					HasItemSlot     = true;
					HasColorPalette = false;
					HasDropdown     = true;
					return;
				case AvatarTable.eFaceOptionColorType.RGB:
					HasItemSlot     = true;
					HasColorPalette = true;
					HasDropdown     = true;
					SetColorPaletteFromColorTable(faceOption);
					break;
				case AvatarTable.eFaceOptionColorType.TEXTURE:
					HasItemSlot     = true;
					HasColorPalette = true;
					HasDropdown     = true;
					break;
				case AvatarTable.eFaceOptionColorType.SKIN:
					HasItemSlot     = true;
					HasColorPalette = true;
					HasDropdown     = true;
					SetColorPaletteSkin();
					break;
			}
		}

		private void HideAllOption()
		{
			HasItemSlot     = false;
			HasColorPalette = false;
			HasDropdown     = false;
		}

		/// <summary>
		/// ColorType이 RGB인 경우, 해당 faceOption에 미리 정의된 컬러 팔레트 데이터를 가져옴
		/// </summary>
		private void SetColorPaletteFromColorTable(eFaceOption faceOption)
		{
			ClearColorItems();
			List<string>? colorTable = null;
			switch (faceOption)
			{
				case eFaceOption.EYE_BROW_OPTION:
					colorTable = AvatarTable.EyeBrowColorHtmlStringList;
					break;
				case eFaceOption.EYE_LASH:
					colorTable = AvatarTable.EyeLashColorHtmlStringList;
					break;
				case eFaceOption.HAIR_STYLE:
					colorTable = AvatarTable.HairColorHtmlStringList;
					break;
			}

			if (colorTable == null)
				return;

			var colorIndex = 0;
			foreach (var htmlStringColor in colorTable)
			{
				var colorItemViewModel = new CustomizeColorItemViewModel
				{
					ColorToHex = htmlStringColor,
					ColorIndex = colorIndex++,
				};
				colorItemViewModel.OnColorClickedEvent += OnColorItemSelected;
				_colorItems.AddItem(colorItemViewModel);
			}
		}

		/// <summary>
		/// ColorType이 Skin인 경우, 데이터 테이블에 정의된 스킨 컬러 팔레트 데이터를 가져옴
		/// </summary>
		private void SetColorPaletteSkin()
		{
			ClearColorItems();

			var avatarCloset = AvatarMediator.Instance.AvatarCloset;
			var faceItemList = avatarCloset.GetFaceItemList();
			if (faceItemList == null)
				return;

			var colorIndex = 0;
			foreach (var faceItemId in faceItemList)
			{
				if (AvatarTable.IdToFaceOption(faceItemId) != eFaceOption.SKIN_TYPE)
					continue;

				var skinItem = AvatarTable.GetFaceItem(faceItemId);
				if (skinItem == null)
					continue;

				if (avatarCloset.CurrentAvatarInfo != null && avatarCloset.CurrentAvatarInfo.AvatarType != skinItem.AvatarType)
					continue;

				var colorItemViewModel = new CustomizeColorItemViewModel
				{
					ItemId     = faceItemId,
					ColorToHex = $"#{skinItem.Color}",
					ColorIndex = colorIndex++,
				};
				colorItemViewModel.OnColorClickedEvent += OnColorItemSelected;
				_colorItems.AddItem(colorItemViewModel);
			}
		}

		private void SetEmptySlot() { }

		public void SetCurrentPresetItem()
		{
			var faceViewModel = ViewModelManager.Instance.Get<AvatarSelectionFaceViewModel>();

			if (faceViewModel == null)
				return;

			if (_itemSlots.Value != null)
				foreach (var itemViewModel in _itemSlots.Value)
					itemViewModel.IsSelected = itemViewModel.ItemId == faceViewModel.LastSelectedPresetId;

			if (faceViewModel.LastSelectedPresetId == 0)
			{
				var firstItem = _itemSlots.FirstItem();
				if (firstItem != null)
					firstItem.IsSelected = true;
			}
		}

		public void SelectCurrentAvatarItem(FaceItemInfo faceItem)
		{
			if (_itemSlots.Value != null)
				foreach (var itemViewModel in _itemSlots.Value)
					itemViewModel.IsSelected = itemViewModel.ItemId == faceItem.ItemId;

			if (_colorItems.Value != null)
				foreach (var colorItemViewModel in _colorItems.Value)
					colorItemViewModel.IsSelected = colorItemViewModel.ColorIndex == faceItem.ColorId;

			if (faceItem.FaceOption == eFaceOption.PRESET_LIST)
				SetCurrentPresetItem();
		}
#endregion Initialize

		private void OnItemSlotSelected(CustomizeItemViewModel viewModel)
		{
			if (_itemSlots.Value != null)
				foreach (var itemViewModel in _itemSlots.Value)
					itemViewModel.IsSelected = itemViewModel == viewModel;

			var avatarCloset = AvatarMediator.Instance.AvatarCloset;
			if (!avatarCloset.HasAvatar)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "Avatar is not created");
				return;
			}

			if (viewModel.IsCreatedAI)
			{
				avatarCloset.UpdateAvatarModel(viewModel.AvatarInfo, () =>
				{
					var faceViewModel = ViewModelManager.Instance.Get<AvatarSelectionFaceViewModel>();

					if (faceViewModel != null)
						faceViewModel.LastSelectedPresetId = viewModel.ItemId;
					SetCurrentPresetItem();
				}).Forget();
				return;
			}

			var faceItem = AvatarTable.GetFaceItem(viewModel.ItemId);
			if (faceItem == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"Cannot found faceItem id: {viewModel.ItemId}");
				return;
			}

			if (faceItem.FaceOption == eFaceOption.PRESET_LIST)
			{
				var faceViewModel = ViewModelManager.Instance.Get<AvatarSelectionFaceViewModel>();

				if (faceViewModel != null)
					faceViewModel.LastSelectedPresetId = viewModel.ItemId;

				avatarCloset.ApplyFaceItem(viewModel.ItemId);
				avatarCloset.SetFacePreset(viewModel.ItemId);
				SetCurrentPresetItem();
			}
			else
			{
				avatarCloset.ApplyFaceItem(viewModel.ItemId);
			}
		}

		private void OnColorItemSelected(CustomizeColorItemViewModel viewModel)
		{
			if (_colorItems.Value != null)
				foreach (var colorItem in _colorItems.Value)
					colorItem.IsSelected = colorItem.ColorIndex == viewModel.ColorIndex;

			var avatarCloset = AvatarMediator.Instance.AvatarCloset;
			if (!avatarCloset.HasAvatar)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "Avatar is not created");
				return;
			}

			if (AvatarTable.FaceOptionFeatures.TryGetValue(_faceOption, out var faceOptionFeature))
			{
				switch (faceOptionFeature.ColorType)
				{
					case AvatarTable.eFaceOptionColorType.NONE:
						break;
					case AvatarTable.eFaceOptionColorType.RGB:
						avatarCloset.ApplyFaceColorItem(viewModel.ColorIndex, _faceOption);
						break;
					case AvatarTable.eFaceOptionColorType.TEXTURE:
						break;
					case AvatarTable.eFaceOptionColorType.SKIN:
						avatarCloset.ApplyFaceItem(viewModel.ItemId);
						avatarCloset.ApplyFaceColorItem(viewModel.ColorIndex, eFaceOption.FACE_SHAPE);
						break;
				}
			}
		}

		private void OnSetDropdownOpen(bool value)
		{
			if (!HasDropdown) return;

			IsDropdownOpen = value;
		}

		private void OnClickSetForceLayoutRebuildNextFrame()
		{
			var parentViewModel = ViewModelManager.Instance.Get<AvatarSelectionFaceViewModel>();
			if (parentViewModel != null)
			{
				parentViewModel.OnClickSetForceLayoutRebuildNextFrame();
			}
			else
			{
				C2VDebug.LogWarningCategory(GetType().Name, "Cannot found parentViewModel");
			}
		}
	}
}
