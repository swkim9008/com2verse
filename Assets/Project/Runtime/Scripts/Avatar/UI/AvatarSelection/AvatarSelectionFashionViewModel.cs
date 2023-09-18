/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarSelectionFashionViewModel.cs
* Developer:	eugene9721
* Date:			2023-03-17 17:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Avatar;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[ViewModelGroup("AvatarCustomize")]
	public sealed class AvatarSelectionFashionViewModel : AvatarSelectionViewModelBase
	{
		private const string TopMenuKey         = "UI_AvatarCreate_Cloth_Menu_Top";
		private const string BottomMenuKey      = "UI_AvatarCreate_Cloth_Menu_Bottom";
		private const string ShoeMenuKey        = "UI_AvatarCreate_Cloth_Menu_Shoe";
		private const string AccessariesMenuKey = "UI_AvatarCreate_Cloth_Menu_Accessaries";

#region Fields
		private Collection<CustomizeItemViewModel>    _itemSlots       = new();
		private Collection<CustomizeSubMenuViewModel> _fashionSubMenuList = new();

		private bool _hasSubMenu;

		private bool            _isInitializedSubMenu;
		private eFashionSubMenu _currentSubMenu;
#endregion Fields

#region Field Properties
		[UsedImplicitly]
		public Collection<CustomizeSubMenuViewModel> FashionSubMenuList
		{
			get => _fashionSubMenuList;
			set => SetProperty(ref _fashionSubMenuList, value);
		}

		[UsedImplicitly]
		public Collection<CustomizeItemViewModel> ItemSlots
		{
			get => _itemSlots;
			set => SetProperty(ref _itemSlots, value);
		}

		[UsedImplicitly]
		public bool HasSubMenu
		{
			get => _hasSubMenu;
			set => SetProperty(ref _hasSubMenu, value);
		}
#endregion Field Properties

		public override void Show()
		{
			if (Owner == null) return;

			Owner.SubTitleTextKey = AvatarSelectionManagerViewModel.FaceFashionSubTitleTextKey;
			Owner.RefreshButtons();
			Owner.AvatarCloset.Controller?.SetFullBodyVirtualCamera();

			Owner.ShowPreviewAvatarAsync(() =>
			{
				AddMenuItems();
				RefreshFashionItem();
			}).Forget();
		}

		public override void Hide()
		{
			ClearMenuItems();
			HasSubMenu            = false;
			_isInitializedSubMenu = false;
		}
		public override void Clear() { }

#region MenuItem
		private void AddMenuItems()
		{
			if (Owner == null) return;

			ClearMenuItems();
			foreach (eFashionMenu menu in Enum.GetValues(typeof(eFashionMenu)))
			{
				var menuViewModel = new CustomizeMenuViewModel
				{
					MenuTypeKey = menu.ToString() ?? string.Empty,
					MenuTextKey = GetStringKeyOfMenuItem(menu),
				};
				menuViewModel.OnSelectedEvent += OnMenuClicked;
				Owner.ItemMenuList.AddItem(menuViewModel);
			}

			if (Owner.ItemMenuList.Value is { Count: > 0 })
			{
				var firstViewModel = Owner.ItemMenuList.Value[0];
				if (firstViewModel != null)
					OnMenuClicked(firstViewModel);
			}
		}

		private string GetStringKeyOfMenuItem(eFashionMenu menu)
		{
			switch (menu)
			{
				case eFashionMenu.TOP:
					return TopMenuKey;
				case eFashionMenu.BOTTOM:
					return BottomMenuKey;
				case eFashionMenu.SHOE:
					return ShoeMenuKey;
				case eFashionMenu.ACCESSARIES:
					return AccessariesMenuKey;
			}

			return string.Empty;
		}

		private void ClearMenuItems()
		{
			if (Owner?.ItemMenuList.Value != null)
				foreach (var viewModel in Owner.ItemMenuList.Value)
					viewModel.OnSelectedEvent -= OnMenuClicked;

			Owner?.ItemMenuList.Reset();
		}

		private void OnMenuClicked(CustomizeMenuViewModel menu)
		{
			if (Owner == null) return;

			if (Owner.ItemMenuList.Value != null)
				foreach (var viewModel in Owner.ItemMenuList.Value)
					viewModel.IsSelected = viewModel == menu;

			var menuType = (eFashionMenu)Enum.Parse(typeof(eFashionMenu), menu.MenuTypeKey);
			if (!Enum.IsDefined(typeof(eFashionMenu), menuType))
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"value {menuType} is not defined in enum {typeof(eFashionMenu)}");
				return;
			}

			OnFashionMenuChanged(menuType);
		}

		private void OnFashionMenuChanged(eFashionMenu menu)
		{
			OnFashionMenuClick(menu);
		}

		private void OnFashionMenuClick(eFashionMenu menu)
		{
			HasSubMenu = CheckHasSubMenu(menu);
			if (HasSubMenu)
				SetSubMenuCollection(menu);

			OnFashionSubMenuClick(GetFirstSubMenu(menu));
		}

		private bool CheckHasSubMenu(eFashionMenu menu) => AvatarTable.CheckHasSubMenu(menu);

		private eFashionSubMenu GetFirstSubMenu(eFashionMenu menu) => AvatarTable.GetFirstSubMenu(menu);
