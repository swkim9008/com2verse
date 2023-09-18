/*===============================================================
* Product:		Com2Verse
* File Name:	ShapeUtility.cs
* Developer:	ljk
* Date:			2022-07-28 12:56
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using UnityEngine;


namespace Com2Verse.Rendering.Utility
{
	public static class ShapeUtility
	{
		public const float FAR = 999999;
		// Vector4 는 라벨링이 필요할 때 사용
		// * 공통접선을 구부리는데 사용중.
		public static List<Vector4> CalculateConvexHull(List<Vector4> input)
		{
			if (input.Count < 4)
				return input;
			return ConvexHull(input); 
		}
		
		public static List<Vector3> CalculateConvexHull(List<Vector3> input)
		{
			if (input.Count < 4)
				return input;
			return ConvexHull(input); 
		}

		// Concave - 미구현
		public static Vector3[] CalculateConcaveHull(Vector3[] input)
		{
			return new Vector3[0];
		}

		//TODO: Lerp 함수의 동작 수정 
		public static void LerpVertexArray(this Vector3[] me, Vector3[] newVert,float time)
		{
			for (int i = 0; i < newVert.Length && i < me.Length; i++)
				me[i] = Vector3.Lerp(me[i], newVert[i], time);
		}

		public static float Lerp(float a , float b , float time)
		{
			return (1 - time) * a + time * b;
		}

		// centerForce => 주 버텍스에서 일정이상 떨어진 버텍스들은 중앙으로 쏠림 힘을 받게 한다. ( 탄성 있는 모습을 연출하기 위해..)
		public static List<Vector3> CalculateSmoothContour(Vector3 p0,List<Vector4> edgedContourVertex, int vertexCount, float centerForce = 0,float centerSmoothExp = 1.5f, bool addCenter = false)
	    {
	        float totalLength = 0;
	        float[] edgeLengths = new float[edgedContourVertex.Count];
	        for (int i = 0; i < edgedContourVertex.Count; i++)
	        {
	            int next = (i == edgedContourVertex.Count-1) ? 0 : i + 1;
	            float edgeLength = ((Vector3)edgedContourVertex[next] - (Vector3)edgedContourVertex[i]).magnitude;
	            totalLength += edgeLength;
	            edgeLengths[i] = edgeLength;
	        }

	        float vertexAddDistance = totalLength / (vertexCount);
	        Vector3 center = Vector3.zero;
	        
	        edgedContourVertex.ForEach(x =>
		        {
			        center += (Vector3)(x / edgedContourVertex.Count);
		        }
			);
	        
	        #if UNITY_EDITOR
	     //   Debug.DrawLine(center,center+Vector3.up,Color.red,0.016f);
	        #endif
		    
	        List<Vector3> result = new List<Vector3>();
	        int startpoint = 0;
	        float dist = 999;
	        for (int i = 0; i < edgedContourVertex.Count; i++)
	        {
		        float dot = ((Vector3)edgedContourVertex[i] - p0).sqrMagnitude;
		        if (dot < dist)
		        {
			        startpoint = i;
			        dist = dot;
		        }
	        }
	        
	        for (int ind = startpoint; ind < edgedContourVertex.Count + startpoint; ind++)
	        {
		        int currentIndex = ind % edgedContourVertex.Count;
	            int nextIndex = (currentIndex == edgedContourVertex.Count-1) ? 0 : currentIndex+1;

	            Vector3 current = edgedContourVertex[currentIndex];
	            Vector3 next = edgedContourVertex[nextIndex];
	            Vector3 currentLineNormal = (next - current).normalized;

	            float currentLabel = edgedContourVertex[currentIndex].w;
	            float nextLabel = edgedContourVertex[nextIndex].w;
	            float edgeLength = edgeLengths[currentIndex];
	            float distance = vertexAddDistance;
	            
	            result.Add(current);
	            Vector3 dirn = Vector3.Cross(  next - current , Vector3.up ).normalized;
	            
	            while (distance <= edgeLength)
	            {
		            bool freeVertex = (currentLabel != nextLabel) &&
		                              (distance + vertexAddDistance < edgeLength);
		            
	                Vector3 vertexOnLine = current + distance * currentLineNormal;
	                if (freeVertex)					// 원에 속하지 않은 버텍스 ( 공통접선 위의 점 )
	                {								// 이 점들을 내부로 향하게 하여 젤리처럼 보이게 한다
		                float expForSmooth = Mathf.Sin(distance / edgeLength * Mathf.PI);
		                expForSmooth = Mathf.Pow(expForSmooth,centerSmoothExp);
		                vertexOnLine += dirn * expForSmooth *centerForce*edgeLength ;
	                }
		            
	                result.Add(vertexOnLine);
	                distance += vertexAddDistance;
	            }
	        }

	        DebugPolyLine(result, Color.blue, Vector3.up * 0.2f, 0.01f, true);

	        if (addCenter)
	        {
			    result.Add(center);
	        }

	        return result;
	    }

		public static void DebugPolyLine(List<Vector3> points,Color color,Vector3 offset,float duration = 0.1f,bool drawpoint = false,int limitCount = 0)
		{
			for (int i = 0; i < points.Count && (limitCount == 0 || i < limitCount) ; i++)
			{
				if(i == 0)
					UnityEngine.Debug.DrawLine(points[points.Count-1]+offset,points[i]+offset,color,duration);
				else
					UnityEngine.Debug.DrawLine(points[i - 1]+offset, points[i]+offset,color,duration);
				if(drawpoint)
					UnityEngine.Debug.DrawLine(points[i] - Vector3.up*0.02f+offset,points[i] + Vector3.up*0.02f+offset,i == 0 ? Color.red : color,duration);
			}
		}
		
		public static void DebugPolyLine(List<Vector4> points,Color color,Vector4 offset,float duration = 0.1f,bool drawpoint = false,int limitCount = 0)
		{
			for (int i = 0; i < points.Count && (limitCount == 0 || i < limitCount) ; i++)
			{
				if(i == 0)
					UnityEngine.Debug.DrawLine(points[points.Count-1]+offset,points[i]+offset,color,duration);
				else
					UnityEngine.Debug.DrawLine(points[i - 1]+offset, points[i]+offset,color,duration);
				if(drawpoint)
					UnityEngine.Debug.DrawLine(points[i] - upVector4*0.02f+offset,points[i] + upVector4*0.02f+offset,i == 0 ? Color.red : color,duration);
			}
		}

		public static float GetAngle(Vector4 p1,Vector4 p2)
		{
			float deg = Mathf.Atan2((p2.z - p1.z), (p2.x - p1.x)) * Mathf.Rad2Deg;
			
			return deg;
		}
		
		public static float GetAngle(Vector3 p1,Vector3 p2)
		{
			float deg = Mathf.Atan2((p2.z - p1.z), (p2.x - p1.x)) * Mathf.Rad2Deg;
			
			return deg;
		}
		
		private static Vector4 upVector4 = new Vector4(0, 1, 0, 0);
		
		// Graham's  ConvexHull 
		private const int TURN_LEFT = 1;
		#region ConvexHullVector3
		private static int TurnInHull(Vector3 p,Vector3 q,Vector3 r)
		{
			return ((q.x - p.x) * (r.z - p.z) - (r.x - p.x) * (q.z - p.z)).CompareTo(0);
		}

		private static void KeepLeftInHull(List<Vector3> hull, Vector3 point)
		{
			while (hull.Count > 1 && TurnInHull(hull[hull.Count - 2], hull[hull.Count - 1], point) != TURN_LEFT)
			{
				hull.RemoveAt(hull.Count-1);
			}

			if (hull.Count == 0 || !hull[hull.Count - 1].Equals(point) )
			{
				hull.Add(point);
			}
		}
		
		private static List<Vector3> ConvexHull(List<Vector3> points)
		{
			Vector3 p0 = points[0];

			for (int i = 0; i < points.Count; i++)
			{
				if ( points[i].z > p0.z)
					p0 = points[i];
			}

			List<Vector3> order = new List<Vector3>();
			for (int i = 0; i < points.Count; i++)
			{
				if(!points[i].Equals(p0) )
					order.Add(points[i]);
			}

			order.Sort(((pA, pB) =>
			{
				float angleA = GetAngle(pA,p0);
				float angleB = GetAngle(pB, p0);
				if (angleA > angleB)
					return 1;
				else if (angleA < angleB)
					return -1;

				return 0;
			}));
			
			List<Vector3> result = new List<Vector3>();
			result.Add(p0);
			result.Add(order[0]);
			result.Add(order[1]);
			order.RemoveAt(0);
			order.RemoveAt(1);

			for (int i = 0; i < order.Count; i++)
			{
				KeepLeftInHull(result,order[i]);
			}

			return result;
		}
		#endregion

		#region ConvexHullVector4
		private static int TurnInHull(Vector4 p,Vector4 q,Vector4 r)
		{
			return ((q.x - p.x) * (r.z - p.z) - (r.x - p.x) * (q.z - p.z)).CompareTo(0);
		}

		private static void KeepLeftInHull(List<Vector4> hull, Vector4 point)
		{
			while (hull.Count > 1 && TurnInHull(hull[hull.Count - 2], hull[hull.Count - 1], point) != TURN_LEFT)
			{
				hull.RemoveAt(hull.Count-1);
			}

			if (hull.Count == 0 || !hull[hull.Count - 1].Equals(point) )
			{
				hull.Add(point);
			}
		}
		
		private static List<Vector4> ConvexHull(List<Vector4> points)
		{
			Vector4 p0 = points[0];

			for (int i = 0; i < points.Count; i++)
			{
				if ( points[i].z > p0.z)
					p0 = points[i];
			}

			List<Vector4> order = new List<Vector4>();
			for (int i = 0; i < points.Count; i++)
			{
				if(!points[i].Equals(p0) )
					order.Add(points[i]);
			}

			order.Sort(((pA, pB) =>
			{
				float angleA = GetAngle(pA,p0);
				float angleB = GetAngle(pB, p0);
				if (angleA > angleB)
					return 1;
				else if (angleA < angleB)
					return -1;

				return 0;
			}));
			
			List<Vector4> result = new List<Vector4>();
			result.Add(p0);
			result.Add(order[0]);
			result.Add(order[1]);
			order.RemoveAt(0);
			order.RemoveAt(1);

			for (int i = 0; i < order.Count; i++)
			{
				KeepLeftInHull(result,order[i]);
			}

			return result;
		}
		#endregion
	}

}
