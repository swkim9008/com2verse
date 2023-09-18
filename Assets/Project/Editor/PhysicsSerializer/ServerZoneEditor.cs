// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	ServerZoneEditor.cs
//  * Developer:	yangsehoon
//  * Date:		2023-06-12 오후 2:06
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse;
using Com2Verse.Data;
using Com2Verse.Extension;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Com2VerseEditor.PhysicsAssetSerialization
{
    [CustomEditor(typeof(ServerZone))] 
    public class ServerZoneEditor : Editor 
    {
        private SerializedProperty _zoneName;
        private SerializedProperty _spaceZoneId;
        private SerializedProperty _callback;
        private static long _spaceZoneIdCounter = 1;

        private static List<eLogicType> _triggerInteraction;
        
        private static class Styles
        {
            public static readonly GUIContent Callback = EditorGUIUtility.TrTextContent("Callback Function", "Callback functions and parameters");
            public static readonly GUIContent ZoneName = EditorGUIUtility.TrTextContent("Zone Name", "Name of Zone (debug)");
            public static readonly GUIContent SpaceZoneId = EditorGUIUtility.TrTextContent("Space Zone ID", "Space Zone ID");
            public static readonly GUIContent CallbackFunction = EditorGUIUtility.TrTextContent("Function", "Callback function");
            public static readonly GUIContent CallbackTag = EditorGUIUtility.TrTextContent("Interaction Parameter", "Trigger parameters");
        }
        
        public void OnEnable()
        {
            _callback = serializedObject.FindProperty("Callback");
            _zoneName = serializedObject.FindProperty("ZoneName");
            _spaceZoneId = serializedObject.FindProperty("SpaceZoneId");

            LoadData().Forget();

            serializedObject.Update();
            for (int sceneIndex = 0; sceneIndex < EditorSceneManager.sceneCount; sceneIndex++)
            {
                Scene currentScene = EditorSceneManager.GetSceneAt(sceneIndex);
                var roots = currentScene.GetRootGameObjects();
                foreach (var rootObject in roots)
                {
                    var zoneObject = rootObject.GetComponent<ServerZone>();
                    if (zoneObject.IsReferenceNull()) continue;
                    _spaceZoneIdCounter = Math.Max(_spaceZoneIdCounter, zoneObject.SpaceZoneId + 1);
                    if (!ReferenceEquals(zoneObject, target) && zoneObject.SpaceZoneId == _spaceZoneId.longValue) _spaceZoneId.longValue = 0;
                }
            }

            if (PrefabStageUtility.GetCurrentPrefabStage() == null)
            {
                if (_spaceZoneId.longValue == 0)
                {
                    _spaceZoneId.longValue = _spaceZoneIdCounter++;
                }
            }
            else
            {
                _spaceZoneId.longValue = 0;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnDestroy()
        {
            _spaceZoneIdCounter = 1;
        }
        
        private static async UniTask LoadData()
        {
            if (_triggerInteraction == null)
            {
                var interactionData = await Addressables.LoadAssetAsync<TextAsset>("Interaction.bytes").Task;
                var interactionTable = TableInteraction.Load(interactionData?.bytes)?.Datas ?? new Dictionary<eLogicType, Interaction>();
                
                _triggerInteraction = new List<eLogicType>();
                foreach (var interaction in interactionTable.Values)
                {
                    if (interaction.LinkTarget == eInteractionLinkTarget.ALL || interaction.LinkTarget == eInteractionLinkTarget.ZONE)
                    {
                        _triggerInteraction.Add(interaction.ID);
                    }
                }
            }
        }

        private bool CheckValidZoneInteraction(Enum value)
        {
            return _triggerInteraction == null || _triggerInteraction.Contains((eLogicType)value);
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(_zoneName, Styles.ZoneName);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_spaceZoneId, Styles.SpaceZoneId);
            EditorGUI.EndDisabledGroup();
            
            _callback.isExpanded = EditorGUILayout.Foldout(_callback.isExpanded, Styles.Callback);
            if (_callback.isExpanded)
            {
                if (_callback.arraySize == 0)
                {
                    EditorGUILayout.HelpBox("No callback registered.", MessageType.Info);
                }
                
                using (new EditorGUI.IndentLevelScope())
                {
                    for (int index = 0; index < _callback.arraySize; ++index)
                    {
                        var currentProperty = _callback.GetArrayElementAtIndex(index);
                        var callbackFunction = currentProperty.FindPropertyRelative("LogicType");
                        var callbackTag = currentProperty.FindPropertyRelative("InteractionValue");

                        currentProperty.isExpanded = EditorGUILayout.Foldout(currentProperty.isExpanded, new GUIContent($"Callback {index + 1}"));
                        if (currentProperty.isExpanded)
                        {
                            using (new EditorGUI.IndentLevelScope())
                            {
                                eLogicType currentLogic = (eLogicType)callbackFunction.intValue;
                                if (!CheckValidZoneInteraction(currentLogic))
                                {
                                    currentLogic = (eLogicType)(-1);
                                }
                                callbackFunction.intValue = (int)(eLogicType)EditorGUILayout.EnumPopup(Styles.CallbackFunction, currentLogic, CheckValidZoneInteraction, false);
                                EditorGUILayout.PropertyField(callbackTag, Styles.CallbackTag);

                                GUIStyle style = new GUIStyle(GUI.skin.button);
                                style.margin = new RectOffset(style.margin.left + EditorGUI.indentLevel * 22, style.margin.right, style.margin.top, style.margin.bottom);
                                if (GUILayout.Button("Remove This Callback", style))
                                {
                                    _callback.DeleteArrayElementAtIndex(index);
                                }
                            }
                        }
                    }
                }
            }

            if (GUILayout.Button("Add Callback"))
            {
                _callback.InsertArrayElementAtIndex(_callback.arraySize);
            }
            
            if (serializedObject.hasModifiedProperties)
                serializedObject.ApplyModifiedProperties();
        }
    }
}
