/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarInfo.cs
* Developer:	tlghks1009
* Date:			2022-08-03 13:27
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Google.Protobuf.Collections;
using FaceItem = Protocols.FaceItem;
using FashionItem = Protocols.FashionItem;

namespace Com2Verse.Avatar
{
	/// <summary>
	/// 아바타 한 셋트에 대한 정보 클래스
	/// </summary>
	public class AvatarInfo : IEquatable<AvatarInfo>
	{
#region Properties
		/// <summary>
		/// 월드에서 아바타 생성 요청이 왔을 때, SerialId를 콜백해줘야 해서 저장
		/// </summary>
		public long SerialId
		{
			get => _serialId;
			set => _serialId = value;
		}

		/// <summary>
		/// 아바타 생성시 AvatarID를 발급 / 아바타 Update에서 사용
		/// </summary>
		// ReSharper disable once ConvertToAutoProperty
		public long AvatarId
		{
			get => _avatarId;
			set => _avatarId = value;
		}

		public eAvatarType AvatarType
		{
			get => _avatarType;
			set => _avatarType = value;
		}

		public string AvatarTypePath => AvatarType switch
		{
			eAvatarType.NONE   => throw new ArgumentOutOfRangeException(),
			eAvatarType.PC01_W => "PC01_W",
			eAvatarType.PC01_M => "PC01_M",
			eAvatarType.COMMON => throw new ArgumentOutOfRangeException(),
			_                  => throw new ArgumentOutOfRangeException()
		};

		public int BodyShape
		{
			get => _bodyShape;
			private set => _bodyShape = value;
		}
#endregion Properties

#region Fields
		private readonly Dictionary<eFaceOption, FaceItemInfo>        _faceOptionDict  = new();
		private readonly Dictionary<eFashionSubMenu, FashionItemInfo> _fashionItemDict = new();

		private long        _serialId;
		private long        _avatarId;
		private int         _bodyShape = -1;
		private eAvatarType _avatarType;
#endregion Fields

		public AvatarInfo() { }

		public AvatarInfo(eAvatarType avatarType, FaceItemInfo[] faceOptionItems, int bodyShape, int[] fashionItemInfoList)
		{
			Set(avatarType, faceOptionItems, bodyShape, fashionItemInfoList);
		}

		/// <summary>
		/// 아바타 생성 요청 후 Response에서 서버에서 받은 정보를 덮어씌울 때 사용되는 생성자
		/// </summary>
		public AvatarInfo(Protocols.Avatar avatar)
		{
			Set(0, avatar);
		}

		/// <summary>
		/// 월드에서 아바타 생성시 사용되는 생성자
		/// </summary>
		public AvatarInfo(long serialId, Protocols.Avatar avatar)
		{
			Set(serialId, avatar);
		}

		public void Set(eAvatarType avatarType, FaceItemInfo[] faceOptionItems, int bodyShape, int[] fashionItemInfoList)
		{
			ClearAllItems();

			SerialId   = 0;
			AvatarId   = 0;
			AvatarType = avatarType;
			//C2VDebug.Log($"AvatarType : {AvatarType}");

			InitializeFaceItem(faceOptionItems);
			BodyShape = bodyShape;
			InitializeFashionItem(fashionItemInfoList);
		}

		public void Set(long serialId, Protocols.Avatar avatar)
		{
			ClearAllItems();

			SerialId = serialId;
			AvatarId   = avatar.AvatarID;
			AvatarType = (eAvatarType)avatar.AvatarType;
			//C2VDebug.Log($"AvatarId : {AvatarId} , AvatarType : {AvatarType}");

			if (null != avatar.FaceItemList)
				InitializeFaceItem(avatar.FaceItemList.ToList());
			BodyShape = avatar.BodyShape;
			if (null != avatar.FashionItemList)
				InitializeFashionItem(avatar.FashionItemList.ToList());
		}

		public AvatarInfo Clone()
		{
			var avatarInfo = new AvatarInfo
			{
				SerialId   = SerialId,
				AvatarId   = AvatarId,
				AvatarType = AvatarType,
				BodyShape  = BodyShape,
			};

			foreach (var faceItem in _faceOptionDict)
				avatarInfo._faceOptionDict.Add(faceItem.Key, faceItem.Value.Clone());

			foreach (var fashionItem in _fashionItemDict)
				avatarInfo._fashionItemDict.Add(fashionItem.Key, fashionItem.Value.Clone());

			return avatarInfo;
		}

