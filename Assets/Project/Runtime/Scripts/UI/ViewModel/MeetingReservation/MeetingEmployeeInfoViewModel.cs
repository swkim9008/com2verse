/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingEmployeeInfoViewModel.cs
* Developer:	tlghks1009
* Date:			2022-09-08 20:10
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Linq;
using Com2Verse.MeetingReservation;
using Com2Verse.Organization;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MemberIdType = System.Int64;
using MeetingUserType = Com2Verse.WebApi.Service.Components.MeetingMemberEntity;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using MeetingIdType = System.Int64;
using MeetingStatus = Com2Verse.WebApi.Service.Components.MeetingStatus;
using AuthorityCode = Com2Verse.WebApi.Service.Components.AuthorityCode;

namespace Com2Verse.UI
{
    public class MeetingEmployeeInfoModel : DataModel
    {
        public string EmployeeName;
        public Texture ProfileIcon;

        public MeetingUserType MeetingUserInfo;
        public MeetingInfoType MeetingInfo;
    }

    //FIXME :ACCOUNT ID
    [ViewModelGroup("MeetingReservation")]
    public partial class MeetingEmployeeInfoViewModel : ViewModelDataBase<MeetingEmployeeInfoModel>
    {
        private Action<MeetingEmployeeInfoViewModel> _onDeletedEvent;
        private Action<MeetingEmployeeInfoViewModel> _onOrganizerChangedEvent;

        // ReSharper disable InconsistentNaming

        //public CommandHandler Command_OrganizerChangeButtonClick { get; }

        // ReSharper restore InconsistentNaming


        public MeetingEmployeeInfoViewModel(MeetingInfoType meetingInfo, MeetingUserType meetingUserInfo, MemberModel memberModel, Action<MeetingEmployeeInfoViewModel> onOrganizerChanged)
        {
            base.Model.MeetingInfo      = meetingInfo;
            base.Model.MeetingUserInfo  = meetingUserInfo;

            MeetingID           = meetingInfo.MeetingId;
            MemberId            = memberModel?.Member.AccountId ?? -1;
            EmployeeName        = memberModel?.Member.MemberName ?? string.Empty;

            FindEmployeeIcon(meetingUserInfo);

            _onOrganizerChangedEvent = onOrganizerChanged;
        }

        public MemberIdType MemberId { get; }

        public MeetingIdType MeetingID { get; }

        public string EmployeeName
        {
            get => base.Model.EmployeeName;
            set
            {
                base.Model.EmployeeName = value;

                RefreshView();
                base.InvokePropertyValueChanged(nameof(EmployeeName), value);
            }
        }

        public bool IsVisibleOrganizerMark => base.Model.MeetingUserInfo.AuthorityCode == AuthorityCode.Organizer;

        public bool IsVisibleOrganizerChangeButton
        {
            get
            {
                if (MeetingReservationProvider.IsOrganizer(base.Model.MeetingInfo.MeetingMembers?.ToList()))
                {
                    return base.Model.MeetingUserInfo.AuthorityCode != AuthorityCode.Organizer;
                }

                return false;
            }
        }

        public Texture ProfileIcon
        {
            get => base.Model.ProfileIcon;
            set => SetProperty(ref base.Model.ProfileIcon, value);
        }


        private void RefreshView()
        {
            base.InvokePropertyValueChanged(nameof(IsVisibleOrganizerMark), IsVisibleOrganizerMark);
            base.InvokePropertyValueChanged(nameof(IsVisibleOrganizerChangeButton), IsVisibleOrganizerChangeButton);
        }


        private void FindEmployeeIcon(MeetingUserType meetingUserInfo)
        {
            var memberModel = DataManager.Instance.GetMember(meetingUserInfo.AccountId);
            if (memberModel != null)
            {
                if (!string.IsNullOrEmpty(memberModel.Member.PhotoPath))
                {
                    Util.DownloadTexture(memberModel.Member.PhotoPath, (success, texture) => ProfileIcon = texture).Forget();
                }
            }
        }

        private bool CanChangeOrganizer()
        {
            if (base.Model.MeetingInfo?.MeetingStatus is MeetingStatus.MeetingExpired or MeetingStatus.MeetingPassed)
            {
                UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_MeetingApp_Desc_AlreadyEnd"), null);
                return false;
            }

            if (base.Model.MeetingInfo?.CancelYn == "Y")
            {
                UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_MeetingApp_Desc_Deleted"), null);
                return false;
            }
            return true;
        }
    }
}
