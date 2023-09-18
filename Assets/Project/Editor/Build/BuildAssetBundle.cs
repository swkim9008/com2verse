/*===============================================================
* Product:		Com2Verse
* File Name:	BuildAssetBundle.cs
* Developer:	tlghks1009
* Date:			2023-04-12 13:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#define BUILD_SCRIPT_LOG
using Com2Verse.Logger;
using Com2VerseEditor.AssetSystem;
using UnityEngine;

// #define DETAIL_BUILD_SCRIPT_LOG
// #define SONG_TEST

#if UNITY_EDITOR

#endif
namespace Com2VerseEditor.Build
{
	public static class BuildAssetBundle
	{
#region Test
		// Test 하시고 추후에 제거.
		public static void TestAPI()
		{
			//Upload();
		}
#endregion Test

		/// <summary>
		/// 새로운 빌드 Local
		/// </summary>
		public static void BuildLocal()
		{
			using (var compositeRoot = C2VAddressablesEditorCompositionRoot.RequestInstance())
			{
				var buildService = compositeRoot.ServicePack.GetService<C2VAddressablesBuildService>();

				buildService.Build(eEnvironment.LOCAL);
			}
		}


		/// <summary>
		/// 새로운 빌드 EditorHosted
		/// </summary>
		public static void BuildEditorHosted()
		{
			using (var compositeRoot = C2VAddressablesEditorCompositionRoot.RequestInstance())
			{
				var buildService = compositeRoot.ServicePack.GetService<C2VAddressablesBuildService>();

				buildService.Build(eEnvironment.EDITOR_HOSTED);
			}
		}

		/// <summary>
		/// 새로운 빌드 Remote
		/// </summary>
		public static void BuildRemote()
		{
			using (var compositeRoot = C2VAddressablesEditorCompositionRoot.RequestInstance())
			{
				var buildService = compositeRoot.ServicePack.GetService<C2VAddressablesBuildService>();

				buildService.Build(eEnvironment.REMOTE);
			}
		}


		/// <summary>
		/// 업데이트 빌드 Remote
		/// </summary>
		public static void UpdateBuildRemote()
		{
			using (var compositeRoot = C2VAddressablesEditorCompositionRoot.RequestInstance())
			{
				var buildService = compositeRoot.ServicePack.GetService<C2VAddressablesBuildService>();

				buildService.Build(eEnvironment.REMOTE);
			}
		}


		/// <summary>
		/// 새로운 빌드 후 업로드 Remote
		/// </summary>
		public static void BuildAndUploadRemote()
		{
			using (var compositeRoot = C2VAddressablesEditorCompositionRoot.RequestInstance())
			{
				var buildService = compositeRoot.ServicePack.GetService<C2VAddressablesBuildService>();

				buildService.BuildAndUpload(eEnvironment.REMOTE);
			}
		}


		/// <summary>
		/// 업데이트 빌드 후 업로드 Remote
		/// </summary>
		public static void UpdateBuildAndUploadRemote()
		{
			using (var compositeRoot = C2VAddressablesEditorCompositionRoot.RequestInstance())
			{
				var buildService = compositeRoot.ServicePack.GetService<C2VAddressablesBuildService>();

				buildService.BuildAndUpload(eEnvironment.REMOTE);
			}
		}
	}
}
