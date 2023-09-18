/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarItemInfo.cs
* Developer:	tlghks1009
* Date:			2022-11-01 10:33
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Data;

namespace Com2Verse.Avatar
{
	public abstract class AvatarItemInfo
	{
		private int _resId;

		public int ItemId { get; set; }

		// ReSharper disable once ConvertToAutoProperty
		public int ResId
		{
			get => _resId;
			set => _resId = value;
		}
	}

	public class FaceItemInfo : AvatarItemInfo, IEquatable<FaceItemInfo>
	{
		private eFaceOption _faceOption;

		private int _colorId;

		// ReSharper disable once ConvertToAutoProperty
		public eFaceOption FaceOption
		{
			get => _faceOption;
			private set => _faceOption = value;
		}

		// ReSharper disable once ConvertToAutoProperty
		public int ColorId
		{
			get => _colorId;
			set => _colorId = value;
		}

#region IEquatable
		public bool Equals(FaceItemInfo itemInfo) =>
			itemInfo.ItemId == ItemId &&
			itemInfo.ColorId == ColorId;

		public override bool Equals(object obj) => obj is FaceItemInfo other && Equals(other);

		public override int GetHashCode() => HashCode.Combine(ResId, FaceOption, ColorId);
#endregion IEquatable

		public FaceItemInfo Clone()
		{
			var faceItemInfo = new FaceItemInfo();
			faceItemInfo.Copy(this);
			return faceItemInfo;
		}

		public void Copy(FaceItemInfo itemInfo)
		{
			ItemId     = itemInfo.ItemId;
			ResId      = itemInfo.ResId;
			FaceOption = itemInfo.FaceOption;
			ColorId    = itemInfo.ColorId;
		}

		private FaceItemInfo() { }

		public FaceItemInfo(int id)
		{
			ItemId     = id;
			ResId      = AvatarTable.GetResIdToInt(id);
			FaceOption = AvatarTable.IdToFaceOption(id);
		}

		public FaceItemInfo(int id, int color) : this(id)
		{
			ColorId = color;
		}

		public FaceItemInfo(Protocols.FaceItem faceItem)
		{
			var hasColor = int.TryParse(faceItem.FaceColor!, out var colorId);

			ItemId     = faceItem.FaceID;
			ResId      = AvatarTable.GetResIdToInt(faceItem.FaceID);
			FaceOption = (eFaceOption)faceItem.FaceKey;
			ColorId    = hasColor ? colorId : 0;
		}

		public FaceItemInfo(FaceItem faceItem)
		{
			ItemId     = faceItem.id;
			ResId      = AvatarTable.GetResIdToInt(faceItem.id);
			FaceOption = faceItem.FaceOption;
			ColorId    = faceItem.ColorKey;
		}

		public Protocols.FaceItem GetProtobufType() => new() { FaceID = ItemId, FaceKey = (int)FaceOption, FaceColor = ColorId.ToString() };

		public bool EqualsWithoutColor(FaceItemInfo itemInfo) => ItemId == itemInfo.ItemId;

		public static FaceItemInfo GetDefaultItemInfo(eAvatarType avatarType, eFaceOption faceOption) => new(AvatarTable.GetDefaultFaceOptionId(avatarType, faceOption));
	}

	public class FashionItemInfo : AvatarItemInfo, IEquatable<FashionItemInfo>
	{
		private eFashionSubMenu _fashionSubMenu;

		// ReSharper disable once ConvertToAutoProperty
		public eFashionSubMenu FashionSubMenu
		{
			get => _fashionSubMenu;
			private set => _fashionSubMenu = value;
		}

#region IEquatable
		public bool Equals(FashionItemInfo itemInfo) =>
			itemInfo.ItemId == ItemId;

		public override bool Equals(object obj) => obj is FashionItemInfo other && Equals(other);

		public override int GetHashCode() => HashCode.Combine(ResId, FashionSubMenu);
#endregion IEquatable

		public FashionItemInfo Clone()
		{
			var faceItemInfo = new FashionItemInfo();
			faceItemInfo.Copy(this);
			return faceItemInfo;
		}

		public void Copy(FashionItemInfo itemInfo)
		{
			ItemId         = itemInfo.ItemId;
			ResId          = itemInfo.ResId;
			FashionSubMenu = itemInfo.FashionSubMenu;
		}

		private FashionItemInfo() { }

		public FashionItemInfo(int id)
		{
			ItemId         = id;
			ResId          = AvatarTable.GetResIdToInt(id);
			FashionSubMenu = AvatarTable.IdToFashionSubMenu(id);
		}

		public FashionItemInfo(Protocols.FashionItem fashionItem)
		{
			ItemId         = fashionItem.FashionID;
			ResId          = AvatarTable.GetResIdToInt(ItemId);
			FashionSubMenu = AvatarTable.IdToFashionSubMenu(ItemId);
		}

		public FashionItemInfo(AvatarFashionItem fashionItem)
		{
			ItemId         = fashionItem.id;
			ResId          = AvatarTable.GetResIdToInt(ItemId);
			FashionSubMenu = AvatarTable.IdToFashionSubMenu(ItemId);
		}

		public Protocols.FashionItem GetProtobufType() => new() { FashionID = ItemId, FashionKey = (int)FashionSubMenu };

		public static FashionItemInfo GetDefaultItemInfo(eAvatarType avatarType, eFashionSubMenu fashionSubMenu) => new FashionItemInfo(AvatarTable.GetDefaultFashionSubMenuId(avatarType, fashionSubMenu));
	}
}
