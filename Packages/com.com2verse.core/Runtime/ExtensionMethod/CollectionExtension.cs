/*===============================================================
* Product:		Com2Verse
* File Name:	CollectionExtension.cs
* Developer:	urun4m0r1
* Date:			2022-05-19 13:11
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;

namespace Com2Verse.Extension
{
	public static class CollectionExtension
	{
		public static bool TryAdd<T>(this ICollection<T> collection, T item)
		{
			if (collection.Contains(item))
			{
				return false;
			}

			collection.Add(item);
			return true;
		}

		public static bool TryRemove<T>(this ICollection<T> collection, T item)
		{
			if (collection.Contains(item))
			{
				collection.Remove(item);
				return true;
			}

			return false;
		}

		public static bool HasIndex<T>(this ICollection<T> collection, int index) =>
			index >= 0 && index < collection.Count;

		public static bool TryGetAt<T>(this IList<T> collection, int index, out T? item)
		{
			if (!collection.HasIndex(index))
			{
				item = default;
				return false;
			}

			item = collection[index];
			return true;
		}
	}
}
