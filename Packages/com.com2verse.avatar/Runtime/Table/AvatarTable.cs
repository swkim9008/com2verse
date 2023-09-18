/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarTable.cs
* Developer:	tlghks1009
* Date:			2023-02-01 15:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using Com2Verse.Data;
using Com2Verse.Logger;

namespace Com2Verse.Avatar
{
	public static class AvatarTable
	{
#region Tmp
		public enum eFaceOptionColorType
		{
			NONE,
			RGB,
			TEXTURE,
			SKIN
		}

		public enum eCustomizeItemType
		{
			FACE,
			BODY,
			FASHION,
		}

		public struct FaceOptionFeature
		{
			public eFaceOptionColorType ColorType    { get; }
			public bool                 HasEmptySlot { get; }

			public FaceOptionFeature(eFaceOptionColorType colorType, bool hasEmptySlot)
			{
				ColorType    = colorType;
				HasEmptySlot = hasEmptySlot;
			}
		}

		public struct FashionSubMenuFeature
		{
			public bool HasEmptySlot { get; }

			public FashionSubMenuFeature(bool hasEmptySlot)
			{
				HasEmptySlot = hasEmptySlot;
			}
		}

		public static readonly Dictionary<eFaceMenu, List<eFaceSubMenu>> FaceMenuDictionary = new()
		{
			[eFaceMenu.PRESET]  = new List<eFaceSubMenu>()
			{
				eFaceSubMenu.PRESET_MENU,
			},
			[eFaceMenu.FACE] = new List<eFaceSubMenu>()
			{
				eFaceSubMenu.FACE_DETAILS,
				eFaceSubMenu.EYE_BROW,
				eFaceSubMenu.EYE,
				eFaceSubMenu.NOSE,
				eFaceSubMenu.MOUTH,
			},
			[eFaceMenu.MAKE_UP] = new List<eFaceSubMenu>()
			{
				eFaceSubMenu.EYE_DECO,
				eFaceSubMenu.CHEEK,
				eFaceSubMenu.LIP,
				eFaceSubMenu.TATTOO,
			},
			[eFaceMenu.HAIR] = new List<eFaceSubMenu>()
			{
				eFaceSubMenu.HAIR_MENU,
			},
		};

		public static readonly Dictionary<eFaceSubMenu, List<eFaceOption>> FaceSubMenuDictionary = new()
		{
			[eFaceSubMenu.PRESET_MENU] = new List<eFaceOption>()
			{
				eFaceOption.PRESET_LIST,
			},
			[eFaceSubMenu.FACE_DETAILS] = new List<eFaceOption>()
			{
				eFaceOption.FACE_SHAPE,
				// eFaceOption.SKIN_TYPE,
			},
			[eFaceSubMenu.EYE] = new List<eFaceOption>()
			{
				eFaceOption.EYE_SHAPE,
				eFaceOption.PUPIL_TYPE,
			},
			[eFaceSubMenu.EYE_BROW] = new List<eFaceOption>()
			{
				eFaceOption.EYE_BROW_OPTION,
			},
			[eFaceSubMenu.NOSE] = new List<eFaceOption>()
			{
				eFaceOption.NOSE_SHAPE,
			},
			[eFaceSubMenu.MOUTH] = new List<eFaceOption>()
			{
				eFaceOption.MOUTH_SHAPE,
			},
			[eFaceSubMenu.EYE_DECO] = new List<eFaceOption>()
			{
				eFaceOption.EYE_MAKE_UP_TYPE,
				eFaceOption.EYE_LASH,
			},
			[eFaceSubMenu.CHEEK] = new List<eFaceOption>()
			{
				eFaceOption.CHEEK_TYPE,
			},
			[eFaceSubMenu.LIP] = new List<eFaceOption>()
			{
				eFaceOption.LIP_TYPE,
			},
			[eFaceSubMenu.TATTOO] = new List<eFaceOption>()
			{
				eFaceOption.TATTOO_OPTION,
			},
			[eFaceSubMenu.HAIR_MENU] = new List<eFaceOption>()
			{
				eFaceOption.HAIR_STYLE,
			},
		};

