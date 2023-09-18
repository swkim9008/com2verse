/*===============================================================
* Product:		Com2Verse
* File Name:	MiceBusinessCardBookViewModel_CardList.cs
* Developer:	wlemon
* Date:			2023-04-06 12:55
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using Com2Verse.Mice;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
	public partial class MiceBusinessCardBookViewModel
	{
		public enum eSortFunction
		{
			ALL_NAME,
			ALL_LATEST,
			EXCHANGED_NAME,
			EXCHANGED_LATEST,
			RECEIVE_NAME,
			RECEIVE_LATEST,
		}

#region Variables
		private List<MiceBusinessCardListItemViewModel>       _totalCardList = new();
		private Collection<MiceBusinessCardListItemViewModel> _cardList      = new();
		private int                                       _cardCount;
		private bool                                      _isCardListEmpty;
		private bool                                      _isCardListEditMode;
		private eSortFunction                             _sortFunction;
		private int                                       _selectedCardCount;
		private bool                                      _isCardSelectAllAvailable;
		private bool                                      _isCardUnselectAllAvailable;
		private List<MiceUserInfo>                        _userInfoList;
		private List<string>                              _sortFunctionList;

		public CommandHandler EditCardList        { get; private set; }
		public CommandHandler CancelEditCardList  { get; private set; }
		public CommandHandler RemoveSelectedCards { get; private set; }
		public CommandHandler SelectAllCard       { get; private set; }
		public CommandHandler UnselectAllCard     { get; private set; }
#endregion

#region Properties
		public Collection<MiceBusinessCardListItemViewModel> CardList
		{
			get => _cardList;
			set => SetProperty(ref _cardList, value);
		}

		public bool IsCardListEmpty
		{
			get => _isCardListEmpty;
			set => SetProperty(ref _isCardListEmpty, value);
		}

		public int CardCount
		{
			get => _cardCount;
			set => SetProperty(ref _cardCount, value);
		}

		public bool IsCardListEditMode
		{
			get => _isCardListEditMode;
			set
			{
				SetProperty(ref _isCardListEditMode, value);
				IsCardSelectAllAvailable   = SelectedCardCount == 0 && IsCardListEditMode;
				IsCardUnselectAllAvailable = SelectedCardCount >= 1 && IsCardListEditMode;
			}
		}

		public eSortFunction SortFunction
		{
			get => _sortFunction;
			set
			{
				_sortFunction = value;
				RefreshCardList();
				InvokePropertyValueChanged(nameof(SortFunction), SortFunction);
			}
		}

		public List<string> SortFunctionList
		{
			get => _sortFunctionList;
			set => SetProperty(ref _sortFunctionList, value);
		}

		public int SelectedCardCount
		{
			get => _selectedCardCount;
			set
			{
				SetProperty(ref _selectedCardCount, value);
				IsCardSelectAllAvailable   = SelectedCardCount == 0 && IsCardListEditMode;
				IsCardUnselectAllAvailable = SelectedCardCount >= 1 && IsCardListEditMode;
			}
		}

		public bool IsCardSelectAllAvailable
		{
			get => _isCardSelectAllAvailable;
			set => SetProperty(ref _isCardSelectAllAvailable, value);
		}

		public bool IsCardUnselectAllAvailable
		{
			get => _isCardUnselectAllAvailable;
			set => SetProperty(ref _isCardUnselectAllAvailable, value);
		}
#endregion

#region Initialize
		public void InitializeUserList()
		{
			EditCardList        = new CommandHandler(OnEditCardList);
			CancelEditCardList  = new CommandHandler(OnCancelEditCardList);
			SelectAllCard       = new CommandHandler(OnSelectAllCard);
			UnselectAllCard     = new CommandHandler(OnUnselectAllCard);
			RemoveSelectedCards = new CommandHandler(OnRemoveSelectedCardList);

			RefreshSortList();
		}

		public void SetUserList(List<MiceUserInfo> userInfoList)
		{
			OnCancelEditCardList();

			_userInfoList = userInfoList;
			if (_userInfoList != null)
			{
				_totalCardList.Clear();
				foreach (var userInfo in _userInfoList)
				{
					_totalCardList.Add(new MiceBusinessCardListItemViewModel(userInfo, OnClickCardListItem, OnClickShowCard));
				}
			}

			RefreshCardList();
		}
#endregion

#region Binding Events
		private void OnEditCardList()
		{
			OnUnselectAllCard();
			IsCardListEditMode = true;
			IsCardViewVisible  = false;
		}

		private void OnCancelEditCardList()
		{
			OnUnselectAllCard();
			IsCardListEditMode = false;
			IsCardViewVisible  = false;
		}

		private void OnRemoveSelectedCardList()
		{
			UIManager.Instance.ShowPopupYesNo(Data.Localization.eKey.MICE_UI_BC_Btn_DeletBC.ToLocalizationString(),
			                                  Data.Localization.eKey.MICE_UI_BC_Popup_Desc_DeleteBC.ToLocalizationString(),
			                                  (_) =>
			                                  {
				                                  IsCardViewVisible = false;
				                                  RemoveSelectedCardList().Forget();
			                                  });
		}

		private async UniTask RemoveSelectedCardList()
		{
			var accountIdList =
				from card in CardList.Value
				where card.IsSelected
				select card.UserInfo.AccountId;

			if (accountIdList.Count() == 0) return;
			await MiceInfoManager.Instance.RemoveCardList(accountIdList);

			var cardList =
			(
				from card in _totalCardList
				where _userInfoList.Contains(card.UserInfo)
				select card).ToArray();

			_totalCardList.Clear();
			_totalCardList.AddRange(cardList);
			RefreshCardList();
		}

		private void OnSelectAllCard()
		{
			foreach (var card in CardList.Value)
			{
				card.IsSelected = true;
			}

			SelectedCardCount = CardList.CollectionCount;
		}

		private void OnUnselectAllCard()
		{
			foreach (var card in CardList.Value)
			{
				card.IsSelected = false;
			}

			SelectedCardCount = 0;
		}

		private void OnClickCardListItem(MiceBusinessCardListItemViewModel viewModel)
		{
			if (!IsCardListEditMode) return;
			viewModel.IsSelected = !viewModel.IsSelected;
			if (viewModel.IsSelected)
				SelectedCardCount = SelectedCardCount + 1;
			else
				SelectedCardCount = SelectedCardCount - 1;
		}

		private void OnClickShowCard(MiceBusinessCardListItemViewModel viewModel)
		{
			IsCardViewVisible = true;
			_miceBusinessCardViewModel.Set(viewModel.UserInfo);
		}

		private void RefreshCardList()
		{
			if (_totalCardList == null) return;
			OnUnselectAllCard();

			var result =
				from card in _totalCardList
				where SortFunction switch
				{
					eSortFunction.EXCHANGED_NAME   => card.UserInfo.IsExchanged,
					eSortFunction.EXCHANGED_LATEST => card.UserInfo.IsExchanged,
					eSortFunction.RECEIVE_NAME     => !card.UserInfo.IsExchanged,
					eSortFunction.RECEIVE_LATEST   => !card.UserInfo.IsExchanged,
					_                              => true
				}
				orderby SortFunction switch
				{
					eSortFunction.ALL_NAME       => card.UserInfo.Name,
					eSortFunction.EXCHANGED_NAME => card.UserInfo.Name,
					eSortFunction.RECEIVE_NAME   => card.UserInfo.Name,
					_                            => string.Empty
				}
				select card;

			CardList.Reset();
			CardList.AddRange(result.ToList());
			CardCount       = CardList.CollectionCount;
			IsCardListEmpty = CardCount == 0;
		}

		private void RefreshSortList()
		{
			var sortFunctionList = new List<string>();
			sortFunctionList.Add(Data.Localization.eKey.MICE_UI_BC_Title_Filter_AllName.ToLocalizationString());
			sortFunctionList.Add(Data.Localization.eKey.MICE_UI_BC_Title_Filter_AllDate.ToLocalizationString());
			sortFunctionList.Add(Data.Localization.eKey.MICE_UI_BC_Title_Filter_OnlyExchangeName.ToLocalizationString());
			sortFunctionList.Add(Data.Localization.eKey.MICE_UI_BC_Title_Filter_OnlyExchangeDate.ToLocalizationString());
			sortFunctionList.Add(Data.Localization.eKey.MICE_UI_BC_Title_Filter_OnlyReceivedName.ToLocalizationString());
			sortFunctionList.Add(Data.Localization.eKey.MICE_UI_BC_Title_Filter_OnlyReceivedDate.ToLocalizationString());
			SortFunctionList = sortFunctionList;
		}
		
		public override void OnLanguageChanged()
		{
			base.OnLanguageChanged();
			RefreshSortList();
		}
#endregion
	}
}
