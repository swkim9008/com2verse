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
using Object = UnityEngine.Object;
using UnityEditor.SceneManagement;
using Unity.VisualScripting;

namespace Com2verseEditor.UnityAssetTool {
   
    public partial class TATUGUITool : EditorWindow
    {
        void SelectSpriteImage(Sprite _sprite)
        {
            List<GameObject> gameObjectList = new List<GameObject>();
            gameObjectList.Clear();

            GameObject rootObj = Selection.activeObject as GameObject;
            if (null == rootObj) return;

            Image[] tmpImageList = rootObj.GetComponentsInChildren<Image>(true);

            foreach (Image image in tmpImageList)
            {
                if (image.sprite == null) continue;
                if (image.sprite == _sprite)
                {
                    gameObjectList.Add(image.gameObject);
                }
            }
            Selection.objects = gameObjectList.ToArray();
        }


        void SelectMissingSpriteImage()
        {
            List<GameObject> gameObjectList = new List<GameObject>();
            gameObjectList.Clear();

            GameObject rootObj = Selection.activeObject as GameObject;
            if (null == rootObj) return;

            Image[] targetImageList = rootObj.GetComponentsInChildren<Image>(true);

            foreach (Image image in targetImageList)
            {
                if (image.sprite == null)
                {
                    gameObjectList.Add(image.gameObject);
                }
            }
            Selection.objects = gameObjectList.ToArray();
        }


        void SelectTargetSpriteButton(Sprite _sprite)
        {
            List<GameObject> gameObjectList = new List<GameObject>();
            gameObjectList.Clear();

            GameObject rootObj = Selection.activeObject as GameObject;
            if (null == rootObj) return;

            Button[] targetImageList = rootObj.GetComponentsInChildren<Button>(true);

            foreach (Button button in targetImageList)
            {
                if (null == button.targetGraphic) continue;

                if (button.targetGraphic.gameObject.GetComponent<Image>().sprite == _sprite)
                {
                    gameObjectList.Add(button.gameObject);
                }
            }
            Selection.objects = gameObjectList.ToArray();
        }


        void RemoveComponent<T>() where T : Component
        {
            bool bDo = EditorUtility.DisplayDialog("경고", $"선택한 노드 하위의 모든 {typeof(T).Name} 컴포넌트를 제거합니다.\n\n(프로그램쪽에서 작업중인 Prefab 에는 사용하지 마세요.)", "네", "아니요");
            if (!bDo) return;

            GameObject targetObj = Selection.activeObject as GameObject;
            if (null == targetObj) return;

            T[] targetComponentList = targetObj.GetComponentsInChildren<T>(true);

            foreach (T _target in targetComponentList)
            {
                DestroyImmediate(_target);
            }
        }



        void ResetSprite()
        {
            bool buttonResetSprite = GUILayout.Button("Reset\nSelected Sprite", GUILayout.Height(60));
            if (buttonResetSprite)
            {
                //GameObject targetObj = Selection.activeObject as GameObject;
                //UISprite targetUISprite = targetObj.GetComponent<UISprite>();
                //targetUISprite.type = UIBasicSprite.Type.Simple;
                //targetUISprite.width = targetUISprite.GetAtlasSprite().width;
                //targetUISprite.height = targetUISprite.GetAtlasSprite().height;
            }
        }
    
        // ---------------------------------------------------------------------------------------------



        // Prefab 만들기 -----------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------
    
        void CreatePrefabFromAtlas() {

            EditorGUILayout.BeginVertical();

            bool buttonMakePrefab = GUILayout.Button("선택된 아틀라스의 스프라이트를 프리펩으로 저장", GUILayout.Height(30));

            if (buttonMakePrefab)
            {
                string targetPath = EditorUtility.SaveFolderPanel("Save textures to folder", Application.dataPath + "/Resources/GUI/Prefab/Icon", "");

                if (!string.IsNullOrEmpty(targetPath))
                {
                    //Debug.Log(NGUISettings.atlas.spriteList.Count.ToString());

                    //int num = NGUISettings.atlas.spriteList.Count;
                    //List<UISpriteData> spriteList = NGUISettings.atlas.spriteList;
                    //GameObject go = NGUIEditorTools.SelectedRoot();

                    //for (int i = 0; i < num; i++)
                    //{
                    //    //Debug.Log(spriteList[0].name);
                    //    string targetSpriteName = spriteList[i].name;
                    //    NGUISettings.selectedSprite = targetSpriteName;
                    //    GameObject currentSprite = NGUISettings.AddSprite(go).gameObject;
                    //    currentSprite.name = targetSpriteName;

                    //    string assetPath = targetPath.Replace(Application.dataPath, "Assets") + "/" + targetSpriteName + ".prefab";

                    //    PrefabUtility.CreatePrefab(assetPath, currentSprite, ReplacePrefabOptions.ConnectToPrefab);

                    //    //프로그레스바.
                    //    ShowProgressbar("Create Icon Prefab", targetSpriteName, i, spriteList.Count);
                    //    if (progressCancel)
                    //    {
                    //        EditorUtility.ClearProgressBar();
                    //    }

                    //}
                }
            }
            EditorGUILayout.EndVertical();
        }


