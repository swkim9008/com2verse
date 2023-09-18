/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarCreator_SMR.cs
* Developer:	tlghks1009
* Date:			2022-08-10 18:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Text;
using System.Threading;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Com2Verse.LruObjectPool;

namespace Com2Verse.Avatar
{
    public struct CreatedSmrObjectItem
    {
        public AvatarItemInfo AvatarItemInfo    { get; }
        public GameObject     FashionItemPrefab { get; }

        public CreatedSmrObjectItem(AvatarItemInfo avatarItemInfo, GameObject fashionItemPrefab)
        {
            AvatarItemInfo    = avatarItemInfo;
            FashionItemPrefab = fashionItemPrefab;
        }
    }

    public static partial class AvatarCreator
    {
        public static class SmrObjectCreator
        {
            private static StringBuilder _sb = new StringBuilder();

            public static async UniTask<CreatedSmrObjectItem?> CreateAsync(AvatarItemInfo itemInfo, CancellationTokenSource? cancellationTokenSource = null)
            {
                var itemAddressableName = GetAddressableName(itemInfo);
                GameObject loadedAsset = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<GameObject>(itemAddressableName, cancellationTokenSource);
                if (loadedAsset.IsReferenceNull())
                {
                    C2VDebug.LogWarning(nameof(SmrObjectCreator), "failed load asset");
                    return null;
                }

                return new CreatedSmrObjectItem(itemInfo, loadedAsset);
            }

            private static string GetAddressableName(AvatarItemInfo itemInfo)
            {
                var itemName = string.Empty;

                if (itemInfo is FashionItemInfo fashionItemInfo) itemName = GetFashionItemName(fashionItemInfo);
                else if (itemInfo is FaceItemInfo faceItemInfo) itemName  = GetHairItemName(faceItemInfo);

                if (string.IsNullOrEmpty(itemName))
                    return string.Empty;

                _sb.Clear();
                _sb.Append($"{itemName}.prefab");
                return _sb.ToString();
            }

            private static string GetHairItemName(FaceItemInfo faceItemInfo)
            {
                var item = AvatarTable.GetFaceItem(faceItemInfo.ItemId);
                return item == null ? string.Empty : $"{item.AvatarType}_HAIR_{AvatarTable.GetResId(item)}_{faceItemInfo.ColorId:D3}";
            }

            private static string GetFashionItemName(FashionItemInfo fashionItemInfo)
            {
                var item                 = AvatarTable.GetFashionItem(fashionItemInfo.ItemId);
                var fashionSubMenu       = fashionItemInfo.FashionSubMenu;
                var fashionSubMenuString = fashionSubMenu == eFashionSubMenu.GLASSES ? "GLS" : fashionSubMenu.ToString()?.ToUpper() ?? string.Empty;
                return item == null ? string.Empty : $"{item.AvatarType}_{fashionSubMenuString}_{item.ItemKey:D3}_{item.ColorKey:D3}";
            }
        }
    }
}