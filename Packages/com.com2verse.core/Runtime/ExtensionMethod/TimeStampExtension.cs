/*===============================================================
* Product:		Com2Verse
* File Name:	TimeStampExtension.cs
* Developer:	haminjeong
* Date:			2022-07-15 13:58
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.Extension
{
	public static class TimeStampExtension
	{
		public static string ToFormattedDateString(this long timeStamp, string format)
		{
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			DateTime now = origin.AddMilliseconds(timeStamp);
			return now.ToString(format);
		}
	}
}
