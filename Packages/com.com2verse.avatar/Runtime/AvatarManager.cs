/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarManager.cs
* Developer:	tlghks1009
* Date:			2022-05-25 14:40
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.CustomizeLayer;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.RenderFeatures.Data;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Enum = System.Enum;
using FaceItem = Protocols.FaceItem;

namespace Com2Verse.Avatar
{
	/// <summary>
	/// 아바타 커스터마이즈를 전체적으로 관리하는 Singleton 클래스<br/>
	/// <a href="https://docs.google.com/spreadsheets/d/1BFiu7WrBbg61yX_p3DtKymlxElDnjd02oheTwjKT3JU/edit#gid=1457011078">아바타 코스튬 아이템 테이블</a>
	/// </summary>
	public class AvatarManager : Singleton<AvatarManager>
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private AvatarManager() { }

		private readonly Dictionary<int, Avatar3AssemblerContextAsset> _presetAssetDict = new();

		public uint AvatarRenderingLayerMask { get; set; }
		public int  AvatarBodyLayer          { get; set; }

		public void LoadTable()
		{
			AvatarTable.LoadTable();
			LoadPresetAssets().Forget();
			MapController.Instance.AvatarDtoToStruct = GetAvatarDtoToStruct();
		}

		private void RemoveFaceItem(eFaceOption faceOption, AvatarController avatarController, AvatarInfo targetAvatarInfo)
		{
			avatarController.OnStartLoadAvatarParts();
			avatarController.RemoveFaceOption(faceOption);
			targetAvatarInfo.RemoveFaceItem(faceOption);
		}

		private async UniTask AddFaceItem(FaceItemInfo newOption, AvatarController avatarController, AvatarInfo targetAvatarInfo)
		{
			if (newOption.ItemId == 0) return;

			avatarController.OnStartLoadAvatarParts();
			await avatarController.SetFaceOption(newOption);
			targetAvatarInfo.UpdateFaceItem(newOption);
		}

		private async UniTask ChangeFaceItem(FaceItemInfo currentOption, FaceItemInfo newOption, AvatarController avatarController, AvatarInfo targetAvatarInfo)
		{
			if (newOption.ItemId == 0) return;

			if (currentOption.ItemId != newOption.ItemId || currentOption.ColorId != newOption.ColorId)
			{
				avatarController.OnStartLoadAvatarParts();
				await avatarController.SetFaceOption(newOption);
				targetAvatarInfo.UpdateFaceItem(newOption);
			}
		}

		private async UniTask RemoveFashionItem(eFashionSubMenu fashionSubMenu, AvatarController avatarController, AvatarInfo targetAvatarInfo)
		{
			avatarController.OnStartLoadAvatarParts();
			avatarController.RemoveFashionItem(fashionSubMenu);
			targetAvatarInfo.RemoveFashionItem(fashionSubMenu);

			if (fashionSubMenu == eFashionSubMenu.HAT)
			{
				var currentHair = avatarController.Info?.GetFaceOption(eFaceOption.HAIR_STYLE);
				if (currentHair == null)
					return;

				await avatarController.SetFaceOption(currentHair);
			}
		}

		private async UniTask AddFashionItem(FashionItemInfo newOption, AvatarController avatarController, AvatarInfo targetAvatarInfo)
		{
			if (newOption.ItemId == 0) return;

			avatarController.OnStartLoadAvatarParts();
			await avatarController.SetFashionMenu(newOption);
			targetAvatarInfo.UpdateFashionItem(newOption);
		}

		private async UniTask AddDefaultFashionItem(eFashionSubMenu fashionSubMenu, AvatarController avatarController, AvatarInfo avatarInfo)
		{
			if (AvatarTable.FashionSubMenuFeatures[fashionSubMenu].HasEmptySlot)
				return;

			var avatarType = avatarInfo.AvatarType;
			avatarInfo.UpdateFashionItem(FashionItemInfo.GetDefaultItemInfo(avatarType, fashionSubMenu));
			var newOption = avatarInfo.GetFashionItem(fashionSubMenu);

			if (newOption == null)
				return;

			await AddFashionItem(newOption, avatarController, avatarInfo);
		}

