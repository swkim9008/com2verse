/*===============================================================
* Product:		Com2Verse
* File Name:	MapObjectUtil.cs
* Developer:	eugene9721
* Date:			2022-11-21 15:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;

namespace Com2Verse.Utils
{
	public static class MapObjectUtil
	{
		private static float _cellWidth;
		private static float _cellHeight;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			_cellHeight = 0;
			_cellWidth  = 0;
		}

		public static void SetCellSize(float cellWidth, float cellHeight)
		{
			_cellWidth = cellWidth;
			_cellHeight = cellHeight;
		}

		public static Quaternion ConversionChairCoordinateSystemToUnity(Transform transform)
		{
			var rotation = transform.rotation;
			rotation.SetLookRotation(-transform.up, transform.forward);
			return rotation;
		}

		public static Vector3 GetCellOffset(float cellIdHorizontal, float cellIdVertical) =>
			new(cellIdHorizontal * _cellWidth, 0, cellIdVertical * _cellHeight);

		public static Vector3 GetCellOffset(Vector2Int cellIndex) =>
			GetCellOffset(cellIndex.x, cellIndex.y);

		public static Vector3 GetCellOffset(Vector2 cellIndex) =>
			GetCellOffset(cellIndex.x, cellIndex.y);
	}
}
