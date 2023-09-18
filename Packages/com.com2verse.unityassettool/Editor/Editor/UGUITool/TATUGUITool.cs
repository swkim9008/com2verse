using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System;
using System.Linq;
using Unity.VisualScripting;
using TMPro;
using UnityEditor.SceneManagement;
using Com2Verse;
using NUnit.Framework.Internal;
//using ;

namespace Com2verseEditor.UnityAssetTool
{

    public partial class TATUGUITool : EditorWindow
    {

        [MenuItem("Com2Verse/ART/UGUI Tool &#1")] //핫키 alt+shift+1
        public static void Initialize()
        {
            TATUGUITool guiToolWindow = (TATUGUITool)EditorWindow.GetWindow(typeof(TATUGUITool));
            guiToolWindow.Show();
        }

        private List<IObjInfo> objInfoList = new List<IObjInfo>();
        private Vector2 objInfoListScrollViewPos;
        private Vector2 LocalizationListScrollViewPos;

        private int childcount = 0;         //선택된 노드의 자식 노드 숫자

        private float progressBar;
        private bool progressCancel = false;

        private int menuTab = 0;
        private int oldMenuTab = -1;

        private int labelDepth = 500;
        private int spriteDepth = 100;
        private Sprite targetSprite;
        static string selectedSprite = string.Empty;


        private void OnEnable() { Repaint(); }
        void OnInspectorUpdate() { Repaint(); }

        

        void OnGUI()
        {
            try
            {
                //this.menuTab = GUILayout.Toolbar(this.menuTab, new string[] { "Edit", "Layout Control", "Collider", "ADD UI" });
                this.menuTab = GUILayout.Toolbar(this.menuTab, new string[] { "Edit", "Layout Control", "UI Object Info", "Prefab Work", "C2V Object Info" });

                if (this.oldMenuTab != this.menuTab)
                {
                    this.objInfoList.Clear();

                    this.oldMenuTab = this.menuTab;
                }

                switch (this.menuTab)
                {
                    case 0:
                        {
                            EditorGUILayout.LabelField("Game Object", EditorStyles.boldLabel);
                            EditorGUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField("Rename Game Object", EditorStyles.boldLabel);
                            UIGroup_Rename();
                            GUILayout.Space(10);
                            
                            if (GUILayout.Button("Child GameObject Active", GUILayout.Height(30))) { SetGameObjectActive(true); }
                            EditorGUILayout.EndVertical();
                            GUILine();
                            GUILayout.Space(10);

                            EditorGUILayout.LabelField("Sprite", EditorStyles.boldLabel);
                            EditorGUILayout.BeginVertical("box");
                            UIGroup_Sprite();
                            EditorGUILayout.EndVertical();

                            GUILine();
                            
                            GUILayout.Space(10);
                            EditorGUILayout.LabelField("Text", EditorStyles.boldLabel);
                            EditorGUILayout.BeginVertical("box");
                            //UIGroup_Text();
                            UIGroup_TextMeshPro();
                            EditorGUILayout.EndVertical();
                            
                            EditorGUILayout.EndFoldoutHeaderGroup();

                            //EditorGUILayout.Separator();
                            //EditorGUILayout.LabelField("Create Custom Font", EditorStyles.boldLabel);
                            //EditorGUILayout.BeginVertical("box");
                            ////UIGroup_CreateCustomFont();
                            //EditorGUILayout.EndVertical();
                            //EditorGUILayout.BeginVertical();
                            //EditorGUILayout.EndVertical();
                            GUILine();
                            GUILayout.Space(10);
                            EditorGUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField("Remove Component", EditorStyles.boldLabel);
                            UIGroup_CleanUp();

                            GUILine();
                            if (GUILayout.Button("Spirte Import Setting", GUILayout.Height(60))) { GUISpriteImporteSetting(); }

                            GUILine();
                            if (GUILayout.Button("COPY", GUILayout.Height(60))) { TEST(); }
                            if (GUILayout.Button("PASTE", GUILayout.Height(60))) { TEST2(); }


                            EditorGUILayout.EndVertical();
                            break;
                        }
                    case 1:
                        {
                            EditorGUILayout.BeginVertical("box");
                            UIGroup_Layout();
                            EditorGUILayout.EndVertical();
                            break;
                        }
                    case 2:
                        {
                            EditorGUILayout.BeginVertical("box");
                            UIGroup_ObjInfo();
                            EditorGUILayout.EndVertical();
                            break;
                        }
                    case 3:
                        {
                            EditorGUILayout.BeginVertical("box");
                            UIGroup_MakePrefabs();
                            EditorGUILayout.EndVertical();
                            break;
                        }
                    case 4:
                        {
                            EditorGUILayout.BeginVertical("box");
                            UIGroup_Localization();




                            EditorGUILayout.EndVertical();
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
            }
        }


        private string fromString;
        private string toString;

        void UIGroup_Rename()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("R", GUILayout.Height(30), GUILayout.Width(30))) { this.fromString = String.Empty; }
            EditorGUILayout.LabelField("From Text:", GUILayout.Width(80));
            this.fromString = EditorGUILayout.TextField(this.fromString);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("R", GUILayout.Height(30), GUILayout.Width(30))) { this.toString = String.Empty; }
            EditorGUILayout.LabelField("To Text:", GUILayout.Width(80));
            this.toString = EditorGUILayout.TextField(this.toString);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUI.color = Color.cyan;
            //if (GUILayout.Button("Rename All", GUILayout.Height(30))) { ReNameAll(this.fromString, this.toString); }