		private async UniTask ChangeFashionItem(FashionItemInfo currentOption, FashionItemInfo newOption, AvatarController avatarController, AvatarInfo targetAvatarInfo)
		{
			if (newOption.ItemId     == 0) return;
			if (currentOption.ItemId == newOption.ItemId) return;

			avatarController.OnStartLoadAvatarParts();
			await avatarController.SetFashionMenu(newOption);
			targetAvatarInfo.UpdateFashionItem(newOption);

			if (newOption.FashionSubMenu == eFashionSubMenu.HAT)
			{
				var currentHair = avatarController.Info?.GetFaceOption(eFaceOption.HAIR_STYLE);
				if (currentHair == null)
					return;

				await avatarController.SetFaceOption(currentHair);
			}
		}

		public async UniTask UpdateAvatarParts(AvatarController avatarController, AvatarInfo avatarInfo, bool needRemoveEssentialItem = true)
		{
			// 아바타 로딩중인경우 무시
			if (!avatarController.IsCompletedLoadAvatarParts)
				return;

			var targetAvatarInfo = avatarController.Info ?? new AvatarInfo();

			foreach (eFashionSubMenu fashionSubMenu in Enum.GetValues(typeof(eFashionSubMenu)))
			{
				var needRemove = needRemoveEssentialItem || AvatarTable.FashionSubMenuFeatures[fashionSubMenu].HasEmptySlot;

				var currentOption = targetAvatarInfo.GetFashionItem(fashionSubMenu);
				var newOption     = avatarInfo.GetFashionItem(fashionSubMenu);

				if (currentOption == null && newOption     == null) await AddDefaultFashionItem(fashionSubMenu, avatarController, avatarInfo);
				else if (needRemove       && currentOption != null && newOption == null) await RemoveFashionItem(fashionSubMenu, avatarController, targetAvatarInfo);
				else if (currentOption                     == null && newOption != null) await AddFashionItem(newOption, avatarController, targetAvatarInfo);
				else if (currentOption                     != null && newOption != null) await ChangeFashionItem(currentOption, newOption, avatarController, targetAvatarInfo);
			}

			foreach (eFaceOption faceOption in Enum.GetValues(typeof(eFaceOption)))
			{
				var currentOption = targetAvatarInfo.GetFaceOption(faceOption);
				var newOption     = avatarInfo.GetFaceOption(faceOption);

				if (currentOption == null && newOption == null) continue;

				var needRemove = needRemoveEssentialItem;
				if (needRemove && currentOption != null && newOption == null) RemoveFaceItem(faceOption, avatarController, targetAvatarInfo);
				else if (currentOption          == null && newOption != null) await AddFaceItem(newOption, avatarController, targetAvatarInfo);
				else if (currentOption          != null && newOption != null) await ChangeFaceItem(currentOption, newOption, avatarController, targetAvatarInfo);
			}

			if (targetAvatarInfo.BodyShape != avatarInfo.BodyShape)
			{
				avatarController.SetBodyShapeIndex(avatarInfo.BodyShape);
				targetAvatarInfo.UpdateBodyShapeItem(avatarInfo.BodyShape);
			}

			if (!avatarController.IsCompletedLoadAvatarParts)
				avatarController.OnCompleteLoadAvatarParts();
		}

#region Assets
		private readonly Dictionary<string, C2VAsyncOperationHandle<ScriptableObject>> _scriptableObjectHandles = new();

		public C2VAsyncOperationHandle<ScriptableObject>? GetScriptableObjectHandle(string addressableName)
		{
			if (_scriptableObjectHandles.TryGetValue(addressableName, out var handle))
				return handle;

			var assetHandle = C2VAddressables.LoadAssetAsync<ScriptableObject>(addressableName);
			if (assetHandle == null)
				return null;

			_scriptableObjectHandles[addressableName] = assetHandle;
			return assetHandle;
		}

		public void ClearAssetHandles()
		{
			ClearScriptableObjectHandles();
		}

		private void ClearScriptableObjectHandles()
		{
			foreach (var handle in _scriptableObjectHandles.Values)
				handle.Release();
			_scriptableObjectHandles.Clear();
		}

		private async UniTask LoadPresetAssets()
		{
			var presetData = AvatarTable.TableFacePreset?.Datas;
			if (presetData == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Preset data is null");
				return;
			}

			foreach (var (key, facePreset) in presetData)
			{
				var addressableName = facePreset.address ?? string.Empty;
				var presetInfoHandle = C2VAddressables.LoadAssetAsync<ScriptableObject>(addressableName);

				if (presetInfoHandle == null)
				{
					C2VDebug.LogErrorCategory(GetType().Name, $"presetInfoHandle is null. addressableName : {addressableName}");
					continue;
				}

				var presetInfoAsset = await presetInfoHandle.ToUniTask() as Avatar3AssemblerContextAsset;
				if (presetInfoAsset.IsUnityNull())
				{
					C2VDebug.LogErrorCategory(GetType().Name, $"presetInfoAsset is null. addressableName : {addressableName}");
					continue;
				}

				_presetAssetDict.TryAdd(key, presetInfoAsset!);
			}
		}

