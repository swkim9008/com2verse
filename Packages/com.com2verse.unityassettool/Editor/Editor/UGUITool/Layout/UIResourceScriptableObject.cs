/*===============================================================
* Product:		Com2Verse
* File Name:	PopupResourceScriptableObject.cs
* Developer:	jooinsung
* Date:			2023-04-13 21:44
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor;

namespace Com2Verse
{
    [CreateAssetMenu(fileName = "UIResource", menuName = "ScriptableObjects/UIResourceScriptableObject", order = 1)]
    public sealed class UIResourceScriptableObject : ScriptableObject
	{
        
        public Sprite image_Bg;

        [Header("Button")]
        public Sprite button_Cancle;
        public Sprite button_Apply;

        [Header("Animation")]
        public AnimationClip popupOpen;
        public AnimationClip popupClose;
    }

    [CustomEditor(typeof(UIResourceScriptableObject))]
    public class UIResourceScriptableObjectEditor : Editor
    {
        UIResourceScriptableObject value;

        private GUILayoutOption[] options;

        private void OnEnable()
        {
            options = new GUILayoutOption[] { GUILayout.Width(128), GUILayout.Height(128) };
            value = serializedObject.targetObject as UIResourceScriptableObject;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            //EditorGUILayout.PropertyField(image_Bg);
            
            value.image_Bg =  (Sprite)EditorGUILayout.ObjectField("윈도우 배경이미지", value.image_Bg, typeof(Sprite), true);
            value.button_Cancle = (Sprite)EditorGUILayout.ObjectField("부정 버튼 이미지", value.button_Cancle, typeof(Sprite), true);
            value.button_Apply = (Sprite)EditorGUILayout.ObjectField("긍정 버튼 이미지", value.button_Apply, typeof(Sprite), true);
            GUILine(1);
            serializedObject.ApplyModifiedProperties();
        }

        void GUILine(int lineHeight = 1)
        {
            EditorGUILayout.Space();
            Rect rect = EditorGUILayout.GetControlRect(false, lineHeight);
            rect.height = lineHeight;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space();
        }
    }


}
