using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using Cinemachine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Playables;
using UnityEditor.SceneManagement;
using UnityEngine.Timeline;
using System.Reflection;


namespace Com2verseEditor.UnityAssetTool
{
    public partial class SceneTool : EditorWindow
    {
        private void TabSceneSetting()
        {
            EditorGUILayout.BeginVertical();
            GUI.color = Color.cyan;
            if (GUILayout.Button("UI Scene Setting", GUILayout.Height(30))) { SetUIScene(new string[] { "Lobby", "Marble", "Training", "GrandPrix", "WorldGrandPrix" }); }
            GUI.color = Color.white;

            EditorGUILayout.Separator();
            if (GUILayout.Button("UI Light Setting", GUILayout.Height(30))) { SetUIScene3DLight(); }
            if (GUILayout.Button("ADD UI Reflection Probe", GUILayout.Height(30))) { AddReflectioProbe(); }

            EditorGUILayout.Separator();
            if (GUILayout.Button("Track Scene Setting", GUILayout.Height(30)))
            {
                RemoveUIFromTrackLight();
                AddTrackPreviewRT(); 
            }

            EditorGUILayout.Separator();
            if (GUILayout.Button("Create 10 Random Marble", GUILayout.Height(30))) { CreateRandomMarbles(10); }
            if (GUILayout.Button("Create 100 Random Marble", GUILayout.Height(30))) { CreateRandomMarbles(100); }

            EditorGUILayout.Separator();
            if (GUILayout.Button("Test", GUILayout.Height(30))) { Test(); }
            EditorGUILayout.EndVertical();
        }


        private Camera CreateUICamera(Transform root, string objectName, string layer, string tag, float zPos)
        {
            Camera camera = GetComponentByGameObjectName<Camera>(objectName, layer);

            camera.transform.parent = root;
            camera.transform.localRotation = Quaternion.identity;
            camera.transform.localPosition = new Vector3(0, 0, -zPos);

            camera.orthographic = false;     //Projection
            camera.nearClipPlane = 1.0f;
            camera.farClipPlane = 1000;
            camera.orthographicSize = 100;
            camera.gameObject.tag = tag;
            camera.depth = 0;     //카메라 우선순위를 맞추고 레이어로 순서 조정
            camera.fieldOfView = 30;

            CameraExtensions.GetUniversalAdditionalCameraData(camera).renderType = CameraRenderType.Overlay;

            LayerMaskOnlyAdd(camera, LayerMask.NameToLayer(layer));

            var cm = camera.GetComponent<CinemachineBrain>();   //시네머신이 있는 카메라를 받으면 시네머신 제거...
            if (cm != null) { DestroyImmediate(cm); }

            return camera;
        }


        private Camera AddCinemachine(Transform root, Camera camera)
        {
            CinemachineBrain cinemachineBrain = camera.gameObject.GetComponent<CinemachineBrain>() ?? camera.gameObject.AddComponent<CinemachineBrain>();
            cinemachineBrain.m_DefaultBlend.m_Time = 0.5f;

            //시네머신 버츄얼 카메라 추가
            CinemachineVirtualCamera virtualCamera = GetComponentByGameObjectName<CinemachineVirtualCamera>("CM vcam_Start", "UIScene3D");
            virtualCamera.transform.parent = root;
            virtualCamera.transform.localRotation = Quaternion.identity;
            virtualCamera.transform.localPosition = new Vector3(0, 0, 0);
            virtualCamera.transform.localScale = new Vector3(1, 1, 1);  
            virtualCamera.Priority = 10;            //////////////////////////////////////////
            virtualCamera.m_Lens.FieldOfView = 30f;
            virtualCamera.m_Lens.NearClipPlane = 1.0f;
            virtualCamera.m_Lens.FarClipPlane = 1000f;

            return camera;
        }

