/*===============================================================
* Product:		Com2Verse
* File Name:	AudioRecordInfo.cs
* Developer:	ydh
* Date:			2023-05-23 19:29
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.AudioRecord
{
	public sealed class AudioRecordInfo
	{
		public bool RecommendAvailable;
		public int RecommendCount = 0;
		public DateTime CreateDateTime;
		public string FileName;
		public string FilePath;
		public bool IsMine;
		public string RecordName;
		public long BoardSeq;
		public string ObjectId;
		public long AccountId;
	}
}
