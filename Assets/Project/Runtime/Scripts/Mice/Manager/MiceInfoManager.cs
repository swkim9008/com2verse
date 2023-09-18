/*===============================================================
* Product:		Com2Verse
* File Name:	MiceInfo.cs
* Developer:	ikyoung
* Date:			2023-04-04 14:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;
using System;

namespace Com2Verse.Mice
{
	public sealed partial class MiceInfoManager : Singleton<MiceInfoManager>, IDisposable
	{
        private Action _onMiceInfoChanged;
		
		public event Action OnMiceInfoChangedEvent
		{
			add
			{
				_onMiceInfoChanged -= value;
				_onMiceInfoChanged += value;
			}
			remove => _onMiceInfoChanged -= value;
		}
		
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly]
		private MiceInfoManager() { }

		public void Initialize() 
		{
			this.ParticipantInfo = new MiceParticipantInfo();
			this.SessionQuestionInfo = new MiceSessionQuestionInfo();
        }

		public void Dispose() 
		{
			this.ParticipantInfo = null;
			this.SessionQuestionInfo = null;
        }

		public void Notify()
		{
			_onMiceInfoChanged?.Invoke();
		}

        public MiceEventInfo GetEventInfo(long eventID)
        {
			return this.EventInfos.TryGetValue(eventID, out var info) ? info : null;
        }

        public MiceProgramInfo GetProgramInfo(MiceEventID eventID, MiceProgramID programID)
        {
	        return GetEventInfo(eventID)?.GetProgramInfo(programID) ?? null;
        }
        
		public MiceSessionInfo GetSessionInfo(MiceEventID eventID, MiceSessionID sessionID)
		{
			return GetEventInfo(eventID)?.GetSessionInfo(sessionID) ?? null;
        }
    }

    public sealed partial class MiceInfoManager
	{
		public MiceParticipantInfo ParticipantInfo { get; private set; }
		public MiceSessionQuestionInfo SessionQuestionInfo { get; private set; }
	}
}




