/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingEmployeeListViewModel.cs
* Developer:	tlghks1009
* Date:			2022-09-08 20:10
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Linq;
using Com2Verse.Organization;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using AuthorityCode = Com2Verse.WebApi.Service.Components.AuthorityCode;

namespace Com2Verse.UI
{
    public class MeetingEmployeeListModel : DataModel
    {
        public string InvitedEmployeeCountText;
        public string WaitingEmployeeCountText;
        public string ReaderEmployeeCountText;
    }

    [ViewModelGroup("MeetingReservation")]
    public sealed class MeetingEmployeeListViewModel : ViewModelDataBase<MeetingEmployeeListModel>
    {
        private Collection<MeetingEmployeeInfoViewModel> _meetingEmployeeInfoCollection = new();

        private bool _isVisiblePopup;
        public CommandHandler Command_CloseButtonClick { get; }

        public MeetingEmployeeListViewModel()
        {
            Command_CloseButtonClick = new CommandHandler(OnCommand_CloseButtonClicked);

            _isVisiblePopup = true;
        }

        public override void OnInitialize()
        {
            base.OnInitialize();

            _isVisiblePopup = true;
        }

        public void Set(MeetingInfoType meetingInfo)
        {
            ScrollReset();

            SetupParticipants(meetingInfo);

            _isVisiblePopup = true;

            InvitedEmployeeCountText    = Localization.Instance.GetString("UI_MeetingAppDetail_Popup_List_Title_Participant", _meetingEmployeeInfoCollection.CollectionCount.ToString());
            WaitingEmployeeCountText    = Localization.Instance.GetString("UI_MeetingAppDetail_Popup_List_Title_Wait", 0.ToString());
            ReaderEmployeeCountText     = Localization.Instance.GetString("UI_MeetingAppDetail_Popup_List_Title_Reader", 0.ToString());
        }

        public Collection<MeetingEmployeeInfoViewModel> MeetingEmployeeInfoCollection
        {
            get => _meetingEmployeeInfoCollection;
            set
            {
                _meetingEmployeeInfoCollection = value;
                base.InvokePropertyValueChanged(nameof(MeetingEmployeeInfoCollection), value);
            }
        }

        public bool IsVisiblePopup
        {
            get => _isVisiblePopup;
            set => SetProperty(ref _isVisiblePopup, value);
        }


        public string InvitedEmployeeCountText
        {
            get => base.Model.InvitedEmployeeCountText;
            set => SetProperty(ref base.Model.InvitedEmployeeCountText, value);
        }

        public string WaitingEmployeeCountText
        {
            get => base.Model.WaitingEmployeeCountText;
            set => SetProperty(ref base.Model.WaitingEmployeeCountText, value);
        }

        public string ReaderEmployeeCountText
        {
            get => base.Model.ReaderEmployeeCountText;
            set => SetProperty(ref base.Model.ReaderEmployeeCountText, value);
        }


        public float ScrollRectHorizontalNormalizedPosition => 0;

        private void OnCommand_CloseButtonClicked()
        {
            _meetingEmployeeInfoCollection.Reset();

            IsVisiblePopup = false;
        }

        private void SetupParticipants(MeetingInfoType meetingInfo)
        {
            var sortedByOrganizer = meetingInfo.MeetingMembers?.OrderByDescending(x => x.AuthorityCode == AuthorityCode.Organizer);

            foreach (var meetingUserInfo in sortedByOrganizer)
            {
                var memberModel = DataManager.Instance.GetMember(meetingUserInfo.AccountId);
                if (memberModel == null)
                {
                    continue;
                }

                var meetingEmployeeInfo = new MeetingEmployeeInfoViewModel(meetingInfo, meetingUserInfo, memberModel, OnOrganizerChanged);

                _meetingEmployeeInfoCollection.AddItem(meetingEmployeeInfo);
            }
        }


        private void OnOrganizerChanged(MeetingEmployeeInfoViewModel meetingEmployeeInfoViewModel)
        {
            OnCommand_CloseButtonClicked();
        }


        private void ScrollReset()
        {
            base.InvokePropertyValueChanged(nameof(ScrollRectHorizontalNormalizedPosition), ScrollRectHorizontalNormalizedPosition);
        }
    }
}
