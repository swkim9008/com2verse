/*===============================================================
* Product:		Com2Verse
* File Name:	CustomizeItemViewModel.cs
* Developer:	eugene9721
* Date:			2023-03-17 19:34
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using UnityEngine;
using Com2Verse.Avatar;
using Com2Verse.Data;
using JetBrains.Annotations;
using String = System.String;

namespace Com2Verse.UI
{
	[ViewModelGroup("AvatarCustomize")]
	public sealed class CustomizeItemViewModel : ViewModelBase
	{
#region Fields
		private Action<CustomizeItemViewModel>? _onSelectedEvent;

		private AvatarItemInfo? _avatarItemInfo;
		private Transform?      _itemHolder;

		private int _itemId;

		private bool _isEmpty;
		private bool _isSelected;

#region AdditonalInfos
		private bool _isWearing;
		private bool _isNew;
		private bool _isDisabled;
		private bool _isPeriod;
		private bool _isNFT;
		private bool _isCreatedAI;
#endregion AdditonalInfos

		private bool _isUseAdditionalInfo;

		private Texture? _aiCreatedPresetTexture;
#endregion Fields

#region Field Properties
		public event Action<CustomizeItemViewModel> OnSelectedEvent
		{
			add
			{
				_onSelectedEvent -= value;
				_onSelectedEvent += value;
			}
			remove => _onSelectedEvent -= value;
		}

		[UsedImplicitly]
		public bool IsEmpty
		{
			get => _isEmpty;
			set => SetProperty(ref _isEmpty, value);
		}

		[UsedImplicitly]
		public int ItemId
		{
			get => _itemId;
			set => SetProperty(ref _itemId, value);
		}

		[UsedImplicitly]
		public bool IsSelected
		{
			get => _isSelected;
			set => SetProperty(ref _isSelected, value);
		}

		[UsedImplicitly]
		public bool IsWearing
		{
			get => _isWearing;
			set => SetProperty(ref _isWearing, value && _isUseAdditionalInfo);
		}

		[UsedImplicitly]
		public bool IsNew
		{
			get => _isNew;
			set => SetProperty(ref _isNew, value && _isUseAdditionalInfo);
		}

		[UsedImplicitly]
		public bool IsDisabled
		{
			get => _isDisabled;
			set => SetProperty(ref _isDisabled, value && _isUseAdditionalInfo);
		}

		[UsedImplicitly]
		public bool IsPeriod
		{
			get => _isPeriod;
			set => SetProperty(ref _isPeriod, value && _isUseAdditionalInfo);
		}

		[UsedImplicitly]
		public bool IsNFT
		{
			get => _isNFT;
			set => SetProperty(ref _isNFT, value && _isUseAdditionalInfo);
		}

		[UsedImplicitly]
		public bool IsCreatedAI
		{
			get => _isCreatedAI;
			set => SetProperty(ref _isCreatedAI, value);
		}

		[UsedImplicitly]
		public Transform? ItemHolder
		{
			get => _itemHolder;
			set => SetProperty(ref _itemHolder, value);
		}

		[UsedImplicitly]
		public Texture? AICreatedPresetTexture
		{
			get => _aiCreatedPresetTexture;
			set => SetProperty(ref _aiCreatedPresetTexture, value);
		}

		public AvatarTable.eCustomizeItemType CustomizeItemType { get; set; }

		public eFashionSubMenu FashionSubMenu { get; set; }
		public eFaceOption     FaceOption     { get; set; }

		public AvatarInfo? AvatarInfo { get; set; }
#endregion Field Properties

		[UsedImplicitly] public CommandHandler CustomizeItemClickEvent { get; }
		[UsedImplicitly] public CommandHandler CustomizeItemHoverEvent { get; }

		public CustomizeItemViewModel()
		{
			CustomizeItemClickEvent = new CommandHandler(OnClickEvent);
			CustomizeItemHoverEvent = new CommandHandler(OnHoverEvent);
		}

		public void SetAdditionalInfo(bool value)
		{
			_isUseAdditionalInfo = value;
		}

		private void OnClickEvent()
		{
			IsSelected = true;
			_onSelectedEvent?.Invoke(this);
		}

		private void OnHoverEvent()
		{
			var infoViewModel = ViewModelManager.Instance.Get<CustomizeItemInfoViewModel>();
			if (infoViewModel == null) return;

			switch (CustomizeItemType)
			{
				case AvatarTable.eCustomizeItemType.FACE:
					infoViewModel.CustomizeItemName = string.Empty;
					return;
				case AvatarTable.eCustomizeItemType.BODY:
					OnHoverBodyItem(infoViewModel);
					return;
				case AvatarTable.eCustomizeItemType.FASHION:
					OnHoverFashionItem(infoViewModel);
					return;
			}
		}

		private void OnHoverBodyItem(CustomizeItemInfoViewModel infoViewModel)
		{
			var bodyShapeItem = AvatarTable.GetBodyShapeItem(ItemId);
			if (bodyShapeItem == null)
			{
				infoViewModel.CustomizeItemName = string.Empty;
				return;
			}

			var avatarTypeString = bodyShapeItem.AvatarType.ToString();
			var resId            = AvatarTable.GetBodyShapeResId(bodyShapeItem.id);

			var avatarItemStringKey = $"UI_AvatarItem_Name_{avatarTypeString}_BodyShape_{resId}".ToUpper();
			infoViewModel.CustomizeItemName = Localization.Instance.GetAvatarItemString(avatarItemStringKey);
		}

		private void OnHoverFashionItem(CustomizeItemInfoViewModel infoViewModel)
		{
			var fashionItem = AvatarTable.GetFashionItem(ItemId);
			if (fashionItem == null)
			{
				infoViewModel.CustomizeItemName = string.Empty;
				return;
			}

			var avatarTypeString     = fashionItem.AvatarType.ToString();
			var fashionSubMenuString = FashionSubMenu == eFashionSubMenu.GLASSES ? "GLS" : FashionSubMenu.ToString();

			var itemKeyString  = fashionItem.ItemKey.ToString("D3");
			var colorKeyString = fashionItem.ColorKey.ToString("D3");

			var avatarItemStringKey = $"UI_AvatarItem_Name_{avatarTypeString}_{fashionSubMenuString}_{itemKeyString}_{colorKeyString}".ToUpper();

			infoViewModel.CustomizeItemName = Localization.Instance.GetAvatarItemString(avatarItemStringKey);
		}

		public void RefreshViewModel()
		{
			InvokePropertyValueChanged(nameof(ItemId),      ItemId);
			InvokePropertyValueChanged(nameof(IsSelected),  IsSelected);
			InvokePropertyValueChanged(nameof(IsWearing),   IsWearing);
			InvokePropertyValueChanged(nameof(IsNew),       IsNew);
			InvokePropertyValueChanged(nameof(IsDisabled),  IsDisabled);
			InvokePropertyValueChanged(nameof(IsPeriod),    IsPeriod);
			InvokePropertyValueChanged(nameof(IsNFT),       IsNFT);
			InvokePropertyValueChanged(nameof(IsCreatedAI), IsCreatedAI);
		}
	}
}
