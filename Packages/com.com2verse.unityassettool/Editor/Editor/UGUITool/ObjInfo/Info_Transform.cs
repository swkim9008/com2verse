using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Com2verseEditor.UnityAssetTool
{

    [System.Serializable]
    public class Info_Transform : IObjInfo
    {
        public Transform transform;
        public int componentCount;

        public Info_Transform(GameObject _obj)
        {
            this.transform = _obj.transform;
            this.componentCount = _obj.transform.GetComponents<Component>().Length;
        }

        public string GetDescription()
        {
            return "Transform";
            //throw new NotImplementedException();
        }

        public void MatchString(string str)
        {
            //throw new NotImplementedException();
        }

        public void Render()
        {
            if (1 < this.componentCount) return;

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.ObjectField(this.transform.gameObject, typeof(GameObject), true, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();
        }

        public void SetFilter(bool[] _filter)
        {
            
        }
    }
}
