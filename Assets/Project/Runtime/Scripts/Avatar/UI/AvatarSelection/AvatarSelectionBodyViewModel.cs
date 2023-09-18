/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarSelectionBodyViewModel.cs
* Developer:	eugene9721
* Date:			2023-03-17 17:22
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Avatar;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[ViewModelGroup("AvatarCustomize")]
	public sealed class AvatarSelectionBodyViewModel : AvatarSelectionViewModelBase
	{
		private const string MenuKey  = "UI_AvatarCreate_Tab_BodyShape";
		private const string MenuType = "Body";

#region Fields
		private Collection<CustomizeItemViewModel> _itemSlots = new();
#endregion Fields

#region Command Properties
#endregion Command Properties

#region Field Properties
		[UsedImplicitly]
		public Collection<CustomizeItemViewModel> ItemSlots
		{
			get => _itemSlots;
			set => SetProperty(ref _itemSlots, value);
		}
#endregion Field Properties

#region Initialize
#endregion Initialize

#region AvatarSelectionViewModelBase
		public override void Show()
		{
			if (Owner == null)
				return;

			Owner.AvatarCloset.Controller?.SetFullBodyVirtualCamera();
			Owner.SubTitleTextKey = AvatarSelectionManagerViewModel.FaceBodyShapeSubTitleTextKey;
			Owner.RefreshButtons();

			Owner.ShowPreviewAvatarAsync(() =>
			{
				InitializeItemSlots();
				AddMenuItems();
				SetBaseFashionItem();
			}).Forget();
		}

		public override void Hide()
		{
			ClearMenuItems();
			ClearItemSlots();
			RefreshFashionItem();
		}

		private void SetBaseFashionItem()
		{
			var avatarCloset  = AvatarMediator.Instance.AvatarCloset;
			var currentAvatar = avatarCloset.CurrentAvatar;
			if (currentAvatar.IsUnityNull() || avatarCloset.CurrentAvatarInfo == null) return;

			var currentInfo = avatarCloset.CurrentAvatarInfo.Clone();
			var prevInfo    = currentInfo.Clone();
			currentInfo.SetBaseFashionItem();

			AvatarManager.Instance.UpdateAvatarParts(currentAvatar!, currentInfo).Forget();
			avatarCloset.SetAvatarInfo(prevInfo);
		}

		private void RefreshFashionItem()
		{
			var avatarCloset  = AvatarMediator.Instance.AvatarCloset;
			var currentAvatar = avatarCloset.CurrentAvatar;
			if (currentAvatar.IsUnityNull() || avatarCloset.CurrentAvatarInfo == null) return;

			if (currentAvatar!.Info?.HasBaseFashionItem() ?? true)
				AvatarManager.Instance.UpdateAvatarParts(currentAvatar, avatarCloset.CurrentAvatarInfo).Forget();
		}

		public override void Clear()
		{
		}
#endregion AvatarSelectionViewModelBase

#region MenuItem
		private void AddMenuItems()
		{
			if (Owner == null) return;

			ClearMenuItems();
			var menuViewModel = new CustomizeMenuViewModel
			{
				MenuTypeKey = MenuType,
				MenuTextKey = MenuKey,
				IsSelected  = true,
			};
			Owner.ItemMenuList.AddItem(menuViewModel);
		}

		private void ClearMenuItems()
		{
			Owner?.ItemMenuList.Reset();
		}
#endregion MenuItem

#region BodyItem
		private void ClearItemSlots()
		{
			if (_itemSlots.Value != null)
				foreach (var itemSlot in _itemSlots.Value)
					itemSlot.OnSelectedEvent -= OnItemSlotSelected;

			_itemSlots.Reset();
		}

		private void InitializeItemSlots()
		{
			ClearItemSlots();
			var avatarCloset      = AvatarMediator.Instance.AvatarCloset;
			var bodyShapeItemList = avatarCloset.GetBodyShapeItemList();

			if (bodyShapeItemList == null)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "bodyShapeItemList is null");
				return;
			}

			var currentAvatar = avatarCloset.CurrentAvatar;
			if (currentAvatar.IsUnityNull() || currentAvatar!.Info == null)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "currentAvatar or info is null");
				return;
			}

			foreach (var id in bodyShapeItemList)
			{
				var bodyShapeItem = AvatarTable.GetBodyShapeItem(id);

				if (bodyShapeItem == null)
					continue;

				if (bodyShapeItem.AvatarType != currentAvatar.Info.AvatarType)
					continue;

				AddItemSlot(bodyShapeItem);
			}
		}

		private void AddItemSlot(BodyShapeItem itemInfo)
		{
			var avatarCloset = AvatarMediator.Instance.AvatarCloset;

			var itemViewModel = new CustomizeItemViewModel
			{
				ItemId            = itemInfo.id,
				CustomizeItemType = AvatarTable.eCustomizeItemType.BODY,
			};
			itemViewModel.SetAdditionalInfo(avatarCloset.Controller?.IsUseAdditionalInfoAtItem ?? false);
			itemViewModel.OnSelectedEvent += OnItemSlotSelected;
			_itemSlots.AddItem(itemViewModel);

			SelectCurrentAvatarItem(avatarCloset.CurrentAvatarInfo);
		}

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

			var fashionItem = AvatarTable.GetBodyShapeItem(viewModel.ItemId);
			if (fashionItem == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"Cannot found bodyShapeItem id: {viewModel.ItemId}");
				return;
			}

			avatarCloset.SetBodyShapeItem(viewModel.ItemId);
		}


		private void SelectCurrentAvatarItem(AvatarInfo? avatarItemInfo)
		{
			if (avatarItemInfo == null)
				return;

			var bodyShapeValue = avatarItemInfo.BodyShape;
			if (_itemSlots.Value == null)
				return;

			foreach (var bodyShapeViewModel in _itemSlots.Value)
				bodyShapeViewModel.IsSelected = bodyShapeViewModel.ItemId == bodyShapeValue;
		}
#endregion BodyItem

#region Handlers
#endregion Handlers
	}
}
