/*===============================================================
* Product:		Com2Verse
* File Name:	MiceInfoManager_Event.cs
* Developer:	ikyoung
* Date:			2023-04-14 12:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Mice
{
    public sealed partial class MiceInfoManager
    {
        public Dictionary<long, MiceEventInfo> EventInfos = new Dictionary<long, MiceEventInfo>();

        public async UniTask SyncEventEntity()
        {
            var result = await MiceWebClient.Event.EventGet();
            if (result)
            {
                var datas = result.Data;
                foreach (var entry in datas)
                {
                    if (EventInfos.TryGetValue(entry.EventId, out MiceEventInfo value))
                    {
                        value.Sync(entry);
                    }
                    else
                    {
                        var info = new MiceEventInfo();
                        EventInfos.Add(entry.EventId, info);
                        info.Sync(entry);
                    }

                    EventTutorialSkipClickedPrefs.Load(entry.EventId);
                }
            }

            await UniTask.CompletedTask;
        }
        
        public async UniTask SyncEventEntity(long eventID)
        {
            var result = await MiceWebClient.Event.EventGet_EventId(eventID);
            if (result)
            {
                var entry = result.Data;
                {
                    if (EventInfos.TryGetValue(entry.EventId, out MiceEventInfo value))
                    {
                        value.Sync(entry);
                    }
                    else
                    {
                        var info = new MiceEventInfo();
                        EventInfos.Add(entry.EventId, info);
                        info.Sync(entry);
                    }

                    EventTutorialSkipClickedPrefs.Load(entry.EventId);
                }
            }

            await UniTask.CompletedTask;
        }
    }
}