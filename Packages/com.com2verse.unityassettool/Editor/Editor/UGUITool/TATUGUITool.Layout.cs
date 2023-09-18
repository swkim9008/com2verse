using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System;
using TMPro;

namespace Com2verseEditor.UnityAssetTool {

    public partial class TATUGUITool : EditorWindow
    {

        void UIGroup_Layout()
        {
            EditorGUILayout.LabelField("Control Sibling", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("UnLink", GUILayout.Height(65), GUILayout.Width(150))) { UnLink(); }
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Up Sibling", GUILayout.Height(30))) { ControlSibling(-1); }
            if (GUILayout.Button("Down Sibling", GUILayout.Height(30))) { ControlSibling(1); }
            EditorGUILayout.EndVertical();
            if (GUILayout.Button("Link", GUILayout.Height(65), GUILayout.Width(150))) { Link(); }
            EditorGUILayout.EndHorizontal();

            //EditorGUILayout.BeginVertical("box");
            //EditorGUILayout.LabelField("Control Anchor", EditorStyles.boldLabel);
            //UIGroup_ControlAnchor();
            //EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Create UI Templete", EditorStyles.boldLabel);
            UIGroup_WindowTemplete();
            EditorGUILayout.EndVertical();
        }


        void UIGroup_ControlAnchor()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Anchor Full", GUILayout.Height(100), GUILayout.Width(150))) { SetAnchor(Selection.activeGameObject,eAnchor.CENTER); };
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Head Area", GUILayout.Height(30))) { SetAnchor(Selection.activeGameObject, eAnchor.CENTER); };
            if (GUILayout.Button("SubPage Area", GUILayout.Height(30))) { SetAnchor(Selection.activeGameObject, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, -80.0f, new Vector2(0.5f, 0.5f)); };
            if (GUILayout.Button("Main Tab Area", GUILayout.Height(30))) { SetAnchor(Selection.activeGameObject, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, -160.0f, 0.0f, -80.0f, new Vector2(0.5f, 0.5f)); };
            if (GUILayout.Button("Set Button Anchor", GUILayout.Height(30))) { SetAnchor(Selection.activeGameObject, 0.0f, 0, 0.0f, 0, 1.0f, 0, 0.0f, 120, new Vector2(0.5f, 0.5f)); };
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }


        void UIGroup_WindowTemplete()
        {
            if (GUILayout.Button("Popup", GUILayout.Height(65), GUILayout.Width(150))) { CreatePopup(); }
            if (GUILayout.Button("Button YES", GUILayout.Height(65), GUILayout.Width(150))) { AddButton(Selection.activeGameObject,eButtonType.YES); }
            if (GUILayout.Button("Button Empty", GUILayout.Height(65), GUILayout.Width(150))) { AddButton(Selection.activeGameObject,eButtonType.EMPTY); }
        }


        //UIResourceScriptableObject uIPopupResourceScriptableObject = new UIResourceScriptableObject();

