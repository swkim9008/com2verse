/*===============================================================
 * Product:		Com2Verse
 * File Name:	Define.cs
 * Developer:	urun4m0r1
 * Date:		2023-01-04 18:34
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.SmallTalk
{
	public static class Define
	{
		public static class TableIndex
		{
			public static readonly int Default   = 1;
			public static readonly int AreaBased = 2;
		}

		public static readonly int DistanceCheckInterval = 100;

		public static readonly int BlobNodeWeightUpdateInterval = 100;

		public static readonly float BlobNodeWeightIncreaseRate = 0.1f;

		public static readonly string BlobPrefabPath = "UI_SmallTalk_BlobConnector.prefab";
	}
}
