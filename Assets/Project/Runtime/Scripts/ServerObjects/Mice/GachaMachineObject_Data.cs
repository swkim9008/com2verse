/*===============================================================
* Product:		Com2Verse
* File Name:	PrizeDrawingMachineObject_Data.cs
* Developer:	seaman2000
* Date:			2023-07-19 18:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Cysharp.Threading.Tasks;
using Com2Verse.Mice;
using System.Threading;
using System.Collections.Generic;
using static Com2Verse.Mice.MiceWebClient;


namespace Com2Verse.Network
{
    // 가차 정보
    public sealed class GachaInfo
    {
        public long PrizeId { get; private set; }
        public int TryCount { get; private set; }
        public int MyTryCount { get; private set; }
        public bool PersonalInfoNeeded { get; private set; }
        public long? WinPrizeItemId { get; private set; }
        public int? WinPrizeItemSeq { get; private set; }
        public DateTime StartDateTime { get; private set; }
        public DateTime EndDateTime { get; private set; }
        public eMicePrizeReceiveTypeCode? WinReceiveType { get; private set; }
        public eMicePrizePrivacyAgreeTypeCode WinPrivacyAgreeType { get; private set; }
        public bool HasTryCount { get => MyTryCount > 0; }


        public void SyncData(MiceWebClient.Entities.PrizeInfoEntity info)
        {
            this.PrizeId = info.PrizeId;
            this.TryCount = info.TryCount;
            this.MyTryCount = info.MyTryCount;
            this.PersonalInfoNeeded = info.PersonalInfoNeeded;
            this.WinPrizeItemId = info.WinPrizeItemId;
            this.WinPrizeItemSeq = info.WinPrizeItemSeq;
            this.WinReceiveType = info.WinReceiveType;
            this.WinPrivacyAgreeType = info.WinPrivacyAgreeType;
            this.StartDateTime = info.StartDateTime;
            this.EndDateTime = info.EndDateTime;
        }
        
        public bool IsInPlayTime() // 가차 가능 시간
        {
            var curTick = DateTime.UtcNow.Ticks;
            if (curTick < this.StartDateTime.Ticks) return false;
            if (curTick > this.EndDateTime.Ticks) return false;

            return true;
        }

        public void UseTicket()
        {
            this.MyTryCount--;
        }
    }

    // 상품 정보
    public sealed class PrizeInfo
    {
        public long PrizeItemId { get; private set; }
        public long PrizeId { get; private set; }
        public string ItemName { get; private set; }
        public int ItemQuantity { get; private set; }
        public eMicePrizeReceiveTypeCode ReceiveType { get; private set; }
        public int? PrizeItemIdSeq { get; private set; }
        public string ItemPhoto { get; private set; }
        public bool HasPrize { get => ItemQuantity > 0; }
        public bool PersonalInfoNeeded { get; private set; }
        public eMicePrizePrivacyAgreeTypeCode PrizePrivacyAgreeType { get; private set; }

        ~PrizeInfo()
        {
        }

        public void SyncData(Network.GachaInfo item)
        {
            this.PrizeId = item.PrizeId;
            this.PrizeItemId = item.WinPrizeItemId.HasValue ? (long)item.WinPrizeItemId : 0;
            this.PrizeItemIdSeq = item.WinPrizeItemSeq.HasValue ? (int)item?.WinPrizeItemSeq : 0;
            this.PrizePrivacyAgreeType = item.WinPrivacyAgreeType;

            this.ReceiveType = item.WinReceiveType.HasValue ? (eMicePrizeReceiveTypeCode)item.WinReceiveType : eMicePrizeReceiveTypeCode.RECEIVE_NONE;
            this.PersonalInfoNeeded = item.PersonalInfoNeeded;

        }


        public void SyncData(MiceWebClient.Entities.PrizeItem item, bool personalInfoNeeded=false)
        {
            this.PrizeItemId = item.PrizeItemId;
            this.PrizeId = item.PrizeId;
            this.ItemQuantity = item.ItemQuantity;
            this.PrizeItemIdSeq = item.PrizeItemIdSeq.HasValue ? item.PrizeItemIdSeq : 0;
            this.ItemName = item.ItemName;
            this.ItemPhoto = item.ItemPhoto;

            this.ReceiveType = item.ReceiveType;
            this.PrizePrivacyAgreeType = item.PrivacyAgreeType;
            this.PersonalInfoNeeded = personalInfoNeeded;

        }

        public void PersonalInfoSendComplete()
        {
            this.PersonalInfoNeeded = false;
        }
    }

    // 당첨자 정보
    public sealed class PersonalInfo
    {
        public long prizeId;
        public long prizeItemId;
        public long prizeItemSeq;
        public string phoneNumber;
        public string email;
        public string address;

    }



    public sealed partial class GachaMachineObject   // Data
    {
        private GachaInfo gachaInfo = new GachaInfo();
        private List<PrizeInfo> prizeInfo = new List<PrizeInfo>();
        private PrizeInfo resultPrizeInfo = new PrizeInfo();

        private async UniTask<bool> SyncGachaInfo(long prizeEventKey)
        {
            var result = await MiceWebClient.Prize.InfoGet_PrizeId(prizeEventKey);
            if (result)
            {
                this.gachaInfo.SyncData(result.Data);

                // 배송지 관련 마지막 상품을 미리 만들어놓는다.
                this.resultPrizeInfo.SyncData(this.gachaInfo);
            }
            return result;
        }

        private async UniTask<bool> SyncPrizeInfo(long prizeid)
        {
            var result = await MiceWebClient.Prize.ItemsGet_PrizeId(prizeid);
            if (result)
            {
                this.prizeInfo.Clear();
                foreach (var entry in result.Data)
                {
                    var newItem = new PrizeInfo();
                    newItem.SyncData(entry);
                    this.prizeInfo.Add(newItem);
                }
            }
            return result;
        }

        private async UniTask<bool> SendPrizeTry(long prizeid, CancellationToken token)
        {
            var response = await MiceWebClient.Prize.TryPost(prizeid);//.ManualErrorHandling();
            token.ThrowIfCancellationRequested();

            if(response)
            {
                // 꽝일 경우는 MiceWebClient.eMiceHttpErrorCode.PRIZE_TRY_NONE 으로 내려온다.
                bool hasPrize = !string.IsNullOrEmpty(response.Data.ItemName); //  response.Result.MiceStatusCode == MiceWebClient.eMiceHttpErrorCode.OK;
                resultPrizeInfo.SyncData(response.Data, hasPrize);
                return true;
            }

            // TODO : 추후 에러 메세지가 확정되면 작업
            // error process
            //switch (response.Result.MiceStatusCode)
            //{
            //    case eMiceHttpErrorCode.PRIZE_NOT_IN_TIME:
            //        UIManager.Instance.SendToastMessage("PRIZE_NOT_IN_TIME", 3f, UIManager.eToastMessageType.WARNING);
            //        return false;
            //    case eMiceHttpErrorCode.PRIZE_TRY_COUNT_NONE:
            //        UIManager.Instance.SendToastMessage("PRIZE_TRY_COUNT_NONE", 3f, UIManager.eToastMessageType.WARNING);
            //        return false;
            //    case eMiceHttpErrorCode.PRIZE_TRY_NONE:
            //        UIManager.Instance.SendToastMessage("PRIZE_TRY_NONE", 3f, UIManager.eToastMessageType.WARNING);
            //        return false;
            //}

            //response.ShowErrorMessage();
            return false;
        }
    }
}
