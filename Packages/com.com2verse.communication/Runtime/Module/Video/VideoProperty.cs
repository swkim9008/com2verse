/*===============================================================
* Product:		Com2Verse
* File Name:	VideoProperty.cs
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

namespace Com2Verse.Communication
{
	public readonly struct VideoProperty : IEquatable<VideoProperty>, IEqualityComparer<VideoProperty>, IEqualityComparer
	{
#region StaticValues
		public static readonly VideoProperty Fallback    = new VideoProperty(width: 480,  height: 360,  fps: 15);
		public static readonly VideoProperty SdkMinLimit = new VideoProperty(width: 145,  height: 49,   fps: 0);
		public static readonly VideoProperty SdkMaxLimit = new VideoProperty(width: 4096, height: 4096, fps: 240);
#endregion //StaticValues

		public int Width   { get; }
		public int Height  { get; }
		public int Fps     { get; }

		public float AspectRatio => Width / (float)Height;

		public VideoProperty(int width, int height, int fps)
		{
			Width   = width;
			Height  = height;
			Fps     = fps;
		}

#region Interfaces
		public override string ToString() => $"{Width.ToString()}x{Height.ToString()}px_{Fps.ToString()}fps";

		public static bool operator ==(VideoProperty a, VideoProperty b) => a.Equals(b);
		public static bool operator !=(VideoProperty a, VideoProperty b) => !a.Equals(b);

		public override bool Equals(object? obj) => obj is VideoProperty other && Equals(other);

		public bool Equals(VideoProperty x, VideoProperty y) => x.Equals(y);

		public bool Equals(VideoProperty other) => Width   == other.Width
		                                        && Height  == other.Height
		                                        && Fps     == other.Fps;

		public int GetHashCode(VideoProperty obj) => obj.GetHashCode();

		public override int GetHashCode() => HashCode.Combine(Width, Height, Fps);

		bool IEqualityComparer.Equals(object x, object y) => x is VideoProperty a && y is VideoProperty b && Equals(a, b);

		int IEqualityComparer.GetHashCode(object obj) => obj is VideoProperty a ? GetHashCode(a) : 0;
#endregion // Interfaces
	}
}
