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
using System;
using NUnit.Framework.Constraints;

namespace Com2Verse
{
    [CreateAssetMenu(fileName = "SpriteMatchingTableScriptableObject", menuName = "ScriptableObjects/SpriteMatchingTableScriptableObject", order = 1)]
    public sealed class SpriteMatchingTableScriptableObject : ScriptableObject
	{
        public SpritePair[] sprites = new SpritePair[] { };
        
        
        //public List<Sprite[]> sprites = new List<Sprite[]>();
    }

    [Serializable]
    public class SpritePair
    {
        public Sprite fromSprite;
        public Sprite toSprite;
    }



    [CustomEditor(typeof(SpriteMatchingTableScriptableObject))]
    public class SpriteMatchingTableScriptableObjectEditor : Editor
    {
        SpriteMatchingTableScriptableObject value;

        private GUILayoutOption[] options;

        private void OnEnable()
        {
            this.options = new GUILayoutOption[] { GUILayout.Width(128), GUILayout.Height(128) };

            value = serializedObject.targetObject as SpriteMatchingTableScriptableObject;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            for (int i = 0; i < value.sprites.Length; i++) 
            {
                GUILayout.BeginHorizontal();
                value.sprites[i].fromSprite = (Sprite)EditorGUILayout.ObjectField(value.sprites[i].fromSprite, typeof(Sprite), true, this.options);
                EditorGUILayout.LabelField(">>>", GUILayout.Width(30), GUILayout.Height(128));
                value.sprites[i].toSprite = (Sprite)EditorGUILayout.ObjectField(value.sprites[i].toSprite, typeof(Sprite), true, this.options);
                GUILayout.EndHorizontal();
                GUILine(1);
            }

            

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
