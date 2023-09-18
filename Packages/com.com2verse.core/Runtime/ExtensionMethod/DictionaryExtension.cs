/*===============================================================
* Product:		Com2Verse
* File Name:	DictionaryExtension.cs
* Developer:	urun4m0r1
* Date:			2022-04-22 13:03
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;

namespace Com2Verse.Extension
{
	public static class DictionaryExtension
	{
		public static void DisposeAndClear<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary)
		{
			dictionary.DisposeKeyValues();
			dictionary?.Clear();
		}

		public static void DisposeKeyValues<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary)
		{
			dictionary.DisposeKeys();
			dictionary.DisposeValues();
		}

		public static void DisposeKeys<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary)
		{
			if (dictionary == null) return;
			foreach (var x in dictionary.Keys) (x as IDisposable)?.Dispose();
		}

		public static void DisposeValues<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary)
		{
			if (dictionary == null) return;
			foreach (var x in dictionary.Values) (x as IDisposable)?.Dispose();
		}

		public static TValue? Find<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary, TKey type)
		{
			if (dictionary == null) return default;
			return dictionary.TryGetValue(type, out var value) ? value : default;
		}

		public static TValue? Find<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue>? dictionary, TKey type)
		{
			if (dictionary == null) return default;
			return dictionary.TryGetValue(type, out var value) ? value : default;
		}

		public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary)
		{
			return new Dictionary<TKey, TValue>(dictionary!);
		}

		public static bool IsKeyValueEquals<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary, IDictionary<TKey, TValue>? other)
		{
			if (dictionary == null && other == null) return true;
			if (dictionary == null || other == null) return false;
			if (dictionary.Count != other.Count) return false;

			foreach (var (key, value) in dictionary)
			{
				if (!other.TryGetValue(key, out var otherValue)) return false;
				if (!Equals(value, otherValue)) return false;
			}

			return true;
		}
	}
}