		public void DeepCopy(AvatarInfo other)
		{
			ClearAllItems();

			SerialId = other.SerialId;
			AvatarId = other.AvatarId;
			AvatarType = other.AvatarType;
			BodyShape = other.BodyShape;

			foreach (var faceItem in other._faceOptionDict)
				_faceOptionDict.Add(faceItem.Key, faceItem.Value.Clone());

			foreach (var fashionItem in other._fashionItemDict)
				_fashionItemDict.Add(fashionItem.Key, fashionItem.Value.Clone());
		}

#region IEquatable
		public bool Equals(AvatarInfo? other) => other is not null                                       &&
		                                         SerialId   == other.SerialId                            &&
		                                         AvatarId   == other.AvatarId                            &&
		                                         AvatarType == other.AvatarType                          &&
		                                         BodyShape  == other.BodyShape                           &&
		                                         _faceOptionDict.IsKeyValueEquals(other._faceOptionDict) &&
		                                         _fashionItemDict.IsKeyValueEquals(other._fashionItemDict);

		public override bool Equals(object obj) => obj is AvatarInfo other && Equals(other);

		public override int GetHashCode()
		{
			var hashCode = new HashCode();
			hashCode.Add(SerialId);
			hashCode.Add(AvatarId);
			hashCode.Add((int)AvatarType);
			hashCode.Add(BodyShape);
			hashCode.Add(_faceOptionDict);
			hashCode.Add(_fashionItemDict);
			return hashCode.ToHashCode();
		}

		public static bool operator ==(AvatarInfo? lhs, AvatarInfo? rhs) => (lhs is null && rhs is null) || lhs?.Equals(rhs) == true;

		public static bool operator !=(AvatarInfo? lhs, AvatarInfo? rhs) => !(lhs == rhs);
#endregion IEquatable

#region GetItem
		public List<FaceItemInfo> GetFaceOptionList() => _faceOptionDict.Values.ToList();

		public FaceItemInfo? GetFaceOption(eFaceOption faceOption) => _faceOptionDict.ContainsKey(faceOption) ? _faceOptionDict[faceOption] : null;

		public List<FashionItemInfo> GetFashionItemList() => _fashionItemDict.Values.ToList();

		public FashionItemInfo? GetFashionItem(eFashionSubMenu fashionMenu) => _fashionItemDict.ContainsKey(fashionMenu) ? _fashionItemDict[fashionMenu] : null;
#endregion GetItem

#region UpdateItem
		public void UpdateBodyShapeItem(int itemId)
		{
			var fashionItem = AvatarTable.GetBodyShapeItem(itemId);
			if (fashionItem == null)
			{
				BodyShape = AvatarTable.GetBodyShapeId(_avatarType, MetaverseAvatarDefine.DefaultBodyShapeId);
				return;
			}

			BodyShape = itemId;
		}

		public void UpdateFashionItem(FashionItemInfo itemInfo)
		{
			if (_fashionItemDict.TryGetValue(itemInfo.FashionSubMenu, out var fashionItem) && fashionItem != null)
				fashionItem.Copy(itemInfo);
			else
				_fashionItemDict.Add(itemInfo.FashionSubMenu, itemInfo.Clone());
		}

		public void UpdateFaceItem(FaceItemInfo itemInfo)
		{
			// TODO: 프리셋 아이템의 경우 스킵해도 될지 확인
			if (_faceOptionDict.TryGetValue(itemInfo.FaceOption, out var faceOption) && faceOption != null)
				faceOption.Copy(itemInfo);
			else
				_faceOptionDict.Add(itemInfo.FaceOption, itemInfo.Clone());
		}
#endregion UpdateItem

#region RemoveItem
		public void ClearFashionItem()
		{
			_fashionItemDict.Clear();
		}

		public void ClearFaceItem()
		{
			_faceOptionDict.Clear();
		}

		private void ClearAllItems()
		{
			BodyShape = -1;
			_fashionItemDict.Clear();
			_faceOptionDict.Clear();
		}

		public void Clear()
		{
			ClearAllItems();

			SerialId = 0;
			AvatarId = 0;
		}

		public void RemoveFashionItem(eFashionSubMenu fashionSubMenu)
		{
			if (_fashionItemDict.ContainsKey(fashionSubMenu))
				_fashionItemDict.Remove(fashionSubMenu);
		}


		public void RemoveFaceItem(eFaceOption faceOption)
		{
			if (_faceOptionDict.ContainsKey(faceOption))
				_faceOptionDict.Remove(faceOption);
		}
#endregion RemoveItem

