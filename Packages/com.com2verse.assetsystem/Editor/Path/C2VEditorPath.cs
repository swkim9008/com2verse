/*===============================================================
* Product:		Com2Verse
* File Name:	C2VEditorPath.cs
* Developer:	tlghks1009
* Date:			2023-04-18 15:58
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using UnityEngine;

namespace Com2VerseEditor.AssetSystem
{
	public static class C2VEditorPath
	{
		/// <summary>
		/// Sftp 서버 Url
		/// </summary>
		public static readonly string SftpRemotePath = "sftp.finder.co.kr";

		/// <summary>
		/// Sftp 인증용 유저 이름
		/// </summary>
		public static readonly string SftpUserName = "tlghks1009sftp-1";

		/// <summary>
		/// Sftp 인증 키 경로
		/// </summary>
		public static readonly string SftpAuthKeyPath = @"D:\sftp\tlghks1009sftp-1.key";

		/// <summary>
		/// Sftp CDN 리모트 주소 -> ~~/ClientTeam/AssetBundles
		/// </summary>
		public static readonly string SftpRemoteBundlePath = "metaverse-platform-seoul/metaverse-platform/ClientTeam/AssetBundle";

		/// <summary>
		///  빌드 시 번들 캐시 경로
		/// </summary>
		public static readonly string BundleCacheInfoDirectoryPath = $"{Application.dataPath}/Project/Resources/AssetBundle";

		/// <summary>
		/// 번들 캐시 정보 파일 이름
		/// </summary>
		public static readonly string BundleCacheInfoFileName = "assetBundleCacheInfo.json";

		/// <summary>
		/// 에셋 번들 버전
		/// </summary>
		public static string AssetBundleVersion => AppInfo.Instance.Data.AssetBundleVersion;

		/// <summary>
		/// 빌드 타겟
		/// </summary>
		public static string BuildTarget => AppInfo.Instance.Data.BuildTarget;

		/// <summary>
		/// 현재 빌드 환경
		/// </summary>
		public static string BuildEnvironment => AppInfo.Instance.Data.Environment.ToString();


		/// <summary>
		/// 리모트 빌드 타겟 주소
		/// </summary>
		public static string SftpRemoteBuildTargetPath => $"/{SftpRemoteBundlePath}/{BuildEnvironment}/{BuildTarget}";
	}
}