        private Camera CreateMainCamera()
        {
            Camera mainCamera = GetComponentByGameObjectName<Camera>("MainCamera");
            mainCamera.transform.localRotation = Quaternion.identity;
            mainCamera.transform.localPosition = new Vector3(0, 0, 0);
            mainCamera.fieldOfView = 60f;
            CameraExtensions.GetUniversalAdditionalCameraData(mainCamera).renderType = CameraRenderType.Base;

            LayerMaskRemove(mainCamera, LayerMask.NameToLayer("UI3D"));
            LayerMaskRemove(mainCamera, LayerMask.NameToLayer("UI"));
            LayerMaskRemove(mainCamera, LayerMask.NameToLayer("UIScene3D"));
            LayerMaskRemove(mainCamera, LayerMask.NameToLayer("UIScene"));

            CinemachineBrain cinemachineBrain = mainCamera.gameObject.GetComponent<CinemachineBrain>() ?? mainCamera.gameObject.AddComponent<CinemachineBrain>();
            cinemachineBrain.m_DefaultBlend.m_Time = 0.5f;

            return mainCamera;
        }



        private Canvas CreateUISceneCanvas(Camera camera, float planeDistance)
        {
            Canvas canvas = GetComponentByGameObjectName<Canvas>("Canvas", "UIScene");
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = planeDistance;
            canvas.tag = "UISceneCanvas";

            GraphicRaycaster graphicRaycaster = canvas.gameObject.GetComponent<GraphicRaycaster>() ?? canvas.gameObject.AddComponent<GraphicRaycaster>();
            //ScreenAspectHelper screenAspectHelper = canvas.gameObject.GetComponent<ScreenAspectHelper>() ?? canvas.gameObject.AddComponent<ScreenAspectHelper>();
            CanvasScaler canvasScaler = canvas.gameObject.GetComponent<CanvasScaler>() ?? canvas.gameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1280, 720);

            return canvas;
        }


        /// <summary>
        /// checkList의 문자열이 경로에 있으면 true
        /// </summary>
        /// <param name="_sceneName"></param>
        /// <param name="checkList"></param>
        /// <returns></returns>
        private bool IsTargetScene(string[] checkList)
        {
            string directoryName = Path.GetDirectoryName(EditorSceneManager.GetActiveScene().path);

            for(int i = 0; i < checkList.Length; i++)
            {
                if (directoryName.Contains(checkList[i])) return true;
            }

            return false;
        }


        private void AddTimeLineGroup(PlayableDirector playableDirector, Canvas canvas)
        {
            AddTimelineGroupTrack(playableDirector, "SCENEOBJECT"); //씬 진입시 연출 그룹추가

            Transform transform = canvas.transform;  //Canvas 하위 구조에 따라 TimeLine에 그룹 추가
            for (int i = 0, cnt = transform.childCount; i < cnt; i++)
            {
                Transform child = transform.GetChild(i);
                AddTimelineGroupTrack(playableDirector, child.name.ToUpper());
            }
        }


        void RemoveCinemachineNode(GameObject root)
        {
            var cmvcams = root.GetComponentsInChildren<CinemachineVirtualCamera>();
            foreach (CinemachineVirtualCamera v in cmvcams)
            {
                DestroyImmediate(v.gameObject);
            }
        }

