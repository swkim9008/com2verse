/*===============================================================
 * Product:		Com2Verse
 * File Name:	ScreenId.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-01 19:32
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using ValueType = System.Int32;

namespace Com2Verse.ScreenShare
{
	public readonly struct ScreenId : IEquatable<ScreenId>, IFormattable
	{
		private readonly ValueType _value;

		public ScreenId(ValueType value) => _value = value;

		public static implicit operator ValueType(ScreenId value) => value._value;
		public static implicit operator ScreenId(ValueType value) => new(value);

		public static bool operator ==(ScreenId left, ScreenId right) => left.Equals(right);
		public static bool operator !=(ScreenId left, ScreenId right) => !left.Equals(right);

		public override bool   Equals(object? obj) => obj is ScreenId rhs && Equals(rhs);
		public override int    GetHashCode()       => _value.GetHashCode();
		public override string ToString()          => _value.ToString();

		public bool Equals(ScreenId other) => _value.Equals(other._value);

		public string ToString(string          format)                                 => _value.ToString(format);
		public string ToString(IFormatProvider formatProvider)                         => _value.ToString(formatProvider);
		public string ToString(string          format, IFormatProvider formatProvider) => _value.ToString(format, formatProvider);
	}

	public sealed class ScreenIdComparer : IEqualityComparer<ScreenId>, IEqualityComparer
	{
		public static ScreenIdComparer Default { get; } = new();

		public bool Equals(ScreenId x, ScreenId y) => x.Equals(y);

		public int GetHashCode(ScreenId obj) => obj.GetHashCode();

		bool IEqualityComparer.Equals(object x, object y) => x is ScreenId lhs && y is ScreenId rhs && Equals(lhs, rhs);

		int IEqualityComparer.GetHashCode(object obj) => obj is ScreenId rhs ? GetHashCode(rhs) : obj.GetHashCode();
	}
}
