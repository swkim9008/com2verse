// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	DrawVertices.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-09 오후 12:43
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/
#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Com2Verse.Builder.Debug
{
	public class DrawVertices : MonoBehaviour
	{
		private void OnDrawGizmos()
		{
			var filters = GetComponentsInChildren<MeshFilter>();

			Vector3 min = Vector3.positiveInfinity;
			Vector3 max = Vector3.negativeInfinity;
			foreach (var filter in filters)
			{
				var vertices = filter.sharedMesh.vertices;
				foreach (var vertex in vertices)
				{
					Vector3 worldCoordinateVertex = filter.transform.TransformPoint(vertex);
					Gizmos.DrawSphere(worldCoordinateVertex, 0.01f);
				}
			}
		}
	}
}
#endif