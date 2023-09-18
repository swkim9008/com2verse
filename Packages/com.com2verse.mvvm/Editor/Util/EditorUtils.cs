
#if UNITY_EDITOR
using UnityEngine;

namespace Com2VerseEditor.UI
{
    public class LineHelper
    {
        public static void Draw(Color color)
        {
            Color backupColor = GUI.color;
            GUI.color = color;
            GUILayout.Box (Texture2D.whiteTexture, GUILayout.ExpandWidth(true) , GUILayout.Height (1f));
            GUI.color = backupColor;
        }
    }

    public class LabelHelper
    {
        public static void Draw(string label, Color color)
        {
            Color backupColor = GUI.color;
            GUI.color = color;
            GUILayout.Label( label );
            GUI.color = backupColor;
        }
    }

    public class ButtonHelper
    {
        public static bool Button(string name, Color backgroundColor, float width)
        {
            Color backupColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(name, GUILayout.Width( width )))
            {
                return true;
            }
            GUI.backgroundColor = backupColor;
            GUILayout.FlexibleSpace();
            return false;
        }
        
        
        public static bool Button( string name, Color backgroundColor )
        {   
            Color backupColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
            if (GUILayout.Button(name ))
            {
                return true;
            }
            GUI.backgroundColor = backupColor;
            return false;
        }
    

        public static bool Button(string name)
        {
            return Button(name, GUI.backgroundColor);
        }
    }



}
#endif