		public static readonly Dictionary<eFaceOption, FaceOptionFeature> FaceOptionFeatures = new()
		{
			[eFaceOption.PRESET_LIST]      = new FaceOptionFeature(eFaceOptionColorType.NONE, false),
			[eFaceOption.FACE_SHAPE]       = new FaceOptionFeature(eFaceOptionColorType.SKIN, false),
			[eFaceOption.SKIN_TYPE]        = new FaceOptionFeature(eFaceOptionColorType.SKIN, false),
			[eFaceOption.EYE_SHAPE]        = new FaceOptionFeature(eFaceOptionColorType.NONE, false),
			[eFaceOption.PUPIL_TYPE]       = new FaceOptionFeature(eFaceOptionColorType.NONE, false),
			[eFaceOption.EYE_BROW_OPTION]  = new FaceOptionFeature(eFaceOptionColorType.RGB,  false),
			[eFaceOption.NOSE_SHAPE]       = new FaceOptionFeature(eFaceOptionColorType.NONE, false),
			[eFaceOption.MOUTH_SHAPE]      = new FaceOptionFeature(eFaceOptionColorType.NONE, false),
			[eFaceOption.EYE_MAKE_UP_TYPE] = new FaceOptionFeature(eFaceOptionColorType.NONE, false),
			[eFaceOption.EYE_LASH]         = new FaceOptionFeature(eFaceOptionColorType.RGB,  false),
			[eFaceOption.CHEEK_TYPE]       = new FaceOptionFeature(eFaceOptionColorType.NONE, false),
			[eFaceOption.LIP_TYPE]         = new FaceOptionFeature(eFaceOptionColorType.NONE, false),
			[eFaceOption.TATTOO_OPTION]    = new FaceOptionFeature(eFaceOptionColorType.NONE, false),
			[eFaceOption.HAIR_STYLE]       = new FaceOptionFeature(eFaceOptionColorType.RGB,  false),
		};

		public static readonly Dictionary<eFashionMenu, List<eFashionSubMenu>> FashionMenuDictionary = new()
		{
			[eFashionMenu.TOP] = new List<eFashionSubMenu>()
			{
				eFashionSubMenu.TOP,
			},
			[eFashionMenu.BOTTOM] = new List<eFashionSubMenu>()
			{
				eFashionSubMenu.BOTTOM,
			},
			[eFashionMenu.SHOE] = new List<eFashionSubMenu>()
			{
				eFashionSubMenu.SHOE,
			},
			[eFashionMenu.ACCESSARIES] = new List<eFashionSubMenu>()
			{
				eFashionSubMenu.BAG,
				eFashionSubMenu.HAT,
				eFashionSubMenu.GLASSES,
			},
		};

		public static readonly Dictionary<eFashionSubMenu, FashionSubMenuFeature> FashionSubMenuFeatures = new()
		{
			[eFashionSubMenu.TOP]     = new FashionSubMenuFeature(false),
			[eFashionSubMenu.BOTTOM]  = new FashionSubMenuFeature(false),
			[eFashionSubMenu.SHOE]    = new FashionSubMenuFeature(false),
			[eFashionSubMenu.BAG]     = new FashionSubMenuFeature(true),
			[eFashionSubMenu.HAT]     = new FashionSubMenuFeature(true),
			[eFashionSubMenu.GLASSES] = new FashionSubMenuFeature(true),
		};

		public static bool CheckHasSubMenu(eFaceMenu menu)
		{
			if (!FaceMenuDictionary.ContainsKey(menu))
				return false;

			if (FaceMenuDictionary[menu] == null)
				return false;

			return FaceMenuDictionary[menu]!.Count > 1;
		}

		public static bool CheckHasSubMenu(eFashionMenu menu)
		{
			if (!FashionMenuDictionary.ContainsKey(menu))
				return false;

			if (FashionMenuDictionary[menu] == null)
				return false;

			return FashionMenuDictionary[menu]!.Count > 1;
		}