            if (GUILayout.Button("Rename", GUILayout.Height(60))) { ReNameChildOnly(this.fromString, this.toString, GetActiveObject()); }
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        void UIGroup_CleanUp()
        {
            GUI.color = Color.red;
            if (GUILayout.Button("Remove Binder", GUILayout.Height(30))) { RemoveComponent<Com2Verse.UI.Binder>(); }
            if (GUILayout.Button("Remove GUIView", GUILayout.Height(30))) { RemoveComponent<Com2Verse.UI.GUIView>(); }
            if (GUILayout.Button("Remove Localization UI", GUILayout.Height(30))) { RemoveComponent<Com2Verse.UI.LocalizationUI>(); }
            if (GUILayout.Button("Remove GameObjectPropertyExtensions", GUILayout.Height(30))) { RemoveComponent<Com2Verse.UI.GameObjectPropertyExtensions>(); }

            GUI.color = Color.white;
        }

        SpriteMatchingTableScriptableObject spriteMatchingTableScriptableObject = null;

        void UIGroup_Sprite()
        {
            EditorGUILayout.LabelField($"Hierarchy에서 Sprite 찾기");
            EditorGUILayout.BeginHorizontal();
            this.targetSprite = (Sprite)EditorGUILayout.ObjectField(this.targetSprite, typeof(Sprite), false, GUILayout.Width(GetSpriteSize(this.targetSprite).x), GUILayout.Height(GetSpriteSize(this.targetSprite).y));

            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            EditorGUILayout.LabelField($"{GetSpriteName(this.targetSprite)}");
            if (GUILayout.Button("현재 선택된 GameOjbect의 Sprite 선택", GUILayout.Height(30))) { GetSpriteBySelecedImage(); }
            if (GUILayout.Button("동일한 Sprite를 사용한 Image 선택", GUILayout.Height(30))) { SelectSpriteImage(this.targetSprite); }
            //if (GUILayout.Button("동일한 Sprite를 사용한 Button 선택", GUILayout.Height(30))) { SelectTargetSpriteButton(this.targetSprite); }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("매칭 테이블을 이용하여 Sprite 교체");
            EditorGUILayout.BeginHorizontal();
            this.spriteMatchingTableScriptableObject = (SpriteMatchingTableScriptableObject)EditorGUILayout.ObjectField(this.spriteMatchingTableScriptableObject, typeof(SpriteMatchingTableScriptableObject), false, GUILayout.Height(30));
            if (GUILayout.Button("설정된 Sprite 교체", GUILayout.Height(30))) { ChangeSpriteFromMatchingTable(); }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUI.color = Color.yellow;
            if (GUILayout.Button("Sprite Missing Image 선택", GUILayout.Height(30))) { SelectMissingSpriteImage(); }
            GUI.color = Color.white;
        }

        private void ChangeSpriteFromMatchingTable()
        {
            GameObject rootObj = Selection.activeObject as GameObject;
            if (null == rootObj) return;

            Image[] targetImageList = rootObj.GetComponentsInChildren<Image>(true);

            foreach (Image targetImage in targetImageList)
            {
                foreach (SpritePair spritePair in this.spriteMatchingTableScriptableObject.sprites)
                {
                    if (spritePair.fromSprite == null || spritePair.toSprite == null) continue;

                    if (targetImage.sprite == spritePair.fromSprite)
                    {
                        targetImage.sprite = spritePair.toSprite;
                    }
                }
            }
        }




        private string GetSpriteName(Sprite _sprite) { return _sprite == null ? "찾고자 하는 스프라이트를 선택해 주세요." : _sprite.name; }


        private Vector2 GetSpriteSize(Sprite _sprite)
        {
            if (_sprite == null) return new Vector2(128, 128);
            float aspectW = _sprite.textureRect.height / _sprite.textureRect.width;
            float aspectH = _sprite.textureRect.width / _sprite.textureRect.height;
            float viewSize = 128;

            Vector2 size = aspectW < 1.0 ? new Vector2(viewSize, viewSize * aspectW) : new Vector2(viewSize * aspectH, viewSize);
            if (size.x < 30 || size.y < 30) size *= 2;

            return size;
        }


        private void GetSpriteBySelecedImage()
        {
            GameObject tmpObj = Selection.activeObject as GameObject;
            if (null == tmpObj) return;

            Image tmpImage = tmpObj.GetComponent<Image>();

            this.targetSprite = tmpImage?.sprite ?? null;
        }





        void GUISpriteImporteSetting()
        {
            string[] assetGUIDs = Selection.assetGUIDs;

            foreach (string assetGUID in assetGUIDs)
            {
                string imageFullPath = AssetDatabase.GUIDToAssetPath(assetGUID);

                TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(imageFullPath);
                if (!textureImporter) return;

                //Debug.Log($"IMPORT!!! Texture:{assetPath}");
                textureImporter.textureType = TextureImporterType.Sprite;
                
                textureImporter.mipmapEnabled = false;

                TextureImporterSettings textureImporterSettings = new TextureImporterSettings();
                textureImporter.ReadTextureSettings(textureImporterSettings);

                textureImporterSettings.spriteMeshType = SpriteMeshType.FullRect;
                textureImporterSettings.spriteGenerateFallbackPhysicsShape = false;
                textureImporterSettings.spriteExtrude = 0;
                textureImporterSettings.readable = false;
                textureImporterSettings.wrapMode = TextureWrapMode.Clamp;
                textureImporterSettings.alphaIsTransparency = true;
                textureImporter.SetTextureSettings(textureImporterSettings);

                textureImporter.SaveAndReimport();
            }
        }

