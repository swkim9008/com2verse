/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingTagViewModel.cs
* Developer:	tlghks1009
* Date:			2022-09-07 18:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Organization;
using Cysharp.Threading.Tasks;
using UnityEngine;
using EmployeeNoType = System.String;
using MemberIdType = System.Int64;

namespace Com2Verse.UI
{
    [ViewModelGroup("MeetingReservation")]
    public sealed class MeetingTagViewModel : ViewModelBase
    {
        private Action<MeetingTagViewModel> _onClicked;

        private MemberIdType _memberId;
        private string _memberName;
        private Texture _profileIcon;
        private string _positionLevelDeptInfo;
        private string _employeeAddress;
        private bool _isMine;

        public CommandHandler Command_RemoveButtonClick { get; }


        public MeetingTagViewModel(MemberIdType memberId, Action<MeetingTagViewModel> onClicked)
        {
            _memberId = memberId;
            _onClicked = onClicked;

            var employeePayload = DataManager.Instance.GetMember(memberId);
            if (employeePayload != null)
            {
                EmployeeName          = employeePayload.Member.MemberName;
                PositionLevelDeptInfo = employeePayload.GetPositionLevelTeamStr();
                EmployeeAddress       = employeePayload.Member.MailAddress;
                IsMine                = employeePayload.IsMine();
                if (!string.IsNullOrEmpty(employeePayload.Member.PhotoPath))
                    Util.DownloadTexture(employeePayload.Member.PhotoPath, (success, texture) => ProfileIcon = texture).Forget();
            }

            Command_RemoveButtonClick = new CommandHandler(OnRemoveButtonClicked);
        }

        public MemberIdType MemberId => _memberId;

        public string EmployeeName
        {
            get => _memberName;
            set => SetProperty(ref _memberName, value);
        }

        public Texture ProfileIcon
        {
            get => _profileIcon;
            set => SetProperty(ref _profileIcon, value);
        }

        public string PositionLevelDeptInfo
        {
            get => _positionLevelDeptInfo;
            set => SetProperty(ref _positionLevelDeptInfo, value);
        }

        public string EmployeeAddress
        {
            get => _employeeAddress;
            set => SetProperty(ref _employeeAddress, value);
        }

        public bool IsMine
        {
            get => _isMine;
            set => SetProperty(ref _isMine, value);
        }


        private void OnRemoveButtonClicked()
        {
            _onClicked?.Invoke(this);
        }
    }
}
