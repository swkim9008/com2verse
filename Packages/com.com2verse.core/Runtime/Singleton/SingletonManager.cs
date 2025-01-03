﻿/*===============================================================
* Product:    Com2Verse
* File Name:  SingletonManager.cs
* Developer:  urun4m0r1
* Date:       2023-01-12 16:46
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Reflection;
using UnityEngine;

namespace Com2Verse
{
    internal static class SingletonManager
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(SingletonDefine.SingletonDestroyLoadType)]
        private static void OnRuntimeInitialize()
        {
            if (!SingletonDefine.DestroySingletonInstancesOnLoad)
                return;

            DestroySingletonInstances();
            GC.Collect();
        }

        private static void DestroySingletonInstances()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    var isConcreteClass = type is { IsClass: true, IsAbstract: false, IsGenericType: false };
                    if (!isConcreteClass)
                        continue;

                    if (TryInvokeDestroySingletonInstanceMethod(type, typeof(DestroyableMonoSingleton<>))) continue;
                    if (TryInvokeDestroySingletonInstanceMethod(type, typeof(MonoSingleton<>))) continue;
                    if (TryInvokeDestroySingletonInstanceMethod(type, typeof(Singleton<>))) continue;
                }
            }
        }

        private static bool TryInvokeDestroySingletonInstanceMethod(Type? targetType, Type? genericType)
        {
            if (targetType == null || genericType == null)
                return false;

            if (!IsSubclassOfRawGeneric(targetType, genericType))
                return false;

            var type = genericType.MakeGenericType(targetType);
            InvokeDestroySingletonInstanceMethod(type);
            return true;
        }

        private static bool IsSubclassOfRawGeneric(Type? targetType, Type? genericType)
        {
            while (targetType != null && targetType != typeof(object))
            {
                var currentType = targetType.IsGenericType ? targetType.GetGenericTypeDefinition() : targetType;
                if (currentType == genericType)
                    return true;

                targetType = targetType.BaseType;
            }

            return false;
        }

        private static void InvokeDestroySingletonInstanceMethod(Type? type)
        {
            type?.GetMethod("DestroySingletonInstance", BindingFlags.Public | BindingFlags.Static)?.Invoke(null!, null!);
        }
#endif // UNITY_EDITOR
    }
}
