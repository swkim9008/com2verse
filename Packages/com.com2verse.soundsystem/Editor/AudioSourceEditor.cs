/*===============================================================
* Product:    Com2Verse
* File Name:  AudioSourceEditor.cs
* Developer:  yangsehoon
* Date:       2022-04-14 11:45
* History:    
* Documents:  AudioSource custom editor. Prevent editing value directly in the audio source component.
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.SoundSystem
{
	[CustomEditor(typeof(AudioSource))]
	public class AudioSourceEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox("Do not use AudioSource component directly. please use Metaverse Audio Source Instead.", MessageType.Error);
		}
	}
}