#endregion MenuItem

#region SubMenu
		private void ClearSubMenuCollection()
		{
			if (_fashionSubMenuList.Value != null)
				foreach (var item in _fashionSubMenuList.Value)
					item.OnSelectedEvent -= OnFashionSubMenuItemSelected;

			_fashionSubMenuList.Reset();
		}

		private void SetSubMenuCollection(eFashionMenu menu)
		{
			if (!AvatarTable.FashionMenuDictionary.TryGetValue(menu, out var subMenuList) || subMenuList == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"value {menu} is not defined in enum {typeof(eFashionMenu)}");
				return;
			}

			ClearSubMenuCollection();
			foreach (eFashionSubMenu subMenu in subMenuList)
			{
				var subMenuViewModel = new CustomizeSubMenuViewModel
				{
					FashionSubMenu = subMenu,
				};

				subMenuViewModel.OnSelectedEvent += OnFashionSubMenuItemSelected;
				_fashionSubMenuList.AddItem(subMenuViewModel);
			}

			if (_fashionSubMenuList.Value is { Count: > 0 })
				OnFashionSubMenuItemSelected(_fashionSubMenuList.FirstItem()!);
		}

		private void OnFashionSubMenuItemSelected(CustomizeSubMenuViewModel viewModel)
		{
			OnFashionSubMenuClick(viewModel.FashionSubMenu);

			if (_fashionSubMenuList.Value != null)
				foreach (var subMenu in _fashionSubMenuList.Value)
					subMenu.IsSelected = subMenu == viewModel;

			// InvokePropertyValueChanged(nameof(SetForceLayoutRebuild), SetForceLayoutRebuild);
		}
#endregion SubMenu

