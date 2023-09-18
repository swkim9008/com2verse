/*===============================================================
* Product:		Com2Verse
* File Name:	Commander_Option.cs
* Developer:	mikeyid77
* Date:			2023-05-17 15:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Globalization;
using Com2Verse.AvatarAnimation;
using Com2Verse.Logger;
using Com2Verse.Option;
using Protocols;

namespace Com2Verse.Network
{
	public sealed partial class Commander
	{
		public void SettingValueRequest()
		{
			Protocols.CommonLogic.SettingValueRequest request = new();

			C2VDebug.LogCategory("OptionController", $"Sending {nameof(SettingValueRequest)}");
			NetworkManager.Instance.Send(request,
			                             Protocols.CommonLogic.MessageTypes.SettingValueRequest,
			                             Protocols.Channels.CommonLogic,
			                             (int)Protocols.CommonLogic.MessageTypes.SettingValueResponse,
			                             timeoutAction:() =>
			                             {
				                             User.Instance.DefaultTimeoutProcess();
			                             });
		}

		public void AccountSettingRequest(int language, int alarm)
		{
			// language : 0-한국어 / 1-영어
			// alarm    : 0-알림1개 / 1-알림2개 / 2-알림3개
			Protocols.CommonLogic.AccountSettingRequest request = new()
			{
				Language = language,
				AlramCount = alarm,
			};

			C2VDebug.LogCategory("OptionController", $"Sending {nameof(AccountSettingRequest)} : {language} {alarm}");
			NetworkManager.Instance.Send(request,
			                             Protocols.CommonLogic.MessageTypes.AccountSettingRequest,
			                             Protocols.Channels.CommonLogic,
			                             (int)Protocols.CommonLogic.MessageTypes.AccountSettingResponse,
			                             timeoutAction:() =>
			                             {
				                             User.Instance.DefaultTimeoutProcess();
			                             });
		}

		public void ChattingSettingRequest(ChatOption chatOption)
		{
			if (chatOption == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "ChatOption is null");
				return;
			}

			Protocols.CommonLogic.ChattingSettingRequest request = new()
			{
				Timestamp    = chatOption.IsOnTimeStamp,
				SpeechBubble = (int)chatOption.ShowBubbleOption,
				ChatOpacity  = (int)chatOption.ChatTransparent,
				ChatFontSize = (int)chatOption.FontSize,
				LinkWarnYn   = chatOption.IsOnLinkWarning,
				ChatSimplify = chatOption.IsAutoSimplify,
				ChatFilter   = (int)chatOption.ChattingFilter,
			};

			C2VDebug.LogCategory("OptionController", $"Sending {nameof(ChattingSettingRequest)} : {chatOption}");
			NetworkManager.Instance.Send(request,
			                             Protocols.CommonLogic.MessageTypes.ChattingSettingRequest,
			                             Protocols.Channels.CommonLogic,
			                             (int)Protocols.CommonLogic.MessageTypes.ChattingSettingResponse,
			                             timeoutAction:() =>
			                             {
				                             User.Instance.DefaultTimeoutProcess();
			                             });
		}
	}
}
