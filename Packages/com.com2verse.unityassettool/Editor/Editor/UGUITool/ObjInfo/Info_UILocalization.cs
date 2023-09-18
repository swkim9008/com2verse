using System;
using Com2Verse.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Com2verseEditor.UnityAssetTool
{
    [System.Serializable]
    public class Info_UILocalization : IObjInfo
    {
        public TMP_Text tMP_text;
        public LocalizationUI localizationUI = null;
        private string localKey = string.Empty;

        private bool[] filter;
        private string filterString = string.Empty;
        
        //private bool isTextLocalization = false;
        //public UIFontLocalization uiFontLocalization = null;
        //public UIDataField uIDataField = null;
        public string GetDescription()
        {
            return "Localization UI 컴포넌트";
        }
        
        public void SetFilter(bool[] _filter) { this.filter = _filter; }

        public void MatchString(string str) { this.filterString = str; }

        public Info_UILocalization(GameObject _obj)
        {
            this.tMP_text = _obj.GetComponent<TMP_Text>();
            this.localizationUI = _obj.GetComponent<LocalizationUI>();

            //this.uiFontLocalization = _obj.GetComponent<UIFontLocalization>();
            //this.uIDataField= _obj.GetComponent<UIDataField>();
            //this.isTextLocalization = this.uiTextLocalization != null;
            //this.isFontLocalization = this.uIFontLocalization != null;
            if (null == this.localizationUI) return;
            this.localKey = this.localizationUI.TextKey;
        }


        public void AddUIFontLocalization()
        {
            if (PrefabUtility.IsPartOfPrefabInstance(this.tMP_text.gameObject)) return;

            //if (!this.isTextLocalization) return;
            //if (this.uiFontLocalization != null) return;

            //this.text.gameObject.AddComponent<UIFontLocalization>();
            //this.uiFontLocalization = this.text.gameObject.GetComponent<UIFontLocalization>();
        }

        public void Render()
        {
            if (null == this.tMP_text) return;
            if (!this.filter[0] && (this.localizationUI != null)) return;
            if (3 <= this.filterString.Length && !this.localKey.Contains(this.filterString)) return;

            GUI.color = Color.white;

            EditorGUILayout.BeginHorizontal("box");
            
            EditorGUILayout.LabelField(this.tMP_text.text, GUILayout.Width(200), GUILayout.ExpandWidth(true));
            EditorGUILayout.ObjectField(this.tMP_text.gameObject, typeof(GameObject), true, GUILayout.Width(200));
            GUI.color = Color.white;
            if (this.localizationUI != null)
            {
                GUI.color = this.localKey.Length <= 0 ? Color.red : Color.white;
                this.localKey = EditorGUILayout.TextField(this.localKey, GUILayout.Width(200));
                this.localizationUI.TextKey = this.localKey;

                GUI.color = Color.yellow;
                if (GUILayout.Button("Remove LocalizationUI", GUILayout.Height(20), GUILayout.Width(150)))
                {
                    if (0 < this.localKey.Length)
                    {
                        if (EditorUtility.DisplayDialog("확인", "LocalizationUI 컴포넌트를 제거하겠습니까?", "제거", "취소"))
                        {
                            GameObject.DestroyImmediate(this.localizationUI);
                        }
                    }
                    else
                    {
                        GameObject.DestroyImmediate(this.localizationUI);
                    }
                }
                GUI.color = Color.white;
            }
            else
            {
                if (GUILayout.Button("ADD LocalizationUI", GUILayout.Height(20), GUILayout.Width(200)))
                {
                    this.tMP_text.gameObject.AddComponent<LocalizationUI>();
                    this.localizationUI = this.tMP_text.gameObject.GetComponent<LocalizationUI>();
                }
                GUILayout.Space(153);
            }

            
            
            //EditorGUILayout.Space(10,false);

            //EditorGUILayout.Space(10, false);
            //GUI.color = this.isFontLocalization == false ? Color.white : Color.cyan;
            //this.tMP_text.font = (TMP_FontAsset)EditorGUILayout.ObjectField(this.tMP_text.font, typeof(TMP_FontAsset), true, GUILayout.Width(120));

            //if (this.isFontLocalization)
            //{
            
            //    if (GUILayout.Button("REMOVE UIFontLocalization", GUILayout.Height(30), GUILayout.Width(200)))
            //    {
            //        if (this.uiFontLocalization == null) return;
            //        Component.DestroyImmediate(this.text.gameObject.GetComponent<UIFontLocalization>());
            //    }
            //} else {
            //    GUI.color = Color.white;
            //    if (GUILayout.Button("ADD UIFontLocalization", GUILayout.Height(30), GUILayout.Width(200)))
            //    {
            //        if (this.uiFontLocalization != null) return;
            //        if (this.text.font.name == "Number" || this.text.font.name == "PositionFont") return;

            //        this.text.gameObject.AddComponent<UIFontLocalization>();
            //        //this.uiFontLocalization = this.text.gameObject.GetComponent<UIFontLocalization>();
            //    }
            //}
            
            EditorGUILayout.EndHorizontal();
        }


    }
}

