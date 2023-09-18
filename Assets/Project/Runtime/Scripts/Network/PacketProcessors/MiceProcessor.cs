/*===============================================================
* Product:		Com2Verse
* File Name:	MiceProcessor.cs
* Developer:	ikyoung
* Date:			2023-03-29 16:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;

namespace Com2Verse.Network
{
    [UsedImplicitly]
    [Channel(Protocols.Channels.Mice)]
    public sealed class MiceProcessor : BaseMessageProcessor
    {
        public override void Initialize()
        {
            SetMessageProcessCallback((int)Protocols.Mice.MessageTypes.EnterLobbyResponse,
                                      payload => Protocols.Mice.EnterLobbyResponse.Parser.ParseFrom(payload),
                                      UI.PacketReceiver.Instance.OnEnterMiceLobbyResponse);

            SetMessageProcessCallback((int)Protocols.Mice.MessageTypes.EnterLoungeResponse,
                                      payload => Protocols.Mice.EnterLoungeResponse.Parser.ParseFrom(payload),
                                      UI.PacketReceiver.Instance.OnEnterMiceLoungeResponse);
            
            SetMessageProcessCallback((int)Protocols.Mice.MessageTypes.EnterFreeLoungeResponse,
                                        payload => Protocols.Mice.EnterFreeLoungeResponse.Parser.ParseFrom(payload),
                                     UI.PacketReceiver.Instance.OnEnterMiceFreeLoungeResponse);

            SetMessageProcessCallback((int)Protocols.Mice.MessageTypes.EnterHallResponse,
                                      payload => Protocols.Mice.EnterHallResponse.Parser.ParseFrom(payload),
                                      UI.PacketReceiver.Instance.OnEnterMiceHallResponse);
            
            SetMessageProcessCallback((int)Protocols.Mice.MessageTypes.MiceRoomNotify,
                                      payload => Protocols.Mice.MiceRoomNotify.Parser.ParseFrom(payload),
                                      UI.PacketReceiver.Instance.OnMiceRoomNotify);
            
            SetMessageProcessCallback((int)Protocols.Mice.MessageTypes.ForcedMiceTeleportNotify,
                                      payload => Protocols.Mice.ForcedMiceTeleportNotify.Parser.ParseFrom(payload),
                                      UI.PacketReceiver.Instance.OnForcedMiceTeleportNotify);
        }

        public override void ErrorProcess(Protocols.Channels channel, int command, Protocols.ErrorCode errorCode)
        {
            base.ErrorProcess(channel, command, errorCode);
        }
    }
}
