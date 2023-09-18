/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUIEnterHallSessionViewModel.cs
* Developer:	seaman2000
* Date:			2023-05-10 13:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.UI;
using Com2Verse.Mice;
using Cysharp.Threading.Tasks;

namespace Com2Verse
{
    [ViewModelGroup("Mice")]
    public sealed class MiceUIEnterHallSessionSpeakerViewModel : MiceViewModel
    {
        private string _speakerName;

        private MiceSessionInfo.Speaker _speaker;

        public CommandHandler ShowSpeakerDetailInfo { get; private set; }

        public string SpeakerName
        {
            get => _speakerName;
            set => SetProperty(ref _speakerName, value);
        }

        public MiceUIEnterHallSessionSpeakerViewModel()
        {
            ShowSpeakerDetailInfo = new CommandHandler(OnClickShowDetailInfo);
        }

        void OnClickShowDetailInfo()
        {
            if (_speaker == null) return;

            async UniTask Show(MiceSessionInfo.Speaker speaker)
            {
                var view = await MiceUIEnterHallSessionSpeakerInfoViewModel.ShowView();
                if (view.ViewModelContainer.TryGetViewModel(typeof(MiceUIEnterHallSessionSpeakerInfoViewModel), out var viewModel))
                {
                    var miceUISessionInfoViewModel = viewModel as MiceUIEnterHallSessionSpeakerInfoViewModel;
                    miceUISessionInfoViewModel?.SetData(speaker);
                }
            }

            Show(_speaker).Forget();
        }

        public void SetData(MiceSessionInfo.Speaker speakerInfo)
        {
            _speaker = speakerInfo;

            this.SpeakerName = speakerInfo.StrName;
        }
    }
}
