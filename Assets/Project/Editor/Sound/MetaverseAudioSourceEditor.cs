/*===============================================================
 * Product:		Com2Verse
 * File Name:	MetaverseAudioSourceEditor.cs
 * Developer:	yangsehoon
 * Date:		2023-01-06 오전 11:00
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using Com2Verse.Sound;
using Com2Verse.SoundSystem;
using UnityEditor;
using UnityEngine.Audio;

namespace Com2VerseEditor.SoundSystem
{
	[CustomEditor(typeof(MetaverseAudioSource))]
	public class MetaverseAudioSourceEditor : Com2VerseEditor.Sound.MetaverseAudioSourceEditor
	{
		public override void OnInspectorGUI()
		{
			SerializedObject audioSourceSerializedObject = new SerializedObject((target as MetaverseAudioSource).AudioSource);
			audioSourceSerializedObject.Update();
			
			eAudioMixerGroup currentMixerGroup = SoundManagerEditor.GetMixerGroupIndex((AudioMixerGroup)audioSourceSerializedObject.FindProperty("OutputAudioMixerGroup").objectReferenceValue);
			currentMixerGroup = (eAudioMixerGroup)EditorGUILayout.EnumPopup("Target Mixer Group", currentMixerGroup);
			audioSourceSerializedObject.FindProperty("OutputAudioMixerGroup").objectReferenceValue = SoundManagerEditor.GetMixerGroup(currentMixerGroup);

			if (audioSourceSerializedObject.hasModifiedProperties)
				audioSourceSerializedObject.ApplyModifiedProperties();
			
			base.OnInspectorGUI();
		}
	}
}
