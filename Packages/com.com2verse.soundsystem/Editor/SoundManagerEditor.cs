/*===============================================================
* Product:    Com2Verse
* File Name:  SoundManagerEditor.cs
* Developer:  yangsehoon
* Date:       2022-04-11 17:08
* History:    
* Documents:  SoundManager custom editor.
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#if UNITY_EDITOR
using Com2Verse.Sound;
using UnityEditor;

namespace Com2VerseEditor.Sound
{
    [CustomEditor(typeof(SoundManager))]
    public class SoundManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
	        EditorGUILayout.HelpBox("Metaverse Sound Manager", MessageType.None);
        }
    }
}
#endif
