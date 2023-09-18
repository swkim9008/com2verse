/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingSearchViewModel.cs
* Developer:	ksw
* Date:			2023-03-07 11:52
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Organization;
using Com2Verse.UI;
using MemberIdType = System.Int64;

namespace Com2Verse
{
    public sealed partial class MeetingInquiryListViewModel
    {
        // TODO : 기획 데이터 변경
        private static readonly int MaxSearchResult = 15;
        private bool _onSearchFieldFocusedByOrganizer;
        private bool _onSearchFieldFocusedByParticipating;
        private int _indexOfFindOrganizer;
        private int _indexOfFindParticipating;
        private Collection<EmployeeSearchResultViewModel> _meetingOrganizerSearchResults = new();
        private Collection<EmployeeSearchResultViewModel> _meetingParticipatingSearchResults = new();
        private bool _setActiveOrganizerSearchField = false;
        private bool _setActiveParticipatingSearchField = false;
        private MemberIdType _organizerMemberId;
        private MemberIdType _participatingMemberId;
#region Properties
        public string SearchParticipating
        {
            get => _meetingParticipating;
            set
            {
                SetProperty(ref _meetingParticipating, value);

                if (string.IsNullOrWhiteSpace(value))
                {
                    ResetSearchTimer();
                    MeetingParticipatingSearchResults.Reset();
                    return;
                }

                SetSearchTimer(() =>
                {
                    var employeePayload = DataManager.Instance.FindMemberByName(_meetingParticipating);
                    employeePayload.Sort(MemberModel.CompareByName);
                    MeetingParticipatingSearchResults.Reset();

                    int count = 0;
                    foreach (var memberModel in employeePayload)
                    {
                        if (count >= MaxSearchResult) break;

                        MeetingParticipatingSearchResults.AddItem(new EmployeeSearchResultViewModel(memberModel.Member.AccountId)
                        {
                            SearchText = SearchParticipating,
                        });
                        count++;
                    }
                });
            }
        }

        public string SearchOrganizer
        {
            get => _meetingOrganizer;
            set
            {
                SetProperty(ref _meetingOrganizer, value);

                if (string.IsNullOrWhiteSpace(value))
                {
                    ResetSearchTimer();
                    MeetingOrganizerSearchResults.Reset();
                    return;
                }

                SetSearchTimer(() =>
                {
                    var employeePayload = DataManager.Instance.FindMemberByName(_meetingOrganizer);
                    employeePayload.Sort(MemberModel.CompareByName);
                    MeetingOrganizerSearchResults.Reset();

                    int count = 0;
                    foreach (var memberModel in employeePayload)
                    {
                        if (count >= MaxSearchResult) break;

                        MeetingOrganizerSearchResults.AddItem(new EmployeeSearchResultViewModel(memberModel.Member.AccountId)
                        {
                            SearchText = SearchOrganizer,
                        });
                        count++;
                    }
                });
            }
        }

        public bool OnSearchFieldFocusedByOrganizer
        {
            get => _onSearchFieldFocusedByOrganizer;
            set
            {
                _onSearchFieldFocusedByOrganizer = value;
                if (_onSearchFieldFocusedByOrganizer)
                {
                    if (!string.IsNullOrWhiteSpace(SearchOrganizer))
                    {
                        SearchOrganizer = SearchOrganizer;
                    }
                }

                base.InvokePropertyValueChanged(nameof(OnSearchFieldFocusedByOrganizer), value);
            }
        }

        public bool OnSearchFieldFocusedByParticipating
        {
            get => _onSearchFieldFocusedByParticipating;
            set
            {
                _onSearchFieldFocusedByParticipating = value;
                if (_onSearchFieldFocusedByParticipating)
                {
                    if (!string.IsNullOrWhiteSpace(SearchParticipating))
                    {
                        SearchParticipating = SearchParticipating;
                    }
                }

                base.InvokePropertyValueChanged(nameof(OnSearchFieldFocusedByParticipating), value);
            }
        }
        
        public int IndexOfFindOrganizer
        {
            get => _indexOfFindOrganizer;
            set
            {
                _indexOfFindOrganizer = value;

                SelectOrganizer();

                base.InvokePropertyValueChanged(nameof(IndexOfFindOrganizer), value);
            }
        }

        public int IndexOfFindParticipating
        {
            get => _indexOfFindParticipating;
            set
            {
                _indexOfFindParticipating = value;

                SelectParticipating();

                base.InvokePropertyValueChanged(nameof(IndexOfFindParticipating), value);
            }
        }

        public int IndexOfFindOrganizerWithSubmitted
        {
            get => _indexOfFindOrganizer;
            set
            {
                _indexOfFindOrganizer = value;
                
                SelectOrganizer();
                base.InvokePropertyValueChanged(nameof(IndexOfFindOrganizerWithSubmitted), value);
            }
        }

        public int IndexOfFindParticipatingWithSubmitted
        {
            get => _indexOfFindParticipating;
            set
            {
                _indexOfFindParticipating = value;

                SelectParticipating();
                base.InvokePropertyValueChanged(nameof(IndexOfFindParticipatingWithSubmitted), value);
            }
        }

        public Collection<EmployeeSearchResultViewModel> MeetingOrganizerSearchResults
        {
            get => _meetingOrganizerSearchResults;
            set
            {
                _meetingOrganizerSearchResults = value;
                InvokePropertyValueChanged(nameof(MeetingOrganizerSearchResults), value);
            }
        }

        public Collection<EmployeeSearchResultViewModel> MeetingParticipatingSearchResults
        {
            get => _meetingParticipatingSearchResults;
            set
            {
                _meetingParticipatingSearchResults = value;
                InvokePropertyValueChanged(nameof(MeetingParticipatingSearchResults), value);
            }
        }

        private void SelectOrganizer()
        {
            var searchResult = MeetingOrganizerSearchResults.Value[IndexOfFindOrganizer];

            _organizerMemberId = searchResult.MemberId;
            MeetingOrganizer = GetMemberName(searchResult.MemberId);
            SetActiveOrganizerSearchField = false;
        }

        private void SelectParticipating()
        {
            var searchResult = MeetingParticipatingSearchResults.Value[IndexOfFindParticipating];

            _participatingMemberId = searchResult.MemberId;
            MeetingParticipating = GetMemberName(searchResult.MemberId);
            SetActiveParticipatingSearchField = false;
        }

        private static string GetMemberName(MemberIdType memberId)
        {
            var memberModel = DataManager.Instance.GetMember(memberId);

            return memberModel == null ? string.Empty : memberModel.Member.MemberName;
        }

        public bool SetActiveOrganizerSearchField
        {
            get => _setActiveOrganizerSearchField;
            set
            {
                _setActiveOrganizerSearchField = value;
                InvokePropertyValueChanged(nameof(SetActiveOrganizerSearchField), value);
            }
        }

        public bool SetActiveParticipatingSearchField
        {
            get => _setActiveParticipatingSearchField;
            set
            {
                _setActiveParticipatingSearchField = value;
                InvokePropertyValueChanged(nameof(SetActiveParticipatingSearchField), value);
            }
        }
#endregion

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
