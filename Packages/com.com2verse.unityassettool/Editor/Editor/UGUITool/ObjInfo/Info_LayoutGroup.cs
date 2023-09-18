using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using Unity.VisualScripting;

namespace Com2verseEditor.UnityAssetTool
{
    [System.Serializable]
    public class Info_LayoutGroup : IObjInfo
    {
        public LayoutGroup layoutGroup;
        public ContentSizeFitter contentSizeFitter;

        private bool[] filter;
        public void SetFilter(bool[] _filter) { this.filter = _filter; }

        public string GetDescription()
        {
            return $"Layout Group ";
        }

        public Info_LayoutGroup(GameObject _obj)
        {
            this.layoutGroup = _obj.GetComponent<LayoutGroup>();
            if (this.layoutGroup == null ) { return; }
            this.contentSizeFitter = _obj.GetComponent<ContentSizeFitter>();
        }

        

        public void Render()
        {
            if (null == this.layoutGroup) return;
            if (!this.filter[0] && this.layoutGroup is VerticalLayoutGroup) return;
            if (!this.filter[1] && this.layoutGroup is HorizontalLayoutGroup) return;
            if (!this.filter[2] && this.layoutGroup is GridLayoutGroup) return;

            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.ObjectField(this.layoutGroup.gameObject, typeof(GameObject), false, GUILayout.Width(200));

            if (this.layoutGroup is HorizontalLayoutGroup)
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField("HorizontalLayoutGroup", GUILayout.Width(150));

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Control Child Size", GUILayout.Width(150));
                EditorGUILayout.LabelField("Use Child Scale", GUILayout.Width(150));
                EditorGUILayout.LabelField("Child Force Expand", GUILayout.Width(150));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.ToggleLeft("Width", ((HorizontalLayoutGroup)this.layoutGroup).childControlWidth, GUILayout.Width(80));
                EditorGUILayout.ToggleLeft("Width", ((HorizontalLayoutGroup)this.layoutGroup).childScaleWidth, GUILayout.Width(80));
                EditorGUILayout.ToggleLeft("Width", ((HorizontalLayoutGroup)this.layoutGroup).childForceExpandWidth, GUILayout.Width(80));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.ToggleLeft("Height", ((HorizontalLayoutGroup)this.layoutGroup).childControlHeight, GUILayout.Width(80));
                EditorGUILayout.ToggleLeft("Height", ((HorizontalLayoutGroup)this.layoutGroup).childScaleHeight, GUILayout.Width(80));
                EditorGUILayout.ToggleLeft("Height", ((HorizontalLayoutGroup)this.layoutGroup).childForceExpandHeight, GUILayout.Width(80));
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(50);

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Spacing", GUILayout.Width(150));
                EditorGUILayout.LabelField("Child Alignment", GUILayout.Width(150));
                EditorGUILayout.LabelField("Reverse Arrangement", GUILayout.Width(150));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.FloatField(((HorizontalLayoutGroup)this.layoutGroup).spacing, GUILayout.Width(150));
                EditorGUILayout.EnumPopup(((HorizontalLayoutGroup)this.layoutGroup).childAlignment, GUILayout.Width(150));
                EditorGUILayout.Toggle(((HorizontalLayoutGroup)this.layoutGroup).reverseArrangement, GUILayout.Width(150));
                EditorGUILayout.EndVertical();

                
            }
            else if (this.layoutGroup is VerticalLayoutGroup)
            {
                GUI.color = Color.cyan;
                EditorGUILayout.LabelField("VerticalLayoutGroup", GUILayout.Width(150));

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Control Child Size", GUILayout.Width(150));
                EditorGUILayout.LabelField("Use Child Scale", GUILayout.Width(150));
                EditorGUILayout.LabelField("Child Force Expand", GUILayout.Width(150));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.ToggleLeft("Width",((VerticalLayoutGroup)this.layoutGroup).childControlWidth, GUILayout.Width(80));
                EditorGUILayout.ToggleLeft("Width", ((VerticalLayoutGroup)this.layoutGroup).childScaleWidth, GUILayout.Width(80));
                EditorGUILayout.ToggleLeft("Width", ((VerticalLayoutGroup)this.layoutGroup).childForceExpandWidth, GUILayout.Width(80));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.ToggleLeft("Height", ((VerticalLayoutGroup)this.layoutGroup).childControlHeight, GUILayout.Width(80));
                EditorGUILayout.ToggleLeft("Height", ((VerticalLayoutGroup)this.layoutGroup).childScaleHeight, GUILayout.Width(80));
                EditorGUILayout.ToggleLeft("Height", ((VerticalLayoutGroup)this.layoutGroup).childForceExpandHeight, GUILayout.Width(80));
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(50);

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Spacing", GUILayout.Width(150));
                EditorGUILayout.LabelField("Child Alignment", GUILayout.Width(150));
                EditorGUILayout.LabelField("Reverse Arrangement", GUILayout.Width(150));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.FloatField(((VerticalLayoutGroup)this.layoutGroup).spacing, GUILayout.Width(150));
                EditorGUILayout.EnumPopup(((VerticalLayoutGroup)this.layoutGroup).childAlignment, GUILayout.Width(150));
                EditorGUILayout.Toggle(((VerticalLayoutGroup)this.layoutGroup).reverseArrangement, GUILayout.Width(150));
                EditorGUILayout.EndVertical();
            }
            else if (this.layoutGroup is GridLayoutGroup)
            {
                EditorGUILayout.LabelField("GridLayoutGroup", GUILayout.Width(150));

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Cell Size", GUILayout.Width(150));
                EditorGUILayout.LabelField("Spacing", GUILayout.Width(150));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.Vector2Field("", ((GridLayoutGroup)this.layoutGroup).cellSize, GUILayout.Width(160));
                EditorGUILayout.Vector2Field("", ((GridLayoutGroup)this.layoutGroup).spacing, GUILayout.Width(160));
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(50);

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Start Corner", GUILayout.Width(150));
                EditorGUILayout.LabelField("Start Axis", GUILayout.Width(150));
                EditorGUILayout.LabelField("Child Alignment", GUILayout.Width(150));
                EditorGUILayout.LabelField("Constraint", GUILayout.Width(150));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.EnumPopup(((GridLayoutGroup)this.layoutGroup).startCorner, GUILayout.Width(150));
                EditorGUILayout.EnumPopup(((GridLayoutGroup)this.layoutGroup).startAxis, GUILayout.Width(150));
                EditorGUILayout.EnumPopup(((GridLayoutGroup)this.layoutGroup).childAlignment, GUILayout.Width(150));
                EditorGUILayout.EnumPopup(((GridLayoutGroup)this.layoutGroup).constraint, GUILayout.Width(150));
                EditorGUILayout.EndVertical();

            }
            
            
            if (this.contentSizeFitter != null)
            {
                //EditorGUILayout.LabelField("Content Size Fitter");
                EditorGUILayout.Space(50);

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Horizontal Fit", GUILayout.Width(100));
                EditorGUILayout.LabelField("Vertical Fit", GUILayout.Width(100));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                GUI.color = this.layoutGroup is HorizontalLayoutGroup && this.contentSizeFitter.horizontalFit == ContentSizeFitter.FitMode.Unconstrained ? Color.yellow : Color.white;
                EditorGUILayout.EnumPopup(this.contentSizeFitter.horizontalFit, GUILayout.Width(150));
                GUI.color = this.layoutGroup is VerticalLayoutGroup && this.contentSizeFitter.verticalFit == ContentSizeFitter.FitMode.Unconstrained ? Color.yellow : Color.white;
                EditorGUILayout.EnumPopup(this.contentSizeFitter.verticalFit, GUILayout.Width(150));
                EditorGUILayout.EndVertical();
            }

