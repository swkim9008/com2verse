using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Com2verseEditor.UnityAssetTool
{
    [System.Serializable]
    public class Info_UIScrollRect : IObjInfo
    {
        public ScrollRect scrollRect;
        public ScrollRect.MovementType movementType;
        public bool useHorizontal;
        public bool useVertical;
        public RectTransform content;

        public Image scrollBg = null;

        public RectMask2D rectMask2D = null;
        public bool useRectMask2D = false;

        public ContentSizeFitter contentSizeFitter;
        public ContentSizeFitter.FitMode horizontalFit;
        public ContentSizeFitter.FitMode verticalFit;

        public HorizontalOrVerticalLayoutGroup layoutGroup;
        public TextAnchor childAligement;
        public bool controlChildSizeWidth;
        public bool controlChildSizeHeight;
        public bool useChildScaleWidth;
        public bool useChildScaleHeight;
        public bool childForceExpandWidth;
        public bool childForceExpandHeight;

        public string GetDescription()
        {
            throw new NotImplementedException();
        }
        public Info_UIScrollRect(GameObject _obj)
        {
            this.scrollRect = _obj.GetComponent<ScrollRect>();
            this.useHorizontal = this.scrollRect.horizontal;
            this.useVertical = this.scrollRect.vertical;
            this.movementType = this.scrollRect.movementType;
            this.content = this.scrollRect.content;

            this.rectMask2D = _obj.GetComponent<RectMask2D>();
            if (null != this.rectMask2D)
            {
                this.useRectMask2D = this.rectMask2D.enabled;
            }
            this.scrollBg = _obj.GetComponent<Image>();

            Debug.Log(">>>" + this.scrollRect.gameObject.name);

            this.contentSizeFitter = this.content.GetComponent<ContentSizeFitter>();

            if (null != this.contentSizeFitter)
            {
                this.horizontalFit = this.contentSizeFitter.horizontalFit;
                this.verticalFit = this.contentSizeFitter.verticalFit;
            }

            this.layoutGroup = this.content.GetComponent<HorizontalOrVerticalLayoutGroup>();

            if (null != this.layoutGroup)
            {
                this.childAligement = layoutGroup.childAlignment;
                this.controlChildSizeWidth = layoutGroup.childScaleWidth;
                this.controlChildSizeHeight = layoutGroup.childScaleHeight;
                this.useChildScaleWidth = layoutGroup.childControlWidth;
                this.useChildScaleHeight = layoutGroup.childControlHeight;
                this.childForceExpandWidth = layoutGroup.childForceExpandWidth;
                this.childForceExpandHeight = layoutGroup.childForceExpandHeight;
            }
        }


        public void Mask2DEnable(bool _bool)
        {
            if (this.rectMask2D == null) return;
            this.rectMask2D.enabled = _bool;
        }

        public void Render()
        {
            if (null == this.scrollRect) return;

            GUI.color = this.useHorizontal ? Color.green : Color.cyan;
            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.BeginVertical("box", GUILayout.Width(250));
            EditorGUILayout.ObjectField(this.scrollRect.gameObject, typeof(GameObject), true, GUILayout.Width(200));
            this.movementType = (ScrollRect.MovementType)EditorGUILayout.EnumPopup("Movement Type", this.movementType);
            if (null != this.rectMask2D) { this.useRectMask2D = EditorGUILayout.Toggle("Mask", this.useRectMask2D); }
            this.useHorizontal = EditorGUILayout.Toggle("Horizontal", this.useHorizontal);
            this.useVertical = EditorGUILayout.Toggle("Vertical", this.useVertical);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.Width(250));
            EditorGUILayout.ObjectField(this.content.gameObject, typeof(GameObject), true);
            EditorGUILayout.Toggle("Apply ContentSizeFitter", this.contentSizeFitter != null ? true : false);
            this.horizontalFit = (ContentSizeFitter.FitMode)EditorGUILayout.EnumPopup("Horizontal Fit ", this.horizontalFit);
            this.verticalFit = (ContentSizeFitter.FitMode)EditorGUILayout.EnumPopup("Vertical Fit ", this.verticalFit);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.Width(300));
            EditorGUILayout.ObjectField(this.layoutGroup, typeof(LayoutGroup), true);
            this.childAligement = (TextAnchor)EditorGUILayout.EnumPopup("Child Alignment ", this.childAligement);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Control Child Size");
            this.controlChildSizeWidth = GUILayout.Toggle(this.controlChildSizeWidth, " Width", GUILayout.Width(80));
            this.controlChildSizeHeight = GUILayout.Toggle(this.controlChildSizeHeight, " Height", GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Use Child Scale");
            this.useChildScaleWidth = GUILayout.Toggle(this.useChildScaleWidth, " Width", GUILayout.Width(80));
            this.useChildScaleHeight = GUILayout.Toggle(this.useChildScaleHeight, " Height", GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Child Force Expand");
            this.childForceExpandWidth = GUILayout.Toggle(this.childForceExpandWidth, " Width", GUILayout.Width(80));
            this.childForceExpandHeight = GUILayout.Toggle(this.childForceExpandHeight, " Height", GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.Width(100));

            if (this.scrollBg == null && this.useVertical){ EditorGUILayout.ObjectField(this.scrollBg, typeof(Image), true); }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            if (GUILayout.Button("Apply", GUILayout.Height(30), GUILayout.Width(100)))
            {
                this.rectMask2D.enabled = this.useRectMask2D;
                this.scrollRect.movementType = this.movementType;
                this.scrollRect.horizontal = this.useHorizontal;
                this.scrollRect.vertical = this.useVertical;
                this.contentSizeFitter.horizontalFit = this.horizontalFit;
                this.contentSizeFitter.verticalFit = this.verticalFit;

                this.layoutGroup.childAlignment = this.childAligement;
                this.layoutGroup.childScaleWidth = this.controlChildSizeWidth;
                this.layoutGroup.childScaleHeight = this.controlChildSizeHeight;
                this.layoutGroup.childControlWidth = this.useChildScaleWidth;
                this.layoutGroup.childControlHeight = this.useChildScaleHeight;
                this.layoutGroup.childForceExpandWidth = this.childForceExpandWidth;
                this.layoutGroup.childForceExpandHeight = this.childForceExpandHeight;
            }
            EditorGUILayout.EndHorizontal();
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
