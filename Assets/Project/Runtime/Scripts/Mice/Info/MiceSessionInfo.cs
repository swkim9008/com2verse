/*===============================================================
* Product:		Com2Verse
* File Name:	MiceSessionInfo.cs
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
	public sealed partial class MiceSessionInfo : MiceBaseInfo, IEqualityComparer<MiceSessionInfo>
	{
		public class Speaker
		{
			public string StrName { get; private set; }
            public string StrAffiliation { get; private set; } // 소속, 직위
            public string StrDescription { get; private set; }
            public string PhotoUrl { get; private set; }
            

            public Speaker(MiceWebClient.Entities.SessionStaffEntity speaker)
			{
				this.StrName = speaker.StaffName;
                this.StrAffiliation = speaker.DomainName;
                this.StrDescription = speaker.StaffDescription;
                this.PhotoUrl = speaker.PhotoUrl;
            }

            public Speaker(string dummyName)
            {
                StrName = dummyName;
            }
		}

        public class AttachmentFile
        {
            public string StrName { get; private set; }
            public string FileUrl { get; private set; }

            public AttachmentFile(MiceWebClient.Entities.SessionAttachmentFile attachmentFile)
            {
                this.StrName = attachmentFile.FileName;
                this.FileUrl = attachmentFile.FileUrl;
            }
        }

        public MiceWebClient.Entities.SessionEntity SessionEntity { get; private set; }
        
		public MiceSessionID ID { get; private set; } = default;
        public MiceProgramID ProgramID { get; private set; } = default;
        private DateTime StartUTCDatetime { get; set; } = default;
        private DateTime EndUTCDatetime { get; set; } = default;
        public string StrName { get; private set; } = default;
        public string StrHallName { get; private set; } = default;
        public string StrDescription { get; private set; } = default;
        public string StrBannerImageURL { get; private set; } = default;
        public string StrBannerLinkUrl { get; private set; } = default;

        public List<Speaker> Speakers { get; private set; } = new();
        public List<AttachmentFile> AttachmentFiles { get; private set; } = new();


        public DateTime StartDatetime { get { return this.StartUTCDatetime.ToLocalTime(); } }
        public DateTime EndDatetime { get { return this.EndUTCDatetime.ToLocalTime(); } }
        public string StrStartDataTime { get { return this.StartDatetime.ToString(); } }
        public string StrEndDataTime { get { return this.EndDatetime.ToString(); } }
        public int MaxMemeberCount { get; private set; } = default; // 세션 진입 최대 인원
        public bool IsUsePreliminaryQuestion { get; private set; } = default; // 사전 질문

        public void Sync(MiceWebClient.Entities.SessionEntity sessionEntity)
        {
            SessionEntity = sessionEntity;
			this.ID = sessionEntity.SessionId.ToMiceSessionID();
            this.ProgramID = sessionEntity.ProgramId.ToMiceProgramID();
			this.StartUTCDatetime = sessionEntity.StartDatetime;
            this.EndUTCDatetime = sessionEntity.EndDatetime;
			this.StrName = sessionEntity.SessionName;
			this.StrHallName = sessionEntity.HallName;
			this.StrDescription = sessionEntity.SessionDescription;
            this.MaxMemeberCount = SessionEntity.MaxMemberCount;
            this.IsUsePreliminaryQuestion = sessionEntity.IsQuestion; // 사전 질문 기능 사용여부

            if (sessionEntity.EventPopUpBanner != null)
            {
                this.StrBannerImageURL = sessionEntity.EventPopUpBanner.ImageUrl;
                this.StrBannerLinkUrl = sessionEntity.EventPopUpBanner.LinkUrl;
            }
            
            this.Speakers.Clear();
            foreach (var entry in sessionEntity.SessionStaffs)
            {
                if (entry.StaffType == MiceWebClient.eMiceStaffType.SPEAKER)
				{
					this.Speakers.Add(new Speaker(entry));
                }
            }

            this.AttachmentFiles.Clear();
            foreach(var entry in sessionEntity.SessionAttachmentFiles)
            {
                // 빈값이 들어오면 안쓰도록 처리한다.
                if (string.IsNullOrEmpty(entry.FileName) || string.IsNullOrEmpty(entry.FileUrl)) continue;

                this.AttachmentFiles.Add(new AttachmentFile(entry));
            }

            SetSyncState(SyncState.Synced, this.SyncVersion);
		}

        public bool Equals(MiceSessionInfo x, MiceSessionInfo y)
        {
            return x.ID == x.ID;
        }

        public int GetHashCode(MiceSessionInfo obj) => obj.ID.GetHashCode();

        //public override int GetHashCode() => this.ID.GetHashCode();

        public bool IsOpenDay(DateTime dateTime)
        {
            if (!this.ID.IsValid()) return false;
            return this.StartDatetime.DayOfYear <= dateTime.DayOfYear && this.EndDatetime.DayOfYear >= dateTime.DayOfYear;
        }
    }
}




