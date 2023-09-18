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
	public enum eVideoType
	{
		DEFAULT,
		SCREEN,
		SCREEN_THUMBNAIL,
	}

	public interface IReadOnlyVideoSettings
	{
		int Width   { get; }
		int Height  { get; }
		int Fps     { get; }

		float AspectRatio { get; }

		event Action<IReadOnlyVideoSettings>? SettingsChanged;

		string GetInfoText();
	}

	public sealed class VideoSettings : IReadOnlyVideoSettings
	{
		public event Action<IReadOnlyVideoSettings>? SettingsChanged;

		private VideoProperty _property;

		public VideoSettings() : this(VideoProperty.Fallback) { }

		public VideoSettings(VideoProperty property)
		{
			Property = property;
		}

		public string GetInfoText() => Property.ToString();

		public VideoProperty Property
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

		public int Width
		{
			get => Property.Width;
			private set => ChangeSettings(width: value);
		}

		public int Height
		{
			get => Property.Height;
			private set => ChangeSettings(height: value);
		}

		public int Fps
		{
			get => Property.Fps;
			private set => ChangeSettings(fps: value);
		}

		public float AspectRatio => Property.AspectRatio;

		public void ResetSettings()
		{
			ChangeSettings(
				width: default
			  , height: default
			  , fps: default);
		}

		public void ChangeSettings(int width = -1, int height = -1, int fps = -1)
		{
			if (width   <= 0) width   = Property.Width;
			if (height  <= 0) height  = Property.Height;
			if (fps     <= 0) fps     = Property.Fps;

			Property = new VideoProperty(width, height, fps);
		}

		public void ChangeSettings(VideoProperty property)
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

		public static VideoProperty CreateSettings(VideoResolutionSettings? settings, eVideoType videoType)
		{
			if (settings == null)
			{
				return VideoProperty.Fallback;
			}

			return videoType switch
			{
				eVideoType.DEFAULT          => new(settings.VideoWidth, settings.VideoHeight, settings.VideoFps),
				eVideoType.SCREEN           => new(settings.ScreenWidth, settings.ScreenHeight, settings.ScreenFps),
				eVideoType.SCREEN_THUMBNAIL => new(settings.ScreenThumbnailWidth, settings.ScreenThumbnailHeight, default),
				_                           => throw new ArgumentOutOfRangeException(nameof(videoType), videoType, null!),
			};
		}
	}
}
