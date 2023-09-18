/*===============================================================
* Product:		Com2Verse
* File Name:	PacketReceiver_Setting.cs
* Developer:	mikeyid77
* Date:			2023-05-17 16:00
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using Google.Protobuf;

namespace Com2Verse.UI
{
	public partial class PacketReceiver
	{
		private Action<Protocols.CommonLogic.SettingValueResponse>    _settingValueResponse;
		private Action<Protocols.CommonLogic.AccountSettingResponse>  _accountSettingResponse;
		private Action<Protocols.CommonLogic.ChattingSettingResponse> _chattingSettingResponse;

		public event Action<Protocols.CommonLogic.SettingValueResponse> SettingValueResponse
		{
			add
			{
				_settingValueResponse -= value;
				_settingValueResponse += value;
			}
			remove => _settingValueResponse -= value;
		}

		public event Action<Protocols.CommonLogic.AccountSettingResponse> AccountSettingResponse
		{
			add
			{
				_accountSettingResponse -= value;
				_accountSettingResponse += value;
			}
			remove => _accountSettingResponse -= value;
		}

		public event Action<Protocols.CommonLogic.ChattingSettingResponse> ChattingSettingResponse
		{
			add
			{
				_chattingSettingResponse -= value;
				_chattingSettingResponse += value;
			}
			remove => _chattingSettingResponse -= value;
		}

		public void OnSettingValueResponse(IMessage message)
		{
			if (message is Protocols.CommonLogic.SettingValueResponse settingResponse)
			{
				C2VDebug.Log($"Receive {nameof(OnSettingValueResponse)}");
				_settingValueResponse?.Invoke(settingResponse);
			}
		}

		public void OnAccountSettingResponse(IMessage message)
		{
			if (message is Protocols.CommonLogic.AccountSettingResponse accountResponse)
			{
				C2VDebug.Log($"Receive {nameof(OnAccountSettingResponse)}");
				_accountSettingResponse?.Invoke(accountResponse);
			}
		}

		public void OnChattingSettingResponse(IMessage message)
		{
			if (message is Protocols.CommonLogic.ChattingSettingResponse chattingSettingResponse)
			{
				C2VDebug.Log($"Receive {nameof(OnChattingSettingResponse)}");
				_chattingSettingResponse?.Invoke(chattingSettingResponse);
			}
		}
	}
}
