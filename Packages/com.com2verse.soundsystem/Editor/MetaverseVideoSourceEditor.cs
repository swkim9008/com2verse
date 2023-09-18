/*===============================================================
* Product:    Com2Verse
* File Name:  MetaverseVideoSourceEditor.cs
* Developer:  yangsehoon
* Date:       2022-04-11 10:45
* History:    
* Documents:  MetaverseVideoSource custom editor. (Hides origin VideoPlayer, destroy origin VideoPlayer when this destroyed)
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#if UNITY_EDITOR
using System.Collections.Generic;
using Com2Verse.Sound;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Video;

namespace Com2VerseEditor.Sound
{
    [CustomEditor(typeof(MetaverseVideoSource), isFallback = true)]
    public class MetaverseVideoSourceEditor : WrapperComponent<VideoPlayer, AudioSource, MetaverseVideoSource>
    {
        private List<string> _properties = new List<string>();
        
        public override void OnInspectorGUI()
        {
            SerializedObject videoPlayerSerializedObject = new SerializedObject((target as MetaverseVideoSource).VideoPlayer);
            SerializedObject audioSourceSerializedObject = new SerializedObject((target as MetaverseVideoSource).AudioSource);
            videoPlayerSerializedObject.Update();
            audioSourceSerializedObject.Update();
            serializedObject.Update();

            videoPlayerSerializedObject.FindProperty("m_PlayOnAwake").boolValue = false;
            audioSourceSerializedObject.FindProperty("m_PlayOnAwake").boolValue = false;

            VideoSource videoSource = (VideoSource) videoPlayerSerializedObject.FindProperty("m_DataSource").intValue;
            EditorGUILayout.PropertyField(videoPlayerSerializedObject.FindProperty("m_DataSource"));
            if (videoSource == VideoSource.Url)
            {
                EditorGUILayout.PropertyField(videoPlayerSerializedObject.FindProperty("m_Url"));
            }
            else if (videoSource == VideoSource.VideoClip)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_mediaFile"));
            }
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_bindOnLoad"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_playOnAwake"));
            EditorGUILayout.PropertyField(videoPlayerSerializedObject.FindProperty("m_Looping"), new GUIContent("Loop"));
            EditorGUILayout.PropertyField(videoPlayerSerializedObject.FindProperty("m_SkipOnDrop"));
            float volume = audioSourceSerializedObject.FindProperty("m_Volume").floatValue;
            audioSourceSerializedObject.FindProperty("m_Volume").floatValue = EditorGUILayout.Slider("Volume", volume, 0, 1);
            EditorGUILayout.PropertyField(audioSourceSerializedObject.FindProperty("Mute"));
            EditorGUILayout.PropertyField(audioSourceSerializedObject.FindProperty("BypassEffects"));
            EditorGUILayout.PropertyField(audioSourceSerializedObject.FindProperty("BypassListenerEffects"));
            EditorGUILayout.PropertyField(audioSourceSerializedObject.FindProperty("BypassReverbZones"));

            float spatialBlend = audioSourceSerializedObject.FindProperty("panLevelCustomCurve").animationCurveValue[0].value;
            spatialBlend = EditorGUILayout.Slider(new GUIContent("Spatial Blend", "1 means 3D sound. 0 means 2D sound. Any between values are representing transition state."), spatialBlend, 0, 1);
            AnimationCurve spatialCurve = new AnimationCurve();
            spatialCurve.AddKey(0, spatialBlend);
            spatialCurve.AddKey(1, spatialBlend);
            audioSourceSerializedObject.FindProperty("panLevelCustomCurve").animationCurveValue = spatialCurve;
            
            float playbackSpeed = videoPlayerSerializedObject.FindProperty("m_PlaybackSpeed").floatValue;
            videoPlayerSerializedObject.FindProperty("m_PlaybackSpeed").floatValue = EditorGUILayout.Slider("Playback Speed", playbackSpeed, 0, 10);
            
            VideoRenderMode renderMode = (VideoRenderMode) videoPlayerSerializedObject.FindProperty("m_RenderMode").intValue;
            EditorGUILayout.PropertyField(videoPlayerSerializedObject.FindProperty("m_RenderMode"));
            switch (renderMode)
            {
                case VideoRenderMode.RenderTexture:
                    if (Application.isPlaying)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                    }
                    bool createRenderTextureAtRuntime = serializedObject.FindProperty("_createRenderTextureAtRuntime").boolValue;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_createRenderTextureAtRuntime"));
                    if (createRenderTextureAtRuntime)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_RenderTextureSize"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_renderTextureFormat"));
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.PropertyField(videoPlayerSerializedObject.FindProperty("m_TargetTexture"));
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(videoPlayerSerializedObject.FindProperty("m_TargetTexture"));
                    }

                    if (Application.isPlaying)
                    {
                        EditorGUI.EndDisabledGroup();
                    }
                    break;
                case VideoRenderMode.CameraFarPlane:
                case VideoRenderMode.CameraNearPlane:
                    EditorGUILayout.PropertyField(videoPlayerSerializedObject.FindProperty("m_TargetCamera"), new GUIContent("Render Camera"));
                    float alpha = videoPlayerSerializedObject.FindProperty("m_TargetCameraAlpha").floatValue;
                    videoPlayerSerializedObject.FindProperty("m_TargetCameraAlpha").floatValue = EditorGUILayout.Slider("Alpha", alpha, 0, 1);
                    EditorGUILayout.PropertyField(videoPlayerSerializedObject.FindProperty("m_TargetCamera3DLayout"));
                    break;
                case VideoRenderMode.MaterialOverride:
                    SerializedProperty materialRendererProperty = videoPlayerSerializedObject.FindProperty("m_TargetMaterialRenderer");
                    SerializedProperty materialProperty = videoPlayerSerializedObject.FindProperty("m_TargetMaterialProperty");
                    EditorGUILayout.PropertyField(materialRendererProperty, new GUIContent("Renderer"));
                    Renderer renderer = (Renderer)materialRendererProperty.objectReferenceValue;
                    EditorGUI.BeginDisabledGroup(renderer == null);
                    int index = -1;
                    if (renderer != null)
                    {
                        foreach (Material mat in renderer.sharedMaterials)
                        {
                            if (mat != null)
                            {
                                for (int i = 0, e = mat.shader.GetPropertyCount(); i < e; ++i)
                                {
                                    if (mat.shader.GetPropertyType(i) == ShaderPropertyType.Texture)
                                    {
                                        string propertyName = mat.shader.GetPropertyName(i);
                                        if (!_properties.Contains(propertyName))
                                            _properties.Add(propertyName);
                                    }
                                }
                                
                                index = _properties.IndexOf(materialProperty.stringValue);
                            }
                        }

                        int propertyIndex = EditorGUILayout.Popup("Material Property", index, _properties.ToArray());
                        if (propertyIndex >= 0)
                        {
                            materialProperty.stringValue = _properties[propertyIndex];
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                    break;
                default:
                    EditorGUILayout.HelpBox("Not Implemented Yet", MessageType.Error);
                    break;

            }
            EditorGUILayout.PropertyField(videoPlayerSerializedObject.FindProperty("m_AspectRatio"));

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Play Video"))
                {
                    (target as MetaverseVideoSource).Play();
                }
            }

            if (videoPlayerSerializedObject.hasModifiedProperties)
                videoPlayerSerializedObject.ApplyModifiedProperties();
            if (audioSourceSerializedObject.hasModifiedProperties)
                audioSourceSerializedObject.ApplyModifiedProperties();
            if (serializedObject.hasModifiedProperties)
                serializedObject.ApplyModifiedProperties();
        }
        
        public override bool TryGetComponent<T>(out T component)
        {
            component = (target as MetaverseVideoSource).AudioSource as T;
            if (component == null)
                component = (target as MetaverseVideoSource).VideoPlayer as T;
            return component != null;
        }

        public override void SetComponent<T>(T component)
        {
            AudioSource audioComponent = component as AudioSource;
            if (audioComponent != null)
                (target as MetaverseVideoSource).AudioSource = audioComponent;
            else
            {
                VideoPlayer videoComponent = component as VideoPlayer;
                (target as MetaverseVideoSource).VideoPlayer = videoComponent;
            }
        }
    }
}
#endif
