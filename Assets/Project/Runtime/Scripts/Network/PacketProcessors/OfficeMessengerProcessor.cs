/*===============================================================
* Product:		Com2Verse
* File Name:	OfficeMessengerProcessor.cs
* Developer:	mikeyid77
* Date:			2023-04-24 18:56
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.UI;
using JetBrains.Annotations;
using Protocols;

namespace Com2Verse.Network
{
	[UsedImplicitly]
	[Channel(Protocols.Channels.OfficeMessenger)]
	public sealed class OfficeMessengerProcessor : BaseMessageProcessor
	{
		public override void Initialize()
		{
			SetMessageProcessCallback((int)Protocols.OfficeMessenger.MessageTypes.LoginOfficeResponse,
			                          (payload) => Protocols.OfficeMessenger.LoginOfficeResponse.Parser.ParseFrom(payload),
			                          UI.PacketReceiver.Instance.OnLoginOfficeResponse);
		}

		public override void ErrorProcess(Protocols.Channels channel, int command, Protocols.ErrorCode errorCode)
		{
			if (command == (int)Protocols.OfficeMessenger.MessageTypes.LoginOfficeResponse)
			{
				UIManager.Instance.HideWaitingResponsePopup();
				C2VDebug.LogError($"LoginOfficeResponse : {errorCode}");
				if (errorCode == ErrorCode.ExpiredToken)
				{
					// TODO : ErrorString 필요
					NetworkManager.Instance.TokenExpired = true;
					UIManager.Instance.ShowPopupCommon("토큰이 만료되었습니다.", async () =>
					{
						await LoginManager.Instance.TryRefreshToken(() =>
						{
							NetworkManager.Instance.TokenExpired = false;
							NetworkManager.Instance.ResendMessage?.Invoke();
						});
					});
				}
				else
				{
					switch (errorCode)
					{
						case ErrorCode.InvalidToken:
							UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("WRONG_TOKEN"));
							break;
						case ErrorCode.MismatchDid:
							// TODO : ErrorString 필요
							UIManager.Instance.ShowPopupCommon("잘못된 기기정보입니다.");
							break;
						default:
							break;
					}
					
				}
				return;
			}
			base.ErrorProcess(channel, command, errorCode);
		}
	}
}
