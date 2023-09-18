/*===============================================================
* Product:		Com2Verse
* File Name:	MiceEventInfo_Package.cs
* Developer:	seaman2000
* Date:			2023-04-10 14:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;

namespace Com2Verse.Mice
{
    public partial class MiceEventInfo
    {
        public Dictionary<string, MicePackageInfo> PackageInfos { get; } = new();

        public bool AddOrUpdatePackageInfo(MiceWebClient.Entities.PackageEntity packageEntity)
        {
            if (packageEntity == null || string.IsNullOrEmpty(packageEntity.PackageId))
                return false;

            if (PackageInfos.TryGetValue(packageEntity.PackageId, out var packageInfo))
                packageInfo.Sync(packageEntity);
            else
                PackageInfos.Add(packageEntity.PackageId, new MicePackageInfo(packageEntity));

            return true;
        }
    }
}