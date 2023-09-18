/*===============================================================
* Product:		Com2Verse
* File Name:	SpeechDetector.cs
* Developer:	urun4m0r1
* Date:			2022-06-13 15:10
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Data;
using Com2Verse.Utils;
using UnityEngine;
using static Com2Verse.Communication.eSpeakerType;

namespace Com2Verse.Communication
{
	public sealed class SpeechDetector : IDisposable
	{
#region SpeakerType
		public event Action<eSpeakerType>? SpeakerTypeChanged;

		private eSpeakerType _speakerType = NONE;

		public eSpeakerType SpeakerType
		{
			get => _speakerType;
			private set
			{
				var prevValue = _speakerType;
				if (prevValue == value)
					return;

				_speakerType = value;
				SpeakerTypeChanged?.Invoke(value);
			}
		}

		public bool IsSpeaker
		{
			get => HasSpeakerType(SPEAKER);
			private set
			{
				if (value) AddSpeakerType(SPEAKER);
				else RemoveSpeakerType(SPEAKER);
			}
		}

		public bool IsSpeaking
		{
			get => HasSpeakerType(SPEAKING);
			private set
			{
				if (value) AddSpeakerType(SPEAKING);
				else RemoveSpeakerType(SPEAKING);
			}
		}

		private bool HasSpeakerType(eSpeakerType    filter) => SpeakerType.IsFilterMatch(filter, eFlagMatchType.CONTAINS);
		private void AddSpeakerType(eSpeakerType    target) => SpeakerType = SpeakerType.AddFlags(target);
		private void RemoveSpeakerType(eSpeakerType target) => SpeakerType = SpeakerType.SubtractFlags(target);
#endregion // SpeakerType

		private static VoiceDetectionSettings? Settings => VoiceDetectionManager.InstanceOrNull?.Settings;

		private float _speechDuration;
		private float _silenceDuration;
		private float _previousTimeStamp;

		private readonly VolumeDetector _volumeDetector;

		public SpeechDetector(VolumeDetector volumeDetector)
		{
			_volumeDetector = volumeDetector;

			_volumeDetector.DetectionStarted += ResetSpeechState;
			_volumeDetector.DetectionStopped += ResetSpeechState;
			_volumeDetector.PpmLevelChanged  += DetectSpeaking;
		}

		public void Dispose()
		{
			_volumeDetector.DetectionStarted -= ResetSpeechState;
			_volumeDetector.DetectionStopped -= ResetSpeechState;
			_volumeDetector.PpmLevelChanged  -= DetectSpeaking;

			ResetSpeechState();
		}

		public void ResetSpeechState()
		{
			IsSpeaking = false;
			IsSpeaker  = false;

			_speechDuration    = 0f;
			_silenceDuration   = 0f;
			_previousTimeStamp = 0f;
		}

		public void DetectSpeaking(float level)
		{
			var wasSpeaking = IsSpeaking;
			IsSpeaking = level > Settings?.DefaultSpeechThreshold;
			UpdateSpeechTime(wasSpeaking, IsSpeaking);
		}

		private void UpdateSpeechTime(bool wasSpeaking, bool isSpeaking)
		{
			var currentTime = Time.realtimeSinceStartup;
			var deltaTime   = currentTime - _previousTimeStamp;
			_previousTimeStamp = currentTime;

			if (isSpeaking)
				_speechDuration += deltaTime;
			else
				_silenceDuration += deltaTime;

			if (isSpeaking && !wasSpeaking)
				_silenceDuration = 0f;

			if (!isSpeaking && wasSpeaking && IsSpeaker)
				_speechDuration = 0f;

			if (_silenceDuration > Settings?.SpeechTimerResetDuration)
				_speechDuration = 0f;

			if (_speechDuration >= Settings?.SpeakerPromotingSpeechDuration)
				IsSpeaker = true;

			if (_silenceDuration >= Settings?.SpeakerDemotingSilenceDuration)
				IsSpeaker = false;
		}
	}
}
