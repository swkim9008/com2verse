/*===============================================================
* Product:		Com2Verse
* File Name:	AudioRecordViewModel.cs
* Developer:	ydh
* Date:			2023-03-21 14:10
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.UI;

namespace Com2Verse.AudioRecord
{
    [ViewModelGroup("AudioRecord")]
    public class AudioRecordViewModel : ViewModel
    {
        private string _todayDate;
        public string TodayDate
        {
            get => _todayDate;
            set => SetProperty(ref _todayDate, value);
        }

        private bool _audioRecordItemCollectionIsIn;
        public bool AudioRecordItemCollectionIsIn 
        { 
            get => _audioRecordItemCollectionIsIn;
            set => SetProperty(ref _audioRecordItemCollectionIsIn, value);
        }

        private bool _recordButtonToggleOn;
        public bool RecordButtonToggleOn
        {
            get => _recordButtonToggleOn;
            set => SetProperty(ref _recordButtonToggleOn, value);
        }
        
        private Collection<AudioRecordItemViewModel> _audioRecordItemCollection = new ();
        public Collection<AudioRecordItemViewModel> AudioRecordItemCollection
        {
            get => _audioRecordItemCollection;
            set => SetProperty(ref _audioRecordItemCollection, value);
        }
        
        private bool _maxUploadToggleEnable;
        public bool MaxUploadToggleEnable
        {
            get => _maxUploadToggleEnable;  
            set => SetProperty(ref _maxUploadToggleEnable, value);
        }

        private StackRegisterer _stackRegisterer;

        public StackRegisterer StackRegisterer
        {
            get => _stackRegisterer;
            set
            {
                _stackRegisterer = value;
                _stackRegisterer.WantsToQuit += OnCommand_ExitBtn;
            }
        }

        private Action ExitAction { get; set; }
        private Action PopUpAction { get; set; }
        private Action SortAction { get; set; }
        public CommandHandler Command_RecordPopUpBtn { get; }
        public CommandHandler Command_ExitBtn { get; }
        public CommandHandler Command_SortBtn { get; }

        public AudioRecordViewModel()
        {
            Command_RecordPopUpBtn = new CommandHandler(OnCommand_RecordPopUpBtn);
            Command_ExitBtn = new CommandHandler(OnCommand_ExitBtn);
            Command_SortBtn = new CommandHandler(OnCommand_SortBtn);
        }

        public void Refresh(Action exitAction, Action popUpOpen, Action audioRecordSort)
        {
            RecordButtonToggleOn = true;

            TodayDate =
                $"{DateTime.Now.ToShortDateString()} {DateTimeExtension.GetLocalizationKeyOfDayOfWeekFullName(DateTime.Now.DayOfWeek)}";

            ExitAction = exitAction;
            PopUpAction = popUpOpen;
            SortAction = audioRecordSort;
        }

        private void OnCommand_RecordPopUpBtn()
        {
            if (!MaxUploadToggleEnable)
                return;
            
            PopUpAction?.Invoke();
        }

        private void OnCommand_SortBtn()
        {
            SortAction?.Invoke();
        }
        
        private void OnCommand_ExitBtn()
        {
            ExitAction?.Invoke();
            _stackRegisterer.HideComplete();
        }
    }
}