#region FashionItem
		private void ClearItemSlots()
		{
			if (_itemSlots.Value != null)
				foreach (var itemSlot in _itemSlots.Value)
					itemSlot.OnSelectedEvent -= OnItemSlotSelected;

			_itemSlots.Reset();
		}

		private void OnFashionSubMenuClick(eFashionSubMenu subMenu)
		{
			if (_isInitializedSubMenu && _currentSubMenu == subMenu) 
				return;

			ClearItemSlots();
			var avatarCloset    = AvatarMediator.Instance.AvatarCloset;
			var fashionItemList = avatarCloset.GetFashionItemList();

			if (fashionItemList == null)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "fashionItemList is null");
				return;
			}

			var currentAvatar = avatarCloset.CurrentAvatar;
			if (currentAvatar.IsUnityNull() || currentAvatar!.Info == null)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "currentAvatar or info is null");
				return;
			}

			if (AvatarTable.FashionSubMenuFeatures.TryGetValue(subMenu, out var fashionSubMenuFeature) && fashionSubMenuFeature.HasEmptySlot)
				AddEmptySlot(subMenu);

			foreach (var id in fashionItemList)
			{
				var fashionItem = AvatarTable.GetFashionItem(id);
				if (fashionItem?.FashionSubMenu != subMenu)
					continue;

				if (fashionItem.AvatarType != currentAvatar.Info.AvatarType)
					continue;

				AddItemSlot(fashionItem);
			}

			SelectCurrentAvatarItem(avatarCloset.CurrentAvatarInfo);

			_isInitializedSubMenu = true;
			_currentSubMenu       = subMenu;
		}

		private void AddEmptySlot(eFashionSubMenu subMenu)
		{
			var itemViewModel = new CustomizeItemViewModel
			{
				ItemId            = -1,
				CustomizeItemType = AvatarTable.eCustomizeItemType.FASHION,
				FashionSubMenu    = subMenu,
				IsEmpty           = true,
			};
			itemViewModel.SetAdditionalInfo(false);
			itemViewModel.OnSelectedEvent += OnItemSlotSelected;
			_itemSlots.AddItem(itemViewModel);
		}

		private void AddItemSlot(AvatarFashionItem itemInfo)
		{
			var avatarCloset = AvatarMediator.Instance.AvatarCloset;

			var itemViewModel = new CustomizeItemViewModel
			{
				ItemId            = itemInfo.id,
				CustomizeItemType = AvatarTable.eCustomizeItemType.FASHION,
				FashionSubMenu    = itemInfo.FashionSubMenu,
			};
			itemViewModel.SetAdditionalInfo(avatarCloset.Controller?.IsUseAdditionalInfoAtItem ?? false);
			itemViewModel.OnSelectedEvent += OnItemSlotSelected;
			_itemSlots.AddItem(itemViewModel);
		}

		private void OnItemSlotSelected(CustomizeItemViewModel viewModel)
		{
			var avatarCloset = AvatarMediator.Instance.AvatarCloset;
			if (!avatarCloset.HasAvatar)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "Avatar is not created");
				return;
			}

			if (viewModel.IsEmpty)
			{
				avatarCloset.RemoveFashionItem(viewModel.FashionSubMenu);
				return;
			}

			var fashionItem = AvatarTable.GetFashionItem(viewModel.ItemId);
			if (fashionItem == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"Cannot found fashionItem id: {viewModel.ItemId}");
				return;
			}

			avatarCloset.SetFashionItem(viewModel.ItemId);
		}

		private void SelectCurrentAvatarItem(AvatarInfo? avatarItemInfo)
		{
			if (avatarItemInfo == null)
				return;

			var fashionItemList = avatarItemInfo.GetFashionItemList();
			if (_itemSlots.Value == null)
				return;

			foreach (var fashionItemViewModel in _itemSlots.Value)
			{
				fashionItemViewModel.IsSelected = false;

				// 착용한 아이템이 없어서 아래 루프를 돌지 않는 경우 -1(미착용)아이템 선택
				if (fashionItemViewModel.ItemId == -1)
					fashionItemViewModel.IsSelected = true;

				foreach (var fashionItem in fashionItemList)
				{
					var viewModelMenu   = fashionItemViewModel.FashionSubMenu;
					var fashionItemMenu = AvatarTable.IdToFashionSubMenu(fashionItem.ItemId);
					if (viewModelMenu != fashionItemMenu)
						continue;

					fashionItemViewModel.IsSelected = fashionItemViewModel.ItemId == fashionItem.ItemId;
				}
			}
		}
#endregion FashionItem

		public override void OnAvatarItemInfoChanged(AvatarInfo avatarItemInfo)
		{
			SelectCurrentAvatarItem(avatarItemInfo);
		}

		private void RefreshFashionItem()
		{
			var avatarCloset  = AvatarMediator.Instance.AvatarCloset;
			var currentAvatar = avatarCloset.CurrentAvatar;
			if (currentAvatar.IsUnityNull() || avatarCloset.CurrentAvatarInfo == null) return;

			var avatarManager = AvatarManager.Instance;
			if (avatarCloset.CurrentAvatarInfo.HasBaseFashionItem())
			{
				var defaultFashionInfo = avatarCloset.CurrentAvatarInfo;
				defaultFashionInfo.SetDefaultFashionItem();
				avatarManager.UpdateAvatarParts(currentAvatar!, defaultFashionInfo).Forget();
			}
		}
	}
}
