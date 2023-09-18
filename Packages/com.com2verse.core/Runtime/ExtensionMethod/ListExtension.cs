/*===============================================================
* Product:		Com2Verse
* File Name:	ListExtension.cs
* Developer:	tlghks1009
* Date:			2022-05-20 18:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;

namespace Com2Verse
{
	public static class ListExtension
	{
		public static Queue<T> ToQueue<T>(this List<T> thisList)
		{
			var queue = new Queue<T>();
			foreach (var item in thisList)
				queue.Enqueue(item);

			return queue;
		}

		public static void Swap<T>(this List<T> thisList, int indexA, int indexB)
		{
			(thisList[indexA], thisList[indexB]) = (thisList[indexB], thisList[indexA]);
		}


		public static T LastItem<T>(this List<T> thisList)
		{
			return thisList[^1];
		}

		public static void Move<T>(this List<T> thisList, int oldIndex, int newIndex)
		{
			if ((oldIndex == newIndex) || (0 > oldIndex) || (oldIndex >= thisList.Count) || (0 > newIndex) ||
			    (newIndex >= thisList.Count)) return;

			var i = 0;
			T tmp = thisList[oldIndex];

			if (oldIndex < newIndex)
			{
				for (i = oldIndex; i < newIndex; i++)
				{
					thisList[i] = thisList[i + 1];
				}
			}
			else
			{
				for (i = oldIndex; i > newIndex; i--)
				{
					thisList[i] = thisList[i - 1];
				}
			}

			thisList[newIndex] = tmp;
		}
	}
}
