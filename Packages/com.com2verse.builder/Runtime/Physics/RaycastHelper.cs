// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	RaycastHelper.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-09 오후 4:26
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.Builder
{
	public static class RaycastHelper
	{
		private static readonly RaycastHit[] RayHits = new RaycastHit[128];
		private static readonly RaycastSorter Sorter = new RaycastSorter();

		private class RaycastSorter : IComparer<RaycastHit>
		{
			public int Compare(RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance);
		}

		public static bool FindTopMostCollision(Ray ray, Transform me, int myStackLevel, out RaycastHit hit, out Transform parent)
		{
			int count = Physics.RaycastNonAlloc(ray, RayHits, float.PositiveInfinity, (1 << SpaceManager.StackBaseObjectLayer) | (1 << SpaceManager.SelectableObjectLayer) | (1 << SpaceManager.GroundLayer));
			if (count == 128) throw new Exception("Raycast limit exceeds");
			Array.Sort(RayHits, 0, count, Sorter);

			for (int i = 0; i < count; i++)
			{
				var raycastHit = RayHits[i];
				if (raycastHit.transform.IsReferenceNull() || raycastHit.transform.IsChildOf(me)) continue;

				// hit floor or wall
				if (!raycastHit.transform.GetComponentInParent<BaseWallObject>().IsReferenceNull())
				{
					hit = raycastHit;
					parent = SpaceManager.Instance.MapRoot.transform;
					return true;
				}
				
				// hit other object
				var builderObject = raycastHit.transform.GetComponentInParent<BuilderObject>();
				if (!builderObject.IsReferenceNull() && builderObject.StackLevel <= myStackLevel)
				{
					hit = raycastHit;
					parent = builderObject.transform;
					return true;
				}
				
				// hit other things (space template ?)
				hit = raycastHit;
				parent = SpaceManager.Instance.MapRoot.transform;
				return true;
			}

			hit = default;
			parent = null;
			
			return false;
		}
	}
}