        //void UIGroup_ControlAnchor()
        //{
        //    EditorGUILayout.BeginHorizontal();
        //    if (GUILayout.Button("Set Anchor Full", GUILayout.Height(100), GUILayout.Width(150))) { SetRectAchor(0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f);};
        //    EditorGUILayout.BeginVertical();
        //    if (GUILayout.Button("Head Area", GUILayout.Height(30))) { SetRectAchor(0.0f, 1.0f, 1.0f, 1.0f, 0.0f, -80.0f, 0.0f, 0.0f); };
        //    if (GUILayout.Button("SubPage Area", GUILayout.Height(30))) { SetRectAchor(0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, -80.0f); };
        //    if (GUILayout.Button("Main Tab Area", GUILayout.Height(30))) { SetRectAchor(0.0f, 1.0f, 1.0f, 1.0f, 0.0f, -160.0f, 0.0f, -80.0f); };
        //    if (GUILayout.Button("Set Button Anchor", GUILayout.Height(30))) { SetRectAchor(0.0f, 0, 0.0f, 0, 1.0f, 0, 0.0f, 120); };
        //    EditorGUILayout.EndVertical();
        //    EditorGUILayout.EndHorizontal();
        //}


        void GUILine(int lineHeight = 1)
        {
            EditorGUILayout.Space();
            Rect rect = EditorGUILayout.GetControlRect(false, lineHeight);
            rect.height = lineHeight;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space();
        }
        
        
        public Vector3[] vertices;


        void TEST()
        {
            GameObject tmpObj = Selection.activeObject as GameObject;
            if (null == tmpObj) return;

            Mesh mesh = tmpObj.GetComponent<MeshFilter>().mesh;
            if (null == mesh) return;
            vertices = mesh.vertices;
            
            var bottom = vertices.ToList().Where(pos => pos.y < 0).OrderBy(pos => pos.x).ToList();
            var top = vertices.ToList().Where(pos => pos.y > 0).OrderByDescending(pos => pos.x).ToList();

            List<Vector3> sortedList = new List<Vector3>();
            sortedList.AddRange(bottom);
            sortedList.AddRange(top);

            List<Vector3> finalList = new List<Vector3>();
            foreach (Vector3 pos in sortedList)
            {
                 finalList.Add(tmpObj.transform.TransformPoint(pos));
            }


            foreach (Vector3 v in finalList)
            {
                Debug.Log(v);
            }
        }

        void TEST2()
        {
            foreach (Vector3 v in vertices)
            {
                Debug.Log(v);
            }


            GameObject tmpObj = Selection.activeObject as GameObject;
            if (null == tmpObj) return;

            //UILineRendererSimple mesh = tmpObj.GetComponent<UILineRendererSimple>();
            //if (null == mesh) return;
        }

    }
}















/*
       [System.Serializable]
public class ObjInfo<T>
{
    GameObject _obj;
    public ObjInfo(GameObject _obj,T _type)
    {

    }
}
*/

//void TabCollider()
//{
//    EditorGUILayout.BeginHorizontal();

//    if (GUILayout.Button("Select Collider", GUILayout.Height(30)))
//    {
//        GetColliderInfoFromSelection();
//    }

//    GUI.backgroundColor = Color.red;
//    if (GUILayout.Button("Reset", GUILayout.Height(30), GUILayout.Width(50)))
//    {
//        this.colliderInfoList.Clear();
//    };
//    GUI.backgroundColor = Color.white;
//    EditorGUILayout.EndHorizontal();

//    this.colliderInfoScrollViewPos = EditorGUILayout.BeginScrollView(this.colliderInfoScrollViewPos);       //Scroll View Start
//    foreach (Info_Collider colInfo in colliderInfoList)
//    {
//        EditorGUILayout.BeginHorizontal("box");
//        EditorGUILayout.ObjectField(colInfo.gameObject, typeof(Collider), true);
//        EditorGUILayout.BeginVertical();
//        GUILayout.Label(string.Format("Center : {0}, {1}, {2}", colInfo.center.x, colInfo.center.y, colInfo.center.z));
//        GUILayout.Label(string.Format("Size : {0}, {1}, {2}", colInfo.size.x, colInfo.size.y, colInfo.size.z));
//        EditorGUILayout.EndVertical();
//        EditorGUILayout.EndHorizontal();
//    }
//    EditorGUILayout.EndScrollView();

//}

//private Vector2 colliderInfoScrollViewPos;
//private List<Info_Collider> colliderInfoList = new List<Info_Collider>();



// Collider ------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------

//void GetColliderInfoFromSelection()
//{
//    this.colliderInfoList.Clear();

//    GameObject rootObj = Selection.activeObject as GameObject;

//    if (null == rootObj) return;

//    Collider[] targetColliderList = rootObj.GetComponentsInChildren<Collider>(true);

//    foreach (Collider col in targetColliderList)
//    {
//        Info_Collider curCol = new Info_Collider(col);

//        this.colliderInfoList.Add(curCol);
//    }
//}

/*
    //void GetFromSelection<T>(T _type)
    void FindFromSelection<T>()
    {
        this.findObjList.Clear();
        GameObject rootObj = Selection.activeObject as GameObject;
        if (null == rootObj) return;

        //var targetList = rootObj.GetComponentsInChildren(typeof(T));


        var targetList = rootObj.GetComponentsInChildren(typeof(T));

        foreach (var obj in targetList)
        {
            //string _type = typeof(T).Name;
            Debug.Log(">>>" + typeof(T));

 
           // this.findObjList.Add(obj.gameObject);
        }
    }
    */


//void SelectSameAtlasNode()
//{
//    List<GameObject> gameObjectList = new List<GameObject>();
//    gameObjectList.Clear();

//    GameObject rootObj = Selection.activeObject as GameObject;
//    if (null == rootObj) return;

//    UISprite[] targetSpriteList = rootObj.GetComponentsInChildren<UISprite>(true);

//    foreach (UISprite mSprite in targetSpriteList)
//    {
//        if (null == mSprite.atlas) continue;

