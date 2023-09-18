/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomCreditViewModel.cs
* Developer:	ksw
* Date:			2023-07-07 11:40
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Chat;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.WebApi.Service;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using TMPro;

namespace Com2Verse.UI
{
	[ViewModelGroup("MeetingRoom")]
	public sealed class MeetingRoomCreditViewModel : ViewModelBase
	{
		private StackRegisterer _meetingRoomCreditGUIViewRegisterer;

		public StackRegisterer MeetingRoomCreditGUIViewRegisterer
		{
			get => _meetingRoomCreditGUIViewRegisterer;
			set
			{
				_meetingRoomCreditGUIViewRegisterer             =  value;
				_meetingRoomCreditGUIViewRegisterer.WantsToQuit += CloseCreditPopup;
			}
		}

		public enum ePaymentType
		{
			NONE,
			CREDIT,
			FREE_TICKET,
		}
		public CommandHandler      CommandCloseCreditPopup  { get; }
		public CommandHandler<int> CommandSelectPaymentType { get; }
		public CommandHandler      CommandClickPayment      { get; }

		private ePaymentType _paymentType = ePaymentType.NONE;
		private GUIView      _guiView;
		private bool         _freeTicketSelected;
		private bool         _creditSelected;

		private string _freeTicketRetainString;
		private string _creditRetainString;

		private int _freeTicketRetain;
		private int _creditRetain;

		private string _freeTicketString;
		private string _creditString;

		private readonly Color _enabledColor  = new Color(0.2f, 0.231f, 0.349f, 1f);
		private readonly Color _disabledColor = new Color(0.2f, 0.231f, 0.349f, 0.5f);
		private          Color _freeTicketStringColor;
		private          Color _creditStringColor;

		private readonly Color _disabledArrowColor = new Color(0.35f,  0.35f,  0.38f, 0.8f);
		private readonly Color _enabledArrowColor  = new Color(0.275f, 0.372f, 1f, 1f);
		private          Color _freeTicketArrowColor;
		private          Color _creditArrowColor;

		private Vector3 _inactiveArrowAngle = new Vector3(0f, 0f, 270f);
		private Vector3 _activeArrowAngle   = new Vector3(0f, 0f, 90f);
		private Vector3 _freeTicketArrowAngle;
		private Vector3 _creditArrowAngle;

		private List<TMP_Dropdown.OptionData> _freeTicketOptions;
		private List<TMP_Dropdown.OptionData> _creditOptions;

		private TMP_Dropdown.DropdownEvent _dropdownEventOfFreeTicket;
		private TMP_Dropdown.DropdownEvent _dropdownEventOfCredit;

		private MetaverseDropdown _freeTicketDropdown;
		private MetaverseDropdown _creditDropdown;

		private int _indexOfFreeTicket;
		private int _indexOfCredit;
		private int _freeTicketConsumed;
		private int _creditConsumed;

		//private TableOfficeEvent _eventTable;
#region Property
		public bool FreeTicketSelected
		{
			get => _freeTicketSelected;
			set
			{
				FreeTicketStringColor   = value ? _enabledColor : _disabledColor;
				FreeTicketArrowColor    = value ? _enabledArrowColor : _disabledArrowColor;
				FreeTicketRotationArrow = _inactiveArrowAngle;
				CreditRotationArrow     = _inactiveArrowAngle;
				SetProperty(ref _freeTicketSelected, value);
			}
		}

		public bool CreditSelected
		{
			get => _creditSelected;
			set
			{
				CreditStringColor       = value ? _enabledColor : _disabledColor;
				CreditArrowColor        = value ? _enabledArrowColor : _disabledArrowColor;
				FreeTicketRotationArrow = _inactiveArrowAngle;
				CreditRotationArrow     = _inactiveArrowAngle;
				SetProperty(ref _creditSelected, value);
			}
		}

		public ePaymentType PaymentType
		{
			get => _paymentType;
			set
			{
				FreeTicketSelected = value == ePaymentType.FREE_TICKET;
				CreditSelected     = value == ePaymentType.CREDIT;
				SetProperty(ref _paymentType, value);
			}
		}

		public string FreeTicketRetain
		{
			get => _freeTicketRetainString;
			set => SetProperty(ref _freeTicketRetainString, value);
		}

		public string CreditRetain
		{
			get => _creditRetainString;
			set => SetProperty(ref _creditRetainString, value);
		}

		public Color FreeTicketStringColor
		{
			get => _freeTicketStringColor;
			set => SetProperty(ref _freeTicketStringColor, value);
		}

		public Color CreditStringColor
		{
			get => _creditStringColor;
			set => SetProperty(ref _creditStringColor, value);
		}

		public Color FreeTicketArrowColor
		{
			get => _freeTicketArrowColor;
			set => SetProperty(ref _freeTicketArrowColor, value);
		}

