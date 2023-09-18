/*===============================================================
* Product:		Com2Verse
* File Name:	IListExtension.cs
* Developer:	jhkim
* Date:			2022-11-08 17:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Com2Verse.Extension
{
	public static class IListExtension
	{
#region Sort
		// https: //stackoverflow.com/questions/15486/sorting-an-ilist-in-c-sharp
		// Sorts an IList<T> in place.
		public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
		{
			ArrayList.Adapter((IList) list).Sort(new ComparisonComparer<T>(comparison));
		}

		// Sorts in IList<T> in place, when T is IComparable<T>
		public static void Sort<T>(this IList<T> list) where T : IComparable<T>
		{
			Comparison<T> comparison = (l, r) => l.CompareTo(r);
			Sort(list, comparison);
		}

		// Convenience method on IEnumerable<T> to allow passing of a
		// Comparison<T> delegate to the OrderBy method.
		public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> list, Comparison<T> comparison)
		{
			return list.OrderBy(t => t, new ComparisonComparer<T>(comparison));
		}
#endregion // Sort
	}

	// Wraps a generic Comparison<T> delegate in an IComparer to make it easy
	// to use a lambda expression for methods that take an IComparer or IComparer<T>
	public class ComparisonComparer<T> : IComparer<T>, IComparer
	{
		private readonly Comparison<T> _comparison;
	
		public ComparisonComparer(Comparison<T> comparison)
		{
			_comparison = comparison;
		}
	
		public int Compare(T x, T y)
		{
			return _comparison(x, y);
		}
	
		public int Compare(object o1, object o2)
		{
			return _comparison((T) o1, (T) o2);
		}
	}
}
