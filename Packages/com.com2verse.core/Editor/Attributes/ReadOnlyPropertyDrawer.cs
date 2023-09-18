/*===============================================================
* Product:		Com2Verse
* File Name:	ReadOnlyPropertyDrawer.cs
* Developer:	urun4m0r1
* Date:			2022-05-30 15:39
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Utils;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.Utils
{
	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyPropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			EditorGUI.GetPropertyHeight(property, label, true);

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = true;
		}
	}
}
