/*===============================================================
* Product:    Com2Verse
* File Name:  MapController.cs
* Developer:  haminjeong
* Date:       2023-01-02 18:38
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Text;
using Google.Protobuf;
using UnityEngine;

namespace Com2Verse.Network
{
    public sealed class MapController : MonoSingleton<MapController>
    {
        public enum eObjectDefinition
        {
            ACTIVE = 1,
            STATIC,
        }

        /// <summary>
        /// CS Log Enable
        /// </summary>
        [field: SerializeField] public bool IsCSLogEnable { get; set; } = false;

        /// <summary>
        /// Enable Avatar Create
        /// </summary>
        [field: SerializeField] public bool IsEnableAvatarCreate { get; set; } = true;

        /// <summary>
        /// Enable Object Count Log
        /// </summary>
        [field: SerializeField] public bool IsEnableObjectCountLog { get; set; } = false;

        /// <summary>
        /// Enable Object Count Limit
        /// </summary>
        [field: SerializeField] public int ObjectCountLimit { get; set; } = 0;

        private Dictionary<long, BaseMapObject> _staticObjects;
        private Dictionary<long, BaseMapObject> _staticObjectsByOwner;
        private Dictionary<long, BaseMapObject> _activeObjects;
        private Dictionary<long, BaseMapObject> _activeObjectsByOwner;
        private List<(long, long)> _reservedRemoveObjects;
        private static readonly int RemoveObjectDelay = 200;
        
        private List<BaseMapObject> _activeObjectList;

        /// <summary>
        /// Serial ID로 오브젝트를 가져옵니다.
        /// </summary>
        /// <param name="id">Serial ID</param>
        public GameObject this[long id]
        {
            get
            {
                if (_staticObjects!.TryGetValue(id, out var value1) && !value1.IsUnityNull())
                    return value1!.gameObject;
                if (_activeObjects!.TryGetValue(id, out var value2) && !value2.IsUnityNull())
                    return value2!.gameObject;
                return null;
            }
        }

        /// <summary>
        /// 유저 ID로 오브젝트를 가져옵니다.
        /// </summary>
        /// <param name="id">유저 ID</param>
        /// <returns>오브젝트의 BaseMapObject</returns>
        public BaseMapObject GetObjectByUserID(long id)
        {
            if (_staticObjectsByOwner!.TryGetValue(id, out var result1))
                return result1;
            if (_activeObjectsByOwner!.TryGetValue(id, out var result2))
                return result2;
            return null;
        }

        /// <summary>
        /// Serial ID로 ActiveObject 중에서 오브젝트를 가져옵니다.
        /// </summary>
        /// <param name="id">Serial ID</param>
        /// <returns>오브젝트의 BaseMapObject</returns>
        public BaseMapObject GetActiveObjectByID(long id)
        {
            if (_activeObjects!.TryGetValue(id, out var activeObject))
                return activeObject;
            return null;
        }

        /// <summary>
        /// Serial ID로 StaticObject 중에서 오브젝트를 가져옵니다.
        /// </summary>
        /// <param name="id">Serial ID</param>
        /// <returns>오브젝트의 BaseMapObject</returns>
        public BaseMapObject GetStaticObjectByID(long id)
        {
            if (_staticObjects!.TryGetValue(id, out var staticObject))
                return staticObject;
            return null;
        }

        /// <summary>
        /// SpaceObject ID로 StaticObject 중에서 오브젝트를 가져옵니다.
        /// </summary>
        /// <param name="id">SpaceObject ID</param>
        /// <returns>오브젝트의 BaseMapObject</returns>
        public BaseMapObject GetStaticObjectBySpaceID(long id)
        {
            foreach (var pair in _staticObjects!)
            {
                if (pair.Value.SpaceObjectId == id)
                    return pair.Value;
            }
            return null;
        }

        /// <summary>
        /// BaseObjectID로 오브젝트를 가져옵니다.
        /// </summary>
        /// <param name="id">BaseObjectID</param>
        /// <returns>오브젝트의 BaseMapObject</returns>
        public BaseMapObject GetObjectByBaseObjectID(long id)
        {
            foreach (var pair in _staticObjects!)
            {
                if (pair.Value.ObjectTypeId == id)
                    return pair.Value;
            }
            foreach (var pair in _activeObjects!)
            {
                if (pair.Value.ObjectTypeId == id)
                    return pair.Value;
            }
            return null;
        }

        [field: SerializeField, ReadOnly]
        private int _cellWidth  = 16;

        [field: SerializeField, ReadOnly]
        private int _cellHeight = 16;

        public int CellWidth
        {
            get => _cellWidth;
            private set
            {
                _cellWidth = value;
                MapObjectUtil.SetCellSize(_cellWidth, _cellHeight);
            }
        }

        public int CellHeight
        {
            get => _cellHeight;
            private set
            {
                _cellHeight = value;
                MapObjectUtil.SetCellSize(_cellWidth, _cellHeight);
            }
        }

        public int TotalObjectCount  => _activeObjects!.Count + _staticObjects!.Count;
        public int StaticObjectCount => _staticObjects!.Count;

        [field: SerializeField, ReadOnly] private long _fieldID;

        [field: SerializeField, ReadOnly] private long _lastFieldID;
        [field: SerializeField, ReadOnly] private int _localNumber;
        public long LastFieldID => _lastFieldID;
        public long FieldID => _fieldID;

        public int LocalNo => _localNumber;

        public long UserID { get; set; } = -1;
        public long UserSerialID { get; set; }

        public float MultiplyOfDeltaTime = 1.5f; // 보간 정도를 결정하는 값, 높을 수록 보간의 세밀함이 떨어진다.

        public struct ObjectStateInfo
        {
            public long Serial;
            public long Time;
            public Vector3 Offset;
            public Vector2 CellIndex;
            public Protocols.ObjectState ObjState;
        }
        private Dictionary<long, ObjectStateInfo> _willCreateDictionary;
        private Queue<ObjectStateInfo> _reservedUpdatePackets;
        private float _updateDelayTime = 0.0f;

        private static readonly float TIMEOUT_FOR_UPDATEPACKET = 2.0f;

        public ObjectStateInfo GetCreateInfo(long serial) => _willCreateDictionary!.TryGetValue(serial, out var info) ? info : default;

        public IReadOnlyDictionary<long, BaseMapObject> ActiveObjects => _activeObjects;
        public IReadOnlyDictionary<long, BaseMapObject> StaticObjects => _staticObjects;

        public BaseObjectStatePublisher StatePublisher { get; set; }

        public Func<Protocols.Avatar, BaseMapObject.AvatarCustomizeInfo> AvatarDtoToStruct { get; set; } = null;

        private Action<Protocols.ObjectState, BaseMapObject> _onMapObjectCreate;
        private Action<BaseMapObject>                        _onMapObjectRemove;
        private Action<Protocols.ObjectState>                _onMapObjectWillCreate;
        public event Action<Protocols.ObjectState, BaseMapObject> OnMapObjectCreate
        {
            add
            {
                _onMapObjectCreate -= value;
                _onMapObjectCreate += value;
            }
            remove => _onMapObjectCreate -= value;
        }
        public event Action<BaseMapObject> OnMapObjectRemove
        {
            add
            {
                _onMapObjectRemove -= value;
                _onMapObjectRemove += value;
            }
            remove => _onMapObjectRemove -= value;
        }

        public event Action<Protocols.ObjectState> OnMapObjectWillCreate
        {
            add
            {
                _onMapObjectWillCreate -= value;
                _onMapObjectWillCreate += value;
            }
            remove => _onMapObjectWillCreate -= value;
        }
        public event Action OnUpdateEvent;

#region Creator
        private Dictionary<long, IObjectCreator> _objectCreators;
        private bool _isAlreadyAddCreators = false;

        /// <summary>
        /// 외부에서 어셈블리를 조회하여 Creator가 담긴 클래스를 등록시켜 줍니다.
        /// </summary>
        /// <param name="types">IObjectCreator가 담긴 클래스의 모음</param>
        public void RegisterObjectCreators(IEnumerable<Type> types)
        {
            if (_isAlreadyAddCreators) return;
            foreach (var type in types)
            {
                C2VDebug.Log($"-> {type.Name}");
                IObjectCreator objectCreator = Activator.CreateInstance(type) as IObjectCreator;
                objectCreator.Initialize(CheckObjectExist, CheckPoolStack, _objectRootTrans);
                _objectCreators!.TryAdd(type.GetCustomAttribute<DefinitionAttribute>().Definition, objectCreator);
                C2VDebug.Log($"-> {type.Name}...DONE");
            }

            _isAlreadyAddCreators = true;
        }
#endregion

#region ObjectPooling
        private static readonly string PoolRootName = "Pool";
        private Dictionary<long, Stack<BaseMapObject>> _staticObjectPoolStack;
        private Dictionary<long, Stack<BaseMapObject>> _activeObjectPoolStack;
        private Transform _objectRootTrans;
        private Transform _objectPoolTrans;

        private bool CheckObjectExist(long serial, long definition)
        {
            switch ((eObjectDefinition)definition)
            {
                case eObjectDefinition.ACTIVE:
                {
                    if (_activeObjects!.ContainsKey(serial))
                    {
                        C2VDebug.LogError($"Duplicated ActiveObject Serial {serial}");
                        _willCreateDictionary!.Remove(serial);
                        ReviveObject(serial);
                        return true;
                    }
                    break;
                }
                default:
                {
                    if (_staticObjects!.ContainsKey(serial))
                    {
                        C2VDebug.LogError($"Duplicated StaticObject Serial {serial}");
                        _willCreateDictionary!.Remove(serial);
                        ReviveObject(serial);
                        return true;
                    }
                    break;
                }
            }
            return false;
        }

        private BaseMapObject CheckPoolStack(long definition, int avatarType)
        {
            BaseMapObject mapObject = null;
            switch ((eObjectDefinition)definition)
            {
                case eObjectDefinition.ACTIVE:
                {
                    if (!_activeObjectPoolStack!.TryGetValue(avatarType, out var stack))
                    {
                        stack = new();
                        _activeObjectPoolStack.Add(avatarType, stack);
                    }

                    if (stack.Count == 0)
                        return mapObject;
                    mapObject = stack.Pop();
                    break;
                }
                default:
                {
                    if (!_staticObjectPoolStack!.TryGetValue(definition, out var stack))
                    {
                        stack = new();
                        _staticObjectPoolStack.Add(definition, stack);
                    }

                    if (stack.Count == 0)
                        return mapObject;
                    mapObject = stack.Pop();
                    break;
                }
            }
            return mapObject;
        }

        private void ReleaseMapObject(BaseMapObject mapObject)
        {
            _onMapObjectRemove?.Invoke(mapObject);
            mapObject!.gameObject.SetActive(false);
            mapObject.transform.SetParent(_objectPoolTrans, false);
            mapObject.ReleaseObject();
            if (_activeObjectPoolStack!.TryGetValue(mapObject.AvatarType, out var stack1))
            {
                _objectCreators![(long)eObjectDefinition.ACTIVE].ReleaseObject(mapObject);
                stack1.Push(mapObject);
            }
            else if (_staticObjectPoolStack!.TryGetValue(mapObject.ObjectTypeId, out var stack2))
            {
                _objectCreators![(long)eObjectDefinition.STATIC].ReleaseObject(mapObject);
                stack2.Push(mapObject);
            }
        }
#endregion

#region ReservedObject
        private Dictionary<long, ObjectStateInfo> _reservedObjectInfos;
        private List<ObjectStateInfo>             _processingReservedObjects;
        private bool                              _needToUpdateReservedObjects;

        public Func<Protocols.ObjectState, bool> CheckReserveObject { get; set; }
#endregion
        
#region Mono
        protected override void AwakeInvoked()
        {
            _objectRootTrans = transform;

            var poolRootObj = new GameObject(PoolRootName);
            poolRootObj.transform.SetParent(_objectRootTrans);
            poolRootObj.transform.localPosition = new Vector3(10000, 0, 10000);
            poolRootObj.transform.localScale = Vector3.one;
            _objectPoolTrans = poolRootObj.transform;

            _activeObjectList      = new();
            _staticObjects         = new();
            _activeObjects         = new();
            _staticObjectsByOwner  = new();
            _activeObjectsByOwner  = new();
            _staticObjectPoolStack = new Dictionary<long, Stack<BaseMapObject>>();
            _activeObjectPoolStack = new Dictionary<long, Stack<BaseMapObject>>();
            _willCreateDictionary  = new();
            _reservedUpdatePackets = new();
            _reservedRemoveObjects = new();
            _objectCreators        = new();

            _reservedObjectInfos           = new();
            _processingReservedObjects = new();
            _needToUpdateReservedObjects = false;

            MapObjectUtil.SetCellSize(CellWidth, CellHeight);
        }

        private void Update()
        {
            OnUpdateEvent?.Invoke();
        }
        
        public void OnLateUpdate()
        {
            UpdateReservedUpdateQueue();
            MetaverseWatch.TimerUpdate();
            foreach (BaseMapObject activeObject in _activeObjects!.Values)
            {
                if (activeObject.IsUnityNull()) continue;
                activeObject!.UpdateObject();
            }

            foreach (BaseMapObject staticObject in _staticObjects!.Values)
            {
                if (staticObject.IsUnityNull()) continue;
                staticObject!.UpdateObject();
            }

            CheckRemoveQueue();
            UpdateReservedObjects();
            StatePublisher?.CheckStateDirty();
        }

        protected override void OnDestroyInvoked()         => Destroy();
        protected override void OnApplicationQuitInvoked() => Destroy();

        private void Destroy()
        {
            TagProcessorManager.InstanceOrNull?.Clear();
            OnUpdateEvent = null;
            CleanObjects();
            _isAlreadyAddCreators = false;
            _objectCreators?.Clear();
        }
#endregion

        private void UpdateReservedUpdateQueue()
        {
            while (_reservedUpdatePackets!.TryPeek(out var info))
            {
                GameObject obj = this[info.Serial];
                if (obj.IsReferenceNull())
                {
                    _updateDelayTime += Time.unscaledDeltaTime;
                    if (_updateDelayTime > TIMEOUT_FOR_UPDATEPACKET)
                    {
                        _updateDelayTime = 0.0f;
                        _reservedUpdatePackets.Dequeue();
                    }
                    return;
                }
                ObjectStateProcess(info.Time, info.ObjState, info.Offset, info.CellIndex);
                _reservedUpdatePackets!.Dequeue();
            }
        }

        /// <summary>
        /// 현재 필드 ID를 갱신합니다.
        /// </summary>
        /// <param name="cell">현재 필드의 Cell 정보</param>
        public void UpdateWorldInfo(Protocols.WorldState.Cell cell)
        {
            _fieldID = cell.FieldId;
            // _localNumber = cell.LocalNo;
        }

        /// <summary>
        /// 씬 전환이나 앱 종료 시 MapController를 정리합니다.
        /// </summary>
        public void CleanObjects()
        {
            _lastFieldID = _fieldID;
            _fieldID = 0;
            _localNumber = 0;
            StatePublisher?.ClearMyObject();

            _activeObjects?.Keys.ToList().ForEach(RemoveObject);
            _staticObjects?.Keys.ToList().ForEach(RemoveObject);
            _activeObjectsByOwner?.Clear();
            _staticObjectsByOwner?.Clear();
            _willCreateDictionary?.Clear();
            _reservedRemoveObjects?.Clear();
            _reservedUpdatePackets?.Clear();
            _reservedObjectInfos?.Clear();
            _processingReservedObjects?.Clear();
            _needToUpdateReservedObjects = false;
            
            ClearPoolObjects();
        }

        private void ClearPoolObjects()
        {
            if (_activeObjectPoolStack == null || _staticObjectPoolStack == null) return;
            foreach (var poolStack in _activeObjectPoolStack.Values)
            {
                while (poolStack.Count > 0)
                {
                    var baseObject = poolStack.Pop();
                    Destroy(baseObject.gameObject);
                }
            }

            foreach (var poolStack in _staticObjectPoolStack.Values)
            {
                while (poolStack.Count > 0)
                {
                    var baseObject = poolStack.Pop();
                    Destroy(baseObject.gameObject);
                }
            }
            
            _activeObjectPoolStack.Clear();
            _staticObjectPoolStack.Clear();
        }

        /// <summary>
        /// 한 개의 Cell 정보에 해당하는 CellState를 처리합니다.
        /// </summary>
        /// <param name="mapData">Cell 정보</param>
        public void OnMapData(Protocols.WorldState.CellState mapData)
        {
#if UNITY_EDITOR && !METAVERSE_RELEASE
            if (IsCSLogEnable && mapData.Objects.Count > 0)
                C2VDebug.LogErrorCategory("MapController", ToStringCS(mapData));
#endif
            var cell = mapData.Cell;
            UpdateWorldInfo(cell);
            var offset = new Vector3(cell.CellIdHorizontal * CellWidth, 0, cell.CellIdVertical * CellHeight);
            Vector2 cellIndex = new Vector2(cell.CellIdHorizontal, cell.CellIdVertical);

            for (int index = 0; index < mapData.Objects.Count; ++index)
            {
                Protocols.ObjectState objState = mapData.Objects[index];
                // NOTE: 이미 생성된 static object 들도 새로운 구독 셀이 추가됨에 따라 포함되서 올 수 있다.
                GameObject obj = this[objState.Serial];
                if (obj.IsReferenceNull())
                    ObjectAddProcess(ServerTime.Time, objState, offset, cellIndex);
                else
                    ObjectStateProcess(ServerTime.Time, objState, offset, cellIndex);
            }

            _needToUpdateReservedObjects = true;
        }

#if UNITY_EDITOR && !METAVERSE_RELEASE
        private string ToStringCS(Protocols.WorldState.CellState mapData)
        {
            var sb = ZString.CreateStringBuilder();
            sb.AppendLine($"CellState Field:{mapData.Cell.FieldId} Cell:{mapData.Cell.CellIdHorizontal},{mapData.Cell.CellIdHorizontal} ObjectCount:{mapData.Objects.Count}");
            sb.AppendLine("-------------start-------------------");
            var offset = new Vector3(mapData.Cell.CellIdHorizontal * CellWidth, 0, mapData.Cell.CellIdVertical * CellHeight);
            for (int index = 0; index < mapData.Objects.Count; ++index)
            {
                Protocols.ObjectState objState = mapData.Objects[index];
                sb.Append($"Serial:{objState.Serial} Owner:{objState.OwnerId}");
                if (objState.Definition != null)
                    sb.Append($" BaseObjectId:{objState.Definition.BaseObjectId}");
                if (objState.PhysicsState is { Position: { } })
                {
                    var position = new Vector3(objState.PhysicsState.Position.X * 0.01f, objState.PhysicsState.Position.Y * 0.01f, objState.PhysicsState.Position.Z * 0.01f) + offset;
                    sb.Append($" Position:{position}\n");
                }
                if (objState.ObjectAvatar != null)
                    sb.Append($"Avatar:{objState.ObjectAvatar.AvatarID}\n");
            }
            sb.AppendLine("---------------end-------------------");
            return sb.ToString();
        }

        private string ToStringCSU(Protocols.CellStateUpdate.CellStateUpdate mapData)
        {
            var sb = ZString.CreateStringBuilder();
            sb.AppendLine($"CellStateUpdate Field:{mapData.Cell.FieldId} Cell:{mapData.Cell.CellIdHorizontal},{mapData.Cell.CellIdHorizontal} ObjectCount:{mapData.Objects.Count}");
            sb.AppendLine($"AddedObject:{mapData.AddedObjects.Count} RemoveObject:{mapData.RemovedObjects.Count}");
            sb.AppendLine("-------------start-------------------");
            var offset = new Vector3(mapData.Cell.CellIdHorizontal * CellWidth, 0, mapData.Cell.CellIdVertical * CellHeight);
            for (int index = 0; index < mapData.Objects.Count; ++index)
            {
                Protocols.ObjectState objState = mapData.Objects[index];
                sb.Append($"Serial:{objState.Serial} Owner:{objState.OwnerId}");
                if (objState.Definition != null)
                    sb.Append($" BaseObjectId:{objState.Definition.BaseObjectId}");
                if (objState.PhysicsState is { Position: { } })
                {
                    var position = new Vector3(objState.PhysicsState.Position.X * 0.01f, objState.PhysicsState.Position.Y * 0.01f, objState.PhysicsState.Position.Z * 0.01f) + offset;
                    sb.Append($" Position:{position}\n");
                }
                if (objState.ObjectAvatar != null)
                    sb.Append($"Avatar:{objState.ObjectAvatar.AvatarID}\n");
            }
            sb.AppendLine("---------------end-------------------");
            return sb.ToString();
        }
#endif

        /// <summary>
        /// 한 개의 Cell 정보에 해당하는 CellStateUpdate를 처리합니다. 변화가 있는 Cell에서만 응답이 옵니다.
        /// </summary>
        /// <param name="mapData">Cell 정보</param>
        public void OnMapUpdate(Protocols.CellStateUpdate.CellStateUpdate mapData)
        {
            // maybe cellstate update before warp
            if (_fieldID != mapData.Cell.FieldId) return;// || _localNumber != mapData.Cell.LocalNo) return;

#if UNITY_EDITOR && !METAVERSE_RELEASE
            if (IsCSLogEnable && (mapData.Objects.Count > 0 || mapData.RemovedObjects.Count > 0))
                C2VDebug.LogErrorCategory("MapController", ToStringCSU(mapData));
#endif

            var     cell      = mapData.Cell;
            var     offset    = MapObjectUtil.GetCellOffset(cell.CellIdHorizontal, cell.CellIdVertical);
            Vector2 cellIndex = new Vector2(cell.CellIdHorizontal, cell.CellIdVertical);

            if (mapData.RemovedObjects.Count > 0)
            {
                for (int index = 0; index < mapData.RemovedObjects.Count; ++index)
                {
                    if (mapData.RemovedObjects[index] == UserSerialID) continue;
                    RemoveReservedObject(mapData.RemovedObjects[index]);
                    
                    GameObject obj = this[mapData.RemovedObjects[index]];
                    if (obj.IsReferenceNull()) continue;
                    if (_reservedRemoveObjects!.FindIndex(removeObj => removeObj.Item1 == mapData.RemovedObjects[index]) != -1) continue;
                    ReserveRemove(mapData.RemovedObjects[index], cellIndex);
                }
            }

            for (int index = 0; index < mapData.Objects.Count; ++index)
            {
                Protocols.ObjectState objState = mapData.Objects[index];
                var serial = objState.Serial;
                // NOTE: 현재 필드 이동을 하면 SerialID가 다르게 발급된다.
                if (mapData.AddedObjects.Contains(serial))
                {
                    GameObject obj = this[serial];
                    if (obj.IsReferenceNull())
                        ObjectAddProcess(ServerTime.Time, objState, offset, cellIndex);
                    else
                    {
                        // if (serial != UserSerialID)
                        //     C2VDebug.LogWarning($"AddedObject serial {serial} is already exist!");
                        ObjectStateProcess(ServerTime.Time, objState, offset, cellIndex);
                    }
                }
                else
                    ObjectStateProcess(ServerTime.Time, objState, offset, cellIndex);
            }
            
            _needToUpdateReservedObjects = true;
        }

        private void ObjectAddProcess(long serverTime, Protocols.ObjectState objState, Vector3 offset, Vector2 cellIndex)
        {
            if (!IsValidCreatePacket(objState)) return;
            ObjectStateInfo info = new ObjectStateInfo
            {
                Serial = objState.Serial,
                Time = serverTime,
                ObjState = objState,
                Offset = offset,
                CellIndex = cellIndex,
            };

            if (TryAddReserveObject(info)) return;
            
            if (!_willCreateDictionary!.TryAdd(objState.Serial, info))
            {
                C2VDebug.LogWarning($"ObjectState Serial {objState.Serial} is waiting for create. Packet will reserved.");
                _reservedUpdatePackets!.Enqueue(info);
                return;
            }
            _onMapObjectWillCreate?.Invoke(objState);

            ReviveObject(objState.Serial);
            
            var initialPosition = new Vector3(objState.PhysicsState.Position.X / 100f, objState.PhysicsState.Position.Y / 100f, objState.PhysicsState.Position.Z / 100f) + offset;
            CreateObject(objState.Serial, objState.Definition.BaseObjectId, objState.ObjectAvatar, initialPosition, (serialID, mapObject) =>
            {
                if (!_willCreateDictionary.TryGetValue(serialID, out var createInfo))
                {
                    C2VDebug.LogError($"Avatar Doesn't Exist Serial {serialID}");
                    return;
                }

                if (!mapObject.IsReferenceNull())
                {
                    mapObject.CellIndex = createInfo.CellIndex;
                    mapObject.ProcessState(createInfo.Offset, createInfo.Time, createInfo.ObjState, eMapObjectUpdateState.ADDED);
                }

                _willCreateDictionary.Remove(serialID);
                _onMapObjectCreate?.Invoke(objState, mapObject);
            });
        }

        private void ObjectStateProcess(long serverTime, Protocols.ObjectState objState, Vector3 offset, Vector2 cellIndex)
        {
            long serial = objState.Serial;
            GameObject obj = this[serial];
            if (obj.IsReferenceNull())
            {
                if (_willCreateDictionary!.ContainsKey(serial))
                {
#if UNITY_EDITOR && !METAVERSE_RELEASE
                    if (!IsEnableAvatarCreate)
                        return;
#endif
                    ObjectStateInfo info = new ObjectStateInfo
                    {
                        Serial = serial,
                        Time = serverTime,
                        ObjState = objState,
                        Offset = offset,
                        CellIndex = cellIndex,
                    };
                    C2VDebug.LogWarning($"ObjectState Serial {serial} is waiting for create. Packet will reserved.");
                    _reservedUpdatePackets!.Enqueue(info);
                }
                else
                {
                    C2VDebug.LogWarning($"Invalid ObjectState Serial {serial}");
                    ObjectAddProcess(serverTime, objState, offset, cellIndex);
                }
            }
            else
            {
                ReviveObject(serial);
                BaseMapObject objectInCell = obj.GetComponent<BaseMapObject>();
                objectInCell.CellIndex = cellIndex;
                objectInCell.ProcessState(offset, serverTime, objState);
            }
        }

        private void CreateObject(long serial, long definition, Protocols.Avatar avatar, Vector3 initialPosition, Action<long, BaseMapObject> onCompleted)
        {
            var creatorKey = definition == (long)eObjectDefinition.ACTIVE ? (long)eObjectDefinition.ACTIVE : (long)eObjectDefinition.STATIC;
            _objectCreators![creatorKey].CreateObject(serial, definition, avatar, initialPosition, (serialID, obj) =>
            {
                if (obj.IsReferenceNull())
                {
                    C2VDebug.LogError($"Avatar Object Null Serial {serialID}");
                    _willCreateDictionary!.Remove(serialID);
                    return;
                }

                if (!_willCreateDictionary!.TryGetValue(serialID, out var createInfo))
                {
                    C2VDebug.LogWarning($"Avatar Doesn't Exist WillCreateDictionary Serial {serialID}");
                    obj.DestroyGameObject();
                    return;
                }
                
#if UNITY_EDITOR && !METAVERSE_RELEASE
                // 가장 업데이트가 오래된 오브젝트를 지운다.
                if (ObjectCountLimit > 0)
                    CheckLimitActiveObjectCount();
#endif

                obj.Init(serialID, createInfo.ObjState.OwnerId, createInfo.ObjState.Definition.BaseObjectId == (int)eObjectDefinition.ACTIVE);
                obj.EnableObject(_objectRootTrans);
                if (createInfo.ObjState.Definition.BaseObjectId == (int)eObjectDefinition.ACTIVE)
                {
                    _activeObjectList!.TryAdd(obj);
                    _activeObjects!.TryAdd(serialID, obj);
                    _activeObjectsByOwner!.TryAdd(createInfo.ObjState.OwnerId, obj);
                }
                else
                {
                    _staticObjects!.TryAdd(serialID, obj);
                    _staticObjectsByOwner!.TryAdd(createInfo.ObjState.OwnerId, obj);
                }

                onCompleted?.Invoke(serialID, obj);
            });
        }

        private void CheckLimitActiveObjectCount()
        {
            if (_activeObjectList!.Count <= ObjectCountLimit) return;
            _activeObjectList.Sort((a, b) =>
            {
                if (a.IsUnityNull() || b.IsUnityNull()) return 0;
                return a!.UpdatedState.Time.CompareTo(b!.UpdatedState.Time);
            });
            while (_activeObjectList!.Count > ObjectCountLimit)
            {
                var removeNominee = _activeObjectList[0];
                if (removeNominee.IsUnityNull()) return;
                var reservedRemoveIndex = _reservedRemoveObjects!.FindIndex((obj) => obj.Item1 == removeNominee!.ObjectID);
                if (reservedRemoveIndex != -1)
                    _reservedRemoveObjects.RemoveAt(reservedRemoveIndex);
                RemoveObject(removeNominee!.ObjectID);
            }
        }

#region Remove Process
        private void CheckRemoveQueue()
        {
            for (int i = _reservedRemoveObjects!.Count - 1; i >= 0; --i)
            {
                var peekObject = _reservedRemoveObjects[i];
                if (MetaverseWatch.Time > peekObject.Item2)
                {
                    RemoveObject(peekObject.Item1);
                    _reservedRemoveObjects!.RemoveAt(i);
                }
            }
        }

        private void ReviveObject(long id)
        {
            _reservedRemoveObjects!.RemoveAll(obj => obj.Item1 == id);
        }

        private void ReserveRemove(long id, Vector2 cellIndex)
        {
            // 간헐적으로 add -> remove 순으로 오는 경우가 있어 체크
            BaseMapObject mapObject = this[id]!.GetComponent<BaseMapObject>();
            if (!mapObject!.CellIndex.Equals(cellIndex))
            {
                // C2VDebug.LogWarningCategory("MapController", "Ignore remove packet from previous cell index");
                return; // 이전 셀의 remove가 온 경우 무시한다.
            }
            _reservedRemoveObjects!.TryAdd((id, MetaverseWatch.Time + RemoveObjectDelay));
        }

        private void RemoveObject(long id)
        {
            if (_activeObjects!.TryGetValue(id, out var value1))
            {
                _activeObjectsByOwner!.Remove(value1!.OwnerID);
                _activeObjectList!.Remove(value1);
                _activeObjects.Remove(id);
                ReleaseMapObject(value1);
            }
            else if (_staticObjects!.TryGetValue(id, out var value3))
            {
                _staticObjectsByOwner!.Remove(value3!.OwnerID);
                _staticObjects.Remove(id);
                ReleaseMapObject(value3);
            }
        }
#endregion

        private bool IsValidCreatePacket(Protocols.ObjectState objState)
        {
            long serial = objState.Serial;
            if (objState.Definition == null)
            {
                C2VDebug.LogWarning($"Serial {serial} ObjectState Definition is null");
                return false;
            }
            if (objState.Definition.BaseObjectId == (int)eObjectDefinition.ACTIVE)
            {
                if (objState.ObjectAvatar == null)
                {
                    C2VDebug.LogWarning($"[MapController] avatar is empty. serialID : {serial}");
                    return false;
                }

                if (objState.ObjectAvatar.FaceItemList.Count > 0)
                {
                    foreach (var bodyItem in objState.ObjectAvatar.FaceItemList?.ToList())
                    {
                        if (bodyItem.FaceID == 0)
                        {
                            C2VDebug.LogError($"[MapController] it's exists avatar face item id zero in list. serialID : {serial}");
                            return false;
                        }
                    }
                }
                // FashionID가 0인 경우가 있어 제거
                /*if (objState.ObjectAvatar.FashionItemList.Count > 0)
                {
                    foreach (var fashionItem in objState.ObjectAvatar.FashionItemList?.ToList())
                    {
                        if (fashionItem.FashionID == 0)
                        {
                            C2VDebug.LogError($"[MapController] it's exists avatar fashion item id zero in list. serialID : {serial}");
                            return false;
                        }
                    }
                }*/
            }
            if (objState.PhysicsState == null)
            {
                C2VDebug.LogError($"Serial {serial} ObjectState PhysicsState is null");
                return false;
            }
            if (objState.PhysicsState.Position == null)
            {
                C2VDebug.LogError($"Serial {serial} PhysicsState Position is null");
                return false;
            }
            if (objState.PhysicsState.Rotation == null)
            {
                C2VDebug.LogError($"Serial {serial} PhysicsState Rotation is null");
                return false;
            }

            return true;
        }