//        if (NGUISettings.atlas.Equals(mSprite.atlas))
//        {
//            gameObjectList.Add(mSprite.gameObject);
//        }
//    }
//    Selection.objects = gameObjectList.ToArray();
//}


//void SelectSameSpriteNode()
//{
//    List<GameObject> gameObjectList = new List<GameObject>();
//    gameObjectList.Clear();

//    GameObject rootObj = Selection.activeObject as GameObject;
//    if (null == rootObj) return;

//    UISprite[] targetSpriteList = rootObj.GetComponentsInChildren<UISprite>(true);

//    foreach (UISprite mSprite in targetSpriteList)
//    {
//        if (null == mSprite.atlas) continue;

//        if (NGUISettings.atlas.Equals(mSprite.atlas) && selectedSprite == mSprite.spriteName)
//        {
//            gameObjectList.Add(mSprite.gameObject);
//        }
//    }
//    Selection.objects = gameObjectList.ToArray();
//}



////NGUI Code Start      UICreateWidgetWizard.cs -------------------------------------------------
//#region
//void OnSelectAtlas(UnityEngine.Object obj) {
//    // Legacy atlas support
//    if (obj != null && obj is GameObject) obj = (obj as GameObject).GetComponent<UIAtlas>();

//    if (NGUISettings.atlas != obj as INGUIAtlas)
//    {
//        NGUISettings.atlas = obj as INGUIAtlas;

//        Repaint();
//    }
//    OnSelectSprite(selectedSprite);
//}

//static void SaveString(string field, string val)
//{
//    if (string.IsNullOrEmpty(val))
//    {
//        EditorPrefs.DeleteKey(field);
//    }
//    else
//    {
//        EditorPrefs.SetString(field, val);
//    }
//}


//void OnSelectSprite(string val)
//{
//    selectedSprite = val;
//    SaveString("NGUI Button", selectedSprite);

//    Repaint();
//}


////static public bool ShouldCreate(GameObject go, bool isValid) {
////    GUI.color = isValid ? Color.green : Color.grey;

////    GUILayout.BeginHorizontal();
////    bool retVal = GUILayout.Button("Add To", GUILayout.Width(76f));
////    GUI.color = Color.white;
////    GameObject sel = EditorGUILayout.ObjectField(go, typeof(GameObject), true, GUILayout.Width(140f)) as GameObject;
////    GUILayout.Label("Select the parent in the Hierarchy View", GUILayout.MinWidth(10000f));
////    GUILayout.EndHorizontal();

////    if (sel != go) Selection.activeGameObject = sel;

////    if (retVal && isValid)
////    {
////        NGUIEditorTools.RegisterUndo("Add a Widget");
////        return true;
////    }
////    return false;
////}

////void CreateSprite(GameObject go) {
////    if (NGUISettings.atlas != null)
////    {
////        NGUIEditorTools.DrawSpriteField("Sprite", "Sprite that will be created", NGUISettings.atlas, NGUISettings.selectedSprite, OnSprite, GUILayout.Width(120f));

////        if (!string.IsNullOrEmpty(NGUISettings.selectedSprite))
////        {
////            GUILayout.BeginHorizontal();
////            NGUISettings.pivot = (UIWidget.Pivot)EditorGUILayout.EnumPopup("Pivot", NGUISettings.pivot, GUILayout.Width(200f));
////            GUILayout.Space(20f);
////            GUILayout.Label("Initial pivot point used by the sprite");
////            GUILayout.EndHorizontal();
////        }
////    }

////    if (ShouldCreate(go, NGUISettings.atlas != null))
////    {
////        Selection.activeGameObject = NGUISettings.AddSprite(go).gameObject;
////    }
////}


////void OnSprite(string val) {
////    if (NGUISettings.selectedSprite != val)
////    {
////        NGUISettings.selectedSprite = val;
////        Repaint();
////    }
////}


//#endregion
////NGUI Code End        UICreateWidgetWizard.cs -------------------------------------------------



//void TabLabelDepth()
//{
//    GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
//    boxStyle.normal.textColor = Color.gray;

//    EditorGUILayout.BeginHorizontal();
//    labelDepth = EditorGUILayout.IntField("Label Depth", labelDepth, GUILayout.Width(200));

//    EditorGUILayout.BeginVertical();
//    EditorGUILayout.BeginHorizontal();
//    GUILayout.Box("모든 하위 Label", boxStyle, GUILayout.Width(100));
//    if (GUILayout.Button("Set Depth", GUILayout.Height(30))) { SetLabelDepth(labelDepth, false, false); }
//    if (GUILayout.Button("Offset Depth", GUILayout.Height(30))) { SetLabelDepth(labelDepth, true, false); }
//    EditorGUILayout.EndHorizontal();

//    EditorGUI.BeginDisabledGroup(null == currentFont);
//    EditorGUILayout.BeginHorizontal();
//    GUILayout.Box("cur 폰트 Label", boxStyle, GUILayout.Width(100));
//    if (GUILayout.Button("Set Depth", GUILayout.Height(30))) { SetLabelDepth(labelDepth, false, true); }
//    if (GUILayout.Button("Offset Depth", GUILayout.Height(30))) { SetLabelDepth(labelDepth, true, true); }
//    EditorGUILayout.EndHorizontal();
//    EditorGUI.EndDisabledGroup();
//    EditorGUILayout.EndVertical();

//    EditorGUILayout.EndHorizontal();
//}



//void SetLabelDepth(int _depth, bool _isOffset, bool _isSameFont)
//{
//    GameObject target = Selection.activeObject as GameObject;

//    if (target)
//    {
//        Text[] targetTextList = target.GetComponentsInChildren<Text>(true);

