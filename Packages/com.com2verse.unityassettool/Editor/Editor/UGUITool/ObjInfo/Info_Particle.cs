using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Com2verseEditor.UnityAssetTool
{

    [System.Serializable]
    public class Info_Particle : IObjInfo
    {
        public ParticleSystem particleSystem;
        public GameObject rootObject;
        public string GetDescription()
        {
            throw new NotImplementedException();
        }
        public Info_Particle(GameObject _obj)
        {
            this.particleSystem = _obj.GetComponent<ParticleSystem>();
            this.rootObject = PrefabUtility.GetNearestPrefabInstanceRoot(_obj);
        }

  

        public void Render()
        {
            if (null == this.particleSystem) return;

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.ObjectField(this.particleSystem.gameObject, typeof(GameObject), true, GUILayout.Width(200));
            EditorGUILayout.ObjectField(rootObject, typeof(GameObject), true);
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
