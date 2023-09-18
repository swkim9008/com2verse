/*===============================================================
* Product:    Com2Verse
* File Name:  BindableProperty.cs
* Developer:  tlghks1009
* Date:       2022-03-04 14:12
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.UI
{
    [Obsolete("더 이상 사용하지 않습니다")]
    public class BindableProperty<T>
    {
        public event Action<T> OnPropertyValueChanged;

        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyValueChanged?.Invoke(_value);
            }
        }

        public BindableProperty() { }

        public BindableProperty(T value)
        {
            _value = value;
        }
    }
}