		public bool IsWearingFashionItem(int id)
		{
			foreach (var fashionItem in _fashionItemDict)
				if (fashionItem.Value.ItemId == id)
					return true;

			return false;
		}

		public bool IsSetFaceItemWithoutColor(int id)
		{
			foreach (var faceItem in _faceOptionDict)
				if (faceItem.Value.ItemId == id)
					return true;

			return false;
		}

#region InitializeItemDict
		private void InitializeFaceItem(FaceItemInfo[] itemInfos)
		{
			foreach (var itemInfo in itemInfos)
				_faceOptionDict.Add(itemInfo.FaceOption, itemInfo);
		}

		private void InitializeFaceItem(List<FaceItem> faceItemList)
		{
			foreach (var faceItem in faceItemList)
			{
				var tableData = AvatarTable.GetFaceItem(faceItem.FaceID);
				if (tableData == null)
				{
					C2VDebug.LogErrorCategory(GetType().Name, $"Can't find Table. BodyID : {faceItem.FaceID}");
					continue;
				}

				_faceOptionDict.Add(tableData.FaceOption, new FaceItemInfo(faceItem));
			}
		}

		public void InitializeFaceItem(List<FaceItemInfo> faceItemList)
		{
			_faceOptionDict.Clear();
			foreach (var faceItemInfo in faceItemList)
				_faceOptionDict.Add(faceItemInfo.FaceOption, faceItemInfo);
		}

		/// <summary>
		/// 패션아이템 초기화
		/// </summary>
		private void InitializeFashionItem(int[] itemIds)
		{
			foreach (var itemId in itemIds)
			{
				var tableData = AvatarTable.GetFashionItem(itemId);

				if (tableData == null)
				{
					C2VDebug.LogErrorCategory(GetType().Name, $"Can't find Table. FashionID : {itemId}");
					continue;
				}

				var fashionItem = new FashionItem()
				{
					FashionKey = (int)tableData.FashionSubMenu,
					FashionID  = tableData.id,
				};

				_fashionItemDict.Add(tableData.FashionSubMenu, new FashionItemInfo(fashionItem));
			}
		}

		/// <summary>
		/// 프로토버프 Dto에 의한 패션아이템 초기화
		/// </summary>
		public void InitializeFashionItem(List<FashionItem> fashionItemList)
		{
			_fashionItemDict.Clear();
			foreach (var fashionItem in fashionItemList)
			{
				// 미착용 아이템에 대해서는 DB에 0으로 저장되어 있음
				if (fashionItem.FashionID == 0)
					continue;

				var tableData = AvatarTable.GetFashionItem(fashionItem.FashionID);

				if (tableData == null)
				{
					C2VDebug.LogErrorCategory(GetType().Name, $"Can't find Table. FashionID : {fashionItem.FashionID}");
					continue;
				}

				_fashionItemDict.Add(tableData.FashionSubMenu, new FashionItemInfo(fashionItem));
			}
		}

		public void InitializeFashionItem(List<FashionItemInfo> fashionItemList)
		{
			_fashionItemDict.Clear();
			foreach (var fashionItem in fashionItemList)
				_fashionItemDict.Add(fashionItem.FashionSubMenu, fashionItem);
		}
#endregion InitializeItemDict

#region Utils
		public bool HasBaseFashionItem()
		{
			foreach (var fashionItem in _fashionItemDict.Values)
			{
				var currentSubMenu = fashionItem.FashionSubMenu;
				if (AvatarTable.FashionSubMenuFeatures[currentSubMenu].HasEmptySlot)
					continue;

				var resId = fashionItem.ResId;
				if (resId == 0) return true;
			}

			return false;
		}

		/// <summary>
		/// 아바타의 패션아이템 정보를 초기화한 후, 속옷으로 셋팅합니다
		/// 임시 렌더링용 아바타인포에만 사용해주세요
		/// </summary>
		public void SetBaseFashionItem()
		{
			_fashionItemDict.Clear();

			var isMan = AvatarType == eAvatarType.PC01_M;

			_fashionItemDict.Add(eFashionSubMenu.TOP, new FashionItemInfo(isMan ? 16011000 : 15011000));
			_fashionItemDict.Add(eFashionSubMenu.BOTTOM, new FashionItemInfo(isMan ? 16021000 : 15021000));
			_fashionItemDict.Add(eFashionSubMenu.SHOE, new FashionItemInfo(isMan ? 16031000 : 15031000));
		}