		public static eFaceSubMenu GetFirstSubMenu(eFaceMenu menu)
		{
			if (!FaceMenuDictionary.ContainsKey(menu))
				return eFaceSubMenu.PRESET_MENU;

			if (FaceMenuDictionary[menu] == null)
				return eFaceSubMenu.PRESET_MENU;

			return FaceMenuDictionary[menu]!.Count > 0 ? FaceMenuDictionary[menu]![0] : eFaceSubMenu.PRESET_MENU;
		}

		public static eFashionSubMenu GetFirstSubMenu(eFashionMenu menu)
		{
			if (!FashionMenuDictionary.ContainsKey(menu))
				return eFashionSubMenu.TOP;

			if (FashionMenuDictionary[menu] == null)
				return eFashionSubMenu.TOP;

			return FashionMenuDictionary[menu]!.Count > 0 ? FashionMenuDictionary[menu]![0] : eFashionSubMenu.TOP;
		}

		public static readonly List<string> EyeBrowColorHtmlStringList = new()
		{
			"#370200",
			"#686868",
			"#737E7A",
			"#B3A07F",
			"#1F1c1b",
			"#6A3513",
			"#67110D",
			"#9D4B3C",
			"#1E6B76",
			"#805578",
		};

		public static readonly List<string> EyeLashColorHtmlStringList = new()
		{
			"#3F0B0C",
			"#9C4D0A",
			"#B4B4B4",
			"#636363",
		};

		public static readonly List<string> HairColorHtmlStringList = new()
		{
			"#505050",
			"#9A9A9A",
			"#797E77",
			"#DBB271",
			"#725745",
			"#B08268",
			"#855350",
			"#C68275",
			"#83A2A8",
			"#A48BA4",
		};

		public static string GetItemSpriteAddressableName(int itemId, eCustomizeItemType type)
		{
			if (itemId == -1)
				return "THM_AvatarParts_None.prefab";

			string genderString = string.Empty;
			switch (type)
			{
				case eCustomizeItemType.FACE:
					var faceItem = GetFaceItem(itemId);
					if (faceItem == null)
						return string.Empty;

					genderString = FaceIdToAvatarType(itemId) == eAvatarType.PC01_M ? "M" : "W";
					switch (faceItem.FaceOption)
					{
						case eFaceOption.HAIR_STYLE:
							return $"THM_PC01_{genderString}_HAIR_{GetResId(itemId)}_000.prefab";
						case eFaceOption.PRESET_LIST:
							return $"THM_PC01_{genderString}_PRESET_{GetResId(itemId)}.prefab";
						case eFaceOption.FACE_SHAPE:
							return $"THM_PC01_{genderString}_FACETYPE_{GetResId(itemId)}.prefab";
						case eFaceOption.EYE_SHAPE:
							return $"THM_PC01_{genderString}_EYETYPE_{GetResId(itemId)}.prefab";
						case eFaceOption.PUPIL_TYPE:
							return $"THM_PC01_{genderString}_PUPIL_{GetResId(itemId)}.prefab";
						case eFaceOption.EYE_BROW_OPTION:
							return $"THM_PC01_{genderString}_EYEBROW_{GetResId(itemId)}.prefab";
						case eFaceOption.NOSE_SHAPE:
							return $"THM_PC01_{genderString}_NOSETYPE_{GetResId(itemId)}.prefab";
						case eFaceOption.MOUTH_SHAPE:
							return $"THM_PC01_{genderString}_MOUTHTYPE_{GetResId(itemId)}.prefab";
						case eFaceOption.EYE_MAKE_UP_TYPE:
							return $"THM_PC01_{genderString}_EYESHADE_{GetResId(itemId)}.prefab";
						case eFaceOption.EYE_LASH:
							return $"THM_PC01_{genderString}_EYELASH_{GetResId(itemId)}.prefab";
						case eFaceOption.CHEEK_TYPE:
							return $"THM_PC01_{genderString}_CHEEK_{GetResId(itemId)}.prefab";
						case eFaceOption.LIP_TYPE:
							return $"THM_PC01_{genderString}_LIPS_{itemId % 1000 / 2:000}_{itemId % 2:000}.prefab";
						case eFaceOption.TATTOO_OPTION:
							return $"THM_PC01_{genderString}_FACEDECAL_{GetResId(itemId)}.prefab";
					}
					return string.Empty;

				case eCustomizeItemType.BODY:
					genderString = BodyShapeIdToAvatarType(itemId) == eAvatarType.PC01_M ? "M" : "W";
					return $"THM_PC01_{genderString}_BODYSHAPE_{GetBodyShapeResId(itemId)}.prefab";

				case eCustomizeItemType.FASHION:
					var fashionItem = GetFashionItem(itemId);
					if (fashionItem == null)
						return string.Empty;

					genderString = fashionItem.AvatarType == eAvatarType.PC01_M ? "M" : "W";
					var fashionSubMenuString = fashionItem.FashionSubMenu == eFashionSubMenu.GLASSES ? "GLS" : fashionItem.FashionSubMenu.ToString();

					return $"THM_PC01_{genderString}_{fashionSubMenuString}_{fashionItem.ItemKey:D3}_{fashionItem.ColorKey:D3}.prefab";
			}

			return string.Empty; // TODO: 빈 리소스 로드
		}

