/*===============================================================
* Product:		Com2Verse
* File Name:	AudioPublishSettings.cs
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
	public interface IReadOnlyAudioPublishSettings
	{
		int Bitrate { get; }

		event Action<IReadOnlyAudioPublishSettings>? SettingsChanged;

		string GetInfoText();
	}

	public sealed class AudioPublishSettings : IReadOnlyAudioPublishSettings
	{
		public event Action<IReadOnlyAudioPublishSettings>? SettingsChanged;

		private AudioPublishProperty _property;

		public AudioPublishSettings() : this(AudioPublishProperty.Fallback) { }

		public AudioPublishSettings(AudioPublishProperty property)
		{
			Property = property;
		}

		public string GetInfoText() => Property.ToString();

		public AudioPublishProperty Property
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

		public int Bitrate
		{
			get => Property.Bitrate;
			private set => ChangeSettings(bitrate: value);
		}

		public void ResetSettings()
		{
			ChangeSettings(
				bitrate: default);
		}

		public void ChangeSettings(int bitrate = -1)
		{
			if (bitrate <= 0) bitrate = Property.Bitrate;

			Property = new AudioPublishProperty(bitrate);
		}

		public void ChangeSettings(AudioPublishProperty property)
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

		public static AudioPublishProperty CreateSettings(AudioQualitySettings? settings, eAudioType audioType)
		{
			if (settings == null)
			{
				return AudioPublishProperty.Fallback;
			}

			return audioType switch
			{
				eAudioType.DEFAULT   => new(settings.VoiceBitrate),
				eAudioType.SCREEN    => new(settings.ScreenAudioBitrate),
				_                    => throw new ArgumentOutOfRangeException(nameof(audioType), audioType, null!),
			};
		}
	}
}
