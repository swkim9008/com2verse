/*===============================================================
* Product:    Com2Verse
* File Name:  TypeMemberHolder.cs
* Developer:  tlghks1009
* Date:       2022-04-19 16:27
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Com2Verse.UI
{
    public sealed class ViewModelTypeHolder
    {
        private static readonly Dictionary<string, Type> _typeDictionary = new();
        private static readonly List<string> ASSEMBLY_NAME = new List<string>() { "Com2Verse", "Com2Verse.MVVM.Runtime"};

        public static Type GetType(string typeName) => _typeDictionary.TryGetValue(typeName!, out var type) ? type : null;

        public static void Initialize() => RegisterViewModelTypes();

        private static void RegisterViewModelTypes()
        {
            //ClearAll();
            // var assembly = Assembly.Load(ASSEMBLY_NAME!);
            // foreach (var assemblyType in assembly.GetTypes())
            // {
            //     if (!CompareToBaseType(assemblyType, typeof(ViewModel)))
            //     {
            //         continue;
            //     }
            //
            //     if (assemblyType.IsAbstract)
            //     {
            //         continue;
            //     }
            //
            //     _typeDictionary.Add(assemblyType.Name, assemblyType);
            // }

            ClearAll();
            foreach (var assemblyType in ASSEMBLY_NAME)
                AddViewModelTypes(assemblyType);
        }

        private static void AddViewModelTypes(string name)
        {
            var assembly = Assembly.Load(name!);
            foreach (var assemblyType in assembly.GetTypes())
            {
                if (!CompareToBaseType(assemblyType, typeof(ViewModel)))
                {
                    continue;
                }

                if (assemblyType.IsAbstract)
                {
                    continue;
                }

                _typeDictionary.Add(assemblyType.Name, assemblyType);
            }
        }

        private static bool CompareToBaseType(Type type, Type fixedType)
        {
            var baseType = type.BaseType;
            if (baseType == fixedType)
            {
                return true;
            }

            while (baseType != null)
            {
                baseType = baseType.BaseType;
                if (baseType == fixedType)
                {
                    return true;
                }
            }

            return false;
        }


        public static void ClearAll()
        {
            _typeDictionary.Clear();
        }
    }
}
