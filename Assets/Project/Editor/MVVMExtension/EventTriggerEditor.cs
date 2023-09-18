/*===============================================================
* Product:		Com2Verse
* File Name:	EventListenerEditor.cs
* Developer:	tlghks1009
* Date:			2022-05-16 12:55
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Reflection;
using Com2Verse.UI;
using UnityEditor;
using CommandHandler = Com2Verse.UI.CommandHandler;

namespace Com2VerseEditor.UI
{
    [CustomEditor(typeof(EventTrigger))]
    public class EventTriggerEditor : BinderEditor
    {
        private EventTrigger _eventTrigger;

        protected override void OnEnable()
        {
            base.OnEnable();

            _eventTrigger = target as EventTrigger;
        }

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspectorWithoutScriptField();

            serializedObject.Update();

            var targetTriggerType = (EventTrigger.eTargetTriggerType) serializedObject.FindProperty("_targetTriggerType").intValue;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetTriggerType"));

            serializedObject.ApplyModifiedProperties();

            if (targetTriggerType == EventTrigger.eTargetTriggerType.UNITY_COMPONENT)
                DrawUnityComponent();

            if (targetTriggerType == EventTrigger.eTargetTriggerType.VIEW_MODEL)
                DrawViewModel();

            DrawTriggerConditionType();


            serializedObject.Update();

            var eventType = (EventTrigger.eEventType) serializedObject.FindProperty("_eventType").intValue;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_eventType"));

            serializedObject.ApplyModifiedProperties();

            if (eventType == EventTrigger.eEventType.VIEW_MODEL_HANDLER)
                DrawViewModelEventType();

            if (eventType == EventTrigger.eEventType.UNITY_EVENT)
                DrawUnityEventType();
        }


        private void DrawViewModel()
        {
            serializedObject.Update();

            var propertyInfos = Finder.GetBindableProperties(ViewModelTypes, (property) =>
                                                                 property.GetGetMethod(false) != null &&
                                                                 property.GetCustomAttribute(typeof(ObsoleteAttribute), true) == null);

            ShowMemberTypeMenu(_eventTrigger, "Target Property", propertyInfos, GetBindingFullPath(serializedObject, "_sourcePath"), (updateOwner, updateProperty) =>
            {
                serializedObject.FindProperty("_sourcePath.propertyOwner").stringValue = updateOwner;
                serializedObject.FindProperty("_sourcePath.property").stringValue = updateProperty;
            });
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawUnityComponent()
        {
            serializedObject.Update();

            {
                var propertyInfos = Finder.GetBindableProperties(
                    _eventTrigger.gameObject,
                    (property) =>
                        property.GetGetMethod(false) != null &&
                        property.GetCustomAttribute(typeof(ObsoleteAttribute), true) == null);


                var selectedPropertyInfo = ShowMemberTypeMenu
                (
                    _eventTrigger,
                    "Target Property",
                    propertyInfos,
                    GetBindingFullPath(serializedObject, "_targetPath"),
                    (updateOwner, updateProperty) =>
                    {
                        serializedObject.FindProperty("_targetPath.propertyOwner").stringValue = updateOwner;
                        serializedObject.FindProperty("_targetPath.property").stringValue = updateProperty;
                    }
                );

                if (selectedPropertyInfo != null)
                {
                    var propertyOwnerName = serializedObject.FindProperty("_targetPath.propertyOwner").stringValue;
                    UpdateTargetComponent(_eventTrigger, propertyOwnerName, (component) => { serializedObject.FindProperty("_targetPath.component").objectReferenceValue = component; });

                    EditorGUI.indentLevel += 1;
                    var fieldInfos = Finder.GetUnityEvents(selectedPropertyInfo.RootType);
                    var selectedFieldInfo = ShowMemberTypeMenu
                    (
                        _eventTrigger,
                        "Event",
                        fieldInfos,
                        ObjectNames.NicifyVariableName(serializedObject.FindProperty("_eventPath.property").stringValue),
                        (updateOwner, updateProperty) =>
                        {
                            serializedObject.FindProperty("_eventPath.propertyOwner").stringValue = updateOwner;
                            serializedObject.FindProperty("_eventPath.property").stringValue = updateProperty;
                        }
                    );
                    EditorGUI.indentLevel -= 1;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTriggerConditionType()
        {
            serializedObject.Update();

            var eventTriggerConditionType = (EventTrigger.eEventTriggerConditionType) serializedObject.FindProperty("_eventTriggerConditionType").intValue;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_eventTriggerConditionType"));
            EditorGUI.indentLevel += 1;
            if (eventTriggerConditionType == EventTrigger.eEventTriggerConditionType.BOOL)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_triggerBoolValue"));

            if (eventTriggerConditionType == EventTrigger.eEventTriggerConditionType.STRING)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_triggerStringValue"));

            if (eventTriggerConditionType == EventTrigger.eEventTriggerConditionType.INT)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_triggerIntValue"));
            EditorGUI.indentLevel -= 1;

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawUnityEventType()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_onEventTriggerEvent"));
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawViewModelEventType()
        {
            serializedObject.Update();

            var propertyInfos = Finder.GetBindableProperties(ViewModelTypes, (property) =>
                                                                 property.GetGetMethod(false) != null &&
                                                                 property.GetCustomAttribute(typeof(ObsoleteAttribute), true) == null &&
                                                                 (property.PropertyType == typeof(CommandHandler) ||
                                                                  property.PropertyType == typeof(CommandHandler<int>) ||
                                                                  property.PropertyType == typeof(CommandHandler<string>) ||
                                                                  property.PropertyType == typeof(CommandHandler<bool>)));

            ShowMemberTypeMenu(_eventTrigger, "Target Property", propertyInfos, GetBindingFullPath(serializedObject, "_viewModelProperty"), (updateOwner, updateProperty) =>
            {
                serializedObject.FindProperty("_viewModelProperty.propertyOwner").stringValue = updateOwner;
                serializedObject.FindProperty("_viewModelProperty.property").stringValue = updateProperty;
            });
            serializedObject.ApplyModifiedProperties();

            DrawAdditionalData();
        }


        private void DrawAdditionalData()
        {
            // TODO : AdditionalData

            serializedObject.Update();

            var additionalDataType = (CommandBinder.eBindingParameterType) serializedObject.FindProperty("_bindingParameter.bindingParameterType").intValue;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_bindingParameter.bindingParameterType"));

            //serializedObject.ApplyModifiedProperties();

            switch (additionalDataType)
            {
                case CommandBinder.eBindingParameterType.BOOLEAN:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_bindingParameter.boolValue"));
                    break;
                case CommandBinder.eBindingParameterType.INT:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_bindingParameter.intValue"));
                    break;
                case CommandBinder.eBindingParameterType.FLOAT:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_bindingParameter.floatValue"));
                    break;
                case CommandBinder.eBindingParameterType.STRING:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_bindingParameter.stringValue"));
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
