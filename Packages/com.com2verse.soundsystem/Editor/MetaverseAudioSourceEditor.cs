/*===============================================================
* Product:    Com2Verse
* File Name:  MetaverseAudioSourceEditor.cs
* Developer:  yangsehoon
* Date:       2022-04-11 09:40
* History:    
* Documents:  MetaverseAudioSource custom editor. (Hides origin AudioSource, destroy origin AudioSource when this destroyed)
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#if UNITY_EDITOR
using Com2Verse.Sound;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.Sound
{
    [CustomEditor(typeof(MetaverseAudioSource), isFallback = true)]
    public class MetaverseAudioSourceEditor : WrapperComponent<AudioSource, MetaverseAudioSource>
    {
        public override void OnInspectorGUI()
        {
            SerializedObject audioSourceSerializedObject = new SerializedObject((target as MetaverseAudioSource).AudioSource);
            serializedObject.Update();
            audioSourceSerializedObject.Update();

            MetaverseAudioSource.eAudioClipType sourceType = (MetaverseAudioSource.eAudioClipType) serializedObject.FindProperty("_audioClipType").intValue;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_audioClipType"));
            if (sourceType == MetaverseAudioSource.eAudioClipType.ASSET_REFERENCE)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_audioFile"));
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_bindOnLoad"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_doNotDestroyOnLoad"));
            audioSourceSerializedObject.FindProperty("m_PlayOnAwake").boolValue = false;
            EditorGUILayout.PropertyField(audioSourceSerializedObject.FindProperty("Mute"));
            EditorGUILayout.PropertyField(audioSourceSerializedObject.FindProperty("BypassEffects"));
            EditorGUILayout.PropertyField(audioSourceSerializedObject.FindProperty("BypassListenerEffects"));
            EditorGUILayout.PropertyField(audioSourceSerializedObject.FindProperty("BypassReverbZones"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_playOnAwake"));
            audioSourceSerializedObject.FindProperty("Loop").boolValue = true;
            int priority = audioSourceSerializedObject.FindProperty("Priority").intValue;
            audioSourceSerializedObject.FindProperty("Priority").intValue = EditorGUILayout.IntSlider(new GUIContent("Priority", "Higher value means higher priority"), priority, 0, 256);
            float volume = audioSourceSerializedObject.FindProperty("m_Volume").floatValue;
            audioSourceSerializedObject.FindProperty("m_Volume").floatValue = EditorGUILayout.Slider("Volume", volume, 0, 1);
            float pitch = audioSourceSerializedObject.FindProperty("m_Pitch").floatValue;
            audioSourceSerializedObject.FindProperty("m_Pitch").floatValue = EditorGUILayout.Slider("Pitch", pitch, -3, 3);

            float spatialBlend = audioSourceSerializedObject.FindProperty("panLevelCustomCurve").animationCurveValue[0].value;
            spatialBlend = EditorGUILayout.Slider(new GUIContent("Spatial Blend", "1 means 3D sound. 0 means 2D sound. Any between values are representing transition state."), spatialBlend, 0, 1);
            AnimationCurve spatialCurve = new AnimationCurve();
            spatialCurve.AddKey(0, spatialBlend);
            spatialCurve.AddKey(1, spatialBlend);
            audioSourceSerializedObject.FindProperty("panLevelCustomCurve").animationCurveValue = spatialCurve;

            EditorGUILayout.PropertyField(audioSourceSerializedObject.FindProperty("rolloffMode"), new GUIContent("Distance Based Fade Mode"));
            EditorGUILayout.PropertyField(audioSourceSerializedObject.FindProperty("MinDistance"));
            EditorGUILayout.PropertyField(audioSourceSerializedObject.FindProperty("MaxDistance"));
            
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Play Sound"))
                {
                    (target as MetaverseAudioSource).Play();
                }

                if (GUILayout.Button("Stop Sound"))
                {
                    (target as MetaverseAudioSource).Stop();
                }
            }
            
            if (audioSourceSerializedObject.hasModifiedProperties)
                audioSourceSerializedObject.ApplyModifiedProperties();
            if (serializedObject.hasModifiedProperties)
                serializedObject.ApplyModifiedProperties();
        }

        public override bool TryGetComponent<T>(out T component)
        {
            component = (target as MetaverseAudioSource).AudioSource as T;
            return component != null;
        }

        public override void SetComponent<T>(T component)
        {
            AudioSource source = component as AudioSource;
            source.playOnAwake = false;
            (target as MetaverseAudioSource).AudioSource = source;
        }
    }
}
#endif
