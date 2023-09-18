/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarSelectionFaceViewModel_Edit_Collection.cs
* Developer:	eugene9721
* Date:			2023-05-02 17:39
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Avatar;
using Com2Verse.Data;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
	public partial class AvatarSelectionFaceViewModel
	{
		private bool         _isInitializedSubMenu;
		private eFaceSubMenu _currentSubMenu;

#region MenuItem
		private void AddMenuItems()
		{
			if (Owner == null) return;

			ClearMenuItems();
			foreach (eFaceMenu menu in Enum.GetValues(typeof(eFaceMenu)))
			{
				var menuViewModel = new CustomizeMenuViewModel
				{
					MenuTypeKey = menu.ToString() ?? string.Empty,
					MenuTextKey = GetStringKeyOfMenuItem(menu),
				};
				menuViewModel.OnSelectedEvent += OnMenuClicked;
				Owner.ItemMenuList.AddItem(menuViewModel);
			}
		}

		private string GetStringKeyOfMenuItem(eFaceMenu menu)
		{
			switch (menu)
			{
				case eFaceMenu.PRESET:
					return PresetMenuKey;
				case eFaceMenu.FACE:
					return FaceMenuKey;
				case eFaceMenu.MAKE_UP:
					return MakeUpMenuKey;
				case eFaceMenu.HAIR:
					return HairMenuKey;
			}

			return string.Empty;
		}

		private void ClearMenuItems()
		{
			if (Owner?.ItemMenuList.Value != null)
				foreach (var viewModel in Owner.ItemMenuList.Value)
					viewModel.OnSelectedEvent -= OnMenuClicked;

			if (Owner != null)
			{
				Owner.ItemMenuList.Reset();
				Owner.IsSelectedPreset = false;
			}
		}

		private void OnMenuClicked(CustomizeMenuViewModel menu)
		{
			if (Owner?.ItemMenuList.Value != null)
				foreach (var viewModel in Owner.ItemMenuList.Value)
					viewModel.IsSelected = viewModel == menu;

			var menuType = (eFaceMenu)Enum.Parse(typeof(eFaceMenu), menu.MenuTypeKey);
			if (!Enum.IsDefined(typeof(eFaceMenu), menuType))
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"value {menuType} is not defined in enum {typeof(eFaceMenu)}");
				return;
			}

			if (Owner != null)
				Owner.IsSelectedPreset = menuType == eFaceMenu.PRESET;

			OnFaceMenuChanged(menuType);
		}
#endregion MenuItem

#region SubMenu
		private void ClearSubMenuCollection()
		{
			if (_faceSubMenuList.Value != null)
				foreach (var item in _faceSubMenuList.Value)
					item.OnSelectedEvent -= OnFaceSubMenuItemSelected;

			_faceSubMenuList.Reset();
		}

		private void SetSubMenuCollection(eFaceMenu menu)
		{
			if (!AvatarTable.FaceMenuDictionary.TryGetValue(menu, out var subMenuList) || subMenuList == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"value {menu} is not defined in enum {typeof(eFaceMenu)}");
				return;
			}

			ClearSubMenuCollection();
			foreach (eFaceSubMenu subMenu in subMenuList)
			{
				var subMenuViewModel = new CustomizeSubMenuViewModel
				{
					FaceSubMenu = subMenu,
				};

				subMenuViewModel.OnSelectedEvent += OnFaceSubMenuItemSelected;
				_faceSubMenuList.AddItem(subMenuViewModel);
			}
			if (_faceSubMenuList.Value is { Count: > 0 })
				OnFaceSubMenuItemSelected(_faceSubMenuList.FirstItem()!);
		}

		private void OnFaceSubMenuItemSelected(CustomizeSubMenuViewModel viewModel)
		{
			SetFaceOptionCollection(viewModel.FaceSubMenu);

			if (_faceSubMenuList.Value != null)
				foreach (var subMenu in _faceSubMenuList.Value)
					subMenu.IsSelected = subMenu == viewModel;

			InvokePropertyValueChanged(nameof(SetForceLayoutRebuild), SetForceLayoutRebuild);
		}
#endregion SubMenu

#region FaceOptionItem
		private void ClearFaceOptionCollection()
		{
			_faceOptionList.Reset();
		}

		private void SetFaceOptionCollection(eFaceSubMenu subMenu)
		{
			if (_isInitializedSubMenu && _currentSubMenu == subMenu)
				return;

			ClearFaceOptionCollection();

			if (!AvatarTable.FaceSubMenuDictionary.TryGetValue(subMenu, out var faceOptions) || faceOptions == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"value {subMenu} is not defined in enum {typeof(eFaceSubMenu)}");
				return;
			}

			foreach (eFaceOption option in faceOptions)
			{
				// skin_type은 FACE_SHAPE와 함께 관리됩니다.
				if (option == eFaceOption.SKIN_TYPE)
					continue;

				var faceOptionViewModel = new CustomizeFaceOptionViewModel
				{
					FaceOption  = option,
					HasDropdown = faceOptions.Count != 1,
				};
				_faceOptionList.AddItem(faceOptionViewModel);
			}

			InvokePropertyValueChanged(nameof(SetForceLayoutRebuild), SetForceLayoutRebuild);
			SetForceLayoutRebuildAsync().Forget();

			if (subMenu == eFaceSubMenu.PRESET_MENU)
				SelectCurrentAvatarPreset();
			else
				SelectCurrentAvatarItem(AvatarMediator.Instance.AvatarCloset.CurrentAvatarInfo);

			_isInitializedSubMenu = true;
			_currentSubMenu       = subMenu;
		}

		private async UniTask SetForceLayoutRebuildAsync()
		{
			await UniTask.Yield(PlayerLoopTiming.LastUpdate);
			InvokePropertyValueChanged(nameof(SetForceLayoutRebuild), SetForceLayoutRebuild);
		}

		private void SelectCurrentAvatarPreset()
		{
			if (_faceOptionList.Value == null)
				return;

			foreach (var faceOptionViewModel in _faceOptionList.Value)
			{
				if (faceOptionViewModel.FaceOption != eFaceOption.PRESET_LIST)
					continue;

				faceOptionViewModel.SetCurrentPresetItem();
			}
		}

		private void SelectCurrentAvatarItem(AvatarInfo? avatarItemInfo)
		{
			if (avatarItemInfo == null)
				return;

			var faceOptionList = avatarItemInfo.GetFaceOptionList();
			if (_faceOptionList.Value == null)
				return;

			foreach (var faceOptionViewModel in _faceOptionList.Value)
			{
				foreach (var faceItem in faceOptionList)
				{
					if (faceOptionViewModel.FaceOption != AvatarTable.IdToFaceOption(faceItem.ItemId))
						continue;

					faceOptionViewModel.SelectCurrentAvatarItem(faceItem);
				}
			}
		}
#endregion FaceOptionItem

		public override void OnAvatarItemInfoChanged(AvatarInfo avatarItemInfo)
		{
			SelectCurrentAvatarItem(avatarItemInfo);
		}
	}
}
