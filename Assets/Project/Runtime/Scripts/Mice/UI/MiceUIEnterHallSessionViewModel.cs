/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUIEnterHallSessionViewModel.cs
* Developer:	seaman2000
* Date:			2023-05-10 13:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.UI;
using Com2Verse.Mice;
using Cysharp.Threading.Tasks;
using System.Net;
using Com2Verse.Interaction;
using Com2Verse.EventTrigger;
using UnityEngine;

namespace Com2Verse
{
    [ViewModelGroup("Mice")]
    public sealed class MiceUIEnterHallSessionViewModel : MiceViewModel
    {
        private GameObject _goMiceUIViewSessionList;


        private string _sessionTimeSchedule;
        private string _sessionName;
        private Collection<MiceUIEnterHallSessionSpeakerViewModel> _speakerSlots = new();
        private bool _layoutRebuild;

        private MiceSessionInfo _sessionInfo;
        private GUIView _parentView;
        private TriggerInEventParameter _triggerInParameter;

        public CommandHandler EnterSession { get; private set; }

        public string SessionTimeSchedule
        {
            get => _sessionTimeSchedule;
            set => SetProperty(ref _sessionTimeSchedule, value);
        }

        public string SessionName
        {
            get => _sessionName;
            set => SetProperty(ref _sessionName, value);
        }

        public Collection<MiceUIEnterHallSessionSpeakerViewModel> SpeakerSlots
        {
            get => _speakerSlots;
            set => SetProperty(ref _speakerSlots, value);
        }

        public bool LayoutRebuild
        {
            get => _layoutRebuild;
            set => SetProperty(ref _layoutRebuild, value);
        }

        public GameObject GoMiceUIViewSessionList
        {
            get => _goMiceUIViewSessionList;
            set => SetProperty(ref _goMiceUIViewSessionList, value);
        }
        


        public MiceUIEnterHallSessionViewModel()
        {
            EnterSession = new CommandHandler(OnClickEnterSession);
        }

        void OnClickEnterSession()
        {
            if(_sessionInfo == null) return;

            if (MiceInfoManager.Instance.SelectProperUserAuthorityWithSessionID(_sessionInfo.SessionEntity.SessionId) == MiceWebClient.eMiceAuthorityCode.NORMAL 
                && (_sessionInfo.SessionEntity.StateCode == MiceWebClient.eMiceSessionStateCode.READY
                    || _sessionInfo.SessionEntity.StateCode == MiceWebClient.eMiceSessionStateCode.FINISH))
            {
                UIManager.Instance.ShowPopupConfirm(
                    Data.Localization.eKey.MICE_UI_Popup_Title_Lounge_SpaceMove.ToLocalizationString(),
                    Data.Localization.eKey.MICE_UI_Popup_Msg_Lounge_SpaceMove.ToLocalizationString());
            }
            else
            {
                _Process().Forget();
            }
            
            async UniTask _Process()
            {
                var response = await MiceWebClient.Event.EnterHallPost_SessionId(_sessionInfo.ID);
                if (response.Result.HttpStatusCode == HttpStatusCode.OK)
                {
                    // 안보이도록 한다.
                    if(_triggerInParameter != null)
                    {
                        InteractionManager.Instance.UnsetInteractionUI(_triggerInParameter.SourceTrigger, _triggerInParameter.CallbackIndex);
                    }

                    await MiceService.Instance.RequestEnterMiceHall(_sessionInfo.ID);
                    _parentView?.Hide();
                }
                else if (response.Result.HttpStatusCode == HttpStatusCode.Conflict)
                {
                    if (response.Result.MiceStatusCode == MiceWebClient.eMiceHttpErrorCode.NO_EVENT
                    || response.Result.MiceStatusCode == MiceWebClient.eMiceHttpErrorCode.NO_SESSION
                    || response.Result.MiceStatusCode == MiceWebClient.eMiceHttpErrorCode.NEED_TICKET
                    || response.Result.MiceStatusCode == MiceWebClient.eMiceHttpErrorCode.NOT_YET_SESSION_OPENED)
                    {
                        UIManager.Instance.ShowPopupConfirm(
                            Data.Localization.eKey.MICE_UI_Popup_Title_Lounge_SpaceMove.ToLocalizationString(),
                            Data.Localization.eKey.MICE_UI_Popup_Msg_Lounge_SpaceMove.ToLocalizationString());
                    }
                }
            }
        }

/*
        public void SetData(GUIView view, MiceSessionInfo sessionInfo, TriggerInEventParameter triggerInParameter)
        {
            _parentView = view;
            _sessionInfo = sessionInfo;
            _triggerInParameter = triggerInParameter;

            this.SessionName = sessionInfo.StrName;
            this.SessionTimeSchedule = $"{sessionInfo.StrStartDataTime} - {sessionInfo.StrEndDataTime}";

            // speaker collection...
            int speakerCount = sessionInfo.Speakers.Count;
            while (SpeakerSlots.Value.Count > speakerCount) { SpeakerSlots.RemoveItem(0); }

            for (int loop = 0, max = speakerCount; loop < max; ++loop)
            {
                if (SpeakerSlots.Value.Count <= loop)
                {
                    var speakerViewModel = new MiceUIEnterHallSessionSpeakerViewModel();
                    SpeakerSlots.AddItem(speakerViewModel);
                }
                var curViewModel = SpeakerSlots.Value[loop];
                curViewModel.SetData(sessionInfo.Speakers[loop]);
            }
        }
*/


        public async UniTask SetData(GUIView view, MiceSessionInfo sessionInfo, TriggerInEventParameter triggerInParameter)
        {
            _parentView = view;
            _sessionInfo = sessionInfo;
            _triggerInParameter = triggerInParameter;

            this.SessionName = sessionInfo.StrName;
            this.SessionTimeSchedule = $"{sessionInfo.StrStartDataTime} - {sessionInfo.StrEndDataTime}";

            await UniTask.WaitUntil(()=>this.GoMiceUIViewSessionList != null);
            var go = this.GoMiceUIViewSessionList.GetComponent<MiceUIViewSessionList>();
            if (go != null)
            {
                go.UpdateSpeaker(sessionInfo);
            }
        }
    }
}
