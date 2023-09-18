/*===============================================================
* Product:    Com2Verse
* File Name:  BaseMapObject.cs
* Developer:  haminjeong
* Date:       2023-01-02 18:38
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using Google.Protobuf.Collections;
using JetBrains.Annotations;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Com2Verse.Network
{
    public enum eMapObjectUpdateState
    {
        UPDATED,
        ADDED,
        REMOVED,
    }

    public class BaseMapObject : MonoBehaviour
    {
        /// <summary>
        /// 태그 정보를 담을 구조체
        /// </summary>
        [Serializable]
        public struct TagInfo
        {
            public string key;
            public string value;

            public TagInfo(string k, string v)
            {
                key = k;
                value = v;
            }

            public bool IsEqualTo(TagInfo other) => key == other.key && value == other.value;
        }

        /// <summary>
        /// CellState에서 주어지는 오브젝트 현재 상태를 담는 구조체
        /// </summary>
        [Serializable]
        public struct State
        {
            public long Time;
            public long Version;
            public eMapObjectUpdateState UpdateState;
            public Vector3 Position;
            public Quaternion Orientation;
            public Vector3 Scale;
            public Vector3 Velocity;

            public int  AvatarType;
            public long ObjectTypeId;
            public long SpaceObjectId;

            public AvatarCustomizeInfo AvatarCustomizeInfo;

            public int AnimatorID;
            public float Gravity;

            public int CharacterState;
            public int EmotionState;
            public long EmotionTarget;

            public List<TagInfo> TagInfos;
            public bool TagChanged;
            public List<Protocols.ObjectInteractionMessage> InteractionValues;
            public bool InteractionValueChanged;

            public string NickName;

            public bool IsExpired;

            // 변경이 일어난 것만 업데이트
            public void Update(Vector3 offset, Protocols.ObjectState state, bool isExpired = false)
            {
                IsExpired = isExpired;
                var changed = state.Changed;
                if (changed.HasFlag(Protocols.Changed.Type))
                {
                    if (state.Definition?.BaseObjectId != 0)
                        ObjectTypeId = state.Definition.BaseObjectId;
                }

                if (changed.HasFlag(Protocols.Changed.Position))
                {
                    if (state.PhysicsState?.Position == null)
                        C2VDebug.LogWarningCategory(nameof(BaseMapObject), "PhysicsState or Position is null");
                    else
                        Position = new Vector3(state.PhysicsState.Position.X * 0.01f, state.PhysicsState.Position.Y * 0.01f, state.PhysicsState.Position.Z * 0.01f) + offset;
                }

                if (changed.HasFlag(Protocols.Changed.Rotation))
                {
                    if (state.PhysicsState?.Rotation == null)
                        C2VDebug.LogWarningCategory(nameof(BaseMapObject), "PhysicsState or Rotation is null");
                    else
                        Orientation = new Quaternion(state.PhysicsState.Rotation.X * 0.005f, state.PhysicsState.Rotation.Y * 0.005f, state.PhysicsState.Rotation.Z * 0.005f, state.PhysicsState.Rotation.W *
                                                     0.005f);
                }

                if (changed.HasFlag(Protocols.Changed.Scale))
                {
                    Scale = new Vector3(state.Definition.Scale.X * 0.01f, state.Definition.Scale.Y * 0.01f, state.Definition.Scale.Z * 0.01f);
                }

                if (changed.HasFlag(Protocols.Changed.Velocity))
                {
                    if (state.PhysicsState?.Velocity != null)
                    {
                        Velocity = new Vector3(state.PhysicsState.Velocity.X * 0.001f, state.PhysicsState.Velocity.Y * 0.001f, state.PhysicsState.Velocity.Z * 0.001f);
                        Gravity  = new Vector3(0, state.PhysicsState.Velocity.Y * 0.001f, 0).magnitude;
                    }
                }

                if (changed.HasFlag(Protocols.Changed.Animator))
                {
                    AnimatorID = state.AnimatorId;
                }
                else
                    AnimatorID = 0;

                if (changed.HasFlag(Protocols.Changed.CharacterState))
                {
                    CharacterState = (int)state.CharacterState;
                }
                else
                    CharacterState = -1;

                if (changed.HasFlag(Protocols.Changed.Customize))
                {
                    var mapController = MapController.InstanceOrNull;
                    AvatarCustomizeInfo = mapController.IsReferenceNull() ? default : mapController!.AvatarDtoToStruct?.Invoke(state.ObjectAvatar) ?? default;
                }
                else
                    AvatarCustomizeInfo = default;

                if (changed.HasFlag(Protocols.Changed.Emotion))
                {
                    EmotionState = state.EmotionState.EmotionId;
                    EmotionTarget = state.EmotionState.EmotionTarget;
                }
                else
                    EmotionState = -1;

                if (!IsEqualToTagInfo(state.ObjectTags!, changed.HasFlag(Protocols.Changed.ObjectTags)))
                {
                    TagInfos ??= new List<TagInfo>();
                    TagInfos.Clear();
                    for (int index = 0; index < state.ObjectTags.Count; ++index)
                    {
                        Protocols.ObjectTag tagInfo = state.ObjectTags[index];
                        TagInfos!.Add(new TagInfo(tagInfo.Key, tagInfo.Value));
                    }

                    TagChanged = true;
                }

                if (changed.HasFlag(Protocols.Changed.Interaction))
                {
                    if (InteractionValues == null)
                        InteractionValues = new List<Protocols.ObjectInteractionMessage>();
                    else
                        InteractionValues.Clear();

                    if (state.Interaction != null)
                    {
                        for (int index = 0; index < state.Interaction.ObjectInteractions.Count; ++index)
                        {
                            InteractionValues.Add(state.Interaction.ObjectInteractions[index]);
                        }
                    }

                    InteractionValueChanged = true;
                }

                if (state.ObjectAvatar is { Nickname: { } })
                    NickName = state.ObjectAvatar.Nickname;

                if (state.Definition?.SpaceObjectId != 0)
                    SpaceObjectId = state.Definition.SpaceObjectId;
            }

            private bool IsEqualToTagInfo([NotNull] RepeatedField<Protocols.ObjectTag> newTagInfos, bool hasChangeFlag)
            {
                if (newTagInfos.Count == 0 && !hasChangeFlag) return true;
                if ((TagInfos == null || TagInfos.Count == 0) && newTagInfos.Count > 0) return false;
                if (TagInfos is { Count: > 0 } && newTagInfos.Count == 0 && hasChangeFlag) return false;
                if ((TagInfos == null || TagInfos.Count == 0) && newTagInfos.Count == 0 && hasChangeFlag) return true;
                if (TagInfos?.Count != newTagInfos.Count) return false;
                for (int index = 0; index < newTagInfos.Count; ++index)
                {
                    Protocols.ObjectTag tagInfo = newTagInfos[index];
                    var newTagInfo = new TagInfo(tagInfo.Key, tagInfo.Value);
                    if (!TagInfos[index].IsEqualTo(newTagInfo))
                        return false;
                }
                return true;
            }

            public void Copy(Vector3 offset, Protocols.ObjectState state, bool isExpired = false)
            {
                IsExpired = isExpired;
                if (state.PhysicsState.Position != null)
                {
                    Position = new Vector3(state.PhysicsState.Position.X * 0.01f, state.PhysicsState.Position.Y * 0.01f, state.PhysicsState.Position.Z * 0.01f) + offset;
                }

                if (state.PhysicsState.Rotation != null)
                {
                    Orientation = new Quaternion(state.PhysicsState.Rotation.X * 0.005f, state.PhysicsState.Rotation.Y * 0.005f, state.PhysicsState.Rotation.Z * 0.005f, state.PhysicsState.Rotation.W *
                                                 0.005f);
                }

                Scale = new Vector3(state.Definition.Scale.X * 0.01f, state.Definition.Scale.Y * 0.01f, state.Definition.Scale.Z * 0.01f);

                if (state.PhysicsState.Velocity != null)
                {
                    Velocity = new Vector3(state.PhysicsState.Velocity.X * 0.001f, state.PhysicsState.Velocity.Y * 0.001f, state.PhysicsState.Velocity.Z * 0.001f);
                    Gravity  = new Vector3(0, state.PhysicsState.Velocity.Y * 0.001f, 0).magnitude;
                }

                if (state.ObjectAvatar != null)
                    AvatarType = (int) state.ObjectAvatar.AvatarType;
                ObjectTypeId = state.Definition.BaseObjectId;

                AnimatorID = state.AnimatorId;
                CharacterState = (int)state.CharacterState;
                if (state.EmotionState != null)
                {
                    EmotionState = state.EmotionState.EmotionId;
                    EmotionTarget = state.EmotionState.EmotionTarget;
                }
                else
                    EmotionState = -1;

                if (state.ObjectAvatar != null)
                {
                    var mapController = MapController.InstanceOrNull;
                    AvatarCustomizeInfo = mapController.IsReferenceNull() ? default : mapController!.AvatarDtoToStruct?.Invoke(state.ObjectAvatar) ?? default;
                }
                else
                    AvatarCustomizeInfo = default;

                TagInfos ??= new List<TagInfo>();
                TagInfos.Clear();
                for (int index = 0; index < state.ObjectTags.Count; ++index)
                {
                    Protocols.ObjectTag tagInfo = state.ObjectTags[index];
                    TagInfos!.Add(new TagInfo(tagInfo.Key, tagInfo.Value));
                }

                TagChanged = true;

                if (InteractionValues == null)
                    InteractionValues = new List<Protocols.ObjectInteractionMessage>();
                else
                    InteractionValues.Clear();

                if (state.Interaction != null)
                {
                    for (int index = 0; index < state.Interaction.ObjectInteractions.Count; ++index)
                    {
                        InteractionValues.Add(state.Interaction.ObjectInteractions[index]);
                    }
                }

                InteractionValueChanged = true;

                if (state.ObjectAvatar is { Nickname: { } })
                    NickName = state.ObjectAvatar.Nickname;

                if (state.Definition?.SpaceObjectId != 0)
                    SpaceObjectId = state.Definition.SpaceObjectId;
            }
        }

        [Serializable]
        public struct AvatarCustomizeInfo : IEquatable<AvatarCustomizeInfo>
        {
            public int AvatarType;
            public int BodyShape;

            public Dictionary<eFaceOption, AvatarItemInfo>     FaceItems;
            public Dictionary<eFashionSubMenu, AvatarItemInfo> FashionItems;

            public bool IsDefault => AvatarType == 0 && BodyShape == 0 && FaceItems == null && FashionItems == null;

#region IEquatable
            public bool Equals(AvatarCustomizeInfo other) =>
                AvatarType == other.AvatarType              &&
                BodyShape  == other.BodyShape               &&
                FaceItems.IsKeyValueEquals(other.FaceItems) &&
                FashionItems.IsKeyValueEquals(other.FashionItems);

            public override bool Equals(object obj) => obj is AvatarCustomizeInfo other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(AvatarType, BodyShape, FaceItems, FashionItems);

            public static bool operator ==(AvatarCustomizeInfo lhs, AvatarCustomizeInfo rhs) => lhs.Equals(rhs);

            public static bool operator !=(AvatarCustomizeInfo lhs, AvatarCustomizeInfo rhs) => !(lhs == rhs);
#endregion IEquatable

            public AvatarCustomizeInfo Clone()
            {
                var instance = new AvatarCustomizeInfo
                {
                    AvatarType = AvatarType,
                    BodyShape  = BodyShape,
                };
                if (FaceItems != null)
                    instance.FaceItems = new Dictionary<eFaceOption, AvatarItemInfo>(FaceItems);
                if (FashionItems != null)
                    instance.FashionItems = new Dictionary<eFashionSubMenu, AvatarItemInfo>(FashionItems);
                return instance;
            }
        }

        [Serializable]
        public struct AvatarItemInfo : IEquatable<AvatarItemInfo>
        {
            public int Id;
            public int Color;

            public AvatarItemInfo(int id, int color)
            {
                Id = id;
                Color = color;
            }

#region IEquatable
            public bool Equals(AvatarItemInfo other) => Id == other.Id && Color == other.Color;

            public override bool Equals(object obj) => obj is AvatarItemInfo other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(Id, Color);
#endregion IEquatable
        }

        private static readonly float DistanceOfSyncServer = 0.3f;
        private static readonly float AngleOfSyncServer    = 5f;
        private static readonly int   ExpiredPacketFactor  = 10000; // 패킷을 폐기하는 기준이 되는 처리 속도
        private static readonly float AngleOfExpiredPacket = 5f; // 패킷을 폐기하는 기준이 되는 최소 각도

        private static readonly string UIRootName = "UIRoot";

        protected static readonly float FallbackHeight = 1.8f;

        protected virtual float PositionXZSmoothFactor => 1f;
        protected virtual float PositionYSmoothFactor  => 10f;
        protected virtual float RotationSmoothFactor   => 1f;

        protected static readonly float TurningAngleThreshold = 20f;

        public float UIHeight { get; protected set; } = FallbackHeight;

        [field: SerializeField, ReadOnly] public    long ObjectID     { get; protected set; }
        [field: SerializeField, ReadOnly] public    long OwnerID      { get; protected set; }
        [field: SerializeField, ReadOnly] public    int  AvatarType   { get; protected set; }
        [field: SerializeField, ReadOnly] protected long ProcessDelay { get; set; }
        [field: SerializeField, ReadOnly] public    long ObjectTypeId { get; protected set; }
        [field: SerializeField, ReadOnly] public    long SpaceObjectId { get; protected set; }

        public event Action<BaseMapObject> Initialized;
        public event Action<BaseMapObject> Enabled;
        public event Action<BaseMapObject> Updated;
        public event Action<BaseMapObject> Released;
        public event Action<BaseMapObject> InteractionValueChanged;

        public event Action<BaseMapObject, string, string> NameChanged;

        private readonly List<State> _snapShots = new();

        private static readonly State EmptyState = new()
        {
            CharacterState          = 1,
            AnimatorID              = 0,
            Scale                   = Vector3.one,
            Velocity                = Vector3.zero,
            EmotionState            = -1,
            AvatarCustomizeInfo     = default,
            TagInfos                = null,
            InteractionValues       = null,
            IsExpired               = false,
            TagChanged              = false,
            InteractionValueChanged = false,
            NickName                = null,
        };

        private State _updatedState = EmptyState;

        public State UpdatedState
        {
            get => _updatedState;
            protected set => _updatedState = value;
        }

        public bool IsInitialized { get; protected set; } = false;
        public bool IsNeedUpdate { get; protected set; } = false;

        /// <summary>
        /// 해당 값은 값이 변화한 프레임에서만 의미 있는 값을 가짐
        /// </summary>
        public int CharacterState => ControlPoints[1].CharacterState;

        [SerializeField, ReadOnly] private   Vector3 _currentTargetPosition;
        private Vector3 CurrentTargetPosition => _currentTargetPosition;
        public    Vector3 TargetPositionDiff    => CurrentTargetPosition - transform.position;

        public string Name
        {
            get => _name;
            protected set
            {
                var prevName = _name;
                if (prevName == value)
                    return;

                _name = value;
                NameChanged?.Invoke(this, prevName, _name);
            }
        }

        protected State[] ControlPoints { get; } = new State[2];

        private bool _isNeedLerpTransform;

        [field: SerializeField, ReadOnly] private bool _isForceStopUpdate = false;

        public bool IsForceStopUpdate
        {
            get => _isForceStopUpdate;
            set => _isForceStopUpdate = value;
        }

        [field: SerializeField] public List<TagInfo>      TagInfos;
        public List<Protocols.ObjectInteractionMessage> InteractionValues;

#if UNITY_EDITOR
        [Serializable]
        public struct InteractionValueStruct
        {
            public long   InteractionLinkID;
            public int    InteractionNo;
            public string InteractionValue;

            public InteractionValueStruct([NotNull] Protocols.ObjectInteractionMessage interaction)
            {
                InteractionLinkID = interaction.InteractionLinkId;
                InteractionNo     = interaction.InteractionNo;
                InteractionValue  = interaction.InteractionValue;
            }

            public Protocols.ObjectInteractionMessage ToProto() => new()
            {
                InteractionLinkId = InteractionLinkID,
                InteractionNo     = InteractionNo,
                InteractionValue  = InteractionValue,
            };

            public void FromProto([NotNull] Protocols.ObjectInteractionMessage interaction)
            {
                InteractionLinkID = interaction.InteractionLinkId;
                InteractionNo     = interaction.InteractionNo;
                InteractionValue  = interaction.InteractionValue;
            }
        }
        public List<InteractionValueStruct> InteractionValueStructs  = new();
        public bool                         ApplyToInteractionValues = false;

        private void OnValidate()
        {
            if (ApplyToInteractionValues)
            {
                ApplyToInteractionValues = false;
                InteractionValues        = new List<Protocols.ObjectInteractionMessage>();
                for (int i = 0; i < InteractionValueStructs?.Count; ++i)
                    InteractionValues.Add(InteractionValueStructs[i].ToProto());
            }
        }
#endif
        private Transform _uiRoot;
        public Transform UIRoot => _uiRoot.IsReferenceNull() ? GetUIRoot() : _uiRoot;

        [field: SerializeField, ReadOnly] public Vector2 CellIndex = Vector2.zero;

        private Rigidbody           _triggerRigidbody;
        private CapsuleCollider     _triggerColliderInNavigating;
        private CharacterController _characterController;

        [field: ReadOnly, SerializeField] protected bool _isNavigating = true;

        [SerializeField, ReadOnly] private string _name;

        public bool IsNavigating
        {
            get => _isNavigating;
            set
            {
                if (!IsMine) return;
                if (!_isNeedLerpTransform) return;
                if (_isNavigating && !value) MapController.Instance.StatePublisher?.UpdateCurrentState();
                _isNavigating = value;
                if (!_characterController.IsUnityNull() || TryGetComponent(out _characterController))
                    _characterController!.enabled = !_isNavigating;
                if (!_triggerColliderInNavigating.IsUnityNull() || TryGetComponent(out _triggerColliderInNavigating))
                    _triggerColliderInNavigating!.enabled = _isNavigating;
                if (!_triggerRigidbody.IsUnityNull() || TryGetComponent(out _triggerRigidbody))
                    _triggerRigidbody!.detectCollisions = _isNavigating;
            }
        }

        /// <summary>
        /// 현재 클라이언트가 조종하는 오브젝트인 경우 true를 반환
        /// </summary>
        public bool IsMine
        {
            get
            {
                if (!MapController.InstanceExists) return false;
                return MapController.Instance.UserID == OwnerID && MapController.Instance.UserSerialID == ObjectID;
            }
        }

        protected bool IsTurning { get; set; }

#if ENABLE_CHEATING
        public bool IsDebug { get; set; } = false;
#endif // ENABLE_CHEATING

        /// <summary>
        /// 오브젝트 생성 후 초기화합니다.
        /// </summary>
        /// <param name="serial">Serial ID</param>
        /// <param name="ownerID">유저 ID</param>
        /// <param name="needUpdate">지속적인 업데이트가 필요한지 여부(ActiveObject에 해당)</param>
        public virtual void Init(long serial, long ownerID, bool needUpdate)
        {
            ObjectID = serial;
            OwnerID = ownerID;
            IsNeedUpdate = needUpdate;

            Initialized?.Invoke(this);
        }

        public void EnableObject(Transform parent)  => OnObjectEnabled(parent);
        public void ReleaseObject() => OnObjectReleased();
        public void UpdateObject()  => ProcessUpdate();

        /// <summary>
        /// 오브젝트 UI가 표시될 기준점을 생성하고 얻어옵니다.
        /// </summary>
        /// <returns>UI 기준점 Transform</returns>
        public virtual Transform GetUIRoot()
        {
            if (!_uiRoot.IsUnityNull())
            {
                _uiRoot!.localPosition = new Vector3(0, UIHeight, 0);
                return _uiRoot;
            }

            Transform uiRoot = transform.Find(UIRootName);
            if (!uiRoot.IsUnityNull())
            {
                _uiRoot = uiRoot;

                _uiRoot!.localPosition = new Vector3(0, UIHeight, 0);
                return uiRoot;
            }

            GameObject newUIRoot = new GameObject(UIRootName);
            newUIRoot.transform.SetParent(transform);
            newUIRoot.transform.localPosition = new Vector3(0, UIHeight, 0);
            newUIRoot.transform.localScale = Vector3.one;
            uiRoot = newUIRoot.transform;
            _uiRoot = uiRoot;

            return _uiRoot;
        }

        /// <summary>
        /// CellState나 CellStateUpdate에 의해 넘어오는 오브젝트의 상태값을 받습니다.
        /// </summary>
        /// <param name="offset">Cell의 기준점 위치에 해당하는 offset 값</param>
        /// <param name="time">현재 서버 시간</param>
        /// <param name="state">오브젝트 상태</param>
        /// <param name="updateState">업데이트 타입</param>
        public void ProcessState(Vector3 offset, long time, Protocols.ObjectState state, eMapObjectUpdateState updateState = eMapObjectUpdateState.UPDATED)
        {
            var version = state.Version;

            if (_snapShots?.Count == 0)
            {
                State newSnapShot = _updatedState;

                newSnapShot.Time = time;
                newSnapShot.Version = version;
                newSnapShot.UpdateState = updateState;

                if (updateState == eMapObjectUpdateState.UPDATED)
                {
                    newSnapShot.Update(offset, state);
                }
                else
                {
                    newSnapShot.Copy(offset, state);
                }

                _updatedState.Time = time - ServerTime.SyncInterval;
                _snapShots.Add(_updatedState);
                _snapShots.Add(newSnapShot);

                _updatedState = newSnapShot;
            }
            else
            {
                State newSnapShot;
                int index;
                for (index = _snapShots.Count - 1; index >= 0;--index)
                {
                    var snapShot = _snapShots[index];

                    if (snapShot.Version <= version) // 데이터를 입력할 위치
                    {
                        break;
                    }
                }

                bool isExpired = false;
                if (index >= 0)
                {
                    if (_snapShots[index].Time > time)
                    {
                        if (index < _snapShots.Count - 1)
                        {
                            time = (_snapShots[index].Time + _snapShots[index + 1].Time) / 2;
                        }
                        else // 새로운 데이터
                        {
                            time = _snapShots[index].Time + (ServerTime.SyncInterval / 2);
                        }
                    }
                    newSnapShot = _snapShots[index];
                }
                else // 받아온 데이터가 너무 오래 됨
                {
                    if (_snapShots[index + 1].Time < time)
                    {
                        time = _snapShots[index + 1].Time - (ServerTime.SyncInterval / 2);
                    }
                    newSnapShot = _snapShots[index + 1];
                    isExpired = true;
                }

                if (updateState == eMapObjectUpdateState.UPDATED)
                {
                    newSnapShot.Update(offset, state, isExpired);
                }
                else
                {
                    newSnapShot.Copy(offset, state, isExpired);
                }

                newSnapShot.Time        = time;
                newSnapShot.Version     = version;
                newSnapShot.UpdateState = updateState;

                _snapShots.Insert(index + 1, newSnapShot);

                _updatedState = newSnapShot;
            }
        }

        /// <summary>
        /// Update 주기로 축적된 State를 이용해 보간을 합니다.
        /// </summary>
        /// <returns>이번 주기에 업데이트가 되었는지 여부</returns>
        private void ProcessUpdate()
        {
            if (CanProcessUpdate())
                OnObjectUpdated();

            Updated?.Invoke(this);
        }

        protected virtual bool CanProcessUpdate()
        {
#if ENABLE_CHEATING
            if (IsDebug) return false;
#endif // ENABLE_CHEATING

            if (!FindControlPoints(ServerTime.Time)) return false;

            return true;
        }

        protected virtual void OnObjectUpdated()
        {
            long serverTime = Math.Min(ServerTime.Time, ControlPoints[1].Time + ServerTime.SyncInterval);
            ProcessDelay = ServerTime.Time - ControlPoints[1].Time;

            if (ControlPoints[1].AvatarType != 0)
                AvatarType = ControlPoints[1].AvatarType;

            if (ControlPoints[1].ObjectTypeId != 0)
                ObjectTypeId = ControlPoints[1].ObjectTypeId;

            if (ControlPoints[1].SpaceObjectId != 0)
                SpaceObjectId = ControlPoints[1].SpaceObjectId;

            _currentTargetPosition = ControlPoints[1].Position;
            var targetRotation = MathUtil.SphericalInterpolation(ControlPoints[0].Time, ControlPoints[0].Orientation, ControlPoints[1].Time, ControlPoints[1].Orientation, serverTime);
            var targetScale    = MathUtil.LinearInterpolation(ControlPoints[0].Time, ControlPoints[0].Scale, ControlPoints[1].Time, ControlPoints[1].Scale, serverTime);
            if (!IsInitialized)
            {
                targetRotation = ControlPoints[1].Orientation;
                targetScale    = ControlPoints[1].Scale;
            }

            if (ControlPoints[0].IsExpired) // 만료된 패킷은 Physics 값을 폐기한다
            {
                if (ControlPoints[1].IsExpired)
                {
                    _currentTargetPosition = transform.position;
                    targetRotation         = transform.rotation;
                    targetScale            = transform.localScale;
                }
                else
                {
                    _currentTargetPosition = ControlPoints[1].Position;
                    targetRotation         = ControlPoints[1].Orientation;
                    targetScale            = ControlPoints[1].Scale;
                }
            }

            if (ControlPoints[1].TagChanged)
            {
                TagInfos ??= new List<TagInfo>();
                TagInfos.Clear();
                TagInfos.AddRange(ControlPoints[1].TagInfos);

                _updatedState.TagChanged = false;
                TagProcessorManager.Instance.TagProcess(this);
                TagChanged();
            }

            if (ControlPoints[1].InteractionValueChanged)
            {
                InteractionValues = ControlPoints[1].InteractionValues;
#if UNITY_EDITOR
                InteractionValueStructs = new();
                for (int i = 0; i < InteractionValues?.Count; ++i)
                    InteractionValueStructs.Add(new InteractionValueStruct(InteractionValues[i]!));
#endif
                _updatedState.InteractionValueChanged = false;
                InteractionValueChanged?.Invoke(this);
            }

            if (_isForceStopUpdate) return;

            var deltaTime = ServerTime.DeltaTime(Time.unscaledDeltaTime);

            var deltaAngle = Vector3.Angle(transform.rotation.eulerAngles, targetRotation.eulerAngles);
            // 서버와 클라간의 싱크타이밍 차이로 추가보간을 한다
            if (_isNeedLerpTransform)
            {
                IsTurning = deltaAngle > TurningAngleThreshold;
                if (!IsMine || IsNavigating)
                {
                    var currentPosition = transform.position;

                    var positionX = Mathf.Lerp(currentPosition.x, CurrentTargetPosition.x, deltaTime * PositionXZSmoothFactor);
                    var positionY = Mathf.Lerp(currentPosition.y, CurrentTargetPosition.y, deltaTime * PositionYSmoothFactor);
                    var positionZ = Mathf.Lerp(currentPosition.z, CurrentTargetPosition.z, deltaTime * PositionXZSmoothFactor);

                    transform.SetPositionAndRotation(new Vector3(positionX, positionY, positionZ),
                                                     Quaternion.Lerp(transform.rotation, targetRotation, Mathf.Min(deltaTime * RotationSmoothFactor, 1.0f)));
                }

                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, deltaTime);
            }
            else
            {
                IsTurning = false;
                var deltaPosition = (CurrentTargetPosition - transform.position).sqrMagnitude;
                transform.SetPositionAndRotation(CurrentTargetPosition, targetRotation);
                transform.localScale = targetScale;
                
                if (IsNeedUpdate && deltaPosition < DistanceOfSyncServer && deltaAngle < AngleOfSyncServer)
                    _isNeedLerpTransform = true;
            }

            if (!IsInitialized)
                IsInitialized = true;
        }

        protected virtual void TagChanged() { }

        protected bool FindControlPoints(long serverTime)
        {
            if (_snapShots?.Count == 0) // 아직 데이터가 쌓이지 않음
            {
                ControlPoints[0] = _updatedState;
                ControlPoints[1] = _updatedState;
                ControlPoints[0].Time = serverTime - (ServerTime.SyncInterval / 2);
                ControlPoints[1].Time = serverTime + (ServerTime.SyncInterval / 2);
                return true;
            }

            if (serverTime < _snapShots[0].Time) // 미래의 데이터가 도착(지금 처리할 필요가 없음)
            {
                return false;
            }

            ControlPoints[0] = _snapShots[0];
            if (_snapShots.Count == 1)
            {
                ControlPoints[1] = _snapShots[0];
                ControlPoints[1].Time += ServerTime.SyncInterval / 2;
                _snapShots.Clear();
            }
            else
            {
                ControlPoints[1] = _snapShots[1];
                _snapShots.RemoveAt(0);
                _snapShots.RemoveAt(0);
                var   expiredTime  = serverTime - (int)(Time.unscaledDeltaTime * ExpiredPacketFactor);
                float currentAngle = 0f;
                for (int i = 0; i < _snapShots.Count; ++i)
                {
                    if (_snapShots.Count > 1)
                        currentAngle = Quaternion.Angle(_snapShots[0].Orientation, _snapShots[1].Orientation);
                    if (_snapShots[0].Time < expiredTime && currentAngle < AngleOfExpiredPacket) // 오래된 데이터 폐기
                    {
                        C2VDebug.LogCategory("BaseMapObject", $"packet removed exceed time {expiredTime - _snapShots[0].Time}, angle {currentAngle}");
                        _snapShots.RemoveAt(0);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 오브젝트가 활성화될때 호출됩니다.
        /// </summary>
        protected virtual void OnObjectEnabled(Transform parent)
        {
            _isNeedLerpTransform = false;
            _isForceStopUpdate = false;
            GetUIRoot();

            Enabled?.Invoke(this);
        }

        /// <summary>
        /// 보간 처리를 지정된 시간동안 멈춥니다.
        /// </summary>
        /// <param name="milliseconds">보간을 중지할 시간(ms)</param>
        public async UniTask NeedLerpOffWaitTime(int milliseconds = 10)
        {
            _isNeedLerpTransform = false;
            await UniTask.Delay(TimeSpan.FromMilliseconds(milliseconds), DelayType.Realtime);
            _isNeedLerpTransform = true;
        }

        /// <summary>
        /// 보간을 강제로 중지하고 CellStateUpdate도 무시할지를 세팅합니다.
        /// </summary>
        /// <param name="isOn">true이면 보간과 CellStateUpdate를 다시 시작합니다.</param>
        public void ForceSetUpdateEnable(bool isOn)
        {
            _isForceStopUpdate = !isOn;
            _isNeedLerpTransform = isOn;
        }

        /// <summary>
        /// 오브젝트의 현재 위치를 강제로 고정시킵니다.
        /// </summary>
        public void ForceSetCurrentPositionToState()
        {
            _updatedState.Position = transform.position;
            _updatedState.Orientation = transform.rotation;
            if (_snapShots?.Count > 0)
            {
                _snapShots.ForEach((state) =>
                {
                    state.Position = transform.position;
                    state.Orientation = transform.rotation;
                });
            }
        }

        protected void ForceSetCharacterStateToState(int characterState)
        {
            _updatedState.CharacterState = characterState;
            if (_snapShots?.Count > 0)
            {
                _snapShots.ForEach((state) =>
                {
                    state.CharacterState = characterState;
                });
            }
        }

        /// <summary>
        /// 오브젝트가 풀에 들어갈때나 삭제되면 호출됩니다.
        /// </summary>
        protected virtual void OnObjectReleased()
        {
            Released?.Invoke(this);
            TagInfos?.Clear();
            TagProcessorManager.InstanceOrNull?.TagProcess(this);
            TagChanged();

            InteractionValues?.Clear();
#if UNITY_EDITOR
            InteractionValueStructs?.Clear();
#endif
            InteractionValueChanged?.Invoke(this);
            InteractionValueChanged = null;

            if (!_characterController.IsUnityNull())
            {
                DestroyImmediate(_characterController);
                _characterController = null;
            }

            if (!_triggerColliderInNavigating.IsUnityNull())
            {
                DestroyImmediate(_triggerColliderInNavigating);
                _triggerColliderInNavigating = null;
            }

            if (!_triggerRigidbody.IsUnityNull())
            {
                DestroyImmediate(_triggerRigidbody);
                _triggerRigidbody = null;
            }

            _uiRoot = null;

            _name    = null;
            ObjectID = OwnerID = 0;
            _snapShots?.Clear();
            IsInitialized        = false;
            _isNeedLerpTransform = false;
            _updatedState        = EmptyState;
            ControlPoints[0]     = EmptyState;
            ControlPoints[1]     = EmptyState;
        }

        public bool IsContainsTagKey(string key)
        {
            if (TagInfos == null) return false;
            for (int i = 0; i < TagInfos.Count; ++i)
                if (TagInfos[i].key.Equals(key))
                    return true;
            return false;
        }

        public string GetStringFromTags(string key)
        {
            if (TagInfos == null) return null;
            if (string.IsNullOrEmpty(key)) return null;
            for (int i = 0; i < TagInfos.Count; ++i)
            {
                if (!TagInfos[i].key.Equals(key)) continue;
                return TagInfos[i].value;
            }
            return null;
        }
    }
}
