using System;
using System.Collections.Generic;

namespace Com2Verse.LocalCache
{
    public enum eCacheType
    {
        DEFAULT = 0,
        PORTRAIT,
    }

    internal static class CacheTypeExtension
    {
        private static Dictionary<eCacheType, string> _nameMap;

        static CacheTypeExtension()
        {
            _nameMap = new Dictionary<eCacheType, string>();
            var cacheTypes = Enum.GetValues(typeof(eCacheType)) as eCacheType[];
            foreach (var type in cacheTypes)
                _nameMap[type] = type.ToString();
        }

        public static string GetName(this eCacheType type) => _nameMap[type];
    }
}
