using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Com2verseEditor.UnityAssetTool
{
    [System.Serializable]
    public class Info_Collider : IObjInfo
    {
        public BoxCollider collider;

        public string GetDescription()
        {
            throw new NotImplementedException();
        }

        public Info_Collider(GameObject _obj)
        {
            this.collider = _obj.GetComponent<BoxCollider>();
        }

   
        public void Render()
        {
            if (null == this.collider) return;

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.ObjectField(this.collider.gameObject, typeof(GameObject), true, GUILayout.Width(120));
            EditorGUILayout.LabelField(String.Format("Size : {0} ,{1}", this.collider.size.x, this.collider.size.y));
            EditorGUILayout.LabelField(String.Format("Center : {0} ,{1}", this.collider.center.x, this.collider.center.y));

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
