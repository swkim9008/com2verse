/*===============================================================
* Product:		Com2Verse
* File Name:	Binder_Subscribe.cs
* Developer:	tlghks1009
* Date:			2022-12-08 16:28
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Reflection;
using Com2Verse.Logger;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.UI
{
    public abstract partial class Binder
    {
        protected void Subscribe(eBindingMode bindingMode)
        {
            switch (bindingMode)
            {
                case eBindingMode.ONE_TIME:
                    break;
                case eBindingMode.TWO_WAY:
                {
                    SubscribeToTarget();
                    SubscribeToSource(SourceOwnerOfOneWaySource, EventPropertyName);
                }
                    break;
                case eBindingMode.ONE_WAY_TO_SOURCE:
                    SubscribeToSource(SourceOwnerOfOneWaySource, EventPropertyName);
                    break;
                case eBindingMode.ONE_WAY_TO_TARGET:
                    SubscribeToTarget();
                    break;
            }
        }
#region SubscribeToTarget
        private bool _isSubscribed;

        private void SubscribeToTarget()
        {
            if (SourceOwnerOfOneWayTarget is ViewModel)
            {
                if (_isSubscribed)
                {
                    return;
                }

                _isSubscribed = true;

                BindablePropertyDispatcher.Subscribe(SourceOwnerOfOneWayTarget, this);
            }
        }

        private void UnsubscribeToTarget()
        {
            if (SourceOwnerOfOneWayTarget is ViewModel)
            {
                if (!_isSubscribed)
                {
                    return;
                }

                _isSubscribed = false;

                BindablePropertyDispatcher.Unsubscribe(SourceOwnerOfOneWayTarget, this);
            }
        }

        public void NotifyPropertyChanged<T>(string propertyName, T value) where T : unmanaged, IConvertible
        {
            if (_sourcePath.property == propertyName)
            {
                UpdateTargetProp(value);
            }
        }

        public void NotifyPropertyChanged(string propertyName, [CanBeNull] object value)
        {
            if (_sourcePath.property == propertyName)
            {
                UpdateTargetProp(value);
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            if (_sourcePath.property == propertyName)
            {
                UpdateTargetProp();
            }
        }
#endregion ToTarget

#region SubscribeToSource
        private object _unityEvent;

        private UnityEvent _voidEventHandler;

        private UnityEvent<string> _stringEventHandler;

        private UnityEvent<bool> _boolEventHandler;

        private UnityEvent<float> _floatEventHandler;

        private UnityEvent<int> _intEventHandler;

        private UnityEvent<Transform> _transformEventHandler;

        private static readonly BindingFlags _flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        protected virtual void OnUpdateSource() => UpdateSourceProp();
        protected virtual void OnUpdateSource(string updateValue) => UpdateSourceProp();
        protected virtual void OnUpdateSource(bool updateValue) => UpdateSourceProp();
        protected virtual void OnUpdateSource(float updateValue) => UpdateSourceProp();
        protected virtual void OnUpdateSource(int updateValue) => UpdateSourceProp();
        protected virtual void OnUpdateSource(Transform updateValue) => UpdateSourceProp();

        private void SubscribeToSource(object owner, string eventName)
        {
            _unityEvent = GetUnityEvent(owner, eventName);

            if (_unityEvent == null)
            {
                return;
            }

            if (MVVMDefine.TryGetUnityEventType(_unityEvent.GetType(), out var unityEventType))
            {
                AddListener(_unityEvent, unityEventType);
            }
            else
            {
                C2VDebug.LogError($"[UnityEventRegister] Please Register the Type. owner : {owner}, type : {_unityEvent.GetType()}");
            }
        }

        private void UnsubscribeToSource()
        {
            if (_unityEvent == null)
            {
                return;
            }


            if (MVVMDefine.TryGetUnityEventType(_unityEvent.GetType(), out var unityEventType))
            {
                RemoveListener(unityEventType);
            }
        }


        private void AddListener(object unityEvent, MVVMDefine.eUnityEventType unityEventType)
        {
            switch (unityEventType)
            {
                case MVVMDefine.eUnityEventType.BUTTON_CLICK_EVENT:
                {
                    _voidEventHandler ??= unityEvent as UnityEvent;

                    _voidEventHandler.RemoveListener(OnUpdateSource);
                    _voidEventHandler.AddListener(OnUpdateSource);
                }
                    break;
                case MVVMDefine.eUnityEventType.VALUE_CHANGED_EVENT_STRING:
                {
                    _stringEventHandler ??= unityEvent as UnityEvent<string>;

                    _stringEventHandler.RemoveListener(OnUpdateSource);
                    _stringEventHandler.AddListener(OnUpdateSource);
                }
                    break;
                case MVVMDefine.eUnityEventType.VALUE_CHANGED_EVENT_BOOLEAN:
                {
                    _boolEventHandler ??= unityEvent as UnityEvent<bool>;

                    _boolEventHandler.RemoveListener(OnUpdateSource);
                    _boolEventHandler.AddListener(OnUpdateSource);
                }
                    break;

                case MVVMDefine.eUnityEventType.VALUE_CHANGED_EVENT_FLOAT:
                {
                    _floatEventHandler ??= unityEvent as UnityEvent<float>;

                    _floatEventHandler.RemoveListener(OnUpdateSource);
                    _floatEventHandler.AddListener(OnUpdateSource);
                }
                    break;

                case MVVMDefine.eUnityEventType.VALUE_CHANGED_EVENT_INT:
                {
                    _intEventHandler ??= unityEvent as UnityEvent<int>;

                    _intEventHandler.RemoveListener(OnUpdateSource);
                    _intEventHandler.AddListener(OnUpdateSource);
                }
                    break;

                case MVVMDefine.eUnityEventType.DROP_DOWN_EVENT:
                {
                    _intEventHandler ??= unityEvent as UnityEvent<int>;

                    _intEventHandler.RemoveListener(OnUpdateSource);
                    _intEventHandler.AddListener(OnUpdateSource);
                }
                    break;
                case MVVMDefine.eUnityEventType.TRANSFORM_EVENT:
                {
                    _transformEventHandler ??= unityEvent as UnityEvent<Transform>;

                    _transformEventHandler.RemoveListener(OnUpdateSource);
                    _transformEventHandler.AddListener(OnUpdateSource);
                }
                    break;
                default:
                    C2VDebug.LogWarning("Please Check the Registration of the Unity event.");
                    break;
            }
        }


        private void RemoveListener(MVVMDefine.eUnityEventType unityEventType)
        {
            switch (unityEventType)
            {
                case MVVMDefine.eUnityEventType.BUTTON_CLICK_EVENT:
                {
                    _voidEventHandler?.RemoveListener(OnUpdateSource);
                    _voidEventHandler = null;
                }
                    break;
                case MVVMDefine.eUnityEventType.VALUE_CHANGED_EVENT_STRING:
                {
                    _stringEventHandler?.RemoveListener(OnUpdateSource);
                    _stringEventHandler = null;
                }
                    break;
                case MVVMDefine.eUnityEventType.VALUE_CHANGED_EVENT_BOOLEAN:
                {
                    _boolEventHandler?.RemoveListener(OnUpdateSource);
                    _boolEventHandler = null;
                }
                    break;
                case MVVMDefine.eUnityEventType.VALUE_CHANGED_EVENT_FLOAT:
                {
                    _floatEventHandler?.RemoveListener(OnUpdateSource);
                    _floatEventHandler = null;
                }
                    break;
                case MVVMDefine.eUnityEventType.VALUE_CHANGED_EVENT_INT:
                {
                    _intEventHandler?.RemoveListener(OnUpdateSource);
                    _intEventHandler = null;
                }
                    break;
                case MVVMDefine.eUnityEventType.DROP_DOWN_EVENT:
                {
                    _intEventHandler?.RemoveListener(OnUpdateSource);
                    _intEventHandler = null;
                }
                    break;
                case MVVMDefine.eUnityEventType.TRANSFORM_EVENT:
                {
                    _transformEventHandler?.RemoveListener(OnUpdateSource);
                    _transformEventHandler = null;
                }
                    break;
                default:
                    C2VDebug.LogWarning("Please Check Unity Event Release.");
                    break;
            }
        }


        private static object GetUnityEvent(object owner, string eventName)
        {
            var fieldInfo = GetField(owner, eventName);

            return fieldInfo == null ? null : fieldInfo.GetValue(owner);
        }


        private static FieldInfo GetField(object owner, string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return null;
            }

            var ownerType = owner?.GetType();

            if (ownerType == null)
            {
                return null;
            }


            var fieldInfo = ownerType.GetField(fieldName, _flags);

            if (fieldInfo == null)
            {
                fieldInfo = owner.GetType().BaseType.GetField(fieldName, _flags);
            }

            return fieldInfo;
        }
#endregion ToSource
    }
}
