/*===============================================================
* Product:    Com2Verse
* File Name:  RectTransformExtension.cs
* Developer:  hyj
* Date:       2022-04-20 11:58
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;

namespace Com2Verse.Extension
{
	public static class RectTransformExtension
	{
		public static readonly Vector2 TopPivotPosition      = new Vector2(0.5f, 1);
		public static readonly Vector2 TopLeftPivotPosition  = new Vector2(0,    1);
		public static readonly Vector2 TopRightPivotPosition = new Vector2(1,    1);
		public static readonly Vector2 LeftPivotPosition     = new Vector2(0,    0.5f);
		public static readonly Vector2 BottomPivotPosition   = new Vector2(0.5f, 0);

		/// <summary>
		/// Get the corners of the calculated rectangle in world space
		/// </summary>
		/// <param name="rectTransform">rect to get Corners</param>
		/// <returns>array of 4 vertices is clockwise. It starts bottom left</returns>
		public static Vector3[] GetCorners(this RectTransform rectTransform)
		{
			Vector3[] corners = new Vector3[4];
			rectTransform.GetWorldCorners(corners);
			return corners;
		}

		public static float MaxY(this RectTransform rectTransform) => 
			rectTransform.GetCorners()[1].y;

		public static float MinY(this RectTransform rectTransform) => 
			rectTransform.GetCorners()[0].y;

		public static float MaxX(this RectTransform rectTransform) => 
			rectTransform.GetCorners()[2].x;

		public static float MinX(this RectTransform rectTransform) =>
			rectTransform.GetCorners()[0].x;

		private static void SetAnchorPosition(this RectTransform rectTransform, Vector2 pos)
		{
			SetAnchorPosition(rectTransform, pos, pos, pos);
		}

		private static void SetAnchorPosition(this RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
		{
			//Saving to reapply after anchoring. Width and height changes if anchoring is change. 
			var rect = rectTransform.rect;
			float width = rect.width;
			float height = rect.height;

			// Setting Anchor Position
			rectTransform.anchorMin = anchorMin;
			rectTransform.anchorMax = anchorMax;
			rectTransform.pivot = pivot;

			//Reapply size
			rectTransform.sizeDelta = new Vector2(width, height);
		}

		public static void SetTopAnchorStretch(this RectTransform rectTransform)
		{
			SetAnchorPosition(rectTransform, Vector2.up, Vector2.one, TopPivotPosition);
			rectTransform.sizeDelta = Vector2.zero;
		}

		public static void SetLeftAnchorStretch(this RectTransform rectTransform)
		{
			SetAnchorPosition(rectTransform, Vector2.zero, Vector2.up, LeftPivotPosition);
			rectTransform.sizeDelta = Vector2.zero;
		}

		public static void SetTopAnchor(this RectTransform rectTransform) =>
			SetAnchorPosition(rectTransform, TopPivotPosition);

		public static void SetTopLeftAnchor(this RectTransform rectTransform) =>
			SetAnchorPosition(rectTransform, TopLeftPivotPosition);

		public static void SetLeftAnchor(this RectTransform rectTransform) =>
			SetAnchorPosition(rectTransform, LeftPivotPosition);

		public static void SetStretch(this RectTransform rectTransform)
		{
			SetAnchorPosition(rectTransform, Vector2.zero, Vector2.one, Vector2.one * 0.5f);
			rectTransform.sizeDelta = Vector2.zero;
		}
	}
}
