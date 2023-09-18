/*===============================================================
* Product:		Com2Verse
* File Name:	AudioSettings.cs
* Developer:	urun4m0r1
* Date:			2022-08-29 20:12
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Data;
using Com2Verse.Utils;

namespace Com2Verse.Communication
{
	public enum eAudioType
	{
		DEFAULT,
		SCREEN,
	}

	public interface IReadOnlyAudioSettings
	{
		/// <summary>
		/// Buffer audio clip length (ms)
		/// </summary>
		int Length { get; }

		/// <summary>
		/// Frequency in Hz.
		/// </summary>
		int Frequency { get; }

		event Action<IReadOnlyAudioSettings>? SettingsChanged;

		string GetInfoText();
	}

	public sealed class AudioSettings : IReadOnlyAudioSettings
	{
		public event Action<IReadOnlyAudioSettings>? SettingsChanged;

		private AudioProperty _property;

		public AudioSettings() : this(AudioProperty.Fallback) { }

		public AudioSettings(AudioProperty property)
		{
			Property = property;
		}

		public string GetInfoText() => Property.ToString();

		public AudioProperty Property
		{
			get => _property;
			set
			{
				if (_property == value)
					return;

				_property = value;
				SettingsChanged?.Invoke(this);
			}
		}

		public int Length
		{
			get => Property.Length;
			private set => ChangeSettings(length: value);
		}

		public int Frequency
		{
			get => Property.Frequency;
			private set => ChangeSettings(frequency: value);
		}

		public void ResetSettings()
		{
			ChangeSettings(
				length: default
			  , frequency: default);
		}

		public void ChangeSettings(int length = -1, int frequency = -1)
		{
			if (length    <= 0) length    = Property.Length;
			if (frequency <= 0) frequency = Property.Frequency;

			Property = new AudioProperty(length, frequency);
		}

		public void ChangeSettings(AudioProperty property)
		{
			Property = property;
		}

		public void ChangeSettings(AudioQualitySettings? settings, eAudioType audioType)
		{
			Property = CreateSettings(settings, audioType);
		}

		public void ChangeSettings(eCommunicationType communicationType, eAudioType audioType)
		{
			AudioQualitySettings? settings = null;
			AudioQuality.Instance.Table?.TryGetValue(communicationType.CastInt(), out settings);
			ChangeSettings(settings, audioType);
		}

		public static AudioProperty CreateSettings(AudioQualitySettings? settings, eAudioType audioType)
		{
			if (settings == null)
			{
				return AudioProperty.Fallback;
			}

			return audioType switch
			{
				eAudioType.DEFAULT   => new(settings.VoiceLength, settings.VoiceFrequency),
				eAudioType.SCREEN    => new(settings.ScreenAudioLength, settings.ScreenAudioFrequency),
				_                    => throw new ArgumentOutOfRangeException(nameof(audioType), audioType, null!),
			};
		}
	}
}
