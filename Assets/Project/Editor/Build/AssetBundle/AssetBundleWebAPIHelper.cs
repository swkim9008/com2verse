/*===============================================================
* Product:		Com2Verse
* File Name:	AssetBundleDataTable.cs
* Developer:	tlghks1009
* Date:			2023-06-26 11:34
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.BuildHelper;

namespace Com2VerseEditor.Build
{
	public sealed class AssetBundleWebAPIHelper
	{
		public static string GetLastAppVersion(AssetEntity[] assetEntities, eBuildEnv buildEnv)
		{
			var keywordToFind = AppInfoData.GetEnvShortStr(buildEnv);

			foreach (var assetEntity in assetEntities)
			{
				var parts = assetEntity.APP_VERSION.Split('_');
				if (parts.Length > 0)
				{
					var appVersionKeyword = parts[1];
					if (appVersionKeyword == keywordToFind)
					{
						return assetEntity.APP_VERSION;
					}
				}
			}
			return string.Empty;
		}
	}
}