		public void SetDefaultFashionItem()
		{
			var avatarInfos = AvatarTable.TableDefault?.Datas;
			if (avatarInfos == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "AvatarTable was not loaded.");
				return;
			}

			foreach (var avatarInfo in avatarInfos)
			{
				if (avatarInfo.AvatarType != AvatarType)
					continue;

				_fashionItemDict.Clear();

				_fashionItemDict.Add(eFashionSubMenu.TOP,    new FashionItemInfo(avatarInfo.FashionTopId));
				_fashionItemDict.Add(eFashionSubMenu.BOTTOM, new FashionItemInfo(avatarInfo.FashionBottomId));
				_fashionItemDict.Add(eFashionSubMenu.SHOE,   new FashionItemInfo(avatarInfo.FashionShoeId));
			}
		}

		public static AvatarInfo GetTestInfo(eAvatarType type = eAvatarType.PC01_W)
		{
			var avatarInfo     = new AvatarInfo();
			var testAvatarData = GetTestData(type);
			avatarInfo.Set(0, testAvatarData);
			return avatarInfo;
		}

		private static Protocols.Avatar GetTestData(eAvatarType type)
		{
			var avatar = new Protocols.Avatar();
			avatar.AvatarID   = 1;
			avatar.AvatarType = (int)type;
			avatar.BodyShape  = 2;
			avatar.Nickname   = "eugene";

			SetFaceItemList(avatar.FaceItemList, type);
			SetFashionItemList(avatar.FashionItemList, type);

			return avatar;
		}

		private static void SetFashionItemList(RepeatedField<FashionItem> fashionItems, eAvatarType type)
		{
			var isMan = type == eAvatarType.PC01_M;

			fashionItems.Add(new FashionItem()
			{
				FashionKey = (int)eFashionSubMenu.TOP,
				FashionID  = isMan ? 16011001 : 15011001,
			});
			fashionItems.Add(new FashionItem()
			{
				FashionKey = (int)eFashionSubMenu.BOTTOM,
				FashionID  = isMan ? 16021001 : 15021001,
			});
			fashionItems.Add(new FashionItem()
			{
				FashionKey = (int)eFashionSubMenu.SHOE,
				FashionID  = isMan ? 16031001 : 15031001,
			});
		}

		private static void SetFaceItemList(RepeatedField<FaceItem> faceItemList, eAvatarType type)
		{
			var isMan = type == eAvatarType.PC01_M;

			faceItemList.Add(new FaceItem()
			{
				FaceKey   = (int)eFaceOption.FACE_SHAPE,
				FaceID    = isMan ? 16011000 : 15011000,
				FaceColor = string.Empty,
			});
			faceItemList.Add(new FaceItem()
			{
				FaceKey   = (int)eFaceOption.SKIN_TYPE,
				FaceID    = isMan ? 16012000 : 15012000,
				FaceColor = string.Empty,
			});
			faceItemList.Add(new FaceItem()
			{
				FaceKey   = (int)eFaceOption.EYE_SHAPE,
				FaceID    = isMan ? 16021000 : 15021000,
				FaceColor = string.Empty,
			});
			faceItemList.Add(new FaceItem()
			{
				FaceKey   = (int)eFaceOption.PUPIL_TYPE,
				FaceID    = isMan ? 16022000 : 15022000,
				FaceColor = string.Empty,
			});
			faceItemList.Add(new FaceItem()
			{
				FaceKey   = (int)eFaceOption.EYE_BROW_OPTION,
				FaceID    = isMan ? 16031000 : 15031000,
				FaceColor = string.Empty,
			});
			faceItemList.Add(new FaceItem()
			{
				FaceKey   = (int)eFaceOption.NOSE_SHAPE,
				FaceID    = isMan ? 16041000 : 15041000,
				FaceColor = string.Empty,
			});
			faceItemList.Add(new FaceItem()
			{
				FaceKey   = (int)eFaceOption.MOUTH_SHAPE,
				FaceID    = isMan ? 16051000 : 15051000,
				FaceColor = string.Empty,
			});
			faceItemList.Add(new FaceItem()
			{
				FaceKey   = (int)eFaceOption.EYE_MAKE_UP_TYPE,
				FaceID    = isMan ? 16061000 : 15061000,
				FaceColor = string.Empty,
			});
			faceItemList.Add(new FaceItem()
			{
				FaceKey   = (int)eFaceOption.EYE_LASH,
				FaceID    = isMan ? 16062000 : 15062000,
				FaceColor = string.Empty,
			});
			faceItemList.Add(new FaceItem()
			{
				FaceKey   = (int)eFaceOption.CHEEK_TYPE,
				FaceID    = isMan ? 16071000 : 15071000,
				FaceColor = string.Empty,
			});
			faceItemList.Add(new FaceItem()
			{
				FaceKey   = (int)eFaceOption.LIP_TYPE,
				FaceID    = isMan ? 16081000 : 15081000,
				FaceColor = string.Empty,
			});
			faceItemList.Add(new FaceItem()
			{
				FaceKey   = (int)eFaceOption.TATTOO_OPTION,
				FaceID    = isMan ? 16091000 : 15091000,
				FaceColor = string.Empty,
			});
			faceItemList.Add(new FaceItem()
			{
				FaceKey   = (int)eFaceOption.HAIR_STYLE,
				FaceID    = isMan ? 16101000 : 15101000,
				FaceColor = string.Empty,
			});
		}

