/*===============================================================
* Product:		Com2Verse
* File Name:	BezierPosGenerator.cs
* Developer:	eugene9721
* Date:			2023-02-15 11:27
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;
using System.Collections.Generic;

namespace Com2Verse.Utils
{
	public struct BezierPoint
	{
		private Vector3 _pos;
		private float   _delta;

		public Vector3 Position => _pos;
		public float   Delta    => _delta;

		public BezierPoint(Vector3 pos, float delta)
		{
			_pos   = pos;
			_delta = delta;
		}
	}

	public enum eBezierCurveType
	{
		INVALID = 0,
		LINEAR,
		QUADRATIC,
		CUBIC
	}

	public sealed class BezierPosGenerator
	{
		private const int MaxReviseSegmentNum = 5;

		private readonly List<BezierPoint> _leftControlPoints  = new List<BezierPoint>();
		private readonly List<BezierPoint> _rightControlPoints = new List<BezierPoint>();

		// SubDivide

		private Vector3 CalculateCubicBezierPoint(BezierPoint p0, BezierPoint p1, BezierPoint p2, BezierPoint p3, float delta)
		{
			var startDelta = p0.Delta;
			var endDelta   = p3.Delta;

			var totalDelta         = endDelta - startDelta;
			var diffFromStartDelta = delta - startDelta;
			var t                  = totalDelta == 0 ? 0 : diffFromStartDelta / totalDelta;

			var tt  = t * t;
			var ttt = t * tt;
			var u   = 1.0f - t;
			var uu  = u * u;
			var uuu = u * uu;

			var result = p0.Position * uuu;
			result += p1.Position * 3.0f * uu * t;
			result += p2.Position * 3.0f * u * tt;
			result += p3.Position * ttt;

			return result;
		}

		private Vector3 CalculateQuadraticBezierPoint(BezierPoint p0, BezierPoint p1, BezierPoint p2, float delta)
		{
			var startDelta = p0.Delta;
			var endDelta   = p2.Delta;

			var totalDelta         = endDelta - startDelta;
			var diffFromStartDelta = delta - startDelta;
			var t                  = totalDelta == 0 ? 0 : diffFromStartDelta / totalDelta;

			var tt  = t * t;
			var u   = 1.0f - t;
			var uu  = u * u;

			var result = p1.Position;
			result += (p0.Position - p1.Position) * uu;
			result += (p2.Position - p1.Position) * tt;

			return result;
		}

		private Vector3 CalculateLinearBezierPoint(BezierPoint p0, BezierPoint p1, float delta)
		{
			var startDelta = p0.Delta;
			var endDelta   = p1.Delta;

			var totalDelta         = endDelta - startDelta;
			var diffFromStartDelta = delta - startDelta;
			var t                  = totalDelta == 0 ? 0 : diffFromStartDelta / totalDelta;

			var result = p0.Position;
			result += (p1.Position - p0.Position) * t;

			return result;
		}

		public Vector3 GetBezierPos(List<BezierPoint> points, float delta, eBezierCurveType type)
		{
			if (points.Count == 0)
				return Vector3.zero;

			if (type == eBezierCurveType.INVALID)
				return points[0].Position;

			var bezierPointLimit = (int)type;

			if (points.Count < bezierPointLimit)
				return GetBezierPos(points, delta, (eBezierCurveType)points.Count);

			_leftControlPoints.Clear();
			_rightControlPoints.Clear();

			for (var index = 0; index < points.Count; index++)
			{
				var controlPoint = points[index];
				if (index == 0 && controlPoint.Delta >= delta)
					return controlPoint.Position;

				if (index == points.Count - 1 && controlPoint.Delta <= delta)
					return controlPoint.Position;

				if (controlPoint.Delta <= delta)
					_leftControlPoints.Add(controlPoint);

				else if (controlPoint.Delta > delta)
					_rightControlPoints.Add(controlPoint);

				if (_rightControlPoints.Count >= bezierPointLimit - 1)
					break;
			}

			return SelectPoint(_leftControlPoints, _rightControlPoints, delta, type);
		}

		private Vector3 SelectPoint(IReadOnlyList<BezierPoint> leftPointList, IReadOnlyList<BezierPoint> rightPointList, float delta, eBezierCurveType type)
		{
			return type switch
			{
				eBezierCurveType.LINEAR    => SelectLinearPoint(leftPointList, rightPointList, delta),
				eBezierCurveType.QUADRATIC => SelectQuadraticPoint(leftPointList, rightPointList, delta),
				eBezierCurveType.CUBIC     => SelectCubicPoint(leftPointList, rightPointList, delta),
				_                          => Vector3.zero,
			};
		}

		private Vector3 SelectLinearPoint(IReadOnlyList<BezierPoint> leftPointList, IReadOnlyList<BezierPoint> rightPointList, float delta)
		{
			if (leftPointList.Count == 0)
				return rightPointList[0].Position;

			var leftEndIndex = leftPointList.Count - 1;

			if (rightPointList.Count == 0)
				return leftPointList[leftEndIndex].Position;

			return CalculateLinearBezierPoint(leftPointList[leftEndIndex], rightPointList[0], delta);
		}

		private Vector3 SelectQuadraticPoint(IReadOnlyList<BezierPoint> leftPointList, IReadOnlyList<BezierPoint> rightPointList, float delta)
		{
			if (leftPointList.Count + rightPointList.Count < 3)
				return Vector3.zero;

			if (leftPointList.Count == 0)
				return rightPointList[0].Position;

			if (rightPointList.Count == 0)
				return leftPointList[^1].Position;

			int leftCount  = leftPointList.Count;
			int rightCount = rightPointList.Count;
			int startIndex;

			BezierPoint p1;
			BezierPoint p2;

			if (leftCount >= 1 && rightCount >= 2)
			{
				startIndex = leftCount - 1;
				p1 = rightPointList[0];
				p2 = rightPointList[1];
			}
			else
			{
				startIndex = leftCount - 2;
				p1 = leftPointList[leftCount - 1];
				p2 = rightPointList[0];
			}

			var p0 = ReviseStartControlPoint(startIndex, leftPointList, rightPointList, eBezierCurveType.QUADRATIC);
			return CalculateQuadraticBezierPoint(p0, p1, p2, delta);
		}

		private Vector3 SelectCubicPoint(IReadOnlyList<BezierPoint> leftPointList, IReadOnlyList<BezierPoint> rightPointList, float delta)
		{
			if (leftPointList.Count + rightPointList.Count < 4)
				return Vector3.zero;

			if (leftPointList.Count == 0)
				return rightPointList[0].Position;

			if (rightPointList.Count == 0)
				return leftPointList[^1].Position;

			int leftCount  = leftPointList.Count;
			int rightCount = rightPointList.Count;
			int startIndex;

			BezierPoint p1;
			BezierPoint p2;
			BezierPoint p3;

			switch (leftCount)
			{
				case >= 1 when rightCount >= 3:
					startIndex = leftCount - 1;
					p1         = rightPointList[0];
					p2         = rightPointList[1];
					p3         = rightPointList[2];
					break;
				case >= 2 when rightCount >= 2:
					startIndex = leftCount - 2;
					p1         = leftPointList[leftCount - 1];
					p2         = rightPointList[0];
					p3         = rightPointList[1];
					break;
				default:
					startIndex = leftCount - 3;
					p1         = leftPointList[leftCount - 2];
					p2         = leftPointList[leftCount - 1];
					p3         = rightPointList[0];
					break;
			}

			var p0 = ReviseStartControlPoint(startIndex, leftPointList, rightPointList, eBezierCurveType.CUBIC);
			return CalculateCubicBezierPoint(p0, p1, p2, p3, delta);
		}

		private BezierPoint ReviseStartControlPoint(int pointStartIndex, IReadOnlyList<BezierPoint> leftPointList, IReadOnlyList<BezierPoint> rightPointList, eBezierCurveType type)
		{
			if (pointStartIndex == 0)
				return leftPointList[pointStartIndex];

			var argCpStartIndex = pointStartIndex - MaxReviseSegmentNum;
			argCpStartIndex = argCpStartIndex < 0 ? 0 : argCpStartIndex;
			BezierPoint p0 = leftPointList[argCpStartIndex];

			switch (type)
			{
				case eBezierCurveType.QUADRATIC:
					while (argCpStartIndex < pointStartIndex)
					{
						var p1 = ShiftControlPoint(argCpStartIndex + 1, leftPointList, rightPointList);
						var p2 = ShiftControlPoint(argCpStartIndex + 2, leftPointList, rightPointList);

						var newStartBezierPos = CalculateQuadraticBezierPoint(p0, p1, p2, p1.Delta);
						p0 = new BezierPoint(newStartBezierPos, p1.Delta);
						argCpStartIndex++;
					}
					return p0;
				case eBezierCurveType.CUBIC:
					while (argCpStartIndex < pointStartIndex)
					{
						var p1 = ShiftControlPoint(argCpStartIndex + 1, leftPointList, rightPointList);
						var p2 = ShiftControlPoint(argCpStartIndex + 2, leftPointList, rightPointList);
						var p3 = ShiftControlPoint(argCpStartIndex + 3, leftPointList, rightPointList);

						var newStartBezierPos = CalculateCubicBezierPoint(p0, p1, p2, p3, p1.Delta);
						p0 = new BezierPoint(newStartBezierPos, p1.Delta);
						argCpStartIndex++;
					}
					return p0;
				default:
					return p0;
			}
		}

		/// <summary>
		/// left와 right를 하나의 리스트인것 처럼 이어서 포인트를 산출
		/// 인덱스를 델타처럼 사용하므로 정렬되었을 경우에만 사용 가능
		/// </summary>
		private BezierPoint ShiftControlPoint(int shiftIndex, IReadOnlyList<BezierPoint> leftPointList, IReadOnlyList<BezierPoint> rightPointList)
		{
			return shiftIndex > leftPointList.Count - 1 ? rightPointList[shiftIndex - (leftPointList.Count - 1) - 1] : leftPointList[shiftIndex];
		}

		public List<Vector3> MakePath(List<BezierPoint> inputList, eBezierCurveType type, int pointCount)
		{
			List<Vector3> pathPoints = new List<Vector3>();

			int maxPointNum = pointCount;

			var startDelta = inputList[0].Delta;
			var endDelta   = inputList[^1].Delta;
			var unitDelta  = (endDelta - startDelta) / maxPointNum;

			for (var index = 0; index < maxPointNum; index++)
			{
				var currentDelta = unitDelta * index;
				pathPoints.Add(GetBezierPos(inputList, currentDelta, type));
			}

			return pathPoints;
		}
	}
}