//        foreach (Text text in targetTextList)
//        {
//            if (_isSameFont)
//            {
//                if (text.font == currentFont)
//                {
//                    if (_isOffset)
//                    {
//                        //text.depth = text.depth + _depth;
//                    }
//                    else
//                    {
//                        //text.depth = _depth;
//                    }
//                }
//            }
//            else
//            {

//                if (_isOffset)
//                {
//                    //text.depth = text.depth + _depth;
//                }
//                else
//                {
//                    //text.depth = _depth;
//                }
//            }
//        }
//    }
//}



//public interface IObjInfo {
//    void Render();
//    //void SetBool(bool _bool);
//}
//Info Class
//-----------------------------------------------------------------------------------------------------------
//[System.Serializable]
//public class Info_Image : IObjInfo
//{
//    Image image;
//    public string atlasName = "None";
//    string state = "Error";

//    public Info_Image(GameObject _obj)
//    {
//        this.image = _obj.GetComponent<Image>();
//        this.state = this.image.sprite == null ? "Error" : "OK";
//    }


//    public void Render()
//    {
//        GUI.color = this.state == "Error" ? Color.red : Color.white;
//        EditorGUILayout.BeginHorizontal("box");
//        EditorGUILayout.ObjectField(this.image.gameObject, typeof(GameObject), true, GUILayout.Width(120));

//        if (null != this.image.sprite)
//        {
//            EditorGUILayout.ObjectField(this.image.sprite.texture, typeof(Image), true, GUILayout.Width(120));
//            EditorGUILayout.LabelField(String.Format("Packed : {0}", this.image.sprite.packed), GUILayout.Width(120));
//        }
//        EditorGUILayout.EndHorizontal();
//    }
//}

//[System.Serializable]
//public class Info_RawImage : IObjInfo
//{
//    RawImage rawImage;

//    public Info_RawImage(GameObject _obj)
//    {
//        this.rawImage = _obj.GetComponent<RawImage>();
//    }

//    public void Render()
//    {
//        GUI.color = this.rawImage.texture == null ? Color.red : Color.white;
//        EditorGUILayout.BeginHorizontal("box");
//        EditorGUILayout.ObjectField(this.rawImage.gameObject, typeof(GameObject), true, GUILayout.Width(120));
//        EditorGUILayout.ObjectField(this.rawImage, typeof(RawImage), true, GUILayout.Width(100), GUILayout.Height(100));
//        EditorGUILayout.BeginVertical();
//        EditorGUILayout.LabelField(String.Format("Texture Name : {0}", this.rawImage.texture.name));

//        EditorGUILayout.EndVertical();
//        EditorGUILayout.EndHorizontal();
//    }
//}

//[System.Serializable]
//public class Info_UIText : IObjInfo
//{
//    public Text text;
//    public bool raycastTarget;
//    public float lineSpacing;
//    public Color32 fontColor;
//    public HorizontalWrapMode horizontalWrapMode;
//    public VerticalWrapMode verticalWrapMode;
//    //public UILocalization uILocalization;
//    public bool convertToLocalFonts;

//    public Info_UIText(GameObject _obj)
//    {
//        this.text = _obj.GetComponent<Text>();
//        this.raycastTarget = this.text.raycastTarget;
//        this.fontColor = this.text.color;
//        this.horizontalWrapMode = this.text.horizontalOverflow;
//        this.verticalWrapMode = this.text.verticalOverflow;

//        //지워야징
//        //this.uILocalization = _obj.GetComponent<UILocalization>();

//        //if (null != this.uILocalization)
//        //{ 
//        //    this.convertToLocalFonts = this.uILocalization.convertToLocalFonts;
//        //}
//    }

//    public void SetRayCastTarget(bool _bool)
//    {
//        this.raycastTarget = _bool;
//        this.text.raycastTarget = this.raycastTarget;
//    }

//    public void ResetLineSpacing()
//    {
//        this.lineSpacing = 1.0f;
//        this.text.lineSpacing = this.lineSpacing;
//    }

//    public void SetVerticalOverFlow(VerticalWrapMode _verticalWrapMode)
//    {
//        this.verticalWrapMode = _verticalWrapMode;
//        this.text.verticalOverflow = this.verticalWrapMode;
//    }

//    //public void SetConvertToLocalFont(bool _bool)
//    //{
//    //    if (this.uILocalization == null) return;
//    //    this.convertToLocalFonts = _bool;
//    //    this.uILocalization.convertToLocalFonts = this.convertToLocalFonts;
//    //}


//    public void Render()
//    {
//        if (null == this.text) return;

//        GUI.color = this.text.font == null ? Color.red : Color.white;
//        EditorGUILayout.BeginHorizontal("box");
//        EditorGUILayout.ObjectField(this.text.gameObject, typeof(GameObject), true, GUILayout.Width(120));
//        EditorGUILayout.ObjectField(this.text.font, typeof(Font), true, GUILayout.Width(120));
//        this.fontColor = EditorGUILayout.ColorField(this.fontColor, GUILayout.Width(50));
//        EditorGUILayout.LabelField(String.Format("Font Size : {0}", this.text.fontSize), GUILayout.Width(120));

//        //if (this.horizontalWrapMode != HorizontalWrapMode.Overflow) { GUI.color = Color.yellow; }

//        GUI.color = this.horizontalWrapMode != HorizontalWrapMode.Overflow ? Color.cyan : Color.green;
//        this.horizontalWrapMode = (HorizontalWrapMode)EditorGUILayout.EnumPopup("Horizontal Overflow", this.horizontalWrapMode, GUILayout.Width(250));
//        GUI.color = Color.white;

