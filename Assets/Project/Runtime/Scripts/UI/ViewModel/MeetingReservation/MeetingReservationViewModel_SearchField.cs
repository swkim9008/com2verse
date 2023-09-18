/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingReservationViewModel.SearchField.cs
* Developer:	jhkim
* Date:			2022-10-20 12:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using MemberIdType = System.Int64;

namespace Com2Verse.UI
{
    // Search Field
    public partial class MeetingReservationViewModel
    {
#region Variables
        private static readonly int MaxSearchResult = 15;
        private bool _onSearchFieldFocused;
        private int _indexOfFindEmployee;
        private Collection<EmployeeSearchResultViewModel> _meetingEmployeeResults = new();
#endregion // Variables

#region Properties
        public string SearchEmployee
        {
            get => base.Model.SearchMember;
            set
            {
                SetProperty(ref base.Model.SearchMember, value);

                if (string.IsNullOrWhiteSpace(value))
                {
                    ResetSearchTimer();
                    MeetingEmployeeResults.Reset();
                    SetActiveDropDown = false;
                    return;
                }

                SetSearchTimer(() =>
                {
                    var memberModels = DataManager.Instance.FindMemberByName(base.Model.SearchMember);
                    memberModels.Sort(MemberModel.CompareByName);
                    MeetingEmployeeResults.Reset();

                    int count = 0;
                    foreach (var memberModel in memberModels)
                    {
                        if (count >= MaxSearchResult) break;
                        if (IsAlreadyInvitedEmployee(memberModel.Member.AccountId))
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
                    if (!string.IsNullOrEmpty(SearchEmployee))
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
                AddEmployee(MemberId);

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
                AddEmployee(MemberId);

                base.InvokePropertyValueChanged(nameof(IndexOfFindEmployeeWithSubmitted), value);
            }
        }

        public string MemberName
        {
            get => base.Model.MemberName;
            set => SetProperty(ref base.Model.MemberName, value);
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
            MemberName = MeetingReservationProvider.GetMemberNameInOrganization(employee.MemberId);
        }


        private void AddEmployee(MemberIdType memberId)
        {
            if (!CanAddEmployee(memberId))
                return;

            RemoveSelectedEmployeeInSearchField();

            AddMeetingTagElement(memberId);
        }

        private void RemoveSelectedEmployeeInSearchField()
        {
            MemberName = string.Empty;

            MeetingEmployeeResults.RemoveItem(_indexOfFindEmployee);
        }

        private bool CanAddEmployee(MemberIdType memberId)
        {
            if (memberId < 0)
            {
                return false;
            }

            if (memberId == User.Instance.CurrentUserData.ID)
                return false;

            if (_meetingAllUserMemberId.Count + 2 > MeetingReservationProvider.MaxNumberOfParticipants)
            {
                UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Common_OrganizationUserCountFail_Popup_Toast", MeetingReservationProvider.MaxNumberOfParticipants));
                return false;
            }

            if (IsAlreadyInvitedEmployee(memberId))
                return false;

            return DataManager.Instance.GetMember(memberId) != null;
        }


        private bool IsAlreadyInvitedEmployee(MemberIdType memberId)
        {
            if (User.Instance.CurrentUserData is not OfficeUserData userData) return true;
            if (userData.ID == memberId)
                return true;

            foreach (var meetingTagViewModel in _meetingTagCollection.Value)
            {
                if (memberId == meetingTagViewModel.MemberId)
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

        public void ResetSearchTimer() => _searchTimer?.Reset();
        private UIManager.Timer GetSearchTimer() => _searchTimer ??= new UIManager.Timer();
#endregion // Timer
    }
}
