/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomUserInviteViewModel.cs
* Developer:	ksw
* Date:			2023-04-17 14:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.UI;
using MemberIdType = System.Int64;

namespace Com2Verse
{
	public sealed partial class MeetingRoomUserInviteViewModel
	{
#region Variables
        private static readonly int    MaxSearchResult = 15;
        
        private bool   _onSearchFieldFocused;
        private int    _indexOfFindEmployee;
        private string _searchEmployee;
        private bool   _setActiveDropDown;
        private string _employeeName;

        private Collection<EmployeeSearchResultViewModel> _meetingEmployeeResults = new();
#endregion // Variables
#region Properties
        public string SearchEmployee
        {
            get => _searchEmployee;
            set
            {
                SetProperty(ref _searchEmployee, value);

                if (string.IsNullOrWhiteSpace(value))
                {
                    ResetSearchTimer();
                    MeetingEmployeeResults.Reset();
                    SetActiveDropDown = false;
                    return;
                }

                SetSearchTimer(() =>
                {
                    var memberModels = DataManager.Instance.FindMemberByName(_searchEmployee);
                    memberModels.Sort(MemberModel.CompareByName);
                    MeetingEmployeeResults.Reset();

                    int count = 0;
                    foreach (var memberModel in memberModels)
                    {
                        if (count >= MaxSearchResult) break;
                        if (IsAlreadyInvitedMember(memberModel.Member.AccountId))
                            continue;

                        MeetingEmployeeResults.AddItem(new EmployeeSearchResultViewModel(memberModel.Member.AccountId)
                        {
                            SearchText = SearchEmployee,
                        });
                        count++;
                    }

                    SetActiveDropDown = true;
                });
            }
        }

        public bool OnSearchFieldFocused
        {
            get => _onSearchFieldFocused;
            set
            {
                _onSearchFieldFocused = value;
                if (_onSearchFieldFocused)
                {
                    SetActiveDropDown = true;
                    if (!string.IsNullOrWhiteSpace(SearchEmployee))
                    {
                        SearchEmployee = SearchEmployee;
                    }
                }

                base.InvokePropertyValueChanged(nameof(OnSearchFieldFocused), value);
            }
        }
        public int IndexOfFindEmployee
        {
            get => _indexOfFindEmployee;
            set
            {
                _indexOfFindEmployee = value;

                FindEmployee();
                AddMember(MemberId);

                base.InvokePropertyValueChanged(nameof(IndexOfFindEmployee), value);
            }
        }

        public int IndexOfFindEmployeeWithSubmitted
        {
            get => _indexOfFindEmployee;
            set
            {
                _indexOfFindEmployee = value;

                FindEmployee();
                AddMember(MemberId);

                base.InvokePropertyValueChanged(nameof(IndexOfFindEmployeeWithSubmitted), value);
            }
        }

        public bool SetActiveDropDown
        {
            get => _setActiveDropDown;
            set => SetProperty(ref _setActiveDropDown, value);
        }

        public string EmployeeName
        {
            get => _employeeName;
            set => SetProperty(ref _employeeName, value);
        }


        public MemberIdType MemberId { get; set; }

        public Collection<EmployeeSearchResultViewModel> MeetingEmployeeResults
        {
            get => _meetingEmployeeResults;
            set
            {
                _meetingEmployeeResults = value;
                InvokePropertyValueChanged(nameof(MeetingEmployeeResults), value);
            }
        }


        private void FindEmployee()
        {
            var employee = MeetingEmployeeResults.Value[IndexOfFindEmployee];

            MemberId = employee.MemberId;
            EmployeeName = MeetingReservationProvider.GetMemberNameInOrganization(employee.MemberId);
        }


        private void AddMember(MemberIdType employeeNo)
        {
            if (!CanAddEmployee(employeeNo))
                return;

            RemoveSelectedEmployeeInSearchField();

            AddMeetingTagElement(employeeNo);
            OnSearchFieldFocused = false;
            SetActiveDropDown    = false;
        }

        private void RemoveSelectedEmployeeInSearchField()
        {
            EmployeeName = string.Empty;

            MeetingEmployeeResults.RemoveItem(_indexOfFindEmployee);
        }

        private bool CanAddEmployee(MemberIdType employeeNo)
        {
            if (employeeNo < 0)
            {
                return false;
            }

            if (InviteTagCollection.Value.Count > MeetingReservationProvider.MaxNumberOfParticipants - MeetingReservationProvider.EnteredMeetingInfo.MeetingMembers.Length - 1)
            {
                UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Common_OrganizationUserCountFail_Popup_Toast", MeetingReservationProvider.MaxNumberOfParticipants));
                return false;
            }

            if (IsAlreadyInvitedMember(employeeNo))
                return false;

            return DataManager.Instance.GetMember(employeeNo) != null;
        }


        private bool IsAlreadyInvitedMember(MemberIdType memberId)
        {
            if (User.Instance.CurrentUserData is not OfficeUserData userData) return true;
            if (userData.ID == memberId)
                return true;

            foreach (var inviteTag in _inviteTagCollection.Value)
            {
                if (memberId == inviteTag.InviteEmployeeNo)
                    return true;
            }

            foreach (var meetingUserInfo in MeetingReservationProvider.EnteredMeetingInfo.MeetingMembers)
            {
                if (meetingUserInfo.AccountId == memberId)
                    return true;
            }

            return false;
        }
#endregion // Properties
#region Timer
        private UIManager.Timer _searchTimer;

        private static readonly float SearchDelayTime = 0.12f;

		public void SetSearchTimer(Action onTimerEnd)
		{
			var timer = GetSearchTimer();
			UIManager.Instance.StartTimer(timer, SearchDelayTime, onTimerEnd);
		}

		public  void            ResetSearchTimer() => _searchTimer?.Reset();
		private UIManager.Timer GetSearchTimer()   => _searchTimer ??= new UIManager.Timer();
#endregion // Timer
	}
}
