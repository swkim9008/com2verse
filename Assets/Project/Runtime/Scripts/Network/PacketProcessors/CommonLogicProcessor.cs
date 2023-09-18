/*===============================================================
* Product:		Com2Verse
* File Name:	CommonLogicProcessor.cs
* Developer:	mikeyid77
* Date:			2023-05-17 19:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.InputSystem;
using Com2Verse.UI;
using JetBrains.Annotations;

namespace Com2Verse.Network
{
    [UsedImplicitly]
    [Channel(Protocols.Channels.CommonLogic)]
    public sealed class CommonLogicProcessor : BaseMessageProcessor
    {
        public override void Initialize()
        {
            SetMessageProcessCallback((int)Protocols.CommonLogic.MessageTypes.SettingValueResponse,
                (payload) => Protocols.CommonLogic.SettingValueResponse.Parser.ParseFrom(payload),
                UI.PacketReceiver.Instance.OnSettingValueResponse);

            SetMessageProcessCallback((int)Protocols.CommonLogic.MessageTypes.AccountSettingResponse,
                (payload) => Protocols.CommonLogic.AccountSettingResponse.Parser.ParseFrom(payload),
                UI.PacketReceiver.Instance.OnAccountSettingResponse);

            SetMessageProcessCallback((int)Protocols.CommonLogic.MessageTypes.ChattingSettingResponse,
                (payload) => Protocols.CommonLogic.ChattingSettingResponse.Parser.ParseFrom(payload),
                UI.PacketReceiver.Instance.OnChattingSettingResponse);

            SetMessageProcessCallback((int)Protocols.CommonLogic.MessageTypes.ServiceChangeNotify,
                                      payload => Protocols.CommonLogic.ServiceChangeNotify.Parser.ParseFrom(payload),
                                      message =>
                                      {
                                          if (message is Protocols.CommonLogic.ServiceChangeNotify response)
                                              CommonLogic.PacketReceiver.Instance.RaiseServiceChangeNotify(response);
                                      });

            SetMessageProcessCallback((int)Protocols.CommonLogic.MessageTypes.UserConnectableCheckResponse,
                                      (payload) => Protocols.CommonLogic.UserConnectableCheckResponse.Parser.ParseFrom(payload),
                                      message =>
                                      {
                                          if (message is Protocols.CommonLogic.UserConnectableCheckResponse response)
                                              CommonLogic.PacketReceiver.Instance.RaiseUserConnectableCheckResponse(response);
                                      });
            SetMessageProcessCallback((int)Protocols.CommonLogic.MessageTypes.ConnectQueueResponse,
                                      (payload) => Protocols.CommonLogic.ConnectQueueResponse.Parser.ParseFrom(payload),
                                      message =>
                                      {
                                          if (message is Protocols.CommonLogic.ConnectQueueResponse response)
                                              CommonLogic.PacketReceiver.Instance.RaiseConnectQueueResponse(response);
                                      });
            SetMessageProcessCallback((int)Protocols.CommonLogic.MessageTypes.UserAcceptConnectWorldNotify,
                                      (payload) => Protocols.CommonLogic.UserAcceptConnectWorldNotify.Parser.ParseFrom(payload),
                                      message =>
                                      {
                                          if (message is Protocols.CommonLogic.UserAcceptConnectWorldNotify response)
                                              CommonLogic.PacketReceiver.Instance.RaiseUserAcceptConnectWorldNotify(response);
                                      });
            SetMessageProcessCallback((int)Protocols.CommonLogic.MessageTypes.UserInacceptableWorldNotify,
                                      (payload) => Protocols.CommonLogic.UserInacceptableWorldNotify.Parser.ParseFrom(payload),
                                      message =>
                                      {
                                          if (message is Protocols.CommonLogic.UserInacceptableWorldNotify response)
                                              CommonLogic.PacketReceiver.Instance.RaiseUserInacceptableWorldNotify(response);
                                      });
            SetMessageProcessCallback((int)Protocols.CommonLogic.MessageTypes.EscapeAcceptableCheckResponse,
                                      payload => Protocols.CommonLogic.EscapeAcceptableCheckResponse.Parser.ParseFrom(payload),
                                      message =>
                                      {
                                          if (message is Protocols.CommonLogic.EscapeAcceptableCheckResponse response)
                                              CommonLogic.PacketReceiver.Instance.RaiseEscapeAcceptableCheckResponse(response);
                                      });

            SetMessageProcessCallback((int)Protocols.CommonLogic.MessageTypes.UseChairResponse,
                                      (payload) => Protocols.CommonLogic.UseChairResponse.Parser.ParseFrom(payload),
                                      UI.PacketReceiver.Instance.OnUseChairResponse);

            SetMessageProcessCallback((int) Protocols.CommonLogic.MessageTypes.UpdateAvatarResponse,
                                      (payload) => Protocols.CommonLogic.UpdateAvatarResponse.Parser.ParseFrom(payload),
                                      UI.PacketReceiver.Instance.OnUpdateAvatarResponse);

            // Chatting
            SetMessageProcessCallback((int)Protocols.CommonLogic.MessageTypes.WhisperChattingResponse,
                                      payload => Protocols.CommonLogic.WhisperChattingResponse.Parser.ParseFrom(payload),
                                      static message =>
                                      {
                                          if (message is Protocols.CommonLogic.WhisperChattingResponse response)
                                              PacketReceiver.Instance.RaiseWhisperChattingResponse(response);
                                      });
            SetMessageProcessCallback((int)Protocols.CommonLogic.MessageTypes.WhisperChattingNotify,
                                      payload => Protocols.CommonLogic.WhisperChattingNotify.Parser.ParseFrom(payload),
                                      static message =>
                                      {
                                          if (message is Protocols.CommonLogic.WhisperChattingNotify response)
                                              PacketReceiver.Instance.RaiseWhisperChattingNotify(response);
                                      });
        }

        public override void ErrorProcess(Protocols.Channels channel, int command, Protocols.ErrorCode errorCode)
        {
            if (command == (int)Protocols.CommonLogic.MessageTypes.UseChairResponse)
            {
                UIManager.Instance.HideWaitingResponsePopup();
                UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_Interaction_NotAvailable_Msg"));
                return;
            }
            base.ErrorProcess(channel, command, errorCode);
        }
    }
}