		public static string GetPresetAddressableName(FaceItemInfo? faceItemInfo)
		{
			if (faceItemInfo == null)
				return string.Empty;

			var facePreset = GetFacePreset(faceItemInfo.ItemId);
			if (facePreset == null)
				return string.Empty;

			return facePreset.address ?? string.Empty;
		}

		public static string GetLipsDecoAddressableName(FaceItemInfo? faceItemInfo)
		{
			if (faceItemInfo == null)
				return string.Empty;

			var lipsDecoItem = GetFaceItem(faceItemInfo.ItemId);
			if (lipsDecoItem == null)
				return string.Empty;

			return lipsDecoItem.address ?? string.Empty;
		}

		public static string GetDecalAddressableName(FaceItemInfo? faceItemInfo)
		{
			if (faceItemInfo == null)
				return string.Empty;

			var avatarType    = FaceIdToAvatarType(faceItemInfo.ItemId);
			var resId         = faceItemInfo.ResId.ToString("D3");
			return $"{avatarType.ToString()}_FACEDECAL_{resId}.decal";
		}
#endregion Tmp

		public static TableFaceItem?                  TableFaceItem;
		public static TableFacePreset?                TableFacePreset;
		public static TableBodyShapeItem?             TableBodyShapeItem;
		public static TableAvatarFashionItem?         TableAvatarFashionItem;
		public static TableAvatarCreateFaceItem?      TableAvatarCreateFaceItem;
		public static TableExcludedCreateFaceItem?    TableExcludedCreateFaceItem;
		public static TableAvatarCreateFashionItem?   TableAvatarCreateFashionItem;
		public static TableExcludedCreateFashionItem? TableExcludedCreateFashionItem;
		public static TableDefault?                   TableDefault;

		private static bool _isTableLoaded;
		public static  bool  IsTableLoaded => _isTableLoaded;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			TableFaceItem                  = null;
			TableFacePreset                = null;
			TableBodyShapeItem             = null;
			TableAvatarFashionItem         = null;
			TableAvatarCreateFaceItem      = null;
			TableExcludedCreateFaceItem    = null;
			TableAvatarCreateFashionItem   = null;
			TableExcludedCreateFashionItem = null;
			TableDefault                   = null;

			_isTableLoaded = false;
		}

