/*===============================================================
* Product:    Com2Verse
* File Name:  SoundManagerSettingEditor.cs
* Developer:  yangsehoon
* Date:       2022-04-18 14:48
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#if UNITY_EDITOR
using Com2Verse.SoundSystem;
using Com2VerseEditor.Sound;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.SoundSystem
{
	[CustomEditor(typeof(SoundManagerSetting))]
	public sealed class SoundManagerSettingEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			base.OnInspectorGUI();

			if (GUILayout.Button("Clear all sounds"))
			{
				if (EditorUtility.DisplayDialog("Warning", "Are you sure you want to clear entire lst?", "Yes", "No"))
				{
					(target as SoundManagerSetting).ClearClips();
				}
			}
			if (GUILayout.Button("Generate enum for script access"))
			{
				bool success = SoundFileIndexer.MakeEnum() && SoundFileIndexer.UpdateDictionary();
			}

			if (serializedObject.hasModifiedProperties)
				serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