        //Prefab Applay 하기.
        void ShowGUIApplyPrefab() {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("선택한 Prefab의 수정사항 적용", GUILayout.Height(30)))
            {
                ApplyPrefab();
            }
            EditorGUILayout.EndHorizontal();
            //EditorGUILayout.Space();
        }

        //Apply Prefab.
        void ApplyPrefab() {
            progressCancel = false;
            GameObject[] selectedObjects = Selection.gameObjects;
            int c = 0;
            while (c < selectedObjects.Length)
            {
                string targetObjectPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(selectedObjects[c]));
                //Debug.Log(AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(selectedObjects[c])));
                PrefabUtility.SaveAsPrefabAssetAndConnect(selectedObjects[c], targetObjectPath, InteractionMode.AutomatedAction);

                //프로그레스바.
                ShowProgressbar("Apply Prefab", selectedObjects[c].name, c, selectedObjects.Length);
                if (progressCancel)
                {
                    c = selectedObjects.Length;
                    EditorUtility.ClearProgressBar();
                }
                c++;
            }
        }
        //-----------------------------------------------------------------------------------------------
    

        private void SetGameObjectActive(bool _bool)
        {
            GameObject rootObj = Selection.activeObject as GameObject;
            if (null == rootObj) return;

            rootObj.gameObject.SetActive(_bool);

            Transform[] gameObjects = rootObj.GetComponentsInChildren<Transform>(true);

            foreach (Transform go in gameObjects)
            {
                go.gameObject.SetActive(_bool); 
            }
        }


        //하위까지 레이어 변경하기
        public static void ChangeLayersRecursively(Transform trans, string layerName)
        {
            trans.gameObject.layer = LayerMask.NameToLayer(layerName);
            foreach (Transform child in trans)
            {
                ChangeLayersRecursively(child, layerName);
            }
        }


        private void SetTextureImporterPlatformSettings(TextureImporter _textureImporter, int _size, TextureImporterFormat _textureImporterFormat)
        {
            TextureImporterPlatformSettings textureImporterPlatformSettings = new TextureImporterPlatformSettings();
            textureImporterPlatformSettings.name = "Android";
            textureImporterPlatformSettings.maxTextureSize = _size;
            textureImporterPlatformSettings.format = _textureImporterFormat;
            textureImporterPlatformSettings.overridden = true;

            _textureImporter.SetPlatformTextureSettings(textureImporterPlatformSettings);

            TextureImporterPlatformSettings textureImporteriPhonePlatformSettings = new TextureImporterPlatformSettings();
            textureImporteriPhonePlatformSettings.name = "iPhone";
            textureImporteriPhonePlatformSettings.maxTextureSize = _size;
            textureImporteriPhonePlatformSettings.format = _textureImporterFormat;
            textureImporteriPhonePlatformSettings.overridden = true;

            _textureImporter.SetPlatformTextureSettings(textureImporteriPhonePlatformSettings);
        }


        /// <summary>
        /// 현재 배치된 게임 오브젝트의 이름을 모두 변경
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="_to"></param>
        void ReNameAll(string _from, string _to)
        {
            if (_from.Length <= 0 || _to.Length < 0) return;

            GameObject[] tmpGameObjects = GameObject.FindObjectsOfType<GameObject>(true);

            foreach (var _item in tmpGameObjects)
            {
                string srcName = _item.name;
                _item.name = srcName.Replace(_from, _to);
            }
        }



        /// <summary>
        /// 선택된 노드 하위의 이름을 변경
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="_to"></param>
        /// <param name="_root"></param>
        void ReNameChildOnly(string _from, string _to, GameObject _root)
        {
            if (_root == null) return;
            if (_from.Length <= 0 || _to.Length < 0) return;

            //PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            //if (prefabStage != null)
            //{

            //    _root = PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(Selection.activeGameObject);
            //    _root = (GameObject)prefabStage.openedFromInstanceObject;
            //}

            var list = _root.GetComponentsInChildren<Transform>();

            foreach (var item in list)
            {
                string srcName = item.gameObject.name;
                item.gameObject.name = srcName.Replace(_from, _to);
            }

            //EditorUtility.SetDirty(prefabStage.prefabContentsRoot);
            

            //int num = _root.transform.childCount;
            //for (int i = 0; i < num; i++)
            //{
            //    GameObject temp = _root.transform.GetChild(i).gameObject;

            //    ReNameChildOnly(_from, _to, temp);
            //}
        }


        public GameObject GetActiveObject()
        {
            GameObject _root = Selection.activeGameObject;

            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                _root = stage.prefabContentsRoot;
            }

            //PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            //// 수정 작업

            ////

            //EditorUtility.SetDirty(prefabStage.prefabContentsRoot);

            ////PrefabUtility.SaveAsPrefabAsset(prefabStage.prefabContentsRoot, prefabStage.assetPath);
            //prefabStage.ClearDirtiness();


            return _root;
        }



        ////Image에 Sprite 영역 설정
        //private void SetPlayerSpriteRect()
        //{
        //    UnityEngine.Object objSelect = Selection.activeObject;

        //    if (objSelect.GetType() != typeof(Texture2D)) return;
        //    string objPath = AssetDatabase.GetAssetPath(objSelect);  //선택된 녀석의 경로를 가져온다.

        //    TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(objPath);

        //    if (textureImporter.textureType != TextureImporterType.Sprite)
        //    {
        //        textureImporter.textureType = TextureImporterType.Sprite;
        //    }
        //    textureImporter.spriteImportMode = SpriteImportMode.Multiple;
        //    textureImporter.mipmapEnabled = false;

        //    Texture2D objTexture = (Texture2D)objSelect;
        //    TextureImporterSettings textureImporterSettings = new TextureImporterSettings();

        //    textureImporter.ReadTextureSettings(textureImporterSettings);
        //    textureImporterSettings.spriteMeshType = SpriteMeshType.FullRect;
        //    textureImporterSettings.spriteGenerateFallbackPhysicsShape = false;
        //    textureImporter.SetTextureSettings(textureImporterSettings);

        //    List <SpriteMetaData> spriteMetaDataList = new List<SpriteMetaData>();

        //    SpriteMetaData spriteMetaData01 = new SpriteMetaData();
        //    spriteMetaData01.name = "0";
        //    spriteMetaData01.rect = new Rect(0,0, objTexture.width, objTexture.height);
        //    spriteMetaDataList.Add(spriteMetaData01);

        //    SpriteMetaData spriteMetaData02 = new SpriteMetaData();
        //    spriteMetaData02.name = "card";
        //    spriteMetaData02.rect = new Rect(0, 0, 196, 256);
        //    spriteMetaDataList.Add(spriteMetaData02);

        //    SpriteMetaData spriteMetaData03 = new SpriteMetaData();
        //    spriteMetaData03.name = "portrait_team";
        //    spriteMetaData03.rect = new Rect(50, 0, 104, 256);
        //    spriteMetaDataList.Add(spriteMetaData03);

        //    textureImporter.spritesheet = spriteMetaDataList.ToArray();

        //    AssetDatabase.ImportAsset(objPath, ImportAssetOptions.ForceUpdate);
        //    AssetDatabase.Refresh();
        //}



        /*UI 프로그래스바 보여주기 -------------------------------------------------------------------------*/
        void ShowProgressbar(string title, string content, int done, int whole)
        {
            if (done < whole)
            {
                progressBar = (float)(done + 1) / (float)whole;
                int percentage = (int)(progressBar * 100f);

                if (EditorUtility.DisplayCancelableProgressBar(title, $"now [{content}] working - {done + 1}／{whole}({percentage}%)", progressBar))
                {
                    progressCancel = true;
                    Debug.Log("Progress bar canceled by the user");
                }
            }
            if (done + 1 == whole) EditorUtility.ClearProgressBar();
        }

    }
}



