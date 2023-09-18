using UnityEditor;
using UnityEditor.Scripting.Python;

namespace _ArtRawAssets.PythonScripts
{
    public abstract class PyScriptMenuItems
    {
        [MenuItem("Com2Verse/Py/Bottom2Bot")]
        public static void BottomToBot()
        {
            PythonRunner.RunFile("Assets/_ArtRawAssets/PythonScripts/bottom2bot.py");
        }
        
        [MenuItem("Com2Verse/Py/Bot2Bottom")]
        public static void BotToBottom()
        {
            PythonRunner.RunFile("Assets/_ArtRawAssets/PythonScripts/bot2bottom.py");
        }
    }
}