		public Color CreditArrowColor
		{
			get => _creditArrowColor;
			set => SetProperty(ref _creditArrowColor, value);
		}

		public List<TMP_Dropdown.OptionData> MeetingFreeTicketOptions
		{
			get => _freeTicketOptions;
			set => SetProperty(ref _freeTicketOptions, value);
		}

		public List<TMP_Dropdown.OptionData> MeetingCreditOptions
		{
			get => _creditOptions;
			set => SetProperty(ref _creditOptions, value);
		}

		public TMP_Dropdown.DropdownEvent DropdownEventOfFreeTicket
		{
			get => _dropdownEventOfFreeTicket;
			set => _dropdownEventOfFreeTicket = value;
		}


		public TMP_Dropdown.DropdownEvent DropdownEventOfCredit
		{
			get => _dropdownEventOfCredit;
			set => _dropdownEventOfCredit = value;
		}

		public int IndexOfFreeTicket
		{
			get => _indexOfFreeTicket;
			set => SetProperty(ref _indexOfFreeTicket, value);
		}

		public int IndexOfCredit
		{
			get => _indexOfCredit;
			set => SetProperty(ref _indexOfCredit, value);
		}

		public Vector3 FreeTicketRotationArrow
		{
			get => _freeTicketArrowAngle;
			set => SetProperty(ref _freeTicketArrowAngle, value);
		}

		public Vector3 CreditRotationArrow
		{
			get => _creditArrowAngle;
			set => SetProperty(ref _creditArrowAngle, value);
		}

		public MetaverseDropdown FreeTicketDropdown
		{
			get => _freeTicketDropdown;
			set => SetProperty(ref _freeTicketDropdown, value);
		}

		public MetaverseDropdown CreditDropdown
		{
			get => _creditDropdown;
			set => SetProperty(ref _creditDropdown, value);
		}
#endregion

		public MeetingRoomCreditViewModel()
		{
			CommandCloseCreditPopup  = new CommandHandler(CloseCreditPopup);
			CommandSelectPaymentType = new CommandHandler<int>(SelectPaymentType);
			CommandClickPayment      = new CommandHandler(ClickPayment);

			_freeTicketOptions = new List<TMP_Dropdown.OptionData>();
			_creditOptions = new List<TMP_Dropdown.OptionData>();

			Initialize();
		}

		private void Initialize()
		{
			PaymentType             = ePaymentType.FREE_TICKET;
			_guiView                = null;
			FreeTicketRotationArrow = _inactiveArrowAngle;
			CreditRotationArrow     = _inactiveArrowAngle;
			CreateDropdownOptions();
			_indexOfFreeTicket  = 0;
			_indexOfCredit      = 0;
			_freeTicketConsumed = 1;
			_creditConsumed     = 1;
			IndexOfFreeTicket   = 0;
			IndexOfCredit       = 0;
			FreeTicketSelected  = true;

			if (FreeTicketDropdown != null)
			{
				FreeTicketDropdown.OnClicked -= SetFreeTicketArrow;
				FreeTicketDropdown.OnClicked += SetFreeTicketArrow;
			}

			if (CreditDropdown != null)
			{
				CreditDropdown.OnClicked -= SetCreditArrow;
				CreditDropdown.OnClicked += SetCreditArrow;
			}

			//_eventTable = TableDataManager.Instance.Get<TableOfficeEvent>();
		}

		public async void SetCreditPopup( GUIView guiView)
		{
			Initialize();
			_guiView          = guiView;

			PaymentType      = ePaymentType.FREE_TICKET;

			await Commander.Instance.RequestGetCreditInfo(DataManager.Instance.GroupID, Components.GroupAssetType.AssetTypeNone, response =>
			{
				foreach (var info in response.Value.Data.GroupAssetEntities)
				{
					switch (info.GroupAssetType)
					{
						case Components.GroupAssetType.AssetTypeFreeCredit:
							_freeTicketRetain = info.Amount;
							FreeTicketRetain  = Localization.Instance.GetString("UI_ConnectingApp_Reservation_OwnedFreePassCount_Text", _freeTicketRetain);
							break;
						case Components.GroupAssetType.AssetTypeCredit:
							_creditRetain = info.Amount;
							CreditRetain  = Localization.Instance.GetString("UI_ConnectingApp_Reservation_OwnedCreditCount_Text", _creditRetain);
							break;
						default:
							FreeTicketRetain = Localization.Instance.GetString("UI_ConnectingApp_Reservation_OwnedFreePassCount_Text", 9999);
							CreditRetain     = Localization.Instance.GetString("UI_ConnectingApp_Reservation_OwnedCreditCount_Text",   9999);
							break;
					}
				}
			}, onError => { CloseCreditPopup(); });

			AddDropdownEvent();
			//if (_eventTable != null && _eventTable.Datas[eOfficeEventType.CONNECTING_PROMOTION].IsActive)
				//UIManager.Instance.ShowPopUpEvent("UI_Credit_Event_Popup");
			Canvas.ForceUpdateCanvases();
		}

