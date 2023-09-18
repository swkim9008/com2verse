using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Com2verseEditor.UnityAssetTool
{
    [System.Serializable]
    public class Info_RawImage : IObjInfo, ISetRacacstTarget, ISetCullTransparentMesh
    {
        RawImage rawImage;
        private bool raycastTarget;
        private bool cullTransparentMesh;


        public string GetDescription()
        {
            return $"RawImage ";
        }


        public Info_RawImage(GameObject _obj)
        {
            this.rawImage = _obj.GetComponent<RawImage>();
            this.raycastTarget = rawImage.raycastTarget;
        }



        public void Render()
        {
            if (null == this.rawImage) return;

            GUI.color = this.rawImage.texture == null ? Color.red : Color.white;
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.ObjectField(this.rawImage.gameObject, typeof(GameObject), true, GUILayout.Width(120));
            EditorGUILayout.ObjectField(this.rawImage, typeof(RawImage), true, GUILayout.Width(100), GUILayout.Height(100));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(String.Format("Texture Name : {0}", this.rawImage.texture.name));

            EditorGUILayout.EndVertical();

            GUI.color = this.raycastTarget == false ? Color.white : Color.red;
            this.raycastTarget = EditorGUILayout.Toggle("RaycastTarget", this.raycastTarget, GUILayout.Width(180));
            GUI.color = Color.white;

            EditorGUILayout.Space();

            if (GUILayout.Button("Apply", GUILayout.Height(30), GUILayout.Width(100)))
            {
                this.rawImage.raycastTarget = this.raycastTarget;
            }

            EditorGUILayout.EndHorizontal();
        }

        public void SetFilter(bool[] _filter)
        {
            
        }
        public void SetCullTransparentMesh()
        {
            bool _bool = true;
            if (this.rawImage.gameObject.GetComponent<Selectable>() != null || this.rawImage.GetComponent<ScrollRect>() != null)
            {
                _bool = true; //버튼등의 조작가능한 컴포넌트가 있으면 강제로 켜둔다.
            }

            this.cullTransparentMesh = _bool;
            this.rawImage.GetComponent<CanvasRenderer>().cullTransparentMesh = this.cullTransparentMesh;
        }

        public void SetRaycastTarget()
        {
            bool _bool = true;
            if (this.rawImage.gameObject.GetComponent<Selectable>() != null || this.rawImage.GetComponent<ScrollRect>() != null)
            {
                _bool = true; //버튼등의 조작가능한 컴포넌트가 있으면 강제로 켜둔다.
            }

            this.raycastTarget = _bool;
            this.rawImage.raycastTarget = this.raycastTarget;
        }

        public void MatchString(string str)
        {
            //throw new NotImplementedException();
        }
    }
}
