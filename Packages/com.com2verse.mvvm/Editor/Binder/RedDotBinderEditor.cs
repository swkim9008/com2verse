/*===============================================================
* Product:		Com2Verse
* File Name:	RedDotBinderEditor.cs
* Developer:	NGSG
* Date:			2023-04-18 12:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Reflection;
using Com2Verse.UI;
using UnityEditor;
using Com2VerseEditor.UI;
using UnityEngine;
using Binder = Com2Verse.UI.Binder;
using CommandHandler = Com2Verse.UI.CommandHandler;


namespace Com2VerseEditor.UI
{
	[CustomEditor(typeof(RedDotBinder))]
	public sealed class RedDotBinderEditor : BinderEditor
	{
        private RedDotBinder _dataBinder;

        private SerializedProperty _commentProperty;
        private SerializedProperty _targetFindKeyProperty;
        private SerializedProperty _targetPropertyOwner;
        private SerializedProperty _targetProperty;
        private SerializedProperty _targetComponent;
        private SerializedProperty _sourceFindKeyProperty;
        private SerializedProperty _sourcePropertyOwner;
        private SerializedProperty _sourceProperty;

        private List<BindableMember<PropertyInfo>> _targetProperties;
        private List<BindableMember<PropertyInfo>> _sourceProperties;

        private (string[], string[], Dictionary<char, List<KeyValueStruct>>) _targetBindablePropertiesText;
        private (string[], string[], Dictionary<char, List<KeyValueStruct>>) _sourceBindablePropertiesText;

        protected override void OnEnable()
        {
            base.OnEnable();

            _dataBinder = target as RedDotBinder;
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
                _targetFindKeyProperty.stringValue = $"{_targetPropertyOwner.stringValue}/{_targetProperty.stringValue}";

            if (!string.IsNullOrEmpty(_sourcePropertyOwner.stringValue))
                _sourceFindKeyProperty.stringValue = $"{_sourcePropertyOwner.stringValue}/{_sourceProperty.stringValue}";


            if (!string.IsNullOrEmpty(_targetFindKeyProperty.stringValue))
            {
                if (string.IsNullOrEmpty(_targetPropertyOwner.stringValue))
                {
                    EditorGUILayout.HelpBox($"{_targetFindKeyProperty.stringValue}의 컴포넌트를 찾을 수 없습니다. 재지정 해주세요.", MessageType.Error);
                }
            }

            if (!string.IsNullOrEmpty(_sourceFindKeyProperty.stringValue))
            {
                if (string.IsNullOrEmpty(_sourcePropertyOwner.stringValue))
                {
                    EditorGUILayout.HelpBox($"{_sourceFindKeyProperty.stringValue}를 확인해주세요!", MessageType.Error);
                }
            }

            EditorGUILayout.PropertyField(_commentProperty);

            DrawTargetProperty();

            LineHelper.Draw(Color.gray);

            DrawSourceProperty();

            serializedObject.ApplyModifiedProperties();
        }


        private void DrawTargetProperty()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (var scope = new EditorGUILayout.HorizontalScope())
                {
                    _targetProperties ??= Finder.GetBindableProperties(_dataBinder.gameObject, (property) =>
                                                                           property.GetSetMethod(false) != null &&
                                                                           property.GetGetMethod(false) != null &&
                                                                           property.GetCustomAttribute(typeof(ObsoleteAttribute), true) == null);

                    if (_targetBindablePropertiesText.Item1 == null)
                        _targetBindablePropertiesText = base.GetBindableProperties(_targetProperties);

                    var displayList = _targetBindablePropertiesText.Item1;
                    var targetList = _targetBindablePropertiesText.Item2;

                    if (!ScriptShortcut(_targetFindKeyProperty) && IsMouseDown(scope))
                    {
                        OpenSearchWindow(displayList, _targetFindKeyProperty, (result) =>
                        {
                            RecordHistory(targetList[result], _targetFindKeyProperty, _targetPropertyOwner, _targetProperty);
                        });
                    }

                    EditorGUILayout.PropertyField(_targetFindKeyProperty, new GUIContent("Target Property"));
                }
            }

            DrawEvent();
        }


        private void DrawEvent()
        {
            int index = FindIndexOfSelectedProperty(_targetBindablePropertiesText.Item3, _targetFindKeyProperty.stringValue);
            if (index == -1)
            {
                if (!string.IsNullOrEmpty(_targetFindKeyProperty.stringValue))
                {
                    EditorGUILayout.HelpBox($"{_targetFindKeyProperty.stringValue}의 컴포넌트를 찾을 수 없습니다. \n바인딩은 가능 할 수 있지만, 꼭! 컴포넌트를 확인해 주세요.", MessageType.Error);
                }

                return;
            }

            var selectedBindablePropertyInfo = _targetProperties[index];
            UpdateTargetComponent(_dataBinder, _targetPropertyOwner.stringValue, (component) => { _targetComponent.objectReferenceValue = component; });
        }


        private void DrawSourceProperty()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (var scope = new EditorGUILayout.HorizontalScope())
                {
                    _sourceProperties ??= Finder.GetBindableProperties(ViewModelTypes, (property) =>
                                                                           property.GetGetMethod(false) != null &&
                                                                           property.PropertyType != typeof(CommandHandler) &&
                                                                           property.PropertyType != typeof(CommandHandler<int>) &&
                                                                           property.PropertyType != typeof(CommandHandler<long>) &&
                                                                           property.PropertyType != typeof(CommandHandler<string>) &&
                                                                           property.PropertyType != typeof(CommandHandler<bool>) &&
                                                                           property.GetCustomAttribute(typeof(ObsoleteAttribute), true) == null);


                    if (_sourceBindablePropertiesText.Item1 == null)
                    {
                        _sourceBindablePropertiesText = base.GetBindableProperties(_sourceProperties);
                    }

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
	}
}
