/*===============================================================
* Product:    Com2Verse
* File Name:  VidePlayerEditor.cs
* Developer:  yangsehoon
* Date:       2022-04-14 14:54
* History:    
* Documents:  VidePlayer custom editor. Prevent editing value directly in the video source component.
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using UnityEditor;
using UnityEngine.Video;

namespace Com2VerseEditor.Sound
{
	[CustomEditor(typeof(VideoPlayer))]
	public class VideoPlayerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox("Do not use VidePlayer component directly. Please use Metaverse Video Source Instead.", MessageType.Error);
		}
	}
}