//        if (this.verticalWrapMode != VerticalWrapMode.Overflow) { GUI.color = Color.yellow; }
//        this.verticalWrapMode = (VerticalWrapMode)EditorGUILayout.EnumPopup("Vertical Overflow", this.verticalWrapMode, GUILayout.Width(250));
//        GUI.color = Color.white;

//        GUI.color = this.text.lineSpacing == 1.0f ? Color.white : Color.red;
//        EditorGUILayout.LabelField(String.Format("Line Spacing : {0}", this.text.lineSpacing), GUILayout.Width(150));
//        GUI.color = Color.white;

//        GUI.color = this.raycastTarget == false ? Color.white : Color.red;
//        this.raycastTarget = EditorGUILayout.Toggle("RaycastTarget",this.raycastTarget, GUILayout.Width(180));
//        GUI.color = Color.white;

//        //if (this.uILocalization != null) { 
//        //    this.convertToLocalFonts = EditorGUILayout.Toggle("convertLocalFont", this.convertToLocalFonts, GUILayout.Width(150));
//        //}

//        EditorGUILayout.Space();

//        if (GUILayout.Button("Apply", GUILayout.Height(30), GUILayout.Width(100)))
//        {
//            this.text.raycastTarget = this.raycastTarget;
//            this.text.horizontalOverflow = this.horizontalWrapMode;
//            this.text.verticalOverflow = this.verticalWrapMode;
//            //this.uILocalization.convertToLocalFonts = this.convertToLocalFonts;
//        }

//        EditorGUILayout.EndHorizontal();
//    }
//}


//[System.Serializable]
//public class Info_Collider : IObjInfo
//{
//    public BoxCollider collider;

//    public Info_Collider(GameObject _obj)
//    {
//         this.collider = _obj.GetComponent<BoxCollider>();
//    }

//    public void Render()
//    {
//        EditorGUILayout.BeginHorizontal("box");
//        EditorGUILayout.ObjectField(this.collider.gameObject, typeof(GameObject), true, GUILayout.Width(120));
//        EditorGUILayout.LabelField(String.Format("Size : {0} ,{1}", this.collider.size.x, this.collider.size.y));
//        EditorGUILayout.LabelField(String.Format("Center : {0} ,{1}", this.collider.center.x, this.collider.center.y));

//        EditorGUILayout.EndHorizontal();
//    }
////}

//[System.Serializable]
//public class Info_UIButton : IObjInfo
//{
//    public Button button;
//    public RectTransform rectTransform;
//    public Selectable.Transition transition;
//    public UnityEditor.Animations.AnimatorController animatorController = null;
//    public bool interactable;

//    public Info_UIButton(GameObject _obj)
//    {
//        this.button = _obj.GetComponent<Button>();
//        this.transition = this.button.transition;
//        this.rectTransform = _obj.GetComponent<RectTransform>();
//        this.interactable = this.button.interactable;
//    }

//    public static void SetPivot(RectTransform target, Vector2 pivot)
//    {
//        if (!target) return;
//        var offset = pivot - target.pivot;
//        offset.Scale(target.rect.size);
//        var wordlPos = target.position + target.TransformVector(offset);
//        target.pivot = pivot;
//        target.position = wordlPos;
//    }

//    public void Render()
//    {
//        if (null == this.button) return;

//        EditorGUILayout.BeginHorizontal("box");

//        EditorGUILayout.ObjectField(this.button.gameObject, typeof(GameObject), true, GUILayout.Width(120));
//        EditorGUILayout.LabelField(String.Format("Size : {0} ,{1}", this.rectTransform.rect.width, this.rectTransform.rect.height), GUILayout.Width(150));

//        if (GUILayout.Button("+", GUILayout.Height(20), GUILayout.Width(20)))
//        {
//            SetPivot(this.rectTransform, new Vector2(0.5f, 0.5f));
//        }
//        GUI.color = (this.rectTransform.pivot == new Vector2(0.5f, 0.5f)) ? Color.white : Color.red;
//        EditorGUILayout.LabelField(String.Format("Pivot : {0} ,{1}", this.rectTransform.pivot.x, this.rectTransform.pivot.y), GUILayout.Width(150));

//        GUI.color = Color.white;

//        this.interactable = EditorGUILayout.Toggle("Interactable :", this.interactable, GUILayout.Width(180));
//        GUI.color = (this.transition == Selectable.Transition.None) ? Color.yellow : Color.white;
//        this.transition = (Selectable.Transition)EditorGUILayout.EnumPopup(this.transition, GUILayout.Width(120));

//        Sprite srcSprite = null;

//        switch (this.transition)
//        {
//            case Selectable.Transition.None:
//                break;
//            case Selectable.Transition.ColorTint:
//                EditorGUILayout.ObjectField(this.button.targetGraphic, typeof(GameObject), true, GUILayout.Width(120));

//                srcSprite = this.button.targetGraphic.gameObject.GetComponent<Image>().sprite;
//                if (srcSprite == null)
//                {
//                    GUI.color = Color.red;
//                    EditorGUILayout.LabelField("Missing Sprite", GUILayout.Width(120));
//                    GUI.color = Color.white;
//                }
//                else
//                {
//                    EditorGUILayout.ObjectField(srcSprite.texture, typeof(GameObject), true, GUILayout.Width(120));
//                }
//                break;
//            case Selectable.Transition.SpriteSwap:
//                EditorGUILayout.ObjectField(this.button.targetGraphic, typeof(GameObject), true, GUILayout.Width(120));

//                srcSprite = this.button.targetGraphic.gameObject.GetComponent<Image>().sprite;
//                if (srcSprite == null)
//                {
//                    GUI.color = Color.red;
//                    EditorGUILayout.LabelField("Missing Sprite", GUILayout.Width(120));
//                    GUI.color = Color.white;
//                }
//                else
//                {
//                    EditorGUILayout.ObjectField(srcSprite.texture, typeof(GameObject), true, GUILayout.Width(120));
//                }
//                break;
//            case Selectable.Transition.Animation:

