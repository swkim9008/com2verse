/*===============================================================
* Product:		Com2Verse
* File Name:	PropertyPathGen.cs
* Developer:	tlghks1009
* Date:			2023-01-10 10:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Reflection;
using Com2Verse.Extension;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UI
{
    /// <summary>
    /// 자주 사용되는 프로퍼티의 경우 등록 후 사용 - 패키지 내부
    /// ReferenceType의 경우 '명시적으로 <object> 해줄 것'
    /// </summary>
    public static class PropertyPathGen
    {
        public static void Initialize()
        {
            PropertyPathAccessors.ClearAll();

            PropertyPathAccessors.Register<object>(
                typeof(TMP_Text), "text",
                (obj) =>
                {
                    if (obj is TMP_Text tmpText)
                        return tmpText.text;

                    return string.Empty;
                },
                (obj, value) =>
                {
                    if (obj is TMP_Text tmpText) tmpText.text = value?.ToString();
                });


            PropertyPathAccessors.Register<object>(
                typeof(TMP_InputField), "text",
                (obj) =>
                {
                    if (obj is TMP_InputField tmpInputField)
                        return tmpInputField.text;

                    return string.Empty;
                },
                (obj, value) =>
                {
                    if (obj is TMP_InputField tmpInputField) tmpInputField.text = value?.ToString();
                });


            PropertyPathAccessors.Register<object>(
                typeof(TextMeshProUGUI), "text",
                (obj) =>
                {
                    if (obj is TextMeshProUGUI textMeshProUGUI)
                        return textMeshProUGUI.text;

                    return string.Empty;
                },
                (obj, value) =>
                {
                    if (obj is TextMeshProUGUI textMeshProUGUI) textMeshProUGUI.text = value?.ToString();
                });


            PropertyPathAccessors.Register(
                typeof(Toggle), "isOn",
                (obj) =>
                {
                    if (obj is Toggle toggle)
                        return toggle.isOn;

                    return false;
                },
                (obj, value) =>
                {
                    if (obj is Toggle toggle) toggle.isOn = value;
                });


            PropertyPathAccessors.Register(
                typeof(CanvasGroup), "alpha",
                (obj) =>
                {
                    if (obj is CanvasGroup canvasGroup)
                        return canvasGroup.alpha;

                    return 0;
                },
                (obj, value) =>
                {
                    if (obj is CanvasGroup canvasGroup) canvasGroup.alpha = value;
                });


            PropertyPathAccessors.Register(
                typeof(Slider), "value",
                (obj) =>
                {
                    if (obj is Slider slider)
                        return slider.value;

                    return 0;
                },
                (obj, value) =>
                {
                    if (obj is Slider slider) slider.value = value;
                });

            PropertyPathAccessors.Register<object>(
                typeof(Image), "sprite",
                (obj) =>
                {
                    if (obj is Image image)
                        return image.sprite;

                    return null;
                },
                (obj, value) =>
                {
                    if (obj is Image image) image.sprite = value as Sprite;
                });

            PropertyPathAccessors.Register<object>(
                typeof(RectTransform), "anchoredPosition",
                (obj) =>
                {
                    if (obj is RectTransform rectTransform)
                        return rectTransform.anchoredPosition;

                    return Vector2.zero;
                },
                (obj, value) =>
                {
                    if (obj is RectTransform rectTransform) rectTransform.anchoredPosition = (Vector2) value;
                });
        }

        public static void InitializationComplete() => PropertyPathAccessors.Initialize();
    }


    public static class PropertyPathAccessors
    {
        private static readonly Dictionary<Type, Dictionary<string, Func<object, int>>> GettersInt = new();
        private static readonly Dictionary<Type, Dictionary<string, Action<object, int>>> SettersInt = new();

        private static readonly Dictionary<Type, Dictionary<string, Func<object, float>>>   GettersFloat = new();
        private static readonly Dictionary<Type, Dictionary<string, Action<object, float>>> SettersFloat = new();

        private static readonly Dictionary<Type, Dictionary<string, Func<object, bool>>>   GettersBool = new();
        private static readonly Dictionary<Type, Dictionary<string, Action<object, bool>>> SettersBool = new();

        private static readonly Dictionary<Type, Dictionary<string, Func<object, double>>>   GettersDouble = new();
        private static readonly Dictionary<Type, Dictionary<string, Action<object, double>>> SettersDouble = new();

        private static readonly Dictionary<Type, Dictionary<string, Func<object, decimal>>>   GettersDecimal = new();
        private static readonly Dictionary<Type, Dictionary<string, Action<object, decimal>>> SettersDecimal = new();

        private static readonly Dictionary<Type, Dictionary<string, Func<object, long>>>   GettersLong = new();
        private static readonly Dictionary<Type, Dictionary<string, Action<object, long>>> SettersLong = new();

        private static readonly Dictionary<Type, Dictionary<string, Func<object, object>>>   GettersObject = new();
        private static readonly Dictionary<Type, Dictionary<string, Action<object, object>>> SettersObject = new();

        private static bool _isInitialized;


        public static void RegisterSetter<T>(Dictionary<Type, Dictionary<string, Action<object, T>>> container, Type reference, string propertyName, Action<object, T> setter)
        {
            if (container.TryGetValue(reference, out var setterDictionary))
            {
                if (!setterDictionary.ContainsKey(propertyName))
                    setterDictionary.Add(propertyName, setter);
            }
            else
            {
                setterDictionary = new Dictionary<string, Action<object, T>> {{propertyName, setter}};
                container.Add(reference, setterDictionary);
            }
        }

        public static void RegisterGetter<T>(Dictionary<Type, Dictionary<string, Func<object, T>>> container, Type reference, string propertyName, Func<object, T> getter)
        {
            if (container.TryGetValue(reference, out var getterDictionary))
            {
                if (!getterDictionary.ContainsKey(propertyName))
                    getterDictionary.Add(propertyName, getter);
            }
            else
            {
                getterDictionary = new Dictionary<string, Func<object, T>> {{propertyName, getter}};
                container.Add(reference, getterDictionary);
            }
        }

        public static void Register<T>(Type reference, string propertyName, Func<object, T> getter, Action<object, T> setter)
        {
            switch (typeof(T))
            {
                case var t when t == typeof(int):
                {
                    RegisterGetter(GettersInt, reference, propertyName, getter as Func<object, int>);
                    RegisterSetter(SettersInt, reference, propertyName, setter as Action<object, int>);
                }
                    break;

                case var t when t == typeof(float):
                {
                    RegisterGetter(GettersFloat, reference, propertyName, getter as Func<object, float>);
                    RegisterSetter(SettersFloat, reference, propertyName, setter as Action<object, float>);
                }
                    break;

                case var t when t == typeof(bool):
                {
                    RegisterGetter(GettersBool, reference, propertyName, getter as Func<object, bool>);
                    RegisterSetter(SettersBool, reference, propertyName, setter as Action<object, bool>);
                }
                    break;

                case var t when t == typeof(long):
                {
                    RegisterGetter(GettersLong, reference, propertyName, getter as Func<object, long>);
                    RegisterSetter(SettersLong, reference, propertyName, setter as Action<object, long>);
                }
                    break;

                case var t when t == typeof(double):
                {
                    RegisterGetter(GettersDouble, reference, propertyName, getter as Func<object, double>);
                    RegisterSetter(SettersDouble, reference, propertyName, setter as Action<object, double>);
                }
                    break;

                case var t when t == typeof(decimal):
                {
                    RegisterGetter(GettersDecimal, reference, propertyName, getter as Func<object, decimal>);
                    RegisterSetter(SettersDecimal, reference, propertyName, setter as Action<object, decimal>);
                }
                    break;

                default:
                {
                    RegisterGetter(GettersObject, reference, propertyName, getter as Func<object, object>);
                    RegisterSetter(SettersObject, reference, propertyName, setter as Action<object, object>);
                }
                    break;
            }
        }


        public static void ClearAll()
        {
            GettersInt.Clear();
            SettersInt.Clear();

            GettersFloat.Clear();
            SettersFloat.Clear();

            GettersBool.Clear();
            SettersBool.Clear();

            GettersLong.Clear();
            SettersLong.Clear();

            GettersDouble.Clear();
            SettersDouble.Clear();

            GettersDecimal.Clear();
            SettersDecimal.Clear();

            GettersObject.Clear();
            SettersObject.Clear();
        }

#region Setter
        public static bool IsExistsSetter(object owner, string propertyName)
        {
            if (!IsValid(owner, propertyName))
                return false;

            var ownerType = owner.GetType();

            if (TryGetSetter<int>(SettersInt, ownerType, propertyName, out var setterInt))
                return true;

            if (TryGetSetter<float>(SettersFloat, ownerType, propertyName, out var setterFloat))
                return true;

            if (TryGetSetter<double>(SettersDouble, ownerType, propertyName, out var setterDouble))
                return true;

            if (TryGetSetter<long>(SettersLong, ownerType, propertyName, out var setterLong))
                return true;

            if (TryGetSetter<decimal>(SettersDecimal, ownerType, propertyName, out var setterDecimal))
                return true;

            if (TryGetSetter<bool>(SettersBool, ownerType, propertyName, out var setterBool))
                return true;

            if (TryGetSetter<object>(SettersObject, ownerType, propertyName, out var setterObject))
                return true;

            return false;
        }

        public static bool TrySetValue<T>(object owner, string propertyName, T value) where T : unmanaged, IConvertible
        {
            if (!IsValid(owner, propertyName))
                return false;

            var ownerType = owner.GetType();

            switch (typeof(T))
            {
                case var t when t == typeof(int):
                {
                    if (TryGetSetter<int>(SettersInt, ownerType, propertyName, out var setter))
                    {
                        setter(owner, value.CastInt());
                        return true;
                    }
                }
                    break;

                case var t when t == typeof(float):
                {
                    if (TryGetSetter<float>(SettersFloat, ownerType, propertyName, out var setter))
                    {
                        setter(owner, value.CastFloat());
                        return true;
                    }
                }
                    break;

                case var t when t == typeof(double):
                {
                    if (TryGetSetter<double>(SettersDouble, ownerType, propertyName, out var setter))
                    {
                        setter(owner, value.CastDouble());
                        return true;
                    }
                }
                    break;

                case var t when t == typeof(long):
                {
                    if (TryGetSetter<long>(SettersLong, ownerType, propertyName, out var setter))
                    {
                        setter(owner, value.CastLong());
                        return true;
                    }
                }
                    break;

                case var t when t == typeof(decimal):
                {
                    if (TryGetSetter<decimal>(SettersDecimal, ownerType, propertyName, out var setter))
                    {
                        setter(owner, value.CastDecimal());
                        return true;
                    }
                }
                    break;

                case var t when t == typeof(bool):
                {
                    if (TryGetSetter<bool>(SettersBool, ownerType, propertyName, out var setter))
                    {
                        setter(owner, value.CastBool());
                        return true;
                    }
                }
                    break;
            }

            // Target이 Tmp_Text, TextMeshProUGUI, TMP_InputField Type이고, Property가 text 일 때,
            // Source의 PropertyType이 IConvertible 인터페이스를 상속 받고 있다면(ex : int, float, 등), 바인딩 허용
            {
                if (TryGetSetter<object>(SettersObject, ownerType, propertyName, out var setter))
                {
                    setter(owner, value);
                    return true;
                }
            }

            return false;
        }

        public static bool TrySetValue(object owner, string propertyName, object value)
        {
            if (!IsValid(owner, propertyName))
                return false;

            var ownerType = owner.GetType();

            switch (value)
            {
                case int intValue:
                {
                    if (TryGetSetter<int>(SettersInt, ownerType, propertyName, out var setter))
                    {
                        setter(owner, intValue);
                        return true;
                    }
                }
                    break;

                case float floatValue:
                {
                    if (TryGetSetter<float>(SettersFloat, ownerType, propertyName, out var setter))
                    {
                        setter(owner, floatValue);
                        return true;
                    }
                }
                    break;

                case double doubleValue:
                {
                    if (TryGetSetter<double>(SettersDouble, ownerType, propertyName, out var setter))
                    {
                        setter(owner, doubleValue);
                        return true;
                    }
                }
                    break;

                case long longValue:
                {
                    if (TryGetSetter <long>(SettersLong, ownerType, propertyName, out var setter))
                    {
                        setter(owner, longValue);
                        return true;
                    }
                }
                    break;

                case decimal decimalValue:
                {
                    if (TryGetSetter<decimal>(SettersDecimal, ownerType, propertyName, out var setter))
                    {
                        setter(owner, decimalValue);
                        return true;
                    }
                }
                    break;

                case bool boolValue:
                {
                    if (TryGetSetter<bool>(SettersBool, ownerType, propertyName, out var setter))
                    {
                        setter(owner, boolValue);
                        return true;
                    }
                }
                    break;
            }

            {
                if (TryGetSetter<object>(SettersObject, owner.GetType(), propertyName, out var setter))
                {
                    setter(owner, value);
                    return true;
                }
            }
            //Debug.LogError(("OwnerType : " + owner.GetType() + ", propertyName : " + propertyName + ", valueType : " + value.GetType()));

            return false;
        }

        private static bool TryGetSetter<T>(Dictionary<Type, Dictionary<string, Action<object, T>>> container, Type ownerType, string propertyName, out Action<object, T> setter)
        {
            if (container.TryGetValue(ownerType, out var setterDictionary))
            {
                if (setterDictionary.TryGetValue(propertyName, out setter))
                    return true;
            }

            setter = null;
            return false;
        }
#endregion Setter

#region Getter
        public static bool IsExistsGetter(object owner, string propertyName)
        {
            if (!IsValid(owner, propertyName))
                return false;

            var ownerType = owner.GetType();

            if (TryGetGetter<int>(GettersInt, ownerType, propertyName, out var getterInt))
                return true;

            if (TryGetGetter<float>(GettersFloat, ownerType, propertyName, out var getterFloat))
                return true;

            if (TryGetGetter<double>(GettersDouble, ownerType, propertyName, out var getterDouble))
                return true;

            if (TryGetGetter<long>(GettersLong, ownerType, propertyName, out var getterLong))
                return true;

            if (TryGetGetter<decimal>(GettersDecimal, ownerType, propertyName, out var getterDecimal))
                return true;

            if (TryGetGetter<bool>(GettersBool, ownerType, propertyName, out var getterBool))
                return true;

            if (TryGetGetter<object>(GettersObject, ownerType, propertyName, out var getterObject))
                return true;

            return false;
        }
        
        
        private static bool TryGetGetter<T>(Dictionary<Type, Dictionary<string, Func<object, T>>> container, Type ownerType, string propertyName, out Func<object, T> getter)
        {
            if (container.TryGetValue(ownerType, out var getterDictionary))
            {
                if (getterDictionary.TryGetValue(propertyName, out getter))
                    return true;
            }

            getter = null;
            return false;
        }

        public static bool TryGetIntValue(object owner, string propertyName, out int value)
        {
            if (!IsValid(owner, propertyName))
            {
                value = default;
                return false;
            }

            if (TryGetGetter<int>(GettersInt, owner.GetType(), propertyName, out var getter))
            {
                value = getter(owner);
                return true;
            }

            value = default;
            return false;
        }


        public static bool TryGetBoolValue(object owner, string propertyName, out bool value)
        {
            if (!IsValid(owner, propertyName))
            {
                value = default;
                return false;
            }

            if (TryGetGetter<bool>(GettersBool, owner.GetType(), propertyName, out var getter))
            {
                value = getter(owner);
                return true;
            }

            value = default;
            return false;
        }


        public static bool TryGetFloatValue(object owner, string propertyName, out float value)
        {
            if (!IsValid(owner, propertyName))
            {
                value = default;
                return false;
            }

            if (TryGetGetter<float>(GettersFloat, owner.GetType(), propertyName, out var getter))
            {
                value = getter(owner);
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetDoubleValue(object owner, string propertyName, out double value)
        {
            if (!IsValid(owner, propertyName))
            {
                value = default;
                return false;
            }

            if (TryGetGetter<double>(GettersDouble, owner.GetType(), propertyName, out var getter))
            {
                value = getter(owner);
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetLongValue(object owner, string propertyName, out long value)
        {
            if (!IsValid(owner, propertyName))
            {
                value = default;
                return false;
            }

            if (TryGetGetter<long>(GettersLong, owner.GetType(), propertyName, out var getter))
            {
                value = getter(owner);
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetDecimalValue(object owner, string propertyName, out decimal value)
        {
            if (!IsValid(owner, propertyName))
            {
                value = default;
                return false;
            }

            if (TryGetGetter<decimal>(GettersDecimal, owner.GetType(), propertyName, out var getter))
            {
                value = getter(owner);
                return true;
            }

            value = default;
            return false;
        }


        public static bool TryGetObjectValue(object owner, string propertyName, out object value)
        {
            if (!IsValid(owner, propertyName))
            {
                value = default;
                return false;
            }

            if (TryGetGetter<object>(GettersObject, owner.GetType(), propertyName, out var getter))
            {
                value = getter(owner);
                return true;
            }

            value = default;
            return false;
        }
#endregion Getter

        private static bool IsValid(object owner, string propertyName)
        {
            if (!_isInitialized)
                return false;

            if (string.IsNullOrEmpty(propertyName))
                return false;

            return owner != null;
        }

        public static void Initialize()
        {
            _isInitialized = true;
        }

        public static int CastInt<T>(this T e) where T : unmanaged, IConvertible => UnsafeUtility.As<T, int>(ref e);
        public static bool CastBool<T>(this T e) where T : unmanaged, IConvertible => UnsafeUtility.As<T, bool>(ref e);
        public static float CastFloat<T>(this T e) where T : unmanaged, IConvertible => UnsafeUtility.As<T, float>(ref e);
        public static long CastLong<T>(this T e) where T : unmanaged, IConvertible => UnsafeUtility.As<T, long>(ref e);
        public static double CastDouble<T>(this T e) where T : unmanaged, IConvertible => UnsafeUtility.As<T, double>(ref e);
        public static decimal CastDecimal<T>(this T e) where T : unmanaged, IConvertible => UnsafeUtility.As<T, decimal>(ref e);
    }
}
