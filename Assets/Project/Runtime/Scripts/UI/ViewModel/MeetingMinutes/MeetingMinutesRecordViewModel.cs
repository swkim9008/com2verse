/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingMinutesViewModel.cs
* Developer:	ksw
* Date:			2023-05-12 12:55
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Chat;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class MeetingMinutesRecordViewModel : ViewModel, IDisposable
	{
		[UsedImplicitly] public CommandHandler<bool> SetMeetingMinutesRecordPlay { get; }
		[UsedImplicitly] public CommandHandler       OpenHoverPopup              { get; }
		[UsedImplicitly] public CommandHandler       CloseHoverPopup             { get; }

		private bool     _isMeetingMinutesRecordPlay;
		private bool     _isRecordPossible;
		private string   _recordingTime;
		private DateTime _recordStartTime;
		private int      _alreadyRecodingTime;
		private TimeSpan _totalRecordTime;

		private static readonly TimeSpan OffRecordingUIDelay = TimeSpan.FromSeconds(1);

		private readonly Color _normalTimeColor = new Color(0.125f, 0.157f, 0.211f);
		private readonly Color _limitTimeColor  = new Color(1f,     0.4f,   0.36f);
		private          Color _recordingTextColor;

		private readonly int _maxRecordTime = 120;

		private bool _isOpen;

		private bool _isVisiblePopup;
		private bool _playOpenAnimation;
		private bool _playCloseAnimation;

		private bool  _isAvailableVoiceRecord;
		private float _voiceCheckCount = 0f;
		private float _recordStopTime  = 0f;

		private long _meetingId;
		private bool _isOrganizer;

		public event Action<bool> ListToggled;

		public MeetingMinutesRecordViewModel()
		{
			SetMeetingMinutesRecordPlay = new CommandHandler<bool>(OnClickMeetingMinutesPlay);
			OpenHoverPopup              = new CommandHandler(OnOpenHoverPopup);
			CloseHoverPopup             = new CommandHandler(OnCloseHoverPopup);
			RecordingTextColor          = _normalTimeColor;
			_isRecordPossible           = true;
			IsOpen                      = false;
			_meetingId                  = MeetingReservationProvider.EnteredMeetingInfo.MeetingId;
			_isOrganizer                = MeetingReservationProvider.IsOrganizer(User.Instance.CurrentUserData.ID);
			if (_isOrganizer)
				StopRecord();
		}

		public bool IsMeetingMinutesRecordPlay
		{
			get => _isMeetingMinutesRecordPlay;
			set
			{
				MeetingReservationProvider.IsRecording = value;
				SetProperty(ref _isMeetingMinutesRecordPlay, value);
			}
		}

		public string RecordingTime
		{
			get => _recordingTime;
			set => SetProperty(ref _recordingTime, value);
		}

		public Color RecordingTextColor
		{
			get => _recordingTextColor;
			set => SetProperty(ref _recordingTextColor, value);
		}

		public bool IsOpen
		{
			get => _isOpen;
			set
			{
				if (value)
				{
					if (true != _isOpen)
						Initialize();
				}
				else
				{
					if (_isMeetingMinutesRecordPlay)
					{
						CloseMeetingMinutesRecord();
						return;
					}
				}
				_isOpen = value;
				ListToggled?.Invoke(value);
			}
		}

		public bool IsVisiblePopup
		{
			get => _isVisiblePopup;
			set
			{
				_isVisiblePopup = value;
				SetProperty(ref _isVisiblePopup, value);

				PlayVisibleAnimation();
			}
		}

		public bool PlayOpenAnimation
		{
			get => _playOpenAnimation;
			set
			{
				_playOpenAnimation = value;
				if (_playOpenAnimation)
					SetProperty(ref _playOpenAnimation, value);
			}
		}

		public bool PlayCloseAnimation
		{
			get => _playCloseAnimation;
			set
			{
				_playCloseAnimation = value;
				if (_playCloseAnimation)
					SetProperty(ref _playCloseAnimation, value);
			}
		}

		private void OnClickMeetingMinutesPlay(bool isPlay)
		{
			if (!_isRecordPossible)
			{
				// 회의록 녹음 시간을 전부 사용하였습니다
				UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_OtherMenu_Stt_RecordingLimitReached_Toast"));
				return;
			}

			if (isPlay)
			{
				Commander.Instance.RequestRecordStartAsync(MeetingReservationProvider.EnteredMeetingInfo.MeetingId, response =>
				{
					_recordStartTime = DateTime.Now.AddSeconds(-_alreadyRecodingTime);
					_recordStopTime  = 0f;
					UIManager.Instance.AddUpdateListener(OnUpdate);
					IsMeetingMinutesRecordPlay = true;
					ChatManager.Instance.BroadcastCustomNotify(ChatManager.CustomDataType.RECORD_START_NOTIFY);
				}).Forget();
			}
			else
			{
				// 회의록 기록을 중단하시겠습니까?
				CloseMeetingMinutesRecord();
			}
		}

		private void CloseMeetingMinutesRecord()
		{
			UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString("UI_Common_Popup_Title_Text"), Localization.Instance.GetString("UI_ConnectingApp_OtherMenu_Stt_StopNotePopup_Text"),
			                                  StopRecord);
		}

		private void Initialize()
		{
			Commander.Instance.RequestRecordUsageAsync(MeetingReservationProvider.EnteredMeetingInfo.MeetingId, response =>
			{
				_alreadyRecodingTime       = response.Value.Data.ActiveDurationSecs;
				RecordingTime              = ZString.Format("{0:00}:{1:00}", _alreadyRecodingTime / 60, _alreadyRecodingTime % 60);
				RecordingTextColor         = _normalTimeColor;
				_isRecordPossible          = _alreadyRecodingTime <= 7200;
				IsMeetingMinutesRecordPlay = false;
				_isAvailableVoiceRecord    = MeetingReservationProvider.IsAvailableVoiceData();
			}).Forget();
			OnCloseHoverPopup();
			_isOrganizer = MeetingReservationProvider.IsOrganizer(User.Instance.CurrentUserData.ID);
		}

		public void Dispose()
		{
			UIManager.InstanceOrNull?.RemoveUpdateListener(OnUpdate);
			if (_isOrganizer)
				StopRecord();
		}

		private void OnUpdate()
		{
			_voiceCheckCount += Time.deltaTime;
			if (_voiceCheckCount > 1f)
			{
				_voiceCheckCount        = 0f;
				_isAvailableVoiceRecord = MeetingReservationProvider.IsAvailableVoiceData();
			}

			if (!_isAvailableVoiceRecord)
			{
				_recordStopTime += Time.deltaTime;
				return;
			}

			_totalRecordTime = DateTime.Now - _recordStartTime - TimeSpan.FromSeconds(_recordStopTime);
			RecordingTime    = ZString.Format(_totalRecordTime.TotalMinutes - _recordStopTime < 100 ? "{0:00}:{1:00}" : "{0:000}:{1:00}", (int)_totalRecordTime.TotalMinutes, _totalRecordTime.Seconds);

			if (_totalRecordTime.TotalMinutes >= _maxRecordTime)
			{
				C2VDebug.Log("Record Time Over!");
				UIManager.Instance.RemoveUpdateListener(OnUpdate);
				RecordingTextColor = _limitTimeColor;
				RecordingTimeOver().Forget();
			}
		}

		private async UniTask RecordingTimeOver()
		{
			await UniTask.Delay(OffRecordingUIDelay);
			IsMeetingMinutesRecordPlay = false;
			_isRecordPossible          = false;
		}

		private void StopRecord(GUIView _ = null)
		{
			if (!Commander.InstanceExists) return;
			Commander.Instance.RequestRecordStopAsync(_meetingId, response =>
			{
				if (UIManager.InstanceExists)
					UIManager.Instance.RemoveUpdateListener(OnUpdate);
				IsMeetingMinutesRecordPlay = false;
				IsOpen                     = false;
				if (ChatManager.InstanceExists)
					ChatManager.Instance.BroadcastCustomNotify(ChatManager.CustomDataType.RECORD_END_NOTIFY);
			}).Forget();
		}

#region UI
		private void OnOpenHoverPopup()
		{
			IsVisiblePopup = true;
		}

		private void OnCloseHoverPopup()
		{
			IsVisiblePopup = false;
		}

		private void PlayVisibleAnimation()
		{
			if (_isVisiblePopup)
			{
				PlayOpenAnimation  = true;
				PlayCloseAnimation = false;
			}
			else
			{
				PlayCloseAnimation = true;
				PlayOpenAnimation  = false;
			}
		}
#endregion
	}
}
