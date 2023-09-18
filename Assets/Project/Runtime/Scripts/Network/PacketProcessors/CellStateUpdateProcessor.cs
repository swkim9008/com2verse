/*===============================================================
* Product:    Com2Verse
* File Name:  CellStateUpdateProcessor.cs
* Developer:  haminjeong
* Date:       2022-09-15 14:58
* History:    
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;

namespace Com2Verse.Network
{
    [UsedImplicitly]
    [Channel(Protocols.Channels.CellStateUpdate)]
    public sealed class CellStateUpdateProcessor : BaseMessageProcessor
    {
        public override void Initialize()
        {
            SetMessageProcessCallback((int)Protocols.CellStateUpdate.MessageTypes.CellStateUpdate,
                                      (payload) => Protocols.CellStateUpdate.CellStateUpdate.Parser.ParseFrom(payload),
                                      (message) =>
                                      {
                                          Protocols.CellStateUpdate.CellStateUpdate map = message as Protocols.CellStateUpdate.CellStateUpdate;
                                          MapController.Instance.OnMapUpdate(map);
                                      });
        }
    }
}