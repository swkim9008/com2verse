using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using TMPro;
using NUnit.Framework;

namespace Com2verseEditor.UnityAssetTool
{


    public partial class TATUGUITool : EditorWindow
    {

        bool filter_DataCommonBinder = true;
        bool filter_CommandBinder = true;
        bool filter_NavigationBinder = true;
        bool filter_CollectionBinder = true;
        string filter_String = string.Empty;

        void UIGroup_Localization()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Localization Text", GUILayout.Height(30))) { FindFromSelection<TMP_Text, Info_UILocalization>(Selection.activeObject as GameObject); }
            if (GUILayout.Button("Binder", GUILayout.Height(30)))
            {
                FindFromSelection<Com2Verse.UI.Binder, Info_Binder>(Selection.activeObject as GameObject);
                
                MakeUniqueGameObjectList();
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Reset", GUILayout.Height(30), GUILayout.Width(50))) { this.objInfoList.Clear(); }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            

            string message = 0 <= objInfoList.Count ? this.objInfoList.Count.ToString() : "Not found";
            GUILayout.Label(String.Format("Transform Node Count: {0},\t Find Target Count : {1}", this.childcount, message));

            //if (GUILayout.Button("Add UI FontLocalization", GUILayout.Height(30), GUILayout.Width(200)))
            //{
            //    foreach (IObjInfo objInfo in this.objInfoList)
            //    {
            //        (objInfo as Info_UILocalization).AddUIFontLocalization();
            //    }
            //}

            if (0 < this.objInfoList.Count)
            {
                if (this.objInfoList[0] is Info_Binder)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUI.color = Color.white; this.filter_DataCommonBinder = GUILayout.Toggle(this.filter_DataCommonBinder, "Data Binder"); GUI.color = Color.white;
                    GUI.color = Color.cyan; this.filter_CommandBinder = GUILayout.Toggle(this.filter_CommandBinder, "Command Binder"); GUI.color = Color.white;
                    GUI.color = Color.green; this.filter_NavigationBinder = GUILayout.Toggle(this.filter_NavigationBinder, "Navigation Binder"); GUI.color = Color.white;
                    GUI.color = Color.yellow; this.filter_CollectionBinder = GUILayout.Toggle(this.filter_CollectionBinder, "Colleciton Binder"); GUI.color = Color.white;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    this.filter_String = EditorGUILayout.TextField("Match String:", this.filter_String);
                    if (GUILayout.Button("Reset", GUILayout.Height(30), GUILayout.Width(50))) { this.filter_String = string.Empty; }
                    EditorGUILayout.EndHorizontal();
                }
                else if (this.objInfoList[0] is Info_UILocalization)
                {
                    EditorGUILayout.BeginHorizontal();
                    this.filter_String = EditorGUILayout.TextField("Text Key:", this.filter_String);
                    if (GUILayout.Button("Reset", GUILayout.Height(30), GUILayout.Width(50))) { this.filter_String = string.Empty; }
                    EditorGUILayout.EndHorizontal();
                } 
            }

            EditorGUILayout.BeginVertical();
            this.LocalizationListScrollViewPos = EditorGUILayout.BeginScrollView(LocalizationListScrollViewPos);
            foreach (IObjInfo objInfo in this.objInfoList)
            {
                objInfo.SetFilter(new bool[] { this.filter_DataCommonBinder, this.filter_CommandBinder, this.filter_NavigationBinder, filter_CollectionBinder });
                objInfo.MatchString(this.filter_String);
                objInfo.Render();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            // ---------------------------------------------------------------------------------------------
        }

        void MakeUniqueGameObjectList()
        {
            this.objInfoList = this.objInfoList.GroupBy(e => ((Info_Binder)e).gameObject).Select(e => e.ElementAt(0)).ToList();
        }

    }
}



//string GetInfoDescription()
//{
//    string infoString = "Hierarchy â���� Root GameObject�� �������� ��� ��ư�� ���� �ּ���.";
//    if (this.objInfoList.Count <= 0) return infoString;

//    if (this.objInfoList[0] is Info_UIButton) { infoString = "Button ����Ʈ"; }
//    else if (this.objInfoList[0] is Info_Image) { infoString = "Image ����Ʈ"; }
//    else if (this.objInfoList[0] is Info_RawImage) { infoString = "RawImage ����Ʈ"; }
//    else if (this.objInfoList[0] is Info_UITextMeshPro) { infoString = "TMP_Text ����Ʈ"; }

//    return infoString;
//}


//// Sort Atlas Name * Sprte Name
//if (typeof(UISpriteInfo).IsSubclassOf(typeof(InfoType)) || typeof(UISpriteInfo).IsAssignableFrom(typeof(InfoType)))
//{
//    this.objInfoList.Sort((_valueA, _valueB) =>
//    {
//        var valueA = _valueA as UISpriteInfo;
//        var valueB = _valueB as UISpriteInfo;
//        int resultA = string.Compare(valueA.atlasName, valueB.atlasName);
//        if (resultA == 0) { return string.Compare(valueA.spriteName, valueB.spriteName); }
//        return resultA;
//    });
//}