/*===============================================================
* Product:		Com2Verse
* File Name:	MicePackageInfo.cs
* Developer:	klizzard
* Date:			2023-07-21 10:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Linq;

namespace Com2Verse.Mice
{
    public sealed partial class MicePackageInfo : MiceBaseInfo
    {
        public MiceWebClient.Entities.PackageEntity PackageEntity { get; private set; }

        public MicePackageInfo(MiceWebClient.Entities.PackageEntity packageEntity)
        {
            Sync(packageEntity);
        }

        public void Sync(MiceWebClient.Entities.PackageEntity packageEntity)
        {
            PackageEntity = packageEntity;
        }
    }
}

namespace Com2Verse.Mice
{
    public static partial class MiceWebClient
    {
        public static partial class Entities
        {
            public partial class PackageEntity
            {
                private WeakReference<MiceEventInfo> _eventInfo;

                public MiceEventInfo EventInfo
                {
                    get
                    {
                        if (_eventInfo != null && _eventInfo.TryGetTarget(out var target))
                            return target;
                        
                        target = MiceInfoManager.Instance.GetEventInfo(EventId);

                        if (_eventInfo != null) _eventInfo.SetTarget(target);
                        else _eventInfo = new WeakReference<MiceEventInfo>(target);

                        return target;
                    }
                }

                //public DateTime StartDateTimeInSessions =>
                //    TicketInfoList?.Min(e => e.SessionInfo?.StartUTCDatetime ?? DateTime.MaxValue) ?? default;

                //public DateTime EndDateTimeInSessions =>
                //    TicketInfoList?.Max(e => e.SessionInfo?.EndUTCDatetime ?? DateTime.MinValue) ?? default;
            }

            public partial class TicketInfoDetailEntity
            {
                private WeakReference<MiceProgramInfo> _programInfo;
                private WeakReference<MiceSessionInfo> _sessionInfo;

                public MiceProgramInfo ProgramInfo
                {
                    get
                    {
                        if (_programInfo != null && _programInfo.TryGetTarget(out var target))
                            return target;
                        
                        target = MiceInfoManager.Instance.GetProgramInfo(
                            EventId.ToMiceEventID(),
                            ProgramId.ToMiceProgramID());
                            
                        if (_programInfo != null) _programInfo.SetTarget(target);
                        else _programInfo = new WeakReference<MiceProgramInfo>(target);

                        return target;
                    }
                }
                
                public MiceSessionInfo SessionInfo
                {
                    get
                    {
                        if (_sessionInfo != null && _sessionInfo.TryGetTarget(out var target))
                            return target;
                        
                        target = MiceInfoManager.Instance.GetSessionInfo(
                            EventId.ToMiceEventID(),
                            SessionId.ToMiceSessionID());
                            
                        if (_sessionInfo != null) _sessionInfo.SetTarget(target);
                        else _sessionInfo = new WeakReference<MiceSessionInfo>(target);

                        return target;
                    }
                }
            }
        }
    }
}