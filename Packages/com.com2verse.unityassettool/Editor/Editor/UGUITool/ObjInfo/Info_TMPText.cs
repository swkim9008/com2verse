using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Com2verseEditor.UnityAssetTool
{
    [System.Serializable]
    public class Info_TMPText : IObjInfo, ISetRacacstTarget, ISetCullTransparentMesh
    {
        public TMP_Text tMP_text;
        public float fontSize;
        public float newFontSize;
        public Color32 fontColor;
        public bool raycastTarget;
        
        private bool cullTransparentMesh;
        //public float lineSpacing;
        //public HorizontalWrapMode horizontalWrapMode;
        //public VerticalWrapMode verticalWrapMode;
        public string GetDescription()
        {
            return $"TMP Text";
        }

        public Info_TMPText(GameObject _obj)
        {
            this.tMP_text = _obj.GetComponent<TMP_Text>();
            this.fontSize = this.tMP_text.fontSize;
            this.newFontSize = this.tMP_text.fontSize;
            this.fontColor = this.tMP_text.color;
            this.raycastTarget = this.tMP_text.raycastTarget;
            this.cullTransparentMesh = _obj.GetComponent<CanvasRenderer>().cullTransparentMesh;

            //this.horizontalWrapMode = this.text.horizontalOverflow;
            //this.verticalWrapMode = this.text.verticalOverflow;
        }

        public void Render()
        {
            if (null == this.tMP_text) return;

            GUI.color = this.tMP_text.font == null ? Color.red : Color.white;

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.ObjectField(this.tMP_text.gameObject, typeof(GameObject), false);

            GUI.color = this.tMP_text.font.name.Contains("LiberationSans SDF") ? Color.red : Color.white;
            EditorGUILayout.ObjectField(this.tMP_text.font, typeof(Font), false);
            GUI.color=Color.white;

            EditorGUILayout.ObjectField(this.tMP_text.fontMaterial, typeof(Material), false);
            EditorGUILayout.LabelField($"Font Size: {this.tMP_text.fontSize}", GUILayout.Width(120));
            this.fontColor = EditorGUILayout.ColorField(this.fontColor, GUILayout.Width(50));

            //this.newFontSize = int.Parse(EditorGUILayout.TextArea(this.newFontSize.ToString(), GUILayout.Width(50)));

            //GUI.color = this.horizontalWrapMode != HorizontalWrapMode.Overflow ? Color.cyan : Color.green;
            //this.horizontalWrapMode = (HorizontalWrapMode)EditorGUILayout.EnumPopup("Horizontal Overflow", this.horizontalWrapMode, GUILayout.Width(250));
            //GUI.color = Color.white;

            //if (this.verticalWrapMode != VerticalWrapMode.Overflow) { GUI.color = Color.yellow; }
            //this.verticalWrapMode = (VerticalWrapMode)EditorGUILayout.EnumPopup("Vertical Overflow", this.verticalWrapMode, GUILayout.Width(250));
            //GUI.color = Color.white;

            //GUI.color = this.text.lineSpacing == 1.0f ? Color.white : Color.red;
            //EditorGUILayout.LabelField(String.Format("Line Spacing : {0}", this.text.lineSpacing), GUILayout.Width(150));
            //GUI.color = Color.white;

            GUI.color = this.raycastTarget == false ? Color.white : Color.red;
            this.raycastTarget = EditorGUILayout.ToggleLeft("RaycastTarget", this.raycastTarget, GUILayout.Width(120));

            GUI.color = this.cullTransparentMesh == false ? Color.white : Color.yellow;
            this.cullTransparentMesh = EditorGUILayout.ToggleLeft("Cull Transparent Mesh", this.cullTransparentMesh, GUILayout.Width(150));
            GUI.color = Color.white;

            ////GUI.color = this.isLocalization == false ? Color.white : Color.cyan;
            ////this.isLocalization = EditorGUILayout.Toggle("Localization", this.isLocalization, GUILayout.Width(180));
            ////this.localKey = EditorGUILayout.TextField(this.localKey, GUILayout.Width(180));


            //GUI.color = Color.white;
            //EditorGUILayout.Space();

            //GUI.color = this.fontSize == newFontSize ? Color.white : Color.yellow;
            //if (GUILayout.Button("Apply", GUILayout.Height(30), GUILayout.Width(100)))
            //{
            //    this.text.raycastTarget = this.raycastTarget;
            //    //this.text.horizontalOverflow = this.horizontalWrapMode;
            //    //this.text.verticalOverflow = this.verticalWrapMode;
            //    //this.text.fontSize = this.newFontSize;
            //}



            EditorGUILayout.EndHorizontal();
        }

        //public void SetRayCastTarget(bool _bool)
        //{
        //    this.raycastTarget = _bool;
        //    this.text.raycastTarget = this.raycastTarget;
        //}

        //public void ResetLineSpacing()
        //{
        //    this.lineSpacing = 1.0f;
        //    this.text.lineSpacing = this.lineSpacing;
        //}

        //public void SetVerticalOverFlow(VerticalWrapMode _verticalWrapMode)
        //{
        //    this.verticalWrapMode = _verticalWrapMode;
        //    //this.text.verticalOverflow = this.verticalWrapMode;
        //}



        public void SetFilter(bool[] _filter)
        {
            
        }

        public void SetCullTransparentMesh()
        {
            bool _bool = true;
            //if (this.text.gameObject.GetComponent<Selectable>() != null || this.text.GetComponent<ScrollRect>() != null)
            //{
            //    _bool = true; //버튼등의 조작가능한 컴포넌트가 있으면 강제로 켜둔다.
            //}


            this.cullTransparentMesh = _bool;
            this.tMP_text.GetComponent<CanvasRenderer>().cullTransparentMesh = this.cullTransparentMesh;
        }

        public void SetRaycastTarget()
        {
            bool _bool = false;

            //if (this.text.gameObject.GetComponent<Selectable>() != null || this.text.GetComponent<ScrollRect>() != null)
            //{
            //    _bool = true; //버튼등의 조작가능한 컴포넌트가 있으면 강제로 켜둔다.
            //}

            this.raycastTarget = _bool;
            this.tMP_text.raycastTarget = this.raycastTarget;
        }

        public void MatchString(string str)
        {
            //throw new NotImplementedException();
        }
    }
}




//if (this.horizontalWrapMode != HorizontalWrapMode.Overflow) { GUI.color = Color.yellow; }
//public UILocalization uILocalization;
//public bool convertToLocalFonts;
//this.uILocalization.convertToLocalFonts = this.convertToLocalFonts;

//if (this.uILocalization != null) { 
//    this.convertToLocalFonts = EditorGUILayout.Toggle("convertLocalFont", this.convertToLocalFonts, GUILayout.Width(150));
//}

//public void SetConvertToLocalFont(bool _bool)
//{
//    if (this.uILocalization == null) return;
//    this.convertToLocalFonts = _bool;
//    this.uILocalization.convertToLocalFonts = this.convertToLocalFonts;
//}
//지워야징
//this.uILocalization = _obj.GetComponent<UILocalization>();

//if (null != this.uILocalization)
//{ 
//    this.convertToLocalFonts = this.uILocalization.convertToLocalFonts;
//}
