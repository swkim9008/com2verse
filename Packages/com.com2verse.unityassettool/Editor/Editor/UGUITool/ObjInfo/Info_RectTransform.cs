using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Com2verseEditor.UnityAssetTool
{
    [System.Serializable]
    public class Info_RectTransform : IObjInfo
    {
        public RectTransform rectTransform;
        public CanvasRenderer canvasRenderer;
        public MaskableGraphic maskableGraphic;



        //public Component[] components;
        //public bool isAnchored;
        public string GetDescription()
        {
            return $"RectTransform: MaskableGraphic이 없는데 Canvas Renderer가 있을경우 Remove 버튼 표시";
        }
        public Info_RectTransform(GameObject _obj)
        {
            this.rectTransform = _obj.GetComponent<RectTransform>();
            this.canvasRenderer = _obj.GetComponent<CanvasRenderer>();
            this.maskableGraphic = _obj.GetComponent<MaskableGraphic>();
            //this.components = _obj.GetComponents(typeof(Component));
        }

  

        public void Render()
        {
            if (null == this.rectTransform) return;

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.ObjectField(rectTransform.gameObject, typeof(GameObject), true, GUILayout.Width(200));
            //EditorGUILayout.Toggle("isAnchored", this.isAnchored);

            EditorGUILayout.Space();
            if (this.canvasRenderer != null && this.maskableGraphic == null)
            {
                GUI.color = Color.red;
                if (GUILayout.Button("Remove", GUILayout.Height(30), GUILayout.Width(100)))
                {
                    GameObject.DestroyImmediate(this.canvasRenderer);
                }
                GUI.color = Color.white;
            }

            EditorGUILayout.EndHorizontal();
        }


        bool IsEmptyChild()
        {
            
            bool isParentEmpty = this.rectTransform.parent.GetComponent<CanvasRenderer>();
            this.maskableGraphic = this.rectTransform.parent.GetComponent<MaskableGraphic>();

            //if (this.canvasRenderer != null && this.maskableGraphic == null)

            return false; 
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
