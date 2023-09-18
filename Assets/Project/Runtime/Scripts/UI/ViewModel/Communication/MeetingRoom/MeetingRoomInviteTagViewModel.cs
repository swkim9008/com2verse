/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomInviteTagViewModel.cs
* Developer:	ksw
* Date:			2023-04-17 14:50
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Organization;
using MemberIdType = System.Int64;

namespace Com2Verse.UI
{
    [ViewModelGroup("Communication")]
    public sealed class MeetingRoomInviteTagViewModel : ViewModelBase
    {
        private Action<MeetingRoomInviteTagViewModel> _onClickRemoveButton;

        private string _inviteEmployeeName;
        private MemberIdType _inviteEmployeeNo;
        
        public  CommandHandler CommandRemoveButtonClick { get; }


        public MeetingRoomInviteTagViewModel(MemberIdType memberId, Action<MeetingRoomInviteTagViewModel> onClickRemoveButton)
        {
            _onClickRemoveButton  = onClickRemoveButton;

            var memberModel = DataManager.Instance.GetMember(memberId);
            if (memberModel != null)
            {
                InviteEmployeeName = memberModel.Member.MemberName;
                InviteEmployeeNo   = memberModel.Member.AccountId;
            }

            CommandRemoveButtonClick = new CommandHandler(OnRemoveButtonClicked);
        }


        public string InviteEmployeeName
        {
            get => _inviteEmployeeName;
            set => SetProperty(ref _inviteEmployeeName, value);
        }

        public MemberIdType InviteEmployeeNo
        {
            get => _inviteEmployeeNo;
            set => SetProperty(ref _inviteEmployeeNo, value);
        }

        private void OnRemoveButtonClicked()
        {
            _onClickRemoveButton?.Invoke(this);
        }
    }
}