//                if (this.button.animator == null)
//                {
//                    GUI.color = Color.red;

//                    if (GUILayout.Button("Add Animator", GUILayout.Height(30), GUILayout.Width(100)))
//                    {
//                        this.button.gameObject.AddComponent<Animator>();
//                        this.button.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
//                        this.button.transition = this.transition;
//                        this.button.interactable = this.interactable;
//                    }
//                }
//                else
//                {
//                    GUI.color = this.button.animator.runtimeAnimatorController == null ? Color.red : Color.white;
//                    EditorGUILayout.ObjectField(this.button.animator.runtimeAnimatorController, typeof(GameObject), true, GUILayout.Width(120));

//                    if (GUILayout.Button("Main Ani", GUILayout.Height(30), GUILayout.Width(100)))
//                    {
//                        this.button.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
//                        this.button.animator.runtimeAnimatorController = (UnityEditor.Animations.AnimatorController)AssetDatabase.LoadAssetAtPath("Assets/__Work/GUI/Animation/ButtonAni_Common.controller", typeof(UnityEditor.Animations.AnimatorController));
//                    }

//                    if (GUILayout.Button("Sub Ani", GUILayout.Height(30), GUILayout.Width(100)))
//                    {
//                        this.button.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
//                        this.button.animator.runtimeAnimatorController = (UnityEditor.Animations.AnimatorController)AssetDatabase.LoadAssetAtPath("Assets/__Work/GUI/Animation/ButtonAni_Sub.controller", typeof(UnityEditor.Animations.AnimatorController));
//                    }
//                }
//                break;
//        }

//        GUI.color = Color.white;

//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.EndHorizontal();

//        if (GUILayout.Button("Apply", GUILayout.Height(30), GUILayout.Width(100)))
//        {
//            if (this.transition != Selectable.Transition.Animation)
//            {
//                Animator tmpAnimatior = this.button.gameObject.GetComponent<Animator>();
//                if (tmpAnimatior != null) { DestroyImmediate(tmpAnimatior); }
//            }

//            this.button.transition = this.transition;
//            this.button.interactable = this.interactable;
//        }
//        EditorGUILayout.EndHorizontal();
//    }
//}

//    [System.Serializable]
//public class Info_Particle : IObjInfo
//{
//    public ParticleSystem particleSystem;
//    public GameObject rootObject;

//    public Info_Particle(GameObject _obj)
//    {
//        this.particleSystem = _obj.GetComponent<ParticleSystem>();
//        this.rootObject = PrefabUtility.GetNearestPrefabInstanceRoot(_obj);
//    }

//    public void Render()
//    {
//        EditorGUILayout.BeginHorizontal("box");
//        EditorGUILayout.ObjectField(this.particleSystem.gameObject, typeof(GameObject), true, GUILayout.Width(200));
//        EditorGUILayout.ObjectField(rootObject, typeof(GameObject), true);
//        EditorGUILayout.EndHorizontal();
//    }

////}

//[System.Serializable]
//public class Info_Transform : IObjInfo
//{
//    public Transform transform;
//    public int componentCount;

//    public Info_Transform(GameObject _obj)
//    {
//        this.transform = _obj.transform;
//        this.componentCount = _obj.transform.GetComponents<Component>().Length;
//    }


//    public void Render()
//    {
//        if (1 < this.componentCount) return;

//        EditorGUILayout.BeginHorizontal("box");
//        EditorGUILayout.ObjectField(this.transform.gameObject, typeof(GameObject), true, GUILayout.Width(200));
//        EditorGUILayout.EndHorizontal();
//    }
//}

//[System.Serializable]
//public class Info_UIRect : IObjInfo
//{
//    public RectTransform rectTransform;
//    public bool isAnchored;

//    public Info_UIRect(GameObject _obj)
//    {
//        this.rectTransform = _obj.GetComponent<RectTransform>();
//    }

//    public void Render()
//    {
//        EditorGUILayout.BeginHorizontal("box");
//        EditorGUILayout.ObjectField(rectTransform.gameObject, typeof(GameObject), true, GUILayout.Width(200)); //혹시나 모르지만 두개가 붙어 있으면 안됀다.
//        EditorGUILayout.Toggle("isAnchored", this.isAnchored);
//        EditorGUILayout.EndHorizontal();
//    }
//}

//[System.Serializable]
//public class Info_UIScrollRect : IObjInfo
//{
//    public ScrollRect scrollRect;
//    public ScrollRect.MovementType movementType;
//    public bool useHorizontal;
//    public bool useVertical;
//    public RectTransform content;

//    public RectMask2D rectMask2D;
//    public bool useRectMask2D = false;

//    public ContentSizeFitter contentSizeFitter;
//    public ContentSizeFitter.FitMode horizontalFit;
//    public ContentSizeFitter.FitMode verticalFit;

//    public HorizontalOrVerticalLayoutGroup layoutGroup;
//    public TextAnchor childAligement;
//    public bool controlChildSizeWidth;
//    public bool controlChildSizeHeight;
//    public bool useChildScaleWidth;
//    public bool useChildScaleHeight;
//    public bool childForceExpandWidth;
//    public bool childForceExpandHeight;

//    public Info_UIScrollRect(GameObject _obj)
//    {
//        this.scrollRect = _obj.GetComponent<ScrollRect>();
//        this.rectMask2D = _obj.GetComponent<RectMask2D>();

//        if (null != this.rectMask2D)
//        {
//            this.useRectMask2D = this.rectMask2D.enabled;
//        }