#if ENABLE_CHEATING
        public long CreateObjectDebug(BaseMapObject obj, long ownerId)
        {
            int tmpID = 0;
            while (!this[tmpID].IsReferenceNull())
                tmpID = UnityEngine.Random.Range(0, int.MaxValue);

            if (obj is BaseMapObject activeObject)
            {
                _activeObjectList!.TryAdd(activeObject);
                _activeObjects!.TryAdd(tmpID, activeObject);
                _activeObjectsByOwner!.TryAdd(ownerId, activeObject);
            }
            else
            {
                _staticObjects!.TryAdd(tmpID, obj);
                _staticObjectsByOwner!.TryAdd(ownerId, obj);
            }

            obj.Init(tmpID, ownerId, true);
            obj.EnableObject(_objectRootTrans);
            obj.IsDebug = true;
            obj.gameObject.SetActive(true);

            _onMapObjectCreate?.Invoke(null, obj);

            return tmpID;
        }

        public void RemoveObjectDebug(long id)
        {
            // 제거 이벤트는 RemoveObject에서 처리합니다
            RemoveObject(id);
        }
        
         
#endif // ENABLE_CHEATING
        
        private bool TryAddReserveObject(ObjectStateInfo objStateInfo)
        {
            if (CheckReserveObject?.Invoke(objStateInfo.ObjState) ?? false)
            {
                if (!_reservedObjectInfos.TryAdd(objStateInfo.ObjState.Serial, objStateInfo))
                {
                    var oldObjStateInfo = _reservedObjectInfos[objStateInfo.ObjState.Serial];
                    objStateInfo.ObjState.MergeFrom(oldObjStateInfo.ObjState);

                    _reservedObjectInfos[objStateInfo.ObjState.Serial] = objStateInfo;
                }
                return true;
            }
            return false;
        }
        
        private void RemoveReservedObject(long serialID)
        {
            _reservedObjectInfos.Remove(serialID);
        }

        private void UpdateReservedObjects()
        {
            if (!_needToUpdateReservedObjects) return;
            _needToUpdateReservedObjects = false;
            if (_reservedObjectInfos.Count == 0) return;

            _processingReservedObjects.Clear();
            _processingReservedObjects.AddRange(_reservedObjectInfos.Values);
            _reservedObjectInfos.Clear();
            foreach (var objState in _processingReservedObjects)
            {
                ObjectStateProcess(ServerTime.Time, objState.ObjState, objState.Offset, objState.CellIndex);
            }
            _processingReservedObjects.Clear();
        }
    }
}
