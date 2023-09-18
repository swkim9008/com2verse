/*===============================================================
* Product:		Com2Verse
* File Name:	Localization.cs
* Developer:	haminjeong
* Date:			2022-07-08 11:16
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.Logger;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
    public sealed class Localization : Singleton<Localization>
    {
        /// <summary>
        /// Singleton Instance Creation
        /// </summary>
        [UsedImplicitly] private Localization() { }

        public enum eLanguage
        {
            KOR = 0,
            ENG,
            UNKNOWN = 99
        }

        private readonly List<ILocalizationUI>              _localizationUiList  = new();
        private readonly Dictionary<int, string>            _errorTable          = new();
        private readonly Dictionary<string, string>         _stringTable         = new();
        private readonly Dictionary<string, string>         _tutorialStringTable = new();
        private readonly Dictionary<string, string>         _avatarItemTable     = new();
        private readonly Dictionary<int, string>            _officeErrorTable    = new();
        private readonly Dictionary<int, OfficeErrorString> _officeErrorType     = new();
        private readonly Dictionary<string, string>         _loadingTable        = new();
        private readonly Dictionary<int, string>            _miceErrorTable      = new();
        private readonly Dictionary<int, string>            _miceWebErrorTable   = new();
        
        public           eLanguage                          CurrentLanguage { get; private set; } = eLanguage.KOR;

        public void AddLocalizationUI(ILocalizationUI localizationUI)
        {
            if (!_localizationUiList.Contains(localizationUI))
            {
                _localizationUiList.Add(localizationUI);
            }
        }

        public void RemoveLocalizationUI(ILocalizationUI localizationUI)
        {
            _localizationUiList.Remove(localizationUI);
        }

        // 최초 진입시 옵션에서 호출됨.
        public void ChangeLanguage(eLanguage language)
        {
            Initialize(language);

            LoadBuiltInTableAsync().Forget();
            LoadTable();
            
            ApplyLanguage();
        }


        public void Initialize(eLanguage language)
        {
            CurrentLanguage = language;

            _errorTable.Clear();
            _stringTable.Clear();
            _tutorialStringTable.Clear();
            _avatarItemTable.Clear();
            _officeErrorTable.Clear();
            _officeErrorType.Clear();
            _loadingTable.Clear();
            _miceErrorTable.Clear();
            _miceWebErrorTable.Clear();
        }

        private void ApplyLanguage()
        {
            foreach (var localizationUI in _localizationUiList)
            {
                localizationUI.OnLanguageChanged();
            }
        }

        public async UniTask LoadBuiltInTableAsync()
        {
            var loadTypes = new[]
            {
                typeof(TableBuiltInString),
            };

            Loader.ResultInfo[] resultInfos;
            try
            {
                resultInfos = await DataLoader.LoadFromBundlesAsync(loadTypes);
            }
            catch (Exception e)
            {
                C2VDebug.LogError(e.Message);
                return;
            }

            TableBuiltInString? tableBuiltInString = null;

            if (resultInfos == null) return;

            foreach (var resultInfo in resultInfos)
            {
                if (resultInfo.Success)
                {
                    switch (resultInfo.Data)
                    {
                        case TableBuiltInString data:
                            tableBuiltInString = data;
                            break;
                    }
                }
            }

            if (tableBuiltInString?.Datas == null) return;

            foreach (var data in tableBuiltInString.Datas.Values)
            {
                if (data == null) continue;
                switch (CurrentLanguage)
                {
                    case eLanguage.KOR:
                        if (!_stringTable.TryAdd(data.ID, data.KO))
                            _stringTable[data.ID] = data.KO;
                        break;
                    case eLanguage.ENG:
                        if (!_stringTable.TryAdd(data.ID, data.EN))
                            _stringTable[data.ID] = data.EN;
                        break;
                    // TODO: Add for support more languages 
                }
            }
        }

        public void LoadTable()
        {
            TableErrorString?    tableErrorString    = TableDataManager.Instance.Get<TableErrorString>();
            TableString?         tableString         = TableDataManager.Instance.Get<TableString>();
            TableTutorialString? tableTutorialString = TableDataManager.Instance.Get<TableTutorialString>();
            TableAvatarItem?     tableAvatarItem     = TableDataManager.Instance.Get<TableAvatarItem>();
            TableLoading?        tableLoading        = TableDataManager.Instance.Get<TableLoading>();

            if (tableTutorialString?.Datas == null || tableErrorString?.Datas == null || tableString?.Datas == null || tableAvatarItem?.Datas == null || tableLoading?.Datas == null) return;

            foreach (var data in tableErrorString.Datas.Values)
            {
                if (data == null) continue;
                switch (CurrentLanguage)
                {
                    case eLanguage.KOR:
                        if (!_errorTable.TryAdd(data.ID, data.KO))
                            _errorTable[data.ID] = data.KO;
                        break;
                    case eLanguage.ENG:
                        if (!_errorTable.TryAdd(data.ID, data.EN))
                            _errorTable[data.ID] = data.EN;
                        break;
                    // TODO: Add for support more languages 
                }
            }

            foreach (var data in tableTutorialString.Datas.Values)
            {
                if (data == null) continue;
                switch (CurrentLanguage)
                {
                    case eLanguage.KOR:
                        if (!_tutorialStringTable.TryAdd(data.ID, data.KO))
                            _tutorialStringTable[data.ID] = data.KO;
                        break;
                    case eLanguage.ENG:
                        if (!_tutorialStringTable.TryAdd(data.ID, data.EN))
                            _tutorialStringTable[data.ID] = data.EN;
                        break;
                    // TODO: Add for support more languages
                }
            }

            foreach (var data in tableString.Datas.Values)
            {
                if (data == null) continue;
                switch (CurrentLanguage)
                {
                    case eLanguage.KOR:
                        if (!_stringTable.TryAdd(data.ID, data.KO))
                            _stringTable[data.ID] = data.KO;
                        break;
                    case eLanguage.ENG:
                        if (!_stringTable.TryAdd(data.ID, data.EN))
                            _stringTable[data.ID] = data.EN;
                        break;
                    // TODO: Add for support more languages
                }
            }

            foreach (var data in tableAvatarItem.Datas.Values)
            {
                if (data?.ID == null) continue;
                var upperID = data.ID.ToUpper();

                switch (CurrentLanguage)
                {
                    case eLanguage.KOR:
                        if (!_avatarItemTable.TryAdd(upperID, data.KO))
                            _avatarItemTable[upperID] = data.KO;
                        break;
                    case eLanguage.ENG:
                        if (!_avatarItemTable.TryAdd(upperID, data.EN))
                            _avatarItemTable[upperID] = data.EN;
                        break;
                    // TODO: Add for support more languages
                }
            }

            foreach (var data in tableLoading.Datas.Values)
            {
                if (data == null) continue;
                switch (CurrentLanguage)
                {
                    case eLanguage.KOR:
                        if (!_loadingTable.TryAdd(data.ID, data.KO))
                            _loadingTable[data.ID] = data.KO;
                        break;
                    case eLanguage.ENG:
                        if (!_loadingTable.TryAdd(data.ID, data.EN))
                            _loadingTable[data.ID] = data.EN;
                        break;
                    // TODO: Add for support more languages
                }
            }

            LoadOfficeErrorString();
            LoadMiceErrorString();
        }

#region Office Error String
        public OfficeErrorString CheckOfficeErrorStringValue(int errorCode)
        {
            if (_officeErrorType.TryGetValue(errorCode, out var type))
            {
                return type;
            }

            return null;
        }
        
        private void LoadOfficeErrorString()
        {
            _officeErrorTable.Clear();
            _officeErrorType.Clear();

            var tableData = TableDataManager.Instance.Get<TableOfficeErrorString>();
            foreach (var (key, value) in tableData.Datas)
            {
                switch (CurrentLanguage)
                {
                    case eLanguage.KOR:
                        if (!_officeErrorTable.TryAdd(key, value.KO))
                            _officeErrorTable[key] = value.KO;
                        break;
                    case eLanguage.ENG:
                    default:
                        if (!_officeErrorTable.TryAdd(key, value.EN))
                            _officeErrorTable[key] = value.EN;
                        break;
                }

                if (!_officeErrorType.TryAdd(key, value))
                    _officeErrorType[key] = value;
            }
        }

        public string GetOfficeErrorString(int errorCode)
        {
            if (_officeErrorTable.TryGetValue(errorCode, out var key))
            {
                if (string.IsNullOrWhiteSpace(key)) return string.Empty;

                var output = key;

                if (GetStringBody(key, _stringTable, out var resultStr))
                    output = resultStr;

                return output.Replace(@"\n", Environment.NewLine);
            }

            return string.Empty;
        }
#endregion // Office Error String

#region Mice Error String
private void LoadMiceErrorString()
{
    {
        _miceWebErrorTable.Clear();
        var tableData = TableDataManager.Instance.Get<TableMiceWebErrorString>();
        foreach (var (key, value) in tableData.Datas)
        {
            switch (CurrentLanguage)
            {
                case eLanguage.KOR:
                    if (!_miceWebErrorTable.TryAdd(key, value.KO))
                        _miceWebErrorTable[key] = value.KO;
                    break;
                case eLanguage.ENG:
                default:
                    if (!_miceWebErrorTable.TryAdd(key, value.EN))
                        _miceWebErrorTable[key] = value.EN;
                    break;
            }
        }        
    }
    {
        _miceErrorTable.Clear();
        var tableData = TableDataManager.Instance.Get<TableMiceErrorString>();
        foreach (var (key, value) in tableData.Datas)
        {
            switch (CurrentLanguage)
            {
                case eLanguage.KOR:
                    if (!_miceErrorTable.TryAdd(key, value.KO))
                        _miceErrorTable[key] = value.KO;
                    break;
                case eLanguage.ENG:
                default:
                    if (!_miceErrorTable.TryAdd(key, value.EN))
                        _miceErrorTable[key] = value.EN;
                    break;
            }
        }
    }
}

public string GetMiceWebErrorString(int errorCode)
{
    if (_miceWebErrorTable.TryGetValue(errorCode, out var key))
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;

        var output = key;

        return output.Replace(@"\n", Environment.NewLine);
    }

    return string.Empty;
}
public string GetMiceErrorString(int errorCode)
{
    if (_miceErrorTable.TryGetValue(errorCode, out var key))
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;

        var output = key;

        return output.Replace(@"\n", Environment.NewLine);
    }

    return string.Empty;
}
#endregion // Mice Error String
#region Getter
        public string GetErrorString(int id)
        {
            if (!_errorTable.TryGetValue(id, out var result))
                return string.Empty;
            return result ?? string.Empty;
        }

        private bool GetStringBody(string id, IReadOnlyDictionary<string, string> stringTable, out string result)
        {
            if (string.IsNullOrEmpty(id))
            {
                result = string.Empty;
                return false;
            }

            var hasStringKey = stringTable.TryGetValue(id, out result);
            if (!hasStringKey || result == null)
                result = string.Empty;

            return hasStringKey;
        }

        public string GetTutorialString(string id)
        {
            if (!GetStringBody(id, _tutorialStringTable, out string result)) return result;
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetAvatarItemString(string id)
        {
            if (!GetStringBody(id, _avatarItemTable, out string result)) return result;
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetLoadingString(string id)
        {
            if (!GetStringBody(id, _loadingTable, out string result)) return result;
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString(string id)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString<T1>(string id, T1 arg1)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            result = ZString.Format(result, arg1);
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString<T1, T2>(string id, T1 arg1, T2 arg2)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            result = ZString.Format(result, arg1, arg2);
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString<T1, T2, T3>(string id, T1 arg1, T2 arg2, T3 arg3)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            result = ZString.Format(result, arg1, arg2, arg3);
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString<T1, T2, T3, T4>(string id, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            result = ZString.Format(result, arg1, arg2, arg3, arg4);
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString<T1, T2, T3, T4, T5>(string id, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            result = ZString.Format(result, arg1, arg2, arg3, arg4, arg5);
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString<T1, T2, T3, T4, T5, T6>(string id, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            result = ZString.Format(result, arg1, arg2, arg3, arg4, arg5, arg6);
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7>(string id, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            result = ZString.Format(result, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8>(string id, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            result = ZString.Format(result, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string id, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            result = ZString.Format(result, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string id, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            result = ZString.Format(result, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string id, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            result = ZString.Format(result, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string id, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11,
                                                                                   T12 arg12)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            result = ZString.Format(result, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string id, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
                                                                                        T11 arg11, T12 arg12, T13 arg13)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            result = ZString.Format(result, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string id, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
                                                                                             T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            result = ZString.Format(result, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string id, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10,
                                                                                                  T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            result = ZString.Format(result, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
            return result.Replace(@"\n", Environment.NewLine);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(string id, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9,
                                                                                                       T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
        {
            if (!GetStringBody(id, _stringTable, out string result)) return result;
            result = ZString.Format(result, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
            return result.Replace(@"\n", Environment.NewLine);
        }

        public int GetStringByte()
        {
            switch (CurrentLanguage)
            {
                case eLanguage.ENG:
                    return 1;
                case eLanguage.KOR:
                    return 3;
            }

            return 1;
        }
#endregion
    }
}
