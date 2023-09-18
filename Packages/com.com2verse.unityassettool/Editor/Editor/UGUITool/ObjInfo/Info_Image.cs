using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityEngine.UIElements;

namespace Com2verseEditor.UnityAssetTool
{
    [System.Serializable]
    public class Info_Image : IObjInfo, ISetRacacstTarget, ISetCullTransparentMesh
    {
        Image image;
        public string atlasName = "None";
        
        private Color32 imageColor;
        private Material material;
        private Texture texture;

        private bool raycastTarget;
        private bool cullTransparentMesh;

        //private bool isUI;

        public string GetDescription()
        {
            return $"Image ";
        }

        public Info_Image(GameObject _obj)
        {
            Image tmpImage = _obj.GetComponent<Image>();

            this.image = tmpImage;
            this.imageColor = tmpImage.color;
            this.material = tmpImage.material;
            this.raycastTarget = tmpImage.raycastTarget;

            this.texture = tmpImage.sprite != null ? tmpImage.sprite.texture : null;

            this.cullTransparentMesh = _obj.GetComponent<CanvasRenderer>().cullTransparentMesh;

            ////현재 노드에 Selectable 또는 ScrollRect가 있을 경우 확인
            //this.isUI = (_obj.GetComponent<Selectable>() != null) || (_obj.GetComponent<ScrollRect>() != null);

            //부모 노드에 Selectable 또는 ScrollRect가 있을 경우 확인
            //Selectable tmpParentSelectable = _obj.GetComponentInParent<Selectable>();
            
            //if (null != tmpParentSelectable && this.image == tmpParentSelectable.targetGraphic)
            //{
            //    this.isUI = true;
            //}

            //if (null != tmpParentSelectable && tmpParentSelectable is Toggle toggle)
            //{
            //    if (this.image == ((Toggle)tmpParentSelectable).graphic)
            //    {
            //        this.isUI = true;
            //    };
            //}

        }


        public void Render()
        {
            if (null == this.image) return;
            
            EditorGUILayout.BeginHorizontal("box");

            //GUI.color = this.isUI ? Color.cyan : Color.white;
            EditorGUILayout.ObjectField(this.image.gameObject, typeof(GameObject), true, GUILayout.Width(200));

            GUI.color = this.texture == null ? Color.red : Color.white;
            EditorGUILayout.ObjectField(this.texture, typeof(Image), true, GUILayout.Width(120));
            GUI.color = Color.white;

            this.imageColor = EditorGUILayout.ColorField(this.imageColor, GUILayout.Width(50));
            EditorGUILayout.Space(20,false);

            GUI.color = this.imageColor.a > 0 ? Color.white : Color.yellow;
            EditorGUILayout.FloatField((float)this.imageColor.a, GUILayout.Width(50));
            //EditorGUILayout.ToggleLeft("Alpha 0", this.imageColor.a == 0, GUILayout.Width(100));
            GUI.color = Color.white;

            EditorGUILayout.LabelField(this.material.name);

            GUI.color = this.raycastTarget == false ? Color.white : Color.yellow;
            this.raycastTarget = EditorGUILayout.ToggleLeft("RaycastTarget", this.raycastTarget, GUILayout.Width(150));
            GUI.color = Color.white;

            GUI.color = this.cullTransparentMesh == false ? Color.white : Color.yellow;
            this.cullTransparentMesh = EditorGUILayout.ToggleLeft("Cull Transparent Mesh", this.cullTransparentMesh, GUILayout.Width(180));
            GUI.color = Color.white;
            

            //if (null != this.image.sprite)
            //{
            //    EditorGUILayout.LabelField(String.Format("Packed : {0}", this.image.sprite.packed), GUILayout.Width(120));
            //}

            EditorGUILayout.Space();

            if (GUILayout.Button("Apply", GUILayout.Height(30), GUILayout.Width(100)))
            {
                this.image.color = this.imageColor;

                SetRaycastTarget();
                SetCullTransparentMesh();
            }

            EditorGUILayout.EndHorizontal();
        }



        private bool[] filter;
        public void SetFilter(bool[] _filter)
        {
            this.filter = _filter;
        }

        public void SetCullTransparentMesh()
        {
            bool _bool = true;

            if (this.image.GetComponent<Selectable>() != null || this.image.GetComponent<ScrollRect>() != null)
            {
                _bool = true; //버튼등의 조작가능한 컴포넌트가 있으면 강제로 켜둔다.
            }

            this.cullTransparentMesh = _bool;
            this.image.GetComponent<CanvasRenderer>().cullTransparentMesh = this.cullTransparentMesh;
        }


        public void SetRaycastTarget()
        {
            this.raycastTarget = false;

            //본인 또는 부모가 조작 가능한 컴포넌트인경우
            Selectable myBehaviour = this.image.GetComponent<Selectable>();
            if (myBehaviour == null)
            {
                myBehaviour = this.image.gameObject.GetComponentInParent<Selectable>();
            }


            if (myBehaviour != null)
            {
                if (myBehaviour is Button && ((Button)myBehaviour).targetGraphic != null)
                {
                    this.raycastTarget = ((Button)myBehaviour).targetGraphic == this.image; 
                }
                else if (myBehaviour is Toggle && ((Toggle)myBehaviour).targetGraphic != null)
                {
                    this.raycastTarget = ((Toggle)myBehaviour).targetGraphic == this.image;
                }
                else if (myBehaviour is Slider && ((Slider)myBehaviour).interactable && ((Slider)myBehaviour).handleRect != null)
                {
                    this.raycastTarget = ((Slider)myBehaviour).handleRect == this.image.rectTransform;
                }
            }
            else
            {
                this.raycastTarget = this.image.GetComponent<ScrollRect>() != null;
            }

            this.image.raycastTarget = this.raycastTarget;
        }

        public void MatchString(string str)
        {
            //throw new NotImplementedException();
        }
    }
}



//////////////////////////////////////////

//if (this.image.GetComponent<ScrollRect>() != null)
//{
//    this.image.raycastTarget = true;
//    this.raycastTarget = true;
//    return;
//}


//if (uiBehaviour == null)
//{
//    this.raycastTarget = false;
//    this.image.raycastTarget = this.raycastTarget;
//    return;
//}



//if (uiBehaviour != null && uiBehaviour.targetGraphic != null && uiBehaviour.targetGraphic is MaskableGraphic)
//{
//    uiBehaviour.targetGraphic.raycastTarget = true;
//    this.raycastTarget = true;
//    return;
//}
//else if (this.image.GetComponent<ScrollRect>() != null)
//{
//    this.image.raycastTarget = true;
//    this.raycastTarget = true;
//    return;
//}
//else
//{
//    this.image.raycastTarget = false;
//    this.raycastTarget = false;
//}




////슬라이더일경우 확인
//if (target is Slider)
//{
//    var slider = (Slider)target;

//    if (slider.interactable == false)
//    {
//        this.image.raycastTarget = false;
//        this.raycastTarget = false;
//    } else
//    {
//        MaskableGraphic handleMaskableGraphic  = slider.handleRect.GetComponent<MaskableGraphic>();
//        if (handleMaskableGraphic != null) handleMaskableGraphic.raycastTarget = true;
//    }
//    return;
//}