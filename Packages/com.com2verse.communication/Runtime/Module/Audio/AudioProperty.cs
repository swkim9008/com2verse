/*===============================================================
 * Product:		Com2Verse
 * File Name:	AudioProperty.cs
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
	public readonly struct AudioProperty : IEquatable<AudioProperty>, IEqualityComparer<AudioProperty>, IEqualityComparer
	{
#region StaticValues
		public static readonly AudioProperty Fallback    = new AudioProperty(length: 1_000,     frequency: 16_000);
		public static readonly AudioProperty SdkMinLimit = new AudioProperty(length: 1_000,     frequency: 1);
		public static readonly AudioProperty SdkMaxLimit = new AudioProperty(length: 3_600_000, frequency: int.MaxValue);
#endregion //StaticValues

		public int Length    { get; }
		public int Frequency { get; }

		public AudioProperty(int length, int frequency)
		{
			Length    = length;
			Frequency = frequency;
		}

#region Interfaces
		public override string ToString() => $"{Length.ToString()}ms_{Frequency.ToString()}Hz";

		public static bool operator ==(AudioProperty a, AudioProperty b) => a.Equals(b);
		public static bool operator !=(AudioProperty a, AudioProperty b) => !a.Equals(b);

		public override bool Equals(object? obj) => obj is AudioProperty other && Equals(other);

		public bool Equals(AudioProperty x, AudioProperty y) => x.Equals(y);

		public bool Equals(AudioProperty other) => Length    == other.Length
		                                        && Frequency == other.Frequency;

		public int GetHashCode(AudioProperty obj) => obj.GetHashCode();

		public override int GetHashCode() => HashCode.Combine(Length, Frequency);

		bool IEqualityComparer.Equals(object x, object y) => x is AudioProperty a && y is AudioProperty b && Equals(a, b);

		int IEqualityComparer.GetHashCode(object obj) => obj is AudioProperty a ? GetHashCode(a) : 0;
#endregion // Interfaces
	}
}
