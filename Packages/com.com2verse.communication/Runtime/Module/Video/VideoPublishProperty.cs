/*===============================================================
* Product:		Com2Verse
* File Name:	VideoPublishProperty.cs
* Developer:	urun4m0r1
* Date:			2022-11-23 13:50
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Com2Verse.Communication
{
	public readonly struct VideoPublishProperty : IEquatable<VideoPublishProperty>, IEqualityComparer<VideoPublishProperty>, IEqualityComparer
	{
#region StaticValues
		public static readonly VideoPublishProperty Fallback    = new VideoPublishProperty(fps: 15,  bitrate: 500_000,   scale: 1f);
		public static readonly VideoPublishProperty SdkMinLimit = new VideoPublishProperty(fps: 0,   bitrate: 0,         scale: 1f);
		public static readonly VideoPublishProperty SdkMaxLimit = new VideoPublishProperty(fps: 240, bitrate: 5_000_000, scale: 4f);
#endregion //StaticValues

		public int   Fps     { get; }
		public int   Bitrate { get; }
		public float Scale   { get; }

		public VideoPublishProperty(int fps, int bitrate, float scale)
		{
			Fps     = fps;
			Bitrate = bitrate;
			Scale   = scale;
		}

#region Interfaces
		public override string ToString() => $"{Fps.ToString()}fps_{Bitrate.ToString()}bps_{Scale.ToString(CultureInfo.InvariantCulture)}x";

		public static bool operator ==(VideoPublishProperty a, VideoPublishProperty b) => a.Equals(b);
		public static bool operator !=(VideoPublishProperty a, VideoPublishProperty b) => !a.Equals(b);

		public override bool Equals(object? obj) => obj is VideoPublishProperty other && Equals(other);

		public bool Equals(VideoPublishProperty x, VideoPublishProperty y) => x.Equals(y);

		[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		public bool Equals(VideoPublishProperty other) => Fps     == other.Fps
		                                               && Bitrate == other.Bitrate
		                                               && Scale   == other.Scale;

		public int GetHashCode(VideoPublishProperty obj) => obj.GetHashCode();

		public override int GetHashCode() => HashCode.Combine(Fps, Bitrate, Scale);

		bool IEqualityComparer.Equals(object x, object y) => x is VideoPublishProperty a && y is VideoPublishProperty b && Equals(a, b);

		int IEqualityComparer.GetHashCode(object obj) => obj is VideoPublishProperty a ? GetHashCode(a) : 0;
#endregion // Interfaces
	}
}