//        this.useHorizontal = this.scrollRect.horizontal;
//        this.useVertical = this.scrollRect.vertical;
//        this.movementType = this.scrollRect.movementType;
//        this.content = this.scrollRect.content;
//        this.layoutGroup = this.content.GetComponent<HorizontalOrVerticalLayoutGroup>();
//        this.contentSizeFitter = this.content.GetComponent<ContentSizeFitter>();

//        if (null != this.contentSizeFitter) {
//            this.horizontalFit = this.contentSizeFitter.horizontalFit;
//            this.verticalFit = this.contentSizeFitter.verticalFit;
//        }

//        if (null != this.layoutGroup)
//        {
//            this.childAligement = layoutGroup.childAlignment;
//            this.controlChildSizeWidth = layoutGroup.childScaleWidth;
//            this.controlChildSizeHeight = layoutGroup.childScaleHeight;
//            this.useChildScaleWidth = layoutGroup.childControlWidth;
//            this.useChildScaleHeight= layoutGroup.childControlHeight;
//            this.childForceExpandWidth = layoutGroup.childForceExpandWidth;
//            this.childForceExpandHeight = layoutGroup.childForceExpandHeight;
//        }
//    }

//    public void Mask2DEnable(bool _bool) { this.rectMask2D.enabled = _bool; }

//    public void Render()
//    {
//        if (null == this.scrollRect) return;

//        GUI.color = this.useHorizontal ? Color.green : Color.cyan;
//        EditorGUILayout.BeginHorizontal("box");

//        EditorGUILayout.BeginVertical("box",GUILayout.Width(250));
//        EditorGUILayout.ObjectField(this.scrollRect.gameObject, typeof(GameObject), true, GUILayout.Width(200));
//        this.movementType = (ScrollRect.MovementType)EditorGUILayout.EnumPopup("Movement Type", this.movementType);
//        if (null != this.rectMask2D) { this.useRectMask2D = EditorGUILayout.Toggle("Mask", this.useRectMask2D); }
//        this.useHorizontal = EditorGUILayout.Toggle("Horizontal", this.useHorizontal);
//        this.useVertical = EditorGUILayout.Toggle("Vertical", this.useVertical);
//        EditorGUILayout.EndVertical();

//        EditorGUILayout.BeginVertical("box",GUILayout.Width(250));
//        EditorGUILayout.ObjectField(this.content.gameObject, typeof(GameObject), true);
//        EditorGUILayout.Toggle("Apply ContentSizeFitter", this.contentSizeFitter != null ? true : false );
//        this.horizontalFit = (ContentSizeFitter.FitMode)EditorGUILayout.EnumPopup("Horizontal Fit ", this.horizontalFit);
//        this.verticalFit = (ContentSizeFitter.FitMode)EditorGUILayout.EnumPopup("Vertical Fit ", this.verticalFit);
//        EditorGUILayout.EndVertical();

//        EditorGUILayout.BeginVertical("box", GUILayout.Width(300));
//        EditorGUILayout.ObjectField(this.layoutGroup, typeof(LayoutGroup), true);
//        this.childAligement= (TextAnchor)EditorGUILayout.EnumPopup("Child Alignment ", this.childAligement);

//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Control Child Size");
//        this.controlChildSizeWidth = GUILayout.Toggle(this.controlChildSizeWidth, " Width", GUILayout.Width(80));
//        this.controlChildSizeHeight = GUILayout.Toggle(this.controlChildSizeHeight, " Height", GUILayout.Width(80));
//        EditorGUILayout.EndHorizontal();
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Use Child Scale");
//        this.useChildScaleWidth = GUILayout.Toggle(this.useChildScaleWidth, " Width", GUILayout.Width(80));
//        this.useChildScaleHeight = GUILayout.Toggle(this.useChildScaleHeight, " Height", GUILayout.Width(80));
//        EditorGUILayout.EndHorizontal();
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Child Force Expand");
//        this.childForceExpandWidth = GUILayout.Toggle(this.childForceExpandWidth, " Width", GUILayout.Width(80));
//        this.childForceExpandHeight = GUILayout.Toggle(this.childForceExpandHeight, " Height", GUILayout.Width(80));
//        EditorGUILayout.EndHorizontal();

//        EditorGUILayout.EndVertical();

//        EditorGUILayout.Space();

//        if (GUILayout.Button("Apply", GUILayout.Height(30), GUILayout.Width(100)))
//        {
//            this.rectMask2D.enabled = this.useRectMask2D;
//            this.scrollRect.movementType = this.movementType;
//            this.scrollRect.horizontal = this.useHorizontal;
//            this.scrollRect.vertical= this.useVertical;
//            this.contentSizeFitter.horizontalFit = this.horizontalFit;
//            this.contentSizeFitter.verticalFit = this.verticalFit;

//            this.layoutGroup.childAlignment = this.childAligement;
//            this.layoutGroup.childScaleWidth= this.controlChildSizeWidth;
//            this.layoutGroup.childScaleHeight = this.controlChildSizeHeight;
//            this.layoutGroup.childControlWidth = this.useChildScaleWidth;
//            this.layoutGroup.childControlHeight = this.useChildScaleHeight;
//            this.layoutGroup.childForceExpandWidth = this.childForceExpandWidth;
//            this.layoutGroup.childForceExpandHeight = this.childForceExpandHeight;
//        }
//        EditorGUILayout.EndHorizontal();
//    }
//}
//-----------------------------------------------------------------------------------------------------------


//if (GUILayout.Button("Player Image Setting", GUILayout.Height(30)))
//{
//    SetPlayerSpriteRect();
//}

//if (GUILayout.Button("Test", GUILayout.Height(30)))
//{
//    Test();
//}