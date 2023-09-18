/*===============================================================
 * Product:		Com2Verse
 * File Name:	TrackExtensions.cs
 * Developer:	urun4m0r1
 * Date:		2023-03-16 11:26
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Solution.UnityRTCSdk;
using Cysharp.Text;

namespace Com2Verse.Communication.MediaSdk
{
	internal static class TrackExtensions
	{
		internal static string GetInfoText(this StreamTrack track)
		{
			var className = track.GetType().Name;
			var trackId   = track.TrackId;

			return ZString.Format(
				"{0} ({1})"
			  , className, trackId);
		}
	}
}
