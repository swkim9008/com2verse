/*===============================================================
* Product:		Com2Verse
* File Name:	MiceAppViewModel_Ticket.cs
* Developer:	klizzard
* Date:			2023-07-17 15:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Mice;
using Cysharp.Text;

namespace Com2Verse.UI
{
	public partial class MiceAppViewModel //Ticket
	{
		partial void InitTicketOptions();
		partial void RegisterTicketOptionsListener();
		partial void UnregisterTicketOptionsListener();

#region Variables
		private Collection<MiceAppPackageListItemViewModel> _eventCollection = new();
		private MiceAppPackageDetailViewModel               _eventDetailViewModel;
#endregion

#region Properties
		public bool IsTicketViewOn     => IsTicketListView | IsTicketDetailView;
		public bool IsTicketListView   => ViewMode is eViewMode.TICKET_LIST;
		public bool IsTicketDetailView => ViewMode is eViewMode.TICKET_DETAIL;

		public Collection<MiceAppPackageListItemViewModel> EventCollection
		{
			get => _eventCollection;
			set => SetProperty(ref _eventCollection, value);
		}

		public MiceAppPackageDetailViewModel EventDetailViewModel
		{
			get => _eventDetailViewModel;
			set => SetProperty(ref _eventDetailViewModel, value);
		}

		public string EventCollectionCount => Data.Localization.eKey.MICE_UI_Mobile_MP_MyTicket_Ticket_PageTitle_SumList
		                                          .ToLocalizationString(EventCollection.CollectionCount);
#endregion

		partial void InitTicketView()
		{
			EventDetailViewModel = new MiceAppPackageDetailViewModel();
			NestedViewModels.Add(EventDetailViewModel);
		}

		partial void InvokeTicketView()
		{
			if (IsTicketViewOn)
			{
				UpdateTicketView();
				RegisterTicketOptionsListener();
			}
			else
			{
				UnregisterTicketOptionsListener();
			}

			InvokePropertyValueChanged(nameof(IsTicketViewOn),     IsTicketViewOn);
			InvokePropertyValueChanged(nameof(IsTicketListView),   IsTicketListView);
			InvokePropertyValueChanged(nameof(IsTicketDetailView), IsTicketDetailView);
		}

		private void UpdateTicketView()
		{
			EventCollection.Reset();

			var packages = _selectEventSortOptionType.GetOrderedPackages();
			foreach (var elem in packages)
			{
				if (!_selectEventScheduleOptionType.IsVisible(elem))
					continue;

				var viewModel = new MiceAppPackageListItemViewModel(elem);
				viewModel.OnShowPackageDetailView += OnShowPackageDetailView;

				EventCollection.AddItem(viewModel);
			}

			InvokePropertyValueChanged(nameof(EventCollection),      EventCollection);
			InvokePropertyValueChanged(nameof(EventCollectionCount), EventCollectionCount);
		}

		private void OnShowPackageDetailView(string packageId)
		{
			if (MiceInfoManager.Instance.MyUserInfo.PackageInfos.TryGetValue(packageId, out var packageInfo))
			{
				EventDetailViewModel.SetData(packageInfo);
				SetViewMode(eViewMode.TICKET_DETAIL);

				MiceInfoManager.Instance.MyPackageClickedPrefs.Click(packageInfo.PackageId);

				Mice.RedDotManager.SetTrigger(Mice.RedDotManager.RedDotData.TriggerKey.ShowMyPackage);
			}
		}
	}

	[ViewModelGroup("Mice")]
	public partial class MiceAppPackageListItemViewModel : ViewModelBase
	{
#region Variables
		private UIInfo _uiInfoProgramType;

		private string _packageId;
		private string _eventTitle;
		private string _packageType;
		private string _scheduleInSessions;

		private Action<string> _onShowPackageDetailView;

		private WeakReference<MiceWebClient.Entities.UserPackageInfo> _packageInfo;
#endregion

#region Properties
		public UIInfo UIInfoProgramType
		{
			get
			{
				if (_uiInfoProgramType == null)
					InitializeUIInfoProgramType();
				return _uiInfoProgramType;
			}
		}

		public string EventTitle
		{
			get => _eventTitle;
			set => SetProperty(ref _eventTitle, value);
		}

		public string PackageType
		{
			get => _packageType;
			set => SetProperty(ref _packageType, value);
		}

		public string ScheduleInSessions
		{
			get => _scheduleInSessions;
			set => SetProperty(ref _scheduleInSessions, value);
		}

		public CommandHandler ShowPackageDetailView { get; }

		public event Action<string> OnShowPackageDetailView
		{
			add
			{
				_onShowPackageDetailView -= value;
				_onShowPackageDetailView += value;
			}
			remove => _onShowPackageDetailView -= value;
		}

		public MiceWebClient.Entities.UserPackageInfo PackageInfo
		{
			get
			{
				if (_packageInfo != null && _packageInfo.TryGetTarget(out var target))
					return target;

				if (!MiceInfoManager.Instance.MyUserInfo.PackageInfos.TryGetValue(_packageId, out target))
					return null;

				PackageInfo = target;
				return target;
			}
			set
			{
				if (_packageInfo != null) _packageInfo.SetTarget(value);
				else _packageInfo = new WeakReference<MiceWebClient.Entities.UserPackageInfo>(value);
			}
		}

		public bool IsActiveRedDot => MiceInfoManager.Instance.MyPackageClickedPrefs.IsNew(_packageId);
#endregion

#region Intialize
		public MiceAppPackageListItemViewModel()
		{
			ShowPackageDetailView = new CommandHandler(() => { _onShowPackageDetailView?.Invoke(_packageId); });
		}

		public MiceAppPackageListItemViewModel(MiceWebClient.Entities.UserPackageInfo packageInfo) : this()
		{
			SetData(packageInfo);
		}

		public virtual void SetData(MiceWebClient.Entities.UserPackageInfo packageInfo)
		{
			_packageId = packageInfo.PackageId;

			// 행사 타이틀.
			EventTitle = packageInfo.EventInfo != null ? packageInfo.EventInfo.StrTitle : packageInfo.EventName;

			// 티켓 구분.
			PackageType = packageInfo.PackageType.ToLocalizationString();

			// 세션 일정.
			{
				DateTime startDateTime = packageInfo.StartDateTimeInSessions;
				DateTime endDateTime   = packageInfo.EndDateTimeInSessions;

				ScheduleInSessions = ZString.Format("{0} ~ {1}",
				                                    startDateTime.Equals(default) ? string.Empty : startDateTime.ToString(),
				                                    endDateTime.Equals(default) ? string.Empty : endDateTime.ToString());
			}

			PackageInfo = packageInfo;
		}

		private void InitializeUIInfoProgramType()
		{
			var titleString = PackageInfo.PackageType.ToLocalizationString();
			var messageString = PackageInfo.PackageType switch
			{
				MiceWebClient.eMicePackageType.ENTRANCE_ALL_IN_EVENT => Data.Localization.eKey
				                                                            .MICE_UI_Mobile_MP_MyTicket_Ticket_InfoPopup_Desc_FreePass.ToLocalizationString(),
				MiceWebClient.eMicePackageType.ENTRANCE_ALL_IN_PROGRAM => Data.Localization.eKey
				                                                              .MICE_UI_Mobile_MP_MyTicket_Ticket_InfoPopup_Desc_ProgramPass.ToLocalizationString(),
				MiceWebClient.eMicePackageType.ENTRANCE_TO_SESSION => Data.Localization.eKey
				                                                          .MICE_UI_Mobile_MP_MyTicket_Ticket_InfoPopup_Desc_Special3.ToLocalizationString(),
				_ => string.Empty
			};

			_uiInfoProgramType = new UIInfo(true);
			_uiInfoProgramType.Set(UIInfo.eInfoType.INFO_TYPE_MICE_PROGRAM_TYPE, UIInfo.eInfoLayout.INFO_LAYOUT_UP,
			                       titleString, messageString);
		}
#endregion
	}

	[ViewModelGroup("Mice")]
	public partial class MiceAppPackageDetailViewModel : MiceAppPackageListItemViewModel
	{
		partial        void UpdateProgramOptions();
		public partial void RegisterProgramOptionsListener();
		public partial void UnregisterProgramOptionsListener();

#region Variables
		private Collection<MiceAppPackageSessionListItemViewModel> _sessionCollection = new();
#endregion

#region Properties
		public bool IsVisibleDropdown => PackageInfo is
			{ PackageType: MiceWebClient.eMicePackageType.ENTRANCE_ALL_IN_EVENT };

		public Collection<MiceAppPackageSessionListItemViewModel> SessionCollection
		{
			get => _sessionCollection;
			set => SetProperty(ref _sessionCollection, value);
		}
#endregion

#region Intialize
		public MiceAppPackageDetailViewModel() : base() { }

		public MiceAppPackageDetailViewModel(MiceWebClient.Entities.UserPackageInfo packageInfo) : base(packageInfo) { }

		public override void SetData(MiceWebClient.Entities.UserPackageInfo packageInfo)
		{
			base.SetData(packageInfo);

			UpdatePackageDetailView(true);
		}

		private void UpdatePackageDetailView(bool isUpdateOptions)
		{
			SessionCollection.Reset();

			// 프로그램 옵션을 초기화 한 후 표시 여부를 체크해야 한다. 
			if (isUpdateOptions)
				UpdateProgramOptions();

			if (PackageInfo is { TicketInfoList: { } })
			{
				foreach (var elem in PackageInfo.TicketInfoList)
				{
					if (!IsVisibleSessionItem(elem)) continue;

					var viewModel = new MiceAppPackageSessionListItemViewModel(elem);
					SessionCollection.AddItem(viewModel);
				}
			}

			InvokePropertyValueChanged(nameof(SessionCollection), SessionCollection);
			InvokePropertyValueChanged(nameof(IsVisibleDropdown), IsVisibleDropdown);
		}
#endregion
	}

	[ViewModelGroup("Mice")]
	public partial class MiceAppPackageSessionListItemViewModel : ViewModelBase
	{
#region Variables
		private string _programTitle;
		private string _sessionTitle;
		private string _sessionSchedule;
#endregion

#region Properties
		public string ProgramTitle
		{
			get => _programTitle;
			set => SetProperty(ref _programTitle, value);
		}

		public string SessionTitle
		{
			get => _sessionTitle;
			set => SetProperty(ref _sessionTitle, value);
		}

		public string SessionSchedule
		{
			get => _sessionSchedule;
			set => SetProperty(ref _sessionSchedule, value);
		}
#endregion

#region Intialize
		public MiceAppPackageSessionListItemViewModel(MiceWebClient.Entities.TicketInfoDetailEntity entity)
		{
			var programInfo = entity!.ProgramInfo;
			var sessionInfo = entity.SessionInfo;

			ProgramTitle = programInfo!.StrTitle;
			SessionTitle = sessionInfo!.StrName;
			SessionSchedule = ZString.Format("{0} ~ {1}",
			                                 sessionInfo.StartDatetime.ToString(),
			                                 sessionInfo.EndDatetime.ToString());
		}
#endregion
	}
}