		public Avatar3AssemblerContextAsset? GetFirstPresetAsset(eAvatarType avatarType) => GetPresetAsset(AvatarTable.GetDefaultFaceOptionId(avatarType, eFaceOption.PRESET_LIST));

		public Avatar3AssemblerContextAsset? GetPresetAsset(int itemId) => _presetAssetDict.TryGetValue(itemId, out var value) ? value : null;
#endregion Assets

#region Mapobject Avatar Struct
		public void UpdateAvatarParts(AvatarController avatarController, BaseMapObject.AvatarCustomizeInfo partsData)
		{
			UpdateAvatarPartsAsync(avatarController, partsData).Forget();
		}

		private async UniTask UpdateAvatarPartsAsync(AvatarController avatarController, BaseMapObject.AvatarCustomizeInfo partsData)
		{
			var avatarInfo = GetAvatarInfoFromCustomizeInfo(partsData);

			// FIXME: 임시처리
			var faceOptions = avatarInfo.GetFaceOptionList();
			if (faceOptions.Count == 1)
			{
				var presetData = avatarInfo.GetFaceOption(eFaceOption.PRESET_LIST);
				if (presetData != null)
					avatarInfo = GetFacePresetInfo(presetData.ItemId, avatarInfo, false);
			}

			await UpdateAvatarParts(avatarController, avatarInfo);
		}

		public AvatarInfo GetAvatarInfoFromCustomizeInfo(BaseMapObject.AvatarCustomizeInfo partsData) =>
			new(
				GetAvatarTypeFromPartsData(partsData),
				GetFaceOptionItemsFromPartsData(partsData),
				GetBodyShapeFromPartsData(partsData),
				GetFashionMenuItemsFromPartsData(partsData));

		private Func<Protocols.Avatar, BaseMapObject.AvatarCustomizeInfo> GetAvatarDtoToStruct()
		{
			return avatar =>
			{
				var info = new BaseMapObject.AvatarCustomizeInfo
				{
					AvatarType = avatar.AvatarType,
					BodyShape  = avatar.BodyShape,
				};

				info.FaceItems    ??= new Dictionary<eFaceOption, BaseMapObject.AvatarItemInfo>();
				info.FashionItems ??= new Dictionary<eFashionSubMenu, BaseMapObject.AvatarItemInfo>();

				foreach (var faceItem in avatar.FaceItemList)
				{
					var faceColor = int.TryParse(faceItem.FaceColor ?? "0", out var color) ? color : 0;
					var itemInfo  = new BaseMapObject.AvatarItemInfo(faceItem.FaceID, faceColor);
					info.FaceItems.Add((eFaceOption)faceItem.FaceKey, itemInfo);
				}

				foreach (var fashionItem in avatar.FashionItemList)
				{
					var itemInfo = new BaseMapObject.AvatarItemInfo(fashionItem.FashionID, 0);
					info.FashionItems.Add((eFashionSubMenu)fashionItem.FashionKey, itemInfo);
				}

				return info;
			};
		}

		private eAvatarType GetAvatarTypeFromPartsData(BaseMapObject.AvatarCustomizeInfo partsData)
		{
			var avatarType = (eAvatarType)partsData.AvatarType;
			if (!Enum.IsDefined(typeof(eAvatarType), avatarType))
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"AvatarTypeFromPartsDataArray: avatarType is not defined. avatarType: {avatarType}");
			}
			return (eAvatarType)partsData.AvatarType;
		}

		private int GetBodyShapeFromPartsData(BaseMapObject.AvatarCustomizeInfo partsData)
		{
			return partsData.BodyShape;
		}

		private FaceItemInfo[] GetFaceOptionItemsFromPartsData(BaseMapObject.AvatarCustomizeInfo partsData)
		{
			var faceOptionItems = new List<FaceItemInfo>();

			if (partsData.FaceItems == null)
				return faceOptionItems.ToArray();

			foreach (var faceOption in partsData.FaceItems.Values)
			{
				var faceItemId = faceOption.Id;
				var faceColor  = faceOption.Color;
				faceOptionItems.Add(new FaceItemInfo(faceItemId, faceColor));
			}
			return faceOptionItems.ToArray();
		}

		private int[] GetFashionMenuItemsFromPartsData(BaseMapObject.AvatarCustomizeInfo partsData)
		{
			var fashionMenuItems = new List<int>();

			if (partsData.FashionItems == null)
				return fashionMenuItems.ToArray();

			foreach (var fashionMenu in partsData.FashionItems.Values)
			{
				var fashionItemId = fashionMenu.Id;
				if (fashionItemId == 0)
					continue;

				fashionMenuItems.Add(fashionItemId);
			}
			return fashionMenuItems.ToArray();
		}

