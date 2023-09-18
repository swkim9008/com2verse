/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUIEnterHallViewModel.cs
* Developer:	seaman2000
* Date:			2023-05-10 12:57
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
using System;
using TMPro;
using System.Data;
using System.Linq;
using Com2Verse.EventTrigger;

namespace Com2Verse
{
    [ViewModelGroup("Mice")]
    public sealed partial class MiceUIEnterHallViewModel : MiceViewModel
    {
        private static readonly string UI_ASSET = "UI_Popup_SessionList";

        public CommandHandler MoveToPrevDay { get; private set; }
        public CommandHandler MoveToNextDay { get; private set; }


        private string _eventName;
        private string _eventDateSchedule;
        private string _programName;
        private string _date;
        private string _dayCount;

        private bool _viewEmptyMsg;
        private int _programDropdownIndex;


        private List<TMP_Dropdown.OptionData> _dropDownOption;
        private TMP_Dropdown.DropdownEvent _dropDownValueChangeEvent;
        private bool _dropDownInteractable;
        private Collection<MiceUIEnterHallSessionViewModel> _sessionCollection = new();
        private GUIView _myView;
        private MiceEventInfo _miceEventInfo;
        private int _daySize = 0;
        private List<MiceProgramID> _programIDLists = new();
        private int _curProgramIndex = 0;
        private int _curDay = 0;

        private TriggerInEventParameter _triggerInParameter;
        public int ProgramDropdownIndex
        {
            get => _programDropdownIndex;
            set => SetProperty(ref _programDropdownIndex, value);
        }
       
        public string EventName
        {
            get => _eventName;
            set => SetProperty(ref _eventName, value);
        }

        public string EventDateSchedule
        {
            get => _eventDateSchedule;
            set => SetProperty(ref _eventDateSchedule, value);
        }
        

        public string ProgramName
        {
            get => _programName;
            set => SetProperty(ref _programName, value);
        }

        public string Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        public string DayCount
        {
            get => _dayCount;
            set => SetProperty(ref _dayCount, value);
        }


        public List<TMP_Dropdown.OptionData> DropDownOption
        {
            get => _dropDownOption;
            set => SetProperty(ref _dropDownOption, value);
        }

        public TMP_Dropdown.DropdownEvent DropDownValueChangeEvent
        {
            get => _dropDownValueChangeEvent;
            set => SetProperty(ref _dropDownValueChangeEvent, value);
        }

        public Collection<MiceUIEnterHallSessionViewModel> SessionCollection
        {
            get => _sessionCollection;
            set => SetProperty(ref _sessionCollection, value);
        }

        public bool DropDownInteractable
        {
            get => _dropDownInteractable;
            set => SetProperty(ref _dropDownInteractable, value);
        }
        public bool ViewEmptyMsg
        {
            get => _viewEmptyMsg;
            set => SetProperty(ref _viewEmptyMsg, value);
        }



        public static async UniTask<GUIView> ShowView(Action<GUIView> onShow = null, Action<GUIView> onHide = null)
        {
            GUIView view = await UI_ASSET.AsGUIView();

            void OnOpenedEvent(GUIView view)
            {
                onShow?.Invoke(view);
            }

            void OnClosedEvent(GUIView view)
            {
                onHide?.Invoke(view);

                view.OnOpenedEvent -= OnOpenedEvent;
                view.OnClosedEvent -= OnClosedEvent;
            }

            view.OnOpenedEvent += OnOpenedEvent;
            view.OnClosedEvent += OnClosedEvent;


            view.Show();

            return view;
        }

        public MiceUIEnterHallViewModel()
        {
            DropDownOption = new List<TMP_Dropdown.OptionData>();
            DropDownInteractable = false;

            MoveToPrevDay = new CommandHandler(OnClickMoveToPrevDay);
            MoveToNextDay = new CommandHandler(OnClickMoveToNextDay);
        }

        void OnClickMoveToNextDay()
        {
            if (_miceEventInfo == null) return;

            _curDay = Mathf.Min(_curDay + 1, _daySize - 1);
            UpdateUI();
        }

        void OnClickMoveToPrevDay()
        {
            if (_miceEventInfo == null) return;

            _curDay = Mathf.Max(_curDay - 1, 0);
            UpdateUI();

        }
        void OnDropDownValueChange(int index)
        {
            if (_miceEventInfo == null) return;

            _curProgramIndex = index;
            UpdateUI();
        }

        public void SyncData(GUIView view, TriggerInEventParameter triggerInParameter)
        {
            _myView = view;
            _triggerInParameter = triggerInParameter;

            _miceEventInfo = MiceService.Instance.GetCurrentEventInfo();
            if (_miceEventInfo == null) return;

            // 저장데이타
            _daySize = _miceEventInfo.EndDatetime.DayOfYear - _miceEventInfo.StartDatetime.DayOfYear + 1;
            _programIDLists = (from session in _miceEventInfo.SessionInfoList
                                  orderby session.ProgramID ascending
                                  select session.ProgramID).Distinct().ToList();

            _curProgramIndex = 0;

            // 처음에만 해준다.
            ProgramDropdownIndex = 0;

            _curDay = 0;

            UpdateUI();
        }

        void UpdateUI()
        {
            if (_miceEventInfo == null) return;

            MiceProgramID programID = _programIDLists[_curProgramIndex];
            DateTime curDateTime = _miceEventInfo.StartDatetime.AddDays(_curDay);

            _miceEventInfo = MiceService.Instance.GetCurrentEventInfo();
            EventName = _miceEventInfo.StrTitle;
            EventDateSchedule = $"{_miceEventInfo.StartDatetime.ToShortDateString()} ~ {_miceEventInfo.EndDatetime.ToShortDateString()}";
            DayCount = $"DAY {_curDay+1}";
            Date = curDateTime.ToShortDateString();

            // list
            var curSessionList = (from session in _miceEventInfo.SessionInfoList
                                  where session.ProgramID == programID && session.IsOpenDay(curDateTime)
                                  orderby session.ID
                                  select session).ToList();

            // consist menu
            DropDownOption.Clear();
            foreach (var id in _programIDLists)
            {
                if (!_miceEventInfo.ProgramInfos.TryGetValue(id, out var programInfo)) continue;
                DropDownOption.Add(new TMP_Dropdown.OptionData(programInfo.StrTitle));
            }

            // 1개 이상일 경우만 선택을 활성화시킨다.
            DropDownInteractable = _programIDLists.Count > 1;
            DropDownValueChangeEvent.RemoveListener(OnDropDownValueChange);
            DropDownValueChangeEvent.AddListener(OnDropDownValueChange);

            // collection..
            SessionCollection.Reset();
            foreach (var sessionInfo in curSessionList)
            {
                var item = new MiceUIEnterHallSessionViewModel();
                item.SetData(_myView, sessionInfo, _triggerInParameter).Forget();
                SessionCollection.AddItem(item);
            }

            // show empty message...
            ViewEmptyMsg = curSessionList.Count == 0;
        }
    }
}