        private Canvas SetUIScene(string[] workTargets)
        {
            if (EditorSceneManager.GetActiveScene().name == String.Empty) return null;   //저장되지 않은 씬
            if (IsTargetScene(workTargets) == false) return null;

            //카메라 및 UI용 루트 추가
            GameObject uiSceneRoot = CreateRootGameObject("UIScene", "UIScene3D");

            float canvasDistance = 200f;
            //카메라 생성
            Camera mainCamera = CreateMainCamera();
            Camera uiSceneCamera = CreateUICamera(uiSceneRoot.transform, "UISceneCamera", "UIScene", "UISceneCamera", canvasDistance);
            Camera uiScene3dCamera = CreateUICamera(uiSceneRoot.transform, "UIScene3DCamera", "UIScene3D", "UIScene3DCamera", canvasDistance);
            
            //AddCinemachine(uiSceneRoot.transform, uiScene3dCamera);
            RemoveCinemachineNode(uiSceneRoot);             //UI용 시네머신 제거

            //캔버스 추가
            Canvas uiCanvas = CreateUISceneCanvas(uiSceneCamera, canvasDistance);

            //URP 카메라 Stack설정
            UniversalAdditionalCameraData cameraData = mainCamera.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Clear();
            cameraData.cameraStack.Add(uiSceneCamera);
            cameraData.cameraStack.Add(uiScene3dCamera);

            //카메라에 붙어 있는 AudioListener 제거
            DestroyImmediate(uiSceneCamera.GetComponent<AudioListener>());
            DestroyImmediate(uiScene3dCamera.GetComponent<AudioListener>());
            DestroyImmediate(mainCamera.GetComponent<AudioListener>());

            //TimeLine 설정fkd
            PlayableDirector playableDirector = CreatePlayableDirector();
            AddTimeLineGroup(playableDirector, uiCanvas);

            CreateRootGameObject("3DRoot", "Default");

            //순서정리
            string[] siblings = { "_SceneObject", "_TimeLine", "MainCamera", "3DRoot", "UIScene", "Canvas" };
            SetSiblings(siblings);

            //이벤트 시스템 제거
            GameObject eventSystem = GameObject.Find("EventSystem");
            if (null != eventSystem) DestroyImmediate(eventSystem);

            return uiCanvas;
        }



        private PlayableDirector CreatePlayableDirector()
        {
            PlayableDirector playableDirector = GetComponentByGameObjectName<PlayableDirector>("_TimeLine");
            playableDirector.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            playableDirector.playOnAwake = false;
            playableDirector.extrapolationMode = DirectorWrapMode.Hold;

            string path = EditorSceneManager.GetActiveScene().path.Replace("unity", "playable");
            TimelineAsset timeline = (TimelineAsset)AssetDatabase.LoadAssetAtPath(path, typeof(TimelineAsset));

            if (timeline == null)
            {
                timeline = TimelineAsset.CreateInstance<TimelineAsset>();
                AssetDatabase.CreateAsset(timeline, path);
            }

            playableDirector.playableAsset = timeline;

            return playableDirector;
        }

        private GameObject CreateRootGameObject(string objectName, string layer)
        {
            GameObject rootGameObject = GameObject.Find(objectName) ?? new GameObject(objectName);
            //rootGameObject.SetLayerRecursively(LayerMask.NameToLayer(_layer));
            return rootGameObject;
        }


        /// <summary>
        /// 해당 이름의 컴포넌트가 있는 노드를 돌려준다. 없으면 생성해서 돌려준다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectName"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        private T GetComponentByGameObjectName<T>(string objectName, string layer = "Default") where T : Component
        {
            GameObject tmpGameObject = GameObject.Find(objectName) ?? new GameObject(objectName);
            //tmpGameObject.SetLayerRecursively(LayerMask.NameToLayer(_layer));

            T tmpComponent = tmpGameObject.GetComponent<T>();
            if (tmpComponent == null)
            {
                tmpComponent = (T)tmpGameObject.AddComponent(typeof(T));
            }

            return tmpComponent;
        }


        //Camera layerMask 설정
#region Camera LayerMask
        public void LayerMaskOnlyAdd(Camera camera, int layerIndex) => camera.cullingMask = 1 << layerIndex;
        public void LayerMaskOnlyRemove(Camera camera, int layerIndex) => camera.cullingMask = ~(1 << layerIndex);
        public void LayerMaskRemove(Camera camera, int layerIndex) => camera.cullingMask &= ~(1 << layerIndex);
        public void LayerMaskAdd(Camera camera, int layerIndex) => camera.cullingMask |= 1 << layerIndex;
        public void LayerMaskEverything(Camera camera) => camera.cullingMask = -1;
        public void LayerMaskNothing(Camera camera) => camera.cullingMask = ~-1;
#endregion // Camera LayerMask

