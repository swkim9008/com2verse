/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUISessionInfoSpeakerViewModel.cs
* Developer:	seaman2000
* Date:			2023-05-08 18:21
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.UI;
using static Com2Verse.Mice.MiceWebClient.Entities;
using UnityEngine.UI;
using Com2Verse.Mice;

namespace Com2Verse
{
    [ViewModelGroup("Mice")]
    public sealed class MiceUISessionInfoSpeakerViewModel : ViewModelBase
	{
		private string _speakerName;
        private string _speakerDesc;

        public string SpeakerName
		{
			get => _speakerName;
            set => SetProperty(ref _speakerName, value);
        }

        public string SpeakerDesc
        {
            get => _speakerDesc;
            set => SetProperty(ref _speakerDesc, value);
        }

        public void SetSpeakerInfo(MiceSessionInfo.Speaker speaker)
		{
            SpeakerName = speaker.StrName;
            SpeakerDesc = speaker.StrDescription;
        }

        public void SetSpeakerDummyInfo(int index)
        {
            SpeakerName = $"연사 이름 ({index})";
            SpeakerDesc = $"연사 설명 ({index})";
        }

    }
}
