/*===============================================================
* Product:    Com2Verse
* File Name:  AudioClipScriptReferenceEditor.cs
* Developer:  yangsehoon
* Date:       2022-04-18 10:38
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using System.Linq;
using Com2Verse.SoundSystem;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.SoundSystem
{
	[CustomPropertyDrawer(typeof(SoundManagerSetting.AudioClipScriptReference))]
	public sealed class AudioClipScriptReferencePropertyDrawer : PropertyDrawer 
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			int index = property.FindPropertyRelative("Index").intValue;
			bool notInScript = false;
			if (index > (int)eSoundIndex.NONE && index <= (int)Enum.GetValues(typeof(eSoundIndex)).Cast<eSoundIndex>().Max())
			{
				label = new GUIContent(((eSoundIndex)index).ToString());
			}
			else
			{
				label = new GUIContent("Not Scripted");
				notInScript = true;
			}

			EditorGUI.BeginProperty(position, label, property);
			Color originColor = GUI.color;
			if (notInScript)
				GUI.color = Color.red;
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			GUI.color = originColor;

			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			var indexRect = new Rect(position.x, position.y, 50, position.height);
			var refRect = new Rect(position.x + 55, position.y, position.width - 55, position.height);

			EditorGUI.PropertyField(indexRect, property.FindPropertyRelative("Index"), GUIContent.none);
			EditorGUI.PropertyField(refRect, property.FindPropertyRelative("Reference"), GUIContent.none);

			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}
	}
}
