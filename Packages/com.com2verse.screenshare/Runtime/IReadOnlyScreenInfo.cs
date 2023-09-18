/*===============================================================
 * Product:		Com2Verse
 * File Name:	IReadOnlyScreenInfo.cs
 * Developer:	urun4m0r1
 * Date:		2023-01-06 10:47
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using UnityEngine;

namespace Com2Verse.ScreenShare
{
	public interface IReadOnlyScreenInfo
	{
		event Action<string?>? TitleChanged;
		event Action<bool>?    VisibilityChanged;

		event Action<Texture2D?>? IconChanged;
		event Action<Texture2D?>? ThumbnailChanged;
		event Action<Texture2D?>? ScreenChanged;

		ScreenId    Id         { get; }
		eScreenType ScreenType { get; }

		string? Title     { get; }
		bool    IsVisible { get; }

		Texture2D? Icon      { get; }
		Texture2D? Thumbnail { get; }
		Texture2D? Screen    { get; }
	}
}
