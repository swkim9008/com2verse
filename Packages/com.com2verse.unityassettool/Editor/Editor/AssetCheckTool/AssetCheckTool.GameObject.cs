using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Com2VerseEditor.UnityAssetTool
{
    public partial class AssetCheckTool : EditorWindow
    {
        readonly List<InfoData> _infoDatas = new List<InfoData>();
        private Vector2 _infoDataListScrollViewPos;

        private int _subMenuTab = 0;

        void TabGameObjectCheck()
        {
            this._subMenuTab = GUILayout.Toolbar(this._subMenuTab, new string[] { "UI Material","Test", "Test"}, GUILayout.Height(30));

            switch (this._subMenuTab)
            {
                case 0:
                    EditorGUILayout.BeginVertical();
                    TabUIMaterial();
                    EditorGUILayout.EndVertical();
                    break;
                case 1:
                    EditorGUILayout.BeginVertical();
                    
                    EditorGUILayout.EndVertical();
                    break;
                case 2:
                    EditorGUILayout.BeginVertical();
                    
                    EditorGUILayout.EndVertical();
                    break;
            }
        }

        void TabUIMaterial()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("UI Material Check", GUILayout.Height(30)))
            {
                GetUGUIMaterialList(); 
            }
            if (GUILayout.Button("Reset", GUILayout.Width(100), GUILayout.Height(30))) { this._infoDatas.Clear(); }
            EditorGUILayout.EndHorizontal();

            this._infoDataListScrollViewPos = EditorGUILayout.BeginScrollView(_infoDataListScrollViewPos);
            foreach (InfoData infodata in _infoDatas)
            {
                EditorGUILayout.BeginVertical();
                infodata.Render();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }



        void GetUGUIMaterialList()
        {
            this._infoDatas.Clear();

            MaskableGraphic[] maskableGraphics = FindObjectsOfType<MaskableGraphic>(true);

            foreach (MaskableGraphic maskableGraphic in maskableGraphics)
            {
                InfoDataMaskableGraphic tmpInfoData = new InfoDataMaskableGraphic(maskableGraphic);
                this._infoDatas.Add(tmpInfoData);
            }
        }
    }
}