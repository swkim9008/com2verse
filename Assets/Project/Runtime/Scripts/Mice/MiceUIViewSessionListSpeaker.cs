/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUIViewSessionList.cs
* Developer:	wlemon
* Date:			2023-04-10 11:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Com2Verse.Extension;
using Com2Verse.UIExtension;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace Com2Verse.Mice
{

    public class MiceUIViewSessionListSpeaker : MonoBehaviour
    {
        [SerializeField] private TMP_Text _spekerName;
        [SerializeField] private Button _buttonShowDetail;

        private MiceSessionInfo.Speaker _speakerInfo;

        private void Awake()
        {
            this._buttonShowDetail.onClick.AddListener(() =>
            {
                OnClickShowDetailInfo();
            });
        }

        public void SetData(MiceSessionInfo.Speaker speakerInfo)
        {
            _speakerInfo = speakerInfo;
            _spekerName.text = speakerInfo.StrName;
        }

        void OnClickShowDetailInfo()
        {
            if (_speakerInfo == null) return;

            async UniTask Show(MiceSessionInfo.Speaker speaker)
            {
                var view = await MiceUIEnterHallSessionSpeakerInfoViewModel.ShowView();
                if (view.ViewModelContainer.TryGetViewModel(typeof(MiceUIEnterHallSessionSpeakerInfoViewModel), out var viewModel))
                {
                    var miceUISessionInfoViewModel = viewModel as MiceUIEnterHallSessionSpeakerInfoViewModel;
                    miceUISessionInfoViewModel?.SetData(speaker);
                }
            }

            Show(_speakerInfo).Forget();
        }
    }
}

