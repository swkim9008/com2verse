/*===============================================================
* Product:		Com2Verse
* File Name:	RebindActionHelper.cs
* Developer:	mikeyid77
* Date:			2023-04-13 10:16
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.UI;

namespace Com2Verse.InputSystem
{
    public sealed class RebindActionHelper
    {
        public class ActionInfo
        {
            public string Name;
            public bool CanRebind;
        }

        public static Dictionary<string, ActionInfo> RebindActionDict => _rebindActionDict;
        private static Dictionary<string, ActionInfo> _rebindActionDict = new()
        {
            ["mouse"]      = new ActionInfo{ Name = RebindString.ActionMouse, CanRebind = false },
            ["up"]         = new ActionInfo{ Name = RebindString.ActionUp, CanRebind = true },
            ["down"]       = new ActionInfo{ Name = RebindString.ActionDown, CanRebind = true },
            ["left"]       = new ActionInfo{ Name = RebindString.ActionLeft, CanRebind = true },
            ["right"]      = new ActionInfo{ Name = RebindString.ActionRight, CanRebind = true },
            ["sprint"]     = new ActionInfo{ Name = RebindString.ActionSprint, CanRebind = true },
            ["jump"]       = new ActionInfo{ Name = RebindString.ActionJump, CanRebind = true },
            ["mypad"]      = new ActionInfo{ Name = RebindString.ActionMyPad, CanRebind = false },
            ["chat"]       = new ActionInfo{ Name = RebindString.ActionChat, CanRebind = false },
            ["screenshot"] = new ActionInfo{ Name = RebindString.ActionScreenShot, CanRebind = true },
        };
    }

    public static class RebindString
    {
        public static string ActionMouse => Localization.Instance.GetString("UI_Setting_KeyBinding_MouseMove");
        public static string ActionUp => Localization.Instance.GetString("UI_Setting_KeyBinding_MoveForward");
        public static string ActionDown => Localization.Instance.GetString("UI_Setting_KeyBinding_MoveBackward");
        public static string ActionLeft => Localization.Instance.GetString("UI_Setting_KeyBinding_MoveLeft");
        public static string ActionRight => Localization.Instance.GetString("UI_Setting_KeyBinding_MoveRight");
        public static string ActionSprint => Localization.Instance.GetString("UI_Setting_KeyBinding_Run");
        public static string ActionJump => Localization.Instance.GetString("UI_Setting_KeyBinding_Jump");
        public static string ActionMyPad => Localization.Instance.GetString("UI_Setting_KeyBinding_MyPad");
        public static string ActionChat => Localization.Instance.GetString("UI_Setting_KeyBinding_Chatting");
        public static string ActionScreenShot => Localization.Instance.GetString("UI_Setting_KeyBinding_Screenshot");
        public static string PopupCommonTitle => Localization.Instance.GetString("UI_Setting_Operate_KeyBinding_Popup_Title");
        public static string PopupCommonYes => Localization.Instance.GetString("UI_Common_Btn_Yes");
        public static string PopupCommonNo => Localization.Instance.GetString("UI_Common_Btn_No");
        public static string PopupCantBind => Localization.Instance.GetString("UI_Setting_Operate_KeyBinding_Popup_Msg");
        public static string PopupBindingContext(string name) => Localization.Instance.GetString("UI_Setting_Operate_KeyBinding_Change_Popup_Desc2", name);
        public static string PopupSaveContext => Localization.Instance.GetString("UI_Setting_Operate_KeyBinding_Cancel_Popup");
        public static string PopupNeedRebindContext => Localization.Instance.GetString("UI_Setting_Operate_KeyBinding_Apply_Popup");
    }
}