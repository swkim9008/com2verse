using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;


namespace Com2verseEditor.UnityAssetTool
{
    public partial class SceneTool : EditorWindow
    {
        void TapBaseUIScene()
        {
            EditorGUILayout.Separator();

            MakeSceneButton("로고", GetScenePath("SceneSplash"));                         //"Assets/Project/Runtime/Scenes/SceneSplash.unity"
            MakeSceneButton("번들다운로드", GetScenePath("SceneAssetBundleDownloader")); //"Assets/Project/Runtime/Scenes/SceneAssetBundleDownloader.unity"

            EditorGUILayout.Separator();
            //MakeSceneButton("타이틀", GetScenePath("SceneTitle"));                         //Assets/Project/Runtime/Scenes/SceneTitle.unity"
            EditorGUILayout.Separator();
            MakeSceneButton("아바타선택", GetScenePath("SceneAvatarSelection"));             //"Assets/Project/Runtime/Scenes/SceneAvatarSelection.unity"

            MakeSceneButton("빌더?", GetScenePath("SceneBuilder"));           //"Assets/Project/Runtime/Scenes/SceneBuilder.unity"
            //MakeSceneButton("씬로딩?", GetScenePath("SceneLoading"));          //"Assets/Project/Runtime/Scenes/SceneLoading.unity"
            MakeSceneButton("월드?", GetScenePath("SceneWorld"));              //"Assets/Project/Runtime/Scenes/SceneWorld.unity"

            if (GUILayout.Button("Test", GUILayout.Height(30))) { GetScenePath("SceneSplash"); }
        }

        string[] filePaths;

        private void OnFocus()
        {
            this.filePaths = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);
        }

        string GetScenePath(string sceneName)
        {
            foreach (string filePath in filePaths) 
            {
                if (filePath.Contains(sceneName))
                {
                    return filePath;
                }
            }
            return string.Empty;
        }


        void TapUIOfficeScene()   //UI 작업용 씬 모음
        {
            EditorGUILayout.Separator();
            MakeSceneListButton("Office", "Assets/Project/Bundles/02_Construction/01_Office/Scene_Office_AAG", true, false);
        }

        void TapUIMiceScene()   //UI 작업용 씬 모음
        {
            EditorGUILayout.Separator();
            MakeSceneListButton("Mice", "Assets/Project/Bundles/02_Construction/03_MICE/Scene_AAG", true, false);
            
        }

        void TapUIWorldScene()   //UI 작업용 씬 모음
        {
            EditorGUILayout.Separator();
            MakeSceneListButton("World", "Assets/Project/Bundles/02_Construction/02_World/Scene_World_AAG", true, false);
        }

        void TapUIRTScene()
        {
            EditorGUILayout.Separator();
            MakeSceneButton("명함 Avata RT", "Assets/Project/Bundles/04_UI/03_Mice/RTScene_AAG/BusinessCardRTScene.unity");
        }


        //string _path = "Assets/Scenes/Track/Training/";
        void MakeSceneListButton(string groupName, string path, bool isSelectable = true, bool showFileName = true)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(groupName, EditorStyles.boldLabel);
            if (GUILayout.Button(">>", GUILayout.Height(30), GUILayout.Width(50))) 
            { 
                string workPath = Application.dataPath.Replace("Assets", path).Replace("/","\\");
                System.Diagnostics.Process.Start("explorer.exe", workPath);
            }
            EditorGUILayout.EndHorizontal();

            string[] fileList = Directory.GetFiles(path, "*.unity");

            foreach (string file in fileList)
            {
                //string buttonName = Path.GetFileNameWithoutExtension(file).Split("_")[2];
                //MakeSceneButton(buttonName, file, _isSelectable, _showFileName);

                //씬 이름 미입력 체크 
                //string[] buttonName = Path.GetFileNameWithoutExtension(file).Split(char.Parse("_"));
                //if(buttonName.Length > 2)
                //{
                //    MakeSceneButton(buttonName[2], file, isSelectable, showFileName);
                //}
                //else
                //{
                    MakeSceneButton(Path.GetFileNameWithoutExtension(file), file, isSelectable, showFileName);
                //}
            }
        }


        void MakeSceneButton(string buttonName, string path, bool isSelectable = true, bool showFileName = true)
        {
            GUI.color = File.Exists(path) ? Color.white : Color.red;

            EditorGUILayout.BeginHorizontal();
            string buttonString = showFileName ? $"{buttonName} ({Path.GetFileNameWithoutExtension(path)})" : buttonName;   //메뉴이름(씬 이름)

            bool isCurrent = Path.GetFileNameWithoutExtension(path) == EditorSceneManager.GetActiveScene().name;
            GUI.contentColor = isCurrent ? Color.gray : Color.white;
            if (GUILayout.Button(buttonString, GUILayout.Height(30)))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                }

                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            }

            if (isSelectable)
            {
                if (GUILayout.Button("Select", GUILayout.Height(30), GUILayout.Width(50)))
                {
                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path.Replace(Application.dataPath, "Assets"), typeof(UnityEngine.Object));
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }
            }

            GUI.contentColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }
    }
}