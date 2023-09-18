using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System;
using System.Linq;
using TMPro;
using Unity.VisualScripting;

namespace Com2verseEditor.UnityAssetTool
{
    public interface IObjInfo
    {
        void Render();
        void SetFilter(bool[] _filter);
        void MatchString(string str);

        public string GetDescription();
    }


    /// <summary>
    /// 조작가능하지 않은 마스커블 오브젝트의 경우 꺼둔다. (성능에 영향)
    /// </summary>
    public interface ISetRacacstTarget
    {
        void SetRaycastTarget();
    }

    /// <summary>
    /// 버텍트 컬러의 값이 0에 가까우면 컬링한다. (투명한경우 컬링)
    /// </summary>
    public interface ISetCullTransparentMesh
    {
        void SetCullTransparentMesh();
    }

    public partial class TATUGUITool : EditorWindow
    {
        private bool[] filters = new bool[] { };
        private bool filter_LayoutGroup_Vertical = true;
        private bool filter_LayoutGroup_Horizontal = true;
        private bool filter_LayoutGroup_Grid = true;


        void UIGroup_ObjInfo()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Image", GUILayout.Height(30))) { FindFromSelection<Image, Info_Image>(Selection.activeObject as GameObject); }
            if (GUILayout.Button("Texture", GUILayout.Height(30))) { FindFromSelection<RawImage, Info_RawImage>(Selection.activeObject as GameObject); }
            //if (GUILayout.Button("UI Text", GUILayout.Height(30))) { FindFromSelection<Text, Info_UIText>(); }
            if (GUILayout.Button("TMP Text", GUILayout.Height(30))) { FindFromSelection<TMP_Text, Info_TMPText>(Selection.activeObject as GameObject); }
            if (GUILayout.Button("Button", GUILayout.Height(30))) { FindFromSelection<Button, Info_Button>(Selection.activeObject as GameObject); }
            if (GUILayout.Button("Canvas", GUILayout.Height(30))) { FindFromSelection<Canvas, Info_Canvas>(Selection.activeObject as GameObject); }
            if (GUILayout.Button("Layout Group", GUILayout.Height(30))) { FindFromSelection<LayoutGroup, Info_LayoutGroup>(Selection.activeObject as GameObject); }
            //if (GUILayout.Button("UI ScrollRect", GUILayout.Height(30))) { FindFromSelection<ScrollRect, Info_UIScrollRect>(); }
            if (GUILayout.Button("UI Rect", GUILayout.Height(30))) { FindFromSelection<RectTransform, Info_RectTransform>(Selection.activeObject as GameObject); }
            //if (GUILayout.Button("Box Collider", GUILayout.Height(30))) { FindFromSelection<Collider, Info_Collider>(); }
            //if (GUILayout.Button("Particle", GUILayout.Height(30))) { FindFromSelection<ParticleSystem, Info_Particle>();}
            //if (GUILayout.Button("Transform", GUILayout.Height(30))) { FindFromSelection<Transform, Info_Transform>(); }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Reset", GUILayout.Height(30), GUILayout.Width(50)))
            {
                this.objInfoList.Clear();
            };

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();


            string message = 0 <= objInfoList.Count ? this.objInfoList.Count.ToString() : "Not found";
            GUILayout.Label($"Transform Node Count: {this.childcount},\t Find Target Count : {message}");


