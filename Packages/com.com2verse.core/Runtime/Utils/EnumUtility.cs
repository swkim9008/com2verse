/*===============================================================
* Product:		Com2Verse
* File Name:	EnumUtility.cs
* Developer:	urun4m0r1
* Date:			2022-06-30 10:29
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;

namespace Com2Verse.Utils
{
	public enum eEnumMatchType
	{
		IGNORE,
		ANY,
		EQUALS,
		NOT_EQUALS,
	}

	public enum eFlagMatchType
	{
		IGNORE,
		ANY,
		EQUALS,
		NOT_EQUALS,
		CONTAINS,
		NOT_CONTAINS,
	}

	public enum ePrimitiveMatchType
	{
		IGNORE,
		ANY,
		EQUALS,
		NOT_EQUALS,
	}

	public static class EnumUtility
	{
		public static IEnumerable<T> Foreach<T>() where T : unmanaged, Enum =>
			Enum.GetValues(typeof(T)).Cast<T>();

		public static T AddFlags<T>(this T lhs, T rhs) where T : unmanaged, Enum
		{
			var a      = lhs.CastInt();
			var b      = rhs.CastInt();
			var result = a | b;

			return result.CastEnum<T>();
		}

		public static T SubtractFlags<T>(this T lhs, T rhs) where T : unmanaged, Enum
		{
			var a      = lhs.CastInt();
			var b      = rhs.CastInt();
			var result = a & ~b;

			return result.CastEnum<T>();
		}

		public static T GetHighestFlag<T>(this T value) where T : unmanaged, Enum
		{
			var target = value.CastInt();
			switch (target)
			{
				case 0:  return 0.CastEnum<T>();
				case -1: return Enum.GetValues(typeof(T)).Cast<T>().Max();
			}

			var shiftCount = 0;

			while (target > 0)
			{
				target >>= 1;
				++shiftCount;
			}

			return (1 << (shiftCount - 1)).CastEnum<T>();
		}

		public static T GetHighestFlag<T>() where T : Enum
		{
			var enums = Enum.GetValues(typeof(T)).Cast<T>().ToList();
			return enums.Max() ?? enums.Last();
		}

		/// <summary>
		/// Cast enum to int without boxing.
		/// </summary>
		public static int CastInt<T>(this T e) where T : unmanaged, Enum => UnsafeUtility.As<T, int>(ref e);

		/// <summary>
		/// Cast int to enum without boxing.
		/// </summary>
		public static T CastEnum<T>(this int i) where T : unmanaged, IConvertible => UnsafeUtility.As<int, T>(ref i);

		public static bool IsFilterMatch<T>(this T target, T filter, eEnumMatchType type) where T : unmanaged, Enum
		{
			var targetInt = target.CastInt();
			var filterInt = filter.CastInt();

			return type switch
			{
				eEnumMatchType.IGNORE     => false,
				eEnumMatchType.ANY        => true,
				eEnumMatchType.EQUALS     => targetInt == filterInt,
				eEnumMatchType.NOT_EQUALS => targetInt != filterInt,
				_                         => throw new ArgumentOutOfRangeException(),
			};
		}

		public static bool IsFilterMatch<T>(this T target, T filter, eFlagMatchType type) where T : unmanaged, Enum
		{
			var targetInt = target.CastInt();
			var filterInt = filter.CastInt();

			return type switch
			{
				eFlagMatchType.IGNORE       => false,
				eFlagMatchType.ANY          => true,
				eFlagMatchType.EQUALS       => targetInt               == filterInt,
				eFlagMatchType.NOT_EQUALS   => targetInt               != filterInt,
				eFlagMatchType.CONTAINS     => (targetInt & filterInt) != 0,
				eFlagMatchType.NOT_CONTAINS => (targetInt & filterInt) == 0,
				_                           => throw new ArgumentOutOfRangeException(),
			};
		}
	}
}
