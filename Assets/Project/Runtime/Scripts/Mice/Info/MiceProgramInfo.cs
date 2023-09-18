/*===============================================================
* Product:		Com2Verse
* File Name:	MiceProgramInfo.cs
* Developer:	seaman2000
* Date:			2023-04-10 14:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/
using System.Collections.Generic;
using Grpc.Core;

namespace Com2Verse.Mice
{
	public sealed partial class MiceProgramInfo : MiceBaseInfo
	{
		public MiceWebClient.Entities.ProgramEntity ProgramEntity { get; private set; }
		public Dictionary<MiceSessionID, MiceSessionInfo> SessionInfos { get; private set; } = new();

		public MiceProgramID ID { get; private set; } = default;
		public string StrTitle { get; private set; } = default;
        public string StrDescription { get; private set; } = default;
        public MiceWebClient.eMiceProgramType ProgramType { get; private set; } = default;
        

        public void Sync(MiceWebClient.Entities.ProgramEntity programEntity)
		{
			SetSyncState(SyncState.Synced, this.SyncVersion);

			this.ID = programEntity.ProgramId.ToMiceProgramID();
			this.StrTitle = programEntity.ProgramName;
			this.StrDescription = programEntity.Description;
			this.ProgramType = programEntity.MiceProgramType;

			ProgramEntity = programEntity;
            SessionInfos.Clear();
			foreach (var entry in programEntity.Sessions)
			{
				var sessionID = entry.SessionId.ToMiceSessionID();
                if (!SessionInfos.TryGetValue(sessionID, out MiceSessionInfo value))
				{
                    value = new MiceSessionInfo();
                    value.OnAdd(this);
                    SessionInfos.Add(sessionID, value);
				}
                value.Sync(entry);
                value.OnUpdate(this);
			}
        }
    }
}




