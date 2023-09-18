/*===============================================================
 * Product:		Com2Verse
 * File Name:	AudioPublishSettingsConverter.cs
 * Developer:	urun4m0r1
 * Date:		2023-06-27 11:24
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Utils;

namespace Com2Verse.Communication.MediaSdk
{
	public static class AudioPublishSettingsConverter
	{
		public static ulong Convert(this IReadOnlyAudioPublishSettings settings)
		{
			var bitrate = MathUtil.Clamp(settings.Bitrate, AudioPublishProperty.SdkMinLimit.Bitrate, AudioPublishProperty.SdkMaxLimit.Bitrate);

			return (ulong)bitrate;
		}
	}
}