#region Preset
		public BaseMapObject.AvatarCustomizeInfo SetCustomizeInfoFromItemKey(Avatar3AssemblerContext context)
		{
			var partsData = (BaseMapObject.AvatarCustomizeInfo)context;

			var avatarType = (eAvatarType)partsData.AvatarType;
			if (!Enum.IsDefined(typeof(eAvatarType), avatarType))
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"SetCustomizeInfoFromItemKey: avatarType is not defined. avatarType: {avatarType}");
				return partsData;
			}

			var faceItems = partsData.FaceItems;
			if (faceItems != null)
			{
				var newFaceItems = new Dictionary<eFaceOption, BaseMapObject.AvatarItemInfo>();
				foreach (eFaceOption faceOption in Enum.GetValues(typeof(eFaceOption)))
				{
					if (!faceItems.ContainsKey(faceOption))
						continue;

					var item = faceItems[faceOption];
					var id = faceOption == eFaceOption.LIP_TYPE ? AvatarTable.GetLipsTypeId(avatarType, context.LipsTexSel, context.LipsMaskTexSel) : AvatarTable.GetFaceOptionId(avatarType, faceOption, item.Id);

					newFaceItems.Add(faceOption, new BaseMapObject.AvatarItemInfo(id, 0));
				}

				partsData.FaceItems = newFaceItems;
			}

			partsData.BodyShape = AvatarTable.GetBodyShapeId(avatarType, partsData.BodyShape);

			var fashionItem = partsData.FashionItems;
			if (fashionItem != null)
			{
				var newFashionItem = new Dictionary<eFashionSubMenu, BaseMapObject.AvatarItemInfo>();
				foreach (eFashionSubMenu fashionType in Enum.GetValues(typeof(eFashionSubMenu)))
				{
					if (!fashionItem.ContainsKey(fashionType))
						continue;

					var item = fashionItem[fashionType];
					if (item.Id == -1)
						continue;

					var id   = AvatarTable.GetFashionSubMenuId(avatarType, fashionType, item.Id);
					newFashionItem.Add(fashionType, new BaseMapObject.AvatarItemInfo(id, item.Color));
				}

				partsData.FashionItems = newFashionItem;
			}

			ApplyDataFromAddressableKey(context, eFaceOption.HAIR_STYLE,  ref partsData);
			ApplyDataFromAddressableKey(context, eFashionSubMenu.TOP,     ref partsData);
			ApplyDataFromAddressableKey(context, eFashionSubMenu.BOTTOM,  ref partsData);
			ApplyDataFromAddressableKey(context, eFashionSubMenu.SHOE,    ref partsData);
			ApplyDataFromAddressableKey(context, eFashionSubMenu.GLASSES, ref partsData);

			return partsData;
		}

		private bool GetItemKeyFromAddressableKey(string data, out int itemKey, out int colorKey)
		{
			data = data.Replace(".prefab", "");
			var t = data.Split('_');
			if (t == null || t.Length < 5 || t[3] == null || t[4] == null)
			{
				itemKey  = 0;
				colorKey = 0;
				return false;
			}

			var hasItemKey  = int.TryParse(t[3]!, out itemKey);
			var hasColorKey = int.TryParse(t[4]!, out colorKey);

			return hasItemKey && hasColorKey;
		}

		private void ApplyDataFromAddressableKey(Avatar3AssemblerContext context, eFaceOption faceOption, ref BaseMapObject.AvatarCustomizeInfo partsData)
		{
			if (faceOption != eFaceOption.HAIR_STYLE || partsData.FaceItems == null)
				return;

			var data = context.GetAddress(FashionItemParts.Hair)!;
			if (!string.IsNullOrEmpty(data))
			{
				var hasKey = GetItemKeyFromAddressableKey(data, out var itemKey, out var colorKey);

				if (hasKey)
				{
					var avatarType = (eAvatarType)partsData.AvatarType;
					var id         = AvatarTable.GetFaceOptionId(avatarType, eFaceOption.HAIR_STYLE, itemKey);
					var hairItem   = new BaseMapObject.AvatarItemInfo(id, colorKey);
					partsData.FaceItems[eFaceOption.HAIR_STYLE] = hairItem;
				}
			}
		}

		private string GetFashionAddress(Avatar3AssemblerContext context, eFashionSubMenu fashionType)
		{
			return context.GetAddress(fashionType);
		}

		private void ApplyDataFromAddressableKey(Avatar3AssemblerContext context, eFashionSubMenu fashionType, ref BaseMapObject.AvatarCustomizeInfo partsData)
		{
			if (partsData.FashionItems == null)
				return;

			var data = GetFashionAddress(context, fashionType);
			if (!string.IsNullOrEmpty(data))
			{
				var hasKey = GetItemKeyFromAddressableKey(data, out var itemKey, out var colorKey);

				if (hasKey)
				{
					var avatarType = (eAvatarType)partsData.AvatarType;
					var id         = AvatarTable.GetFashionSubMenuId(avatarType, fashionType, itemKey);
					var hairItem   = new BaseMapObject.AvatarItemInfo(id, colorKey);
					partsData.FashionItems[fashionType] = hairItem;
				}
			}
		}
