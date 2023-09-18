/*===============================================================
* Product:		Com2Verse
* File Name:	UI_DrawingMachine_GiftList.cs
* Developer:	seaman2000
* Date:			2023-07-12 16:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Com2Verse.UI;
using Com2Verse.Mice;
using Cysharp.Threading.Tasks;

namespace Com2Verse
{
    [ViewModelGroup("Mice")]
    public sealed class UI_DrawingMachine_GiftList : ViewModelBase
    {
        private string _giftName;
        private string _giftCount;
        private Texture _giftTexture;

        public string GiftName
        {
            get => _giftName;
            set => SetProperty(ref _giftName, value);
        }

        public string GiftCount
        {
            get => _giftCount;
            set => SetProperty(ref _giftCount, value);
        }

        public Texture GiftTexture
        {
            get => _giftTexture;
            set => SetProperty(ref _giftTexture, value);
        }

        public async UniTask SyncData(Network.PrizeInfo info)
        {
            GiftName = info.ItemName;
            GiftCount = info.ItemQuantity.ToString();
            GiftTexture = GiftTexture.GetOrDownloadTexture(info.ItemPhoto, tex => GiftTexture = tex);

            await UniTask.Yield();
        }

    }
}
