/*===============================================================
* Product:		Com2Verse
* File Name:	VideoInfo.cs
* Developer:	urun4m0r1
* Date:			2022-06-24 15:26
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
	public interface IReadOnlyVideoPublishSettings
	{
		int   Fps     { get; }
		int   Bitrate { get; }
		float Scale   { get; }

		event Action<IReadOnlyVideoPublishSettings>? SettingsChanged;

		string GetInfoText();
	}

	public sealed class VideoPublishSettings : IReadOnlyVideoPublishSettings
	{
		public event Action<IReadOnlyVideoPublishSettings>? SettingsChanged;

		private VideoPublishProperty _property;

		public VideoPublishSettings() : this(VideoPublishProperty.Fallback) { }

		public VideoPublishSettings(VideoPublishProperty property)
		{
			Property = property;
		}

		public string GetInfoText() => Property.ToString();

		public VideoPublishProperty Property
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

		public int Fps
		{
			get => Property.Fps;
			private set => ChangeSettings(fps: value);
		}

		public int Bitrate
		{
			get => Property.Bitrate;
			private set => ChangeSettings(bitrate: value);
		}

		public float Scale
		{
			get => Property.Scale;
			private set => ChangeSettings(scale: value);
		}

		public void ResetSettings()
		{
			ChangeSettings(
				fps: default
			  , bitrate: default);
		}

		public void ChangeSettings(int fps = -1, int bitrate = -1, float scale = -1)
		{
			if (fps     <= 0) fps     = Property.Fps;
			if (bitrate <= 0) bitrate = Property.Bitrate;
			if (scale   <= 0) scale   = Property.Scale;

			Property = new VideoPublishProperty(fps, bitrate, scale);
		}

		public void ChangeSettings(VideoPublishProperty property)
		{
			Property = property;
		}

		public void ChangeSettings(VideoResolutionSettings? settings, eVideoType videoType)
		{
			Property = CreateSettings(settings, videoType);
		}

		public void ChangeSettings(eCommunicationType communicationType, eVideoType videoType)
		{
			VideoResolutionSettings? settings = null;
			VideoResolution.Instance.Table?.TryGetValue(communicationType.CastInt(), out settings);
			ChangeSettings(settings, videoType);
		}

		public static VideoPublishProperty CreateSettings(VideoResolutionSettings? settings, eVideoType videoType)
		{
			if (settings == null)
			{
				return VideoPublishProperty.Fallback;
			}

			return videoType switch
			{
				eVideoType.DEFAULT          => new(settings.VideoFps, settings.VideoBitrate, 1f),
				eVideoType.SCREEN           => new(settings.ScreenFps, settings.ScreenBitrate, 1f),
				eVideoType.SCREEN_THUMBNAIL => new(default, default, 1f),
				_                           => throw new ArgumentOutOfRangeException(nameof(videoType), videoType, null!),
			};
		}
	}
}
