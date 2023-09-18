/*===============================================================
* Product:    Com2Verse
* File Name:  SoundIndexPropertyDrawer.cs
* Developer:  yangsehoon
* Date:       2022-04-21 11:25
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#if UNITY_EDITOR
using Com2Verse.SoundSystem;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.SoundSystem
{
	[CustomPropertyDrawer(typeof(eSoundIndex))]
	public sealed class SoundIndexPropertyDrawer : PropertyDrawer
	{
		private const int Height = 48;
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return Height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position.height = Height;
			EditorGUI.HelpBox(position, "Do not use eSoundIndex in inspector. Use AssetReference instead. It is intended to be used in script only.", MessageType.Warning);
		}
	}
}
#endif
