using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Com2Verse.UI;

namespace Com2VerseEditor.UI
{
    public static class AssemblyUtils
    {
        private static readonly List<Type> AssemblyAllTypes = new();

        private static Type[] _viewModelTypes = null;
        public static  Type[] ViewModelTypes => _viewModelTypes;

        public static void RefreshAssembly()
        {
            if (AssemblyAllTypes.Count != 0)
            {
                return;
            }

            AssemblyAllTypes.Clear();

            var appDomain = AppDomain.CurrentDomain;
            var assemblies = appDomain.GetAssemblies();

            foreach (var t in assemblies.Select(t1 => t1.GetTypes()).SelectMany(types => types.Where(t => t.IsPublic)))
            {
                AssemblyAllTypes.Add(t);
            }
        }


        public static Type[] GetTypes<T>()
        {
            var typeList = new List<Type>();

            foreach (var assemblyType in AssemblyAllTypes)
            {
                if (!CompareToBaseType(assemblyType, typeof(T)))
                {
                    continue;
                }

                if (assemblyType.IsAbstract)
                {
                    continue;
                }

                typeList.Add(assemblyType);
            }

            return typeList.ToArray();
        }


        public static void PrepareViewModelTypes()
        {
            if (_viewModelTypes != null)
            {
                return;
            }

            _viewModelTypes = GetTypes<ViewModel>();
        }


        public static Type FindViewModelTypeByName(string typeName)
        {
            if (_viewModelTypes == null)
            {
                return null;
            }

            foreach (var viewModelType in _viewModelTypes)
            {
                if (typeName == viewModelType.Name)
                {
                    return viewModelType;
                }
            }

            return null;
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

        public static void ClearAll() => AssemblyAllTypes.Clear();


        public static Type GetType(string name)
        {
            foreach (var assemblyType in AssemblyAllTypes)
            {
                if (assemblyType.Name.Equals(name))
                {
                    return assemblyType;
                }
            }

            return null;
        }


        public static MethodInfo[] GetMethodInfoByAttribute<TAttribute>(Type instance) where TAttribute : Attribute
        {
            var methodInfos = new List<MethodInfo>();
            var methods = instance.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var t in methods)
            {
                var handler = t.GetCustomAttribute(typeof(TAttribute), true);
                if (handler != null)
                {
                    methodInfos.Add(t);
                }
            }

            return methodInfos.ToArray();
        }


        public static Type GetGenericFieldType(FieldInfo fieldInfo)
        {
            return fieldInfo.FieldType.GetGenericArguments()[0];
        }
    }
}
