/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingReservationCreditViewModel.cs
* Developer:	ksw
* Date:			2023-07-04 17:10
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.WebApi.Service;
using Cysharp.Text;
using UnityEngine;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;

namespace Com2Verse.UI
{
	[ViewModelGroup("MeetingReservation")]
	public sealed class MeetingReservationCreditViewModel : ViewModelBase
	{
		private StackRegisterer _reservationCreditGUIViewRegister;

		public StackRegisterer ReservationCreditGUIViewRegister
		{
			get => _reservationCreditGUIViewRegister;
			set
			{
				_reservationCreditGUIViewRegister             =  value;
				_reservationCreditGUIViewRegister.WantsToQuit += CloseCreditPopup;
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

		private ePaymentType         _paymentType = ePaymentType.NONE;
		private int                  _numberOfConsumed;
		private GUIView              _guiView;
		private Action<ePaymentType> _closeCallback;
		private bool                 _freeTicketSelected;
		private bool                 _creditSelected;

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

		//private TableOfficeEvent _eventTable;
#region Property
		public int NumberOfConsumed
		{
			get => _numberOfConsumed;
			set => SetProperty(ref _numberOfConsumed, value);
		}

		public bool FreeTicketSelected
		{
			get => _freeTicketSelected;
			set
			{
				FreeTicketStringColor = value ? _enabledColor : _disabledColor;
				FreeTicketString = _numberOfConsumed.ToString();
				SetProperty(ref _freeTicketSelected, value);
			}
		}

		public bool CreditSelected
		{
			get => _creditSelected;
			set
			{
				CreditStringColor = value ? _enabledColor : _disabledColor;
				CreditString = _numberOfConsumed.ToString();
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

		public string FreeTicketString
		{
			get => _freeTicketString;
			set => SetProperty(ref _freeTicketString, value);
		}

		public string CreditString
		{
			get => _creditString;
			set => SetProperty(ref _creditString, value);
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
#endregion

		public MeetingReservationCreditViewModel()
		{
			CommandCloseCreditPopup  = new CommandHandler(CloseCreditPopup);
			CommandSelectPaymentType = new CommandHandler<int>(SelectPaymentType);
			CommandClickPayment      = new CommandHandler(ClickPayment);

			Initialize();
		}

		private void Initialize()
		{
			PaymentType       = ePaymentType.FREE_TICKET;
			_numberOfConsumed = 1;
			_closeCallback    = null;
			_guiView          = null;
			//_eventTable       = TableDataManager.Instance.Get<TableOfficeEvent>();
		}

		public async void SetCreditPopup(int numberOfConsumed, Action<ePaymentType> closeCallback, GUIView guiView)
		{
			Initialize();
			_closeCallback    = closeCallback;
			_guiView          = guiView;
			_numberOfConsumed = numberOfConsumed;

			PaymentType      = ePaymentType.FREE_TICKET;
			CreditString     = _numberOfConsumed.ToString();
			FreeTicketString = _numberOfConsumed.ToString();

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
			}, onError =>
			{
				CloseCreditPopup();
			});
			//if (_eventTable != null && _eventTable.Datas[eOfficeEventType.CONNECTING_PROMOTION].IsActive)
				//UIManager.Instance.ShowPopUpEvent("UI_Credit_Event_Popup");
			Canvas.ForceUpdateCanvases();
		}

		private void CloseCreditPopup()
		{
			_guiView.Hide();
		}

		private void SelectPaymentType(int type)
		{
			PaymentType = (ePaymentType)type;
		}

		private void ClickPayment()
		{
			switch (_paymentType)
			{
				case ePaymentType.FREE_TICKET:
					if (_numberOfConsumed > _freeTicketRetain)
					{
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_Payment_NotEnoughFreePass_Toast"));
						return;
					}
					break;
				case ePaymentType.CREDIT:
					if (_numberOfConsumed > _creditRetain)
					{
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_Payment_NotEnoughCredit_Toast"));
						return;
					}
					break;
				default:
					break;
			}
			CloseCreditPopup();
			_closeCallback?.Invoke(_paymentType);
		}
	}
}
