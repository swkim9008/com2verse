using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.PhysicsAssetSerialization;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Com2VerseEditor.PhysicsAssetSerialization
{
    [CustomEditor(typeof(C2VEventTrigger))]
    public class C2VEventTriggerEditor : Editor
    {
        private SerializedProperty _callback;
        private SerializedProperty _triggerGroupID;
        private SerializedProperty _type;
        private SerializedProperty _isTrigger;
        private SerializedProperty _colliderCenter;
        private SerializedProperty _radius;
        private SerializedProperty _height;
        private SerializedProperty _boxSize;
        private SerializedProperty _triggerId;

        private C2VEventTrigger _this;

        private static class Styles
        {
            public static readonly GUIContent Callback = EditorGUIUtility.TrTextContent("Callback Function", "Callback functions and parameters");
            public static readonly GUIContent CallbackFunction = EditorGUIUtility.TrTextContent("Function", "Callback function");
            public static readonly GUIContent CallbackTag = EditorGUIUtility.TrTextContent("Tag", "Trigger parameters");
            public static readonly GUIContent HasGroupId = EditorGUIUtility.TrTextContent("Has Trigger Group", "Whether using trigger group");
            public static readonly GUIContent TriggerGroupId = EditorGUIUtility.TrTextContent("Trigger Group ID", "Trigger with same group ID will be regarded as a whole collider. 0 means no group.");
            public static readonly GUIContent Type = EditorGUIUtility.TrTextContent("Shape", "Shape of the collider");
            public static readonly GUIContent ColliderCenter = EditorGUIUtility.TrTextContent("Center", "Center of the collider");
            public static readonly GUIContent Radius = EditorGUIUtility.TrTextContent("Radius", "Radius of the sphere");
            public static readonly GUIContent Height = EditorGUIUtility.TrTextContent("Height", "Total height of the capsule");
            public static readonly GUIContent BoxSize = EditorGUIUtility.TrTextContent("Size", "Size of the box");
            public static readonly GUIContent ServerCheck = EditorGUIUtility.TrTextContent("Server Check", "Trigger collision server check type");
            public static readonly GUIContent ActionType = EditorGUIUtility.TrTextContent("Action Type", "Trigger Event Action Type");
            public static readonly GUIContent TriggerId = EditorGUIUtility.TrTextContent("Trigger Id", "Trigger id (unique in object scope)");
        }

        private static Dictionary<eLogicType, Interaction> _interaction;
        private static List<eLogicType> _triggerInteraction;

        public void OnEnable()
        {
            _this = (C2VEventTrigger)target;
            _callback = serializedObject.FindProperty("Callback");
            _triggerGroupID = serializedObject.FindProperty("TriggerGroupId");
            _type = serializedObject.FindProperty("Type");
            _colliderCenter = serializedObject.FindProperty("ColliderCenter");
            _radius = serializedObject.FindProperty("Radius");
            _height = serializedObject.FindProperty("Height");
            _boxSize = serializedObject.FindProperty("BoxSize");
            _triggerId = serializedObject.FindProperty("TriggerId");

            LoadData().Forget();
        }
        
        private static async UniTask LoadData()
        {
            if (_interaction == null)
            {
                var interactionData = await Addressables.LoadAssetAsync<TextAsset>("Interaction.bytes").Task;
                _interaction = TableInteraction.Load(interactionData?.bytes)?.Datas ?? new Dictionary<eLogicType, Interaction>();
            }

            if (_triggerInteraction == null)
            {
                _triggerInteraction = new List<eLogicType>();
                foreach (var interaction in _interaction.Values)
                {
                    if (interaction.LinkTarget == eInteractionLinkTarget.ALL || interaction.LinkTarget == eInteractionLinkTarget.TRIGGER)
                    {
                        _triggerInteraction.Add(interaction.ID);
                    }
                }
            }
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_type, Styles.Type);
            _this.Type = (Com2Verse.PhysicsAssetSerialization.eColliderType)_type.enumValueIndex;
            
            switch ((Com2Verse.PhysicsAssetSerialization.eColliderType)_type.enumValueIndex)
            {
                case Com2Verse.PhysicsAssetSerialization.eColliderType.NONE:
                    return;
                case Com2Verse.PhysicsAssetSerialization.eColliderType.SPHERE:
                    EditorGUILayout.PropertyField(_colliderCenter, Styles.ColliderCenter);
                    EditorGUILayout.PropertyField(_radius, Styles.Radius);
                    break;
                case Com2Verse.PhysicsAssetSerialization.eColliderType.BOX:
                    EditorGUILayout.PropertyField(_colliderCenter, Styles.ColliderCenter);
                    EditorGUILayout.PropertyField(_boxSize, Styles.BoxSize);
                    break;
                case Com2Verse.PhysicsAssetSerialization.eColliderType.CAPSULE:
                    EditorGUILayout.PropertyField(_colliderCenter, Styles.ColliderCenter);
                    EditorGUILayout.PropertyField(_radius, Styles.Radius);
                    EditorGUILayout.PropertyField(_height, Styles.Height);
                    if (_height.floatValue < _radius.floatValue * 2)
                    {
                        EditorGUILayout.HelpBox("Height mush be greater than radius * 2.", MessageType.Warning);
                    }
                    break;
                case Com2Verse.PhysicsAssetSerialization.eColliderType.MESH:
                    EditorGUILayout.HelpBox("Mesh 트리거는 현재 지원되지 않습니다.", MessageType.Error);
                    break;
            }

            bool hasGroupId = _this.TriggerGroupId != 0;
            bool groupIdToggle = EditorGUILayout.Toggle(Styles.HasGroupId, hasGroupId);
            if (groupIdToggle != hasGroupId)
            {
                if (groupIdToggle)
                {
                    _this.TriggerGroupId = -1;
                }
                else
                {
                    _this.TriggerGroupId = 0;
                }
            }

            if (hasGroupId)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_triggerGroupID, Styles.TriggerGroupId);
                EditorGUI.indentLevel--;
            }

            var legacyCallbackProperty = serializedObject.FindProperty("callbacks");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(legacyCallbackProperty, new GUIContent("LegacyCallback"));
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("legacy callback 변환"))
            {
                _callback.arraySize = 1;
                var currentProperty = _callback.GetArrayElementAtIndex(0);

                var callbackFunction = currentProperty.FindPropertyRelative("Function");
                var callbackTag = currentProperty.FindPropertyRelative("TagInternal");
                var serverCheck = currentProperty.FindPropertyRelative("ServerCheck");
                var newTags = legacyCallbackProperty.FindPropertyRelative("TagInternal");

                callbackFunction.longValue = legacyCallbackProperty.FindPropertyRelative("Function").longValue - 1;
                serverCheck.intValue = legacyCallbackProperty.FindPropertyRelative("ServerCheck").intValue;

                newTags.arraySize = callbackTag.arraySize;
                for (int i = 0; i < callbackTag.arraySize; i++)
                {
                    var newElement = newTags.GetArrayElementAtIndex(i);
                    var oldElement = callbackTag.GetArrayElementAtIndex(i);

                    newElement.FindPropertyRelative("Key").stringValue = oldElement.FindPropertyRelative("Key").stringValue;
                    newElement.FindPropertyRelative("Value").stringValue = oldElement.FindPropertyRelative("Value").stringValue;
                }
            }
            
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
                        var callbackFunction = currentProperty.FindPropertyRelative("Function");
                        var callbackTag = currentProperty.FindPropertyRelative("TagInternal");
                        var serverCheck = currentProperty.FindPropertyRelative("ServerCheck");

                        currentProperty.isExpanded = EditorGUILayout.Foldout(currentProperty.isExpanded, new GUIContent($"Callback {index + 1}"));
                        if (currentProperty.isExpanded)
                        {
                            using (new EditorGUI.IndentLevelScope())
                            {
                                eLogicType currentLogic = (eLogicType)callbackFunction.intValue;
                                callbackFunction.intValue = (int)(eLogicType)EditorGUILayout.EnumPopup(Styles.CallbackFunction, currentLogic, (interaction) => _triggerInteraction == null || _triggerInteraction.Contains((eLogicType)interaction), false);
                                EditorGUILayout.PropertyField(callbackTag, Styles.CallbackTag);

                                if (_interaction != null && _interaction.TryGetValue(currentLogic, out var interaction))
                                {
                                    EditorGUI.BeginDisabledGroup(true);
                                    serverCheck.intValue = (int)(interaction.TriggerValidationType);
                                    EditorGUILayout.EnumPopup(Styles.ActionType, interaction.ActionType);
                                    EditorGUILayout.PropertyField(serverCheck, Styles.ServerCheck);
                                    EditorGUI.EndDisabledGroup();
                                }
                                else
                                {
                                    EditorGUILayout.PropertyField(serverCheck, Styles.ServerCheck);
                                }

                                if (serverCheck.intValue != (int)eServerCheck.ONLY_CLIENT)
                                {
                                    if (_this.GetComponentInParent<ServerObject>().IsReferenceNull())
                                    {
                                        EditorGUILayout.HelpBox("서버 검증이 필요한 트리거는 오브젝트 루트에 ServerObject 컴포넌트가 있어야 합니다.", MessageType.Error);
                                    }
                                }

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

            EditorGUILayout.PropertyField(_triggerId, Styles.TriggerId);
            EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