		public static void LoadTable()
		{
			if (_isTableLoaded) return;

			TableFaceItem                  = TableDataManager.Instance.Get<TableFaceItem>();
			TableFacePreset                = TableDataManager.Instance.Get<TableFacePreset>();
			TableBodyShapeItem             = TableDataManager.Instance.Get<TableBodyShapeItem>();
			TableAvatarFashionItem         = TableDataManager.Instance.Get<TableAvatarFashionItem>();
			TableAvatarCreateFaceItem      = TableDataManager.Instance.Get<TableAvatarCreateFaceItem>();
			TableExcludedCreateFaceItem    = TableDataManager.Instance.Get<TableExcludedCreateFaceItem>();
			TableAvatarCreateFashionItem   = TableDataManager.Instance.Get<TableAvatarCreateFashionItem>();
			TableExcludedCreateFashionItem = TableDataManager.Instance.Get<TableExcludedCreateFashionItem>();
			TableDefault                   = TableDataManager.Instance.Get<TableDefault>();

			_isTableLoaded = true;
		}

		public static FaceItem? GetFaceItem(int faceItemId)
		{
			if (!_isTableLoaded || TableFaceItem?.Datas == null)
			{
				C2VDebug.LogErrorCategory(nameof(AvatarTable), "AvatarTable was not loaded.");
				return null;
			}

			return !TableFaceItem.Datas.TryGetValue(faceItemId, out var faceItem) ? null : faceItem;
		}

		public static FacePreset? GetFacePreset(int facePresetId)
		{
			if (!_isTableLoaded || TableFacePreset?.Datas == null)
			{
				C2VDebug.LogErrorCategory(nameof(AvatarTable), "FacePreset was not loaded.");
				return null;
			}

			return TableFacePreset.Datas.TryGetValue(facePresetId, out var facePreset) ? facePreset : null;
		}

		public static BodyShapeItem? GetBodyShapeItem(int bodyShapeId)
		{
			if (!_isTableLoaded || TableBodyShapeItem?.Datas == null)
			{
				C2VDebug.LogErrorCategory(nameof(AvatarTable), "BodyShapeItem was not loaded.");
				return null;
			}

			return TableBodyShapeItem.Datas.TryGetValue(bodyShapeId, out var bodyShapeItem) ? bodyShapeItem : null;
		}

		public static AvatarFashionItem? GetFashionItem(int fashionItemId)
		{
			if (!_isTableLoaded || TableAvatarFashionItem?.Datas == null)
			{
				C2VDebug.LogErrorCategory(nameof(AvatarTable), "AvatarFashionItem was not loaded.");
				return null;
			}

			return TableAvatarFashionItem.Datas.TryGetValue(fashionItemId, out var costumeItemParts) ? costumeItemParts : null;
		}

		/// <summary>
		/// 아바타 생성씬에서 생성가능한 아이템인지 확인
		/// </summary>
		/// <param name="id">확인할 패션 아이템의 ID</param>
		/// <returns>아바타 생성씬에서 생성 가능한 아바타면 true, 아니면 false</returns>
		public static bool IsAvatarCreateFashionItem(int id)
		{
			if (!_isTableLoaded || TableAvatarCreateFashionItem?.Datas == null)
			{
				C2VDebug.LogErrorCategory(nameof(AvatarTable), "AvatarTable was not loaded.");
				return false;
			}

			foreach (var data in TableAvatarCreateFashionItem.Datas)
				if (data?.AvatarCreateFashionItemId == id)
					return true;

			return false;
		}

		public static AvatarInfo GetBaseAvatarInfo(eAvatarType avatarType)
		{
			var avatarInfo = new AvatarInfo
			{
				AvatarType = avatarType,
			};

			avatarInfo.UpdateFashionItem(new FashionItemInfo(GetFashionSubMenuId(avatarType, eFashionSubMenu.TOP, 0)));
			avatarInfo.UpdateFashionItem(new FashionItemInfo(GetFashionSubMenuId(avatarType, eFashionSubMenu.BOTTOM, 0)));
			avatarInfo.UpdateFashionItem(new FashionItemInfo(GetFashionSubMenuId(avatarType, eFashionSubMenu.SHOE, 0)));

			avatarInfo.UpdateBodyShapeItem(GetBodyShapeId(avatarType, MetaverseAvatarDefine.DefaultBodyShapeId));

			foreach (eFaceOption faceOption in Enum.GetValues(typeof(eFaceOption)))
				avatarInfo.UpdateFaceItem(new FaceItemInfo(GetFaceOptionId(avatarType, faceOption, 0)));

			return avatarInfo;
		}