        void CreatePopup()
        {
        
            //this.uIPopupResourceScriptableObject = AssetDatabase.LoadAssetAtPath<UIResourceScriptableObject>("Assets/__Work/Editor/UGUITool/Layout/UIPopupResource.asset");

            GameObject target = Selection.activeGameObject;

            GameObject popup = AddChild("UI_Popup_Name", target, typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(Com2Verse.UI.UIView));
            SetAnchor(popup, eAnchor.CENTER);
            //popup.GetComponent<Com2Verse.UI.UIView>().ActiveTransitionType = Com2Verse.UI.GUIView.eActiveTransitionType.ANIMATION; //���Ƴ��� T^T

            GameObject button_CloseArea = AddChild("CloseArea", popup, typeof(Image), typeof(Button));
            AddImage(button_CloseArea, Image.Type.Simple, "Assets/Project/Bundles/04_UI/03_Mice/Textures_AAG/99_Common/UI_Circle_24.png", new Color32(255, 255, 255, 0));
            SetAnchor(button_CloseArea, eAnchor.CENTER);

            GameObject root = AddChild("Root", popup, typeof(RectTransform), typeof(Com2Verse.UI.AnimationPlayer));
            SetSize(root, 600, 400);
            //SetAnimationClip(root, "Assets/Project/Bundles/04_UI/03_Mice/Animations_AAG/Popup_Open_M.anim");
            //SetAnimationClip(root, "Assets/Project/Bundles/04_UI/03_Mice/Animations_AAG/Popup_Close_M.anim");

            GameObject content_BG = AddChild("Image_BG", root, typeof(Image));
            AddImage(content_BG, Image.Type.Sliced, "Assets/Project/Bundles/04_UI/03_Mice/Textures_AAG/99_Common/UI_Popup_System.png", new Color32(255, 255, 255, 255));
            SetAnchor(content_BG, 0.0f, 0.0f, 1.0f, 1.0f, -18.0f, -23.0f, 18.0f, 23.0f, new Vector2(0.5f, 0.5f));

            GameObject content_Head = AddChild("Content_Head", root, typeof(RectTransform));
            SetAnchor(content_Head, eAnchor.TOP);
            SetSize(content_Head, 0, 43f);
            
            GameObject content_Body = AddChild("Content_Body", root, typeof(RectTransform));
            SetAnchor(content_Body, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 90.0f, 0.0f, -43.0f, new Vector2(0.5f, 0.5f));
            

            GameObject content_Button = AddChild("Content_Button", root, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            SetAnchor(content_Button, eAnchor.BOTTOM);
            SetSize(content_Button, 0, 90f);
            HorizontalLayoutGroup buttonLayoutGroup = content_Button.GetComponent<HorizontalLayoutGroup>();
            buttonLayoutGroup.childForceExpandWidth = false;
            buttonLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            buttonLayoutGroup.spacing = 20;

            GameObject button_Close = AddChild("Button_Close", content_Head, typeof(RectTransform), typeof(Image), typeof(Button));
            AddImage(button_Close, Image.Type.Simple, "Assets/Project/Bundles/04_UI/03_Mice/Textures_AAG/99_Common/UI_Circle_24.png", new Color32(255, 255, 255, 255));
            SetAnchor(button_Close, eAnchor.TOP_RIGHT, false);
            SetSize(button_Close, 26, 26);
            SetPosition(button_Close, -25, -22);

            AddText(content_Head, "TITLE", new Color32(25, 25, 27, 255));

            GameObject icon_Close = AddChild("Icon_Close", button_Close, typeof(RectTransform), typeof(Image), typeof(Button));
            AddImage(icon_Close, Image.Type.Simple, "Assets/Project/Bundles/04_UI/03_Mice/Textures_AAG/Icon/UI_Icon_Close_10.png", new Color32(25, 25, 27, 255));
            SetAnchor(icon_Close, eAnchor.CENTER, false);
            SetSize(icon_Close, 14, 14);


            AddButton(content_Button, eButtonType.CANCEL);
            AddButton(content_Button, eButtonType.APPLY);

        }


        enum eButtonType { EMPTY, CANCEL, APPLY, CONFIRM, YES, NO}


        void AddButton(GameObject _parent, eButtonType _type)
        {
            switch (_type)
            {
                case eButtonType.EMPTY:
                    {
                        AddButton(_parent, "EMPTY", "Assets/Project/Bundles/04_UI/03_Mice/Textures_AAG/99_Common/UI_Btn_Default_Line_42.png", new Color32(255, 255, 255, 51), new Color32(25, 25, 27, 255));
                    }
                    break;
                case eButtonType.CANCEL:
                    {
                        AddButton(_parent, "CANCEL", "Assets/Project/Bundles/04_UI/03_Mice/Textures_AAG/99_Common/UI_Btn_Default_Line_42.png", new Color32(255, 255, 255, 51), new Color32(25, 25, 27, 255));
                    }
                    break;
                case eButtonType.NO:
                    {
                        AddButton(_parent, "NO", "Assets/Project/Bundles/04_UI/03_Mice/Textures_AAG/99_Common/UI_Btn_Default_Line_42.png", new Color32(255, 255, 255, 51), new Color32(25, 25, 27, 255));
                    }
                    break;
                case eButtonType.CONFIRM:
                    {
                        AddButton(_parent, "CONFIRM", "Assets/Project/Bundles/04_UI/03_Mice/Textures_AAG/99_Common/UI_Btn_Default_42.png", new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255));
                    }
                    break;
                case eButtonType.APPLY:
                    {
                        AddButton(_parent, "APPLY", "Assets/Project/Bundles/04_UI/03_Mice/Textures_AAG/99_Common/UI_Btn_Default_42.png", new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255));
                    }
                    break;
                case eButtonType.YES:
                    {
                        AddButton(_parent, "CANCEL", "Assets/Project/Bundles/04_UI/03_Mice/Textures_AAG/99_Common/UI_Btn_Default_42.png", new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255));
                    }
                    break;
            }
        }


        void AddButton(GameObject _parent, string _text, string _buttonImagePath, Color _buttonColor, Color _textColor)
        {
            if (_parent == null) return;
            GameObject button_Apply = AddChild($"Button_{_text}", _parent, typeof(RectTransform), typeof(Image), typeof(Button));

            AddImage(button_Apply, Image.Type.Sliced, _buttonImagePath, _buttonColor);
            SetAnchor(button_Apply, eAnchor.CENTER, false);
            SetSize(button_Apply, 142, 44);
    
            AddText(button_Apply, _text, _textColor);
        }

        


        void AddText(GameObject _parent, string _text, Color _color)
        {
            if (_parent == null) return;
            GameObject target = AddChild($"Text_{_text}", _parent, typeof(RectTransform));
            SetAnchor(target, eAnchor.CENTER);

            TextMeshProUGUI TMP_text = target.AddComponent<TextMeshProUGUI>();
            TMP_text.text = _text;
            TMP_text.color = _color;
            TMP_text.fontSize = 16;
            TMP_text.verticalAlignment = VerticalAlignmentOptions.Middle;
            TMP_text.horizontalAlignment = HorizontalAlignmentOptions.Center;
            TMP_text.raycastTarget = false;
        }



        void SetAnimationClip(GameObject _tagret, string _AnimationClipPath)
        {
            AnimationClip animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(_AnimationClipPath);

            Animation animation = _tagret.GetComponent<Animation>() ?? _tagret.AddComponent<Animation>();
            animation.AddClip(animationClip, Path.GetFileNameWithoutExtension(_AnimationClipPath));
        }



        void AddImage(GameObject _target, Image.Type _imageType, string _imagePath, Color _color)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(_imagePath);
            
            Image image = _target.GetComponent<Image>();
            image.sprite = sprite;
            image.type = _imageType;
            image.color = _color;
        }


        void AddImage(GameObject _target, Image.Type _imageType, Sprite _sprite, Color _color)
        {
            if (_sprite == null) return;

            Image image = _target.GetComponent<Image>();
            image.sprite = _sprite;
            image.type = _imageType;
            image.color = _color;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="_name"></param>
        /// <param name="_parent"></param>
        /// <param name="_componets"></param>
        /// <returns></returns>
        private GameObject AddChild(string _name, GameObject _parent, params Type[] _componets)
        {
            if (_parent == null)
            {
                Canvas tmpCanvas = FindObjectOfType<Canvas>() ?? new Canvas();
                _parent = tmpCanvas.gameObject;
            }

            GameObject newNode = new GameObject(_name, _componets);
            newNode.transform.SetParent(_parent.transform);
            //newNode.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            newNode.transform.localScale = Vector3.one;

            return newNode;
        }


        private Image AddImage(string _name, GameObject _parent, string _ImagePath)
        {
            if (_parent == null) return null;

            GameObject newNode = new GameObject(_name);
            newNode.transform.SetParent(_parent.transform);
            newNode.transform.localScale = Vector3.one;

            return newNode.AddComponent<Image>();
        }








        void ControlSibling(int _value)
        {
            GameObject gameObject = Selection.activeGameObject;
            if (null == gameObject) return;

            int newIndex = gameObject.transform.GetSiblingIndex() + _value;
            gameObject.transform.SetSiblingIndex(newIndex);
        }


        void UnLink()
        {
            GameObject[] gameObjectList = Selection.gameObjects;

            if (gameObjectList.Length <= 0) return;

            foreach (GameObject gameObject in gameObjectList)
            {
                if (null == gameObject.transform.parent) continue;

                int newIndex = gameObject.transform.parent.GetSiblingIndex() + 1;

                gameObject.transform.parent = gameObject.transform.parent.transform.parent;
                gameObject.transform.SetSiblingIndex(newIndex);
            }
        }


        void Link()
        {
            if (Selection.gameObjects.Length > 1) return;

            GameObject gameObject = Selection.activeGameObject;
            if (null == gameObject) return;

            if (gameObject.transform.GetSiblingIndex() == 0) return;
            if (gameObject.transform.parent.childCount <= 1) return;

            Transform newParent = gameObject.transform.parent.GetChild(gameObject.transform.GetSiblingIndex() - 1);

            gameObject.transform.parent = newParent;
            gameObject.transform.SetSiblingIndex(gameObject.transform.parent.GetSiblingIndex());
        }


        //void SetRectAchor(float _anchorMinX, float _anchorMinY, float _anchorMaxX, float _anchorMaxY, float _offsetMinX, float _offsetMinY, float _offsetMaxX, float _offsetMaxY)
        //{
        //    GameObject gameObject = Selection.activeGameObject;
        //    if (null == gameObject) return;

        //    RectTransform targetRect = gameObject.GetComponent<RectTransform>();
        //    if (null == targetRect) return;

        //    targetRect.anchorMin = new Vector2(_anchorMinX, _anchorMinY);
        //    targetRect.anchorMax = new Vector2(_anchorMaxX, _anchorMaxY);

        //    targetRect.offsetMin = new Vector2(_offsetMinX, _offsetMinY);
        //    targetRect.offsetMax = new Vector2(_offsetMaxX, _offsetMaxY);
        //    GameObject go = targetRect.parent.gameObject;
        //}

        void SetSize(GameObject target, float _width, float _height)
        {
            if (null == target) return;
            RectTransform targetRect = target.GetComponent<RectTransform>();
            if (null == targetRect) return;

            targetRect.sizeDelta = new Vector2(_width, _height);
        }


        void SetPosition(GameObject target, float _x, float _y)
        {
            if (null == target) return;
            RectTransform targetRect = target.GetComponent<RectTransform>();
            if (null == targetRect) return;
            targetRect.anchoredPosition3D = new Vector3(_x, _y, 0);
        }


        enum eAnchor { CENTER, TOP, TOP_LEFT, TOP_RIGHT, BOTTOM, BOTTOM_LEFT, BOTTOM_RIGHT, LEFT, RIGHT}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_target"></param>
        /// <param name="_anchorMinX"></param>
        /// <param name="_anchorMinY"></param>
        /// <param name="_anchorMaxX"></param>
        /// <param name="_anchorMaxY"></param>
        /// <param name="_offsetMinX"></param>
        /// <param name="_offsetMinY"></param>
        /// <param name="_offsetMaxX"></param>
        /// <param name="_offsetMaxY"></param>
        void SetAnchor(GameObject _target, float _anchorMinX, float _anchorMinY, float _anchorMaxX, float _anchorMaxY, float _offsetMinX, float _offsetMinY, float _offsetMaxX, float _offsetMaxY , Vector2 _Pivot)
        {
            if (null == _target) return;

            RectTransform targetRect = _target.GetComponent<RectTransform>();
            if (null == targetRect) return;

            targetRect.pivot = _Pivot;

            targetRect.anchorMin = new Vector2(_anchorMinX, _anchorMinY);
            targetRect.anchorMax = new Vector2(_anchorMaxX, _anchorMaxY);

            targetRect.offsetMin = new Vector2(_offsetMinX, _offsetMinY);
            targetRect.offsetMax = new Vector2(_offsetMaxX, _offsetMaxY);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_target"></param>
        /// <param name="_type"></param>
        /// <param name="_sizeDelta"></param>
        void SetAnchor(GameObject _target, eAnchor _type , bool _isFull = true)
        {
            if (null == _target) return;

            RectTransform targetRect = _target.GetComponent<RectTransform>();
            if (null == targetRect) return;

            float min = _isFull ? 0 : 0.5f;
            float max = _isFull ? 1 : 0.5f;

            switch (_type)
            {
                case eAnchor.CENTER:
                    {
                        targetRect.pivot = new Vector2(0.5f, 0.5f);
                        
                        targetRect.anchorMin = new Vector2(min, min);
                        targetRect.anchorMax = new Vector2(max, max);
                        targetRect.offsetMin = new Vector2(0, 0);
                        targetRect.offsetMax = new Vector2(0, 0);
                    };
                    break;

                case eAnchor.TOP_LEFT:
                    {
                        targetRect.pivot = new Vector2(0.5f, 0.5f);
                        targetRect.anchorMin = new Vector2(0, 1);
                        targetRect.anchorMax = new Vector2(0, 1);
                    };
                    break;
                case eAnchor.TOP:
                    {
                        targetRect.pivot = new Vector2(0.5f, 1);
                        targetRect.anchorMin = new Vector2(min, 1);
                        targetRect.anchorMax = new Vector2(max, 1);
                    };
                    break;
                case eAnchor.TOP_RIGHT:
                    {
                        targetRect.pivot = new Vector2(0.5f, 0.5f);
                        targetRect.anchorMin = new Vector2(1, 1);
                        targetRect.anchorMax = new Vector2(1, 1);
                    };
                    break;
                case eAnchor.BOTTOM_LEFT:
                    {
                        targetRect.pivot = new Vector2(0.5f, 0.5f);
                        targetRect.anchorMin = new Vector2(0, 0);
                        targetRect.anchorMax = new Vector2(0, 0);
                    };
                    break;
                case eAnchor.BOTTOM:
                    {
                        targetRect.pivot = new Vector2(0.5f, 0);
                        targetRect.anchorMin = new Vector2(min, 0);
                        targetRect.anchorMax = new Vector2(max, 0);
                    };
                    break;
                case eAnchor.BOTTOM_RIGHT:
                    {
                        targetRect.pivot = new Vector2(0.5f, 0.5f);
                        targetRect.anchorMin = new Vector2(1, 0);
                        targetRect.anchorMax = new Vector2(1, 0);
                    };
                    break;
                case eAnchor.LEFT:
                    {
                        targetRect.pivot = new Vector2(0, 0.5f);
                        targetRect.anchorMin = new Vector2(0, min);
                        targetRect.anchorMax = new Vector2(0, max);
                    };
                    break;
                case eAnchor.RIGHT:
                    {
                        targetRect.pivot = new Vector2(0, 1.0f);
                        targetRect.anchorMin = new Vector2(1, min);
                        targetRect.anchorMax = new Vector2(1, max);
                    };
                    break;
            }
        }
    }
}



