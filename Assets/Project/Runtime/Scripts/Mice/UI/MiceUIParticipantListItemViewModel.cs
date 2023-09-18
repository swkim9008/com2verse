/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUIParticipantListItemViewModel.cs
* Developer:	sprite
* Date:			2023-04-19 19:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Net;
using Com2Verse.Logger;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Mice
{
    [ViewModelGroup("Mice")]
    public sealed partial class MiceUIParticipantListItemViewModel : MiceViewModel
    {
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
        public CommandHandler ClickBusinessCard { get; private set; }

        private bool _isSelected;
        private MiceParticipantInfo.Item _data;

        public MiceUIParticipantListItemViewModel(MiceParticipantInfo.Item data)
        {
            _isSelected = false;
            _data = data;

            this.ClickBusinessCard = new CommandHandler(this.OnClickBusinessCard);
        }

        private void OnClickBusinessCard()
        {
            // ToDo: (임시) 눌렸는지 표시.
            this.IsSelected = !this.IsSelected;

            ShowBusinessCard().Forget();
            // ToDo: Business card 표시.
        }

        private async UniTask ShowBusinessCard()
        {
            var response = await MiceWebClient.User.AccountGet_TargetAccountId(_data.AccountId);

            if (response.Result.HttpStatusCode != HttpStatusCode.OK) return;
            if (response.Result.MiceStatusCode == MiceWebClient.eMiceHttpErrorCode.NOT_PUBLIC_ACCOUNT)
            {
                C2VDebug.Log($"Not public account {_data.AccountId}");
                return;
            }
            
            MiceBusinessCardViewModel.ShowView(new MiceUserInfo(response.Data));
        }
    }
}
