/*===============================================================
* Product:    Com2Verse
* File Name:  CommandBinderEditor.cs
* Developer:  tlghks1009
* Date:       2022-03-22 15:12
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
using CommandHandler = Com2Verse.UI.CommandHandler;

namespace Com2VerseEditor.UI
{
    [CustomEditor(typeof(CommandBinder))]
    public class CommandBinderEditor : BinderEditor
    {
        private CommandBinder _commandBinder = null;

        private SerializedProperty _commentProperty;
        private SerializedProperty _targetFindKeyProperty;
        private SerializedProperty _targetPropertyOwner;
        private SerializedProperty _targetProperty;
        private SerializedProperty _targetComponent;
        private SerializedProperty _sourceFindKeyProperty;
        private SerializedProperty _sourcePropertyOwner;
        private SerializedProperty _sourceProperty;

        private List<BindableMember<Component>> _targetProperties;
        private List<BindableMember<PropertyInfo>> _sourceProperties;

        private (string[], string[], Dictionary<char, List<KeyValueStruct>>) _targetBindablePropertiesText;
        private (string[], string[], Dictionary<char, List<KeyValueStruct>>) _sourceBindablePropertiesText;


        protected override void OnEnable()
        {
            base.OnEnable();

            _commandBinder = target as CommandBinder;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _commandBinder = null;
        }


        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspectorWithoutScriptField();

            serializedObject.Update();

            _commentProperty = serializedObject.FindProperty("_comment");
            _targetFindKeyProperty = serializedObject.FindProperty("_targetFindKey");
            _targetPropertyOwner = serializedObject.FindProperty("_targetPath.propertyOwner");
            _targetProperty = serializedObject.FindProperty("_targetPath.property");
            _targetComponent = serializedObject.FindProperty("_targetPath.component");
            _sourceFindKeyProperty = serializedObject.FindProperty("_sourceFindKey");
            _sourcePropertyOwner = serializedObject.FindProperty("_sourcePath.propertyOwner");
            _sourceProperty = serializedObject.FindProperty("_sourcePath.property");


            if (!string.IsNullOrEmpty(_targetPropertyOwner.stringValue))
                _targetFindKeyProperty.stringValue = $"{_targetPropertyOwner.stringValue}";

            if (!string.IsNullOrEmpty(_sourcePropertyOwner.stringValue))
                _sourceFindKeyProperty.stringValue = $"{_sourcePropertyOwner.stringValue}/{_sourceProperty.stringValue}";


            EditorGUILayout.PropertyField(_commentProperty);

            DrawTargetProperty();

            LineHelper.Draw(Color.gray);

            DrawSourceProperty();

            LineHelper.Draw(Color.gray);

            DrawAdditionalData();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTargetProperty()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (var scope = new EditorGUILayout.HorizontalScope())
                {
                    _targetProperties ??= Finder.GetComponents(_commandBinder.gameObject);

                    if (_targetBindablePropertiesText.Item1 == null || _targetBindablePropertiesText.Item2 == null)
                        _targetBindablePropertiesText = base.GetBindableProperties(_targetProperties, false);

                    var displayList = _targetBindablePropertiesText.Item1;
                    var targetList = _targetBindablePropertiesText.Item2;

                    if (!ScriptShortcut(_targetFindKeyProperty) && IsMouseDown(scope))
                    {
                        OpenSearchWindow(displayList, _targetFindKeyProperty, (result) =>
                        {
                            RecordHistory(targetList[result], _targetFindKeyProperty, _targetPropertyOwner, _targetProperty);
                        });
                    }

                    EditorGUILayout.PropertyField(_targetFindKeyProperty, new GUIContent("Target"));
                }
            }

            DrawEvent();
        }


        private void DrawEvent()
        {
            var index = FindIndexOfSelectedProperty(_targetBindablePropertiesText.Item3, _targetFindKeyProperty.stringValue);
            if (index == -1)
            {
                return;
            }

            var selectedBindableProperty = _targetProperties[index];
            UpdateTargetComponent(_commandBinder, _targetPropertyOwner.stringValue, (component) => { _targetComponent.objectReferenceValue = component; });

            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel += 1;
            var unityEventFields = Finder.GetUnityEvents(selectedBindableProperty.RootType);
            var selectedFieldInfo = ShowMemberTypeMenu<FieldInfo>(
                _commandBinder,
                "Event",
                unityEventFields,
                ObjectNames.NicifyVariableName(serializedObject.FindProperty("_eventPath.property").stringValue),
                (updateOwner, updateProperty) =>
                {
                    serializedObject.FindProperty("_eventPath.propertyOwner").stringValue = updateOwner;
                    serializedObject.FindProperty("_eventPath.property").stringValue = updateProperty;
                }
            );
            EditorGUI.indentLevel -= 1;
            if (selectedFieldInfo != null)
            {
                if (selectedFieldInfo.MemberName == "onValueChanged")
                {
                    EditorGUI.indentLevel += 2;
                    var toggleCondition = (CommandBinder.eToggleCondition) EditorGUILayout.EnumPopup((CommandBinder.eToggleCondition) serializedObject.FindProperty("_toggleCondition").intValue,
                                                                                                     GUILayout.Width(200));
                    serializedObject.FindProperty("_toggleCondition").intValue = (int) toggleCondition;
                    EditorGUI.indentLevel -= 2;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSourceProperty()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (var scope = new EditorGUILayout.HorizontalScope())
                {
                    _sourceProperties ??= Finder.GetBindableProperties(ViewModelTypes, (info) =>
                                                                           info.GetCustomAttribute(typeof(ObsoleteAttribute), true) == null &&
                                                                           (info.PropertyType == typeof(CommandHandler) ||
                                                                            info.PropertyType == typeof(CommandHandler<int>) ||
                                                                            info.PropertyType == typeof(CommandHandler<long>) ||
                                                                            info.PropertyType == typeof(CommandHandler<string>) ||
                                                                            info.PropertyType == typeof(CommandHandler<bool>)));

                    if (_sourceBindablePropertiesText.Item1 == null || _sourceBindablePropertiesText.Item2 == null)
                        _sourceBindablePropertiesText = base.GetBindableProperties(_sourceProperties);

                    var displayList = _sourceBindablePropertiesText.Item1;
                    var targetList = _sourceBindablePropertiesText.Item2;

                    if (!ScriptShortcut(_sourceFindKeyProperty) && IsMouseDown(scope))
                    {
                        OpenSearchWindow(displayList, _sourceFindKeyProperty, (result) => 
                        {
                            RecordHistory(targetList[result], _sourceFindKeyProperty, _sourcePropertyOwner, _sourceProperty);
                        });
                    }

                    EditorGUILayout.PropertyField(_sourceFindKeyProperty, new GUIContent("Source Property"));
                }
            }

            var index = FindIndexOfSelectedProperty(_sourceBindablePropertiesText.Item3, _sourceFindKeyProperty.stringValue);
            if (index == -1)
            {
                if (!string.IsNullOrEmpty(_sourceFindKeyProperty.stringValue))
                {
                    EditorGUILayout.HelpBox($"{_sourceFindKeyProperty.stringValue}의 프로퍼티를 찾을 수 없습니다. \n바인딩은 가능 할 수 있지만, 꼭! 프로퍼티를 확인해 주세요.", MessageType.Error);
                }
            }
        }


        private void DrawAdditionalData()
        {
            var additionalDataType = (CommandBinder.eBindingParameterType) serializedObject.FindProperty("_bindingParameter.bindingParameterType").intValue;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_bindingParameter.bindingParameterType"));

            switch (additionalDataType)
            {
                case CommandBinder.eBindingParameterType.BOOLEAN:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_bindingParameter.boolValue"));
                    break;
                case CommandBinder.eBindingParameterType.INT:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_bindingParameter.intValue"));
                    break;
                case CommandBinder.eBindingParameterType.LONG:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_bindingParameter.longValue"));
                    break;
                case CommandBinder.eBindingParameterType.FLOAT:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_bindingParameter.floatValue"));
                    break;
                case CommandBinder.eBindingParameterType.STRING:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_bindingParameter.stringValue"));
                    break;
            }
        }
    }
}