#endregion Preset
#endregion Mapobject Avatar Struct

		public AvatarInfo GetDefaultAvatarInfo(eAvatarType avatarType)
		{
			var avatarInfos = AvatarTable.TableDefault?.Datas;
			if (avatarInfos == null)
			{
				C2VDebug.LogErrorCategory(nameof(AvatarTable), "AvatarTable was not loaded.");
				return AvatarTable.GetBaseAvatarInfo(avatarType);
			}

			foreach (var avatarInfo in avatarInfos)
			{
				if (avatarInfo.AvatarType != avatarType)
					continue;

				var facePresetInfo = GetFacePresetInfo(avatarInfo.FacePresetId);
				if (facePresetInfo == null)
					continue;

				facePresetInfo.UpdateBodyShapeItem(avatarInfo.BodyShapeId);
				facePresetInfo.UpdateFashionItem(new FashionItemInfo(avatarInfo.FashionTopId));
				facePresetInfo.UpdateFashionItem(new FashionItemInfo(avatarInfo.FashionBottomId));
				facePresetInfo.UpdateFashionItem(new FashionItemInfo(avatarInfo.FashionShoeId));

				return facePresetInfo;
			}

			return AvatarTable.GetBaseAvatarInfo(avatarType);
		}

		private AvatarInfo? GetPresetInfo(int itemId)
		{
			var presetInfoAsset = GetPresetAsset(itemId);
			if (presetInfoAsset.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"presetInfoAsset is null. itemId : {itemId}");
				return null;
			}

			var avatarCustomizeInfo = SetCustomizeInfoFromItemKey(presetInfoAsset!.Avatar!);
			var avatarInfo          = GetAvatarInfoFromCustomizeInfo(avatarCustomizeInfo);
			avatarInfo.UpdateFaceItem(new FaceItemInfo(itemId));

			return avatarInfo;
		}

		public AvatarInfo? GetFacePresetInfo(int itemId, AvatarInfo? currentAvatarInfo = null, bool clearFashionInfo = true)
		{
			var avatarInfo = GetPresetInfo(itemId);
			if (avatarInfo == null)
				return null;

			// Face값을 제외한 다른 정보 제거
			if (currentAvatarInfo != null)
			{
				var bodyResId = AvatarTable.GetBodyShapeResIdToInt(currentAvatarInfo.BodyShape);
				avatarInfo.UpdateBodyShapeItem(AvatarTable.GetBodyShapeId(currentAvatarInfo.AvatarType, bodyResId));
				avatarInfo.SerialId = currentAvatarInfo.SerialId;
				avatarInfo.AvatarId = currentAvatarInfo.AvatarId;
				avatarInfo.InitializeFashionItem(currentAvatarInfo.GetFashionItemList());
			}

			if (clearFashionInfo)
				avatarInfo.ClearFashionItem();

			return avatarInfo;
		}
	}
}
