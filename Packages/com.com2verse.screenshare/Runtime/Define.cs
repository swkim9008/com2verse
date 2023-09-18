/*===============================================================
 * Product:		Com2Verse
 * File Name:	Define.cs
 * Developer:	urun4m0r1
 * Date:		2023-01-09 15:20
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;

namespace Com2Verse.ScreenShare
{
	internal static class Define
	{
		internal static readonly int DefaultCaptureFps = 10;

		internal static readonly int MinimizedScreenRestoreDelay = 100;

		internal static readonly Vector2Int DefaultMinScreenSize          = new(x: 145, y: 49);
		internal static readonly Vector2Int DefaultMaxScreenSize          = new(x: 4096, y: 4096);
		internal static readonly Vector2Int DefaultRequestedThumbnailSize = new(x: 480, y: 360);
		internal static readonly Vector2Int DefaultRequestedScreenSize    = new(x: 1920, y: 1080);
	}
}
