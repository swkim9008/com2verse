/*===============================================================
* Product:		Com2Verse
* File Name:	StopwatchViewModel.cs
* Developer:	haminjeong
* Date:			2023-05-26 15:08
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Network;

namespace Com2Verse.UI
{
	[ViewModelGroup("PlayContents")]
	public class StopwatchViewModel : ViewModelBase
	{
		public DateTime StartTime      { get; set; }
		public TimeSpan ResultTimeSpan { get; private set; }
		
		private string _timerString;
		public string TimerString
		{
			get => _timerString;
			set => SetProperty(ref _timerString, value);
		}

		public void UpdateTimerString()
		{
			ResultTimeSpan = MetaverseWatch.NowDateTime - StartTime;
			TimerString    = ResultTimeSpan.ToString(@"hh\:mm\:ss\.ff");
		}
	}
}
