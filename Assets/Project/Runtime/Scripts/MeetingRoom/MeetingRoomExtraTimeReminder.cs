/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomClock.cs
* Developer:	tlghks1009
* Date:			2022-11-22 15:37
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Threading;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;

namespace Com2Verse.MeetingReservation
{
	public sealed class MeetingRoomExtraTimeReminder
	{
		private const string UIMeetingRoomCommonPopupEndGuide = "UI_MeetingRoomCommon_Popup_EndGuide";

		public class MeetingRoomTimeInfo
		{
			public DateTime StartTime { get; private set; }
			public DateTime EndTime { get; private set; }
			public TimeSpan RemainingTime => EndTime - MetaverseWatch.NowDateTime;
			public DateTime MeetingExitTime => EndTime.AddMinutes(20);

			private MeetingRoomTimeInfo()
			{
				var meetingInfo = MeetingReservationProvider.EnteredMeetingInfo;

				StartTime = meetingInfo.StartDateTime;
				EndTime = meetingInfo.EndDateTime;
			}

			public static MeetingRoomTimeInfo Create() => new();
		}

		public class MeetingRoomTimeScheduler
		{
			private CancellationTokenSource _cancellationTokenSource;

			private MeetingRoomTimeScheduler() => _cancellationTokenSource = new CancellationTokenSource();

			public async UniTask AddSchedule(TimeSpan delayTimeSpan, string remainingMinutes, Action<string> afterAction)
			{
				if (delayTimeSpan.TotalSeconds > 0)
				{
					await UniTask.Delay(delayTimeSpan, DelayType.Realtime, cancellationToken: _cancellationTokenSource.Token);
				}

				if (_cancellationTokenSource.IsCancellationRequested)
				{
					return;
				}

				afterAction(remainingMinutes);
			}


			public void Reset()
			{
				_cancellationTokenSource?.Cancel();
				_cancellationTokenSource?.Dispose();
				_cancellationTokenSource = null;
			}

			public static MeetingRoomTimeScheduler Create() => new();
		}


		private static MeetingRoomTimeInfo _meetingRoomTimeInfo;

		private static MeetingRoomTimeScheduler _meetingRoomTimeScheduler;

		public static void Start()
		{
			if (MeetingReservationProvider.EnteredMeetingInfo == null)
			{
				C2VDebug.LogErrorMethod(nameof(MeetingRoomExtraTimeReminder), "EnteredMeetingInfo is null.");
				return;
			}

			_meetingRoomTimeInfo = MeetingRoomTimeInfo.Create();

			_meetingRoomTimeScheduler = MeetingRoomTimeScheduler.Create();


			RegisterSchedules();
		}

		public static void End()
		{
			_meetingRoomTimeInfo = null;

			_meetingRoomTimeScheduler?.Reset();
			_meetingRoomTimeScheduler = null;
		}


		private static void RegisterSchedules()
		{
			Action<string> action = SendToastMessage;

			int scheduleCount = 10;
			int timeInterval = 10;
			var nowDateTime = MetaverseWatch.NowDateTime;

			while (scheduleCount > 0)
			{
				var scheduleTime = _meetingRoomTimeInfo.MeetingExitTime.AddMinutes(-timeInterval);
				var delayTimeSpan = scheduleTime - nowDateTime;

				_meetingRoomTimeScheduler.AddSchedule(delayTimeSpan, timeInterval.ToString(), action).Forget();

				timeInterval--;
				scheduleCount--;
			}
		}


		private static void SendToastMessage(string remainingMinutes)
		{
			UIManager.Instance.SendToastMessage(Localization.Instance.GetString(UIMeetingRoomCommonPopupEndGuide, remainingMinutes));
		}
	}
}
