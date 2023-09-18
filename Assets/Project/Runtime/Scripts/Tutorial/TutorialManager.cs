/*===============================================================
* Product:		Com2Verse
* File Name:	TutorialManager.cs
* Developer:	ydh
* Date:			2023-04-07 10:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Com2Verse.Data;
using Com2Verse.EventTrigger;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Localization = Com2Verse.UI.Localization;

namespace Com2Verse.Tutorial
{
	public sealed partial class TutorialManager : Singleton<TutorialManager>, ILocalizationUI, IDisposable
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private TutorialManager() { }

#region Variable
		private readonly string TUTORIAL_POPUP = "UI_Tutorial";

		private TutorialController _controller;
		private GUIView _tutorialView;
		private TutorialViewModel _viewModel;
		private string _nowStepDesc;
		private int _nowGroupId = 0;
		private long _nowStepId = 0;
		private bool _open;
		private bool _resetTokenCancel;
		private TableTutorialStep _tutorialStepTable;
		private TableTutorialInfo _tutorialInfoTable;
		public TableTutorialInfo TutorialInfoTable => _tutorialInfoTable;

		private List<KeyValuePair<eTutorialGroup, TutorialInfo>> _tutorialTriggerZoneTable;
		private List<KeyValuePair<eTutorialGroup, TutorialInfo>> _tutorialTriggerSpaceTable;
		private List<KeyValuePair<eTutorialGroup, TutorialInfo>> _tutorialInteractionObjectCheck;
		
		private CancellationTokenSource _cancellationToken;
		private CancellationTokenSource _resetCancellationToken;
		
		public static bool AniStopAndGo;
		public bool IsInitializing { get; private set; }
		public bool IsInitialized  { get; private set; }
		private TutorialRun _tutorialRun;
		private TutorialRemindViewModel _tutorialRemindViewModel;
		private GUIView _tutorialRemindGUIView;
#endregion Variable

#region TableData
		private void LoadTable()
		{
			_tutorialStepTable = TableDataManager.Instance.Get<TableTutorialStep>();

			SortList();
			
			_tutorialTriggerZoneTable = _tutorialInfoTable.Datas.Where(data => data.Value.EventTriggerType.Equals(eEventTriggerType.ENTER_ZONE)).Select(data => data).ToList();
			_tutorialTriggerSpaceTable = _tutorialInfoTable.Datas.Where(data => data.Value.EventTriggerType.Equals(eEventTriggerType.ENTER_SPACE)).Select(data => data).ToList();
			_tutorialInteractionObjectCheck = _tutorialInfoTable.Datas.Where(data => data.Value.EventTriggerType.Equals(eEventTriggerType.INTERACTION_OBJECT)).Select(data => data).ToList();
		}

		private bool IsEnglish(char alphabet)
		{
			if ((alphabet >= 'a' && alphabet <= 'z') || (alphabet >= 'A' && alphabet <= 'Z'))
				return true;
			
			return false;
		}

		private string GetSortKey(string input)
		{
			int asciiSum = input.Select(c => (int)c).Sum();
			return asciiSum.ToString("000") + input;
		}
#endregion TableData

#region Initialize
		public void Initialize()
		{
			if (IsInitialized || IsInitializing)
				return;
					
			IsInitializing = true;
			{
				LoadTable();
			}

			_controller ??= new TutorialController();
					
			_controller.OnShowBotPopUp += OnShowBotPopUp;
			_controller.SetChatBotDesc += SetDesc;
			_controller.PlayBefore += PlayBefore;
			_controller.PlayAfter += PlayAfter;

			_cancellationToken ??= new CancellationTokenSource();
			_resetCancellationToken ??= new CancellationTokenSource();

			_controller.Initialize();

			TriggerEventManager.Instance.OnZoneAction = TutorialZoneCheck;
					
			(this as ILocalizationUI).InitializeLocalization();
			
			IsInitialized  = true;
			IsInitializing = false;
		}
		
#endregion Initialize
		public void Dispose()
		{
			_controller = null;
			_tutorialView = null;
			_viewModel = null;
			_tutorialStepTable = null;
			_tutorialInfoTable = null;
			
			if (_cancellationToken != null)
			{
				_cancellationToken.Cancel();
				_cancellationToken.Dispose();
				_cancellationToken = null;
			}
			
			if (_resetCancellationToken != null)
			{
				_resetCancellationToken.Cancel();
				_resetCancellationToken.Dispose();
				_resetCancellationToken = null;
			}

			_onTutorialOpened = null;
			_onTutorialClosed = null;
		}

		public void OnClickNextBtn()
		{
			Reset();
			_controller.NextBtnClick = true;
		}

		public void OnClickPrevBtn()
		{
			Reset();
			_controller.PrevBtnClick = true;
		}

		private void Reset() => _resetTokenCancel = true;

		private void ControllerReset()
		{
			_controller.NextBtnClick = false;
			_controller.PrevBtnClick = false;
			_controller.ClickCloseBtn = false;

			if (_resetTokenCancel)
			{
				_resetCancellationToken.Cancel();
				_resetCancellationToken = new CancellationTokenSource();
			}

			_cancellationToken.Cancel();
			_cancellationToken = new CancellationTokenSource();
			
			_resetTokenCancel = false;
		}
		
		public void TutorialRemindPopUpOpen()
		{
			UIManager.Instance.CreatePopup("UI_Tutorial_Remind", view =>
			{
				_tutorialRemindGUIView = view;
				_tutorialRemindGUIView.Show();
				_tutorialRemindViewModel = view.ViewModelContainer.GetViewModel<TutorialRemindViewModel>();
				_tutorialRemindViewModel.ExitAction = () => { view.Hide(); };
				_tutorialRemindViewModel.TutorialRemindItemCollection.Reset();
				_tutorialRemindViewModel.ChangeLanguage = SortList;
				foreach (var data in _tutorialInfoTable.Datas)
				{
					if(CheckTutorialRemindHave(Localization.Instance.GetTutorialString(data.Value.TutorialTitle)))
						continue;
					
					var item = new TutorialRemindItemViewModel();
					item.TutorialGroup = data.Key;
					item.Desc = Localization.Instance.GetTutorialString(data.Value.TutorialTitle);
					_tutorialRemindViewModel.TutorialRemindItemCollection.AddItem(item);
				}
			}).Forget();
		}

		private void SortList()
		{
			var tutorialInfo = TableDataManager.Instance.Get<TableTutorialInfo>();
			List<TutorialInfo> tutorialInfoList = new List<TutorialInfo>();
			List<TutorialInfo> englishTitle = new List<TutorialInfo>();

			foreach (var data in tutorialInfo.Datas.Values)
			{
				if(!IsEnglish(Localization.Instance.GetTutorialString(data.TutorialTitle)[0]))
				{
					tutorialInfoList.Add(data);
				}
				else
				{
					englishTitle.Add(data);
				}
			}
			
			var sortLocalList = tutorialInfoList.OrderBy( x => Localization.Instance.GetTutorialString(x.TutorialTitle));
			var sortEnglishList= englishTitle.OrderBy( x => Localization.Instance.GetTutorialString(x.TutorialTitle));
			tutorialInfoList = sortLocalList.Concat(sortEnglishList).ToList();
			
			if(_tutorialInfoTable == null)
				_tutorialInfoTable = new TableTutorialInfo();
			else
				_tutorialInfoTable.Datas.Clear();
			
			for (int i = 0; i < tutorialInfoList.Count; i++)
			{
				_tutorialInfoTable.Datas.Add(tutorialInfoList[i].GroupID, tutorialInfoList[i]);
			}
		}
		
		private bool CheckTutorialRemindHave(string str)
		{
			foreach (var data in _tutorialRemindViewModel.TutorialRemindItemCollection.Value)
			{
				if (data.Desc == str)
					return true;
			}
			return false;
		}
#region Event
		private Action _onTutorialOpened;
		private Action _onTutorialClosed;

		public event Action OnTutorialOpenedEvent
		{
			add
			{
				_onTutorialOpened -= value;
				_onTutorialOpened += value;
			}
			remove => _onTutorialOpened -= value;
		}
		
		public event Action OnTutorialClosedEvent
		{
			add
			{
				_onTutorialClosed -= value;
				_onTutorialClosed += value;
			}
			remove => _onTutorialClosed -= value;
		}
#endregion Event
	}
}