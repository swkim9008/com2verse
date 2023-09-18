/*===============================================================
* Product:		Com2Verse
* File Name:	MiceExtensions_String.cs
* Developer:	sprite
* Date:			2023-04-20 21:08
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Logger;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using System;

namespace Com2Verse.Mice
{
    public static partial class StringExtensions
    {
        /// <summary>
        /// 주어진 문자열을 Toast Popup 으로 표시한다.
        /// </summary>
        /// <param name="value"></param>
        public static void ShowAsToast(this string value)
            => UIManager.Instance.SendToastMessage(value);


        /// <summary>
        /// 주어진 문자열을 Log로 출력한다.
        /// </summary>
        /// <param name="value"></param>
        public static void ShowAsLog(this string value)
            => C2VDebug.Log(value);

        /// <summary>
        /// 주어진 문자열을 알림(Notice) 팝업으로 출력한다.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="title"></param>
        public static void ShowAsNotice(this string value, string title = "Notice")
            => UIManager.Instance.ShowPopUpNotice(title, value);

        /// <summary>
        /// 리소스 문자열로부터 <see cref="GUIView"/>를 생성한다.
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="onLoadCompleted"></param>
        /// <returns></returns>
        public static async UniTask<GUIView> AsGUIView(this string asset, Action<GUIView> onLoadCompleted = null)
        {
            GUIView view = null;

            await UIManager.Instance.CreatePopup(asset, v => { view = v; onLoadCompleted?.Invoke(v); });

            return view;
        }

        /// <summary>
        /// 열거형(<typeparamref name="TEnum"/>) 값으로부터 <see cref="GUIView"/>를 생성한다.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <param name="onLoadCompleted"></param>
        /// <returns></returns>
        public static UniTask<GUIView> AsGUIView<TEnum>(this TEnum value, Action<GUIView> onLoadCompleted = null)
            where TEnum : unmanaged, Enum
            => value.ToString().AsGUIView(onLoadCompleted);

        /// <summary>
        /// 주어진 문자열을 <see cref="Localization"/> 텍스트로 변환한다.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ToLocalizationString(this string key)
        {
#if UNITY_EDITOR || ENV_DEV
            var str = Localization.Instance.GetString(key);
            return string.IsNullOrEmpty(str) ? key : str;
#else
            return Localization.Instance.GetString(key);
#endif
        }

        /// <summary>
        /// 주어진 문자열을 인자(<paramref name="args"/>)와 함께 <see cref="Localization"/> 텍스트로 변환한다.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string ToLocalizationString(this string key, params object[] args)
        {
#if UNITY_EDITOR || ENV_DEV
            var str = Localization.Instance.GetString(key);
            var fmt = string.IsNullOrEmpty(str) ? key : str;
#else
            var fmt = Localization.Instance.GetString(key);
#endif
            return string.Format(fmt, args);
        }

        /// <summary>
        /// 주어진 열거형(<typeparamref name="TEnum"/>) 값의 이름을 <see cref="Localization"/> 텍스트로 변환한다.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToLocalizationString<TEnum>(this TEnum value)
            where TEnum : unmanaged, System.Enum
            => EnumToString(value).ToLocalizationString();

        private static string EnumToString<TEnum>(TEnum value)
            where TEnum : unmanaged, System.Enum
        {
            // Data.Localization.eKey 타입인 경우, Localization.Get()으로 변환한다.
            if (typeof(TEnum) == typeof(Data.Localization.eKey))
            {
                return Data.Localization.Get((Data.Localization.eKey)(object)value);
            }

            return value.ToString();
        }

        /// <summary>
        /// 주어진 열거형(<typeparamref name="TEnum"/>) 값의 이름을 인자(<paramref name="args"/>)와 함께 <see cref="Localization"/> 텍스트로 변환한다.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string ToLocalizationString<TEnum>(this TEnum value, params object[] args)
            where TEnum : unmanaged, System.Enum
            => EnumToString(value).ToLocalizationString(args);

        /// <summary>
        /// 주어진 열거형(<typeparamref name="TEnum"/>) 값의 이름을 <see cref="Localization"/> 텍스트로 변환 후 Toast 메시지로 표시한다.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        public static void ShowAsToast<TEnum>(this TEnum value)
             where TEnum : unmanaged, System.Enum
            => value.ToLocalizationString().ShowAsToast();
    }
}
