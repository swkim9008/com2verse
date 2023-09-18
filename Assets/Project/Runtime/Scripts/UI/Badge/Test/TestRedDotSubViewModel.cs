/*===============================================================
* Product:		Com2Verse
* File Name:	TestRedDotSubViewModel.cs
* Developer:	NGSG
* Date:			2023-04-27 15:15
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
	[ViewModelGroup("TestRedDot")]
	public sealed class TestRedDotSubViewModel : ViewModelBase
	{
		public GUIView CurrentView { get; set; }

		private bool _shirtToggle = true;
		public bool ShirtToggle
		{
			get => _shirtToggle;
			set
			{
				if (_shirtToggle != value)
				{
					_shirtToggle = value;
					InvokePropertyValueChanged(nameof(ShirtToggle), value);
					Debug.Log($"{typeof(TestRedDotSubViewModel)} - set ShirtToggle call {value}");
				}
			}
		}

		private bool _pantsToggle = false;
		public bool PantsToggle
		{
			get => _pantsToggle;
			set
			{
				if (_pantsToggle != value)
				{
					_pantsToggle = value;
					InvokePropertyValueChanged(nameof(PantsToggle), value);
					
					if(_pantsToggle == true)
						RedDotManager.Instance.RemoveBadgeType("MAIN_MENU_SUB1_TAB1_ITEM");
					Debug.Log($"{nameof(TestRedDotSubViewModel)} - set PantsToggle call {value}");
				}
			}
		}

		private Collection<TestRedDotCollectionViewModel> _shirtCollectionViewModel = new();
		public Collection<TestRedDotCollectionViewModel> ShirtCollection
		{
			get => _shirtCollectionViewModel;
			set
			{
				_shirtCollectionViewModel = value;
				InvokePropertyValueChanged(nameof(ShirtCollection), value);
			}
		}

		private Collection<TestRedDotCollectionViewModel> _pantsCollectionViewModel = new();
		public Collection<TestRedDotCollectionViewModel> PantsCollection
		{
			get => _pantsCollectionViewModel;
			set
			{
				_pantsCollectionViewModel = value;
				InvokePropertyValueChanged(nameof(PantsCollection), value);
			}
		}

		public CommandHandler onCloseClick       { get; private set; }
		public CommandHandler OnCreateShirtItems { get; private set; }
		public CommandHandler OnCreatePantsItem  { get; private set; }
		public CommandHandler OnDeleteShirtItem  { get; private set; }
		public CommandHandler OnDeletePantsItem  { get; private set; }

		public TestRedDotSubViewModel()
		{
			onCloseClick = new CommandHandler( ()=>
			{
				if (CurrentView)
				{
					CurrentView.Hide();
					UIManager.Instance.Destroy(CurrentView);

					RedDotManager.Instance.NotifyAll();
				}	
			});
			OnCreateShirtItems = new CommandHandler(CreateShirtItems);
			OnCreatePantsItem  = new CommandHandler(CreatePantsItem);

			OnDeleteShirtItem = new CommandHandler(DeleteShirtItems);
			OnDeletePantsItem = new CommandHandler(DeletePantsItem);
			
			CreateAllShirtItems();
		}

		private List<string> _dataTable = new List<string>()
		{
			"a", "b", "c", "d", "e", "f", "g", "h", "i", "j"
		};

		private int _shirtId = 0;
		private int _pantsId = 0;
		private int _delShirtIndex = 0;
		private int _delPantsIndex = 0;
		
		private void DeleteShirtItems()
		{
			foreach (var item in ShirtCollection.Value)
			{
				if (item.Info._data == _dataTable[_delShirtIndex])
				{
					_delShirtIndex++;
					// 아이템 삭제
					ShirtCollection.RemoveItem(item);
					
					// 배찌 정보 삭제
					RedDotManager.Instance.RemoveBadge(new RedDotData("MAIN_MENU_SUB1_TAB1_ITEM", item.RedDot.Id));
					break;
				}
			}
		}

		private void DeletePantsItem()
		{
			foreach (var item in PantsCollection.Value)
			{
				if (item.Info._data == _dataTable[_delPantsIndex])
				{
					_delPantsIndex++;
					// 아이템 삭제
					PantsCollection.RemoveItem(item);

					// 배찌 정보 삭제
					RedDotManager.Instance.RemoveBadge(new RedDotData("MAIN_MENU_SUB1_TAB2_ITEM", item.RedDot.Id));
					break;
				}
			}
		}
		
		private void CreateAllShirtItems()
		{
			for (int i = 0; i < 10; i++)
			{
				RedDotCollectionTestData info = new RedDotCollectionTestData(_shirtId++, _dataTable[i]);
				TestRedDotMainViewModel.ShirtInfoList.Add(info);

				// 배찌 데이타 추가로 넘김
				RedDotData rdd = new RedDotData("MAIN_MENU_SUB1_TAB1_ITEM", info._id);
				var shirtItemViewModel = new TestRedDotCollectionViewModel(info, rdd, OnShirtItemSelected);
				
				_shirtCollectionViewModel.AddItem(shirtItemViewModel);
			}

			// 내부 컬랙션 아이템들이 모두 생성될때까지 기다린후 배찌 처리한다
			WaitAllCreate().Forget();
		}

		private async UniTask WaitAllCreate()
		{
			await UniTask.WaitUntil( ()=> ShirtCollection.Value.All(i => i.IsCreate == true));
			RedDotManager.Instance.Notify("MAIN_MENU_SUB1_TAB1_ITEM");
		}

		private void CreateShirtItems()
		{
			int index = _shirtCollectionViewModel.CollectionCount;
			
			RedDotCollectionTestData info = new RedDotCollectionTestData(_shirtId++, _dataTable[index % _dataTable.Count]);
			TestRedDotMainViewModel.ShirtInfoList.Add(info);
			
			RedDotData rdd = new RedDotData("MAIN_MENU_SUB1_TAB1_ITEM", info._id);
			var shirtItemViewModel = new TestRedDotCollectionViewModel(info, rdd, OnShirtItemSelected);
			
			_shirtCollectionViewModel.AddItem(shirtItemViewModel);

			RedDotManager.Instance.Notify("MAIN_MENU_SUB1_TAB1_ITEM");
		}
		private void OnShirtItemSelected(TestRedDotCollectionViewModel selectedColorViewModel)
		{
			foreach (var shirtCollectionViewModel in ShirtCollection.Value)
			{
				if (selectedColorViewModel == shirtCollectionViewModel)
					continue;

				shirtCollectionViewModel.Selected = false;
			}
			
			if(selectedColorViewModel.RedDot != null)
				RedDotManager.Instance.RemoveBadge(new RedDotData("MAIN_MENU_SUB1_TAB1_ITEM", selectedColorViewModel.RedDot.Id));
		}

		private void CreatePantsItem()
		{
			int index = _pantsCollectionViewModel.CollectionCount;

			RedDotCollectionTestData info = new RedDotCollectionTestData(_pantsId++, _dataTable[index % _dataTable.Count]);
			TestRedDotMainViewModel.PantsInfoList.Add(info);
			
			RedDotData rdd = new RedDotData("MAIN_MENU_SUB1_TAB2_ITEM", info._id);
			var pantsItemViewModel = new TestRedDotCollectionViewModel(info, rdd, OnPantsItemSelected);
			_pantsCollectionViewModel.AddItem(pantsItemViewModel);

			RedDotManager.Instance.Notify("MAIN_MENU_SUB1_TAB2_ITEM");
		}

		private void OnPantsItemSelected(TestRedDotCollectionViewModel selectedColorViewModel)
		{
			foreach (var pantsCollectionViewModel in PantsCollection.Value)
			{
				if (selectedColorViewModel == pantsCollectionViewModel)
					continue;

				pantsCollectionViewModel.Selected = false;
			}

			RedDotManager.Instance.RemoveBadge(new RedDotData("MAIN_MENU_SUB1_TAB2_ITEM", selectedColorViewModel.RedDot.Id));
		}
	}
}
