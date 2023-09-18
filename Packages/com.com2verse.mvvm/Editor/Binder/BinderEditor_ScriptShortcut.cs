/*===============================================================
* Product:		Com2Verse
* File Name:	BinderEditor_ScriptShortcut.cs
* Developer:	sprite
* Date:			2023-04-07 12:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Logger;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using GUIButtonRectMap = System.Collections.Generic.Dictionary<string, (UnityEngine.Rect rect, System.WeakReference wr)>;

namespace Com2VerseEditor.UI
{
    public partial class BinderEditor
    {
        private const int BUTTON_SIZE = 18;
        private static GUIContent guiContentScriptShortcut = null;
        private static GUIStyle guiStyleButton = null;

        protected bool ScriptShortcut(SerializedProperty findKeyProperty)
        {
            if (guiContentScriptShortcut == null)
            {
                guiContentScriptShortcut = new GUIContent(EditorGUIUtility.IconContent("P4_Conflicted"));
            }

            if (guiStyleButton == null)
            {
                guiStyleButton = new GUIStyle(EditorStyles.iconButton);
                guiStyleButton.border = GUI.skin.button.border;
                guiStyleButton.margin = GUI.skin.button.margin;
            }

            Rect rcDropDown = Rect.zero;

            var key = findKeyProperty.stringValue;
            var clicked = GUILayout.Button(guiContentScriptShortcut, guiStyleButton, GUILayout.Width(BUTTON_SIZE));

            GUIButtonRectManager.AddOrUpdate(findKeyProperty.stringValue, findKeyProperty);

            if (!clicked) return false;

            var items = findKeyProperty.stringValue.Split('/');
            var viewModelName = items[0];
            var memberName = items.Length > 1 ? items[1] : string.Empty;
            var type = ViewModelTypes.FirstOrDefault(e => string.Equals(e.Name, viewModelName));

            var assetGuids = AssetDatabase.FindAssets($"{viewModelName} t:Script");
            var scripts = assetGuids
                .Select(e => AssetDatabase.GUIDToAssetPath(e))
                .Where
                (
                    e =>
                    {
                        var fileName = System.IO.Path.GetFileNameWithoutExtension(e);
                        return string.Equals(fileName, viewModelName) || (fileName.Contains(viewModelName) && fileName.IndexOf('_') >= 0);
                    }
                )
                .Select(e => (assetPath: e, textAsset: AssetDatabase.LoadAssetAtPath(e, typeof(TextAsset)) as TextAsset))
                .ToList();

            GenericMenu scriptMenu = new GenericMenu();

            if (string.IsNullOrEmpty(memberName))
            {
                for (int i = 0, cnt = scripts.Count; i < cnt; i++)
                {
                    var info = scripts[i];
                    scriptMenu
                        .AddItem
                        (
                            new GUIContent($"{System.IO.Path.GetFileName(info.assetPath)}"),
                            false,
                            () => InternalEditorUtility.OpenFileAtLineExternal(info.assetPath, 0)
                        );
                }
            }
            else
            {
                for (int i = 0, cnt = scripts.Count; i < cnt; i++)
                {
                    var info = scripts[i];
                    var lines = info.textAsset.text.Split("\r\n");
                    for (int j = 0, cntj = lines.Length; j < cntj; j++)
                    {
                        if (lines[j].Contains(memberName))
                        {
                            var lineNo = j + 1;

                            scriptMenu
                                .AddItem
                                (
                                    new GUIContent($"{System.IO.Path.GetFileName(info.assetPath)}:{lineNo}"),
                                    false,
                                    () => InternalEditorUtility.OpenFileAtLineExternal(info.assetPath, lineNo)
                                );
                            break;
                        }
                    }
                }
            }

            scriptMenu.DropDown(GUIButtonRectManager.GetRect(key));
            EditorGUIUtility.ExitGUI();

            return true;
        }
    }

    internal static class GUIButtonRectManager
    {
        private const int CAPACITY = 10;
        private const int DEFAULT_INTERVAL_FRAMES = 60;

        private static GUIButtonRectMap infoMap = null;

        public static void AddOrUpdate(string key, object forLifetimeCheck, int intervalFramesForShrink = DEFAULT_INTERVAL_FRAMES)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint) return;

            if (GUIButtonRectManager.infoMap == null) GUIButtonRectManager.infoMap = new GUIButtonRectMap(CAPACITY);

            if (!GUIButtonRectManager.infoMap.TryGetValue(key, out var info))
            {
                info.rect = GUILayoutUtility.GetLastRect();
                info.wr = new WeakReference(forLifetimeCheck);
                GUIButtonRectManager.infoMap.Add(key, info);
            }
            else
            {
                info.rect = GUILayoutUtility.GetLastRect();
                GUIButtonRectManager.infoMap[key] = info;
            }

            if ((Time.frameCount % intervalFramesForShrink) != 0)
            {
                GUIButtonRectManager.Shrink();
            }
        }

        private static void Shrink()
        {
            int removeCount = 0;

            var keys = GUIButtonRectManager.infoMap.Keys.ToList();
            for (int i = 0, cnt = keys.Count; i < cnt; i++)
            {
                if (!GUIButtonRectManager.infoMap.TryGetValue(keys[i], out var item)) continue;

                if (!item.wr.IsAlive)
                {
                    GUIButtonRectManager.infoMap.Remove(keys[i]);
                    removeCount++;
                }
            }

            if (removeCount > 0)
            {
                C2VDebug.Log($"*** Removing {removeCount} items from [Com2VerseEditor.UI.GUIButtonRectManager]");
            }
        }

        public static Rect GetRect(string key)
        {
            if (GUIButtonRectManager.infoMap.TryGetValue(key, out var info)) return info.rect;

            return Rect.zero;
        }
    }
}
