/*===============================================================
* Product:    Com2Verse
* File Name:  PropertyFinder.cs
* Developer:  tlghks1009
* Date:       2022-03-04 14:18
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Reflection;
using Com2Verse.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Com2VerseEditor.UI
{
    public class BindableMember<TMember>
    {
        private readonly Type _rootType;
        private readonly ViewModelGroupAttribute _viewModelGroupAttribute;

        private TMember _memberInfo;
        private TMember _childInfo;

        public Type RootType => _rootType;
        public string RootName => _rootType.Name;
        public string RootNameByNicifyVariableName => ObjectNames.NicifyVariableName(_rootType.Name);


        public TMember MemberInfo => _memberInfo;
        public Type MemberType { get; }

        public ViewModelGroupAttribute ViewModelGroupAttribute => _viewModelGroupAttribute;
        public string MemberName => _memberInfo.ToString().Split(' ')[1];
        public string MemberNameByNicifyVariableName => ObjectNames.NicifyVariableName(MemberName);


        public BindableMember(Type rootType, TMember memberInfo, Type memberType)
        {
            _rootType = rootType;
            _memberInfo = memberInfo;
            MemberType = memberType;

            _viewModelGroupAttribute = (ViewModelGroupAttribute) _rootType?.GetCustomAttribute(typeof(ViewModelGroupAttribute));
        }
    }


    public static class Finder
    {
        public static List<BindableMember<Component>> GetComponents(GameObject gameObject)
        {
            var fields = new List<BindableMember<Component>>();
            var componentList = gameObject.GetComponents<Component>();

            for (int componentIndex = 0; componentIndex < componentList.Length; componentIndex++)
            {
                var component = componentList[componentIndex];
                if (!component)
                {
                    continue;
                }

                var type = component.GetType();
                var fieldInfo = new BindableMember<Component>(type, null, null);

                fields.Add(fieldInfo);
            }

            return fields;
        }


        public static List<BindableMember<PropertyInfo>> GetBindableProperties(GameObject gameObject, Func<PropertyInfo, bool> condition)
        {
            var properties = new List<BindableMember<PropertyInfo>>();
            var componentList = gameObject.GetComponents<Component>();

            foreach (var component in componentList)
            {
                if (!component)
                {
                    continue;
                }

                var type = component.GetType();
                GetProperties(type, properties, condition);
            }

            return properties;
        }


        public static List<BindableMember<PropertyInfo>> GetBindableProperties(Type[] types, Func<PropertyInfo, bool> condition)
        {
            var properties = new List<BindableMember<PropertyInfo>>();
            if (types == null)
            {
                return null;
            }

            foreach (var type in types)
            {
                GetProperties(type, properties, condition);
            }

            return properties;
        }

        public static List<BindableMember<FieldInfo>> GetUnityEvents(Type type)
        {
            var fieldInfos = new List<BindableMember<FieldInfo>>();
            var flags = AllBindingFlags;

            var unityEventFields = TypeMemberHolderEditor.GetFields(type, flags, (info) =>
                                                                        typeof(UnityEventBase).IsAssignableFrom(info.FieldType) &&
                                                                        (info.IsPublic || info.GetCustomAttributes(typeof(SerializeField), false).Length > 0)
            );

            foreach (var unityEventField in unityEventFields)
            {
                var unityEventFieldType = unityEventField.GetType();
                var bindableMember = new BindableMember<FieldInfo>(unityEventFieldType, unityEventField, null);

                fieldInfos.Add(bindableMember);
            }

            return fieldInfos;
        }

        public static List<BindableMember<PropertyInfo>> GetProperties(Type type, List<BindableMember<PropertyInfo>> collection, Func<PropertyInfo, bool> condition)
        {
            var propertyInfos = TypeMemberHolderEditor.GetProperties(type, (property) => condition(property)).ToArray();

            for (int propertyInfoIndex = 0; propertyInfoIndex < propertyInfos.Length; propertyInfoIndex++)
            {
                var propertyInfo = propertyInfos[propertyInfoIndex];
                var bindableMember = new BindableMember<PropertyInfo>(type, propertyInfo, propertyInfo.PropertyType);

                collection.Add(bindableMember);
            }

            return collection;
        }

        private static BindingFlags AllBindingFlags => BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    }
}
