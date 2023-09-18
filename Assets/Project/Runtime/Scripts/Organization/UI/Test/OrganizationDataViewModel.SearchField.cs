/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationDataViewModel.SearchField.cs
* Developer:	jhkim
* Date:			2022-09-01 20:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Logger;
using Com2Verse.Organization;

namespace Com2Verse.UI
{
	public partial class OrganizationDataViewModel
	{
#region Variables
		private string _searchText;
		private string _text;
		private bool _isFocused;
		private bool _refreshDropdown;
		private Collection<OrganizationDataButtonListViewModel> _searchResultEmployee = new();
		private Collection<LabelSearchResultViewModel> _searchResults = new();
		private string _placeHolder;
		private int _selectedIndex;
		private int _submittedIndex;
		private int _hoveredIndex;
#endregion // Variables

#region Properties
		public string SearchText
		{
			get => _searchText;
			set
			{
				_searchText = value;
				Search(_searchText);
				InvokePropertyValueChanged(nameof(SearchText), value);
			}
		}

		public string Text
		{
			get => _text;
			set
			{
				_text = value;
				InvokePropertyValueChanged(nameof(Text), value);
			}
		}
		public bool IsFocused
		{
			get => _isFocused;
			set
			{
				_isFocused = value;
				if (value)
				{
					if (!string.IsNullOrWhiteSpace(_searchText))
						Search(_searchText);
				}
				InvokePropertyValueChanged(nameof(IsFocused), value);
			}
		}
		public bool RefreshDropdown
		{
			get => _refreshDropdown;
			set
			{
				_refreshDropdown = value;
				InvokePropertyValueChanged(nameof(RefreshDropdown), value);
			}
		}

		public Collection<OrganizationDataButtonListViewModel> SearchResultEmployee
		{
			get => _searchResultEmployee;
			set
			{
				_searchResultEmployee = value;
				InvokePropertyValueChanged(nameof(SearchResultEmployee), value);
			}
		}

		public Collection<LabelSearchResultViewModel> SearchResults
		{
			get => _searchResults;
			set
			{
				_searchResults = value;
				InvokePropertyValueChanged(nameof(SearchResults), value);
			}
		}
		public string PlaceHolder
		{
			get => _placeHolder;
			set
			{
				_placeHolder = value;
				InvokePropertyValueChanged(nameof(PlaceHolder), value);
			}
		}

		public int SelectedIndex
		{
			get => _selectedIndex;
			set
			{
				_selectedIndex = value;
				if (value < SearchResults.CollectionCount)
				{
					var employee = DataManager.Instance.GetMember(SearchResults.Value[value].ID);
					SetMemberInfo(employee);
				}

				InvokePropertyValueChanged(nameof(SelectedIndex), value);
			}
		}
		public int SubmittedIndex
		{
			get => _submittedIndex;
			set
			{
				_submittedIndex = value;
				C2VDebug.LogWarning($"Submitted Index = {value}");
				InvokePropertyValueChanged(nameof(SubmittedIndex), value);
			}
		}

		public int HoveredIndex
		{
			get => _hoveredIndex;
			set
			{
				_hoveredIndex = value;
				if (value < SearchResults.CollectionCount)
				{
					var memberModel = DataManager.Instance.GetMember(SearchResults.Value[value].ID);
					SetMemberInfo(memberModel);
				}
				InvokePropertyValueChanged(nameof(HoveredIndex), value);
			}
		}
#endregion // Properties

#region Initialization
		private void InitSearchField()
		{
			PlaceHolder = "직원 이름을 입력 해 주세요";
		}
#endregion // Initialization
		private void Search(string text)
		{
			var members = DataManager.Instance.FindMemberByName(text);
			SearchResults.Reset();
			foreach (var memberModel in members)
				SearchResults.AddItem(new LabelSearchResultViewModel
				{
					ID = memberModel.Member.AccountId,
					Label = memberModel.Member.MemberName,
					SearchText = text,
				});

			// SetList(employees);
		}

		void SetList(List<MemberModel> results)
		{
			SearchResultEmployee.Reset();
			foreach (var memberModel in results)
			{
				SearchResultEmployee.AddItem(new OrganizationDataButtonListViewModel(memberModel.Member.MemberName, () =>
				{
					SetMemberInfo(memberModel);
				}));
			}
		}
	}
}
