/*===============================================================
* Product:		Com2Verse
* File Name:	RepeatedFieldExtension.cs
* Developer:	jhkim
* Date:			2022-11-09 16:05
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Collections;

namespace Com2Verse.Extension
{
	public static class RepeatedFieldExtension
	{
#region Sort
		// https: //stackoverflow.com/questions/15486/sorting-an-ilist-in-c-sharp
		// Sorts an RepeatedField<T> in place.
		public static void Sort<T>(this RepeatedField<T> list, Comparison<T> comparison)
		{
			ArrayList.Adapter((IList) list).Sort(new ComparisonComparer<T>(comparison));
		}

		// Sorts in RepeatedField<T> in place, when T is IComparable<T>
		public static void Sort<T>(this RepeatedField<T> list) where T : IComparable<T>
		{
			Comparison<T> comparison = (l, r) => l.CompareTo(r);
			Sort(list, comparison);
		}

		public static List<T> CreateSortedList<T>(this RepeatedField<T> list, Comparison<T> comparison)
		{
			var result = new List<T>(list);
			result.Sort(comparison);
			return result;
		}
#endregion // Sort
	}
}
