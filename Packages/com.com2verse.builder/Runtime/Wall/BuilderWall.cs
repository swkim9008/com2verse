// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	BuilderWall.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-06 오후 3:44
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Threading;
using Com2Verse.Extension;

namespace Com2Verse.Builder
{
	public class BuilderWall : BaseWallObject
	{
		public static int IndexCounter = 0;

		public bool IsCorner { get; set; } = false;

		public bool GroupCullingCalculated { get; set; } = false;

		public BuilderWall()
		{
			Index = Interlocked.Increment(ref IndexCounter);
			Neighbor = new BuilderWall[2];
		}

		public BuilderWall NeighborLeft
		{
			get => (BuilderWall)Neighbor[0];
			set => Neighbor[0] = value;
		}
		public BuilderWall NeighborRight
		{
			get => (BuilderWall)Neighbor[1];
			set => Neighbor[1] = value;
		}

		public override void PropagateAction(Action<BaseWallObject> action)
		{
			action(this);
			IterNeighbor(action, NeighborLeft, true);
			IterNeighbor(action, NeighborRight, false);
		}
		
		private void IterNeighbor(Action<BaseWallObject> action, BuilderWall wall, bool left)
		{
			while (!wall.IsReferenceNull())
			{
				action(wall);
				if (left)
					wall = wall.NeighborLeft;
				else
					wall = wall.NeighborRight;
			}
		}
	}
}
