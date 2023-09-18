/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingReservationTimeHelper.cs
* Developer:	tlghks1009
* Date:			2022-10-07 18:11
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Network;
using Protocols;

namespace Com2Verse
{
	public sealed class MeetingReservationTimeHelper
	{
		public static DateTime NowDateTime => MetaverseWatch.NowDateTime;

		public static DateTime FirstDateTime(int fixedHour)
		{
			var nowDateTime = NowDateTime;
			nowDateTime.SetHour(fixedHour);
			nowDateTime.SetZeroMinute();
			nowDateTime.SetZeroSecond();
			return nowDateTime.AddDays(1 - MetaverseWatch.NowDateTime.Date.Day);
		}

		public static DateTime LastDateTime(DateTime firstDateTime)
		{
			var lastDateTime = firstDateTime.AddMonths(1);

			lastDateTime.SetZeroHms();
			return lastDateTime;
		}

		public static Int64 ToUnixTimeStamp(DateTime dateTime) => (Int64) (dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

		public static Int64 ToUnixTimeStamp(ProtoDateTime dateTime) => (Int64) (dateTime.ToDateTime().ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

		public static DateTime ParseUnixTimeStamp(Int64 timeStamp) => (new DateTime(1970, 1, 1)).AddSeconds(timeStamp).ToLocalTime();
	}
}
