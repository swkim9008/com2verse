using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Com2verseEditor.UnityAssetTool
{

    [System.Serializable]
    public class Info_Canvas : IObjInfo
    {
        public Canvas canvas;
        public RenderMode renderMode;
        public int sortOrder;
        public int targetDisplay;
        public Camera camera = null;

        public CanvasScaler scaler;
        public CanvasScaler.ScaleMode scaleMode;
        public float scaleFactor = -1;
        public Vector2 referenceResolution = new Vector2(0,0);
        public CanvasScaler.ScreenMatchMode matchMode;
        public float match;

        public GraphicRaycaster raycaster = null;
        public SerializedObject raycasterso = null;
        public LayerMask raycasterBlockMask;

        
        public string GetDescription()
        {
            return $"Canvas ";
        }


        public Info_Canvas(GameObject _obj)
        {
            this.canvas = _obj.GetComponent<Canvas>();

            this.renderMode = this.canvas.renderMode;
            this.sortOrder = this.canvas.sortingOrder;
            this.targetDisplay = this.canvas.targetDisplay;

            this.camera = this.canvas.worldCamera;

            this.scaler = _obj.GetComponent<CanvasScaler>();
            if (this.scaler != null)
            {
                this.scaleMode = this.scaler.uiScaleMode;

                this.scaleFactor = this.scaler.scaleFactor;
                this.referenceResolution = this.scaler.referenceResolution;

                this.matchMode = this.scaler.screenMatchMode;
                this.match = this.scaler.matchWidthOrHeight;
            }

            this.raycaster = _obj.GetComponent<GraphicRaycaster>();
            if (this.raycaster != null)
            {
                this.raycasterso = new SerializedObject(this.raycaster);
                this.raycasterBlockMask = this.raycaster.blockingMask;
            }
        }



        public void Render()
        {
            if (this.canvas == null) return;

            EditorGUILayout.BeginHorizontal("box");


            EditorGUILayout.Toggle(this.canvas.overrideSorting, GUILayout.Width(15));
            GUI.color = this.canvas.isRootCanvas ? Color.white : Color.grey;
            this.sortOrder = EditorGUILayout.IntField(this.sortOrder, GUILayout.Width(50));
            GUI.color = Color.white;
            EditorGUILayout.ObjectField(this.canvas.gameObject, typeof(GameObject), true, GUILayout.Width(200));

            //Render Mode
            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            this.renderMode = (RenderMode)EditorGUILayout.EnumPopup(this.renderMode);
            switch (this.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    {
                        //this.targetDisplay = EditorGUILayout.IntField("Target Display:", this.targetDisplay);
                    }
                    break;
                case RenderMode.ScreenSpaceCamera:
                    {
                        this.camera = (Camera)EditorGUILayout.ObjectField(this.camera, typeof(Camera), false);
                    }
                    break;
                case RenderMode.WorldSpace:
                    {
                        this.camera = (Camera)EditorGUILayout.ObjectField(this.camera, typeof(Camera), false);
                    }
                    break;
            }
            EditorGUILayout.EndVertical();

            //Scalar
            if (this.scaler != null)
            {
                EditorGUILayout.BeginVertical();
                this.scaleMode = (CanvasScaler.ScaleMode)EditorGUILayout.EnumPopup(this.scaleMode, GUILayout.Width(200));

                switch (this.scaleMode)
                {
                    case CanvasScaler.ScaleMode.ConstantPixelSize:
                        {
                            this.scaleFactor = EditorGUILayout.FloatField("Scale Factor", this.scaleFactor, GUILayout.Width(200));
                        }
                        break;
                    case CanvasScaler.ScaleMode.ScaleWithScreenSize:
                        {
                            this.referenceResolution = EditorGUILayout.Vector2Field("Reference Resolution", this.referenceResolution, GUILayout.Width(200));

                            this.matchMode = (CanvasScaler.ScreenMatchMode)EditorGUILayout.EnumPopup(this.matchMode, GUILayout.Width(200));

                            switch (this.matchMode)
                            {
                                case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
                                    {
                                        this.match = EditorGUILayout.FloatField(this.match, GUILayout.Width(200));
                                    }
                                    break;
                                case CanvasScaler.ScreenMatchMode.Expand:
                                    {
                                    }
                                    break;
                                case CanvasScaler.ScreenMatchMode.Shrink:
                                    {
                                    }
                                    break;
                            }
                        }
                        break;
                    case CanvasScaler.ScaleMode.ConstantPhysicalSize:
                        {
                        }
                        break;
                }

                if (!this.canvas.isRootCanvas)
                {
                    GUI.color = Color.yellow;
                    if (GUILayout.Button("Remove", GUILayout.Width(200)))
                    {
                        RemoveCanvasScaler();
                    }
                    GUI.color = Color.white;
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                GUILayout.Space(200);
            }


            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            this.raycasterso.Update();
            EditorGUILayout.PropertyField(this.raycasterso.FindProperty("m_BlockingMask"));
            this.raycasterso.ApplyModifiedProperties();

            GUI.color = Color.yellow;
            if (GUILayout.Button("Set Block Mask UI Only"))
            {
                this.raycaster.blockingMask = 1 << LayerMask.NameToLayer("UI");
            }
            GUI.color = Color.white;
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            
        }


        public void RemoveCanvasScaler()
        {
            
            string parentName = this.canvas.gameObject.transform.parent.name;

            if (parentName.Contains("ContentPage") || parentName.Contains("ContentPopup"))
            {
                if (this.scaler != null) GameObject.DestroyImmediate(this.scaler);
            };
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
