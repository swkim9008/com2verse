/*===============================================================
* Product:    Com2Verse
* File Name:  InternalCycleProcessor.cs
* Developer:  haminjeong
* Date:       2022-05-09 14:38
* History:    
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;

namespace Com2Verse.Network
{
    [UsedImplicitly]
    [Channel(Protocols.Channels.InternalCycle)]
    public sealed class InternalCycleProcessor : BaseMessageProcessor
    {
        public override void Initialize()
        {
            SetMessageProcessCallback((int)Protocols.InternalCycle.MessageTypes.SyncClock,
                                      (payload) => Protocols.InternalCycle.SyncClock.Parser.ParseFrom(payload),
                                      (message) =>
                                      {
                                          Protocols.InternalCycle.SyncClock syncClock = message as Protocols.InternalCycle.SyncClock;
                                          int rtt = NetworkManager.Instance.GetRtt();
                                          ServerTime.SetServerTime(syncClock!.Time, rtt);
                                          User.Instance.OnClockSynced();
                                      });
        }
    }
}