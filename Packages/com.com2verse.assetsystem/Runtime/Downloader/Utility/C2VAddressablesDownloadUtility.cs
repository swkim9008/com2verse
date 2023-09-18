/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressableDownloadUtility.cs
* Developer:	tlghks1009
* Date:			2023-02-17 17:21
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.AssetSystem
{
    public enum eAddressableDownloadSizeType
    {
        BYTE,
        KB,
        MB,
        GB
    }


    public static class C2VAddressablesDownloadUtility
    {
        private static readonly long GB = 1_000_000_000;
        private static readonly long MB = 1_000_000;
        private static readonly long KB = 1_000;

        public static bool IsNetworkValid() => Application.internetReachability != NetworkReachability.NotReachable;

        public static bool IsDiskSpaceEnough(long requiredSize) => Caching.defaultCache.spaceFree >= requiredSize;

        public static eAddressableDownloadSizeType GetSizeType(long byteSize)
        {
            if (byteSize >= GB)
            {
                return eAddressableDownloadSizeType.GB;
            }

            if (byteSize >= MB)
            {
                return eAddressableDownloadSizeType.MB;
            }

            return byteSize >= KB ? eAddressableDownloadSizeType.KB : eAddressableDownloadSizeType.BYTE;
        }


        public static double ConvertByte(long byteSize, eAddressableDownloadSizeType sizeType)
        {
            return (byteSize / System.Math.Pow(1024, (long)sizeType));
        }


        public static string GetConvertedByteString(long byteSize, bool appendSizeType = true)
        {
            var sizeType = GetSizeType(byteSize);

            var sizeString = appendSizeType ? sizeType.ToString() : string.Empty;

            return $"{ConvertByte(byteSize, sizeType):0.00}{sizeString}";
        }
    }
}