/*===============================================================
* Product:		Com2Verse
* File Name:	UvFlipper.cs
* Developer:	urun4m0r1
* Date:			2022-03-31 20:01
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.Utils
{
	public static class UvFlipper
	{
#region Internal
		private static float Flip(float value, bool? flip = null)
		{
			if (flip == null) return -value;

			return Mathf.Abs(value) * ((bool)flip ? -1f : 1f);
		}
#endregion //Internal

#region ObjectExtensions
		public static bool IsHorizontalFlipped(this Transform value) => value.localScale.IsHorizontalFlipped();
		public static bool IsVerticalFlipped(this   Transform value) => value.localScale.IsVerticalFlipped();

		public static bool IsHorizontalFlipped(this RectTransform value) => value.localScale.IsHorizontalFlipped();
		public static bool IsVerticalFlipped(this   RectTransform value) => value.localScale.IsVerticalFlipped();

		public static bool IsHorizontalFlipped(this RawImage value) => value.uvRect.IsHorizontalFlipped();
		public static bool IsVerticalFlipped(this   RawImage value) => value.uvRect.IsVerticalFlipped();

		public static void FlipHorizontal(this Transform value, bool? flip = null) => value.localScale = value.localScale.GetHorizontalFlip(flip);
		public static void FlipVertical(this   Transform value, bool? flip = null) => value.localScale = value.localScale.GetVerticalFlip(flip);

		public static void FlipHorizontal(this RectTransform value, bool? flip = null) => value.localScale = value.localScale.GetHorizontalFlip(flip);
		public static void FlipVertical(this   RectTransform value, bool? flip = null) => value.localScale = value.localScale.GetVerticalFlip(flip);

		public static void FlipHorizontal(this RawImage value, bool? flip = null) => value.uvRect = value.uvRect.GetHorizontalFlip(flip);
		public static void FlipVertical(this   RawImage value, bool? flip = null) => value.uvRect = value.uvRect.GetVerticalFlip(flip);
#endregion //ObjectExtensions

#region StructExtensions
		public static Vector3 GetHorizontalFlip(this Vector3 value, bool? flip = null)
		{
			value.FlipHorizontal(flip);
			return value;
		}

		public static Vector3 GetVerticalFlip(this Vector3 value, bool? flip = null)
		{
			value.FlipVertical(flip);
			return value;
		}

		public static Vector2 GetHorizontalFlip(this Vector2 value, bool? flip = null)
		{
			value.FlipHorizontal(flip);
			return value;
		}

		public static Vector2 GetVerticalFlip(this Vector2 value, bool? flip = null)
		{
			value.FlipVertical(flip);
			return value;
		}

		public static Rect GetHorizontalFlip(this Rect value, bool? flip = null)
		{
			value.FlipHorizontal(flip);
			return value;
		}

		public static Rect GetVerticalFlip(this Rect value, bool? flip = null)
		{
			value.FlipVertical(flip);
			return value;
		}

		public static bool IsHorizontalFlipped(this in Vector3 value) => value.x < 0f;
		public static bool IsVerticalFlipped(this in   Vector3 value) => value.y < 0f;

		public static bool IsHorizontalFlipped(this in Vector2 value) => value.x < 0f;
		public static bool IsVerticalFlipped(this in   Vector2 value) => value.y < 0f;

		public static bool IsHorizontalFlipped(this in Rect value) => value.width  < 0f;
		public static bool IsVerticalFlipped(this in   Rect value) => value.height < 0f;

		public static void FlipHorizontal(this ref Vector3 value, bool? flip = null) => value.x = Flip(value.x, flip);
		public static void FlipVertical(this ref   Vector3 value, bool? flip = null) => value.y = Flip(value.y, flip);

		public static void FlipHorizontal(this ref Vector2 value, bool? flip = null) => value.x = Flip(value.x, flip);
		public static void FlipVertical(this ref   Vector2 value, bool? flip = null) => value.y = Flip(value.y, flip);

		public static void FlipHorizontal(this ref Rect value, bool? flip = null) => value.width = Flip(value.width,  flip);
		public static void FlipVertical(this ref   Rect value, bool? flip = null) => value.width = Flip(value.height, flip);
#endregion //StructExtensions
	}
}