		public static string GetResId(int id)
		{
			var resIdInt = id % 1000;
			return resIdInt.ToString("D3");
		}

		public static string GetBodyShapeResId(int id)
		{
			var resIdInt = id % 100;
			return resIdInt.ToString("D3");
		}

		public static int GetResIdToInt(int id) => id % 1000;

		public static int GetBodyShapeResIdToInt(int id) => id % 100;

		public static string GetResId(Data.AvatarFashionItem? fashionItem) => fashionItem == null ? string.Empty : GetResId(fashionItem.id);

		public static string GetResId(Data.FaceItem? faceItem) => faceItem == null ? string.Empty : GetResId(faceItem.id);

		public static eAvatarType FaceIdToAvatarType(int id) => ApplyAvatarTypeFilter((eAvatarType)(id / 1000000));

		public static eAvatarType FashionIdToAvatarType(int id) => ApplyAvatarTypeFilter((eAvatarType)(id / 1000000));

		public static eAvatarType BodyShapeIdToAvatarType(int id) => ApplyAvatarTypeFilter((eAvatarType)(id / 100));

		private static eAvatarType ApplyAvatarTypeFilter(eAvatarType avatarType) => Enum.IsDefined(avatarType.GetType(), avatarType) ? avatarType : eAvatarType.NONE;

		public static eFashionSubMenu IdToFashionSubMenu(int id)
		{
			var type = (eFashionSubMenu)((id / 1000) % 1000);
			return Enum.IsDefined(type.GetType(), type) ? type : eFashionSubMenu.TOP;
		}

		public static eFaceOption IdToFaceOption(int id)
		{
			var type = (eFaceOption)((id / 1000) % 1000);
			return Enum.IsDefined(type.GetType(), type) ? type : eFaceOption.EYE_LASH;
		}

		public static eFaceOption IdToFaceOption(Data.FaceItem faceItem) => IdToFaceOption(faceItem.id);

		public static int GetBodyShapeId(eAvatarType avatarType, int bodyShapeId) => (int)avatarType * 100 + bodyShapeId;

		public static int GetFaceOptionId(eAvatarType avatarType, eFaceOption faceOption, int faceId) => (int)avatarType * 1000000 + (int)faceOption * 1000 + faceId;

		public static int GetFashionSubMenuId(eAvatarType avatarType, eFashionSubMenu fashionType, int fashionId) => (int)avatarType * 1000000 + (int)fashionType * 1000 + fashionId;

		// TODO: 테이블 데이터 이용
		public static int GetDefaultFaceOptionId(eAvatarType avatarType, eFaceOption faceOption) => GetFaceOptionId(avatarType, faceOption, 0);

		public static int GetDefaultFashionSubMenuId(eAvatarType avatarType, eFashionSubMenu fashionType) => GetFashionSubMenuId(avatarType, fashionType, 1);

		public static int GetLipsTypeId(eAvatarType avatarType, int texSel, int maskTexSel)
		{
			var lipsTypeId = GetDefaultFaceOptionId(avatarType, eFaceOption.LIP_TYPE);
			if (TableFaceItem?.Datas == null)
				return lipsTypeId;

			foreach (var faceItem in TableFaceItem.Datas)
			{
				var value = faceItem.Value;
				if (value?.FaceOption != eFaceOption.LIP_TYPE)
					continue;

				if (value.AvatarType != avatarType)
					continue;

				if (value.ItemKey == texSel && value.ColorKey == maskTexSel)
					return value.id;
			}

			return lipsTypeId;
		}
	}
}
