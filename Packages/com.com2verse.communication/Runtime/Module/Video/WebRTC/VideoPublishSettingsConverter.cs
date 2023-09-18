/*===============================================================
 * Product:		Com2Verse
 * File Name:	VideoPublishSettingsConverter.cs
 * Developer:	urun4m0r1
 * Date:		2023-06-27 11:24
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Solution.UnityRTCSdk;
using Com2Verse.Utils;

namespace Com2Verse.Communication.MediaSdk
{
	public static class VideoPublishSettingsConverter
	{
		public static VideoQuality Convert(this IReadOnlyVideoPublishSettings settings)
		{
			var fps     = MathUtil.Clamp(settings.Fps,     VideoPublishProperty.SdkMinLimit.Fps,     VideoPublishProperty.SdkMaxLimit.Fps);
			var bitrate = MathUtil.Clamp(settings.Bitrate, VideoPublishProperty.SdkMinLimit.Bitrate, VideoPublishProperty.SdkMaxLimit.Bitrate);
			var scale   = MathUtil.Clamp(settings.Scale,   VideoPublishProperty.SdkMinLimit.Scale,   VideoPublishProperty.SdkMaxLimit.Scale);

			return new VideoQuality
			{
				Fps     = (uint)fps,
				Bitrate = (ulong)bitrate,
				Scale   = scale,
			};
		}
	}
}
