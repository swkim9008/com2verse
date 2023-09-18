/*===============================================================
* Product:    Com2Verse
* File Name:  BinderEditor.cs
* Developer:  tlghks1009
* Date:       2022-03-17 18:00
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Reflection;
using Com2Verse.Logger;
using TMPro;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEngine;
using Binder = Com2Verse.UI.Binder;

namespace Com2VerseEditor.UI
{
    public struct KeyValueStruct
    {
        public int Key;
        public string Value;
    }

    public partial class BinderEditor : UnityEditor.Editor
    {
        protected Type[] ViewModelTypes => AssemblyUtils.ViewModelTypes;

        protected virtual void OnEnable()
        {
            AssemblyUtils.RefreshAssembly();
            AssemblyUtils.PrepareViewModelTypes();
        }

        protected virtual void OnDisable() { }

        protected void OpenSearchWindow(string[] displayList, SerializedProperty findKeyProperty, Action<int> selected)
        {
            var stringListSearchProvider = ScriptableObject.CreateInstance<StringListSearchProvider>();
            stringListSearchProvider.Set(displayList, findKeyProperty.stringValue, (result) =>
            {
                var index = Array.IndexOf(displayList!, result);
                if (index == -1)
                {
                    Debug.LogError("미스 매치 입니다.");
                    return;
                }

                selected?.Invoke(index);
            });

            SearchWindow.Open(
                new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), 500f, 200f),
                stringListSearchProvider);
        }


        protected void RecordHistory(string             result,
                                     SerializedProperty findKeyProperty,
                                     SerializedProperty propertyOwner,
                                     SerializedProperty property)
        {
            serializedObject.Update();

            findKeyProperty.stringValue = result;

            var items = result.Split('/');
            if (items.Length > 0) propertyOwner.stringValue = items[0];
            if (items.Length > 1) property.stringValue      = items[1];

            serializedObject.ApplyModifiedProperties();
        }

        protected (string[], string[], Dictionary<char, List<KeyValueStruct>>) GetBindableProperties<TMember>(List<BindableMember<TMember>> memberTypeInfos, bool elapsed = true)
        {
            var displayResults = new string[memberTypeInfos.Count];
            var targetResults = new string[memberTypeInfos.Count];
            var propertyStringDict = new Dictionary<char, List<KeyValueStruct>>();

            for (int i = 0; i < memberTypeInfos.Count; i++)
            {
                var memberTypeInfo = memberTypeInfos[i];
                if (memberTypeInfo.MemberInfo == null)
                {
                    displayResults[i] = memberTypeInfo.RootName;
                    targetResults[i] = memberTypeInfo.RootName;

                    RegisterPropertyStringDict(targetResults[i], i);
                }
                else
                {
                    var groupAttribute = memberTypeInfo.ViewModelGroupAttribute;
                    if (groupAttribute != null)
                    {
                        displayResults[i] = $"{groupAttribute.Name}/";
                    }
                    else if (elapsed)
                    {
                        displayResults[i] = ".../";
                    }

                    bool onlyGetter = false;
                    if (memberTypeInfo.MemberInfo is PropertyInfo propertyInfo)
                    {
                        if (propertyInfo.GetSetMethod() == null)
                            onlyGetter = true;
                    }

                    var rootTypeName    = memberTypeInfo.RootType.Name;
                    var memberName      = memberTypeInfo.MemberName;
                    var memberTypeName  = FormatFriendlyTypeName(memberTypeInfo.MemberType);
                    var memberSignature = onlyGetter ? "{ get; }" : "{ get; set; }";

                    displayResults[i] += $"{rootTypeName}/{memberTypeName} {memberName} {memberSignature}";
                    targetResults[i]  =  $"{rootTypeName}/{memberName}";

                    RegisterPropertyStringDict(targetResults[i], i);
                }
            }

            static string FormatFriendlyTypeName(Type type)
            {
                var friendlyTypeName = GetFriendlyTypeName(type);
                var genericArguments = type.GetGenericArguments();

                if (genericArguments.Length == 0)
                    return friendlyTypeName;

                friendlyTypeName = friendlyTypeName.Replace($"`{genericArguments.Length}", "");
                return $"{friendlyTypeName}{FormatGenericArguments(genericArguments)}";
            }

            static string GetFriendlyTypeName(Type type) => type switch
            {
                _ when type == typeof(bool)    => "bool",
                _ when type == typeof(byte)    => "byte",
                _ when type == typeof(sbyte)   => "sbyte",
                _ when type == typeof(char)    => "char",
                _ when type == typeof(decimal) => "decimal",
                _ when type == typeof(double)  => "double",
                _ when type == typeof(float)   => "float",
                _ when type == typeof(int)     => "int",
                _ when type == typeof(uint)    => "uint",
                _ when type == typeof(long)    => "long",
                _ when type == typeof(ulong)   => "ulong",
                _ when type == typeof(object)  => "object",
                _ when type == typeof(short)   => "short",
                _ when type == typeof(ushort)  => "ushort",
                _ when type == typeof(string)  => "string",
                _                               => type.Name,
            };

            static string FormatGenericArguments(Type[] genericArguments)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append("<");
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    sb.Append(GetFriendlyTypeName(genericArguments[i]));
                    if (i < genericArguments.Length - 1)
                        sb.Append(", ");
                }

                sb.Append(">");
                return sb.ToString();
            }

            void RegisterPropertyStringDict(string propertyFullName, int index)
            {
                if (propertyStringDict.TryGetValue(propertyFullName[0]!, out var propertyNameList))
                {
                    var propertyKeeper = new KeyValueStruct
                    {
                        Key = index,
                        Value = propertyFullName,
                    };

                    propertyNameList.Add(propertyKeeper);
                }
                else
                {
                    propertyNameList = new List<KeyValueStruct>();
                    var propertyKeeper = new KeyValueStruct
                    {
                        Key = index,
                        Value = propertyFullName,
                    };
                    propertyNameList.Add(propertyKeeper);
                    if (!propertyStringDict.TryAdd(propertyFullName[0]!, propertyNameList))
                    {
                        C2VDebug.LogError("[DataBinding] Property Name error.");
                    }
                }
            }

            return (displayResults, targetResults, propertyStringDict);
        }

        protected BindableMember<TMember> ShowMemberTypeMenu<TMember>(Binder binder, string label,
                                                                      List<BindableMember<TMember>> memberTypeInfos, string memberFullPath,
                                                                      Action<string, string> updateValue)
        {
            if (memberTypeInfos.Count == 0)
                return null;

            string[] memberInfoNames = new string[memberTypeInfos.Count];
            string[] displayNames = new string[memberTypeInfos.Count];
            for (int memberTypeInfoIndex = 0; memberTypeInfoIndex < memberTypeInfos.Count; memberTypeInfoIndex++)
            {
                var memberTypeInfo = memberTypeInfos[memberTypeInfoIndex];
                if (String.Equals(label, "Event"))
                {
                    displayNames[memberTypeInfoIndex] = $"{memberTypeInfo.MemberName}";
                    memberInfoNames[memberTypeInfoIndex] = displayNames[memberTypeInfoIndex] =
                        $"{memberTypeInfo.MemberNameByNicifyVariableName}";
                    continue;
                }

                if (memberTypeInfo.MemberInfo == null)
                {
                    memberInfoNames[memberTypeInfoIndex] = $"{memberTypeInfo.RootNameByNicifyVariableName}";
                    displayNames[memberTypeInfoIndex] = $"{memberTypeInfo.RootName}";
                }
                else
                {
                    displayNames[memberTypeInfoIndex] =
                        $"{memberTypeInfo.RootType.Name}/{memberTypeInfo.MemberName}({memberTypeInfo.MemberType.Name})";
                    memberInfoNames[memberTypeInfoIndex] =
                        $"{memberTypeInfo.RootNameByNicifyVariableName}/{memberTypeInfo.MemberNameByNicifyVariableName}";
                }
            }

            var oldSelectedIndex = Array.IndexOf(memberInfoNames, memberFullPath);
            var newSelectedIndex = EditorGUILayout.Popup(label, oldSelectedIndex, displayNames);

            if (newSelectedIndex == -1)
                newSelectedIndex = 0;

            if (oldSelectedIndex != newSelectedIndex)
            {
                Undo.RecordObject(binder, "Changed Properties");

                updateValue(memberTypeInfos[newSelectedIndex].RootName,
                            memberTypeInfos[newSelectedIndex].MemberInfo == null
                                ? string.Empty
                                : memberTypeInfos[newSelectedIndex].MemberName);

                MarkSceneWithPrefabDirtyWhenPropertiesChanged(binder);
            }


            return memberTypeInfos[newSelectedIndex];
        }


        protected void UpdateTargetComponent(Binder binder, string componentName, Action<Component> updateValue)
        {
            Component targetComponent = null;

            if (!ParseReferenceComponent(out targetComponent, binder.gameObject, componentName))
                return;

            updateValue(targetComponent);
        }


        protected bool DrawDefaultInspectorWithoutScriptField()
        {
            EditorGUI.BeginChangeCheck();

            this.serializedObject.Update();
            SerializedProperty iterator = this.serializedObject.GetIterator();

            iterator.NextVisible(true);

            while (iterator.NextVisible(false))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }

            this.serializedObject.ApplyModifiedProperties();

            return (EditorGUI.EndChangeCheck());
        }


        protected string GetBindingFullPath(SerializedObject serializedObj, string bindingProperty)
        {
            var propertyOwnerName =
                ObjectNames.NicifyVariableName(serializedObj.FindProperty($"{bindingProperty}.propertyOwner")
                                                            .stringValue);
            if (string.IsNullOrEmpty(serializedObj.FindProperty($"{bindingProperty}.property").stringValue))
            {
                return propertyOwnerName;
            }

            var propertyName =
                ObjectNames.NicifyVariableName(serializedObj.FindProperty($"{bindingProperty}.property").stringValue);

            return $"{propertyOwnerName}/{propertyName}";
        }


        protected void MarkSceneWithPrefabDirtyWhenPropertiesChanged(Binder binder)
        {
            if (Application.isPlaying)
                return;

            EditorSceneManager.MarkSceneDirty(binder.gameObject.scene);

            PrefabUtility.RecordPrefabInstancePropertyModifications(binder);
        }


        protected int FindIndexOfSelectedProperty(Dictionary<char, List<KeyValueStruct>> dict, string findProperty)
        {
            if (string.IsNullOrEmpty(findProperty))
                return 0;

            var index = -1;

            if (dict.TryGetValue(findProperty[0]!, out var propertyKeeperList))
            {
                foreach (var propertyKeeper in propertyKeeperList)
                {
                    if (propertyKeeper.Value == findProperty)
                        index = propertyKeeper.Key;
                }
            }

            return index;
        }


        protected bool IsMouseDown(EditorGUILayout.HorizontalScope scope) =>
            Event.current.type == EventType.MouseDown && scope.rect.Contains(Event.current.mousePosition);

        private bool ParseReferenceComponent(out Component targetComponent, GameObject root, string componentName)
        {
            targetComponent = root.GetComponent(componentName);
            return !object.ReferenceEquals(targetComponent, null);
        }
        
        /// <summary>
        /// Source와 Target의 Type을 비교 합니다.
        /// </summary>
        public static bool ValidateType(SerializedProperty bindingModeProperty,
                                        SerializedProperty targetPropertyOwner,
                                        SerializedProperty sourcePropertyOwner,
                                        SerializedProperty targetProperty,
                                        SerializedProperty sourceProperty,
                                        Binder         binder)
        {
            if (!string.IsNullOrEmpty(targetPropertyOwner.stringValue) && !string.IsNullOrEmpty(sourcePropertyOwner.stringValue))
            {
                var targetOwner = binder.GetComponent(targetPropertyOwner.stringValue);
                var sourceOwner = AssemblyUtils.FindViewModelTypeByName(sourcePropertyOwner.stringValue);

                if (targetOwner == null || sourceOwner == null)
                {
                    return false;
                }

                var targetPropertyInfo = targetOwner.GetType().GetProperty(targetProperty.stringValue!);
                var sourcePropertyInfo = sourceOwner.GetProperty(sourceProperty.stringValue!);

                if (targetPropertyInfo == null || sourcePropertyInfo == null)
                {
                    return false;
                }

                var targetType = targetPropertyInfo.PropertyType;
                var sourceType = sourcePropertyInfo.PropertyType;

                var isCastable  = false;
                var bindingMode = (Binder.eBindingMode) bindingModeProperty.intValue;

                switch (bindingMode)
                {
                    case Binder.eBindingMode.TWO_WAY:
                    {
                        isCastable = (targetType.IsAssignableFrom(sourceType) && sourceType.IsAssignableFrom(targetType)) ||
                                     (sourceType.IsEnum                       && targetType == typeof(int))               ||
                                     (targetType.IsEnum                       && sourceType == typeof(int));
                    }
                        break;
                    case Binder.eBindingMode.ONE_TIME:
                    case Binder.eBindingMode.ONE_WAY_TO_TARGET:
                    {
                        isCastable = targetType.IsAssignableFrom(sourceType)          ||
                                     (sourceType.IsEnum && targetType == typeof(int)) ||
                                     (targetType.IsEnum && sourceType == typeof(int)) ||
                                     (IsCom2VerseAssignableFrom(targetOwner.GetType(), sourceType, targetPropertyInfo.Name));
                    }
                        break;
                    case Binder.eBindingMode.ONE_WAY_TO_SOURCE:
                    {
                        isCastable = sourceType.IsAssignableFrom(targetType)          ||
                                     (sourceType.IsEnum && targetType == typeof(int)) ||
                                     (targetType.IsEnum && sourceType == typeof(int));
                    }
                        break;
                }

                return isCastable;
            }

            return false;

            // Target이 Tmp_Text, TextMeshProUGUI, TMP_InputField Type이고, Property가 text 일 때,
            // Source의 PropertyType이 IConvertible 인터페이스를 상속 받고 있다면(ex : int, float, 등), 바인딩 허용
            bool IsCom2VerseAssignableFrom(Type targetType, Type sourceType, string propertyName)
            {
                var accessiblePropertyName = GetAccessiblePropertyName(targetType);

                if (propertyName == accessiblePropertyName)
                {
                    return sourceType.GetInterface("IConvertible") != null;
                }

                return false;
            }

            string GetAccessiblePropertyName(Type type) => type switch
            {
                _ when type == typeof(TMP_Text)        => "text",
                _ when type == typeof(TextMeshProUGUI) => "text",
                _ when type == typeof(TMP_InputField)  => "text",
                _                                      => string.Empty
            };
        }
    }
}
