/*===============================================================
* Product:		Com2Verse
* File Name:	DefineDataBinding.cs
* Developer:	tlghks1009
* Date:			2022-11-24 12:15
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Com2Verse.UI
{
    public class MVVMDefine
    {
        public enum eUnityEventType
        {
            BUTTON_CLICK_EVENT,
            VALUE_CHANGED_EVENT_BOOLEAN,
            VALUE_CHANGED_EVENT_STRING,
            VALUE_CHANGED_EVENT_FLOAT,
            VALUE_CHANGED_EVENT_INT,
            DROP_DOWN_EVENT,
            TRANSFORM_EVENT,
        }

        private static readonly Dictionary<Type, eUnityEventType> _unityEventTypeDict = new()
        {
            [typeof(Button.ButtonClickedEvent)] = eUnityEventType.BUTTON_CLICK_EVENT,
            [typeof(TMP_InputField.OnChangeEvent)] = eUnityEventType.VALUE_CHANGED_EVENT_STRING,
            [typeof(TMP_InputField.SubmitEvent)] = eUnityEventType.VALUE_CHANGED_EVENT_STRING,
            [typeof(TMP_InputField.SelectionEvent)] = eUnityEventType.VALUE_CHANGED_EVENT_STRING,
            [typeof(TMP_Dropdown.DropdownEvent)] = eUnityEventType.DROP_DOWN_EVENT,
            [typeof(Toggle.ToggleEvent)] = eUnityEventType.VALUE_CHANGED_EVENT_BOOLEAN,
            [typeof(Slider.SliderEvent)] = eUnityEventType.VALUE_CHANGED_EVENT_FLOAT,
            [typeof(UnityEvent)] = eUnityEventType.BUTTON_CLICK_EVENT,
            [typeof(UnityEvent<string>)] = eUnityEventType.VALUE_CHANGED_EVENT_STRING,
            [typeof(UnityEvent<bool>)] = eUnityEventType.VALUE_CHANGED_EVENT_BOOLEAN,
            [typeof(UnityEvent<float>)] = eUnityEventType.VALUE_CHANGED_EVENT_FLOAT,
            [typeof(UnityEvent<int>)] = eUnityEventType.VALUE_CHANGED_EVENT_INT,
            [typeof(UnityEvent<Transform>)] = eUnityEventType.TRANSFORM_EVENT,
            [typeof(Scrollbar.ScrollEvent)] = eUnityEventType.VALUE_CHANGED_EVENT_FLOAT,
        };

        public static bool TryGetUnityEventType(Type type, out eUnityEventType unityEventType) => _unityEventTypeDict.TryGetValue(type, out unityEventType);
    }
}
