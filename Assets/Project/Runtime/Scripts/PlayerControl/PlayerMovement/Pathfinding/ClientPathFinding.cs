// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	ClientPathFinding.cs
//  * Developer:	yangsehoon
//  * Date:		2023-06-29 오후 12:27
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using Com2Verse.Avatar;
using Com2Verse.Extension;
using Com2Verse.Utils;
using Pathfinding;
using UnityEngine;

namespace Com2Verse.Pathfinder
{
	public class ClientPathFinding : MonoSingleton<ClientPathFinding>
	{
		public const int NavmeshCutResolution = 6;
		public const float NavmeshCutPadding = 0.5f;
		public const string NavmeshNameTemplate = "{0}_navmesh.bytes";
		
		private ClientPathFinding() { }

		public Seeker PlayerSeeker { get; private set; }

		public void Initialize()
		{
			var astar = Util.GetOrAddComponent<AstarPath>(gameObject);
			astar.maxNearestNodeDistance = 2;

			PlayerSeeker = Util.GetOrAddComponent<Seeker>(gameObject);
			var modifier = Util.GetOrAddComponent<RaycastModifier>(gameObject);
			modifier.quality = RaycastModifier.Quality.Highest;
			modifier.useGraphRaycasting = true;
			modifier.useRaycasting = false;
		}

		public void LoadNavGraph(TextAsset data)
		{
			AstarPath.active.data.file_cachedStartup = data;
			AstarPath.active.data.LoadFromCache();
		}

		public void ClearGraph()
		{
			if (!AstarPath.active.IsReferenceNull())
				AstarPath.active.data.OnDestroy();
		}

		public void SettingNavmeshCut(GameObject gameObject, float radius, float height, Vector3 center)
		{
			if (enabled && radius > 0)
			{
				var navmeshCut = gameObject.AddComponent<NavmeshCut>();
				navmeshCut.type = NavmeshCut.MeshType.Circle;
				navmeshCut.circleRadius = radius + NavmeshCutPadding;
				navmeshCut.circleResolution = NavmeshCutResolution;
				navmeshCut.height = height + AvatarCreateManager.AvatarColliderHeight;
				navmeshCut.center = center - new Vector3(0, AvatarCreateManager.AvatarColliderHeight / 2, 0);
				navmeshCut.useRotationAndScale = true;
			}
		}
	}
}
