/*===============================================================
* Product:    Com2Verse
* File Name:  AssemblyTypeCash.cs
* Developer:  tlghks1009
* Date:       2022-03-04 12:09
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Com2VerseEditor.UI
{
    public static class TypeMemberHolderEditor
    {
        private static readonly Dictionary<Func<MethodInfo, bool>, List<MethodInfo>> _methodInfoListDict = new();
        private static readonly Dictionary<(Type, Func<FieldInfo, bool>), List<FieldInfo>> _fieldInfoListDict = new();
        private static readonly Dictionary<(Type, Func<PropertyInfo, bool>), List<PropertyInfo>> _propertyInfoListDict = new();
        private static readonly Dictionary<Type, FieldInfo> _fieldInfoDict = new();

        public static List<MethodInfo> GetMethods(Type type, Func<MethodInfo, bool> condition)
        {
            if (_methodInfoListDict.TryGetValue(condition, out var methodInfos))
            {
                return methodInfos;
            }

            methodInfos = type.GetMethods().Where(methodInfo => condition(methodInfo)).ToList();

            _methodInfoListDict.Add(condition, methodInfos);

            return methodInfos;
        }


        public static List<FieldInfo> GetFields(Type type, BindingFlags bindingAttr, Func<FieldInfo, bool> condition)
        {
            var key = (type, condition);
            if (_fieldInfoListDict.TryGetValue(key, out var fieldInfos))
            {
                return fieldInfos;
            }

            fieldInfos = type.GetFields(bindingAttr).Where(fieldInfo => condition(fieldInfo)).ToList();

            if (type.BaseType != null)
            {
                fieldInfos.AddRange(type.BaseType.GetFields(bindingAttr).Where(fieldInfo => condition(fieldInfo)).ToList());
            }


            _fieldInfoListDict.Add(key, fieldInfos);

            return fieldInfos;
        }


        public static List<PropertyInfo> GetProperties(Type type, Func<PropertyInfo, bool> condition)
        {
            var key = (type, condition);

            if (_propertyInfoListDict.TryGetValue(key, out var propertyInfos))
            {
                return propertyInfos;
            }

            propertyInfos = type.GetProperties().Where(propertyInfo => condition(propertyInfo)).ToList();

            _propertyInfoListDict.Add(key, propertyInfos);

            return propertyInfos;
        }


        public static FieldInfo GetField(Type type, string fieldName, BindingFlags bindingAttr)
        {
            if (_fieldInfoDict.TryGetValue(type, out var fieldInfo))
            {
                return fieldInfo;
            }

            fieldInfo = type.GetField(fieldName, bindingAttr);

            _fieldInfoDict.Add(type, fieldInfo);

            return fieldInfo;
        }

        public static void ClearAll()
        {
            _methodInfoListDict.Clear();
            _fieldInfoListDict.Clear();
            _propertyInfoListDict.Clear();
            _fieldInfoDict.Clear();
        }
    }
}
