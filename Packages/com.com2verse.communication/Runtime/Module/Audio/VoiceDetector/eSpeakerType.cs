/*===============================================================
* Product:		Com2Verse
* File Name:	eSpeakerType.cs
* Developer:	urun4m0r1
* Date:			2022-06-13 12:09
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2Verse.Communication
{
	[Flags]
	public enum eSpeakerType
	{
		NONE       = 0,
		EVERYTHING = ~NONE,
		SPEAKING   = 1 << 0, // 음성 감지시 즉시 부여
		SPEAKER    = 1 << 1, // 일정 시간 이상 SPEAKING 지위 유지시 부여
	}

	public static class SpeakerTypeExtensions
	{
		public static bool HasFlagFast(this eSpeakerType value, eSpeakerType flag) => (value & flag) != 0;
	}
}
