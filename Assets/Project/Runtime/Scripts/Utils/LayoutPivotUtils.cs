/*===============================================================
* Product:		Com2Verse
* File Name:	LayoutPivotUtils.cs
* Developer:	urun4m0r1
* Date:			2022-07-28 12:56
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using static Com2Verse.Utils.ePivotType; 

namespace Com2Verse.Utils
{
	[Flags]
	public enum ePivotType
	{
		UNKNOWN = 0,
		BOTTOM  = 1 << 0,
		MIDDLE  = 1 << 1,
		TOP     = 1 << 2,
		LEFT    = 1 << 3,
		CENTER  = 1 << 4,
		RIGHT   = 1 << 5,
	}

	public static class PivotTypeExtensions
	{
		public static bool HasFlagFast(this ePivotType value, ePivotType flag) => (value & flag) != 0;
	}

	public static class LayoutPivotUtils
	{
		public static readonly Dictionary<ePivotType, Vector2> PivotMap = new()
		{
			// 7 8 9
			// 4 5 6
			// 1 2 3
			[BOTTOM | LEFT]   = new(+0.0f, +0.0f), // 1
			[BOTTOM | CENTER] = new(+0.5f, +0.0f), // 2
			[BOTTOM | RIGHT]  = new(+1.0f, +0.0f), // 3
			[MIDDLE | LEFT]   = new(+0.0f, +0.5f), // 4
			[MIDDLE | CENTER] = new(+0.5f, +0.5f), // 5
			[MIDDLE | RIGHT]  = new(+1.0f, +0.5f), // 6
			[TOP    | LEFT]   = new(+0.0f, +1.0f), // 7
			[TOP    | CENTER] = new(+0.5f, +1.0f), // 8
			[TOP    | RIGHT]  = new(+1.0f, +1.0f), // 9
		};

		public static void SetPivot(this RectTransform source, ePivotType pivot)
		{
			var sourcePivot = source.pivot;
			var targetPivot = PivotMap[pivot];

			source.anchoredPosition += (targetPivot - sourcePivot) * source.sizeDelta;
			source.pivot            =  targetPivot;
		}

		public static ePivotType GetPivotType(this Vector2 pivot)
		{
			foreach (var key in PivotMap.Keys)
				if (PivotMap[key] == pivot)
					return key;

			return UNKNOWN;
		}
	}
}