		public static AvatarInfo CreateTestInfo(bool randomAvatar = false, bool randomFashion = false)
		{
			//TODO: 남자 캐릭터 모델 적용 시
			//var avatarType     = UnityEngine.Random.Range(0, 100) > 50 ? eAvatarType.PC01_M : eAvatarType.PC01_W;
			var avatarType = eAvatarType.PC01_W;
			
#region Collect
			var fashionItemDic   = new Dictionary<eFashionSubMenu, List<int>>();
			var faceItemDic      = new Dictionary<eFaceOption, List<int>>();
			var bodyShapes       = new List<int>();
			var tableBodyShape   = AvatarTable.TableBodyShapeItem;
			var tableFashionItem = AvatarTable.TableAvatarFashionItem;
			var tableFaceItem    = AvatarTable.TableFaceItem;

			foreach (var element in tableBodyShape.Datas)
			{
				var data = element.Value;
				if (data.AvatarType == avatarType)
				{
					bodyShapes.Add(data.id);
				}
			}
			
			foreach (var element in tableFashionItem.Datas)
			{
				var data = element.Value;
				if (data.AvatarType == avatarType)
				{
					if (!fashionItemDic.ContainsKey(data.FashionSubMenu))
						fashionItemDic.Add(data.FashionSubMenu, new List<int>());
					
					fashionItemDic[data.FashionSubMenu].Add(data.id);
				}
			}

			foreach (var element in tableFaceItem.Datas)
			{
				var data = element.Value;
				if (data.AvatarType == avatarType)
				{
					if (!faceItemDic.ContainsKey(data.FaceOption))
						faceItemDic.Add(data.FaceOption, new List<int>());

					faceItemDic[data.FaceOption].Add(data.id);
				}
			}

			int GetBodyShape()
			{
				return bodyShapes[randomAvatar ? UnityEngine.Random.Range(0, bodyShapes.Count - 1) : 0];
			}
			
			int GetFashionID(eFashionSubMenu value)
			{
				return fashionItemDic[value][randomFashion ? UnityEngine.Random.Range(0, fashionItemDic[value].Count - 1) : 0];
			}

			int GetFaceID(eFaceOption value)
			{
				return faceItemDic[value][randomAvatar ? UnityEngine.Random.Range(0, faceItemDic[value].Count - 1) : 0];
			}
#endregion

#region Set Data
			var avatar = new Protocols.Avatar();
			avatar.AvatarID   = 1;
			avatar.AvatarType = (int)avatarType;
			avatar.Nickname   = "eugene";
			avatar.BodyShape = GetBodyShape();

			foreach (var value in Enum.GetValues(typeof(eFaceOption)))
			{
				var faceOption = (eFaceOption)value;

				avatar.FaceItemList.Add(new FaceItem()
				{
					FaceKey   = (int)faceOption,
					FaceID    = GetFaceID(faceOption),
					FaceColor = string.Empty,
				});
			}

			foreach (var value in Enum.GetValues(typeof(eFashionSubMenu)))
			{
				var fashionMenu = (eFashionSubMenu)value;
				avatar.FashionItemList.Add(new Protocols.FashionItem()
				{
					FashionKey = (int)fashionMenu,
					FashionID  = GetFashionID(fashionMenu)
				});
			}
#endregion
			var avatarInfo = new AvatarInfo();
			avatarInfo.Set(0, avatar);
			return avatarInfo;
		}

#endregion Utils
	}
}
