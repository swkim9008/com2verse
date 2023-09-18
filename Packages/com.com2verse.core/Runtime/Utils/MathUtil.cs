/*===============================================================
* Product:		Com2Verse
* File Name:	MathUtil.cs
* Developer:	eugene9721
* Date:			2022-05-16 16:49
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Utils
{
	public static class MathUtil
	{
		public enum eAxisToRotate
		{
			FORWARD,
			RIGHT,
			UP,
			ZERO
		}

		/// <summary>
		/// 3개의 포지션 a,b,c의 X, Z 포지션을 이용하여 방향관계를 파악
		/// </summary>
		public static int CcwBetweenXZ(Vector3 a, Vector3 b, Vector3 c)
		{
			float temp = (b.x - a.x) * (c.z - b.z) - (c.x - b.x) * (b.z - a.z);

			if (temp > float.Epsilon) return 1;
			return -1;
		}

		/// <summary>
		/// 3개의 포지션 a,b,c의 X, Z 포지션을 이용하여 방향관계를 파악
		/// </summary>
		public static int CcwBetweenXZ(Transform a, Transform b, Transform c)
		{
			return CcwBetweenXZ(a.position, b.position, c.position);
		}

		public static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle  -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		public static Vector3 LinearInterpolation(long t1, Vector3 p1, long t2, Vector3 p2, long t)
		{
			if (t2 - t1 == 0 || t - t1 == 0)
				return (p2 - p1) / 2 + p1;
			return (p2 - p1) / (t2 - t1) * (t - t1) + p1;
		}

		public static float LinearInterpolation(long t1, float p1, long t2, float p2, long t)
		{
			if (t2 - t1 == 0 || t - t1 == 0)
				return (p2 - p1) / 2 + p1;
			return (p2 - p1) / (t2 - t1) * (t - t1) + p1;
		}

		public static Quaternion SphericalInterpolation(long t1, Quaternion q1, long t2, Quaternion q2, long t)
		{
			Vector3 r1 = q1.eulerAngles;
			Vector3 r2 = q2.eulerAngles;
			return Quaternion.Euler(SphericalInterpolation(t1, r1.x, t2, r2.x, t),
			                        SphericalInterpolation(t1, r1.y, t2, r2.y, t),
			                        SphericalInterpolation(t1, r1.z, t2, r2.z, t));
		}

		public static float SphericalInterpolation(long t1, float p1, long t2, float p2, long t)
		{
			int delta = (int)((p2 - p1) * 100f);
			delta = (delta + 36000) % 36000;
			delta = delta > 18000 ? delta - 36000 : delta;
			if (t2 - t1 == 0 || t - t1 == 0)
				return (delta / 100f) / 2 + p1;
			return (delta / 100f) / (t2 - t1) * (t - t1) + p1;
		}

		public static float AngleDifference(float a, float b, float angleBase = 360) =>
			(a - b + angleBase) % angleBase;

		public static float GetAngle(Vector2 from, Vector2 to)
		{
			var dx  = to.x - from.x;
			var dy  = to.y - from.y;
			var rad = Mathf.Atan2(dy, dx);
			return rad * Mathf.Rad2Deg;
		}

		public static Vector3 RandomPositionOnCircle(float radius, eAxisToRotate axis = eAxisToRotate.UP)
		{
			var randomPosition = Random.onUnitSphere * Random.Range(0, radius);

			switch (axis)
			{
				case eAxisToRotate.RIGHT:
					randomPosition.x = 0;
					break;
				case eAxisToRotate.UP:
					randomPosition.y = 0;
					break;
				case eAxisToRotate.FORWARD:
					randomPosition.z = 0;
					break;
				case eAxisToRotate.ZERO:
				default:
					randomPosition = Vector3.zero;
					break;
			}

			return randomPosition;
		}

		/// <summary>
		/// 한 프레임에서의 회전 각도
		/// </summary>
		/// <param name="prevRotation">이전 프레임에 적용된 Rotation 값</param>
		/// <param name="currentRotation">현재 프레임에 적용된 Rotation 값</param>
		/// <returns>한 프레임에서의 회전 각도</returns>
		public static float GetTargetAngularVelocityY(Quaternion prevRotation, Quaternion currentRotation)
		{
			int direction = CcwBetweenXZ(
				prevRotation * Vector3.one,
				Vector3.zero,
				currentRotation * Vector3.one
			);
			return Quaternion.Angle(prevRotation, currentRotation) * direction;
		}

		public static void Clamp(ref float value, float min, float max)
		{
			value = Mathf.Clamp(value, min, max);
		}

		public static void Clamp(ref int value, int min, int max)
		{
			value = Clamp(value, min, max);
		}

		public static int Clamp(int value, int min, int max)
		{
			if (value < min) return min;
			if (value > max) return max;
			return value;
		}

		public static float Clamp(float value, float min, float max)
		{
			if (value < min) return min;
			if (value > max) return max;
			return value;
		}

		public static int GetIndexOfAngle(Quaternion rotation)
		{
			float indexAngleUnit = 22.5f;
			float angle          = rotation.eulerAngles.y % 360;
			float halfAngleUnit  = indexAngleUnit / 2;
			float remainder      = (angle + halfAngleUnit) % indexAngleUnit;
			return Mathf.RoundToInt((angle - remainder + halfAngleUnit) / indexAngleUnit) % 16;
		}

		public static float GetAxisAngle(Vector3 vectorA, Vector3 vectorB, Vector3 axis, bool signRevise = true)
		{
			var projA       = Vector3.ProjectOnPlane(vectorA, axis);
			var projB       = Vector3.ProjectOnPlane(vectorB, axis);
			var rotateAngle = Vector3.Angle(projA, projB);
			if (signRevise)
			{
				var projARight = Quaternion.AngleAxis(90, axis) * projA;

				if (Vector3.Dot(projARight, projB) < 0)
					rotateAngle *= -1;
			}

			return rotateAngle;
		}

		public static int ToMilliseconds(float seconds)
		{
			return Mathf.RoundToInt(seconds * 1000);
		}

		public static float ToSeconds(int milliseconds)
		{
			return milliseconds * 0.001f;
		}

		public static int ToSecondsInt(int milliseconds)
		{
			return Mathf.RoundToInt(milliseconds * 0.001f);
		}

		/// <summary>
		/// 길찾기 알고리즘을 이용하지 않고 직선상의 웨이포인트를 구한다.
		/// </summary>
		/// <param name="startPoint">길찾기를 시작할 포지션</param>
		/// <param name="endPoint">길찾기의 목적지</param>
		/// <param name="wayPointDistance">웨이포인트 간의 거리</param>
		/// <returns></returns>
		public static List<Vector3> GetStraightWayPoints(Vector3 startPoint, Vector3 endPoint, float wayPointDistance)
		{
			var wayPoints = new List<Vector3>();
			return GetStraightWayPoints(startPoint, endPoint, wayPointDistance, wayPoints);
		}

		/// <summary>
		/// 길찾기 알고리즘을 이용하지 않고 직선상의 웨이포인트를 구한다.
		/// </summary>
		/// <param name="startPoint">길찾기를 시작할 포지션</param>
		/// <param name="endPoint">길찾기의 목적지</param>
		/// <param name="wayPointDistance">웨이포인트 간의 거리</param>
		/// <param name="wayPoints">웨이포인트가 저장될 리스트</param>
		/// <returns></returns>
		public static List<Vector3> GetStraightWayPoints(Vector3 startPoint, Vector3 endPoint, float wayPointDistance, [NotNull] List<Vector3> wayPoints)
		{
			wayPoints.Clear();
			wayPoints.Add(startPoint);

			var startToEndVector    = (endPoint - startPoint);
			var startToEndDirection = startToEndVector.normalized;
			var distance            = startToEndVector.magnitude;
			var numOfSubWaypoint    = Mathf.Floor(distance / wayPointDistance);
			for (var i = 1; i <= numOfSubWaypoint; ++i)
				wayPoints.Add(startPoint + startToEndDirection * (i * wayPointDistance));

			wayPoints.Add(endPoint);
			return wayPoints;
		}

		public static float GetDistanceFromWaypoint(Vector3 worldPosition, List<Vector3> wayPoints, int startIndex = 0)
		{
			if (wayPoints == null || wayPoints.Count <= startIndex) return 0.0f;

			var distance = (worldPosition - wayPoints[startIndex]).magnitude;
			if (wayPoints.Count > startIndex + 1)
			{
				var temp = wayPoints[startIndex];
				for (var i = startIndex + 1; i < wayPoints.Count; ++i)
				{
					distance += (wayPoints[i] - temp).magnitude;
					temp     =  wayPoints[i];
				}
			}

			return distance;
		}
	}
}
