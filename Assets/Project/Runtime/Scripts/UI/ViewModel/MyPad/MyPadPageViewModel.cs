/*===============================================================
* Product:		Com2Verse
* File Name:	MyPadPageViewModel.cs
* Developer:	tlghks1009
* Date:			2022-08-23 11:50
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.Option;

namespace Com2Verse.UI
{
	[ViewModelGroup("MyPad")]
	public sealed class MyPadPageViewModel : ViewModelBase
	{
		private readonly Action<string> _onItemClickedAction;
		private readonly Action<string> _onItemRemovedAction;

		private const int MaxAdvertisementCount = 3;
		private bool _advertisementPageActive;
		private Collection<MyPadItemViewModel> _myPadItemCollection = new();
		private Collection<MyPadNoticeViewModel> _myPadNoticeCollection = new();

		public MyPadPageViewModel(int currentPage, List<string> deletedApp, List<MyPad> origin, int startIndex, int endIndex, Action<string> onClick, Action<string> onRemove)
		{
			AdvertisementPageActive = (currentPage == 1);
			if (AdvertisementPageActive)
			{
				for (int i = 0; i < MaxAdvertisementCount; i++)
				{
					_myPadNoticeCollection.AddItem(new MyPadNoticeViewModel());
				}
			}
			
			_onItemClickedAction = onClick;
			_onItemRemovedAction = onRemove;

			for (int itemIndex = startIndex; itemIndex < endIndex; itemIndex++)
			{
				if (itemIndex >= origin.Count)
					return;
			
				if (deletedApp.Contains(origin[itemIndex].AppID))
					continue;

				if (origin[itemIndex].Number < 0)
					continue;
				
				var myPadItem = origin[itemIndex];
				var myPadItemViewModel = new MyPadItemViewModel(ClickMyPadItemEvent, RemoveMyPadItemEvent)
				{
					Id = myPadItem.AppID,
					Index = myPadItem.Number,
					Type = myPadItem.Type,
					Name = myPadItem.Name,
					IconName = myPadItem.IconRes,
					IsDelete = myPadItem.IsDelete,
					EnableApp = true
				};
				// myPadItemViewModel.RefreshMyPadItem();
				_myPadItemCollection.AddItem(myPadItemViewModel);
			}
		}

#region Property
		public bool AdvertisementPageActive
		{
			get => _advertisementPageActive;
			set => SetProperty(ref _advertisementPageActive, value);
		}

		public Collection<MyPadItemViewModel> MyPadItemCollection
		{
			get => _myPadItemCollection;
			set => SetProperty(ref _myPadItemCollection, value);
		}
		
		public Collection<MyPadNoticeViewModel> MyPadNoticeCollection
		{
			get => _myPadNoticeCollection;
			set => SetProperty(ref _myPadNoticeCollection, value);
		}
#endregion // Property

#region Command
		private void ClickMyPadItemEvent(string id)
		{
			_onItemClickedAction?.Invoke(id);
		}
				
		private void RemoveMyPadItemEvent(string id)
		{
			_onItemRemovedAction?.Invoke(id);
		}

		public void AddMyPadItem()
		{ 
			
		}
		
		public void RefreshMyPadItem()
		{
			foreach (var viewModel in _myPadItemCollection.Value)
				viewModel.RefreshMyPadItem();
		}
		
		public void RemoveMyPadItem(string id)
		{
			var item = _myPadItemCollection.Value.FirstOrDefault(target => target.Id == id);
			if (item != null) _myPadItemCollection.RemoveItem(item);
		}
		
		public void ResetMyPadItem() => _myPadItemCollection.Reset();

		public void RefreshMyPadNotice()
		{
			// FIXME : 이미지만 출력시키는것으로 임시 처리. 추후 원복 예정
			if (!AdvertisementPageActive) return;

			var target = new List<string>
			{
				"UI_Ad_MyPad_MinigameParty",
				"UI_Ad_MyPad_SummonersWar",
				"UI_Ad_MyPad_Zenonia",
				"UI_Ad_MyPad_Walkerhill"
			};

			Random random = new Random();
			for (int i = 0; i < _myPadNoticeCollection.Value.Count; i++)
			{
				if (target.Count <= 0) break;

				var myPadNotice = _myPadNoticeCollection.Value[i];
				if (i == 0)
				{
					myPadNotice.SetDummyTexture("UI_Ad_MyPad_Spaxe", true);
					C2VDebug.LogCategory("MyPad", $"{nameof(RefreshMyPadNotice)} : UI_Ad_MyPad_Spaxe");
				}
				else
				{
					var randomValue = random.Next(0, target.Count);
					myPadNotice.SetDummyTexture(target[randomValue], false);
					C2VDebug.LogCategory("MyPad", $"{nameof(RefreshMyPadNotice)} : {target[randomValue]}");
					target.RemoveAt(randomValue);
				}
			}
		}

		private void RefreshMyPadNoticeFixMe()
		{
			if (!AdvertisementPageActive) return;
			
			var language = OptionController.Instance.GetOption<LanguageOption>().LanguageIndex;
			var target = MyPadManager.Instance.GetAdvertisement(language);
			if (target == null || target.Count <= 0) return;
			
			Random random = new Random();
			for (int i = 0; i < _myPadNoticeCollection.Value.Count; i++)
			{
				if (target.Count <= 0) break;
				
				var myPadNotice = _myPadNoticeCollection.Value[i];
				if (target.Count < MaxAdvertisementCount && i == 0)
				{
					myPadNotice.SetAdvertisementRequestPage();
					C2VDebug.LogCategory("MyPad", $"{nameof(RefreshMyPadNotice)} : RequestAdvertisePage");
				}
				else
				{
					var randomValue = random.Next(0, target.Count);
					myPadNotice.Target = target[randomValue];
					target.RemoveAt(randomValue);
					C2VDebug.LogCategory("MyPad", $"{nameof(RefreshMyPadNotice)} : {myPadNotice.Target.DispalyContents}");
				}
			}
		}
#endregion // Command
	}
}
