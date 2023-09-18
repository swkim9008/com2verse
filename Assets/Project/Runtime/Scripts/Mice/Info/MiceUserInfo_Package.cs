/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUserPackageInfo.cs
* Developer:	klizzard
* Date:			2023-07-21 10:47
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Com2Verse.Mice
{
    public partial class MiceUserInfo
    {
        public Dictionary<string, MiceWebClient.Entities.UserPackageInfo> PackageInfos { get; } = new();

        public void AddOrUpdatePackageInfo(MiceWebClient.Entities.UserPackageInfo packageEntity)
        {
            if (packageEntity == null || string.IsNullOrEmpty(packageEntity.PackageId)) return;

            if (PackageInfos.ContainsKey(packageEntity.PackageId))
                PackageInfos[packageEntity.PackageId] = packageEntity;
            else
                PackageInfos.Add(packageEntity.PackageId, packageEntity);
        }
        
        public bool HasTicketWithEventID(long eventId)
        {
            foreach (var package in PackageInfos)
            {
                if (package.Value.EventId == eventId && package.Value.TicketInfoList?.Count() > 0)
                    return true;
            }

            return false;
        }

        public bool HasTicketWithSessionID(long sessionId)
        {
            foreach (var package in PackageInfos)
            {
                if (package.Value.TicketInfoList == null)
                    continue;

                foreach (var ticket in package.Value.TicketInfoList)
                {
                    if (ticket.SessionId == sessionId)
                        return true;
                }
            }

            return false;
        }

        public MiceWebClient.eMiceAuthorityCode GetAuthorityCodeWithEventID(long eventId)
        {
            foreach (var package in PackageInfos)
            {
                if (package.Value.EventId == eventId)
                    return package.Value.AuthorityCode;
            }

            return MiceWebClient.eMiceAuthorityCode.NORMAL;
        }

        public MiceWebClient.eMiceAuthorityCode GetAuthorityCodeWithSessionID(long sessionId)
        {
            foreach (var package in PackageInfos)
            {
                if (package.Value.TicketInfoList == null)
                    continue;

                foreach (var ticket in package.Value.TicketInfoList)
                {
                    if (ticket.SessionId == sessionId)
                        return package.Value.AuthorityCode;
                }
            }

            return MiceWebClient.eMiceAuthorityCode.NORMAL;
        }
    }
}

namespace Com2Verse.Mice
{
    public static partial class MiceWebClient
    {
        public static partial class Entities
        {
            public partial class UserPackageInfo
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

                public DateTime StartDateTimeInSessions =>
                    TicketInfoList?.Min(e => e.StartDateTime) ?? default;

                public DateTime EndDateTimeInSessions =>
                    TicketInfoList?.Max(e => e.EndDateTime) ?? default;
            }
        }
    }
}