/*===============================================================
* Product:    Com2Verse
* File Name:  Binder.cs
* Developer:  tlghks1009
* Date:       2022-04-14 17:35
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
================================================================*/

using System;
using System.Reflection;
using Com2Verse.Extension;
using Com2Verse.Logger;
using UnityEngine;
using UnityEngine.Serialization;

namespace Com2Verse.UI
{
    public abstract partial class Binder : MonoBehaviour, IBinding
    {
        [Serializable]
        public class BindingPath
        {
            public string propertyOwner;
            public string property;
            public Component component;

            public Type GetOwnerType() => ViewModelTypeHolder.GetType(propertyOwner);
        }

        public enum eBindingMode
        {
            ONE_TIME,
            TWO_WAY,
            ONE_WAY_TO_SOURCE,
            ONE_WAY_TO_TARGET,
        }

#if UNITY_EDITOR
        [HideInInspector] [TextArea] [SerializeField] private string _comment;

        [HideInInspector] [SerializeField] private string _targetFindKey;
        [HideInInspector] [SerializeField] private string _sourceFindKey;
        [HideInInspector] [SerializeField] private string _eventFindKey;
#endif
        [FormerlySerializedAs("_bindingTarget")]
        [HideInInspector] [SerializeField] protected BindingPath _targetPath;

        [FormerlySerializedAs("_event")]
        [HideInInspector] [SerializeField] protected BindingPath _eventPath;

        [FormerlySerializedAs("_source")]
        [HideInInspector] [SerializeField] protected BindingPath _sourcePath;

        [HideInInspector] [SerializeField] protected bool _syncOnAwake = false;
        [HideInInspector] [SerializeField] protected eBindingMode _bindingMode;

        protected object SourceOwnerOfOneWayTarget { get; set; }
        protected object TargetOwnerOfOneWayTarget { get; set; }
        protected object SourceOwnerOfOneWaySource { get; set; }
        protected object TargetOwnerOfOneWaySource { get; set; }

        protected PropertyInfo SourcePropertyInfoOfOneWayTarget { get; set; }
        protected PropertyInfo TargetPropertyInfoOfOneWayTarget { get; set; }
        protected PropertyInfo SourcePropertyInfoOfOneWaySource { get; set; }
        protected PropertyInfo TargetPropertyInfoOfOneWaySource { get; set; }

        protected string SourcePropertyName => _sourcePath.property;
        protected string TargetPropertyName => _targetPath.property;
        protected string EventPropertyName => _eventPath.property;

        public Type SourceOwnerType => _sourcePath.GetOwnerType();

        public virtual void Execute() { }

        public virtual void Bind() => Unbind();

        public virtual void Unbind() => Reset();

        protected virtual void OneTimeToTarget() => InitializeTarget();

        protected virtual void OneWayToTarget() => InitializeTarget();

        protected virtual void OneWayToSource() => InitializeSource();


        protected virtual void InitializeTarget()
        {
            try
            {
                SourceOwnerOfOneWayTarget = SourceViewModel;
                TargetOwnerOfOneWayTarget = _targetPath.component;

                if (!PropertyPathAccessors.IsExistsGetter(SourceOwnerOfOneWayTarget, SourcePropertyName))
                    SourcePropertyInfoOfOneWayTarget = GetProperty(SourceOwnerOfOneWayTarget.GetType(), SourcePropertyName);

                if (!PropertyPathAccessors.IsExistsSetter(TargetOwnerOfOneWayTarget, TargetPropertyName))
                    TargetPropertyInfoOfOneWayTarget = GetProperty(TargetOwnerOfOneWayTarget.GetType(), TargetPropertyName);
            }
            catch (Exception e)
            {
                C2VDebug.LogError($"{this.transform.GetFullPathInHierachy()} - {e.Message}");
            }
        }


        protected virtual void InitializeSource()
        {
            try
            {
                SourceOwnerOfOneWaySource = _targetPath.component;
                TargetOwnerOfOneWaySource = SourceViewModel;

                if (!PropertyPathAccessors.IsExistsGetter(SourceOwnerOfOneWaySource, TargetPropertyName))
                    SourcePropertyInfoOfOneWaySource = GetProperty(SourceOwnerOfOneWaySource.GetType(), TargetPropertyName);

                if (!PropertyPathAccessors.IsExistsSetter(TargetOwnerOfOneWaySource, SourcePropertyName))
                    TargetPropertyInfoOfOneWaySource = GetProperty(TargetOwnerOfOneWaySource.GetType(), SourcePropertyName);
            }
            catch (Exception e)
            {
                C2VDebug.LogError($"{this.transform.GetFullPathInHierachy()} - {e.Message}");
            }
        }

        protected ViewModel SourceViewModel => GetViewModel(_sourcePath);
        protected ViewModel TargetViewModel => GetViewModel(_targetPath);
        protected ViewModel GetViewModel(BindingPath bindingPath)
        {
            var ownerType = bindingPath.GetOwnerType();
            if (ownerType == null)
            {
                C2VDebug.LogWarning($"[Binder] OwnerType not found. objectName : {MVVMUtil.GetFullPathInHierarchy(this.transform)}");
                return null;
            }

            if (_viewModelContainer == null)
            {
                C2VDebug.LogWarning($"[Binder] _viewModelContainer is null : {MVVMUtil.GetFullPathInHierarchy(this.transform)}");
                return null;
            }

            var tryGetViewModel = _viewModelContainer!.TryGetViewModel(ownerType, out var viewModel);

            if (tryGetViewModel)
            {
                return viewModel;
            }

            C2VDebug.LogWarning($"[DataBinder] ViewModel not found. OwnerType : {ownerType} viewModelName : {_sourcePath.propertyOwner} " +
                                $"/ ObjectPath : {MVVMUtil.GetFullPathInHierarchy(this.transform)}");
            return null;
        }

        public static PropertyInfo GetProperty(Type type, string propertyName)
        {
            if (type == null)
            {
                return null;
            }

            try
            {
                return type.GetProperty(propertyName);
            }
            catch (AmbiguousMatchException e)
            {
                PropertyInfo result;
                for (result = null; result == null && type != null; type = type.BaseType)
                {
                    result = type.GetProperty(propertyName);
                }

                C2VDebug.LogError(e.Message);
                return result;
            }
            catch (Exception e)
            {
                C2VDebug.LogError(e.Message);
                return null;
            }
        }

#region ViewModelContainer
        private ViewModelContainer _viewModelContainer;

        public Binder SetViewModelContainer(ViewModelContainer viewModelContainer, bool allowDuplicate = false)
        {
            _viewModelContainer = viewModelContainer;
            _viewModelContainer.CreateInstanceOfViewModel(this, allowDuplicate);

            return this;
        }
#endregion ViewModelContainer

        protected virtual void OnDestroy() => Reset();

        private void Reset()
        {
            UnsubscribeToTarget();
            UnsubscribeToSource();

            SourceOwnerOfOneWayTarget = null;
            TargetOwnerOfOneWayTarget = null;
            SourceOwnerOfOneWaySource = null;
            TargetOwnerOfOneWaySource = null;

            SourcePropertyInfoOfOneWayTarget = null;
            TargetPropertyInfoOfOneWayTarget = null;
            SourcePropertyInfoOfOneWaySource = null;
            TargetPropertyInfoOfOneWaySource = null;
        }
    }
}
