/*===============================================================
* Product:    Com2Verse
* File Name:  WorldStateProcessor.cs
* Developer:  haminjeong
* Date:       2022-05-09 14:38
* History:    
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;
using Protocols.WorldState;

namespace Com2Verse.Network
{
    [UsedImplicitly]
    [Channel(Protocols.Channels.WorldState)]
    public sealed class WorldStateProcessor : BaseMessageProcessor
    {
	    public override void Initialize()
        {
            SetMessageProcessCallback((int)Protocols.WorldState.MessageTypes.CellState,
                                      (payload) => Protocols.WorldState.CellState.Parser.ParseFrom(payload),
                                      (message) =>
                                      {
                                          Protocols.WorldState.CellState mapData = message as Protocols.WorldState.CellState;
                                          MapController.Instance.OnMapData(mapData);
                                      });

            SetMessageProcessCallback((int)Protocols.WorldState.MessageTypes.SetCharacter,
                                      (payload) => Protocols.WorldState.SetCharacter.Parser.ParseFrom(payload),
                                      (message) =>
                                      {
                                          Protocols.WorldState.SetCharacter setCharacter = message as Protocols.WorldState.SetCharacter;
                                          User.Instance.OnSetCharacter(setCharacter);
                                      });

            SetMessageProcessCallback((int)Protocols.WorldState.MessageTypes.TeleportUserStartNotify,
                                      (payload) => Protocols.WorldState.TeleportUserStartNotify.Parser.ParseFrom(payload),
                                      UI.PacketReceiver.Instance.OnTeleportUserStartNotify);
            
            SetMessageProcessCallback((int)Protocols.WorldState.MessageTypes.TeleportToUserFinishNotify,
                                      (payload) => Protocols.WorldState.TeleportToUserFinishNotify.Parser.ParseFrom(payload),
                                      UI.PacketReceiver.Instance.OnTeleportFinishNotify);
            
            SetMessageProcessCallback((int)Protocols.WorldState.MessageTypes.NearZoneNotify,
                                      (payload) => Protocols.WorldState.NearZoneNotify.Parser.ParseFrom(payload),
                                      message =>
                                      {
	                                      if (message is NearZoneNotify notify)
	                                      {
		                                      WorldState.PacketReceiver.Instance.RaiseNearZoneNotify(notify);   
	                                      }
                                      });
        }
    }
}