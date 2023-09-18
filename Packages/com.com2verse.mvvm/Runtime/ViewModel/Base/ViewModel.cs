/*===============================================================
* Product:    Com2Verse
* File Name:  ViewModel.cs
* Developer:  tlghks1009
* Date:       2022-04-01 16:25
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
    public sealed class ViewModelGroupAttribute : Attribute
    {
        public ViewModelGroupAttribute(string name) => Name = $"[{name}]";

        public string Name { get; } = string.Empty;
    }

    public abstract partial class ViewModel
    {
        public string ViewModelInfo => $"{GetType().Name}({GetHashCode().ToString()})";

        /// <summary>
        /// 주의: 해당 옵션이 true가 된 ViewModel은 앱이 종료되기 전까지 영원히 죽지 않고 인스턴스가 MasterViewModel에 남아있게 됩니다.
        /// <br/>정말 꼭 필요한 경우만 true로 설정해주세요. (ex: Scene 전환시에도 살아있어야 하는 ViewModel, 물리 Device에 접근하는 ViewModel 등)
        /// </summary>
        public bool DontDestroyOnLoad { get; set; } = false;

        private ViewModelContainer _container;


        public virtual void OnInitialize() { }

        public virtual void OnRelease() { }

        public void SetContainer(ViewModelContainer container) => _container = container;

        public void ForceRelease() => _container?.Remove(this);


        [Obsolete("SetProperty 함수 또는 InvokePropertyValueChanged에 Value값을 넣어주면 성능이 더욱 좋아집니다.")]
        protected void InvokePropertyValueChanged(string propertyName) => BindablePropertyDispatcher.NotifyPropertyChanged(this, propertyName);

        protected void InvokePropertyValueChanged<T>(string propertyName, T value) where T : unmanaged, IConvertible => BindablePropertyDispatcher.NotifyPropertyChanged(this, propertyName, value);

        protected void InvokePropertyValueChanged(string propertyName, [CanBeNull] object value) => BindablePropertyDispatcher.NotifyPropertyChanged(this, propertyName, value);


        protected void SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "") where T : unmanaged, IConvertible
        {
            storage = value;

            InvokePropertyValueChanged(propertyName, value);
        }

        protected void SetProperty<T>(ref T storage, [CanBeNull] object value, [CallerMemberName] string propertyName = "")
        {
            storage = (T) value;

            InvokePropertyValueChanged(propertyName, value);
        }
    }
}
