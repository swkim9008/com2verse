/*===============================================================
* Product:		Com2Verse
* File Name:	DateTimeExtension.cs
* Developer:	tlghks1009
* Date:			2022-08-29 13:40
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.UI;
using Protocols;

namespace Com2Verse
{
	public static class DateTimeExtension
	{
		public static ProtoDateTime ToProtoDateTime(this DateTime dateTime)
		{
			var protoDateTime = new ProtoDateTime
			{
				Year = dateTime.Year,
				Month = dateTime.Month,
				Day = dateTime.Day,
				Hour = dateTime.Hour,
				Minute = dateTime.Minute,
				Second = dateTime.Second,
			};
			return protoDateTime;
		}

		public static DateTime ToDateTime(this ProtoDateTime thisProtoDateTime)
		{
			var newDateTime = new DateTime
			(
				thisProtoDateTime.Year,
				thisProtoDateTime.Month,
				thisProtoDateTime.Day,
				thisProtoDateTime.Hour,
				thisProtoDateTime.Minute,
				thisProtoDateTime.Second
			);
			return newDateTime;
		}

		public static DateTime ToUniversalTime(this ProtoDateTime thisProtoDateTime) => ToDateTime(thisProtoDateTime).ToUniversalTime();

		public static DateTime ToLocalTime(this ProtoDateTime thisProtoDateTime) => ToDateTime(thisProtoDateTime).ToLocalTime();

		public static ProtoDateTime ToLocalProtoDateTime(this ProtoDateTime thisProtoDateTime) => ToDateTime(thisProtoDateTime).ToLocalTime().ToProtoDateTime();

		public static string GetLocalizationKeyOfDayOfWeek(DayOfWeek dayOfWeek)
		{
			return dayOfWeek switch
			{
				DayOfWeek.Sunday    => Localization.Instance.GetString("UI_Common_Sun"),
				DayOfWeek.Monday    => Localization.Instance.GetString("UI_Common_Mon"),
				DayOfWeek.Tuesday   => Localization.Instance.GetString("UI_Common_Tue"),
				DayOfWeek.Wednesday => Localization.Instance.GetString("UI_Common_Wed"),
				DayOfWeek.Thursday  => Localization.Instance.GetString("UI_Common_Thur"),
				DayOfWeek.Friday    => Localization.Instance.GetString("UI_Common_Fri"),
				DayOfWeek.Saturday  => Localization.Instance.GetString("UI_Common_Sat"),
				_                   => string.Empty
			};
		}

		public static string GetLocalizationKeyOfDayOfWeekFullName(DayOfWeek dayOfWeek)
		{
			return dayOfWeek switch
			{
				DayOfWeek.Sunday    => Localization.Instance.GetString("UI_Common_Sunday"),
				DayOfWeek.Monday    => Localization.Instance.GetString("UI_Common_Monday"),
				DayOfWeek.Tuesday   => Localization.Instance.GetString("UI_Common_Tuesday"),
				DayOfWeek.Wednesday => Localization.Instance.GetString("UI_Common_Wednesday"),
				DayOfWeek.Thursday  => Localization.Instance.GetString("UI_Common_Thursday"),
				DayOfWeek.Friday    => Localization.Instance.GetString("UI_Common_Friday"),
				DayOfWeek.Saturday  => Localization.Instance.GetString("UI_Common_Saturday"),
				_                   => string.Empty
			};
		}

		public static string GetDateTimeFullName(DateTime dateTime) => $"{dateTime.Year}.{dateTime.Month:00}.{dateTime.Day:00} ({GetLocalizationKeyOfDayOfWeekFullName(dateTime.DayOfWeek)})";
		public static string GetDateTimeOnlyDate(DateTime dateTime) => $"{dateTime.Year}.{dateTime.Month:00}.{dateTime.Day:00}";
		public static string GetDateTimeOnlyDayOfWeek(DateTime dateTime) => $"({GetLocalizationKeyOfDayOfWeekFullName(dateTime.DayOfWeek)})";
		public static string GetDateTimeShortName(DateTime dateTime) => $"{dateTime.Year}.{dateTime.Month:00}.{dateTime.Day:00} ({GetLocalizationKeyOfDayOfWeek(dateTime.DayOfWeek)})";
		public static string GetDateTimeForSpaceCode(DateTime dateTime) => $"{(dateTime.Year % 100):00}{dateTime.Month:00}{dateTime.Day:00}";

		/// <summary>
		/// DateTime의 시, 분, 초 모두 0으로 세팅
		/// </summary>
		/// <param name="dateTime"></param>
		public static void SetZeroHms(this ref DateTime dateTime)
		{
			dateTime.SetZeroHour();
			dateTime.SetZeroMinute();
			dateTime.SetZeroSecond();
		}
		public static void SetZeroHour(this ref DateTime dateTime)
		{
			if (dateTime.Hour == 0) return;

			dateTime = dateTime.AddHours(-dateTime.Hour);
		}

		public static void SetZeroMinute(this ref DateTime dateTime)
		{
			if (dateTime.Minute == 0) return;

			dateTime = dateTime.AddMinutes(-dateTime.Minute);
		}

		public static void SetZeroSecond(this ref DateTime dateTime)
		{
			if (dateTime.Second == 0) return;

			dateTime = dateTime.AddSeconds(-dateTime.Second);
		}
		public static void SetHour(this ref DateTime dateTime, int hour)
		{
			if (hour is < 0 or > 23) return;

			dateTime.SetZeroHour();
			dateTime = dateTime.AddHours(hour);
		}

		public static void SetMinute(this ref DateTime dateTime, int minute)
		{
			if (minute is < 0 or > 59) return;

			dateTime.SetZeroMinute();
			dateTime = dateTime.AddMinutes(minute);
		}

		public static void SetSecond(this ref DateTime dateTime, int second)
		{
			if (second is < 0 or > 59) return;

			dateTime.SetZeroSecond();
			dateTime = dateTime.AddSeconds(second);
		}
	}
}
