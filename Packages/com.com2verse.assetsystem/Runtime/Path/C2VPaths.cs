/*===============================================================
* Product:		Com2Verse
* File Name:	C2VPaths.cs
* Developer:	tlghks1009
* Date:			2023-03-03 15:33
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.AssetSystem
{
	public static class C2VPaths
	{
		/// <summary>
		/// Group Builder Window 정보를 담고있는 디렉토리 경로
		/// </summary>
		public static readonly string GroupBuilderDirectoryPath = "AddressableAssetsData/C2VAddressableGroupBuildSettings";

		/// <summary>
		/// Group Builder Window 정보를 담고있는 파일 이름
		/// </summary>
		public static readonly string GroupBuilderFileName = "AddressableGroupBuildSettings.asset";

		/// <summary>
		/// 번들의 레이아웃 정보를 담고있는 디렉토리 경로
		/// </summary>
		public static readonly string BundleLayoutDirectoryPath = "AddressableAssetsData/C2VAddressableBundleLayout";

		/// <summary>
		/// 번들의 레이아웃 정보를 담고있는 파일 경로
		/// </summary>
		public static readonly string BundleLayoutFileName = "AddressableBundleLayout.json";

		/// <summary>
		/// 번들 캐시 폴더
		/// </summary>
		public static readonly string BundleCacheFolderPath = $"{Application.temporaryCachePath}/Bundles";

		/// <summary>
		/// CDN 다운로드 주소
		/// </summary>
		public static readonly string RemoteAssetBundleUrl = "https://metaverse-platform-fn.qpyou.cn/metaverse-platform/ClientTeam/AssetBundle";

		public static string TargetAssetBundleVersion => C2VAssetBundleManager.Instance.TargetAssetBundleVersion;
	}
}
