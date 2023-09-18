/*===============================================================
* Product:		Com2Verse
* File Name:	MicePrefs.cs
* Developer:	klizzard
* Date:			2023-08-02 10:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Logger;
using Cysharp.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Com2Verse.Mice
{
    /// <summary>
    /// 클릭에 따른 레드닷 표시 여부 저장 데이터.
    /// </summary>
    /// <typeparam name="TKey">실제 데이터의 KEY</typeparam>
    /// <typeparam name="TJsonKey">데이터 묶음의 KEY</typeparam>
    public class ClickedPrefs<TKey, TJsonKey>
    {
        [Serializable]
        public class JsonData
        {
            public Dictionary<TKey, long> Clicked = new();
        }

        /// <summary>
        /// 데이터 묶음의 KEY 설정, TKey와 TJsonKey 타입이 같은 경우에만 null로 설정하여 데이터 KEY를 사용할 수 있다.
        /// </summary>
        public Func<TKey, TJsonKey> OnGetJsonKey;

        /// <summary>
        /// PlayerPrefs에 저장될 KEY 설정, *필수 설정. (실제 사용되는 KEY: Mice_{PrefsKey}{JsonKey}_{AccountId})
        /// </summary>
        public Func<string> OnGetPrefsKey;

        /// <summary>
        /// 데이터 갱신 시간 값이 존재하는 경우 설정, 값이 없는 경우 default값을 반환. 
        /// </summary>
        public Func<TKey, DateTime> OnGetUpdateTime;

        /// <summary>
        /// 데이터 묶음
        /// </summary>
        private Dictionary<TJsonKey, JsonData> _jsonDatas = new();

        private bool IsValid()
        {
            if (OnGetJsonKey == null && typeof(TKey) != typeof(TJsonKey))
                return false;
            return OnGetPrefsKey != null;
        }

        private TJsonKey GetJsonKey(TKey key)
        {
            if (OnGetJsonKey != null)
                return OnGetJsonKey.Invoke(key);

            if (typeof(TKey) == typeof(TJsonKey))
                return (TJsonKey)(System.Object)key;

            return default;
        }

        private string GetPrefsKey(TJsonKey jsonKey)
        {
            return ZString.Format("Mice_{0}{1}_{2}",
                arg1: OnGetPrefsKey.Invoke(),
                arg2: jsonKey,
                arg3: MiceInfoManager.Instance.MyUserInfo.AccountId);
        }

        private DateTime GetUpdateTime(TKey key)
        {
            return OnGetUpdateTime?.Invoke(key) ?? default;
        }

        public JsonData Load(TKey key)
        {
            if (!IsValid()) return null;

            JsonData jsonData = null;

            var jsonKey = GetJsonKey(key);
            var prefsKey = GetPrefsKey(jsonKey);

            if (_jsonDatas.ContainsKey(jsonKey))
            {
                jsonData = _jsonDatas[jsonKey];
            }
            else
            {
                if (PlayerPrefs.HasKey(prefsKey))
                {
                    var prefsValue = PlayerPrefs.GetString(prefsKey);
                    try
                    {
                        jsonData = JsonConvert.DeserializeObject<JsonData>(prefsValue);
                    }
                    catch (Exception e)
                    {
                        C2VDebug.LogError(
                            $"[MICE.PREFS] {e.Message} : Invalid Data. {{key={prefsKey}, value={prefsValue}}}");
                    }
                }

                jsonData ??= new JsonData();
                _jsonDatas.Add(jsonKey, jsonData);
            }

            return jsonData;
        }

        public void Click(TKey key)
        {
            if (!IsValid()) return;

            var jsonData = Load(key);
            if (jsonData == null) return;

            if (jsonData.Clicked.ContainsKey(key))
                jsonData.Clicked[key] = DateTime.Now.Ticks;
            else
                jsonData.Clicked.Add(key, DateTime.Now.Ticks);

            var jsonKey = GetJsonKey(key);
            var prefsKey = GetPrefsKey(jsonKey);

            var originStr = string.Empty;
            try
            {
                originStr = JsonConvert.SerializeObject(jsonData);
            }
            catch (Exception e)
            {
                C2VDebug.LogError($"[MICE.PREFS] {e.Message}");
            }

            PlayerPrefs.SetString(prefsKey, originStr);
        }

        public bool IsNew(TKey key)
        {
            if (!IsValid()) return false;

            var jsonData = Load(key);
            if (jsonData == null) return false;

            // 클릭하지 않은 경우.
            if (!jsonData.Clicked.ContainsKey(key))
                return true;

            // 클릭한 경우.
            var clickedTime = new DateTime(jsonData.Clicked[key]);

            // 갱신 시간이 없는 경우.
            var updateTime = GetUpdateTime(key);
            if (updateTime.Equals(default))
                return false;

            // 갱신 시간과 비교한다.
            return clickedTime < updateTime;
        }
    }
}