            if (0 < this.objInfoList.Count)
            {
                GUILayout.Label(this.objInfoList[0].GetDescription());     //<<< 현재 메뉴와 상태 안내
 

                EditorGUILayout.BeginHorizontal();

                if (this.objInfoList[0] is Info_LayoutGroup)
                {
                    GUI.color = Color.green;
                    this.filter_LayoutGroup_Horizontal = GUILayout.Toggle(filter_LayoutGroup_Horizontal, "Horizontal LayoutGroup", GUILayout.Width(180));
                    GUI.color = Color.cyan;
                    this.filter_LayoutGroup_Vertical = GUILayout.Toggle(filter_LayoutGroup_Vertical, "Vertical LayoutGroup", GUILayout.Width(180));
                    GUI.color = Color.white;
                    this.filter_LayoutGroup_Grid = GUILayout.Toggle(filter_LayoutGroup_Grid, "Grid LayoutGroup", GUILayout.Width(150));

                    this.filters = new bool[] { this.filter_LayoutGroup_Vertical, this.filter_LayoutGroup_Horizontal, this.filter_LayoutGroup_Grid };
                }


                if (this.objInfoList[0] is ISetRacacstTarget)
                {
                    if (GUILayout.Button("Set Raycast Target", GUILayout.Height(30)))
                    {
                        foreach (IObjInfo objInfo in this.objInfoList)
                        {
                            (objInfo as ISetRacacstTarget).SetRaycastTarget();
                        }
                    }
                }

                if (this.objInfoList[0] is ISetCullTransparentMesh)
                {
                    if (GUILayout.Button("Set Cull Transparent Mesh", GUILayout.Height(30)))
                    {
                        foreach (IObjInfo objInfo in this.objInfoList)
                        {
                            (objInfo as ISetCullTransparentMesh).SetCullTransparentMesh();
                        }
                    }
                }

                if (this.objInfoList[0] is Info_UIScrollRect)
                {
                    if (GUILayout.Button("All Mask On", GUILayout.Height(30)))
                    {
                        foreach (IObjInfo objInfo in this.objInfoList)
                        {
                            (objInfo as Info_UIScrollRect).Mask2DEnable(true);
                        }
                    }

                    if (GUILayout.Button("All Mask Off", GUILayout.Height(30)))
                    {
                        foreach (IObjInfo objInfo in this.objInfoList)
                        {
                            (objInfo as Info_UIScrollRect).Mask2DEnable(false);
                        }
                    }
                } 
                
                else if (this.objInfoList[0] is Info_Text)
                {
                    if (GUILayout.Button("Reset Line Spacing", GUILayout.Height(30)))
                    {
                        foreach (IObjInfo objInfo in this.objInfoList)
                        {
                            (objInfo as Info_Text).ResetLineSpacing();
                        }
                    }

                    if (GUILayout.Button("Set Vertical Overflow", GUILayout.Height(30)))
                    {
                        foreach (IObjInfo objInfo in this.objInfoList)
                        {
                            (objInfo as Info_Text).SetVerticalOverFlow(VerticalWrapMode.Overflow);
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();
            }




            EditorGUILayout.BeginVertical();
            this.objInfoListScrollViewPos = EditorGUILayout.BeginScrollView(objInfoListScrollViewPos);
            foreach (IObjInfo objInfo in this.objInfoList)
            {
                objInfo.SetFilter(this.filters);
                objInfo.Render();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        
        
        bool FindFromSelection<T, InfoType>(GameObject _rootObj)
            where T : Component
            where InfoType : class
        {
            this.objInfoList.Clear();
            if (_rootObj == null) return false;

            this.childcount = _rootObj.GetComponentsInChildren<Transform>(true).Length;
            
            var list = _rootObj.GetComponentsInChildren<T>(true);
            foreach (var elem in list)
            {
                this.objInfoList.Add(Activator.CreateInstance(typeof(InfoType), new object[] { elem.gameObject }) as IObjInfo);
            }
   
            return true;
        }
    }
}



//string GetInfoDescription()
//{
//    string infoString = "Hierarchy 창에서 Root GameObject를 선택한후 상단 버튼을 눌러 주세요.";
//    if (this.objInfoList.Count <= 0) return infoString;

//    if (this.objInfoList[0] is Info_UIButton) { infoString = "Button 리스트"; }
//    else if (this.objInfoList[0] is Info_Image) { infoString = "Image 리스트"; }
//    else if (this.objInfoList[0] is Info_RawImage) { infoString = "RawImage 리스트"; }
//    else if (this.objInfoList[0] is Info_UITextMeshPro) { infoString = "TMP_Text 리스트"; }

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