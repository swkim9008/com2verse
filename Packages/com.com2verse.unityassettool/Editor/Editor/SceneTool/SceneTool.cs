using UnityEngine;
using UnityEditor;
using UnityEditor.SearchService;

namespace Com2verseEditor.UnityAssetTool
{
    public partial class SceneTool : EditorWindow
    {
        Vector2 _scrollPosition = Vector2.zero;

        [MenuItem("Com2Verse/ART/Scene Tool &#2")]  //핫키 ctrl + Shift + 2
        public static void Initialize()
        {
            SceneTool window = (SceneTool)EditorWindow.GetWindow(typeof(SceneTool));
            window.Show();
            window.minSize = new Vector2(220, 500);
        }

        private int _menuTab = 0;
        private int _subMenuTab = 0;

        private void OnEnable() { Repaint(); }
        void OnInspectorUpdate() { Repaint(); }

        void OnGUI()
        {
            EditorGUILayout.Separator();
            this._menuTab = GUILayout.Toolbar(this._menuTab, new string[] { "Scene Loader", "Scene Setting" }, GUILayout.Height(25));
            EditorGUILayout.Separator();

            EditorGUILayout.BeginVertical();

            switch (this._menuTab)
            {
                case 0:
                    {
                        SubMenuSceneLoader();
                        
                    }
                    break;
                case 1:
                    {
                        //TabSceneSetting();
                    }
                    break;
            }

            EditorGUILayout.EndVertical();
        }


        void SubMenuSceneLoader()
        {
            GUI.color = Color.cyan;
        
            MakeSceneButton("런쳐", GetScenePath("SceneLogin"), false); //"Assets/Project/Runtime/Scenes/SceneLogin.unity"
            GUI.color = Color.white;
            EditorGUILayout.Separator();
            this._subMenuTab = GUILayout.Toolbar(this._subMenuTab, new string[] { "UI 씬", "RT 씬", "Office 씬", "Mice 씬", "World 씬" }, GUILayout.Height(25));
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(position.width));
            switch (this._subMenuTab)
            {
                case 0:
                    TapBaseUIScene();
                    break;
                case 1:
                    TapUIRTScene();
                    break;
                case 2:
                    TapUIOfficeScene();
                    break;
                case 3:
                    TapUIMiceScene();
                    break;
                case 4:
                    TapUIWorldScene();
                    break;
            }
            EditorGUILayout.EndScrollView();
        }








        void GUILine(int lineHeight = 1)
        {
            EditorGUILayout.Space();
            Rect rect = EditorGUILayout.GetControlRect(false, lineHeight);
            rect.height = lineHeight;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space();
        }
    }
}