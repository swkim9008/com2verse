/*===============================================================
* Product:		Com2Verse
* File Name:	NullCheckExtension.cs
* Developer:	haminjeong
* Date:			2022-06-29 12:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;

namespace Com2Verse.Extension
{
    public static class NullCheckExtension
    {
        public static bool IsReferenceNull([CanBeNull] this UnityEngine.Object obj) => ReferenceEquals(obj, null);

        // ReSharper restore once Unity.ExpensiveCode
        public static bool IsUnityNull([CanBeNull] this UnityEngine.Object obj) => !obj;

#if UNITY_EDITOR
        public static bool IsMissing([CanBeNull] this UnityEngine.Object obj) =>
            !obj.IsReferenceNull() && obj.IsUnityNull();
#endif
    }
}
