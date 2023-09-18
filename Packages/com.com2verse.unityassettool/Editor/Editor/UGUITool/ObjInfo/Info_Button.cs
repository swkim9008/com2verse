using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace Com2verseEditor.UnityAssetTool
{
    [System.Serializable]
    public class Info_Button : IObjInfo, ISetRacacstTarget, ISetCullTransparentMesh
    {
        public Button button;
        public Graphic targetGraphic;

        public RectTransform rectTransform;
        public Selectable.Transition transition;
        public UnityEditor.Animations.AnimatorController animatorController = null;
        public bool interactable;
        
        private bool raycastTarget;
        private bool cullTransparentMesh;

        public string GetDescription()
        {
            return $"Button ";
        }

        public Info_Button(GameObject _obj)
        {
            this.button = _obj.GetComponent<Button>();
            this.transition = this.button.transition;

            this.rectTransform = this.button.targetGraphic == null ? _obj.GetComponent<RectTransform>() : this.button.targetGraphic.GetComponent<RectTransform>();

            this.interactable = this.button.interactable;
            
            this.targetGraphic = this.button.targetGraphic;
            this.raycastTarget = (this.targetGraphic != null && this.button.targetGraphic is MaskableGraphic) ? this.button.targetGraphic.raycastTarget : false;
        }


        public static void SetPivot(RectTransform target, Vector2 pivot)
        {
            if (!target) return;
            var offset = pivot - target.pivot;
            offset.Scale(target.rect.size);
            var wordlPos = target.position + target.TransformVector(offset);
            target.pivot = pivot;
            target.position = wordlPos;
        }


        public void Render()
        {
            if (null == this.button || null == this.targetGraphic) return;

            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.ObjectField(this.button.gameObject, typeof(GameObject), false, GUILayout.Width(200));
            EditorGUILayout.ObjectField(this.button.targetGraphic, typeof(GameObject), false, GUILayout.Width(200));
            EditorGUILayout.LabelField(String.Format("Size : {0} ,{1}", this.rectTransform.rect.width, this.rectTransform.rect.height), GUILayout.Width(150));

            if (GUILayout.Button("+", GUILayout.Height(20), GUILayout.Width(20)))
            {
                SetPivot(this.rectTransform, new Vector2(0.5f, 0.5f));
            }
            GUI.color = (this.rectTransform.pivot == new Vector2(0.5f, 0.5f)) ? Color.white : Color.red;
            EditorGUILayout.LabelField(String.Format("Pivot : {0} ,{1}", this.rectTransform.pivot.x, this.rectTransform.pivot.y), GUILayout.Width(150));

            GUI.color = this.interactable ? Color.white : Color.red;
            this.interactable = EditorGUILayout.ToggleLeft("Interactable", this.interactable, GUILayout.Width(120));
            GUI.color = this.raycastTarget == false ? Color.red : Color.white;
            this.raycastTarget = EditorGUILayout.ToggleLeft("RaycastTarget", this.raycastTarget, GUILayout.Width(120));
            GUI.color = Color.white;

            GUI.color = (this.transition == Selectable.Transition.None) ? Color.yellow : Color.white;
            this.transition = (Selectable.Transition)EditorGUILayout.EnumPopup(this.transition, GUILayout.Width(120));
            
            EditorGUILayout.Space(5);

            
            Sprite srcSprite = null;

            switch (this.transition)
            {
                case Selectable.Transition.None:
                    {
                    }
                    break;
                case Selectable.Transition.ColorTint:
                    {
                        EditorGUILayout.ObjectField(this.button.targetGraphic, typeof(GameObject), false);

                        if (this.button.targetGraphic.gameObject.GetComponent<Image>() == null) break;

                        srcSprite = this.button.targetGraphic.gameObject.GetComponent<Image>().sprite;

                        if (srcSprite == null)
                        {
                            GUI.color = Color.red;
                            EditorGUILayout.LabelField("Missing Sprite", GUILayout.Width(120));
                            GUI.color = Color.white;
                        }
                        else
                        {
                            EditorGUILayout.ObjectField(srcSprite.texture, typeof(GameObject), false);
                        }
                    }
                    break;
                case Selectable.Transition.SpriteSwap:
                    {
                        EditorGUILayout.ObjectField(this.button.targetGraphic, typeof(GameObject), false);

                        if (this.button.targetGraphic.gameObject.GetComponent<Image>() == null) break;

                        srcSprite = this.button.targetGraphic.gameObject.GetComponent<Image>().sprite;

                        if (srcSprite == null)
                        {
                            GUI.color = Color.red;
                            EditorGUILayout.LabelField("Missing Sprite", GUILayout.Width(120));
                            GUI.color = Color.white;
                        }
                        else
                        {
                            EditorGUILayout.ObjectField(srcSprite.texture, typeof(GameObject), false);
                        }
                    }
                    break;
                case Selectable.Transition.Animation:
                    {
                        if (this.button.animator == null)
                        {
                            GUI.color = Color.red;

                            if (GUILayout.Button("Add Animator", GUILayout.Height(30), GUILayout.Width(100)))
                            {
                                this.button.gameObject.AddComponent<Animator>();
                                this.button.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                                this.button.transition = this.transition;
                                this.button.interactable = this.interactable;
                            }
                        }
                        else
                        {
                            GUI.color = this.button.animator.runtimeAnimatorController == null ? Color.red : Color.white;
                            EditorGUILayout.ObjectField(this.button.animator.runtimeAnimatorController, typeof(GameObject), false);

                            if (GUILayout.Button("Common Ani", GUILayout.Height(30), GUILayout.Width(100)))
                            {
                                this.button.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                                this.button.animator.runtimeAnimatorController = (UnityEditor.Animations.AnimatorController)AssetDatabase.LoadAssetAtPath("Assets/_NBA/GUI/Animation/Button/Button_Common.controller", typeof(UnityEditor.Animations.AnimatorController));
                            }

                            if (GUILayout.Button("Menu Ani", GUILayout.Height(30), GUILayout.Width(100)))
                            {
                                this.button.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                                this.button.animator.runtimeAnimatorController = (UnityEditor.Animations.AnimatorController)AssetDatabase.LoadAssetAtPath("Assets/_NBA/GUI/Animation/Button/Button_Menu.controller", typeof(UnityEditor.Animations.AnimatorController));
                            }
                        }
                    }
                    break;
            }

            GUI.color = Color.white;

            EditorGUILayout.Space();

            if (GUILayout.Button("Apply", GUILayout.Height(30), GUILayout.Width(100)))
            {
                if (this.transition != Selectable.Transition.Animation)
                {
                    Animator tmpAnimatior = this.button.gameObject.GetComponent<Animator>();
                    if (tmpAnimatior != null)
                    {
                        UnityEngine.Object.DestroyImmediate(tmpAnimatior);
                    }
                }

                this.button.transition = this.transition;
                this.button.interactable = this.interactable;
                this.button.targetGraphic.raycastTarget = (this.button.targetGraphic != null && this.button.targetGraphic is MaskableGraphic) ? this.button.targetGraphic.raycastTarget : false; 
            }
            EditorGUILayout.EndHorizontal();
        }

        public void SetCullTransparentMesh()
        {
            this.cullTransparentMesh = true;
            this.targetGraphic.GetComponent<CanvasRenderer>().cullTransparentMesh = this.cullTransparentMesh;
        }

        public void SetRaycastTarget ()
        {
            if (this.targetGraphic == null) return;

            this.raycastTarget = true;
            this.targetGraphic.raycastTarget = this.raycastTarget;
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
