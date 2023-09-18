// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	BuilderFloor.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-08 오전 10:37
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System.Threading;
using UnityEngine;

namespace Com2Verse.Builder
{
	public class BuilderFloor : BaseWallObject
	{
		public static int IndexCounter = 0;

		public Vector2 FloorScale { get; set; }

		public BuilderFloor()
		{
			Index = Interlocked.Increment(ref IndexCounter);
			Neighbor = new BaseWallObject[0];
		}
	}
}
