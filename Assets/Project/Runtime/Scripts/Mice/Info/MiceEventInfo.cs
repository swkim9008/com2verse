/*===============================================================
* Product:		Com2Verse
* File Name:	MiceEventInfo.cs
* Developer:	seaman2000
* Date:			2023-04-10 14:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;

namespace Com2Verse.Mice
{
    public sealed partial class MiceEventInfo : MiceBaseInfo
    {
	    public MiceWebClient.Entities.EventEntity EventEntity { get; private set; }
	    public Dictionary<MiceProgramID, MiceProgramInfo> ProgramInfos { get; private set; } = new();
        public List<MiceSessionInfo> SessionInfoList { get; private set; } = new();
        public DateTime StartDatetime { get; private set; } = default;
        public DateTime EndDatetime { get; private set; } = default;
		public string StrTitle { get; private set; } = default;

		public bool IsTutorialSkip { get; private set; } = default;

		public void Sync(MiceWebClient.Entities.EventEntity eventEntity)
		{
			this.SyncVersion++;
			SetSyncState(SyncState.Synced, this.SyncVersion);

			EventEntity = eventEntity;
            ProgramInfos.Clear();
            SessionInfoList.Clear();

            foreach (var entry in eventEntity.Programs)
			{
				var programID = entry.ProgramId.ToMiceProgramID();
                if (!ProgramInfos.TryGetValue(programID, out MiceProgramInfo value))
				{
					value = new MiceProgramInfo();
					value.OnAdd(this);
                    ProgramInfos.Add(programID, value);
                }
				value.Sync(entry);
				value.OnUpdate(this);

                SessionInfoList.AddRange(value.SessionInfos.Values);
            }

            // eventInfo
            StartDatetime = eventEntity.StartDatetime.ToLocalTime();
            EndDatetime = eventEntity.EndDatetime.ToLocalTime();
			StrTitle = eventEntity.EventName;
        }

		public MiceWebClient.Entities.SurveyProgramEntity GetAvailableSurvey(long programID)
		{
			foreach (var program in EventEntity.Programs)
			{
				if (program.ProgramId == programID)
				{
					foreach (var survey in program.Surveys)
					{
						if (!MiceInfoManager.Instance.MyUserInfo.CheckIsCompletedSurvey(survey.SurveyNo))
						{
							return survey;
						}
					}
				}
			}
			return null;
		}

		public MiceProgramInfo GetProgramInfo(MiceProgramID programId)
		{
			return this.ProgramInfos.TryGetValue(programId, out var info) ? info : null;
		}
		public MiceProgramInfo GetProgramInfo(MiceSessionID sessionId)
		{
			foreach (var program in ProgramInfos)
			{
				if (program.Value.SessionInfos.ContainsKey(sessionId))
				{
					return program.Value;
				}
			}

			return null;
		}

		public MiceSessionInfo GetSessionInfo(MiceSessionID sessionId)
		{
			return this.SessionInfoList.Find(a => a.ID == sessionId);
		}

		public void SetTutorialSkip()
		{
			IsTutorialSkip = true;
		}
    }
}