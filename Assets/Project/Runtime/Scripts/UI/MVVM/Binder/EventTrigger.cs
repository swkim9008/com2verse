/*===============================================================
* Product:		Com2Verse
* File Name:	EventTrigger.cs
* Developer:	tlghks1009
* Date:			2022-05-16 11:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Extension;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.UI
{
    [AddComponentMenu("[DB]/[DB] EventTrigger")]
    public sealed class EventTrigger : Binder
    {
        public enum eTargetTriggerType
        {
            VIEW_MODEL,
            UNITY_COMPONENT
        }

        public enum eEventType
        {
            UNITY_EVENT,
            VIEW_MODEL_HANDLER,
            ACTIVE_TOGGLE,
            ACTIVE_TOGGLE_REVERSE,
            SET_ACTIVE_OFF,
            SET_ACTIVE_ON,
        }

        public enum eEventTriggerConditionType
        {
            NONE,
            BOOL,
            STRING,
            INT,
            BOOL_ALL,
            LONG,
        }

        [SerializeField] private bool _playOnBind = true;
        [HideInInspector] [SerializeField] private eTargetTriggerType _targetTriggerType;
        [HideInInspector] [SerializeField] private eEventTriggerConditionType _eventTriggerConditionType;
        [HideInInspector] [SerializeField] private bool _triggerBoolValue;
        [HideInInspector] [SerializeField] private string _triggerStringValue;
        [HideInInspector] [SerializeField] private int _triggerIntValue;
        [HideInInspector] [SerializeField] private long _triggerLongValue;
        [HideInInspector] [SerializeField] private eEventType _eventType;
        [HideInInspector] [SerializeField] private BindingPath _viewModelProperty;
        [HideInInspector] [SerializeField] private CommandBinder.BindingParameter _bindingParameter;
        [HideInInspector] [SerializeField] private UnityEvent _onEventTriggerEvent;


        public bool BoolTrigger
        {
            set
            {
                if (_triggerBoolValue == value)
                    Invoke();

                switch (_eventType)
                {
                    case eEventType.ACTIVE_TOGGLE:
                        SetActive(_triggerBoolValue == value);
                        break;
                    case eEventType.ACTIVE_TOGGLE_REVERSE:
                        SetActive(_triggerBoolValue = !value);
                        break;
                }
            }
        }

        public string StringTrigger
        {
            set
            {
                if (_triggerStringValue == value)
                    Invoke();

                switch (_eventType)
                {
                    case eEventType.ACTIVE_TOGGLE:
                        SetActive(_triggerStringValue.Equals(value));
                        break;
                    case eEventType.ACTIVE_TOGGLE_REVERSE:
                        SetActive(!_triggerStringValue.Equals(value));
                        break;
                }
            }
        }


        public int IntTrigger
        {
            set
            {
                if (_triggerIntValue == value)
                    Invoke();

                switch (_eventType)
                {
                    case eEventType.ACTIVE_TOGGLE:
                        SetActive(_triggerIntValue == value);
                        break;
                    case eEventType.ACTIVE_TOGGLE_REVERSE:
                        SetActive(_triggerIntValue != value);
                        break;
                }
            }
        }


        public long LongTrigger
        {
            set
            {
                if (_triggerLongValue == value)
                    Invoke();

                switch (_eventType)
                {
                    case eEventType.ACTIVE_TOGGLE:
                        SetActive(_triggerLongValue == value);
                        break;
                    case eEventType.ACTIVE_TOGGLE_REVERSE:
                        SetActive(_triggerLongValue != value);
                        break;
                }
            }
        }

        public bool BoolAllTrigger
        {
            set
            {
                switch (_eventType)
                {
                    case eEventType.ACTIVE_TOGGLE:
                        SetActive(value);
                        break;
                    case eEventType.ACTIVE_TOGGLE_REVERSE:
                        SetActive(!value);
                        break;
                    default:
                        Invoke();
                        break;
                }
            }
        }

        private void SetActive(bool active)
        {
            var uiView = gameObject.GetComponent<GUIView>();
            if (!uiView.IsUnityNull())
                uiView.SetActive(active);
            else
                gameObject.SetActive(active);
        }


        public override void Bind()
        {
            base.Bind();

            Subscribe();
        }


        private void Subscribe()
        {
            switch (_targetTriggerType)
            {
                case eTargetTriggerType.VIEW_MODEL:
                {
                    InitializeTarget();

                    Subscribe(eBindingMode.ONE_WAY_TO_TARGET);

                    if (_playOnBind)
                        UpdateTargetProp();
                }
                    break;

                case eTargetTriggerType.UNITY_COMPONENT:
                {
                    InitializeSource();

                    Subscribe(eBindingMode.ONE_WAY_TO_SOURCE);

                    if (_playOnBind)
                        UpdateSourceProp();
                }
                    break;
            }
        }

        protected override void InitializeTarget()
        {
            SourceOwnerOfOneWayTarget = SourceViewModel;
            SourcePropertyInfoOfOneWayTarget = GetProperty(SourceOwnerOfOneWayTarget.GetType(), SourcePropertyName);

            TargetOwnerOfOneWayTarget = this;
            TargetPropertyInfoOfOneWayTarget = GetProperty(this.GetType(), GetPropertyNameOfTrigger()!);
        }


        protected override void InitializeSource()
        {
            SourceOwnerOfOneWaySource = _targetPath.component;
            SourcePropertyInfoOfOneWaySource = GetProperty(_targetPath.component.GetType(), TargetPropertyName);

            TargetOwnerOfOneWaySource = this;
            TargetPropertyInfoOfOneWaySource = GetProperty(this.GetType(), GetPropertyNameOfTrigger());
        }


        private void Invoke()
        {
            switch (_eventType)
            {
                case eEventType.UNITY_EVENT:
                {
                    try
                    {
                        _onEventTriggerEvent?.Invoke();
                    }
                    catch (Exception)
                    {
                        throw new Exception($"EventTrigger Binding Error. GameObject Name : {this.gameObject.name} MethodName : {_onEventTriggerEvent.GetPersistentMethodName(0)}");
                    }
                }
                    break;
                case eEventType.VIEW_MODEL_HANDLER:
                {
                    var targetOwner = GetViewModel(_viewModelProperty);
                    var targetPropertyInfo = targetOwner.GetType().GetProperty(_viewModelProperty.property!);
                    var sourceAdditionalData = _bindingParameter?.GetValue();

                    RaiseCommand(targetPropertyInfo, targetOwner, sourceAdditionalData);
                }
                    break;

                case eEventType.SET_ACTIVE_ON:
                    SetActive(true);
                    break;
                case eEventType.SET_ACTIVE_OFF:
                    SetActive(false);
                    break;
            }
        }


        private string GetPropertyNameOfTrigger()
        {
            var triggerName = _eventTriggerConditionType switch
            {
                eEventTriggerConditionType.BOOL     => nameof(BoolTrigger),
                eEventTriggerConditionType.STRING   => nameof(StringTrigger),
                eEventTriggerConditionType.BOOL_ALL => nameof(BoolAllTrigger),
                eEventTriggerConditionType.INT      => nameof(IntTrigger),
                eEventTriggerConditionType.LONG     => nameof(LongTrigger),
                _                                   => string.Empty
            };
            return triggerName;
        }
    }
}
