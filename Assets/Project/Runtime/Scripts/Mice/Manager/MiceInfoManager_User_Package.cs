/*===============================================================
* Product:		Com2Verse
* File Name:	MiceInfoManager_User_Package.cs
* Developer:	klizzard
* Date:			2023-08-02 10:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.UI;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Mice
{
	public sealed partial class MiceInfoManager
	{
		public async UniTask SyncMyPackages()
		{
			MyUserInfo.PackageInfos.Clear();

			var result = await MiceWebClient.User.PackagesGet(Localization.Instance.CurrentLanguage.ToMiceLanguageCode());
			if (result && result.Data != null)
			{
				foreach (var package in result.Data)
				{
					MyUserInfo.AddOrUpdatePackageInfo(package);
					MyPackageClickedPrefs.Load(package.PackageId);
				}
			}
		}
	}
}
