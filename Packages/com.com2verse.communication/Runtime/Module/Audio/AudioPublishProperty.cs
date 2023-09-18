/*===============================================================
 * Product:		Com2Verse
 * File Name:	AudioPublishProperty.cs
 * Developer:	urun4m0r1
 * Date:		2022-12-02 18:16
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
	public readonly struct AudioPublishProperty : IEquatable<AudioPublishProperty>, IEqualityComparer<AudioPublishProperty>, IEqualityComparer
	{
#region StaticValues
		public static readonly AudioPublishProperty Fallback    = new AudioPublishProperty(bitrate: 500_000);
		public static readonly AudioPublishProperty SdkMinLimit = new AudioPublishProperty(bitrate: 0);
		public static readonly AudioPublishProperty SdkMaxLimit = new AudioPublishProperty(bitrate: 5_000_000);
#endregion //StaticValues

		public int Bitrate { get; }

		public AudioPublishProperty(int bitrate)
		{
			Bitrate = bitrate;
		}

#region Interfaces
		public override string ToString() => $"{Bitrate.ToString()}bps";

		public static bool operator ==(AudioPublishProperty a, AudioPublishProperty b) => a.Equals(b);
		public static bool operator !=(AudioPublishProperty a, AudioPublishProperty b) => !a.Equals(b);

		public override bool Equals(object? obj) => obj is AudioPublishProperty other && Equals(other);

		public bool Equals(AudioPublishProperty x, AudioPublishProperty y) => x.Equals(y);

		public bool Equals(AudioPublishProperty other) => Bitrate == other.Bitrate;

		public int GetHashCode(AudioPublishProperty obj) => obj.GetHashCode();

		public override int GetHashCode() => HashCode.Combine(Bitrate);

		bool IEqualityComparer.Equals(object x, object y) => x is AudioPublishProperty a && y is AudioPublishProperty b && Equals(a, b);

		int IEqualityComparer.GetHashCode(object obj) => obj is AudioPublishProperty a ? GetHashCode(a) : 0;
#endregion // Interfaces
	}
}
