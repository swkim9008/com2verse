using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Com2verseEditor.UnityAssetTool
{
    [System.Serializable]
    public class Info_Text : IObjInfo, ISetRacacstTarget, ISetCullTransparentMesh
    {
        public Text text;
        public int fontSize;
        public int newFontSize;
        public Color32 fontColor;

        private bool raycastTarget;
        private bool cullTransparentMesh;

        public float lineSpacing;
        public HorizontalWrapMode horizontalWrapMode;
        public VerticalWrapMode verticalWrapMode;

        //private UITextLocalization uiTextLocalization = null;
        //private bool isLocalization = false;
        //private string localKey = string.Empty;

        public string GetDescription()
        {
            return $"Text ";
        }


        public Info_Text(GameObject _obj)
        {
            this.text = _obj.GetComponent<Text>();
            this.raycastTarget = this.text.raycastTarget;
            this.fontColor = this.text.color;
            this.fontSize = this.text.fontSize;
            this.newFontSize = this.text.fontSize;
            this.horizontalWrapMode = this.text.horizontalOverflow;
            this.verticalWrapMode = this.text.verticalOverflow;

            //this.uiTextLocalization = _obj.GetComponent<UITextLocalization>();
            //this.isLocalization = this.uiTextLocalization != null;
            //if (null == this.uiTextLocalization) return;
            //this.localKey = this.uiTextLocalization.Key;
        }

        public void SetRayCastTarget(bool _bool)
        {
            this.raycastTarget = _bool;
            this.text.raycastTarget = this.raycastTarget;
        }

        public void ResetLineSpacing()
        {
            this.lineSpacing = 1.0f;
            this.text.lineSpacing = this.lineSpacing;
        }

        public void SetVerticalOverFlow(VerticalWrapMode _verticalWrapMode)
        {
            this.verticalWrapMode = _verticalWrapMode;
            this.text.verticalOverflow = this.verticalWrapMode;
        }

        public void Render()
        {
            if (null == this.text) return;

            GUI.color = this.text.font == null ? Color.red : Color.white;
            
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.ObjectField(this.text.gameObject, typeof(GameObject), true, GUILayout.Width(120));
            EditorGUILayout.ObjectField(this.text.font, typeof(Font), true, GUILayout.Width(120));
            this.fontColor = EditorGUILayout.ColorField(this.fontColor, GUILayout.Width(50));
            EditorGUILayout.LabelField(String.Format("Font Size : {0}", this.text.fontSize), GUILayout.Width(120));

            this.newFontSize = int.Parse(EditorGUILayout.TextArea(this.newFontSize.ToString(), GUILayout.Width(50)));

            GUI.color = this.horizontalWrapMode != HorizontalWrapMode.Overflow ? Color.cyan : Color.green;
            this.horizontalWrapMode = (HorizontalWrapMode)EditorGUILayout.EnumPopup("Horizontal Overflow", this.horizontalWrapMode, GUILayout.Width(250));
            GUI.color = Color.white;

            if (this.verticalWrapMode != VerticalWrapMode.Overflow) { GUI.color = Color.yellow; }
            this.verticalWrapMode = (VerticalWrapMode)EditorGUILayout.EnumPopup("Vertical Overflow", this.verticalWrapMode, GUILayout.Width(250));
            GUI.color = Color.white;

            GUI.color = this.text.lineSpacing == 1.0f ? Color.white : Color.red;
            EditorGUILayout.LabelField(String.Format("Line Spacing : {0}", this.text.lineSpacing), GUILayout.Width(150));
            GUI.color = Color.white;

            GUI.color = this.raycastTarget == false ? Color.white : Color.red;
            this.raycastTarget = EditorGUILayout.Toggle("RaycastTarget", this.raycastTarget, GUILayout.Width(180));
            

            //GUI.color = this.isLocalization == false ? Color.white : Color.cyan;
            //this.isLocalization = EditorGUILayout.Toggle("Localization", this.isLocalization, GUILayout.Width(180));
            //this.localKey = EditorGUILayout.TextField(this.localKey, GUILayout.Width(180));


            GUI.color = Color.white;
            EditorGUILayout.Space();

            GUI.color = this.fontSize == newFontSize ? Color.white : Color.yellow;
            if (GUILayout.Button("Apply", GUILayout.Height(30), GUILayout.Width(100)))
            {
                this.text.raycastTarget = this.raycastTarget;
                this.text.horizontalOverflow = this.horizontalWrapMode;
                this.text.verticalOverflow = this.verticalWrapMode;
                this.text.fontSize = this.newFontSize;
            }

            

            EditorGUILayout.EndHorizontal();
        }



        public void SetCullTransparentMesh()
        {
            bool _bool = true;
            //if (this.text.gameObject.GetComponent<Selectable>() != null || this.text.GetComponent<ScrollRect>() != null)
            //{
            //    _bool = true; //버튼등의 조작가능한 컴포넌트가 있으면 강제로 켜둔다.
            //}

            this.cullTransparentMesh = _bool;
            this.text.GetComponent<CanvasRenderer>().cullTransparentMesh = this.cullTransparentMesh;
        }

        public void SetRaycastTarget()
        {
            bool _bool = true;
            //if (this.text.gameObject.GetComponent<Selectable>() != null || this.text.GetComponent<ScrollRect>() != null)
            //{
            //    _bool = true; //버튼등의 조작가능한 컴포넌트가 있으면 강제로 켜둔다.
            //}

            this.raycastTarget = _bool;
            this.text.raycastTarget = this.raycastTarget;
        }

        public void SetFilter(bool[] _filter)
        {

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