        void AddTimelineGroupTrack(PlayableDirector playableDirector, string groupTrackName)
        {
            TimelineAsset timeline = playableDirector.playableAsset as TimelineAsset;

            List<string> rootTrackNames = EnumRootTrackNames(playableDirector).ToList();
            if (rootTrackNames.Contains(groupTrackName) == false)
            {
                timeline.CreateTrack<GroupTrack>(groupTrackName);
            }
        }

        //타임라인 루트 트렉 이름을 돌려준다.
        private IEnumerable<string> EnumRootTrackNames(PlayableDirector playableDirector)
        {
            var timelineAsset = playableDirector.playableAsset as TimelineAsset;

            foreach (var track in timelineAsset.GetRootTracks())
            {
                yield return track.name;
            }
        }


        void SetUIScene3DLight()
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                light.color = Color.white;

                light.cullingMask = ~-1;    //Remove All

                light.cullingMask |= 1 << LayerMask.NameToLayer("UIScene3D");   //Add
                light.cullingMask |= 1 << LayerMask.NameToLayer("UI3D");        //Add

                if (light.type == LightType.Directional)
                {
                    light.transform.eulerAngles = new Vector3(60, -60, 0);
                };
            }
        }

        private void AddReflectioProbe()
        {
            ReflectionProbe reflectionProbe = GetComponentByGameObjectName<ReflectionProbe>("Reflection Probe", "UIScene3D");

            GameObject tmp = GameObject.Find("UIScene3DCamera") ?? null;

            reflectionProbe.transform.parent = tmp.transform;
            reflectionProbe.mode = UnityEngine.Rendering.ReflectionProbeMode.Custom;
            reflectionProbe.size = new Vector3(2000, 2000, 2000);
            string cubeMapPath = "Assets/_MR/Environment/Texture/CubeMap_Test.exr";
            reflectionProbe.customBakedTexture = AssetDatabase.LoadAssetAtPath(cubeMapPath, typeof(Texture)) as Texture;
        }



        private void SetSiblings(string[] lists)
        {
            for (int i = 0; i < lists.Length; i++)
            {
                GameObject.Find(lists[i]).transform.SetSiblingIndex(i);
            }
        }



        void RemoveUIFromTrackLight()    //조명과 리플렉션 에서 UI3D, UI 레이어 제거
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                light.cullingMask &= ~(1 << LayerMask.NameToLayer("UIScene3D"));
                light.cullingMask &= ~(1 << LayerMask.NameToLayer("UIScene"));
                light.cullingMask &= ~(1 << LayerMask.NameToLayer("UI3D"));
                light.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
            }

            ReflectionProbe[] probes = GameObject.FindObjectsOfType<ReflectionProbe>();
            foreach (ReflectionProbe probe in probes)
            {
                probe.cullingMask &= ~(1 << LayerMask.NameToLayer("UIScene3D"));
                probe.cullingMask &= ~(1 << LayerMask.NameToLayer("UIScene"));
                probe.cullingMask &= ~(1 << LayerMask.NameToLayer("UI3D"));
                probe.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
            }
        }

        
        void AddTrackPreviewRT()  //게임로비에서 사용하는 씬 프리뷰용 RT 설정
        {
            if (EditorSceneManager.GetActiveScene().name == String.Empty) return;   //저장되지 않은 씬
            if (false == EditorSceneManager.GetActiveScene().path.Contains("Track_")) return;  //Track에서만 생성 및 설정

            GameObject tmpGameObject = GameObject.Find ("UIPreviewRoot") ?? new GameObject("UIPreviewRoot");
            Camera rtCamera = GetComponentByGameObjectName<Camera>("UIPreviewCamera");
            CameraExtensions.GetUniversalAdditionalCameraData(rtCamera).renderType = CameraRenderType.Base;

            rtCamera.transform.parent = tmpGameObject.transform;
            rtCamera.fieldOfView = 60f;
            rtCamera.clearFlags = CameraClearFlags.Nothing;
            rtCamera.targetTexture = (RenderTexture)AssetDatabase.LoadAssetAtPath("Assets\\BundleResources\\RenderTexture\\RenderTexture_TrackPrevew.renderTexture", typeof(RenderTexture));
            
            LayerMaskOnlyAdd(rtCamera, LayerMask.NameToLayer("Track"));
        }
        


        void Test()
        {

        }


        /*랜덤하게 마블을 만들어 준다.*/
        void CreateRandomMarbles (int maxCount)
        {
            int count = 1;

            var coreList = Directory.GetFiles("Assets\\BundleResources\\Marble\\Core", "Marble_Core_*.prefab", SearchOption.AllDirectories);
            var bodyList = Directory.GetFiles("Assets\\BundleResources\\Marble\\Body", "Marble_Body_*.prefab", SearchOption.AllDirectories);
            var shellList = Directory.GetFiles("Assets\\BundleResources\\Marble\\Shell", "Marble_Shell_*.prefab", SearchOption.AllDirectories);
            var moonList = Directory.GetFiles("Assets\\BundleResources\\Marble\\Moon", "Marble_Moon_*.prefab", SearchOption.AllDirectories);

            GameObject root = new GameObject("MarbleRoot");

            float z = 0;
            float x = 0;

            while (count <= maxCount) 
            {
                string name = $"Marble{count.ToString("D3")}";
                GameObject marbleRoot = new GameObject(name);
                marbleRoot.transform.parent = root.transform;

                //int range = 10;
                //float x = UnityEngine.Random.Range(0, range) * 1.5f;
                //float z = UnityEngine.Random.Range(0, range) * 1.5f;

                marbleRoot.transform.position = new Vector3(x, 0, z);

                x = (count % 10) * 1.5f;
                z = (count / 10) * 1.5f;

                int coreNum = UnityEngine.Random.Range(0, coreList.Length);
                int bodyNum = UnityEngine.Random.Range(0, bodyList.Length);
                int shellNum = UnityEngine.Random.Range(0, shellList.Length);
                int moonNum = UnityEngine.Random.Range(0, moonList.Length);

                GameObject tmpCore = (GameObject)AssetDatabase.LoadAssetAtPath(coreList[coreNum], typeof(GameObject));
                Instantiate(tmpCore, marbleRoot.transform);
                var tmpBody = (GameObject)AssetDatabase.LoadAssetAtPath(bodyList[bodyNum], typeof(GameObject));
                Instantiate(tmpBody, marbleRoot.transform);
                var tmpShell = (GameObject)AssetDatabase.LoadAssetAtPath(shellList[shellNum], typeof(GameObject));
                GameObject shell = GameObject.Instantiate(tmpShell, marbleRoot.transform);
                shell.transform.rotation = UnityEngine.Random.rotation;

                GameObject tmpMoon = (GameObject)AssetDatabase.LoadAssetAtPath(moonList[moonNum], typeof(GameObject));
                GameObject.Instantiate(tmpMoon, marbleRoot.transform);

                count++;
            }
        }


        //리플렉션 테스트
        void ReflectionTest()
        {
            GameObject gameObject = GameObject.Find("Directional Light");
            SetProperties<Light, float>(gameObject, "intensity", 1.0f);
            SetProperties<Light, Color>(gameObject, "color", Color.blue);
        }

        private void SetProperties<T1, T2>(GameObject gameObject, string property, T2 value) where T1 : Component
        {
            Type type = typeof(T1);
            T1 t = gameObject.GetComponent<T1>();

            foreach (PropertyInfo info in type.GetProperties())
            {
                if (info.Name == property)
                {
                    info.SetValue(t, value);
                }
            }
        }
    }
}