using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.PhysicsAssetSerialization;
using Com2VerseEditor.Rendering.World;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Com2VerseEditor.PhysicsAssetSerialization
{
    [CustomEditor(typeof(ServerObject))]
    public class ServerObjectEditor : Editor
    {
        private static Dictionary<long, BaseObject> _objectTable = null;
        private static Dictionary<long, List<InteractionLink>> _interactionLinkTable = null;
        private static long _spaceObjectId = 1;

        private BaseObject _myData;

        [MenuItem("Com2Verse/ServerObject/서버오브젝트 SpaceObjectId Validation")]
        public static void OrderServerObject()
        {
            var currentScene = SceneManager.GetActiveScene();

            if (!currentScene.name.EndsWith("_ServerObjects"))
            {
                Debug.LogError("ServerObject 씬(이 active인 상태)에서만 사용해주세요!");
                return;
            }
            
            List<GameObject> rootGameObjects = new List<GameObject>(currentScene.rootCount);
            currentScene.GetRootGameObjects(rootGameObjects);

            List<long> remIds = new List<long>(rootGameObjects.Count);
            List<long> existIds = new List<long>();
            
            for (long i = 1; i <= rootGameObjects.Count; i++)
                remIds.Add(i);

            foreach (var rootObject in rootGameObjects)
            {
                var currentInstanceId = rootObject.GetComponent<ServerObject>().ObjectInstanceID;
                existIds.Add(currentInstanceId);
                remIds.Remove(currentInstanceId);
            }

            rootGameObjects.Reverse();
            foreach (var rootObject in rootGameObjects)
            {
                var serverObject = rootObject.GetComponent<ServerObject>();
                var instanceId = serverObject.ObjectInstanceID;
                
                if (existIds.FindAll((value) => value == instanceId).Count > 1)
                {
                    var newId = remIds[0];
                    Debug.Log($"{rootObject.name} : 컨플릭트 (id : {instanceId}). 새로 id({newId})를 발급합니다. ");
                    serverObject.ObjectInstanceID = newId;
                    remIds.RemoveAt(0);
                    existIds.Remove(instanceId);
                    
                    EditorUtility.SetDirty(serverObject);
                }
            }
        }
        
        private static async UniTask LoadData()
        {
            if (_objectTable == null)
            {
                var objectData = await Addressables.LoadAssetAsync<TextAsset>("BaseObject.bytes").Task;
                _objectTable = TableBaseObject.Load(objectData?.bytes)?.Datas ?? new Dictionary<long, BaseObject>();
            }

            if (_interactionLinkTable == null)
            {
                var interactionData = await Addressables.LoadAssetAsync<TextAsset>("InteractionLink.bytes").Task;
                var linkList = TableInteractionLink.Load(interactionData?.bytes)?.Datas;
                _interactionLinkTable = new Dictionary<long, List<InteractionLink>>();
                if (linkList != null)
                {
                    foreach (var link in linkList)
                    {
                        if (!_interactionLinkTable.TryGetValue(link.Value.BaseObjectID, out var list))
                        {
                            list = new List<InteractionLink>();
                            _interactionLinkTable.Add(link.Value.BaseObjectID, list);
                        }

                        list.Add(link.Value);
                    }

                    foreach (var list in _interactionLinkTable.Values)
                    {
                        list.Sort((l, r) => l.InteractionLinkID.CompareTo(r.InteractionLinkID));
                    }
                }
            }

            if (EditorSceneManager.sceneCount > 1)
            {
                Scene soScene = EditorSceneManager.GetSceneAt(1);
                var roots = soScene.GetRootGameObjects();
                foreach (var rootObject in roots)
                {
                    var serverObject = rootObject.GetComponent<ServerObject>();
                    _spaceObjectId = Math.Max(_spaceObjectId, serverObject.ObjectInstanceID + 1);
                }
            }
        }

        private async void OnEnable()
        {
            if (Application.isPlaying) return;
            
            await LoadData();

            serializedObject.Update();
            var currentInstance = (target as ServerObject).gameObject;
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            bool find = false;
            if (prefabStage != null && prefabStage.prefabContentsRoot == currentInstance)
            {
                foreach (var baseObject in _objectTable.Values)
                {
                    if ($"{prefabStage.prefabContentsRoot.name}.prefab".Equals(baseObject.Name))
                    {
                        serializedObject.FindProperty("ObjectTypeID").longValue = baseObject.ID;
                        _myData = baseObject;
                        find = true;
                        break;
                    }
                }

                if (!find)
                {
                    serializedObject.FindProperty("ObjectTypeID").longValue = -1;
                }

                serializedObject.FindProperty("ObjectInstanceID").longValue = 0;
            }

            // 원본 프리팹에는 instance id 를 발급하지 않음
            if (prefabStage == null)
            {
                var spaceObjectIdProperty = serializedObject.FindProperty("ObjectInstanceID");
                
                if (spaceObjectIdProperty.longValue == 0)
                {
                    spaceObjectIdProperty.longValue = _spaceObjectId++;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowTriggerValidationInfo(long baseObjectId)
        {
            if (baseObjectId != 0)
            {
                if (_interactionLinkTable != null && _interactionLinkTable.TryGetValue(baseObjectId, out var linkList))
                {
                    var triggers = (target as ServerObject).transform.GetComponentsInChildren<C2VEventTrigger>();
                    if (triggers.Length != linkList.Count)
                    {
                        EditorGUILayout.HelpBox($"Prefab에 할당된 트리거의 갯수가 기획데이터와 일치하지 않습니다 (기획상 {linkList.Count} 개)", MessageType.Error);
                    }
                    else
                    {
                        for (int i = 0; i < linkList.Count; i++)
                        {
                            var targetLink = linkList[i];
                            var targetTrigger = triggers[i];

                            if (!targetTrigger.Type.ToString().Equals(targetLink.ColliderType.ToString()))
                            {
                                EditorGUILayout.HelpBox($"{i + 1} 번째 트리거({targetTrigger.gameObject.name})의 모양이 일치하지 않습니다. (기획상 {targetLink.ColliderType})", MessageType.Error);
                            }
                        }
                    }
                }
            }
        }
        
        private void OnDestroy()
        {
            _spaceObjectId = 1;
        }

        public override void OnInspectorGUI()
        {
            bool isWorldEditorEnabled = WorldEditor.Instance != null &&
                                        WorldEditor.Instance.CanUseEditMenu &&
                                        WorldEditor.Instance.SceneGUIHandle != null;
            
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
			
            EditorGUI.BeginDisabledGroup(true);
            if (_myData != null)
            {
                EditorGUILayout.EnumPopup("Object Type", _myData.ObjectType);
            }
            var baseObjectIdProperty = serializedObject.FindProperty("ObjectTypeID");
            EditorGUILayout.PropertyField(baseObjectIdProperty, new GUIContent("Base Object ID"));
            ShowTriggerValidationInfo(baseObjectIdProperty.longValue);
            
            if(isWorldEditorEnabled)
                EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ObjectInstanceID"), new GUIContent("Space Object ID"));
            
            if(!isWorldEditorEnabled)
                EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Tags"));

            EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();
            
            if (GUILayout.Button("현재 씬 서버오브젝트 보기"))
            {
                ServerObjectEditorWindow.Open();
            }
        }
    }

    public class ServerObjectEditorWindow : EditorWindow
    {
        [Flags]
        public enum eErrorCode
        {
            NONE = 0x0,
            OBJECT_TYPE_ID_INVALID = 0x1,
            OBJECT_INSTANCE_ID_INVALID = 0x2,
            OBJECT_INSTANCE_ID_DUPLICATED = 0x4,
        }
        
        private static readonly int   SceneObjectWidth      = 400;
        private static readonly int   PrefabObjectWidth     = 400;
        private static readonly int   ObjectTypeIDWidth     = 200;
        private static readonly int   ObjectInstanceIDWidth = 200;
        private static readonly Color ValidColor            = Color.white;
        private static readonly Color InvalidColor          = Color.red;
        private static readonly Color ValidBackgroundColor            = Color.white;
        private static readonly Color InvalidBackgroundColor          = Color.yellow;

        public class ServerObjectView
        {
            private ServerObject _serverObject;
            private GameObject   _prefab;
            private eErrorCode   _errorCode;
            
            public ServerObject ServerObject => _serverObject;

            public ServerObjectView(ServerObject serverObject)
            {
                _serverObject = serverObject;
                _prefab       = PrefabUtility.GetCorrespondingObjectFromOriginalSource<GameObject>(_serverObject.gameObject);
                ClearErrorCode();
            }

            public void OnGUI()
            {
                if (_serverObject.IsUnityNull()) return;
            
                GUI.color = IsValid() ? ValidColor : InvalidColor;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(_serverObject.gameObject, typeof(GameObject), GUILayout.Width(SceneObjectWidth));
                EditorGUILayout.ObjectField(_prefab,                  typeof(GameObject), GUILayout.Width(SceneObjectWidth));

                if (IsValidObjectTypeID()) EditorGUILayout.LabelField($"{_serverObject.ObjectTypeID}", GUILayout.Width(ObjectTypeIDWidth));
                else EditorGUILayout.LabelField($"Error => {_serverObject.ObjectTypeID}", EditorStyles.boldLabel, GUILayout.Width(ObjectTypeIDWidth));

                if (IsValidObjectInstanceID()) EditorGUILayout.LabelField($"{_serverObject.ObjectInstanceID}", GUILayout.Width(ObjectInstanceIDWidth));
                else EditorGUILayout.LabelField($"Error => {_serverObject.ObjectInstanceID}", EditorStyles.boldLabel, GUILayout.Width(ObjectInstanceIDWidth));

                EditorGUILayout.EndHorizontal();

                GUI.color = Color.white;
            }

            private bool IsValid()
            {
                return _errorCode == 0;
            }

            private bool IsValidObjectTypeID()
            {
                if ((_errorCode & eErrorCode.OBJECT_TYPE_ID_INVALID) > 0) return false;
                return true;
            }

            private bool IsValidObjectInstanceID()
            {
                if ((_errorCode & eErrorCode.OBJECT_INSTANCE_ID_INVALID) > 0) return false;
                if ((_errorCode & eErrorCode.OBJECT_INSTANCE_ID_DUPLICATED) > 0) return false;
                return true;
            }

            public void ClearErrorCode()
            {
                _errorCode = eErrorCode.NONE;
                if (_serverObject.IsUnityNull())
                {
                    return;
                }

                if (_serverObject.ObjectTypeID < 0)
                {
                    _errorCode |= eErrorCode.OBJECT_TYPE_ID_INVALID;
                }

                if (_serverObject.ObjectInstanceID < 0)
                {
                    _errorCode |= eErrorCode.OBJECT_INSTANCE_ID_INVALID;
                }
            }

            public void AddErrorCode(eErrorCode errorCode)
            {
                _errorCode |= errorCode;
            }
        }

        public static void Open()
        {
            var window = GetWindow<ServerObjectEditorWindow>();
            window.Show();
        }

        private Scene?                 _serverObjectScene = null;
        private List<ServerObjectView> _serverObjectViews = new();
        private Vector2                _scrollPos;

        private void OnEnable()
        {
            SearchServerObjects();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("서버오브젝트 찾기", EditorStyles.toolbarButton, GUILayout.Width(200)))
            {
                SearchServerObjects();
                Repaint();
            }

            if (GUILayout.Button("SpaceObjectID 자동 설정", EditorStyles.toolbarButton, GUILayout.Width(200)))
            {
                AutoSetSpaceObjectID();
                Repaint();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("오브젝트",           GUILayout.Width(SceneObjectWidth));
            EditorGUILayout.LabelField("프리팹",            GUILayout.Width(PrefabObjectWidth));
            EditorGUILayout.LabelField("BaseObjectID",   GUILayout.Width(ObjectTypeIDWidth));
            EditorGUILayout.LabelField($"SpaceObjectID", GUILayout.Width(ObjectInstanceIDWidth));
            EditorGUILayout.EndHorizontal();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            foreach (var serverObjectView in _serverObjectViews)
            {
                serverObjectView.OnGUI();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void SearchServerObjects()
        {
            _serverObjectScene = default;
            _serverObjectViews.Clear();

            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (scene.name.Contains("_ServerObjects"))
                {
                    _serverObjectScene = scene;
                    break;
                }
            }

            if (_serverObjectScene == null) return;

            var serverObjects = new List<ServerObject>();
            foreach (var rootGameObject in _serverObjectScene.Value.GetRootGameObjects())
            {
                foreach (var serverObject in rootGameObject.GetComponentsInChildren<ServerObject>())
                {
                    serverObjects.Add(serverObject);
                }
            }

            _serverObjectViews.AddRange(
                from serverObject in serverObjects
                select new ServerObjectView(serverObject));

            RefreshServerObjectErrorCode();
        }

        private void RefreshServerObjectErrorCode()
        {
            foreach (var serverObjectView in _serverObjectViews)
            {
                serverObjectView.ClearErrorCode();
            }

            var objectInstanceIDSet = new HashSet<long>();
            foreach (var serverObjectView in _serverObjectViews)
            {
                if (!objectInstanceIDSet.TryAdd(serverObjectView.ServerObject.ObjectInstanceID))
                {
                    serverObjectView.AddErrorCode(eErrorCode.OBJECT_INSTANCE_ID_DUPLICATED);
                }
            }
        }

        private void AutoSetSpaceObjectID()
        {
            if (_serverObjectScene == null)
            {
                EditorUtility.DisplayDialog("알림", "서버 오브젝트 찾기 버튼을 눌러주세요", "확인");
                return;
            }
            EditorSceneManager.SetActiveScene(_serverObjectScene.Value);
            SearchServerObjects();
            ServerObjectEditor.OrderServerObject();
            RefreshServerObjectErrorCode();
        }
    }
}