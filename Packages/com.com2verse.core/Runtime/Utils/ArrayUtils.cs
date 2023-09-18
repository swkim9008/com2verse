/*===============================================================
 * Product:		Com2Verse
 * File Name:	ArrayUtils.cs
 * Developer:	urun4m0r1
 * Date:		2023-06-30 12:16
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2Verse.Utils
{
	public static class ArrayUtils
	{
		public static void CycleCopyArray(float[] sourceArray, int sourceIndex, float[] destinationArray, int destinationIndex, int length)
		{
			var remain         = sourceArray.Length - sourceIndex;
			var canReadForward = remain >= length;
			if (canReadForward)
			{
				// 읽을 위치가 버퍼의 끝을 넘어가지 않는 경우
				// ****------, ---****---, ------****
				Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);
			}
			else
			{
				// 읽을 위치가 버퍼의 끝을 넘어가는 경우
				// *------***, **------**, ***------*
				Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex,          remain);
				Array.Copy(sourceArray, 0,           destinationArray, destinationIndex + remain, length - remain);
			}
		}

		public static int GetCycleArrayLength(int length, int headPosition, int tailPosition) => headPosition < tailPosition ? tailPosition - headPosition : length - (headPosition - tailPosition);
	}
}