            GUI.color = Color.white;

            GUILayout.FlexibleSpace();
            
            //EditorGUILayout.Space();
            //if (CheckIsEmptyOption()) 
            //{
            //    GUI.color = Color.red;
            //    if (GUILayout.Button("Remove", GUILayout.Height(30), GUILayout.Width(100)))
            //    {
            //        GameObject.DestroyImmediate(this.layoutGroup);
            //        if(this.contentSizeFitter != null)
            //        { 
            //            GameObject.DestroyImmediate(this.contentSizeFitter);
            //        }
            //    }
            //    GUI.color = Color.white;
            //}
            EditorGUILayout.EndHorizontal();
        }
        

        bool CheckIsEmptyOption()
        {
            if (this.layoutGroup is HorizontalLayoutGroup)
            {
                if (
                    ((HorizontalLayoutGroup)this.layoutGroup).childControlWidth ||
                    ((HorizontalLayoutGroup)this.layoutGroup).childScaleWidth ||
                    ((HorizontalLayoutGroup)this.layoutGroup).childForceExpandWidth ||
                    ((HorizontalLayoutGroup)this.layoutGroup).childControlHeight ||
                    ((HorizontalLayoutGroup)this.layoutGroup).childScaleHeight ||
                    ((HorizontalLayoutGroup)this.layoutGroup).childForceExpandHeight
                    ) { return false; }
            }
            else if (this.layoutGroup is VerticalLayoutGroup)
            {
                if (

                    ((VerticalLayoutGroup)this.layoutGroup).childControlWidth ||
                    ((VerticalLayoutGroup)this.layoutGroup).childScaleWidth ||
                    ((VerticalLayoutGroup)this.layoutGroup).childForceExpandWidth ||
                    ((VerticalLayoutGroup)this.layoutGroup).childControlHeight ||
                    ((VerticalLayoutGroup)this.layoutGroup).childScaleHeight ||
                    ((VerticalLayoutGroup)this.layoutGroup).childForceExpandHeight
                    ) { return false; }

            }
            else if (this.layoutGroup is GridLayoutGroup) { return false; }
            
            return true;
        }

        public void MatchString(string str)
        {
            //throw new NotImplementedException();
        }
    }
}





//EditorGUILayout.LabelField(String.Format("Size : {0} ,{1}", this.rectTransform.rect.width, this.rectTransform.rect.height), GUILayout.Width(150));

