using UnityEngine;

namespace Com2Verse.UI
{
    public static class MVVMUtil
    {
        public static string GetFullPathInHierarchy(Transform transform)
        {
            var path = $"/{transform.name}";

            while (!object.ReferenceEquals(transform.transform.parent, null))
            {
                transform = transform.parent;
                path = $"/{transform.name}{path}";
            }

            return path;
        }
    }
}