		private void CreateDropdownOptions()
		{
			_freeTicketOptions.Clear();
			_creditOptions.Clear();

			var freeTicketData = new TMP_Dropdown.OptionData(
				ZString.Format("{0} {1}  {2}", Localization.Instance.GetString("UI_MeetingRoom_AdditionalReservation_ThirtyMinFreePass_Text"), "<sprite=2>", 1));
			_freeTicketOptions.Add(freeTicketData);
			freeTicketData = new TMP_Dropdown.OptionData(
				ZString.Format("{0} {1}  {2}", Localization.Instance.GetString("UI_MeetingRoom_AdditionalReservation_OneHourFreePass_Text"), "<sprite=2>", 2));
			_freeTicketOptions.Add(freeTicketData);
			freeTicketData = new TMP_Dropdown.OptionData(
				ZString.Format("{0} {1}  {2}", Localization.Instance.GetString("UI_MeetingRoom_AdditionalReservation_TwoHourFreePass_Text"), "<sprite=2>", 4));
			_freeTicketOptions.Add(freeTicketData);

			var creditData = new TMP_Dropdown.OptionData(
				ZString.Format("{0} {1}  {2}", Localization.Instance.GetString("UI_MeetingRoom_AdditionalReservation_ThirtyMinCredit_Text"), "<sprite=0>", 1));
			_creditOptions.Add(creditData);
			creditData = new TMP_Dropdown.OptionData(
				ZString.Format("{0} {1}  {2}", Localization.Instance.GetString("UI_MeetingRoom_AdditionalReservation_OneHourCredit_Text"), "<sprite=0>", 2));
			_creditOptions.Add(creditData);
			creditData = new TMP_Dropdown.OptionData(
				ZString.Format("{0} {1}  {2}", Localization.Instance.GetString("UI_MeetingRoom_AdditionalReservation_TwoHourCredit_Text"), "<sprite=0>", 4));
			_creditOptions.Add(creditData);
		}

		private void AddDropdownEvent()
		{
			_dropdownEventOfFreeTicket.RemoveAllListeners();
			_dropdownEventOfFreeTicket.AddListener((index) =>
			{
				_indexOfFreeTicket      = index;
				_freeTicketConsumed = index switch
				{
					0 => 1,
					1 => 2,
					_ => 4
				};
				SetFreeTicketArrow();
				FreeTicketRotationArrow = _inactiveArrowAngle;
			});

			_dropdownEventOfCredit.RemoveAllListeners();
			_dropdownEventOfCredit.AddListener((index) =>
			{
				_indexOfCredit      = index;
				_creditConsumed = index switch
				{
					0 => 1,
					1 => 2,
					_ => 4
				};
				CreditRotationArrow = _inactiveArrowAngle;
			});
		}

		private void RemoveDropdownEvent()
		{
			_dropdownEventOfFreeTicket.RemoveAllListeners();
			_dropdownEventOfCredit.RemoveAllListeners();
		}

		private void CloseCreditPopup()
		{
			RemoveDropdownEvent();
			_guiView.Hide();
		}

		private void SelectPaymentType(int type)
		{
			PaymentType = (ePaymentType)type;
		}

		private void ClickPayment()
		{
			int extendMinute = 0;
			switch (_paymentType)
			{
				case ePaymentType.FREE_TICKET:
					if (_freeTicketConsumed > _freeTicketRetain)
					{
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_Payment_NotEnoughFreePass_Toast"));
						return;
					}

					extendMinute = _freeTicketConsumed * 30;
					break;
				case ePaymentType.CREDIT:
					if (_creditConsumed > _creditRetain)
					{
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_Payment_NotEnoughCredit_Toast"));
						return;
					}

					extendMinute = _creditConsumed * 30;
					break;
			}

			Commander.Instance.RequestExtendEndAsync(MeetingReservationProvider.EnteredMeetingInfo.MeetingId, extendMinute, (Components.GroupAssetType)_paymentType, response =>
			{
				CloseCreditPopup();
				UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_AdditionalReservation_AddTimeNoti_Toast"));
				ChatManager.Instance.BroadcastCustomNotify(ChatManager.CustomDataType.TIME_EXTENSION_NOTIFY);
			}, error => { CloseCreditPopup(); }).Forget();
		}

		private void SetFreeTicketArrow()
		{
			FreeTicketRotationArrow = FreeTicketDropdown.IsExpanded ? _activeArrowAngle : _inactiveArrowAngle;
		}

		private void SetCreditArrow()
		{
			CreditRotationArrow = CreditDropdown.IsExpanded ? _activeArrowAngle : _inactiveArrowAngle;
		}
	}
}
