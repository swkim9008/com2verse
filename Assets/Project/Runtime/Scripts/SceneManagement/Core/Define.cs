/*===============================================================
 * Product:		Com2Verse
 * File Name:	Define.cs
 * Developer:	urun4m0r1
 * Date:		2023-05-30 17:30
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;

namespace Com2Verse.SceneManagement
{
	public static class Define
	{
		public static readonly string SceneOverlaySettingsResourcesPath = "UI/SceneOverlaySettings";

		public static readonly string SceneSplashName  = "SceneSplash";
		public static readonly string SceneLoadingName = "SceneLoading";
		public static readonly string SceneWorldName   = "SceneWorld";

		public static readonly IReadOnlyList<string> NonAddressableSceneNames = new[]
		{
			SceneSplashName,
			SceneLoadingName,
		};
	}
}
