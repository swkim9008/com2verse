/*===============================================================
 * Product:		Com2Verse
 * File Name:	TrackTypeConverter.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-07 15:27
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Solution.UnityRTCSdk;

namespace Com2Verse.Communication.MediaSdk
{
	internal static class TrackTypeConverter
	{
		internal static eTrackType GetTrackType(this StreamTrack track)
		{
			return track switch
			{
				AudioTrack audioTrack => audioTrack.AudioType.GetTrackType(),
				VideoTrack videoTrack => videoTrack.VideoType.GetTrackType(),
				_                     => eTrackType.UNKNOWN,
			};
		}

		internal static eTrackType GetTrackType(this AUDIO_TYPE audioType)
		{
			return audioType switch
			{
				AUDIO_TYPE.Mic  => eTrackType.VOICE,
				AUDIO_TYPE.File => eTrackType.AUDIO,
				_               => eTrackType.UNKNOWN,
			};
		}

		internal static eTrackType GetTrackType(this VIDEO_TYPE videoType)
		{
			return videoType switch
			{
				VIDEO_TYPE.Camera => eTrackType.CAMERA,
				VIDEO_TYPE.Screen => eTrackType.SCREEN,
				VIDEO_TYPE.File   => eTrackType.VIDEO,
				_                 => eTrackType.UNKNOWN,
			};
		}

		internal static AUDIO_TYPE? GetAudioType(this eTrackType trackType)
		{
			return trackType switch
			{
				eTrackType.VOICE => AUDIO_TYPE.Mic,
				eTrackType.AUDIO => AUDIO_TYPE.File,
				_                => null,
			};
		}

		internal static VIDEO_TYPE? GetVideoType(this eTrackType trackType)
		{
			return trackType switch
			{
				eTrackType.CAMERA => VIDEO_TYPE.Camera,
				eTrackType.SCREEN => VIDEO_TYPE.Screen,
				eTrackType.VIDEO  => VIDEO_TYPE.File,
				_                 => null,
			};
		}
	}
}
