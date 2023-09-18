/*===============================================================
* Product:		Com2Verse
* File Name:	Uid.cs
* Developer:	urun4m0r1
* Date:			2023-03-23 14:00
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using ValueType = System.Int64;

namespace Com2Verse.Communication
{
	/// <summary>
	/// 유니크한 식별자를 나타내는 구조체입니다.
	/// <br/><see cref="ValueType"/>의 Wrapper입니다.
	/// </summary>
	public readonly struct Uid : IEquatable<Uid>, IFormattable
	{
		private readonly ValueType _value;

		public Uid(ValueType value) => _value = value;

		/// <summary>
		/// 유효한 값인지 확인합니다.
		/// <br/>오직 양수만 유효한 값입니다.
		/// </summary>
		public static bool IsValid(Uid uid) => uid._value >= 0;

		/// <inheritdoc cref="IsValid(Uid)"/>
		public bool IsValid() => IsValid(this);

		/// <summary>
		/// 예외를 발생시키지 않고 <see cref="string"/> 값을 <see cref="Uid"/>로 변환합니다.
		/// <br/>유효성 검사 수행 여부를 선택할 수 있습니다.
		/// </summary>
		public static bool TryParse(string? value, out Uid result, bool skipValidation = false)
		{
			var isSuccess = ValueType.TryParse(value!, out var uid);
			result = new(uid);
			return isSuccess && (skipValidation || IsValid(result));
		}

		public static implicit operator ValueType(Uid value) => value._value;
		public static implicit operator Uid(ValueType value) => new(value);

		public static bool operator ==(Uid left, Uid right) => left.Equals(right);
		public static bool operator !=(Uid left, Uid right) => !left.Equals(right);

		public override bool   Equals(object? obj) => obj is Uid rhs && Equals(rhs);
		public override int    GetHashCode()       => _value.GetHashCode();
		public override string ToString()          => _value.ToString()!;

		public bool Equals(Uid other) => _value.Equals(other._value);

		public string ToString(string          format)                                 => _value.ToString(format);
		public string ToString(IFormatProvider formatProvider)                         => _value.ToString(formatProvider);
		public string ToString(string          format, IFormatProvider formatProvider) => _value.ToString(format, formatProvider);
	}

	/// <summary>
	/// <see cref="Uid"/>의 비교자입니다.
	/// <br/><see cref="UidComparer.Default"/>를 사용하면 성능 향상을 기대할 수 있습니다.
	/// </summary>
	public sealed class UidComparer : IEqualityComparer<Uid>, IEqualityComparer
	{
		private UidComparer() { }

		/// <summary>
		/// <see cref="Dictionary{TKey,TValue}"/> 등의 생성자에 전달 시 성능 향상을 기대할 수 있습니다.
		/// </summary>
		public static UidComparer Default { get; } = new();

		public bool Equals(Uid x, Uid y) => x.Equals(y);

		public int GetHashCode(Uid obj) => obj.GetHashCode();

		bool IEqualityComparer.Equals(object x, object y) => x is Uid lhs && y is Uid rhs && Equals(lhs, rhs);

		int IEqualityComparer.GetHashCode(object obj) => obj is Uid rhs ? GetHashCode(rhs) : obj.GetHashCode();
	}
}