//if (GUILayout.Button("+", GUILayout.Height(20), GUILayout.Width(20)))
//{
//    SetPivot(this.rectTransform, new Vector2(0.5f, 0.5f));
//}
//GUI.color = (this.rectTransform.pivot == new Vector2(0.5f, 0.5f)) ? Color.white : Color.red;
//EditorGUILayout.LabelField(String.Format("Pivot : {0} ,{1}", this.rectTransform.pivot.x, this.rectTransform.pivot.y), GUILayout.Width(150));

//GUI.color = this.interactable ? Color.white : Color.red;
//this.interactable = EditorGUILayout.ToggleLeft("Interactable", this.interactable, GUILayout.Width(120));
//GUI.color = this.raycastTarget == false ? Color.red : Color.white;
//this.raycastTarget = EditorGUILayout.ToggleLeft("RaycastTarget", this.raycastTarget, GUILayout.Width(120));
//GUI.color = Color.white;

//GUI.color = (this.transition == Selectable.Transition.None) ? Color.yellow : Color.white;
//this.transition = (Selectable.Transition)EditorGUILayout.EnumPopup(this.transition, GUILayout.Width(120));

//EditorGUILayout.Space(5);


//Sprite srcSprite = null;

//switch (this.transition)
//{
//    case Selectable.Transition.None:
//        {
//        }
//        break;
//    case Selectable.Transition.ColorTint:
//        {
//            EditorGUILayout.ObjectField(this.button.targetGraphic, typeof(GameObject), false);

//            if (this.button.targetGraphic.gameObject.GetComponent<Image>() == null) break;

//            srcSprite = this.button.targetGraphic.gameObject.GetComponent<Image>().sprite;

//            if (srcSprite == null)
//            {
//                GUI.color = Color.red;
//                EditorGUILayout.LabelField("Missing Sprite", GUILayout.Width(120));
//                GUI.color = Color.white;
//            }
//            else
//            {
//                EditorGUILayout.ObjectField(srcSprite.texture, typeof(GameObject), false);
//            }
//        }
//        break;
//    case Selectable.Transition.SpriteSwap:
//        {
//            EditorGUILayout.ObjectField(this.button.targetGraphic, typeof(GameObject), false);

//            if (this.button.targetGraphic.gameObject.GetComponent<Image>() == null) break;

//            srcSprite = this.button.targetGraphic.gameObject.GetComponent<Image>().sprite;

//            if (srcSprite == null)
//            {
//                GUI.color = Color.red;
//                EditorGUILayout.LabelField("Missing Sprite", GUILayout.Width(120));
//                GUI.color = Color.white;
//            }
//            else
//            {
//                EditorGUILayout.ObjectField(srcSprite.texture, typeof(GameObject), false);
//            }
//        }
//        break;
//    case Selectable.Transition.Animation:
//        {
//            if (this.button.animator == null)
//            {
//                GUI.color = Color.red;

//                if (GUILayout.Button("Add Animator", GUILayout.Height(30), GUILayout.Width(100)))
//                {
//                    this.button.gameObject.AddComponent<Animator>();
//                    this.button.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
//                    this.button.transition = this.transition;
//                    this.button.interactable = this.interactable;
//                }
//            }
//            else
//            {
//                GUI.color = this.button.animator.runtimeAnimatorController == null ? Color.red : Color.white;
//                EditorGUILayout.ObjectField(this.button.animator.runtimeAnimatorController, typeof(GameObject), false);

//                if (GUILayout.Button("Common Ani", GUILayout.Height(30), GUILayout.Width(100)))
//                {
//                    this.button.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
//                    this.button.animator.runtimeAnimatorController = (UnityEditor.Animations.AnimatorController)AssetDatabase.LoadAssetAtPath("Assets/_NBA/GUI/Animation/Button/Button_Common.controller", typeof(UnityEditor.Animations.AnimatorController));
//                }

//                if (GUILayout.Button("Menu Ani", GUILayout.Height(30), GUILayout.Width(100)))
//                {
//                    this.button.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
//                    this.button.animator.runtimeAnimatorController = (UnityEditor.Animations.AnimatorController)AssetDatabase.LoadAssetAtPath("Assets/_NBA/GUI/Animation/Button/Button_Menu.controller", typeof(UnityEditor.Animations.AnimatorController));
//                }
//            }
//        }
//        break;
//}

//GUI.color = Color.white;

//EditorGUILayout.Space();

//if (GUILayout.Button("Apply", GUILayout.Height(30), GUILayout.Width(100)))
//{
//    if (this.transition != Selectable.Transition.Animation)
//    {
//        Animator tmpAnimatior = this.button.gameObject.GetComponent<Animator>();
//        if (tmpAnimatior != null)
//        {
//            UnityEngine.Object.DestroyImmediate(tmpAnimatior);
//        }
//    }

//    this.button.transition = this.transition;
//    this.button.interactable = this.interactable;
//    this.button.targetGraphic.raycastTarget = (this.button.targetGraphic != null && this.button.targetGraphic is MaskableGraphic) ? this.button.targetGraphic.raycastTarget : false; 
//}