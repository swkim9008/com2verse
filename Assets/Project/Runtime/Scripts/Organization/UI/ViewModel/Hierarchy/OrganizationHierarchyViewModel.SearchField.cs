/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationHierarchyViewModel.SearchField.cs
* Developer:	jhkim
* Date:			2022-10-18 14:25
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Linq;
using Com2Verse.Organization;
using MemberIdType = System.Int64;

namespace Com2Verse.UI
{
	// Search Field
	public partial class OrganizationHierarchyViewModel
	{
#region Variables
		// TODO : 무한스크롤 적용 후 제거
		private static readonly int MaxSearchResult = 30;

		private string _searchText;
		private string _searchTextWithoutNotify;
		private int _selectedIndex;
		private bool _isSearchFieldEmpty = true;
		private bool _isShowSearchResult;
		private bool _isSearching;
		private float _defaultSearchScrollViewHeight;
		private float _searchScrollViewHeight;
		private Collection<CheckEmployeeSearchResultViewModel> _searchResults = new();
		public CommandHandler ClearSearchField { get; private set; }
#endregion // Variables

#region Properties
		public string SearchText
		{
			get => _searchText;
			set
			{
				_searchText = value;
				_searchTextWithoutNotify = value;
				RefreshSearchFieldUI();
				Search(_searchText);
				InvokePropertyValueChanged(nameof(SearchText), value);
			}
		}

		public string SearchTextWithoutNotify
		{
			get => _searchTextWithoutNotify;
			set
			{
				_searchTextWithoutNotify = value;
				RefreshSearchFieldUI();
				InvokePropertyValueChanged(nameof(SearchTextWithoutNotify), value);
			}
		}

		public int SelectedIndex
		{
			get => _selectedIndex;
			set
			{
				_selectedIndex = value;
				InvokePropertyValueChanged(nameof(SelectedIndex), value);
			}
		}

		public bool IsSearchFieldEmpty
		{
			get => _isSearchFieldEmpty;
			set
			{
				_isSearchFieldEmpty = value;
				InvokePropertyValueChanged(nameof(IsSearchFieldEmpty), value);
			}
		}

		public bool IsShowSearchResult
		{
			get => _isShowSearchResult;
			set
			{
				_isShowSearchResult = value;
				InvokePropertyValueChanged(nameof(IsShowSearchResult), value);
			}
		}

		public bool IsSearching
		{
			get => _isSearching;
			set
			{
				_isSearching = value;
				RefreshCheckEmployeeHeight(value);
				InvokePropertyValueChanged(nameof(IsSearching), value);
			}
		}

		public float DefaultSearchScrollViewheight
		{
			get => _defaultSearchScrollViewHeight;
			set
			{
				_defaultSearchScrollViewHeight = value;
				InvokePropertyValueChanged(nameof(DefaultSearchScrollViewheight), value);
			}
		}

		public float SearchScrollViewHeight
		{
			get => _searchScrollViewHeight;
			set
			{
				_searchScrollViewHeight = value;
				InvokePropertyValueChanged(nameof(SearchScrollViewHeight), value);
			}
		}
		public Collection<CheckEmployeeSearchResultViewModel> SearchResults
		{
			get => _searchResults;
			set
			{
				_searchResults = value;
				InvokePropertyValueChanged(nameof(SearchResults), value);
			}
		}
#endregion // Properties

#region Initialize
		private void InitSearchField()
		{
			ClearSearchField = new CommandHandler(OnclearSearchField);
		}
#endregion // Initialize

#region Binding Events
		private void OnclearSearchField() => ClearSearchText();
#endregion // Binding Events
		private void ClearSearchText()
		{
			SearchTextWithoutNotify = string.Empty;
			CloseSearch();
		}

		private void CloseSearch()
		{
			ResetSearchTimer();
			ResetSearch();
			IsShowSearchResult = false;
			IsSearching = false;
		}

		private void ResetSearch()
		{
			SearchResults?.Reset();
		}
		private void RefreshSearchFieldUI()
		{
			IsSearchFieldEmpty = string.IsNullOrWhiteSpace(SearchTextWithoutNotify);
		}

		private void Search(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				ResetSearchTimer();
				CloseSearch();
				return;
			}

			ResetSearch();
			SetSearchTimer(() =>
			{
				var memberModels = DataManager.Instance.FindMemberByName(text);
				memberModels.Sort(MemberModel.CompareByName);
				for (var i = 0; i < memberModels.Count; i++)
				{
					if (MaxSearchResult > 0 && i >= MaxSearchResult) break;
					var memberModel = memberModels[i];
					var model = new CheckEmployeeSearchResultViewModel(memberModel.Member.AccountId, IsChecked(memberModel), IsInteractable(memberModel), OnClick)
					{
						SearchText = text,
					};
					model.SetOnValueChanged(OnToggleValueChanged);
					SearchResults.AddItem(model);
				}
				RefreshScrollViewHeight();
				IsSearching = true;
			});

			bool IsChecked(MemberModel memberModel) => _groupInviteModel.SelectedInfo.SelectedMap.Keys.Contains(memberModel.Member.AccountId);
			bool IsInteractable(MemberModel employee) => !employee.IsMine();
		}

		private void OnClick(MemberIdType employeeNo, bool isOn) => OnToggleValueChanged(employeeNo, isOn);
		private void OnToggleValueChanged(MemberIdType memberId, bool isOn)
		{
			var memberModel = DataManager.Instance.GetMember(memberId);
			var model = new CheckMemberListModel
			{
				Info = memberModel,
				IsChecked = isOn
			};
			if (isOn)
				AddGroupInvite(model);
			else
				RemoveGroupInvite(model);
		}

		private void RefreshSearchEmployeeCheckUI(MemberIdType employeeNo, bool isOn)
		{
			if (!IsSearching) return;

			foreach (var item in SearchResults.Value)
			{
				if (item.MemberId == employeeNo)
				{
					item.IsChecked = isOn;
					break;
				}
			}
		}

		private void SetSearchEmployeeAllCheck(bool isOn)
		{
			if (!IsSearching) return;

			foreach (var item in SearchResults.Value)
				item.IsChecked = isOn;
		}
	}
}
