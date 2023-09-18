/*===============================================================
* Product:		Com2Verse
* File Name:	LayoutElementWithMaxValuesEditor.cs
* Developer:	tlghks1009
* Date:			2022-10-28 13:40
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.UIExtension;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Com2VerseEditor.UI
{
    [CustomEditor(typeof(LayoutElementWithMaxValues), true)]
    [CanEditMultipleObjects]
    public class LayoutMaxSizeEditor : LayoutElementEditor
    {
        private LayoutElementWithMaxValues _layoutMax;

        private SerializedProperty _maxHeightProperty;
        private SerializedProperty _maxWidthProperty;

        private SerializedProperty _useMaxHeightProperty;
        private SerializedProperty _useMaxWidthProperty;

        private RectTransform _myRectTransform;

        protected override void OnEnable()
        {
            base.OnEnable();

            _layoutMax = target as LayoutElementWithMaxValues;
            _myRectTransform = _layoutMax.transform as RectTransform;

            _maxHeightProperty = serializedObject.FindProperty("_maxHeight");
            _maxWidthProperty = serializedObject.FindProperty("_maxWidth");

            _useMaxHeightProperty = serializedObject.FindProperty("_useMaxHeight");
            _useMaxWidthProperty = serializedObject.FindProperty("_useMaxWidth");
        }

        public override void OnInspectorGUI()
        {
            Draw(_maxWidthProperty, _useMaxWidthProperty);
            Draw(_maxHeightProperty, _useMaxHeightProperty);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }

        void Draw(SerializedProperty property, SerializedProperty useProperty)
        {
            Rect position = EditorGUILayout.GetControlRect();

            GUIContent label = EditorGUI.BeginProperty(position, null, property);

            Rect fieldPosition = EditorGUI.PrefixLabel(position, label);

            Rect toggleRect = fieldPosition;
            toggleRect.width = 16;

            Rect floatFieldRect = fieldPosition;
            floatFieldRect.xMin += 16;


            var use = EditorGUI.Toggle(toggleRect, useProperty.boolValue);
            useProperty.boolValue = use;

            if (use)
            {
                EditorGUIUtility.labelWidth = 4;
                property.floatValue = EditorGUI.FloatField(floatFieldRect, new GUIContent(" "), property.floatValue);
                EditorGUIUtility.labelWidth = 0;
            }


            EditorGUI.EndProperty();
        }
    }
}
