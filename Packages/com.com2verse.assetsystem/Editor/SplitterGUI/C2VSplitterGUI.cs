/*===============================================================
* Product:		Com2Verse
* File Name:	C2VSplitterGUI.cs
* Developer:	tlghks1009
* Date:			2023-03-23 11:16
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Com2VerseEditor.AssetSystem
{
    public sealed class C2VSplitterGUI
    {
        private static bool _splitterActive;
        private static int _splitterActiveId = -1;
        private static Vector2 _splitterMousePosition;

        public static float VerticalSplitter(int id, float value, float min, float max, EditorWindow editorWindow)
        {
            return Splitter(id, ref value, min, max, editorWindow, true);
        }

        public static float HorizontalSplitter(int id, float value, float min, float max, EditorWindow editorWindow)
        {
            return Splitter(id, ref value, min, max, editorWindow, false);
        }

        static float Splitter(int id, ref float value, float min, float max, EditorWindow editorWindow, bool vertical)
        {
            Rect position; // = new Rect();

            if (vertical)
            {
                position = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true));

                var oldColor = GUI.color;
                GUI.color = new Color(0, 0, 0, 0.25f); // new Color(GUI.color.r, GUI.color.g, GUI.color.b, GUI.color.a * 0.25f);
                GUI.DrawTexture(position, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
                GUI.color = oldColor;

                position.y -= 2;
                position.height += 4;
                EditorGUIUtility.AddCursorRect(position, MouseCursor.SplitResizeUpDown);
            }
            else
            {
                position = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandHeight(true));

                var oldColor = GUI.color;
                GUI.color = new Color(0, 0, 0, 0.25f); // new Color(GUI.color.r, GUI.color.g, GUI.color.b, GUI.color.a * 0.25f);
                GUI.DrawTexture(position, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
                GUI.color = oldColor;

                position.x -= 2;
                position.width += 4;
                EditorGUIUtility.AddCursorRect(position, MouseCursor.SplitResizeLeftRight);
            }

            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    if (position.Contains(Event.current.mousePosition))
                    {
                        _splitterActive = true;
                        _splitterActiveId = id;
                        _splitterMousePosition = Event.current.mousePosition;
                    }

                    break;

                case EventType.MouseUp:
                case EventType.MouseLeaveWindow:
                    _splitterActive = false;
                    _splitterActiveId = -1;
                    editorWindow.Repaint();
                    break;

                case EventType.MouseDrag:
                    if (_splitterActive && _splitterActiveId == id)
                    {
                        var delta = Event.current.mousePosition - _splitterMousePosition;
                        _splitterMousePosition = Event.current.mousePosition;

                        if (vertical)
                            value = Mathf.Clamp(value - delta.y / editorWindow.position.height, min, max);
                        else
                            value = Mathf.Clamp(value - delta.x / editorWindow.position.width, min, max);

                        editorWindow.Repaint();
                    }

                    break;
            }

            return value;
        }
    }
}
