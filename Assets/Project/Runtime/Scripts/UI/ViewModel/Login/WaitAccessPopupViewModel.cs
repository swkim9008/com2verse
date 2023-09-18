/*===============================================================
* Product:		Com2Verse
* File Name:	WaitAccessPopupViewModel.cs
* Developer:	mikeyid77
* Date:			2023-06-16 11:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using Com2Verse.Logger;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;
using Protocols.CommonLogic;

namespace Com2Verse.UI
{
	public sealed class WaitAccessPopupViewModel : CommonPopupViewModel
	{
		private const int MaxQueueCount = 9999;
		private const int RequestCycle  = 5;

		private StackRegisterer _guiViewRegisterer = null;
		private bool            _cancelTrigger     = true;
		private bool            _packetCheck       = true;
		private int             _waitUserCount     = 0;
		private int             _targetUserCount   = 0;

		private Action _readyToEnterAction = null;

		public  CommandHandler CancelButtonClicked { get; }

		public StackRegisterer GuiViewRegisterer
		{
			get => _guiViewRegisterer;
			set
			{
				_guiViewRegisterer             =  value;
				_guiViewRegisterer.WantsToQuit += OnCancelButtonClicked;
			}
		}

		public event Action ReadyToEnterEvent
		{
			add
			{
				_readyToEnterAction -= value;
				_readyToEnterAction += value;
			}
			remove => _readyToEnterAction -= value;
		}

		private int WaitUserCount
		{
			get => _waitUserCount;
			set
			{
				if (value >= 0)
				{
					if (_waitUserCount != value)
						C2VDebug.LogCategory("WaitAccess", $"Reset Queue Ranking : {value}");
					_waitUserCount = value;

					var text 
						= (value > MaxQueueCount) 
							? Localization.Instance.GetString("UI_World_WaitAccess_Popup_WaitingNumber2")
							: Localization.Instance.GetString("UI_World_WaitAccess_Popup_WaitingNumber1", value);
					Context = Localization.Instance.GetString("UI_World_WaitAccess_Popup_Desc", text);
				}
			}
		}

		public WaitAccessPopupViewModel()
		{
			CancelButtonClicked = new CommandHandler(OnCancelButtonClicked);
		}

		public void SetPopupText(int queueCount, bool isPublic)
		{
			// TODO : Service쪽 Localization String 요청 필요
			// Title = (isPublic) ? Localization.Instance.GetString("UI_World_WaitAccess_Popup_Title") : "서비스 접속 대기";

			Title            = Localization.Instance.GetString("UI_World_WaitAccess_Popup_Title");
			_cancelTrigger   = false;
			_packetCheck     = true;
			_targetUserCount = queueCount;
			WaitUserCount    = queueCount;
			OnTimeUpdate().Forget();
		}

		private void OnCancelButtonClicked()
		{
			_cancelTrigger = true;
			_guiViewRegisterer.HideComplete();
		}

		private async UniTask OnTimeUpdate()
		{
			C2VDebug.LogCategory("WaitAccess", $"Start TimeUpdate");
			Network.CommonLogic.PacketReceiver.Instance.ConnectQueueResponse         += ResponseConnectQueue;
			Network.CommonLogic.PacketReceiver.Instance.UserAcceptConnectWorldNotify += NotifyUserAcceptConnectWorld;

			float currentTime = RequestCycle;
			while (!_cancelTrigger)
			{
				if (currentTime > 0)
				{
					currentTime -= Time.deltaTime;
				}

				if (currentTime <= 0 && _packetCheck)
				{
					_packetCheck = false;
					currentTime  = RequestCycle;
					Commander.Instance.RequestConnectQueue(TimeoutConnectQueue);
				}

				await UniTask.Yield();
			}

			C2VDebug.LogCategory("WaitAccess", $"Finish TimeUpdate");
			Network.CommonLogic.PacketReceiver.Instance.ConnectQueueResponse         -= ResponseConnectQueue;
			Network.CommonLogic.PacketReceiver.Instance.UserAcceptConnectWorldNotify -= NotifyUserAcceptConnectWorld;
			await UniTask.Yield();
		}

		private void ResponseConnectQueue(ConnectQueueResponse response)
		{
			if (response == null)
			{
				C2VDebug.LogWarningCategory("WaitAccess", $"{nameof(ConnectQueueResponse)} is NULL");
			}
			else
			{
				_packetCheck = true;
				if (_targetUserCount >= response.QueueRanking)
					WaitUserCount = response.QueueRanking;
			}
		}

		private void NotifyUserAcceptConnectWorld(UserAcceptConnectWorldNotify notify)
		{
			C2VDebug.LogCategory("WaitAccess", $"Ready to Enter");

			OnCancelButtonClicked();
			_readyToEnterAction?.Invoke();
		}

		private void TimeoutConnectQueue()
		{
			C2VDebug.LogCategory("WaitAccess", $"Timeout");

			OnCancelButtonClicked();
		}
	}
}
