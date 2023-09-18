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
using UnityEngine.UI;
using Com2Verse.Mice;
using System;

namespace Com2Verse
{
    [ViewModelGroup("Mice")]
    public sealed class MiceUIConferenceDataDwonloadDataNameViewModel : MiceViewModel
    {
        private string _dataName;
        private bool _isSelected;
        private Action ChangeSelect;

        public MiceSessionInfo.AttachmentFile AttachedData { get; private set; } = default;

        public CommandHandler ToggleSelect { get; private set; }

        public string DataName
        {
			get => _dataName;
            set => SetProperty(ref _dataName, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                SetProperty(ref _isSelected, value);

                // 변동사항이 생기면 다시 계산하도록 한다.
                this.ChangeSelect?.Invoke();
            } 
        }

        public MiceUIConferenceDataDwonloadDataNameViewModel()
        {
            this.ToggleSelect = new CommandHandler(() => _isSelected ^= true);
        }


        public void SetInfo(MiceSessionInfo.AttachmentFile attachedData, Action changeSelected)
		{
            this.ChangeSelect = changeSelected;
            this.AttachedData = attachedData;
            this.DataName = attachedData.StrName;

            // 초기값은 true로 설정한다.
            this.IsSelected = true;
        }
    }
}
