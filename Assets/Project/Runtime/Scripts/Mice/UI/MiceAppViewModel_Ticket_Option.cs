/*===============================================================
* Product:		Com2Verse
* File Name:	MiceAppViewModel_Ticket_Option.cs
* Developer:	klizzard
* Date:			2023-07-27 14:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Mice;
using TMPro;

namespace Com2Verse.UI
{
	public partial class MiceAppViewModel //Ticket - Option
	{
		public enum EEventScheduleOptionType
		{
			Ongoing,
			Upcoming,
			Expired,
			Max,
		}

		public enum EEventSortOptionType
		{
			Recently,
			CloseToTerminate,
			Max,
		}

#region Variables
		private EEventScheduleOptionType _selectEventScheduleOptionType;
		private EEventSortOptionType     _selectEventSortOptionType;

		private List<TMP_Dropdown.OptionData> _eventScheduleOptions = new();
		private List<TMP_Dropdown.OptionData> _eventSortOptions     = new();

		private TMP_Dropdown.DropdownEvent _dropDownEventOfEventSchedule;
		private TMP_Dropdown.DropdownEvent _dropDownEventOfEventSort;
#endregion

#region Properties
		public int SelectEventScheduleOptionIndex => (int)_selectEventScheduleOptionType;
		public int SelectEventSortOptionIndex     => (int)_selectEventSortOptionType;

		public List<TMP_Dropdown.OptionData> EventScheduleOptions
		{
			get => _eventScheduleOptions;
			set => SetProperty(ref _eventScheduleOptions, value);
		}

		public List<TMP_Dropdown.OptionData> EventSortOptions
		{
			get => _eventSortOptions;
			set => SetProperty(ref _eventSortOptions, value);
		}

		public TMP_Dropdown.DropdownEvent DropDownEventOfEventSchedule
		{
			get => _dropDownEventOfEventSchedule;
			set => _dropDownEventOfEventSchedule = value;
		}

		public TMP_Dropdown.DropdownEvent DropDownEventOfEventSort
		{
			get => _dropDownEventOfEventSort;
			set => _dropDownEventOfEventSort = value;
		}
#endregion

		partial void InitTicketOptions()
		{
			_selectEventScheduleOptionType = EEventScheduleOptionType.Ongoing;
			_selectEventSortOptionType     = EEventSortOptionType.Recently;

			// 티켓 필터
			{
				EventScheduleOptions.Clear();
				for (int idx = 0; idx < (int)EEventScheduleOptionType.Max; ++idx)
				{
					var optionText = ((EEventScheduleOptionType)idx).ToOptionText();
					if (string.IsNullOrEmpty(optionText)) continue;

					EventScheduleOptions.Add(new TMP_Dropdown.OptionData(optionText));
				}

				InvokePropertyValueChanged(nameof(EventScheduleOptions),           EventScheduleOptions);
				InvokePropertyValueChanged(nameof(SelectEventScheduleOptionIndex), SelectEventScheduleOptionIndex);
			}

			// 티켓 정렬
			{
				EventSortOptions.Clear();
				for (int idx = 0; idx < (int)EEventSortOptionType.Max; ++idx)
				{
					var optionText = ((EEventSortOptionType)idx).ToOptionText();
					if (string.IsNullOrEmpty(optionText)) continue;

					EventSortOptions.Add(new TMP_Dropdown.OptionData(optionText));
				}

				InvokePropertyValueChanged(nameof(EventSortOptions),           EventSortOptions);
				InvokePropertyValueChanged(nameof(SelectEventSortOptionIndex), SelectEventSortOptionIndex);
			}
		}

		partial void RegisterTicketOptionsListener()
		{
			_dropDownEventOfEventSchedule?.AddListener(OnDropDownEventOfEventSchedule);
			_dropDownEventOfEventSort?.AddListener(OnDropDownEventOfEventSort);

			EventDetailViewModel.RegisterProgramOptionsListener();
		}

		partial void UnregisterTicketOptionsListener()
		{
			_dropDownEventOfEventSchedule?.RemoveListener(OnDropDownEventOfEventSchedule);
			_dropDownEventOfEventSort?.RemoveListener(OnDropDownEventOfEventSort);

			EventDetailViewModel.UnregisterProgramOptionsListener();
		}

		private void OnDropDownEventOfEventSchedule(int value)
		{
			_selectEventScheduleOptionType = (EEventScheduleOptionType)value;

			UpdateTicketView();
		}

		private void OnDropDownEventOfEventSort(int value)
		{
			_selectEventSortOptionType = (EEventSortOptionType)value;

			UpdateTicketView();
		}
	}

	public partial class MiceAppPackageDetailViewModel
	{
#region Variables
		private int _selectProgramOptionIndex;

		private List<MiceProgramID>           _programIDs     = new();
		private List<TMP_Dropdown.OptionData> _programOptions = new();

		private TMP_Dropdown.DropdownEvent _dropDownEventOfProgram;
#endregion

#region Properties
		public int SelectProgramOptionIndex => (int)_selectProgramOptionIndex;

		public List<TMP_Dropdown.OptionData> ProgramOptions
		{
			get => _programOptions;
			set => SetProperty(ref _programOptions, value);
		}

		public TMP_Dropdown.DropdownEvent DropDownEventOfProgram
		{
			get => _dropDownEventOfProgram;
			set => _dropDownEventOfProgram = value;
		}
#endregion

		partial void UpdateProgramOptions()
		{
			_selectProgramOptionIndex = 0;

			void AddOption(MiceProgramInfo programInfo)
			{
				if (programInfo != null)
				{
					_programIDs.Add(programInfo.ID);
					ProgramOptions.Add(new TMP_Dropdown.OptionData(programInfo.StrTitle));
				}
				else
				{
					_programIDs.Add(0L.ToMiceProgramID());
					ProgramOptions.Add(new TMP_Dropdown.OptionData(
						                   Data.Localization.eKey.MICE_UI_Mobile_Filter_Program_Popup_List_All.ToLocalizationString()));
				}
			}

			// 프로그램 필터
			{
				//초기화
				_programIDs.Clear();
				ProgramOptions.Clear();

				//전체 조회용
				AddOption(null);

				if (PackageInfo is { TicketInfoList: { } })
				{
					var programIDs = PackageInfo.TicketInfoList
					                            .Select(e => e.ProgramId.ToMiceProgramID())
					                            .Distinct()
					                            .ToList();

					foreach (var programID in programIDs)
					{
						var programInfo = PackageInfo.EventInfo.GetProgramInfo(programID);
						if (programInfo != null)
							AddOption(programInfo);
					}
				}

				InvokePropertyValueChanged(nameof(ProgramOptions),           ProgramOptions);
				InvokePropertyValueChanged(nameof(SelectProgramOptionIndex), SelectProgramOptionIndex);
			}
		}

		public partial void RegisterProgramOptionsListener()
		{
			_dropDownEventOfProgram?.AddListener(OnDropDownEventOfProgram);
		}

		public partial void UnregisterProgramOptionsListener()
		{
			_dropDownEventOfProgram?.RemoveListener(OnDropDownEventOfProgram);
		}

		private void OnDropDownEventOfProgram(int value)
		{
			_selectProgramOptionIndex = value;

			UpdatePackageDetailView(false);
		}

		private bool IsVisibleSessionItem(MiceWebClient.Entities.TicketInfoDetailEntity ticketInfo)
		{
			if (ticketInfo == null) return false;

			if (_programIDs == null || _programIDs.Count <= _selectProgramOptionIndex)
				return false;

			var selectProgramID = _programIDs.ElementAt(_selectProgramOptionIndex);
			return !selectProgramID.IsValid() || selectProgramID.Equals(ticketInfo.ProgramId.ToMiceProgramID());
		}
	}

	public static class EventScheduleOptionExtensions
	{
		public static string ToOptionText(this MiceAppViewModel.EEventScheduleOptionType type)
		{
			return type switch
			{
				MiceAppViewModel.EEventScheduleOptionType.Ongoing => Data.Localization.eKey
				                                                         .MICE_UI_Mobile_MP_MyTicket_Ticket_FilterPopup_Ongoing.ToLocalizationString(),
				MiceAppViewModel.EEventScheduleOptionType.Upcoming => Data.Localization.eKey
				                                                          .MICE_UI_Mobile_MP_MyTicket_Ticket_FilterPopup_Soon.ToLocalizationString(),
				MiceAppViewModel.EEventScheduleOptionType.Expired => Data.Localization.eKey
				                                                         .MICE_UI_Mobile_MP_MyTicket_Ticket_FilterPopup_Complete.ToLocalizationString(),
				_ => null
			};
		}

		public static bool IsVisible(this MiceAppViewModel.EEventScheduleOptionType type,
		                             MiceWebClient.Entities.UserPackageInfo         packageInfo)
		{
			switch (type)
			{
				case MiceAppViewModel.EEventScheduleOptionType.Ongoing:
				{
					var now = DateTime.Now;
					return packageInfo.StartDateTimeInSessions <= now && now < packageInfo.EndDateTimeInSessions;
				}
				case MiceAppViewModel.EEventScheduleOptionType.Upcoming:
					return DateTime.Now < packageInfo.StartDateTimeInSessions;
				case MiceAppViewModel.EEventScheduleOptionType.Expired:
					return packageInfo.EndDateTimeInSessions <= DateTime.Now;
				default: return true;
			}
		}
	}

	public static class EventSortOptionExtensions
	{
		public static string ToOptionText(this MiceAppViewModel.EEventSortOptionType type)
		{
			return type switch
			{
				MiceAppViewModel.EEventSortOptionType.Recently => Data.Localization.eKey
				                                                      .MICE_UI_Mobile_Sorting_Popup_Latest.ToLocalizationString(),
				MiceAppViewModel.EEventSortOptionType.CloseToTerminate => Data.Localization.eKey
				                                                              .MICE_UI_Mobile_Sorting_Popup_EndDate.ToLocalizationString(),
				_ => null
			};
		}

		public static List<MiceWebClient.Entities.UserPackageInfo> GetOrderedPackages(
			this MiceAppViewModel.EEventSortOptionType type)
		{
			return type switch
			{
				MiceAppViewModel.EEventSortOptionType.CloseToTerminate =>
					MiceInfoManager.Instance.MyUserInfo.PackageInfos
					               .Select(e => (e.Value, e.Value.EndDateTimeInSessions))
					               .OrderBy(e => e.EndDateTimeInSessions)
					               .Select(e => e.Value)
					               .ToList(),
				_ => MiceInfoManager.Instance.MyUserInfo.PackageInfos
				                    .Select(e => (e.Value, e.Value.StartDateTimeInSessions))
				                    .OrderByDescending(e => e.StartDateTimeInSessions)
				                    .Select(e => e.Value)
				                    .ToList()
			};
		}
	}
}
