using System;
using UnityEngine;

namespace Com2Verse.LocalCache
{
    [Serializable]
    internal class MetadataSettingInfo
    {
        public string CachePath;

        private static MetadataSettingInfo _default = new()
        {
            CachePath = Application.temporaryCachePath,
        };
        public static MetadataSettingInfo Default => _default;
    }
}
