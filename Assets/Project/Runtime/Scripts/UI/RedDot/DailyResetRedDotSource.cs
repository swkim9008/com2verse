/*===============================================================
 * Product:		Com2Verse
 * File Name:	DailyResetRedDotSource.cs
 * Developer:	yangsehoon
 * Date:		2022-12-06 오전 11:10
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Globalization;
using UnityEngine;

namespace Com2Verse.UI
{
	public class DailyResetRedDotSource : IRedDotSource
	{
		private readonly string _dateTimeFormat = "yyyy/MM/dd HH:mm:ss";
		private readonly string _playerPrefsKeyPrefix = "_DailyResetRedDotSource";
		private string _playerPrefsKey = string.Empty;
		
		private DateTime _lastCheckTime;
		private DateTime LastCheckTime
		{
			get => _lastCheckTime;
			set
			{
				_lastCheckTime = value;
				
				PlayerPrefs.SetString(_playerPrefsKey, _lastCheckTime.ToUniversalTime().ToString(_dateTimeFormat, CultureInfo.InvariantCulture));
				PlayerPrefs.Save();
			}
		}

		private DateTime Yesterday => DateTime.Now.AddDays(-1);

		public DailyResetRedDotSource(string key)
		{ 
			_playerPrefsKey = string.Concat(_playerPrefsKeyPrefix, key);
			string savedDate = PlayerPrefs.GetString(_playerPrefsKey);
			if (string.IsNullOrEmpty(savedDate))
			{
				_lastCheckTime = Yesterday;
			}
			else
			{
				_lastCheckTime = DateTime.ParseExact(savedDate, _dateTimeFormat, CultureInfo.InvariantCulture).ToLocalTime();
			}
		}
		
		public bool Enabled()
		{
			var now = DateTime.Now;
			return _lastCheckTime < now && _lastCheckTime.DayOfYear != now.DayOfYear;
		}

		public void Activate()
		{
			LastCheckTime = Yesterday;
		}

		public void Deactivate()
		{
			LastCheckTime = DateTime.Now;
		}
	}
}
