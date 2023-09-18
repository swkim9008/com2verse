/*===============================================================
* Product:    Com2Verse
* File Name:  CommandBinder.cs
* Developer:  tlghks1009
* Date:       2022-03-22 15:11
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
    [AddComponentMenu("[DB]/[DB] Command Binder")]
    public class CommandBinder : Binder
    {
        public enum eToggleCondition
        {
            IS_ON,
            IS_OFF,
            PASS,
        }

        [HideInInspector] [SerializeField] private BindingParameter _bindingParameter;
        [HideInInspector] [SerializeField] private eToggleCondition _toggleCondition = eToggleCondition.PASS;

        private object _additionalData;

#region Properties
        [UsedImplicitly]
        public string StringValue
        {
            get => _bindingParameter?.stringValue ?? string.Empty;
            set
            {
                if (_bindingParameter == null) return;
                if (_bindingParameter.stringValue == value) return;

                _bindingParameter.stringValue = value;

                Bind();
            }
        }

        [UsedImplicitly]
        public int IntValue
        {
            get => _bindingParameter?.intValue ?? 0;
            set
            {
                if (_bindingParameter == null) return;
                if (_bindingParameter.intValue == value) return;

                _bindingParameter.intValue = value;

                Bind();
            }
        }

        [UsedImplicitly]
        public long LongValue
        {
            get => _bindingParameter?.longValue ?? 0;
            set
            {
                if (_bindingParameter == null) return;
                if (_bindingParameter.longValue == value) return;

                _bindingParameter.longValue = value;

                Bind();
            }
        }

        [UsedImplicitly]
        public float FloatValue
        {
            get => _bindingParameter?.floatValue ?? 0f;
            set
            {
                if (_bindingParameter == null) return;
                if (Mathf.Approximately(_bindingParameter.floatValue, value)) return;

                _bindingParameter.floatValue = value;

                Bind();
            }
        }

        [UsedImplicitly]
        public bool BoolValue
        {
            get => _bindingParameter?.boolValue ?? false;
            set
            {
                if (_bindingParameter == null) return;
                if (_bindingParameter.boolValue == value) return;

                _bindingParameter.boolValue = value;

                Bind();
            }
        }

        [UsedImplicitly]
        public UnityEngine.Object ObjectReference
        {
            get => _bindingParameter?.objectReference;
            set
            {
                if (_bindingParameter == null) return;
                if (_bindingParameter.objectReference == value) return;

                _bindingParameter.objectReference = value;

                Bind();
            }
        }
#endregion Properties


        public override void Bind()
        {
            base.Bind();

            Execute();
        }


        public override void Execute() => OneWayToSource();

        protected override void OneWayToSource()
        {
            base.OneWayToSource();

            if (ReferenceEquals(_targetPath.component, null))
            {
                C2VDebug.LogError($"[CommandBinder] BindingTarget is empty. GameObject : {this.gameObject.name}");
                return;
            }

            Subscribe(eBindingMode.ONE_WAY_TO_SOURCE);
        }

        protected override void InitializeSource()
        {
            base.InitializeSource();

            _additionalData = _bindingParameter?.GetValue();
        }

        protected override void OnUpdateSource() => RaiseCommand();

        protected override void OnUpdateSource(string updateValue) => RaiseCommand();

        protected override void OnUpdateSource(float updateValue) => RaiseCommand();

        protected override void OnUpdateSource(int updateValue) => RaiseCommand();

        protected override void OnUpdateSource(Transform updateValue) => RaiseCommand();

        protected override void OnUpdateSource(bool updateValue)
        {
            switch (_toggleCondition)
            {
                case eToggleCondition.IS_ON:
                    if (!updateValue) return;
                    break;

                case eToggleCondition.IS_OFF:
                    if (updateValue) return;
                    break;
            }

            RaiseCommand();
        }

        private void RaiseCommand() => base.RaiseCommand(TargetPropertyInfoOfOneWaySource, TargetOwnerOfOneWaySource, _additionalData);

        [Serializable]
        public class BindingParameter
        {
            public eBindingParameterType bindingParameterType;
            public UnityEngine.Object objectReference;
            public string stringValue;
            public int intValue;
            public long longValue;
            public float floatValue;
            public bool boolValue;

            public object GetValue()
            {
                switch (bindingParameterType)
                {
                    case eBindingParameterType.BOOLEAN:          return boolValue;
                    case eBindingParameterType.FLOAT:            return floatValue;
                    case eBindingParameterType.INT:              return intValue;
                    case eBindingParameterType.LONG:             return longValue;
                    case eBindingParameterType.OBJECT_REFERENCE: return objectReference;
                    case eBindingParameterType.STRING:           return stringValue;
                    default:
                        return null;
                }
            }
        }

        public enum eBindingParameterType
        {
            NONE,
            OBJECT_REFERENCE,
            STRING,
            INT,
            FLOAT,
            BOOLEAN,
            LONG,
        }
    }
}
