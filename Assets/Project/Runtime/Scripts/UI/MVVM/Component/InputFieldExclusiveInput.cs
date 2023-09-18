/*===============================================================
* Product:		Com2Verse
* File Name:	InputFieldExclusiveInput.cs
* Developer:	sprite
* Date:			2023-04-25 15:44
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.Project.InputSystem;

namespace Com2Verse.UI
{
    [AddComponentMenu("[DB]/[DB] InputFieldExclusiveInput")]
    [RequireComponent(typeof(InputFieldExtensions))]
    public sealed class InputFieldExclusiveInput : MonoBehaviour
	{
		private InputFieldExtensions inputFieldExtensions;

        public Data.eInputSystemState ExclusiveOn = Data.eInputSystemState.UI;

        private void Awake()
        {
            this.inputFieldExtensions = this.GetComponent<InputFieldExtensions>();

            this.inputFieldExtensions._onFocusChangedEvent.AddListener(this.OnFocusChanged);
        }

        private void OnFocusChanged(bool value)
        {
            var lastState = InputFieldExclusiveInput.GetCurrentInputSystemState();

            if (value)
            {
                InputSystemManagerHelper.ChangeState(this.ExclusiveOn);
            }
            else
            {
                InputSystemManager.InstanceOrNull?.ChangePreviousActionMap();
            }

            var newState = InputFieldExclusiveInput.GetCurrentInputSystemState();
            C2VDebug.Log($"[InputFieldExclusiveInput] Focus {(value ? "On" : "Off")} ({lastState} -> {newState})");
        }

        public static readonly Data.eInputSystemState INVALID_VALUE = (Data.eInputSystemState)(-1);

        public static Data.eInputSystemState GetCurrentInputSystemState()
        {
            var actionMap = InputSystemManager.InstanceOrNull?.ActionMap;
            var type = actionMap?.GetType();

            switch (type)
            {
                case var _ when type == typeof(ActionMapCharacterControl): return Data.eInputSystemState.CHARACTER_CONTROL;
                case var _ when type == typeof(ActionMapUIControl): return Data.eInputSystemState.UI;
                case var _ when type == typeof(ActionMapWebViewUIControl): return Data.eInputSystemState.WEB_VIEW_UI;
            }

            return INVALID_VALUE;
        }
    }
}
